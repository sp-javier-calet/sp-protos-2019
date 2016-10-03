using UnityEngine;
using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using SocialPoint.Utils;
using System.Text;

namespace SocialPoint.Lockstep
{
    public class AdminPanelLockstepClientGUI : IAdminPanelGUI, IUpdateable
    {
        ClientLockstepController _client;
        AdminPanel.AdminPanel _adminPanel;
        Text _text;

        public const string Title = "Lockstep";

        public AdminPanelLockstepClientGUI(ClientLockstepController client)
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
            builder.AppendLine("Running: " + _client.Running);
            builder.AppendLine("Connected: " + _client.Connected);
            builder.AppendLine("TurnBuffer: " + _client.TurnBuffer);
            return builder.ToString();
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
                layout.CreateLabel("Config");
                layout.CreateMargin();

                var builder = new StringBuilder();
                builder.AppendLine("Shared");
                builder.AppendLine("==========");
                builder.AppendLine("CommandStepDuration: " + _client.Config.CommandStepDuration);
                builder.AppendLine("SimulationStepDuration: " + _client.Config.SimulationStepDuration);
                builder.AppendLine("");
                builder.AppendLine("Client");
                builder.AppendLine("==========");
                builder.AppendLine("LocalSimulationDelay: " + _client.ClientConfig.LocalSimulationDelay);
                builder.AppendLine("MaxSimulationStepsPerFrame: " + _client.ClientConfig.MaxSimulationStepsPerFrame);
                builder.AppendLine("SpeedFactor: " + _client.ClientConfig.SpeedFactor);

                layout.CreateTextArea(builder.ToString());

                layout.CreateButton("Show State", OnShowFloatingClient);
            }
        }

        void OnShowFloatingClient()
        {
            _layout.OpenFloatingPanel(_clientGui, new FloatingPanelOptions{
                Size = new Vector2(200, 50),
                Title = AdminPanelLockstepClientGUI.Title
            });
        }

    }
}