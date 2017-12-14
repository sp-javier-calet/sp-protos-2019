#if UNITY_5_3_OR_NEWER
#define UNITY
#endif

using System;

namespace SocialPoint.Console
{
    public static class ConsoleUtils
    {
        public static int Verbose = 0;

        public static void SetForegroundColor(ConsoleColor color)
        {
#if !UNITY
            System.Console.ForegroundColor = color;
#endif
        }       
        
        public static void ResetColor()
        {
#if !UNITY
            System.Console.ResetColor();
#endif
        }

        public static void WriteOutput(string data)
        {
            System.Console.Write(data);
        }

        public static void WriteErrorLine(string format, params Object[] arg)
        {
            SetForegroundColor(ConsoleColor.Red);
            System.Console.WriteLine(format, arg);
            ResetColor();
        }

        public static void WriteInfoLine(string format, params Object[] arg)
        {
            SetForegroundColor(ConsoleColor.DarkYellow);
            System.Console.WriteLine(format, arg);
            ResetColor();
        }

        public static void WriteSuccessLine(string format, params Object[] arg)
        {
            SetForegroundColor(ConsoleColor.DarkGreen);
            System.Console.WriteLine(format, arg);
            ResetColor();
        }

        public static void WriteDebugLine(string format, params Object[] arg)
        {
            WriteDebugLine(1, format, arg);
        }

        public static void WriteDebugLine(int level, string format, params Object[] arg)
        {
            if(Verbose >= level)
            {
                System.Console.WriteLine(format, arg);
            }
        }

        public static void WriteException(Exception e, string msg=null)
        {
            if(msg != null)
            {
                WriteErrorLine(msg);
            }
            WriteErrorLine(e.GetType().Name + ": " + e.Message);
            if(Verbose > 0)
            {
                WriteErrorLine(e.StackTrace);
            }
        }
    }
}

