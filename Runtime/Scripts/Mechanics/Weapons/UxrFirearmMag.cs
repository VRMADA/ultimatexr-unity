// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFirearmMag.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Components;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     A magazine that contains ammo for a <see cref="UxrFirearmWeapon" />. Magazines can be attached to firearms using
    ///     <see cref="UxrGrabbableObject" /> functionality.
    /// </summary>
    public class UxrFirearmMag : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private int _rounds;
        [SerializeField] private int _capacity;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Total ammo capacity.
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        ///     Remaining ammo.
        /// </summary>
        public int Rounds
        {
            get => Mathf.Clamp(_rounds, 0, _capacity);
            set
            {
                _rounds = Mathf.Clamp(value, 0, _capacity);
                RoundsChanged?.Invoke();
            }
        }

        /// <summary>
        ///     Event called whenever the number of rounds changed.
        /// </summary>
        public Action RoundsChanged;

        #endregion
    }
}