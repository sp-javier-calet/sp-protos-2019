using System;
using System.Collections.Generic;
using Zenject;
using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;
using SocialPoint.AdminPanel;

public class ScriptEventsInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToSingleInstance(this);

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

        Container.Rebind<IEventDispatcher>().ToSingle<EventDispatcher>();
        Container.Rebind<IScriptEventDispatcher>().ToSingle<ScriptEventDispatcher>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelScriptEvents>();
    }

    FamilyParser<IScriptCondition> CreateScriptConditionParser(InjectContext ctx)
    {
        var children = ctx.Container.Resolve<List<IChildParser<IScriptCondition>>>();
        return new FamilyParser<IScriptCondition>(children);
    }

    ScriptModelParser CreateScriptModelParser(InjectContext ctx)
    {
        var condParser = ctx.Container.Resolve<IParser<IScriptCondition>>();
        return new ScriptModelParser(condParser);
    }

    public void Initialize()
    {
        {
            var dispatcher = Container.Resolve<IEventDispatcher>();
            var bridges = Container.Resolve<List<IEventsBridge>>();
            foreach(var bridge in bridges)
            {
                dispatcher.AddBridge(bridge);
            }
        }
        {
            var dispatcher = Container.Resolve<IScriptEventDispatcher>();
            var bridges = Container.Resolve<List<IScriptEventsBridge>>();
            foreach(var bridge in bridges)
            {
                dispatcher.AddBridge(bridge);
            }
        }
    }

}