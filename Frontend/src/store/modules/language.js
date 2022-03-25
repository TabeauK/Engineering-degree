import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  currentLanguage: "pl",
  translations: {
    "pl" : {
      "Something went wrong. Try again later...": "Coś poszło nie tak. Spróbuj ponownie później...",
      "You have been logged out": "Zostałeś wylogowany",
      "Session expired": "Sesja wygasła",
      "There was an error while submiting join. Try again later...": "Wystąpił błąd podczas zgłaszania prośby dołączenia. Spróbuj ponownie później...",
      "There was an error while removing exchange. Try again later...":"Wystąpił błąd podczas usuwania wymiany. Spróbuj ponownie później...",
      "There was an error while inviting user. Try again later...":"Wystapił błąd podczas zapraszania użytkownika. Spróbuj ponownie później...",
      "There was an error while removing invite. Try again later...":"Wystapił błąd podczas usuwania zaproszenia. Spróbuj ponownie później...",
      "There was an error while getting plan. Try again later...":"Wystapił błąd podczas pobierania planu. Spróbuj ponownie później...",
      "There was an error while getting exchanges relations. Try again later...":"Wystapił błąd podczas pobierania relacji między wymianami. Spróbuj ponownie później...",
      "There was an error while getting exchanges. Try again later...":"Wystapił błąd podczas pobierania wymian. Spróbuj ponownie później...",
      "There was an error while adding relations between exchanges. Try again later...":"Wystapił błąd podczas nadawania relacji. Spróbuj ponownie później...",
      "There was an error while removing relations between exchanges. Try again later...":"Wystapił błąd podczas usuwania relacji. Spróbuj ponownie później...",
      "There was an error while getting your teams. Try again later...":"Wystapił błąd podczas pobierania zespołów. Spróbuj ponownie później...",
      "There was an error while setting your username. Try again later...":"Wystapił błąd podczas zmieniania nazwy użytkownika. Spróbuj ponownie później...",
      "Your username was saved":"Nazwa użytkownika została zmieniona",
      "There was an error while adding leader. Try again later...":"Wystapił błąd podczas dodawania starosty. Spróbuj ponownie później...",
      "There was an error while getting admins and leaders. Try again later...":"Wystapił błąd podczas pobierania starostów i adminów. Spróbuj ponownie później...",
      "There was an error while removing leader. Try again later...": "Wystapił błąd podczas usuwania starosty. Spróbuj ponownie później...",
      "There was an error while adding admin. Try again later...":"Wystapił błąd podczas dodawania admina. Spróbuj ponownie później...",
      "There was an error while removing admin. Try again later...":"Wystapił błąd podczas usuwania admina. Spróbuj ponownie później...",
      "There was an error while sending email to faculty. Try again later...":"Wystapił błąd podczas wysyłania maila do dziekanatu. Spróbuj ponownie później...",
      "There was an error while sending request. Try again later...":"Wystapił błąd podczas wysyłania żądania. Spróbuj ponownie później...",
      "Welcome to UsosFix": "Witamy w",
      "Settings": "Panel użytkownika",
      "My exchanges": "Moje wymiany",
      "My timetable": "Mój plan",
      "My teams": "Moje zespoły",
      "Messages": "Wiadomości",
      "Back" : "Wróć",
      "Go back" : "Powróć",
      "Subject": "Przedmiot",
      "Group" : "Grupa",
      "Class type": "Rodzaj zajęć",
      "Lecturer": "Prowadzący",
      "Place": "Miejsce",
      "Meeting times": "Daty spotkań",
      "Max number of students": "Limit studentów",
      "Current number of students": "Liczba studentów",
      "Irregular": "Nieregularność",
      "Join": "Dołącz",
      "Stop joining": "Przestań dołączać",
      "Already in this group": "Jesteś w tej grupie",
      "currentPageReportTemplate": "Pokazywane od {first} do {last} z {totalRecords}",
      "No students found": "Nie znaleziono uczestników",
      "Loading students, please wait...": "Ładowanie uczestników. Proszę czekać...",
      "you": "ty",
      "Already in your group": "Już jest w twoim zespole",
      "Stop inviting": "Przestań zapraszać",
      "Invite user": "Zaproś użytkownika do zepołu",
      "Currently, you don't have any active exchanges": "Nie masz żadnych zgłoszonych wymian",
      "Add dependency relation": "Dodaj relację powiązania",
      "Close": "Zamknij",
      "To": "Do",
      "From": "Z",
      "No exchanges to choose from": "Brak wymian możliwych do wyboru",
      "Add excluding relation": "Dodaj relację wykluczenia",
      "Cancel relation": "Anuluj relację",
      "Login": "Zaloguj się",
      "Loading exchanges, please wait...": "Ładowanie wymian. Proszę czekać...",
      "Add dependency relation to ": "Dodaj relację powiązania do ",
      "Add excluding relation to ": "Dodaj relację wykluczenia do ",
      "In relation" : "W relacji {0} z",
      "pro": "Projekt",
      "wyk": "Wykład",
      "lab": "Laboratoria",
      "cwi": "Ćwiczenia",
      "Pending": "Zaproszony",
      "Remove user": "Usuń",
      "Stop inviting user": "Usuń",
      "Currently, you don't have any teams": "Nie masz żadnych zepołów",
      "Loading groups, please wait...":"Ładowanie zespołów. Proszę czekać...",
      "Confirmation": "Potwierdzenie",
      "Are you sure you want to remove user": "Jesteś pewien, że chcesz usunąć użytkownika {0} z zespołu do przedmiotu {1}",
      "Are you sure you want to stop inviting": "Jesteś pewien, że chcesz przestać zapraszać {0} z zespołu do przedmiotu {1}",
      "Weekends": "Weekendy",
      "Schedule from Usos": "Plan z Usosa",
      "My schedule": "Plan po wymianach",
      "First name": "Imię",
      "Last name": "Nazwisko",
      "Username":"Nazwa użytkownika",
      "Save": "Zmień",
      "Index": "Numer albumu",
      "Language":"Język",
      "Reveal Yourself": "Ujawnij twoje imię i nazwisko",
      "Already revealed": "Pokazujesz imię i nazwisko innym użytkownikom",
      "Logout": "Wyloguj",
      "Leader's section": "Panel starosty",
      "Send data to administration": "Wyślij dane do dziekanatu",
      "Admin's section": "Panel admina",
      "No leaders": "Brak starostów",
      "Loading leaders, please wait...": "Ładowanie starostów. Proszę czekać",
      "List of leaders": "Lista starostów",
      "Remove": "Usuń",
      "Add": "Dodaj",
      "index": "Numer albumu",
      "Add admin": "Dodaj admina",
      "List of admins": "Lista adminów",
      "Add leader": "Dodaj starostę",
      "Loading admins, please wait...": "Ładowanie starostów.. Proszę czekać",
      "No admins": "Brak adminów",
      "pl": "Polski",
      "en": "Angielski",
      "And": "Powiązanie",
      "Xor": "Wykluczenie",
      "Are you sure you want your name to be visible to all users in this group? This cannot be reversed": "Jestś pewien, że chcesz pokazywac swoje imię i nazwisko wszystkim użytkownikom?",
      "Are you sure you want to take away admin privileges from user with ID: {0}?": "Jesteś pewien, że chcesz odebrać prawa admina użytkownikowi z indeksem: {0}?",
      "Are you sure you want to take away leader privileges from user with ID: {0}?": "Jesteś pewien, że chcesz odebrać prawa starosty użytkownikowi z indeksem: {0}?",
      "Are you sure you want to grant leader privileges to user with ID: {0}?": "Jesteś pewien, że chcesz nadać prawa starosty użytkownikowi z indeksem: {0}?",
      "Are you sure you want to grant admin privileges to user with ID: {0}?": "Jesteś pewien, że chcesz nadać prawa admina użytkownikowi z indeksem: {0}?",
      "Are you sure you want send current state to faculty administrator?": "Jesteś pewien, że chcesz przeszłać maila do adminstarcji z wprowadzonymi wymianami",
      "Cannot remove last admin": "Nie można usunąć ostatniego użytkownika",
      "Admin ID must be a number": "Indeks admina musi być liczbą",
      "Leader ID must be a number":"Indeks starosty musi być liczbą",
      "Students": "Uczestnicy",
      "Mail to faculty was sent": "Mail do administarcji został wysłany",
      "Subjects": "Przedmioty",
      "Loading subjects, please wait...": "Ładowanie przedmiotów, proszę czekać...",
      "No subjects to choose from": "Brak przedmiotów do wyboru",
      "There was an error while getting messages. Try again later...": "Wystapił błąd podczas pobierania wiadomości. Spróbuj ponownie później...",
      "There was an error while getting chats. Try again later...": "Wystapił błąd podczas pobierania chat'ów. Spróbuj ponownie później...",
      "There was an error while sending message. Try again later...": "Wystapił błąd podczas wysyłania wiadomości. Spróbuj ponownie później...",
      "User invited": "Użytkownik został zaproszony",
      "There was an error while sending invite. Try again later...": "Wystapił błąd podczas wysyłania zaproszenia. Spróbuj ponownie później...",
      "There was an error while accepting invite. Try again later...": "Wystapił błąd podczas akceptowania zaproszenia. Spróbuj ponownie później...",
      "There was an error while rejecting invite. Try again later...": "Wystapił błąd podczas odrzucania zaproszenia. Spróbuj ponownie później...",
      "Invite": "Zaproś",
      "Index must be a number": "Numer albumu musi być liczbą",
      "Type a message":"Napisz wiadomość",
      "Invitation": "Zaproszenie",
      "Decide": "Zdecyduj",
      "Accept": "Akceptuj",
      "Reject": "Odrzuć",
      "You have been invited to the team in course {0} by {1}": "Zostałeś zaproszony do zespołu na przedmiocie {0} przez {1}",
      "There was an error while getting your invites. Try again later...": "Wystapił błąd podczas pobierania zaproszeń. Spróbuj ponownie później...",
      "Monday": "Poniedziałek",
      "Tuesday": "Wtorek",
      "Wednesday": "Środa",
      "Thursday": "Czwartek",
      "Friday": "Piątek",
      "Saturday": "Sobota",
      "Sunday": "Niedziela",
      "Relations": "Relacje",
      "Leave": "Opuść",
      "Do you want to leave {0} subject's team?": "Czy chcesz opuścić zespół przedmiotu {0}?",
      "And with": "Powiązane z:",
      "Xor with": "Wykluczone z:",
      "There was an error while getting users. Try again later...": "Wystapił błąd podczas pobierania użytkowników. Spróbuj ponownie później...",
      "User must be selected from list": "Użytkownik musi zostać wybrany z listy",
      "Type in a username...": "Wpisz nazwę użytkownika...",
      "Username cannot contain special characters": "Nazwa użytkownika nie może zawierać znaków specjalnych",
      "Start new semester": "Rozpocznij nowy semestr",
      "Calculate exchanges": "Policz wymiany",
      "Are you sure you want your calculate exchanges for selected subjects?": "Jesteś pewien, że chcesz policzyć wymiany dla zaznaczonych przedmiotów?",
      "Are you sure you want start new semster? This action will delete all current data about exchanges and timetables": "Jesteś pewien, że chcesz ropocząć nowy semestr? Ta akcja skasuje wszystkie obecne informacje o wymianach i planach uzytkowników.",
      "New semster was started": "Nowy semestr został rozpoczęty",
      "There was an error while sending request to start new semester. Try again later...": "Wystapił błąd podczas rozpoczynania nowego semstru. Spróbuj ponownie później...",
      "Exchanges were calculated": "Wymiany zostały policzone",
      "There was an error while calculating exchanges. Try again later...": "Wystapił błąd podczas wyliczania wymian. Spróbuj ponownie później...",
      "Yes": "Tak",
      "No": "Nie",
    },
    "en" : {
      "Something went wrong. Try again later...": "Something went wrong. Try again later...",
      "You have been logged out": "You have been logged out",
      "Session expired": "Session expired",
      "There was an error while submiting join. Try again later...": "An error occurred while submiting join. Try again later...",
      "There was an error while removing exchange. Try again later...":"An error occurred while removing exchange. Try again later...",
      "There was an error while inviting user. Try again later...":"An error occurred while inviting user. Try again later...",
      "There was an error while removing invite. Try again later...":"An error occurred while removing invite. Try again later...",
      "There was an error while getting timetable. Try again later...":"An error occurred while getting plan. Try again later...",
      "There was an error while getting exchanges relations. Try again later...":"An error occurred while getting exchanges relations. Try again later...",
      "There was an error while getting exchanges. Try again later...":"An error occurred while getting exchanges. Try again later...",
      "There was an error while adding relations between exchanges. Try again later...":"An error occurred while adding relations between exchanges. Try again later...",
      "There was an error while removing relations between exchanges. Try again later...":"An error occurred while removing relations between exchanges. Try again later...",
      "There was an error while getting your teams. Try again later...":"An error occurred while getting your teams. Try again later...",
      "There was an error while changing your username. Try again later...":"An error occurred while setting your username. Try again later...",
      "Your username was saved":"Your username was changed",
      "There was an error while adding leader. Try again later...":"An error occurred while adding leader. Try again later...",
      "There was an error while getting admins and leaders. Try again later...":"An error occurred while getting admins and leaders. Try again later...",
      "There was an error while removing leader. Try again later...": "An error occurred while removing leader. Try again later...",
      "There was an error while adding admin. Try again later...":"An error occurred while adding admin. Try again later...",
      "There was an error while removing admin. Try again later...":"An error occurred while removing admin. Try again later...",
      "There was an error while sending data to dean's office. Try again later...":"An error occurred while sending email to faculty. Try again later...",
      "There was an error while sending request. Try again later...":"An error occurred while sending request. Try again later...",
      "Welcome to UsosFix": "Welcome to",
      "Settings": "User panel",
      "My exchanges": "My exchanges",
      "My timetable": "My timetable",
      "My teams": "My teams",
      "Messages": "Message box",
      "Back" : "Back",
      "Go back" : "Go back",
      "Subject": "Subject",
      "Group" : "Group",
      "Class type": "Class type",
      "Lecturer": "Instructor",
      "Place": "Place",
      "Meeting times": "Meetings dates",
      "Max number of students": "Student's limit",
      "Current number of students": "Student's number",
      "Irregular": "Irregular",
      "Join": "Join",
      "Stop joining": "Stop joining",
      "Already in this group": "Already in this group",
      "currentPageReportTemplate": "Showing {first} to {last} of {totalRecords}",
      "No students found": "No participants found",
      "Loading students, please wait...": "Loading participants, please wait...",
      "you": "you",
      "Already in your group": "Already in your group",
      "Stop inviting": "Stop inviting",
      "Invite user": "Invite user",
      "Currently, you don't have any active exchanges": "Currently, you don't have any active exchanges",
      "Add dependency relation": "Add binding relation",
      "Close": "Close",
      "To": "To",
      "From": "From",
      "No exchanges to choose from": "No exchanges to choose from",
      "Add excluding relation": "Add exclusion relation",
      "Cancel relation": "Cancel relation",
      "Login": "Log in",
      "Loading exchanges, please wait...": "Loading exchanges, please wait...",
      "Add dependency relation to ": "Add binding relation to ",
      "Add excluding relation to ": "Add exclusion relation to ",
      "In relation" : "In {0} relation with",
      "pro": "Project",
      "wyk": "Lecture",
      "lab": "Laboratory",
      "cwi": "Tutorials",
      "Pending": "Pending",
      "Remove user": "Remove",
      "Stop inviting user": "Remove",
      "Currently, you don't have any teams": "Currently, you don't have any teams",
      "Loading groups, please wait...":"Loading groups, please wait...",
      "Confirmation": "Confirmation",
      "Are you sure you want to remove user": "Are you sure you want to remove user {0} from group {1}",
      "Are you sure you want to stop inviting": "Are you sure you want to stop inviting user {0} to group {1}",
      "Weekends": "Weekends",
      "Schedule from Usos": "Usos based timetable",
      "My schedule": "Exchanges based timetable",
      "First name": "First name",
      "Last name": "Last name",
      "Username":"Username",
      "Save": "Change",
      "Index": "Student number",
      "Language":"Language",
      "Reveal Yourself": "Reveal Yourself",
      "Already revealed": "Already revealed",
      "Logout": "Log out",
      "Leader's section": "Leader panel",
      "Send data to administration": "Send data to dean's office",
      "Admin's section": "Admin panel",
      "No leaders": "No leaders",
      "Loading leaders, please wait...": "Loading leaders, please wait...",
      "List of leaders": "Leader list",
      "Remove": "Remove",
      "Add": "Add",
      "index": "student number",
      "Add admin": "Add admin",
      "List of admins": "Admin list",
      "Add leader": "Add leader",
      "Loading admins, please wait...": "Loading admins, please wait...",
      "No admins": "No admins",
      "pl": "Polish",
      "en": "English",
      "And": "Binding",
      "Xor": "Exclusion",
      "Are you sure you want your name to be visible to all users in this group? This cannot be reversed": "Are you sure you want your name to be visible to all users?",
      "Are you sure you want to take away admin privileges from user with ID: {0}?": "Are you sure you want to take away admin privileges from user with indexem: {0}?",
      "Are you sure you want to take away leader privileges from user with ID: {0}?": "Are you sure you want to take away leader privileges from user with indexem: {0}?",
      "Are you sure you want to grant leader privileges to user with ID: {0}?": "Are you sure you want to grant leader privileges to user with indexem: {0}?",
      "Are you sure you want to grant admin privileges to user with ID: {0}?": "Are you sure you want to grant admin privileges to user with indexem: {0}?",
      "Are you sure you want send current state to faculty administrator?": "Are you sure you want send current state to faculty administrator?",
      "Cannot remove last admin": "Cannot remove last admin",
      "Admin ID must be a number": "Admin ID must be a number",
      "Leader ID must be a number":"Leader ID must be a number",
      "Students": "Participants",
      "Mail to faculty was sent": "Mail to faculty was sent",
      "Subjects": "Subjects",
      "Loading subjects, please wait...": "Loading subjects, please wait...",
      "No subjects to choose from": "No subjects to choose from",
      "There was an error while getting messages. Try again later...": "An error occurred while getting messages. Try again later...",
      "There was an error while getting chats. Try again later...": "An error occurred while getting chats. Try again later...",
      "There was an error while sending message. Try again later...": "An error occurred while sending message. Try again later...",
      "User invited": "User invited",
      "There was an error while sending invite. Try again later...": "An error occurred while sending invite. Try again later...",
      "There was an error while accepting invite. Try again later...": "An error occurred while accepting invite. Try again later...",
      "There was an error while rejecting invite. Try again later...": "An error occurred while rejecting invite. Try again later...",
      "Invite": "Invite",
      "Index must be a number": "Stundet number must be a number",
      "Type a message": "Type a message",
      "Invitation": "Invitation",
      "Decide": "Decide",
      "Accept": "Accept",
      "Reject": "Reject",
      "You have been invited to the team in course {0} by {1}": "You have been invited to the team in course {0} by {1}",
      "There was an error while getting your invites. Try again later...": "An error occurred while getting your invites. Try again later...",
      "Monday": "Monday",
      "Tuesday": "Tuesday",
      "Wednesday": "Wednesday",
      "Thursday": "Thursday",
      "Friday": "Friday",
      "Saturday": "Saturday",
      "Sunday": "Sunday",
      "Relations": "Relations",
      "Leave": "Leave",
      "Do you want to leave {0} subject's team?": "Do you want to leave {0} subject's team?",
      "And with": "Binds with:",
      "Xor with": "Excludes:",
      "There was an error while getting users. Try again later...": "An error occurred while getting users. Try again later...",
      "User must be selected from list": "User must be selected from list",
      "Type in a username...": "Type in a username...",
      "Username cannot contain special characters": "Username cannot contain special characters",
      "Start new semester": "Start new semester",
      "Calculate exchanges": "Calculate exchanges",
      "Are you sure you want your calculate exchanges for selected subjects?": "Are you sure you want your calculate exchanges for selected subjects?",
      "Are you sure you want start new semster? This action will delete all current data about exchanges and timetables": "Are you sure you want start new semster? This action will delete all current data about exchanges and timetables",
      "New semster was started": "New semster was started",
      "There was an error while sending request to start new semester. Try again later...": "There was an error while sending request to start new semester. Try again later...",
      "Exchanges were calculated": "Exchanges were calculated",
      "There was an error while calculating exchanges. Try again later...": "There was an error while calculating exchanges. Try again later...",
      "Yes": "Yes",
      "No": "No",
    },
  }
};

const getters = {
  getString: state => string => state.translations[state.currentLanguage][string],
  getLang: state => object => object[state.currentLanguage],
  getLanguage: state => state.currentLanguage
};

const actions = {
  async changeLanguage({commit}, language) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Account/SetLanguage?token=${token}&languageString=${language == "en"? "English" : "Polish"}`
      ).then(() => {
        commit("setLanguage", language);
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while setting language. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
};

const mutations = {
  setLanguage: (state, language) => state.currentLanguage = language,
};

export default {
  state,
  getters,
  actions,
  mutations
};