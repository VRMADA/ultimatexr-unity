// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagAmmoIndicator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.Mechanics.Weapons;
using UnityEngine;

namespace UltimateXR.Examples.FullScene.Weapons
{
    [RequireComponent(typeof(UxrFirearmMag))]
    public class MagAmmoIndicator : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool     _setValueOnStart = true;
        [SerializeField] private Renderer _renderer;

        #endregion

        #region Unity

        protected override void Awake()
        {
            base.Awake();
            _mag = GetComponent<UxrFirearmMag>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _mag.RoundsChanged += OnRoundsChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _mag.RoundsChanged -= OnRoundsChanged;
        }

        protected override void Start()
        {
            base.OnEnable();

            if (_setValueOnStart)
            {
                OnRoundsChanged();
            }
        }

        #endregion

        #region Event Trigger Methods

        private void OnRoundsChanged()
        {
            _renderer.material.SetFloat(FillVariableName, (float)_mag.Rounds / _mag.Capacity);
        }

        #endregion

        #region Private Types & Data

        private const string FillVariableName = "_Fill";

        private UxrFirearmMag _mag;

        #endregion
    }
}