using BXFW.Data;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BXFW.Tools.Editor
{
    public class ConvertLocalizedTextDataToAsset : EditorTask
    {
        public TextAsset targetTextAsset;
        private string exportDirectory;
        // Directory.GetCurrentDirectory() does not contain the last path limiter
        // Which unity seems to freak out about if the first path limiter is left in
        private string RelativeExportDirectory => exportDirectory?.Substring(Directory.GetCurrentDirectory().Length + 1);

        public override bool GetWarning()
        {
            if (targetTextAsset == null)
            {
                EditorUtility.DisplayDialog("ConvertLocalizedTextDataToAsset::GetWarning", "No 'targetTextAsset' was assigned to this task. Task will not run.", "Ok");
                return false;
            }

            exportDirectory = EditorUtility.SaveFilePanel("Export Localized Text Into", string.Empty, $"Converted{targetTextAsset.name}", "asset");
            return !string.IsNullOrWhiteSpace(exportDirectory);
        }

        public override void Run()
        {
            LocalizedTextListAsset generateAsset = CreateInstance<LocalizedTextListAsset>();
            try
            {
                RunWithAsset(ref generateAsset);
            }
            // ah yes, error handling
            // This is what happens when no constructor access
            // As creation would have failed if 'CreateInstance' took ctor params.
            catch (Exception e)
            {
                // Dispose of temporary values
                DestroyImmediate(generateAsset);
                throw e;
            }
        }

        private void RunWithAsset(ref LocalizedTextListAsset generateAsset)
        {
            generateAsset.AddRange(LocalizedTextParser.Parse(targetTextAsset.text, out Dictionary<string, string> globalPragmaSettings));
            if (globalPragmaSettings.Count > 0)
            {
                generateAsset.pragmaDefinitons = new SerializableDictionary<string, string>(globalPragmaSettings);
            }

            AssetDatabase.CreateAsset(generateAsset, RelativeExportDirectory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
