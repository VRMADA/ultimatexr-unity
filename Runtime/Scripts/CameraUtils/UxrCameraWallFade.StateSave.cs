// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCameraWallFade.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;

namespace UltimateXR.CameraUtils
{
    public partial class UxrCameraWallFade
    {
        #region Protected Overrides

        /// <inheritdoc />
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            // Changes are already handled each frame, we don't serialize them in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                SerializeStateValue(level, options, nameof(_lastValidPos),            ref _lastValidPos);
                SerializeStateValue(level, options, nameof(_lastValidPosInitialized), ref _lastValidPosInitialized);
            }
        }

        #endregion
    }
}