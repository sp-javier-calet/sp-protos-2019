using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.GUI
{
    public class UIViewControllerFactory
    {
        public delegate UIViewController Delegate();

        public delegate UIViewController DefaultDelegate(Type t);

        public delegate string PrefabDelegate();

        public delegate string DefaultPrefabDelegate(Type t);

        public delegate void FilterDelegate(UIViewController ctrl,Type t);

        private IDictionary<Type, Delegate> _creators = new Dictionary<Type,Delegate>();
        private IDictionary<Type, PrefabDelegate> _prefabCreators = new Dictionary<Type,PrefabDelegate>();
        private DefaultDelegate _defaultCreator;
        private DefaultPrefabDelegate _defaultPrefabCreator;
        private UIViewControllerFactory _parent;

        public event FilterDelegate Filter;

        public UIViewControllerFactory(UIViewControllerFactory parent=null)
        {
            _parent = parent;
        }

        public void Define(DefaultPrefabDelegate dlg)
        {
            _defaultPrefabCreator = dlg;
        }

        public void Define(DefaultDelegate dlg)
        {
            _defaultCreator = dlg;
        }

        public void Define(Type c, PrefabDelegate dlg)
        {
            _prefabCreators[c] = dlg;
        }
        
        public void Define<C>(PrefabDelegate dlg) where C : UIViewController
        {
            Define(typeof(C), dlg);
        }

        public void Define(Type c, Delegate dlg)
        {
            _creators[c] = dlg;
        }

        public void Define<C>(Delegate dlg) where C : UIViewController
        {
            Define(typeof(C), dlg);
        }
        
        public void Define(Type c, string prefab)
        {
            Define(c, () => {
                return Create(c, prefab);
            });
        }

        public void Define<C>(string prefab) where C : UIViewController
        {
            Define(typeof(C), prefab);
        }

        public C Create<C>(string name=null) where C : UIViewController
        {
            return (C)Create(typeof(C), name);
        }
        
        private static UIViewController CreateFromResource(string prefab)
        {
            var robj = Resources.Load(prefab);
            if(robj != null)
            {
                var go = (GameObject)GameObject.Instantiate(robj);
                return go.GetComponent<UIViewController>();
            }
            return null;
        }

        private static UIViewController CreateFromResource(Type c, string prefab)
        {
            var robj = Resources.Load(prefab);
            if(robj != null)
            {
                var go = (GameObject)GameObject.Instantiate(robj);
                return (UIViewController)go.GetComponent(c);
            }
            return null;
        }

        public UIViewController Create(Type c, string prefab)
        {
            if(prefab == null)
            {
                return Create(c);
            }
            else
            {
                var ctrl = CreateFromResource(c, prefab);
                return CreateEnd(c, prefab, ctrl);
            }
        }

        public UIViewController Create(Type c)
        {
            UIViewController ctrl = null;
            string prefab = null;
            if(ctrl == null)
            {
                Delegate creator = null;
                if(_creators.TryGetValue(c, out creator))
                {
                    ctrl = creator();
                }
            }
            if(ctrl == null)
            {
                if(_defaultCreator != null)
                {
                    ctrl = _defaultCreator(c);
                }
            }
            if(ctrl == null)
            {
                PrefabDelegate prefabCreator = null;
                if(_prefabCreators.TryGetValue(c, out prefabCreator))
                {
                    prefab = prefabCreator();
                    ctrl = CreateFromResource(prefab);
                }
            }
            if(ctrl == null)
            {
                if(_defaultPrefabCreator != null)
                {
                    prefab = _defaultPrefabCreator(c);
                    ctrl = CreateFromResource(prefab);
                }
            }
            return CreateEnd(c, prefab, ctrl);
        }

        private UIViewController CreateEnd(Type c, string prefab, UIViewController ctrl)
        {
            if(ctrl == null)
            {
                if(_parent != null)
                {
                    ctrl = _parent.Create(c, prefab);
                }
            }
            if(ctrl == null)
            {
                throw new MissingComponentException(string.Format(
                    "Could not find controller for type {0} and prefab {1}.", c, prefab)); 
            }
            if(prefab != null)
            {
                ctrl.gameObject.name = prefab;
            }
            if(Filter != null)
            {
                Filter(ctrl, c);
            }
            return ctrl;
        }
       
    }
}