using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.AssetVersioning
{
    public static class AssetVersioningParser
    {
        public static void Parse(AssetVersioningDictionary assetVersioningDictionary, JsonStreamReader reader, string version)
        {
            var assetVersioning = new Dictionary<string, AssetVersioningData>();

            if (reader.Token == StreamToken.ObjectStart)
            {
                while(reader.Read() && reader.Token != StreamToken.ObjectEnd)
                {
                    string key = (string)reader.Value;
                    reader.Read();
                    if (key == "asset_versioning")
                    {
                        ParseAssetVersioningArray(assetVersioning, reader, version);
                    }
                    else
                    {
                        reader.SkipElement();
                    }
                }
            }

            assetVersioningDictionary.SetInternalData(assetVersioning);
        }

        static void ParseAssetVersioningArray(Dictionary<string, AssetVersioningData> assetVersioning, JsonStreamReader reader, string version)
        {
            if (reader.Token == StreamToken.ArrayStart)
            {
                while(reader.Read() && reader.Token != StreamToken.ArrayEnd)
                {
                    ParseAssetVersioning(assetVersioning, reader, version);
                }
            }
        }

        static void ParseAssetVersioning(Dictionary<string, AssetVersioningData> assetVersioning, JsonStreamReader reader, string version)
        {
            if (reader.Token == StreamToken.ObjectStart)
            {
                AssetVersioningData result = new AssetVersioningData();
                string name = null;
                while(reader.Read() && reader.Token != StreamToken.ObjectEnd)
                {
                    string key = (string)reader.Value;
                    reader.Read();
                    if (!reader.CheckVersion(key, version))
                    {
                        reader.SkipToObjectEnd();
                        return;
                    }
                    switch(key)
                    {
                    case "client":
                            if (reader.Token != StreamToken.Null)
                        {
                            string value = reader.GetStringValue();
                            if (!string.IsNullOrEmpty(value))
                            {
                                result.Client = value;
                                break;
                            }
                        }
                        result.Client = DownloadManager.SpamData.Instance.client;
                        break;
                    case "parent":
                            if (reader.Token != StreamToken.Null)
                        {
                            string value = reader.GetStringValue();
                            if (!string.IsNullOrEmpty(value))
                            {
                                result.Parent = reader.GetStringValue();
                            }
                        }
                        break;
                    case "name":
                        name = reader.GetStringValue();
                        break;
                    case "version":
                        result.Version = reader.GetIntValue();
                        break;
                    case "isLocal":
                        result.IsLocal = reader.GetBoolValue();
                        break;
                    default:
                        reader.SkipElement();
                        break;
                    }
                }
                if (name != null)
                {
                    assetVersioning.Add(name, result);
                }
            }
        }
    }
}