using UnityVector = UnityEngine.Vector3;
using PathVector = SharpNav.Geometry.Vector3;
using SocialPoint.Attributes;

namespace SocialPoint.Pathfinding
{
    public static class UnityModelExtensions
    {
        public static UnityVector ToUnity(this PathVector v)
        {
            return new UnityVector(v.X, v.Y, v.Z);
        }

        public static PathVector ToPathfinding(this UnityVector v)
        {
            return new PathVector(v.x, v.y, v.z);
        }
    }
}
