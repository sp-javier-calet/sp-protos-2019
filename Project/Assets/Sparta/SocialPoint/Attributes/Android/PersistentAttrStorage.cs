using System;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Attributes
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    public sealed class PersistentAttrStorage : IAttrStorage, IDisposable
    {
        public event Action<Exception> ExceptionThrown;

        readonly string _prefix;
        IAttrSerializer _serializer;
        IAttrParser _parser;
        AndroidJavaObject _persistentAttrStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentAttrStorage"/> class.
        /// This storage is shared between Unity Games.
        /// </summary>
        /// <param name="prefix">Custom Prefix. The plugin uses by default the package name</param>
        /// <param name="deviceUid">Crypto key. used to encrypt the data, use the device UID</param>
        public PersistentAttrStorage(string deviceUid, string prefix = "")
            : this(new JsonAttrParser(), new JsonAttrSerializer(), deviceUid, prefix)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentAttrStorage"/> class.
        /// This storage is shared between Unity Games.
        /// </summary>
        /// <param name = "parser"></param>
        /// <param name = "serializer"></param>
        /// <param name="prefix">Custom Prefix. The plugin uses by default the package name</param>
        /// <param name="deviceUid">Crypto key. used to encrypt the data, use the device UID</param>
        public PersistentAttrStorage(IAttrParser parser, IAttrSerializer serializer, string deviceUid, string prefix = "")
        {
            _parser = parser;
            _serializer = serializer;
            _prefix = prefix;
            _persistentAttrStorage = new AndroidJavaObject("es.socialpoint.unity.base.PersistentAttrStorage", AndroidContext.CurrentActivity, deviceUid);
        }

        [System.Diagnostics.Conditional("DEBUG_SPPERSISTENT")]
        void DebugLog(string msg)
        {
            Log.i(string.Format("AndroidPersistentAttrStorage {0}", msg));
        }

        #region IAttrStorage implementation

        public Attr Load(string key)
        {
            var attrString = _persistentAttrStorage.Call<string>("getAttrForKey", _prefix, key, String.Empty);
            return _parser.ParseString(attrString);
        }

        public void Save(string key, Attr attr)
        {
            _persistentAttrStorage.Call<bool>("setAttrForKey", _prefix, key, _serializer.SerializeString(attr));
        }

        public void Remove(string key)
        {
            _persistentAttrStorage.Call<bool>("removeAttrForKey", _prefix, key);
        }

        public bool Has(string key)
        {
            return _persistentAttrStorage.Call<bool>("contains", _prefix, key);
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            _persistentAttrStorage.Dispose();
        }

        #endregion
    }
    #endif
}
