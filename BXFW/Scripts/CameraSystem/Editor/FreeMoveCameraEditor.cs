using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(FreeMoveCamera))]
    public class FreeMoveCameraEditor : Editor
    {
        private FreeMoveCamera Target;

        private void OnEnable()
        {
            Target = (FreeMoveCamera)target;
        }

        public override void OnInspectorGUI()
        {
            // Draw 'serialized objects'
            Tools.Editor.EditorAdditionals.DrawCustomDefaultInspector(serializedObject, new Dictionary<string, KeyValuePair<Tools.Editor.MatchGUIActionOrder, System.Action>>
            {
                // isEnabled is private variable.
                { "isEnabled", new KeyValuePair<Tools.Editor.MatchGUIActionOrder, System.Action>(Tools.Editor.MatchGUIActionOrder.OmitAndInvoke, () =>
                    {
                        EditorGUI.BeginChangeCheck();
                        var setIsEnabled = EditorGUILayout.Toggle(new GUIContent("Is Enabled", "Set whether if the free move camera is enabled."), Target.IsEnabled);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Target.IsEnabled = setIsEnabled;
                        }
                    })
                }
            });
        }
    }
}