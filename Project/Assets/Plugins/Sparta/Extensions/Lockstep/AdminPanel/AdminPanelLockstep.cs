#if ADMIN_PANEL 

using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Lockstep
{
    public class AdminPanelLockstepClientGUI : IFloatingPanelGUI, IUpdateable
    {
        LockstepClient _client;
        Text _text;

        public const string Title = "LockstepClient";

        public AdminPanelLockstepClientGUI(LockstepClient client)
        {
            _client = client;
        }

        string GenerateInfo()
        {
            if(_client == null)
            {
                return null;
            }
            var builder = new StringBuilder();
            builder.Append(Title + " ");
            if(_client.Running)
            {
                builder.Append("(running) ");
                if(_client.Connected)
                {
                    if(_client.Recovering)
                    {
                        builder.AppendLine("(recovering)");
                    }
                    else
                    {
                        builder.AppendLine("(connected)");
                    }
                }
                else
                {
                    builder.AppendLine("(disconnected)");
                }
            }
            else
            {
                builder.AppendLine("(stopped)");
            }
            builder.AppendLine("TurnBuffer: " + _client.TurnBuffer);
            builder.AppendLine("Time: " + _client.UpdateTime + " sim:" + _client.SimulationDeltaTime + " cmd:" + _client.CommandDeltaTime);
            return builder.ToString();
        }

        public void OnCreateFloatingPanel(FloatingPanelController ctrl)
        {
            ctrl.Size = new Vector2(200, 50);
            ctrl.Title = Title;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(_client == null)
            {
                return;
            }

            _text = layout.CreateTextArea(GenerateInfo());
            layout.RegisterUpdateable(this);
        }

        public void Update()
        {
            if(_text != null)
            {
                _text.text = GenerateInfo();
            }
        }
    }

    public class AdminPanelLockstepServerGUI : IFloatingPanelGUI, IUpdateable
    {
        LockstepNetworkServer _server;
        Text _text;

        public const string Title = "LockstepServer";

        public AdminPanelLockstepServerGUI(LockstepNetworkServer server)
        {
            _server = server;
        }

        string GenerateInfo()
        {
            if(_server == null)
            {
                return null;
            }
            var builder = new StringBuilder();
            if(_server.Running)
            {
                builder.Append("(running) ");
            }
            builder.AppendFormat("players: max={0} ready={1} finished={2}\n", _server.MaxPlayers, _server.ReadyPlayerCount, _server.FinishedPlayerCount);

            builder.AppendFormat("Time: {0} cmd: {1}", _server.UpdateTime, _server.CommandDeltaTime);
            return builder.ToString();
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(_server == null)
            {
                return;
            }

            _text = layout.CreateTextArea(GenerateInfo());
            layout.RegisterUpdateable(this);
        }

        public void Update()
        {
            if(_text != null)
            {
                _text.text = GenerateInfo();
            }
        }

        public void OnCreateFloatingPanel(FloatingPanelController ctrl)
        {
            ctrl.Size = new Vector2(200, 50);
            ctrl.Title = Title;
        }
    }

    public sealed class AdminPanelLockstep : IAdminPanelConfigurer, IAdminPanelGUI
    {
        LockstepClient _client;
        LockstepNetworkServer _server;

        public AdminPanelLockstep(LockstepClient client)
        {
            _client = client;
        }

        public void RegisterServer(LockstepNetworkServer server)
        {
            _server = server;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Lockstep", this));
        }

        static string GetConfigDescription(LockstepConfig config)
        {
            var builder = new StringBuilder();
            builder.AppendLine("CommandStepDuration: " + config.CommandStepDuration);
            builder.AppendLine("SimulationStepDuration: " + config.SimulationStepDuration);
            return builder.ToString();
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Lockstep");
            layout.CreateMargin();

            if(_client != null)
            {
                layout.CreateLabel("Client");
                layout.CreateMargin();

                var builder = new StringBuilder();
                builder.Append(GetConfigDescription(_client.Config));
                builder.AppendLine("LocalSimulationDelay: " + _client.ClientConfig.LocalSimulationDelay);
                builder.AppendLine("MaxSimulationStepsPerFrame: " + _client.ClientConfig.MaxSimulationStepsPerFrame);
                builder.AppendLine("SpeedFactor: " + _client.ClientConfig.SpeedFactor);

                layout.CreateTextArea(builder.ToString());

                layout.CreateButton("Show Client Info", OnShowClientInfoClicked);
            }
            if(_server != null)
            {
                layout.CreateLabel("Server");
                layout.CreateMargin();

                var builder = new StringBuilder();
                builder.Append(GetConfigDescription(_server.Config));
                builder.AppendLine("MaxPlayers: " + _server.ServerConfig.MaxPlayers);
                builder.AppendLine("ClientStartDelay: " + _server.ServerConfig.ClientStartDelay);
                builder.AppendLine("ClientSimulationDelay: " + _server.ServerConfig.ClientSimulationDelay);

                layout.CreateTextArea(builder.ToString());

                layout.CreateButton("Show Server Info", OnShowServerInfoClicked);
            }
        }

        void OnShowClientInfoClicked()
        {
            FloatingPanelController.Create(new AdminPanelLockstepClientGUI(_client)).Show();
        }

        void OnShowServerInfoClicked()
        {
            FloatingPanelController.Create(new AdminPanelLockstepServerGUI(_server)).Show();
        }
    }
}

#endif
