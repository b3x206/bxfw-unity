using UnityEditor;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Property drawer for any GeneratorTask.
    /// </summary>
    [CustomPropertyDrawer(typeof(EditorTask), true)]
    public class EditorTaskPropertyDrawer : ScriptableObjectFieldInspector<EditorTask>
    { }
}
