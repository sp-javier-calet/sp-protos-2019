//#define USE_CACHED_FILE
using LitJson;
using UnityEngine;
using SocialPoint.Attributes;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using SocialPointEditor.Assets.PlatformEx;

namespace SocialPoint.Editor.SPAMGui
{
    /// <summary>
    /// BR response. This class represents the response from the SPAM server when asking the versioning 
    /// information and extra for a given unity project.
    /// - active client versions:
    ///     - location of the Unity project for the client version under svn root
    ///     [if not developing version]
    ///     - versioning information for that client version:
    ///         - list of included bundles:
    ///             - last bundle version done
    ///             - prod bundle version deployed
    /// </summary>
    public class BRResponse
    {
        static readonly string CACHED_FILE = (Application.dataPath + "/.spam_cache" + "/.tmp_spam_versioning").ToSysPath();

        public int      version_timestamp;
        public string 	description;
        public Data 	data;

        public bool     IsCached { get { return data != null; } }

        void Init()
        {
            this.data.Init();
        }

        public void Store()
        {
            //Ensure folder
            Directory.CreateDirectory(Path.GetDirectoryName(CACHED_FILE));

            using(var writer = new StreamWriter(CACHED_FILE))
            {
                var serialized = JsonMapper.ToJson(this);
                writer.Write(serialized);
            }
        }

        public static BRResponse FromJson(string jsonContent)
        {
            var brResponse = JsonMapper.ToObject<BRResponse>(jsonContent);
            //New timestamp for cached version
            brResponse.version_timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            brResponse.Init();

            return brResponse;
        }

        public static BRResponse FromAttr(Attr data)
        {
            var serializer = new LitJsonAttrSerializer();
            var writer = new JsonWriter();
            serializer.Serialize(data, writer);
            return FromJson(writer.ToString());
        }

        public static BRResponse Instance()
        {
            var brResponse = new BRResponse();
#if USE_CACHED_FILE
            if(File.Exists(CACHED_FILE))
            {
                using(var reader = new StreamReader(CACHED_FILE))
                {
                    brResponse = JsonMapper.ToObject<BRResponse>(reader);
                    brResponse.Init();
                }
            }
#endif
            return brResponse;
        }

        #region AccessMethods

        public string[] VersionNames
        {
            get
            {
                return data.tagged_project_versions.Keys.ToArray();
            }
        }
       
        #endregion

        public class Data
        {
            public string 									project_name;
            public Dictionary<string, TaggedProjectVersion> tagged_project_versions;

            public void Init()
            {
                foreach(var tagged_project_version in this.tagged_project_versions)
                {
                    tagged_project_version.Value.name = tagged_project_version.Key;
                    tagged_project_version.Value.Init();
                }
            }

            public class TaggedProjectVersion
            {
                public string 								name;
                public string 								project_path;
                public string                               source;
                public bool									is_develop = false;
                public Dictionary<string, VersioningBundle> versioning = null;

                public void Init()
                {
                    if(!is_develop)
                    {
                        string[] bundle_names = new string[this.versioning.Keys.Count];
                        this.versioning.Keys.CopyTo(bundle_names, 0);
                        for(int i = 0; i < bundle_names.Length; ++i)
                        {
                            this.versioning[bundle_names[i]].name = bundle_names[i];
                        }
                    }
                }

                public class VersioningBundle
                {
                    public string 	name;
                    public bool     is_included;
                    public int 		prod;
                    public int 		last;
                }
            }
        }
    }
}