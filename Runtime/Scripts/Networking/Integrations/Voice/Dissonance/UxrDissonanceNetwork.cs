// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrDissonanceNetwork.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;
#if ULTIMATEXR_USE_DISSONANCE_SDK && UNITY_EDITOR
using UnityEditor;
#endif
#if ULTIMATEXR_USE_DISSONANCE_SDK
using UltimateXR.Extensions.Unity;
using Dissonance;
#endif

namespace UltimateXR.Networking.Integrations.Voice.Dissonance
{
    /// <summary>
    ///     Implementation of networking voice support using Dissonance.
    /// </summary>
    public class UxrDissonanceNetwork : UxrNetworkVoiceImplementation
    {
        #region Public Overrides UxrNetworkVoiceImplementation

        /// <inheritdoc />
        public override string SdkName => UxrConstants.SdkDissonance;

        /// <inheritdoc />
        public override IEnumerable<string> CompatibleNetworkSDKs
        {
            get
            {
                yield return UxrConstants.SdkFishNet;
                yield return UxrConstants.SdkMirror;
                yield return UxrConstants.SdkPhotonFusion;
                yield return UxrConstants.SdkUnityNetCode;
            }
        }

        /// <inheritdoc />
        public override void SetupGlobal(string networkingSdk, UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_DISSONANCE_SDK && UNITY_EDITOR

            if (string.IsNullOrEmpty(networkingSdk))
            {
                return;
            }

            string     setupPrefabGuid = null;
            GameObject setupInstance   = null;

            if (string.Equals(networkingSdk, UxrConstants.SdkFishNet))
            {
                Debug.LogWarning($"{UxrConstants.NetworkingModule} FishNet Dissonance integration package doesn't come with a prefab and components should be added manually. We're working on a pull request to add integration seamlessly.");
            }
            else if (string.Equals(networkingSdk, UxrConstants.SdkMirror))
            {
                setupPrefabGuid = "1264c01c7f8182e47ac9f784af03d895";
            }
            else if (string.Equals(networkingSdk, UxrConstants.SdkPhotonFusion))
            {
                setupPrefabGuid = "803e2767acc738a4498f245ae19bb598";
            }
            else if (string.Equals(networkingSdk, UxrConstants.SdkUnityNetCode))
            {
                setupPrefabGuid = "2c50758a6d3b8114a8ce30a2fd9e4380";
            }

            if (setupPrefabGuid != null)
            {
                setupInstance = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(setupPrefabGuid))) as GameObject;

                if (setupInstance == null)
                {
                    Debug.LogError($"{UxrConstants.NetworkingModule} Could not find the {UxrConstants.SdkDissonance} setup prefab for {networkingSdk}. Check for the {networkingSdk} integration here: https://placeholder-software.co.uk/dissonance/docs/Basics/Getting-Started.html");
                   
                }
                else
                {
                    Undo.RegisterCreatedObjectUndo(setupInstance, "Create Dissonance GameObject");
                }
            }

            if (setupInstance != null)
            {
                VoiceBroadcastTrigger broadcastTrigger = Undo.AddComponent<VoiceBroadcastTrigger>(setupInstance);
                VoiceReceiptTrigger   receiptTrigger   = Undo.AddComponent<VoiceReceiptTrigger>(setupInstance);

                broadcastTrigger.ChannelType = CommTriggerTarget.Room;
                broadcastTrigger.RoomName    = "Global";
                receiptTrigger.RoomName      = "Global";

                Undo.RegisterFullObjectHierarchyUndo(setupInstance, "Setup Dissonance GameObject");

                newGameObjects.Add(setupInstance);
                newComponents.Add(broadcastTrigger);
                newComponents.Add(receiptTrigger);
            }
#endif
        }

        /// <inheritdoc />
        public override void SetupAvatar(string networkingSdk, UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

            // No setup required
        }
        
        #endregion
    }
}