using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public class Group : Step
    {
        [SerializeField]
        Transform _itemsRoot;

        protected Transform ItemsRoot
        {
            get
            {
                if(_itemsRoot == null)
                {
                    SetOrCreateItemsRoot();
                }

                return _itemsRoot;
            }
        }

        [SerializeField]
        protected List<Step> _animItems = new List<Step>();

        public List<Step> AnimItems
        {
            get
            {
                if(_animItems.Count == 0)
                {
                    _animItems = new List<Step>(ItemsRoot.GetComponents<Step>());
                }

                return _animItems;
            } 
        }

        void SetOrCreateItemsRoot()
        {
            if(_itemsRoot == null)
            {
                _itemsRoot = AnchorUtility.CreateParentTransform(StepName + "_Group");
            }

            _itemsRoot.SetParent(transform, false);
            ItemsRoot.gameObject.name = StepName + "Effects";
        }

        public void OnAnimationStartPlaying()
        {
            if(_parent == null)
            {
                float totalEndingTime = Animation.GetEndingTime();
                float scale = EndTime / totalEndingTime;
                SetEndTime(totalEndingTime, AnimTimeMode.Global);

                for(int i = 0; i < AnimItems.Count; ++i)
                {
                    AnimItems[i].ScaleTime(scale);
                }
            }
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);
			
            for(int i = 0; i < AnimItems.Count; ++i)
            {
                AnimItems[i].Invert(true);
            }
        }

        public override void Refresh()
        {
            SetOrCreateItemsRoot();

            _animItems.Clear();

            for(int i = 0; i < AnimItems.Count; ++i)
            {
                AnimItems[i].Refresh();
            }
        }

        public override void Init(Animation animation, Step parent)
        {
            base.Init(animation, parent);

            SetOrCreateItemsRoot();

            for(int i = 0; i < AnimItems.Count; ++i)
            {
                AnimItems[i].Init(animation, this);
            }
        }

        public void AddAndCopyAnimationItems(List<Step> animItems, bool calculateContainerTime)
        {
            float collectionStarTime = 999999f;
            float collectionEndTime = 0f;

            for(int i = 0; i < animItems.Count; ++i)
            {
                Step animItem = animItems[i];
                collectionStarTime = Mathf.Min(collectionStarTime, animItem.GetStartTime(AnimTimeMode.Global));
                collectionEndTime = Mathf.Max(collectionEndTime, animItem.GetEndTime(AnimTimeMode.Global));
            }

            // Calculte our Time
            if(calculateContainerTime)
            {
                SetStartTime(collectionStarTime, AnimTimeMode.Global);
                SetEndTime(collectionEndTime, AnimTimeMode.Global);
            }

            animItems.Sort(Animation.SortByStartTime);

            // Add Animation Items
            for(int i = 0; i < animItems.Count; ++i)
            {
                int slot = GetFirstFreeSlot(0, 999);

                Step copy = AddAndCopyAnimationItem(animItems[i], this);
                copy.SetStartTime(animItems[i].GetStartTime(AnimTimeMode.Global), AnimTimeMode.Global);
                copy.SetEndTime(animItems[i].GetEndTime(AnimTimeMode.Global), AnimTimeMode.Global);
                copy.SetSlot(slot);
            }

            Animation.RefreshAndInit();
        }

        public Step AddAndCopyAnimationItem(Step animItem, Step newParent = null)
        {
            var copy = (Step)ItemsRoot.gameObject.AddComponent(animItem.GetType());
            copy.Animation = Animation;

            copy.Copy(animItem);
            if(newParent != null)
            {
                copy.Init(Animation, newParent);
            }

            Animation.RefreshAndInit();

            return copy;
        }

        public Step AddAnimationItem(System.Type type, string name)
        {
            name = name != "" ? name : type.ToString();
            var newAnimItem = ItemsRoot.gameObject.AddComponent(type);
            ((Step)newAnimItem).StepName = name;
            ((Step)newAnimItem).OnCreated();
            _animation.RefreshAndInit();
			
            return (Step)newAnimItem;
        }

        public void RemoveAnimItem<T>(T item) where T:Step
        {
            _animation.Refresh();

            bool exist = AnimItems.Find(i => i == item);
            if(exist)
            {
                item.OnRemoved();
                Object.DestroyImmediate(item);
            }
            else
            {
                Log.w("[SPCollection] Trying to remove item " + item.name + " that is not in this collection");
            }
            _animation.RefreshAndInit();
        }

        public override void Copy(Step other)
        {
            base.Copy(other);

            _itemsRoot = null;
            SetOrCreateItemsRoot();

            CopyCollection((Group)other);
        }

        public void CopyCollection(Group other)
        {
            for(int i = 0; i < other.AnimItems.Count; ++i)
            {
                var copy = ItemsRoot.gameObject.AddComponent(other.AnimItems[i].GetType());
                ((Step)copy).Copy(other.AnimItems[i]);
            }
        }

        public static T MoveItem<T>(Group target, Group source, T sourceItem) where T:Step
        {
            T copiedItemInTarget = CopyItem(target, source, sourceItem, false);
            if(copiedItemInTarget != default(T))
            {
                source.RemoveAnimItem(sourceItem);
            }

            return copiedItemInTarget;
        }

        public static T CopyItem<T>(Group target, Group source, T sourceItem, bool isRecursive) where T:Step
        {
            bool existInSource = source.AnimItems.Find(i => i == sourceItem);
            if(!existInSource)
            {
                Log.w("[SPCollection] Trying to move item " + sourceItem.name + " that is not in this collection");
                return default(T);
            }
			
            var copiedItemInTarget = (T)target.AddAnimationItem(typeof(T), "");
            copiedItemInTarget.Copy(sourceItem);

            return copiedItemInTarget;
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            for(int i = 0; i < AnimItems.Count; ++i)
            {
                AnimItems[i].SaveValuesAt(localTimeNormalized);
            }
        }

        public override void OnRemoved()
        {
            for(int i = 0; i < AnimItems.Count; ++i)
            {
                AnimItems[i].OnRemoved();
            }

            for(int i = 0; i < AnimItems.Count; ++i)
            {
                RemoveAnimItem<Step>(AnimItems[i]);
            }

            AnimItems.Clear();

            if(_itemsRoot != null)
            {
                Object.DestroyImmediate(_itemsRoot.gameObject);
                _itemsRoot = null;
            }
        }

        public int GetFirstFreeSlot(int min, int max)
        {
            bool isFree;
            int slot = min;
            for(; slot < max; ++slot)
            {
                isFree = !AnimItems.Exists(animItem => animItem.Slot == slot);
                if(isFree)
                {
                    return slot;
                }
            }
            return slot;
        }
    }
}
