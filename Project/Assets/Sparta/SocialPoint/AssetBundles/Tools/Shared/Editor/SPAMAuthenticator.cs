using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using LitJson;
using SocialPoint.Attributes;
using SocialPoint.Utils;
using System.Runtime.Serialization;

namespace SocialPoint.Tool.Shared
{
    public class SPAMAuthenticator
    {
        public class NotAuthenticatedException : Exception
        {
            public NotAuthenticatedException() : base("User is not authenticated properly")
            {
            }
        }

        public const string MULTIPART = "multipart/form-data";
        public const string OCTETSTREAM = "application/octet-stream";
        public const string ZIP = "application/zip";
        public const string FORM = "application/x-www-form-urlencoded";

        #if LOCAL_SERVER_ENABLED
        public const string SPAM_SERVICES_HOSTNAME = "localhost";
        // LOCAL
        public const string SPAM_SERVICES_ENDPOINT = "http://localhost:8080";
        // LOCAL
        public static bool PROD_SERVER = false;
        #elif REMOTE_DEV_SERVER_ENABLED
		public const string SPAM_SERVICES_HOSTNAME = "toolsdev.example.com"; // PROD
		public const string SPAM_SERVICES_ENDPOINT = "http://toolsdev.example.com:8080"; // DEV
        public static bool PROD_SERVER = false;
		
#else
		public const string SPAM_SERVICES_HOSTNAME = "toolsserver.socialpoint.es"; // PROD
		public const string SPAM_SERVICES_ENDPOINT = "http://toolsserver.socialpoint.es"; // PROD
        public static bool PROD_SERVER = true;
		#endif

        public delegate void LoginOK(string sessionId,string email);

        public delegate void LoginKO(string error);

        public const int SOCKET_LOCAL_PORT = 1239;
        public const string COOKIE_KEY = "cached_cookie_key";
        public const string CACHED_USER = "cached_user_email";

        System.Object _authLock;

        private static TcpListener listener;
        private static bool destroyed;
        private AsyncDataWorker dataWorker;
        private string session_cookie;
        private string cached_user;

        Thread perform_request = null;

        public SPAMAuthenticator(string _session_cookie = null)
        {
            session_cookie = _session_cookie ?? GetSessionCookie();
            cached_user = GetCachedUser();
            _authLock = new System.Object();
        }

        public static string GetLoginAuthenticationURI()
        {
            return SPAM_SERVICES_ENDPOINT + "/login/?next=/succesfulLogin/";
        }

        public static string GetCookieAuthenticationURI()
        {
            return SPAM_SERVICES_ENDPOINT + "/succesfulLogin/";
        }

        public static string GetLogoutURI()
        {
            return SPAM_SERVICES_ENDPOINT + "/logout/";
        }

        // Must be called outside a thread
        public static string GetSessionCookie()
        {
            if(!EditorPrefs.HasKey(COOKIE_KEY) || EditorPrefs.GetString(COOKIE_KEY) == null)
                return null;

            var _session_cookie = EditorPrefs.GetString(COOKIE_KEY);

            if(_session_cookie == String.Empty)
                return null;

            return _session_cookie;
        }

        // Must be called outside a thread
        public static string GetCachedUser()
        {
            if(!EditorPrefs.HasKey(CACHED_USER) || EditorPrefs.GetString(CACHED_USER) == null)
                return null;

            var _cached_user = EditorPrefs.GetString(CACHED_USER);
			
            if(_cached_user == String.Empty)
                return null;
			
            return _cached_user;
        }

        // Must be called outside a thread
        public void SaveLoginPrefs()
        {
            EditorPrefs.SetString(COOKIE_KEY, session_cookie);
            EditorPrefs.SetString(CACHED_USER, cached_user);
        }

        // Must be called outside a thread
        public void ResetLoginPrefs()
        {
            EditorPrefs.DeleteKey(COOKIE_KEY);
            EditorPrefs.DeleteKey(CACHED_USER);
            session_cookie = null;
            cached_user = null;
        }

        NameValueCollection GetCookieHeader()
        {
            if(session_cookie == null)
                return null;

            NameValueCollection headers = new NameValueCollection();
            headers.Add("Cookie", session_cookie);

            return headers;
        }

        void StartListening()
        {
            if(listener != null)
            {
                listener.Stop();
                GC.Collect();
            }
            listener = new TcpListener(IPAddress.Any, SOCKET_LOCAL_PORT);
            listener.Start();
            dataWorker = new AsyncDataWorker();
            dataWorker.RunAsync(Service);
        }

        // Best used with threading
        void PerformRequestToAuthUri(NameValueCollection header = null)
        {
            //Debug.Log ("Performing auth");
            HttpWebRequest login_request = (HttpWebRequest)WebRequest.Create(GetCookieAuthenticationURI());
            //login_request.Timeout = 10000; 
            if(header != null)
            {
                login_request.Headers.Add(header);
            }

            using(var response = login_request.GetResponse())
            {
            }
        }

        // Best used with threading
        public HttpWebResponse PerformAuthenticatedRequest(string url, NameValueCollection postData = null)
        {
            //Debug.Log ("Performing request: " + url);
            var cHeader = GetCookieHeader();
            if(cHeader == null)
                throw new NotAuthenticatedException();

            HttpWebRequest auth_request = (HttpWebRequest)WebRequest.Create(url);
            if(cHeader != null)
            {
                auth_request.Headers.Add(cHeader);
            }

            auth_request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            auth_request.AllowAutoRedirect = false;

            try
            {
                if(postData != null)
                {

                    StringBuilder sb = new StringBuilder();
                    foreach(string key in postData.Keys)
                    {

                        string[] arrayParam;
                        if(key.EndsWith("[]"))
                            arrayParam = postData[key].Split(',').Select(sValue => sValue.Trim()).ToArray();
                        else
                            arrayParam = new string[] { postData[key] };

                        foreach(string listParam in arrayParam)
                        {
                            sb.AppendFormat("{0}={1}&", key, WWW.EscapeURL(listParam));
                        }
                    }
                    sb.Remove(sb.Length - 1, 1); // remove the last '&'
                    var bytes = Encoding.UTF8.GetBytes(sb.ToString());

                    auth_request.Method = "POST";
                    auth_request.ContentType = FORM + ";charset=UTF-8";
                    auth_request.ContentLength = bytes.Length;

                    Stream webpageStream = auth_request.GetRequestStream();
                    webpageStream.Write(bytes, 0, bytes.Length);
                    webpageStream.Close();
                }

                return (HttpWebResponse)(auth_request.GetResponse()); 
            }
            catch(WebException ex)
            {
                Debug.LogError(ex);
            }
            return null;
        }

        public HttpWebResponse PerformAuthenticatedRequestForUploading(string url, string file, NameValueCollection postData = null, 
                                                                 Action<float> progressCb = null,
                                                                 string paramName = "file",
                                                                 string contentType = "application/zip")
        {
            var cHeader = GetCookieHeader();
            if(cHeader == null)
                throw new NotAuthenticatedException();

            HttpWebRequest auth_request = (HttpWebRequest)WebRequest.Create(url);
            if(cHeader != null)
            {
                auth_request.Headers.Add(cHeader);
            }

            auth_request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            auth_request.AllowAutoRedirect = false;

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            auth_request.Method = "POST";
            auth_request.ContentType = MULTIPART + "; boundary=" + boundary;
            auth_request.KeepAlive = true;
            auth_request.Credentials = CredentialCache.DefaultCredentials;

            Stream rs = auth_request.GetRequestStream();

            if(postData != null)
            {
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                foreach(string key in postData.Keys)
                {

                    string[] arrayParam;
                    if(key.EndsWith("[]"))
                        arrayParam = postData[key].Split(',').Select(sValue => sValue.Trim()).ToArray();
                    else
                        arrayParam = new string[] { postData[key] };

                    foreach(string listParam in arrayParam)
                    {
                        rs.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, listParam);
                        byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                        rs.Write(formitembytes, 0, formitembytes.Length);
                    }
                }
            }

            rs.Write(boundarybytes, 0, boundarybytes.Length);
			
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            using(FileStream f = File.OpenRead(file))
            {

                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                long accumBytes = 0;
                long totalBytes = f.Length;
                float progressCount = 0;
                bool bUsingProgressCallback = progressCb != null;
				
                while((bytesRead = f.Read(buffer, 0, buffer.Length)) != 0)
                {
                    accumBytes += bytesRead;
                    rs.Write(buffer, 0, bytesRead);
                    if(bUsingProgressCallback)
                    {
                        float newProgress = ((float)accumBytes / (float)totalBytes) * 100f;
                        if(newProgress != progressCount)
                        {
                            progressCount = newProgress;
                            progressCb(progressCount);
                        }
                    }
                }
            }

            byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            rs.Close();

            try
            {
                return (HttpWebResponse)(auth_request.GetResponse());
            }
            catch(WebException ex)
            {
                Debug.LogError(ex);
            }
            return null;
        }

        Attr TimeoutExceededResponse()
        {
            string message = String.Format("Timeout Reached: Could not connect to the host {0}", SPAM_SERVICES_ENDPOINT);
#if UNITY_EDITOR_WIN
            message += String.Format("\\nTry disabling the Windows Firewall or opening TCP port {0}", SOCKET_LOCAL_PORT);
#endif
            return new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes("{\"success\": \"false\", \"message\": \"" + message + "\", \"is_recoverable\": \"false\"}"));
        }

        Attr NotChachedResponse()
        {
            return new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes("{\"success\": \"false\", \"message\": \"Cookie not cached\", \"is_recoverable\": \"true\"}"));
        }

        Attr LoginFailedResponse(string message)
        {
            return new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{\"success\": \"false\", \"message\": \"Error during login: {0}\", \"is_recoverable\": \"false\"}", message)));
        }

        Attr HandleRequestResponse(int timeout)
        {
            int timeout_time = Environment.TickCount + timeout;
            while(IsRunning())
            {
                if(Environment.TickCount >= timeout_time)
                {
                    Destroy();
                    return TimeoutExceededResponse();
                }
            }
            var result = GetResult<object>();
            while(result == null)
            {
                result = GetResult<object>();
            }

            if(result is Exception)
            {
                Destroy();
                return LoginFailedResponse(((Exception)result).Message);
            }

            Attr attr_result = GetResult<Attr>();
            SessionResponse response = SessionResponse.FromAttr(attr_result);
            if(response.Success)
            {
                session_cookie = response.response.session_id;
                cached_user = response.response.email;
            }
            else
            {
                session_cookie = null;
                cached_user = null;
            }
            Destroy();

            return attr_result;
        }

        // This method must be protected with a mutex
        public Attr TryCachedAuthentication(int timeout)
        {
            lock(_authLock)
            {
                var header = GetCookieHeader();
                if(header == null)
                    return NotChachedResponse();

                StartListening();
                ThreadStart starter = delegate {
                    PerformRequestToAuthUri(header);
                };
                perform_request = new Thread(starter);

                perform_request.Start();
                var response = HandleRequestResponse(timeout);

                return response;
            }
        }

        // This method must be protected with a mutex
        public Attr WebAuthentication(int timeout)
        {
            lock(_authLock)
            {
                StartListening();
                var response = HandleRequestResponse(timeout);
			
                return response;
            }
        }

        public bool IsRunning()
        {
            if(dataWorker == null)
            {
                return false;
            }
            else
            {
                return dataWorker.IsRunning;
            }
        }

        public T GetResult<T>() where T : class
        {
            if(dataWorker == null)
            {
                return null;
            }
            else
            {
                return dataWorker.GetResult<T>();
            }
        }

        public void Destroy()
        {
            if(dataWorker != null)
            {
                dataWorker.Cancel();
                dataWorker.Destroy();
            }
            if(listener != null)
            {
                listener.Stop();
                GC.Collect();
            }
            if(perform_request != null && perform_request.IsAlive)
            {
                perform_request.Abort();
            }

            dataWorker = null;
            listener = null;
            perform_request = null;
        }

        static Attr Service()
        {
            AttrDic result = null;

            string responseId = null;
            string params_ = null;
            while(responseId == null || responseId.Equals(""))
            {
                Socket soc = listener.AcceptSocket();
                Stream s = new NetworkStream(soc); 
                StreamReader sr = new StreamReader(s);

                responseId = sr.ReadLine();
                params_ = sr.ReadLine();

                s.Close();
                sr.Close();
                soc.Close();
            }

            if(!responseId.Equals("CONFIRMED"))
            {
                result = new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{{\"success\": \"false\", \"message\": \"Login rejected by SPAM\", \"response_id\": \"{0}\", \"is_recoverable\": \"true\"}}", responseId))).AsDic;
            }
            else
            { //CONFIRMED
                result = new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{{\"success\": \"true\", \"response_id\": \"{0}\"}}", responseId))).AsDic;
                Attr jsonParams = new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes(params_));
                result["response"] = jsonParams;
            }

            listener.Stop();
            return result;
        }

        [PreferenceItem("SPAM")]
        public static void PreferencesGUI()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            List<string> defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(new char[] { ';' }));

            int checkedIdx = -1;
            if(defines.Contains("LOCAL_SERVER_ENABLED"))
            {
                checkedIdx = 0;
            }
            else if(defines.Contains("REMOTE_DEV_SERVER_ENABLED"))
            {
                checkedIdx = 1;
            }

            int newVal = ToggleGroupGUI(checkedIdx, "LOCAL_SERVER_ENABLED", "REMOTE_DEV_SERVER_ENABLED");
            if(newVal != checkedIdx)
            {
                switch(newVal)
                {
                case -1:
                    {
                        defines.Remove("LOCAL_SERVER_ENABLED");
                        defines.Remove("REMOTE_DEV_SERVER_ENABLED");
                        break;
                    }
                case 0:
                    {
                        defines.Add("LOCAL_SERVER_ENABLED");
                        defines.Remove("REMOTE_DEV_SERVER_ENABLED");
                        break;
                    }
                case 1:
                    {
                        defines.Remove("LOCAL_SERVER_ENABLED");
                        defines.Add("REMOTE_DEV_SERVER_ENABLED");
                        break;
                    }
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, String.Join(";", defines.ToArray()));
            }


            if(GUILayout.Button("Open config file"))
            {
                #if UNITY_EDITOR_OSX
                var filePath = "file://" + Path.Combine(Application.dataPath, "SpamConfig/spamConfigJson.json");
                #else
                var filePath = Path.Combine(Application.dataPath, "SpamConfig/spamConfigJson.json");
                #endif
                Application.OpenURL(filePath);
            }
        }

        static int ToggleGroupGUI(int optionChecked, params string[] options)
        {
            int newlyChecked = optionChecked; // meaninig no check at all
            bool unCheckAll = false;
            for(int i = 0; i < options.Length; ++i)
            {
                bool wasChecked = optionChecked == i;
                if(EditorGUILayout.Toggle(options[i], wasChecked & !unCheckAll))
                {
                    if(wasChecked != true)
                    {
                        newlyChecked = i;
                        unCheckAll = true;
                    }
                }
                else
                {
                    if(wasChecked && !unCheckAll)
                    {
                        newlyChecked = -1;
                    }
                }
            }

            return newlyChecked;
        }
    }
}
