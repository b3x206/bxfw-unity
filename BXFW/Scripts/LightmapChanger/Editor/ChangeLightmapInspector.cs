using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

using System.IO;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ChangeLightmap))]
    internal class ChangeLightmapInspector : Editor
    {
        /// <summary>
        /// Compares the enum names with the lightmap name to check if the name is available.
        /// </summary>
        /// <returns>False if the <paramref name="lmapName"/> equals to a member in <see cref="LightmapEnum"/>.</returns>
        private bool IsLightmapNameAvailable(string lmapName)
        {
            // Ignore null / empty strings.
            if (string.IsNullOrWhiteSpace(lmapName))
                return false;

            var EnumArray = System.Enum.GetValues(typeof(LightmapEnum)).ToTypeArray<LightmapEnum>();

            foreach (LightmapEnum s in EnumArray)
            {
                if (s.ToString().Equals(lmapName))
                {
                    return false;
                }
            }

            return true;
        }

        private bool lManagerIsUncollapsed = false;
        private Vector2 lManagerScrollCurrent = Vector2.zero;
        private void LightmapManagerGUI()
        {
            // TODO : Make a lightmap manager GUI.
            var gContent = new GUIContent();
            GUILayout.BeginArea(EditorGUILayout.GetControlRect(), gContent);
            GUILayout.BeginScrollView(lManagerScrollCurrent);
            lManagerIsUncollapsed = EditorGUILayout.DropdownButton(gContent, FocusType.Keyboard, EditorStyles.foldout);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public override void OnInspectorGUI()
        {
            var lightmapData = (ChangeLightmap)target;
            var gEnabled = GUI.enabled;

            GUI.enabled = !Lightmapping.isRunning;

            // Drawn inspector
            SerializedObject lDataProp = new SerializedObject(lightmapData);
            // Name field
            SerializedProperty lDataName = lDataProp.FindProperty(nameof(lightmapData.LightmapName));
            // Enum booleans
            SerializedProperty lDataEnumBool = lDataProp.FindProperty(nameof(lightmapData.SetDefaultLightmapOnAwake));
            SerializedProperty lDataLMapEnum = lDataProp.FindProperty(nameof(lightmapData.DefaultLightmap));

            // Lightmap enums are going to be displayed like : 
            // -----------------
            // | List     <|   |
            // |---------------|
            // | 1 : Opt1    - |
            // | 2 : Opt2    - | (Highlighted)
            // | [Input Box] + |
            // |               |
            // -----------------
            LightmapManagerGUI();

            /* Display gui */
            EditorGUILayout.PropertyField(lDataName);
            EditorGUILayout.PropertyField(lDataEnumBool);
            if (lightmapData.SetDefaultLightmapOnAwake)
            { EditorGUILayout.PropertyField(lDataLMapEnum); }

            lDataProp.ApplyModifiedProperties();

            // Buttons
            if (GUILayout.Button("Bake"))
            {
                if (!IsLightmapNameAvailable(lightmapData.LightmapName))
                {
                    EditorUtility.DisplayDialog("Warning", $"There is already a saved lightmap with the name {lightmapData.LightmapName}.", "Ok, i will change it");
                    

                    return;
                }

                if (string.IsNullOrEmpty(lightmapData.LightmapName))
                {
                    EditorUtility.DisplayDialog("Info", $"The {nameof(lightmapData.LightmapName)} cannot be left blank.", "Ok");
                    return;
                }

                if (EditorUtility.DisplayDialog("Warning!",
                    $"Do you want to bake lightmap for scene {SceneManager.GetActiveScene().name} with the name {lightmapData.LightmapName}?",
                    "Yes", "No"))
                {
                    lightmapData.BakeLightmapInstance();
                }
            }
            if (GUILayout.Button("Load"))
            {
                if (!IsLightmapNameAvailable(lightmapData.LightmapName))
                {
                    EditorUtility.DisplayDialog("Warning", string.Format("There is already a saved/loaded lightmap with the name {0}.", lightmapData.LightmapName), "Ok, i will change it");
                    return;
                }

                string DirPath = EditorUtility.OpenFolderPanel("Select lightmap directory to load...", string.Format("{0}/Assets", Directory.GetCurrentDirectory()), null);
                // if no path is selected.
                if (string.IsNullOrEmpty(DirPath)) { return; }

                DirectoryInfo inf = new DirectoryInfo(DirPath);

                // Check if selected file is valid.
                bool IsValidDir = false;
                string[] files = Directory.GetFiles(inf.FullName, "*.asset", SearchOption.TopDirectoryOnly);
                foreach (string s in files)
                {
#if UNITY_EDITOR_WIN
                    char filePathSeperatorChar = '\\';
#else
                    char filePathSeperatorChar = '/';
#endif
                    string[] split = s.Split(filePathSeperatorChar);
                    // Get the last name of the files.
                    if (split[split.Length - 1].Equals("LightingData.asset"))
                    {
                        IsValidDir = true;
                        break;
                    }
                }

                if (!IsValidDir)
                {
                    Debug.LogWarning(string.Format("[ChangeLightmap] The loaded directory \"{0}\" is invalid because no file exists with the name LightingData.asset.\n", inf.FullName) +
                        "If you selected a valid file but you are getting this error make sure your lightmap file is named like \"LightingData\".");
                    return;
                }

                if (!string.IsNullOrEmpty(lightmapData.LightmapName))
                {
                    // Generate the directory.
                    if (inf.Name.Equals(lightmapData.LightmapName))
                    {
                        lightmapData.AddToBakedList(inf, false);
                        Debug.Log("[ChangeLightmapInspector] Since the directory name and the LightmapName is same, we instantly created an entry.");
                        return;
                    }

                    switch (EditorUtility.DisplayDialogComplex("Info",
                        string.Format("Would you like to use the directory name ({0}) or the name in the LightmapName ({1}) for your lightmap name?", inf.Name, lightmapData.LightmapName),
                        "Directory", "Lightmap Name", "Cancel"))
                    {
                        // Directory 
                        case 0:
                            lightmapData.AddToBakedList(inf, false);
                            break;
                        case 1:
                            lightmapData.AddToBakedList(inf, true);
                            break;
                        case 2:
                            return;
                    }
                }
            }
            if (GUILayout.Button("Clear Lightmaps"))
            {
                switch (EditorUtility.DisplayDialogComplex("Warning!",
                   "Do you want to clear all saved lightmaps?",
                   "Yes", "No", "Yes, don't delete the SavedLightmaps folder"))
                {
                    case 0:
                        lightmapData.ClearData(true);
                        break;
                    case 2:
                        lightmapData.ClearData(false);
                        break;

                    case 1:
                    default:
                        break;
                }
            }

            GUI.enabled = gEnabled;

            if (Lightmapping.isRunning)
            {
                EditorGUILayout.HelpBox("[ChangeLightmap] Currently baking lightmaps...", MessageType.Warning);
            }
        }
    }
}