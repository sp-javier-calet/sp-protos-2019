using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

public class ScriptEventsInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);

        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<FixedConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<NameConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<ArgumentsConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<AndConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<OrConditionParser>();
        Container.Bind<IChildParser<IScriptCondition>>().ToSingle<NotConditionParser>();

        Container.Bind<ScriptBridge>().ToMethod<ScriptBridge>(CreateScriptBridge);
        Container.Bind<IEventsBridge>().ToLookup<ScriptBridge>();
        Container.Bind<IScriptEventsBridge>().ToLookup<ScriptBridge>();
        
        Container.Rebind<IParser<IScriptCondition>>().ToMethod<FamilyParser<IScriptCondition>>(CreateScriptConditionParser);
        Container.Rebind<IParser<ScriptModel>>().ToMethod<ScriptModelParser>(CreateScriptModelParser);

        Container.Rebind<IEventDispatcher>().ToSingle<EventDispatcher>();
        Container.Rebind<IScriptEventDispatcher>().ToMethod<ScriptEventDispatcher>(CreateScriptEventDispatcher);

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelScriptEvents>(CreateAdminPanel);
    }

    AdminPanelScriptEvents CreateAdminPanel()
    {
        return new AdminPanelScriptEvents(
            Container.Resolve<IScriptEventDispatcher>(),
            Container.Resolve<IParser<ScriptModel>>());
    }

    ScriptEventDispatcher CreateScriptEventDispatcher()
    {
        return new ScriptEventDispatcher(
            Container.Resolve<IEventDispatcher>());
    }

    ScriptBridge CreateScriptBridge()
    {
        return new ScriptBridge(
            Container.Resolve<IParser<ScriptModel>>(),
            Container.Resolve<ICoroutineRunner>());
    }

    FamilyParser<IScriptCondition> CreateScriptConditionParser()
    {
        var children = Container.ResolveList<IChildParser<IScriptCondition>>();
        return new FamilyParser<IScriptCondition>(children);
    }

    ScriptModelParser CreateScriptModelParser()
    {
        var condParser = Container.Resolve<IParser<IScriptCondition>>();
        return new ScriptModelParser(condParser);
    }

    public void Initialize()
    {
        {
            var dispatcher = Container.Resolve<IEventDispatcher>();
            var bridges = Container.ResolveArray<IEventsBridge>();
            for(var i = 0; i < bridges.Length; i++)
            {
                dispatcher.AddBridge(bridges[i]);
            }
        }
        {
            var dispatcher = Container.Resolve<IScriptEventDispatcher>();
            var bridges = Container.ResolveArray<IScriptEventsBridge>();
            for(var i = 0; i < bridges.Length; i++)
            {
                dispatcher.AddBridge(bridges[i]);
            }
        }
    }

}