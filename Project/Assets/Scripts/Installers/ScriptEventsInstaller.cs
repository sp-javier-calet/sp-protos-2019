using System;
using System.Collections.Generic;
using Zenject;
using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;
using SocialPoint.AdminPanel;

public class ScriptEventsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<FixedConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<NameConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<ArgumentsConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<AndConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<OrConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<NotConditionParser>();

        Container.Bind<IEventsBridge>().ToSingle<ScriptBridge>();
        Container.Bind<IScriptEventsBridge>().ToSingle<ScriptBridge>();
        
        Container.Rebind<IParser<IScriptCondition>>().ToSingleMethod<FamilyParser<IScriptCondition>>(CreateScriptConditionParser);
        Container.Rebind<IParser<ScriptModel>>().ToSingleMethod<ScriptModelParser>(CreateScriptModelParser);

        Container.Rebind<IEventDispatcher>().ToSingleMethod<EventDispatcher>(CreateEventDispatcher);
        Container.Rebind<IScriptEventDispatcher>().ToSingleMethod<ScriptEventDispatcher>(CreateScriptEventDispatcher);

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelScriptEvents>();
    }

    public FamilyParser<IScriptCondition> CreateScriptConditionParser(InjectContext ctx)
    {
        var children = Container.Resolve<List<IChildParser<IScriptCondition>>>();
        return new FamilyParser<IScriptCondition>(children);
    }

    public ScriptModelParser CreateScriptModelParser(InjectContext ctx)
    {
        var condParser = Container.Resolve<IParser<IScriptCondition>>();
        return new ScriptModelParser(condParser);
    }

    public EventDispatcher CreateEventDispatcher(InjectContext ctx)
    {
        var dispatcher = Container.Instantiate<EventDispatcher>();
        dispatcher.AddBridges(Container.Resolve<List<IEventsBridge>>());
        return dispatcher;
    }

    public ScriptEventDispatcher CreateScriptEventDispatcher(InjectContext ctx)
    {
        var dispatcher = Container.Instantiate<ScriptEventDispatcher>();
        dispatcher.AddBridges(Container.Resolve<List<IScriptEventsBridge>>());
        return dispatcher;
    }

}