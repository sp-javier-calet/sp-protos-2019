
namespace Jitter.LinearMath
{
    public static class JBBoxExtensions
    {
        public static float GetDistanceSquaredToPointIgnoreY(this JBBox box, JVector point)
        {
            // Remove Y component
            JVector rectMin = box.Min.ZeroYValue();
            JVector rectMax = box.Max.ZeroYValue();

            //  Calculate a distance between a point and a rectangle.
            //  The area around/in the rectangle is defined in terms of
            //  several regions:
            //
            //        I   |    II    |  III
            //      ======+==========+======   --zMin
            //       VIII |  IX (in) |  IV
            //      ======+==========+======   --zMax
            //       VII  |    VI    |   V

            if(point.X < rectMin.X)
            { // Region I, VIII, or VII
                if(point.Z < rectMin.Z)
                { // I
                    JVector diff = point - new JVector(rectMin.X, 0f, rectMin.Z);
                    return diff.LengthSquared();
                }
                else if(point.Z > rectMax.Z)
                { // VII
                    JVector diff = point - new JVector(rectMin.X, 0f, rectMax.Z);
                    return diff.LengthSquared();
                }
                else
                { // VIII
                    float diff = rectMin.X - point.X;
                    return diff * diff;
                }
            }
            else if(point.X > rectMax.X)
            { // Region III, IV, or V
                if(point.Z < rectMin.Z)
                { // III
                    JVector diff = point - new JVector(rectMax.X, 0f, rectMin.Z);
                    return diff.LengthSquared();
                }
                else if(point.Z > rectMax.Z)
                { // V
                    JVector diff = point - new JVector(rectMax.X, 0f, rectMax.Z);
                    return diff.LengthSquared();
                }
                else
                { // IV
                    float diff = point.X - rectMax.X;
                    return diff * diff;
                }
            }
            else
            { // Region II, IX, or VI
                if(point.Z < rectMin.Z)
                { // II
                    float diff = rectMin.Z - point.Z;
                    return diff * diff;
                }
                else if(point.Z > rectMax.Z)
                { // VI
                    float diff = point.Z - rectMax.Z;
                    return diff * diff;
                }
                else
                { // IX
                    return 0f;
                }
            }
        }
    }
}