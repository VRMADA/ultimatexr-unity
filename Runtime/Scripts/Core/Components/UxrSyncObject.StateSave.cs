// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSyncObject.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core.Components
{
    public partial class UxrSyncObject
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override bool SaveStateWhenDisabled => _syncWhileDisabled;

        /// <inheritdoc />
        protected override bool SerializeActiveAndEnabledState => _syncActiveAndEnabled;

        #endregion
    }
}