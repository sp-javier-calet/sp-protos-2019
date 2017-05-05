using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Exporter
{
    public class ExporterContainer : ScriptableObject
    {
        public string ExportPath;

        public List<BaseExporter> Exporters = new List<BaseExporter>();
        public List<string> Tags = new List<string>();

        public void CleanEmptyExporters()
        {
            for(int i = Exporters.Count - 1; i >= 0; --i)
            {
                var exporter = Exporters[i];
                if(exporter == null)
                {
                    Exporters.RemoveAt(i);
                }
            }
        }

        public int Export(BaseExporter exporter, Log.ILogger log)
        {
            var filesCount = 0;
            if(exporter != null)
            {
                log.Log("<size=16>" + exporter + " starting...</size>");
                try
                {
                    var files = new FileManagerObserver(new FileManagerWrapper(new UnityFileManager(), ExportPath));
                    exporter.Export(files, log);
                    log.Log(exporter + " finished OK:");
                    filesCount = files.WriteFiles.Count;
                    for(var i=0; i<filesCount; i++)
                    {
                        log.Log("    - " + files.WriteFiles[i] + " exported");
                    }
                }
                catch(Exception e)
                {
                    log.LogException(e);
                    throw;
                }
            }
            return filesCount;
        }

        public void Export(Log.ILogger log)
        {
            int exportedFileCount = 0, errorCount = 0, okCount = 0;
            for(int i = 0; i < Exporters.Count; ++i)
            {
                try
                {
                    exportedFileCount += Export(Exporters[i], log);
                    ++okCount;
                }
                catch(Exception)
                {
                    ++errorCount;
                }
            }
            var msg = string.Format(
                "<size=16>{0} exports finished:\n    - {1} finished OK ({2} files exported)\n    - {3} finished with errors</size>",
                Exporters.Count, okCount, exportedFileCount, errorCount);
            if(errorCount == 0)
            {
                log.Log(msg);
            }
            else if(okCount == 0)
            {
                log.LogError(msg);
            }
            else
            {
                log.LogWarning(msg);
            }
        }
    }
}