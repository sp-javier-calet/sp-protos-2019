namespace SocialPoint.Base
{
    public class Error
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
    }

    public delegate void ErrorDelegate(Error err);
}
