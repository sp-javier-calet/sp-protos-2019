namespace SocialPoint.Utils
{
    public class Error
    {
        public string Msg { get; set; }

        public int Code { get; set; }

        public Error()
        {
            Code = 0;
            Msg = "";
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
            return Code + ": " + Msg;
        }

        public void SetData(int code = 0, string msg = null)
        {
            if(msg == null)
            {
                msg = string.Empty;
            }
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
                return Code != 0 || Msg.Length > 0;
            }
        }
    }

    public delegate void ErrorDelegate(Error err);
}
