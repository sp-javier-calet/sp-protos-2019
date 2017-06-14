#if !BEHAVIOR_DESIGNER_STANDALONE
using UnityEngine;
using System.IO;
using SocialPoint.BehaviorTree;
using SocialPoint.IO;

namespace BehaviorDesigner.Runtime.Standalone
{
    public class BehaviorTreeUnityBuilder
    {
        public static readonly BehaviorTreeUnityBuilder Instance = new BehaviorTreeUnityBuilder();
        public static GameObject BehaviorTreeParent = null;
        string _standalonePath = null;
        string _scriptablePath = null;
        string _name = null;

        public BehaviorTreeUnityBuilder WithStandaloneTree(string path)
        {
            _standalonePath = path;
            return this;
        }

        public BehaviorTreeUnityBuilder WithScriptableObject(string path)
        {
            _scriptablePath = path;
            return this;
        }

        public BehaviorTreeUnityBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        Stream _stream;

        public BehaviorTreeUnityBuilder WithStream(Stream stream)
        {
            _stream = stream;
            return this;
        }

        public BehaviorTree Build(IFileManager fileManager)
        {
            var treeGo = new GameObject();
            var tree = treeGo.AddComponent<BehaviorTree>();
            tree.StartWhenEnabled = false;

            ExternalBehaviorTree treeScriptable = null;
            if(!string.IsNullOrEmpty(_scriptablePath))
            {
                treeScriptable = Resources.Load<ExternalBehaviorTree>(_scriptablePath);
            }

            if(treeScriptable == null)
            {
                BehaviorSource behaviorSource = new BehaviorSource();
                using(var fh = fileManager.Read(_standalonePath))
                {
                    BehaviorSourceSerializer.Instance.Deserialize(behaviorSource, new BehaviorReaderWrapper(fh.Reader));
                }
                treeScriptable = ScriptableObject.CreateInstance<ExternalBehaviorTree>();
                treeScriptable.SetBehaviorSource(behaviorSource);

            }
            tree.ExternalBehavior = treeScriptable;

            tree.BehaviorName = !string.IsNullOrEmpty(_name) ? _name : tree.BehaviorName;
            treeGo.name = tree.BehaviorName;

            if(BehaviorTreeParent == null)
            {
                BehaviorTreeParent = new GameObject("Behavior Trees");
            }

            var parent = GameObject.Find(_name);
            parent = parent ?? BehaviorTreeParent;
                
            tree.transform.SetParent(parent.transform, false);

            return tree;
        }
    }
}
#endif