// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPhotonFusionNetwork.RigInput.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
using Fusion;
using UnityEngine;

namespace UltimateXR.Networking.Integrations.Net.PhotonFusion
{
    public partial class UxrPhotonFusionNetwork
    {
        #region Public Types & Data

        /// <summary>
        ///     Stores all the input information that describes an avatar to be used in client/server mode.
        /// </summary>
        public struct RigInput : INetworkInput
        {
            #region Public Types & Data

            public Vector3    avatarPosition;
            public Quaternion avatarRotation;
            public Vector3    avatarScale;
            public Vector3    cameraPosition;
            public Quaternion cameraRotation;
            public Vector3    leftHandPosition;
            public Quaternion leftHandRotation;
            public Vector3    rightHandPosition;
            public Quaternion rightHandRotation;

            #endregion
        }

        #endregion
    }
}
#endif