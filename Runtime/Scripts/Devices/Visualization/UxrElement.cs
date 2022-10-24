// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrElement.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    /// <summary>
    ///     Describes the properties of a VR controller input element.
    /// </summary>
    [Serializable]
    public class UxrElement
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrElementType        _elementType = UxrElementType.NotSet;
        [SerializeField] private UxrHandSide           _hand;
        [SerializeField] private UxrControllerElements _element;
        [SerializeField] private GameObject            _gameObject;
        [SerializeField] private UxrFingerType         _finger;
        [SerializeField] private GameObject            _fingerContactPoint;
        [SerializeField] private Vector3               _buttonPressedOffset;
        [SerializeField] private Vector3               _input1DPressedOffsetAngle;
        [SerializeField] private Vector3               _input1DPressedOffset;
        [SerializeField] private Vector3               _input2DFirstAxisOffsetAngle;
        [SerializeField] private Vector3               _input2DSecondAxisOffsetAngle;
        [SerializeField] private Vector3               _input2DFirstAxisOffset;
        [SerializeField] private Vector3               _input2DSecondAxisOffset;
        [SerializeField] private Vector3               _dpadFirstAxisOffsetAngle;
        [SerializeField] private Vector3               _dpadSecondAxisOffsetAngle;
        [SerializeField] private Vector3               _dpadFirstAxisOffset;
        [SerializeField] private Vector3               _dpadSecondAxisOffset;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the input element type.
        /// </summary>
        public UxrElementType ElementType => _elementType;

        /// <summary>
        ///     Gets which controller element(s) the input element describes.
        /// </summary>
        public UxrControllerElements Element => _element;

        /// <summary>
        ///     Gets the object that represents the input element.
        /// </summary>
        public GameObject ElementObject => _gameObject;

        /// <summary>
        ///     Gets which finger interacts with the input element.
        /// </summary>
        public UxrFingerType Finger => _finger;

        /// <summary>
        ///     Gets the finger contact point if there is any. If null it will try to contact <see cref="ElementObject" />'s
        ///     transform.
        /// </summary>
        public GameObject FingerContactPoint => _fingerContactPoint;

        /// <summary>
        ///     Gets the pressed offset for a <see cref="UxrElementType.Button" /> input element.
        /// </summary>
        public Vector3 ButtonPressedOffset => _buttonPressedOffset;

        /// <summary>
        ///     Gets the pressed offset euler angles for a <see cref="UxrElementType.Input1DRotate" /> input element.
        /// </summary>
        public Vector3 Input1DPressedOffsetAngle => _input1DPressedOffsetAngle;

        /// <summary>
        ///     Gets the pressed offset for a <see cref="UxrElementType.Input1DPush" /> input element.
        /// </summary>
        public Vector3 Input1DPressedOffset => _input1DPressedOffset;

        /// <summary>
        ///     Gets the maximum positive angle range for the first axis in a <see cref="UxrElementType.Input2DJoystick" />
        ///     input element. The other side will be the negated angle.
        /// </summary>
        public Vector3 Input2DFirstAxisOffsetAngle => _input2DFirstAxisOffsetAngle;

        /// <summary>
        ///     Gets the maximum positive angle range for the second axis in a <see cref="UxrElementType.Input2DJoystick" />
        ///     input element. The other side will be the negated angle.
        /// </summary>
        public Vector3 Input2DSecondAxisOffsetAngle => _input2DSecondAxisOffsetAngle;

        /// <summary>
        ///     Gets the maximum positive offset of the first axis in a <see cref="UxrElementType.Input2DTouch" /> input
        ///     element. The other side will be the negated offset.
        /// </summary>
        public Vector3 Input2DFirstAxisOffset => _input2DFirstAxisOffset;

        /// <summary>
        ///     Gets the maximum positive offset of the second axis in a <see cref="UxrElementType.Input2DTouch" /> input
        ///     element. The other side will be the negated offset.
        /// </summary>
        public Vector3 Input2DSecondAxisOffset => _input2DSecondAxisOffset;

        /// <summary>
        ///     Gets the maximum positive angle range for the first axis in a <see cref="UxrElementType.DPad" /> input element. The
        ///     other side will be the negated angle.
        /// </summary>
        public Vector3 DpadFirstAxisOffsetAngle => _dpadFirstAxisOffsetAngle;

        /// <summary>
        ///     Gets the maximum positive angle range for the second axis in a <see cref="UxrElementType.DPad" /> input element.
        ///     The other side will be the negated angle.
        /// </summary>
        public Vector3 DpadSecondAxisOffsetAngle => _dpadSecondAxisOffsetAngle;

        /// <summary>
        ///     Gets the maximum positive offset for the first axis in a <see cref="UxrElementType.DPad" /> input element. The
        ///     other side will be the negated offset.
        /// </summary>
        public Vector3 DpadFirstAxisOffset => _dpadFirstAxisOffset;

        /// <summary>
        ///     Gets the maximum positive offset for the second axis in a <see cref="UxrElementType.DPad" /> input element. The
        ///     other side will be the negated offset.
        /// </summary>
        public Vector3 DpadSecondAxisOffset => _dpadSecondAxisOffset;

        /// <summary>
        ///     Gets the hand that is used to interact with the input.
        /// </summary>
        public UxrHandSide HandSide
        {
            get => _hand;
            internal set => _hand = value;
        }

        #endregion

        #region Internal Types & Data

        /// <summary>
        ///     Gets or sets the transform's initial local position.
        /// </summary>
        internal Vector3 InitialLocalPos { get; set; }

        /// <summary>
        ///     Gets or sets the transform's initial local rotation.
        /// </summary>
        internal Quaternion InitialLocalRot { get; set; }

        /// <summary>
        ///     Gets or sets the initial local position of the finger point of contact.
        /// </summary>
        internal Vector3 FingerContactInitialLocalPos { get; set; }

        /// <summary>
        ///     Gets or sets the local right (X) offset axis.
        /// </summary>
        internal Vector3 LocalOffsetX { get; set; }

        /// <summary>
        ///     Gets or sets the local up (Y) offset axis.
        /// </summary>
        internal Vector3 LocalOffsetY { get; set; }

        /// <summary>
        ///     Gets or sets the local forward (Z) offset axis.
        /// </summary>
        internal Vector3 LocalOffsetZ { get; set; }

        /// <summary>
        ///     Gets or sets the local right (X) offset of the finger point of contact.
        /// </summary>
        internal Vector3 LocalFingerPosOffsetX { get; set; }

        /// <summary>
        ///     Gets or sets the local up (Y) offset of the finger point of contact.
        /// </summary>
        internal Vector3 LocalFingerPosOffsetY { get; set; }

        /// <summary>
        ///     Gets or sets the local forward (Z) offset of the finger point of contact.
        /// </summary>
        internal Vector3 LocalFingerPosOffsetZ { get; set; }

        #endregion
    }
}