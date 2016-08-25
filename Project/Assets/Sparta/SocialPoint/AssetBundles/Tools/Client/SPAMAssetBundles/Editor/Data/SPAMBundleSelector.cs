using System;
using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;
using System.Collections.Generic;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class SPAMBundleSelector : TLWTreeSelector<SPAMBundleSelectorItem> {

        TLEvent<SPAMBundleSelectorItem> _itemStateChanged;
        public TLEvent<SPAMBundleSelectorItem> itemStateChanged { get { return _itemStateChanged; } }

        TLEvent _bundleVersionChanged;
        public TLEvent bundleVersionChanged { get { return _bundleVersionChanged; } }

        TLEvent<SPAMBundleSelectorItem> _bundleItemRemoved;
        public TLEvent<SPAMBundleSelectorItem> bundleItemRemoved { get { return _bundleItemRemoved; } }

        public SPAMBundleSelector( TLView view, string name, GUILayoutOption[] options) : base ( view, name, options ) 
        {
            _bundleVersionChanged = new TLEvent ("bundleVersionChanged");
            _itemStateChanged = new TLEvent<SPAMBundleSelectorItem> ("itemStateChanged");
            _bundleItemRemoved = new TLEvent<SPAMBundleSelectorItem> ("bundleItemRemoved");
        }

        public new void SetListItems (List<SPAMBundleSelectorItem> items, bool sorted=false)
        {
            base.SetListItems (items, sorted);

            foreach (var item in _items) {
                item.itemChanged.Connect(itemStateChanged);
                item.itemRemoved.Connect(OnRemoveItemFromTree);
                bundleVersionChanged.Connect(item.OnVersionChanged);
            }
        }

        void OnRemoveItemFromTree(SPAMBundleSelectorItem item)
        {
            RemoveItem (item);
            bundleItemRemoved.Send(View.window, item);
        }
    }
}
