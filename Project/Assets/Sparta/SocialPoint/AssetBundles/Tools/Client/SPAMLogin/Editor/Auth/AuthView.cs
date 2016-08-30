using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
	public sealed class AuthView : TLView
	{
		public AuthModel 		Model { get { return (AuthModel)_model; } }
		public AuthController 	Controller 	{ get { return (AuthController)_controller; } }

		static Dictionary<string, TLStyle>			_stylesDict;
		public static Dictionary<string, TLStyle>	stylesDict
		{
			get
			{
				if(_stylesDict == null) {
					_stylesDict = InitStylesDict();
				}
				return _stylesDict;
			}
		}

		public TLWHorizontalLayout	hlHlayout_0 	{ get; set; }

		public TLWIcon				icoLogginStatus { get; set; }
		public TLWLabel 			lbLogginMessage { get; set; }
		public TLWLabel 			lbloadingDots 	{ get; set; }

		public AuthView( TLWindow window, TLModel model ): base ( window, model )
		{
			position = new Rect (100, 100, 200, 100);

			hlHlayout_0 = new TLWHorizontalLayout (this, "hlHlayout_0");

			icoLogginStatus = new TLWIcon (this, "icoLogginStatus", TLIcons.failImg, stylesDict ["label_stl"], TLLayoutOptions.expandall);
			icoLogginStatus.SetVisible(false);

			lbLogginMessage = new TLWLabel (this, "textLabel", "Trying to automatically login", stylesDict ["label_stl_message"], TLLayoutOptions.expandall);

			string dotsText = "" + string.Concat(Enumerable.Repeat(".", Model.numLoadingDots).ToArray());
			lbloadingDots = new TLWLabel (this, "lbloadingDots", dotsText, stylesDict ["label_stl"], TLLayoutOptions.expandall);

			hlHlayout_0.AddWidget (icoLogginStatus);
			hlHlayout_0.AddWidget (lbLogginMessage);

			AddWidget (hlHlayout_0);
			AddWidget (lbloadingDots);
		}

		static Dictionary<string, TLStyle> InitStylesDict()
		{
			Dictionary<string, TLStyle> dict = new Dictionary<string, TLStyle> ();

			TLStyle label_stl = new TLStyle("Label");
			label_stl.alignment = TextAnchor.MiddleCenter;
			label_stl.stretchWidth = true;
			dict.Add ("label_stl", label_stl);

			TLStyle label_stl_message = new TLStyle("Label");
			label_stl_message.alignment = TextAnchor.MiddleCenter;
			label_stl_message.stretchWidth = true;
			label_stl_message.wordWrap = true;
			dict.Add ("label_stl_message", label_stl_message);

			return dict;
		}

        public override void OnDestroy()
        {
            Controller.Abort();
            
            base.OnDestroy();
        }
	}
}
