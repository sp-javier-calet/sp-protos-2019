using PathVector = SharpNav.Geometry.Vector3;
using MultiplayerVector = Jitter.LinearMath.JVector;

namespace SocialPoint.Pathfinding
{
    public static class MultiplayerModelExtensions
    {
        public static MultiplayerVector ToMultiplayer(this PathVector v)
        {
            return new MultiplayerVector(v.X, v.Y, v.Z);
        }

        public static PathVector ToPathfinding(this MultiplayerVector v)
        {
            return new PathVector(v.X, v.Y, v.Z);
        }
    }
}
