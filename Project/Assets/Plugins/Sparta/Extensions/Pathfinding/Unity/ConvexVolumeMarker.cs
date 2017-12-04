using UnityEngine;
using System.Collections;
using SharpNav;
using SocialPoint.Utils;
using SocialPoint.Exporter;
using ConvexVolume = SharpNav.Geometry.ConvexVolume;

namespace SocialPoint.Pathfinding
{
    [RequireComponent(typeof(ExportConfiguration))]
    public class ConvexVolumeMarker : MonoBehaviour
    {
        public class ExportData
        {
            public SharpNav.Geometry.Vector3[] Vertices;

            public float Hmin;

            public float Hmax;

            public string Area;

            public TagSet Tag;
        }

        /// <summary>
        /// Gizmo color. It will change alpha value if volume is not convex.
        /// </summary>
        public Color Color = Color.red;

        /// <summary>
        /// Height of the volume. Measured from the lowest vertex.
        /// </summary>
        public float Height = 2;

        Vector3[] _vertices;
        float _min;
        float _max;

        ExportConfiguration _exportData;

        // Unity function
        void OnDrawGizmos()
        {
            CalculateData();
            bool valid = ValidateData();
            Draw(valid);
        }

        public ExportData GetExportData()
        {
            CalculateData();
            bool valid = ValidateData();
            if(valid)
            {
                var tags = (_exportData != null) ? _exportData.Tags : new TagSet();
                var cv = new ExportData {
                    Vertices = PathfindingUnityUtils.UnityVectorsToPathfinding(_vertices),
                    Hmin = _min,
                    Hmax = _max,
                    Area = (tags.Count > 0) ? tags.ToArray()[0] : string.Empty,
                    Tag = tags,
                };
                return cv;
            }
            return null;
        }

        void CalculateData()
        {
            int verticesAmount = transform.childCount;
            if(verticesAmount == 0)
            {
                return;
            }

            _exportData = GetComponent<ExportConfiguration>();

            var transformVertices = new Vector3[verticesAmount];
            var firstVertex = transform.GetChild(0).position;
            _min = firstVertex.y;
            for(int i = 0; i < verticesAmount; i++)
            {
                transformVertices[i] = transform.GetChild(i).position;
                if(transformVertices[i].y < _min)
                {
                    _min = transformVertices[i].y;
                }
            }
            _max = _min + Height;

            //From transform vertices, get those that create a convex hull
            int[] outVertices;
            int convexVertices = ConvexHull(transformVertices, out outVertices);
            _vertices = new Vector3[convexVertices];
            for(int i = 0; i < convexVertices; i++)
            {
                _vertices[i] = transformVertices[outVertices[i]];
            }
        }

        bool ValidateData()
        {
            return _vertices != null && _vertices.Length >= 3;
        }

        void Draw(bool valid)
        {
            if(valid)
            {
                Color.a = 1.0f;
            }
            else
            {
                Color.a = 0.3f;
            }

            for(int i = 0; i < _vertices.Length; i++)
            {
                int nextIndex = (i + 1) % _vertices.Length;
                Vector3 c = _vertices[i];
                Vector3 n = _vertices[nextIndex];
                Vector3 ctop = new Vector3(c.x, _max, c.z);
                Vector3 cbot = new Vector3(c.x, _min, c.z);
                Vector3 ntop = new Vector3(n.x, _max, n.z);
                Vector3 nbot = new Vector3(n.x, _min, n.z);

                float radius = 0.1f;
                Color prevColor = Gizmos.color;
                Gizmos.color = Color;
                Gizmos.DrawWireSphere(ctop, radius);
                Gizmos.DrawWireSphere(cbot, radius);
                Gizmos.DrawLine(cbot, ctop);
                Gizmos.DrawLine(cbot, nbot);
                Gizmos.DrawLine(ctop, ntop);
                Gizmos.color = prevColor;
            }
        }

        /// <summary>
        /// From Recast/Detour cpp code:
        /// Returns true if 'c' is left of line 'a'-'b'.
        /// </summary>
        bool Left(Vector3 a, Vector3 b, Vector3 c)
        { 
            float u1 = b.x - a.x;
            float v1 = b.z - a.z;
            float u2 = c.x - a.x;
            float v2 = c.z - a.z;
            return u1 * v2 - v1 * u2 < 0;
        }

        /// <summary>
        /// From Recast/Detour cpp code:
        /// Returns true if 'a' is more lower-left than 'b'.
        /// </summary>
        bool Cmppt(Vector3 a, Vector3 b)
        {
            if(a.x < b.x)
                return true;
            if(a.x > b.x)
                return false;
            if(a.z < b.z)
                return true;
            if(a.z > b.z)
                return false;
            return false;
        }


        /// <summary>
        /// From Recast/Detour cpp code:
        /// Calculates convex hull on xz-plane of points on 'pts',
        /// stores the indices of the resulting hull in 'out' and
        /// returns number of points on hull.
        /// </summary>
        int ConvexHull(Vector3[] pts, out int[] outPts)
        {
            outPts = new int[pts.Length];
            int i = 0;

            // Find lower-leftmost point.
            int hull = 0;
            for(i = 1; i < pts.Length; ++i)
            {
                if(Cmppt(pts[i], pts[hull]))
                {
                    hull = i;
                }
            }

            // Gift wrap hull.
            int endpt = 0;
            i = 0;
            do
            {
                outPts[i++] = hull;
                endpt = 0;
                for(int j = 1; j < pts.Length; ++j)
                {
                    if(hull == endpt || Left(pts[hull], pts[endpt], pts[j]))
                    {
                        endpt = j;
                    }
                }
                hull = endpt;
            }
            while (endpt != outPts[0]);

            return i;
        }
    }
}
