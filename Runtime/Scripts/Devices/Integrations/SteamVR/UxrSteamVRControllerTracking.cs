// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSteamVRControllerTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
#if ULTIMATEXR_USE_STEAMVR_SDK
using Valve.VR;
#endif

namespace UltimateXR.Devices.Integrations.SteamVR
{
    /// <summary>
    ///     Base class for tracking devices that use SteamVR.
    /// </summary>
    public abstract class UxrSteamVRControllerTracking : UxrControllerTracking
    {
        #region Public Overrides UxrTrackingDevice

        /// <summary>
        ///     Gets SDK dependency. SteamVR tracking devices require SteamVR SDK installed.
        /// </summary>
        public override string SDKDependency => UxrManager.SdkSteamVR;

        #endregion

        #region MonoBehaviour

        /// <summary>
        ///     Subscribes to SteamVR pose update events
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

#if ULTIMATEXR_USE_STEAMVR_SDK
            global::Valve.VR.SteamVR.Initialize();

            if (global::Valve.VR.SteamVR.initializedState is global::Valve.VR.SteamVR.InitializedStates.Initializing or global::Valve.VR.SteamVR.InitializedStates.InitializeSuccess)
            {
                if (poseAction != null)
                {
                    poseAction[SteamVR_Input_Sources.LeftHand].onUpdate += PoseAction_OnUpdate;
                    poseAction[SteamVR_Input_Sources.RightHand].onUpdate += PoseAction_OnUpdate;
                }
            }
#endif
        }

        /// <summary>
        ///     Subscribes from SteamVR pose update events
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

#if ULTIMATEXR_USE_STEAMVR_SDK
            if (global::Valve.VR.SteamVR.initializedState is global::Valve.VR.SteamVR.InitializedStates.Initializing or global::Valve.VR.SteamVR.InitializedStates.InitializeSuccess)
            {
                if (poseAction != null)
                {
                    poseAction[SteamVR_Input_Sources.LeftHand].onUpdate -= PoseAction_OnUpdate;
                    poseAction[SteamVR_Input_Sources.RightHand].onUpdate -= PoseAction_OnUpdate;
                }
            }
#endif
        }

        #endregion

        #region Event Handling Methods

#if ULTIMATEXR_USE_STEAMVR_SDK
        /// <summary>
        ///     Handles the pose action update
        /// </summary>
        /// <param name="fromAction">The action that triggered the event</param>
        /// <param name="fromSource">The source that was updated</param>
        private void PoseAction_OnUpdate(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            if (fromSource == SteamVR_Input_Sources.LeftHand)
            {
                UpdateSensor(UxrHandSide.Left, poseAction[fromSource].localPosition, poseAction[fromSource].localRotation);
            }
            else if (fromSource == SteamVR_Input_Sources.RightHand)
            {
                UpdateSensor(UxrHandSide.Right, poseAction[fromSource].localPosition, poseAction[fromSource].localRotation);
            }
        }

#endif

        #endregion

        #region Private types & Data

#if ULTIMATEXR_USE_STEAMVR_SDK
        private readonly SteamVR_Action_Pose poseAction = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");
#endif

        #endregion
    }
}