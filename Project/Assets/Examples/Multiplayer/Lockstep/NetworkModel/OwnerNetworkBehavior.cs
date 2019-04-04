//-----------------------------------------------------------------------
// OwnerNetworkBehavior.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.NetworkModel;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class OwnerNetworkBehavior : INetworkBehaviour
    {
        public byte PlayerNumber { get; set; }

        public NetworkGameObject GameObject { get; set; }

        void IDisposable.Dispose()
        {
        }

        void INetworkBehaviour.OnAdded()
        {
        }

        void INetworkBehaviour.OnObjectDestroyed()
        {
        }

        void INetworkBehaviour.OnRemoved()
        {
        }

        void IDeltaUpdateable<int>.Update(int elapsed)
        {
        }
    }
}