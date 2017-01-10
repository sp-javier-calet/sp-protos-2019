namespace SocialPoint.Base
{
    public sealed class Error
    {
        public string Msg { get; set; }

        public string ClientMsg { get; set; }

        public string ClientLocalize { get; set; }

        public int Code { get; set; }

        public Error()
        {
            Code = 0;
            Msg = string.Empty;
        }

        public Error(int code) : this()
        {
            Code = code;
        }

        public Error(string msg) : this()
        {
            Msg = msg;
        }

        public Error(int code, string msg) : this(code)
        {
            Msg = msg;
        }
        
        public override string ToString()
        {
            return string.Format("{0}: {1}", Code, Msg);
        }

        public void SetData(int code = 0, string msg = null)
        {
            Code = code;
            Msg = msg;
        }

        public void Clear()
        {
            SetData();
        }

        public bool HasError
        {
            get
            {
                return Code != 0 || !string.IsNullOrEmpty(Msg);
            }
        }

        public static bool IsNullOrEmpty(Error err)
        {
            return err == null || !err.HasError;
        }

        const char Separator = ':';

        public static Error FromString(string str)
        {
            var err = new Error();
            if(!string.IsNullOrEmpty(str))
            {
                var i = str.IndexOf(Separator);
                if(i >= 0)
                {
                    int code = 0;
                    if(int.TryParse(str.Substring(0, i), out code))
                    {
                        err.Code = code;
                    }
                    err.Msg = str.Substring(i + 1);
                }
                else
                {
                    err.Msg = str;
                }
            }
            return err;
        }
    }

    public delegate void ErrorDelegate(Error err);
}