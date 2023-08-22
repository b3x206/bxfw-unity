using BXFW.UI;
using BXFW.Tools.Editor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// The multi UI manager editor.
    /// <br>Override from this class to be able to edit <see cref="MultiUIManager{TElement}"/>s properly.</br>
    /// </summary>
    [CustomEditor(typeof(MultiUIManagerBase), true), CanEditMultipleObjects]
    public class MultiUIManagerBaseEditor : Editor
    {
        /// <summary>
        /// Omits the drawn value in the dictionary.
        /// </summary>
        protected static readonly KeyValuePair<MatchGUIActionOrder, Action> OMIT_ACTION = new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null); 

        /// <summary>
        /// Get the values for the dictionary.
        /// <br>Don't forget to call this method as <see langword="base"/>.<see cref="GetCustomPropertyDrawerDictionary(in Dictionary{string, KeyValuePair{MatchGUIActionOrder, Action}}, MultiUIManagerBase[])"/></br>
        /// </summary>
        /// <param name="dict">The given dictionary for the property names and the behaviour to run on those.</param>
        /// <param name="targets">Array of targets for the <see cref="CanEditMultipleObjects"/> attribute editor.</param>
        protected virtual void GetCustomPropertyDrawerDictionary(in Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict, MultiUIManagerBase[] targets)
        {
            dict.Add("m_ElementCount", new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
            {
                // Check targets, show mixed value
                int firstElemCount = targets[0].ElementCount;
                bool showMixedPrev = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = targets.Any(t => t.ElementCount != firstElemCount);

                // Set value (CanEditMultipleObjects)
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                int elemCountFieldValue = EditorGUILayout.IntField(
                    new GUIContent("Element Count", "The count of the current elements in this MultiUIManager.\nChanging this will spawn in more elements."),
                    firstElemCount
                );
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
                int refElemIndexValue = EditorGUILayout.IntField(
                    new GUIContent("Reference Element Index", "The index of the element to spawn/instantiate in when the 'Element Count' is changed."),
                    firstRefElemIndex
                );
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

            // Other things for the other BXFW classes
            // Yes, this is a bad way of adding other stuff but unity doesn't match open generic types for editors
            // As it doesn't serialize c# generic classes

            // MultiUIManager<TElement>
            // (for the time being don't draw this array)
            dict.Add("uiElements", OMIT_ACTION);

            // InteractableMultiUIManager<TElement>
            dict.Add("interactable", new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
            {
                using SerializedProperty targetInteractable = serializedObject.FindProperty("interactable");

                // We have to set 'Interactable' property instead of the 'SerializedProperty'
                // But fortunately the only callback this does is that 'UpdateElementsAppearance' which exists on the base
                // The only thing that we have to do (probably) is to call that. (which will update interactability accordingly)
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(targetInteractable);
                if (EditorGUI.EndChangeCheck())
                {
                    // Call this as the bool value is not up to date
                    targetInteractable.serializedObject.ApplyModifiedProperties();
                    foreach (var target in targets)
                    {
                        target.UpdateElementsAppearance();
                    }
                }
            }));
        }

        public override void OnInspectorGUI()
        {
            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
            var targets = base.targets.Cast<MultiUIManagerBase>().ToArray();
            GetCustomPropertyDrawerDictionary(dict, targets);

            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}
