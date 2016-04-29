using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SocialPoint.Tool.Shared.TLGUI
{
	public static class TLTreeSelectorStyles
	{
        public static readonly float    fixedItemHeight = 20f;

		public static readonly TLStyle 	TreeSelectorStyle;
        public static readonly TLStyle  TreeSelectorHeaderStyle;
        public static readonly TLStyle  TreeSelectorHeaderLabelStyle;
        public static readonly TLStyle  TreeSelectorHeaderLabelStyleDisabled;
		public static readonly TLStyle 	InnerSearchFieldStyle;
        public static readonly TLStyle  InnerSearchFieldInactiveStyle;
		public static readonly TLStyle 	InnerExpandButtonStyle;

		static TLTreeSelectorStyles()
		{
			TreeSelectorStyle = new TLStyle ();
			TreeSelectorStyle.normal.background = new TLImage(TLIcons.insetFrameImg);
			TreeSelectorStyle.border = new RectOffset (2, 1, 2, 1);

            TreeSelectorHeaderStyle = new TLStyle ();
            TreeSelectorHeaderStyle.normal.background = new TLImage(TLIcons.frameHeaderImg);
            TreeSelectorHeaderStyle.border = new RectOffset (0, 3, 0, 3);
            TreeSelectorHeaderStyle.padding = new RectOffset (5, 5, 0, 0);

            TreeSelectorHeaderLabelStyle = new TLStyle ();
            TreeSelectorHeaderLabelStyle.fontSize = 12;
            TreeSelectorHeaderLabelStyle.normal.textColor = Color.gray;
            TreeSelectorHeaderLabelStyle.alignment = TextAnchor.MiddleCenter;

            TreeSelectorHeaderLabelStyleDisabled = new TLStyle(TreeSelectorHeaderLabelStyle);
            TreeSelectorHeaderLabelStyleDisabled.normal.textColor = Color.black;

			InnerSearchFieldStyle = new TLStyle ("TextField");
			InnerSearchFieldStyle.margin = InnerSearchFieldStyle.margin ?? new RectOffset ();
			InnerSearchFieldStyle.margin.bottom = 10;

            InnerSearchFieldInactiveStyle = new TLStyle(InnerSearchFieldStyle);
            InnerSearchFieldInactiveStyle.normal.textColor = Color.gray;
            InnerSearchFieldInactiveStyle.fontStyle = FontStyle.Italic; 

			InnerExpandButtonStyle = new TLStyle ();
			InnerExpandButtonStyle.padding = new RectOffset (6, 6, 6, 6);
            InnerExpandButtonStyle.margin = new RectOffset (0, 0, 0, 0);
			//InnerExpandButtonStyle.alignment = TextAnchor.MiddleCenter;
		}
	}

    /// <summary>
    /// A tree selector widget.
    /// </summary>
    /// Tree selector allows to draw generic TLTreeSelectorItem in a column and select one of them.
    /// It is drawn onto a scroll view so if the elements don't fit, they can still be accessed via scoll.
    /// Has an optional search box widget to scroll and select directly to matched elements.
    /// Can send selected item chanege events.
	public class TLWTreeSelector<T> : TLWidget where T : TLTreeSelectorItem<T>
	{	
        protected GUILayoutOption[]   _searchFieldLayoutOpts;

        private TLEvent 	_onSelectedChange;
        protected TLEvent 	_onSearchTextChanged;
        private TLEvent		_onItemsChanged;

        /// <summary>
        /// Gets the selected item change event to connect to.
        /// </summary>
        /// <value>The selected item change event to connect to.</value>
        public TLEvent		onSelectedChange { get { return _onSelectedChange; } }

		protected List<T> 	_items;
        protected T[]       _itemsArr; // items in array structure, this should be faster for drawing

        private float       _lastScrollViewHeight; // for drawing optimization purposes
        private float       _lastScrollPosY; // for drawing optimization purposes

		private Vector2 	_scrollPos;
		private bool		_scrollNeeded;

        protected string    _searchControlName;
		private int 		_searchItemPosition;
		private T 			_selected;
        /// <summary>
        /// Gets or programatically sets the selected TLTreeSelectorItem item.
        /// </summary>
        /// If the selected item changes, it will fire a selected item change event.
        /// If the selected item changes and the search box is enabled, it will scroll to the selected position.
        /// <value>The selected item.</value>
		public T Selected { 
			get {
				return _selected;
			}
			protected set {
				if(_selected != value){
					_selected = value;
					if (_selected != null && _selected.Depth == 0)
						_searchItemPosition = GetItemPosition(_selected);
					View.window.eventManager.AddEvent(_onSelectedChange);
				}
			}
		}
        /// <summary>
        /// Gets the search box text.
        /// </summary>
        /// <value>The search box text.</value>
		public string SearchText 	{ get; protected set; }
		bool _isSearchVisible;
        /// <summary>
        /// Gets or sets a value indicating whether the search box is visible or not.
        /// </summary>
        /// <value><c>true</c> if the search box is visible; otherwise, <c>false</c>.</value>
		public bool IsSeachVisible  { 
			get
			{
				return _isSearchVisible;
			}
			set 
			{
				if(_isSearchVisible != value) {
					_isSearchVisible = value;
					View.window.Repaint();
				}
			} 
		}

        public struct HeaderLabel
        {
            public string label;
            public float width;
            public bool expand;

            public GUILayoutOption[] GetLayoutOptions()
            {
                if(expand)
                {
                    return new GUILayoutOption[] { GUILayout.Height(18f), GUILayout.ExpandWidth(true) };
                }
                else
                {
                    return new GUILayoutOption[] { GUILayout.Height(18f), GUILayout.Width(width) };
                }
            }
        }

        HeaderLabel[] _headerLabels;
        bool _isHeaderVisible;
        public bool IsHeaderVisible {
            get
            {
                return _isHeaderVisible;
            }
            set
            {
                if(_isHeaderVisible != value) {
                    _isHeaderVisible = value;
                    View.window.Repaint();
                }
            }
        }

		public TLWTreeSelector( TLView view, string name ): base ( view, name, TLLayoutOptions.noexpand )
		{
			Init ();
		}

		public TLWTreeSelector( TLView view, string name, GUILayoutOption[] options ): base ( view, name, options )
		{
			Init ();
		}

		protected virtual void Init( TLStyle style = null )
		{
			// Set the default unique style and apply the passed one if needed
			Style = new TLStyle( TLTreeSelectorStyles.TreeSelectorStyle );
			Style.Combine (style);
			
			_onSelectedChange = new TLEvent( "OnSelectedChange" );
			_onSearchTextChanged = new TLEvent ("OnSearchTextChanges");
			_onItemsChanged = new TLEvent ("OnItemsChanged");

            _lastScrollViewHeight = -1f;
			_items = new List<T> ();
            _itemsArr = null;
			SearchText = "";
			_scrollPos = new Vector2 (0, 0);
            _lastScrollPosY = 0f;
			_scrollNeeded = false;
			_searchItemPosition = -1;
            _headerLabels = new HeaderLabel[] {};
			IsSeachVisible = true;
            IsHeaderVisible = false;
            _searchControlName = Name + "_searchWidget";

			_searchFieldLayoutOpts = new GUILayoutOption[] { GUILayout.MinWidth (150), GUILayout.MinHeight (20),
				GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (false) };

			_onSearchTextChanged.Connect (SearchWord);
			_onItemsChanged.Connect (OnItemsChanged);
		}

		static int CompareItemsByName( T x, T y )
		{
			return x.Content.CompareTo (y.Content);
		}

        /// <summary>
        /// Sets the tree list items.
        /// </summary>
        /// <param name="items">Items.</param>
        /// <param name="sorted">If set to <c>true</c> the items will be sorted by name.</param>
		public void SetListItems (List<T> items, bool sorted=false)
		{
			if (_items != null)
				foreach (T item in _items)
					item.RemoveFromTree ();

			_items = items;

			if (_items != null) {
                if (sorted)
				    _items.Sort (TLWTreeSelector<T>.CompareItemsByName);

				foreach (T item in _items) {
					item.Depth = 0;
					item.SetTreeWidget (this);
					item.onChildsChanged.Connect (OnItemChanged);
				}
			}

            _itemsArr = _items.ToArray();

			View.window.eventManager.AddEvent(_onItemsChanged);
		}

        /// <summary>
        /// Removes the item from the list.
        /// </summary>
        /// <param name="item">Item.</param>
        public void RemoveItem (T item)
        {
            for (int i = 0; i < _items.Count; ++i) {
                if (_items[i] == item) {
                    _items[i].RemoveFromTree ();
                    _items.RemoveAt (i);

                    View.window.eventManager.AddEvent(_onItemsChanged);
                }
            }

            _itemsArr = _items.ToArray();
        }

        /// <summary>
        /// Gets the tree list items.
        /// </summary>
        /// <returns>The items.</returns>
		public List<T> GetListItems (bool excludeChilds = false)
		{
			if(excludeChilds)
            {
                return _items;
            }

            var result = new List<T> ();
            for(int i=0; i<_items.Count; ++i)
            {
                result.Add(_items[i]);
                result.AddRange(_items[i].GetChilds());
            }
            return result;
		}

        public void SetHeaderLabels(HeaderLabel[] labels)
        {
            _headerLabels = labels;
        }

		public override void Update(double elapsed) 
		{
            if(_itemsArr != null)
            {
    			for (int i=0; i<_itemsArr.Length; ++i) {
                    _itemsArr[i].Update(elapsed);
    			}
            }

            _lastScrollPosY = _scrollPos.y;
		}

        public virtual void DrawSearch()
        {
            GUI.SetNextControlName (_searchControlName);

            bool isSearchFocused = GUI.GetNameOfFocusedControl () == _searchControlName;

            if (isSearchFocused)
            {
                if (View.window.PrevFocusedControl != _searchControlName)
                {
                    if (SearchText == String.Empty)
                    {
                        GUILayout.TextField ("", TLTreeSelectorStyles.InnerSearchFieldStyle.GetStyle (), _searchFieldLayoutOpts);
                    }
                }
                else
                {
                    string newSearchText = GUILayout.TextField (SearchText, TLTreeSelectorStyles.InnerSearchFieldStyle.GetStyle (), _searchFieldLayoutOpts);
                    
                    if (newSearchText != SearchText) {
                        View.window.eventManager.AddEvent (_onSearchTextChanged);
                        SearchText = newSearchText;
                    }
                }
            }
            else
            {
                if (SearchText == String.Empty)
                {
                    GUILayout.TextField ("Search...", TLTreeSelectorStyles.InnerSearchFieldInactiveStyle.GetStyle (), _searchFieldLayoutOpts);
                }
                else
                {
                    GUILayout.TextField (SearchText, TLTreeSelectorStyles.InnerSearchFieldStyle.GetStyle (), _searchFieldLayoutOpts);
                }
            }
        }
		
		public override void Perform ()
		{
            GUILayout.BeginVertical (TLLayoutOptions.basic);

            // Display search field(a header row previous to the table that can be customized)
            if (IsSeachVisible)
            {
                GUILayout.BeginHorizontal ();

                DrawSearch();

                GUILayout.EndHorizontal ();
            }


            GUI.SetNextControlName (Name);

            // Display Header on top of the scroll view
            if (IsHeaderVisible) {
                GUILayout.BeginHorizontal(TLTreeSelectorStyles.TreeSelectorHeaderStyle.GetStyle(), new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
                //Invisible label if no labels are defined
                if (_headerLabels.Length <= 0)
                {
                    GUILayout.Label("", new GUILayoutOption[] { GUILayout.Height(18f), GUILayout.ExpandWidth(false) });
                }
                //Header labels
                TLStyle style = IsDisabled ? TLTreeSelectorStyles.TreeSelectorHeaderLabelStyleDisabled : TLTreeSelectorStyles.TreeSelectorHeaderLabelStyle;
                foreach(var headerLabel in _headerLabels)
                {
                    GUILayout.Label(headerLabel.label,
                                    style.GetStyle(),
                                    headerLabel.GetLayoutOptions());
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical ();
            GUILayout.BeginVertical (Options);
            
            _scrollPos = GUILayout.BeginScrollView (_scrollPos,  GetStyle(), Options );
            GUILayout.BeginVertical (TLListSelectorStyles.InnerLayoutStyle.GetStyle ());

            // Optimized draw
            if(_lastScrollViewHeight == -1f)
            {
                BasicDraw();
            }
            else
            {
                OptimizedDraw();
            }

            //
			
			if (_scrollNeeded) {
				ScrollToSelected ();
				_scrollNeeded = false;
			}

			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();

			GUILayout.EndVertical ();
            StoreLastScrollViewSize();
		}

        void StoreLastScrollViewSize()
        {
            if (Event.current.type == EventType.Repaint)
            {
                _lastScrollViewHeight = GUILayoutUtility.GetLastRect().height;
            }
        }

        void BasicDraw()
        {
            if (_itemsArr != null) {
                T baseSelected = Selected;
                for (int i = 0; i < _itemsArr.Length; ++i)
                    _itemsArr[i].Draw (ref baseSelected);
                Selected = baseSelected;
            }
        }

        void OptimizedDraw()
        {
            if (_itemsArr != null) {
                // This ranges will display double the size of the scroll view in elements because they are not clamped
                float displayHeightStart = _lastScrollPosY - _lastScrollViewHeight;
                float displayHeightEnd = _lastScrollPosY + _lastScrollViewHeight;
                float accumHeight = 0f;
                float prevAccumheight = 0f;

                // Empty spaces to fill the scroll non-viewed elements
                float preItemSpace = 0f;
                float postItemSpace = 0f;

                List<T> displyedItems = new List<T> ();
                for (int i = 0; i < _itemsArr.Length; ++i){
                    prevAccumheight = accumHeight;
                    accumHeight += _itemsArr[i].TotalHeight;

                    if(accumHeight < displayHeightStart)
                        preItemSpace = accumHeight;
                    else if(prevAccumheight > displayHeightEnd)
                        postItemSpace += _itemsArr[i].TotalHeight;
                    else
                    {
                        displyedItems.Add(_itemsArr[i]);
                    }
                }

                if(preItemSpace > 0f)
                {
                    GUILayout.Space(preItemSpace);
                }
                if(displyedItems.Count > 0)
                {
                    T baseSelected = Selected;
                    foreach (T item in displyedItems)
                        item.Draw (ref baseSelected);
                    Selected = baseSelected;
                }
                if(postItemSpace > 0f)
                {
                    GUILayout.Space(postItemSpace);
                }
            }
        }

		int GetItemPosition( T item ){

			if (item == null) return -1;

			int position = 0;
			foreach (T prevItem in _items) {
				if(item == prevItem){
					return position;
				} else {
					position += 1 + prevItem.GetExpandedPositionsCount();
				}
			}

			return -1;
		}

		void OnItemsChanged()
		{
			Selected = null;
		}

		void OnItemChanged()
		{
			if (Selected != null && !Selected.HasTree)
				Selected = null;
		}

		void ScrollToSelected()
		{
			int defaultMargin = 4; // layed out objects default margin
			Vector2 itemSize = TLListSelectorStyles.InnerItemStyle.GetStyle ().CalcSize (new GUIContent (Selected.Content));
			_scrollPos = new Vector2 (0, (itemSize.y + defaultMargin) * _searchItemPosition);
		}

		void SearchWord()
		{
			// Over a sorted list
			if (SearchText != "" && _items != null) {
				foreach (T item in _items) {
					if (item.Depth == 0 && item.Content.StartsWith (SearchText, true, CultureInfo.GetCultureInfo(0x000A))) {
						if(Selected != item) {
							Selected = item;
							_scrollNeeded = true;
						}
						return;
					}
				}
				Selected = null;
			}
		}
	}
}
