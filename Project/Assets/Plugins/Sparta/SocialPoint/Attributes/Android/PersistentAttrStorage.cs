using System;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Attributes
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    public sealed class PersistentAttrStorage : IAttrStorage, IDisposable
    {
        public event Action<Exception> ExceptionThrown;

        void OnExceptionThrown(Exception ex)
        {
            var handler = ExceptionThrown;
            if(handler != null)
            {
                handler(ex);
            }
        }

        readonly string _groupPrefix;
        readonly string _customPrefix;
        IAttrSerializer _serializer;
        IAttrParser _parser;
        AndroidJavaObject _persistentAttrStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentAttrStorage"/> class.
        /// This storage is shared between Unity Games.
        /// </summary>
        /// <param name="prefix">Custom Prefix. The plugin uses by default the package name</param>
        /// <param name="deviceUid">Crypto key. used to encrypt the data, use the device UID</param>
        public PersistentAttrStorage(string deviceUid, string customPrefix = "")
            : this(new JsonAttrParser(), new JsonAttrSerializer(), deviceUid, "", customPrefix)
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
        public PersistentAttrStorage(IAttrParser parser, IAttrSerializer serializer, string deviceUid, string groupPrefix = "", string customPrefix = "")
        {
            _parser = parser;
            _serializer = serializer;
            _groupPrefix = groupPrefix;
            _customPrefix = customPrefix;
            _persistentAttrStorage = new AndroidJavaObject("es.socialpoint.unity.base.PersistentAttrStorage", AndroidContext.CurrentActivity, deviceUid);
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugPersistentAttrFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("AndroidPersistentAttrStorage {0}", msg));
        }

        #region IAttrStorage implementation

        public Attr Load(string key)
        {
            var attrString = _persistentAttrStorage.Call<string>("getAttrForKey", _groupPrefix, _customPrefix, key, String.Empty);
            return _parser.ParseString(attrString);
        }

        public void Save(string key, Attr attr)
        {
            _persistentAttrStorage.Call<bool>("setAttrForKey", _groupPrefix, _customPrefix, key, _serializer.SerializeString(attr));
        }

        public void Remove(string key)
        {
            _persistentAttrStorage.Call<bool>("removeAttrForKey", _groupPrefix, _customPrefix, key);
        }

        public bool Has(string key)
        {
            return _persistentAttrStorage.Call<bool>("contains", _groupPrefix, _customPrefix, key);
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
