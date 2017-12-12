﻿using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;

namespace SocialPoint.Network
{
    public class TCPServerListener
    {
        const int ReadLoopIntervalMs = 10;

        internal bool QueueStop { get; set; }

        private TcpListener _listener = null;
        private List<TcpClient> _connectedClients = new List<TcpClient>();
        private List<TcpClient> _disconnectedClients = new List<TcpClient>();
        //        private List<byte> _queuedMsg = new List<byte>();
        private IPAddress _ipAddress;
        private int _port;


        //        public delegate void StartEvent();
        //        public delegate void StopEvent(Exception exception = null);
        public delegate void ConnectClientEvent(TcpClient client);
        public delegate void DisconnectClientEvent(TcpClient client);
        //        public delegate void DataEvent(IPEndPoint remoteIPEP,byte[] data);


        //        public event StartEvent OnStart;
        //        public event StopEvent OnStop;
        public event ConnectClientEvent OnConnectClient;
        public event DisconnectClientEvent OnDisconnectClient;
        //        public event DataEvent OnData;

        public TCPServerListener(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;

            QueueStop = false;

            _listener = new TcpListener(_ipAddress, _port);
        }

        public void Start()
        {
            _listener.Start();

            System.Threading.ThreadPool.QueueUserWorkItem(ListenerLoop);
        }

        void ListenerLoop(object state)
        {
            while (!QueueStop)
            {
                try
                {
                    RunLoopStep();
                }
                catch
                {

                }

                System.Threading.Thread.Sleep(ReadLoopIntervalMs);
            }
            _listener.Stop();
        }

        private void RunLoopStep()
        {
            if (_disconnectedClients.Count > 0)
            {
                var disconnectedClients = _disconnectedClients.ToArray();
                _disconnectedClients.Clear();

                foreach (var client in disconnectedClients)
                {
                    if (OnDisconnectClient != null)
                    {
                        OnDisconnectClient(client);
                    }
                    _connectedClients.Remove(client);
                }
            }

            if (_listener.Pending())
            {
                var newClient = _listener.AcceptTcpClient();
                _connectedClients.Add(newClient);
                if (OnConnectClient != null)
                {
                    OnConnectClient(newClient);
                }
            }

//            _delimiter = _server.Delimiter;
//
//            foreach (var c in _connectedClients)
//            {
//
//                if ( IsSocketConnected(c.Client) == false)
//                {
//                    _disconnectedClients.Add(c);
//                }
//
//                int bytesAvailable = c.Available;
//                if (bytesAvailable == 0)
//                {
//                    //Thread.Sleep(10);
//                    continue;
//                }
//
//                List<byte> bytesReceived = new List<byte>();
//
//                while (c.Available > 0 && c.Connected)
//                {
//                    byte[] nextByte = new byte[1];
//                    c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
//                    bytesReceived.AddRange(nextByte);
//
//                    if (nextByte[0] == _delimiter)
//                    {
//                        byte[] msg = _queuedMsg.ToArray();
//                        _queuedMsg.Clear();
//                        _server.NotifyDelimiterMessageRx(this, c, msg);
//                    } else
//                    {
//                        _queuedMsg.AddRange(nextByte);
//                    }
//                }
//
//                if (bytesReceived.Count > 0)
//                {
//                    _server.NotifyEndTransmissionRx(this, c, bytesReceived.ToArray());
//                }  
//            }
        }

        bool IsSocketConnected(Socket s)
        {
            // https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if ((part1 && part2) || !s.Connected)
                return false;
            else
                return true;
        }
    }
}