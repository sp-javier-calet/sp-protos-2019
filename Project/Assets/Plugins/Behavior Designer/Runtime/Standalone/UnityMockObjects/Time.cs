using System;

namespace BehaviorDesigner.Runtime.Standalone
{   
    public class Time
    {
        public static float deltaTime{ get; private set;}
        public static float time{ get; private set;}
        public static float realtimeSinceStartup{ get; private set;}
        
        public static void Start()
        {
            time = deltaTime = realtimeSinceStartup = 0f;
        }

        public static void UpdateTime(float deltaTime)
        {
            Time.deltaTime = deltaTime;
            Time.time += Time.deltaTime;
            Time.realtimeSinceStartup = Time.time;
        }
    }
}
