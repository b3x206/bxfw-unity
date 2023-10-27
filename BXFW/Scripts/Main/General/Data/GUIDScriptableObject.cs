using UnityEngine;

namespace BXFW.Data
{
    /// <summary>
    /// A scriptable object that contains an GUID+FileID pair.
    /// <br>The GUID+FileID is seperated using <see cref="ObjIdentifierValueSeperator"/>.</br>
    /// </summary>
    public abstract class GUIDScriptableObject : ScriptableObject
    {
        /// <summary>
        /// UUID gathered on the editor, for loading with JSONUtility.
        /// </summary>
        [ReadOnlyView, SerializeField] private string m_GUID;
        /// <inheritdoc cref="m_GUID"/>
        public string GUID
        {
            get
            {
#if UNITY_EDITOR
                if (m_GUID == $"{ObjIdentifierValueSeperator}0")
                {
                    if (!Application.isPlaying)
                    {
                        m_GUID = Editor.UnityObjectIDUtility.GetUnityObjectIdentifier(this, ObjIdentifierValueSeperator);
                    }
                }
#endif
                return m_GUID;
            }
        }

        /// <summary>
        /// The seperator for the GUID+FileID values on the string respectively.
        /// <br>Before this string =&gt; GUID</br>
        /// <br>After this string  =&gt; FileID</br>
        /// </summary>
        protected const string ObjIdentifierValueSeperator = "::";

#if UNITY_EDITOR
        /// <summary>
        /// <c>[EDITOR ONLY]</c> Used to set the <see cref="m_GUID"/> value.
        /// </summary>
        protected virtual void OnValidate()
        {
            m_GUID = Editor.UnityObjectIDUtility.GetUnityObjectIdentifier(this, ObjIdentifierValueSeperator);
        }
#endif
        /// <summary>
        /// Used to set the <see cref="m_GUID"/> value, but only in editor.
        /// <br>Calling the base is only mandatory for editor purposes.</br>
        /// </summary>
        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            m_GUID = Editor.UnityObjectIDUtility.GetUnityObjectIdentifier(this, ObjIdentifierValueSeperator);
#endif
        }
    }
}
