using UnityEditor;
using UnityEngine;

namespace SocialPoint.Utils
{
    [CustomPropertyDrawer(typeof(UnityLayer))]
    public sealed class UnityLayerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(UnityEngine.Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUI.BeginProperty(_position, GUIContent.none, _property);

            SerializedProperty layerIndex = _property.FindPropertyRelative("LayerIndex");

            _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
            if(layerIndex != null)
            {
                layerIndex.intValue = EditorGUI.LayerField(_position, layerIndex.intValue);
            }

            EditorGUI.EndProperty();
        }
    }
}