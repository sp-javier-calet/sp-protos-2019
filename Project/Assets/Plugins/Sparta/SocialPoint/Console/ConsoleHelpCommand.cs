using System;
using System.Collections.Generic;

namespace SocialPoint.Console
{
    public sealed class ConsoleHelpCommand : ConsoleCommand
    {
        readonly ConsoleApplication _app;

        public ConsoleHelpCommand(ConsoleApplication app)
        {
            _app = app;
        }

        override public void Define()
        {
            Description = "show help about the console application";
            WithOption(new ConsoleCommandOption("*|command")
                .WithDescription("the command to show help for"));
        }


        static void WriteCommandInfo(string name, ConsoleCommand cmd)
        {
            ConsoleUtils.SetForegroundColor(ConsoleColor.DarkYellow);
            System.Console.Write(name);
            ConsoleUtils.ResetColor();
            System.Console.WriteLine(": {0}", cmd.Description);
        }

        static void WriteOptionInfo(ConsoleCommandOption opt)
        {
            ConsoleUtils.SetForegroundColor(ConsoleColor.Yellow);
            System.Console.Write(opt.Config);
            ConsoleUtils.ResetColor();
            System.Console.WriteLine(": {0}", opt.Description);
        }

        override public void Execute()
        {
            ConsoleUtils.SetForegroundColor(ConsoleColor.DarkGreen);
            System.Console.WriteLine(_app.Name);
            ConsoleUtils.ResetColor();
            System.Console.WriteLine("----");
            if(_app.Description != null)
            {
                System.Console.WriteLine(_app.Description);
                System.Console.WriteLine("");
            }
            if(_app.HelpText != null)
            {
                System.Console.WriteLine(_app.HelpText);
                System.Console.WriteLine("");
            }

            var cmdName = this["command"].Value;
            if(!string.IsNullOrEmpty(cmdName))
            {
                ConsoleCommand cmd = _app.FindCommand(cmdName);
                if(cmd == null)
                {
                    throw new ConsoleException("Could not find the command");
                }
                else
                {
                    WriteCommandInfo(cmdName, cmd);
                    System.Console.WriteLine("----");
                    var itr = cmd.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var opt = itr.Current;
                        WriteOptionInfo(opt);
                    }
                    itr.Dispose();
                }
            }
            else
            {
                var itr = _app.GetEnumerator();
                while(itr.MoveNext())
                {
                    var pair = itr.Current;
                    WriteCommandInfo(pair.Key, pair.Value);
                }
                itr.Dispose();
            }
        }
    }
}
