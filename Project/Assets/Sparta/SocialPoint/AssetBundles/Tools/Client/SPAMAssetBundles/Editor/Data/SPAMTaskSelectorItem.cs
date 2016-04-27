using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public class SPAMTaskSelectorItem : TLTreeSelectorItem<SPAMTaskSelectorItem> {

        static readonly TLStyle             rowInnerLayoutStyle;
        static readonly TLStyle             rowButtonsInnerLayoutStyle;

        static readonly GUILayoutOption[]   rowLayoutOpts;
        static readonly GUILayoutOption[]   bundleNameLayoutOpts;
        static readonly GUILayoutOption[]   taskDateLayoutOpts;
        static readonly GUILayoutOption[]   taskStatusLayoutOpts;

        static Dictionary<string, TLStyle>          _stylesDict;
        public static Dictionary<string, TLStyle>   stylesDict
        {
            get
            {
                if(_stylesDict == null) {
                    _stylesDict = InitStylesDict();
                }
                return _stylesDict;
            }
        }

        //Need for icons
        public TLView                       View { get; private set; }
        public SPAMTaskResult               Model { get; private set; }
        //Contents
        TLWLabel                            taskScene = null;
        TLWLabel                            taskDate = null;
        TLWIcon                             loadingIcon = null;
        TLWIcon                             successIcon = null;
        TLWIcon                             failIcon = null;
        TLWLabel                            bundleSize = null;

        static SPAMTaskSelectorItem()
        {
            rowInnerLayoutStyle = new TLStyle("Box");
            rowInnerLayoutStyle.clipping = TextClipping.Clip;
            rowInnerLayoutStyle.margin = new RectOffset ();

            rowButtonsInnerLayoutStyle = new TLStyle ("Box");
            rowButtonsInnerLayoutStyle.margin = new RectOffset ();

            rowLayoutOpts = new GUILayoutOption[] { GUILayout.MaxHeight(20f) };
            bundleNameLayoutOpts = new GUILayoutOption[] { GUILayout.Width(300f) };
            taskDateLayoutOpts = new GUILayoutOption[] { GUILayout.Width(100f) };
            taskStatusLayoutOpts = new GUILayoutOption[] {
                GUILayout.ExpandWidth (false),
                GUILayout.ExpandHeight (false),
                GUILayout.MaxWidth (32)
            };
        }

        public SPAMTaskSelectorItem(SPAMTaskResult model, TLView view) : base (model.Scene) {
            View = view;
            Model = model;
            taskScene = new TLWLabel (null, "", Model.Scene, stylesDict ["scene_label_stl"]);
            taskDate = new TLWLabel (null, "", String.Format("{0} {1}", Model.Date.ToShortDateString(), Model.Date.ToShortTimeString()));
            loadingIcon = new TLWIcon (View, "", SPAMResources.loadingAtlasSml, stylesDict ["right_ico_stl"], new GUILayoutOption[] { GUILayout.Width(40f) });
            successIcon = new TLWIcon (View, "", TLIcons.successImg, stylesDict ["right_ico_stl"], new GUILayoutOption[] { GUILayout.Width(40f) });
            failIcon    = new TLWIcon (View, "", TLIcons.failImg, stylesDict ["right_ico_stl"], new GUILayoutOption[] { GUILayout.Width(40f) });
            bundleSize = new TLWLabel (null, "", SizeToString(Model.Size), stylesDict ["size_label_stl"]);
        }

        public override void Draw (ref SPAMTaskSelectorItem selectedItem)
        {
            GUILayout.BeginHorizontal (_tabsStyle.GetStyle(), rowLayoutOpts);

            GUILayout.BeginHorizontal (rowInnerLayoutStyle.GetStyle(), bundleNameLayoutOpts);
            TLEditorUtils.BeginCenterVertical ();

            taskScene.Perform ();

            TLEditorUtils.EndCenterVertical ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal (rowButtonsInnerLayoutStyle.GetStyle(), taskDateLayoutOpts);
            TLEditorUtils.BeginCenterVertical ();
            
            taskDate.Perform ();
            
            TLEditorUtils.EndCenterVertical ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal (rowButtonsInnerLayoutStyle.GetStyle(), taskStatusLayoutOpts);
            TLEditorUtils.BeginCenterVertical ();

            switch(Model.State)
            {
            case SPAMTaskResult.TaskState.PENDING:
                loadingIcon.Perform ();
                break;
            case SPAMTaskResult.TaskState.SUCCESSFUL:
                successIcon.Perform ();
                break;
            case SPAMTaskResult.TaskState.FAILED:
                failIcon.Perform ();
                break;
            default:
                break;
            }

            TLEditorUtils.EndCenterVertical ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal (rowButtonsInnerLayoutStyle.GetStyle());
            TLEditorUtils.BeginCenterVertical ();

            bundleSize.Perform ();

            TLEditorUtils.EndCenterVertical ();
            GUILayout.EndHorizontal ();

            GUILayout.EndHorizontal ();
        }

        public override void Update (double elapsed)
        {
            base.Update(elapsed);

            if (View != null && Model.State == SPAMTaskResult.TaskState.PENDING)
                loadingIcon.Update (elapsed);
        }

        string SizeToString(long size)
        {
            if (size < 0)
            {
                return "-";
            }
            else
            {
                float sizeInMb = size * 0.001f * 0.001f;
                return sizeInMb.ToString("N3") + " MB";
            }
        }

        static Dictionary<string, TLStyle> InitStylesDict()
        {
            Dictionary<string, TLStyle> dict = new Dictionary<string, TLStyle> ();
            
            TLStyle right_ico_stl = new TLStyle("Label");
            right_ico_stl.alignment = TextAnchor.MiddleRight;
            dict.Add ("right_ico_stl", right_ico_stl);

            TLStyle box_frame_stl = new TLStyle("Box");
            dict.Add ("box_frame_stl", box_frame_stl);

            TLStyle scene_label_stl = new TLStyle("Label");
            scene_label_stl.wordWrap = true;
            dict.Add ("scene_label_stl", scene_label_stl);

            TLStyle size_label_stl = new TLStyle("Label");
            size_label_stl.alignment = TextAnchor.MiddleRight;
            dict.Add ("size_label_stl", size_label_stl);
            
            return dict;
        }
    }
}