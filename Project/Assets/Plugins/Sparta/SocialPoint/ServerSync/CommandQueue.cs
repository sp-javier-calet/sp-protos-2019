using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.ServerSync
{
    public sealed class CommandQueue : ICommandQueue, IUpdateable
    {
        public int SendInterval { get; set; }

        public bool PingEnabled { get; set; }

        public delegate void ResponseDelegate(HttpResponse resp);

        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

        const string Uri = "packet";
        const string AttrKeyPackets = "packets";
        const string AttrKeyCommands = "commands";
        const string AttrKeyPush = "push";
        const string AttrKeyAcks = "acks";
        const string AttrKeyResponse = "response";
        const string ErrorEventName = "errors.sync";
        const string AttrKeyEventError = "error";
        const string AttrKeyEventSync = "sync";
        const string AttrKeyEventErrorType = "error_type";
        const string AttrKeyEventErrorMessage = "error_desc";
        const string AttrKeyEventErrorHttpCode = "error_code";
        const string HttpParamSessionId = "session_id";
        const string SyncChangeEventName = "sync.change";
        const string AttrKeyGame = "game";
        const string AttrKeySynced = "synced";

        const int SessionLostErrorStatusCode = 482;
        const int FutureTimeStatusCode = 472;
        const int StartPacketId = 1;

        public DateTime CurrentTime
        {
            get
            {
                return TimeUtils.Now;
            }
        }

        public long CurrentTimestamp
        {
            get
            {
                return TimeUtils.Timestamp;
            }
        }

        public bool Synced
        {
            get
            {
                return _synced;
            }
        }

        public DateTime SyncTime
        {
            get
            {
                return TimeUtils.GetTime(SyncTimestamp);
            }
        }

        public long SyncTimestamp
        {
            get
            {
                return _syncTimestamp;
            }
        }

        IAppEvents _appEvents;

        public IAppEvents AppEvents
        {
            get
            {
                return _appEvents;
            }
            set
            {
                if(_appEvents != null)
                {
                    DisconnectAppEvents(_appEvents);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    ConnectAppEvents(_appEvents);
                }
            }
        }

        public ILoginData LoginData;

        #region App Events

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground.Add(-25, OnAppWillGoBackground);
            appEvents.GameWillRestart.Add(-25, OnGameWillRestart);
            appEvents.GameWasLoaded.Add(-1000, OnGameWasLoaded);
            appEvents.WasOnBackground.Add(0, OnWasOnBackground);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground.Remove(OnAppWillGoBackground);
            appEvents.GameWillRestart.Remove(OnGameWillRestart);
            appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            appEvents.WasOnBackground.Remove(OnWasOnBackground);
        }

        void OnGameWasLoaded()
        {
            if(!_running)
            {
                Start();
            }
        }

        void OnGameWillRestart()
        {
            if(_running)
            {
                Stop();
                Send();
            }
            Reset();
        }

        void OnAppWillGoBackground()
        {
            _goToBackgroundTS = TimeUtils.Timestamp;
            if(_running)
            {
                SendUpdate();
            }
        }

        void OnWasOnBackground()
        {
            if(_goToBackgroundTS > TimeUtils.Timestamp)
            {
                RaiseClockChangeError();
            }
        }


        #endregion


        [Obsolete("Use CommandError event instead")]
        public event CommandErrorDelegate CommandErrorEvent
        {
            add { CommandError += value; }
            remove { CommandError -= value; }
        }

        [Obsolete("Use SyncChange event instead")]
        public event Action ConnectionEvent
        {
            add { SyncChange += value; }
            remove { SyncChange -= value; }
        }

        [Obsolete("Use GeneralError event instead")]
        public event CommandQueueErrorDelegate ErrorEvent
        {
            add { GeneralError += value; }
            remove { GeneralError -= value; }
        }

        public event Action SyncChange;
        public event CommandQueueErrorDelegate GeneralError;
        public event CommandErrorDelegate CommandError;
        public event CommandResponseDelegate CommandResponse;
        public event ResponseDelegate ResponseReceive;

        int _lastAutoSyncDataHash;

        public CommandReceiver CommandReceiver { get; set; }

        public SyncDelegate AutoSync{ set; private get; }

        bool _autoSyncEnabled = true;

        public bool AutoSyncEnabled
        {
            get
            {
                return _autoSyncEnabled;
            }
            set
            {
                _autoSyncEnabled = value;
            }
        }

        public TrackEventDelegate TrackSystemEvent;

        public const bool DefaultIgnoreResponses = false;
        public const int DefaultSendInterval = 20;
        public const int DefaultMaxOutOfSyncInterval = 0;
        public const float DefaultTimeout = 60.0f;
        public const float DefaultBackoffMultiplier = 1.1f;
        public const bool DefaultPingEnabled = true;

        public bool IgnoreResponses = DefaultIgnoreResponses;
        public int MaxOutOfSyncInterval = DefaultMaxOutOfSyncInterval;
        public float Timeout = DefaultTimeout;
        public float BackoffMultiplier = DefaultBackoffMultiplier;


        IHttpClient _httpClient;
        IUpdateScheduler _updateScheduler;
        Packet _sendingPacket;
        Packet _currentPacket;
        List<Packet> _sentPackets;
        List<string> _pendingAcks;
        List<string> _sendingAcks;
        bool _pendingSend;
        bool _sending;
        bool _synced;
        bool _currentPacketFlushed;
        long _syncTimestamp;
        int _lastPacketId;
        long _lastSendTimestamp;
        float _currentTimeout;
        IHttpConnection _httpConn;
        Action _sendFinish;
        long _goToBackgroundTS;
        bool _running;

        public CommandQueue(IUpdateScheduler updateScheduler, IHttpClient client)
        {
            SendInterval = DefaultSendInterval;
            PingEnabled = DefaultPingEnabled;
            DebugUtils.Assert(updateScheduler != null);
            DebugUtils.Assert(client != null);
            TimeUtils.OffsetChanged += OnTimeOffsetChanged;
            _updateScheduler = updateScheduler;
            _httpClient = client;
            _synced = true;
            _running = false;
            Reset();
        }

        public void Reset()
        {
            _lastPacketId = StartPacketId;
            _currentPacket = null;
            _sendingPacket = null;
            _currentPacketFlushed = false;
            _sending = false;
            _sentPackets = new List<Packet>();
            _pendingAcks = new List<string>();
            _sendingAcks = new List<string>();
            if(_httpConn != null)
            {
                _httpConn.Release();
            }
        }

        void SetStartValues()
        {
            _lastSendTimestamp = CurrentTimestamp - SendInterval;
            _currentTimeout = Timeout;
            _syncTimestamp = CurrentTimestamp;
            _synced = true;
        }

        void OnTimeOffsetChanged(TimeSpan diff)
        {
            var dt = (long)diff.TotalSeconds;
            _lastSendTimestamp += dt;
            _syncTimestamp += dt;
        }

        public void Add(Command cmd, Action<Attr, Error> callback = null)
        {
            if(_currentPacket == null)
            {
                _currentPacket = new Packet();
            }
            if(!_currentPacket.Add(cmd, callback))
            {
                RaiseClockChangeError();
            }
        }

        public int Remove(Packet.FilterDelegate callback = null)
        {
            int count = 0;
            if(_currentPacket != null)
            {
                count = _currentPacket.Remove(callback);
            }
            return count;
        }

        public void Flush(Action callback)
        {
            Flush(err => {
                if(callback != null)
                {
                    callback();
                }
            });
        }

        public void Flush(Packet.FinishDelegate callback = null)
        {
            if(_currentPacket == null)
            {
                _currentPacket = new Packet();
            }
            _currentPacketFlushed = true;
            if(callback != null)
            {
                _currentPacket.Finished += callback;
            }
        }

        public void Start()
        {
            if(_running)
            {
                return;
            }

            SetStartValues();

            if(LoginData == null)
            {
                throw new InvalidOperationException("LoginData not assigned.");
            }
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, SendInterval);
                _running = true;
            }
        }

        public void Stop()
        {
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
                _running = false;
            }
        }

        public void Dispose()
        {
            Stop();
            Reset();
            AutoSync = null;
            TrackSystemEvent = null;
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
            TimeUtils.OffsetChanged -= OnTimeOffsetChanged;
        }

        #region IUpdateable implementation

        public void Update()
        {
            SendUpdate();
        }

        #endregion

        public void Send(Action finish = null)
        {
            if(_sending)
            {
                _pendingSend = true;
                _sendFinish += finish;
            }
            else
            {
                _sending = true;
                AddSyncCommand();
                if(_sendingPacket != null)
                {
                    DoSend(_sendingPacket, () => {
                        if(_sendingPacket == null)
                        {
                            SendCurrent(finish);
                        }
                        else
                        {
                            if(finish != null)
                            {
                                finish();
                            }
                            AfterSend();
                        }
                    });
                }
                else
                {
                    SendCurrent(finish);
                }
            }
        }

        void SendCurrent(Action finish = null)
        {
            var packet = PrepareNextPacket(false);
            DoSend(packet, () => {
                if(finish != null)
                {
                    finish();
                }
                AfterSend();
            });
        }

        void AfterSend()
        {
            _sending = false;
            if(_pendingSend)
            {
                _pendingSend = false;
                Send(_sendFinish);
            }
        }

        Packet PrepareNextPacket(bool withPing)
        {
            if(_sendingPacket != null)
            {
                return _sendingPacket;
            }
            else if(_currentPacket != null && (_currentPacketFlushed || _currentPacket.Atomic))
            {
                _sendingPacket = _currentPacket;
                _currentPacketFlushed = false;
                _currentPacket = null;
                return _sendingPacket;
            }
            else if(withPing && PingEnabled)
            {
                _sendingPacket = new Packet();
                return _sendingPacket;
            }
            return null;
        }

        void SendUpdate()
        {
            AddSyncCommand();
            if(!_sending)
            {
                _sending = true;
                var packet = PrepareNextPacket(true);
                DoSend(packet, AfterSend);
            }
        }

        void AddSyncCommand()
        {
            if(_autoSyncEnabled && AutoSync != null)
            {
                Attr data = null;
                try
                {
                    data = AutoSync();
                }
                catch(Exception e)
                {
                    CatchException(e);
                }
                if(data != null)
                {
                    var hash = data.GetHashCode();
                    if(hash != _lastAutoSyncDataHash)
                    {
                        _lastAutoSyncDataHash = hash;
                        Add(new SyncCommand(data));
                    }
                    Flush();
                }
            }
        }

        void BeforePacketSent(Packet packet)
        {
            if(!packet.HasId)
            {
                packet.Id = _lastPacketId;
                _lastPacketId++;
            }
        }

        void AfterPacketSent(Packet packet, bool success)
        {
            if(success)
            {
                if(_sendingPacket == packet)
                {
                    _sendingPacket = null;
                }
                _sentPackets.Add(packet);
            }
            ApplyBackoff(success);
        }

        void AddPendingAcksToRequest(Attr attr)
        {
            if(_pendingAcks.Count > 0)
            {
                var attrdic = attr.AsDic;
                if(attrdic != null)
                {
                    var attracks = new AttrList();
                    _sendingAcks = new List<string>(_pendingAcks);
                    for(int i = 0, _sendingAcksCount = _sendingAcks.Count; i < _sendingAcksCount; i++)
                    {
                        var ack = _sendingAcks[i];
                        attracks.AddValue(ack);
                    }
                    attrdic.Set(AttrKeyAcks, attracks);
                }
            }
        }

        void RemoveNotifiedAcks()
        {
            for(int i = 0, _sendingAcksCount = _sendingAcks.Count; i < _sendingAcksCount; i++)
            {
                var ack = _sendingAcks[i];
                _pendingAcks.Remove(ack);
            }
            _sendingAcks.Clear();
        }

        void DoSend(Packet packet, Action finish)
        {
            if(packet == null)
            {
                _lastSendTimestamp = CurrentTimestamp;
                if(finish != null)
                {
                    finish();
                }
                return;
            }

            BeforePacketSent(packet);
            var attr = packet.ToRequestAttr();

            // Add Acks to request
            AddPendingAcksToRequest(attr);

            var req = new HttpRequest();
            req.Body = new JsonAttrSerializer().Serialize(attr);
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.AddHeader(HttpRequest.AcceptHeader, HttpRequest.ContentTypeJson);
            req.AcceptCompressed = true;
            req.CompressBody = true;
            req.Priority = HttpRequestPriority.High;
            if(Math.Abs(req.Timeout) < Single.Epsilon)
            {
                req.Timeout = _currentTimeout;
            }

            if(LoginData != null)
            {
                try
                {
                    LoginData.SetupHttpRequest(req, Uri);
                }
                catch(Exception e)
                {
                    CatchException(e);
                }
            }

            if(!req.HasParam(HttpParamSessionId))
            {
                // no session, we wait
                if(finish != null)
                {
                    finish();
                }
                return;
            }

            _lastSendTimestamp = CurrentTimestamp;
            _httpConn = _httpClient.Send(req, resp => {
                ProcessResponse(resp, packet);
                if(finish != null)
                {
                    finish();
                }
            });
        }

        void ProcessResponse(HttpResponse resp, Packet packet)
        {
            if(ResponseReceive != null)
            {
                ResponseReceive(resp);
            }

            if(IgnoreResponses)
            {
                AfterPacketSent(packet, true);
                var itr = packet.GetEnumerator();
                while(itr.MoveNext())
                {
                    var pcmd = itr.Current;
                    if(pcmd.Finished != null)
                    {
                        pcmd.Finished(null, null);
                    }
                }
                itr.Dispose();
                if(packet.Finished != null)
                {
                    packet.Finished(null);
                }

                RemoveNotifiedAcks();

                return;
            }

            bool success = CheckSync(resp);
            AfterPacketSent(packet, success);
            if(success)
            {
                var data = ParseResponse(resp);
                if(data != null)
                {
                    ValidateResponse(data);
                }
            }
        }

        void ApplyBackoff(bool success)
        {
            if(success)
            {
                var transTime = CurrentTimestamp - _lastSendTimestamp;
                _currentTimeout = Math.Max(transTime, Timeout);
            }
            else
            {
                _currentTimeout *= BackoffMultiplier;
            }
        }

        void NotifyError(CommandQueueErrorType type, Error err, int httpCode = 0)
        {
            if(err.Code == 0)
            {
                err.Code = (int)type;
            }
            if(TrackSystemEvent != null)
            {
                var data = new AttrDic();
                var errData = new AttrDic();
                data.Set(AttrKeyEventError, errData);
                var syncData = new AttrDic();
                errData.Set(AttrKeyEventSync, syncData);
                syncData.SetValue(AttrKeyEventErrorType, (int)type);
                syncData.SetValue(AttrKeyEventErrorMessage, err.Msg);
                syncData.SetValue(AttrKeyEventErrorHttpCode, httpCode);
                TrackSystemEvent(ErrorEventName, data);
            }
            if(GeneralError != null)
            {
                GeneralError(type, err);
            }
        }

        void NotifySyncChange()
        {
            if(TrackSystemEvent != null)
            {
                var data = new AttrDic();
                var gameData = new AttrDic();
                data.Set(AttrKeyGame, gameData);
                gameData.SetValue(AttrKeySynced, _synced);
                TrackSystemEvent(SyncChangeEventName, data);
            }
            if(SyncChange != null)
            {
                SyncChange();
            }
        }

        bool CheckSync(HttpResponse resp)
        {
            bool oldconn = _synced;
            _synced = !resp.HasRecoverableError;
            if(oldconn != _synced)
            {
                _syncTimestamp = CurrentTimestamp;
                NotifySyncChange();
            }

            if(!_synced && MaxOutOfSyncInterval > 0 && _syncTimestamp + MaxOutOfSyncInterval < CurrentTimestamp)
            {
                NotifyError(CommandQueueErrorType.OutOfSync, new Error("Too much time passed without sync."));
            }

            return _synced;
        }

        AttrDic ParseResponse(HttpResponse resp)
        {
            AttrDic data = null;
            string jsonerr = "Could not parse json response.";
            try
            {
                data = new JsonAttrParser().Parse(resp.Body).AsDic;
            }
            catch(Exception e)
            {
                jsonerr = e.ToString();
            }
            if(resp.HasError)
            {
                switch(resp.StatusCode)
                {
                case SessionLostErrorStatusCode:
                    NotifyError(CommandQueueErrorType.SessionLost, resp.Error, resp.StatusCode);
                    break;
                case FutureTimeStatusCode:
                    NotifyError(CommandQueueErrorType.ClockChange, resp.Error, resp.StatusCode);
                    break;
                default:
                    {
                        var commandsSent = new System.Text.StringBuilder();
                        for(int i = 0; i < _sentPackets.Count; ++i)
                        {
                            var currentPacket = _sentPackets[i].GetEnumerator();
                            while(currentPacket.MoveNext())
                            {
                                commandsSent.Append(currentPacket.Current.Command.Name + ", ");
                            }
                        }
                        NotifyError(CommandQueueErrorType.HttpResponse, new Error(commandsSent.ToString()), resp.StatusCode);
                    }
                    break;
                }
                return null;
            }
            else if(data == null)
            {
                NotifyError(CommandQueueErrorType.InvalidJson, new Error(jsonerr));
            }
            return data;
        }

        Packet GetSentPacket(string sid)
        {
            int id;
            if(int.TryParse(sid, out id))
            {
                for(int i = 0, _sentPacketsCount = _sentPackets.Count; i < _sentPacketsCount; i++)
                {
                    var p = _sentPackets[i];
                    if(id == p.Id)
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        void ValidateResponse(Attr data, PackedCommand pcmd)
        {
            var response = data.AsDic.Get(AttrKeyResponse);
            if(pcmd == null)
            {
                return;
            }
            if(pcmd.Command != null && CommandResponse != null)
            {
                CommandResponse(pcmd.Command, response);
            }
            Error err = AttrUtils.GetError(response);
            if(err == null && pcmd.Command != null)
            {
                err = pcmd.Command.Validate(response);
            }
            if(pcmd.Finished != null)
            {
                pcmd.Finished(response, err);
            }
            if(err != null && CommandError != null)
            {
                CommandError(pcmd.Command, err, response);
            }
        }

        void ValidateResponse(Attr data, Packet packet)
        {
            if(packet == null)
            {
                return;
            }
            var err = AttrUtils.GetError(data);
            if(err != null)
            {
                if(packet.Finished != null)
                {
                    packet.Finished(err);
                }
                NotifyError(CommandQueueErrorType.ResponseJson, err);
                var itr = packet.GetEnumerator();
                while(itr.MoveNext())
                {
                    var pcmd = itr.Current;
                    if(pcmd.Finished != null)
                    {
                        pcmd.Finished(data, err);
                    }
                }
                itr.Dispose();
                _sentPackets.Remove(packet);
                return;
            }
            var datadic = data.AsDic;
            if(datadic.ContainsKey(AttrKeyCommands))
            {
                var cmdsAttr = datadic.Get(AttrKeyCommands).AsDic;
                var itr = cmdsAttr.GetEnumerator();
                while(itr.MoveNext())
                {
                    var cmdAttrPair = itr.Current;
                    var pcmd = packet.GetCommand(cmdAttrPair.Key);
                    ValidateResponse(cmdAttrPair.Value, pcmd);
                    packet.Remove(pcmd);
                    if(pcmd.Command != null)
                    {
                        _pendingAcks.Add(pcmd.Command.Id);
                    }
                }
                itr.Dispose();
            }
            if(packet.Count == 0)
            {
                if(packet.Finished != null)
                {
                    packet.Finished(err);
                }
                _sentPackets.Remove(packet);
            }
        }

        void ValidateResponse(AttrDic data)
        {
            var err = AttrUtils.GetError(data);
            if(err != null)
            {
                NotifyError(CommandQueueErrorType.ResponseJson, err);
                return;
            }

            var packsAttr = data.Get(AttrKeyPackets).AsDic;

            var itr = packsAttr.GetEnumerator();
            while(itr.MoveNext())
            {
                var packAttrPair = itr.Current;
                var packet = GetSentPacket(packAttrPair.Key);
                ValidateResponse(packAttrPair.Value, packet);
            }
            itr.Dispose();

            // Handle Server to Client Commands
            var pushAttr = data.Get(AttrKeyPush).AsDic;
            var pushCommands = pushAttr.Get(AttrKeyCommands).AsDic;

            var itr2 = pushCommands.GetEnumerator();
            while(itr2.MoveNext())
            {
                var pushCommand = itr2.Current;
                string commandId = STCCommand.getId(pushCommand.Value.AsDic);
                if(CommandReceiver != null)
                {
                    CommandReceiver.Receive(pushCommand.Value.AsDic, out commandId);
                }
                // Add a pending ack for the command response
                _pendingAcks.Add(commandId);
            }
            itr2.Dispose();

            RemoveNotifiedAcks();
        }

        void CatchException(Exception e)
        {
            Log.x(e);

            #if UNITY_EDITOR
            DebugUtils.Stop();
            #else
            NotifyError(CommandQueueErrorType.Exception, new Error(e.ToString()));
            #endif
        }

        void RaiseClockChangeError()
        {
            if(GeneralError != null)
            {
                GeneralError(CommandQueueErrorType.ClockChange, new Error("Clock changed to the past"));
            }
        }
    }
}
