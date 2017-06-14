using SocialPoint.IO;
using SocialPoint.BehaviorTree;

namespace BehaviorDesigner.Runtime.Standalone
{
    public class BehaviorTreeBuilder
    {
        public static readonly BehaviorTreeBuilder Instance = new BehaviorTreeBuilder();

        string _defaultPath = null;
        string _debugPath = null;
        string _name = null;

        public BehaviorTreeBuilder WithTree(string path, string debugPath = null)
        {
            _defaultPath = path;
            _debugPath = debugPath;
            return this;
        }

        public BehaviorTreeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public BehaviorTree Build(
#if BEHAVIOR_DESIGNER_STANDALONE
            SocialPoint.Multiplayer.NetworkSceneContext context,
#endif
            IFileManager fileManager)
        {
            #if !BEHAVIOR_DESIGNER_STANDALONE
            BehaviorManager behaviourManager = BehaviorManager.instance;
            var tree = BehaviorTreeUnityBuilder.Instance.WithScriptableObject(_debugPath).WithStandaloneTree(_defaultPath).WithName(_name).Build(fileManager);
            #else
            BehaviorManager behaviourManager = context.BehaviourManager;
            BehaviorSource behaviorSource = new BehaviorSource();
            behaviorSource.binaryDeserialization = context.BinaryDeserialization;
            using (var fh = fileManager.Read(_defaultPath))
            {
                BehaviorSourceSerializer.Instance.Deserialize(behaviorSource, new BehaviorReaderWrapper(fh.Reader));
            }
            var tree = new BehaviorTree(behaviourManager, context.BinaryDeserialization);
            tree.SetBehaviorSource(behaviorSource);
            behaviorSource.Initialize(tree);
            tree.RestartWhenComplete = true;
            tree.EnableBehavior();
            #endif

            behaviourManager.UpdateInterval = UpdateIntervalType.Manual;

            return tree;
        }

        public void Destroy(BehaviorTree tree)
        {
            #if !BEHAVIOR_DESIGNER_STANDALONE
            UnityEngine.GameObject.Destroy(tree.gameObject);
            #endif
        }
    }
}
