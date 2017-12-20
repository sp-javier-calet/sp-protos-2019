using UnityEngine;
using System.Collections.Generic;

namespace AssetBundleGraph
{
    /*
	 * ScriptableObject helper object to let NodeGUI edit from Inspector
	 */
    public class NodeGUIInspectorHelper : ScriptableObject
    {
        public NodeGUI node;
        public List<string> errors = new List<string>();
        public bool isActive = false;

        public void UpdateNode(NodeGUI node)
        {
            name = node.Name;
            this.node = node;
        }

        public void UpdateErrors(List<string> errorsSource)
        {
            this.errors = errorsSource;
        }
    }
}
