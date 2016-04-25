using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public class BRCompilationResultSelector : TLWTreeSelector<BRCompilationResultSelectorItem>
    {
            
        public BRCompilationResultSelector(TLView view, string name, GUILayoutOption[] options) : base ( view, name, options )
        {
        }
    }

}
