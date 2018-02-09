using System;
using System.Collections.Generic;
using System.Threading;
using SocialPoint.Base;
using SocialPoint.Console;
using SocialPoint.Utils;

namespace SocialPoint.Examples.Sockets
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if(args.Length < 7)
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
            List<SocketClient> _netClientsList = new List<SocketClient>();
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

            if(!portOK || !updateTimeOK || !numClientsOK || !testTimeOK)
            {
                Log.d("Please enter clients +  number of clients, the protocol(tcp|udp), the port and the update time: ex -> clients --numClients=10 --protocol=tcp --ipAdress=52.90.17.72 --port=7777 --update=100 --testTime=120");
                return;
            }
            SocketClient.Protocol protocol = SocketClient.Protocol.TCP;
            if(cmd["protocol"].Value == "tcp")
            {
                protocol = SocketClient.Protocol.TCP;
            }
            if(cmd["protocol"].Value == "udp")
            {
                protocol = SocketClient.Protocol.UDP;
            }

            float scaleTime = 1000f / updateTime;
            int matchId = 0;
            SocketClient client = null;
            for(int i = 0; i < numClients; i++)
            {
                client = new SocketClient(protocol, ipAdress, port, "matchID" + matchId.ToString(), updateScheduler);
                client.Connect();
                _netClientsList.Add((client));
                updateScheduler.Update(scaleTime, scaleTime);

                if(i % 2 == 1)
                {
                    matchId++;
                }
            }
            for(int i = 0; i < _netClientsList.Count; i++)
            {
                _netClientsList[i].SendMessage("Message -> TestMessage Client: " + i + " Ticks: " + DateTime.UtcNow.Ticks);
                updateScheduler.Update(scaleTime, scaleTime);
            }

            DateTime startTime = DateTime.UtcNow;

            while(DateTime.UtcNow - startTime < TimeSpan.FromSeconds(testTime))
            {
                if(System.Console.KeyAvailable)
                {
                    return;
                }

                updateScheduler.Update(scaleTime, scaleTime);
                Thread.Sleep(updateTime);
            }

            for(int i = 0; i < _netClientsList.Count; i++)
            {
                _netClientsList[i].Disconnect();
            }
        }
    }
}
