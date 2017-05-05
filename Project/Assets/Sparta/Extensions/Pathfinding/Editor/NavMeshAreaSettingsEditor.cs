using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SocialPoint.Utils;

namespace SocialPoint.Pathfinding
{
    public class AreaSettingsAttribute : PropertyAttribute
    {
    }

    [CustomPropertyDrawer(typeof(AreaSettingsAttribute))]
    public class NavMeshAreaSettingsEditor : PropertyDrawer
    {
        const float LineSeparation = 5.0f;

        List<string> _areas;
        List<string> _flags;
        string _newFlag = string.Empty;
        Dictionary<string, List<string>> _areaToFlagsMap;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, "Area Settings");
            ListAreas(ref position, property, label);
            ListFlags(ref position, property, label);
            ListAreaToFlagsMap(ref position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //title 
            float size = EditorGUIUtility.singleLineHeight; 

            //areas
            var areas = property.FindPropertyRelative("_areas");
            size += EditorGUI.GetPropertyHeight(areas);
            //areas comment
            size += EditorGUIUtility.singleLineHeight * 2;

            //flags title
            size += 2 * (EditorGUIUtility.singleLineHeight + LineSeparation);
            //flags
            var flagsCount = _flags == null ? 0 : _flags.Count;
            size += (EditorGUIUtility.singleLineHeight + LineSeparation) * flagsCount;
            //flag add
            size += EditorGUIUtility.singleLineHeight;
            //flags comment
            size += EditorGUIUtility.singleLineHeight * 2;

            //map title
            size += 2 * (EditorGUIUtility.singleLineHeight + LineSeparation);
            //flags
            if(_areaToFlagsMap != null)
            {
                var itr = _areaToFlagsMap.GetEnumerator();
                while(itr.MoveNext())
                {
                    size += (EditorGUIUtility.singleLineHeight + LineSeparation);
                    size += (EditorGUIUtility.singleLineHeight + LineSeparation) * itr.Current.Value.Count;
                }
                itr.Dispose();   
            }

            return size;
        }

        void ListAreas(ref Rect position, SerializedProperty property, GUIContent label)
        {
            var areas = property.FindPropertyRelative("_areas");
            var tags = areas.FindPropertyRelative("_tags");

            //Init areas from property if needed
            if(_areas == null)
            {
                _areas = new List<string>();
                FillListWithProperty(ref _areas, tags);
            }

            //Draw TagSet (let its own PropertyDrawer to do the job)
            AddLineSeparation(ref position);
            var fieldPosition = position;
            float fieldHeight = EditorGUI.GetPropertyHeight(areas);
            fieldPosition.size = new Vector2(position.size.x, EditorGUIUtility.singleLineHeight);
            position.y += fieldHeight;
            EditorGUI.PropertyField(fieldPosition, areas);

            //Add description
            AddSingleLine(ref position);
            AddDescription(position, "IMPORTANT: Areas must be the first Tag in convex volumes export config");
            AddLineSeparation(ref position);

            //Copy areas from updated property
            FillListWithProperty(ref _areas, tags);
        }

        void ListFlags(ref Rect position, SerializedProperty property, GUIContent label)
        {
            var flags = property.FindPropertyRelative("_flags");

            //Init flags from property if needed
            if(_flags == null)
            {
                _flags = new List<string>();
                FillListWithProperty(ref _flags, flags);
            }

            //Draw flags list
            AddLineSeparation(ref position);
            List<string> toRemoveFlags;
            Rect flagsPosition;
            DrawFlags(ref position, out flagsPosition, out toRemoveFlags);
            DrawAddFlagField(ref position, flagsPosition);

            //Add description
            AddLineSeparation(ref position);
            AddDescription(position, "Flags will bitmask-filter polygons for navigation queries");
            AddLineSeparation(ref position);

            //Clear unused
            RemoveFlags(toRemoveFlags);
            //Save values back to property
            FillPropertyWithList(ref flags, _flags);
        }

        void ListAreaToFlagsMap(ref Rect position, SerializedProperty property, GUIContent label)
        {
            var mapAreas = property.FindPropertyRelative("_mapAreas");
            var mapFlags = property.FindPropertyRelative("_mapFlags");

            //Init map data from properties if needed
            if(_areaToFlagsMap == null)
            {
                _areaToFlagsMap = new Dictionary<string, List<string>>();
                FillMapWithProperties(ref _areaToFlagsMap, mapAreas, mapFlags);
            }
            //Update unique areas
            UpdateAreasInMap();

            //Draw title
            AddLineSeparation(ref position);
            EditorGUI.LabelField(position, "Area-Flags Map");
            AddLineSeparation(ref position);

            //Draw map
            DrawMap(ref position);

            //Save values back to properties
            FillPropertiesWithMap(ref mapAreas, ref mapFlags, _areaToFlagsMap);
        }

        void DrawFlags(ref Rect position, out Rect flagsPosition, out List<string> toRemoveFlags)
        {
            toRemoveFlags = new List<string>();
            flagsPosition = EditorGUI.PrefixLabel(position, new GUIContent("Flags"));
            var w = position.size.x;

            var itr = _flags.GetEnumerator();
            while(itr.MoveNext())
            {
                var flag = itr.Current;
                EditorGUI.LabelField(flagsPosition, flag);
                var buttonPosition = flagsPosition;
                buttonPosition.size = new Vector2(30.0f, EditorGUIUtility.singleLineHeight);
                buttonPosition.x = w - buttonPosition.size.x;
                if(GUI.Button(buttonPosition, "-"))
                {
                    toRemoveFlags.Add(flag);
                }
                AddLineSeparation(ref flagsPosition);
                AddLineSeparation(ref position);
            }
            itr.Dispose();
        }

        void DrawAddFlagField(ref Rect position, Rect flagsPosition)
        {
            var w = position.size.x;
            var inputPosition = flagsPosition;
            var buttonPosition = flagsPosition;
            buttonPosition.size = new Vector2(40.0f, EditorGUIUtility.singleLineHeight);
            buttonPosition.x = w - buttonPosition.size.x;
            inputPosition.size = new Vector2(flagsPosition.size.x - buttonPosition.size.x - 30.0f, EditorGUIUtility.singleLineHeight);

            _newFlag = EditorGUI.TextField(inputPosition, _newFlag);
            if(GUI.Button(buttonPosition, "Add"))
            {
                if(!string.IsNullOrEmpty(_newFlag) && !_flags.Contains(_newFlag))
                {
                    _flags.Add(_newFlag);
                    _newFlag = string.Empty;
                }
            }
        }

        void RemoveFlags(List<string> toRemoveFlags)
        {
            var itr = toRemoveFlags.GetEnumerator();
            while(itr.MoveNext())
            {
                _flags.Remove(itr.Current);

                var mapItr = _areaToFlagsMap.GetEnumerator();
                while(mapItr.MoveNext())
                {
                    mapItr.Current.Value.Remove(itr.Current);
                }
                mapItr.Dispose();
            }
            itr.Dispose();
        }

        void UpdateAreasInMap()
        {
            {
                var itr = _areas.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(!_areaToFlagsMap.ContainsKey(itr.Current))
                    {
                        _areaToFlagsMap.Add(itr.Current, new List<string>());
                    }
                }
                itr.Dispose();
            }

            var areasToRemove = new List<string>();
            {
                var itr = _areaToFlagsMap.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(!_areas.Contains(itr.Current.Key))
                    {
                        areasToRemove.Add(itr.Current.Key);
                    }
                }
                itr.Dispose();
            }

            {
                var itr = areasToRemove.GetEnumerator();
                while(itr.MoveNext())
                {
                    _areaToFlagsMap.Remove(itr.Current);
                }
                itr.Dispose();
            }
        }

        void DrawMap(ref Rect position)
        {
            var itr = _areaToFlagsMap.GetEnumerator();
            while(itr.MoveNext())
            {
                DrawMapEntry(ref position, itr.Current);
            }
            itr.Dispose();
        }

        void DrawMapEntry(ref Rect position, KeyValuePair<string, List<string>> mapEntry)
        {
            var area = mapEntry.Key;
            var flags = mapEntry.Value;

            //Draw area name
            var w = position.size.x;
            var areaPosition = position;
            areaPosition.size = new Vector2(w * 0.4f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(areaPosition, area);

            //Draw flags list
            Rect flagsPosition;
            List<string> toRemoveFlags;
            DrawMapFlagsList(area, flags, ref position, out flagsPosition, out toRemoveFlags);

            //Update flags
            RemoveMapFlags(area, toRemoveFlags);

            //Draw selector for flags
            DrawAddMapFlagSelector(area, ref position, flagsPosition);
        }

        void DrawMapFlagsList(string area, List<string> flagsList, ref Rect position, out Rect flagsPosition, out List<string> toRemoveFlags)
        {
            var w = position.size.x;
            toRemoveFlags = new List<string>();
            flagsPosition = position;
            flagsPosition.x += w * 0.5f;
            flagsPosition.size = new Vector2(w * 0.3f, EditorGUIUtility.singleLineHeight);
            var buttonPosition = position;
            buttonPosition.x += w * 0.85f;
            buttonPosition.size = new Vector2(w * 0.15f, EditorGUIUtility.singleLineHeight);
            var flagItr = flagsList.GetEnumerator();
            while(flagItr.MoveNext())
            {
                var flag = flagItr.Current;
                EditorGUI.LabelField(flagsPosition, flag);
                if(GUI.Button(buttonPosition, "-"))
                {
                    toRemoveFlags.Add(flag);
                }
                AddLineSeparation(ref buttonPosition);
                AddLineSeparation(ref flagsPosition);
                AddLineSeparation(ref position);
            }
            flagItr.Dispose();
        }

        void DrawAddMapFlagSelector(string area, ref Rect position, Rect flagsPosition)
        {
            var availableFlags = new List<string>(_flags.Count + 1);
            availableFlags.Add("");
            var availableItr = _flags.GetEnumerator();
            while(availableItr.MoveNext())
            {
                var flag = availableItr.Current;
                if(!_areaToFlagsMap[area].Contains(flag))
                {
                    availableFlags.Add(flag);
                }
            }
            availableItr.Dispose();
            var idx = EditorGUI.Popup(flagsPosition, "", 0, availableFlags.ToArray());
            if(idx != 0)
            {
                _areaToFlagsMap[area].Add(availableFlags[idx]);
            }
            AddLineSeparation(ref position);
        }

        void RemoveMapFlags(string area, List<string> toRemoveFlags)
        {
            var removeItr = toRemoveFlags.GetEnumerator();
            while(removeItr.MoveNext())
            {
                var flag = removeItr.Current;
                _areaToFlagsMap[area].Remove(flag);
            }
            removeItr.Dispose();
        }

        static void AddSingleLine(ref Rect position)
        {
            position.y += EditorGUIUtility.singleLineHeight;
        }

        static void AddLineSeparation(ref Rect position)
        {
            position.y += EditorGUIUtility.singleLineHeight + LineSeparation;
        }

        static void AddDescription(Rect position, string text)
        {
            var descStyle = new GUIStyle();
            descStyle.fontStyle = FontStyle.Italic;
            descStyle.normal.textColor = Color.gray;
            EditorGUI.LabelField(position, text, descStyle);
        }

        static void FillListWithProperty(ref List<string> list, SerializedProperty property)
        {
            list.Clear();
            for(var i = 0; i < property.arraySize; i++)
            {
                var value = property.GetArrayElementAtIndex(i).stringValue;
                if(!list.Contains(value))
                {
                    list.Add(value);
                }
            }
        }

        static void FillPropertyWithList(ref SerializedProperty property, List<string> list)
        {
            property.ClearArray();
            property.arraySize = list.Count;
            int i = 0;
            var itr = list.GetEnumerator();
            while(itr.MoveNext())
            {
                property.GetArrayElementAtIndex(i).stringValue = itr.Current;
                ++i;
            }
            itr.Dispose();
        }

        static void FillMapWithProperties(ref Dictionary<string, List<string>> map, SerializedProperty keyProperty, SerializedProperty valueProperty)
        {
            map.Clear();
            for(var i = 0; i < keyProperty.arraySize; i++)
            {
                var key = keyProperty.GetArrayElementAtIndex(i).stringValue;
                var value = valueProperty.GetArrayElementAtIndex(i).stringValue;
                if(!map.ContainsKey(key))
                {
                    map.Add(key, new List<string>());
                }
                map[key].Add(value);
            }
        }

        static void FillPropertiesWithMap(ref SerializedProperty keyProperty, ref SerializedProperty valueProperty, Dictionary<string, List<string>> map)
        {
            keyProperty.ClearArray();
            valueProperty.ClearArray();
            int totalMaps = 0;
            var mapItr = map.GetEnumerator();
            while(mapItr.MoveNext())
            {
                totalMaps += mapItr.Current.Value.Count;
            }
            mapItr.Dispose(); 
            keyProperty.arraySize = totalMaps;
            valueProperty.arraySize = totalMaps;

            int i = 0;
            var keyItr = map.GetEnumerator();
            while(keyItr.MoveNext())
            {
                string keyName = keyItr.Current.Key;
                var valueItr = keyItr.Current.Value.GetEnumerator();
                while(valueItr.MoveNext())
                {
                    string valueName = valueItr.Current;
                    keyProperty.GetArrayElementAtIndex(i).stringValue = keyName;
                    valueProperty.GetArrayElementAtIndex(i).stringValue = valueName;
                    ++i;
                }
                valueItr.Dispose();
            }
            keyItr.Dispose();
        }
    }
}
