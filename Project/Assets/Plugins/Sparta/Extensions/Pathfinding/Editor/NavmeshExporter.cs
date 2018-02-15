using System;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Exporter;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Pathfinding
{
    public class NavmeshExporter : SceneExporter
    {
        public float AgentHeight = 2.0f;
        public float AgentRadius = 0.6f;
        public string ExportName = "Navmesh";

        [ExportTagSet]
        [SerializeField]
        TagSet _tags;

        [AreaSettings]
        [SerializeField]
        NavmeshAreaSettings _areaSettings;

        protected override void ExportScene(IFileManager files, Log.ILogger log)
        {
            if(string.IsNullOrEmpty(GetPathExportName(ExportName)))
            {
                throw new InvalidOperationException("Export Name is not configured");
            }

            if(_tags == null || _tags.Count == 0)
            {
                throw new InvalidOperationException("Navmesh Container Tag is not configured");
            }

            var gos = ExportConfiguration.FindObjects(_tags);

            if(gos == null || gos.Length == 0)
            {
                throw new InvalidOperationException("Unable to find a GameObjects with tag.");
            }

            //Important: prepare area settings before extracting convex volumes
            _areaSettings.InitData();

            var settings = NavMeshGenerationSettings.Default;
            settings.AgentHeight = AgentHeight;
            settings.AgentRadius = AgentRadius;
            var convexVolumeMarkers = PathfindingUnityUtils.GetConvexVolumeMarkers(gos);
            var convexVolumes = ExtractConvexVolumes(convexVolumeMarkers);

            var navMesh = PathfindingUnityUtils.CreateNavMesh(gos, settings, convexVolumes, _areaSettings.ExportFlags);

            var fh = files.Write(GetPathExportName(ExportName));
            NavMeshSerializer.Instance.Serialize(navMesh, fh.Writer);
            NavmeshAreaSettingsSerializer.Instance.Serialize(_areaSettings, fh.Writer);
            fh.CloseStream();
        }

        public override string ToString()
        {
            return "Navmesh exporter " + (Scene != null ? ("[" + Scene.name + ".unity]") : "");
        }

        IEnumerable<ConvexVolume> ExtractConvexVolumes(IEnumerable<ConvexVolumeMarker> markers)
        {
            var convexVolumes = new List<ConvexVolume>();
            var itr = markers.GetEnumerator();
            while(itr.MoveNext())
            {
                try
                {
                    var data = itr.Current.GetExportData();
                    if(data != null)
                    {
                        convexVolumes.Add(CreateConvexVolume(data));
                    }
                }
                catch
                {
                    throw new Exception("Convex volume marker " + itr.Current.name + " has invalid data. "
                    + "Make sure:\n"
                    + "1- The volume has an attached export configuration.\n"
                    + "2- The intended area is the first tag in it.\n"
                    + "3- That area is added to the exporter settings with assigned flags.\n");
                }
            }
            itr.Dispose();
            return convexVolumes;
        }

        ConvexVolume CreateConvexVolume(ConvexVolumeMarker.ExportData data)
        {
            return new ConvexVolume {
                Vertices = data.Vertices,
                Hmin = data.Hmin,
                Hmax = data.Hmax,
                Area = _areaSettings.ExportArea(data.Area),
                Tag = data.Tag,
            };

        }
    }
}
