using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine.UI;

namespace SocialPoint.Locale
{
    public sealed class LocalizeAttribute : Attribute
    {
        public string Key;
        public string Default;

        public LocalizeAttribute(string key=null, string def=null)
        {
            Key = key;
            Default = def;
        }
    }

    public abstract class BaseLocalizeAttributeObserver<T> : BaseMemberAttributeObserver<T, LocalizeAttribute> where T : class
    {
        protected Localization _locale;
        
        protected BaseLocalizeAttributeObserver(Localization locale)
        {
            _locale = locale;
        }        
        
        protected string GetString(string str, LocalizeAttribute attr)
        {
            if(!string.IsNullOrEmpty(attr.Key))
            {
                str = attr.Key;
            }
            return _locale.Get(str, attr.Default);
        }
    }

    public sealed class StringLocalizeAttributeObserver : BaseLocalizeAttributeObserver<string>
    {
        override public object Clone()
        {
            return new StringLocalizeAttributeObserver(_locale);
        }

        public StringLocalizeAttributeObserver(Localization locale):base(locale)
        {
        }

        override protected string ApplyType(string str, LocalizeAttribute attr)
        {
            return GetString(str, attr);
        }
    }

    public sealed class UITextLocalizeAttributeObserver : BaseLocalizeAttributeObserver<Text>
    {
        override public object Clone()
        {
            return new UITextLocalizeAttributeObserver(_locale);
        }

        public UITextLocalizeAttributeObserver(Localization locale):base(locale)
        {
        }

        override protected Text ApplyType(Text text, LocalizeAttribute attr)
        {
            text.text = GetString(text.text, attr);
            return text;
        }
    }

    public sealed class LocalizeAttributeConfiguration : MemberAttributeConfiguration<LocalizeAttribute>
    {
        public LocalizeAttributeConfiguration(Localization locale, List<IMemberAttributeObserver<LocalizeAttribute>> prototypes=null):
        base(prototypes)
        {
            AddObserver(new StringLocalizeAttributeObserver(locale));
            AddObserver(new UITextLocalizeAttributeObserver(locale));
        }
    }

}
