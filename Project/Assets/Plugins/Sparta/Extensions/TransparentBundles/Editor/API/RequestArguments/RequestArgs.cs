using System;

namespace SocialPoint.TransparentBundles
{
    public abstract class RequestArgs
    {
        RequestReport _requestReport;
        public bool AutoRetryLogin = true;
        public Action<RequestReport> OnSuccessCallback;
        public Action<RequestReport> OnFailedCallback;

        protected RequestArgs(Action<RequestReport> SuccessCallback, Action<RequestReport> FailedCallback, bool autoRetryLogin = true)
        {
            OnSuccessCallback = SuccessCallback;
            OnFailedCallback = FailedCallback;
            AutoRetryLogin = autoRetryLogin;
        }

        /// <summary>
        /// Gets the result of the petition
        /// </summary>
        /// <returns>RequestReport of the related petition</returns>
        public RequestReport GetRequestReport()
        {
            return _requestReport;
        }

        /// <summary>
        /// Sets the result of the petition (This should only be made from the API)
        /// </summary>
        /// <param name="report">RequestReport of the related petition</param>
        public void SetRequestReport(RequestReport report)
        {
            _requestReport = report;
        }

        /// <summary>
        /// Updates the report with a response from the server and raises the apropriate success or failure callback
        /// </summary>
        /// <param name="responseResult">Response of the AsyncRequest</param>
        public void UpdateReportAndCallback(ResponseResult responseResult)
        {
            if(_requestReport == null)
            {
                _requestReport = new RequestReport();
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
