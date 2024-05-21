// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAutoSlideInObject.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;

namespace UltimateXR.Manipulation.Helpers
{
    public partial class UxrAutoSlideInObject
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            // Manipulations are already handled through events, we don't serialize them in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                SerializeStateValue(level, options, nameof(_insertAxis),                 ref _insertAxis);
                SerializeStateValue(level, options, nameof(_insertOffset),               ref _insertOffset);
                SerializeStateValue(level, options, nameof(_insertOffsetSign),           ref _insertOffsetSign);
                SerializeStateValue(level, options, nameof(_objectLocalSize),            ref _objectLocalSize);
                SerializeStateValue(level, options, nameof(_slideInTimer),               ref _slideInTimer);
                SerializeStateValue(level, options, nameof(_placedAfterSlidingIn),       ref _placedAfterSlidingIn);
                SerializeStateValue(level, options, nameof(_manipulationHapticFeedback), ref _manipulationHapticFeedback);
                SerializeStateValue(level, options, nameof(_minHapticAmplitude),         ref _minHapticAmplitude);
                SerializeStateValue(level, options, nameof(_maxHapticAmplitude),         ref _maxHapticAmplitude);
            }
        }

        #endregion
    }
}