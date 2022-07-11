// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDelayedDestroy.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Animation.GameObjects
{
    /// <summary>
    ///     Component that allows to destroy the <see cref="GameObject" /> it is attached to after a variable amount of seconds
    /// </summary>
    public class UxrDelayedDestroy : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _seconds;

        #endregion

        #region Unity

        /// <summary>
        ///     Programs the object destruction.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            Destroy(gameObject, _seconds);
        }

        #endregion
    }
}