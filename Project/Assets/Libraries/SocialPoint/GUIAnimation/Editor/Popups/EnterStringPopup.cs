using UnityEngine;
using UnityEditor;
using System;

namespace SocialPoint.GUIAnimation
{
	public class EnterStringPopup : EditorWindowCallback
	{
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(EnterStringPopup));
		}

		public void SetTitle(string title)
		{
			_title = title;
		}

		public string Value;

		string _title = "Animation Name";
		Action _onExit;
		
		void OnGUI()
		{
			GUILayout.Label(_title, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleCenter));
			GUILayout.Space(10f);

			GUILayout.BeginHorizontal();
			GUILayout.Space(position.width*0.5f -100f);
			Value = EditorGUILayout.TextField(Value, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField, TextAnchor.MiddleCenter), GUILayout.Width(200f));
			GUILayout.EndHorizontal();

			GUILayout.BeginArea (new Rect((position.width/2f)-100, 65, 200, 50));

			GUILayout.BeginHorizontal();
			if( GUILayout.Button("Accept", GUILayout.MaxWidth(100f), GUILayout.ExpandWidth(false)) )
			{
				OnAccept();
				Close();
			}
			
			if( GUILayout.Button("Cancel", GUILayout.MaxWidth(100f), GUILayout.ExpandWidth(false)) )
			{
				OnCancel();
				Close();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
}
