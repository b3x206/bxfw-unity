using BXFW.Tools.Editor;
using System;
using System.Linq;
using UnityEngine;

namespace BXFW.Tweening.Next.Editor
{
    /// <summary>
    /// Use this to generate type instance extension(s) for BXSTween.
    /// </summary>
    public class BXSTweenExtensionGeneratorTask : EditorTask
    {
        [Tooltip("The '.cs' is appended to the last file name."), EditDisallowChars("?<>:*|\"")]
        public string shorthandFileName = "Scripts/BXSTween/Extension/CustomExtension.cs";

        // The 'targetTypeName' should be ensured that it can be accessed from both BXFW and Assembly-CSharp?
        // Or just make it a local file lol and the people who know that there's no cyclic dependencies can just use it with that knowledge
        // (which is me, this is just a tool for generating for unity types)
        public SerializableSystemType targetExtensionType;
        public struct GenerateMethodInfo
        {
            public string methodName;
            public string PrefixedMethodName => $"BXTw{methodName}";
            [ReadOnlyView] public string interpObjectType;
            [ReadOnlyView] public string interpFieldName;

            private Type m_InterpolateObjectType;
            public Type InterpolateObjectType
            {
                get
                {
                    string strType = interpObjectType;
                    if (m_InterpolateObjectType == null)
                    {
                        m_InterpolateObjectType = TypeListProvider.GetDomainTypesByPredicate((Type t) => t.IsPublic && t.Name == strType).First();
                    }

                    return m_InterpolateObjectType;
                }
            }
        }
        public override void Run()
        {
            throw new NotImplementedException("TODO");
        }
    }
}
