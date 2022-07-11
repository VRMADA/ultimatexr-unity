// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrButton3DRotate.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateXR.UI.UnityInputModule.Utils
{
    /// <summary>
    ///     Component that rotates a 3D object when a given UI control is being pressed.
    ///     This allows to model buttons that rotate depending on the point of pressure.
    ///     The axis of rotation will be computed automatically, the center will be given by <see cref="UxrButton3D.Target" />
    ///     and the pressure applied will be on the transform of this component.
    /// </summary>
    public class UxrButton3DRotate : UxrButton3D
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Vector3 _buttonLocalUpAxis = Vector3.up;
        [SerializeField] private float   _pressedDegrees    = 2.0f;

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Key down event. The object is rotated according to the pressing point.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        protected override void OnKeyPressed(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (Target)
            {
                Vector3 rotationAxis = Vector3.Cross(_buttonLocalUpAxis, Target.InverseTransformVector(Target.position - transform.position).normalized);
                Target.Rotate(rotationAxis, -_pressedDegrees, Space.Self);
            }
        }

        /// <summary>
        ///     Key up event. The original object rotation is restored.
        /// </summary>
        /// <param name="controlInput">Control that triggered the event</param>
        /// <param name="eventData">Input event data</param>
        protected override void OnKeyReleased(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (Target)
            {
                Target.localRotation = InitialTargetLocalRotation;
            }
        }

        #endregion
    }
}