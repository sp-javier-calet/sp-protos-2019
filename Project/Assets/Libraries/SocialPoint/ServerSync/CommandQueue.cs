
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.ServerSync
{
    public enum CommandQueueErrorType
    {
        HttpResponse,
        InvalidJson,
        ResponseJson,
        SessionLost,
        OutOfSync
    }
    ;

    public class CommandQueue : ICommandQueue
    {
        public delegate string StringDelegate();

        public delegate void RequestSetupDelegate(HttpRequest req,string Uri);

        public delegate void GeneralErrorDelegate(CommandQueueErrorType type,Error err);

        public delegate void CommandErrorDelegate(Command cmd,Error err,Attr resp);

        public delegate void ResponseDelegate(HttpResponse resp);

        public delegate void TrackEventDelegate(string eventName,AttrDic data = null,ErrorDelegate del = null);

        private const string Uri = "packet";
        private const string AttrKeyPackets = "packets";
        private const string AttrKeyCommands = "commands";
        private const string AttrKeyAcks = "acks";
        private const string ErrorEventName = "errors.sync";
        private const string AttrKeyEventError = "error";
        private const string AttrKeyEventSync = "sync";
        private const string AttrKeyEventErrorType = "error_type";
        private const string AttrKeyEventErrorMessage = "error_desc";
        private const string AttrKeyEventErrorHttpCode = "error_code";
        private const string HttpParamSessionId = "session_id";

        private const int MinServerErrorStatusCode = 500;
        private const int SessionLostErrorStatusCode = 482;
        private const int StartPacketId = 1;

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

        private IAppEvents _appEvents;

        public IAppEvents AppEvents
        {
            get
            {
                return _appEvents;
            }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("_appEvents", "_appEvents cannot be null or empty!");
                }
                if(_appEvents != null)
                {
                    DisconnectAppEvents(_appEvents);
                }
                _appEvents = value;
                ConnectAppEvents(_appEvents);
            }
        }

        #region App Events

        private void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground += OnAppWillGoBackground;
        }

        private void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.WillGoBackground -= OnAppWillGoBackground;
        }

        void OnAppWillGoBackground()
        {
            SendUpdate();
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
        public event GeneralErrorDelegate ErrorEvent
        {
            add { GeneralError += value; }
            remove { GeneralError -= value; }
        }

        public event Action SyncChange = delegate {};
        public event GeneralErrorDelegate GeneralError = delegate {};
        public event CommandErrorDelegate CommandError = delegate {};
        public event ResponseDelegate ResponseReceive = delegate {};

        private SyncDelegate _autoSync;
        public SyncDelegate AutoSync
        {
            set
            {
                _autoSync = value;
            }
        }

        private bool _autoSyncEnabled = true;
        public bool AutoSyncEnabled
        {
            set
            {
                _autoSyncEnabled = value;
            }
        }

        public TrackEventDelegate TrackEvent;

        public const bool DefaultIgnoreResponses = false;
        public const int DefaultSendInterval = 20;
        public const int DefaultMaxOutOfSyncInterval = 0;
        public const float DefaultTimeout = 60.0f;
        public const float DefaultBackoffMultiplier = 1.1f;
        public const bool DefaultPingEnabled = true;

        public RequestSetupDelegate RequestSetup;
        public bool IgnoreResponses = DefaultIgnoreResponses;
        public int SendInterval = DefaultSendInterval;
        public int MaxOutOfSyncInterval = DefaultMaxOutOfSyncInterval;
        public float Timeout = DefaultTimeout;
        public float BackoffMultiplier = DefaultBackoffMultiplier;
        public bool PingEnabled = DefaultPingEnabled;


        IHttpClient _httpClient;
        MonoBehaviour _behaviour;
        Packet _sendingPacket;
        Packet _currentPacket;
        List<Packet> _sentPackets;
        List<string> _pendingAcks;
        List<string> _sendingAcks;
        bool _sending;
        bool _synced;
        bool _currentPacketFlushed;
        long _syncTimestamp;
        Coroutine _updateCoroutine;
        int _lastPacketId;
        long _lastSendTimestamp;
        float _currentTimeout;
        float _currentSendInterval;
        IHttpConnection _httpConn;
        Action _sendFinish;

        public CommandQueue(MonoBehaviour behaviour, IHttpClient client)
        {
            DebugUtils.Assert(behaviour != null);
            DebugUtils.Assert(client != null);
            TimeUtils.OffsetChanged += OnTimeOffsetChanged;
            _behaviour = behaviour;
            _httpClient = client;
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
                _httpConn.Cancel();
            }
        }

        void SetStartValues()
        {
            _lastSendTimestamp = CurrentTimestamp;
            _currentTimeout = Timeout;
            _currentSendInterval = SendInterval;
            _syncTimestamp = CurrentTimestamp;
            _synced = true;
        }

        void OnTimeOffsetChanged(TimeSpan diff)
        {
            var dt = (long)diff.TotalSeconds;
            _lastSendTimestamp += dt;
            _syncTimestamp += dt;
        }

        public void Add(Command cmd, Action callback)
        {
            Add(cmd, (Error err) => {
                if(callback != null)
                {
                    callback();
                }
            });
        }

        public void Add(Command cmd, ErrorDelegate callback = null)
        {
            if(_currentPacket == null)
            {
                _currentPacket = new Packet();
            }
            _currentPacket.Add(cmd, callback);
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
            Flush((Error err) => {
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
            SetStartValues();

            if(RequestSetup == null)
            {
                throw new MissingComponentException("Request setup callback not assigned.");
            }
            if(_updateCoroutine == null)
            {
                _updateCoroutine = _behaviour.StartCoroutine(UpdateCoroutine());
            }
        }

        public void Stop()
        {
            if(_updateCoroutine != null)
            {
                _behaviour.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        public void Dispose()
        {
            Stop();
            Reset();
            _autoSync = null;
            TrackEvent = null;
        } 

        IEnumerator UpdateCoroutine()
        {
            while(true)
            {
                Update();
                yield return true;
            }
        }

        void Update()
        {
            if(_lastSendTimestamp + (long)_currentSendInterval < CurrentTimestamp)
            {
                SendUpdate();
            }
        }

        public void Send(Action finish = null)
        {
            if(_sending)
            {
                _sendFinish += finish;
            }
            else
            {
                _sending = true;
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

        void SendCurrent(Action finish=null)
        {
            var packet = PrepareNextPacket();
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
            if(_sendFinish != null)
            {
                var finish = _sendFinish;
                _sendFinish = null;
                Send(finish);
            }
        }

        Packet PrepareNextPacket()
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
            return null;
        }

        void SendUpdate()
        {
            if(_autoSyncEnabled && _autoSync != null)
            {
                Add(new SyncCommand(_autoSync()));
                Flush();
            }
            if(!_sending)
            {
                _sending = true;
                var packet = PrepareNextPacket();
                if(packet == null && PingEnabled)
                {
                    packet = new Packet();
                }
                DoSend(packet, AfterSend);
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

        void DoSend(Packet packet, Action finish)
        {
            if(packet == null)
            {
                if(finish != null)
                {
                    finish();
                }
                return;
            }

            BeforePacketSent(packet);
            var attr = packet.ToRequestAttr();

            if(_pendingAcks.Count > 0)
            {
                var attrdic = attr.AsDic;
                if(attrdic != null)
                {
                    var attracks = new AttrList();
                    _sendingAcks = new List<string>(_pendingAcks);
                    foreach(var ack in _sendingAcks)
                    {
                        attracks.AddValue(ack);
                    }
                    attrdic.Set(AttrKeyAcks, attracks);
                }
            }

            var req = new HttpRequest();
            req.Body = new JsonAttrSerializer().Serialize(attr);
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.AddHeader(HttpRequest.AcceptHeader, HttpRequest.ContentTypeJson);
            req.AcceptCompressed = true;
            req.CompressBody = true;
            req.Priority = HttpRequestPriority.High;
            if(req.Timeout == 0.0f)
            {
                req.Timeout = _currentTimeout;
            }

            if(RequestSetup != null)
            {
                RequestSetup(req, Uri);
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
            _httpConn = _httpClient.Send(req, (HttpResponse resp) => {
                ProcessResponse(resp, packet);
                if(finish != null)
                {
                    finish();
                }
            });
        }
        
        void ProcessResponse(HttpResponse resp, Packet packet)
        {
            ResponseReceive(resp);

            if(IgnoreResponses)
            {
                AfterPacketSent(packet, true);
                foreach(var pcmd in packet)
                {
                    if(pcmd.Finished != null)
                    {
                        pcmd.Finished(null);
                    }
                }
                if(packet.Finished != null)
                {
                    packet.Finished(null);
                }
                foreach(var ack in _sendingAcks)
                {
                    _pendingAcks.Remove(ack);
                }
                _sendingAcks.Clear();
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
                _currentTimeout = Mathf.Max(transTime, Timeout);
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
            if(TrackEvent != null)
            {
                var data = new AttrDic();
                var errData = new AttrDic();
                data.Set(AttrKeyEventError, errData);
                var syncData = new AttrDic();
                errData.Set(AttrKeyEventSync, syncData);
                syncData.SetValue(AttrKeyEventErrorType, (int)type);
                syncData.SetValue(AttrKeyEventErrorMessage, err.Msg);
                syncData.SetValue(AttrKeyEventErrorHttpCode, httpCode);
                TrackEvent(ErrorEventName, data);
            }
            if(GeneralError != null)
            {
                GeneralError(type, err);
            }
        }

        bool CheckSync(HttpResponse resp)
        {
            bool oldconn = _synced;
            _synced = !resp.HasConnectionError && resp.StatusCode < MinServerErrorStatusCode;
            if(oldconn != _synced)
            {
                _syncTimestamp = CurrentTimestamp;
                SyncChange();
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
                if(resp.StatusCode == SessionLostErrorStatusCode)
                {
                    NotifyError(CommandQueueErrorType.SessionLost, resp.Error, resp.StatusCode);
                }
                else
                {
                    NotifyError(CommandQueueErrorType.HttpResponse, resp.Error, resp.StatusCode);
                }
                return null;
            }
            else
            if(data == null)
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
                foreach(var p in _sentPackets)
                {
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
            if(pcmd == null)
            {
                return;
            }
            Error err = null;
            if(err == null)
            {
                err = AttrUtils.GetError(data);
            }
            if(err == null && pcmd.Command != null)
            {
                err = pcmd.Command.Validate(data);
            }
            if(pcmd.Finished != null)
            {
                pcmd.Finished(err);
            }
            if(err != null)
            {
                CommandError(pcmd.Command, err, data);
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
                foreach(var pcmd in packet)
                {
                    if(pcmd.Finished != null)
                    {
                        pcmd.Finished(err);
                    }
                }
                _sentPackets.Remove(packet);
                return;
            }
            var datadic = data.AsDic;
            if(datadic.ContainsKey(AttrKeyCommands))
            {
                var cmdsAttr = datadic.Get(AttrKeyCommands).AsDic;
                foreach(var cmdAttrPair in cmdsAttr)
                {
                    var pcmd = packet.GetCommand(cmdAttrPair.Key);
                    ValidateResponse(cmdAttrPair.Value, pcmd);
                    packet.Remove(pcmd);
                    if(pcmd.Command != null)
                    {
                        _pendingAcks.Add(pcmd.Command.Id);
                    }
                }
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

            foreach(var packAttrPair in packsAttr)
            {
                var packet = GetSentPacket(packAttrPair.Key);
                ValidateResponse(packAttrPair.Value, packet);
            }

            foreach(var ack in _sendingAcks)
            {
                _pendingAcks.Remove(ack);
            }
            _sendingAcks.Clear();
        }
    }
}
