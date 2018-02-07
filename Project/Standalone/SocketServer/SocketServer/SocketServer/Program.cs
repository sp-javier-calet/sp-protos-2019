#define SPARTA_LOG_VERBOSE

using System.Threading;
using SocialPoint.Base;
using SocialPoint.Console;
using SocialPoint.Utils;

namespace SocialPoint.Sockets
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Log.d("Please enter server +  the protocol(tcp|udp), the port and the update time: ex -> server --protocol=tcp --port=7777 --update=100");
                return;
            }

            ConsoleApplication console = new ConsoleApplication();
            var serverCmd = new ConsoleCommand()
                .WithDelegate(OnServerCommand)
                .WithDescription("Start a socket server")
                .WithOption(new ConsoleCommandOption("protocol")
                            .WithDescription("The protocol used for the server (tcp|udp)"))
                .WithOption(new ConsoleCommandOption("port")
                            .WithDescription("The port used for listening the server (7777)"))
                .WithOption(new ConsoleCommandOption("update")
                            .WithDescription("Update time of the server (100)"));
            console.AddCommand("server", serverCmd);
            console.Run(args);

        }

        static void OnServerCommand(ConsoleCommand cmd)
        {
            UpdateScheduler updateScheduler = new UpdateScheduler();

            int port;
            int updateTime;

            bool portOK = int.TryParse(cmd["port"].Value, out port);
            bool updateTimeOK = int.TryParse(cmd["update"].Value, out updateTime);
           
            if(!portOK || !updateTimeOK)
            {
                Log.d("Please enter server +  the protocol(tcp|udp), the port and the update time: ex -> server --protocol=tcp --port=7777 --update=100");
                return;
            }

            SocketServer server = null;
            if(cmd["protocol"].Value == "tcp")
            {
                server = new SocketServer(SocketServer.Protocol.TCP, port, updateScheduler);
            }
            if (cmd["protocol"].Value == "udp")
            {
                server = new SocketServer(SocketServer.Protocol.UDP, port, updateScheduler);
            }
            server.Start();

            float scaleTime = 1000f / updateTime;
            while (true)
            {
                if (System.Console.KeyAvailable)
                {
                    return;
                }

                updateScheduler.Update(scaleTime, scaleTime);
                Thread.Sleep(updateTime);
            }
        }
    }
}
