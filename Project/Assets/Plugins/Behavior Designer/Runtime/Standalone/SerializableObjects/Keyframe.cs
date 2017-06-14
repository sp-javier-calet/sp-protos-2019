namespace BehaviorDesigner.Runtime.Standalone
{
    public class Keyframe
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;

        public int tangentMode;

        public Keyframe()
        {
        }

        public Keyframe(float time, float value, float inTangent, float outTangent)
        {
            this.time = time;
            this.value = value;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
        }
    }
}
