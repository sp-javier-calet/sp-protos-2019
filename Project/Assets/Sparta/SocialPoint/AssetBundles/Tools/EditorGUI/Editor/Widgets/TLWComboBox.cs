using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Rollable ComboBox Widget.
    /// </summary>
    /// A Button Widget that displays a drop-down TLWListSelector Widget when clicked.
    /// This is a special TLFloater Widget, meaning that (part of)its display is performed OVER other widgets and requires special handling.
    /// Can send selection change events.
    /// Can send list expanded true or false events(when expanded or contracted).
    public sealed class TLWComboBox : TLFloater
    {
        TLWVeticalLayout _vLayout;
        TLWListSelector<TLListSelectorItem> _expandList;
        int _expandListHeight;
        bool _dirtyItems;

        bool _expand;

        TLEvent<TLListSelectorItem> _selectionChange;
        /// <summary>
        /// Gets the selection change event to connect to.
        /// </summary>
        /// <value>The selection change event to connect to.</value>
        public TLEvent<TLListSelectorItem> selectionChange { get { return _selectionChange; } }

        TLEvent<bool> _expanded;
        /// <summary>
        /// Gets the list expanded event to connect to wich can be true or false depending if it was expanded or contracted.
        /// </summary>
        /// <value>The list expanded event to connect to.</value>
        public TLEvent<bool> expanded { get { return _expanded; } }

        /// <summary>
        /// Gets name of the selected element.
        /// </summary>
        /// <value>The selected element name.</value>
        public string Selected { get { return _expandList.Selected != null ? _expandList.Selected.Content : ""; } }

        public TLWComboBox( TLView view, string name ): base ( view, name )
        {
            Init();
            Style = new TLStyle("Button");
            Style.margin = new RectOffset();
            Style.alignment = TextAnchor.MiddleLeft;
        }

        public TLWComboBox( TLView view, string name, TLStyle style ): base ( view, name, style )
        {
            Init();
        }

        public TLWComboBox( TLView view, string name, GUILayoutOption[] options ): base ( view, name, options )
        {
            Init();
            Style = new TLStyle ("Button");
            Style.margin = new RectOffset();
            Style.alignment = TextAnchor.MiddleLeft;
        }
        
        public TLWComboBox( TLView view, string name, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
        {
            Init();
        }

        public void SetSelected(string item)
        {
            _expandList.SetSelected(item);
        }

        public override void Perform()
        {
            string selectedName = _expandList.Selected != null ? _expandList.Selected.Content : string.Empty;

            if (GUILayout.Button(selectedName, Style.GetStyle(), Options))
            {
                _expand = !_expand;
                if (_expand)
                {
                    expanded.Send(View.window, true);
                }
                else
                {
                    expanded.Send(View.window, false);
                }
            }

            if (Event.current.type.Equals(EventType.Repaint)) {
                var lastRect = GUILayoutUtility.GetLastRect ();
                paintArea = new Rect(lastRect.min.x, lastRect.max.y, lastRect.width, _expandListHeight);
            }
        }

        protected override void PerformFloater()
        {
            if (_expand)
            {
                if (_dirtyItems) {
                    float minEntryHeight = TLListSelectorStyles.InnerItemStyle.GetStyle().lineHeight;
                    _expandListHeight = 28 + ((int)minEntryHeight + 4) * _expandList.Count;
                    _dirtyItems = false;
                }

                GUILayout.BeginArea(paintArea);

                _vLayout.Draw();
                
                GUILayout.EndArea();
            }
        }

        /// <summary>
        /// Clear all the elements from the list.
        /// </summary>
        public void Clear() {
            _expandList.SetListItems(new List<TLListSelectorItem> ());
            _dirtyItems = true;
        }

        /// <summary>
        /// Add the specified items to the list.
        /// </summary>
        /// <param name="items">Item names array.</param>
        public void Add(string[] items) {
            var itemList = new List<TLListSelectorItem> ();
            for (int i=0; i < items.Length; ++i)
                itemList.Add( new TLListSelectorItem(items[i]) );
            _expandList.ExtendListItems (itemList);
            _dirtyItems = true;
        }

        void Init()
        {
            _vLayout = null;
            _expandList = null;
            _expandListHeight = 0;
            _dirtyItems = true;
            _expand = false;

            _vLayout = new TLWVeticalLayout (View,
                                             Name + "_vLayout",
                                             new GUILayoutOption[] { GUILayout.Height (20) });

            _expandList = new TLWListSelector<TLListSelectorItem> (View, Name + "_expandList");
            _expandList.Style.normal.background = TLEditorUtils.lightGrayTransImg;
            _expandList.Style.border = new RectOffset();

            _vLayout.AddWidget(_expandList);

            _selectionChange = new TLEvent<TLListSelectorItem>("selectionChange");
            _expanded = new TLEvent<bool>("expanded");
            _expandList.onSelectedChange.ConnectWithArguments(selectionChange);
        }
    }
}