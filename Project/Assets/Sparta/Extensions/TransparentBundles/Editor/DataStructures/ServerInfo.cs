using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class ServerInfo
    {
        public ServerStatus Status;
        public string Log;
        public Dictionary<int, BundleOperation> ProcessingQueue;

        public ServerInfo(ServerStatus status, string log, Dictionary<int, BundleOperation> processingQueue)
        {
            Status = status;
            Log = log;
            ProcessingQueue = processingQueue;
        }
    }

    public enum ServerStatus
    {
        Ok,
        Warning,
        Error}

    ;
}