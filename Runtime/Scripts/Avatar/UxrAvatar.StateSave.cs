// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatar.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.StateSave;

namespace UltimateXR.Avatar
{
    public partial class UxrAvatar
    {
        #region Public Overrides UxrComponent

        /// <inheritdoc />
        public override UxrTransformSpace TransformStateSaveSpace => GetLocalTransformIfParentedOr(UxrTransformSpace.World);

        /// <inheritdoc />
        public override bool RequiresTransformSerialization(UxrStateSaveLevel level)
        {
            return true;
        }

        #endregion

        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeStateInternal(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeStateInternal(isReading, stateSerializationVersion, level, options);

            // TODO: Figure out how to avoid cheating by saving UxrCameraWallFade state too.

            SerializeStateTransform(level, options, "cam.tf",   UxrTransformSpace.Avatar, CameraComponent.transform);
            SerializeStateTransform(level, options, "left.tf",  UxrTransformSpace.Avatar, LeftHandBone);
            SerializeStateTransform(level, options, "right.tf", UxrTransformSpace.Avatar, RightHandBone);

            // Controller and hand poses are already handled through events, we don't serialize them in incremental changes

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
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

        #endregion
    }
}