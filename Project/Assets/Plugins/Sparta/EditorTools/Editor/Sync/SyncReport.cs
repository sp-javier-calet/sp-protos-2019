using UnityEngine;
using System.Text;

namespace SpartaTools.Editor.Sync
{
    public static class SyncReport
    {
        static Report CurrentReport = new Report();

        public static void Start(string title = "")
        {
            CurrentReport = new Report(title);
        }

        public static void Log(string line)
        {
            CurrentReport.Log(line);
        }

        public static void Dump()
        {
            CurrentReport.Dump();
        }

        /*
         * Internal class to manage report content
         */
        class Report
        {
            readonly string _title;
            readonly StringBuilder _builder;

            public Report(string reportTitle = "")
            {
                _title = reportTitle;
                _builder = new StringBuilder();
            }

            public void Log(string line)
            {
                _builder.AppendLine(line);
            }

            public void Dump()
            {
                Debug.Log("SyncReport: " + _title + "\n" + _builder);
            }
        }
    }
}
