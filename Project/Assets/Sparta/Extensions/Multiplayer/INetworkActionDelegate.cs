using System;
using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkActionDelegate
    {
        /// <summary>
        /// Implementation must apply the action to the scene object.
        /// </summary>
        /// <param name="action">Action. Should be cast to the expected type.</param>
        /// <param name="scene">Scene.</param>
        void ApplyAction(object action, NetworkScene scene);
    }
}
