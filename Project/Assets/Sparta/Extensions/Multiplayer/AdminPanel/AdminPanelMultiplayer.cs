
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using UnityEngine.UI;
using System;
using System.Text;

namespace SocialPoint.Multiplayer
{
    public class AdminPanelMultiplayer : IAdminPanelGUI, IAdminPanelConfigurer, INetworkClientDelegate, INetworkServerDelegate
    {
        DependencyContainer _container;

        INetworkClient _client;
        bool _clientRunning;
        INetworkServer _server;
        bool _serverRunning;

        Text _textArea;
        StringBuilder _log = new StringBuilder();

        Dropdown _msgOrigin;
        InputField _msgType;
        InputField _msgChannel;
        InputField _msgBody;

        Text _opServer;
        Text _opClient;

        public AdminPanelMultiplayer(DependencyContainer container=null)
        {
            _container = container;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Multiplayer", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            // Inflate layout
            layout.CreateLabel("Multiplayer");
            layout.CreateMargin();

            _textArea = layout.CreateVerticalScrollLayout().CreateTextArea(_log.ToString());

            layout.CreateLabel("Send Message");
            layout.CreateMargin();

            using(var hlayout = layout.CreateHorizontalLayout())
            {
                hlayout.CreateFormLabel("Origin");
                _msgOrigin = hlayout.CreateDropdown("Server", new string[]{ "Client" });
            }

            using(var hlayout = layout.CreateHorizontalLayout())
            {
                hlayout.CreateFormLabel("Type");
                _msgType = hlayout.CreateTextInput("0");
            }
            using(var hlayout = layout.CreateHorizontalLayout())
            {
                hlayout.CreateFormLabel("Channel");
                _msgChannel = hlayout.CreateTextInput("0");
            }

            _msgBody = layout.CreateTextInput();

            layout.CreateButton("Send", OnSendMessageClicked);
            layout.CreateMargin();

            layout.CreateLabel("Setup");
            layout.CreateMargin();

            layout.CreateOpenPanelButton("Unity Networking", new AdminPanelUnetMultiplayer(this, _container));
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
            _log.AppendLine(msg);
            if(_textArea != null)
            {
                _textArea.text = _log.ToString();
            }
        }

        public void StartServer(INetworkServer server)
        {
            Log("starting server... " + server.ToString());
            _server = server;
            _server.AddDelegate(this);
            _server.Start();
        }

        public void StartClient(INetworkClient client)
        {
            Log("starting client... " + client.ToString());
            _client = client;
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
                _server = _container.Resolve<INetworkServer>();
            }
            if(_server == null)
            {
                Log("no server loaded");
                return;
            }
            if(_serverRunning)
            {
                _server.Stop();
            }
            else
            {
                _server.Start();
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
                _client = _container.Resolve<INetworkClient>();
            }
            if(_client == null)
            {
                Log("no client loaded");
                return;
            }
            if(_clientRunning)
            {
                _client.Connect();
            }
            else
            {
                _client.Connect();
            }
        }

        void OnSendMessageClicked()
        {
            byte type;
            if(!byte.TryParse(_msgType.text, out type))
            {
                type = 0;
            }
            int chan;
            if(!int.TryParse(_msgChannel.text, out chan))
            {
                chan = 0;
            }
            INetworkMessage msg = null;
            if(_msgOrigin.value == 0)
            {
                if(_server == null)
                {
                    Log("error: no server found");
                }
                else
                {
                    Log("sending message from server to client of type " + type + " through channel " + chan);
                    msg = _server.CreateMessage(type, chan);
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
                    Log("sending message from client to server of type " + type + " through channel " + chan);
                    msg = _client.CreateMessage(type, chan);
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

        void INetworkClientDelegate.OnConnected()
        {
            Log("client connected");
            _clientRunning = true;
            UpdateOpClient();
        }

        void INetworkClientDelegate.OnDisconnected()
        {
            Log("client disconnected");
            _clientRunning = false;
            UpdateOpClient();
        }

        void INetworkClientDelegate.OnMessageReceived(ReceivedNetworkMessage msg)
        {
            Log("client received message of type " + msg.MessageType + " through channel " + msg.ChannelId);
        }

        void INetworkClientDelegate.OnError(SocialPoint.Base.Error err)
        {
            Log("client got error " + err.ToString());
        }

        #endregion

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnStarted()
        {
            Log("server started");
            _serverRunning = true;
            UpdateOpServer();
        }

        void INetworkServerDelegate.OnStopped()
        {
            Log("server stopped");
            _serverRunning = false;
            UpdateOpServer();
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            Log("server connected to client "+clientId);
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            Log("server disconnected from client "+clientId);
        }

        void INetworkServerDelegate.OnMessageReceived(byte clientId, ReceivedNetworkMessage msg)
        {
            Log("server received message from client "+clientId+" of type " + msg.MessageType + " through channel " + msg.ChannelId);
        }

        void INetworkServerDelegate.OnError(SocialPoint.Base.Error err)
        {
            Log("server got error " + err.ToString());
        }

        #endregion
    }

    public class AdminPanelUnetMultiplayer : IAdminPanelGUI
    {
        DependencyContainer _container;

        InputField _serverPort;
        InputField _clientAddress;
        InputField _clientPort;
        AdminPanelMultiplayer _parent;

        public AdminPanelUnetMultiplayer(AdminPanelMultiplayer parent, DependencyContainer container=null)
        {
            _parent = parent;
            _container = container;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            // Inflate layout
            layout.CreateLabel("Unity Networking");
            layout.CreateMargin();

            layout.CreateLabel("Server");
            using(var hlayout = layout.CreateHorizontalLayout())
            {
                hlayout.CreateFormLabel("Port");
                _serverPort = hlayout.CreateTextInput(UnetNetworkServer.DefaultPort.ToString());
            }
            layout.CreateButton("Start", OnUnetServerStartClicked);
            layout.CreateMargin();

            layout.CreateLabel("Client");
            using(var hlayout = layout.CreateHorizontalLayout())
            {
                hlayout.CreateFormLabel("Server Address");
                _clientAddress = hlayout.CreateTextInput(UnetNetworkClient.DefaultServerAddr);
            }
            using(var hlayout = layout.CreateHorizontalLayout())
            {
                hlayout.CreateFormLabel("Server Port");
                _clientPort = hlayout.CreateTextInput(UnetNetworkServer.DefaultPort.ToString());
            }
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
            var server = new UnetNetworkServer(port);
            if(_container != null)
            {
                _container.Rebind<UnetNetworkServer>().ToInstance(server);
                _container.Rebind<INetworkServer>().ToLookup<UnetNetworkServer>();
                _container.Bind<IDisposable>().ToInstance(server);
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
                _container.Bind<IDisposable>().ToInstance(client);
            }
            _parent.StartClient(client);
        }
    }
}
