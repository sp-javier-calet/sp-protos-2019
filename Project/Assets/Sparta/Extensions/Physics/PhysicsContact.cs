using System;
using System.Collections;
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.Collision.Shapes;

namespace SocialPoint.Physics
{
    public struct PhysicsContact
    {
        public RigidBody other;
        
        public JVector normal, tangent;
        
        public JVector point;
        
        public float penetration;

        public PhysicsContact(Contact contact, RigidBody body)
        {
            var Is1 = body == contact.Body1;
            other = Is1 ? contact.Body2 : contact.Body1;
            normal = contact.Normal;
            tangent = contact.Tangent;
            point = Is1 ? contact.Position1 : contact.Position2;
            penetration = contact.Penetration;
        }
    }
}

