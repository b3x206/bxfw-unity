using UnityEngine;

namespace BXFW.Data
{
    /// <summary>
    /// A scriptable object that contains an GUID pair.
    /// <br>The GUID is seperated using <see cref="ScriptableObjectIDUtility.OBJ_IDENTIFIER_PROPERTY_SEP"/>.</br>
    /// </summary>
    public class GUIDScriptableObject : ScriptableObject
    {
        /// <summary>
        /// UUID gathered on the editor, for loading with JSONUtility.
        /// </summary>
        [ReadOnlyView, SerializeField] private string m_GUID;
        /// <inheritdoc cref="m_GUID"/>
        public string GUID => m_GUID;

#if UNITY_EDITOR
        /// <summary>
        /// <c>[EDITOR ONLY]</c> Used to set the <see cref="m_GUID"/> value.
        /// </summary>
        protected virtual void OnValidate()
        {
            m_GUID = ScriptableObjectIDUtility.GetUnityObjectIdentifier(this);
        }
#endif
        /// <summary>
        /// Used to set the <see cref="m_GUID"/> value, but only in editor.
        /// <br>Calling the base is only mandatory for editor purposes.</br>
        /// </summary>
        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            m_GUID = ScriptableObjectIDUtility.GetUnityObjectIdentifier(this);
#endif
        }
    }
}
