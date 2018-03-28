using System.Collections;
using System.IO;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Lifecycle;
using SocialPoint.Lockstep;

namespace Examples.Multiplayer.Lockstep
{
    public class LockstepPlayReplayLifecycleComponent : ISetupComponent, IStartComponent, IErrorDispatcher
    {
        readonly LockstepReplay _replay;
        readonly string _replayPath;

        public LockstepPlayReplayLifecycleComponent(LockstepClient client, LockstepCommandFactory commandFactory, string replayPath)
        {
            _replayPath = replayPath;
            _replay = new LockstepReplay(client, commandFactory);
        }

        IErrorHandler IErrorDispatcher.Handler { get; set; }

        IEnumerator ISetupComponent.Setup()
        {
            try
            {
                var stream = new FileStream(_replayPath, FileMode.Open);
                var reader = new SystemBinaryReader(stream);
                _replay.Deserialize(reader);
                stream.Close();
                stream.Dispose();
            }
            catch(IOException e)
            {
                TriggerError(new Error("Could not load replay file: " + e));
            }

            return null;
        }

        void IStartComponent.Start()
        {
            _replay.Replay();
        }

        void TriggerError(Error err)
        {
            if(((IErrorDispatcher) this).Handler != null)
            {
                ((IErrorDispatcher) this).Handler.OnError(err);
            }
        }
    }
}