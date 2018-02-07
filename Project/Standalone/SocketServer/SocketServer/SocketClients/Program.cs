#define SPARTA_LOG_VERBOSE

using System;
using System.Threading;
using SocialPoint.Base;
using SocialPoint.Console;
using SocialPoint.Utils;

namespace SocialPoint.Sockets
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            if (args.Length < 3)
            {
                Log.d("Please enter clients +  number of clients, the protocol(tcp|udp), the port and the update time: ex -> clients --numClients=10 --protocol=tcp --ipAdress=52.90.17.72 --port=7777 --update=100 --testTime=120");
                return;
            }

            ConsoleApplication console = new ConsoleApplication();
            var clientCmd = new ConsoleCommand()
                .WithDelegate(OnCommandCommand)
                .WithDescription("Test multiple clients stress")
                .WithOption(new ConsoleCommandOption("numClients")
                            .WithDescription("Number of clients to test"))
                .WithOption(new ConsoleCommandOption("protocol")
                            .WithDescription("The protocol used for the server (tcp|udp)"))
                .WithOption(new ConsoleCommandOption("ipAdress")
                            .WithDescription("IP adress of the server (52.90.17.72)"))
                .WithOption(new ConsoleCommandOption("port")
                            .WithDescription("The port used for listening to the server (7777)"))
                .WithOption(new ConsoleCommandOption("update")
                            .WithDescription("Update time of the server (100)"))
                .WithOption(new ConsoleCommandOption("testTime")
                            .WithDescription("Time to test in seconds (120)"));
            console.AddCommand("clients", clientCmd);
            console.Run(args);

        }

        static void OnCommandCommand(ConsoleCommand cmd)
        {

            UpdateScheduler updateScheduler = new UpdateScheduler();

            int numClients;
            int port;
            int updateTime;
            int testTime;
            string ipAdress = cmd["ipAdress"].Value;

            bool numClientsOK = int.TryParse(cmd["numClients"].Value, out numClients);
            bool portOK = int.TryParse(cmd["port"].Value, out port);
            bool updateTimeOK = int.TryParse(cmd["update"].Value, out updateTime);
            bool testTimeOK = int.TryParse(cmd["testTime"].Value, out testTime);

            if (!portOK || !updateTimeOK || !numClientsOK || !testTimeOK)
            {
                Log.d("Please enter clients +  number of clients, the protocol(tcp|udp), the port and the update time: ex -> clients --numClients=10 --protocol=tcp --ipAdress=52.90.17.72 --port=7777 --update=100 --testTime=120");
                return;
            }

            SocketClients clients = null;
            if (cmd["protocol"].Value == "tcp")
            {
                clients = new SocketClients(numClients,SocketClients.Protocol.TCP,ipAdress, port, updateScheduler);
            }
            if (cmd["protocol"].Value == "udp")
            {
                clients = new SocketClients(numClients, SocketClients.Protocol.UDP, ipAdress, port, updateScheduler);
            }
            clients.Connect();

            float scaleTime = 1000f / updateTime;

            DateTime startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(testTime))
            {
                if (System.Console.KeyAvailable)
                {
                    return;
                }

                updateScheduler.Update(scaleTime, scaleTime);
                Thread.Sleep(updateTime);
            }

            clients.Disconnect();
        }
    }
}
