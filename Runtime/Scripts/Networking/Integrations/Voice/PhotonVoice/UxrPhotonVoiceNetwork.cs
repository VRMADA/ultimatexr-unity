// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPhotonVoiceNetwork.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.Unity;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ULTIMATEXR_USE_PHOTONFUSION_SDK
using Fusion;
#endif
#if ULTIMATEXR_USE_PHOTONVOICE_SDK
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
#endif


namespace UltimateXR.Networking.Integrations.Voice.PhotonVoice
{
    /// <summary>
    ///     Implementation of networking voice support using Photon Fusion.
    /// </summary>
    public class UxrPhotonVoiceNetwork : UxrNetworkVoiceImplementation
    {
        #region Public Overrides UxrNetworkVoiceImplementation

        /// <inheritdoc />
        public override string SdkName => UxrConstants.SdkPhotonVoice;

        /// <inheritdoc />
        public override IEnumerable<string> CompatibleNetworkSDKs
        {
            get
            {
                yield return UxrConstants.SdkPhotonFusion;
            }
        }

        /// <inheritdoc />
        public override void SetupGlobal(string networkingSdk, UxrNetworkManager networkManager, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_PHOTONFUSION_SDK && ULTIMATEXR_USE_PHOTONVOICE_SDK && UNITY_EDITOR

            Component runner = networkManager.CreatedGlobalComponents.FirstOrDefault(g => g.GetComponent<NetworkRunner>() != null);

            if (runner)
            {
                GameObject recorderObject = new GameObject("Recorder");
                Undo.RegisterCreatedObjectUndo(recorderObject, "Create Photon Voice Support GameObject");
                Undo.SetTransformParent(recorderObject.transform, runner.transform, "Parent Photon Voice GameObject");
                recorderObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                FusionVoiceClient voiceClientComponent = Undo.AddComponent<FusionVoiceClient>(runner.gameObject);
                Recorder          recorderComponent    = Undo.AddComponent<Recorder>(recorderObject);

                voiceClientComponent.UseFusionAppSettings = true;
                voiceClientComponent.UseFusionAuthValues  = true;
                voiceClientComponent.PrimaryRecorder      = recorderComponent;

                Undo.RegisterFullObjectHierarchyUndo(runner.gameObject, "Setup Photon GameObject");

                newGameObjects.Add(recorderObject);
                newComponents.Add(recorderComponent);
                newComponents.Add(voiceClientComponent);
            }
            else
            {
                Debug.LogError($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonVoiceNetwork)}.{nameof(SetupGlobal)} Cannot find {nameof(NetworkRunner)} to set up.");
            }

#endif
        }

        /// <inheritdoc />
        public override void SetupAvatar(string networkingSdk, UxrAvatar avatar, out List<GameObject> newGameObjects, out List<Component> newComponents)
        {
            newGameObjects = new List<GameObject>();
            newComponents  = new List<Component>();

#if ULTIMATEXR_USE_PHOTONFUSION_SDK && ULTIMATEXR_USE_PHOTONVOICE_SDK && UNITY_EDITOR

            Camera cameraComponent = avatar.CameraComponent;

            if (cameraComponent != null)
            {
                GameObject photonVoice = new GameObject("PhotonVoice");
                Undo.RegisterCreatedObjectUndo(photonVoice, "Create Photon Voice Support GameObject");
                Undo.SetTransformParent(photonVoice.transform, cameraComponent.transform, "Parent Photon Voice GameObject");
                photonVoice.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                Component voiceNetworkObjectComponent = avatar.GetOrAddComponent<VoiceNetworkObject>();
                Component speakerComponent            = photonVoice.GetOrAddComponent<Speaker>();
                Component audioSourceComponent        = photonVoice.GetOrAddComponent<AudioSource>();

                Undo.RegisterCompleteObjectUndo(avatar.gameObject, "Setup Photon Voice");
                Undo.RegisterFullObjectHierarchyUndo(cameraComponent.gameObject, "Setup Photon Voice");
                
                newGameObjects.Add(photonVoice);

                newComponents.Add(voiceNetworkObjectComponent);
                newComponents.Add(speakerComponent);
                newComponents.Add(audioSourceComponent);
            }
            else if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Errors)
            {
                Debug.LogError($"{UxrConstants.NetworkingModule} {nameof(UxrPhotonVoiceNetwork)}.{nameof(SetupAvatar)} Cannot find {nameof(Camera)} on avatar to set up voice components.");
            }

#endif
        }
        
        #endregion
    }
}