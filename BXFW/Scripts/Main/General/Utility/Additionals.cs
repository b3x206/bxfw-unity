using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BXFW
{
    /// <summary>
    /// The additionals class.
    /// <br>Contains additionals that doesn't exactly fit somewhere, or it is too small that it doesn't have it's own class.</br>
    /// </summary>
    public static class Additionals
    {
        // -- GameObject / Transform
        /// <summary>
        /// Get the path of the gameobject.
        /// </summary>
        public static string GetPath(this Component target)
        {
            return target.transform.GetPath();
        }
        /// <summary>
        /// Get the path of the gameobject.
        /// </summary>
        public static string GetPath(this GameObject target)
        {
            return target.transform.GetPath();
        }
        /// <summary>
        /// Get the path of the gameobject.
        /// <br>This is useful for <see cref="Debug.Log"/>-ging with better troubleshooting.</br>
        /// </summary>
        public static string GetPath(this Transform target)
        {
            if (target.parent == null)
            {
                return string.Format("/{0}", target.name);
            }

            return string.Format("{0}/{1}", target.parent.GetPath(), target.name);
        }
        /// <summary>
        /// Scales a transform <paramref name="target"/> around <paramref name="pivot"/>, using the given <paramref name="scale"/> and <paramref name="pivotSpace"/>.
        /// <br>Note : May put your transform into a floating point number heaven. (<see cref="float.PositiveInfinity"/> or <see cref="float.NaN"/>)</br>
        /// <br>Use with caution as there's no clamping present. </br>
        /// </summary>
        /// <param name="pivotSpace">
        /// Space of the pivot. 
        /// <br>When this is set to 'World', the <paramref name="pivot"/> is untouched.</br>
        /// <br>
        /// When this is set to 'Self', the <paramref name="pivot"/> is converted to a transform point using <see cref="Transform.InverseTransformPoint(Vector3)"/>.
        /// This gets affected by stuff like rotation of <paramref name="target"/> and stuff etc.
        /// </br>
        /// <br>To use a consistent point as pivot, use the given <c>transform's position + constant pivot point you want</c> with the parameter <paramref name="pivot"/>.</br>
        /// </param>
        public static void ScaleAroundPivot(this Transform target, Vector3 pivot, Vector3 scale, Space pivotSpace = Space.World)
        {
            // Necessary points
            Vector3 targetPoint = target.localPosition;
            Vector3 pivotPoint;
            switch (pivotSpace)
            {
                default:
                case Space.World:
                    pivotPoint = pivot;
                    break;
                case Space.Self:
                    pivotPoint = target.InverseTransformPoint(pivot);
                    break;
            }
            Vector3 diff = targetPoint - pivotPoint;

            // Scale relativeScale to previous scale
            // float relativeScale = scale.x / target.transform.localScale.x;
            Vector3 relativeScaleVector = new Vector3(scale.x / target.transform.localScale.x, scale.y / target.transform.localScale.y, scale.z / target.transform.localScale.z);
            // Move the relative scale amount from pivot point (as we assume the pivot is the given 'pivotPoint')
            Vector3 finalPos = pivotPoint + Vector3.Scale(diff, relativeScaleVector);

            target.localScale = scale;
            target.localPosition = finalPos;
        }

        // -- Rigidbody
        /// <summary>
        /// Clamps rigidbody velocity.
        /// </summary>
        public static void ClampVelocity(this Rigidbody rb, float MaxSpeed)
        {
            if (rb == null)
            {
                Debug.LogError("[Additionals::ClampVelocity] The referenced rigidbody is null.");
                return;
            }

            // Trying to Limit Speed
            if (rb.velocity.magnitude > MaxSpeed)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxSpeed);
            }
        }
        /// <summary>
        /// Adds an explosion to rigidbody.
        /// </summary>
        public static void AddExplosionForce(this Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0f, ForceMode2D mode = ForceMode2D.Force)
        {
            Vector2 explosionDir = rb.position - explosionPosition;
            float explosionDistance = explosionDir.magnitude / explosionRadius;

            // Normalize without computing magnitude again
            if (upwardsModifier == 0)
            {
                explosionDir /= explosionDistance;
            }
            else
            {
                // If you pass a non-zero value for the upwardsModifier parameter, the direction
                // will be modified by subtracting that value from the Y component of the centre point.
                explosionDir.y += upwardsModifier;
                explosionDir.Normalize();
            }

            rb.AddForce(Mathf.Lerp(0f, explosionForce, (1f - explosionDistance)) * explosionDir, mode);
        }

        // --- SpriteRenderer + Camera
        /// <summary>
        /// Resizes an sprite renderer to the size of the <b>orthographic</b> camera fit.
        /// </summary>
        /// <param name="relativeCam">Orthographic camera to resize.</param>
        /// <param name="sr">Sprite renderer to resize.</param>
        /// <param name="axis">Axis to resize.</param>
        public static void ResizeSpriteToScreen(this Camera relativeCam, SpriteRenderer sr, TransformAxis2D axis = TransformAxis2D.XYAxis)
        {
            if (sr == null || relativeCam == null)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] There is a null variable. Returning.");
                return;
            }

            sr.transform.localScale = new Vector3(1, 1, 1);

            float width = sr.sprite.bounds.size.x;
            float height = sr.sprite.bounds.size.y;

            float worldScreenHeight = relativeCam.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            Vector3 scale;
            switch (axis)
            {
                default:
                case TransformAxis2D.XYAxis:
                    scale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.XAxis:
                    scale = new Vector3(worldScreenWidth / width, sr.transform.localScale.y, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.YAxis:
                    scale = new Vector3(sr.transform.localScale.x, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
            }

            if (sr.drawMode != SpriteDrawMode.Tiled)
            {
                sr.transform.localScale = scale;
            }
            else
            {
                // If the drawing is tiled, resize to screen using the tile properties.
                sr.size = scale;
            }
        }
        /// <summary>
        /// Resizes an mesh renderer to the size of the <b>orthographic</b> camera fit.
        /// </summary>
        /// <param name="relativeCam">Orthographic camera to resize.</param>
        /// <param name="sr">Sprite renderer to resize.</param>
        /// <param name="axis">Axis to resize.</param>
        public static void ResizeMeshToScreen(this Camera relativeCam, MeshFilter sr, TransformAxis2D axis = TransformAxis2D.XYAxis)
        {
            if (sr == null || relativeCam == null)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] There is a null variable. Returning.");
                return;
            }
            if (!relativeCam.orthographic)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] Camera relative to resize isn't ortographic. The resizing won't be correct.");
            }

            sr.transform.localScale = new Vector3(1, 1, 1);

            float width = sr.mesh.bounds.size.x;
            float height = sr.mesh.bounds.size.y;

            float worldScreenHeight = relativeCam.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            switch (axis)
            {
                // Do nothing if none.
                case TransformAxis2D.None:
                    Debug.LogWarning("[Additionals::ResizeSpriteToScreen] TransformAxis2D to resize was none.");
                    return;

                default:
                case TransformAxis2D.XYAxis:
                    sr.transform.localScale =
                        new Vector3(worldScreenWidth / width, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.XAxis:
                    sr.transform.localScale =
                        new Vector3(worldScreenWidth / width, sr.transform.localScale.y, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.YAxis:
                    sr.transform.localScale =
                        new Vector3(sr.transform.localScale.x, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
            }
        }

        // -- FileSystem
        /// <summary>
        /// Copies the given directory.
        /// </summary>
        /// <param name="sourceDirName">The given directory to copy. If this does not exist then <see cref="DirectoryNotFoundException"/> is thrown.</param>
        /// <param name="destDirName">
        /// The destination directory to copy into. This is created if it doesn't exist, 
        /// and it shouldn't be the same as <paramref name="sourceDirName"/>.
        /// </param>
        /// <param name="copySubDirs">Whether to also copy the subdirectories on the <paramref name="sourceDirName"/> into <paramref name="destDirName"/>. (recursive method)</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(string.Format("[Additionals::DirectoryCopy] Source directory does not exist or could not be found: {0}", sourceDirName));
            }

            if (sourceDirName.Equals(destDirName))
            {
                Debug.LogWarning("[Additionals::DirectoryCopy] The directory you are trying to copy is the same as the destination directory.");
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Returns a byte array from an object.
        /// </summary>
        /// <param name="obj">Object that has <see cref="SerializableAttribute"/>.</param>
        /// <returns>Object as serializd byte array.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static byte[] ObjectToByteArray(object obj)
        {
            // Can't convert null object.
            if (obj is null)
            {
                throw new ArgumentNullException("[Additionals::ObjectToByteArray] The given object is null.");
            }
            // Require SerializableAttribute
            if (obj.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                throw new ArgumentException(string.Format("[Additionals::ObjectToByteArray] Given object '{0}' with type '{1}' does not have the [System.Serializable] attribute.", obj, obj.GetType()));
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // -- String
        /// <summary>
        /// Utility method to remove diacritics (sans some non-ascii non-diacritic characters) from the string.
        /// </summary>
        /// <param name="str">Target string.</param>
        /// <param name="predicateMustKeep">
        /// Return true if the given character parameter 
        /// should be kept as is (from source string <paramref name="str"/>).
        /// </param>
        public static string RemoveInvalidChars(string str, Func<char, bool> predicateMustKeep = null)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            string normalizedStr = str.Normalize(NormalizationForm.FormD);

            for (int i = 0, j = 0; i < normalizedStr.Length; i++, j++)
            {
                char cNorm = normalizedStr[i];

                // nonspacing mark : the diacritic itself, just as a seperate char
                if (CharUnicodeInfo.GetUnicodeCategory(cNorm) != UnicodeCategory.NonSpacingMark)
                {
                    // Access 'cDefault' here to avoid IndexOutOfRangeException becuase incrementing-decrementing
                    // the base string index happens after this if control (basically we need the safe 'j' index)
                    char cDefault = str[j];

                    // Keep if the predicate is successful
                    bool? pred = predicateMustKeep?.Invoke(cDefault);
                    if (pred ?? false)
                    {
                        sb.Append(cDefault);
                        continue;
                    }
                    else if (pred != null)
                    {
                        // Predicate failed but exists, convert directly to ascii counterpart of it assuming the character is latin.
                        // TODO : but how?
                        switch (cNorm)
                        {
                            default:
                                // This warning is redundant for diacriticified chars that fail the predicate test.
                                // so just control the cNorm as that is successfully seperated from it's diacritics.

                                // i also decided to not print the warning.
                                // Debug.LogWarning(string.Format("[LocalizedText::RemoveDiacritics] Failed to convert fail match char '{0}' into ascii. Appending normalized as is.", cDefault));
                                break;

                            // This is actually diacriticifying (the thing that is called a 'diacritic' is the points on top the letters
                            // This is more like 'ascii-ifying' the text.
                            // Only latin case is this. this will do for now.
                            case 'ı':
                                cNorm = 'i';
                                break;
                        }
                    }

                    sb.Append(cNorm);
                }
                else
                {
                    // nonspacing marks doesn't exist on the actual string.
                    j--;
                }
            }

            return sb.ToString();
        }
    }
}
