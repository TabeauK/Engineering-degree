namespace UsosFix.ViewModels
{
    public record GroupOverview(int Id, LanguageString DisplayName, int GroupNumber, string ClassType)
    {
        public GroupOverview(Models.Group model) : this(model.Id, model.Subject.Name, model.GroupNumber, model.ClassType)
        {
        }
    }
}
