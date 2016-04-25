using UnityEngine;
using UnityEditor;
using SocialPoint.Tool.Shared.TLGUI;


namespace SocialPoint.Editor.LevelCaptureTool
{
	/// <summary>
	/// A TLWButton that opens a folder selection dialogue.
	/// </summary>
	public class TLWSelectFolderButton : TLWButton
	{

		private TLEvent _onPathSelectedEvent;
		private string _selectedPath;
		private string _title;

		/// <summary>
		/// Gets the selected path.
		/// </summary>
		/// <value>The selected path after the dialog has closed.</value>
		public string selectedPath { get { return _selectedPath; } }
		public TLEvent onPathSelectedEvent { get { return _onPathSelectedEvent; } }

		public TLWSelectFolderButton (TLView view, string name, string title, string text): base ( view, name, text )
		{
			Init(title);
		}
		
		public TLWSelectFolderButton (TLView view, string name, string title, string text, TLStyle style): base ( view, name, text, style )
		{
			Init(title);
		}
		
		public TLWSelectFolderButton (TLView view, string name, string title, string text, GUILayoutOption[] options): base ( view, name, text, options )
		{
			Init(title);
		}
		
		public TLWSelectFolderButton (TLView view, string name, string title, Texture2D tex): base ( view, name, tex )
		{
			Init(title);
		}
		
		public TLWSelectFolderButton (TLView view, string name, string title, Texture2D tex, float width, float height): base ( view, name, tex )
		{
			Init(title);
		}
		
		public TLWSelectFolderButton (TLView view, string name, string title, Texture2D tex, GUILayoutOption[] options): base ( view, name, tex, options )
		{
			Init(title);
		}
		
		public TLWSelectFolderButton (TLView view, string name, string title, Texture2D tex, float width, float height, GUILayoutOption[] options): base ( view, name, tex, width, height, options )
		{
			Init(title);
		}

		void Init(string title)
		{
			_selectedPath = "";
			_title = title;
			_onPathSelectedEvent = new TLEvent( "OnPathSelected" );
			onClickEvent.Connect(OpenDialog);
		}

		void OpenDialog()
		{
			var newPath = EditorUtility.OpenFolderPanel(_title, selectedPath, "");
			if (!newPath.Equals(string.Empty))
			{
				_selectedPath = newPath;
				View.window.eventManager.AddEvent( onPathSelectedEvent );
			}
		}
	}
}