// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHapticImpactReceiver.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;

namespace UltimateXR.Haptics.Helpers
{
    /// <summary>
    ///     Base class for components that, added to an object, can receive notifications when a collider on the same object or
    ///     any of its children gets hit with a <see cref="UxrHapticOnImpact" /> component.
    /// </summary>
    public abstract class UxrHapticImpactReceiver : UxrComponent
    {
        #region Event Trigger Methods

        /// <summary>
        ///     Overridable method that gets called whenever an object with a <see cref="UxrHapticOnImpact" /> hits the collider
        ///     with the <see cref="UxrHapticImpactReceiver" /> or any of its children.
        /// </summary>
        /// <param name="sender">Source component</param>
        /// <param name="eventArgs">Event parameters</param>
        public virtual void OnHit(UxrHapticOnImpact sender, UxrHapticImpactEventArgs eventArgs)
        {
        }

        #endregion
    }
}