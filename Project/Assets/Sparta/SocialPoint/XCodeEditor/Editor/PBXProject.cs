using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.XCodeEditor
{
    public class PBXProject : PBXObject
    {
        protected string MAINGROUP_KEY = "mainGroup";
        protected string ATTRIBUTES_KEY = "attributes";
        protected string TARGET_ATTRIBUTES_KEY = "TargetAttributes";
        
        public PBXProject() : base()
        {
        }
        
        public PBXProject(string guid, PBXDictionary dictionary) : base( guid, dictionary )
        { 
        }

        public void AddTargetAttributes(PBXDictionary targetAttrs)
        {
            PBXDictionary pbxAttrs;
            if(!_data.ContainsKey(ATTRIBUTES_KEY))
            {
                pbxAttrs = new PBXDictionary();
                _data[ATTRIBUTES_KEY] = pbxAttrs;
            }
            else
            {
                pbxAttrs = (PBXDictionary)_data[ATTRIBUTES_KEY];
            }

            PBXDictionary pbxTargetAttrs;
            if(!pbxAttrs.ContainsKey(TARGET_ATTRIBUTES_KEY))
            {
                pbxTargetAttrs = new PBXDictionary();
                pbxAttrs[TARGET_ATTRIBUTES_KEY] = pbxTargetAttrs;
            }
            else
            {
                pbxTargetAttrs = (PBXDictionary)pbxAttrs[TARGET_ATTRIBUTES_KEY];
            }
            pbxTargetAttrs.Combine(targetAttrs);
        }

        public string mainGroupID
        {
            get
            {
                return (string)_data[MAINGROUP_KEY];
            }
        }
    }
}
