#define SPARTA_COLLECT_DEPENDENCIES
using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Dependency
{
    public static class DependencyGraphBuilder
    {
        const string CollectDependenciesFlag = "SPARTA_COLLECT_DEPENDENCIES";

        static DependencyGraph _graph = new DependencyGraph();

        static Stack<Node> _nodeStack = new Stack<Node>();

        public static DependencyGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        public static bool IsAvailable
        {
            get
            {
                #if SPARTA_COLLECT_DEPENDENCIES
                return true;
                #else
                return false;
                #endif
            }
        }

        static Node Current
        {
            get
            {
                return _nodeStack.Peek();
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Bind(Type type, Type bind, string tag)
        {
            var node = _graph.GetNode(type, tag);
            node.History.Add(Action.Bind);

            var binded = _graph.GetNode(bind, tag);
            binded.History.Add(Action.Binded);

            if(type != bind)
            {
                if(type.IsInterface)
                {
                    binded.Implements.Add(node);
                }
                else
                {
                    binded.Aliases.Add(node);
                }
                node.Definitions.Add(binded);
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Remove(Type type, string tag)
        {
            var node = _graph.GetNode(type, tag);
            node.History.Add(Action.Remove);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Alias(Type fromType, string fromTag, Type toType, string toTag)
        {
            if(fromType != toType)
            {
                var fromNode = _graph.GetNode(fromType, fromTag);
                var toNode = _graph.GetNode(toType, toTag);

                if(fromType.IsInterface)
                {
                    toNode.Implements.Add(fromNode);
                }
                else
                {
                    toNode.Aliases.Add(fromNode);
                }
                fromNode.Definitions.Add(toNode);
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartCreation(Type type, string tag)
        {
            var node = _graph.TryGetNode(type, tag);
            if(node != null)
            {
                node.History.Add(Action.Create);

                if(_nodeStack.Count > 0)
                {
                    node.Instigator = Current;
                    Current.Outcoming.Add(node);
                    node.Incoming.Add(Current);
                }
                else
                {
                    _graph.RootNodes.Add(node);
                    node.CreationStack = Environment.StackTrace ?? "Unknown stack";
                }

                _nodeStack.Push(node);
            }
            else
            {
                throw new Exception(string.Format("Start creation with undefined type {0}", type.Name));
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartSetup(Type type, string tag)
        {
            var node = _graph.TryGetNode(type, tag);
            if(node != null)
            {
                node.History.Add(Action.Setup);
            }
            else
            {
                throw new Exception("Start setup with undefined type");
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Finalize(Type type)
        {
            if(_nodeStack.Count > 0)
            {
                var node = _nodeStack.Pop();
                if(node.Class != type.Name)
                {
                    throw new Exception("Invalid type");
                }
            }
            else
            {
                Log.e("Invalid stack");
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Resolve(Type type, string tag)
        {
            var node = _graph.TryGetNode(type, tag);
            if(node != null)
            {
                node.History.Add(Action.Resolve);
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Reset()
        {
            _graph = new DependencyGraph();
            _nodeStack = new Stack<Node>();
        }
    }

    public class DependencyGraph : IEnumerable<Node>
    {
        public readonly HashSet<Node> RootNodes;
        public readonly Dictionary<Type, Dictionary<string, Node>> Bindings;

        public DependencyGraph()
        {
            RootNodes = new HashSet<Node>();
            Bindings = new Dictionary<Type, Dictionary<string, Node>>();
        }

        public void AddNode(Type type, Node node)
        {
            Dictionary<string, Node> instances = null;
            if(!Bindings.TryGetValue(type, out instances))
            {
                instances = new Dictionary<string, Node>();
                Bindings.Add(type, instances);
            }

            instances.Add(node.Tag, node);
        }

        public Node TryGetNode(Type type, string tag)
        {
            Dictionary<string, Node> instances = null;
            if(Bindings.TryGetValue(type, out instances))
            {
                Node n = null;
                if(instances.TryGetValue(tag ?? string.Empty, out n))
                {
                    return n;
                }
            }
            return null;
        }

        public Node GetNode(Type type, string tag)
        {
            var node = TryGetNode(type, tag);
            if(node == null)
            {
                node = new Node(type);
                AddNode(type, node);
            }
            return node;
        }

        #region IEnumerable implementation

        public IEnumerator<Node> GetEnumerator()
        {
            foreach(var type in Bindings.Values)
            {
                foreach(var node in type.Values)
                {
                    yield return node;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

    public enum Origin
    {
        Explicit,
        Creation,
        Setup
    }

    public enum Action
    {
        Bind,
        Binded,
        Resolve,
        Create,
        Setup,
        Remove
    }

    public class Node
    {
        // Incoming edges.
        public HashSet<Node> Incoming;

        // Outcoming edges. Dependencies.
        public HashSet<Node> Outcoming;

        // Outcoming edges. Interface types
        public HashSet<Node> Implements;

        // Outcoming edges. Aliased types
        public HashSet<Node> Aliases;

        // Incomig edges. Types aliased as this
        public HashSet<Node> Definitions;

        // Historic
        public List<Action> History;

        readonly Type _type;

        readonly string _tag;

        // Dependency Tag
        public string Tag
        {
            get
            {
                return _tag;
            }
        }   

        // Namespace
        public string Namespace
        {
            get
            {
                return _type.Namespace;
            }
        }

        // Class
        public string Class
        {
            get
            {
                return _type.Name;
            }
        }

        // Creator node
        public Node Instigator;

        // Origin type
        public Origin Origin;

        // Stacktrace for explicit creation
        public string CreationStack;

        public bool IsSingle
        {
            get
            {
                return Definitions.Count <= 1;
            }
        }

        public bool IsRoot
        {
            get
            {
                return Instigator == null;
            }
        }

        public bool Instantiated
        {
            get
            {
                return !string.IsNullOrEmpty(CreationStack) || Instigator != null;
            }
        }

        public bool IsInterface
        {
            get
            {
                return _type.IsInterface;
            }
        }

        public string Name
        {
            get
            {
                var root = IsRoot ? "· " : string.Empty;
                var tag = string.IsNullOrEmpty(Tag) ? string.Empty : string.Format(" <{0}>", Tag);
                var list = IsSingle ? string.Empty : " []";
                var className = Class;
                return string.Format("{0}{1}{2}{3}", root, className, list, tag);
            }
        }

        public Node()
        {
            Incoming = new HashSet<Node>();
            Outcoming = new HashSet<Node>();
            Implements = new HashSet<Node>();
            Aliases = new HashSet<Node>();
            Definitions = new HashSet<Node>();
            History = new List<Action>();

            Origin = Origin.Explicit;
            CreationStack = string.Empty;
        }

        public Node(Type type, string tag = "") : this()
        {
            _type = type;
            _tag = tag ?? string.Empty;
        }

        public override string ToString()
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("##########");
            content.AppendLine(Class);
            content.AppendLine(Tag);
            content.AppendLine(Namespace);
            content.Append("Single: ").AppendLine(IsSingle.ToString());
            content.AppendLine();
            content.AppendLine("Creation:")
            .Append("Root: ").AppendLine(IsRoot.ToString())
            .Append("Instantiated: ").AppendLine(Instantiated.ToString())
            .Append("Instigator: ").AppendLine(Instigator != null ? Instigator.Class ?? "<none>" : "<none>")
            .AppendLine(CreationStack ?? string.Empty).AppendLine();
            content.AppendLine("History:");
            foreach(var h in History)
            {
                content.AppendLine(h.ToString());
            }
            content.AppendLine();
            content.AppendLine("Dependencies:");
            foreach(var dep in Outcoming)
            {
                content.AppendLine(dep.Class);
            }
            content.AppendLine();

            content.AppendLine("Needed for;");
            foreach(var dep in Incoming)
            {
                content.AppendLine(dep.Class);
            }
            content.AppendLine();

            content.AppendLine("Implements:");
            foreach(var dep in Implements)
            {
                content.AppendLine(dep.Class);
            }
            content.AppendLine();

            content.AppendLine("Aliases:");
            foreach(var dep in Aliases)
            {
                content.AppendLine(dep.Class);
            }
            content.AppendLine();

            content.AppendLine("Defines:");
            foreach(var dep in Definitions)
            {
                content.AppendLine(dep.Class);
            }
            content.AppendLine("##########");
            return content.ToString();
        }
    }
}