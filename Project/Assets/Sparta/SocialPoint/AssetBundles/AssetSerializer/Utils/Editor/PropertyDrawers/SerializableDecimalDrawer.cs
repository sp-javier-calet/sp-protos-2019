using System;
using UnityEngine;
using UnityEditor;

namespace SocialPoint.AssetSerializer.Serializers
{
    // Todo: Adpat serializable class inspector drawer to allow for it's editable use in nested classes instances
    // and containers.
    [CustomPropertyDrawer(typeof(SerializableDecimal))]
    public sealed class SerializableDecimalDrawer : PropertyDrawer
    {
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            var obj = property.serializedObject.targetObject;
            SerializableDecimal inst;
            try {
                inst = (SerializableDecimal)this.fieldInfo.GetValue(obj);
            }
            catch {
                throw new Exception("SerializableDecimal can only be used directly as an attribute on the inspected class, not in any sublcass or container");
            }
            var fieldRect = EditorGUI.PrefixLabel(position, label);
            string text = GUI.TextField(fieldRect, inst.value.ToString());
            if (GUI.changed)
            {
                decimal val;
                if(decimal.TryParse(text, out val))
                {
                    inst.value = val;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
