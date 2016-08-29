using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SocialPoint.Tool.Shared;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class BRView : TLView 
    {
        public BRModel      Model           { get { return (BRModel)_model; } }
        public BRController Controller      { get { return (BRController)_controller; } }

        static Dictionary<string, TLStyle>          _stylesDict;
        public static Dictionary<string, TLStyle>   stylesDict
        {
            get
            {
                if (_stylesDict == null) {
                    _stylesDict = InitStylesDict();
                }
                return _stylesDict;
            }
        }

        //head
        public TLWLabel lbTitle;
        public TLWLabel lbDevSpamServer;
        public TLWCheck ckSplitEnabled;
        public TLWLabel lbSplitThreshold;
        public TLWButton btLogout;
        public TLWComboBox cbVersionSelector;
        //

        //body
        public BundleSelectorWidget twBundleSelector;
        public TLWButton btRequestBundles;

        public BRCompilationResultSelector twCompilationResult;


        public BRView(TLWindow window, TLModel model) : base(window, model)
        {
            position = new Rect (100, 100, 575, 800);
            minSize = new Vector2 (575, 600);

            TLWVeticalLayout master_layout = new TLWVeticalLayout (this,
                                                                   "master_layout",
                                                                   stylesDict ["layout_margins_stl"],
                                                                   TLLayoutOptions.expandall);
            master_layout.spacing = 20f;

            TLWHorizontalLayout hLayout_0 = new TLWHorizontalLayout (this,
                                                                     "hLayout_0");

            lbTitle = new TLWLabel(this, "lbTitle", "Untitled", stylesDict["title_stl"]);
            lbDevSpamServer = new TLWLabel(this, "lbDevSpamServer", SPAMAuthenticator.PROD_SERVER ? "" : SPAMAuthenticator.SPAM_SERVICES_ENDPOINT, 
                                           stylesDict["dev_spam_server_stl"]); 
            btLogout = new TLWButton(this, "btLogout", TLIcons.powerImg);
            btLogout.SetDisabled(true);

            hLayout_0.AddWidget(lbTitle);
            hLayout_0.AddWidget(new TLWSpacer(this,"sp0_h",true));
            hLayout_0.AddWidget(lbDevSpamServer);
            hLayout_0.AddWidget(btLogout);


            TLWHorizontalLayout hLayout_1 = new TLWHorizontalLayout(this, "hLayout_1");
            TLWVeticalLayout vlayout_1 = new TLWVeticalLayout(this, "vLayout_1", TLLayoutOptions.noexpand);

            cbVersionSelector = new TLWComboBox(this, "cbVersionSelector", new GUILayoutOption[] { GUILayout.Width(150) } );
            ckSplitEnabled = new TLWCheck(this, "ckSplitEnabled", "Auto-Split enabled");
            lbSplitThreshold = new TLWLabel(this, "lbSplitThreshold", "Auto-Split threshold");


            hLayout_1.AddWidget(cbVersionSelector);
            hLayout_1.AddWidget(new TLWSpacer(this, "versionSelectorSpacer", true));
            vlayout_1.AddWidget(ckSplitEnabled);
            vlayout_1.AddWidget(lbSplitThreshold);
            hLayout_1.AddWidget(vlayout_1);

            master_layout.AddWidget(hLayout_0);
            master_layout.AddWidget(hLayout_1);

            twBundleSelector = new BundleSelectorWidget(this, "twBundleSelector", TLLayoutOptions.expandall);
            twBundleSelector.SetHeaderLabels( new BundleSelectorWidget.HeaderLabel[] {
                new BundleSelectorWidget.HeaderLabel() {label="bundle", expand=true,    width=300f},
                new BundleSelectorWidget.HeaderLabel() {label="incl",   expand=false,   width=100f}
            } );
            twBundleSelector.IsHeaderVisible = true;

            twCompilationResult = new BRCompilationResultSelector(this, "twCompilationResult", new GUILayoutOption[] { GUILayout.Height (125) });
            twCompilationResult.IsHeaderVisible = false;
            twCompilationResult.IsSeachVisible = true;

            btRequestBundles = new TLWButton(this, "btRequestBundles", "Request Bundles");

            master_layout.AddWidget(twBundleSelector);
            master_layout.AddWidget(twCompilationResult);
            master_layout.AddWidget(btRequestBundles);

            AddWidget(master_layout);
        }

        static Dictionary<string, TLStyle> InitStylesDict()
        {
            Dictionary<string, TLStyle> dict = new Dictionary<string, TLStyle> ();
            
            TLStyle layout_margins_stl = new TLStyle();
            layout_margins_stl.margin = new RectOffset (10, 10, 10, 10);
            dict.Add ("layout_margins_stl", layout_margins_stl);
            
            TLStyle title_stl = new TLStyle("Label");
            title_stl.alignment = TextAnchor.MiddleLeft;
            title_stl.margin = new RectOffset();
            title_stl.fontSize = 16;
            title_stl.fontStyle = FontStyle.Bold;
            dict.Add ("title_stl", title_stl);

            TLStyle dev_spam_server_stl = new TLStyle("Label");
            dev_spam_server_stl.alignment = TextAnchor.MiddleRight;
            dev_spam_server_stl.margin = new RectOffset();
            dev_spam_server_stl.fontSize = 12;
            dev_spam_server_stl.fontStyle = FontStyle.Italic;
            dev_spam_server_stl.normal.textColor = Color.green;
            dict.Add ("dev_spam_server_stl", dev_spam_server_stl);
            
            return dict;
        }
	}
}
