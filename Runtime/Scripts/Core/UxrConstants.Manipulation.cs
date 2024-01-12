// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrConstants.Manipulation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    public static partial class UxrConstants
    {
        #region Public Types & Data

        /// <summary>
        ///     Manipulation smooth transitions duration: the position and rotation of objects being grabbed/released/placed or
        ///     hands snapping to/from constrained objects.
        /// </summary>
        public const float SmoothManipulationTransitionSeconds = 0.1f;

        /// <summary>
        ///     How much a difference in angle will offset the distance at which we compute a grabbable object from a grabber.
        ///     Objects that are not so well aligned with the grabber will be considered slightly farther away by(angle*
        ///     DistanceOffsetByAngle) units, this means in the [0, 0.05] range.
        ///     This will favor grabbing objects that are better aligned when there are two or more at similar distances.
        /// </summary>
        public const float DistanceOffsetByAngle = 1.0f / 1800.0f;

        /// <summary>
        ///     Minimum distance allowed between two grabbable points that can be grabbed at the same time. Avoids hand
        ///     overlapping.
        /// </summary>
        public const float MinHandGrabInterDistance = Hand.HandWidth * 0.5f + 0.01f;

        /// <summary>
        ///     Used by the editor to identify the default avatar when no avatars have been registered for grips.
        /// </summary>
        public const string DefaultAvatarName = "[Default]";

        /// <summary>
        ///     Used by the editor to prefix the left grab pose mesh.
        /// </summary>
        public const string LeftGrabPoseMeshSuffix = " left";

        /// <summary>
        ///     Used by the editor to prefix the right grab pose mesh.
        /// </summary>
        public const string RightGrabPoseMeshSuffix = " right";

        #endregion
    }
}