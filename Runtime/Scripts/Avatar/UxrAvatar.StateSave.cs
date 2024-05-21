// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatar.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Linq;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core;
using UltimateXR.Core.StateSave;
using UltimateXR.Devices;

namespace UltimateXR.Avatar
{
    public partial class UxrAvatar
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override UxrTransformSpace TransformStateSaveSpace => GetLocalTransformIfParentedOr(UxrTransformSpace.World);

        /// <inheritdoc />
        protected override bool RequiresTransformSerialization(UxrStateSaveLevel level)
        {
            return true;
        }

        /// <inheritdoc />
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            // TODO: Figure out how to avoid cheating by saving UxrCameraWallFade state too.

            SerializeStateTransform(level, options, CamTransformName,       UxrTransformSpace.Avatar, CameraComponent.transform);
            SerializeStateTransform(level, options, LeftHandTransformName,  UxrTransformSpace.Avatar, LeftHandBone);
            SerializeStateTransform(level, options, RightHandTransformName, UxrTransformSpace.Avatar, RightHandBone);

            // Controller and hand poses are already handled through events, we don't serialize them in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                // Avatar render mode

                SerializeStateValue(level, options, nameof(_renderMode), ref _renderMode);

                // We serialize the controller input 

                SerializeStateValue(level, options, null, ref _externalControllerInput);

                // Hand poses

                string leftPoseName  = GetCurrentRuntimeHandPose(UxrHandSide.Left)?.PoseName;
                string rightPoseName = GetCurrentRuntimeHandPose(UxrHandSide.Right)?.PoseName;

                float leftBlendValue  = GetCurrentHandPoseBlendValue(UxrHandSide.Left);
                float rightBlendValue = GetCurrentHandPoseBlendValue(UxrHandSide.Right);

                SerializeStateValue(level, options, "leftPose",   ref leftPoseName);
                SerializeStateValue(level, options, "rightPose",  ref rightPoseName);
                SerializeStateValue(level, options, "leftBlend",  ref leftBlendValue);
                SerializeStateValue(level, options, "rightBlend", ref rightBlendValue);

                if (isReading)
                {
                    // Render mode

                    SetAvatarRenderMode(_renderMode, UxrControllerInput.GetComponents(this).ToList());

                    // When deserializing, we need to manually set the hand pose state from the serialized data.

                    if (leftPoseName != null)
                    {
                        SetCurrentHandPose(UxrHandSide.Left, leftPoseName, leftBlendValue);
                    }

                    if (rightPoseName != null)
                    {
                        SetCurrentHandPose(UxrHandSide.Right, rightPoseName, rightBlendValue);
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override UxrVarInterpolator GetInterpolator(string varName)
        {
            if (IsTransformPositionVarName(varName, CamTransformName))
            {
                return _camPosInterpolator;
            }
            if (IsTransformRotationVarName(varName, CamTransformName))
            {
                return _camRotInterpolator;
            }
            if (IsTransformPositionVarName(varName, LeftHandTransformName))
            {
                return _leftHandPosInterpolator;
            }
            if (IsTransformRotationVarName(varName, LeftHandTransformName))
            {
                return _leftHandRotInterpolator;
            }
            if (IsTransformPositionVarName(varName, RightHandTransformName))
            {
                return _rightHandPosInterpolator;
            }
            if (IsTransformRotationVarName(varName, RightHandTransformName))
            {
                return _rightHandRotInterpolator;
            }

            // Null means using the default interpolator for the type
            return null;
        }

        /// <inheritdoc />
        protected override void InterpolateState(in UxrStateInterpolationVars vars, float t)
        {
            base.InterpolateState(in vars, t);

            InterpolateStateTransform(vars, t, CamTransformName,       CameraComponent.transform, UxrTransformSpace.Avatar);
            InterpolateStateTransform(vars, t, LeftHandTransformName,  LeftHandBone,              UxrTransformSpace.Avatar);
            InterpolateStateTransform(vars, t, RightHandTransformName, RightHandBone,             UxrTransformSpace.Avatar);
        }

        #endregion

        #region Private Types & Data

        private const string CamTransformName       = "cam.tf";
        private const string LeftHandTransformName  = "left.tf";
        private const string RightHandTransformName = "right.tf";

        private const float SmoothPosInterpolation = 0.3f;
        private const float SmoothRotInterpolation = 0.3f;

        private readonly UxrVector3Interpolator    _camPosInterpolator       = new UxrVector3Interpolator(SmoothPosInterpolation);
        private readonly UxrQuaternionInterpolator _camRotInterpolator       = new UxrQuaternionInterpolator(SmoothRotInterpolation);
        private readonly UxrVector3Interpolator    _leftHandPosInterpolator  = new UxrVector3Interpolator(SmoothPosInterpolation);
        private readonly UxrQuaternionInterpolator _leftHandRotInterpolator  = new UxrQuaternionInterpolator(SmoothRotInterpolation);
        private readonly UxrVector3Interpolator    _rightHandPosInterpolator = new UxrVector3Interpolator(SmoothPosInterpolation);
        private readonly UxrQuaternionInterpolator _rightHandRotInterpolator = new UxrQuaternionInterpolator(SmoothRotInterpolation);

        #endregion
    }
}