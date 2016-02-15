using System;
using System.Text;
using System.Runtime.Serialization;
using SocialPoint.Base;
using LitJson;

namespace SocialPoint.Attributes
{
    public class FastJsonAttrParser : StreamReaderAttrParser
    {
        protected override IStreamReader CreateStreamReader(string data)
        {
            return new FastJsonStreamReader(data);
        }
    }
}
