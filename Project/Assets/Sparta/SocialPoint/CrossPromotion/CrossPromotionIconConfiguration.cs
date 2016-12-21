using UnityEngine;

namespace SocialPoint.CrossPromotion
{
    public sealed class CrossPromotionIconConfiguration : MonoBehaviour
    {
        [SerializeField]
        Texture[] _textures;

        public Texture[] Textures
        {
            get { return _textures; }
        }

        [SerializeField]
        float _frameDelay = 0.0333f;

        public float FrameDelay
        { 
            get { return _frameDelay; }
        }

        [SerializeField]
        float _startDelay;

        public float StartDelay
        {
            get { return _startDelay; }
        }
    }
}
