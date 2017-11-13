using SocialPoint.Base;

namespace Jitter.LinearMath
{
    public static class JBBoxExtensions
    {
        public static JVector GetClosestPointIgnoreY(this JBBox box, JVector point)
        {
            point.Y = 0f;

            var containsCheck = box.Contains(point);
            if(containsCheck == JBBox.ContainmentType.Contains || containsCheck == JBBox.ContainmentType.Intersects)
            {
                return point;
            }
            else
            {
                // Remove Y component
                JVector rectMin = box.Min.ZeroYValue();
                JVector rectMax = box.Max.ZeroYValue();
                JVector rectCenter = (rectMin + rectMax) * 0.5f;

                // Check intersection between every rectangle segment and the segment between the point and the rectangle center
                JVector intersection;
                if(SegmentSegment2DIntersect(rectMin.X, rectMin.Z, rectMax.X, rectMin.Z, point.X, point.Z, rectCenter.X, rectCenter.Z, out intersection))
                {
                    return intersection;
                }
                if(SegmentSegment2DIntersect(rectMax.X, rectMin.Z, rectMax.X, rectMax.Z, point.X, point.Z, rectCenter.X, rectCenter.Z, out intersection))
                {
                    return intersection;
                }
                if(SegmentSegment2DIntersect(rectMax.X, rectMax.Z, rectMin.X, rectMax.Z, point.X, point.Z, rectCenter.X, rectCenter.Z, out intersection))
                {
                    return intersection;
                }
                if(SegmentSegment2DIntersect(rectMin.X, rectMax.Z, rectMin.X, rectMin.Z, point.X, point.Z, rectCenter.X, rectCenter.Z, out intersection))
                {
                    return intersection;
                }

                DebugUtils.Assert(false, "We should never get here, segment between external point and center of the rectangle must intersect one of the sides");
                return JVector.Zero;
            }
        }

        static bool SegmentSegment2DIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, out JVector result)
        {
            float a1 = y2 - y1;
            float b1 = x1 - x2;
            float c1 = a1 * x1 + b1 * y1;

            float a2 = y4 - y3;
            float b2 = x3 - x4;
            float c2 = a2 * x3 + b2 * y3;

            float det = a1 * b2 - a2 * b1;
            float rx = (b2 * c1 - b1 * c2) / det;
            float ry = (a1 * c2 - a2 * c1) / det;

            result = new JVector(rx, 0f, ry);

            return (rx >= x1 && rx <= x2 || rx >= x2 && rx <= x1)
            && (rx >= x3 && rx <= x4 || rx >= x4 && rx <= x3)
            && (ry >= y1 && ry <= y2 || ry >= y2 && ry <= y1)
            && (ry >= y3 && ry <= y4 || ry >= y4 && ry <= y3);
        }
    }
}