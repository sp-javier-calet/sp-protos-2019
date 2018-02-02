using System;
using SocialPoint.Utils;

namespace SocialPoint.Sockets
{
    class Program
    {
        public static void Main(string[] args)
        {
            UpdateScheduler updateScheduler = new UpdateScheduler();
            if(args.Length == 0 || args.Length == 1)
            {
                Console.WriteLine("Please enter (TCP or UDP) and the port: ex -> TCP 8888");
                return;
            }
            if(args[0] == "TCP" || args[0] == "UDP")
            {
                int port;
                bool portOK = int.TryParse(args[1], out port);
                if(portOK)
                {
                    SocketServer server;
                    if(args[0] == "TCP")
                    {
                        Console.WriteLine("INSTANTIATE TCP SERVER");
                        server = new SocketServer(SocketServer.Protocol.TCP, port, updateScheduler);
                        server.Start();
                    }
                    else if(args[0] == "UDP")
                    {
                        Console.WriteLine("INSTANTIATE UDP SERVER");
                        server = new SocketServer(SocketServer.Protocol.UDP, port, updateScheduler);
                        server.Start();
                    }
                }
                else
                {
                    Console.WriteLine("Please add a correct port: ex -> 8888");
                }
            }
            else
            {
                Console.WriteLine("Please enter (TCP or UDP): ex -> TCP");
            }

            while(true)
            {
                if(Console.KeyAvailable)
                {
                    return;
                }

                updateScheduler.Update(1.0f, 1.0f);
            }
        }
    }
}
