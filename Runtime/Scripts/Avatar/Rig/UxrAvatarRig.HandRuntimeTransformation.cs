// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRig.HandRuntimeTransformation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    partial class UxrAvatarRig
    {
        #region Public Methods

        /// <summary>
        ///     Saves all the transform information of the bones of a hand so that it can later be restored using
        ///     <see cref="PopHandTransforms" />.
        /// </summary>
        /// <param name="hand">Hand to store all the transforms information of</param>
        /// <returns>Transform information</returns>
        public static Dictionary<Transform, UxrTransform> PushHandTransforms(UxrAvatarHand hand)
        {
            Dictionary<Transform, UxrTransform> transforms = new Dictionary<Transform, UxrTransform>();

            foreach (Transform transform in hand.Transforms)
            {
                transforms.Add(transform, new UxrTransform(transform));
            }

            return transforms;
        }

        /// <summary>
        ///     Restores all the transform information of the bones of a hand saved using <see cref="PushHandTransforms" />.
        /// </summary>
        /// <param name="hand">Hand to restore</param>
        /// <param name="transforms">Transform information</param>
        /// <remarks>The transform information is restored using local position/rotation/scale values</remarks>
        public static void PopHandTransforms(UxrAvatarHand hand, Dictionary<Transform, UxrTransform> transforms)
        {
            foreach (var transform in transforms)
            {
                transform.Value.ApplyTo(transform.Key);
            }
        }

        /// <summary>
        ///     Curls an avatar finger.
        /// </summary>
        /// <param name="avatar">Avatar to curl the finger of</param>
        /// <param name="handSide">Which hand the finger belongs to</param>
        /// <param name="finger">Finger to curl</param>
        /// <param name="proximalCurl">Curl angle in degrees for the proximal bone</param>
        /// <param name="intermediateCurl">Curl angle in degrees for the intermediate bone</param>
        /// <param name="distalCurl">Curl angle in degrees for the distal bone</param>
        /// <param name="spread">Spread angle in degrees for the finger (finger "left" or "right" amount with respect to the wrist)</param>
        public static void CurlFinger(UxrAvatar avatar, UxrHandSide handSide, UxrAvatarFinger finger, float proximalCurl, float intermediateCurl, float distalCurl, float spread = 0.0f)
        {
            UxrUniversalLocalAxes fingerAxes = avatar.AvatarRigInfo.GetArmInfo(handSide).FingerUniversalLocalAxes;

            if (avatar.GetInitialBoneLocalRotation(finger.Proximal, out Quaternion localRotationProximal))
            {
                finger.Proximal.Rotate(fingerAxes.LocalRight, proximalCurl,                                           Space.Self);
                finger.Proximal.Rotate(fingerAxes.LocalUp,    spread * (handSide == UxrHandSide.Left ? 1.0f : -1.0f), Space.Self);
            }

            if (avatar.GetInitialBoneLocalRotation(finger.Intermediate, out Quaternion localRotationIntermediate))
            {
                finger.Intermediate.Rotate(fingerAxes.LocalRight, intermediateCurl, Space.Self);
            }

            if (avatar.GetInitialBoneLocalRotation(finger.Distal, out Quaternion localRotationDistal))
            {
                finger.Distal.Rotate(fingerAxes.LocalRight, distalCurl, Space.Self);
            }
        }

        /// <summary>
        ///     Updates the hand transforms using a runtime hand descriptor.
        /// </summary>
        /// <param name="avatar">Avatar to update</param>
        /// <param name="handSide">The hand to update</param>
        /// <param name="handDescriptor">The runtime descriptor of the hand pose</param>
        public static void UpdateHandUsingRuntimeDescriptor(UxrAvatar avatar, UxrHandSide handSide, UxrRuntimeHandDescriptor handDescriptor)
        {
            UxrAvatarHand hand = avatar.GetHand(handSide);

            UpdateFingerUsingRuntimeDescriptor(hand.Thumb,  handDescriptor.Thumb);
            UpdateFingerUsingRuntimeDescriptor(hand.Index,  handDescriptor.Index);
            UpdateFingerUsingRuntimeDescriptor(hand.Middle, handDescriptor.Middle);
            UpdateFingerUsingRuntimeDescriptor(hand.Ring,   handDescriptor.Ring);
            UpdateFingerUsingRuntimeDescriptor(hand.Little, handDescriptor.Little);
        }

        /// <summary>
        ///     Updates the hand transforms blending between two runtime hand descriptors.
        /// </summary>
        /// <param name="avatar">Avatar to update</param>
        /// <param name="handSide">The hand to update</param>
        /// <param name="handDescriptorA">The runtime descriptor of the hand pose to blend from</param>
        /// <param name="handDescriptorB">The runtime descriptor of the hand pose to blend to</param>
        /// <param name="blend">Interpolation value [0.0, 1.0]</param>
        public static void UpdateHandUsingRuntimeDescriptor(UxrAvatar avatar, UxrHandSide handSide, UxrRuntimeHandDescriptor handDescriptorA, UxrRuntimeHandDescriptor handDescriptorB, float blend)
        {
            UxrAvatarHand hand = avatar.GetHand(handSide);

            UpdateFingerUsingRuntimeDescriptor(hand.Thumb,  handDescriptorA.Thumb,  handDescriptorB.Thumb,  blend);
            UpdateFingerUsingRuntimeDescriptor(hand.Index,  handDescriptorA.Index,  handDescriptorB.Index,  blend);
            UpdateFingerUsingRuntimeDescriptor(hand.Middle, handDescriptorA.Middle, handDescriptorB.Middle, blend);
            UpdateFingerUsingRuntimeDescriptor(hand.Ring,   handDescriptorA.Ring,   handDescriptorB.Ring,   blend);
            UpdateFingerUsingRuntimeDescriptor(hand.Little, handDescriptorA.Little, handDescriptorB.Little, blend);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates a finger's transforms from a runtime finger descriptor.
        /// </summary>
        /// <param name="finger">The finger to update</param>
        /// <param name="fingerDescriptor">The runtime descriptor to get the data from</param>
        private static void UpdateFingerUsingRuntimeDescriptor(UxrAvatarFinger finger, UxrRuntimeFingerDescriptor fingerDescriptor)
        {
            if (fingerDescriptor.HasMetacarpalInfo)
            {
                finger.Metacarpal.localRotation = fingerDescriptor.MetacarpalRotation;
            }

            finger.Proximal.localRotation     = fingerDescriptor.ProximalRotation;
            finger.Intermediate.localRotation = fingerDescriptor.IntermediateRotation;
            finger.Distal.localRotation       = fingerDescriptor.DistalRotation;
        }

        /// <summary>
        ///     Updates a finger's transforms from a runtime finger descriptor.
        /// </summary>
        /// <param name="finger">The finger to update</param>
        /// <param name="fingerDescriptorA">The runtime descriptor to blend from</param>
        /// <param name="fingerDescriptorB">The runtime descriptor to blend to</param>
        /// <param name="blend">The interpolation parameter [0.0, 1.0]</param>
        private static void UpdateFingerUsingRuntimeDescriptor(UxrAvatarFinger finger, UxrRuntimeFingerDescriptor fingerDescriptorA, UxrRuntimeFingerDescriptor fingerDescriptorB, float blend)
        {
            if (fingerDescriptorA.HasMetacarpalInfo && fingerDescriptorB.HasMetacarpalInfo)
            {
                finger.Metacarpal.localRotation = Quaternion.Slerp(fingerDescriptorA.MetacarpalRotation, fingerDescriptorB.MetacarpalRotation, blend);
            }

            finger.Proximal.localRotation     = Quaternion.Slerp(fingerDescriptorA.ProximalRotation,     fingerDescriptorB.ProximalRotation,     blend);
            finger.Intermediate.localRotation = Quaternion.Slerp(fingerDescriptorA.IntermediateRotation, fingerDescriptorB.IntermediateRotation, blend);
            finger.Distal.localRotation       = Quaternion.Slerp(fingerDescriptorA.DistalRotation,       fingerDescriptorB.DistalRotation,       blend);
        }

        #endregion
    }
}