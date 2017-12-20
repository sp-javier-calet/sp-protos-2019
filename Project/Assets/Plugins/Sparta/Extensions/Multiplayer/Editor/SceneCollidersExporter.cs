using SocialPoint.Exporter;
using SocialPoint.IO;
using SocialPoint.Physics;
using SocialPoint.Utils;
using SocialPoint.Base;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SocialPoint.Multiplayer
{
    public class SceneCollidersExporter : SceneExporter
    {
        public string ExportName = "SceneColliders";

        [ExportTagSet]
        [SerializeField]
        TagSet _tags;

        protected override void ExportScene(IFileManager files, Log.ILogger log)
        {
            var unityColliders = ExportConfiguration.FindObjectsOfType<UnityEngine.Collider>(_tags);
            log.Log(string.Format("Saving {0} colliders...", unityColliders.Length));
            var fh = files.Write(ExportName);

            fh.Writer.Write(unityColliders.Length);
            for(var i=0; i<unityColliders.Length; i++)
            {
                unityColliders[i].ToMultiplayer().Serialize(fh.Writer);
            }
            fh.CloseStream();
        }

        public override string ToString()
        {
            return "Scene Colliders exporter" + (Scene != null ? (" [" + Scene.name + ".unity]") : string.Empty);
        }
    }
}