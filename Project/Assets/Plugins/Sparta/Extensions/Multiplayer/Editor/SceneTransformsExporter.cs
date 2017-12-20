using SocialPoint.Exporter;
using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Base;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.Multiplayer
{
    public class SceneTransformsExporter : SceneExporter
    {
        public string ExportName = "SceneTransforms";

        [ExportTagSet]
        [SerializeField]
        TagSet _tags;

        protected override void ExportScene(IFileManager files, Log.ILogger log)
        {
            var parents = ExportConfiguration.FindObjects(_tags);
            log.Log(string.Format("Saving {0} transforms...", parents.Length));
            var fh = files.Write(ExportName);
            fh.Writer.Write(parents.Length);
            for(var i=0; i<parents.Length; i++)
            {
                parents[i].transform.ToMultiplayerSceneTransform().Serialize(fh.Writer);
            }
            fh.CloseStream();
        }

        public override string ToString()
        {
            return "Scene Transforms exporter " + (Scene != null ? ("[" + Scene.name + ".unity]") : "");
        }
    }
}
