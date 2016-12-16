using UnityVector = UnityEngine.Vector3;
using PathVector = SharpNav.Geometry.Vector3;
using PathPoint = SharpNav.Pathfinding.NavPoint;

namespace SocialPoint.Geometry
{
    // PathVector adapter
    public partial struct Vector
    {
        public static Vector Convert(PathVector v)
        {
            return v;
        }

        public static implicit operator Vector(PathVector v)
        {
            return new Vector(v.X, v.Y, v.Z);
        }

        public static implicit operator PathVector(Vector v)
        {
            return new PathVector(v.X, v.Y, v.Z);
        }

        public static Vector Convert(PathPoint v)
        {
            return v;
        }

        public static implicit operator Vector(PathPoint v)
        {
            return v.Position;
        }
    }
}
