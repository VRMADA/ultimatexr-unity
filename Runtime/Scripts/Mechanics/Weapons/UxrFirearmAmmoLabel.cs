// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFirearmAmmoLabel.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Component that draws the ammo left in a firearm magazine.
    /// </summary>
    [RequireComponent(typeof(UxrFirearmWeapon))]
    public class UxrFirearmAmmoLabel : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Text _textTarget;
        [SerializeField] private int  _triggerIndex;
        [SerializeField] private bool _showCapacity = true;
        [SerializeField] private int  _digits       = 2;

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _firearm = GetComponent<UxrFirearmWeapon>();
        }

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called after all avatars were updated. Updates the ammo information.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            if (!_textTarget || !_firearm)
            {
                return;
            }

            string ammoRemaining = _firearm.HasMagAttached(_triggerIndex) ? _firearm.GetAmmoLeft(_triggerIndex).ToString() : string.Empty;
            string ammoCapacity  = _firearm.HasMagAttached(_triggerIndex) ? _firearm.GetAmmoCapacity(_triggerIndex).ToString() : string.Empty;

            if (_firearm.HasMagAttached(_triggerIndex))
            {
                ammoRemaining = ammoRemaining.PadLeft(_digits, '0');
                ammoCapacity  = ammoCapacity.PadLeft(_digits, '0');
            }
            else
            {
                ammoRemaining = ammoRemaining.PadLeft(_digits, '-');
                ammoCapacity  = ammoCapacity.PadLeft(_digits, '-');
            }

            _textTarget.text = _showCapacity ? $"{ammoRemaining}/{ammoCapacity}" : ammoRemaining;
        }

        #endregion

        #region Private Types & Data

        private UxrFirearmWeapon _firearm;

        #endregion
    }
}