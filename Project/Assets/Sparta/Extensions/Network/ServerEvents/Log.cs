using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.Network.ServerEvents
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

        public ErrorDelegate ResponseDelegate;

        public LogLevel Level { private set; get; }

        public string Message { private set; get; }

        public AttrDic Context { private set; get; }

        public Log(LogLevel level, string message, AttrDic context = null, ErrorDelegate responseDelegate = null)
        {
            Level = level;
            Message = message;
            Context = context ?? new AttrDic();
            ResponseDelegate = responseDelegate;
        }

        public Attr ToAttr()
        {
            var dic = new AttrDic();
            dic.SetValue(AttrKeyLevel, Level.ToString().ToLower());
            dic.SetValue(AttrKeyMessage, Message);
            dic.Set(AttrKeyContext, Context);
            return dic;
        }
    }
}