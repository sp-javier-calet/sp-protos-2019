using SocialPoint.Utils;
using System.Collections.Generic;
using UnityEditor;

namespace SocialPoint.Exporter
{
    [CustomPropertyDrawer(typeof(ExportTagSetAttribute))]
    public class ExportTagSetPropertyDrawer : TagSetPropertyDrawer
    {
        ExporterContainer _container;

        protected override IEnumerable<string> GetAllTags()
        {
            if(_container == null)
            {
                _container = ExporterSettings.ExporterContainer;
            }
            return _container.Tags;
        }
    }
}
