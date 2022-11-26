// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControlInputDestroyOnPress.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateXR.UI.UnityInputModule.Utils
{
    /// <summary>
    ///     Component that, added to a <see cref="GameObject" /> with a <see cref="UxrControlInput" /> component, will destroy
    ///     the GameObject whenever the control is clicked.
    /// </summary>
    [RequireComponent(typeof(UxrControlInput))]
    public class UxrControlInputDestroyOnPress : UxrComponent
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="UxrControlInput" /> component.
        /// </summary>
        public UxrControlInput ControlInput => GetCachedComponent<UxrControlInput>();

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            ControlInput.Clicked += Control_Clicked;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            ControlInput.Clicked -= Control_Clicked;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the object was clicked.
        /// </summary>
        /// <param name="controlInput">Control that was clicked</param>
        /// <param name="eventData">Event data</param>
        private void Control_Clicked(UxrControlInput controlInput, PointerEventData eventData)
        {
            Destroy(gameObject);
        }

        #endregion
    }
}