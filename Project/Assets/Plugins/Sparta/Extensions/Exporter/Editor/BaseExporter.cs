using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Base;

namespace SocialPoint.Exporter
{
    public abstract class BaseExporter : ScriptableObject
    {
        public abstract void Export(IFileManager files, Log.ILogger log);
    }
}