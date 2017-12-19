#if ADMIN_PANEL 

using System;
using SocialPoint.AdminPanel;

namespace SocialPoint.Hardware
{
    public sealed class AdminPanelHardware : IAdminPanelConfigurer, IAdminPanelGUI
    {
        AdminPanelConsole _console;
        AdminPanelLayout _layout;
        IDeviceInfo _deviceInfo;
        IStorageAnalyzer _storageAnalyzer;
        StorageUnit _storageAnalyzerUnit;
        string[] _storageUnitNames;
        bool _listeningStorageWarning;

        public AdminPanelHardware(IDeviceInfo deviceInfo, IStorageAnalyzer storageAnalyzer)
        {
            _deviceInfo = deviceInfo;
            _storageAnalyzer = storageAnalyzer;
            _storageAnalyzerUnit = StorageUnit.MegaBytes;
            _storageUnitNames = Enum.GetNames(typeof(StorageUnit));
            _listeningStorageWarning = false;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_deviceInfo != null)
            {
                adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Hardware", this));
            }
            _console = adminPanel.Console;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;
            layout.CreateLabel("Device Info");
            layout.CreateTextArea(_deviceInfo.ToString());

            layout.CreateLabel("Storage Analyzer");
            //Analyze
            layout.CreateButton("Analyze Now", () => {
                _storageAnalyzer.AnalyzeNow();
            });
            //Start/Stop
            layout.CreateToggleButton(
                _storageAnalyzer.Running ? "Stop" : "Start",
                _storageAnalyzer.Running,
                value => {
                    if(_storageAnalyzer.Running)
                    {
                        _storageAnalyzer.Stop();
                    }
                    else
                    {
                        _storageAnalyzer.Start();
                    }
                    layout.Refresh();
                });
            //Listeners
            layout.CreateToggleButton(
                _listeningStorageWarning ? "Unregister Listener" : "Register Listener",
                _listeningStorageWarning,
                value => {
                    if(_listeningStorageWarning)
                    {
                        _storageAnalyzer.LowStorage -= OnLowStorageSpace;
                    }
                    else
                    {
                        _storageAnalyzer.LowStorage += OnLowStorageSpace;
                    }
                    _listeningStorageWarning = !_listeningStorageWarning;
                    layout.Refresh();
                });

            //Storage size warning
            var storageLayout = layout.CreateHorizontalLayout();
            storageLayout.CreateLabel("Storage Warning At (" + _storageAnalyzerUnit.ToString() + ")");
            var val = _storageAnalyzer.Config.FreeStorageWarning.ToAmount(_storageAnalyzerUnit);
            storageLayout.CreateTextInput(val.ToString(), value => {
                ulong parsed;
                if(ulong.TryParse(value, out parsed))
                {
                    UpdateAnalyzerFreeStorageWarningValue(parsed);
                }
                layout.Refresh();
            });

            //Check interval
            var timeLayout = layout.CreateHorizontalLayout();
            timeLayout.CreateLabel("Check Interval (Seconds)");
            timeLayout.CreateTextInput(_storageAnalyzer.Config.AnalysisInterval.ToString(), value => {
                float parsed;
                if(float.TryParse(value, out parsed))
                {
                    UpdateAnalyzerCheckInterval(parsed);
                }
                layout.Refresh();
            });

            //Auto stop
            layout.CreateToggleButton(
                "Stop On First Warning",
                _storageAnalyzer.Config.StopOnFirstWarning,
                value => {
                    UpdateAnalyzerAutoStop(!_storageAnalyzer.Config.StopOnFirstWarning);
                    layout.Refresh();
                });
        }

        StorageUnit StringToStorageUnit(string value)
        {
            for(int i = 0; i < _storageUnitNames.Length; i++)
            {
                if(value == _storageUnitNames[i])
                {
                    return (StorageUnit)i;
                }
            }
            return StorageUnit.Bytes;
        }

        void UpdateAnalyzerFreeStorageWarningValue(ulong value)
        {
            var config = _storageAnalyzer.Config;
            config.FreeStorageWarning = new StorageAmount(value, _storageAnalyzerUnit);
            _storageAnalyzer.Config = config;
        }

        void UpdateAnalyzerCheckInterval(float value)
        {
            var config = _storageAnalyzer.Config;
            config.AnalysisInterval = value;
            _storageAnalyzer.Config = config;
        }

        void UpdateAnalyzerAutoStop(bool value)
        {
            var config = _storageAnalyzer.Config;
            config.StopOnFirstWarning = value;
            _storageAnalyzer.Config = config;
        }

        void OnLowStorageSpace(ulong freeBytesStorage, ulong requiredBytesStorage)
        {
            if(_console != null)
            {
                _console.Print("Low Storage Detected! [Free: " + freeBytesStorage + " bytes | Expected: " + requiredBytesStorage + " bytes]");
            }

            if(_layout != null)
            {
                _layout.Refresh();
            }
        }
    }
}

#endif
