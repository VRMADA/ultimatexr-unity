// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandIntegration.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Avatar
{
    /// <summary>
    ///     Component that allows to select different graphical variations for the hands that are shown grabbing the
    ///     controllers using IK.
    /// </summary>
    [DisallowMultipleComponent]
    public partial class UxrHandIntegration : UxrAvatarComponent<UxrHandIntegration>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField]                   private GizmoHandSize _gizmoHandSize;
        [SerializeField]                   private UxrHandSide   _handSide;
        [SerializeField] [HideInInspector] private int           _selectedRenderPipeline;
        [SerializeField]                   private string        _objectVariationName;
        [SerializeField]                   private string        _materialVariationName;

        #endregion

        #region Public Types & Data

        public UxrHandSide HandSide => _handSide;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Tries to align the hand integration transform to fit the current avatar's hand as best as possible.
        /// </summary>
        /// <returns>
        ///     True if the transform was changed, false otherwise. False means that the avatar hand was missing the required
        ///     bone references, either fingers or the wrist
        /// </returns>
        public bool TryToMatchHand()
        {
            // Get component and look for requirements:

            UxrAvatar avatar = GetComponentInParent<UxrAvatar>();

            if (avatar == null)
            {
                return false;
            }

            UxrAvatarHand hand = avatar.GetHand(_handSide);

            if (hand == null || !hand.HasFingerData() || hand.Wrist == null)
            {
                return false;
            }

            // Set position

            transform.position = hand.Wrist.position;

            // Set orientation knowing that UltimateXR hand integrations use:
            //  left hand: X = to forearm, Y = backhand, Z = right.
            //  right hand: X = to forearm, Y = palm, Z = left.

            hand.GetPalmCenter(out Vector3 palmCenter);
            hand.GetPalmToFingerDirection(out Vector3 palmToFinger);
            hand.GetPalmOutDirection(_handSide, out Vector3 palmOut);

            if (_handSide == UxrHandSide.Left)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.Cross(-palmToFinger, -palmOut), -palmOut);
            }
            else if (_handSide == UxrHandSide.Right)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.Cross(-palmToFinger, palmOut), palmOut);
            }

            // Keeping the new orientation, try to offset so that the grabber stays in the center of the palm.

            UxrGrabber grabber = GetComponentInChildren<UxrGrabber>();

            if (grabber)
            {
                Vector3 offset = grabber.transform.position - (palmCenter + palmOut * 0.02f);

                // Remove left/right/up/down offset

                offset   = transform.InverseTransformDirection(offset);
                offset.z = 0.0f;
                offset   = transform.TransformDirection(offset);

                // Apply offset

                transform.position -= offset;
            }

            return true;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Parents the hand integration to the avatar's hand at the beginning so that all components in the hand integration
        ///     work correctly.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Avatar)
            {
                Transform handBone = Avatar.GetHandBone(_handSide);

                if (handBone)
                {
                    transform.SetParent(handBone);
                }
            }
        }

        #endregion
    }
}