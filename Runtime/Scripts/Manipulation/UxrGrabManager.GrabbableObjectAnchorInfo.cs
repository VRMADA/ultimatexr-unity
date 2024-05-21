// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.GrabbableObjectAnchorInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Serialization;

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
        /// <remarks>
        ///     Implements <see cref="IUxrSerializable" /> to help <see cref="UxrGrabManager" />'s implementation of the
        ///     <see cref="IUxrStateSave" /> interface (<see cref="UxrGrabManager.SerializeState" />).
        ///     For now this information is not serialized, because most of it can be inferred at runtime on the client side, but
        ///     it might get used in the future.
        /// </remarks>
        private class GrabbableObjectAnchorInfo : IUxrSerializable
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets or sets whether the given <see cref="UxrGrabbableObjectAnchor" /> had a compatible
            ///     <see cref="UxrGrabbableObject" /> within a valid drop distance the last frame.
            /// </summary>
            public bool HadCompatibleObjectNearLastFrame
            {
                get => _hadCompatibleObjectNearLastFrame;
                set => _hadCompatibleObjectNearLastFrame = value;
            }

            /// <summary>
            ///     Gets or sets whether the given <see cref="UxrGrabbableObjectAnchor" /> has currently a compatible
            ///     <see cref="UxrGrabbableObject" /> within a valid drop distance.
            /// </summary>
            public bool HasCompatibleObjectNear
            {
                get => _hasCompatibleObjectNear;
                set => _hasCompatibleObjectNear = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that currently can grab the <see cref="UxrGrabbableObject" /> placed on
            ///     the given <see cref="UxrGrabbableObjectAnchor" />. Null if there is none.
            /// </summary>
            public UxrGrabber GrabberNear
            {
                get => _grabberNear;
                set => _grabberNear = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that could grab the <see cref="UxrGrabbableObject" /> placed on the
            ///     given <see cref="UxrGrabbableObjectAnchor" /> during last frame. Null if there was none.
            /// </summary>
            public UxrGrabber LastValidGrabberNear
            {
                get => _lastValidGrabberNear;
                set => _lastValidGrabberNear = value;
            }

            /// <summary>
            ///     Gets or sets the grab point index of the <see cref="UxrGrabbableObject" /> that is placed on the given
            ///     <see cref="UxrGrabbableObjectAnchor" /> that can currently be grabbed by <see cref="GrabberNear" />. -1 If there is
            ///     none.
            /// </summary>
            public int GrabPointNear
            {
                get => _grabPointNear;
                set => _grabPointNear = value;
            }

            /// <summary>
            ///     Gets or sets the grab point index of the <see cref="UxrGrabbableObject" /> that is placed on the given
            ///     <see cref="UxrGrabbableObjectAnchor" /> that could be grabbed by <see cref="GrabberNear" /> during last frame. -1
            ///     if there was none.
            /// </summary>
            public int LastValidGrabPointNear
            {
                get => _lastValidGrabPointNear;
                set => _lastValidGrabPointNear = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that currently is grabbing an <see cref="UxrGrabbableObject" /> that can
            ///     be placed on the given <see cref="UxrGrabbableObjectAnchor" />. Null if there is none.
            /// </summary>
            public UxrGrabber FullGrabberNear
            {
                get => _fullGrabberNear;
                set => _fullGrabberNear = value;
            }

            /// <summary>
            ///     Gets or sets the <see cref="UxrGrabber" /> that currently is grabbing an <see cref="UxrGrabbableObject" /> that
            ///     could be placed on the given <see cref="UxrGrabbableObjectAnchor" /> during last frame. Null if there was none.
            /// </summary>
            public UxrGrabber LastFullGrabberNear
            {
                get => _lastFullGrabberNear;
                set => _lastFullGrabberNear = value;
            }

            #endregion

            #region Constructors & Finalizer

            /// <summary>
            ///     Default constructor required for serialization.
            /// </summary>
            public GrabbableObjectAnchorInfo()
            {
            }

            #endregion

            #region Implicit IUxrSerializable

            /// <inheritdoc />
            public int SerializationVersion => 0;

            /// <inheritdoc />
            public void Serialize(IUxrSerializer serializer, int serializationVersion)
            {
                serializer.Serialize(ref _hadCompatibleObjectNearLastFrame);
                serializer.Serialize(ref _hasCompatibleObjectNear);
                serializer.SerializeUniqueComponent(ref _grabberNear);
                serializer.SerializeUniqueComponent(ref _lastValidGrabberNear);
                serializer.Serialize(ref _grabPointNear);
                serializer.Serialize(ref _lastValidGrabPointNear);
                serializer.SerializeUniqueComponent(ref _fullGrabberNear);
                serializer.SerializeUniqueComponent(ref _lastFullGrabberNear);
            }

            #endregion

            #region Private Types & Data

            private bool       _hadCompatibleObjectNearLastFrame;
            private bool       _hasCompatibleObjectNear;
            private UxrGrabber _grabberNear;
            private UxrGrabber _lastValidGrabberNear;
            private int        _grabPointNear          = -1;
            private int        _lastValidGrabPointNear = -1;
            private UxrGrabber _fullGrabberNear;
            private UxrGrabber _lastFullGrabberNear;

            #endregion
        }

        #endregion
    }
}