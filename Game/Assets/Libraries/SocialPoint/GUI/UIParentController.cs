using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.GUI
{
    public class UIParentController : UIViewController
    {
        public C CreateChild<C>(string prefab=null) where C : UIViewController
        {
            return (C)CreateChild(typeof(C), prefab);
        }

        public UIViewController CreateChild(Type c, string prefab=null)
        {
            UIViewController ctrl = null;
            if(ctrl == null)
            {
                var ctrls = gameObject.GetComponentsInChildren(c, true);
                for(int i=0; i<ctrls.Length; ++i)
                {
                    var elm = (UIViewController)ctrls[i];
                    if(elm.Parent == null)
                    {
                        ctrl = elm;
                        break;
                    }
                }
            }
            if(ctrl == null)
            {
				ctrl = Factory.Create(c, prefab);
            }
            if(ctrl != null)
            {
                AddChild(ctrl);
            }
            return ctrl;
        }

        public void AddChild(GameObject go)
        {
            var ctrl = go.GetComponent<UIViewController>();
            if(ctrl == null)
            {
                throw new MissingComponentException("Could not find UIViewController component.");
            }
            AddChild(ctrl);
        }

        public void AddChild(UIViewController ctrl)
        {
            if(ctrl == null)
            {
                throw new ArgumentException("Controller cannot be null.");
            }
            RemoveChild(ctrl);
            ctrl.Parent = this;
			ctrl.transform.SetParent(transform);
            ctrl.transform.localScale = Vector3.one;
            ctrl.ViewEvent += OnChildViewStateChanged;
        }

        protected void RemoveChild(UIViewController ctrl)
        {
            if(ctrl == null)
            {
                throw new ArgumentException("Controller cannot be null.");
            }
            ctrl.transform.SetParent(null);
            ctrl.ViewEvent -= OnChildViewStateChanged;
        }

        virtual protected void OnChildViewStateChanged(UIViewController ctrl, ViewState state)
        {
        }

        override protected void Disable()
        {
            // prevent the parent controller
            // from being SetActive(false);
        }

    }
}
