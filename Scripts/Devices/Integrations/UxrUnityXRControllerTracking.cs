// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUnityXRControllerTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace UltimateXR.Devices.Integrations
{
    /// <summary>
    ///     Base class for tracking devices based on OpenVR.
    /// </summary>
    public abstract class UxrUnityXRControllerTracking : UxrControllerTracking
    {
        #region Protected Overrides UxrTrackingDevice

        /// <inheritdoc />
        protected override void UpdateSensors()
        {
            base.UpdateSensors();

            if (Avatar.CameraComponent == null)
            {
                Debug.LogWarning("No camera has been setup for this avatar");
                return;
            }

            List<XRNodeState> nodeStates = new List<XRNodeState>();
            InputTracking.GetNodeStates(nodeStates);

            foreach (XRNodeState nodeState in nodeStates)
            {
                if (nodeState.nodeType == XRNode.LeftHand)
                {
                    nodeState.TryGetRotation(out Quaternion localAvatarLeftHandSensorRot);
                    nodeState.TryGetPosition(out Vector3 localAvatarLeftHandSensorPos);

                    LocalAvatarLeftHandSensorRot = localAvatarLeftHandSensorRot;
                    LocalAvatarLeftHandSensorPos = localAvatarLeftHandSensorPos;
                }
                else if (nodeState.nodeType == XRNode.RightHand)
                {
                    nodeState.TryGetRotation(out Quaternion localAvatarRightHandSensorRot);
                    nodeState.TryGetPosition(out Vector3 localAvatarRightHandSensorPos);

                    LocalAvatarRightHandSensorRot = localAvatarRightHandSensorRot;
                    LocalAvatarRightHandSensorPos = localAvatarRightHandSensorPos;
                }
            }
        }

        #endregion
    }
}