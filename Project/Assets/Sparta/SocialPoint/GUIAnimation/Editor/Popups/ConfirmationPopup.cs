using UnityEngine;
using UnityEditor;
using System;

namespace SocialPoint.GUIAnimation
{
	public class ConfirmationPopup : EditorWindowCallback
	{
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(ConfirmationPopup));
		}

		public void SetTitle(string title)
		{
			_title = title;
		}

		public string Value;

		string _title = "Confirm";
		Action _onExit;

		void OnGUI()
		{
			GUILayout.Label(_title, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleCenter));

			GUILayout.BeginArea (new Rect((Screen.width/2)-100, 50, 200, 50));

			GUILayout.BeginHorizontal();
			if( GUILayout.Button("Accept", GUILayout.MaxWidth(100f)) )
			{
				OnAccept();
				Close();
			}
			
			if( GUILayout.Button("Cancel", GUILayout.MaxWidth(100f)) )
			{
				OnCancel();
				Close();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
}
