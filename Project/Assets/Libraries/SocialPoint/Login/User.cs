using System;
using System.Collections.Generic;

using UnityEngine;

using SocialPoint.Attributes;
using SocialPoint.Utils;
using SocialPoint.IO;

namespace SocialPoint.Login
{
    public struct UserMapping
    {
        public string Id { get; set; }

        public string Provider { get; set; }

        public UserMapping(string id, string provider) : this()
        {
            Id = id;
            Provider = provider;
        }

        public override string ToString()
        {
            return string.Format("[UserMapping: Id={0}, Provider={1}]", Id, Provider);
        }
    }

    public class User
    {
        public UInt64 Id { get; set; }

        public Dictionary<string, string> Names { get; set; }

        public Dictionary<string, string> PhotoPaths { get; set; }

        public List<UserMapping> Links { get; set; }

        public AttrDic Info { get; set; }

        public string TempId { get; set; }

        public Texture2D PhotoTexture
        {
            get
            {
                if(_photoTexture == null)
                {
                    if(FileUtils.Exists(PhotoPath, IOTarget.File))
                    {
                        var data = FileUtils.ReadAllBytes(PhotoPath);
                        _photoTexture = new Texture2D(1, 1);
                        _photoTexture.LoadImage(data);
                    }
                }
                return _photoTexture;
            }
        }

        Texture2D _photoTexture;

        public User() : this(0)
        {
        }

        public User(UInt64 userId, List<UserMapping> links = null, AttrDic info = null)
        {
            Id = userId;
            TempId = RandomUtils.GenerateSecurityToken();
            Names = new Dictionary<string, string>();
            PhotoPaths = new Dictionary<string, string>();
            Links = links ?? new List<UserMapping>();
            Info = info ?? new AttrDic();
        }

        public User(User other)
        {
            Id = other.Id;
            Names = other.Names;
            PhotoPaths = other.PhotoPaths != null
                ? new Dictionary<string, string>(other.PhotoPaths)
                : new Dictionary<string, string>();
            Links = new List<UserMapping>(other.Links);
            Info = new AttrDic(other.Info);
            TempId = other.TempId;
        }

        [Obsolete("Use AppRequestRecipient property")]
        public UserMapping GetAppRequestRecipient()
        {
            
            return AppRequestRecipient;
        }

        public UserMapping AppRequestRecipient
        {
            get
            {
                if(AppInstalled)
                {
                    return new UserMapping(Id.ToString(), null);
                }
                
                if(Links.Count > 0)
                {
                    return Links[0];
                }
                
                return new UserMapping();
            }
        }

        public string GetExternalId(string provider)
        {
            var enumerator = Links.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var link = enumerator.Current;
                if(link.Provider == provider)
                {
                    return link.Id;
                }
            }
            return null;
        }

        public List<string> GetExternalIds(string provider)
        {
            var ids = new List<string>();
            var enumerator = Links.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var link = enumerator.Current;
                if(link.Provider == provider)
                {
                    ids.Add(link.Id);
                }
            }
            return ids;
        }

        public bool HasLink(string provider, string externalId = null)
        {
            var enumerator = Links.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var link = enumerator.Current;
                if(link.Provider == provider)
                {
                    if(externalId == null || link.Id == externalId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool AddLink(string externalId, string provider)
        {
            if(HasLink(provider, externalId))
            {
                return false;
            }
            Links.Add(new UserMapping(externalId, provider));
            return true;
        }

        public void AddName(string name, string provider = "")
        {
            Names[provider] = name;
        }

        public string GetName(string provider)
        {
            string name = null;
            Names.TryGetValue(provider, out name);
            return name;
        }

        [Obsolete("Use Name property")]
        public string GetName()
        {
            return Name + "";
        }

        public string Name
        {
            get
            {
                var itr = Names.GetEnumerator();
                while(itr.MoveNext())
                {
                    var data = itr.Current;
                    if(!string.IsNullOrEmpty(data.Value))
                    {
                        return data.Value;
                    }
                }
                return null;
            }
        }

        public void AddPhotoPath(string photoPath, string provider = "")
        {
            PhotoPaths[provider] = photoPath;
        }

        public string GetPhotoPath(string provider)
        {
            string path = null;
            PhotoPaths.TryGetValue(provider, out path);
            return path;
        }

        [Obsolete("Use PhotoPath property")]
        public string GetPhotoPath()
        {
            return PhotoPath;
        }

        public string PhotoPath
        {
            get
            {
                var itr = PhotoPaths.GetEnumerator();
                while(itr.MoveNext())
                {
                    var data = itr.Current;
                    if(!string.IsNullOrEmpty(data.Value))
                    {
                        return data.Value;
                    }
                }
                return null;
            }
        }

        [Obsolete("Use AppInstalled property")]
        public bool UsesApp()
        {
            return AppInstalled;
        }

        public bool AppInstalled
        {
            get
            {
                return Id != 0;
            }
        }

        public static bool operator ==(User lu, User ru)
        {
            if(System.Object.ReferenceEquals(lu, null))
            {
                if(System.Object.ReferenceEquals(ru, null))
                {
                    return true;
                }
                return false;
            }

            if(System.Object.ReferenceEquals(ru, null))
            {
                return false;
            }

            if(lu.AppInstalled && lu.Id == ru.Id)
            {
                return true;
            }

            var enumerator = lu.Links.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var link = enumerator.Current;
                if(ru.HasLink(link.Provider, link.Id))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool operator !=(User lu, User ru)
        {
            return !(lu == ru);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            User p = obj as User;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public override int GetHashCode()
        {
            return (int)Id;
        }

        public bool Combine(User other)
        {
            if(this != other)
            {
                Id = other.Id;
                TempId = other.TempId;
                Links = new List<UserMapping>(other.Links);
                Info = new AttrDic(other.Info);
                PhotoPaths = new Dictionary<string, string>(other.PhotoPaths);
                Names = new Dictionary<string, string>(other.Names);
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(
                "[User: Id={0}, Names={1}, PhotoPaths={2}, Links={3}, Info={4}, TempId={5}]",
                Id, Name, PhotoPath, Links, Info, TempId);
        }

    }
}