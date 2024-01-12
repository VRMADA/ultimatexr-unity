// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrClientNetworkTransform.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#if ULTIMATEXR_USE_UNITY_NETCODE
using Unity.Netcode.Components;
#else
using UnityEngine;
#endif

namespace UltimateXR.Networking.Integrations.Net.UnityNetCode
{
#if ULTIMATEXR_USE_UNITY_NETCODE
    public class UxrClientNetworkTransform : NetworkTransform
    {
        /// <summary>
        ///     Behave like a NetworkTransform but authority is the owner, not the server.
        /// </summary>
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
#else
    public class UxrClientNetworkTransform : MonoBehaviour
    {
    }
#endif
}