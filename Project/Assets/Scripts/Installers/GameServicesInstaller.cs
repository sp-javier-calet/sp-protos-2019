//-----------------------------------------------------------------------
// GameServicesInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Notifications;

[InstallerGameCategory]
public class GameServicesInstaller : Installer, IInitializable
{
    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<IInitializable>().ToInstance(this);

        // Purchase store // TODO IVAN
        //container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);
    }

    public void Initialize(IResolutionContainer container)
    {
        SetupNotificationsProvider(container);
        SetupGameLoadingOperations(container);
    }

    static void SetupNotificationsProvider(IResolutionContainer container)
    {
        var manager = container.Resolve<INotificationManager>();
        if(manager == null)
        {
            return;
        }

        manager.NotificationsProvider = () =>
        {
            var notify = new Notification(10, Notification.OffsetType.None)
            {
                Title = "Notification!", Message = "This is a notification manager notification."
            };

            return new List<Notification>
            {
                notify
            };
        };
    }

    LoadingOperation _gameOperation;

    void SetupGameLoadingOperations(IResolutionContainer container)
    {
        var manager = container.Resolve<ILoadingManager>();

        //Example of how to append LoadingOperations to the LoadingManager.
        manager.GameOperationsSource = () =>
        {
            _gameOperation = new LoadingOperation(0.0f, DoGameOperation);

            return new List<ILoadingOperation>
            {
                _gameOperation
            };
        };
    }

    void DoGameOperation()
    {
        //
        // Do operation stuff here.
        //

        _gameOperation.Finish();

        Log.i("GameOperation Finished");
    }
}
