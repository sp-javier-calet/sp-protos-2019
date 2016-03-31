using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
	public class SPAMProgressBar
	{
		TLImage _frameImg;
		TLImage _fillImg;
		float _progress;
		int _maxWidth;
		int _maxHeight;

		public bool IsHidden	{ get; set; }

		TLStyle _placeholderStyle;
		TLStyle _frameStyle;
		TLStyle _fillStyle;
		
		public SPAMProgressBar ( int maxWidth, int maxHeight)
		{
			_maxWidth = maxWidth;
			_maxHeight = maxHeight;

			Init ();
		}

		void Init()
		{
			_frameImg = SPAMResources.progressBarFrame;
			_fillImg = SPAMResources.progressBarFill;
			_progress = 0;
			IsHidden = true;

			_placeholderStyle = new TLStyle ();
			_placeholderStyle.normal.background = TLEditorUtils.transImg;

			_frameStyle = new TLStyle ("Box");
			_frameStyle.normal.background = _frameImg;
			_frameStyle.border = new RectOffset (10, 10, 5, 5);

			_fillStyle = new TLStyle ();
			_fillStyle.normal.background = _fillImg;
			_fillStyle.border = new RectOffset (0, 5, 0, 0);
		}

		public void UpdateProgress(float progress)
		{
			_progress = progress;
			if (_progress > 100f)
				_progress = 100f;
		}

		public void Draw()
		{
			if (!IsHidden) {

				GUILayout.BeginHorizontal (new GUILayoutOption[] { GUILayout.ExpandWidth (false), 
					GUILayout.ExpandHeight (false),
					GUILayout.MinWidth (_maxWidth),
					GUILayout.MinHeight (_maxHeight)
				});

				// placeholder box to get window rect
				GUILayout.Box (new GUIContent (), TLEditorUtils.placeholderStyle.GetStyle (), TLLayoutOptions.expandall);

				Rect lastR = GUILayoutUtility.GetLastRect ();
				Rect newR = new Rect (lastR);
				newR.width = _fillStyle.border.right + (lastR.width * _progress / 100f);

				if( newR.width > lastR.width )
					newR.width = lastR.width;

				GUI.Box (newR, new GUIContent (), _fillStyle.GetStyle ());
				GUI.Box (lastR, new GUIContent (), _frameStyle.GetStyle ());

				GUILayout.EndHorizontal ();

			} else {
				// when hidden, does not occupy horizontal space
				GUILayout.BeginHorizontal (new GUILayoutOption[] { GUILayout.ExpandWidth (false), 
					GUILayout.ExpandHeight (false),
					GUILayout.MaxWidth (1),
					GUILayout.MinHeight (_maxHeight)
				});

				// placeholder box to get window rect
				GUILayout.Box (new GUIContent (), TLEditorUtils.placeholderStyle.GetStyle (), TLLayoutOptions.expandall);

				GUILayout.EndHorizontal ();
			}
		}
	}
}
