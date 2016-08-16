using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;

public interface IModelCondition : IScriptCondition
{
    int RequiredRepetitions{ get; }

    bool ValidateEvent(PlayerModel playerModel, Attr eventData);

    bool ValidateModel(PlayerModel playerModel);
}