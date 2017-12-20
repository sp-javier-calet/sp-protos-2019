using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public abstract class STCCommand
    {
        const string CommandIdKey = "cid";
        const string CommandNameKey = "cmd";
        const string CommandTimestampKey = "ts";
        const string CommandArgsKey = "args";

        public string Id { get; private set; }
        public string Name { get; private set; }
        public long Timestamp { get; private set; }

        public AttrDic Args { get; private set; }

        protected STCCommand(string name, AttrDic data)
        {
            if(name != getName(data))
            {
                throw new System.ArgumentException("Not matching command name");
            }

            Name = name;
            Id = getId(data);
            Timestamp = getTimestamp(data);
            Args = getArgs(data);
        }

        public abstract void Exec();

        #region Static data parsing 

        public static string getName(AttrDic data)
        {
            return data.Get(CommandNameKey).AsValue.ToString();
        }

        public static string getId(AttrDic data)
        {
            return data.Get(CommandIdKey).AsValue.ToString();
        }

        public static long getTimestamp(AttrDic data)
        {
            return data.Get(CommandTimestampKey).AsValue.ToLong();
        }

        public static AttrDic getArgs(AttrDic data)
        {
            return data.Get(CommandArgsKey).AsDic;
        }

        #endregion
    }

    #region Command Factory interface

    public interface ISTCCommandFactory
    {
        STCCommand Create(AttrDic data);    
    }

    #endregion
}
