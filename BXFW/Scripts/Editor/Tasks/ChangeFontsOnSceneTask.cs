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

            public TMP_FontAsset assetFontFilter;
            public Operation op = Operation.Exclude;
        }

        [SerializeField] public TMP_FontAsset fontAssetSet;
        [SerializeField, Tooltip("Filters of the font filter. By default all tmp text objects are to apply.")] public FontFilter[] filters;

        public override bool GetWarning()
        {
            return EditorUtility.DisplayDialog("[ChangeFontsOnSceneTask] Warning", $"This is going to modify fonts in the scene with '{fontAssetSet.name}' instead. Are you sure?", "Yes", "No");
        }

        public override void Run()
        {
            // Because unity is stubborn about the transform iterator
            GameObject[] objs = FindObjectsOfType<GameObject>(true);

            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];

                if (obj.TryGetComponent(out TMP_Text text))
                {
                    bool applyFont = true;

                    foreach (var filter in filters)
                    {
                        // Check font filter
                        if (filter.assetFontFilter == text.font)
                        {
                            switch (filter.op)
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
                        Debug.Log($"[ChangeFontsOnSceneTask] Applied font to object '{obj.GetPath()}'.");
                        text.font = fontAssetSet;
                    }
                }

                EditorUtility.DisplayProgressBar("Applying...", "Applying fonts.", i / (objs.Length - 1));
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorUtility.ClearProgressBar();
        }
    }
}
