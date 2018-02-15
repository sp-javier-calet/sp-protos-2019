using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace SpartaTools.Editor.Build
{
    public class CompilerSettingsFile
    {
        readonly string[] CompilerSettingsFiles = { "mcs" };
        const string CompilerSettingsFileExtension = ".rsp";

        public readonly WarningAsErrorsSettings WarningAsErrors;
        public readonly DefinedSymbolsSettings DefinedSymbols;

        public CompilerSettingsFile()
        {
            WarningAsErrors = new WarningAsErrorsSettings();
            DefinedSymbols = new DefinedSymbolsSettings();
        }

        public void Save()
        {
            var builder = new StringBuilder();
            WarningAsErrors.Write(builder);
            DefinedSymbols.Write(builder);

            // Write files
            var content = builder.ToString();
            var assetsFolder = Application.dataPath;
            for(var i = 0; i < CompilerSettingsFiles.Length; ++i)
            {
                var fileName = CompilerSettingsFiles[i] + CompilerSettingsFileExtension;
                File.WriteAllText(Path.Combine(assetsFolder, fileName), content);
            }
        }

        #region Warning as Errors

        public sealed class WarningAsErrorsSettings
        {
            const string GlobalEnabled = "-warnaserror+";
            const string EnabledFlag = "-warnaserror+:";
            const string DisabledFlag = "-warnaserror-:";

            readonly List<int> Enabled;
            readonly List<int> Disabled;

            public WarningAsErrorsSettings()
            {
                Enabled = new List<int>();
                Disabled = new List<int>();
            }

            public bool Enable { get ; set; }

            public void EnableWarning(int code)
            {
                Disabled.Remove(code);
                Enabled.Add(code);
            }

            public void DisableWarning(int code)
            {
                Enabled.Remove(code);
                Disabled.Add(code);
            }

            public void Write(StringBuilder builder)
            {
                if(Enable)
                {
                    builder.AppendLine(GlobalEnabled);
                }

                var itr = Enabled.GetEnumerator();
                while(itr.MoveNext())
                {
                    builder.Append(EnabledFlag).AppendLine(itr.Current.ToString());
                }
                itr.Dispose();

                itr = Disabled.GetEnumerator();
                while(itr.MoveNext())
                {
                    builder.Append(DisabledFlag).AppendLine(itr.Current.ToString());
                }
                itr.Dispose();
            }
        }

        #endregion

        #region Global Defined symbols

        public sealed class DefinedSymbolsSettings
        {
            const string DefineFlag = "-define:";

            readonly List<string> Symbols;

            public DefinedSymbolsSettings()
            {
                Symbols = new List<string>();
            }

            public void Add(string symbol)
            {
                Symbols.Add(symbol);
            }

            public void Write(StringBuilder builder)
            {
                var itr = Symbols.GetEnumerator();
                while(itr.MoveNext())
                {
                    builder.Append(DefineFlag).AppendLine(itr.Current);
                }
                itr.Dispose();
            }
        }

        #endregion
    }
}