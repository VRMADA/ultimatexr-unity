// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectAnchor.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;

namespace UltimateXR.Manipulation
{
    public partial class UxrGrabbableObjectAnchor
    {
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            // Manipulations are already handled through events, we don't serialize them in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                SerializeStateValue(level, options, nameof(_currentPlacedObject), ref _currentPlacedObject);
            }
        }
    }
}