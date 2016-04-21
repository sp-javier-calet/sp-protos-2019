using UnityEngine;
using System;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Base class for TLTreeSelectorItem.
    /// </summary>
    /// TLTreeSelectorItem and its subclasses are used in TLWTreeSelector widgets.
    /// This sub classes should have it's own fairly-complex drawing methods. TLWTreeSelectorItems are meant to be an improved version of
    /// TLListSelectorItem with complex display and functionality.
    /// For instance, every item is meant to have nested sub-items(although they may not and be used only for the more complex functionality
    /// that this class provides over TLListSelectorItem).
    /// Can send childs changed event.
    public abstract class TLTreeSelectorItem<T> where T : TLTreeSelectorItem<T>
    {	
        public static readonly GUILayoutOption[] OuterLayoutOptions;
        public static readonly GUILayoutOption[] ExpandButtonLayoutOptions;

        static TLTreeSelectorItem()
        {
            OuterLayoutOptions = new GUILayoutOption[] { GUILayout.Height(TLTreeSelectorStyles.fixedItemHeight), GUILayout.ExpandWidth(true) };
            ExpandButtonLayoutOptions = new GUILayoutOption[] { GUILayout.Width(40f) };
        }

        protected List<T>						_childs;
        protected TLStyle						_tabsStyle;
        protected TLWidget						_treeWidget;
        protected T								_parent;
        private	 TLEvent						_onChildsChanged;
        protected TLEvent						_onCmdClick;

        //Drawing elements
        protected TLWButton                     _btnExpand;
        protected TLWSpacer                     _sp0; 

        /// <summary>
        /// Gets the childs changed event to connect to.
        /// </summary>
        /// <value>The childs changed event to connect to.</value>
        public TLEvent							onChildsChanged { get { return _onChildsChanged; } }

        /// <summary>
        /// Gets or sets the text content for this item.
        /// </summary>
        /// <value>The content.</value>
        public string							Content { get; set; }
        protected uint							_depth;
        /// <summary>
        /// Gets or sets the depth in the child hierarchy for this item.
        /// </summary>
        /// <value>The depth.</value>
        public uint								Depth
        {
            get
            {
                return _depth;
            }
            set
            {
                _depth = value;
                _tabsStyle.margin = new RectOffset((int)_depth * 12, 0, 0, 0);
                foreach(T child in _childs)
                    child.Depth = _depth + 1;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this child can be expanded.
        /// </summary>
        /// <value><c>true</c> if this instance is expansible; otherwise, <c>false</c>.</value>
        public bool IsExpansible { get; protected set; }
        /// <summary>
        /// Gets or sets a value indicating whether this child is expanded.
        /// </summary>
        /// <value><c>true</c> if this instance is expanded; otherwise, <c>false</c>.</value>
        public bool								IsExpanded { get; protected set; }
        /// <summary>
        /// Gets a value indicating whether this instance has a TLWTreeSelector widget as parent.
        /// </summary>
        /// <value><c>true</c> if this instance has tree; otherwise, <c>false</c>.</value>
        public bool								HasTree { get { return _treeWidget != null; } }
        /// <summary>
        /// Total display height of the widget including it's children(taking into account if it's expanded or not)
        /// </summary>
        /// <value>The total height.</value>
        public float                            TotalHeight { get; private set; }


        

        public TLTreeSelectorItem()
        {
            Content = "";
            _depth = 0;
            IsExpansible = false;
            IsExpanded = false;
            _childs = new List<T>();
            _tabsStyle = new TLStyle();

            _onChildsChanged = new TLEvent("OnChildsChanged");
            _onCmdClick = new TLEvent("OnCmdClicked");
        }

        public TLTreeSelectorItem(string text)
        {
            Content = text;
            _depth = 0;
            IsExpansible = false;
            IsExpanded = false;
            _childs = new List<T>();
            _tabsStyle = new TLStyle();

            _onChildsChanged = new TLEvent("OnChildsChanged");
            _onCmdClick = new TLEvent("OnCmdClicked");
        }

        /// <summary>
        /// Sets the childs sub-items for this node.
        /// </summary>
        /// <param name="childs">Childs.</param>
        public void SetChilds(List<T> childs)
        {
            foreach(T child in _childs)
            {
                child.RemoveFromTree();
            }

            _childs = childs;
            foreach(T child in _childs)
            {
                if(child == this)
                {
                    throw new Exception("TLTreeSelectorItem cannot be added as a child of himself");
                }
                child.Depth = Depth + 1;
                child.SetTreeWidget(_treeWidget);
                child.SetParent((T)this);
            }
            if(_childs != null && _childs.Count > 0)
            {
                IsExpansible = true;
            }

            _treeWidget.View.window.eventManager.AddEvent(_onChildsChanged);
        }

        /// <summary>
        /// Sets the another TLTreeSelectorItem sublcass as parent.
        /// </summary>
        /// <param name="parent">Parent.</param>
        public void SetParent(T parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Sets the another TLTreeSelectorItem sublcass as parent.
        /// </summary>
        /// <param name="parent">Parent.</param>
        public T GetParent()
        {
            return  _parent;
        }

        /// <summary>
        /// Sets the TLWTreeSelector widget top parent and the same for all the instance children.
        /// </summary>
        /// <param name="parent">Parent.</param>
        public void SetTreeWidget(TLWidget treeWidget)
        {
            _treeWidget = treeWidget;

            foreach(T child in _childs)
            {
                child.SetTreeWidget(treeWidget);
            }

            //Init drawing widgets now that we can get the view from the parent widget
            InitDrawing();
        }

        public List<T> GetChilds(bool excludeSubChilds=false)
        {
            if(excludeSubChilds)
            {
                return _childs;
            }

            var result = new List<T> ();
            for(int i=0; i<_childs.Count; ++i)
            {
                result.Add(_childs[i]);
                result.AddRange(_childs[i].GetChilds());
            }
            return result;
        }

        protected virtual void InitDrawing()
        {
            if(_treeWidget == null)
            {
                throw new Exception("TLTreeSelectorItem must be added to a TLWTreeSelectorWidget");
            }

            if(_treeWidget.View == null)
            {
                throw new Exception("TLTreeSelectorItem's _treeWidget item must be added to a TLView");
            }

            _btnExpand = new TLWButton(_treeWidget.View, "_btnExpand", TLIcons.plusImg, 20f, 20f, TLTreeSelectorStyles.InnerExpandButtonStyle);
            _btnExpand.onClickEvent.Connect(OnExpandButtonClicked);

            //Spacer placeholder where the expand button should be(for non expansible items)
            _sp0 = new TLWSpacer(_treeWidget.View, "_sp0", 24);
        }

        /// <summary>
        /// Removes this item and children from the top TLWTreeSelector widget.
        /// </summary>
        public void RemoveFromTree()
        {
            _treeWidget = null;
			
            foreach(T child in _childs)
            {
                child.RemoveFromTree();
            }
        }

        public virtual bool IsSelected()
        {
            return false;
        }

        /// <summary>
        /// Expand or contract the node.
        /// </summary>
        /// <param name="value">If set to <c>true</c> expand.</param>
        public void Expand(bool value)
        {
            if(IsExpansible && value != IsExpanded)
            {
                var img = value ? TLIcons.contractImg : TLIcons.plusImg;
                _btnExpand.SetImage(img, 12f, 12f);

                IsExpanded = value;
            }
        }

        /// <summary>
        /// Get the expansion depth level for the childs of the node.
        /// </summary>
        /// <returns>The expanded positions count.</returns>
        public int GetExpandedPositionsCount()
        {
            int count = 0;
            if(IsExpanded)
            {

                foreach(T child in _childs)
                {
                    count += 1 + child.GetExpandedPositionsCount();
                }
            }

            return count;
        }

        public virtual void Update(double elapsed)
        {
            BasicUpdate(elapsed);
        }

        public virtual void Draw(ref T selectedItem)
        {
            BasicDraw(ref selectedItem);
        }

        /// <summary>
        /// Basic Update to be called withing the Update cycle. Call this method too if overriding Update.
        /// </summary>
        /// <param name="elapsed">Elapsed.</param>
        public void BasicUpdate(double elapsed)
        {
            for(int i=0; i<_childs.Count; ++i)
            {
                _childs[i].Update(elapsed);
            }

            TotalHeight = CalcTotalHeight();
        }

        /// <summary>
        /// A basic draw method to be called within the Draw cycle. It can be safely overrided and not used at all.
        /// </summary>
        /// <param name="selectedItem">The Selected item in the tree.</param>
        public void BasicDraw(ref T selectedItem)
        {
            GUILayout.BeginHorizontal(_tabsStyle.GetStyle(), OuterLayoutOptions);

            if(IsExpansible)
            {
                TLEditorUtils.BeginCenterVertical();
                
                _btnExpand.Draw();
                
                TLEditorUtils.EndCenterVertical();
            }
            //Put an empty space where the expand button should be
            else
            {
                _sp0.Draw();
            }

            GUIStyle usedStyle = this == selectedItem ? TLListSelectorStyles.InnerItemSelectedStyle.GetStyle() : TLListSelectorStyles.InnerItemStyle.GetStyle();
			
            if(GUILayout.Button(Content, usedStyle, TLLayoutOptions.basic))
            {
                if(Event.current.command)
                {
                    _treeWidget.View.window.eventManager.AddEvent(_onCmdClick);
                }

                if(selectedItem != this)
                {
                    selectedItem = (T)this;
                }
            }

            GUILayout.EndHorizontal();

            if(IsExpanded)
            {
                foreach(T child in _childs)
                    child.Draw(ref selectedItem);
            }

        }

        float CalcTotalHeight()
        {
            var totalHeight = TLTreeSelectorStyles.fixedItemHeight;
            if(!IsExpanded)
            {
                return totalHeight;
            }
            for(int i = 0; i < _childs.Count; ++i)
            {
                totalHeight += _childs[i].CalcTotalHeight();
            }
            return totalHeight;
        }

        void OnExpandButtonClicked()
        {
            Expand(!IsExpanded);
        }
    }
}