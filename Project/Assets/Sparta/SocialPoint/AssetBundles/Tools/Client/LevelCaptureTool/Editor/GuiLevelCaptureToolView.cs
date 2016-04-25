using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.LevelCaptureTool
{
    public class GuiLevelCaptureToolView : TLView
    {
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

        //
        public TLWTextField 			tfOutputPath	{ get; set; }
        public TLWSelectFolderButton	btSelectFolder { get; set; }

        public LevelSelectorWidget		twLevelSelector { get; set; }
        public TLWButton                btSelectFromBundles { get; set; }

		public TLWLabel					lbResolution { get; set; } 
		public TLWLabel					lbWidth { get; set; }
		public TLWNumberField			nfWidth { get; set; }
		public TLWLabel					lbHeight { get; set; }
		public TLWNumberField			nfHeight { get; set; }

        public TLWButton                btCapture { get; set; }
        //

        public GuiLevelCaptureToolView(TLWindow window, TLModel model): base ( window, model )
        {
            position = new Rect(100, 100, 575, 800);
            minSize = new Vector2(575, 600);

            TLWVeticalLayout master_layout = new TLWVeticalLayout(this, 
                                                                        "master_layout",
                                                                        stylesDict["layout_margins_stl"],
                                                                        TLLayoutOptions.expandall);

            tfOutputPath = new TLWTextField(this, "tfOutputPath");
            tfOutputPath.readOnly = true;
            btSelectFolder = new TLWSelectFolderButton(this, "btSelectFolder", "Output Images Folder", "Select Folder");

            TLWHorizontalLayout hlHlayout_n0 = new TLWHorizontalLayout(this, 
                                                                       "hlHlayout_n0",
                                                                       stylesDict["layout_top_margins_stl"],
                                                                       new GUILayoutOption[] { GUILayout.Height(20) });

            hlHlayout_n0.AddWidget(tfOutputPath);
            hlHlayout_n0.AddWidget(btSelectFolder);

            master_layout.AddWidget(hlHlayout_n0);

            twLevelSelector = new LevelSelectorWidget(this, "twLevelSelector", TLLayoutOptions.expandall);

            TLWVeticalLayout hlVlayout_n1 = new TLWVeticalLayout(this,
                                                                 "hlVlayout_n1",
                                                                 stylesDict["layout_top_margins_stl"],
                                                                 TLLayoutOptions.expandall);

            hlVlayout_n1.AddWidget(twLevelSelector);

            master_layout.AddWidget(hlVlayout_n1);

            //BundleManager features
            TryToInitBundleManager(master_layout);
            //

			lbResolution = new TLWLabel(this, "lbResolution", "Resolution");
			lbWidth = new TLWLabel(this, "lbWidth", "W");
			nfWidth = new TLWNumberField(this, "nfWidth", "800", 4, new GUILayoutOption[] { GUILayout.Width(75) });
			lbHeight = new TLWLabel(this, "lbHeight", "H");
			nfHeight = new TLWNumberField(this, "nfHeight", "600", 4, new GUILayoutOption[] { GUILayout.Width(75) });

			TLWHorizontalLayout hlHlayout_n2 = new TLWHorizontalLayout(this, 
			                                                           "hlHlayout_n2",
			                                                           new GUILayoutOption[] { GUILayout.Height(20) });

			hlHlayout_n2.AddWidget(lbWidth);
			hlHlayout_n2.AddWidget(nfWidth);
			hlHlayout_n2.AddWidget(lbHeight);
			hlHlayout_n2.AddWidget(nfHeight);
			hlHlayout_n2.AddWidget(new TLWSpacer(this, "sp_0", true));

			TLWVeticalLayout hlVlayout_n3 = new TLWVeticalLayout(this, 
			                                                     "hlVlayout_n3",
			                                                     stylesDict["layout_top_margins_stl"],
			                                                     TLLayoutOptions.expandall);

			hlVlayout_n3.AddWidget(lbResolution);
			hlVlayout_n3.AddWidget(hlHlayout_n2);

			master_layout.AddWidget(hlVlayout_n3);
            
            btCapture = new TLWButton(this, "btCapture", "CAPTURE!");

            master_layout.AddWidget(btCapture);

            AddWidget(master_layout);
        }

        /// <summary>
        /// If BundleManager is defined, try to add a button to select all bundlemanager scenes.
        /// </summary>
        void TryToInitBundleManager(TLWVeticalLayout layout)
        {
            if(Utils.IsBundleManagerLoaded())
            {
                btSelectFromBundles = new TLWButton(this, "btSelectFromBundles", "Select From Bundles");

                layout.AddWidget(btSelectFromBundles);
            }
        }
                                                       
        static Dictionary<string, TLStyle> InitStylesDict()
        {
            Dictionary<string, TLStyle> dict = new Dictionary<string, TLStyle>();
			
            TLStyle layout_margins_stl = new TLStyle();
            layout_margins_stl.margin = new RectOffset(10, 10, 10, 10);
            dict.Add("layout_margins_stl", layout_margins_stl);
			
            TLStyle layout_top_margins_stl = new TLStyle();
            layout_top_margins_stl.margin = new RectOffset(0, 0, 10, 10);
            dict.Add("layout_top_margins_stl", layout_top_margins_stl);
			
            return dict;
        }
    }
}