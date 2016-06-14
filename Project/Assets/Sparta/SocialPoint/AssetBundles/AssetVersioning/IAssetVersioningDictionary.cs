using System;
using System.Collections.Generic;

namespace SocialPoint.AssetVersioning
{
    public class AssetVersioningData
    {
        public string Client;
        public int Version;
        public bool IsLocal;
        public string Parent;
        public uint CRC;
    }

    public interface IAssetVersioningDictionary : IDictionary<string, AssetVersioningData>
    {
        IList<string> GetLocalBundles();

        IList<string> GetLocalTextureNames();
    }
}

