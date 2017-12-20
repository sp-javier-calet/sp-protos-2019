using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public static class Snapper
    {
        public static float kMaxVerticalDistance = 350f;

        public struct ResultData
        {
            public Rect Rect;
            public Vector2 Pos;
            public float Dist;
        }

        public static bool Snap(ref ResultData result, List<Rect> rects, Vector2 position, float snapDistance)
        {
            var closestRectsX = new List<ResultData>();
            var closestRectsY = new List<ResultData>();
            var intersection = new List<ResultData>();

            FindClosestRects(closestRectsX, rects, position, snapDistance, Vector2.right);
            FindClosestRects(closestRectsY, rects, position, kMaxVerticalDistance, Vector2.up);
            Intersect(intersection, closestRectsX, closestRectsY);
            Order(intersection);

            if(intersection.Count > 0)
            {
                result = intersection[0];
                return true;
            }
            return false;
        }

        static void FindClosestRects(List<ResultData> closestRects, List<Rect> rects, Vector2 position, float snapDistance, Vector2 comparisonDir)
        {
            for(int i = 0; i < rects.Count; ++i)
            {
                float distMin = Mathf.Abs(rects[i].position.x - position.x) * comparisonDir.x + Mathf.Abs(rects[i].position.y - position.y) * comparisonDir.y;

                if(distMin < snapDistance)
                {
                    var snappedPos = new Vector2(
                                         position.x * (1f - comparisonDir.x) + rects[i].position.x * comparisonDir.x,
                                         position.y * (1f - comparisonDir.y) + rects[i].position.y * comparisonDir.y
                                     );
					
                    closestRects.Add(new ResultData { Pos = snappedPos, Dist = distMin, Rect = rects[i] });
                }

                float distMax = Mathf.Abs((rects[i].position.x + rects[i].size.x) - position.x) * comparisonDir.x + Mathf.Abs((rects[i].position.y + rects[i].size.y) - position.y) * comparisonDir.y;
                if(distMax < snapDistance && distMax < distMin)
                {
                    var snappedPos = new Vector2(
                                         position.x * (1f - comparisonDir.x) + (rects[i].position.x + rects[i].size.x) * comparisonDir.x,
                                         position.y * (1f - comparisonDir.y) + (rects[i].position.y + +rects[i].size.y) * comparisonDir.y
                                     );

                    closestRects.Add(new ResultData { Pos = snappedPos, Dist = distMax, Rect = rects[i] });
                }
            }
        }

        static void Intersect(List<ResultData> result, List<ResultData> a, List<ResultData> b)
        {
            for(int i = 0; i < a.Count; ++i)
            {
                int foundIdx = b.FindIndex(test => (test.Rect.position - a[i].Rect.position).sqrMagnitude < 1e-3f);
                if(foundIdx >= 0)
                {
                    result.Add(new ResultData {
                        Pos = a[i].Pos,
                        Dist = a[i].Dist + b[foundIdx].Dist,
                        Rect = a[i].Rect
                    });
                }
            }
        }

        static void Order(List<ResultData> data)
        {
            data.Sort((a, b) => a.Dist < b.Dist ? -1 : a.Dist > b.Dist ? 1 : 0);
        }
    }
}
