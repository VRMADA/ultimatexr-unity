// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCanvas.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Component that, added to a <see cref="GameObject" /> with a <see cref="Canvas" /> component, enables interaction
    ///     using <see cref="UxrFingerTip" /> components or <see cref="UxrLaserPointer" /> components.
    /// </summary>
    public class UxrCanvas : UxrComponent<UxrCanvas>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] protected UxrInteractionType _interactionType;
        [SerializeField] protected float              _fingerTipMinHoverDistance = UxrFingerTipRaycaster.FingerTipMinHoverDistanceDefault;
        [SerializeField] protected bool               _autoEnableLaserPointer;
        [SerializeField] protected float              _autoEnableDistance = 5.0f;
        [SerializeField] protected bool               _allowLeftHand      = true;
        [SerializeField] protected bool               _allowRightHand     = true;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the Unity <see cref="Canvas" /> component.
        /// </summary>
        public Canvas UnityCanvas => GetCachedComponent<Canvas>();

        /// <summary>
        ///     Gets or sets whether the <see cref="UxrLaserPointer" /> components will automatically show their laser while
        ///     pointing towards the canvas.
        /// </summary>
        public bool AutoEnableLaserPointer
        {
            get => _autoEnableLaserPointer;
            set => _autoEnableLaserPointer = value;
        }

        /// <summary>
        ///     Gets or sets the distance below which the <see cref="UxrLaserPointer" /> will automatically show the laser while
        ///     pointing towards the canvas.
        /// </summary>
        public float AutoEnableDistance
        {
            get => _autoEnableDistance;
            set => _autoEnableDistance = value;
        }

        /// <summary>
        ///     Gets or sets the type of interaction with the UI components in the canvas.
        /// </summary>
        public UxrInteractionType CanvasInteractionType
        {
            get => _interactionType;
            set
            {
                _interactionType = value;

                if (_oldRaycaster != null)
                {
                    DestroyVRCanvas();
                    CreateVRCanvas();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the distance below which a <see cref="UxrFingerTip" /> component will generate hovering events.
        /// </summary>
        public float FingerTipMinHoverDistance
        {
            get => _fingerTipMinHoverDistance;
            set => _fingerTipMinHoverDistance = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if the canvas can be used with the given hand. This allows some canvases to work for the left or
        ///     right hand only.
        /// </summary>
        /// <param name="handSide">Which hand to check</param>
        /// <returns>Boolean telling whether the given hand is compatible or not</returns>
        public bool IsCompatible(UxrHandSide handSide)
        {
            return (handSide == UxrHandSide.Left && _allowLeftHand) || (handSide == UxrHandSide.Right && _allowRightHand);
        }

        /// <summary>
        ///     Sets up the canvas so that it can be used with <see cref="UxrPointerInputModule" />.
        /// </summary>
        /// <param name="inputModule">The input module</param>
        public void SetupCanvas(UxrPointerInputModule inputModule)
        {
            CanvasInteractionType = inputModule.InteractionTypeOnAutoEnable;

            if (_newRaycasterFingerTips != null)
            {
                _newRaycasterFingerTips.FingerTipMinHoverDistance = inputModule.FingerTipMinHoverDistance;
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (UxrPointerInputModule.Instance && UxrPointerInputModule.Instance.AutoAssignEventCamera && UnityCanvas && UxrAvatar.LocalAvatar)
            {
                UnityCanvas.worldCamera = UxrAvatar.LocalAvatar.CameraComponent;
            }

            if (_newRaycasterFingerTips == null && _newRaycasterLaserPointer == null)
            {
                CreateVRCanvas();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates the raycaster required to use the <see cref="UxrCanvas" /> component with
        ///     <see cref="UxrPointerInputModule" />.
        /// </summary>
        private void CreateVRCanvas()
        {
            if (UnityCanvas == null)
            {
                return;
            }

            if (_oldRaycaster == null)
            {
                _oldRaycaster = UnityCanvas.gameObject.GetComponent<GraphicRaycaster>();
            }

            if (_interactionType == UxrInteractionType.FingerTips)
            {
                _newRaycasterFingerTips                           = GetOrAddRaycaster<UxrFingerTipRaycaster>(_oldRaycaster);
                _newRaycasterFingerTips.FingerTipMinHoverDistance = _fingerTipMinHoverDistance;
            }
            else if (_interactionType == UxrInteractionType.LaserPointers)
            {
                _newRaycasterLaserPointer = GetOrAddRaycaster<UxrLaserPointerRaycaster>(_oldRaycaster);
            }
        }

        /// <summary>
        ///     Sets up the new raycaster.
        /// </summary>
        /// <param name="oldRaycaster"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetOrAddRaycaster<T>(GraphicRaycaster oldRaycaster) where T : GraphicRaycaster
        {
            bool copyParameters = UnityCanvas.GetComponent<T>() == null;
            T    rayCaster      = UnityCanvas.GetOrAddComponent<T>();
            
            rayCaster.enabled = true;

            if (oldRaycaster && rayCaster)
            {
                if (copyParameters)
                {
                    rayCaster.ignoreReversedGraphics = oldRaycaster.ignoreReversedGraphics;
                    rayCaster.blockingObjects        = GraphicRaycaster.BlockingObjects.All;
                    rayCaster.blockingMask           = oldRaycaster.blockingMask;
                }
                
                oldRaycaster.enabled = false;
            }

            return rayCaster;
        }

        /// <summary>
        ///     Destroys the new raycaster and restores the old one.
        /// </summary>
        private void DestroyVRCanvas()
        {
            if (_newRaycasterFingerTips)
            {
                Destroy(_newRaycasterFingerTips);
            }

            if (_newRaycasterLaserPointer)
            {
                Destroy(_newRaycasterLaserPointer);
            }

            if (_oldRaycaster && _oldRaycaster.enabled == false)
            {
                _oldRaycaster.enabled = true;
            }
        }

        #endregion

        #region Private Types & Data

        private GraphicRaycaster         _oldRaycaster;
        private UxrFingerTipRaycaster    _newRaycasterFingerTips;
        private UxrLaserPointerRaycaster _newRaycasterLaserPointer;

        #endregion
    }
}