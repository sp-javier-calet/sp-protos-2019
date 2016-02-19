using SocialPoint.Base;

namespace SocialPoint.ServerSync
{
    public class PackedCommand
    {
        public Command Command;
        public ErrorDelegate Finished;

        public PackedCommand(Command cmd, ErrorDelegate finish = null)
        {
            DebugUtils.Assert(cmd != null);
            Command = cmd;
            Finished = finish;
        }
    }
}
