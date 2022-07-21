// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAnimationType.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     Enumerates the different well-known hand animation types (poses).
    /// </summary>
    public enum UxrAnimationType
    {
        /// <summary>
        ///     Non-relevant left hand pose
        /// </summary>
        LeftHandOther,

        /// <summary>
        ///     Non-relevant right hand pose
        /// </summary>
        RightHandOther,

        /// <summary>
        ///     Left hand pose used for grabbing.
        /// </summary>
        LeftHandGrab,

        /// <summary>
        ///     Right hand pose used for grabbing.
        /// </summary>
        RightHandGrab,

        /// <summary>
        ///     Left hand pose used for pointing with the finger.
        /// </summary>
        LeftFingerPoint,

        /// <summary>
        ///     Right hand pose used for pointing with the finger.
        /// </summary>
        RightFingerPoint
    }
}