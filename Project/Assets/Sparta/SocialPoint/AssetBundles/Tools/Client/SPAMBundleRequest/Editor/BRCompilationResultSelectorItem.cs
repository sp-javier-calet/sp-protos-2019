using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public class BRCompilationResultSelectorItem : TLTreeSelectorItem<BRCompilationResultSelectorItem>
    {
        static readonly TLStyle             rowInnerLayoutStyle;
        static readonly TLStyle             rowButtonsInnerLayoutStyle;
        
        static readonly GUILayoutOption[]   rowLayoutOpts;
        static readonly GUILayoutOption[]   compilationDateLayoutOpts;
        static readonly GUILayoutOption[] authorLayoutOpts;
        static readonly GUILayoutOption[]   compilationStatusLayoutOpts;
        static readonly GUILayoutOption[]   iconOpts;
        
        static Dictionary<string, TLStyle>          _stylesDict;
        public static Dictionary<string, TLStyle>   stylesDict
        {
            get
            {
                if(_stylesDict == null)
                {
                    _stylesDict = InitStylesDict();
                }
                return _stylesDict;
            }
        }
        
        //Need for icons
        public TLView                       View { get; private set; }
        public BRCompilationResult          Model { get; private set; }
        //Contents
        TLWButton                           compilationId = null;
        TLWLabel                            author = null;
        TLWLabel                            compilationDate = null;
        TLWIcon                             loadingIcon = null;
        TLWIcon                             successIcon = null;
        TLWIcon                             failIcon = null;
        TLWIcon                             warnIcon = null;

        public TLEvent<List<string>>        CompilationResultClickedEvent { get; private set; }
        
        static BRCompilationResultSelectorItem()
        {
            rowInnerLayoutStyle = new TLStyle("Box");
            rowInnerLayoutStyle.clipping = TextClipping.Clip;
            rowInnerLayoutStyle.margin = new RectOffset();
            
            rowButtonsInnerLayoutStyle = new TLStyle("Box");
            rowButtonsInnerLayoutStyle.margin = new RectOffset();
            
            rowLayoutOpts = new GUILayoutOption[] { GUILayout.MaxHeight(20f) };
            authorLayoutOpts = new GUILayoutOption[] { GUILayout.Width(140f) };
            compilationDateLayoutOpts = new GUILayoutOption[] { GUILayout.MinWidth(140f) };
            compilationStatusLayoutOpts = new GUILayoutOption[] {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
                GUILayout.MaxWidth(32)
            };
            iconOpts = new GUILayoutOption[] { GUILayout.Width(40f), GUILayout.MinHeight(20f) };
        }
        
        public BRCompilationResultSelectorItem(BRCompilationResult model, TLView view) : base (model.Id.ToString())
        {
            View = view;
            Model = model;
            compilationId = new TLWButton(View, "", Model.Id.ToString(), stylesDict["id_label_stl"], TLLayoutOptions.expandall);
            compilationDate = new TLWLabel(View, "", String.Format("{0} {1}", Model.Date.ToShortDateString(), Model.Date.ToShortTimeString()), stylesDict["date_lbl_stl"], new GUILayoutOption[] { GUILayout.MinHeight(20f) });
            author = new TLWLabel(View, "", Model.author, stylesDict["date_lbl_stl"], new GUILayoutOption[] { GUILayout.MinHeight(20f) });
            loadingIcon = new TLWIcon(View, "", SPAMResources.loadingAtlasSml, stylesDict["right_ico_stl"], iconOpts);
            successIcon = new TLWIcon(View, "", TLIcons.successImg, stylesDict["right_ico_stl"], iconOpts);
            failIcon = new TLWIcon(View, "", TLIcons.failImg, stylesDict["right_ico_stl"], iconOpts);
            warnIcon = new TLWIcon(View, "", TLIcons.warningImg, stylesDict["right_ico_stl"], iconOpts);

            CompilationResultClickedEvent = new TLEvent<List<string>>("CompilationResultClickedEvent");
            CompilationResultClickedEvent.SetValue(model.Bundles);
            compilationId.onClickEvent.Connect(CompilationResultClickedEvent);
        }
        
        public override void Draw(ref BRCompilationResultSelectorItem selectedItem)
        {
            GUILayout.BeginHorizontal(_tabsStyle.GetStyle(), rowLayoutOpts);
            
            //GUILayout.BeginHorizontal(rowInnerLayoutStyle.GetStyle(), TLLayoutOptions.basic);
            //TLEditorUtils.BeginCenterVertical();
            
            compilationId.Perform();

            //TLEditorUtils.EndCenterVertical();
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(rowButtonsInnerLayoutStyle.GetStyle(), authorLayoutOpts);
            TLEditorUtils.BeginCenterVertical();

            author.Perform();

            TLEditorUtils.EndCenterVertical();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal(rowButtonsInnerLayoutStyle.GetStyle(), compilationDateLayoutOpts);
            TLEditorUtils.BeginCenterVertical();
            
            compilationDate.Perform();
            
            TLEditorUtils.EndCenterVertical();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal(rowButtonsInnerLayoutStyle.GetStyle(), compilationStatusLayoutOpts);
            TLEditorUtils.BeginCenterVertical();
            
            switch(Model.State)
            {
            case BRCompilationResult.CompilationState.PENDING:
                loadingIcon.Perform();
                break;
            case BRCompilationResult.CompilationState.SUCCESSFUL:
                successIcon.Perform();
                break;
            case BRCompilationResult.CompilationState.FAILED:
                failIcon.Perform();
                break;
            case BRCompilationResult.CompilationState.WARNING:
                warnIcon.Perform();
                break;
            default:
                break;
            }
            
            TLEditorUtils.EndCenterVertical();
            GUILayout.EndHorizontal();
            
            GUILayout.EndHorizontal();
        }
        
        public override void Update(double elapsed)
        {
            base.Update(elapsed);
            
            if(View != null && Model.State == BRCompilationResult.CompilationState.PENDING)
            {
                loadingIcon.Update(elapsed);
            }
        }
        
        static Dictionary<string, TLStyle> InitStylesDict()
        {
            Dictionary<string, TLStyle> dict = new Dictionary<string, TLStyle>();

            TLStyle right_ico_stl = new TLStyle("Label");
            right_ico_stl.alignment = TextAnchor.MiddleRight;
            dict.Add("right_ico_stl", right_ico_stl);


            TLStyle date_lbl_stl = new TLStyle("Label");
            date_lbl_stl.alignment = TextAnchor.MiddleCenter;
            dict.Add("date_lbl_stl", date_lbl_stl);

            TLStyle box_frame_stl = new TLStyle("Box");
            dict.Add("box_frame_stl", box_frame_stl);
            
            TLStyle id_label_stl = new TLStyle(rowInnerLayoutStyle);
            id_label_stl.wordWrap = true;
            id_label_stl.alignment = TextAnchor.MiddleLeft;
            id_label_stl.fontSize = right_ico_stl.fontSize;
            id_label_stl.fontStyle = right_ico_stl.fontStyle;
            id_label_stl.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            dict.Add("id_label_stl", id_label_stl);
            
            return dict;
        }
    }
}

