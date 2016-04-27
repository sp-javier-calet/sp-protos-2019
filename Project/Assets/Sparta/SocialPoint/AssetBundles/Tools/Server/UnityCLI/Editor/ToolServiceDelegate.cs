using System.Collections.Generic;
using SocialPoint.Attributes;


namespace SocialPoint.Tool.Server
{
    public abstract class ToolServiceDelegate
    {
        protected ToolServiceResults logResults;
        public ToolServiceResults LogResults { get { return logResults; } }

        public abstract void perform(ToolServiceParameters parameters);
    }
}
