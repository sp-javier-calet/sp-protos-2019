
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.IO;
using UnityEngine;

namespace SocialPoint.Attributes
{
    public class PersistentAttrStorage : IAttrStorage
    {
        public event Action<Exception> ExceptionThrown;

        readonly string _prefix;
        readonly string _password;
        readonly string _storageFilePath;
        const string _storageFileName = ".spstorageUnity";
        const string _passwordDefault = "ea37jm4nl2l0at15";
        AttrDic _storage;
        RijndaelManaged _cryptoAlg;
        IAttrSerializer _serializer;
        IAttrParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentAttrStorage"/> class.
        /// This storage is shared between Unity Games.
        /// </summary>
        /// <param name="prefix">Prefix. Used to avoid same keys between games. Usually app ID</param>
        /// <param name="cryptoKey">Crypto key. Usually the device UID</param>
        public PersistentAttrStorage(string cryptoKey, string prefix = "")
            : this(new JsonAttrParser(), new JsonAttrSerializer(), cryptoKey, prefix)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentAttrStorage"/> class.
        /// This storage is shared between Unity Games.
        /// </summary>
        /// <param name = "parser"></param>
        /// <param name = "serializer"></param>
        /// <param name="prefix">Prefix. Used to avoid same keys between games. Usually app ID</param>
        /// <param name="cryptoKey">Crypto key. Usually the device UID</param>
        public PersistentAttrStorage(IAttrParser parser, IAttrSerializer serializer, string cryptoKey, string prefix = "")
        {
            _prefix = prefix;
            _password = (cryptoKey + _passwordDefault).Substring(0, 16);
            _storageFilePath = PersistentPath + "/" + _storageFileName;

            DebugLog(string.Format("AndroidPersistentAttrStorage {0}", _storageFilePath));

            _cryptoAlg = new RijndaelManaged();
            _cryptoAlg.Mode = CipherMode.ECB;
            _cryptoAlg.Key = Encoding.UTF8.GetBytes(_password);

            _parser = parser;
            _serializer = serializer;

            byte[] data = LoadData();
            var dataString = Encoding.UTF8.GetString(data);
            dataString = dataString.TrimEnd('\0');//trim de padding added by the encryptor
            _storage = _parser.ParseString(dataString).AsDic;

            ExceptionThrown += (Exception obj) => {
                DebugUtils.Log(obj.Message);
            };
        }

        [System.Diagnostics.Conditional("DEBUG_SPPERSISTENT")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("AndroidPersistentAttrStorage {0}", msg));
        }

        string PersistentPath
        {
            get
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var env = new AndroidJavaClass("android.os.Environment");
                return env.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<string>("getAbsolutePath");
                #else
                return PathsManager.PersistentDataPath;
                #endif
            }
        }

        byte[] LoadData()
        {
            var data = new byte[0];
            if(!FileUtils.Exists(_storageFilePath))
            {
                try
                {
                    FileUtils.CreateFile(_storageFilePath);
                    data = FileUtils.ReadAllBytes(_storageFilePath);
                }
                catch(Exception ex)
                {
                    DebugLog(ex.Message);
                    if(ExceptionThrown != null)
                    {
                        ExceptionThrown(ex);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                data = Decrypt(FileUtils.ReadAllBytes(_storageFilePath));//FileUtils.ReadAllBytes(_storagePath);//
            }
            return data;
        }

        void StoreData()
        {
            byte[] encryptedData = Encrypt(_serializer.Serialize(_storage));//Serializer.Serialize(_storage);//
            try
            {
                FileUtils.WriteAllBytes(_storageFilePath, encryptedData);
            }
            catch(Exception ex)
            {
                DebugLog(ex.Message);
                if(ExceptionThrown != null)
                {
                    ExceptionThrown(ex);
                }
                else
                {
                    throw ex;
                }
            }
        }

        byte[] Encrypt(byte[] data)
        {
            using(var stream = new MemoryStream())
            {
                using(var cs = new CryptoStream(stream, _cryptoAlg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return stream.ToArray();
                }
            }
        }

        byte[] Decrypt(byte[] encryptedData)
        {
            byte[] finalBytes = new byte[encryptedData.Length];
            using(var stream = new MemoryStream(encryptedData))
            {
                using(var cs = new CryptoStream(stream, _cryptoAlg.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(finalBytes, 0, encryptedData.Length);
                }
            }
            return finalBytes;
        }

        #region IAttrStorage implementation

        public Attr Load(string key)
        {
            return _storage[_prefix + key];
        }

        public void Save(string key, Attr attr)
        {
            _storage.Set(_prefix + key, attr);
            StoreData();
        }

        public void Remove(string key)
        {
            _storage.Remove(key);
            StoreData();
        }

        public bool Has(string key)
        {
            return _storage.ContainsKey(key);
        }

        #endregion
    }
}
