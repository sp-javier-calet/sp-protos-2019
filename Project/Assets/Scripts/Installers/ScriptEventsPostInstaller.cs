using System.Collections.Generic;
using Zenject;
using SocialPoint.ScriptEvents;


public class ScriptEventsPostInstaller : MonoInstaller
{
    /**
     * this installer will add all the bridges defined in the scene installers
     * to the dispatchers that are created in the global composition root
     */
    public override void InstallBindings()
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