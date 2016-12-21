using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetBundleGraph.Modifiers {
	
	/*
	 * Code template for Scene modifier.
	 * You can copy and create your CustomModifier.
	 */ 

	[Serializable] 
	[CustomModifier("Default Modifier(Scene)", typeof(UnityEngine.SceneManagement.Scene))]
	public class SceneModifier : IModifier {
		
		public SceneModifier () {}

		public bool IsModified (object asset) {
			//var anim = asset as UnityEngine.SceneManagement.Scene;

			// Do your work here

			var changed = false;
			return changed; 
		}

		public void Modify (object asset) {
			//var anim = asset as UnityEngine.SceneManagement.Scene;

			// Do your work here
		}

		public void OnInspectorGUI (Action onValueChanged) {
			GUILayout.Label("Implement your modifier for this type.");
		}

		public string Serialize() {
			return JsonUtility.ToJson(this);
		}
	}	
}