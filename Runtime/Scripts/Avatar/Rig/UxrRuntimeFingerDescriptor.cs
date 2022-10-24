// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrRuntimeFingerDescriptor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Avatar.Rig
{
    /// <summary>
    ///     Runtime, lightweight version of <see cref="UxrFingerDescriptor" />. See <see cref="UxrRuntimeHandDescriptor" />.
    /// </summary>
    public class UxrRuntimeFingerDescriptor
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether the descriptor contains metacarpal information.
        /// </summary>
        public bool HasMetacarpalInfo { get; private set; }

        /// <summary>
        ///     Gets the metacarpal local rotation.
        /// </summary>
        public Quaternion MetacarpalRotation { get; private set; }

        /// <summary>
        ///     Gets the proximal local rotation.
        /// </summary>
        public Quaternion ProximalRotation { get; private set; }

        /// <summary>
        ///     Gets the intermediate local rotation.
        /// </summary>
        public Quaternion IntermediateRotation { get; private set; }

        /// <summary>
        ///     Gets the proximal local rotation.
        /// </summary>
        public Quaternion DistalRotation { get; private set; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UxrRuntimeFingerDescriptor()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="avatar">Avatar to compute the runtime finger descriptor for</param>
        /// <param name="handSide">Which hand to process</param>
        /// <param name="handDescriptor">The source data</param>
        /// <param name="fingerType">Which finger to store</param>
        public UxrRuntimeFingerDescriptor(UxrAvatar avatar, UxrHandSide handSide, UxrHandDescriptor handDescriptor, UxrFingerType fingerType)
        {
            UxrAvatarHand         avatarHand       = avatar.GetHand(handSide);
            UxrAvatarFinger       avatarFinger     = avatarHand.GetFinger(fingerType);
            UxrUniversalLocalAxes handLocalAxes    = avatar.AvatarRigInfo.GetArmInfo(handSide).HandUniversalLocalAxes;
            UxrUniversalLocalAxes fingerLocalAxes  = avatar.AvatarRigInfo.GetArmInfo(handSide).FingerUniversalLocalAxes;
            UxrFingerDescriptor   fingerDescriptor = handDescriptor.GetFinger(fingerType);

            HasMetacarpalInfo = fingerDescriptor.HasMetacarpalInfo && avatarFinger.Metacarpal != null;

            Quaternion metacarpalWorldRotation;
            Quaternion proximalWorldRotation;

            // Compute world rotations

            if (HasMetacarpalInfo)
            {
                metacarpalWorldRotation = GetRotation(avatarHand.Wrist,        avatarFinger.Metacarpal, fingerDescriptor.Metacarpal, handLocalAxes,   fingerLocalAxes);
                proximalWorldRotation   = GetRotation(avatarFinger.Metacarpal, avatarFinger.Proximal,   fingerDescriptor.Proximal,   fingerLocalAxes, fingerLocalAxes);
            }
            else
            {
                metacarpalWorldRotation = Quaternion.identity;
                proximalWorldRotation   = GetRotation(avatarHand.Wrist, avatarFinger.Proximal, fingerDescriptor.ProximalNoMetacarpal, handLocalAxes, fingerLocalAxes);
            }

            Quaternion intermediateWorldRotation = GetRotation(avatarFinger.Proximal,     avatarFinger.Intermediate, fingerDescriptor.Intermediate, fingerLocalAxes, fingerLocalAxes);
            Quaternion distalWorldRotation       = GetRotation(avatarFinger.Intermediate, avatarFinger.Distal,       fingerDescriptor.Distal,       fingerLocalAxes, fingerLocalAxes);

            // Compute relative rotations

            if (HasMetacarpalInfo)
            {
                MetacarpalRotation = Quaternion.Inverse(avatarHand.Wrist.rotation) * metacarpalWorldRotation;
                ProximalRotation   = Quaternion.Inverse(avatarFinger.Metacarpal.rotation) * proximalWorldRotation;
            }
            else
            {
                MetacarpalRotation = Quaternion.identity;
                ProximalRotation   = Quaternion.Inverse(avatarHand.Wrist.rotation) * proximalWorldRotation;
            }

            IntermediateRotation = Quaternion.Inverse(avatarFinger.Proximal.rotation) * intermediateWorldRotation;
            DistalRotation       = Quaternion.Inverse(avatarFinger.Intermediate.rotation) * distalWorldRotation;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="hasMetacarpalInfo">Whether the finger contains metacarpal information</param>
        /// <param name="metacarpalRotation">Metacarpal local rotation (optional)</param>
        /// <param name="proximalRotation">Proximal local rotation</param>
        /// <param name="intermediateRotation">Intermediate local rotation</param>
        /// <param name="distalRotation">Distal local rotation</param>
        public UxrRuntimeFingerDescriptor(bool hasMetacarpalInfo, Quaternion metacarpalRotation, Quaternion proximalRotation, Quaternion intermediateRotation, Quaternion distalRotation)
        {
            HasMetacarpalInfo    = hasMetacarpalInfo;
            MetacarpalRotation   = metacarpalRotation;
            ProximalRotation     = proximalRotation;
            IntermediateRotation = intermediateRotation;
            DistalRotation       = distalRotation;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Copies the data from another descriptor.
        /// </summary>
        /// <param name="fingerDescriptor">Descriptor to copy the data from</param>
        public void CopyFrom(UxrRuntimeFingerDescriptor fingerDescriptor)
        {
            if (fingerDescriptor == null)
            {
                return;
            }

            HasMetacarpalInfo    = fingerDescriptor.HasMetacarpalInfo;
            MetacarpalRotation   = fingerDescriptor.MetacarpalRotation;
            ProximalRotation     = fingerDescriptor.ProximalRotation;
            IntermediateRotation = fingerDescriptor.IntermediateRotation;
            DistalRotation       = fingerDescriptor.DistalRotation;
        }

        /// <summary>
        ///     Interpolates towards another runtime finger descriptor.
        /// </summary>
        /// <param name="fingerDescriptor">Runtime finger descriptor</param>
        /// <param name="blend">Interpolation value [0.0, 1.0]</param>
        public void InterpolateTo(UxrRuntimeFingerDescriptor fingerDescriptor, float blend)
        {
            if (fingerDescriptor == null)
            {
                return;
            }

            if (HasMetacarpalInfo && fingerDescriptor.HasMetacarpalInfo)
            {
                MetacarpalRotation = Quaternion.Slerp(MetacarpalRotation, fingerDescriptor.MetacarpalRotation, blend);
            }

            ProximalRotation     = Quaternion.Slerp(ProximalRotation,     fingerDescriptor.ProximalRotation,     blend);
            IntermediateRotation = Quaternion.Slerp(IntermediateRotation, fingerDescriptor.IntermediateRotation, blend);
            DistalRotation       = Quaternion.Slerp(DistalRotation,       fingerDescriptor.DistalRotation,       blend);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the local rotation of a <see cref="UxrFingerDescriptor" /> when applied to an object.
        /// </summary>
        /// <param name="parent">Parent the node descriptor references its rotation to</param>
        /// <param name="node">Transform to get the local rotation of</param>
        /// <param name="nodeDescriptor">
        ///     Bone information in the well-known coordinate system of a <see cref="UxrHandPoseAsset" />
        /// </param>
        /// <param name="parentLocalAxes">Coordinate system of the <paramref name="parent" /> transform</param>
        /// <param name="nodeLocalAxes">Coordinate system of the <paramref name="node" /> transform</param>
        /// <returns>
        ///     Local rotation that should be applied to <paramref name="node" /> when using
        ///     <paramref name="nodeDescriptor" />
        /// </returns>
        private static Quaternion GetRotation(Transform parent, Transform node, UxrFingerNodeDescriptor nodeDescriptor, UxrUniversalLocalAxes parentLocalAxes, UxrUniversalLocalAxes nodeLocalAxes)
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

            return nodeUniversalMatrix.rotation * nodeUniversalToActual;
        }

        #endregion
    }
}