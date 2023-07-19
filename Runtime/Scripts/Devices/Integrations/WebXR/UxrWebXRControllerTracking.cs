using System.Collections;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Extensions.Unity;
using UnityEngine;
#if ULTIMATEXR_USE_WEBXR_SDK
using WebXR;
#endif

namespace UltimateXR.Devices.Integrations.WebXR
{
    public abstract class UxrWebXRControllerTracking : UxrControllerTracking
    {
#if ULTIMATEXR_USE_WEBXR_SDK
        private Camera cameraMain = null;
        private Camera cameraRight;
        private Rect defaultRect;
        
        [SerializeField]
        private Transform cameraFollower = null;
        private WebXRState xrState = WebXRState.NORMAL;

        private bool hasFollower = false;
        public static Rect _leftRect, _rightRect;
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
            cameraMain = Avatar.CameraComponent;
            defaultRect = cameraMain.rect;
            xrState = WebXRManager.Instance.XRState;
            hasFollower = cameraFollower != null;
            SwitchXRState();

            WebXRManager.OnControllerUpdate += WebXRManager_OnControllerUpdate;
            WebXRManager.OnHeadsetUpdate += WebXRManager_OnHeadsetUpdate;
            WebXRManager.OnXRChange += OnXRChange;
#endif
        }
        protected override void OnDisable()
        {
            base.OnDisable();
#if ULTIMATEXR_USE_WEBXR_SDK
            WebXRManager.OnControllerUpdate -= WebXRManager_OnControllerUpdate;
            WebXRManager.OnHeadsetUpdate -= WebXRManager_OnHeadsetUpdate;
            WebXRManager.OnXRChange -= OnXRChange;
#endif
        }
#if ULTIMATEXR_USE_WEBXR_SDK
        private void Update()
        {
            UpdateFollower();
        }

        private void SwitchXRState()
        {
            switch (xrState)
            {
                case WebXRState.VR:
                    cameraRight = Instantiate(cameraMain, cameraMain.transform.parent);
                    cameraMain.rect = _leftRect;
                    cameraRight.rect = _rightRect;
                    break;
                case WebXRState.NORMAL:
                    if (cameraRight != null) Destroy(cameraRight.gameObject);
                    cameraMain.rect = defaultRect;
                    break;
            }
        }

        private void UpdateFollower()
        {
            if (!hasFollower)
            {
                return;
            }
            switch (xrState)
            {
                case WebXRState.VR:
                    cameraFollower.localRotation = cameraMain.transform.localRotation;
                    if (cameraRight) cameraFollower.localPosition = (cameraMain.transform.localPosition + cameraRight.transform.localPosition) * 0.5f;
                    else cameraFollower.localPosition = cameraMain.transform.localPosition;
                    return;
            }
            cameraFollower.localRotation = cameraMain.transform.localRotation;
            cameraFollower.localPosition = cameraMain.transform.localPosition;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            _leftRect = leftRect;
            _rightRect = rightRect;

            SwitchXRState();
        }

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

        private void WebXRManager_OnHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix, 
            Matrix4x4 rightProjectionMatrix, 
            Quaternion leftRotation, 
            Quaternion rightRotation, 
            Vector3 leftPosition, 
            Vector3 rightPosition)
        {
            //Update camera position and rotation
            if (xrState == WebXRState.VR)
            {
                cameraMain.transform.SetLocalPositionAndRotation(leftPosition, leftRotation);
                cameraMain.projectionMatrix = leftProjectionMatrix;
                cameraRight.transform.SetLocalPositionAndRotation(rightPosition, rightRotation);
                cameraRight.projectionMatrix = rightProjectionMatrix;
            }
        }
#endif
    }
}
