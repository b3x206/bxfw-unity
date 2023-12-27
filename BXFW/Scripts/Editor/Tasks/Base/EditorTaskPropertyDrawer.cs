using UnityEditor;
using BXFW.Tools.Editor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Property drawer for any GeneratorTask.
    /// </summary>
    [CustomPropertyDrawer(typeof(EditorTask), true)]
    public class EditorTaskPropertyDrawer : ScriptableObjectFieldInspector<EditorTask>
    {
        protected override HideFlags DefaultHideFlags => HideFlags.HideInHierarchy | HideFlags.DontSave;
    }
}
