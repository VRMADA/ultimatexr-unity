// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabManager.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabManager
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            // Manipulations are already handled through events, we don't serialize them in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                // We don't want to compare dictionaries, we save the manipulations info always by using null as name to avoid overhead.
                SerializeStateValue(level, options, null, ref _currentManipulations);
            }
        }

        #endregion
    }
}