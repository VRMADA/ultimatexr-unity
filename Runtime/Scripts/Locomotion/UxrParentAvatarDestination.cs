// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrParentAvatarDestination.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Locomotion
{
    /// <summary>
    ///     Component that tells an avatar should be re-parented to the GameObject whenever any locomotion takes the avatar to
    ///     the object or any of its children.
    ///     If a hierarchy contains more than a single <see cref="UxrParentAvatarDestination" />, the closest object or parent
    ///     upwards will be selected.
    ///     Some components, such as <see cref="UxrTeleportLocomotion" />, have a setting that controls the default behaviour (
    ///     <see cref="UxrTeleportLocomotionBase.ParentToDestination" />). The <see cref="UxrParentAvatarDestination" /> can in
    ///     this case be used to override the default behaviour.
    /// </summary>
    public class UxrParentAvatarDestination : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool _parentAvatar;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Whether the avatar should be re-parented to the object containing the component whenever a locomotion takes the
        ///     avatar to the object or any of its children.
        /// </summary>
        public bool ParentAvatar
        {
            get => _parentAvatar;
            set => _parentAvatar = value;
        }

        #endregion
    }
}