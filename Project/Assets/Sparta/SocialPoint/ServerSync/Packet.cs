using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.ServerSync
{
    /// <summary>
    /// A container for a list of pending commands
    /// </summary>
    public class Packet : IEnumerable<PackedCommand>
    {
        public delegate bool FilterDelegate(PackedCommand item);

        public delegate void FinishDelegate(Error err);

        public const int NoId = -1;
        static readonly string CommandsKey = "commands";
        static readonly string IdKey = "pid";
        static readonly string TimestampKey = "ts";
        readonly IList<PackedCommand> _commands = new List<PackedCommand>();
        public int Id = NoId;
        public long Timestamp;
        public FinishDelegate Finished = delegate {
        };

        public int Count
        {
            get
            {
                return _commands.Count;
            }
        }

        public bool HasId
        {
            get
            {
                return Id != NoId;
            }
        }

        public bool Atomic
        {
            get
            {
                foreach(var pcmd in _commands)
                {
                    if(!pcmd.Command.Atomic)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public Packet()
        {
            Timestamp = TimeUtils.Timestamp;
        }

        public Packet(Attr data)
        {
            FromAttr(data);
        }

        public  IEnumerator<PackedCommand> GetEnumerator()
        {
            return _commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(PackedCommand pcmd)
        {
            return _commands.Remove(pcmd);
        }

        public int Remove(FilterDelegate dlg)
        {
            int count = 0;
            if(dlg != null)
            {
                for(int i = _commands.Count - 1; i >= 0; i--)
                {
                    var pcmd = _commands[i];
                    if(dlg(pcmd))
                    {
                        count++;
                        _commands.RemoveAt(i);
                    }
                }
            }
            return count;
        }

        public void FromAttr(Attr data)
        {
            var dic = data.AsDic;
            Id = dic.GetValue(IdKey).ToInt();
            Timestamp = dic.GetValue(TimestampKey).ToLong();
            if(dic.ContainsKey(CommandsKey))
            {
                var cmds = dic.Get(CommandsKey).AsList;
                foreach(var cmd in cmds)
                {
                    Add(new Command(cmd));
                }
            }
        }

        public Attr ToAttr()
        {
            var data = new AttrDic();
            var cmds = new AttrList();
            foreach(var pcmd in _commands)
            {
                var cmd = pcmd.Command;
                if(cmd != null)
                {
                    cmds.Add(cmd.ToAttr());
                }
            }
            data.Set(CommandsKey, cmds);
            data.SetValue(IdKey, Id);
            data.SetValue(TimestampKey, Timestamp);
            return data;
        }

        public Attr ToRequestAttr()
        {
            var data = new AttrDic();
            if(_commands.Count > 0)
            {
                var cmds = new AttrList();
                foreach(var pcmd in _commands)
                {
                    var cmd = pcmd.Command;
                    if(cmd != null)
                    {
                        cmds.Add(cmd.ToRequestAttr());
                    }
                }
                data.Set(CommandsKey, cmds);
            }
            data.SetValue(IdKey, Id);
            data.SetValue(TimestampKey, Timestamp);
            return data;
        }

        public bool Add(Command cmd, ErrorDelegate callback = null)
        {
            DebugUtils.Assert(cmd != null);

            if(cmd.Unique)
            {
                for(int i = _commands.Count - 1; i >= 0; i--)
                {
                    var pcmd = _commands[i];
                    if(pcmd.Command.Name == cmd.Name)
                    {
                        if (pcmd.Command.Timestamp > cmd.Timestamp)
                        {
                            return false;
                        }
                        else
                        {
                            callback += pcmd.Finished;
                            _commands.RemoveAt(i);
                        }
                    }
                }
            }

            var item = new PackedCommand(cmd, callback);
            _commands.Add(item);
            return true;
        }

        public PackedCommand GetCommand(string id)
        {
            foreach(var pcmd in _commands)
            {
                if(pcmd.Command.Id == id)
                {
                    return pcmd;
                }
            }
            return null;
        }
    }
}
