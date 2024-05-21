

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
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override UxrTransformSpace TransformStateSaveSpace => GetLocalTransformIfParentedOr(UxrTransformSpace.Local);

        /// <inheritdoc />
        protected override bool RequiresTransformSerialization(UxrStateSaveLevel level)
        {
            // Save always
            return level >= UxrStateSaveLevel.ChangesSincePreviousSave;
        }

        /// <inheritdoc />
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

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

            if (!isReading && options.HasFlag(UxrStateSaveOptions.FirstFrame))
            {
                // To avoid changes, because IsSleeping() will return false the first frame. We force it to be sleeping the first frame. 
                isSleeping = true;
            }

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