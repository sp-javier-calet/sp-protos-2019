using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.GUIAnimation
{
    public enum AnchorMode
    {
        Disabled,
        CurrentPosition,
        ClosestSide,
        Custom,
    }

    public static class AnchorUtility
    {
        public static void GetNormalizedPositionSize(Transform trans, ref Vector2 normPos, ref Vector2 normSize)
        {
            #if NGUI
            GetNormalizedPositionSizeNGUI(trans, ref normPos, ref normSize);
            #else
            #endif
        }

        public static void UpdateAndRemoveAnchors(Transform trans, bool isRecursive)
        {
            Update(trans, isRecursive);
            RemoveAnchors(trans, isRecursive);
        }

        public static void SetToAnchoredPosition(Transform trans, Vector2 anchoredPosition)
        {
            #if NGUI
            SetToAnchoredPositionNGUI(trans, anchoredPosition);
            #else
            #endif
        }

        public static Vector3 GetNormalizedPosition(Vector3 position, Transform root)
        {
            #if NGUI
            return GetNormalizedPositionNGUI(position, root);
            #else
            return  position;
            #endif
        }

        public static void Update(Transform trans, bool isRecursive)
        {
            #if NGUI
            UpdateNGUI(trans, isRecursive);
            #else
            #endif
        }

        public static Vector2 ToPixels(Vector2 clipSpace)
        {
            #if NGUI
            return ToPixelsNGUI(clipSpace);
            #else
            throw new System.NotImplementedException();
            #endif
        }

        public static Vector2 ToClipSpace(Vector2 pixels, GameObject obj = null)
        {
            #if NGUI
            return ToClipSpaceNGUI(pixels, obj);
            #else
            throw new System.NotImplementedException();
            #endif
        }

        public static bool CanBeAnchorParent(GameObject go)
        {
            #if NGUI
            return CanBeAnchorParentNGUI(go);
            #else
            return CanBeAnchorParentNative(go);
            #endif
        }

        static bool CanBeAnchorParentNative(GameObject go)
        {
            return go.GetComponent<RectTransform>() != null;
        }

        public static Vector2 GetCanvasScale()
        {
            #if NGUI
            return GetCanvasScaleNGUI();
            #else
            return GetCanvasScaleNative();
            #endif
        }

        public static bool CanBeAnchored(GameObject go)
        {
            #if NGUI
            return CanBeAnchoredNGUI(go);
            #else
            return CanBeAnchoredNative(go);
            #endif
        }

        static bool CanBeAnchoredNative(GameObject go)
        {
            return go.GetComponent<RectTransform>() != null;
        }

        public static Transform ConvertTransformToRectTransform(Transform trans)
        {
            #if NGUI
            if(trans.GetComponent<UIWidget>() == null)
            {
                trans.gameObject.AddComponent<UIWidget>();
            }
            #else
            if(trans.GetComponent<RectTransform>() == null)
            {
                trans = trans.gameObject.AddComponent<RectTransform>();
            }
            #endif

            return trans;
        }

        // Create a Transform that will be the parent for other child actions
        public static Transform CreateParentTransform(string name = "")
        {
            Transform trans = CreatePivotTransform(name);
            SetStretchedAnchors(trans);
            return trans;
        }

        // Create a Transform that will be used as a pivot in the animation tool
        public static Transform CreatePivotTransform(string name = "")
        {
            #if NGUI
            Log.d("Setting Dimmensions...");

            GameObject go = new GameObject(name);
            UIWidget widget = go.AddComponent<UIWidget>();
            if(widget != null)
            {
                widget.SetDimensions(1, 1);
            }
            return go.transform;
            #else
            GameObject go = new GameObject(name);
            return go.AddComponent<RectTransform>();
            #endif
        }

        public static void SetStretchedAnchors(Transform trans)
        {
            #if NGUI
            #else
            SetStretchedAnchorsNative(trans);
            #endif
        }

        public static Transform GetAnchorParent(Transform trans)
        {
            #if NGUI
            return GetAnchorParentNGUI(trans);
            #else
            return trans.parent;
            #endif
        }

        public static Vector2[] SetAnchors(Transform trans, Transform transParent, AnchorMode anchorsMode, bool isRecursive)
        {
            #if NGUI
            return SetAnchorsNGUI(trans, transParent, anchorsMode, isRecursive);
            #else
            return SetAnchorsNative(trans, anchorsMode);
            #endif
        }

        public static void SetAnchors(Transform trans, Transform transParent, Vector2 anchorMin, Vector2 anchorMax, bool isRecursive)
        {
            #if NGUI
            SetAnchorsNGUI(trans, transParent, anchorMin, anchorMax, isRecursive);
            #else
            SetAnchorsNative(trans, anchorMin, anchorMax);
            #endif
        }

        public static Vector2[] SetAnchors(Transform trans, Vector2 anchorsMin, Vector2 anchorsMax, Vector2 offsetsMin, Vector2 offsetsMax)
        {
            #if NGUI
            return SetAnchorsNGUI(trans, anchorsMin, anchorsMax, offsetsMin, offsetsMax);
            #else
            return SetAnchorsNative(trans, anchorsMin, anchorsMax, offsetsMin, offsetsMax);
            #endif
        }

        public static bool IsFullyAnchored(Transform trans)
        {
            #if NGUI
            return IsFullyAnchoredNGUI(trans);
            #else
            return true;
            #endif
        }

        public static void RemoveAnchors(Transform trans, bool isRecursive)
        {
            #if NGUI
            RemoveAnchorsNGUI(trans, isRecursive);
            #else
            #endif
        }

        public static bool GetAnchors(Transform trans, out Vector2 anchorsMin, out Vector2 anchorsMax, out Vector2 offsetsMin, out Vector2 offsetsMax)
        {
            #if NGUI
            return GetAnchorsNGUI(trans, out anchorsMin, out anchorsMax, out offsetsMin, out offsetsMax);
            #else
            return GetAnchorsNative(trans, out anchorsMin, out anchorsMax, out offsetsMin, out offsetsMax);
            #endif
        }
        //----
        //-- Native UI
        //----

        static Vector2 GetCanvasScaleNative()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            return new Vector2(canvas.scaleFactor, canvas.scaleFactor);
        }

        static void SetStretchedAnchorsNative(Transform trans)
        {
            RectTransform rectTrans = trans.GetComponent<RectTransform>();
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.one;
        }

        static Vector2[] SetAnchorsNative(Transform trans, AnchorMode anchorMode)
        {
            DebugUtils.Assert(anchorMode != AnchorMode.Disabled);

            Vector2 anchorMin = new Vector2(0.5f, 0.5f);
            Vector2 anchorMax = new Vector2(0.5f, 0.5f);

            // Parent Rect Info
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            RectTransform canvasTrans = canvas.GetComponent<RectTransform>();

            Rect parentRect = canvasTrans.rect;
            Transform parentTrans = trans.parent;
            RectTransform parentRectTrans = null;
            if(parentTrans != null)
            {
                parentRectTrans = parentTrans.GetComponentInParent<RectTransform>();
            }
            if(parentRectTrans != null)
            {
                parentRect = parentRectTrans.rect;
            }

            Vector3[] parentWorldCorners = new Vector3[4];
            parentRectTrans.GetWorldCorners(parentWorldCorners);
            Vector2 parentPosMin = new Vector2(parentWorldCorners[0].x, parentWorldCorners[0].y) * canvasTrans.localScale.x;

            Vector2 parentPos = parentRect.position;
            Vector2 parentSize = (new Vector2(parentWorldCorners[2].x, parentWorldCorners[2].y) - new Vector2(parentWorldCorners[0].x, parentWorldCorners[0].y)) * 0.5f * canvasTrans.localScale.x;

            // My Rect Info
            RectTransform rect = trans.GetComponent<RectTransform>();
            Vector2 pos = rect.position;
            Vector3[] rectCorners = new Vector3[4];
            rect.GetWorldCorners(rectCorners);
            Vector2 posMin = new Vector2(rectCorners[0].x, rectCorners[0].y) * canvasTrans.localScale.x;
            Vector2 posMax = new Vector2(rectCorners[2].x, rectCorners[2].y) * canvasTrans.localScale.x;

            Vector2 localPos = pos - parentPos;
            if(anchorMode == AnchorMode.ClosestSide)
            {
                // Horizontal
                if(localPos.x > parentSize.x * 0.33f)
                {
                    anchorMin.x = 1f;
                    anchorMax.x = 1f;
                }
                else if(localPos.x < -parentSize.x * 0.33f)
                {
                    anchorMin.x = 0f;
                    anchorMax.x = 0f;
                }
				
                // Vertical
                if(localPos.y > parentSize.y * 0.33f)
                {
                    anchorMin.y = 1f;
                    anchorMax.y = 1f;
                }
                else if(localPos.y < -parentSize.y * 0.33f)
                {
                    anchorMin.y = 0f;
                    anchorMax.y = 0f;
                }
            }
            else if(anchorMode == AnchorMode.CurrentPosition)
            {
                anchorMin = new Vector2((posMin.x - parentPosMin.x) / (2f * parentSize.x), (posMin.y - parentPosMin.y) / (2f * parentSize.y));
                anchorMax = new Vector2((posMax.x - parentPosMin.x) / (2f * parentSize.x), (posMax.y - parentPosMin.y) / (2f * parentSize.y));
            }

            Vector2 offsetMin = new Vector2(posMin.x - (parentPosMin.x + anchorMin.x * (2f * parentSize.x)), posMin.y - (parentPosMin.y + anchorMin.y * (2f * parentSize.y)));
            Vector2 offsetMax = new Vector2(posMax.x - (parentPosMin.x + anchorMax.x * (2f * parentSize.x)), posMax.y - (parentPosMin.y + anchorMax.y * (2f * parentSize.y)));

            return SetAnchorsNative(trans, anchorMin, anchorMax, offsetMin, offsetMax);
        }

        static Vector2[] SetAnchorsNative(Transform trans, Vector2 anchorMin, Vector2 anchorMax)
        {
            // Parent Rect Info
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            RectTransform canvasTrans = canvas.GetComponent<RectTransform>();

            Transform parentTrans = trans.parent;
            RectTransform parentRectTrans = null;
            if(parentTrans != null)
            {
                parentRectTrans = parentTrans.GetComponentInParent<RectTransform>();
            }

            Vector3[] parentWorldCorners = new Vector3[4];
            parentRectTrans.GetWorldCorners(parentWorldCorners);
            Vector2 parentPosMin = new Vector2(parentWorldCorners[0].x, parentWorldCorners[0].y) * canvasTrans.localScale.x;

            Vector2 parentSize = (new Vector2(parentWorldCorners[2].x, parentWorldCorners[2].y) - new Vector2(parentWorldCorners[0].x, parentWorldCorners[0].y)) * 0.5f * canvasTrans.localScale.x;

            // My Rect Info
            RectTransform rect = trans.GetComponent<RectTransform>();
            Vector3[] rectCorners = new Vector3[4];
            rect.GetWorldCorners(rectCorners);
            Vector2 posMin = new Vector2(rectCorners[0].x, rectCorners[0].y) * canvasTrans.localScale.x;
            Vector2 posMax = new Vector2(rectCorners[2].x, rectCorners[2].y) * canvasTrans.localScale.x;

            Vector2 offsetMin = new Vector2(posMin.x - (parentPosMin.x + anchorMin.x * (2f * parentSize.x)), posMin.y - (parentPosMin.y + anchorMin.y * (2f * parentSize.y)));
            Vector2 offsetMax = new Vector2(posMax.x - (parentPosMin.x + anchorMax.x * (2f * parentSize.x)), posMax.y - (parentPosMin.y + anchorMax.y * (2f * parentSize.y)));

            return SetAnchorsNative(trans, anchorMin, anchorMax, offsetMin, offsetMax);
        }

        public static Vector2[] SetAnchorsNative(Transform trans, Vector2 relativeMin, Vector2 relativeMax, Vector2 absoluteMin, Vector2 absoluteMax)
        {
            RectTransform rectTrans = trans.GetComponent<RectTransform>();
			
            // Change anchors
            rectTrans.anchorMin = relativeMin;
            rectTrans.anchorMax = relativeMax;

            // Reset position and this will recalculate the anchors relative position
            rectTrans.offsetMin = absoluteMin;
            rectTrans.offsetMax = absoluteMax;
			
            return new Vector2[] {
                rectTrans.anchorMin,
                rectTrans.anchorMax,
                rectTrans.offsetMin,
                rectTrans.offsetMax
            };
        }

        static bool GetAnchorsNative(Transform trans, out Vector2 anchorsMin, out Vector2 anchorsMax, out Vector2 offsetsMin, out Vector2 offsetsMax)
        {
            RectTransform rectTrans = trans.GetComponent<RectTransform>();

            // Anchors
            anchorsMin = rectTrans.anchorMin;
            anchorsMax = rectTrans.anchorMax;
			
            // Offsets
            offsetsMin = rectTrans.offsetMin;
            offsetsMax = rectTrans.offsetMax;

            return true;
        }

        #if NGUI
        //----
        //-- NGUI
        //----
        static void UpdateNGUI(Transform trans, bool isRecursive)
        {
            if(isRecursive)
            {
                List<UIRect> rects = new List<UIRect>(trans.GetComponentsInChildren<UIRect>(true));
                for(int i = 0; i < rects.Count; ++i)
                {
                    rects[i].Update();
                }
            }
            else
            {
                UIRect rect = trans.GetComponent<UIRect>();
                if(rect != null)
                {
                    rect.Update();
                }
            }

            UIPanel panel = trans.GetComponentInParent<UIPanel>();
            if(panel != null)
            {
                panel.Refresh();
                panel.Update();
            }
        }

        static bool CanBeAnchorParentNGUI(GameObject go)
        {
            if(go.GetComponent<UIRect>() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static Vector2 GetCanvasScaleNGUI()
        {
            UIRoot uiRoot = GameObject.FindObjectOfType<UIRoot>();
            UICamera uiCamera = uiRoot.GetComponentInChildren<UICamera>();
			
            float camW = (float)uiCamera.cachedCamera.pixelWidth;
            float camH = (float)uiCamera.cachedCamera.pixelHeight;
			
            float ar = (float)camW / camH;
            float w = camW;
            float h = camH;
			
            if(uiRoot.scalingStyle == UIRoot.Scaling.Constrained
                || uiRoot.scalingStyle == UIRoot.Scaling.ConstrainedOnMobiles)
            {
                w = uiRoot.manualWidth;
                h = uiRoot.manualHeight;
				
                if(uiRoot.fitHeight)
                {
                    h = uiRoot.manualHeight;
                    w = h * ar;
                }
                if(uiRoot.fitWidth)
                {
                    w = uiRoot.manualHeight;
                    h = w / ar;
                }
            }

            return new Vector2(w / uiRoot.manualWidth, h / uiRoot.manualHeight);
        }

        public static Vector2 GetCanvasSizeNGUI(GameObject obj = null)
        {
            UIRoot uiRoot;
            if(obj != null)
            {
                uiRoot = obj.FindObjectOfTypeRecursiveUp<UIRoot>();
            }
            else
            {
                uiRoot = GameObject.FindObjectOfType<UIRoot>();
            }
            UICamera uiCamera = uiRoot.GetComponentInChildren<UICamera>();
			
            float camW = (float)uiCamera.cachedCamera.pixelWidth;
            float camH = (float)uiCamera.cachedCamera.pixelHeight;
			
            float ar = (float)camW / camH;
            float w = camW;
            float h = camH;
			
            if(uiRoot.scalingStyle == UIRoot.Scaling.Constrained
                || uiRoot.scalingStyle == UIRoot.Scaling.ConstrainedOnMobiles)
            {
                w = uiRoot.manualWidth;
                h = uiRoot.manualHeight;
				
                if(uiRoot.fitHeight)
                {
                    h = uiRoot.manualHeight;
                    w = h * ar;
                }
                if(uiRoot.fitWidth)
                {
                    w = uiRoot.manualHeight;
                    h = w / ar;
                }
            }

            return new Vector2(w, h);
        }

        static Vector2 GetNormalizedPositionNGUI(Vector3 position, Transform root)
        {
            UIRoot uiRoot = GameObject.FindObjectOfType<UIRoot>();
            UIRect parentGraphic = uiRoot.GetComponent<UIRect>();
            if(root != null)
            {
                parentGraphic = uiRoot.GetComponentInParent<UIRect>();
            }
            Vector3[] parentWC = parentGraphic.worldCorners;
            Vector2 parentHalfSize = new Vector2((parentWC[2].x - parentWC[1].x) * 0.5f, (parentWC[1].y - parentWC[0].y) * 0.5f);
            Vector2 parentPos = new Vector2((parentWC[2].x + parentWC[1].x) * 0.5f, (parentWC[1].y + parentWC[0].y) * 0.5f);
            Vector2 parentPosMin = parentPos - parentHalfSize;

            Vector3 normaPos = new Vector2((position.x - parentPosMin.x) / parentHalfSize.x, (position.y - parentPosMin.y) / parentHalfSize.y) / 2f;
            return normaPos;
        }

        static void SetToAnchoredPositionNGUI(Transform trans, Vector2 newAnchoredPos)
        {
            UIRect graphic = trans.GetComponentInChildren<UIRect>();
            Vector2 anchorPos = new Vector2(graphic.leftAnchor.relative + graphic.rightAnchor.relative, graphic.topAnchor.relative + graphic.bottomAnchor.relative) * 0.5f;
            Vector2 offsetPos = newAnchoredPos - anchorPos;

            graphic.leftAnchor.relative += offsetPos.x;
            graphic.rightAnchor.relative += offsetPos.x;

            graphic.topAnchor.relative += offsetPos.y;
            graphic.bottomAnchor.relative += offsetPos.y;
        }

        static Vector2 ToPixelsNGUI(Vector2 screenDelta)
        {
            Vector2 cs = GetCanvasSizeNGUI();

            screenDelta.x *= cs.y * 0.5f;
            screenDelta.y *= cs.y * 0.5f;

            return screenDelta;
        }

        static Vector2 ToClipSpaceNGUI(Vector2 pixels, GameObject obj = null)
        {
            Vector2 cs = GetCanvasSizeNGUI(obj);
			
            pixels.x /= (cs.y * 0.5f);
            pixels.y /= (cs.y * 0.5f);
			
            return pixels;
        }

        static bool CanBeAnchoredNGUI(GameObject go)
        {
            UIRect graphic = go.GetComponent<UIRect>();
            if(graphic != null && graphic.canBeAnchored)
            {
                return true;
            }

            return false;
        }

        static bool IsFullyAnchoredNGUI(Transform trans)
        {
            UIRect rect = trans.GetComponent<UIRect>();
            return rect.leftAnchor.target != null && rect.rightAnchor.target != null && rect.topAnchor.target != null && rect.bottomAnchor != null;
        }

        static void RemoveAnchorsNGUI(Transform trans, bool isRecursive)
        {
            if(isRecursive)
            {
                List<UIRect> rects = new List<UIRect>(trans.GetComponentsInChildren<UIRect>(true));
                for(int i = 0; i < rects.Count; ++i)
                {
                    if(!rects[i].Anchored)
                    {
                        DoRemoveAnchorsNGUI(rects[i]);
                    }
                }
            }
            else
            {
                UIRect graphic = trans.GetComponent<UIRect>();
                if(graphic != null)
                {
                    DoRemoveAnchorsNGUI(graphic);
                }
            }
        }

        static void DoRemoveAnchorsNGUI(UIRect graphic)
        {
            graphic.leftAnchor.target = null;
            graphic.rightAnchor.target = null;
            graphic.bottomAnchor.target = null;
            graphic.topAnchor.target = null;
        }

        static bool GetAnchorsNGUI(Transform trans, out Vector2 anchorsMin, out Vector2 anchorsMax, out Vector2 offsetsMin, out Vector2 offsetsMax)
        {
            UIRect graphic = trans.GetComponent<UIRect>();
            if(graphic != null)
            {
                DoGetAnchorsNGUI(graphic, out anchorsMin, out anchorsMax, out offsetsMin, out offsetsMax);
                return true;
            }
            anchorsMin = anchorsMax = offsetsMin = offsetsMax = Vector2.zero;
            return false;
        }

        static void DoGetAnchorsNGUI(UIRect graphic, out Vector2 anchorsMin, out Vector2 anchorsMax, out Vector2 offsetsMin, out Vector2 offsetsMax)
        {
            // Anchors
            anchorsMin.x = graphic.leftAnchor.relative;
            anchorsMax.x = graphic.rightAnchor.relative;

            anchorsMin.y = graphic.bottomAnchor.relative;
            anchorsMax.y = graphic.topAnchor.relative;

            // Offsets
            offsetsMin.x = graphic.leftAnchor.absolute;
            offsetsMax.x = graphic.rightAnchor.absolute;
			
            offsetsMin.y = graphic.bottomAnchor.absolute;
            offsetsMax.y = graphic.topAnchor.absolute;
        }

        struct NGUIAnchorData
        {
            public UIRoot uiRoot;
            public UICamera uiCamera;

            public UIRect graphic;
            public UIRect parentGraphic;

            public float w;
            public float h;
            public float ar;

            public Vector2 pos;
            public Vector2 posMin;
            public Vector2 posMax;

            public Vector2 halfSize;

            public Vector2 parentPos;
            public Vector2 parentPosMin;
            public Vector2 parentPosMax;

            public Vector2 parentHalfSize;

            public void Init(Transform trans, Transform transParent)
            {
                uiRoot = GameObject.FindObjectOfType<UIRoot>();
                uiCamera = uiRoot.GetComponentInChildren<UICamera>();

                float camW = (float)uiCamera.cachedCamera.pixelWidth;
                float camH = (float)uiCamera.cachedCamera.pixelHeight;

                ar = (float)camW / camH;
                w = camW;
                h = camH;

                if(uiRoot.scalingStyle == UIRoot.Scaling.Constrained
                    || uiRoot.scalingStyle == UIRoot.Scaling.ConstrainedOnMobiles)
                {
                    w = uiRoot.manualWidth;
                    h = uiRoot.manualHeight;

                    if(uiRoot.fitHeight)
                    {
                        h = uiRoot.manualHeight;
                        w = h * ar;
                    }
                    if(uiRoot.fitWidth)
                    {
                        w = uiRoot.manualHeight;
                        h = w / ar;
                    }
                }
				
                graphic = trans.GetComponent<UIRect>();
                Vector3[] wc = graphic.worldCorners;
                halfSize = new Vector2((wc[2].x - wc[1].x) * 0.5f, (wc[1].y - wc[0].y) * 0.5f);
                pos = new Vector2((wc[2].x + wc[1].x) * 0.5f, (wc[1].y + wc[0].y) * 0.5f);
                posMin = pos - halfSize;
                posMax = pos + halfSize;

                parentHalfSize = new Vector2(ar, 1f);
                parentPos = Vector2.zero;
                parentGraphic = uiRoot.GetComponentInChildren<UIRect>();
                if(transParent != null)
                {
                    parentGraphic = transParent.GetComponentInParent<UIRect>();
                    Vector3[] parentWC = parentGraphic.worldCorners;
                    parentHalfSize = new Vector2((parentWC[2].x - parentWC[1].x) * 0.5f, (parentWC[1].y - parentWC[0].y) * 0.5f);
                    parentPos = new Vector2((parentWC[2].x + parentWC[1].x) * 0.5f, (parentWC[1].y + parentWC[0].y) * 0.5f);
                }

                parentPosMin = parentPos - parentHalfSize;
                parentPosMax = parentPos + parentHalfSize;
            }

            public bool HasRect(Transform trans)
            {
                return trans.GetComponent<UIRect>() != null;
            }
        }

        static Transform GetAnchorParentNGUI(Transform trans)
        {
            UIRect graphic = trans.GetComponent<UIRect>();
            if(graphic != null)
            {
                if(graphic.leftAnchor != null)
                    return graphic.leftAnchor.target;
                if(graphic.rightAnchor != null)
                    return graphic.rightAnchor.target;
                if(graphic.topAnchor != null)
                    return graphic.topAnchor.target;
                if(graphic.bottomAnchor != null)
                    return graphic.bottomAnchor.target;
            }
            return null;
        }

        static Vector2[] SetAnchorsNGUI(Transform trans, Transform transParent, AnchorMode anchorMode, bool isResursive)
        {
            DebugUtils.Assert(anchorMode != AnchorMode.Disabled);

            Vector2 anchorMin = new Vector2(0.5f, 0.5f);
            Vector2 anchorMax = new Vector2(0.5f, 0.5f);

            NGUIAnchorData anchorData = new NGUIAnchorData();
            bool hasRect = anchorData.HasRect(trans);

            if(hasRect)
            {
                anchorData.Init(trans, transParent);

                if(anchorMode == AnchorMode.CurrentPosition)
                {
                    anchorMin = new Vector2((anchorData.posMin.x - anchorData.parentPosMin.x) / (2f * anchorData.parentHalfSize.x), (anchorData.posMin.y - anchorData.parentPosMin.y) / (2f * anchorData.parentHalfSize.y));
                    anchorMax = new Vector2((anchorData.posMax.x - anchorData.parentPosMin.x) / (2f * anchorData.parentHalfSize.x), (anchorData.posMax.y - anchorData.parentPosMin.y) / (2f * anchorData.parentHalfSize.y));
                }
                else if(anchorMode == AnchorMode.Custom)
                {
                    // Setup the Min X
                    if(Mathf.Abs(anchorData.posMin.x - anchorData.parentPosMin.x) < Mathf.Abs(anchorData.posMin.x - anchorData.parentPos.x))
                    {
                        anchorMin.x = 0f;
                    }
                    if(Mathf.Abs(anchorData.posMin.x - anchorData.parentPosMax.x) < Mathf.Abs(anchorData.posMin.x - anchorData.parentPosMin.x))
                    {
                        anchorMin.x = 1f;
                    }
					
                    // Setup the Min Y
                    if(Mathf.Abs(anchorData.posMin.y - anchorData.parentPosMin.y) < Mathf.Abs(anchorData.posMin.y - anchorData.parentPos.y))
                    {
                        anchorMin.y = 0f;
                    }
                    if(Mathf.Abs(anchorData.posMin.y - anchorData.parentPosMax.y) < Mathf.Abs(anchorData.posMin.y - anchorData.parentPosMin.y))
                    {
                        anchorMin.y = 1f;
                    }


                    // Setup the Max X
                    if(Mathf.Abs(anchorData.posMax.x - anchorData.parentPosMin.x) < Mathf.Abs(anchorData.posMax.x - anchorData.parentPos.x))
                    {
                        anchorMax.x = 0f;
                    }
                    if(Mathf.Abs(anchorData.posMax.x - anchorData.parentPosMax.x) < Mathf.Abs(anchorData.posMax.x - anchorData.parentPosMin.x))
                    {
                        anchorMax.x = 1f;
                    }
					
                    // Setup the Max Y
                    if(Mathf.Abs(anchorData.posMax.y - anchorData.parentPosMin.y) < Mathf.Abs(anchorData.posMax.y - anchorData.parentPos.y))
                    {
                        anchorMax.y = 0f;
                    }
                    if(Mathf.Abs(anchorData.posMax.y - anchorData.parentPosMax.y) < Mathf.Abs(anchorData.posMax.y - anchorData.parentPosMin.y))
                    {
                        anchorMax.y = 1f;
                    }
                }
                else if(anchorMode == AnchorMode.ClosestSide)
                {
                    // Setup the Min X
                    if(Mathf.Abs(anchorData.pos.x - anchorData.parentPosMin.x) < Mathf.Abs(anchorData.pos.x - anchorData.parentPos.x))
                    {
                        anchorMin.x = 0f;
                    }
                    if(Mathf.Abs(anchorData.pos.x - anchorData.parentPosMax.x) < Mathf.Abs(anchorData.pos.x - anchorData.parentPosMin.x))
                    {
                        anchorMin.x = 1f;
                    }

                    // Setup the Min Y
                    if(Mathf.Abs(anchorData.pos.y - anchorData.parentPosMin.y) < Mathf.Abs(anchorData.pos.y - anchorData.parentPos.y))
                    {
                        anchorMin.y = 0f;
                    }
                    if(Mathf.Abs(anchorData.pos.y - anchorData.parentPosMax.y) < Mathf.Abs(anchorData.pos.y - anchorData.parentPosMin.y))
                    {
                        anchorMin.y = 1f;
                    }

                    // Setup the Max X and Max Y equals as AnchorMin
                    anchorMax = anchorMin;
                }

                return DoSetAnchorsNGUI(trans, transParent, anchorMin, anchorMax);
            }

            if(isResursive)
            {
                for(int i = 0; i < trans.childCount; ++i)
                {
                    SetAnchorsNGUI(trans.GetChild(i), trans, anchorMode, isResursive);
                }
            }

            return new Vector2[] {
                anchorMin,
                anchorMax,
                new Vector2(anchorData.graphic.leftAnchor.absolute, anchorData.graphic.rightAnchor.absolute),
                new Vector2(anchorData.graphic.bottomAnchor.absolute, anchorData.graphic.topAnchor.absolute)
            };
        }

        // Note: It is recomended to Refresh the parent panel of the graphic to avoid weird things after changing the anchor
        static void SetAnchorsNGUI(Transform trans, Transform transParent, Vector2 anchorMin, Vector2 anchorMax, bool isResursive)
        {
            NGUIAnchorData anchorData = new NGUIAnchorData();
            anchorData.Init(trans, transParent);

            bool hasRect = anchorData.HasRect(trans);
            if(hasRect)
            {
                DoSetAnchorsNGUI(trans, transParent, anchorMin, anchorMax);
            }

            if(isResursive)
            {
                for(int i = 0; i < trans.childCount; ++i)
                {
                    SetAnchorsNGUI(trans.GetChild(i), trans, anchorMin, anchorMax, isResursive);
                }
            }
        }

        public static Vector2[] SetAnchorsNGUI(Transform trans, Vector2 relativeMin, Vector2 relativeMax, Vector2 absoluteMin, Vector2 absoluteMax)
        {
            UIRect graphic = trans.GetComponentInChildren<UIRect>();

            graphic.leftAnchor.Set(relativeMin.x, absoluteMin.x);
            graphic.rightAnchor.Set(relativeMax.x, absoluteMax.x);
            graphic.bottomAnchor.Set(relativeMin.y, absoluteMin.y);
            graphic.topAnchor.Set(relativeMax.y, absoluteMax.y);

            return new Vector2[] { relativeMin, relativeMax, absoluteMin, absoluteMax };
        }

        static Vector2[] DoSetAnchorsNGUI(Transform trans, Transform transParent, Vector2 anchorMin, Vector2 anchorMax)
        {
            NGUIAnchorData anchorData = new NGUIAnchorData();
            anchorData.Init(trans, transParent);
			
            // Set the position relative to the anchorPos
            Vector2 anchorPosMin = new Vector2(Mathf.LerpUnclamped(anchorData.parentPosMin.x, anchorData.parentPosMax.x, anchorMin.x),
                                       Mathf.LerpUnclamped(anchorData.parentPosMin.y, anchorData.parentPosMax.y, anchorMin.y));
			
            Vector2 anchorPosMax = new Vector2(Mathf.LerpUnclamped(anchorData.parentPosMin.x, anchorData.parentPosMax.x, anchorMax.x),
                                       Mathf.LerpUnclamped(anchorData.parentPosMin.y, anchorData.parentPosMax.y, anchorMax.y));
			
            Vector2 anchoredPosMin = anchorData.posMin - anchorPosMin;
            Vector2 anchoredPosMax = anchorData.posMax - anchorPosMax;
			
            Vector2 anchoredPosAbsMin = new Vector2(anchoredPosMin.x * anchorData.h * 0.5f, anchoredPosMin.y * anchorData.h * 0.5f);
            Vector2 anchoredPosAbsMax = new Vector2(anchoredPosMax.x * anchorData.h * 0.5f, anchoredPosMax.y * anchorData.h * 0.5f);
			
            // Set Anchors
            anchorData.graphic.leftAnchor.Set(anchorData.parentGraphic.transform, anchorMin.x, anchoredPosAbsMin.x);
            anchorData.graphic.rightAnchor.Set(anchorData.parentGraphic.transform, anchorMax.x, anchoredPosAbsMax.x);
            anchorData.graphic.bottomAnchor.Set(anchorData.parentGraphic.transform, anchorMin.y, anchoredPosAbsMin.y);
            anchorData.graphic.topAnchor.Set(anchorData.parentGraphic.transform, anchorMax.y, anchoredPosAbsMax.y);

            return new Vector2[] {
                anchorMin,
                anchorMax,
                anchoredPosAbsMin,
                anchoredPosAbsMax
            };
        }

        static void GetNormalizedPositionSizeNGUI(Transform trans, ref Vector2 normPos, ref Vector2 normSize)
        {
            UIRoot uiRoot = GameObject.FindObjectOfType<UIRoot>();
            UIRect parentGraphic = uiRoot.GetComponent<UIRect>();
            Vector3[] parentWC = parentGraphic.worldCorners;
            Vector2 parentHalfSize = new Vector2((parentWC[2].x - parentWC[1].x) * 0.5f, (parentWC[1].y - parentWC[0].y) * 0.5f);
            Vector2 parentPos = new Vector2((parentWC[2].x + parentWC[1].x) * 0.5f, (parentWC[1].y + parentWC[0].y) * 0.5f);
            Vector2 parentPosMin = parentPos - parentHalfSize;

            UIRect graphic = trans.GetComponentInChildren<UIRect>();
            Vector3[] wc = graphic.worldCorners;
            Vector2 halfSize = new Vector2((wc[2].x - wc[1].x) * 0.5f, (wc[1].y - wc[0].y) * 0.5f);
            Vector2 pos = new Vector2((wc[2].x + wc[1].x) * 0.5f, (wc[1].y + wc[0].y) * 0.5f);
            Vector2 posMin = pos - halfSize;
            Vector2 posMax = pos + halfSize;

            // Normalized positions
            Vector2 anchorMin = new Vector2((posMin.x - parentPosMin.x) / (2f * parentHalfSize.x), (posMin.y - parentPosMin.y) / (2f * parentHalfSize.y));
            Vector2 anchorMax = new Vector2((posMax.x - parentPosMin.x) / (2f * parentHalfSize.x), (posMax.y - parentPosMin.y) / (2f * parentHalfSize.y));

            normPos = (anchorMax + anchorMin) * 0.5f;
            normSize = (anchorMax - anchorMin) * 0.5f;
        }

        #endif
    }
}
