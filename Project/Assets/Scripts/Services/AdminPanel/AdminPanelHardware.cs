using Zenject;
using SocialPoint.Hardware;

public class AdminPanelHardware : AdminPanelHardwareGUI 
{
    [Inject]
    public IDeviceInfo InjectDeviceInfo
    {
        set
        {
            DeviceInfo = value;
        }
    }
}
