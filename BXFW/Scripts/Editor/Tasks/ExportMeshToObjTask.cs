using UnityEngine;
using System.Globalization;
using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Used as a task to export <see cref="Mesh"/> to OBJ format file.
    /// </summary>
    /// Maybe TODO : Create a 'ExportOBJTask' task with predefined and optimized methods to create an OBJ mesh string.
    public class ExportMeshToObjTask : EditorTask
    {
        private void GetAndWriteOBJMesh(UnityEngine.Object obj, string writePath)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "[ExportMeshToObjTask::GetAndWriteOBJMesh] Given argument 'obj' was null.");
            }

            Matrix4x4 transformationMatrix = Matrix4x4.identity;

            GameObject gameObject = obj as GameObject;
            Mesh targetMesh = obj as Mesh;

            if (gameObject != null)
            {
                if (!gameObject.TryGetComponent(out MeshFilter meshFilter))
                {
                    Debug.LogWarning("[ExportMeshToObjTask::GetAndWriteOBJMesh] No MeshFilter is found in selected UnityEngine.GameObject", obj);
                    return;
                }
                else
                {
                    targetMesh = meshFilter.sharedMesh;
                    transformationMatrix = gameObject.transform.localToWorldMatrix;
                }
            }

            if (targetMesh == null)
            {
                Debug.LogWarning($"[ExportMeshToObjTask::GetAndWriteOBJMesh] No mesh is found in given UnityEngine.Object, {obj}", obj);
                return;
            }

            CultureInfo previousCurrentCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                using StreamWriter writer = new StreamWriter(writePath);
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                writer.Write(GetMeshOBJ(obj.name, targetMesh, transformationMatrix));
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExportMeshToObjTask::GetAndWriteOBJMesh] An exception occured while exporting given UnityEngine.Object : {e.Message}\n{e.StackTrace}", obj);
            }

            Thread.CurrentThread.CurrentCulture = previousCurrentCulture;
        }

        private readonly List<Vector3> meshVerts = new List<Vector3>();
        private readonly List<Vector3> meshUVs = new List<Vector3>();
        private readonly List<Vector3> meshNorms = new List<Vector3>();

        /// <summary>
        /// Returns the given <paramref name="mesh"/>'s OBJ string.
        /// </summary>
        public string GetMeshOBJ(string name, Mesh mesh, Matrix4x4 objTransform)
        {
            StringBuilder sb = new StringBuilder();
            meshVerts.Clear();
            mesh.GetVertices(meshVerts);

            sb.Capacity += meshVerts.Count * 27; // OBJ float length is 9 chars
            foreach (Vector3 v in meshVerts)
            {
                Vector3 writeV = applyTransformMatrix ? objTransform.MultiplyPoint(v) : v;
                sb.Append(string.Format("v {0} {1} {2}\n", writeV.x, writeV.y, writeV.z));
            }

            meshUVs.Clear();
            mesh.GetUVs(0, meshUVs);

            sb.Capacity += meshUVs.Count * 27;
            foreach (Vector3 v in meshUVs)
            {
                sb.Append(string.Format("vt {0} {1} {2}\n", v.x, v.y, v.z));
            }

            meshNorms.Clear();
            mesh.GetNormals(meshNorms);

            sb.Capacity += meshNorms.Count * 27;
            foreach (Vector3 v in meshNorms)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }

            for (int material = 0; material < mesh.subMeshCount; material++)
            {
                sb.Append(string.Format("\ng {0}\n", name));
                int[] triangles = mesh.GetTriangles(material);

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(
                        string.Format("f {0}/{0} {1}/{1} {2}/{2}\n",
                            triangles[i] + 1,
                            triangles[i + 1] + 1,
                            triangles[i + 2] + 1
                        )
                    );
                }
            }

            return sb.ToString();
        }

        [Tooltip("Whether to apply transformations if any GameObject's with transform were assigned to the 'objects' field.")]
        public bool applyTransformMatrix = false;

        [ObjectFieldTypeConstraint(typeof(Mesh), typeof(GameObject))]
        public UnityEngine.Object[] objects;

        /// <summary>
        /// Folder used to export the OBJ's.
        /// </summary>
        private string exportFolderPath;

        public override bool GetWarning()
        {
            exportFolderPath = EditorUtility.SaveFolderPanel("Export OBJ's to", exportFolderPath, "OBJs");
            return !string.IsNullOrWhiteSpace(exportFolderPath);
        }

        public override void Run()
        {
            char[] invalidPathChars = Path.GetInvalidFileNameChars();
            char replacementLegalChar = '0';

            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null)
                {
                    Debug.LogWarning("[ExportMeshToObjTask::Run] Given 'obj' is null, continuing.");
                    continue;
                }

                StringBuilder writeName = new StringBuilder(obj.name);
                // check if illegal chars to write are in the 'object's name
                for (int i = 0; i < writeName.Length; i++)
                {
                    if (invalidPathChars.Contains(writeName[i]))
                    {
                        writeName[i] = replacementLegalChar;
                    }
                }

                GetAndWriteOBJMesh(obj, Path.Combine(exportFolderPath, $"{writeName}.obj"));
            }
        }
    }
}
