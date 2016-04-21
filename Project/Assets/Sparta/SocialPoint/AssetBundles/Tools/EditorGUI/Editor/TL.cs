using UnityEngine;
using System.Collections;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Utilities for common layout configurations.
    /// </summary>
	public static class TLLayoutOptions
	{
		private static GUILayoutOption[] _basic;
        /// <summary>
        /// Expand only horizontally.
        /// </summary>
		public static GUILayoutOption[] basic { get { return _basic; } } 

		private static GUILayoutOption[] _expandall;
        /// <summary>
        /// Expand both horizontally and vertically.
        /// </summary>
		public static GUILayoutOption[] expandall { get { return _expandall; } }

		private static GUILayoutOption[] _vertical;
        /// <summary>
        /// Expand only vertically.
        /// </summary>
		public static GUILayoutOption[] vertical { get { return _vertical; } }

		private static GUILayoutOption[] _noexpand;
        /// <summary>
        /// Don't expand, use the minimum space required.
        /// </summary>
		public static GUILayoutOption[] noexpand { get { return _noexpand; } }

        private static GUILayoutOption[] _center_vertical;
        /// <summary>
        /// Force expand only vertical by limiting horizontal size to half pixel.
        /// </summary>
        public static GUILayoutOption[] center_vertical { get { return _center_vertical; } }

        private static GUILayoutOption[] _center_horizontal;
        /// <summary>
        /// Force expand only horizontal by limiting vertical size to half pixel.
        /// </summary>
        public static GUILayoutOption[] center_horizontal { get { return _center_horizontal; } }

		static TLLayoutOptions()
		{
			_basic = new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (false) };
			_expandall = new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true) };
			_vertical = new GUILayoutOption[] { GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true) };
			_noexpand = new GUILayoutOption[] { GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (false) };
            _center_vertical = new GUILayoutOption[] { GUILayout.ExpandHeight (true), GUILayout.Width (0.5f) };
            _center_horizontal = new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (0.5f) };
		}
	}

	public static class TLWindowDefaults
	{
		private static Rect 			_position;
		public static Rect 				position { get { return _position; } }

		private static Vector2 			_minSize;
		public static Vector2 			minSize { get { return _minSize; } }

		private static Vector2 			_maxSize;
		public static Vector2 			maxSize { get { return _maxSize; } }

		static TLWindowDefaults()
		{
			// Values taken from the orignal UnityEngine.EditorWindow default constructor
			_position = new Rect(0, 0, 320, 240);
			_minSize = new Vector2 (100, 100);
			_maxSize = new Vector2 (4000, 4000);
		}
	}

	public static class TLEditorUtils
	{
        /// <summary>
        /// A default white TLImage.
        /// </summary>
        public static TLImage   whiteImg;
        /// <summary>
        /// A default semi-transparent blue TLImage.
        /// </summary>
		public static TLImage 	blueTransImg;
        /// <summary>
        /// A default semi-transparent light gray TLImage.
        /// </summary>
        public static TLImage 	lightGrayTransImg;
        /// <summary>
        /// A default completely transparent TLImage.
        /// </summary>
        public static TLImage 	transImg;

        /// <summary>
        /// A TLStyle with a transparent TLImage used to ocuppy an invisible place.
        /// </summary>
		public static readonly TLStyle		placeholderStyle;

		static TLEditorUtils()
		{
            var whiteTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            whiteTexture.SetPixel (0,0,new Color(1.0f,1.0f,1.0f,1.0f));
            whiteTexture.Apply ();
            whiteImg = new TLImage(whiteTexture);

            var blueTransTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            blueTransTexture.SetPixel (0,0,new Color(0f,0f,0.5f,0.2f));
            blueTransTexture.Apply ();
            blueTransImg = new TLImage(blueTransTexture);

            var lightGrayTransTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            lightGrayTransTexture.SetPixel (0,0,new Color(0.7f,0.7f,0.7f,0.2f));
            lightGrayTransTexture.Apply ();
            lightGrayTransImg = new TLImage(lightGrayTransTexture);
            
            var transTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            transTexture.SetPixel (0,0,new Color(0f,0f,0f,0f));
            transTexture.Apply ();
            transImg = new TLImage(transTexture);

			placeholderStyle = new TLStyle ();
			placeholderStyle.normal.background = transImg;
		}

        /// <summary>
        /// Reimport all the associated Texture2D to the TLImage of this class.
        /// </summary>
        public static void Reimport ()
        {
            var whiteTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            whiteTexture.SetPixel (0,0,new Color(1.0f,1.0f,1.0f,1.0f));
            whiteTexture.Apply ();
            whiteImg.SetTexture (whiteTexture);

            var blueTransTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            blueTransTexture.SetPixel (0,0,new Color(0f,0f,0.5f,0.2f));
            blueTransTexture.Apply ();
            blueTransImg.SetTexture (blueTransTexture);
            
            var lightGrayTransTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            lightGrayTransTexture.SetPixel (0,0,new Color(0.7f,0.7f,0.7f,0.2f));
            lightGrayTransTexture.Apply ();
            lightGrayTransImg.SetTexture (lightGrayTransTexture);
            
            var transTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false);
            transTexture.SetPixel (0,0,new Color(0f,0f,0f,0f));
            transTexture.Apply ();
            transImg.SetTexture (transTexture);
        }

        /// <summary>
        /// Begin centering a widget vertically.
        /// </summary>
		static public void BeginCenterVertical()
		{
            GUILayout.BeginVertical(TLLayoutOptions.center_vertical);
			GUILayout.FlexibleSpace();
		}

        /// <summary>
        /// End centering a widget vertically.
        /// </summary>
		static public void EndCenterVertical()
		{
			GUILayout.FlexibleSpace ();
			GUILayout.EndVertical();
		}

        /// <summary>
        /// Begins centering a widget horizontally.
        /// </summary>
        static public void BeginCenterHorizontal()
        {
            GUILayout.BeginHorizontal(TLLayoutOptions.center_horizontal);
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// Ends centering a widget horizontally.
        /// </summary>
        static public void EndCenterHorizontal()
        {
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal();
        }
	}
}
