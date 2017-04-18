#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using System;
using System.Text;

namespace SocialPoint.ServerSync
{
    public sealed class AdminPanelCommandQueue : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly ICommandQueue _commandQueue;

        public AdminPanelCommandQueue(ICommandQueue commandQueue)
        {
            _commandQueue = commandQueue;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Command Queue", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Command Queue");

            var cq = _commandQueue as CommandQueue;
            if(cq != null)
            {
                layout.CreateLabel("Status");

                var running = Reflection.GetPrivateField<CommandQueue, bool>(_commandQueue, "_running");
                var content = new StringBuilder();
                content.AppendLine("Configuration:");
                content.Append("  Send Interval: ").AppendLine(cq.SendInterval.ToString());
                content.Append("  Max OutOfSync Interval: ").AppendLine(cq.MaxOutOfSyncInterval.ToString());
                content.Append("  Send Timeout: ").AppendLine(cq.Timeout.ToString());
                content.Append("  Backoff Multiplier: ").AppendLine(cq.BackoffMultiplier.ToString());
                content.Append("  Has Command Receiver: ").AppendLine((cq.CommandReceiver != null).ToString());

                content.AppendLine().AppendLine("Internals:");
                content.Append("  Pending Send: ").AppendLine(Reflection.GetPrivateField<CommandQueue, bool>(_commandQueue, "_pendingSend").ToString());
                content.Append("  Sending: ").AppendLine(Reflection.GetPrivateField<CommandQueue, bool>(_commandQueue, "_sending").ToString());
                content.Append("  Current Packet flushed: ").AppendLine(Reflection.GetPrivateField<CommandQueue, bool>(_commandQueue, "_currentPacketFlushed").ToString());
                content.Append("  Last Packet Id: ").AppendLine(Reflection.GetPrivateField<CommandQueue, int>(_commandQueue, "_lastPacketId").ToString());
                content.Append("  Running: ").AppendLine(running.ToString());
            
                content.AppendLine().AppendLine("Status");
                content.Append("  Server DateTime: ").AppendLine(cq.CurrentTime.ToString());
                content.Append("  Server Timestamp: ").AppendLine(cq.CurrentTimestamp.ToString());
                content.Append("  Synced: ").AppendLine(cq.Synced.ToString());
                content.Append("  Last Sync: ").AppendLine(cq.SyncTimestamp.ToString());

                layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());

                layout.CreateMargin();

                layout.CreateButton("Refresh", layout.Refresh);

                layout.CreateMargin();

                layout.CreateToggleButton("AutoSync", cq.AutoSyncEnabled, (value) => {
                    cq.AutoSyncEnabled = value;
                });

                layout.CreateToggleButton("Ping Enabled", cq.PingEnabled, (value) => {
                    cq.PingEnabled = value;
                });

                layout.CreateToggleButton("Ignore Responses", cq.IgnoreResponses, (value) => {
                    cq.IgnoreResponses = value;
                });

                layout.CreateMargin();

                layout.CreateToggleButton("Running", running, (value) => {
                    if(value)
                    {
                        cq.Start();
                    }
                    else
                    {
                        cq.Stop();
                    }
                });

                layout.CreateConfirmButton("Reset", cq.Reset);
            }
            else
            {
                layout.CreateLabel("Unknown Command Queue Implementation");
            }
        }
    }
}

#endif
