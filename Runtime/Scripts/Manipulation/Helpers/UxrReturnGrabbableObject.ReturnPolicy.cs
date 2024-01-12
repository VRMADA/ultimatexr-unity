// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrReturnGrabbableObject.ReturnPolicy.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation.Helpers
{
    public partial class UxrReturnGrabbableObject
    {
        #region Public Types & Data

        /// <summary>
        ///     Enumerates the different policies when returning an object to a previous anchor.
        /// </summary>
        public enum ReturnPolicy
        {
            /// <summary>
            ///     Return to last anchor where the object was placed.
            /// </summary>
            LastAnchor = 0,

            /// <summary>
            ///     Return to the original anchor where the object was placed.
            /// </summary>
            OriginalAnchor,
        }

        #endregion
    }
}