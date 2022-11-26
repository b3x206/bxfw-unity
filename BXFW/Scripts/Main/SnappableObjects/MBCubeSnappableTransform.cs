using System;
using System.Collections.Generic;
using UnityEngine;

/// About the 'SnappableTransforms' being a class that you inherit from :
///     * That may cause issues on multiple classes that use a component system instead of a 'oop' approach
///     * However, this approach has the advantage of not calling <see cref="GameObject.GetComponent{T}"/> twice.
///     
///     * Because of this, i may make these classes as sealed class components instead of inheritable classes.
///     * so, a maybe TODO moment here again??
///     (this method is fine though if you are careful enough)

namespace BXFW
{
    /// <summary>
    /// Snap points.
    /// <br>(assuming the cube's front point is looking to <see cref="Vector3.forward"/> 
    /// and we are looking towards the cube's <see cref="Transform.forward"/>.)</br>
    /// </summary>
    public enum SnapPoint
    {
        // Corners
        LowerDwnLeft,       // 0: (-1, -1, -1)
        LowerDwnRight,      // 1: (1, -1, -1)
        UpperDwnRight,      // 2: (1, 1, -1)
        UpperDwnLeft,       // 3: (-1, 1, -1)
        LowerUpLeft,        // 4: (-1, -1, 1)
        LowerUpRight,       // 5: (1, -1, 1)
        UpperUpRight,       // 6: (1, 1, 1)
        UpperUpLeft,        // 7: (-1, 1, 1)

        // Center Pieces (Inbetween corners)
        LowerDwnCenter,
        UpperDwnCenter,
        LowerUpCenter,
        UpperUpCenter,
        LowerLeftCenter,
        UpperLeftCenter,
        LowerRightCenter,
        UpperRightCenter,

        // Center (it's literally the center)
        ObjCenter
    }

    /// <summary>
    /// Makes any transform have cubic snappable points.
    /// Also includes the <see cref="MonoBehaviour"/>, use it as a class that you inherit from.
    /// </summary>
    [Serializable]
    public class MBCubeSnappableTransform : MonoBehaviour
    {
        /// <summary>
        /// Boolean to check if whether a <see cref="SnappableCubeTransform"/> is setup.
        /// </summary>
        public bool Snappable_IsSetup { get; private set; } = false;

        private Dictionary<SnapPoint, Transform> m_Snappable_SnapPoints;
        /// <summary>
        /// Snap points that are assigned on <see cref="InitSetupSnapTransform()"/>
        /// </summary>
        public Dictionary<SnapPoint, Transform> Snappable_SnapPoints
        {
            get
            {
                if (m_Snappable_SnapPoints == null)
                {
                    InitSetupSnapTransform();
                }

                return m_Snappable_SnapPoints;
            }
            private set
            {
                m_Snappable_SnapPoints = value;
            }
        }

        protected Action OnSnapTransformCall;
        protected Action OnAlignTransformCall;

        public void InitSetupSnapTransform()
        {
            if (Snappable_IsSetup) return;

            Snappable_SnapPoints = new Dictionary<SnapPoint, Transform>();

            // Weird, but this might happen on very edge situations.
            // (Like calling 'Destroy()' at the same time on this method or having a 'new Transform transform' variable)
            if (transform == null)
            {
                Debug.LogError(string.Format("[SnappableCubeTransform::SetupTransform] Transform is null. Please setup the script properly on class \"{0}\".", GetType().FullName));
                return;
            }

            #region Corner Snap Pieces
            // ---- 0 ---- ////////////////////////// DOWN (back)
            var tLDL = new GameObject("CubeSnap-LowerDwnLeft").transform;
            tLDL.SetParent(transform);
            tLDL.localScale = Vector3.one;
            tLDL.localRotation = Quaternion.identity;
            tLDL.localPosition = new Vector3(-0.5f, -0.5f, -0.5f);
            Snappable_SnapPoints.Add(SnapPoint.LowerDwnLeft, tLDL);

            // ---- 1 ----
            var tLDR = new GameObject("CubeSnap-LowerDwnRight").transform;
            tLDR.SetParent(transform);
            tLDR.localScale = Vector3.one;
            tLDR.localRotation = Quaternion.identity;
            tLDR.localPosition = new Vector3(0.5f, -0.5f, -0.5f);
            Snappable_SnapPoints.Add(SnapPoint.LowerDwnRight, tLDR);

            // ---- 2 ----
            var tUDR = new GameObject("CubeSnap-UpperDwnRight").transform;
            tUDR.SetParent(transform);
            tUDR.localScale = Vector3.one;
            tUDR.localRotation = Quaternion.identity;
            tUDR.localPosition = new Vector3(0.5f, 0.5f, -0.5f);
            Snappable_SnapPoints.Add(SnapPoint.UpperDwnRight, tUDR);

            // ---- 3 ----
            var tUDL = new GameObject("CubeSnap-UpperDwnLeft").transform;
            tUDL.SetParent(transform);
            tUDL.localScale = Vector3.one;
            tUDL.localRotation = Quaternion.identity;
            tUDL.localPosition = new Vector3(-0.5f, 0.5f, -0.5f);
            Snappable_SnapPoints.Add(SnapPoint.UpperDwnLeft, tUDL);

            // ---- 4 ---- ////////////////////////// UP (front)
            var tLUL = new GameObject("CubeSnap-LowerUpLeft").transform;
            tLUL.SetParent(transform);
            tLUL.localScale = Vector3.one;
            tLUL.localRotation = Quaternion.identity;
            tLUL.localPosition = new Vector3(-0.5f, -0.5f, 0.5f);
            Snappable_SnapPoints.Add(SnapPoint.LowerUpLeft, tLUL);

            // ---- 5 ----
            var tLUR = new GameObject("CubeSnap-LowerUpRight").transform;
            tLUR.SetParent(transform);
            tLUR.localScale = Vector3.one;
            tLUR.localRotation = Quaternion.identity;
            tLUR.localPosition = new Vector3(0.5f, -0.5f, 0.5f);
            Snappable_SnapPoints.Add(SnapPoint.LowerUpRight, tLUR);

            // ---- 6 ----
            var tUUR = new GameObject("CubeSnap-UpperUpRight").transform;
            tUUR.SetParent(transform);
            tUUR.localScale = Vector3.one;
            tUUR.localRotation = Quaternion.identity;
            tUUR.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
            Snappable_SnapPoints.Add(SnapPoint.UpperUpRight, tUUR);

            // ---- 7 ----
            var tUUL = new GameObject("CubeSnap-UpperUpLeft").transform;
            tUUL.SetParent(transform);
            tUUL.localScale = Vector3.one;
            tUUL.localRotation = Quaternion.identity;
            tUUL.localPosition = new Vector3(-0.5f, 0.5f, 0.5f);
            Snappable_SnapPoints.Add(SnapPoint.UpperUpLeft, tUUL);
            #endregion

            #region Center Snap Pieces
            var tLDC = new GameObject("CubeSnap-LowerDwnCenter").transform;
            tLDC.SetParent(transform);
            tLDC.localScale = Vector3.one;
            tLDC.localRotation = Quaternion.identity;
            tLDC.localPosition = new Vector3(0f, -0.5f, -0.5f);
            Snappable_SnapPoints.Add(SnapPoint.LowerDwnCenter, tLDC);

            var tUDC = new GameObject("CubeSnap-UpperDwnCenter").transform;
            tUDC.SetParent(transform);
            tUDC.localScale = Vector3.one;
            tUDC.localRotation = Quaternion.identity;
            tUDC.localPosition = new Vector3(0f, 0.5f, -0.5f);
            Snappable_SnapPoints.Add(SnapPoint.UpperDwnCenter, tUDC);

            var tLUC = new GameObject("CubeSnap-LowerUpCenter").transform;
            tLUC.SetParent(transform);
            tLUC.localScale = Vector3.one;
            tLUC.localRotation = Quaternion.identity;
            tLUC.localPosition = new Vector3(0f, -0.5f, 0.5f);
            Snappable_SnapPoints.Add(SnapPoint.LowerUpCenter, tLUC);

            var tUUC = new GameObject("CubeSnap-UpperUpCenter").transform;
            tUUC.SetParent(transform);
            tUUC.localScale = Vector3.one;
            tUUC.localRotation = Quaternion.identity;
            tUUC.localPosition = new Vector3(0f, 0.5f, 0.5f);
            Snappable_SnapPoints.Add(SnapPoint.UpperUpCenter, tUUC);

            var tLLC = new GameObject("CubeSnap-LowerLeftCenter").transform;
            tLLC.SetParent(transform);
            tLLC.localScale = Vector3.one;
            tLLC.localRotation = Quaternion.identity;
            tLLC.localPosition = new Vector3(-0.5f, -0.5f, 0f);
            Snappable_SnapPoints.Add(SnapPoint.LowerLeftCenter, tLLC);

            var tULC = new GameObject("CubeSnap-UpperLeftCenter").transform;
            tULC.SetParent(transform);
            tULC.localScale = Vector3.one;
            tULC.localRotation = Quaternion.identity;
            tULC.localPosition = new Vector3(-0.5f, 0.5f, 0f);
            Snappable_SnapPoints.Add(SnapPoint.UpperLeftCenter, tULC);

            var tLRC = new GameObject("CubeSnap-LowerRightCenter").transform;
            tLRC.SetParent(transform);
            tLRC.localScale = Vector3.one;
            tLRC.localRotation = Quaternion.identity;
            tLRC.localPosition = new Vector3(0.5f, -0.5f, 0f);
            Snappable_SnapPoints.Add(SnapPoint.LowerRightCenter, tLRC);

            var tURC = new GameObject("CubeSnap-UpperRightCenter").transform;
            tURC.SetParent(transform);
            tURC.localScale = Vector3.one;
            tURC.localRotation = Quaternion.identity;
            tURC.localPosition = new Vector3(0.5f, 0.5f, 0f);
            Snappable_SnapPoints.Add(SnapPoint.UpperRightCenter, tURC);

            var tCTR = new GameObject("CubeSnap-ObjCenter").transform;
            tCTR.SetParent(transform);
            tCTR.localScale = Vector3.one;
            tCTR.localRotation = Quaternion.identity;
            tCTR.localPosition = new Vector3(0.5f, 0.5f, 0f);
            Snappable_SnapPoints.Add(SnapPoint.ObjCenter, tURC);
            #endregion

            Snappable_IsSetup = true;
        }

        #region Extension Functions
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="transformTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <param name="pointTarget">Snap point for target.</param>
        /// <param name="snapTarget">Given Snap. Change this to swap the object to move.</param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(MBCubeSnappableTransform transformTarget, SnapPoint pointThis, SnapPoint pointTarget, bool snapTarget = false)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null) return false;

            // Check and setup transforms.
            if (!Snappable_IsSetup) InitSetupSnapTransform();
            if (!transformTarget.Snappable_IsSetup) transformTarget.InitSetupSnapTransform();

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var SnapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            if (snapTarget)
            {
                var PrevParent = transformTarget.transform.parent;

                SnapHelper.position = transformTarget.Snappable_SnapPoints[pointTarget].position;
                transformTarget.transform.SetParent(SnapHelper);
                SnapHelper.position = Snappable_SnapPoints[pointThis].position;
                transformTarget.transform.SetParent(PrevParent);
            }
            else
            {
                var PrevParent = transform.parent;

                SnapHelper.position = Snappable_SnapPoints[pointThis].position;
                transform.SetParent(SnapHelper);
                SnapHelper.position = transformTarget.Snappable_SnapPoints[pointTarget].position;
                transform.SetParent(PrevParent);
            }

            Destroy(SnapHelper.gameObject);
            if (OnSnapTransformCall != null)
                OnSnapTransformCall();

            return true;
        }
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="transformTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(Transform transformTarget, SnapPoint pointThis, Vector3 transformTargetPosOffset = default)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null) return false;

            // Check and setup transforms.
            if (!Snappable_IsSetup) InitSetupSnapTransform();

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var SnapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            var PrevParent = transformTarget.transform.parent;

            SnapHelper.position = transformTarget.position + transformTargetPosOffset;
            transformTarget.transform.SetParent(SnapHelper);
            SnapHelper.position = Snappable_SnapPoints[pointThis].position;
            transformTarget.transform.SetParent(PrevParent);

            Destroy(SnapHelper.gameObject);
            if (OnSnapTransformCall != null)
                OnSnapTransformCall();

            return true;
        }

        /// <summary>
        /// NOTE : This method is not tested. Please test before using.
        /// TODO (would be nice for more platform variety).
        /// </summary>
        /// <param name="transformTarget"></param>
        /// <param name="pointTarget"></param>
        /// <param name="alignAxis"></param>
        /// <param name="alignToTarget"></param>
        /// <param name="customTargetDist"></param>
        /// <returns></returns>
        public bool AlignTransform(MBCubeSnappableTransform transformTarget, SnapPoint pointThis, SnapPoint pointTarget,
            Vector3 alignAxis, bool alignToTarget = true, float? customTargetDist = null)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null || alignAxis == Vector3.zero) return false;

            // Check and setup transforms.
            if (!Snappable_IsSetup) InitSetupSnapTransform();
            if (!transformTarget.Snappable_IsSetup) transformTarget.InitSetupSnapTransform();

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var SnapHelper = new GameObject("SnapHelper").transform;

            if (alignToTarget)
            {
                var PrevParent = transformTarget.transform.parent;

                SnapHelper.position = transformTarget.Snappable_SnapPoints[pointTarget].position;
                transformTarget.transform.SetParent(SnapHelper);

                // -- Setup axis stuff
                var sHelperPosSet = new Vector3();
                var snappableSPointPos = Snappable_SnapPoints[pointThis].position;
                if (alignAxis.x > 0)
                {
                    sHelperPosSet.x = snappableSPointPos.x;
                }
                else if (alignAxis.x < 0)
                {
                    sHelperPosSet.x = -snappableSPointPos.x;
                }

                if (alignAxis.y > 0)
                {
                    sHelperPosSet.y = snappableSPointPos.y;
                }
                else if (alignAxis.y < 0)
                {
                    sHelperPosSet.y = -snappableSPointPos.y;
                }

                if (alignAxis.z > 0)
                {
                    sHelperPosSet.z = snappableSPointPos.z;
                }
                else if (alignAxis.z < 0)
                {
                    sHelperPosSet.z = -snappableSPointPos.z;
                }

                SnapHelper.position = sHelperPosSet; // Snappable_SnapPoints[pointParent].position;
                transformTarget.transform.SetParent(PrevParent);
            }
            else
            {
                var PrevParent = transform.parent;

                SnapHelper.position = Snappable_SnapPoints[pointThis].position;
                transform.SetParent(SnapHelper);
                // -- Setup axis stuff
                var sHelperPosSet = new Vector3();
                var snappableSPointPos = transformTarget.Snappable_SnapPoints[pointThis].position;
                if (alignAxis.x > 0)
                {
                    sHelperPosSet.x = snappableSPointPos.x;
                }
                else if (alignAxis.x < 0)
                {
                    sHelperPosSet.x = -snappableSPointPos.x;
                }

                if (alignAxis.y > 0)
                {
                    sHelperPosSet.y = snappableSPointPos.y;
                }
                else if (alignAxis.y < 0)
                {
                    sHelperPosSet.y = -snappableSPointPos.y;
                }

                if (alignAxis.z > 0)
                {
                    sHelperPosSet.z = snappableSPointPos.z;
                }
                else if (alignAxis.z < 0)
                {
                    sHelperPosSet.z = -snappableSPointPos.z;
                }

                SnapHelper.position = sHelperPosSet; // transformTarget.Snappable_SnapPoints[pointParent].position;
                transform.SetParent(PrevParent);
            }

            Destroy(SnapHelper.gameObject);
            if (OnAlignTransformCall != null)
                OnAlignTransformCall();

            return true;
        }
        // TODO 2 : Add align transform method for normal transforms.
        #endregion
    }
}