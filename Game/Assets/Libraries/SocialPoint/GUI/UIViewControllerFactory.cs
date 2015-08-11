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
        public delegate string StringDelegate();
        public delegate string DefaultStringDelegate(Type t);

        private IDictionary<Type, Delegate> _creators = new Dictionary<Type,Delegate>();
        private DefaultDelegate _defaultCreator;
		private UIViewControllerFactory _parent;

		public UIViewControllerFactory(UIViewControllerFactory parent=null)
		{
			_parent = parent;
		}

		public void Define(DefaultStringDelegate dlg)
        {
            Define((Type c) => {
                return CreateFromResource(c, dlg(c));
            });
        }

        public void Define(DefaultDelegate dlg)
        {
            _defaultCreator = dlg;
        }

        public void Define(Type c, StringDelegate dlg)
        {
            Define(c, () => {
                return CreateFromResource(c, dlg());
            });
        }
        
        public void Define<C>(StringDelegate dlg) where C : UIViewController
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

		
		public static UIViewController CreateFromResource(string prefab=null)
		{
			var robj = Resources.Load(prefab);
			if(robj != null)
			{
				var go = (GameObject)GameObject.Instantiate(robj);
				return go.GetComponent<UIViewController>();
			}
			return null;
		}

        public UIViewController CreateFromResource(Type c, string prefab=null)
        {
            var robj = Resources.Load(prefab);
            if(robj != null)
            {
                var go = (GameObject)GameObject.Instantiate(robj);
                return (UIViewController)go.GetComponent(c);
            }
			return null;
        }

        public UIViewController Create(Type c, string prefab=null)
        {
            UIViewController ctrl = null;
            if(ctrl == null && prefab != null)
            {
                ctrl = CreateFromResource(c, prefab);
            }
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
				if(_parent != null)
				{
					ctrl = _parent.Create(c, prefab);
				}
			}
            if(ctrl == null)
            {
                var go = new GameObject();
                if(prefab == null)
                {
                    prefab = c.ToString();
                }
                ctrl = (UIViewController)go.AddComponent(c);
            }
            if(prefab != null)
            {
                ctrl.gameObject.name = prefab;
            }
            return ctrl;
        }
       
    }
}