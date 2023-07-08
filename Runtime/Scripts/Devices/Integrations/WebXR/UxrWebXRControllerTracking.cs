using System.Collections;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEngine;
#if ULTIMATEXR_USE_WEBXR_SDK
using WebXR;
#endif

namespace UltimateXR.Devices.Integrations.WebXR
{
    public abstract class UxrWebXRControllerTracking : UxrControllerTracking
    {
#if ULTIMATEXR_USE_WEBXR_SDK
        [SerializeField] WebXRController leftWebXRController, rightWebXRController;
#endif
        /// <summary>
        ///     Gets SDK dependency. WebXR tracking devices require WebXR SDK installed.
        /// </summary>
        public override string SDKDependency => "WebXR";

        protected override void Awake()
        {
            base.Awake();
#if UNITY_WEBGL && !UNITY_EDITOR
            enabled = true;
#endif
        }
        protected override void OnEnable()
        {
            base.OnEnable();
#if ULTIMATEXR_USE_WEBXR_SDK
            WebXRManager.OnControllerUpdate += WebXRManager_OnControllerUpdate;
#endif
        }
        protected override void OnDisable()
        {
            base.OnDisable();
#if ULTIMATEXR_USE_WEBXR_SDK
            WebXRManager.OnControllerUpdate -= WebXRManager_OnControllerUpdate;
#endif
        }
#if ULTIMATEXR_USE_WEBXR_SDK
        private void WebXRManager_OnControllerUpdate(WebXRControllerData controllerData)
        {
            if (controllerData.hand == (int)WebXRControllerHand.LEFT)
            {
                UpdateSensor(UxrHandSide.Left, controllerData.position, controllerData.rotation);
            }
            else if (controllerData.hand == (int)WebXRControllerHand.RIGHT)
            {
                UpdateSensor(UxrHandSide.Right, controllerData.position, controllerData.rotation);
            }
        }
#endif
    }
}
