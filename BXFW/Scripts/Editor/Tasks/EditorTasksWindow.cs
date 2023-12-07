using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Runs editor related scripting tasks, using given abstract 
    /// <see cref="EditorTask"/> inheriting ScriptableObject behaviours.
    /// </summary>
    public class EditorTasksWindow : EditorWindow
    {
        /// <summary>
        /// Creates a window.
        /// <br>All window values are init here.</br>
        /// </summary>
        /// <returns>The created window.</returns>
        [MenuItem("Window/BXFW/Editor Tasks")]
        public static EditorTasksWindow CreateGenerator()
        {
            EditorTasksWindow w = GetWindow<EditorTasksWindow>(true, "Editor Tasks");
            w.minSize = new Vector2(400f, 400f);

            return w;
        }

        [SerializeField] private List<EditorTask> currentTasksGen = new List<EditorTask>();
        private SerializedObject serializedObject;
        private float taskTakenTime = 0f;
        private Vector2 scrollCurrent = Vector2.zero;

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
        }
        private void OnDestroy()
        {
            foreach (EditorTask task in currentTasksGen)
            {
                // asset actually exists, we are going to lose reference anyways.
                if (!string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(task)))
                {
                    continue;
                }

                // since these could be temp, avoid memory leaks manually
                DestroyImmediate(task);
            }

            currentTasksGen.Clear();
            serializedObject.Dispose();
        }

        private GUIStyle boldBigText;
        private GUIStyle boldButtonText;
        private void GatherGUIStyles()
        {
            boldBigText ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
            };
            boldButtonText ??= new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 16,
            };
        }
        private void DrawConfigGUIWarnings()
        {
            bool hasNullTask = currentTasksGen.Any(t => t == null);
            if (hasNullTask)
            {
                EditorGUILayout.HelpBox("[CurrentTasks] : There's empty tasks in the current task list.\nNull tasks will be ignored.", MessageType.Warning);
            }
        }

        private void OnGUI()
        {
            GatherGUIStyles();
            float windowWidth = position.width;

            // -----------------------------------------
            // ConfigGUI : Draw configuration interface.
            scrollCurrent = GUILayout.BeginScrollView(scrollCurrent);
            GUILayout.Label("  [ Configuration ]", boldBigText, GUILayout.Height(50f));
            GUIAdditionals.DrawUILineLayout(Color.gray);

            serializedObject.UpdateIfRequiredOrScript(); // call this otherwise the ui doesn't refresh and gets stuck.
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(currentTasksGen)));
            serializedObject.ApplyModifiedProperties();
            DrawConfigGUIWarnings();
            GUILayout.EndScrollView();

            GUIAdditionals.DrawUILineLayout(Color.gray);
            GUILayout.Space(12f);

            // -----------------------------------------
            // DoTasks : Show progress bar until done.
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Begin Tasks", boldButtonText, GUILayout.Height(30f), GUILayout.Width(windowWidth * .8f)))
            {
                bool warningPass = true;
                // Call 'GetWarning's
                for (int i = 0; i < currentTasksGen.Count; i++)
                {
                    EditorTask t = currentTasksGen[i];

                    if (t == null)
                    {
                        continue;
                    }

                    if (!t.GetWarning())
                    {
                        warningPass = false;
                        break;
                    }
                }

                if (warningPass)
                {
                    float start = (float)EditorApplication.timeSinceStartup;

                    for (int i = 0; i < currentTasksGen.Count; i++)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Doing tasks...", $"Currently doing task #{i + 1}.", i / (float)currentTasksGen.Count))
                        {
                            break;
                        }

                        EditorTask t = currentTasksGen[i];
                        // pretty sure the user can see that there's null tasks, i also put a helpbox there.
                        if (t == null)
                        {
                            continue;
                        }

                        try
                        {
                            t.Run();
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("[EditorTasksWindow::OnGUI(Begin Tasks)] An exception occured during running of a task. The next log will contain details. Other tasks will not be run.");
                            Debug.LogException(e);
                            break;
                        }
                    }

                    EditorUtility.ClearProgressBar();
                    taskTakenTime = (float)EditorApplication.timeSinceStartup - start;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(14f); // Line adds 10 padding + 2 height

            if (taskTakenTime > .1f)
            {
                EditorGUILayout.HelpBox($"Last run took {taskTakenTime} seconds.", MessageType.Info);
            }
        }
    }
}
