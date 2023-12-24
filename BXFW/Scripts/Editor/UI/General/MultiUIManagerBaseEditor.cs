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
        protected static readonly KeyValuePair<MatchGUIActionOrder, Action> OMIT_ACTION = EditorAdditionals.OmitAction;
        /// <summary>
        /// List of GameObjects existing on the MultiUIManager.
        /// <br>Used to be able to register objects that were just created into the Undo stack.</br>
        /// </summary>
        protected readonly List<UnityEngine.Object> m_existingUndoRecord = new List<UnityEngine.Object>();

        /// <summary>
        /// Records a generative event for a MultiUIManager system targets array.
        /// <br>Can be overriden in case of a custom recording style.</br>
        /// </summary>
        /// <param name="generativeEvent">The event delegate called when the generative event is setup for recording.</param>
        /// <param name="undo">Undo message to send to the <see cref="Undo"/> stack.</param>
        protected virtual void UndoRecordGenerativeEvent(Action<MultiUIManagerBase> generativeEvent, string undo)
        {
            var targets = base.targets.Cast<MultiUIManagerBase>().ToArray();

            if (m_existingUndoRecord.Count > 0)
            {
                m_existingUndoRecord.Clear();
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(undo);
            int undoID = Undo.GetCurrentGroup();

            // Register all buttons into the undo record.
            foreach (var manager in targets)
            {
                foreach (Component element in manager.IterableElements())
                {
                    if (element == null)
                    {
                        continue;
                    }

                    m_existingUndoRecord.Add(element.gameObject);
                }
            }

            foreach (var manager in targets)
            {
                // Undo.RecordObject does not work, because unity wants to be unity.
                Undo.RegisterCompleteObjectUndo(manager, string.Empty);

                if (!PrefabUtility.IsPartOfAnyPrefab(manager))
                {
                    EditorUtility.SetDirty(manager);
                }

                generativeEvent(manager);

                if (PrefabUtility.IsPartOfAnyPrefab(manager))
                {
                    // RegisterCompleteObjectUndo does not immediately add the object into the Undo list
                    // So do this to avoid bugs, as this needs to be done after the undo list was updated.
                    EditorApplication.delayCall += () =>
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(manager);
                    };
                }
            }

            // Apply created elements on the undo thing
            foreach (var manager in targets)
            {
                foreach (
                    Component checkUndoElementComponent in manager
                        .IterableElements()
                        .Where(comp => !m_existingUndoRecord.Contains(comp.gameObject))
                )
                {
                    if (checkUndoElementComponent == null)
                    {
                        continue;
                    }

                    Undo.RegisterCreatedObjectUndo(checkUndoElementComponent.gameObject, string.Empty);
                }
            }

            Undo.CollapseUndoOperations(undoID);
        }

        /// <summary>
        /// Get the values for the dictionary.
        /// <br>Don't forget to call this method as <see langword="base"/>.<see cref="GetCustomPropertyDrawerDictionary(Dictionary{string, KeyValuePair{MatchGUIActionOrder, Action}}, MultiUIManagerBase[])"/></br>
        /// </summary>
        /// <param name="dict">The given dictionary for the property names and the behaviour to run on those.</param>
        /// <param name="targets">Array of targets for the <see cref="CanEditMultipleObjects"/> attribute editor.</param>
        protected virtual void GetCustomPropertyDrawerDictionary(Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict, MultiUIManagerBase[] targets)
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
                    UndoRecordGenerativeEvent((manager) => manager.ElementCount = elemCountFieldValue, "set element count");
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
                    UndoRecordGenerativeEvent((manager) => manager.GenerateElements(), "regenerate MultiUIManager");
                }
                if (GUILayout.Button("Reset"))
                {
                    UndoRecordGenerativeEvent((manager) => manager.ResetElements(false), "reset MultiUIManager");
                }
                if (GUILayout.Button("Clear + Reset"))
                {
                    UndoRecordGenerativeEvent((manager) => manager.ResetElements(true), "clear+reset MultiUIManager");
                }
                GUILayout.EndHorizontal();
            }));

            // Other things for the other BXFW classes
            // Yes, this is a bad way of adding other stuff but unity doesn't match open generic types for editors
            // As it doesn't serialize c# generic classes

            // MultiUIManager<TElement>
            // (for the time being don't draw this array)
            dict.Add("m_Elements", OMIT_ACTION);

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
