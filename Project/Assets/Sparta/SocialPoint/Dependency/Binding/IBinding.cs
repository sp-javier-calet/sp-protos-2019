using UnityEngine;
using System.Collections;

namespace SocialPoint.Dependency
{
    public interface IBinding
    {
        BindingKey Key { get; }

        bool Resolved { get; }

        object Resolve();

        void OnResolved();
    }
}