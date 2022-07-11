// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabPointShape.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Base class to create more advanced grips (cylindrical, box...).
    ///     An <see cref="UxrGrabbableObject" /> enables grabbing an object. A grabPoint inside the
    ///     <see cref="UxrGrabbableObject" /> defines where and how the object will snap to the hand. Additionally, if there is
    ///     an <see cref="UxrGrabPointShape" /> based component on the same object, it will "expand" the  snapping from a
    ///     single point to a more complex shape like a an axis, a cylinder, a box... This way an object can be picked up from
    ///     many different places just by specifying a snap point and some additional properties.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public abstract class UxrGrabPointShape : UxrComponent<UxrGrabbableObject, UxrGrabPointShape>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] protected UxrGrabPointIndex _grabPointIndex = new UxrGrabPointIndex(0);

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the grab point from the <see cref="UxrGrabbableObject" /> this object extends.
        /// </summary>
        public UxrGrabPointIndex GrabPoint => _grabPointIndex;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the distance from a <see cref="UxrGrabber" /> to a grab point, defined by transform used for snapping and the
        ///     transform used to compute proximity.
        /// </summary>
        /// <param name="grabber">Grabber to compute the distance from</param>
        /// <param name="snapTransform">The <see cref="Transform" /> on the grabbable object that is used to align to the grabber</param>
        /// <param name="distanceTransform">
        ///     The <see cref="Transform" /> on the grabbable object that is used to compute the
        ///     distance to the grabber
        /// </param>
        /// <returns>Distance value</returns>
        public abstract float GetDistanceFromGrabber(UxrGrabber grabber, Transform snapTransform, Transform distanceTransform);

        /// <summary>
        ///     Gets the closest snap position and rotation that should be used when a <see cref="UxrGrabber" /> tries to a grab
        ///     point, defined by transform used for snapping and the transform used to compute proximity.
        /// </summary>
        /// <param name="grabber">Grabber to compute the snapping for</param>
        /// <param name="snapTransform">The <see cref="Transform" /> on the grabbable object that is used to align to the grabber</param>
        /// <param name="distanceTransform">
        ///     The <see cref="Transform" /> on the grabbable object that is used to compute the
        ///     distance to the grabber
        /// </param>
        /// <param name="position">Snap position</param>
        /// <param name="rotation">Snap rotation</param>
        /// <returns>Distance value</returns>
        public abstract void GetClosestSnap(UxrGrabber grabber, Transform snapTransform, Transform distanceTransform, out Vector3 position, out Quaternion rotation);

        #endregion
    }
}