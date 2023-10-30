using BXFW.Tools.Editor;
using System;
using System.Linq;
using System.Reflection;
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

        // The 'targetTypeName' should be ensured that it can be accessed from both BXFW and Assembly-CSharp.
        [SerializeField] private string targetTypeName = string.Empty;
        private const AssemblyFlags TypeListAsmFlags = AssemblyFlags.BXFW | AssemblyFlags.BXFWEditor 
            | AssemblyFlags.AssemblyCSharp | AssemblyFlags.AssemblyCSharpEditor 
            | AssemblyFlags.UnityAssembly | AssemblyFlags.AssetScript;
        public Type TargetType
        {
            get
            {
                return TypeListProvider.GetDomainTypesByPredicate((Type t) => t.IsPublic && t.Name == targetTypeName, TypeListAsmFlags).First();
            }
        }
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
