// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWeapon.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.StateSave;

namespace UltimateXR.Mechanics.Weapons
{
    public abstract partial class UxrWeapon
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeStateInternal(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeStateInternal(isReading, stateSerializationVersion, level, options);

            // Logic is already handled through events, we don't serialize these parameters in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                SerializeStateValue(level, options, nameof(_owner), ref _owner);
            }
        }

        #endregion
    }
}