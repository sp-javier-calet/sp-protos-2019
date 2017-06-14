﻿using System;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Photon.Hive.Plugin;
using SocialPoint.IO;
using SocialPoint.Base;
using SocialPoint.Utils;
using log4net;
using SocialPoint.Network.ServerEvents;
using SocialPoint.Attributes;

namespace SocialPoint.Network
{
    /**
     * more info:
     * https://doc.photonengine.com/en/onpremise/current/plugins/manual
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-faq
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-upload-guide
     */
    public abstract class NetworkServerPlugin : PluginBase, INetworkServer
    {
        string _pluginName;
        public override string Name
        {
            get
            {
                return _pluginName;
            }
        }

        bool INetworkServer.Running
        {
            get
            {
                return true;
            }
        }

        string INetworkServer.Id
        {
            get
            {
                return PluginHost.GameId;
            }
        }

        public HttpServerEventTracker PluginEventTracker { get; private set; }

        List<INetworkServerDelegate> _delegates;
        INetworkMessageReceiver _receiver;
        protected UpdateScheduler _updateScheduler;
        protected NetworkStatsServer _statsServer;
        protected bool _statsServerEnabled;
        ILog _log;
        protected IFileManager _fileManager;
        object _timer;

        const byte FailEventCode = 199;
        const byte EventContentParam = 245;
        const byte MaxPlayersKey = 255;
        const byte IsOpenKey = 253;
        const byte MasterClientIdKey = 248;

        const string BackendBaseUrlConfigKey = "BackendBaseUrl";
        const string ClientCanChangeBackendBaseUrlConfigKey = "ClientCanChangeBackendBaseUrl";

        const string BackendEnvKey = "be_env";
        const string ServerIdRoomProperty = "server";

        const string LoggerNameConfig = "LoggerName";
        const string PluginNameConfig = "PluginName";
        const string StatsServerEnabled = "StatsServerEnabled";
        const string AssetsPathConfig = "AssetsPath";

        const string FullErrorMsg = "Game is full.";
        const string ServerPresentErrorMsg = "This room already has a server.";
        const string ExceptionMetricName = "multiplayer.exception_raised";

        public INetworkServer NetworkServer
        {
            get
            {
                if(_statsServerEnabled)
                {
                    return _statsServer;
                }
                return this;
            }
        }

        abstract protected int MaxPlayers { get; }
        abstract protected bool Full { get; }
        abstract protected int UpdateInterval { get; }

        public string BaseBackendUrl { get; private set; }
        bool _clientCanChangeBackendBaseUrl;

        protected NetworkServerPlugin(string pluginName)
        {
            _pluginName = pluginName;
            UseStrictMode = true;
            _delegates = new List<INetworkServerDelegate>();
            var httpServer = new ImmediateWebRequestHttpClient();
            _updateScheduler = new UpdateScheduler();
            PluginEventTracker = new HttpServerEventTracker(_updateScheduler, httpServer);
            PluginEventTracker.Platform = "PhotonPlugin";
            PluginEventTracker.Start();
        }

        /*
         * to change the configuration values in the local build, edit:
         * deploy/LoadBalancing/GameServer/bin/Photon.LoadBalancing.dll.config
         */
        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if(!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }
            string configStr;
            string assetsPath;
            if(config.TryGetValue(PluginNameConfig, out configStr))
            {
                _pluginName = configStr;
            }
            if(config.TryGetValue(LoggerNameConfig, out configStr))
            {
                _log = LogManager.GetLogger(configStr);
            }
            if(config.TryGetValue(StatsServerEnabled, out configStr))
            {
                _statsServerEnabled = configStr.Equals("true") ? true : false;
                if(_statsServerEnabled)
                {
                    _statsServer = new NetworkStatsServer(this, _updateScheduler);
                }
            }
            if(PluginEventTracker != null)
            {
                PluginEventTracker.UpdateCommonTrackData += (dic => dic.SetValue("ver", AppVersion));
            }
            if(config.TryGetValue(AssetsPathConfig, out configStr))
            {
                assetsPath = configStr;
            }
            else
            {
                assetsPath = Path.GetDirectoryName(GetType().Assembly.Location);
            }
            _fileManager = new FileManagerWrapper(new StandaloneFileManager(),
                Path.Combine(assetsPath, "{0}.bytes"), true);

            SetupBackendUrl(config);
            return true;
        }

        protected void LogDebug(params object[] parms)
        {
            if(_log != null)
            {
                _log.Debug(parms);
            }
            else
            {
                PluginHost.LogDebug(parms);
            }
        }

        protected void LogWarn(params object[] parms)
        {
            if(_log != null)
            {
                _log.Warn(parms);
            }
            else
            {
                PluginHost.LogWarning(parms);
            }
        }

        protected void LogError(params object[] parms)
        {
            if(_log != null)
            {
                _log.Error(parms);
            }
            else
            {
                PluginHost.LogError(parms);
            }
        }

        bool CheckServer(ICallInfo info)
        {
            if(Full)
            {
                info.Fail(FullErrorMsg);
                LogWarn(FullErrorMsg);
            }
            else
            {
                info.Continue();
                return true;
            }
            return false;
        }

        byte GetClientId(string userId)
        {
            var actors = PluginHost.GameActors;
            for(var i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                if(actor.UserId == userId)
                {
                    return GetClientId(actor.ActorNr);
                }
            }
            return 0;
        }

        byte GetClientId(int actorId)
        {
            return (byte)actorId;
        }

        public override void OnCloseGame(ICloseGameCallInfo info)
        {
            if(_timer != null)
            {
                PluginHost.StopTimer(_timer);
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            info.Continue();
        }

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            if(!CheckServer(info))
            {
                return;
            }

            CheckAndOverwriteBackendEnv(info);

            PluginHost.SetProperties(0, new Hashtable {
                { (int)MaxPlayersKey,MaxPlayers },
                { (int)MasterClientIdKey, 0 },
                { ServerIdRoomProperty, 0 },
            }, null, false);

            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }

            if(_statsServer != null)
            {
                _statsServer.Start();
            }

            var u = UpdateInterval;
            if(u > 0)
            {
                _timer = PluginHost.CreateTimer(TryUpdate, 0, u);
            }
            var clientId = GetClientId(info.UserId);
            OnClientConnected(clientId);
        }

        protected virtual void OnClientConnected(byte clientId)
        {
            try
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientConnected(clientId);
                }
                OnClientChanged();
            }
            catch(Exception e)
            {
                HandleException(e);
                LogError("OnClientConnected", e);
            }
        }

        public override void BeforeJoin(IBeforeJoinGameCallInfo info)
        {
            CheckServer(info);
        }

        public override void OnJoin(IJoinGameCallInfo info)
        {
            info.Continue();
            OnClientConnected(GetClientId(info.ActorNr));
        }

        public override void OnLeave(ILeaveGameCallInfo info)
        {
            OnClientDisconnected(GetClientId(info.ActorNr));
            info.Continue();
        }

        protected virtual void OnClientDisconnected(byte clientId)
        {
            try
            {
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnClientDisconnected(clientId);
                }
                OnClientChanged();
            }
            catch(Exception e)
            {
                HandleException(e);
                LogError("OnClientDisconnected", e);
            }
        }

        protected virtual void OnClientChanged()
        {
            PluginHost.SetProperties(0,
                new Hashtable { { (int)IsOpenKey, !Full } }, null, false);
        }

        public override void OnSetProperties(ISetPropertiesCallInfo info)
        {
            if(info.Request.Properties.ContainsKey(ServerIdRoomProperty))
            {
                info.Fail(ServerPresentErrorMsg);
                LogWarn(ServerPresentErrorMsg);
            }
            else
            {
                info.Continue();
            }
        }

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            info.Continue();

            if(_receiver == null)
            {
                return;
            }
            try
            {   
                var data = info.Request.Data as byte[];
                if(data != null)
                {
                    data = HttpEncoding.Decode(data, HttpEncoding.DefaultBodyCompression);

                    var stream = new MemoryStream(data);
                    var reader = new SystemBinaryReader(stream);
                    var netData = new NetworkMessageData
                    {
                        ClientId = GetClientId(info.ActorNr),
                        MessageType = info.Request.EvCode
                    };
                    _receiver.OnMessageReceived(netData, reader);
                }
            }
            catch(Exception e)
            {
                HandleException(e);
                LogError("OnRaiseEvent", e);
            }
        }

        protected static int GetConfigOption(Dictionary<string, string> config, string key, int def)
        {
            string sval;
            if(config.TryGetValue(key, out sval))
            {
                int val;
                if(int.TryParse(sval, out val))
                {
                    return val;
                }
            }
            return def;
        }

        protected static bool GetConfigOption(Dictionary<string, string> config, string key, bool def)
        {
            string sval;
            if(config.TryGetValue(key, out sval))
            {
                bool val;
                if(bool.TryParse(sval, out val))
                {
                    return val;
                }
            }
            return def;
        }

        void TryUpdate()
        {
            try
            {
                Update();
            }
            catch(Exception e)
            {
                LogError("Update", e);
                HandleException(e);
            }
        }

        protected virtual void Update()
        {
            _updateScheduler.Update((float)UpdateInterval/1000.0f);
        }

        void BroadcastError(Error err)
        {
            PluginEventTracker.SendMetric(new Metric(MetricType.Counter, ExceptionMetricName, 1));
            var context = new AttrDic();
            if(err.Detail != string.Empty)
            {
                context.SetValue("detail", err.Detail);
                context.SetValue("match_id", PluginHost.GameId);
            }
            PluginEventTracker.SendLog(new Network.ServerEvents.Log(LogLevel.Error, err.Msg, context), true);
            var dic = new Dictionary<byte, object>();
            dic.Add(EventContentParam, err.ToString());
            BroadcastEvent(FailEventCode, dic);
            PluginHost.LogError(err.ToString());
        }

        protected void HandleException(Exception e)
        {
            BroadcastError(new Error(e.Message, e.StackTrace));
        }

        void INetworkServer.Start()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }
        }

        void INetworkServer.Stop()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }
            LogDebug("Stop");
            BroadcastError(new Error("server stopped"));
        }

        public void Fail(Error err)
        {
            BroadcastError(err);
        }

        INetworkMessage INetworkMessageSender.CreateMessage(NetworkMessageData info)
        {
            List<int> actors = null;
            if(info.ClientId != 0)
            {
                actors = new List<int>();
                actors.Add(info.ClientId);
            }
            LogDebug("CreateMessage", info.MessageType, info.ClientId, info.Unreliable);
            return new PluginNetworkMessage(PluginHost, info.MessageType, info.Unreliable, actors);
        }

        void INetworkServer.AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        void INetworkServer.RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        void INetworkServer.RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        int INetworkServer.GetTimestamp()
        {
            return System.Environment.TickCount;
        }

        bool INetworkServer.LatencySupported
        {
            get
            {
                return true;
            }
        }

        protected object CreateInstanceFromAssembly(string assemblyName, string typeName)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, assemblyName);
            var gameType = Assembly.LoadFile(path).GetType(typeName);
            return Activator.CreateInstance(gameType);
        }

        void SetupBackendUrl(Dictionary<string, string> config)
        {
            _clientCanChangeBackendBaseUrl = GetConfigOption(config, ClientCanChangeBackendBaseUrlConfigKey, 0) != 0;

            string baseUrl;
            if(config.TryGetValue(BackendBaseUrlConfigKey, out baseUrl))
            {
                BaseBackendUrl = baseUrl;
            }
        }

        void CheckAndOverwriteBackendEnv(ICreateGameCallInfo info)
        {
            if(_clientCanChangeBackendBaseUrl && info.Request.GameProperties.ContainsKey(BackendEnvKey))
            {
                object url = info.Request.GameProperties[BackendEnvKey];
                if(url is string)
                {
                    BaseBackendUrl = url as string;
                }
            }
        }
    }
}
