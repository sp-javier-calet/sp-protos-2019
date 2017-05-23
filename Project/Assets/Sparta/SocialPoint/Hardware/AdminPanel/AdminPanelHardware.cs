#if ADMIN_PANEL 

using System;
using SocialPoint.AdminPanel;

namespace SocialPoint.Hardware
{
    public sealed class AdminPanelHardware : IAdminPanelConfigurer, IAdminPanelGUI
    {
        AdminPanelConsole _console;
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
            layout.CreateLabel("Device Info");
            layout.CreateTextArea(_deviceInfo.ToString());

            layout.CreateLabel("Storage Analyzer");
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
            layout.CreateToggleButton(
                _listeningStorageWarning ? "Unregister Listener" : "Register Listener",
                _listeningStorageWarning,
                value => {
                    if(_listeningStorageWarning)
                    {
                        _storageAnalyzer.UnregisterLowStorageWarningHandler(OnLowStorageSpace);
                    }
                    else
                    {
                        _storageAnalyzer.RegisterLowStorageWarningHandler(OnLowStorageSpace);
                    }
                    _listeningStorageWarning = !_listeningStorageWarning;
                    layout.Refresh();
                });

            var storageLayout = layout.CreateHorizontalLayout();
            storageLayout.CreateLabel("Storage Warning At (" + _storageAnalyzerUnit.ToString() + ")");
            storageLayout.CreateTextInput(StorageToDisplayValue(_storageAnalyzer.Config.FreeStorageWarning).ToString(), value => {
                try
                {
                    var parsed = ulong.Parse(value);
                    UpdateAnalyzerFreeStorageWarningValue(parsed);
                }
                catch(Exception)
                {
                }
                layout.Refresh();
            });

            var timeLayout = layout.CreateHorizontalLayout();
            timeLayout.CreateLabel("Check Interval (Seconds)");
            timeLayout.CreateTextInput(_storageAnalyzer.Config.AnalysisInterval.ToString(), value => {
                try
                {
                    var parsed = float.Parse(value);
                    UpdateAnalyzerCheckInterval(parsed);
                }
                catch(Exception)
                {
                }
                layout.Refresh();
            });
        }

        ulong StorageToDisplayValue(ulong value)
        {
            return StorageUtils.TransformStorageUnit(value, StorageUnit.Bytes, _storageAnalyzerUnit);
        }

        ulong DisplayToStorageValue(ulong value)
        {
            return StorageUtils.TransformStorageUnit(value, _storageAnalyzerUnit, StorageUnit.Bytes);
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
            config.FreeStorageWarning = DisplayToStorageValue(value);
            _storageAnalyzer.Config = config;
        }

        void UpdateAnalyzerCheckInterval(float value)
        {
            var config = _storageAnalyzer.Config;
            config.AnalysisInterval = value;
            _storageAnalyzer.Config = config;
        }

        void OnLowStorageSpace(ulong freeBytesStorage, ulong requiredBytesStorage)
        {
            if(_console != null)
            {
                _console.Print("Low Storage Detected! [Free: " + freeBytesStorage + " bytes | Expected: " + requiredBytesStorage + " bytes]");
            }
        }
    }
}

#endif
