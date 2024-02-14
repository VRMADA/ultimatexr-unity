// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObject.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.StateSave;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabbableObject
    {
        #region Public Overrides UxrComponent

        /// <inheritdoc />
        public override UxrTransformSpace TransformStateSaveSpace => GetGrabbableParent(this) != null ? UxrTransformSpace.Local : GetLocalTransformIfParentedOr(UxrTransformSpace.World);

        /// <summary>
        ///     <para>
        ///         Gets whether the transform data needs to be serialized when loading/saving the grabbable state. This will
        ///         automatically save the Transform using the space defined by <see cref="TransformStateSaveSpace" />.
        ///     </para>
        ///     <para>
        ///         Grabbable transforms are serialized, mainly to store the trajectories when they are driven by physics when
        ///         thrown or dropped. Serialization can change over time, and will be disabled in these cases:
        ///         <list type="bullet">
        ///             <item>
        ///                 Objects that are being grabbed don't need to serialize transform data because they are already driven
        ///                 by the avatar grabber.
        ///             </item>
        ///             <item>
        ///                 Objects that have a parent grabbable don't need to serialize transform data because they are already
        ///                 driven by the parent when not being grabbed.
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <param name="level">The amount of data to serialize</param>
        /// <returns>Whether to serialize the transform</returns>
        /// <remarks>
        ///     Even if transform data needs to be serialized, the serialization system will not save redundant information. This
        ///     means that grabbable objects that don't move will not generate new data.
        /// </remarks>
        public override bool RequiresTransformSerialization(UxrStateSaveLevel level)
        {
            // If level >= UxrStateSaveLevel.ChangesSinceBeginning we want to save a snapshot of the transform
            return level >= UxrStateSaveLevel.ChangesSinceBeginning || !UxrGrabManager.Instance.IsBeingGrabbed(this);
        }

        #endregion

        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeStateInternal(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeStateInternal(isReading, stateSerializationVersion, level, options);

            if (level <= UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                // All the variables below are not needed in incremental snapshots.
                // In a replay system, these variables will be updated by events.
                return;
            }

            // Backing fields for public properties

            SerializeStateValue(level, options, nameof(_currentAnchor),             ref _currentAnchor);
            SerializeStateValue(level, options, nameof(_isPlaceable),               ref _isPlaceable);
            SerializeStateValue(level, options, nameof(_isLockedInPlace),           ref _isLockedInPlace);
            SerializeStateValue(level, options, nameof(_priority),                  ref _priority);
            SerializeStateValue(level, options, nameof(_useParenting),              ref _useParenting);
            SerializeStateValue(level, options, nameof(_tag),                       ref _tag);
            SerializeStateValue(level, options, nameof(_translationConstraintMode), ref _translationConstraintMode);
            SerializeStateValue(level, options, nameof(_translationLimitsMin),      ref _translationLimitsMin);
            SerializeStateValue(level, options, nameof(_translationLimitsMax),      ref _translationLimitsMax);
            SerializeStateValue(level, options, nameof(_rotationConstraintMode),    ref _rotationConstraintMode);
            SerializeStateValue(level, options, nameof(_rotationAngleLimitsMin),    ref _rotationAngleLimitsMin);
            SerializeStateValue(level, options, nameof(_rotationAngleLimitsMax),    ref _rotationAngleLimitsMax);
            SerializeStateValue(level, options, nameof(_translationResistance),     ref _translationResistance);
            SerializeStateValue(level, options, nameof(_rotationResistance),        ref _rotationResistance);
            SerializeStateValue(level, options, nameof(_dropSnapMode),              ref _dropSnapMode);

            // Backing fields for internal properties

            SerializeStateValue(level, options, nameof(_singleRotationAngleCumulative), ref _singleRotationAngleCumulative);
            SerializeStateValue(level, options, nameof(_placementOptions),              ref _placementOptions);

            // Backing fields for IUxrGrabbable interface

            SerializeStateValue(level, options, nameof(_isGrabbable), ref _isGrabbable);

            bool isKinematic = _rigidBodySource && _rigidBodySource.isKinematic;
            bool isSleeping  = _rigidBodySource && _rigidBodySource.IsSleeping();

            SerializeStateValue(level, options, nameof(isKinematic), ref isKinematic);
            SerializeStateValue(level, options, nameof(isSleeping),  ref isSleeping);

            if (isReading && _rigidBodySource)
            {
                _rigidBodySource.isKinematic = isKinematic;

                if (isSleeping)
                {
                    _rigidBodySource.Sleep();
                }
            }

            if (isReading)
            {
                LocalPositionBeforeUpdate = transform.localPosition;
                LocalRotationBeforeUpdate = transform.localRotation;
            }

            // Private vars

            SerializeStateValue(level, options, nameof(_grabPointEnabledStates), ref _grabPointEnabledStates);
        }

        #endregion
    }
}