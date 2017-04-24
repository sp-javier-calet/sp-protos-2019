using System.Collections;

namespace SharpNav.Geometry
{
    public class ConvexVolume
    {
        //[SP-Change] Added this class to support area marking upon navmesh creation (copied from cpp code)

        public Vector3[] Vertices;
        public float Hmin;
        public float Hmax;
        public Area Area;
        public object Tag;
    }
}
