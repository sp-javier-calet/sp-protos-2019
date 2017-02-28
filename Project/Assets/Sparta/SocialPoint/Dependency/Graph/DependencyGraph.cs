using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Dependency.Graph
{
    public static class DependencyGraphBuilder
    {
        const string CollectDependenciesFlag = "SPARTA_COLLECT_DEPENDENCIES";

        static DependencyGraph _graph = new DependencyGraph();

        static Stack<Node> _nodeStack = new Stack<Node>();

        static Phase _currentActionPhase = Phase.GlobalInstall;

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
        public static void Bind(Type type, Type bind, TagValue tag)
        {
            var node = _graph.GetNode(type, tag);
            node.History.Add(new HistoryAction(_currentActionPhase, Action.Bind, bind.Name));

            var bound = _graph.GetNode(bind, tag);
            bound.History.Add(new HistoryAction(_currentActionPhase, Action.Bound, type.Name));

            if(type != bind)
            {
                if(type.IsInterface)
                {
                    bound.Implements.Add(node);
                }
                else
                {
                    bound.Aliases.Add(node);
                }
                node.Definitions.Add(bound);
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Remove(Type type, TagValue tag)
        {
            var node = _graph.GetNode(type, tag);
            node.History.Add(new HistoryAction(_currentActionPhase, Action.Remove));
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Alias(Type fromType, string fromTag, Type toType, string toTag)
        {
            if(fromType != toType)
            {
                var fromNode = _graph.GetNode(fromType, fromTag);
                fromNode.History.Add(new HistoryAction(_currentActionPhase, Action.Lookup, toType.Name));

                var toNode = _graph.GetNode(toType, toTag);
                fromNode.History.Add(new HistoryAction(_currentActionPhase, Action.Aliased, fromNode.Name));

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
        public static void StartCreation(Type type, TagValue tag)
        {
            var node = _graph.TryGetNode(type, tag);
            if(node != null)
            {
                if(_nodeStack.Count > 0)
                {
                    var current = Current;
                    // Avoid adding circular references for the same node when bound to itself
                    if(current.Class != type.Name || current.Tag != tag)
                    {
                        node.History.Add(new HistoryAction(_currentActionPhase, Action.Create));
                    
                        node.Instigator = current;
                        current.Outcoming.Add(node);
                        node.Incoming.Add(Current);
                    }
                }
                else
                {
                    node.History.Add(new HistoryAction(_currentActionPhase, Action.Create, "Root"));
                    _graph.RootNodes.Add(node);
                    node.CreationStack = System.Environment.StackTrace ?? "Unknown stack";
                }

                _nodeStack.Push(node);
            }
            else
            {
                _nodeStack.Push(new Node());
                Log.e(string.Format("Start creation with undefined type {0}", type.Name));
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartSetup(Type type, TagValue tag)
        {
            var node = _graph.TryGetNode(type, tag);
            if(node != null)
            {
                node.History.Add(new HistoryAction(_currentActionPhase, Action.Setup));
            }
            else
            {
                throw new Exception("Start setup with undefined type");
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Finalize(Type type, Object instance)
        {
            if(instance == null)
            {
                if(_nodeStack.Count > 0)
                {
                    var node = _nodeStack.Peek();
                    node.HasNullValue = true;
                }
            }
            Finalize(type);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Finalize(Type type)
        {
            if(_nodeStack.Count > 0)
            {
                var node = _nodeStack.Pop();
                if(node.Valid && node.Class != type.Name)
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
        public static void Resolve(Type type, TagValue tag)
        {
            var node = _graph.TryGetNode(type, tag);
            if(node != null)
            {
                node.History.Add(new HistoryAction(_currentActionPhase, Action.Resolve));
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Reset()
        {
            _graph = new DependencyGraph();
            _nodeStack = new Stack<Node>();
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartGlobalInstall()
        {
            _currentActionPhase = Phase.GlobalInstall;
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartInstall()
        {
            _currentActionPhase = Phase.Install;
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartInitialization()
        {
            _currentActionPhase = Phase.Initialization;
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void EndPhase()
        {
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

            if(!instances.ContainsKey(node.Tag))
            {
                instances.Add(node.Tag, node);
            }
        }

        public Node TryGetNode(Type type, TagValue tag)
        {
            Dictionary<string, Node> instances = null;
            if(Bindings.TryGetValue(type, out instances))
            {
                Node n = null;
                if(instances.TryGetValue(tag, out n))
                {
                    return n;
                }
            }
            return null;
        }

        public Node GetNode(Type type, TagValue tag)
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
            var itr = Bindings.Values.GetEnumerator();
            while(itr.MoveNext())
            {
                var type = itr.Current;
                var itr2 = type.Values.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var node = itr2.Current;
                    yield return node;
                }
                itr2.Dispose();
            }
            itr.Dispose();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

    public enum Phase
    {
        GlobalInstall,
        Install,
        Initialization
    }

    public enum Action
    {
        Bind,
        Bound,
        Lookup,
        Aliased,
        Resolve,
        Create,
        Setup,
        Remove
    }

    public struct HistoryAction
    {
        public readonly Phase Phase;
        public readonly Action Action;
        public readonly string Data;

        public HistoryAction(Phase phase, Action action)
        {
            Phase = phase;
            Action = action;
            Data = string.Empty;
        }

        public HistoryAction(Phase phase, Action action, string data)
        {
            Phase = phase;
            Action = action;
            Data = data;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} on {2}", Action, Data, Phase);
        }
    }

    /// <summary>
    /// Tag value.
    /// Encapsulates the actual Tag string to avoid null values.
    /// </summary>
    public struct TagValue
    {
        string _value;

        public TagValue(string tag)
        {
            _value = tag ?? string.Empty;
        }

        public static implicit operator TagValue(string tag)
        {
            return new TagValue(tag);
        }

        public static implicit operator string(TagValue tag)
        {
            return tag._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(!(obj is TagValue))
            {
                return false;
            }
            var other = (TagValue)obj;
            return _value == other._value;
        }

        public override string ToString()
        {
            return _value;
        }

        public static bool operator ==(TagValue value1, TagValue value2)
        {
            return value1._value == value2._value;
        }

        public static bool operator !=(TagValue value1, TagValue value2)
        {
            return value1._value != value2._value;
        }
    }

    /// <summary>
    /// Dependency graph Node.
    /// </summary>
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
        public List<HistoryAction> History;

        public readonly bool Valid;

        readonly Type _type;

        readonly TagValue _tag;

        // Dependency Tag
        public TagValue Tag
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

        // Stacktrace for explicit creation
        public string CreationStack;

        public bool HasNullValue;

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
                return Instigator == null && !string.IsNullOrEmpty(CreationStack);
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

        internal class UndefinedType
        {
        }

        public Node()
        {
            Incoming = new HashSet<Node>();
            Outcoming = new HashSet<Node>();
            Implements = new HashSet<Node>();
            Aliases = new HashSet<Node>();
            Definitions = new HashSet<Node>();
            History = new List<HistoryAction>();
            CreationStack = string.Empty;
             
            _type = typeof(UndefinedType);
            _tag = string.Empty;
            Valid = false;
        }

        public Node(Type type, string tag = "") : this()
        {
            _type = type;
            _tag = tag;
            Valid = true;
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
                .Append("Null Values: ").AppendLine(HasNullValue.ToString())
            .Append("Instigator: ").AppendLine(Instigator != null ? Instigator.Class ?? "<none>" : "<none>")
            .AppendLine(CreationStack ?? string.Empty).AppendLine();
            content.AppendLine("History:");
            for(int i = 0, HistoryCount = History.Count; i < HistoryCount; i++)
            {
                var h = History[i];
                content.AppendLine(h.ToString());
            }
            content.AppendLine();
            content.AppendLine("Dependencies:");

            var iter = Outcoming.GetEnumerator();
            while(iter.MoveNext())
            {
                var dep = iter.Current;
                content.AppendLine(dep.Class);
            }
            iter.Dispose();
            content.AppendLine();

            content.AppendLine("Needed for;");
            iter = Incoming.GetEnumerator();
            while(iter.MoveNext())
            {
                var dep = iter.Current;
                content.AppendLine(dep.Class);
            }
            iter.Dispose();
            content.AppendLine();

            content.AppendLine("Implements:");
            iter = Implements.GetEnumerator();
            while(iter.MoveNext())
            {
                var dep = iter.Current;
                content.AppendLine(dep.Class);
            }
            iter.Dispose();
            content.AppendLine();

            content.AppendLine("Aliases:");
            iter = Aliases.GetEnumerator();
            while(iter.MoveNext())
            {
                var dep = iter.Current;
                content.AppendLine(dep.Class);
            }
            iter.Dispose();
            content.AppendLine();

            content.AppendLine("Defines:");
            iter = Definitions.GetEnumerator();
            while(iter.MoveNext())
            {
                var dep = iter.Current;
                content.AppendLine(dep.Class);
            }
            iter.Dispose();
            content.AppendLine("##########");
            return content.ToString();
        }
    }
}