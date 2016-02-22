using System;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.ServerSync
{
    public class PackedCommand
    {
        public Command Command;
        public Action<Attr, Error> Finished;

        public PackedCommand(Command cmd, Action<Attr, Error> finish = null)
        {
            DebugUtils.Assert(cmd != null);
            Command = cmd;
            Finished = finish;
        }
    }
}
