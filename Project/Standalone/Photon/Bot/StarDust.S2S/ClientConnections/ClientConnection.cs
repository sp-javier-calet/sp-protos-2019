// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientConnection.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The client connection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ClientConnections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;

    using ExitGames.Concurrency.Fibers;
    using ExitGames.Logging;
    using ExitGames.Threading;

    using Photon.SocketServer;
    using Photon.SocketServer.ServerToServer;
    using Photon.Stardust.S2S.Server.ConnectionStates;
    using Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing;
    using Photon.Stardust.S2S.Server.Diagnostics;
    using Photon.Stardust.S2S.Server.Enums;

    public class ClientConnection 
    {
        #region Constants and Fields
        
        public const int MaxMatchmakingRetries = 10;

        public int MatchmakingRetryCount = 0;

        public string GameServerAddress { get; set; }

        protected readonly Application Application; 

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        protected static readonly Stopwatch watch = Stopwatch.StartNew();

        public PoolFiber Fiber { get; private set; }

        public string GameName { get; set; }

        public string LobbyName { get; set; }

        public bool StayInLobby { get; set; }

        public int Number { get; set; }

        public string AuthenticationToken { get; set; }

        public GamingPeer Peer { get; private set; }

        protected IDisposable counterTimer;

        protected IDisposable sendFlushTimer;

        protected IDisposable sendReliableTimer;

        protected IDisposable sendUnreliableTimer;

        protected Random random = new Random();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ClientConnection" /> class.
        /// </summary>
        /// <param name = "gameName">
        ///   The game Name.
        /// </param>
        /// <param name = "number">the player number</param>
        public ClientConnection(string gameName, string lobbyName, int number, bool stayInLobby, Application application)
        {
            // movement channel + data channel
            this.Application = application; 
            this.GameName = gameName;
            this.LobbyName = lobbyName;
            this.StayInLobby = stayInLobby; 
            this.Number = number;
            this.State = Disconnected.Instance;
            this.Fiber = new PoolFiber(new FailSafeBatchExecutor());
            this.Fiber.Start();
        }

        #endregion

        #region Events

        #endregion

        #region Properties

        public IConnectionState State { get; set; }

        #endregion

        #region Public Methods

        public bool ConnectToServer()
        {
            return this.ConnectToServer(Settings.ServerAddress, Settings.Protocol, "Master"); 
        }

        public bool ConnectToServer(string address, NetworkProtocolType protocol, string application)
        {
            bool result;
            string ip = address.Split(':')[0];
            string port = address.Split(':')[1];

            var gamingPeer = CreateGamingPeer(); 

            var endpoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            if (protocol == NetworkProtocolType.Udp)
            {
                result = gamingPeer.ConnectToServerUdp(endpoint, application, null, 2, null);
            }
            else if (protocol == NetworkProtocolType.Tcp)
            {
                result = gamingPeer.ConnectTcp(endpoint, application, null);
            }
            else
            {
                throw new NotSupportedException("Only TCP and UDP S2S connections supported for now");
            }

            return result;
        }

        protected virtual GamingPeer CreateGamingPeer()
        {
            return new GamingPeer(this, this.Application);
        }

        protected const int UpdateIntervalMillis = 5;

        public void EnqueueUpdate()
        {
            this.Fiber.Schedule(this.Update, UpdateIntervalMillis);
        }

        public void OnEncryptionEstablished()
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Encryption established");
            }

            // continue: to "connected"
            this.State.TransitState(this);
        }

        public void OnConnected(GamingPeer peer)
        {
            this.Peer = peer; 

            if (Settings.UseEncryption)
            {
                // wait for encryption callback before join!
                //this.Peer.InitializeEncryption();
                throw new NotImplementedException("Encryption currently not supported");
            }
            else
            {
                // continue: to "connected"
                this.State.TransitState(this);
            }
        }

        /// <summary>
        /// Called by a OnStatusChanged, if the peer was Connected. 
        /// Overridden by subclasses to modify performance counters
        /// </summary>
        public virtual void OnDisconnected()
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Disconnected");
            }

            this.StopTimers();
        }

        ///// <summary>
        /////   The peer service.
        ///// </summary>
        //public void PeerService()
        //{
        //    this.Peer.Service();
        //}

        /// <summary>
        ///   The start.
        /// </summary>
        public virtual void Start()
        {
            this.State = WaitingForMasterConnect.Instance;
            this.State.EnterState(this);
        }

        /// <summary>
        ///   stops the timers and disconnects.
        /// </summary>
        public virtual void Stop()
        {
            // stop position sending
            this.StopTimers();

            if (Settings.ActiveDisconnect && this.Peer != null)
            {
                this.Fiber.Enqueue(this.Peer.Disconnect);
            }
            else
            {
                this.Fiber.Enqueue(() => this.State.StopClient(this));
            }
        }


        /// <summary>
        ///   The update.
        /// </summary>
        public virtual void Update()
        {
            this.State.OnUpdate(this);
        }
        #endregion

        #region Implemented Interfaces

        #region IPhotonPeerListener

        public virtual void OnEvent(IEventData eventData)
        {
            if (Enum.IsDefined(typeof(LoadBalancingEventCode), eventData.Code))
            {
                Counters.ReliableEventsReceived.Increment();
                WindowsCounters.ReliableEventsReceived.Increment();

                if (StayInLobby)
                {
                    if (eventData.Code == (byte)LoadBalancingEventCode.GameList
                        || eventData.Code == (byte)LoadBalancingEventCode.GameListUpdate)
                    {

                        //string printData = null;
                        var gameList = eventData.Parameters[(byte)LoadBalancingParameterCode.GameList];
                        
                        log.InfoFormat(
                            "Event {0} received for Client {1} (Lobby {2})",
                            Enum.GetName(typeof(LoadBalancingEventCode), eventData.Code),
                            this.Number,
                            this.LobbyName);
                    }
                }
            }
            else
            {

                switch (eventData.Code)
                {
                        // unreliable event
                    case 101:
                        {
                            var data = (Hashtable)eventData[LiteOpKey.Data];
                            long now = watch.ElapsedMilliseconds;
                            var sendTime = (long)data[0];
                            long diff = now - sendTime;
                            Counters.UnreliableEventRoundTripTime.IncrementBy(diff);
                            WindowsCounters.UnreliableEventRoundTripTime.IncrementBy(diff);
                            WindowsCounters.UnreliableEventRoundtripTimeBase.Increment();

                            Counters.UnreliableEventsReceived.Increment();
                            WindowsCounters.UnreliableEventsReceived.Increment();
                            break;
                        }

                        // reliable event
                    case 102:
                        {
                            var data = (Hashtable)eventData[LiteOpKey.Data];
                            long now = watch.ElapsedMilliseconds;
                            var sendTime = (long)data[0];
                            long diff = now - sendTime;

                            Counters.ReliableEventRoundTripTime.IncrementBy(diff);
                            WindowsCounters.ReliableEventRoundTripTime.IncrementBy(diff);
                            WindowsCounters.ReliableEventRoundtripTimeBase.Increment();

                            Counters.ReliableEventsReceived.Increment();
                            WindowsCounters.ReliableEventsReceived.Increment();
                            break;
                        }

                    case 103:
                        {
                            var data = (Hashtable)eventData[LiteOpKey.Data];
                            long now = watch.ElapsedMilliseconds;
                            var sendTime = (long)data[0];
                            long diff = now - sendTime;
                            Counters.FlushEventRoundTripTime.IncrementBy(diff);
                            WindowsCounters.FlushEventRoundTripTime.IncrementBy(diff);
                            WindowsCounters.FlusheventRoundtripTimeBase.Increment();
                            Counters.FlushEventsReceived.Increment();
                            WindowsCounters.FlushEventsReceived.Increment();
                            break;
                        }

                    case LiteEventCode.Leave:
                    case LiteEventCode.Join:
                    case LiteEventCode.PropertiesChanged:
                        {
                            Counters.ReliableEventsReceived.Increment();
                            WindowsCounters.ReliableEventsReceived.Increment();
                            break;
                        }

                    default:
                        {
                            log.WarnFormat("OnEventReceive: unexpected event {0}", eventData.Code);
                            break;
                        }
                }
            }
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            // for stardust server
            switch (operationResponse.OperationCode)
            {
                // unreliable operation
                case 101:
                    {
                        var data = (Hashtable)operationResponse.Parameters[LiteOpKey.Data];
                        long now = watch.ElapsedMilliseconds;
                        var sendTime = (long)data[0];
                        long diff = now - sendTime;
                        Counters.UnreliableEventRoundTripTime.IncrementBy(diff);
                        WindowsCounters.UnreliableEventRoundTripTime.IncrementBy(diff);
                        WindowsCounters.UnreliableEventRoundtripTimeBase.Increment();

                        Counters.UnreliableEventsReceived.Increment();
                        WindowsCounters.UnreliableEventsReceived.Increment();
                        break;
                    }

                // reliable operation
                case 102:
                    {
                        var data = (Hashtable)operationResponse.Parameters[LiteOpKey.Data];
                        long now = watch.ElapsedMilliseconds;
                        var sendTime = (long)data[0];
                        long diff = now - sendTime;
                        Counters.ReliableEventRoundTripTime.IncrementBy(diff);
                        WindowsCounters.ReliableEventRoundTripTime.IncrementBy(diff);
                        WindowsCounters.ReliableEventRoundtripTimeBase.Increment();
                        Counters.ReliableEventsReceived.Increment();
                        WindowsCounters.ReliableEventsReceived.Increment();
                        break;
                    }

                default:
                    {
                        this.State.OnOperationReturn(this, operationResponse);
                        break;
                    }
            }
        }

        #endregion

        #endregion

        #region Methods
        /// <summary>
        ///   The op raise event.
        /// </summary>
        /// <param name = "eventCode">
        ///   The event code.
        /// </param>
        /// <param name = "eventData">
        ///   The event data.
        /// </param>
        /// <param name = "sendReliable">
        ///   The send reliable.
        /// </param>
        /// <param name = "channelId">
        ///   The channel id.
        /// </param>
        protected void OpRaiseEvent(byte eventCode, Hashtable eventData, bool sendReliable, byte channelId, bool useEncryption)
        {
            var wrap = new Dictionary<byte, object> { { LiteOpKey.Data, eventData }, { LiteOpKey.Code, eventCode } };
            this.Peer.SendOperationRequest(new OperationRequest(LiteOpCode.RaiseEvent, wrap), new SendParameters() { Unreliable = !sendReliable, Encrypted = useEncryption, ChannelId = channelId }); 
        }
        

        /// <summary>
        ///   Sends and event that has a minimal RTT.
        /// </summary>
        private void SendFlush()
        {
            const byte EventCode = 103;
            var eventData = new Hashtable { { 0, watch.ElapsedMilliseconds } };
            var wrap = new Dictionary<byte, object> { { LiteOpKey.Data, eventData }, { LiteOpKey.Code, EventCode }, { 243, true } };
            this.Peer.SendOperationRequest(new OperationRequest(LiteOpCode.RaiseEvent, wrap), new SendParameters() { Unreliable = !Settings.FlushReliable, ChannelId = Settings.FlushChannel }); 

            Counters.FlushOperationsSent.Increment();
            WindowsCounters.FlushOperationsSent.Increment();
        }

        private void SetPropertiesReliable()
        {
            var properties = new Hashtable();
            properties["UpdateGame"] = watch.ElapsedMilliseconds;
            properties["LobbyProps"] = new byte[Settings.ReliableDataSize];

            var wrap = new Dictionary<byte, object> { { LiteOpKey.Properties, properties } };

            this.Peer.SendOperationRequest(
                new OperationRequest(LiteOpCode.SetProperties, wrap),
                new SendParameters() { Unreliable = false, Encrypted = Settings.UseEncryption, ChannelId = Settings.ReliableDataChannel }); 


            Counters.ReliableOperationsSent.Increment();
            WindowsCounters.ReliableOperationsSent.Increment();
        }


        /// <summary>
        ///   The send reliable event.
        /// </summary>
        private void SendReliableEvent()
        {
            var data = new byte[Settings.ReliableDataSize];
            this.OpRaiseEvent(102, new Hashtable { { 0, watch.ElapsedMilliseconds }, { LiteOpKey.Data, data } }, true, Settings.ReliableDataChannel, Settings.UseEncryption);
            Counters.ReliableOperationsSent.Increment();
            WindowsCounters.ReliableOperationsSent.Increment();
        }

        /// <summary>
        ///   The send unreliable event.
        /// </summary>
        private void SendUnreliableEvent()
        {
            var data = new byte[Settings.UnreliableDataSize];
            this.OpRaiseEvent(101, new Hashtable { { 0, watch.ElapsedMilliseconds }, { LiteOpKey.Data, data } }, false, Settings.UnreliableDataChannel, Settings.UseEncryption);
            Counters.UnreliableOperationsSent.Increment();
            WindowsCounters.UnreliableOperationsSent.Increment();
        }

        /// <summary>
        ///   The start timers.
        /// </summary>
        public void StartTimers()
        {
            if (Settings.SendUnreliableData)
            {
                this.sendUnreliableTimer = this.Fiber.ScheduleOnInterval(
                    this.SendUnreliableEvent, Settings.UnreliableDataSendInterval, Settings.UnreliableDataSendInterval);
            }

            if (Settings.SendReliableData)
            {
                this.sendReliableTimer = this.Fiber.ScheduleOnInterval(
                    this.SendReliableEvent, Settings.ReliableDataSendInterval, Settings.ReliableDataSendInterval);
            }

            // only the first client sends the flush event
            if (this.Number == 0 && Settings.FlushInterval > 0)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(
                        "Scheduled flush with interval {0} sending {1}reliable", Settings.FlushInterval, Settings.FlushReliable ? string.Empty : "un");
                }

                this.sendFlushTimer = this.Fiber.ScheduleOnInterval(this.SendFlush, Settings.FlushInterval, Settings.FlushInterval);
            }

            this.counterTimer = this.Fiber.ScheduleOnInterval(
                () =>
                {
                    Counters.RoundTripTimeVariance.IncrementBy(this.Peer.RoundTripTimeVariance);
                    WindowsCounters.RoundTripTimeVariance.IncrementBy(this.Peer.RoundTripTimeVariance);
                    WindowsCounters.RoundtripTimeVarianceBase.Increment();

                    Counters.RoundTripTime.IncrementBy(this.Peer.RoundTripTime);
                    WindowsCounters.RoundTripTime.IncrementBy(this.Peer.RoundTripTime);
                    WindowsCounters.RoundtripTimeBase.Increment();
                },
                0,
                1000);
        }

        /// <summary>
        ///   The stop timers.
        /// </summary>
        public void StopTimers()
        {
            if (this.sendUnreliableTimer != null)
            {
                this.sendUnreliableTimer.Dispose();
                this.sendUnreliableTimer = null;
            }

            // stop data sending
            if (this.sendReliableTimer != null)
            {
                this.sendReliableTimer.Dispose();
                this.sendReliableTimer = null;
            }

            if (this.sendFlushTimer != null)
            {
                this.sendFlushTimer.Dispose();
                this.sendFlushTimer = null;
            }

            if (this.counterTimer != null)
            {
                this.counterTimer.Dispose();
                this.counterTimer = null;
            }
        }

        #endregion
    }
}