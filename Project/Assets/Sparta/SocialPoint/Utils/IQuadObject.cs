using System;

namespace SocialPoint.Utils
{
    public interface IQuadObject
    {
        QuadTreeRect Bounds { get; }
        event EventHandler BoundsChanged;
    }
}