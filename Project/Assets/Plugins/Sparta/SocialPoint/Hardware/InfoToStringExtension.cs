using System.Text;

namespace SocialPoint.Hardware
{
    public static class InfoToStringExtension
    {
        public static string ToString(this IDeviceInfo info)
        {
            var str = new StringBuilder();
            str.Append("string: ");
            str.Append(info.String);
            str.Append("\nuid: ");
            str.Append(info.Uid);
            str.Append("\nidfa: ");
            str.Append(info.AdvertisingId);
            str.Append(" ");
            str.Append(info.AdvertisingIdEnabled);
            str.Append("\nplatform: ");
            str.Append(info.Platform);
            str.Append(" ");
            str.Append(info.PlatformVersion);
            str.Append("\narchitecture: ");
            str.Append(info.Architecture);
            if(info.AppInfo != null)
            {
                str.Append("\napp:\n");
                str.Append(info.AppInfo.ToString());
            }
            if(info.MemoryInfo != null)
            {
                str.Append("\nmemory:\n");
                str.Append(info.MemoryInfo.ToString());
            }
            if(info.StorageInfo != null)
            {
                str.Append("\nstorage:\n");
                str.Append(info.StorageInfo.ToString());
            }
            if(info.NetworkInfo != null)
            {
                str.Append("\nnet:\n");
                str.Append(info.NetworkInfo.ToString());
            }
            return str.ToString();
        }

        public static string ToString(this IAppInfo info)
        {
            var str = new StringBuilder();

            str.Append("seed id: ");
            str.Append(info.SeedId);
            str.Append("\nid: ");
            str.Append(info.Id);
            str.Append("\nversion: ");
            str.Append(info.Version);
            str.Append("\nshort version: ");
            str.Append(info.ShortVersion);
            str.Append("\nlanguage: ");
            str.Append(info.Language);
            str.Append("\ncountry: ");
            str.Append(info.Country);

            return str.ToString();
        }

        public static string ToString(this IMemoryInfo info)
        {
            var str = new StringBuilder();

            str.Append("total memory: ");
            str.Append(info.TotalMemory);
            str.Append("\nfree memory: ");
            str.Append(info.FreeMemory);
            str.Append("\nused memory: ");
            str.Append(info.UsedMemory);
            str.Append("\nactive memory: ");
            str.Append(info.ActiveMemory);

            return str.ToString();
        }

        public static string ToString(this IStorageInfo info)
        {
            var str = new StringBuilder();

            str.Append("total storage: ");
            str.Append(info.TotalStorage);
            str.Append("\nfree storage: ");
            str.Append(info.FreeStorage);
            str.Append("\nused storage: ");
            str.Append(info.UsedStorage);

            return str.ToString();
        }

        public static string ToString(this INetworkInfo info)
        {
            var str = new StringBuilder();

            str.Append("conectivity: ");
            str.Append(info.Connectivity);
            str.Append("\nproxy: ");
            str.Append(info.Proxy != null ? info.Proxy.ToString() : "");
            str.Append("\nip address: ");
            str.Append(info.IpAddress);

            return str.ToString();
        }
    }

}