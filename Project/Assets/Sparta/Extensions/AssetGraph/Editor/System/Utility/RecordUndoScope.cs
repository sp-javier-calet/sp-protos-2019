﻿using UnityEngine;
using AssetBundleGraph;

namespace AssetBundleGraph {
	public class RecordUndoScope : GUI.Scope {

		private NodeGUI node;
		private bool saveOnScopeEnd;

		public RecordUndoScope (string message) {
			NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
		}

		public RecordUndoScope (string message, bool saveOnScopeEnd) {
			this.saveOnScopeEnd = saveOnScopeEnd;
			NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
		}

		public RecordUndoScope (string message, NodeGUI node) {
			this.node = node;
			NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
		}

		public RecordUndoScope (string message, NodeGUI node, bool saveOnScopeEnd) {
			this.node = node;
			this.saveOnScopeEnd = saveOnScopeEnd;
			NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
		}

		protected override void CloseScope () {
			if(node != null) {
				node.UpdateNodeRect();
				node.ResetErrorStatus();
			}
			if(saveOnScopeEnd) {
				NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_SAVE));
			}
		}
	}
}
