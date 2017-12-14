using System.Collections.Generic;
using SocialPoint.Multiplayer;
using Jitter.LinearMath;

public static class SceneColliderExtensions
{
    public static List<SceneCollider> GetCollidersWithTags(this List<SceneCollider> colliders, params string[] tags)
    {
        List<SceneCollider> res = new List<SceneCollider>();

        for(int i = 0; i < colliders.Count; ++i)
        {
            SceneCollider collider = colliders[i];
            if (collider.Tags.MatchAll(tags))
            {
                res.Add(collider);
            }
        }

        return res;
    }

    public static JBBox ComputeBoundingBox(this SceneCollider collider)
    {
        JVector center = collider.Transform.Position;
        JVector scale = collider.Transform.Scale;
        JBBox bbox = collider.Shape.CollisionShape.BoundingBox;

        JVector size = bbox.Max - bbox.Min;
        size.X *= scale.X;
        size.Y *= scale.Y;
        size.Z *= scale.Z;

        return new JBBox(center - size*0.5f, center + size*0.5f);
    }
}
