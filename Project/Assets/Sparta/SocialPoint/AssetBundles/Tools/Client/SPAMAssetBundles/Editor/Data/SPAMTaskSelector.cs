﻿using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class SPAMTaskSelector : TLWTreeSelector<SPAMTaskSelectorItem> {

        public SPAMTaskSelector( TLView view, string name, GUILayoutOption[] options) : base ( view, name, options ) {}
    }
}