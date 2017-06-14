#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
using GameObject = BehaviorDesigner.Runtime.Standalone.BehaviorGameObject;
#else
using UnityEngine;
#endif
using System;
using System.Reflection;

namespace BehaviorDesigner.Runtime
{
    public abstract class SharedVariable
    {
        [SerializeField]
        private bool mIsShared = false;
        public bool IsShared { get { return mIsShared; } set { mIsShared = value; } }

        [SerializeField]
        private bool mIsGlobal = false;
        public bool IsGlobal { get { return mIsGlobal; } set { mIsGlobal = value; } }

        [SerializeField]
        private string mName;
        public string Name { get { return mName; } set { mName = value; } }

        [SerializeField]
        private string mPropertyMapping;
        public string PropertyMapping { get { return mPropertyMapping; } set { mPropertyMapping = value; } }

        [SerializeField]
        private GameObject mPropertyMappingOwner;
        public GameObject PropertyMappingOwner { get { return mPropertyMappingOwner; } set { mPropertyMappingOwner = value; } }

        [SerializeField]
        private bool mNetworkSync;
        public bool NetworkSync { get { return mNetworkSync; } set { mNetworkSync = value; } }

#if ENABLE_MULTIPLAYER
        private Behavior mOwner;
        public Behavior Owner { set { mOwner = value; } }
#endif

        public bool IsNone { get { return mIsShared && string.IsNullOrEmpty(mName); } }

        public void ValueChanged()
        {
#if ENABLE_MULTIPLAYER
            if (mNetworkSync) {
                if (mIsGlobal) {
                    GlobalVariablesNetworkSync.DirtyVariable(this);
                } else if (mOwner != null) { 
                    mOwner.DirtyVariable(this); 
                }
            }
#endif
        }
        public virtual void InitializePropertyMapping(BehaviorSource behaviorSource) { }

        public abstract object GetValue();
        public abstract void SetValue(object value);
    }

    public abstract class SharedVariable<T> : SharedVariable
    {
        private Func<T> mGetter;
        private Action<T> mSetter;

        public override void InitializePropertyMapping(BehaviorSource behaviorSource)
        {
            if (!Application.isPlaying || !(behaviorSource.Owner.GetObject() is Behavior)) {
                return;
            }

            if (!string.IsNullOrEmpty(PropertyMapping)) {
                var propertyValue = PropertyMapping.Split('/');
                GameObject gameObject;
                if (!Equals(PropertyMappingOwner, null)) {
                    gameObject = PropertyMappingOwner;   
                } else {
                    gameObject = (behaviorSource.Owner.GetObject() as Behavior).gameObject;
                }
                var component = gameObject.GetComponent(TaskUtility.GetTypeWithinAssembly(propertyValue[0]));
                var componentType = component.GetType();
                var property = componentType.GetProperty(propertyValue[1]);
                if (property != null) {
                    var propertyMethod = property.GetGetMethod();
                    if (propertyMethod != null) {
#if NETFX_CORE && !UNITY_EDITOR
                        mGetter = (Func<T>)propertyMethod.CreateDelegate(typeof(Func<T>), component);
#else
                        mGetter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), component, propertyMethod);
#endif
                    }
                    propertyMethod = property.GetSetMethod();
                    if (propertyMethod != null) {
#if NETFX_CORE && !UNITY_EDITOR
                        mSetter = (Action<T>)propertyMethod.CreateDelegate(typeof(Action<T>), component);
#else
                        mSetter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), component, propertyMethod);
#endif
                    }
                }
            }
        }

        public T Value
        {
            get { return (mGetter != null ? mGetter() : mValue); }
            set
            {
                var changed = NetworkSync && !mValue.Equals(value);
                if (mSetter != null) {
                    mSetter(value);
                } else {
                    mValue = value;
                }
                if (changed) { ValueChanged(); }
            }
        }
        [SerializeField]
        protected T mValue;

        public override object GetValue() { return Value; }
        public override void SetValue(object value)
        {
            if (mSetter != null) {
                mSetter((T)value);
            } else {
                mValue = (T)value;
            }
        }

        public override string ToString()
        {
            return (Value == null ? "(null)" : Value.ToString());
        }
    }
}