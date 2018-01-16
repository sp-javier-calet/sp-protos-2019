using UnityEngine;

namespace SocialPoint.Base
{
    public static class RectTransformExtensions
    {
        public static readonly Vector2 PivotTopLeft = new Vector2(0f, 1f);
        public static readonly Vector2 PivotTopCenter = new Vector2(0.5f, 1f);
        public static readonly Vector2 PivotTopRight = new Vector2(1f, 1f);

        public static readonly Vector2 PivotMiddleLeft = new Vector2(0f, 0.5f);
        public static readonly Vector2 PivotMiddleCenter = new Vector2(0.5f, 0.5f);
        public static readonly Vector2 PivotMiddleRight = new Vector2(1f, 0.5f);

        public static readonly Vector2 PivotBottomLeft = new Vector2(0f, 0f);
        public static readonly Vector2 PivotBottomCenter = new Vector2(0.5f, 0f);
        public static readonly Vector2 PivotBottomRight = new Vector2(1f, 0f);

        public static void SetDefaultScale(this RectTransform trans)
        {
            trans.localScale = new Vector3(1, 1, 1);
        }

        public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec)
        {
            trans.pivot = aVec;
            trans.anchorMin = aVec;
            trans.anchorMax = aVec;
        }

        public static void SetAndAnchors(this RectTransform trans, Vector2 aVec)
        {
            trans.anchorMin = aVec;
            trans.anchorMax = aVec;
        }
            
        public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
        }

        public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }

        public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }

        public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
        }

        public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
        }

        public static void SetSize(this RectTransform trans, Vector2 newSize)
        {
            Vector2 oldSize = trans.rect.size;
            Vector2 deltaSize = newSize - oldSize;
            trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
        }

        public static void SetWidth(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(newSize, trans.rect.size.y));
        }

        public static void SetHeight(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(trans.rect.size.x, newSize));
        }

        public static Vector3 GetWorldCenterPoint(this RectTransform trans) 
        {
            var objectCorners = new Vector3[4];
            trans.GetWorldCorners(objectCorners);

            var bottomLeft = objectCorners[0];
            var topLeft = objectCorners[1];
            var bottomRight = objectCorners[3];

            var newCenterX = bottomLeft.x + (bottomRight.x - bottomLeft.x) * 0.5f;
            var newCenterY = bottomLeft.y + (topLeft.y - bottomLeft.y) * 0.5f;
            var newCenterZ = bottomLeft.z;

            return new Vector3(newCenterX, newCenterY, newCenterZ);
        }

        public static Rect GetWorldRect(this RectTransform trans, Vector2 scale) 
        {
            var objectCorners = new Vector3[4];
            trans.GetWorldCorners(objectCorners);

            var bottomLeft = objectCorners[0];

            // Rescale the size appropiately based on the current Canvas scale
            var scaledSize = new Vector2(scale.x * trans.rect.size.x, scale.y * trans.rect.size.y);

            return new Rect(bottomLeft, scaledSize);
        }

        public static Rect GetScreenRect(this RectTransform trans, Camera camera)
        {
            var objectCorners = new Vector3[4];
            trans.GetWorldCorners(objectCorners);

            var bottomLeft = camera.WorldToScreenPoint(objectCorners[0]);
            var size = new Vector2(camera.WorldToScreenPoint(objectCorners[3]).x - bottomLeft.x, camera.WorldToScreenPoint(objectCorners[1]).y - bottomLeft.y);

            return new Rect(bottomLeft, size);
        }
 
        static int CountCornersInsideBounds(this RectTransform trans, Camera camera, Rect boundsRect)
        {
            var objectCorners = new Vector3[4];
            trans.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            Vector3 tempScreenSpaceCorner;
            for(var i = 0; i < objectCorners.Length; i++)
            {
                // Transform world space position of corners to screen space and check if they are contained in boundsRect
                tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); 
                if(boundsRect.Contains(tempScreenSpaceCorner))
                {
                    visibleCorners++;
                }
            }

            return visibleCorners;
        }

        public static bool IsOutOfBounds(this RectTransform trans, Camera camera, Rect boundsRect)
        {
            return CountCornersInsideBounds(trans, camera, boundsRect) < 4; 
        }

        public static bool IsTotallyOutOfBounds(this RectTransform trans, Camera camera, Rect boundsRect)
        {
            return CountCornersInsideBounds(trans, camera, boundsRect) == 0; 
        }

        public static bool IsOutOfScreenBounds(this RectTransform trans, Camera camera)
        {
            var screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
            return CountCornersInsideBounds(trans, camera, screenBounds) < 4; 
        }

        public static bool IsTotallyOutOfScreenBounds(this RectTransform trans, Camera camera)
        {
            var screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
            return CountCornersInsideBounds(trans, camera, screenBounds) == 0; 
        }
    }
}