using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class ServerInfo
    {
        public ServerStatus Status;
        public string Log;

        public ServerInfo(ServerStatus status, string log)
        {
            Status = status;
            Log = log;
        }
    }

    public enum ServerStatus { Ok, Warning, Error };
}