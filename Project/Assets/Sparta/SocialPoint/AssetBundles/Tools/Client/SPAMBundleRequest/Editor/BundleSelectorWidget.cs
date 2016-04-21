using System;
using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VersioningBundle = SocialPoint.Editor.SPAMGui.BRResponse.Data.TaggedProjectVersion.VersioningBundle;
using UnityEditor;

namespace SocialPoint.Editor.SPAMGui
{
    public static class BundleSelectorWidgetStyles
    {
        static public TLStyle ButtonNoMarginsStyle;
        static public GUILayoutOption[] LabelLayoutOptions;

        static BundleSelectorWidgetStyles()
        {
            ButtonNoMarginsStyle = new TLStyle("Button");
            ButtonNoMarginsStyle.margin = new RectOffset();

            LabelLayoutOptions = new GUILayoutOption[] { GUILayout.MinWidth(30f), GUILayout.ExpandWidth(false) };
        }
    }

    public class BundleSelectorWidget : TLWTreeFilteredSelector<BundleSelectorItem>
    {
        //subcontrols
        TLWButton                   _btnSelectFiltered;
        TLWButton                   _btnUselectFiltered;

        string                      _filterControlName;

        protected TLEvent<bool>     _onSelectedForRequest;
        protected TLEvent<bool>     _onSelectedForUnrequest;
        //protected TLEvent<string>   _onFilterTextChanged;

        public string FilterText    { get; protected set; }

        public List<BundleSelectorItem> Requested
        {
            get
            {
                return GetListItems().Where(x => x.Requested == true).ToList();
            }
        }

        int _RequestedCount = 0; // cached 

		
        public BundleSelectorWidget(TLView view, string name, GUILayoutOption[] options) : base ( view, name, options )
        {
        }

        protected override void Init( TLStyle style = null )
        {
            base.Init(style: style);

            _onSelectedForRequest = new TLEvent<bool> ("OnSelectedForRequest", true);
            _onSelectedForUnrequest = new TLEvent<bool> ("OnSelectedForUnrequest", false);

            _filterControlName = Name + "_filterWidget";
            FilterText = "";

            //subcontrols init
            _btnSelectFiltered = new TLWButton(View, Name + "_btnSelectFiltered", "Select", BundleSelectorWidgetStyles.ButtonNoMarginsStyle);
            _btnUselectFiltered = new TLWButton(View, Name + "_btnUselectFiltered", "Unselect", BundleSelectorWidgetStyles.ButtonNoMarginsStyle);

            //connect signals

            _btnSelectFiltered.onClickEvent.Connect(_onSelectedForRequest);
            _btnUselectFiltered.onClickEvent.Connect(_onSelectedForUnrequest);
            _onSelectedForRequest.Connect(SetFilteredItemsForRequest);
            _onSelectedForUnrequest.Connect(SetFilteredItemsForRequest);
        }
		
        public void SetListItems(Dictionary<string, BundleData> bundles, Dictionary<string, VersioningBundle> versionings, bool isDevProjectVersion, bool sorted=false)
        {
            var topBundles = bundles.Where(x => x.Value.parent == "");
            var items = new List<BundleSelectorItem> ();

            foreach(var bundlePair in topBundles)
            {
                string bname = bundlePair.Key;
                BundleData bdata = bundlePair.Value;

                VersioningBundle versioning;
                BundleSelectorItem item;
                if(versionings.TryGetValue(bname, out versioning))
                {
                    item = new BundleSelectorItem(bname, bdata, versioning, isDevProjectVersion);
                }
                else
                {
                    item = new BundleSelectorItem(bname, bdata, isDevProjectVersion);
                }
                item.SetTreeWidget(this);

                item.bundleRequested.Connect(OnBundleRequested);

                List<BundleSelectorItem> childs = new List<BundleSelectorItem> ();
                for(int i = 0; i < bdata.children.Count; ++i)
                {
                    var childname = bdata.children[i];
                    childs.Add(CreateChild(childname, bundles, versionings, isDevProjectVersion));
                }
                item.SetChilds(childs);

                items.Add(item);
            }

            base.SetListItems(items, sorted);
        }

        BundleSelectorItem CreateChild(string name, Dictionary<string, BundleData> bundles, Dictionary<string, VersioningBundle> versionings, bool isDevProjectVersion)
        {
            BundleData bdata = bundles[name];
            VersioningBundle versioning;
            BundleSelectorItem item;
            if(versionings.TryGetValue(name, out versioning))
            {
                item = new BundleSelectorItem(name, bdata, versioning, isDevProjectVersion);
            }
            else
            {
                item = new BundleSelectorItem(name, bdata, isDevProjectVersion);
            }
            item.SetTreeWidget(this);

            item.bundleRequested.Connect(OnBundleRequested);

            List<BundleSelectorItem> childs = new List<BundleSelectorItem> ();
            for(int i = 0; i < bdata.children.Count; ++i)
            {
                var childname = bdata.children[i];
                childs.Add(CreateChild(childname, bundles, versionings, isDevProjectVersion));
            }

            item.SetChilds(childs);
            return item;
        }

        /// <summary>
        /// When a bundle is checked for request, we also select all sub bundles to be requested and disable the check button(BundleManager logic)
        /// </summary>
        void OnBundleRequested(BundleSelectorItem bundle, bool requested)
        {
            foreach(var child in bundle.GetChilds())
            {
                child.Requested = requested;
                child.DisableRequestWidget(requested);
            }
            //Also expand if requested is true
            if(requested)
            {
                bundle.Expand(requested);
            }

            _RequestedCount += requested ? 1 : -1;
        }

        public override void DrawSearch()
        {
            //Base search field
            base.DrawSearch();

            //Filter and select field
            GUILayout.Space(12f);
            DrawFilter();
        }

        void DrawFilter()
        {
            GUI.SetNextControlName (_filterControlName);

            GUILayout.Label("Include selected");
            bool checkValue = EditorGUILayout.Toggle(includeSelected);

            if (checkValue != includeSelected)
            {
                includeSelected = checkValue;
                _onSearchTextChanged.Send(View.window,SearchText); 
            }


            GUILayout.FlexibleSpace();//.Space(6f);
            GUILayout.Label(String.Format("F({0})", _filteredItems.Length), BundleSelectorWidgetStyles.LabelLayoutOptions);

            _btnSelectFiltered.Draw();
            
            _btnUselectFiltered.Draw();
        }

        public override void Perform()
        {
            base.Perform();

            //draw after the table has been drawn

            GUILayout.BeginHorizontal ();


            if (_itemsArr != null && _filteredItems != null)
            {
                if (_itemsArr.Length != _filteredItems.Length && GUILayout.Button("Clear Filter"))
                {
                    SearchText = "";
                    _filteredItems = _itemsArr;
                    EditorGUI.FocusTextInControl(null);
                }
            }

            //Requested bundles label
            GUILayout.FlexibleSpace();
            GUILayout.Label(String.Format("Requested({0})", _RequestedCount));

            GUILayout.EndHorizontal ();
        }

  

        void SetFilteredItemsForRequest(bool value)
        {
            for (int i = 0; i < _filteredItems.Length; ++i)
            {
                _filteredItems[i].Requested = value;
            }
        }

        public void MarkRequestedItems(List<string> bundles)
        {
            //Select bundles and unselect all the rest
            foreach(var item in GetListItems())
            {
                item.Requested = bundles.Contains(item.Content);
            }

            SetFilteredContent(bundles);
        }
    }
}
