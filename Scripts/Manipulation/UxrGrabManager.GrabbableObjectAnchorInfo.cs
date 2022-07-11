// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.GrabbableObjectAnchorInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information to handle grab events (<see cref="UxrManipulationEventArgs" />) for
        ///     <see cref="UxrGrabbableObjectAnchor" /> objects:
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="UxrGrabManager.PlacedObjectRangeEntered" />
        ///         </item>
        ///         <item>
        ///             <see cref="UxrGrabManager.PlacedObjectRangeLeft" />
        ///         </item>
        ///         <item>
        ///             <see cref="UxrGrabManager.AnchorRangeEntered" />
        ///         </item>
        ///         <item>
        ///             <see cref="UxrGrabManager.AnchorRangeLeft" />
        ///         </item>
        ///     </list>
        /// </summary>
        private class GrabbableObjectAnchorInfo
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets or sets whether the given <see cref="UxrGrabbableObjectAnchor" /> had a compatible
            ///     <see cref="UxrGrabbableObject" /> within a valid drop distance the last frame.
            /// </summary>
            public bool HadCompatibleObjectNearLastFrame { get; set; }

            /// <summary>
            ///     Gets or sets whether the given <see cref="UxrGrabbableObjectAnchor" /> has currently a compatible
            ///     <see cref="UxrGrabbableObject" /> within a valid drop distance.
            /// </summary>
            public bool HasCompatibleObjectNear { get; set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that currently can grab the <see cref="UxrGrabbableObject" /> placed on
            ///     the given <see cref="UxrGrabbableObjectAnchor" />. Null if there is none.
            /// </summary>
            public UxrGrabber GrabberNear { get; set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that could grab the <see cref="UxrGrabbableObject" /> placed on the
            ///     given <see cref="UxrGrabbableObjectAnchor" /> during last frame. Null if there was none.
            /// </summary>
            public UxrGrabber LastValidGrabberNear { get; set; }

            /// <summary>
            ///     Gets or sets the grab point index of the <see cref="UxrGrabbableObject" /> that is placed on the given
            ///     <see cref="UxrGrabbableObjectAnchor" /> that can currently be grabbed by <see cref="GrabberNear" />. -1 If there is
            ///     none.
            /// </summary>
            public int GrabPointNear { get; set; } = -1;

            /// <summary>
            ///     Gets or sets the grab point index of the <see cref="UxrGrabbableObject" /> that is placed on the given
            ///     <see cref="UxrGrabbableObjectAnchor" /> that could be grabbed by <see cref="GrabberNear" /> during last frame. -1
            ///     if there was none.
            /// </summary>
            public int LastValidGrabPointNear { get; set; } = -1;

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that currently is grabbing an <see cref="UxrGrabbableObject" /> that can
            ///     be placed on the given <see cref="UxrGrabbableObjectAnchor" />. Null if there is none.
            /// </summary>
            public UxrGrabber FullGrabberNear { get; set; }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that currently is grabbing an <see cref="UxrGrabbableObject" /> that
            ///     could be placed on the given <see cref="UxrGrabbableObjectAnchor" /> during last frame. Null if there was none.
            /// </summary>
            public UxrGrabber LastFullGrabberNear { get; set; }

            #endregion
        }

        #endregion
    }
}