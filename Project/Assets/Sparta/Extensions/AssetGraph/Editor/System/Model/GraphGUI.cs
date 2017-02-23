using UnityEngine;
using System.Collections.Generic;

namespace AssetBundleGraph
{
    [System.Serializable]
    public class GraphGUI
    {
        [SerializeField]
        private List<NodeGUI> nodes;
        [SerializeField]
        private List<ConnectionGUI> connections;

        public List<NodeGUI> Nodes
        {
            get
            {
                return nodes;
            }
        }

        public List<ConnectionGUI> Connections
        {
            get
            {
                return connections;
            }
        }

        public GraphGUI()
        {
            nodes = new List<NodeGUI>();
            connections = new List<ConnectionGUI>();
        }

        public GraphGUI(Graph graph)
        {
            var currentNodes = new List<NodeGUI>();
            var currentConnections = new List<ConnectionGUI>();

            foreach(var node in graph.Nodes)
            {
                var newNodeGUI = new NodeGUI(node);
                newNodeGUI.WindowId = GetSafeWindowId(currentNodes);
                currentNodes.Add(newNodeGUI);
            }

            // load connections
            foreach(var c in graph.Connections)
            {
                var startNode = currentNodes.Find(node => node.Id == c.FromNodeId);
                if(startNode == null)
                {
                    continue;
                }

                var endNode = currentNodes.Find(node => node.Id == c.ToNodeId);
                if(endNode == null)
                {
                    continue;
                }
                var startPoint = startNode.Data.FindConnectionPoint(c.FromNodeConnectionPointId);
                var endPoint = endNode.Data.FindConnectionPoint(c.ToNodeConnectionPointId);

                currentConnections.Add(ConnectionGUI.LoadConnection(c.Label, c.Id, startPoint, endPoint));
            }

            nodes = currentNodes;
            connections = currentConnections;
        }

        private static int GetSafeWindowId(List<NodeGUI> nodeGUIs)
        {
            int id = -1;

            foreach(var nodeGui in nodeGUIs)
            {
                if(nodeGui.WindowId > id)
                {
                    id = nodeGui.WindowId;
                }
            }
            return id + 1;
        }

        public GraphGUI GetSubGraph(NodeGUI[] rootNodes, bool includeWarps = true)
        {
            var newgraph = new GraphGUI();
            newgraph.Nodes.AddRange(rootNodes);
            List<NodeGUI> nodesToAdd = new List<NodeGUI>();
            List<ConnectionGUI> connectionsToAdd = new List<ConnectionGUI>();

            foreach(NodeGUI node in rootNodes)
            {
                GetParentRelatives(node, ref nodesToAdd, ref connectionsToAdd, includeWarps);
                GetChildRelatives(node, ref nodesToAdd, ref connectionsToAdd, includeWarps);
            }

            newgraph.Nodes.AddRange(nodesToAdd);
            newgraph.Connections.AddRange(connectionsToAdd);

            return newgraph;
        }

        private void GetParentRelatives(NodeGUI node, ref List<NodeGUI> graphRelatedNodes, ref List<ConnectionGUI> graphRelatedCon, bool includeWarps)
        {
            if((node.Kind == NodeKind.WARP_IN || node.Kind == NodeKind.WARP_OUT) && !includeWarps)
            {
                return;
            }

            var backConnections = connections.FindAll(x => x.OutputNodeId == node.Id);
            foreach(ConnectionGUI c in backConnections)
            {
                graphRelatedCon.Add(c);
                var fromNode = nodes.Find(x => x.Id == c.InputNodeId);
                graphRelatedNodes.Add(fromNode);
                GetParentRelatives(fromNode, ref graphRelatedNodes, ref graphRelatedCon, includeWarps);
            }
        }

        private void GetChildRelatives(NodeGUI node, ref List<NodeGUI> graphRelatedNodes, ref List<ConnectionGUI> graphRelatedCon, bool includeWarps)
        {
            if((node.Kind == NodeKind.WARP_IN || node.Kind == NodeKind.WARP_OUT) && !includeWarps)
            {
                return;
            }

            var forwardConnections = connections.FindAll(x => x.InputNodeId == node.Id);
            foreach(ConnectionGUI c in forwardConnections)
            {
                graphRelatedCon.Add(c);
                var toNode = nodes.Find(x => x.Id == c.OutputNodeId);
                graphRelatedNodes.Add(toNode);
                GetChildRelatives(toNode, ref graphRelatedNodes, ref graphRelatedCon, includeWarps);
            }
        }
    }
}
