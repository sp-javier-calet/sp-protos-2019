using UnityEngine;

namespace SocialPoint.Utils
{
    public static class EmailUtils
    {
        static public void SendEmail(string mailto, string subject, string body)
        {
            Application.OpenURL("mailto:" + mailto + "?subject=" + EscapeUrl(subject) + "&body=" + EscapeUrl(body));
        }

        static public string EscapeUrl(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }
    }
}

