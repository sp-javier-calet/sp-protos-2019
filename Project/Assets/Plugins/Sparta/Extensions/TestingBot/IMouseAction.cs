using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SocialPoint.TestingBot
{
    public interface IMouseAction
    {
        bool Clicked { get; }

        Vector2 Position { get; }

        event Action<IMouseAction> Started;

        event Action<IMouseAction> Finished;

        void Update(float dt);
    }
}