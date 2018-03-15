using System.IO;
using SocialPoint.IO;
using SocialPoint.Lifecycle;
using SocialPoint.Lockstep;

namespace Examples.Multiplayer.Lockstep
{
    public class LockstepStoreReplayLifecycleComponent : ICleanupComponent, IStartComponent
    {
        readonly LockstepReplay _replay;
        readonly string _replayPath;

        public LockstepStoreReplayLifecycleComponent(LockstepClient client, LockstepCommandFactory commandFactory, string replayPath)
        {
            _replayPath = replayPath;
            _replay = new LockstepReplay(client, commandFactory);
        }

        void ICleanupComponent.Cleanup()
        {
            var stream = new FileStream(_replayPath, FileMode.OpenOrCreate);
            var writer = new SystemBinaryWriter(stream);
            _replay.Serialize(writer);
            stream.Close();
            stream.Dispose();
        }

        void IStartComponent.Start()
        {
            _replay.Record();
        }
    }
}