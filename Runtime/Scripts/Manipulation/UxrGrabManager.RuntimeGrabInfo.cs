// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.RuntimeGrabInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information of grabs performed on a <see cref="UxrGrabbableObject" /> at runtime.
        /// </summary>
        private class RuntimeGrabInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the index in the <see cref="GrabbedPoints" /> (internal list of grab points currently being grabbed) that is
            ///     the main grab.
            ///     The main grab is the first grab point of an <see cref="UxrGrabbableObject" />. If the main grab point isn't
            ///     currently being grabbed, it returns the first grab point in the internal list.
            /// </summary>
            /// <seealso cref="UxrGrabbableObject.FirstGrabPointIsMain" />
            public int MainPointIndex
            {
                get
                {
                    for (int i = 0; i < GrabbedPoints.Count; ++i)
                    {
                        if (GrabbedPoints[i] == 0)
                        {
                            return i;
                        }
                    }

                    return 0;
                }
            }

            /// <summary>
            ///     Gets the current interpolation value of a smooth "look at" rotation. "Look at" rotations of
            ///     <see cref="UxrGrabbableObject" /> are transitions to/from a two-handed grab.
            /// </summary>
            public float LookAtT
            {
                get
                {
                    if (LookAtTimer < 0.0f)
                    {
                        return 1.0f;
                    }

                    return 1.0f - Mathf.Clamp01(LookAtTimer / UxrGrabbableObject.HandLockSeconds);
                }
            }

            /// <summary>
            ///     Gets the list of grabbers that are currently grabbing the object. Each item maps to the
            ///     <see cref="GrabbedPoints" /> list.
            /// </summary>
            public List<UxrGrabber> Grabbers { get; }

            /// <summary>
            ///     Gets the list of points are currently being grabbed from the object. Each item maps to the <see cref="Grabbers" />
            ///     list.
            /// </summary>
            public List<int> GrabbedPoints { get; }

            /// <summary>
            ///     Gets the target from where the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public UxrGrabbableObjectAnchor AnchorFrom { get; }

            /// <summary>
            ///     Gets <see cref="UxrGrabbableObject" />'s local position before being updated by the grab manager.
            /// </summary>
            public Vector3 LocalPositionBeforeUpdate { get; set; }

            /// <summary>
            ///     Gets <see cref="UxrGrabbableObject" />'s local rotation before being updated by the grab manager.
            /// </summary>
            public Quaternion LocalRotationBeforeUpdate { get; set; }

            /// <summary>
            ///     Gets the timer value that is used to perform smooth "Look At" transitions.
            ///     <seealso cref="LookAtT" />
            /// </summary>
            public float LookAtTimer { get; set; }

            /// <summary>
            ///     Gets or sets the grabbable parent being grabbed if there is one.
            /// </summary>
            public UxrGrabbableObject GrabbableParentBeingGrabbed { get; set; }

            /// <summary>
            ///     Gets or sets the amount of grabbable objects that dependent on this grab.
            /// </summary>
            public int ChildDependentGrabCount { get; set; }

            /// <summary>
            ///     Gets or sets the amount of dependent grabs processed in multiple-pass grab processing. It is reset each frame.
            /// </summary>
            public int ChildDependentGrabProcessed { get; set; }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="grabber">Grabber of the grab</param>
            /// <param name="grabPoint">Grab point index of the <see cref="UxrGrabbableObject" /> that was grabbed.</param>
            /// <param name="anchorFrom">Target if the grabbed object was placed on any.</param>
            public RuntimeGrabInfo(UxrGrabber grabber, int grabPoint, UxrGrabbableObjectAnchor anchorFrom = null)
            {
                Grabbers      = new List<UxrGrabber>();
                GrabbedPoints = new List<int>();
                Grabbers.Add(grabber);
                GrabbedPoints.Add(grabPoint);

                LocalPositionBeforeUpdate = grabber.GrabbedObject.transform.localPosition;
                LocalRotationBeforeUpdate = grabber.GrabbedObject.transform.localRotation;

                LookAtTimer = -1.0f;

                GrabbableParentBeingGrabbed = null;
                ChildDependentGrabCount     = 0;
                ChildDependentGrabProcessed = 0;

                AnchorFrom = anchorFrom;
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Registers a grabber swap to indicate that a different hand is now grabbing the point.
            /// </summary>
            /// <param name="oldGrabber">Old grabber that was grabbing</param>
            /// <param name="newGrabber">New grabber that the grab switched to</param>
            public void SwapGrabber(UxrGrabber oldGrabber, UxrGrabber newGrabber)
            {
                Grabbers[Grabbers.IndexOf(oldGrabber)] = newGrabber;
            }

            /// <summary>
            ///     Registers a grabber swap to indicate that a different hand is now grabbing another point.
            /// </summary>
            /// <param name="oldGrabber">Old grabber that was grabbing</param>
            /// <param name="oldGrabPoint">Old grab point of the <see cref="UxrGrabbableObject" /> grabbed by the old grabber</param>
            /// <param name="newGrabber">New grabber that the grab switched to</param>
            /// <param name="newGrabPoint">New grab point of the <see cref="UxrGrabbableObject" /> the grab switched to</param>
            public void SwapGrabber(UxrGrabber oldGrabber, int oldGrabPoint, UxrGrabber newGrabber, int newGrabPoint)
            {
                int index = Grabbers.IndexOf(oldGrabber);
                Grabbers[index]      = newGrabber;
                GrabbedPoints[index] = newGrabPoint;
            }

            /// <summary>
            ///     Registers a new grab.
            /// </summary>
            /// <param name="grabber">Grabber that performed the grab</param>
            /// <param name="grabPoint">The point of the <see cref="UxrGrabbableObject" /> that was grabbed.</param>
            /// <param name="append">
            ///     Whether to append or insert at the beginning. If there is more than one grab point and none of
            ///     them is the 0 index (main grab), the main grab will be the first one in the list.
            /// </param>
            public void AddGrabber(UxrGrabber grabber, int grabPoint, bool append = true)
            {
                if (append)
                {
                    Grabbers.Add(grabber);
                    GrabbedPoints.Add(grabPoint);
                }
                else
                {
                    Grabbers.Insert(0, grabber);
                    GrabbedPoints.Insert(0, grabPoint);
                }
            }

            /// <summary>
            ///     Registers a release of a grab.
            /// </summary>
            /// <param name="grabber">Grabber that released the grab.</param>
            public void RemoveGrabber(UxrGrabber grabber)
            {
                int index = Grabbers.IndexOf(grabber);

                if (index >= 0)
                {
                    Grabbers.RemoveAt(index);
                    GrabbedPoints.RemoveAt(index);
                }
            }

            /// <summary>
            ///     Removes all grabs registered.
            /// </summary>
            public void RemoveAll()
            {
                Grabbers.Clear();
                GrabbedPoints.Clear();
            }

            #endregion

            #region Private Types & Data

            private Rigidbody _rigidbody;

            #endregion
        }

        #endregion
    }
}