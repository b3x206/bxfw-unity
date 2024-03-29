﻿using System;
using UnityEngine;
using System.Collections.Generic;

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
        public bool IsSetup { get; private set; } = false;

        /// <inheritdoc cref="SnapPoints"/>
        private Dictionary<SnapPoint, Transform> m_SnapPoints;
        /// <summary>
        /// Snap points that are assigned on <see cref="SetupSnapTransform()"/>
        /// </summary>
        public Dictionary<SnapPoint, Transform> SnapPoints
        {
            get
            {
                if (m_SnapPoints == null)
                {
                    SetupSnapTransform();
                }

                return m_SnapPoints;
            }
        }

        /// <summary>
        /// An action called when any of the <see cref="SnapTransform"/> methods were called.
        /// </summary>
        protected Action OnSnapTransformCall;
        /// <summary>
        /// An action called when any of the <see cref="AlignTransform"/> methods were called.
        /// </summary>
        protected Action OnAlignTransformCall;
        
        /// <summary>
        /// Sets up the snap transform.
        /// </summary>
        public void SetupSnapTransform()
        {
            if (IsSetup)
            {
                return;
            }

            m_SnapPoints = new Dictionary<SnapPoint, Transform>();

            // Weird, but this might happen on very edge situations.
            // (Like calling 'Destroy()' at the same time on this method or having a 'new Transform transform' variable)
            if (transform == null)
            {
                Debug.LogError(string.Format("[SnappableCubeTransform::SetupTransform] Transform is null. Object : \"{0}\".", GetType().FullName));
                return;
            }

            #region Corner Snap Pieces
            // ---- 0 ---- ////////////////////////// DOWN (back)
            var tLDL = new GameObject("CubeSnap-LowerDwnLeft").transform;
            tLDL.SetParent(transform);
            tLDL.localScale = Vector3.one;
            tLDL.localRotation = Quaternion.identity;
            tLDL.localPosition = new Vector3(-0.5f, -0.5f, -0.5f);
            SnapPoints.Add(SnapPoint.LowerDwnLeft, tLDL);

            // ---- 1 ----
            var tLDR = new GameObject("CubeSnap-LowerDwnRight").transform;
            tLDR.SetParent(transform);
            tLDR.localScale = Vector3.one;
            tLDR.localRotation = Quaternion.identity;
            tLDR.localPosition = new Vector3(0.5f, -0.5f, -0.5f);
            SnapPoints.Add(SnapPoint.LowerDwnRight, tLDR);

            // ---- 2 ----
            var tUDR = new GameObject("CubeSnap-UpperDwnRight").transform;
            tUDR.SetParent(transform);
            tUDR.localScale = Vector3.one;
            tUDR.localRotation = Quaternion.identity;
            tUDR.localPosition = new Vector3(0.5f, 0.5f, -0.5f);
            SnapPoints.Add(SnapPoint.UpperDwnRight, tUDR);

            // ---- 3 ----
            var tUDL = new GameObject("CubeSnap-UpperDwnLeft").transform;
            tUDL.SetParent(transform);
            tUDL.localScale = Vector3.one;
            tUDL.localRotation = Quaternion.identity;
            tUDL.localPosition = new Vector3(-0.5f, 0.5f, -0.5f);
            SnapPoints.Add(SnapPoint.UpperDwnLeft, tUDL);

            // ---- 4 ---- ////////////////////////// UP (front)
            var tLUL = new GameObject("CubeSnap-LowerUpLeft").transform;
            tLUL.SetParent(transform);
            tLUL.localScale = Vector3.one;
            tLUL.localRotation = Quaternion.identity;
            tLUL.localPosition = new Vector3(-0.5f, -0.5f, 0.5f);
            SnapPoints.Add(SnapPoint.LowerUpLeft, tLUL);

            // ---- 5 ----
            var tLUR = new GameObject("CubeSnap-LowerUpRight").transform;
            tLUR.SetParent(transform);
            tLUR.localScale = Vector3.one;
            tLUR.localRotation = Quaternion.identity;
            tLUR.localPosition = new Vector3(0.5f, -0.5f, 0.5f);
            SnapPoints.Add(SnapPoint.LowerUpRight, tLUR);

            // ---- 6 ----
            var tUUR = new GameObject("CubeSnap-UpperUpRight").transform;
            tUUR.SetParent(transform);
            tUUR.localScale = Vector3.one;
            tUUR.localRotation = Quaternion.identity;
            tUUR.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
            SnapPoints.Add(SnapPoint.UpperUpRight, tUUR);

            // ---- 7 ----
            var tUUL = new GameObject("CubeSnap-UpperUpLeft").transform;
            tUUL.SetParent(transform);
            tUUL.localScale = Vector3.one;
            tUUL.localRotation = Quaternion.identity;
            tUUL.localPosition = new Vector3(-0.5f, 0.5f, 0.5f);
            SnapPoints.Add(SnapPoint.UpperUpLeft, tUUL);
            #endregion

            #region Center Snap Pieces
            var tLDC = new GameObject("CubeSnap-LowerDwnCenter").transform;
            tLDC.SetParent(transform);
            tLDC.localScale = Vector3.one;
            tLDC.localRotation = Quaternion.identity;
            tLDC.localPosition = new Vector3(0f, -0.5f, -0.5f);
            SnapPoints.Add(SnapPoint.LowerDwnCenter, tLDC);

            var tUDC = new GameObject("CubeSnap-UpperDwnCenter").transform;
            tUDC.SetParent(transform);
            tUDC.localScale = Vector3.one;
            tUDC.localRotation = Quaternion.identity;
            tUDC.localPosition = new Vector3(0f, 0.5f, -0.5f);
            SnapPoints.Add(SnapPoint.UpperDwnCenter, tUDC);

            var tLUC = new GameObject("CubeSnap-LowerUpCenter").transform;
            tLUC.SetParent(transform);
            tLUC.localScale = Vector3.one;
            tLUC.localRotation = Quaternion.identity;
            tLUC.localPosition = new Vector3(0f, -0.5f, 0.5f);
            SnapPoints.Add(SnapPoint.LowerUpCenter, tLUC);

            var tUUC = new GameObject("CubeSnap-UpperUpCenter").transform;
            tUUC.SetParent(transform);
            tUUC.localScale = Vector3.one;
            tUUC.localRotation = Quaternion.identity;
            tUUC.localPosition = new Vector3(0f, 0.5f, 0.5f);
            SnapPoints.Add(SnapPoint.UpperUpCenter, tUUC);

            var tLLC = new GameObject("CubeSnap-LowerLeftCenter").transform;
            tLLC.SetParent(transform);
            tLLC.localScale = Vector3.one;
            tLLC.localRotation = Quaternion.identity;
            tLLC.localPosition = new Vector3(-0.5f, -0.5f, 0f);
            SnapPoints.Add(SnapPoint.LowerLeftCenter, tLLC);

            var tULC = new GameObject("CubeSnap-UpperLeftCenter").transform;
            tULC.SetParent(transform);
            tULC.localScale = Vector3.one;
            tULC.localRotation = Quaternion.identity;
            tULC.localPosition = new Vector3(-0.5f, 0.5f, 0f);
            SnapPoints.Add(SnapPoint.UpperLeftCenter, tULC);

            var tLRC = new GameObject("CubeSnap-LowerRightCenter").transform;
            tLRC.SetParent(transform);
            tLRC.localScale = Vector3.one;
            tLRC.localRotation = Quaternion.identity;
            tLRC.localPosition = new Vector3(0.5f, -0.5f, 0f);
            SnapPoints.Add(SnapPoint.LowerRightCenter, tLRC);

            var tURC = new GameObject("CubeSnap-UpperRightCenter").transform;
            tURC.SetParent(transform);
            tURC.localScale = Vector3.one;
            tURC.localRotation = Quaternion.identity;
            tURC.localPosition = new Vector3(0.5f, 0.5f, 0f);
            SnapPoints.Add(SnapPoint.UpperRightCenter, tURC);

            var tCTR = new GameObject("CubeSnap-ObjCenter").transform;
            tCTR.SetParent(transform);
            tCTR.localScale = Vector3.one;
            tCTR.localRotation = Quaternion.identity;
            tCTR.localPosition = new Vector3(0.5f, 0.5f, 0f);
            SnapPoints.Add(SnapPoint.ObjCenter, tURC);
            #endregion

            IsSetup = true;
        }

        #region Extension Functions
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="transformTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <param name="pointTarget">Snap point for target.</param>
        /// <param name="snapTarget">
        /// The snap object target => 
        /// <br><see langword="true"/> = This method will move &amp; snap the <paramref name="transformTarget"/>.</br>
        /// <br><see langword="false"/> = This method will move &amp; snap the <see cref="Component.transform"/>.</br>
        /// </param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(MBCubeSnappableTransform transformTarget, SnapPoint pointThis, SnapPoint pointTarget, bool snapTarget = false)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null)
            {
                return false;
            }

            // Check and setup transforms.
            if (!IsSetup)
            {
                SetupSnapTransform();
            }

            if (!transformTarget.IsSetup)
            {
                transformTarget.SetupSnapTransform();
            }

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var snapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            if (snapTarget)
            {
                var prevParent = transformTarget.transform.parent;

                snapHelper.position = transformTarget.SnapPoints[pointTarget].position;
                transformTarget.transform.SetParent(snapHelper);
                snapHelper.position = SnapPoints[pointThis].position;
                transformTarget.transform.SetParent(prevParent);
            }
            else
            {
                var prevParent = transform.parent;

                snapHelper.position = SnapPoints[pointThis].position;
                transform.SetParent(snapHelper);
                snapHelper.position = transformTarget.SnapPoints[pointTarget].position;
                transform.SetParent(prevParent);
            }

            Destroy(snapHelper.gameObject);
            OnSnapTransformCall?.Invoke();

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
            if (transformTarget == null)
            {
                return false;
            }

            // Check and setup transforms.
            if (!IsSetup)
            {
                SetupSnapTransform();
            }

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var snapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            var prevParent = transformTarget.transform.parent;

            snapHelper.position = transformTarget.position + transformTargetPosOffset;
            transformTarget.transform.SetParent(snapHelper);
            snapHelper.position = SnapPoints[pointThis].position;
            transformTarget.transform.SetParent(prevParent);

            Destroy(snapHelper.gameObject);
            OnSnapTransformCall?.Invoke();

            return true;
        }

        /// <summary>
        /// Aligns two <see cref="MBCubeSnappableTransform"/>s to each other depending on the point.
        /// </summary>
        /// <param name="transformTarget">Target to align or be aligned into depending on <paramref name="alignTarget"/></param>
        /// <param name="pointTarget">Point of <paramref name="transformTarget"/> to align into.</param>
        /// <param name="alignAxis">Axis to align on. This axis does not specify magnitude and 
        /// only the sign of the axis are taken to account.</param>
        /// <param name="alignTarget">
        /// The align object target => 
        /// <br><see langword="true"/> = This method will move &amp; snap the <paramref name="transformTarget"/>.</br>
        /// <br><see langword="false"/> = This method will move &amp; snap the <see cref="Component.transform"/>.</br>
        /// </param>
        public bool AlignTransform(MBCubeSnappableTransform transformTarget, SnapPoint pointThis, SnapPoint pointTarget, Vector3 alignAxis, bool alignTarget = true)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null || alignAxis == Vector3.zero)
            {
                return false;
            }

            // Check and setup transforms.
            if (!IsSetup)
            {
                SetupSnapTransform();
            }

            if (!transformTarget.IsSetup)
            {
                transformTarget.SetupSnapTransform();
            }

            // -- Alignment
            var snapHelper = new GameObject("SnapHelper").transform;
            if (alignTarget)
            {
                var prevParent = transformTarget.transform.parent;

                snapHelper.position = transformTarget.SnapPoints[pointTarget].position;
                transformTarget.transform.SetParent(snapHelper);

                // -- Setup axis stuff
                var snappableSPointPos = SnapPoints[pointThis].position;
                var sHelperPosSet = Vector3.Scale(snappableSPointPos, alignAxis.SignVector());
                
                snapHelper.position = sHelperPosSet; // Snappable_SnapPoints[pointParent].position;
                transformTarget.transform.SetParent(prevParent);
            }
            else
            {
                var PrevParent = transform.parent;

                snapHelper.position = SnapPoints[pointThis].position;
                transform.SetParent(snapHelper);
                
                // -- Setup axis stuff
                var snappableSPointPos = transformTarget.SnapPoints[pointThis].position;
                var sHelperPosSet = Vector3.Scale(snappableSPointPos, alignAxis.SignVector());

                snapHelper.position = sHelperPosSet; // transformTarget.Snappable_SnapPoints[pointParent].position;
                transform.SetParent(PrevParent);
            }

            Destroy(snapHelper.gameObject);
            OnAlignTransformCall?.Invoke();

            return true;
        }
        // TODO 2 : Add align transform method for normal transforms.
        #endregion
    }
}
