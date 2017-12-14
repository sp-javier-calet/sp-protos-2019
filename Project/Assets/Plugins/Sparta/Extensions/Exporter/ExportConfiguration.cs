using UnityEngine;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Exporter
{
    public class ExportTagSetAttribute : PropertyAttribute
    {
    }

    public class ExportConfiguration : MonoBehaviour
    {
        [ExportTagSet]
        public TagSet Tags;

        public static T[] FindObjectsOfType<T>(TagSet tags) where T : UnityEngine.Component
        {
            var list = new List<T>(Component.FindObjectsOfType<T>());
            list.RemoveAll(elm => {
                var config = elm.gameObject.GetComponent<ExportConfiguration>();
                return config == null || !config.Tags.MatchAny(tags);
            });
            return list.ToArray();
        }

        public static GameObject[] FindObjects(TagSet tags)
        {
            var list = new List<GameObject>(Component.FindObjectsOfType<GameObject>());
            list.RemoveAll(go => {
                var config = go.GetComponent<ExportConfiguration>();
                return config == null || !config.Tags.MatchAny(tags);
            });
            return list.ToArray();
        }
    }

    public static class ExportConfigurationExtensions
    {
        public static TagSet GetExportTags(this Transform t)
        {
            var config = t.GetComponent<ExportConfiguration>();
            return config == null ? null : config.Tags;
        }
    }
}
