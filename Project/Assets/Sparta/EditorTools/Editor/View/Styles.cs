using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.View
{
    public static class Styles
    {
        public static GUIStyle InvalidProjectLabel { get; private set; }

        public static GUIStyle ValidProjectLabel { get; private set; }

        public static GUIStyle Box { get; private set; }

        public static GUIStyle ModuleStatus { get; private set; }

        public static GUIStyle Dependency { get; private set; }

        public static GUIStyle Warning { get; private set; }

        public static GUIStyle Group { get; private set; }

        public static GUIStyle TableLabel { get; private set; }

        public static GUIStyle TableContent { get; private set; }

        public static readonly GUILayoutOption[] PopupLayoutOptions = new GUILayoutOption[] {
            GUILayout.Width(60),
            GUILayout.Height(15)
        };

        public static readonly GUILayoutOption[] TableLabelOptions = new GUILayoutOption[] {
            GUILayout.MaxWidth(20)
        };

        static Styles()
        {
            InvalidProjectLabel = new GUIStyle();
            InvalidProjectLabel.normal.textColor = Color.red;
            InvalidProjectLabel.margin = new RectOffset(0, 70, 0, 0);
            InvalidProjectLabel.alignment = TextAnchor.MiddleRight;

            ValidProjectLabel = new GUIStyle();
            ValidProjectLabel.normal.textColor = Color.green;
            ValidProjectLabel.margin = new RectOffset(0, 70, 0, 0);
            ValidProjectLabel.alignment = TextAnchor.MiddleRight;

            Box = new GUIStyle();
            Box.alignment = TextAnchor.UpperRight;

            ModuleStatus = new GUIStyle();
            ModuleStatus.normal.textColor = new Color(.7f, .7f, .7f, .5f);
            ModuleStatus.alignment = TextAnchor.LowerRight;

            Dependency = new GUIStyle();
            Dependency.normal.textColor = new Color(.7f, .7f, .7f, .5f);
            Dependency.padding = new RectOffset(15, 0, 2, 0);

            Warning = new GUIStyle();
            Warning.normal.textColor = Color.red;


            Group = new GUIStyle();
            Group.padding = new RectOffset(15, 10, 0, 10);

            TableLabel = new GUIStyle();
            TableLabel.normal.textColor = EditorStyles.label.normal.textColor;
            TableLabel.fontStyle = FontStyle.Bold;

            TableContent = new GUIStyle();
            TableContent.normal.textColor = EditorStyles.label.normal.textColor;
        }
    }
}
