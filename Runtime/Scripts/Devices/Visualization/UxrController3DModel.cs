// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrController3DModel.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Devices.Visualization
{
    /// <summary>
    ///     Represents the 3D model of a VR controller. It allows to graphically render the current position/orientation and
    ///     input state of the device.
    /// </summary>
    public class UxrController3DModel : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool              _needsBothHands;
        [SerializeField] private UxrHandSide       _handSide;
        [SerializeField] private UxrControllerHand _controllerHand;
        [SerializeField] private UxrControllerHand _controllerHandLeft;
        [SerializeField] private UxrControllerHand _controllerHandRight;
        [SerializeField] private Transform         _forward;
        [SerializeField] private List<UxrElement>  _controllerElements = new List<UxrElement>();

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the controller requires two hands to hold it.
        /// </summary>
        public bool NeedsBothHands => _needsBothHands;

        /// <summary>
        ///     Gets the hand required to hold the controller, if <see cref="NeedsBothHands" /> is false.
        /// </summary>
        public UxrHandSide HandSide => _handSide;

        /// <summary>
        ///     Gets the forward transform as it is currently in the scene. It can be different than the actual forward tracking
        ///     when we use grab mechanics because the hand transform can be modified by the grab manager and the controller
        ///     usually hangs from the hand hierarchy.
        ///     If you need to know the forward controller transform using the information of tracking sensors without any
        ///     intervention by external elements like the grabbing mechanics use <see cref="ForwardTrackingRotation" />.
        /// </summary>
        public Transform Forward => _forward;

        /// <summary>
        ///     Gets the rotation that represents the controller's forward orientation. We use this mainly to be able to align
        ///     certain mechanics no matter the controller that is currently active. A gun in a game needs to be aligned to the
        ///     controller, teleport mechanics, etc.
        /// </summary>
        public Quaternion ForwardTrackingRotation
        {
            get
            {
                IUxrControllerTracking controllerTracking = _avatar != null ? _avatar.FirstControllerTracking : null;

                if (controllerTracking == null)
                {
                    return _forward.rotation;
                }

                Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * _forward.transform.rotation;
                Quaternion sensorRotation   = _handSide == UxrHandSide.Left ? controllerTracking.SensorLeftRot : controllerTracking.SensorRightRot;

                return sensorRotation * relativeRotation;
            }
        }

        /// <summary>
        ///     Gets or sets the hand that is interacting with the controller, when the controller is used with only one hand.
        /// </summary>
        public UxrControllerHand ControllerHand
        {
            get => _controllerHand;
            set => _controllerHand = value;
        }

        /// <summary>
        ///     Gets or sets the left hand that is interacting with the controller, when the controller can be held using both
        ///     hands.
        /// </summary>
        public UxrControllerHand ControllerHandLeft
        {
            get => _controllerHandLeft;
            set => _controllerHandLeft = value;
        }

        /// <summary>
        ///     Gets or sets the right hand that is interacting with the controller, when the controller can be held using both
        ///     hands.
        /// </summary>
        public UxrControllerHand ControllerHandRight
        {
            get => _controllerHandRight;
            set => _controllerHandRight = value;
        }

        /// <summary>
        ///     Gets or sets whether the controller is visible.
        /// </summary>
        public bool IsControllerVisible
        {
            get => _isControllerVisible;
            set
            {
                _isControllerVisible = value;
                gameObject.SetActive(_isControllerVisible);
            }
        }

        /// <summary>
        ///     Gets or sets whether the hand, if present, is visible. In setups where both hands are used, it targets visibility
        ///     of both hands.
        /// </summary>
        public bool IsHandVisible
        {
            get => _isHandVisible;
            set
            {
                _isHandVisible = value;

                if (_controllerHand != null)
                {
                    _controllerHand.gameObject.SetActive(_isHandVisible);
                }

                if (_controllerHandLeft != null)
                {
                    _controllerHandLeft.gameObject.SetActive(_isHandVisible);
                }

                if (_controllerHandRight != null)
                {
                    _controllerHandRight.gameObject.SetActive(_isHandVisible);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Updates the current visual state using the given input.
        /// </summary>
        /// <param name="controllerInput">The input device to update the controller with</param>
        /// <param name="onlyIfControllerHand">Whether to update the visual state only if a controller hand is currently assigned</param>
        public void UpdateFromInput(UxrControllerInput controllerInput, bool onlyIfControllerHand = false)
        {
            if (controllerInput == null)
            {
                return;
            }

            foreach (UxrFingerType fingerType in Enum.GetValues(typeof(UxrFingerType)))
            {
                if (fingerType != UxrFingerType.None)
                {
                    _fingerContacts[fingerType].Transform      = null;
                    _fingerContactsLeft[fingerType].Transform  = null;
                    _fingerContactsRight[fingerType].Transform = null;
                }
            }

            foreach (UxrElement element in _controllerElements)
            {
                if (element.ElementObject == null)
                {
                    continue;
                }

                // Update controller element

                bool            contact          = false;
                UxrInputButtons controllerButton = UxrControllerInput.ControllerElementToButton(element.Element);

                switch (element.ElementType)
                {
                    case UxrElementType.Button:

                        if (onlyIfControllerHand && !IsControllerHandPresent(element.HandSide))
                        {
                            element.ElementObject.transform.localPosition = element.InitialLocalPos;
                        }
                        else
                        {
                            if (controllerInput.GetButtonsPress(element.HandSide, controllerButton, true))
                            {
                                element.ElementObject.transform.localPosition = element.InitialLocalPos +
                                                                                element.LocalOffsetX * element.ButtonPressedOffset.x +
                                                                                element.LocalOffsetY * element.ButtonPressedOffset.y +
                                                                                element.LocalOffsetZ * element.ButtonPressedOffset.z;
                            }
                            else
                            {
                                element.ElementObject.transform.localPosition = element.InitialLocalPos;
                            }
                        }

                        break;

                    case UxrElementType.Input1DRotate:

                        float inputRotateValue = 0.0f;

                        if (onlyIfControllerHand && !IsControllerHandPresent(element.HandSide))
                        {
                        }
                        else
                        {
                            inputRotateValue = controllerInput.GetInput1D(element.HandSide, UxrControllerInput.ControllerElementToInput1D(element.Element), true);
                        }

                        Vector3 euler = element.Input1DPressedOffsetAngle * inputRotateValue;
                        element.ElementObject.transform.localRotation = element.InitialLocalRot * Quaternion.Euler(euler);

                        contact = contact || inputRotateValue > 0.01f;
                        break;

                    case UxrElementType.Input1DPush:

                        float inputPushValue = 0.0f;

                        if (onlyIfControllerHand && !IsControllerHandPresent(element.HandSide))
                        {
                        }
                        else
                        {
                            inputPushValue = controllerInput.GetInput1D(element.HandSide, UxrControllerInput.ControllerElementToInput1D(element.Element), true);
                        }

                        Vector3 offset = element.Input1DPressedOffset * inputPushValue;
                        element.ElementObject.transform.localPosition = element.InitialLocalPos + element.LocalOffsetX * offset.x + element.LocalOffsetY * offset.y + element.LocalOffsetZ * offset.z;
                        contact                                       = contact || inputPushValue > 0.01f;
                        break;

                    case UxrElementType.Input2DJoystick:

                        Vector2 inputValueJoystick = Vector2.zero;

                        if (onlyIfControllerHand && !IsControllerHandPresent(element.HandSide))
                        {
                        }
                        else
                        {
                            inputValueJoystick = controllerInput.GetInput2D(element.HandSide, UxrControllerInput.ControllerElementToInput2D(element.Element), true);
                        }

                        Vector3 euler1 = Vector3.Lerp(-element.Input2DFirstAxisOffsetAngle,  element.Input2DFirstAxisOffsetAngle,  (inputValueJoystick.x + 1.0f) * 0.5f);
                        Vector3 euler2 = Vector3.Lerp(-element.Input2DSecondAxisOffsetAngle, element.Input2DSecondAxisOffsetAngle, (inputValueJoystick.y + 1.0f) * 0.5f);
                        element.ElementObject.transform.localRotation = Quaternion.Euler(euler2) * Quaternion.Euler(euler1) * element.InitialLocalRot;
                        contact                                       = contact || inputValueJoystick != Vector2.zero;
                        break;

                    case UxrElementType.Input2DTouch:

                        Vector2 inputValueTouch = controllerInput.GetInput2D(element.HandSide, UxrControllerInput.ControllerElementToInput2D(element.Element), true);

                        if (onlyIfControllerHand && !IsControllerHandPresent(element.HandSide))
                        {
                        }
                        else
                        {
                            inputValueTouch = controllerInput.GetInput2D(element.HandSide, UxrControllerInput.ControllerElementToInput2D(element.Element), true);
                        }

                        Vector3 offset1 = Vector3.Lerp(-element.Input2DFirstAxisOffset,  element.Input2DFirstAxisOffset,  (inputValueTouch.x + 1.0f) * 0.5f);
                        Vector3 offset2 = Vector3.Lerp(-element.Input2DSecondAxisOffset, element.Input2DSecondAxisOffset, (inputValueTouch.y + 1.0f) * 0.5f);

                        element.FingerContactPoint.transform.localPosition = element.FingerContactInitialLocalPos +
                                                                             element.LocalFingerPosOffsetX * offset1.x + element.LocalFingerPosOffsetY * offset1.y + element.LocalFingerPosOffsetZ * offset1.z +
                                                                             element.LocalFingerPosOffsetX * offset2.x + element.LocalFingerPosOffsetY * offset2.y + element.LocalFingerPosOffsetZ * offset2.z;
                        contact = contact || inputValueTouch != Vector2.zero;
                        break;

                    case UxrElementType.DPad:

                        bool dpadLeft  = false;
                        bool dpadRight = false;
                        bool dpadUp    = false;
                        bool dpadDown  = false;

                        if (onlyIfControllerHand && !IsControllerHandPresent(element.HandSide))
                        {
                        }
                        else
                        {
                            dpadLeft  = controllerInput.GetButtonsPress(element.HandSide, UxrInputButtons.DPadLeft,  true);
                            dpadRight = controllerInput.GetButtonsPress(element.HandSide, UxrInputButtons.DPadRight, true);
                            dpadUp    = controllerInput.GetButtonsPress(element.HandSide, UxrInputButtons.DPadUp,    true);
                            dpadDown  = controllerInput.GetButtonsPress(element.HandSide, UxrInputButtons.DPadDown,  true);
                        }

                        Vector3 dpadEuler1 = dpadLeft  ? -element.DpadFirstAxisOffsetAngle :
                                             dpadRight ? element.DpadFirstAxisOffsetAngle : Vector3.zero;
                        Vector3 dpadEuler2 = dpadUp   ? -element.DpadSecondAxisOffsetAngle :
                                             dpadDown ? element.DpadSecondAxisOffsetAngle : Vector3.zero;
                        Vector3 dpadOffset1 = dpadLeft  ? -element.DpadFirstAxisOffset :
                                              dpadRight ? element.DpadFirstAxisOffset : Vector3.zero;
                        Vector3 dpadOffset2 = dpadUp   ? -element.DpadSecondAxisOffset :
                                              dpadDown ? element.DpadSecondAxisOffset : Vector3.zero;

                        element.ElementObject.transform.localRotation = Quaternion.Euler(dpadEuler2) * Quaternion.Euler(dpadEuler1) * element.InitialLocalRot;

                        element.FingerContactPoint.transform.localPosition = element.FingerContactInitialLocalPos +
                                                                             element.LocalFingerPosOffsetX * dpadOffset1.x +
                                                                             element.LocalFingerPosOffsetY * dpadOffset1.y +
                                                                             element.LocalFingerPosOffsetZ * dpadOffset1.z +
                                                                             element.LocalFingerPosOffsetX * dpadOffset2.x +
                                                                             element.LocalFingerPosOffsetY * dpadOffset2.y +
                                                                             element.LocalFingerPosOffsetZ * dpadOffset2.z;

                        contact = contact || dpadLeft || dpadRight || dpadUp || dpadDown;
                        break;

                    case UxrElementType.NotSet: break;
                }

                // Update finger contact?

                contact = contact || (controllerButton != UxrInputButtons.None && (controllerInput.GetButtonsTouch(element.HandSide, controllerButton, true) || controllerInput.GetButtonsPress(element.HandSide, controllerButton, true)));

                if (onlyIfControllerHand && !IsControllerHandPresent(element.HandSide))
                {
                    contact = false;
                }

                if (element.FingerContactPoint == null)
                {
                    continue;
                }

                if (element.FingerContactPoint != element.ElementObject)
                {
                    bool handVisible = _controllerHand && _controllerHand.gameObject.activeSelf;

                    if (_needsBothHands)
                    {
                        handVisible = (element.HandSide == UxrHandSide.Left && _controllerHandLeft != null && _controllerHandLeft.gameObject.activeSelf) ||
                                      (element.HandSide == UxrHandSide.Right && _controllerHandRight != null && _controllerHandRight.gameObject.activeSelf);
                    }

                    element.FingerContactPoint.SetActive(contact && !handVisible);
                }

                if (!contact || element.Finger == UxrFingerType.None)
                {
                    continue;
                }

                if (_needsBothHands == false)
                {
                    _fingerContacts[element.Finger].Transform = element.FingerContactPoint.transform;
                }
                else
                {
                    switch (element.HandSide)
                    {
                        case UxrHandSide.Left:
                            _fingerContactsLeft[element.Finger].Transform = element.FingerContactPoint.transform;
                            break;

                        case UxrHandSide.Right:
                            _fingerContactsRight[element.Finger].Transform = element.FingerContactPoint.transform;
                            break;

                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }

            // Update fingers

            if (_needsBothHands == false)
            {
                if (_controllerHand != null && _fingerContacts != null)
                {
                    foreach (KeyValuePair<UxrFingerType, UxrFingerContactInfo> fingerTransformPair in _fingerContacts)
                    {
                        _controllerHand.UpdateFinger(fingerTransformPair.Key, fingerTransformPair.Value);
                    }
                }
            }
            else
            {
                if (_controllerHandLeft != null && _fingerContactsLeft != null)
                {
                    foreach (KeyValuePair<UxrFingerType, UxrFingerContactInfo> fingerTransformPair in _fingerContactsLeft)
                    {
                        _controllerHandLeft.UpdateFinger(fingerTransformPair.Key, fingerTransformPair.Value);
                    }
                }

                if (_controllerHandRight != null && _fingerContactsRight != null)
                {
                    foreach (KeyValuePair<UxrFingerType, UxrFingerContactInfo> fingerTransformPair in _fingerContactsRight)
                    {
                        _controllerHandRight.UpdateFinger(fingerTransformPair.Key, fingerTransformPair.Value);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the list of GameObjects that represent the given different controller input elements.
        /// </summary>
        /// <param name="elements">Flags representing the input elements to get the objects of</param>
        /// <returns>List of GameObjects representing the given controller input elements</returns>
        public IEnumerable<GameObject> GetElements(UxrControllerElements elements)
        {
            foreach (var value in Enum.GetValues(typeof(UxrControllerElements)))
            {
                UxrControllerElements enumValue = (UxrControllerElements)value;

                if (elements.HasFlag(enumValue) && _hashedElements.TryGetValue(enumValue, out GameObject elementGameObject))
                {
                    yield return elementGameObject;
                }
            }
        }

        /// <summary>
        ///     Gets the list of materials of all objects that represent the given different controller input elements.
        /// </summary>
        /// <param name="elements">Flags representing the input elements to get the materials from</param>
        /// <returns>List of materials used by the objects representing the given controller input elements</returns>
        public IEnumerable<Material> GetElementsMaterials(UxrControllerElements elements)
        {
            foreach (var value in Enum.GetValues(typeof(UxrControllerElements)))
            {
                UxrControllerElements enumValue = (UxrControllerElements)value;

                if (elements.HasFlag(enumValue) && _hashedElements.TryGetValue(enumValue, out GameObject elementGameObject))
                {
                    Renderer elementRenderer = elementGameObject.GetComponent<Renderer>();

                    if (elementRenderer != null && elementRenderer.material != null)
                    {
                        yield return elementRenderer.material;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the list of original shared materials of all objects that represent the given different controller input
        ///     elements. The original materials are the shared materials that the input elements had at the beginning, before any
        ///     modifications.
        /// </summary>
        /// <param name="elements">Flags representing the input elements to get the original shared materials from</param>
        /// <returns>List of original shared materials used by the objects representing the given controller input elements</returns>
        public IEnumerable<Material> GetElementsOriginalMaterials(UxrControllerElements elements)
        {
            foreach (var value in Enum.GetValues(typeof(UxrControllerElements)))
            {
                UxrControllerElements enumValue = (UxrControllerElements)value;

                if (elements.HasFlag(enumValue) && _hashedElementsOriginalMaterial.TryGetValue(enumValue, out Material elementMaterial))
                {
                    yield return elementMaterial;
                }
            }
        }

        /// <summary>
        ///     Changes the material of the objects that represent the given different controller input elements.
        /// </summary>
        /// <param name="elements">Flags representing the input elements whose materials will be changed</param>
        /// <param name="material">New material to assign</param>
        public void SetElementsMaterial(UxrControllerElements elements, Material material)
        {
            foreach (var value in Enum.GetValues(typeof(UxrControllerElements)))
            {
                UxrControllerElements enumValue = (UxrControllerElements)value;

                if (elements.HasFlag(enumValue) && _hashedElements.TryGetValue(enumValue, out GameObject elementGameObject) 
                    && elementGameObject.TryGetComponent<Renderer>(out var elementRenderer))
                {
                    elementRenderer.material = material;
                }
            }
        }

        /// <summary>
        ///     Restores the materials of the objects that represent the given different controller input elements.
        /// </summary>
        /// <param name="elements">Flags representing the input elements whose materials to restore</param>
        public void RestoreElementsMaterials(UxrControllerElements elements)
        {
            foreach (var value in Enum.GetValues(typeof(UxrControllerElements)))
            {
                UxrControllerElements enumValue = (UxrControllerElements)value;

                if (elements.HasFlag(enumValue) && _hashedElements.TryGetValue(enumValue, out GameObject elementGameObject) 
                    && elementGameObject.TryGetComponent<Renderer>(out var elementRenderer))
                {
                    elementRenderer.sharedMaterial = _hashedElementsOriginalMaterial[enumValue];
                }
            }
        }

        /// <summary>
        ///     Changes the current hand to use the controller to the opposite side.
        /// </summary>
        public void SwitchHandedness()
        {
            if (_needsBothHands)
            {
                return;
            }

            _handSide = _handSide == UxrHandSide.Left ? UxrHandSide.Right : UxrHandSide.Left;

            foreach (UxrElement element in _controllerElements)
            {
                element.HandSide = element.HandSide == UxrHandSide.Left ? UxrHandSide.Right : UxrHandSide.Left;
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize data

            _avatar = GetComponentInParent<UxrAvatar>();

            foreach (UxrFingerType fingerType in Enum.GetValues(typeof(UxrFingerType)))
            {
                if (fingerType != UxrFingerType.None)
                {
                    _fingerContacts.Add(fingerType, new UxrFingerContactInfo(null));
                    _fingerContactsLeft.Add(fingerType, new UxrFingerContactInfo(null));
                    _fingerContactsRight.Add(fingerType, new UxrFingerContactInfo(null));
                }
            }

            if (_controllerElements != null)
            {
                foreach (UxrElement element in _controllerElements)
                {
                    if (element.ElementObject != null)
                    {
                        // Initialize initial pos/rot

                        element.InitialLocalPos = element.ElementObject.transform.localPosition;
                        element.InitialLocalRot = element.ElementObject.transform.localRotation;

                        // Initialize original materials and hashed elements

                        if (_hashedElements.ContainsKey(element.Element))
                        {
                            //Debug.LogWarning($"Element {element.Element} was already found in the {nameof(UxrController3DModel)} list of {name}. Ignoring.");
                        }
                        else
                        {
                            // Element
                            _hashedElements.Add(element.Element, element.ElementObject);

                            // Original materials
                            Renderer renderer = element.ElementObject.GetComponent<Renderer>();
                            _hashedElementsOriginalMaterial.Add(element.Element, renderer != null ? renderer.sharedMaterial : null);
                        }

                        element.LocalOffsetX = element.ElementObject.transform.parent.InverseTransformDirection(element.ElementObject.transform.right);
                        element.LocalOffsetY = element.ElementObject.transform.parent.InverseTransformDirection(element.ElementObject.transform.up);
                        element.LocalOffsetZ = element.ElementObject.transform.parent.InverseTransformDirection(element.ElementObject.transform.forward);

                        if (element.FingerContactPoint != null)
                        {
                            element.LocalFingerPosOffsetX = element.FingerContactPoint.transform.parent.InverseTransformDirection(element.ElementObject.transform.right);
                            element.LocalFingerPosOffsetY = element.FingerContactPoint.transform.parent.InverseTransformDirection(element.ElementObject.transform.up);
                            element.LocalFingerPosOffsetZ = element.FingerContactPoint.transform.parent.InverseTransformDirection(element.ElementObject.transform.forward);
                        }
                    }

                    if (element.ElementObject != null && element.FingerContactPoint != null)
                    {
                        element.FingerContactInitialLocalPos = element.FingerContactPoint.transform.localPosition;

                        if (element.FingerContactPoint != element.ElementObject)
                        {
                            element.FingerContactPoint.SetActive(false);
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets whether the component has a visual hand available for visualization.
        /// </summary>
        /// <param name="handSide">Hand to check for</param>
        /// <returns>Whether there is a visual hand available</returns>
        private bool IsControllerHandPresent(UxrHandSide handSide)
        {
            if (_needsBothHands)
            {
                return handSide == UxrHandSide.Left ? _controllerHandLeft != null : _controllerHandRight != null;
            }

            return _controllerHand != null;
        }

        #endregion

        #region Private Types & Data

        private readonly Dictionary<UxrControllerElements, GameObject>   _hashedElements                 = new Dictionary<UxrControllerElements, GameObject>();
        private readonly Dictionary<UxrControllerElements, Material>     _hashedElementsOriginalMaterial = new Dictionary<UxrControllerElements, Material>();
        private readonly Dictionary<UxrFingerType, UxrFingerContactInfo> _fingerContacts                 = new Dictionary<UxrFingerType, UxrFingerContactInfo>();
        private readonly Dictionary<UxrFingerType, UxrFingerContactInfo> _fingerContactsLeft             = new Dictionary<UxrFingerType, UxrFingerContactInfo>();
        private readonly Dictionary<UxrFingerType, UxrFingerContactInfo> _fingerContactsRight            = new Dictionary<UxrFingerType, UxrFingerContactInfo>();

        private UxrAvatar _avatar;
        private bool      _isControllerVisible = true;
        private bool      _isHandVisible       = true;

        #endregion
    }
}