using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace UsosFix.Services;

public class MailService : IMailService
{
    private readonly SendGridClient _sendGridClient;
    private readonly TimetableService _timetableService;

    public MailService(string apiKey, TimetableService timetableService)
    {
        _timetableService = timetableService;
        _sendGridClient = new SendGridClient(apiKey);
    }

    private SendGridMessage CreateMail()
    {
        var from = new EmailAddress("usosfix@gmail.com", "UsosFix");
        var subject = "New groups";
        var to = new EmailAddress("lewandowskipiotr98@gmail.com", "Piotr Lewandowski");
        var plainTextContent = "Here's the latest batch of groups from UsosFix.";
        var htmlContent = "Here's the latest batch of groups from UsosFix.";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        return msg;
    }

    private async Task<SendGridMessage> AddAttachments(SendGridMessage message, int subjectId)
    {
        var result = _timetableService.GetCurrentGroups(subjectId);

        foreach (var mailGroup in result)
        {
            var filename = $"{string.Join("_", mailGroup.SubjectName)}_{mailGroup.GroupNumber}.csv";

            await using var writer = new StreamWriter(filename);
            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(mailGroup.Students);
            }

            var file = await File.ReadAllBytesAsync(filename);
            var content = Convert.ToBase64String(file);
            message.AddAttachment(filename, content);
            File.Delete(filename);
        }

        return message;
    }

    public async Task<bool> SendAsync(IEnumerable<int> subjectIds)
    {
        var mail = CreateMail();

        foreach (var subjectId in subjectIds)
        {
            mail = await AddAttachments(mail, subjectId);
        }

        var response = await _sendGridClient.SendEmailAsync(mail);

        return response.IsSuccessStatusCode;
    }

    public Task<bool> SendAsync(int subjectId) => SendAsync(new[] { subjectId });
}