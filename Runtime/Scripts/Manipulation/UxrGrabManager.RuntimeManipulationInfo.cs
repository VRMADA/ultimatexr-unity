// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.RuntimeManipulationInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Serialization;
using UltimateXR.Extensions.System.Math;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information of grabs performed on a <see cref="UxrGrabbableObject" /> at runtime.
        ///     An object being manipulated can have multiple grabs, registered in <see cref="Grabs" />.
        /// </summary>
        /// <remarks>
        ///     Implements <see cref="IUxrSerializable" /> to help <see cref="UxrGrabManager" />'s implementation of the
        ///     <see cref="IUxrStateSave" /> interface (<see cref="UxrGrabManager.SerializeState" />).
        /// </remarks>
        [Serializable]
        private sealed class RuntimeManipulationInfo : IUxrSerializable
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the grabbers currently manipulating the object.
            /// </summary>
            public IEnumerable<UxrGrabber> Grabbers
            {
                get
                {
                    foreach (RuntimeGrabInfo grabInfo in Grabs)
                    {
                        yield return grabInfo.Grabber;
                    }
                }
            }

            /// <summary>
            ///     Gets the points currently being grabbed on the object.
            /// </summary>
            public IEnumerable<int> GrabbedPoints
            {
                get
                {
                    foreach (RuntimeGrabInfo grabInfo in Grabs)
                    {
                        yield return grabInfo.GrabbedPoint;
                    }
                }
            }

            /// <summary>
            ///     Gets the current grabs manipulating the object.
            /// </summary>
            public List<RuntimeGrabInfo> Grabs => _grabs;

            /// <summary>
            ///     Gets the target from where the <see cref="UxrGrabbableObject" /> was grabbed.
            /// </summary>
            public UxrGrabbableObjectAnchor SourceAnchor => _sourceAnchor;

            /// <summary>
            ///     Gets the current rotation angle, in objects constrained to a single rotation axis, contributed by all the grabbers
            ///     manipulating the object.
            /// </summary>
            public float CurrentSingleRotationAngleContributions
            {
                get
                {
                    float accumulation      = 0.0f;
                    int   contributionCount = 0;

                    foreach (RuntimeGrabInfo grabInfo in Grabs)
                    {
                        accumulation += grabInfo.SingleRotationAngleContribution - grabInfo.LastAccumulatedAngle;
                        contributionCount++;
                    }

                    if (contributionCount > 1)
                    {
                        accumulation /= contributionCount;
                    }

                    return accumulation;
                }
            }

            /// <summary>
            ///     Gets the grabbed object.
            /// </summary>
            public UxrGrabbableObject GrabbableObject => _grabbableObject;

            /// <summary>
            ///     Gets the rotation pivot when child grabbable objects manipulate this object's orientation.
            /// </summary>
            public Vector3 LocalManipulationRotationPivot
            {
                get => _localManipulationRotationPivot;
                set => _localManipulationRotationPivot = value;
            }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="grabber">Grabber of the grab</param>
            /// <param name="grabPoint">Grab point index of the <see cref="UxrGrabbableObject" /> that was grabbed.</param>
            /// <param name="sourceAnchor">Target if the grabbed object was placed on any.</param>
            public RuntimeManipulationInfo(UxrGrabber grabber, int grabPoint, UxrGrabbableObjectAnchor sourceAnchor = null)
            {
                Grabs.Add(new RuntimeGrabInfo(grabber, grabPoint));

                _grabbableObject = grabber.GrabbedObject;
                _sourceAnchor    = sourceAnchor;
            }

            /// <summary>
            ///     Default constructor required for serialization.
            /// </summary>
            private RuntimeManipulationInfo()
            {
            }

            #endregion

            #region Implicit IUxrSerializable

            /// <inheritdoc />
            public int SerializationVersion => 0;

            /// <inheritdoc />
            public void Serialize(IUxrSerializer serializer, int serializationVersion)
            {
                serializer.Serialize(ref _grabs);
                serializer.SerializeUniqueComponent(ref _sourceAnchor);
                serializer.SerializeUniqueComponent(ref _grabbableObject);
                serializer.Serialize(ref _localManipulationRotationPivot);
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Registers a new grab.
            /// </summary>
            /// <param name="grabber">Grabber that performed the grab</param>
            /// <param name="grabPoint">The point of the <see cref="UxrGrabbableObject" /> that was grabbed.</param>
            /// <param name="append">
            ///     Whether to append or insert at the beginning. If there is more than one grab point and none of
            ///     them is the 0 index (main grab), the main grab will be the first one in the list.
            /// </param>
            /// <returns>The newly created grab info entry</returns>
            public RuntimeGrabInfo RegisterNewGrab(UxrGrabber grabber, int grabPoint, bool append = true)
            {
                RuntimeGrabInfo runtimeGrabInfo = new RuntimeGrabInfo(grabber, grabPoint);

                if (append)
                {
                    Grabs.Add(runtimeGrabInfo);
                }
                else
                {
                    Grabs.Insert(0, runtimeGrabInfo);
                }

                return runtimeGrabInfo;
            }

            /// <summary>
            ///     Removes a grab.
            /// </summary>
            /// <param name="grabber">Grabber that released the grab</param>
            public void RemoveGrab(UxrGrabber grabber)
            {
                Grabs.RemoveAll(g => g.Grabber == grabber);
            }

            /// <summary>
            ///     Removes all grabs registered.
            /// </summary>
            public void RemoveAll()
            {
                Grabs.Clear();
            }

            /// <summary>
            ///     Notifies a the start of a new grab, in order to compute all required data.
            /// </summary>
            /// <param name="grabber">Grabber responsible for grabbing the object</param>
            /// <param name="grabbableObject">The object being grabbed</param>
            /// <param name="grabPoint">Point that was grabbed</param>
            /// <param name="snapPosition">The grabber snap position to use</param>
            /// <param name="snapRotation">The grabber snap rotation to use</param>
            /// <param name="sourceGrabEventArgs">
            ///     If non-null, the grab will use the information on the event to ensure that
            ///     it is performed in exactly the same way. This is used in multi-player environments.
            /// </param>
            /// <returns>Grab information</returns>
            public RuntimeGrabInfo NotifyBeginGrab(UxrGrabber grabber, UxrGrabbableObject grabbableObject, int grabPoint, Vector3 snapPosition, Quaternion snapRotation, UxrManipulationEventArgs sourceGrabEventArgs = null)
            {
                RuntimeGrabInfo grabInfo = GetGrabInfo(grabber);

                if (grabInfo == null)
                {
                    grabInfo = RegisterNewGrab(grabber, grabPoint);
                }

                // If it's an object constrained to a single rotation axis, accumulate current contributions first

                AccumulateSingleRotationAngle(grabbableObject);

                // Compute data

                grabbableObject.NotifyBeginGrab(grabber, grabPoint, snapPosition, snapRotation);
                grabInfo.Compute(grabber, grabbableObject, grabPoint, snapPosition, snapRotation, sourceGrabEventArgs);

                // Smooth transitions

                if (Instance.Features.HasFlag(UxrManipulationFeatures.SmoothTransitions))
                {
                    grabbableObject.StartSmoothManipulationTransition();
                    grabber.StartSmoothManipulationTransition();

                    if (grabbableObject.GrabbableParent != null && grabbableObject.UsesGrabbableParentDependency && grabbableObject.ControlParentDirection)
                    {
                        grabbableObject.GrabbableParent.StartSmoothManipulationTransition();
                    }
                }

                return grabInfo;
            }

            /// <summary>
            ///     Notifies the end of a grab.
            /// </summary>
            /// <param name="grabber">Grabber that released the object</param>
            /// <param name="grabbableObject">Object that was released</param>
            /// <param name="grabPoint">Grab point that was released</param>
            public void NotifyEndGrab(UxrGrabber grabber, UxrGrabbableObject grabbableObject, int grabPoint)
            {
                // If it's an object constrained to a single rotation axis, accumulate current contributions first

                AccumulateSingleRotationAngle(grabbableObject);

                // Notify object

                grabbableObject.NotifyEndGrab(grabber, grabPoint);

                // Smooth transitions

                if (Instance.Features.HasFlag(UxrManipulationFeatures.SmoothTransitions))
                {
                    if (!(grabbableObject.RigidBodySource != null && grabbableObject.RigidBodyDynamicOnRelease))
                    {
                        grabbableObject.StartSmoothManipulationTransition();
                    }
                    
                    grabber.StartSmoothManipulationTransition();

                    if (grabbableObject.GrabbableParent != null && grabbableObject.UsesGrabbableParentDependency && grabbableObject.ControlParentDirection)
                    {
                        grabbableObject.GrabbableParent.StartSmoothManipulationTransition();
                    }
                }
            }

            /// <summary>
            ///     Gets the grab information of a specific grabber.
            /// </summary>
            /// <param name="grabber">Grabber</param>
            /// <returns>Grab info or null if not found</returns>
            public RuntimeGrabInfo GetGrabInfo(UxrGrabber grabber)
            {
                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    if (grabInfo.Grabber == grabber)
                    {
                        return grabInfo;
                    }
                }

                return null;
            }

            /// <summary>
            ///     Gets the point grabbed by the given grabber.
            /// </summary>
            /// <param name="grabber">Grabber</param>
            /// <returns>Grabbed point in <see cref="UxrGrabbableObject" /> or -1 if not found</returns>
            public int GetGrabbedPoint(UxrGrabber grabber)
            {
                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    if (grabInfo.Grabber == grabber)
                    {
                        return grabInfo.GrabbedPoint;
                    }
                }

                return -1;
            }

            /// <summary>
            ///     Gets the grabber grabbing the given point.
            /// </summary>
            /// <param name="grabPoint">Grab point in <see cref="UxrGrabbableObject" /></param>
            /// <returns>Grabber grabbing the given point or null if not found</returns>
            public UxrGrabber GetGrabberGrabbingPoint(int grabPoint)
            {
                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    if (grabInfo.GrabbedPoint == grabPoint)
                    {
                        return grabInfo.Grabber;
                    }
                }

                return null;
            }

            /// <summary>
            ///     Checks if the given grabber is being used to manipulate the object.
            /// </summary>
            /// <param name="grabber">Grabber to check</param>
            /// <returns>Whether the given grabber is being used to manipulate the object</returns>
            public bool IsGrabberUsed(UxrGrabber grabber)
            {
                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    if (grabInfo.Grabber == grabber)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            ///     Checks if the given grab point is being grabbed on the object.
            /// </summary>
            /// <param name="grabPoint">Grab point to check</param>
            /// <returns>Whether the given grab point is being grabbed on the object</returns>
            public bool IsPointGrabbed(int grabPoint)
            {
                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    if (grabInfo.GrabbedPoint == grabPoint)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            ///     Registers a grabber swap to indicate that a different hand is now grabbing the point.
            /// </summary>
            /// <param name="oldGrabber">Old grabber that was grabbing</param>
            /// <param name="newGrabber">New grabber that the grab switched to</param>
            public void SwapGrabber(UxrGrabber oldGrabber, UxrGrabber newGrabber)
            {
                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    if (grabInfo.Grabber == oldGrabber)
                    {
                        grabInfo.Grabber = newGrabber;
                        return;
                    }
                }
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
                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    if (grabInfo.Grabber == oldGrabber)
                    {
                        grabInfo.Grabber      = newGrabber;
                        grabInfo.GrabbedPoint = newGrabPoint;
                        return;
                    }
                }
            }

            /// <summary>
            ///     Accumulates the contributions of all grabbers in an object constrained to a single angle rotation.
            /// </summary>
            public void AccumulateSingleRotationAngle(UxrGrabbableObject grabbableObject)
            {
                int singleRotationAxisIndex = grabbableObject.SingleRotationAxisIndex;

                if (singleRotationAxisIndex == -1)
                {
                    return;
                }

                float accumulation      = 0.0f;
                int   contributionCount = 0;

                foreach (RuntimeGrabInfo grabInfo in Grabs)
                {
                    accumulation                  += grabInfo.SingleRotationAngleContribution - grabInfo.LastAccumulatedAngle;
                    grabInfo.LastAccumulatedAngle =  grabInfo.SingleRotationAngleContribution;
                    contributionCount++;
                }

                if (contributionCount > 0)
                {
                    accumulation /= contributionCount;
                    grabbableObject.SingleRotationAngleCumulative = (grabbableObject.SingleRotationAngleCumulative + accumulation).Clamped(grabbableObject.RotationAngleLimitsMin[singleRotationAxisIndex],
                                                                                                                                           grabbableObject.RotationAngleLimitsMax[singleRotationAxisIndex]);
                }
            }

            #endregion

            #region Private Types & Data

            private List<RuntimeGrabInfo>    _grabs = new List<RuntimeGrabInfo>();
            private UxrGrabbableObjectAnchor _sourceAnchor;
            private UxrGrabbableObject       _grabbableObject;
            private Vector3                  _localManipulationRotationPivot;

            #endregion
        }

        #endregion
    }
}