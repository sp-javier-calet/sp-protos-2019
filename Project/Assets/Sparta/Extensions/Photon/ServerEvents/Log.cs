using SocialPoint.Attributes;

namespace SocialPoint.Photon.ServerEvents
{
    public enum LogLevel
    {
        Debug,
        Info,
        Notice,
        Warning,
        Error,
        Critical,
        Alert,
        Emergency
    }

    public class Log
    {
        const string AttrKeyLevel = "level";
        const string AttrKeyMessage = "message";
        const string AttrKeyContext = "context";

        public LogLevel Level { private set; get; }

        public string Message { private set; get; }

        public string Context { private set; get; }

        public Log(LogLevel level, string message, string context)
        {
            Level = level;
            Message = message;
            Context = context;
        }

        public Attr ToAttr()
        {
            var dic = new AttrDic();
            dic.SetValue(AttrKeyLevel, Level.ToString().ToLower());
            dic.SetValue(AttrKeyMessage, Message);
            dic.SetValue(AttrKeyContext, Context);
            return dic;
        }
    }
}