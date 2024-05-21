// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotion.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;

namespace UltimateXR.Locomotion
{
    public partial class UxrTeleportLocomotion
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            // Teleportation logic is already handled through events, we don't serialize parameters in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                SerializeStateValue(level, options, nameof(_previousFrameHadArc), ref _previousFrameHadArc);
                SerializeStateValue(level, options, nameof(_arcCancelled),        ref _arcCancelled);
                SerializeStateValue(level, options, nameof(_arcCancelledByAngle), ref _arcCancelledByAngle);

                SerializeStateValue(level, options, nameof(_lastSyncIsArcEnabled),        ref _lastSyncIsArcEnabled);
                SerializeStateValue(level, options, nameof(_lastSyncIsTargetEnabled),     ref _lastSyncIsTargetEnabled);
                SerializeStateValue(level, options, nameof(_lastSyncIsValidTeleport),     ref _lastSyncIsValidTeleport);
                SerializeStateValue(level, options, nameof(_lastSyncTargetArrowLocalRot), ref _lastSyncTargetArrowLocalRot);

                if (isReading)
                {
                    EnableArc(_lastSyncIsArcEnabled, _lastSyncIsValidTeleport);
                }
            }
        }

        #endregion
    }
}