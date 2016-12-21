using System;

namespace SocialPoint.TransparentBundles
{
    public abstract class RequestArgs
    {
        private RequestReport _requestReport;
        public Action<RequestReport> OnSuccessCallback;
        public Action<RequestReport> OnFailedCallback;

        public RequestArgs(Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback)
        {
            OnSuccessCallback = SuccessCallback;
            OnFailedCallback = FailedCallback;
        }

        public RequestReport GetRequestReport()
        {
            return _requestReport;
        }

        public void SetRequestReport(RequestReport report)
        {
            _requestReport = report;
        }

        public void UpdateReportAndCallback(ResponseResult responseResult)
        {
            if(_requestReport == null)
            {
                _requestReport = new RequestReport(false, false);
            }

            _requestReport.ResponseRes = responseResult;

            if(responseResult != null && responseResult.Success)
            {
                OnSuccessCallback(_requestReport);
            }
            else
            {
                OnFailedCallback(_requestReport);
            }
        }
    }
}
