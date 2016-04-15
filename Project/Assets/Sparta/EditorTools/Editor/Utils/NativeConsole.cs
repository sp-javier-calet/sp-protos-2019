using System;
using System.Diagnostics;

namespace SpartaTools.Editor.Utils
{
    public static class NativeConsole
    {
        public enum OutputType
        {
            Standard,
            Error
        }

        public static int RunProcess(string exe, string args, string path, Action<OutputType, string> output)
        {
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
                        output(OutputType.Standard, content);
                    }
                }
                if(!proc.StandardError.EndOfStream)
                {
                    string content = proc.StandardError.ReadToEnd();
                    if(content != null)
                    {
                        output(OutputType.Error, content);
                    }
                }
            }

            proc.WaitForExit();
            int code = proc.ExitCode;
            proc.Close();
            return code;
        }
    }
}