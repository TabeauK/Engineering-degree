using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UsosFix.Models;
using UsosFix.UsosApi;
using UsosFix.UsosApi.Methods;
using UsosFix.ViewModels;
using Exchange = UsosFix.Models.Exchange;
using GroupMeeting = UsosFix.Models.GroupMeeting;
using Semester = UsosFix.Models.Semester;

namespace UsosFix.Services;

public class DataLoader
{
    public DataLoader(ApplicationDbContext context, ApiConnector apiConnector, IHttpClientFactory httpClientFactory,
        ILogger<DataLoader> logger, ISemesterService semesterService)
    {
        DbContext = context;
        ApiConnector = apiConnector;
        HttpClientFactory = httpClientFactory;
        Logger = logger;
        SemesterService = semesterService;
    }

    private ApiConnector ApiConnector { get; }
    private IHttpClientFactory HttpClientFactory { get; }
    private ApplicationDbContext DbContext { get; }
    private ILogger<DataLoader> Logger { get; }
    private ISemesterService SemesterService { get; }

    private string TranslateToUsosName(Semester semester)
    {
        var seasonSymbol = semester.Season == SemesterSeason.Summer ? "L" : "Z";
        return $"{semester.Year}{seasonSymbol}";
    }

    public async Task<User?> GetAndPopulateUserForToken(string token, string secret)
    {
        var url = ApiConnector.GetUrl(new UserMethod(),
            token, secret, true);
        var httpClient = HttpClientFactory.CreateClient();

        try
        {
            var result = await httpClient.GetStringAsync(url);
            Logger.LogDebug("Request to {} returned {}", url, result);

            var account = JsonSerializer.Deserialize<User>(result);

            if (account is null)
            {
                return null;
            }

            account.Username = "Username";
            account.PreferredLanguage = Language.Polish;
            var currentSemester = await SemesterService.GetCurrentSemesterAsync();

            var dbAccount = DbContext.Users.SingleOrDefault(a => a.StudentNumber == account.StudentNumber);

            if (dbAccount is null)
            {
                await DbContext.Users.AddAsync(account);
                await DbContext.SaveChangesAsync();

                await PopulateGroups(token, secret, account, currentSemester);
                await DbContext.SaveChangesAsync();

                return account;
            }

            var hasGroupsForCurrentSemester =
                DbContext.Users.FirstOrDefault(u => u.Groups.Any(g => g.Subject.Semester.IsCurrent)) is not null;

            if (!hasGroupsForCurrentSemester)
            {
                await PopulateGroups(token, secret, dbAccount, currentSemester);
                await DbContext.SaveChangesAsync();
            }

            return dbAccount;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task PopulateGroups(string token, string secret, User user, Semester currentSemester)
    {
        var url = ApiConnector.GetUrl(new GroupInfoMethod(),
            token, secret, true);
        var httpClient = HttpClientFactory.CreateClient();

        try
        {
            var result = await httpClient.GetStringAsync(url);
            Logger.LogDebug("Request to {} returned {}", url, result);
            var groupsJson = JsonDocument.Parse(result)
                .RootElement
                .GetProperty("groups")
                .GetProperty(TranslateToUsosName(currentSemester))
                .EnumerateArray();

            await PopulateSubjects(token, secret, currentSemester);
            await DbContext.SaveChangesAsync();

            foreach (var json in groupsJson)
            {
                Logger.LogDebug("Processing group {}.", json);
                var subjectNameString = json.GetProperty("course_name").GetRawText();
                var subjectName = JsonSerializer.Deserialize<LanguageString>(subjectNameString)!;
                var subject = DbContext.Subjects.Single(s =>
                    s.Name.Polish == subjectName.Polish && s.Name.English == subjectName.English &&
                    s.Semester.IsCurrent);
                var lecturers = json.GetProperty("lecturers")
                    .EnumerateArray()
                    .Select(l =>
                        l.GetProperty("first_name").GetString() + " " + l.GetProperty("last_name").GetString());
                var lecturersString = string.Join(", ", lecturers);
                var classType = json.GetProperty("class_type_id").GetString()!;
                var membersCount = json.GetProperty("participants").GetArrayLength();
                var size = GetPossibleSize(membersCount, classType);

                var group = new Group
                {
                    UsosUnitId = json.GetProperty("course_unit_id").GetString()!,
                    GroupNumber = json.GetProperty("group_number").GetInt32(),
                    Lecturers = lecturersString,
                    ClassType = classType,
                    Subject = subject,
                    Students = new List<User>(),
                    ExchangesTo = new List<Exchange>(),
                    ExchangesFrom = new List<Exchange>(),
                    MaxMembers = size,
                    CurrentMembers = membersCount
                };

                var dbGroup = await DbContext.Groups
                    .Include(g => g.Students)
                    .SingleOrDefaultAsync(g => g.Subject.Name.Polish == subject.Name.Polish &&
                                               g.Subject.Name.English == subject.Name.English &&
                                               g.GroupNumber == group.GroupNumber &&
                                               g.ClassType == group.ClassType &&
                                               g.Subject.Semester.IsCurrent);

                if (dbGroup is null)
                {
                    group.Students = new List<User> { user };
                    var meetings = await GetGroupMeetings(token, secret, group);
                    await DbContext.SaveChangesAsync();
                    if (meetings is not null)
                    {
                        group.Meetings = meetings;
                    }

                    await DbContext.Entry(user).Collection(u => u.Groups).LoadAsync();
                    await DbContext.Groups.AddAsync(group);
                    user.Groups.Add(group);

                    subject.Groups.Add(group);
                }
                else if (dbGroup.Students.All(s => s.Id != user.Id))
                {
                    dbGroup.Students.Add(user);
                }
            }
        }
        catch (HttpRequestException) { }
        catch (KeyNotFoundException e)
        {
            Logger.LogError(e, "Populating groups failed.");
        }
    }

    private int GetPossibleSize(int current, string type)
    {
        var sizes = new[] { 16, 18, 20, 30, 35, 150 };
        var size = type switch
        {
            "CWI" => 30,
            "WYK" => 150,
            "LAB" => 16,
            "PRO" => 16,
            _ => 30
        };
        if (current > size)
        {
            size = sizes.First(x => x >= size);
        }

        return size;
    }

    private async Task PopulateSubjects(string token, string secret, Semester currentSemester)
    {
        var url = ApiConnector.GetUrl(new SubjectsMethod(),
            token, secret, true);
        var httpClient = HttpClientFactory.CreateClient();

        try
        {
            var result = await httpClient.GetStringAsync(url);
            var subjectsJson = JsonDocument.Parse(result)
                .RootElement
                .GetProperty("course_editions")
                .GetProperty(TranslateToUsosName(currentSemester))
                .EnumerateArray();

            var subjects = new List<Subject>();

            foreach (var json in subjectsJson)
            {
                var nameJson = json.GetProperty("course_name").GetRawText();
                var name = JsonSerializer.Deserialize<LanguageString>(nameJson);
                if (name is null)
                {
                    return;
                }

                var subject = new Subject
                {
                    Name = name,
                    Groups = new List<Group>(),
                    Exchanges = new List<Exchange>(),
                    Semester = currentSemester
                };

                var alreadyExists = DbContext.Subjects.Any(s =>
                    s.Name.Polish == subject.Name.Polish && s.Name.English == subject.Name.English &&
                    s.Semester.IsCurrent);
                if (!alreadyExists)
                {
                    subjects.Add(subject);
                }
            }

            DbContext.Subjects.AddRange(subjects);
        }
        catch (HttpRequestException) { }
        catch (KeyNotFoundException e)
        {
            Logger.LogError(e, "Populating subjects failed.");
        }
    }

    private async Task<ICollection<GroupMeeting>?> GetGroupMeetings(string token, string secret, Group group)
    {
        var url = ApiConnector.GetUrl(new GroupMeetingsMethod(group.GroupNumber, group.UsosUnitId),
            token, secret, true);

        var httpClient = HttpClientFactory.CreateClient();

        try
        {
            var result = await httpClient.GetStringAsync(url);

            var meetingsJson = JsonDocument.Parse(result)
                .RootElement
                .EnumerateArray();

            var meetings = new List<GroupMeeting>();

            foreach (var json in meetingsJson.Where(m =>
                         m.GetProperty("type").GetString() == "classgroup" ||
                         m.GetProperty("type").GetString() == "classgroup2"))
            {
                var startTimeString = json.GetProperty("start_time").GetString()!;
                var startTime = DateTime.ParseExact(startTimeString, "yyyy-MM-dd HH:mm:ss", null).ToUniversalTime();
                var endTimeString = json.GetProperty("end_time").GetString()!;
                var endTime = DateTime.ParseExact(endTimeString, "yyyy-MM-dd HH:mm:ss", null).ToUniversalTime();
                var room = json.GetProperty("room_number").GetString()!;
                var buildingString = json.GetProperty("building_name").GetRawText();
                var building = JsonSerializer.Deserialize<LanguageString>(buildingString)!;

                var meeting = new GroupMeeting
                {
                    Group = group,
                    Building = building,
                    StartTime = startTime,
                    EndTime = endTime,
                    Room = room
                };

                meetings.Add(meeting);
            }

            return meetings;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}