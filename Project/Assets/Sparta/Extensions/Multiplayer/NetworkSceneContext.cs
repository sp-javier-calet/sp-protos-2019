namespace SocialPoint.Multiplayer
{
    public class NetworkSceneContext
    {
        SocialPoint.Utils.ObjectPool _pool = null;
        public SocialPoint.Utils.ObjectPool Pool
        {
            get
            {
                return _pool;
            }
        }

        BehaviorDesigner.Runtime.BehaviorManager _behaviourManager = null;
        public BehaviorDesigner.Runtime.BehaviorManager BehaviourManager
        {
            get
            {
                return _behaviourManager;
            }
        }

        BinaryDeserialization _binaryDeserialization = null;
        public BinaryDeserialization BinaryDeserialization
        {
            get
            {
                return _binaryDeserialization;
            }
        }

        public NetworkSceneContext()
        {
            _pool = new SocialPoint.Utils.ObjectPool();
            _behaviourManager = BehaviorDesigner.Runtime.Behavior.CreateBehaviorManager();
            _binaryDeserialization = new BinaryDeserialization();
        }
    }
}
