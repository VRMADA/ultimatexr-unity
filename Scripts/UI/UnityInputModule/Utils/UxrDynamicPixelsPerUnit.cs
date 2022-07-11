// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDynamicPixelsPerUnit.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule.Utils
{
    /// <summary>
    ///     Component that adjusts the dynamic pixels per unit value in a <see cref="Canvas" /> component depending on
    ///     the distance to the avatar. It helps removing filtering artifacts when using composition layers is not
    ///     possible.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class UxrDynamicPixelsPerUnit : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _updateSeconds     = 0.3f;
        [SerializeField] private float _rangeNear         = 0.3f;
        [SerializeField] private float _rangeFar          = 4.0f;
        [SerializeField] private float _pixelsPerUnitNear = 1.0f;
        [SerializeField] private float _pixelsPerUnitFar  = 0.1f;

        #endregion

        #region Unity

        /// <summary>
        ///     Caches components.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _canvasScaler = GetComponent<CanvasScaler>();
        }

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            UxrManager.AvatarMoved += UxrManager_AvatarMoved;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            UxrManager.AvatarMoved -= UxrManager_AvatarMoved;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when an avatar moved: Adjusts the dynamic pixels per unit.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrManager_AvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            if (Time.time - _timeLastUpdate > _updateSeconds)
            {
                _timeLastUpdate = Time.time;
                float distance = Vector3.Distance(e.Avatar.CameraPosition, _canvasScaler.transform.position);
                _canvasScaler.dynamicPixelsPerUnit = Mathf.Lerp(_pixelsPerUnitNear, _pixelsPerUnitFar, Mathf.Clamp01((distance - _rangeNear) / (_rangeFar - _rangeNear)));
            }
        }

        #endregion

        #region Private Types & Data

        private float        _timeLastUpdate = -1.0f;
        private CanvasScaler _canvasScaler;

        #endregion
    }
}