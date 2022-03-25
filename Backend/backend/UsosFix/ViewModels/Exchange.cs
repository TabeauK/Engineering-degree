using UsosFix.Models;

namespace UsosFix.ViewModels;

public record Exchange(int Id, GroupOverview GroupFrom, GroupOverview GroupTo, ExchangeState ExchangeState)
{
    public Exchange(Models.Exchange model) :
        this(model.Id, new GroupOverview(model.SourceGroup), new GroupOverview(model.TargetGroup), model.State) { }
}