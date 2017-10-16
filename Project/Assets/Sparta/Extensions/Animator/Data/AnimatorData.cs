using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public class AnimatorData : INetworkShareable
    {
        public string Name;
        public LayerData[] Layers;
        public ParameterData[] Parameters;

        public void Serialize(IWriter writer)
        {
            writer.Write(Name);
            writer.WriteArray<LayerData>(Layers);
            writer.WriteArray<ParameterData>(Parameters);
        }

        public void Deserialize(IReader reader)
        {
            Name = reader.ReadString();
            Layers = reader.ReadArray<LayerData>();
            Parameters = reader.ReadArray<ParameterData>();
        }
    }
}
    