
using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using SocialPoint.Network;

namespace SocialPoint.ServerSync
{
    public class PackedCommand
    {
        public delegate void FinishDelegate(Error err);

        public Command Command;
        public FinishDelegate Finished;

        public PackedCommand(Command cmd, FinishDelegate finish = null)
        {
            SocialPoint.Base.Debug.Assert(cmd != null);
            Command = cmd;
            Finished = finish;
        }
    }
}
