// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLocomotion.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Base class for locomotion components. Locomotion components enable different ways for an <see cref="UxrAvatar" />
    ///     to move around the scenario.
    /// </summary>
    public abstract class UxrLocomotion : UxrAvatarComponent<UxrLocomotion>, IUxrLocomotionUpdater
    {
        #region Public Types & Data

        /// <summary>
        ///     <para>
        ///         Gets whether the locomotion updates the avatar each frame. An example of smooth locomotion is
        ///         <see cref="UxrSmoothLocomotion" /> where the user moves the avatar in an identical way to a FPS video-game.
        ///         An example of non-smooth locomotion is <see cref="UxrTeleportLocomotion" /> where the avatar is moved only on
        ///         specific occasions.
        ///     </para>
        ///     <para>
        ///         The smooth locomotion concept should not be confused with the ability to move the head around each frame.
        ///         Smooth locomotion refers to the avatar position, which is determined by the avatar's root GameObject.
        ///         It should also not be confused with the ability to perform teleportation in a smooth way. Even if some
        ///         teleportation locomotion methods can teleport using smooth transitions, it should not be considered as smooth
        ///         locomotion.
        ///     </para>
        ///     <para>
        ///         The smooth locomotion property can be used to determine whether certain operations, such as LOD switching,
        ///         should be processed each frame or only when the avatar position changed.
        ///     </para>
        /// </summary>
        public abstract bool IsSmoothLocomotion { get; }

        #endregion

        #region Explicit IUxrLocomotionUpdater

        /// <inheritdoc />
        void IUxrLocomotionUpdater.UpdateLocomotion()
        {
            UpdateLocomotion();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Logs if there is a missing <see cref="Avatar" /> component upwards in the hierarchy.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Avatar == null)
            {
                UxrManager.LogMissingAvatarInHierarchyError(this);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Updates the locomotion and the avatar's position/orientation the component belongs to.
        /// </summary>
        protected abstract void UpdateLocomotion();

        #endregion
    }
}