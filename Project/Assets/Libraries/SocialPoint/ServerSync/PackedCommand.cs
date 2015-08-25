
using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.Network;

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
