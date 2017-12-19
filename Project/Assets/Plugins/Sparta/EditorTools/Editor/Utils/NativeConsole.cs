using System.Diagnostics;
using System.Text;

namespace SpartaTools.Editor.Utils
{
    public static class NativeConsole
    {
        public enum OutputType
        {
            Standard,
            Error
        }

        public class Result
        {
            readonly StringBuilder _outputBuilder = new StringBuilder();
            readonly StringBuilder _infoBuilder = new StringBuilder();
            readonly StringBuilder _errorBuilder = new StringBuilder();

            public string Output
            {
                get
                {
                    return _outputBuilder.ToString();
                }
            }

            public string Standard
            {
                get
                {
                    return _infoBuilder.ToString();
                }
            }
            public string Error
            {
                get
                {
                    return _errorBuilder.ToString();
                }
            }

            public bool HasError
            { 
                get
                { 
                    return _errorBuilder.Length > 0;
                }
            }

            public void Log(NativeConsole.OutputType type, string message)
            {
                _outputBuilder.Append(message);
                switch(type)
                {
                case NativeConsole.OutputType.Standard:
                    _infoBuilder.Append(message);
                    break;
                case NativeConsole.OutputType.Error:
                    _errorBuilder.Append(message);
                    break;
                }
            }

            public int Code { get; set; }
        }

        public static Result RunProcess(string exe, string args, string path)
        {
            var result = new Result();

            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    CreateNoWindow = true,
                    WorkingDirectory = path
                }
            };
            proc.Start();

            while(!proc.StandardOutput.EndOfStream || !proc.StandardError.EndOfStream)
            {
                if(!proc.StandardOutput.EndOfStream)
                {
                    string content = proc.StandardOutput.ReadToEnd();
                    if(content != null)
                    {
                        result.Log(OutputType.Standard, content);
                    }
                }
                if(!proc.StandardError.EndOfStream)
                {
                    string content = proc.StandardError.ReadToEnd();
                    if(content != null)
                    {
                        result.Log(OutputType.Error, content);
                    }
                }
            }

            proc.WaitForExit();
            result.Code = proc.ExitCode;
            proc.Close();
            return result;
        }
    }
}