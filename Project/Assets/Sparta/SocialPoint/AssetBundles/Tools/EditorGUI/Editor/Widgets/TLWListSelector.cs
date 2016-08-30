using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
	public static class TLListSelectorStyles
	{
		public static readonly TLStyle 	ListSelectorStyle;
		public static readonly TLStyle 	InnerLayoutStyle;
		public static readonly TLStyle 	InnerItemStyle;
		public static readonly TLStyle	InnerItemSelectedStyle;

		static TLListSelectorStyles()
		{
			ListSelectorStyle = new TLStyle ();
			ListSelectorStyle.normal.background = new TLImage(TLIcons.insetFrameImg);
			ListSelectorStyle.border = new RectOffset (2, 1, 2, 1);
			
			InnerLayoutStyle = new TLStyle ();
			InnerLayoutStyle.margin = new RectOffset (5, 5, 8, 8);
			
			InnerItemStyle = new TLStyle ("Label");
			InnerItemStyle.alignment = TextAnchor.MiddleLeft;
			InnerItemStyle.hover.background = TLEditorUtils.blueTransImg;
			InnerItemStyle.hover.textColor = Color.white;
			
			InnerItemSelectedStyle = new TLStyle ("Label");
			InnerItemSelectedStyle.alignment = TextAnchor.MiddleLeft;
			InnerItemSelectedStyle.fontStyle = FontStyle.Bold;
			InnerItemSelectedStyle.normal.background = TLEditorUtils.lightGrayTransImg;
			InnerItemSelectedStyle.normal.textColor = Color.white;
			InnerItemSelectedStyle.hover.background = TLEditorUtils.blueTransImg;
			InnerItemSelectedStyle.hover.textColor = Color.white;
		}
	}

    /// <summary>
    /// A generic list selector widget.
    /// </summary>
    /// List selector allows to draw generic TLListSelectorItem in a column and select one of them.
    /// Can send selected item chanege events.
	public sealed class TLWListSelector<T> : TLWidget where T : TLListSelectorItem, new()
	{	
        private TLEvent<T> _onSelectedChange;
        private TLEvent	_onItemsChanged;

        /// <summary>
        /// Gets the selected item change event to connect to.
        /// </summary>
        /// <value>The selected item change event to connect to.</value>
        public TLEvent<T> onSelectedChange 	{ get { return _onSelectedChange; } }

		private List<T> _items;

		private T 			_selected;
        /// <summary>
        /// Gets or programatically sets the selected TLListSelectorItem item.
        /// </summary>
        /// If the selected item changes, it will fire a selected item change event.
        /// <value>The selected item.</value>
		public T Selected { 
			get {
				return _selected;
			}
			private set {
				if(_selected != value){
					_selected = value;
                    onSelectedChange.Send(View.window, _selected);
				}
			}
		}

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The item count.</value>
        public int Count { get; private set; }


		public TLWListSelector( TLView view, string name ): base ( view, name, TLLayoutOptions.noexpand )
		{
			Init ();
		}

		public TLWListSelector( TLView view, string name, TLStyle style ): base ( view, name, TLLayoutOptions.noexpand )
		{
			Init (style);
		}

		public TLWListSelector( TLView view, string name, GUILayoutOption[] options ): base ( view, name, options )
		{
			Init ();
		}

		public TLWListSelector( TLView view, string name, TLStyle style, GUILayoutOption[] options ): base ( view, name, options )
		{
			Init (style);
		}

		private void Init( TLStyle style = null )
		{
			// Set the default unique style and apply the passed one if needed
			Style = new TLStyle( TLListSelectorStyles.ListSelectorStyle );
			Style.Combine (style);

            _onSelectedChange = new TLEvent<T>( "OnSelectedChange" );
			_onItemsChanged = new TLEvent ("OnItemsChanged");

			_items = new List<T> ();
            Count = 0;

			_onItemsChanged.Connect (OnItemsChanged);
		}

        public void SetSelected(string item)
        {
            for(int i = 0; i < _items.Count; ++i)
            {
                if(_items[i].Content == item)
                {
                    Selected = _items[i];
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the items in the list.
        /// </summary>
        /// <param name="items">Items.</param>
		public void SetListItems (List<T> items)
		{
			_items = items;
            Count = _items.Count;
            Selected = null;

			View.window.eventManager.AddEvent(_onItemsChanged);
		}

        /// <summary>
        /// Extends the items list.
        /// </summary>
        /// <param name="items">Items.</param>
        public void ExtendListItems (List<T> items)
        {
            _items.AddRange (items);
            Count = _items.Count;
            Selected = null;

            View.window.eventManager.AddEvent(_onItemsChanged);
        }
		
		public override void Perform ()
		{
			GUILayout.BeginHorizontal (GetStyle(), Options);
			GUILayout.BeginVertical (TLListSelectorStyles.InnerLayoutStyle.GetStyle (), TLLayoutOptions.noexpand);

			if (_items != null) {
				foreach (T item in _items) {
					GUIStyle usedStyle = item == Selected ? TLListSelectorStyles.InnerItemSelectedStyle.GetStyle () : TLListSelectorStyles.InnerItemStyle.GetStyle ();

					if (GUILayout.Button (item.Content, usedStyle, TLLayoutOptions.basic)) {
						if (Selected != item)
							Selected = item;
					}
				}
			}

			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
		}

		void OnItemsChanged()
		{
			View.window.Repaint ();
		}
	}
}