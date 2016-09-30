using UnityEngine;
using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using System.Text;

namespace SocialPoint.Lockstep
{
    public class AdminPanelLockstepClientGUI : IAdminPanelGUI
    {
        ClientLockstepController _client;
        AdminPanel.AdminPanel _adminPanel;

        public const string Title = "ClientLockstepController";

        public AdminPanelLockstepClientGUI(ClientLockstepController client)
        {
            _client = client;
        }

        string GenerateClientInfo()
        {
            if(_client == null)
            {
                return null;
            }
            var builder = new StringBuilder();
            builder.AppendLine("State");
            builder.AppendLine("==========");
            builder.AppendLine("Running: " + _client.Running);
            builder.AppendLine("Connected: " + _client.Connected);
            builder.AppendLine("TurnBuffer: " + _client.TurnBuffer);
            builder.AppendLine("");
            builder.AppendLine("Config");
            builder.AppendLine("==========");
            builder.AppendLine("CommandStepDuration: " + _client.Config.CommandStepDuration);
            builder.AppendLine("SimulationStepDuration: " + _client.Config.SimulationStepDuration);
            builder.AppendLine("ClientConfig");
            builder.AppendLine("==========");
            builder.AppendLine("LocalSimulationDelay: " + _client.ClientConfig.LocalSimulationDelay);
            builder.AppendLine("MaxSimulationStepsPerFrame: " + _client.ClientConfig.MaxSimulationStepsPerFrame);
            builder.AppendLine("SpeedFactor: " + _client.ClientConfig.SpeedFactor);
            return builder.ToString();
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(_client == null)
            {
                return;
            }

            layout.CreateTextArea(GenerateClientInfo());
        }
    }

    public sealed class AdminPanelLockstep : IAdminPanelConfigurer, IAdminPanelGUI
    {
        ClientLockstepController _client;
        AdminPanelLayout _layout;
        AdminPanelLockstepClientGUI _clientGui;

        public AdminPanelLockstep(ClientLockstepController client)
        {
            _client = client;
            _clientGui = new AdminPanelLockstepClientGUI(_client);
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Lockstep", this));
        }


        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;
            layout.CreateLabel("Lockstep");
            layout.CreateMargin();

            if(_client != null)
            {
                layout.CreateLabel(AdminPanelLockstepClientGUI.Title);
                layout.CreateMargin();
                _clientGui.OnCreateGUI(layout);
                layout.CreateButton("Show", OnShowFloatingClient);
            }
        }

        void OnShowFloatingClient()
        {
            _layout.OpenFloatingPanel(_clientGui, new FloatingPanelOptions{
                Size = new Vector2(0.3f, 0.5f),
                Title = AdminPanelLockstepClientGUI.Title
            });
        }

    }
}