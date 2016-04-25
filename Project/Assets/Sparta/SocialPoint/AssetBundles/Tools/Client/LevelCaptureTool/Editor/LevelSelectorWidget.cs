using System;
using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;
using System.Collections.Generic;
using System.Linq;

namespace SocialPoint.Editor.LevelCaptureTool
{
    public class LevelSelectorWidget : TLWTreeSelector<LevelSelectorItem>
    {
		
        public LevelSelectorWidget(TLView view, string name, GUILayoutOption[] options) : base ( view, name, options )
        {

        }

        public void SetListItems(string[] items, bool sorted=false)
        {
            base.SetListItems(items.Select(a => new LevelSelectorItem(a, View)).ToList(), sorted);
        }

        public string[] GetSelectedLevels()
        {
            var selectedLevels = new List<string>();

            foreach(var item in _items)
            {
                if(item.IsChecked)
                {
                    selectedLevels.Add(item.Content);
                }
            }

            return selectedLevels.ToArray();
        }

        public void SelectLevels(string[] selectedLevels)
        {
            var auxList = new List<string>(selectedLevels);
            foreach(var item in _items)
            {
                if(auxList.Remove(item.Content))
                {
                    item.SetCheck(true);
                }
            }
        }
    }
}