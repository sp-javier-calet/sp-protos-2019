using SocialPoint.Base;

namespace SocialPoint.Attributes
{

    public static class AttrUtils
    {
        private const string ErrorKey = "error";
        private const string ErrorCodeKey = "code";
        private const string ErrorMessageKey = "message";

        public static Error GetError(Attr data)
        {
            if(data == null)
            {
                return null;
            }
            var dic = data.AsDic;
            if(!dic.ContainsKey(ErrorKey))
            {
                return null;
            }
            var errdic = dic.Get(ErrorKey).AsDic;
            var err = new Error();
            if(errdic.ContainsKey(ErrorMessageKey))
            {
                err.Msg = errdic.GetValue(ErrorMessageKey).ToString();
            }
            if(errdic.ContainsKey(ErrorCodeKey))
            {
                err.Code = errdic.GetValue(ErrorCodeKey).ToInt();
            }
            return err;
        }
    }
}