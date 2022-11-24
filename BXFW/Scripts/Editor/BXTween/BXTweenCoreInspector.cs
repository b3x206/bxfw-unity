using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using BXFW.Tweening;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(BXTweenCore))]
    public class BXTweenCoreInspector : Editor
    {
        private GUIStyle headerTextStyle;

        private void OnEnable()
        {
            headerTextStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22
            };

            // Put repaint of this inspector to update to get realtime data viewed on inspector.
            EditorApplication.update += Repaint;
        }
        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        /// <summary>
        /// Interface filtering filter.
        /// </summary>
        private struct TweenFilter
        {
            public bool IgnoreNullTargetObject;
            public Object TargetObject;

            public bool ShouldFilter(ITweenCTX tw)
            {
                return (IgnoreNullTargetObject && tw.TargetObject == null) || tw.TargetObject != TargetObject;
            }
        }

        private TweenFilter currentFilter;

        private GUIStyle boxStyle;
        private GUIStyle buttonStyle;

        private Vector2 runningTwScroll;
        private bool expandFilterDebugTweens = false;
        private readonly List<bool> expandedTweens = new List<bool>();
        public override void OnInspectorGUI()
        {
            boxStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button);

            // Draw a field for 'm_Script'
            var gEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            // Draw ReadOnly status properties 
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("[BXTween] BXTween (for now) only works in runtime.", MessageType.Warning);
                GUI.enabled = gEnabled;
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("-- [BXTween] --", headerTextStyle, GUILayout.Height(40f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            // Draw stats from the CTween class
            EditorGUILayout.LabelField(string.Format("Tween Amount (running) = {0}", BXTween.CurrentRunningTweens.Count));
            // more of a placeholder here.
            EditorGUILayout.LabelField(string.Format("Core Status = {0}", BXTween.CheckStatus() ? "OK" : "Error (null?)"));

            // Re enable (or disable, we don't know) the GUI.
            GUI.enabled = gEnabled;

            const float scrollAreaHeight = 250;

            // Draw filter button
            expandFilterDebugTweens = GUILayout.Toggle(expandFilterDebugTweens, "Filter", buttonStyle, GUILayout.Width(70));
            if (expandFilterDebugTweens)
            {
                EditorGUI.indentLevel += 2;

                // Draw filter tweens area
                currentFilter.IgnoreNullTargetObject = EditorGUILayout.Toggle("Ignore Null Target Object", currentFilter.IgnoreNullTargetObject);
                currentFilter.TargetObject = EditorGUILayout.ObjectField("Target Object", currentFilter.TargetObject, typeof(Object), true);
                
                EditorGUI.indentLevel -= 2;
            }

            runningTwScroll = GUILayout.BeginScrollView(runningTwScroll, GUILayout.Height(scrollAreaHeight));
            EditorGUI.indentLevel += 2;
            // Draw the list of current running tweens (with name)
            for (int i = 0; i < BXTween.CurrentRunningTweens.Count; i++)
            {
                ITweenCTX tw = BXTween.CurrentRunningTweens[i];

                if (i > expandedTweens.Count - 1)
                    expandedTweens.Add(false);

                if (currentFilter.ShouldFilter(tw))
                    continue;

                expandedTweens[i] = GUILayout.Toggle(expandedTweens[i], $"Tween {i,2} | Type={tw.TweenedType}, Target={tw.TargetObject}", boxStyle);

                if (expandedTweens[i])
                {
                    // Show more information about the tween
                    // Assume that this type is BXTweenCTX, but the generic is unknown
                    // Interfaces always return concrete type : So GetType is used.
                    foreach (var v in tw.GetType().GetProperties())
                    {
                        EditorGUILayout.LabelField(string.Format("[Property] {0}:::{1} = {2}", v.Name, v.PropertyType, v.GetValue(tw)));
                    }
                    foreach (var v in tw.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        EditorGUILayout.LabelField(string.Format("[Field] {0}:::{1} = {2}", v.Name, v.FieldType, v.GetValue(tw)));
                    }
                }
            }
            EditorGUI.indentLevel -= 2;
            GUILayout.EndScrollView();
        }
    }
}