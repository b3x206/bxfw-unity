using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(FreeMoveCamera), true), CanEditMultipleObjects]
    public class FreeMoveCameraEditor : Editor
    {
        private FreeMoveCamera[] Targets => targets.Cast<FreeMoveCamera>().ToArray();

        public override void OnInspectorGUI()
        {
            // Draw 'serialized objects'
            Tools.Editor.EditorAdditionals.DrawCustomDefaultInspector(serializedObject, new Dictionary<string, KeyValuePair<Tools.Editor.MatchGUIActionOrder, System.Action>>
            {
                // isEnabled is private variable.
                { "isEnabled", new KeyValuePair<Tools.Editor.MatchGUIActionOrder, System.Action>(Tools.Editor.MatchGUIActionOrder.OmitAndInvoke, () =>
                    {
                        EditorGUI.BeginChangeCheck();

                        bool targetIsEnabledTest = Targets[0].IsEnabled;
                        bool showMixed = EditorGUI.showMixedValue;
                        EditorGUI.showMixedValue = Targets.Any(c => c.IsEnabled != targetIsEnabledTest);
                        var setIsEnabled = EditorGUILayout.Toggle(new GUIContent("Is Enabled", "Set whether if the free move camera is enabled."), targetIsEnabledTest);
                        EditorGUI.showMixedValue = showMixed;

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.IncrementCurrentGroup();
                            Undo.SetCurrentGroupName("set enabled");
                            int undoID = Undo.GetCurrentGroup();
                            foreach (var target in Targets)
                            {
                                Undo.RecordObject(target, string.Empty);

                                target.IsEnabled = setIsEnabled;
                            }
                            Undo.CollapseUndoOperations(undoID);
                        }
                    })
                }
            });
        }
    }
}