using System.Collections.Generic;
using UnityEngine;

namespace BXFW.Data
{
    /// <summary>
    /// A scriptable object that contains an GUID pair.
    /// <br>The GUID is seperated using <see cref="ScriptableObjectIDSerializer.OBJ_IDENTIFIER_PROPERTY_SEP"/>.</br>
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
            m_GUID = ScriptableObjectIDSerializer.GetUnityObjectIdentifier(this);
        }
#endif
    }
}
