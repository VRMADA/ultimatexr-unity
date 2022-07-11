// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrButton3DPress.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateXR.UI.UnityInputModule.Utils
{
    /// <summary>
    ///     Component that moves a 3D object when a given UI control is being pressed
    /// </summary>
    public class UxrButton3DPress : UxrButton3D
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Vector3 _pressedLocalOffset;

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Key down event. The object is moved to the pressed local coordinates.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        protected override void OnKeyPressed(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (Target)
            {
                Vector3 pressedLocalOffset = _pressedLocalOffset;

                if (Target.parent != null)
                {
                    pressedLocalOffset = Target.parent.InverseTransformDirection(Target.TransformDirection(pressedLocalOffset));
                }

                Target.localPosition = InitialTargetLocalPosition + pressedLocalOffset;
            }
        }

        /// <summary>
        ///     Key up event. The original object position is restored.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        protected override void OnKeyReleased(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (Target)
            {
                Target.localPosition = InitialTargetLocalPosition;
            }
        }

        #endregion
    }
}