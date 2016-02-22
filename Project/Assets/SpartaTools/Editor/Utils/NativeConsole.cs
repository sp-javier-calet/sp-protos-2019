using System;
using System.Diagnostics;

namespace SpartaTools.Editor.Utils
{
    public static class NativeConsole
    {
        public static int RunProcess(string exe, string args, string path, Action<string> output)
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
            while(!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if(output != null)
                {
                    output(line);
                }
            }
            proc.WaitForExit();
            int code = proc.ExitCode;
            proc.Close();
            return code;
        }
    }
}