using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace AssetBundleGraph
{

    [Serializable]
    public class ConnectionPointData
    {

        private const string ID = "id";
        private const string LABEL = "label";
        private const string COLOR = "labelColor";
        private const string PRIORITY = "orderPriority";
        private const string SHOWLABEL = "showLabel";
        private const string HIDDEN = "hidden";

        /**
		* In order to support Unity serialization for Undo, cyclic reference need to be avoided.
		* For that reason, we are storing parentId instead of pointer to parent NodeData
		*/

        [SerializeField]
        private string id;
        [SerializeField]
        private string label;
        [SerializeField]
        private string parentId;
        [SerializeField]
        private bool isInput;
        [SerializeField]
        private Rect buttonRect;
        [SerializeField]
        private Color labelColor;
        [SerializeField]
        private bool hidden;

        //		private int orderPriority;
        //		private bool showLabel;

        public ConnectionPointData(string id, string label, NodeData parent, bool isInput, bool isHidden = false/*, int orderPriority, bool showLabel */)
        {
            this.id = id;
            this.label = label;
            this.parentId = parent.Id;
            this.isInput = isInput;
            labelColor = NodeData.DEFAULT_COLOR;
            hidden = isHidden;
            //			this.orderPriority = orderPriority;
            //			this.showLabel = showLabel;
        }

        public ConnectionPointData(string label, NodeData parent, bool isInput, bool isHidden = false)
        {
            this.id = Guid.NewGuid().ToString();
            this.label = label;
            this.parentId = parent.Id;
            this.isInput = isInput;
            labelColor = NodeData.DEFAULT_COLOR;
            hidden = isHidden;
            //			this.orderPriority = pointGui.orderPriority;
            //			this.showLabel = pointGui.showLabel;
        }

        public ConnectionPointData(Dictionary<string, object> dic, NodeData parent, bool isInput)
        {

            this.id = dic[ID] as string;
            this.label = dic[LABEL] as string;
            this.parentId = parent.Id;
            this.isInput = isInput;
            labelColor = NodeData.DEFAULT_COLOR;
            if(dic.ContainsKey(HIDDEN))
            {
                hidden = Convert.ToBoolean(dic[HIDDEN]);
            }
            else
            {
                hidden = false;
            }

            //			this.orderPriority = pointGui.orderPriority;
            //			this.showLabel = pointGui.showLabel;
        }


        public string Id
        {
            get
            {
                return id;
            }
        }

        public string Label
        {
            get
            {
                return label;
            }
            set
            {
                label = value;
            }
        }

        public Color LabelColor
        {
            get
            {
                return labelColor;
            }
            set
            {
                labelColor = value;
            }
        }

        public string NodeId
        {
            get
            {
                return parentId;
            }
        }

        public bool IsInput
        {
            get
            {
                return isInput;
            }
        }

        public bool IsOutput
        {
            get
            {
                return !isInput;
            }
        }

        public bool IsHidden
        {
            get
            {
                return hidden;
            }
        }

        public Rect Region
        {
            get
            {
                return buttonRect;
            }
        }

        //		public int OrderPriority {
        //			get {
        //				return orderPriority;
        //			}
        //		}
        //		public bool ShowLabel {
        //			get {
        //				return showLabel;
        //			}
        //		}

        // returns rect for outside marker
        public Rect GetGlobalRegion(NodeGUI node)
        {
            var baseRect = node.Region;
            return new Rect(
                baseRect.x + buttonRect.x,
                baseRect.y + buttonRect.y,
                buttonRect.width,
                buttonRect.height
            );
        }

        // returns rect for connection dot
        public Rect GetGlobalPointRegion(NodeGUI node)
        {
            if(IsInput)
            {
                return GetInputPointRect(node);
            }
            else
            {
                return GetOutputPointRect(node);
            }
        }

        public Vector2 GetGlobalPosition(NodeGUI node)
        {
            var x = 0f;
            var y = 0f;

            var baseRect = node.Region;

            if(IsInput)
            {
                x = baseRect.x;
                y = baseRect.y + buttonRect.y + (buttonRect.height / 2f) - 1f;
            }

            if(IsOutput)
            {
                x = baseRect.x + baseRect.width;
                y = baseRect.y + buttonRect.y + (buttonRect.height / 2f) - 1f;
            }

            return new Vector2(x, y);
        }

        public void UpdateRegion(NodeGUI node, int index, int max)
        {
            var parentRegion = node.Region;
            if(IsInput)
            {

                var initialY = (AssetGraphRelativePaths.NODE_BASE_HEIGHT - AssetGraphRelativePaths.INPUT_POINT_HEIGHT) / 2f;
                var marginY = initialY + AssetGraphRelativePaths.FILTER_OUTPUT_SPAN * (index);

                buttonRect = new Rect(
                    0,
                    marginY,
                    AssetGraphRelativePaths.INPUT_POINT_WIDTH,
                    AssetGraphRelativePaths.INPUT_POINT_HEIGHT);
            }
            else
            {

                var initialY = (AssetGraphRelativePaths.NODE_BASE_HEIGHT - AssetGraphRelativePaths.OUTPUT_POINT_HEIGHT) / 2f;
                var marginY = initialY + AssetGraphRelativePaths.FILTER_OUTPUT_SPAN * (index);

                buttonRect = new Rect(
                    parentRegion.width - AssetGraphRelativePaths.OUTPUT_POINT_WIDTH + 1f,
                    marginY,
                    AssetGraphRelativePaths.OUTPUT_POINT_WIDTH,
                    AssetGraphRelativePaths.OUTPUT_POINT_HEIGHT);
            }
        }

        private Rect GetOutputPointRect(NodeGUI node)
        {
            var baseRect = node.Region;
            return new Rect(
                baseRect.x + baseRect.width - 8f,
                baseRect.y + buttonRect.y + 1f,
                AssetGraphRelativePaths.CONNECTION_POINT_MARK_SIZE,
                AssetGraphRelativePaths.CONNECTION_POINT_MARK_SIZE
            );
        }

        private Rect GetInputPointRect(NodeGUI node)
        {
            var baseRect = node.Region;
            return new Rect(
                baseRect.x - 2f,
                baseRect.y + buttonRect.y + 3f,
                AssetGraphRelativePaths.CONNECTION_POINT_MARK_SIZE,
                AssetGraphRelativePaths.CONNECTION_POINT_MARK_SIZE
            );
        }

        public Dictionary<string, object> ToJsonDictionary()
        {
            return new Dictionary<string, object>() {
                {ID, this.id},
                {LABEL, this.label},
                {HIDDEN, this.hidden}
            };
        }
    }
}
