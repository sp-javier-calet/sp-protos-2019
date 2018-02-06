#define SPARTA_LOG_VERBOSE

using System.Threading;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Sockets
{
    class Program
    {

        public static void Main(string[] args)
        {
            int sleepTime = 0;

            UpdateScheduler updateScheduler = new UpdateScheduler();
            if (args.Length == 0 || args.Length == 1 || args.Length == 2)
            {
                Log.d("Please enter (TCP or UDP), the port and the ThreadSleepTime: ex -> TCP 8888 100");
                return;
            }
            if (args[0] == "TCP" || args[0] == "UDP")
            {
                int port;
                bool portOK = int.TryParse(args[1], out port);

                bool sleepTimeOK = int.TryParse(args[2], out sleepTime);

                if (portOK && sleepTimeOK)
                {
                    SocketServer server;
                    if (args[0] == "TCP")
                    {
                        server = new SocketServer(SocketServer.Protocol.TCP, port, updateScheduler);
                        server.Start();
                    }
                    else if (args[0] == "UDP")
                    {
                        server = new SocketServer(SocketServer.Protocol.UDP, port, updateScheduler);
                        server.Start();
                    }
                }
                else
                {
                    Log.d("Please add a correct port and a corect ThreadSleepTime: ex -> 8888 100");
                    return;
                }
            }
            else
            {
                Log.d("Please enter (TCP or UDP): ex -> TCP");
                return;
            }

            float updateTime = 1000f / sleepTime;
            while (true)
            {
                if (System.Console.KeyAvailable)
                {
                    return;
                }

                updateScheduler.Update(updateTime, updateTime);
                Thread.Sleep(sleepTime);
            }
        }
    }
}
