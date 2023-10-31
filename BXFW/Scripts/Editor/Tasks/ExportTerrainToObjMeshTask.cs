using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;

using UnityEditor;
using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Save format for the terrain.
    /// </summary>
    public enum SaveFormat { Triangles, Quads }
    /// <summary>
    /// Resolution for saving the terrain data.
    /// <br>Higher resolutions mean more detailed '.obj' terrain meshes at the cost of performance and export speed.</br>
    /// </summary>
    public enum SaveResolution { Full, Half, Quarter, Eighth, Sixteenth }

    /// <summary>
    /// A <see cref="EditorTask"/> for exporting a unity <see cref="Terrain"/>'s data into a standard mesh with '.obj' format.
    /// </summary>
    public class ExportTerrainToObjMeshTask : EditorTask
    {
        public SaveFormat saveFormat = SaveFormat.Triangles;
        public SaveResolution saveResolution = SaveResolution.Half;
        public TerrainData targetTerrain;
        
        private Vector3 targetObjectPosition; // Offset for the verts
        private string saveFileName;

        private int elapsedCount;    // Elapsed count
        private int totalCount;      // Total mesh quad/tri count
        private int progressCounter; // Counts the progress
        private const int PROGRESS_COUNT_INTERVAL = 8192; // progress count interval

        /// <summary>
        /// Assigns <see cref="targetTerrain"/> if it's <see langword="null"/>.
        /// <br>Returns whether if <see cref="targetTerrain"/> is still null.</br>
        /// </summary>
        private bool Init()
        {
            if (targetTerrain == null)
            {
                var terrainObject = Selection.activeObject as Terrain;
                if (terrainObject == null)
                {
                    terrainObject = Terrain.activeTerrain;
                }

                // don't throw null reference exception if the terrain is still null.
                if (terrainObject != null)
                {
                    targetTerrain = terrainObject.terrainData;
                    targetObjectPosition = terrainObject.transform.position;
                }
            }

            return targetTerrain != null;
        }

        private void OnEnable()
        {
            // Call this for good measure + pre-assigned terrain
            Init();
        }

        /// <summary>
        /// Shows the file select dialog.
        /// <br>If no file is selected this task will return 'not acknowledged' (false).</br>
        /// </summary>
        public override bool GetWarning()
        {
            if (!Init())
            {
                EditorUtility.DisplayDialog("Info", "No terrain was assigned to this 'ExportTerrainObjMeshTask'. Doing nothing.", "Ok");
                return false;
            }

            saveFileName = EditorUtility.SaveFilePanel("[ExportTerrainToObj] Export .obj file into", string.Empty, "Terrain", "obj");
            return !string.IsNullOrWhiteSpace(saveFileName);
        }

        /// <summary>
        /// Exports the terrain data to an .obj file.
        /// </summary>
        public override void Run()
        {
            // Don't run if target is null
            if (!Init())
            {
                return;
            }

            var w = targetTerrain.heightmapResolution;
            var h = targetTerrain.heightmapResolution;

            var meshScale = targetTerrain.size;
            var tRes = (int)Mathf.Pow(2, (int)saveResolution);
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            var uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            var tData = targetTerrain.GetHeights(0, 0, w, h);

            w = ((w - 1) / tRes) + 1;
            h = ((h - 1) / tRes) + 1;
            var tVertices = new Vector3[w * h];
            var tUV = new Vector2[w * h];

            int[] tPolys;

            tPolys = saveFormat == SaveFormat.Triangles ? new int[(w - 1) * (h - 1) * 6] : new int[(w - 1) * (h - 1) * 4];

            // Build vertices and UVs
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    tVertices[(y * w) + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + targetObjectPosition;
                    tUV[(y * w) + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                }
            }

            var index = 0;
            if (saveFormat == SaveFormat.Triangles)
            {
                // Build triangle indices: 3 indices into vertex array for each triangle
                for (var y = 0; y < h - 1; y++)
                {
                    for (var x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output two triangles
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = (y * w) + x + 1;

                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;
                    }
                }
            }
            else
            {
                // Build quad indices: 4 indices into vertex array for each quad
                for (var y = 0; y < h - 1; y++)
                {
                    for (var x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output one quad
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;
                    }
                }
            }

            CultureInfo prevCulture = Thread.CurrentThread.CurrentCulture;

            // Export to .obj
            var sw = new StreamWriter(saveFileName);
            try
            {
                sw.WriteLine("# Unity terrain OBJ File");

                // Write vertices
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                progressCounter = elapsedCount = 0;
                totalCount = ((tVertices.Length * 2) + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / PROGRESS_COUNT_INTERVAL;
                for (var i = 0; i < tVertices.Length; i++)
                {
                    UpdateProgress();
                    var sb = new StringBuilder("v ", 20);
                    // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}" etc. format
                    // Which is important when you're exporting huge terrains.
                    sb.Append(tVertices[i].x.ToString()).Append(" ").
                       Append(tVertices[i].y.ToString()).Append(" ").
                       Append(tVertices[i].z.ToString());
                    sw.WriteLine(sb);
                }
                // Write UVs
                for (int i = 0; i < tUV.Length; i++)
                {
                    UpdateProgress();
                    var sb = new StringBuilder("vt ", 22);
                    sb.Append(tUV[i].x.ToString()).Append(" ").
                       Append(tUV[i].y.ToString());
                    sw.WriteLine(sb);
                }

                if (saveFormat == SaveFormat.Triangles)
                {
                    // Write triangles
                    for (int i = 0; i < tPolys.Length; i += 3)
                    {
                        UpdateProgress();
                        var sb = new StringBuilder("f ", 43);
                        sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                           Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                           Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
                        sw.WriteLine(sb);
                    }
                }
                else
                {
                    // Write quads
                    for (var i = 0; i < tPolys.Length; i += 4)
                    {
                        UpdateProgress();
                        var sb = new StringBuilder("f ", 57);
                        sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                           Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                           Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                           Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
                        sw.WriteLine(sb);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.LogError(string.Format("[ExportTerrainToObj::Run] Error saving file: {0}\n StackTrace : {1}", err.Message, err.StackTrace));
            }

            sw.Close();
            targetTerrain = null;
            Thread.CurrentThread.CurrentCulture = prevCulture;

            EditorUtility.DisplayProgressBar("[ExportTerrainToObj::Run] Saving file to disc.", "This might take a while...", 1f);
            EditorUtility.ClearProgressBar();
        }
        private void UpdateProgress()
        {
            if (progressCounter++ != PROGRESS_COUNT_INTERVAL)
            {
                return;
            }

            progressCounter = 0;
            EditorUtility.DisplayProgressBar("[ExportTerrainToObj::Run] Saving...", "", Mathf.InverseLerp(0, totalCount, ++elapsedCount));
        }
    }
}
