
namespace SocialPoint.TransparentBundles
{
    public class RequestReport
    {
        public bool RequestCancelled;
        public ResponseResult ResponseRes;

        public RequestReport(bool requestCancelled = false)
        {
            RequestCancelled = requestCancelled;
        }

        public RequestReport(ResponseResult loginResult, bool requestCancelled = false)
        {
            RequestCancelled = requestCancelled;
            ResponseRes = loginResult;
        }
    }
}
