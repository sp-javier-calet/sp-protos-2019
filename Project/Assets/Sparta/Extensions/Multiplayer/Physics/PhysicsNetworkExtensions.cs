using UnityEngine;
using System.Collections;

namespace SocialPoint.Multiplayer
{
    public static class PhysicsNetworkExtensions
    {
        //TODO: Simplify extension? use an extension that creates a default rigidbody?

        public static void AddRigidbody(this NetworkServerSceneController ctrl, int id, PhysicsRigidBody rigidBody)
        {
            ctrl.AddBehaviour(id, rigidBody);
        }
    }
}
