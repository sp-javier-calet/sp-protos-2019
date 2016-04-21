using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public class SPAMBundleSelectorItem : TLTreeSelectorItem<SPAMBundleSelectorItem> {
        
        static readonly GUILayoutOption[]   rowInnerLayoutOptions;
        static readonly TLStyle             version_stl;
        
        //Need for icons
        public TLView                       View { get; private set; }
        public BundleMetaData               Model { get; private set; }
        //Contents
        TLWCheck                            bundleCb = null;
        TLWLabel                            bundleVersionLb = null;
        TLWCheck                            includedCb = null;
        TLWButton                           removeBt = null;
        TLWSpacer                           spacer_0 = null;
        TLWSpacer                           spacer_1 = null;

        public bool                         IsChecked { get { return bundleCb.isChecked; } }
        public bool                         IsIncluded { get { return includedCb.isChecked; } }

        TLEvent<SPAMBundleSelectorItem>                             _itemChanged;
        public TLEvent<SPAMBundleSelectorItem>                      itemChanged { get { return _itemChanged; } }

        TLEvent<SPAMBundleSelectorItem>                             _itemRemoved;
        public TLEvent<SPAMBundleSelectorItem>                      itemRemoved { get { return _itemRemoved; } }

        //TLEvent                             _itemRemoved;
        //public TLEvent                      itemRemoved { get { return _itemRemoved; } }
        
        static SPAMBundleSelectorItem()
        {
            rowInnerLayoutOptions = TLLayoutOptions.basic;
            version_stl = new TLStyle("Label");
            version_stl.alignment = TextAnchor.MiddleRight;
        }
        
        public SPAMBundleSelectorItem(BundleMetaData model, TLView view) : base (model.name) {
            View = view;
            Model = model;
            bundleCb = new TLWCheck (view, "cb0_" + model.name, model.name, new GUILayoutOption[] { GUILayout.Width(370) });
            bundleCb.SetCheck (model.isChecked);

            bundleVersionLb = new TLWLabel (view, "lb_" + model.name, model.version.ToString(), version_stl);

            spacer_0 = new TLWSpacer (view, "sp0_" + model.name, 40);

            includedCb = new TLWCheck (view, "cb1_" + model.name, "", new GUILayoutOption[] { GUILayout.Width(50) });
            includedCb.SetCheck (model.isBundled);
            OnIncludedChange();

            spacer_1 = new TLWSpacer (view, "sp1_" + model.name, true);

            removeBt = new TLWButton (view, "bt_" + model.name, TLIcons.contractImg, 20, 20);

            _itemChanged = new TLEvent<SPAMBundleSelectorItem> ("itemChanged", this);
            _itemRemoved = new TLEvent<SPAMBundleSelectorItem> ("itemRemoved", this);
            bundleCb.onCheckEvent.Connect(OnItemChanged);
            includedCb.onCheckEvent.Connect(OnItemChanged);
            includedCb.onCheckEvent.Connect(OnIncludedChange);

            removeBt.onClickEvent.Connect(itemRemoved);
        }

        public void SetCheck (bool value)
        {
            bundleCb.SetCheck (value);
        }
        
        public override void Draw (ref SPAMBundleSelectorItem selectedItem)
        {
            GUILayout.BeginHorizontal (_tabsStyle.GetStyle(), new GUILayoutOption[] { GUILayout.MaxHeight(20f) });
            
            GUILayout.BeginHorizontal (rowInnerLayoutOptions);

            //Checkbox + name
            TLEditorUtils.BeginCenterVertical ();
            
            bundleCb.Perform ();
            
            TLEditorUtils.EndCenterVertical ();

            //Version
            TLEditorUtils.BeginCenterVertical ();

            bundleVersionLb.Perform ();

            TLEditorUtils.EndCenterVertical ();
            //Spacer
            spacer_0.Perform ();

            //Included in game checkbox
            TLEditorUtils.BeginCenterVertical ();
            
            includedCb.Perform ();
            
            TLEditorUtils.EndCenterVertical ();
            //Spacer
            spacer_1.Perform ();

            //Remove Button
            TLEditorUtils.BeginCenterVertical ();

            removeBt.Perform ();

            TLEditorUtils.EndCenterVertical ();

            GUILayout.EndHorizontal ();
            GUILayout.EndHorizontal ();
        }

        void OnItemChanged()
        {
            Model.isChecked = IsChecked; //pick up the value from the checkbox widget
            Model.isBundled = IsIncluded;
            itemChanged.Send(View.window, this);
        }

        void OnIncludedChange()
        {
            includedCb.text = IsIncluded ? "Incl." : "";
        }

        public void OnVersionChanged()
        {
            bundleVersionLb.text = Model.version.ToString();
        }
    }
}