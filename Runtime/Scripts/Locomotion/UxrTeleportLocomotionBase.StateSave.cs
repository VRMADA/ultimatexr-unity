// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotionBase.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;

namespace UltimateXR.Locomotion
{
    public abstract partial class UxrTeleportLocomotionBase
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeStateInternal(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeStateInternal(isReading, stateSerializationVersion, level, options);

            // Locomotion is are already handled through events, we don't serialize parameters in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                int layerMaskValue = _layerMaskRaycast.value;

                SerializeStateValue(level, options, nameof(layerMaskValue), ref layerMaskValue);

                if (isReading)
                {
                    _layerMaskRaycast.value = layerMaskValue;
                }
            }
        }

        #endregion
    }
}