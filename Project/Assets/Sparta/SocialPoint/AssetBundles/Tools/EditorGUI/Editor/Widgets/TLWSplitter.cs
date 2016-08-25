using UnityEngine;
using UnityEditor;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A line that visually splits spaces.
    /// </summary>
    /// When used in a TLWHorizontalLayout or TLWVerticalLayout will draw a horizontal or vertical line, visually dividing zones in the view.
	public sealed class TLWSplitter : TLWidget 
	{
		private Color? baseColor;

		public TLWSplitter( TLView view, string name ): base ( view, name )
		{
			InitWidget ();
		}

		public TLWSplitter( TLView view, string name, int thickness ): base ( view, name )
		{
			InitWidget (thickness);
		}

		private void InitWidget(int thickness = 1)
		{
			Options = new GUILayoutOption[TLLayoutOptions.basic.Length + 1];
			TLLayoutOptions.basic.CopyTo (Options, 0);
			Options [Options.Length - 1] = GUILayout.Height (thickness);
			TLStyle nStyle = new TLStyle ("Label");
			nStyle.stretchWidth = true;
			nStyle.normal.background = TLEditorUtils.whiteImg;
			Style = nStyle;
		}

		public override void Perform()
		{
			if (baseColor == null)
				baseColor = Style.GetBaseStyle().normal.textColor;

			Color bkpColor = GUI.color;
			GUI.color = baseColor.Value;
			GUILayout.Box("", GetStyle(), Options );
			GUI.color = bkpColor;
		}
	}
}
