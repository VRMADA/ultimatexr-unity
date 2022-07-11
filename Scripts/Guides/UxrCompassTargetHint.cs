// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCompassTargetHint.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Guides
{
    /// <summary>
    ///     <para>
    ///         When attached to a GameObject, it will tell the <see cref="UxrCompass" /> where to point to. Otherwise the
    ///         compass will always point to the GameObject's transform.
    ///     </para>
    ///     It can be used to highlight different spots depending on the <see cref="UxrCompassDisplayMode" />.
    /// </summary>
    public class UxrCompassTargetHint : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform _defaultTarget;
        [SerializeField] private Transform _locationTarget;
        [SerializeField] private Transform _grabTarget;
        [SerializeField] private Transform _lookTarget;
        [SerializeField] private Transform _useTarget;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets or sets the target where the <see cref="UxrCompass" /> should point to by default, if the other transforms are
        ///     null or not applicable.
        /// </summary>
        public Transform DefaultTarget
        {
            get => _defaultTarget;
            set => _defaultTarget = value;
        }

        /// <summary>
        ///     Gets or sets the target where the <see cref="UxrCompass" /> should point to when
        ///     <see cref="UxrCompass.DisplayMode" /> is <see cref="UxrCompassDisplayMode.Location" />.
        /// </summary>
        public Transform LocationTarget
        {
            get => _locationTarget;
            set => _locationTarget = value;
        }

        /// <summary>
        ///     Gets or sets the target where the <see cref="UxrCompass" /> should point to when
        ///     <see cref="UxrCompass.DisplayMode" /> is <see cref="UxrCompassDisplayMode.Grab" />.
        /// </summary>
        public Transform GrabTarget
        {
            get => _grabTarget;
            set => _grabTarget = value;
        }

        /// <summary>
        ///     Gets or sets the target where the <see cref="UxrCompass" /> should point to when
        ///     <see cref="UxrCompass.DisplayMode" /> is <see cref="UxrCompassDisplayMode.Look" />.
        /// </summary>
        public Transform LookTarget
        {
            get => _lookTarget;
            set => _lookTarget = value;
        }

        /// <summary>
        ///     Gets or sets the target where the <see cref="UxrCompass" /> should point to when
        ///     <see cref="UxrCompass.DisplayMode" /> is <see cref="UxrCompassDisplayMode.Use" />.
        /// </summary>
        public Transform UseTarget
        {
            get => _useTarget;
            set => _useTarget = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the appropriate transform where the compass should point to.
        /// </summary>
        /// <param name="compass">Display mode to get the transform for</param>
        /// <returns>Transform where the compass should point to</returns>
        public Transform GetTransform(UxrCompass compass)
        {
            switch (compass.DisplayMode)
            {
                case UxrCompassDisplayMode.Location:
                    return LocationTarget != null ? LocationTarget :
                           DefaultTarget != null  ? DefaultTarget : transform;

                case UxrCompassDisplayMode.Grab:
                    return GrabTarget != null    ? GrabTarget :
                           DefaultTarget != null ? DefaultTarget : transform;

                case UxrCompassDisplayMode.Look:
                    return LookTarget != null    ? LookTarget :
                           DefaultTarget != null ? DefaultTarget : transform;

                case UxrCompassDisplayMode.Use:
                    return UseTarget != null     ? LookTarget :
                           DefaultTarget != null ? DefaultTarget : transform;
            }

            return DefaultTarget != null ? DefaultTarget : transform;
        }

        #endregion
    }
}