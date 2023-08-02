using BXFW.UI;
using BXFW.Tools.Editor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(MultiUIManagerBase), true), CanEditMultipleObjects]
    public class MultiUIManagerBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
            var targets = base.targets.Cast<MultiUIManagerBase>().ToArray();

            dict.Add("m_ElementCount", new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
            {
                // Check targets, show mixed value
                int firstElemCount = targets[0].ElementCount;
                bool showMixedPrev = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = targets.Any(t => t.ElementCount != firstElemCount);

                // Set value (CanEditMultipleObjects)
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                int elemCountFieldValue = EditorGUILayout.IntField("Element Count", firstElemCount);
                if (GUILayout.Button("+", GUILayout.Width(20f)))
                {
                    elemCountFieldValue++;
                }
                if (GUILayout.Button("-", GUILayout.Width(20f)))
                {
                    elemCountFieldValue--;
                }
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("set element count");
                    int undoGroup = Undo.GetCurrentGroup();
                    foreach (var target in targets)
                    {
                        Undo.RecordObject(target, string.Empty);
                        target.ElementCount = elemCountFieldValue;
                    }
                    Undo.CollapseUndoOperations(undoGroup);
                }

                EditorGUI.showMixedValue = showMixedPrev;
            }));
            dict.Add("m_ReferenceElementIndex", new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
            {
                // Check targets, show mixed value
                int firstRefElemIndex = targets[0].ReferenceElementIndex;
                bool showMixedPrev = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = targets.Any(t => t.ReferenceElementIndex != firstRefElemIndex);

                // Set value (CanEditMultipleObjects)
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                int refElemIndexValue = EditorGUILayout.IntField("Reference Element Index", firstRefElemIndex);
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("set reference element index");
                    int undoGroup = Undo.GetCurrentGroup();
                    foreach (var target in targets)
                    {
                        Undo.RecordObject(target, string.Empty);
                        Undo.RecordObject(target.gameObject, string.Empty);

                        target.ReferenceElementIndex = refElemIndexValue;
                    }
                    Undo.CollapseUndoOperations(undoGroup);
                }

                EditorGUI.showMixedValue = showMixedPrev;
            }));
            dict.Add(nameof(MultiUIManagerBase.TruncateCloneNameOnCreate), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After, () =>
            {
                // Show action buttons for resetting, generation, etc.
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Regenerate"))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("regenerate multi ui manager");
                    int undoGroup = Undo.GetCurrentGroup();
                    foreach (var target in targets)
                    {
                        Undo.RecordObject(target, string.Empty);
                        Undo.RecordObject(target.gameObject, string.Empty);

                        target.GenerateElements();
                    }

                    Undo.CollapseUndoOperations(undoGroup);
                }
                if (GUILayout.Button("Reset"))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("reset multi ui manager");
                    int undoGroup = Undo.GetCurrentGroup();
                    foreach (var target in targets)
                    {
                        Undo.RecordObject(target, string.Empty);
                        Undo.RecordObject(target.gameObject, string.Empty);

                        target.ResetElements();
                    }

                    Undo.CollapseUndoOperations(undoGroup);
                }
                GUILayout.EndHorizontal();
            }));

            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}