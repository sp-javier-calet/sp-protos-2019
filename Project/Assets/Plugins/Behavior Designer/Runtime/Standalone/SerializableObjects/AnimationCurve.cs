using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Standalone
{
    public enum WrapMode
    {
        Once = 1,
        Loop,
        PingPong = 4,
        Default = 0,
        ClampForever = 8,
        Clamp = 1
    }
    
    public class AnimationCurve
    {
        public WrapMode preWrapMode = WrapMode.Default;
        public WrapMode postWrapMode = WrapMode.Default;
        public List<Keyframe> keyframes = new List<Keyframe>();
        
        public AnimationCurve()
        {
        }

        public void AddKey(Keyframe keyframe)
        {
            keyframes.Add(keyframe);
        }
    }
}
