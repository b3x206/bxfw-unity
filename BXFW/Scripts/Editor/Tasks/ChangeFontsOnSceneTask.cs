using TMPro;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Allows for mass changing the fonts in the currently open scenes.
    /// <br>Only supports <see cref="TMP_Text"/>'s.</br>
    /// </summary>
    public class ChangeFontsOnSceneTask : EditorTask
    {
        /// <summary>
        /// A font asset filter.
        /// <br>Ignored if 'assetFontFilter' is left blank.</br>
        /// </summary>
        [Serializable]
        public class FontFilter
        {
            public enum Operation
            {
                Exclude,
                Include
            }

            public TMP_FontAsset assetFilter;
            public Operation operation = Operation.Exclude;
        }

        [Tooltip("Target font to set.")]
        public TMP_FontAsset fontAssetSet;
        [Tooltip("Filters of the font filter. By default all tmp text objects are to apply.")]
        public FontFilter[] filters;
        [Tooltip("Parent GameObject to modify it's children TextMeshPro's fonts.")]
        public GameObject parentTransform;
        [Tooltip("Whether to record applications as collapsed into the undo stack.")]
        public bool recordUndo = true;

        public override bool GetWarning()
        {
            return EditorUtility.DisplayDialog("[ChangeFontsOnSceneTask] Warning", $"This is going to modify all TextMeshPro fonts in the '{(parentTransform != null ? parentTransform.name : "scene")}' with '{fontAssetSet.name}' instead. Are you sure?", "Yes", "No");
        }

        private void ApplyFontWithFilters(TMP_Text text)
        {
            bool applyFont = true;

            foreach (FontFilter filter in filters)
            {
                // Check font filter
                if (filter.assetFilter == text.font)
                {
                    switch (filter.operation)
                    {
                        case FontFilter.Operation.Exclude:
                            applyFont = false;
                            break;
                        case FontFilter.Operation.Include:
                            applyFont = true;
                            break;
                    }

                    break;
                }
            }

            if (applyFont)
            {
                Debug.Log($"[ChangeFontsOnSceneTask] Applied font to object '{text.GetPath()}'.");
                if (recordUndo)
                {
                    Undo.RecordObject(text, string.Empty);
                }
                else
                {
                    // Set the object dirty/modified if not going to record undo
                    EditorUtility.SetDirty(text);
                }

                text.font = fontAssetSet;
            }
        }

        public override void Run()
        {
            int currentUndoGroup = -1;
            if (recordUndo)
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("ChangeFontsOnSceneTask");
                currentUndoGroup = Undo.GetCurrentGroup();
            }

            // A simple 'GetComponentsInChildren' will work for a specified transform parent.
            if (parentTransform != null)
            {
                // TODO (?) : This may or may not be an arbitary limitation
                // But prefabs are finicky in unity so only allow application of font change while inside the exact prefab scene.
                bool isPrefab = PrefabUtility.GetCorrespondingObjectFromSource(parentTransform) != null;
                if (isPrefab)
                {
                    // Check if on a prefab stage
                    PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                    if (stage == null)
                    {
                        throw new InvalidOperationException($"Cannot change fonts of a prefab while not inside a prefab stage. Prefab was \"{parentTransform}\".");
                    }
                    if (!stage.IsPartOfPrefabContents(parentTransform))
                    {
                        throw new InvalidOperationException($"Cannot change fonts of a prefab while not inside the prefab stage for parentTransform. Prefab was \"{parentTransform}\".");
                    }
                }

                TMP_Text[] modifyTexts = parentTransform.GetComponentsInChildren<TMP_Text>(true);

                for (int i = 0; i < modifyTexts.Length; i++)
                {
                    ApplyFontWithFilters(modifyTexts[i]);
                    EditorUtility.DisplayProgressBar("Applying...", "Applying fonts.", i / (modifyTexts.Length - 1));
                }

                if (recordUndo)
                {
                    Undo.CollapseUndoOperations(currentUndoGroup);
                }

                EditorUtility.ClearProgressBar();
                return;
            }

            // -- Default scene root behaviour
            // Because unity is stubborn about the transform iterator and getting the scene root is not a thing in unity
            GameObject[] objs = FindObjectsOfType<GameObject>(true);

            for (int i = 0; i < objs.Length; i++)
            {
                GameObject obj = objs[i];

                if (obj.TryGetComponent(out TMP_Text text))
                {
                    ApplyFontWithFilters(text);
                }

                EditorUtility.DisplayProgressBar("Applying...", "Applying fonts.", i / (objs.Length - 1));
            }

            if (recordUndo)
            {
                Undo.CollapseUndoOperations(currentUndoGroup);
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorUtility.ClearProgressBar();
        }
    }
}
