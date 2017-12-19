using UnityEngine;

namespace SocialPoint.Hardware
{
    public class EmptyDeviceInfo : IDeviceInfo
    {
        public EmptyDeviceInfo()
        {
            MemoryInfo = new EmptyMemoryInfo();
            StorageInfo = new EmptyStorageInfo();
            AppInfo = new EmptyAppInfo();
            NetworkInfo = new EmptyNetworkInfo();
        }

        public string String { get; set; }

        public string Uid { get; set; }

        public IMemoryInfo MemoryInfo { get; set; }

        public IStorageInfo StorageInfo
        {
            get;
            set;
        }

        public IAppInfo AppInfo
        {
            get;
            set;
        }

        public INetworkInfo NetworkInfo
        {
            get;
            set;
        }

        public string Platform
        {
            get;
            set;
        }

        public string PlatformVersion
        {
            get;
            set;
        }

        public string Architecture
        {
            get;
            set;
        }

        public string Model
        {
            get;
            set;
        }

        public string Language
        {
            get;
            set;
        }

        public string AdvertisingId
        {
            get;
            set;
        }

        public string IDFA
        {
            get
            {
                return AdvertisingId;
            }
        }

        public bool AdvertisingIdEnabled
        {
            get;
            set;
        }

        public bool Rooted
        {
            get;
            set;
        }

        public int MaxTextureSize
        {
            get;
            set;
        }

        public Vector2 ScreenSize
        {
            get;
            set;
        }

        public Rect SafeAreaRectSize
        {
            get;
            set;
        }

        public float ScreenDpi
        {
            get;
            set;
        }

        public int CpuCores
        {
            get;
            set;
        }

        public int CpuFreq
        {
            get;
            set;
        }

        public string CpuModel
        {
            get;
            set;
        }

        public string CpuArchitecture
        {
            get;
            set;
        }

        public string OpenglVendor
        {
            get;
            set;
        }

        public string OpenglRenderer
        {
            get;
            set;
        }

        public string OpenglExtensions
        {
            get;
            set;
        }

        public int OpenglShadingVersion
        {
            get;
            set;
        }

        public string OpenglVersion
        {
            get;
            set;
        }

        public int OpenglMemorySize
        {
            get;
            set;
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

