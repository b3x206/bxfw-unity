using BXFW.Data;
using System;
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
        // Which unity seems to freak out about
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
            // This is what happens when no constructor access and no RAII
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
            generateAsset.AddRange(LocalizedTextParser.Parse(targetTextAsset.text));

            AssetDatabase.CreateAsset(generateAsset, RelativeExportDirectory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
