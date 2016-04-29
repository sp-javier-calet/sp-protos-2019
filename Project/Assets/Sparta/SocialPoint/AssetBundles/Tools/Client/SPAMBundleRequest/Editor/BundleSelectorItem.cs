using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;
using VersioningBundle = SocialPoint.Editor.SPAMGui.BRResponse.Data.TaggedProjectVersion.VersioningBundle;

namespace SocialPoint.Editor.SPAMGui
{
    public class BundleSelectorItem : TLTreeSelectorItem<BundleSelectorItem>
    {
        //static readonly GUILayoutOption[]   rowInnerLayoutOptions;
        static readonly GUILayoutOption[]   IncCheckboxOptions;
        static readonly GUILayoutOption[]   ReqCheckboxOptions;
        static readonly TLStyle             InnerItemLabelStyle;

        static readonly Texture2D           assetBundleImg;
        static readonly Texture2D           sceneBundleImg;
        static readonly Texture2D           assetBundleImgUnv;
        static readonly Texture2D           sceneBundleImgUnv;

        //Drawing elements
        TLWIcon                 _icoBundleName;
        TLWCheck _chkIncluded;
        TLWCheck _chkRequested;

        //backed versioning data
        VersioningBundle        _versioning;
        
        //Contents
        public bool     IsVersioned { get; private set; }

        public bool     IsScene { get; private set; }
        public int?     Prod { get; private set; }
        public int?     Last { get; private set; }

        public bool Requested
        {
            get
            {
                return _chkRequested.isChecked;
            }
            set
            {
                _chkRequested.SetCheck(value);
            }
        }

        public bool Included
        {
            get
            {
                return _chkIncluded.isChecked;
            }
        }

        /// <summary>
        /// Bundle request event, informing if this item has been marked or unmarked for request
        /// </summary>
        /// <value>The bundle requested.</value>
        public TLEvent<BundleSelectorItem, bool> bundleRequested { get; private set; }
        
        static BundleSelectorItem()
        {
            //rowInnerLayoutOptions = TLLayoutOptions.basic;

            IncCheckboxOptions = new GUILayoutOption[] { GUILayout.Width(40f) };
            ReqCheckboxOptions = new GUILayoutOption[] { GUILayout.Width(20f) };

            InnerItemLabelStyle = new TLStyle("Label");
            InnerItemLabelStyle.fontSize = 12;
            InnerItemLabelStyle.alignment = TextAnchor.MiddleCenter;

            assetBundleImg = BMGUIStyles.GetIcon("assetBundleIcon");
            sceneBundleImg = BMGUIStyles.GetIcon("sceneBundleIcon");
            assetBundleImgUnv = BMGUIStyles.GetIcon("assetBundleIconUnversioned");
            sceneBundleImgUnv = BMGUIStyles.GetIcon("sceneBundleIconUnversioned");
        }
        
        public BundleSelectorItem(string bundleName, BundleData data, bool isDevVersion) : base (bundleName)
        {
            Init();
            InitBundleData(data);
            IsVersioned = false;
        }

        public BundleSelectorItem(string bundleName, BundleData data, VersioningBundle versioning, bool isDevVersion) : base (bundleName)
        {
            Init();
            InitBundleData(data);
            InitVersioning(versioning, isDevVersion);
            IsVersioned = true;
        }

        void Init()
        {
            bundleRequested = new TLEvent<BundleSelectorItem, bool>("BundleRequested", this, false);
        }

        void InitBundleData(BundleData data)
        {
            IsScene = data.sceneBundle;
        }

        void InitVersioning(VersioningBundle versioning, bool isDevVersion)
        {
            _versioning = versioning;

            if(!isDevVersion)
            {
                Prod = versioning.prod;
                Last = versioning.last;
            }
        }

        protected override void InitDrawing()
        {
            base.InitDrawing();

            _chkRequested = new TLWCheck(_treeWidget.View, "_chkRequested", "");
            
            _chkRequested.onCheckEvent.ConnectWithArguments(bundleRequested, positionMap:EventArgsMapping.Map(0,1));

            Texture2D bundleImg;
            if(IsVersioned)
            {
                bundleImg = IsScene ? sceneBundleImg: assetBundleImg;
            }
            else
            {
                bundleImg = IsScene ? sceneBundleImgUnv: assetBundleImgUnv;
            }

            _icoBundleName = new TLWIcon(_treeWidget.View, "_icoBundleName", bundleImg, " " + Content);

            _chkIncluded = new TLWCheck(_treeWidget.View, "_chkIncluded", "", IncCheckboxOptions);
            _chkIncluded.SetCheck( IsVersioned ? _versioning.is_included : false );
        }

        public void DisableRequestWidget(bool disabled)
        {
            _chkRequested.SetDisabled(disabled);
        }

        public override bool IsSelected()
        {
            return _chkRequested.isChecked;
        }

        public override void Draw(ref BundleSelectorItem selectedItem)
        {
            GUILayout.BeginHorizontal(_tabsStyle.GetStyle(), OuterLayoutOptions);

            //[Expand button] and bundle name

            //Do not higlight selected item
            GUIStyle usedStyle = TLListSelectorStyles.InnerItemStyle.GetStyle();

            GUILayout.BeginHorizontal(usedStyle, TLLayoutOptions.basic);

            if(IsExpansible)
            {
                TLEditorUtils.BeginCenterVertical();

                _btnExpand.Draw();

                TLEditorUtils.EndCenterVertical();
            }
            //Put an empty space where the expand button should be
            else
            {
                _sp0.Draw();
            }

            //Requested
            GUILayout.BeginHorizontal(ReqCheckboxOptions);
            _chkRequested.Draw();
            GUILayout.EndHorizontal();

            //Bunlde icon and name
            _icoBundleName.Draw();
            
            GUILayout.EndHorizontal();

            //Included in build
            GUILayout.BeginHorizontal(IncCheckboxOptions);
            TLEditorUtils.BeginCenterHorizontal();
            _chkIncluded.Draw();
            TLEditorUtils.EndCenterHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
            
            if(IsExpanded)
            {
                foreach(BundleSelectorItem child in _childs)
                    child.Draw(ref selectedItem);
            }
        }
    }
}