using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class ServerInfo
    {
        public ServerStatus Status;
        public string Log;
        public Dictionary<int, ServerTask> ProcessingQueue;
        public float Progress;
        public string ProgressMessage;

        public ServerInfo(ServerStatus status, string log, Dictionary<int, ServerTask> processingQueue, float progress, string progressMessage)
        {
            Status = status;
            Log = log;
            ProcessingQueue = processingQueue;
            Progress = progress;
            ProgressMessage = progressMessage;
        }
    }

    public enum ServerStatus
    {
        Ok,
        Warning,
        Error
    }


}
