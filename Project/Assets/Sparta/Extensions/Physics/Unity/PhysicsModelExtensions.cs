using UnityVector = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;
using PhysicsVector = Jitter.LinearMath.JVector;
using PhysicsQuaternion = Jitter.LinearMath.JQuaternion;
using UnityEngine;
using System.Collections.Generic;
using Jitter.LinearMath;
using Jitter.Collision;

namespace SocialPoint.Physics
{
    public static class PhysicsModelExtensions
    {
        public static UnityVector ToUnity(this PhysicsVector v)
        {
            return new UnityVector(v.X, v.Y, v.Z);
        }

        public static UnityQuaternion ToUnity(this PhysicsQuaternion q)
        {
            return new UnityQuaternion(q.X, q.Y, q.Z, q.W);
        }


        public static PhysicsVector ToPhysics(this UnityVector v)
        {
            return new PhysicsVector(v.x, v.y, v.z);
        }

        public static PhysicsQuaternion ToPhysics(this UnityQuaternion q)
        {
            return new PhysicsQuaternion(q.x, q.y, q.z, q.w);
        }

        public static IPhysicsShape ToPhysics(this Collider collider)
        {
            var box = collider as BoxCollider;
            if(box != null)
            {
                return new PhysicsBoxShape(box.size.ToPhysics());
            }
            var sphere = collider as SphereCollider;
            if(sphere != null)
            {
                return new PhysicsSphereShape(sphere.radius);
            }
            var capsule = collider as CapsuleCollider;
            if(capsule != null)
            {
                var radius = capsule.radius;
                var height = capsule.height - (radius * 2);
                return new PhysicsCapsuleShape(height, radius);
            }
            var mesh = collider as MeshCollider;
            if(mesh != null)
            {
                var triangles = mesh.sharedMesh.triangles;
                var indices = new List<TriangleVertexIndices>();
                for(int i = 0; i < triangles.Length; i += 3)
                {
                    indices.Add(new TriangleVertexIndices(triangles[i + 2], triangles[i + 1], triangles[i + 0]));
                }
                var scale = mesh.gameObject.transform.localScale;
                var vertices = new List<JVector>();
                var meshVertices = mesh.sharedMesh.vertices;
                foreach(var vertex in meshVertices)
                {
                    vertices.Add(new JVector(vertex.x * scale.x, vertex.y * scale.y, vertex.z * scale.z));
                }
                return new PhysicsMeshShape(vertices, indices);
            }
            throw new System.InvalidOperationException("Could not convert the Unity collider.");
        }
    }
}
