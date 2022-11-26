// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarRig.HandTransformation.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    partial class UxrAvatarRig
    {
        #region Public Methods

        /// <summary>
        ///     Updates an avatar's hand transforms using a fixed hand descriptor.
        /// </summary>
        /// <param name="avatar">The avatar to update</param>
        /// <param name="handSide">Which hand to update</param>
        /// <param name="handDescriptor">The descriptor to get the data from</param>
        public static void UpdateHandUsingDescriptor(UxrAvatar avatar, UxrHandSide handSide, UxrHandDescriptor handDescriptor)
        {
            UpdateHandUsingDescriptor(avatar.GetHand(handSide), handDescriptor, avatar.AvatarRigInfo.GetArmInfo(handSide).HandUniversalLocalAxes, avatar.AvatarRigInfo.GetArmInfo(handSide).FingerUniversalLocalAxes);
        }

        /// <summary>
        ///     Updates an avatar's hand transforms using two hand descriptors and a blend value.
        /// </summary>
        /// <param name="avatar">The avatar to update</param>
        /// <param name="handSide">Which hand to update</param>
        /// <param name="handDescriptorA">The descriptor for the hand pose to blend from</param>
        /// <param name="handDescriptorB">The descriptor for the hand pose to blend to</param>
        /// <param name="blend">The interpolation value [0.0, 1.0]</param>
        public static void UpdateHandUsingDescriptor(UxrAvatar avatar, UxrHandSide handSide, UxrHandDescriptor handDescriptorA, UxrHandDescriptor handDescriptorB, float blend)
        {
            UpdateHandUsingDescriptor(avatar.GetHand(handSide), handDescriptorA, handDescriptorB, blend, avatar.AvatarRigInfo.GetArmInfo(handSide).HandUniversalLocalAxes, avatar.AvatarRigInfo.GetArmInfo(handSide).FingerUniversalLocalAxes);
        }

        /// <summary>
        ///     Updates the hand transforms using a hand descriptor.
        /// </summary>
        /// <param name="hand">The hand to update</param>
        /// <param name="handDescriptor">The descriptor of the hand pose</param>
        /// <param name="handLocalAxes">The universal coordinate system of the hand transform</param>
        /// <param name="handLocalFingerAxes">The universal coordinate system of the finger transforms</param>
        public static void UpdateHandUsingDescriptor(UxrAvatarHand hand, UxrHandDescriptor handDescriptor, UxrUniversalLocalAxes handLocalAxes, UxrUniversalLocalAxes handLocalFingerAxes)
        {
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Thumb,  handDescriptor.Thumb,  handLocalAxes, handLocalFingerAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Index,  handDescriptor.Index,  handLocalAxes, handLocalFingerAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Middle, handDescriptor.Middle, handLocalAxes, handLocalFingerAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Ring,   handDescriptor.Ring,   handLocalAxes, handLocalFingerAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Little, handDescriptor.Little, handLocalAxes, handLocalFingerAxes);
        }

        /// <summary>
        ///     Updates the hand transforms using two hand descriptors and an interpolation value.
        /// </summary>
        /// <param name="hand">The hand to update</param>
        /// <param name="handDescriptorA">The descriptor of the hand pose to blend from</param>
        /// <param name="handDescriptorB">The descriptor of the hand pose to blend to</param>
        /// <param name="blend">The interpolation value [0.0, 1.0]</param>
        /// <param name="handLocalAxes">The universal coordinate system of the hand transform</param>
        /// <param name="fingerLocalAxes">The universal coordinate system of the finger transforms</param>
        public static void UpdateHandUsingDescriptor(UxrAvatarHand hand, UxrHandDescriptor handDescriptorA, UxrHandDescriptor handDescriptorB, float blend, UxrUniversalLocalAxes handLocalAxes, UxrUniversalLocalAxes fingerLocalAxes)
        {
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Thumb,  handDescriptorA.Thumb,  handDescriptorB.Thumb,  blend, handLocalAxes, fingerLocalAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Index,  handDescriptorA.Index,  handDescriptorB.Index,  blend, handLocalAxes, fingerLocalAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Middle, handDescriptorA.Middle, handDescriptorB.Middle, blend, handLocalAxes, fingerLocalAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Ring,   handDescriptorA.Ring,   handDescriptorB.Ring,   blend, handLocalAxes, fingerLocalAxes);
            UpdateFingerUsingDescriptor(hand.Wrist, hand.Little, handDescriptorA.Little, handDescriptorB.Little, blend, handLocalAxes, fingerLocalAxes);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates a finger's transforms from a finger descriptor.
        /// </summary>
        /// <param name="wrist">The wrist (root) transform of the hand</param>
        /// <param name="finger">The finger to update</param>
        /// <param name="fingerDescriptor">The descriptor to get the data from</param>
        /// <param name="handLocalAxes">The universal coordinate system of the hand transform</param>
        /// <param name="fingerLocalAxes">The universal coordinate system of the finger transforms</param>
        private static void UpdateFingerUsingDescriptor(Transform wrist, UxrAvatarFinger finger, UxrFingerDescriptor fingerDescriptor, UxrUniversalLocalAxes handLocalAxes, UxrUniversalLocalAxes fingerLocalAxes)
        {
            if (fingerDescriptor.HasMetacarpalInfo && finger.Metacarpal)
            {
                UpdateFingerNodeUsingDescriptor(wrist,             finger.Metacarpal, fingerDescriptor.Metacarpal, handLocalAxes,   fingerLocalAxes);
                UpdateFingerNodeUsingDescriptor(finger.Metacarpal, finger.Proximal,   fingerDescriptor.Proximal,   fingerLocalAxes, fingerLocalAxes);
            }
            else
            {
                UpdateFingerNodeUsingDescriptor(wrist, finger.Proximal, fingerDescriptor.ProximalNoMetacarpal, handLocalAxes, fingerLocalAxes);
            }

            UpdateFingerNodeUsingDescriptor(finger.Proximal,     finger.Intermediate, fingerDescriptor.Intermediate, fingerLocalAxes, fingerLocalAxes);
            UpdateFingerNodeUsingDescriptor(finger.Intermediate, finger.Distal,       fingerDescriptor.Distal,       fingerLocalAxes, fingerLocalAxes);
        }

        /// <summary>
        ///     Updates a finger's transforms using two finger descriptors and an interpolation value.
        /// </summary>
        /// <param name="wrist">The wrist (root) transform of the hand</param>
        /// <param name="finger">The finger to update</param>
        /// <param name="fingerDescriptorA">The descriptor A to get the data from</param>
        /// <param name="fingerDescriptorB">The descriptor B to get the data from</param>
        /// <param name="blend">The interpolation value [0.0, 1.0]</param>
        /// <param name="handLocalAxes">The universal coordinate system of the hand transform</param>
        /// <param name="fingerLocalAxes">The universal coordinate system of the finger transforms</param>
        private static void UpdateFingerUsingDescriptor(Transform             wrist,
                                                        UxrAvatarFinger       finger,
                                                        UxrFingerDescriptor   fingerDescriptorA,
                                                        UxrFingerDescriptor   fingerDescriptorB,
                                                        float                 blend,
                                                        UxrUniversalLocalAxes handLocalAxes,
                                                        UxrUniversalLocalAxes fingerLocalAxes)
        {
            if (fingerDescriptorA.HasMetacarpalInfo && finger.Metacarpal)
            {
                UpdateFingerNodeUsingDescriptor(wrist,             finger.Metacarpal, fingerDescriptorA.Metacarpal, fingerDescriptorB.Metacarpal, blend, handLocalAxes,   fingerLocalAxes);
                UpdateFingerNodeUsingDescriptor(finger.Metacarpal, finger.Proximal,   fingerDescriptorA.Proximal,   fingerDescriptorB.Proximal,   blend, fingerLocalAxes, fingerLocalAxes);
            }
            else
            {
                UpdateFingerNodeUsingDescriptor(wrist, finger.Proximal, fingerDescriptorA.ProximalNoMetacarpal, fingerDescriptorB.ProximalNoMetacarpal, blend, handLocalAxes, fingerLocalAxes);
            }

            UpdateFingerNodeUsingDescriptor(finger.Proximal,     finger.Intermediate, fingerDescriptorA.Intermediate, fingerDescriptorB.Intermediate, blend, fingerLocalAxes, fingerLocalAxes);
            UpdateFingerNodeUsingDescriptor(finger.Intermediate, finger.Distal,       fingerDescriptorA.Distal,       fingerDescriptorB.Distal,       blend, fingerLocalAxes, fingerLocalAxes);
        }

        /// <summary>
        ///     Updates a finger bone transform from a node descriptor.
        /// </summary>
        /// <param name="parent">The node parent</param>
        /// <param name="node">The node being updated</param>
        /// <param name="nodeDescriptor">The descriptor to get the data from</param>
        /// <param name="parentLocalAxes">The universal coordinate system of the parent's transform</param>
        /// <param name="nodeLocalAxes">The universal coordinate system of the node transform</param>
        private static void UpdateFingerNodeUsingDescriptor(Transform parent, Transform node, UxrFingerNodeDescriptor nodeDescriptor, UxrUniversalLocalAxes parentLocalAxes, UxrUniversalLocalAxes nodeLocalAxes)
        {
            Matrix4x4 nodeLocalAxesMatrix = new Matrix4x4();
            nodeLocalAxesMatrix.SetColumn(0, nodeLocalAxes.LocalRight);
            nodeLocalAxesMatrix.SetColumn(1, nodeLocalAxes.LocalUp);
            nodeLocalAxesMatrix.SetColumn(2, nodeLocalAxes.LocalForward);
            nodeLocalAxesMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));
            Quaternion nodeUniversalToActual = Quaternion.Inverse(nodeLocalAxesMatrix.rotation);

            Matrix4x4 parentUniversalMatrix = new Matrix4x4();
            parentUniversalMatrix.SetColumn(0, parent.TransformVector(parentLocalAxes.LocalRight));
            parentUniversalMatrix.SetColumn(1, parent.TransformVector(parentLocalAxes.LocalUp));
            parentUniversalMatrix.SetColumn(2, parent.TransformVector(parentLocalAxes.LocalForward));
            parentUniversalMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));

            Matrix4x4 nodeUniversalMatrix = new Matrix4x4();
            nodeUniversalMatrix.SetColumn(0, parentUniversalMatrix.MultiplyVector(nodeDescriptor.Right));
            nodeUniversalMatrix.SetColumn(1, parentUniversalMatrix.MultiplyVector(nodeDescriptor.Up));
            nodeUniversalMatrix.SetColumn(2, parentUniversalMatrix.MultiplyVector(nodeDescriptor.Forward));
            nodeUniversalMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));

            node.rotation = nodeUniversalMatrix.rotation * nodeUniversalToActual;
        }

        /// <summary>
        ///     Updates a finger bone transform using two node descriptors and an interpolation value.
        /// </summary>
        /// <param name="parent">The node parent</param>
        /// <param name="node">The node being updated</param>
        /// <param name="nodeDescriptorA">The descriptor A to get the data from</param>
        /// <param name="nodeDescriptorB">The descriptor B to get the data from</param>
        /// <param name="blend">The interpolation value [0.0, 1.0]</param>
        /// <param name="parentLocalAxes">The universal coordinate system of the parent's transform</param>
        /// <param name="nodeLocalAxes">The universal coordinate system of the node transform</param>
        private static void UpdateFingerNodeUsingDescriptor(Transform               parent,
                                                            Transform               node,
                                                            UxrFingerNodeDescriptor nodeDescriptorA,
                                                            UxrFingerNodeDescriptor nodeDescriptorB,
                                                            float                   blend,
                                                            UxrUniversalLocalAxes   parentLocalAxes,
                                                            UxrUniversalLocalAxes   nodeLocalAxes)
        {
            UpdateFingerNodeUsingDescriptor(parent, node, nodeDescriptorA, parentLocalAxes, nodeLocalAxes);
            Quaternion rotA = node.rotation;
            UpdateFingerNodeUsingDescriptor(parent, node, nodeDescriptorB, parentLocalAxes, nodeLocalAxes);
            Quaternion rotB = node.rotation;

            node.rotation = Quaternion.Slerp(rotA, rotB, blend);
        }

        #endregion
    }
}