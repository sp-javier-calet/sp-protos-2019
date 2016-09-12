using UnityEngine;
using System.Collections;

namespace SocialPoint.Multiplayer
{
    public static class PhysicsNetworkExtensions
    {
        public static void AddRigidbody(this NetworkServerSceneController ctrl, int id)
        {
            ctrl.AddRigidbody(id, new PhysicsRigidBody());
        }

        public static void AddRigidbody(this NetworkServerSceneController ctrl, int id, PhysicsRigidBody rigidBody)
        {
            ctrl.AddBehaviour(id, rigidBody);
        }
    }
}
