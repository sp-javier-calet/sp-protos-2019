using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Interface that encapsulate any kind of graphic object that has a color, alpha, material and can be refreshed
    public interface IGraphicObject
    {
        Color Color { get; set; }

        float Alpha { get; set; }

        Material Material { get; set; }

        void Refresh();
    }
}
