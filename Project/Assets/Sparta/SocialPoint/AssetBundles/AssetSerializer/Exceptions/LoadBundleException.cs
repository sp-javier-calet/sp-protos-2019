using System;

namespace SocialPoint.AssetSerializer.Exceptions
{
    public sealed class LoadBundleException: Exception
    {
        private string message = "";

        public LoadBundleException(string msg)
        {
            message = msg;
        }

        override public string ToString()
        {
            return  "LoadBundleException - Error: " + message;
        }
    }
}

