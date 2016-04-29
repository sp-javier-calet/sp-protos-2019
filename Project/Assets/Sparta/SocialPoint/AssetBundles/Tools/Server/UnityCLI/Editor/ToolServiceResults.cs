namespace SocialPoint.Tool.Server
{
    public class ToolServiceResults
    {
        /**
         * Base properties
         */
        public string result;
        public string execution_date;
        public string error_message;

        public ToolServiceResults()
        {
            result = "OK";
        }

        /**
         * Serialize method
         */
        public string ToJson()
        {
            var handler = Utils.JsonMapperToJson();
            return handler(this);
        }

        public void MarkAsFailed(string message)
        {
            result = "ERROR";
            error_message = message;
        }
    }
}
