using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.XCodeEditor
{
    public class PBXList : ArrayList
    {
        public PBXList()
        {
            
        }
        
        public PBXList(object firstValue)
        {
            this.Add(firstValue);
        }
    }
}
