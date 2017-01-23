using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    public class ServerOutputs
    {
        public Dictionary<string, BundleDependenciesData> CurrentBundles;

        public ServerOutputs(Dictionary<string, BundleDependenciesData> updatedBundles)
        {
            CurrentBundles = updatedBundles;
        }

        public void Save(string path, bool pretty = true)
        {
            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = pretty;
            JsonMapper.ToJson(CurrentBundles, writer);
            File.WriteAllText(path, writer.ToString());
        }
    }
}
