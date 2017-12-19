using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetBundleGraph
{
    [Serializable]
    public class StateCheck
    {
        [SerializeField]
        public string stateName = "";
        [SerializeField]
        public string speedParam;
        [SerializeField]
        public List<TransitionCheck> outputs = new List<TransitionCheck>();

        public List<EventCheck> events = new List<EventCheck>();

        [NonSerialized]
        public bool showState = false;
        bool showEvents = false;
        bool showOutputs = false;

        public void OnInspectorGUI(Action onValueChanged, Action OnRemoved)
        {
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    var foldoutText = string.IsNullOrEmpty(stateName) ? "Unnamed State" : stateName;
                    showState = EditorGUILayout.Foldout(showState, foldoutText, true, AnimatorValidator.foldoutStyle);

                    if(GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        OnRemoved();
                        onValueChanged();
                    }
                }
                if(showState)
                {
                    var newName = EditorGUILayout.TextField("State name", stateName);

                    if(newName != stateName)
                    {
                        stateName = newName;
                        onValueChanged();
                    }

                    using(new EditorGUILayout.HorizontalScope())
                    {
                        var newSpeed = EditorGUILayout.TextField("Speed Multiplier", speedParam);

                        if(newSpeed != speedParam)
                        {
                            speedParam = newSpeed;
                            onValueChanged();
                        }
                    }

                    if(events.Count > 0 && outputs.Count == 0)
                    {
                        PaintEventList(onValueChanged);
                        PaintTransitionList(onValueChanged);
                    }
                    else
                    {
                        PaintTransitionList(onValueChanged);
                        PaintEventList(onValueChanged);
                    }

                }
            }
        }


        void PaintTransitionList(Action onValueChanged)
        {
            bool showButton = true;
            if(outputs.Count > 0)
            {
                showOutputs = EditorGUILayout.Foldout(showOutputs, "Transitions", true, AnimatorValidator.foldoutStyle);

                if(showOutputs)
                {
                    for(int j = 0; j < outputs.Count; j++)
                    {
                        var transition = outputs[j];

                        transition.OnInspectorGUI(onValueChanged, () =>
                        {
                            outputs.RemoveAt(j);
                            j--;
                            onValueChanged();
                        });
                    }
                }

                showButton = showOutputs;
            }
            if(showButton && GUILayout.Button("Add Transition Check"))
            {
                outputs.Add(new TransitionCheck());
                outputs[outputs.Count - 1].showTransition = true;
                showOutputs = true;
                onValueChanged();
            }
        }

        void PaintEventList(Action onValueChanged)
        {
            bool showButton = true;
            if(events.Count > 0)
            {
                showEvents = EditorGUILayout.Foldout(showEvents, "Animation Events", true, AnimatorValidator.foldoutStyle);

                if(showEvents)
                {
                    for(int j = 0; j < events.Count; j++)
                    {
                        var eventInstance = events[j];

                        eventInstance.OnInspectorGUI(onValueChanged, () =>
                        {
                            events.RemoveAt(j);
                            j--;
                            onValueChanged();
                        });
                    }
                }

                showButton = showEvents;
            }

            if(showButton && GUILayout.Button("Add Animation Event Check"))
            {
                events.Add(new EventCheck());
                events[events.Count - 1].showEvent = true;
                showEvents = true;
                onValueChanged();
            }
        }
    }
}
