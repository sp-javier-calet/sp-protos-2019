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
      
        /* Foldout status */
        public static GUIStyle GreenFoldout { get; private set; }
        public static GUIStyle YellowFoldout { get; private set; }
        public static GUIStyle RedFoldout { get; private set; }
        public static GUIStyle GrayFoldout { get; private set; }

        /* Label status */
        public static GUIStyle GreenLabel { get; private set; }
        public static GUIStyle YellowLabel { get; private set; }
        public static GUIStyle RedLabel { get; private set; }
        public static GUIStyle GrayLabel { get; private set; }

        public static readonly GUILayoutOption[] ActionButtonOptions = new GUILayoutOption[] {
            GUILayout.Width(60),
            GUILayout.Height(18)
        };

        public static readonly GUILayoutOption[] PopupLayoutOptions = new GUILayoutOption[] {
            GUILayout.Width(60),
            GUILayout.Height(15)
        };

        public static readonly GUILayoutOption[] TableLabelOptions = new GUILayoutOption[] {
            GUILayout.MaxWidth(20)
        };

        static Styles()
        {
            var RedColor = new Color(0.8f, 0.1f, 0.1f, 1.0f);
            var GreenColor = new Color(0.1f, 0.8f, 0.1f, 1.0f);
            var YellowColor = new Color(0.8f, 0.8f, 0.1f, 1.0f);
            var GrayColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

            InvalidProjectLabel = new GUIStyle();
            InvalidProjectLabel.normal.textColor = RedColor;
            InvalidProjectLabel.margin = new RectOffset(0, 70, 0, 0);
            InvalidProjectLabel.alignment = TextAnchor.MiddleRight;

            ValidProjectLabel = new GUIStyle();
            ValidProjectLabel.normal.textColor = GreenColor;
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
            Warning.normal.textColor = RedColor;


            Group = new GUIStyle();
            Group.padding = new RectOffset(15, 10, 0, 10);

            TableLabel = new GUIStyle();
            TableLabel.normal.textColor = EditorStyles.label.normal.textColor;
            TableLabel.fontStyle = FontStyle.Bold;

            TableContent = new GUIStyle();
            TableContent.normal.textColor = EditorStyles.label.normal.textColor;

            GreenFoldout = new GUIStyle(EditorStyles.foldout);
            SetStyleColor(GreenFoldout, GreenColor);
            YellowFoldout = new GUIStyle(EditorStyles.foldout);
            SetStyleColor(YellowFoldout, YellowColor);
            RedFoldout = new GUIStyle(EditorStyles.foldout);
            SetStyleColor(RedFoldout, RedColor);
            GrayFoldout = new GUIStyle(EditorStyles.foldout);
            SetStyleColor(GrayFoldout, GrayColor);

            GreenLabel = new GUIStyle(EditorStyles.label);
            SetStyleColor(GreenLabel, GreenColor);
            YellowLabel = new GUIStyle(EditorStyles.label);
            SetStyleColor(YellowLabel, YellowColor);
            RedLabel = new GUIStyle(EditorStyles.label);
            SetStyleColor(RedLabel, RedColor);
            GrayLabel = new GUIStyle(EditorStyles.label);
            SetStyleColor(GrayLabel, GrayColor);

        }

        static void SetStyleColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.onNormal.textColor = color;
            style.active.textColor = color;
            style.onActive.textColor = color;
            style.focused.textColor = color;
            style.onFocused.textColor = color;
            style.hover.textColor = color;
            style.onHover.textColor = color;
        }
    }
}
