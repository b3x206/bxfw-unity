using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// <c>[EDITOR ONLY]</c>
    /// Events to apply when a generation proccess is going on.
    /// <br>All classes that inherit and implement <see cref="EditorTask"/> is scanned and automatically applied.</br>
    /// <br>You can also pass datas for events, you can create custom field inspectors for all 'GeneratorEvents' 
    /// using BXFW's <see cref="ScriptableObjectFieldInspector{T}"/> or your own thing.</br>
    /// <br>Overriding this will allow your class to be usable as a task.</br>
    /// </summary>
    [System.Serializable]
    public abstract class EditorTask : ScriptableObject
    {
        /// <summary>
        /// Print out the warnings with this editor task.
        /// <br>No 'Run' should be called on this method.</br>
        /// </summary>
        /// <returns>Return <see langword="true"/> if the warning was acknowledged.</returns>
        public virtual bool GetWarning()
        {
            return true;
        }

        /// <summary>
        /// Called when the task should be run.
        /// </summary>
        public abstract void Run();
    }
}
