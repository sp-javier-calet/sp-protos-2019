using SocialPoint.Base;

namespace SocialPoint.Attributes
{

    public static class AttrUtils
    {
        private const string ErrorKey = "error";
        private const string ErrorCodeKey = "code";
        private const string ErrorMessageKey = "message";
        private const string ErrorClientMessageKey = "client_message";
        private const string ErrorClientLocalizeKey = "client_localize";

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
            if(errdic.ContainsKey(ErrorClientMessageKey))
            {
                err.ClientMsg = errdic.GetValue(ErrorClientMessageKey).ToString();
            }
            if(errdic.ContainsKey(ErrorClientLocalizeKey))
            {
                err.ClientLocalize = errdic.GetValue(ErrorClientLocalizeKey).ToString();
            }
            return err;
        }
    }
}