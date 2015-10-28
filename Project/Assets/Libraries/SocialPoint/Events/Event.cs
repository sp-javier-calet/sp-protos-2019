using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Events
{
    public class Event
    {
        const string AttrKeyType = "type";
        const string AttrKeyTimestamp = "ts";
        const string AttrKeyData = "data";
        const string AttrKeyNum = "num";
        public ErrorDelegate ResponseDelegate;
        public string Name;
        public long Timestamp;
        public AttrDic Data;
        public const int NoNum = -1;
        public int Num = NoNum;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Events.Event"/> class.
        /// </summary>
        /// <param name="name">Event name.</param>
        /// <param name="data">Data.</param>
        /// <param name="responseDelegate">Response delegate that will be called when the request is finished.</param>
        public Event(string name, AttrDic data, ErrorDelegate responseDelegate = null)
        {
            Name = name;
            Data = data;
            Timestamp = TimeUtils.Timestamp;
            ResponseDelegate = responseDelegate;
        }

        public void FromAttr(Attr data)
        {
            var datadic = data.AssertDic;
            Name = datadic.GetValue(AttrKeyType).ToString();
            Timestamp = datadic.GetValue(AttrKeyTimestamp).ToLong();
            Num = datadic.ContainsKey(AttrKeyNum) ? datadic.GetValue(AttrKeyNum).ToInt() : -1;
            Data = datadic.Get(AttrKeyData).AsDic;
        }

        public Attr ToAttr()
        {
            var dic = new AttrDic();
            dic.SetValue(AttrKeyType, Name);
            dic.SetValue(AttrKeyTimestamp, Timestamp);
            dic.Set(AttrKeyData, Data);
            if(Num != NoNum)
            {
                dic.SetValue(AttrKeyNum, Num);
            }
            return dic;
        }
    }
}

