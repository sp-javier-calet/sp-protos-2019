using UnityEngine;

namespace SocialPoint.Utils
{
    [System.Serializable]
    public sealed class UnityLayer
    {
        [SerializeField]
        public int LayerIndex = 0;

        public void Set(int layerIndex)
        {
            if(layerIndex > 0 && layerIndex < 32)
            {
                LayerIndex = layerIndex;
            }
        }
    
        public int Mask
        {
            get { return 1 << LayerIndex; }
        }
    }
}