#if ADMIN_PANEL

using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Utils;
using UnityEngine.UI;
using System;

namespace SocialPoint.Network
{
    public sealed class AdminPanelNetwork : IAdminPanelGUI, IAdminPanelConfigurer, INetworkClientDelegate, INetworkServerDelegate
    {
        DependencyContainer _container;
        IUpdateScheduler _updateScheduler;

        INetworkClient _client;
        bool _clientRunning;
        INetworkServer _server;
        bool _serverRunning;

        AdminPanelConsole _console;

        Dropdown _msgOrigin;
        InputField _msgType;
        Toggle _msgReliable;
        InputField _msgBody;

        Text _opServer;
        Text _opClient;

        public AdminPanelNetwork(IUpdateScheduler updateScheduler, DependencyContainer container = null)
        {
            _updateScheduler = updateScheduler;
            _container = container;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Network", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            // Inflate layout
            layout.CreateLabel("Network");
            layout.CreateMargin();

            layout.CreateLabel("Send Message");
            layout.CreateMargin();

            var hlayout = layout.CreateHorizontalLayout();
            hlayout.CreateFormLabel("Origin");
            _msgOrigin = hlayout.CreateDropdown("Server", new string[]{ "Client" });

            hlayout = layout.CreateHorizontalLayout();
            hlayout.CreateFormLabel("Type");
            _msgType = hlayout.CreateTextInput("0");

            hlayout = layout.CreateHorizontalLayout();
            _msgReliable = hlayout.CreateToggleButton("Reliable", true, null);

            _msgBody = layout.CreateTextInput();

            layout.CreateButton("Send", OnSendMessageClicked);
            layout.CreateMargin();

            layout.CreateLabel("Setup");
            layout.CreateMargin();

            layout.CreateOpenPanelButton("Unity Networking", new AdminPanelUnetMultiplayer(this, _updateScheduler, _container));
            layout.CreateMargin();

            var opServer = layout.CreateButton("", OnOpServerClicked);
            _opServer = opServer.GetComponentInChildren<Text>();
            UpdateOpServer();
            var opClient = layout.CreateButton("", OnOpClientClicked);
            _opClient = opClient.GetComponentInChildren<Text>();
            UpdateOpClient();
            layout.CreateMargin();

            layout.CreateButton("Start Local Networking", OnLocalStartClicked);
            layout.CreateMargin();
        }

        void Log(string msg)
        {
            _console.Print(msg);
        }

        public void StartServer(INetworkServer server)
        {
            Log("starting server... " + server.ToString());
            _server = server;
            _server.RemoveDelegate(this);
            _server.AddDelegate(this);
            _server.Start();
        }

        public void StartClient(INetworkClient client)
        {
            Log("starting client... " + client.ToString());
            _client = client;
            _client.RemoveDelegate(this);
            _client.AddDelegate(this);
            _client.Connect();
        }

        void UpdateOpServer()
        {
            if(_opServer == null)
            {
                return;
            }
            if(_serverRunning)
            {
                _opServer.text = "Stop Server";
            }
            else
            {
                _opServer.text = "Start Server";
            }
        }

        void OnOpServerClicked()
        {
            if(_server == null && _container != null)
            {
                var factory = _container.Resolve<INetworkServerFactory>();
                _server = factory.Create();
            }
            if(_server == null)
            {
                Log("no server loaded");
                return;
            }
            if(_serverRunning)
            {
                Log("stopping server");
                _server.Stop();
            }
            else
            {
                StartServer(_server);
            }
        }

        void UpdateOpClient()
        {
            if(_opClient == null)
            {
                return;
            }
            if(_clientRunning)
            {
                _opClient.text = "Disconnect Client";
            }
            else
            {
                _opClient.text = "Connect Client";
            }
        }

        void OnOpClientClicked()
        {
            if(_client == null && _container != null)
            {
                var clientFactory = _container.Resolve<INetworkClientFactory>();
                _client = clientFactory.Create();
            }
            if(_client == null)
            {
                Log("no client loaded");
                return;
            }
            if(_clientRunning)
            {
                Log("stopping client");
                _client.Disconnect();
            }
            else
            {
                StartClient(_client);
            }
        }

        void OnSendMessageClicked()
        {
            byte type;
            if(!byte.TryParse(_msgType.text, out type))
            {
                type = 0;
            }
            bool reliable = _msgReliable.isOn;
            var reliableStr = reliable ? "reliable" : "unreliable";
            INetworkMessage msg = null;
            if(_msgOrigin.value == 0)
            {
                if(_server == null)
                {
                    Log("error: no server found");
                }
                else
                {
                    Log("sending " + reliableStr + " message from server to client of type " + type);
                    msg = _server.CreateMessage(new NetworkMessageData {
                        MessageType = type,
                        Unreliable = !reliable
                    });
                }
            }
            else
            {
                if(_server == null)
                {
                    Log("error: no client found");
                }
                else
                {
                    Log("sending " + reliableStr + " message from client to server of type " + type);
                    msg = _client.CreateMessage(new NetworkMessageData {
                        MessageType = type,
                        Unreliable = !reliable
                    });
                }
            }
            if(msg != null)
            {
                msg.Writer.Write(_msgBody.text);
                msg.Send();
            }
        }

        void OnLocalStartClicked()
        {
            var server = new LocalNetworkServer();
            var client = new LocalNetworkClient(server);
            if(_container != null)
            {
                _container.Rebind<LocalNetworkServer>().ToInstance(server);
                _container.Rebind<LocalNetworkClient>().ToInstance(client);
            }
            StartServer(server);
            StartClient(client);
        }

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            Log("client connected");
            _clientRunning = true;
            UpdateOpClient();
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            Log("client disconnected");
            _clientRunning = false;
            UpdateOpClient();
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
            Log("client received message of type " + data.MessageType);
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            Log("client got error " + err.ToString());
        }

        #endregion

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
            Log("server started");
            _serverRunning = true;
            UpdateOpServer();
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            Log("server stopped");
            _serverRunning = false;
            UpdateOpServer();
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            Log("server connected to client " + clientId);
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            Log("server disconnected from client " + clientId);
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
            Log("server received message from client " + data.ClientIds[0] + " of type " + data.MessageType);
        }

        void INetworkServerDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            Log("server got error " + err.ToString());
        }

        #endregion
    }

    public sealed class AdminPanelUnetMultiplayer : IAdminPanelGUI
    {
        DependencyContainer _container;
        IUpdateScheduler _updateScheduler;

        InputField _serverPort;
        InputField _clientAddress;
        InputField _clientPort;
        AdminPanelNetwork _parent;

        public AdminPanelUnetMultiplayer(AdminPanelNetwork parent, IUpdateScheduler updateScheduler, DependencyContainer container = null)
        {
            _updateScheduler = updateScheduler;
            _parent = parent;
            _container = container;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            // Inflate layout
            layout.CreateLabel("Unity Networking");
            layout.CreateMargin();

            layout.CreateLabel("Server");

            var hlayout = layout.CreateHorizontalLayout();
            hlayout.CreateFormLabel("Port");
            _serverPort = hlayout.CreateTextInput(UnetNetworkServer.DefaultPort.ToString());
            layout.CreateButton("Start", OnUnetServerStartClicked);
            layout.CreateMargin();
            layout.CreateLabel("Client");

            hlayout = layout.CreateHorizontalLayout();
            hlayout.CreateFormLabel("Server Address");
            _clientAddress = hlayout.CreateTextInput(UnetNetworkClient.DefaultServerAddr);

            hlayout = layout.CreateHorizontalLayout();
            hlayout.CreateFormLabel("Server Port");
            _clientPort = hlayout.CreateTextInput(UnetNetworkServer.DefaultPort.ToString());

            layout.CreateButton("Start", OnUnetClientStartClicked);
            layout.CreateMargin();
        }


        void OnUnetServerStartClicked()
        {
            int port;
            if(!int.TryParse(_serverPort.text, out port))
            {
                port = UnetNetworkServer.DefaultPort;
            }
            var server = new UnetNetworkServer(_updateScheduler, port);
            if(_container != null)
            {
                _container.Rebind<UnetNetworkServer>().ToInstance(server);
                _container.Rebind<INetworkServer>().ToLookup<UnetNetworkServer>();
                _container.Bind<IDisposable>().ToLookup<UnetNetworkServer>();
            }
            _parent.StartServer(server);
        }

        void OnUnetClientStartClicked()
        {
            int port;
            if(!int.TryParse(_clientPort.text, out port))
            {
                port = UnetNetworkServer.DefaultPort;
            }
            var client = new UnetNetworkClient(_clientAddress.text, port);
            if(_container != null)
            {
                _container.Rebind<UnetNetworkClient>().ToInstance(client);
                _container.Rebind<INetworkClient>().ToLookup<UnetNetworkClient>();
                _container.Bind<IDisposable>().ToLookup<UnetNetworkClient>();
            }
            _parent.StartClient(client);
        }
    }
}

#endif
