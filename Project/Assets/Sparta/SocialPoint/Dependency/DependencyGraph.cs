﻿#define SPARTA_COLLECT_DEPENDENCIES
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
            if(node == null)
            {
                node = new Node(type, tag);
                node.Bind = bind.Name;
                _graph.AddNode(type, node);
            }


            node.History.Add(Action.Bind);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Rebind(Type type, string tag)
        {
            var node = _graph.GetNode(type, tag);
            if(node == null)
            {
                node = new Node(type, tag);
                _graph.AddNode(type, node);
            }
            node.History.Add(Action.Rebind);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Remove(Type type, string tag)
        {
            var node = _graph.GetNode(type, tag);
            if(node == null)
            {
                node = new Node(type);
                _graph.AddNode(type, node);
            }
            node.History.Add(Action.Remove);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Alias(Type fromType, string fromTag, Type toType, string toTag)
        {
            var fromNode = _graph.GetNode(fromType, fromTag);
            if(fromNode == null)
            {
                fromNode = new Node(fromType);
                _graph.AddNode(fromType, fromNode);
            }
            var toNode = _graph.GetNode(toType, toTag);
            if(toNode == null)
            {
                toNode = new Node(toType);
                _graph.AddNode(toType, toNode);
            }

            fromNode.Aliases.Add(toNode);
            toNode.Definitions.Add(fromNode);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartCreation(Type type, string tag)
        {
            var node = _graph.GetNode(type, tag);
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
                //throw new Exception(string.Format("Start creation with undefined type {0}", type.Name));
                Log.e(string.Format("Start creation with undefined type {0}", type.Name));
            }
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void StartSetup(Type type, string tag)
        {
            var node = _graph.GetNode(type, tag);
            if(node != null)
            {
                node.History.Add(Action.Setup);
            }
            else
            {
                //throw new Exception("Start setup with undefined type");
                Log.e("Start setup with undefined type");
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
                    //throw new Exception("Invalid type");
                    Log.e("Invalid type");
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
            var node = _graph.GetNode(type, tag);
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

    public class DependencyGraph
    {
        public readonly List<Node> RootNodes;
        public readonly Dictionary<Type, Dictionary<string, Node>> Bindings;

        public DependencyGraph()
        {
            RootNodes = new List<Node>();
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

        public Node GetNode(Type type, string tag)
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
        Rebind,
        Resolve,
        Create,
        Setup,
        Remove
    }

    public class Node
    {
        // Incoming edges.
        public List<Node> Incoming;

        // Outcoming edges. Dependencies.
        public List<Node> Outcoming;

        // Outcoming edges. Aliased types
        public List<Node> Aliases;

        // Incomig edges. Types aliased as this
        public List<Node> Definitions;

        // Historic
        public List<Action> History;

        public string Namespace;

        // Class
        public string Class;

        // Binded Class
        public string Bind;

        // Dependency Tag
        public string Tag;

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
                return Definitions.Count == 0;
            }
        }

        public bool IsRoot
        {
            get
            {
                return Instigator == null;
            }
        }

        public Node()
        {
            Incoming = new List<Node>();
            Outcoming = new List<Node>();
            Aliases = new List<Node>();
            Definitions = new List<Node>();
            History = new List<Action>();

            Origin = Origin.Explicit;
            CreationStack = string.Empty;
        }

        public Node(Type type, string tag = "") : this()
        {
            Class = type.Name;
            Namespace = type.Namespace ?? string.Empty;
            Tag = tag ?? string.Empty;
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