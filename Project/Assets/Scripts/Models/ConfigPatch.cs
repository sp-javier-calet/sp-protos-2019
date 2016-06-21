using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

public class ConfigPatch : IDisposable
{
    public AttrList Patch {get; private set;}

    public ConfigPatch(AttrList patch = null)
    {
        if(patch == null)
        {
            patch = new AttrList();
        }
        Patch = patch;
    }

    public override string ToString()
    {
        return string.Format("[ConfigPatch: Patches={0}]", Patch);
    }

    #region IDisposable implementation

    public void Dispose()
    {
        
    }

    #endregion
}

