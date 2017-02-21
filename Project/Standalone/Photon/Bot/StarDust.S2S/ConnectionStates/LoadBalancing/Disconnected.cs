// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Disconnected.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The disconnected.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.ConnectionStates.LoadBalancing
{
    internal class Disconnected : ConnectionStateBase
    {
        public static readonly Disconnected Instance = new Disconnected();
        
        // no special methods
    }
}