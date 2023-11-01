using BXFW.Tools.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BXFW.Tweening.Next.Editor
{
    /// <summary>
    /// Use this to generate type instance extension(s) for BXSTween.
    /// </summary>
    public class BXSTweenExtensionGeneratorTask : EditorTask
    {
        /// <summary>
        /// Returns whether if the given type is assignable from generic type <paramref name="genericType"/>.
        /// </summary>
        private static bool IsAssignableFromOpenGeneric(Type target, Type genericType)
        {
            // target      => given type
            // genericType => Generic<> (open type)

            // Can be assigned using interface
            if (genericType.GetInterfaces().Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            // Can be assigned directly (with open type)
            if (target.IsGenericType && target.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            // Reached end of base type
            if (target.BaseType == null)
            {
                return false;
            }

            return IsAssignableFromOpenGeneric(target.BaseType, genericType);
        }

        /// <summary>
        /// Returns whether if the given type has a <see cref="BXSTweenContext{TValue}"/> defined for it.
        /// </summary>
        public static bool IsValidContextType(Type t)
        {
            return IsAssignableFromOpenGeneric(t, typeof(BXSTweenContext<>));
        }

        [Serializable]
        public class GenerateMethodInfo
        {
            /// <summary>
            /// Match everything except ascii characters, numbers and underscores.
            /// <br>MethodName can start with numbers, it will be prefixed with <see cref="MethodNamePrefix"/>.</br>
            /// </summary>
            private const string ReMatchMethodName = "[^a-zA-Z0-9_]";
            /// <summary>
            /// Method name to prefix.
            /// </summary>
            public const string MethodNamePrefix = "BXTw";
            [SerializeField, EditDisallowChars(ReMatchMethodName, isRegex = true)]
            private string m_MethodName;
            public string MethodName
            {
                get
                {
                    return m_MethodName;
                }
                set
                {
                    Regex re = new Regex(ReMatchMethodName, RegexOptions.Multiline);
                    value = re.Replace(value, string.Empty);

                    m_MethodName = value;
                }
            }

            [ReadOnlyView, SerializeField] private string targetFieldIdentifier;
            public MemberInfo[] GetTargetInfos(Type t)
            {
                return t.GetMember(targetFieldIdentifier, BindingFlags.Instance | BindingFlags.Public);
            }
        }
        [Serializable]
        public class ExtensionFileTemplate
        {
            // The 'targetTypeName' should be ensured that it can be accessed from both BXFW and Assembly-CSharp?
            // Or just make it a local file lol and the people who know that there's no cyclic dependencies can just use it with that knowledge
            // (which is me, this is just a tool for generating for unity types)
            public SerializableSystemType targetType;
            public List<GenerateMethodInfo> extensionMethods;
        }

        private string UnityAssetsPath => Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        private string PrefixedFileRelativePath
        {
            get
            {
                string fileName = tweenExtensionsFileName;
                if (!fileName.EndsWith(".cs"))
                {
                    fileName += ".cs";
                }
                return fileName;
            }
        }
        private string GenerateFileAbsolutePath => Path.Combine(UnityAssetsPath, PrefixedFileRelativePath);

        [Tooltip("The '.cs' is appended to the last file name. The directory is local."), EditDisallowChars("?<>:*|\"")]
        public string tweenExtensionsFileName = "Scripts/BXSTween/Extension/CustomExtension.cs";
        /// <summary>
        /// Same as <see cref="GenerateMethodInfo.ReMatchMethodName"/> but with dots allowed and no numbers on start of string.
        /// </summary>
        private const string ReMatchNamespaceName = "[^_.a-zA-Z0-9_]|^[\\d]+";
        [EditDisallowChars(ReMatchNamespaceName, isRegex = true)]
        public string fileNamespace = "BXFW.Tweening.Next";
        public string fileClassName = "BXSTweenExtensions";

        /// <summary>
        /// List of extension pairs to generate.
        /// </summary>
        public List<ExtensionFileTemplate> extensionPairs = new List<ExtensionFileTemplate>();

        private const string TkUsingTemplate = "using {0};";
        private const string TkNamespace = "namespace";
        private const string TkPublic = "public";
        private const string TkStatic = "static";
        private const string TkPartial = "partial";
        private const string TkClass = "class";
        private const string TkIndent = "    ";
        private const char TkOpenScope = '{';
        private const char TkCloseScope = '}';
        private const char TkSemicolon = ';';
        // Use StringBuilder.AppendLine to get the environment's preferred line ending
        /// <summary>
        /// List of namespaces that are currently used.
        /// <br>This will be reset every run.</br>
        /// </summary>
        private List<string> currentUsings = new List<string>();

        public override bool GetWarning()
        {
            if (string.IsNullOrWhiteSpace(fileClassName))
            {
                EditorUtility.DisplayDialog("BXSTweenExtensionGeneratorTask [Error]", "Given class name is invalid.", "Ok");
                return false;
            }
            if (string.IsNullOrWhiteSpace(tweenExtensionsFileName))
            {
                EditorUtility.DisplayDialog("BXSTweenExtensionGeneratorTask [Error]", "Given file directory/name is invalid.", "Ok");
                return false;
            }

            if (File.Exists(GenerateFileAbsolutePath))
            {
                if (!EditorUtility.DisplayDialog("BXSTweenExtensionGeneratorTask [Warning]", $"Given file path \"{PrefixedFileRelativePath}\" already exists, do you want to overwrite the file?\nNote that the file to be overwritten will be backed up with '.bak' extension, but you can use version control.", "Yes", "No"))
                {
                    return false;
                }
            }

            return base.GetWarning();
        }

        public override void Run()
        {
            // -- Reset state
            currentUsings.Clear();

            // -- Start building
            StringBuilder sb = new StringBuilder(extensionPairs.Count * 368);
            bool hasNamespaceDefinition = !string.IsNullOrWhiteSpace(fileNamespace);
            string namespaceIndent = hasNamespaceDefinition ? TkIndent : string.Empty;
            if (hasNamespaceDefinition)
            {
                // append file stuff (namespace 'fileNameSpace')
                sb.AppendLine().AppendLine(TkNamespace).AppendLine(fileNamespace)
                    .Append(TkOpenScope).AppendLine();
            }
            // partial extensions class definition (public static partial class {fileClassName})
            sb.Append(namespaceIndent).Append(TkPublic).Append(TkStatic).Append(TkPartial).Append(TkClass).Append(fileClassName).AppendLine();
            sb.Append(namespaceIndent).Append(TkOpenScope).AppendLine(); // {
            // function definitions
            foreach (ExtensionFileTemplate template in extensionPairs)
            {

            }
            // end class definition + 

        }
    }
}
