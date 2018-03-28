using FixMath.NET;
using SocialPoint.Lifecycle;
using SocialPoint.Lockstep;
using SocialPoint.Pooling;
using UnityEngine;

namespace Examples.Multiplayer.Lockstep
{
    internal class ClickInputEvent
    {
        public Vector3 Position;
    }

    public class ClickInputValidator : IEventValidator<ClickInputEvent>
    {
        readonly GameNetworkSceneController _sceneController;
        readonly Config _config;

        public ClickInputValidator(GameNetworkSceneController sceneController, Config config)
        {
            _sceneController = sceneController;
            _config = config;
        }

        bool IEventValidator<ClickInputEvent>.Validate(ClickInputEvent ev)
        {
            if(_sceneController.LocalPlayerMana < _config.UnitCost)
            {
                return false;
            }
            return true;
        }
    }

    public class ClickInputSuccessEventHandler : IEventHandler<ClickInputEvent>
    {
        readonly ClientConfig _config;
        readonly LockstepClient _lockstep;

        public ClickInputSuccessEventHandler(LockstepClient lockstep, ClientConfig config)
        {
            _config = config;
            _lockstep = lockstep;
        }

        void IEventHandler<ClickInputEvent>.Handle(ClickInputEvent ev)
        {
            var p = ev.Position;
            var cmd = new ClickCommand((Fix64) p.x, (Fix64) p.y, (Fix64) p.z);

            GameObject loading = null;
            if(_config.LoadingPrefab != null)
            {
                loading = UnityObjectPool.Spawn(_config.LoadingPrefab, _config.Container, p, Quaternion.identity);
            }

            _lockstep.AddPendingCommand(cmd, c => FinishLoading(loading));
        }

        static void FinishLoading(GameObject loading)
        {
            UnityObjectPool.Recycle(loading);
        }
    }

    public class ClickInputFailureEventHandler : IEventHandler<ClickInputEvent>
    {
        void IEventHandler<ClickInputEvent>.Handle(ClickInputEvent ev)
        {
            SocialPoint.Base.Log.e("Not enough mana");
        }
    }
}