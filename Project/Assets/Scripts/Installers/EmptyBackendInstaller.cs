using System;
using Zenject;
using SocialPoint.Attributes;
using SocialPoint.Events;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Crash;
using System.Text;

public class EmptyBackendInstaller : MonoInstaller
{
    [Inject]
    GameModel _model;
    
    [Inject]
    IParser<GameModel> _gameParser;
    
    public override void InstallBindings()
    {
        if(!Container.HasBinding<IEventTracker>())
        {
            Container.Bind<IEventTracker>().ToSingle<EmptyEventTracker>();
            Container.Bind<IDisposable>().ToSingle<EmptyEventTracker>();
        }
        if(!Container.HasBinding<ILogin>())
        {
            Container.Bind<ILogin>().ToMethod(CreateEmptyLogin);
            Container.Bind<IDisposable>().ToMethod(CreateEmptyLogin);
        }
        if(!Container.HasBinding<ICommandQueue>())
        {
            Container.Bind<ICommandQueue>().ToSingle<EmptyCommandQueue>();
            Container.Bind<IDisposable>().ToSingle<EmptyCommandQueue>();
        }
        if(!Container.HasBinding<ICrashReporter>())
        {
            Container.Bind<ICrashReporter>().ToSingle<EmptyCrashReporter>();
        }
        if(_model.Player == null)
        {
            var defaultGame = (UnityEngine.Resources.Load("game") as UnityEngine.TextAsset).text;
            var json = new JsonAttrParser().ParseString(defaultGame).AsDic;
            var newModel = _gameParser.Parse(json);
            _model.Assign(newModel);
        }
    }
    
    EmptyLogin _emptyLogin = null;
    EmptyLogin CreateEmptyLogin(InjectContext ctx)
    {
        if(_emptyLogin == null)
        {
            _emptyLogin = new EmptyLogin();
        }
        return _emptyLogin;
    }
}