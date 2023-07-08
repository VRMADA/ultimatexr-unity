using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ULTIMATEXR_USE_WEBXR_SDK
using WebXR;
#endif
namespace UltimateXR.Devices.Integrations.WebXR
{
    public class UxrWebXRCamera : MonoBehaviour
    {
#if ULTIMATEXR_USE_WEBXR_SDK
        [SerializeField]
        private Camera cameraMain = null;
        private Camera cameraRight;
        private Rect defaultRect;
        [SerializeField]
        private Transform cameraFollower = null;

        private WebXRState xrState = WebXRState.NORMAL;

        private bool hasFollower = false;

        private static int _viewCount = 0;
        public static Rect _leftRect, _rightRect;
        private void Start()
        {
            defaultRect = cameraMain.rect;
            if(WebXRManager.Instance == null)
            {
                var webXrManager = Instantiate(new GameObject());
                webXrManager.name = "WebXRManager";
                webXrManager.AddComponent<WebXRManager>();
            }
            xrState = WebXRManager.Instance.XRState;

            SwitchXRState();
        }

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;
            hasFollower = cameraFollower != null;
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
        }

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

        public Quaternion GetLocalRotation()
        {
            return cameraMain.transform.localRotation;
        }

        public Vector3 GetLocalPosition()
        {
            switch (xrState)
            {
                case WebXRState.VR:
                    return (cameraMain.transform.localPosition + cameraRight.transform.localPosition) * 0.5f;
            }
            return cameraMain.transform.localPosition;
        }
        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            _viewCount = viewsCount;
            _leftRect = leftRect;
            _rightRect = rightRect;

            SwitchXRState();
        }

        private void OnHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix,
            Quaternion leftRotation,
            Quaternion rightRotation,
            Vector3 leftPosition,
            Vector3 rightPosition)
        {
            if (xrState == WebXRState.VR)
            {
                cameraMain.transform.localPosition = leftPosition;
                cameraMain.transform.localRotation = leftRotation;
                cameraMain.projectionMatrix = leftProjectionMatrix;
                cameraRight.transform.localPosition = rightPosition;
                cameraRight.transform.localRotation = rightRotation;
                cameraRight.projectionMatrix = rightProjectionMatrix;
            }
        }
#endif
    }
}
