using System;
using System.IO;
using System.Linq;
using System.Text;
using BXFW.Tools.Editor;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BXFW.Tweening.Next.Editor
{
    /// <summary>
    /// Use this to generate type instance extension(s) for BXSTween.
    /// <br>Doesn't support generic classes for the time being.</br>
    /// </summary>
    public class BXSTweenExtensionGeneratorTask : EditorTask
    {
        /// <summary>
        /// Returns the target context type for the given type <paramref name="t"/>.
        /// </summary>
        public static Type ReturnContextTypeForType(Type t)
        {
            Type[] bxsContextTypes = TypeListProvider.GetDomainTypesByPredicate((Type checkType) => checkType.IsAssignableFromOpenGeneric(typeof(BXSTweenContext<>)));

            foreach (Type twType in bxsContextTypes)
            {
                if (twType.IsAbstract)
                {
                    continue;
                }

                // Check if the given type is a valid generic parameter.
                KeyValuePair<Type, Type[]> contextInherit = twType.GetBaseGenericTypeArguments().FirstOrDefault((p) => p.Key == typeof(BXSTweenContext<>));
                if (contextInherit.Value.Contains(t))
                {
                    return twType;
                }
            }

            return null;
        }

        [Serializable]
        public class ExtensionMethodTemplate
        {
            /// <summary>
            /// Match everything except ascii characters, numbers and underscores.
            /// <br>MethodName can start with numbers, it will be prefixed with <see cref="MethodNamePrefix"/>.</br>
            /// </summary>
            private const string ReMatchMethodName = "[^a-zA-Z0-9_]";
            /// <summary>
            /// Method name to prefix.
            /// </summary>
            public const string MethodNamePrefix = "BXSTw";
            /// <summary>
            /// Function name to generate.
            /// </summary>
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

            public string TargetMemberName { get => m_TargetMemberName; internal set => m_TargetMemberName = value; }
            [ReadOnlyView, SerializeField] private string m_TargetMemberName;
            /// <summary>
            /// Returns the member info lists gathered from <paramref name="t"/>
            /// using this ExtensionMethodTemplate's <see cref="m_TargetMemberName"/>.
            /// <br>This only returns <see cref="MemberTypes.Field"/> and <see cref="MemberTypes.Property"/>ies.</br>
            /// </summary>
            public MemberInfo[] GetTargets(Type t)
            {
                return t.GetMember(m_TargetMemberName, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public);
            }
        }
        [Serializable]
        public class ExtensionClassTemplate
        {
            public SerializableType targetType;
            public List<ExtensionMethodTemplate> extensionMethods;
        }

        [Tooltip("The '.cs' is appended to the last file name. The directory is local."), EditDisallowChars("?<>:*|\"")]
        public string tweenExtensionsFileName = "Scripts/BXSTween/Extension/CustomExtension.cs";
        /// <summary>
        /// Same as <see cref="ExtensionMethodTemplate.ReMatchMethodName"/> but with dots allowed and no numbers on start of string.
        /// </summary>
        private const string ReMatchNamespaceName = "[^_.a-zA-Z0-9_]|^[\\d.]+";
        [EditDisallowChars(ReMatchNamespaceName, isRegex = true)]
        public string fileNamespace = "BXFW.Tweening.Next";
        public string fileClassName = "BXSTweenExtensions";

        /// <summary>
        /// List of extension pairs to generate.
        /// </summary>
        public List<ExtensionClassTemplate> extensionPairs = new List<ExtensionClassTemplate>();

        // -- Gen Vars
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

        private const string TkUsingTemplate = "using {0};";
        private const string TkNamespace = "namespace";
        private const string TkPublic = "public";
        private const string TkStatic = "static";
        private const string TkPartial = "partial";
        private const string TkClass = "class";
        private const string TkExtensionThis = "this";
        private const string TkParameterSep = ",";
        private const string TkIndent = "    ";
        private const string TkReturn = "return";
        private const char TkOpenScope = '{';
        private const char TkCloseScope = '}';
        private const char TkOpenParams = '(';
        private const char TkCloseParams = ')';
        private const char TkSemicolon = ';';
        // Use StringBuilder.AppendLine to get the environment's preferred line ending
        /// <summary>
        /// List of namespaces that are currently used.
        /// <br>This will be reset every run.</br>
        /// </summary>
        private readonly List<string> currentNamespaceUsings = new List<string>();

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

            return true;
        }

        public override void Run()
        {
            // -- Reset state
            currentNamespaceUsings.Clear();

            // -- Start building
            StringBuilder sb = new StringBuilder(extensionPairs.Count * 368);
            bool hasNamespaceDefinition = !string.IsNullOrWhiteSpace(fileNamespace);
            string namespaceIndent = hasNamespaceDefinition ? TkIndent : string.Empty;
            if (hasNamespaceDefinition)
            {
                // append file stuff (namespace 'fileNameSpace')
                sb.AppendLine().Append(TkNamespace).Append(" ").Append(fileNamespace).AppendLine()
                    .Append(TkOpenScope).AppendLine();
            }
            // partial extensions class definition (public static partial class {fileClassName})
            sb.Append(namespaceIndent).Append(TkPublic).Append(" ").Append(TkStatic).Append(" ").Append(TkPartial).Append(" ").Append(TkClass).Append(" ").Append(fileClassName).AppendLine();
            sb.Append(namespaceIndent).Append(TkOpenScope).AppendLine(); // {\n
            // constant using definitions (only add these if the root namespace isn't BXFW.Tweening)
            bool isBaseTweeningNamespace = fileNamespace.StartsWith("BXFW.Tweening");
            if (!isBaseTweeningNamespace)
            {
                currentNamespaceUsings.Add("BXFW.Tweening");
            }
            if (!isBaseTweeningNamespace || !fileNamespace.StartsWith("BXFW.Tweening.Next"))
            {
                currentNamespaceUsings.Add("BXFW.Tweening.Next");
            }
            // function definitions
            foreach (ExtensionClassTemplate template in extensionPairs)
            {
                if (template.targetType.Type == null)
                {
                    Debug.LogWarning("[BXSTweenExtensionGeneratorTask::Run] Given template has no type, skipping.");
                    continue;
                }

                // Add given types to using
                string targetTypeNamespace = template.targetType.Type.Namespace;
                if (!string.IsNullOrWhiteSpace(targetTypeNamespace) && !currentNamespaceUsings.Contains(targetTypeNamespace))
                {
                    currentNamespaceUsings.Add(targetTypeNamespace);
                }

                List<string> generatedMethodNames = new List<string>(template.extensionMethods.Count);
                foreach (ExtensionMethodTemplate method in template.extensionMethods)
                {
                    // The following code will cause you to remove your eyes
                    // It is chalice simulator tier. In fact it's probably worse.
                    // Complexity is probably StudentScript.cs and notation is o(n^31289391283)

                    // Check for uniqueness of 'method.MethodName'
                    if (generatedMethodNames.Contains(method.MethodName))
                    {
                        Debug.LogWarning($"[BXSTweenExtensionGeneratorTask::Run] Given name \"{method.MethodName}\" already exists on list more than once. Appending count of 'generatedMethodNames' to it.");
                        method.MethodName += generatedMethodNames.Count;
                    }

                    MemberInfo targetTypeMember = method.GetTargets(template.targetType.Type).First();
                    Type memberFieldType;
                    if (targetTypeMember is FieldInfo fieldInfo)
                    {
                        memberFieldType = fieldInfo.FieldType;
                    }
                    else if (targetTypeMember is PropertyInfo propInfo)
                    {
                        memberFieldType = propInfo.PropertyType;
                    }
                    else
                    {
                        throw new ArgumentException($"[BXSTweenExtensionGeneratorTask::Run] Given MethodTemplate's MemberInfo is invalid for 'targetTypeMember' {targetTypeMember}.");
                    }

                    Type twContextType = ReturnContextTypeForType(memberFieldType);
                    if (!currentNamespaceUsings.Contains(twContextType.Namespace))
                    {
                        currentNamespaceUsings.Add(twContextType.Namespace);
                    }
                    if (!currentNamespaceUsings.Contains(memberFieldType.Namespace) && !Additionals.IsTypeNumerical(memberFieldType))
                    {
                        currentNamespaceUsings.Add(memberFieldType.Namespace);
                    }

                    string TkTargetParameterName = "target";
                    string TkLastValueParameterName = "lastValue";
                    string TkDurationParameterName = "duration";
                    string TkContextValueName = "context";

                    // -> -> public static {twContextType.Name} {BXTw[method.MethodName]}
                    sb.Append(namespaceIndent).Append(TkIndent)
                        .Append(TkPublic).Append(" ").Append(TkStatic).Append(" ").Append(twContextType.Name).Append(" ").Append(ExtensionMethodTemplate.MethodNamePrefix).Append(method.MethodName)
                        // The default parameters for the given context are : 
                        // (this {TargetTypeName} {TkTargetName},
                        .Append(TkOpenParams).Append(TkExtensionThis).Append(" ").Append(template.targetType.Type.Name).Append(" ").Append(TkTargetParameterName).Append(TkParameterSep)
                        // {MemberFieldType.Name} {TkLastValueParameterName},
                        .Append(" ").Append(memberFieldType.Name).Append(" ").Append(TkLastValueParameterName).Append(TkParameterSep)
                        // float {TkDurationParameterName})
                        .Append(" float ").Append(TkDurationParameterName).Append(TkCloseParams)
                     .AppendLine(); // )\n

                    // Method body
                    sb.Append(namespaceIndent).Append(TkIndent).Append(TkOpenScope).AppendLine(); // {\n
                    // Target null check
                    sb.Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append("if (").Append(TkTargetParameterName).Append(" == null)").AppendLine()
                        .Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append(TkOpenScope).AppendLine()  // {
                            .Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append(TkIndent).Append("throw new System.ArgumentNullException(\"[").Append(ExtensionMethodTemplate.MethodNamePrefix).Append(method.MethodName).Append("] Given argument was null.\")").Append(TkSemicolon).AppendLine()
                        .Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append(TkCloseScope).AppendLine() // }
                      .AppendLine() // Gap between the actual method calls
                                    // BXSTweenContext context = new BXSTweenContext(duration);
                      .Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append(twContextType.Name).Append(" ").Append(TkContextValueName).Append(" = new ").Append(twContextType.Name).Append(TkOpenParams).Append(TkDurationParameterName).Append(TkCloseParams).Append(TkSemicolon).AppendLine()
                      // context.SetupContext(() => target.{TargetMemberName},
                      .Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append(TkContextValueName).Append(".SetupContext(() => ").Append(TkTargetParameterName).Append(".").Append(method.TargetMemberName).Append(TkParameterSep).Append(" ")
                      // TkLastValueParameterName, (v) => target.{TargetMemberName} = v);\n
                      .Append(TkLastValueParameterName).Append(TkParameterSep).Append(" ").Append("(v) => ").Append(TkTargetParameterName).Append(".").Append(method.TargetMemberName).Append(" = v").Append(TkCloseParams).Append(TkSemicolon).AppendLine();
                    // context.DelayedPlay();
                    sb.Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append(TkContextValueName).Append(".DelayedPlay();").AppendLine();
                    sb.AppendLine();
                    // return context;
                    sb.Append(namespaceIndent).Append(TkIndent).Append(TkIndent).Append(TkReturn).Append(" ").Append(TkContextValueName).Append(TkSemicolon).AppendLine();
                    sb.Append(namespaceIndent).Append(TkIndent).Append(TkCloseScope).AppendLine(); // }\n

                    // StringBuilder after this : •'_'• (angery)
                    generatedMethodNames.Add(method.MethodName);
                }
            }
            // end class definition + 
            sb.Append(namespaceIndent).Append(TkCloseScope).AppendLine(); // }\n
            if (hasNamespaceDefinition)
            {
                sb.Append(TkCloseScope).AppendLine(); // '(namespace) }\n'
            }

            // Prepend usings
            foreach (string usingStr in currentNamespaceUsings)
            {
                sb.Insert(0, string.Format(TkUsingTemplate, usingStr) + Environment.NewLine);
            }

            // Write class into a given file
            string parentDirectory = GenerateFileAbsolutePath;
            {
                int indexOfPathSeperator = parentDirectory.LastIndexOf('/');
#if UNITY_EDITOR_WIN
                if (indexOfPathSeperator == -1)
                {
                    indexOfPathSeperator = parentDirectory.LastIndexOf('\\');
                }
#endif
                parentDirectory = parentDirectory.Substring(0, indexOfPathSeperator);
            }

            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            File.WriteAllBytes(GenerateFileAbsolutePath, Encoding.UTF8.GetBytes(sb.ToString()));
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}
