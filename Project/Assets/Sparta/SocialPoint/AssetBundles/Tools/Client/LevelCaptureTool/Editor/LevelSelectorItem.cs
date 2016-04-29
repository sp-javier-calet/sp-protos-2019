using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.LevelCaptureTool
{
	public class LevelSelectorItem : TLTreeSelectorItem<LevelSelectorItem>
	{
		
		static readonly GUILayoutOption[]   rowInnerLayoutOptions;
		static readonly TLStyle             version_stl;
		
		//Contents
		TLWCheck                            levelCb = null;
		
		public bool                         IsChecked { get { return levelCb.isChecked; } }
		
		static LevelSelectorItem ()
		{
			rowInnerLayoutOptions = TLLayoutOptions.basic;
		}
		
		public LevelSelectorItem (string levelName, TLView view) : base (levelName)
		{
			levelCb = new TLWCheck (view, "cb0_levelSelectorItem", levelName, TLLayoutOptions.basic);
			levelCb.SetCheck (false);
		}
		
		public void SetCheck (bool value)
		{
			levelCb.SetCheck (value);
		}
		
		public override void Draw (ref LevelSelectorItem selectedItem)
		{
			GUILayout.BeginHorizontal (_tabsStyle.GetStyle (), new GUILayoutOption[] { GUILayout.MaxHeight (20f) });
			
			GUILayout.BeginHorizontal (rowInnerLayoutOptions);
			
			//Checkbox + name
			TLEditorUtils.BeginCenterVertical ();
			
			levelCb.Perform ();
			
			TLEditorUtils.EndCenterVertical ();
			
			GUILayout.EndHorizontal ();
			GUILayout.EndHorizontal ();
		}
	}
}