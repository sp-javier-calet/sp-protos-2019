using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AssetBundleGraph
{
    [System.Serializable]
    public class Graph
    {
        [SerializeField]
        private List<NodeData> nodes;
        [SerializeField]
        private List<ConnectionData> connections;

        public List<NodeData> Nodes
        {
            get
            {
                return nodes;
            }
        }

        public List<ConnectionData> Connections
        {
            get
            {
                return connections;
            }
        }

        public Graph()
        {
            nodes = new List<NodeData>();
            connections = new List<ConnectionData>();
        }

        public Graph(List<NodeGUI> nodes, List<ConnectionGUI> connections)
        {
            this.nodes = nodes.Select(n => n.Data).ToList();
            this.connections = new List<ConnectionData>();

            foreach(var cgui in connections)
            {
                this.connections.Add(new ConnectionData(cgui));
            }
        }

        public List<NodeData> CollectAllLeafNodes()
        {
            var nodesWithIn = new List<NodeData>();
            var nodesWithOut = new List<NodeData>();
            foreach(var c in Connections)
            {
                foreach(NodeData node in Nodes)
                {
                    if(node.Id == c.FromNodeId)
                    {
                        nodesWithOut.Add(node);
                    }
                    else if(node.Id == c.ToNodeId)
                    {
                        nodesWithIn.Add(node);
                    }
                }
            }

            //nodes which have inputs and doesn't have output
            return Nodes.FindAll(x => !nodesWithOut.Contains(x) && nodesWithIn.Contains(x));
        }

        public List<NodeData> CollectAllNodes(Predicate<NodeData> condition)
        {
            return Nodes.FindAll(condition);
        }

        public Graph GetSubGraph(NodeData[] rootNodes, bool includeWarps = true)
        {
            var newgraph = new Graph();
            newgraph.Nodes.AddRange(rootNodes);
            List<NodeData> nodesToAdd = new List<NodeData>();
            List<ConnectionData> connectionsToAdd = new List<ConnectionData>();

            foreach(NodeData node in rootNodes)
            {
                GetParentRelatives(node, ref nodesToAdd, ref connectionsToAdd, includeWarps);
                GetChildRelatives(node, ref nodesToAdd, ref connectionsToAdd, includeWarps);
            }

            newgraph.Nodes.AddRange(nodesToAdd);
            newgraph.Connections.AddRange(connectionsToAdd);

            return newgraph;
        }

        private void GetParentRelatives(NodeData node, ref List<NodeData> graphRelatedNodes, ref List<ConnectionData> graphRelatedCon, bool includeWarps)
        {
            if((node.Kind == NodeKind.WARP_IN || node.Kind == NodeKind.WARP_OUT) && !includeWarps)
            {
                return;
            }

            var backConnections = connections.FindAll(x => x.ToNodeId == node.Id);
            foreach(ConnectionData c in backConnections)
            {
                graphRelatedCon.Add(c);
                var fromNode = nodes.Find(x => x.Id == c.FromNodeId);
                graphRelatedNodes.Add(fromNode);
                GetParentRelatives(fromNode, ref graphRelatedNodes, ref graphRelatedCon, includeWarps);
            }
        }

        private void GetChildRelatives(NodeData node, ref List<NodeData> graphRelatedNodes, ref List<ConnectionData> graphRelatedCon, bool includeWarps)
        {
            if((node.Kind == NodeKind.WARP_IN || node.Kind == NodeKind.WARP_OUT) && !includeWarps)
            {
                return;
            }

            var forwardConnections = connections.FindAll(x => x.FromNodeId == node.Id);
            foreach(ConnectionData c in forwardConnections)
            {
                graphRelatedCon.Add(c);
                var toNode = nodes.Find(x => x.Id == c.ToNodeId);
                graphRelatedNodes.Add(toNode);
                GetChildRelatives(toNode, ref graphRelatedNodes, ref graphRelatedCon, includeWarps);
            }
        }
    }
}
