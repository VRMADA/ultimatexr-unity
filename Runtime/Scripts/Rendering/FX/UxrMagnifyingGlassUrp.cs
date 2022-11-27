// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrMagnifyingGlassUrp.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Devices;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

#if ULTIMATEXR_UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace UltimateXR.Rendering.FX
{
    /// <summary>
    ///     Component that renders a magnifying glass effect on an object, using the URP pipeline:
    ///     <list type="bullet">
    ///         <item>
    ///             If the Glass Axes transform is not set, it will use the transform on the component's GameObject.
    ///         </item>
    ///         <item>
    ///             The magnifying glass normal is determined by the -forward axis, so the user will look through the glass
    ///             pointing in the forward axis direction.
    ///         </item>
    ///         <item>
    ///             The component requires a Renderer on the same GameObject with a material compatible with the URP magnifying
    ///             glass refraction. They can be found in the UltimateXR/FX/ category.
    ///         </item>
    ///     </list>
    /// </summary>
    public class UxrMagnifyingGlassUrp : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        // Inspector

        [SerializeField]                        private bool      _forceClearSkyBox;
        [SerializeField]                        private Transform _glassAxes;
        [SerializeField]                        private bool      _disablePixelLights = true;
        [SerializeField]                        private int       _textureSize        = 1024;
        [SerializeField]                        private int       _antialias          = 1;
        [SerializeField]                        private float     _clipPlaneOffset    = 0.01f;
        [SerializeField]                        private float     _cameraForwardOffset;
        [SerializeField] [Range(0.5f,   3.0f)]  private float     _fovScale  = 1.0f;
        [SerializeField] [Range(-10.0f, 10.0f)] private float     _ipdAdjust = 1.0f;
        [SerializeField] [Range(-0.15f, 0.15f)] private float     _offsetLeft;
        [SerializeField] [Range(-0.15f, 0.15f)] private float     _offsetRight;
        [SerializeField]                        private LayerMask _layers = -1;

        #endregion

        #region Unity

        /// <summary>
        ///     Frees the allocated resources.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_refractionCamera)
            {
                Destroy(_refractionCamera.gameObject);
            }

            if (_renderTextureLeft)
            {
                DestroyImmediate(_renderTextureLeft);
                _renderTextureLeft = null;
            }

            if (_renderTextureRight)
            {
                DestroyImmediate(_renderTextureRight);
                _renderTextureRight = null;
            }
        }

        /// <summary>
        ///     Subscribes to the URP rendering event.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_BeginCameraRendering;
        }

        /// <summary>
        ///     Unsubscribes from the URP rendering event.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_BeginCameraRendering;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called by Unity when the rendering starts. It is used to render the magnifying glass refraction.
        /// </summary>
        private void RenderPipelineManager_BeginCameraRendering(ScriptableRenderContext context, Camera renderCamera)
        {
            if (UxrAvatar.LocalAvatarCamera != renderCamera)
            {
                return;
            }

            Renderer glassRenderer = GetComponent<Renderer>();

            if (!enabled || !glassRenderer || !glassRenderer.sharedMaterial || !glassRenderer.enabled)
            {
                return;
            }

            if (!renderCamera)
            {
                return;
            }

            // Avoid recursive rendering

            if (s_insideRendering)
            {
                return;
            }

            s_insideRendering = true;

            CreateResources(renderCamera, out Camera refractionCamera);

            if (!_refractionCamera)
            {
                return;
            }

            // Lower quality for refraction

            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (_disablePixelLights)
            {
                QualitySettings.pixelLightCount = 0;
            }

            CopyCameraData(renderCamera, refractionCamera);

            // Update parameters

            refractionCamera.cullingMask = ~(1 << 4) & _layers.value;

            if (TryGetComponent<Renderer>(out var theRenderer))
            {
                foreach (Material m in theRenderer.sharedMaterials)
                {
                    if (m.HasProperty(VarRenderTexLeft))
                    {
                        m.SetTexture(VarRenderTexLeft, _renderTextureLeft);
                    }

                    if (m.HasProperty(VarRenderTexRight))
                    {
                        m.SetTexture(VarRenderTexRight, _renderTextureRight);
                    }

                    if (m.HasProperty(VarGlassScreenCenter))
                    {
                        Vector3 glassScreenCenter = renderCamera.WorldToViewportPoint(_glassAxes.position);
                        //glassScreenCenter.y = 1.0f - glassScreenCenter.y;
                        m.SetVector(VarGlassScreenCenter, glassScreenCenter);
                    }
                }
            }

            // Render
            refractionCamera.enabled = true;

            refractionCamera.targetTexture = _renderTextureLeft;
            RenderRefraction(context, renderCamera, refractionCamera, true, true);

            refractionCamera.targetTexture = _renderTextureRight;
            RenderRefraction(context, renderCamera, refractionCamera, true, false);

            refractionCamera.enabled = false;

            // Restore quality

            if (_disablePixelLights)
            {
                QualitySettings.pixelLightCount = oldPixelLightCount;
            }

            s_insideRendering = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Renders the refraction.
        /// </summary>
        /// <param name="context">Render context of the scriptable render pipeline</param>
        /// <param name="renderCamera">Main camera</param>
        /// <param name="refractionCamera">Camera that will render refraction</param>
        /// <param name="stereo">Is stereo mode active?</param>
        /// <param name="isLeft">Is it the left eye in stereo mode?</param>
        private void RenderRefraction(ScriptableRenderContext context, Camera renderCamera, Camera refractionCamera, bool stereo, bool isLeft)
        {
            refractionCamera.ResetWorldToCameraMatrix();
            refractionCamera.ResetCullingMatrix();

            Matrix4x4 projection = renderCamera.projectionMatrix;

            if (stereo && UxrTrackingDevice.GetHeadsetDevice(out InputDevice headsetDevice))
            {
                headsetDevice.TryGetFeatureValue(CommonUsages.leftEyePosition,  out Vector3 leftEye);
                headsetDevice.TryGetFeatureValue(CommonUsages.rightEyePosition, out Vector3 rightEye);
                float ipd = Vector3.Distance(leftEye, rightEye) * _ipdAdjust;

                if (isLeft)
                {
                    refractionCamera.transform.SetPositionAndRotation(renderCamera.transform.position - 0.5f * ipd * renderCamera.transform.right, renderCamera.transform.rotation);
                    projection = renderCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                }
                else
                {
                    refractionCamera.transform.SetPositionAndRotation(renderCamera.transform.position + 0.5f * ipd * renderCamera.transform.right, renderCamera.transform.rotation);
                    projection = renderCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                }
            }
            else
            {
                refractionCamera.transform.SetPositionAndRotation(renderCamera.transform.position, renderCamera.transform.rotation);
            }

            refractionCamera.transform.position += renderCamera.transform.forward * _cameraForwardOffset;

            Vector4 clipPlane = CameraSpacePlane(refractionCamera, _clipPlaneOffset, _glassAxes.position, _glassAxes.forward, 1.0f);

            Vector3 screenPos = renderCamera.WorldToViewportPoint(_glassAxes.position, isLeft ? Camera.MonoOrStereoscopicEye.Left : Camera.MonoOrStereoscopicEye.Right);
            screenPos.x = (screenPos.x - 0.5f) * 2.0f + (isLeft ? _offsetLeft : _offsetRight);
            screenPos.y = (screenPos.y - 0.5f) * 2.0f;
            screenPos.z = 0.0f;
            Matrix4x4 translateMtxA = Matrix4x4.Translate(-screenPos);
            Matrix4x4 scaleMtx      = Matrix4x4.Scale(new Vector3(_fovScale, _fovScale, 0.3f));
            Matrix4x4 translateMtxB = Matrix4x4.Translate(screenPos);
            projection = translateMtxB * scaleMtx * translateMtxA * projection;

            projection                        = projection.GetObliqueMatrix(clipPlane);
            refractionCamera.projectionMatrix = projection;
            refractionCamera.cullingMatrix    = refractionCamera.projectionMatrix * refractionCamera.worldToCameraMatrix;

#if ULTIMATEXR_UNITY_URP
            UniversalRenderPipeline.RenderSingleCamera(context, refractionCamera);
#endif
        }

        /// <summary>
        ///     Copies data from one camera to another.
        /// </summary>
        /// <param name="src">Source data</param>
        /// <param name="dest">Destination data</param>
        private void CopyCameraData(Camera src, Camera dest)
        {
            if (dest == null)
            {
                return;
            }

            if (_forceClearSkyBox == false)
            {
                dest.clearFlags      = src.clearFlags;
                dest.backgroundColor = src.backgroundColor;

                if (src.clearFlags == CameraClearFlags.Skybox)
                {
                    Skybox srcSky = src.GetComponent(typeof(Skybox)) as Skybox;
                    Skybox dstSky = dest.GetComponent(typeof(Skybox)) as Skybox;

                    if (dstSky)
                    {
                        if (!srcSky || !srcSky.material)
                        {
                            dstSky.enabled = false;
                        }
                        else
                        {
                            dstSky.enabled  = true;
                            dstSky.material = srcSky.material;
                        }
                    }
                }
            }

            dest.farClipPlane  = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic  = src.orthographic;

            if (XRSettings.enabled == false)
            {
                dest.fieldOfView = src.fieldOfView;
            }

            dest.aspect           = src.aspect;
            dest.orthographicSize = src.orthographicSize;
        }

        /// <summary>
        ///     Allocates the resources.
        /// </summary>
        /// <param name="currentCamera">Render camera</param>
        /// <param name="refractionCamera">Refraction camera</param>
        private void CreateResources(Camera currentCamera, out Camera refractionCamera)
        {
            refractionCamera = null;

            // Render textures

            if (_oldRenderTextureSize != _textureSize)
            {
                CreateRenderTexture(ref _renderTextureLeft);
                CreateRenderTexture(ref _renderTextureRight);
                _oldRenderTextureSize = _textureSize;
            }

            if (_renderTextureLeft == null)
            {
                CreateRenderTexture(ref _renderTextureLeft);
            }

            if (_renderTextureRight == null)
            {
                CreateRenderTexture(ref _renderTextureRight);
            }

            // Refraction camera

            refractionCamera = _refractionCamera;

            if (!refractionCamera)
            {
                GameObject go = new GameObject($"{nameof(UxrMagnifyingGlassUrp)} Camera", typeof(Camera), typeof(Skybox));
                refractionCamera  = go.GetComponent<Camera>();
                _refractionCamera = refractionCamera;

                if (XRSettings.enabled == false)
                {
                    refractionCamera.fieldOfView = 60.0f;
                }

                refractionCamera.transform.SetPositionAndRotation(transform);
                refractionCamera.enabled = true;
                go.hideFlags             = HideFlags.HideAndDontSave;

                if (_forceClearSkyBox)
                {
                    refractionCamera.clearFlags = CameraClearFlags.Skybox;
                }

                refractionCamera.enabled = false;
            }
        }

        /// <summary>
        ///     Creates a render texture.
        /// </summary>
        /// <param name="texture">Texture to create</param>
        private void CreateRenderTexture(ref RenderTexture texture)
        {
            if (texture)
            {
                DestroyImmediate(texture);
            }

            texture = new RenderTexture(_textureSize, _textureSize, 0, RenderTextureFormat.Default);

            texture.antiAliasing     = _antialias;
            texture.name             = $"{nameof(UxrMagnifyingGlassUrp)} Texture";
            texture.isPowerOfTwo     = true;
            texture.hideFlags        = HideFlags.DontSave;
            texture.autoGenerateMips = true;
            texture.useMipMap        = true; // Mip-mapping can be used for blur
        }

        /// <summary>
        ///     Given a plane point and normal in world coordinates, computes the plane in camera space.
        /// </summary>
        /// <param name="targetCamera">Camera</param>
        /// <param name="offset">Clip plane offset</param>
        /// <param name="position">Point in plane</param>
        /// <param name="normal">Plane normal</param>
        /// <param name="sideSign">Plane side of the camera</param>
        /// <returns>Plane in camera space</returns>
        private Vector4 CameraSpacePlane(Camera targetCamera, float offset, Vector3 position, Vector3 normal, float sideSign)
        {
            Vector3   offsetPos           = position + normal * offset;
            Matrix4x4 worldToCameraMatrix = targetCamera.worldToCameraMatrix;
            Vector3   localPos            = worldToCameraMatrix.MultiplyPoint(offsetPos);
            Vector3   localNormal         = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(localNormal.x, localNormal.y, localNormal.z, -Vector3.Dot(localPos, localNormal));
        }

        #endregion

        #region Private Types & Data

        // Constants

        private const string VarRenderTexLeft     = "_RenderTexLeft";
        private const string VarRenderTexRight    = "_RenderTexRight";
        private const string VarGlassScreenCenter = "_GlassScreenCenter";

        // Static

        private static bool s_insideRendering;

        // Internal

        private Camera        _refractionCamera;
        private RenderTexture _renderTextureLeft;
        private RenderTexture _renderTextureRight;
        private int           _oldRenderTextureSize;

        #endregion
    }
}