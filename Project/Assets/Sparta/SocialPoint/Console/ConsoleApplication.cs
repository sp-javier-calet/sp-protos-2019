using System.Collections;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.Console
{
    public class ConsoleApplication : IEnumerable<KeyValuePair<string, ConsoleCommand>>
    {
        public string Name;
        public string Description;
        public string HelpText;

        public event ConsoleCommandDelegate BeforeCommandExecute;

        IDictionary<string, ConsoleCommand> _cmds = new Dictionary<string, ConsoleCommand>();
        IList<ConsoleCommandOption> _options = new List<ConsoleCommandOption>();
        readonly ConsoleHelpCommand _helpCmd;
        public ConsoleCommand CurrentCommand;

        public ConsoleApplication()
        {
            Name = System.AppDomain.CurrentDomain.FriendlyName;
            _helpCmd = new ConsoleHelpCommand(this);
            AddCommand("help|--help", _helpCmd);
        }

        public void ShowHelp()
        {
            if(CurrentCommand != _helpCmd)
            {
                CurrentCommand = _helpCmd;
                CurrentCommand.Execute();
            }
        }

        public IEnumerator<KeyValuePair<string, ConsoleCommand>> GetEnumerator()
        {
            return _cmds.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ConsoleCommand this[string name]
        {
            get
            {
                return _cmds[name];
            }
            set
            {
                _cmds[name] = value;
            }
        }

        public ConsoleCommandOption AddOption(ConsoleCommandOption opt)
        {
            foreach(var pair in _cmds)
            {
                pair.Value.WithOption(opt);
            }
            _options.Add(opt);
            return opt;
        }

        public ConsoleCommand AddCommand(string name, ConsoleCommand cmd = null)
        {
            if(cmd == null)
            {
                cmd = new ConsoleCommand();
            }
            cmd.WithOptions(_options);
            cmd.Define();
            _cmds[name] = cmd;
            return cmd;
        }

        static int MatchCommandName(string name, IEnumerable<string> args)
        {
            var parts = name.Split(new []{ ' ' });
            int i = 0;
            foreach(var arg in args)
            {
                if(parts.Length <= i || parts[i] != arg)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        public ConsoleCommand FindCommand(string name)
        {
            ConsoleCommand cmd;
            FindCommand(name.Split(new []{ ' ' }), out cmd);
            return cmd;
        }

        int FindCommand(IEnumerable<string> args, out ConsoleCommand cmd)
        {
            foreach(var pair in _cmds)
            {
                var names = pair.Key.Split(new []{ '|' });
                foreach(var name in names)
                {
                    var cname = name.Replace(' ', '-');
                    if(cname != name)
                    {
                        int i = MatchCommandName(cname, args);
                        if(i > 0)
                        {
                            cmd = pair.Value;
                            return i;
                        }
                    }
                }
            }

            ConsoleCommand bestCmd = null;
            int bestI = 0;
            foreach(var pair in _cmds)
            {
                var names = pair.Key.Split(new []{ '|' });
                foreach(var name in names)
                {
                    int i = MatchCommandName(name, args);
                    if(i > bestI)
                    {
                        bestCmd = pair.Value;
                        bestI = i;
                    }
                }
            }
            cmd = bestCmd;
            return bestI;
        }

        public void Run(string[] args)
        {
            Run(new List<string>(args));
        }

        public void Run(IList<string> args)
        {
            CurrentCommand = null;
            if(args.Count == 0)
            {
                CurrentCommand = _helpCmd;
            }
            else
            {
                int i = FindCommand(args, out CurrentCommand);
                for(; i > 0; i--)
                {
                    args.RemoveAt(0);
                }
            }
            if(CurrentCommand == null)
            {
                throw new ConsoleException("Could not find any valid command");
            }
            CurrentCommand.SetOptionValues(args);
            if(BeforeCommandExecute != null)
            {
                BeforeCommandExecute(CurrentCommand);
            }
            CurrentCommand.Execute();
        }
    }
}
