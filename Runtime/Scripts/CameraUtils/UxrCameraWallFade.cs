// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrCameraWallFade.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Render;
using UltimateXR.Locomotion;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.CameraUtils
{
    /// <summary>
    ///     Component added to a camera that enables to fade the camera to black whenever the user tries to stick the head
    ///     inside geometry. It is used to prevent peeking through walls.
    ///     It is also often consulted by <see cref="UxrLocomotion" /> components to check whether the avatar can move around
    ///     in order to prevent cheating through walls.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class UxrCameraWallFade : UxrAvatarComponent<UxrCameraWallFade>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrWallFadeMode _mode = UxrWallFadeMode.AllowTraverse;
        [SerializeField] private LayerMask       _collisionLayers;
        [SerializeField] private bool            _ignoreTriggerColliders = true;
        [SerializeField] private bool            _ignoreDynamicObjects   = true;
        [SerializeField] private bool            _ignoreGrabbedObjects   = true;
        [SerializeField] private Color           _fadeColor;
        [SerializeField] private float           _fadeFarDistance  = 0.24f;
        [SerializeField] private float           _fadeNearDistance = 0.12f;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether the camera is currently inside a wall.
        /// </summary>
        public bool IsInsideWall { get; private set; }

        /// <summary>
        ///     Gets or sets the current working mode.
        /// </summary>
        public UxrWallFadeMode Mode
        {
            get => _mode;
            set
            {
                _mode                    = value;
                _lastValidPosInitialized = false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether an avatar is currently peeking through geometry. The camera object requires to have an
        ///     <see cref="UxrCameraFade" /> in order to work.
        /// </summary>
        /// <param name="avatar">The avatar to check</param>
        /// <returns>
        ///     Whether the avatar has an <see cref="UxrCameraFade" /> component and it currently detects the avatar is
        ///     peeking through geometry
        /// </returns>
        public static bool IsAvatarPeekingThroughGeometry(UxrAvatar avatar)
        {
            if (avatar == null)
            {
                return false;
            }
            
            UxrCameraWallFade wallFade = avatar.CameraComponent != null ? avatar.CameraComponent.GetComponent<UxrCameraWallFade>() : null;
            return wallFade && wallFade._quadObject.activeSelf; //.IsInsideWall;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            CreateCameraQuad();
        }

        /// <summary>
        ///     Subscribes to events. It also initializes the component so that whenever it is enabled, it is considered as being
        ///     "outside".
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _lastValidPosInitialized = false;

            UxrManager.AvatarMoved    += UxrManager_AvatarMoved;
            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarMoved    -= UxrManager_AvatarMoved;
            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever an avatar moved. The state is reset so that it is considered "outside".
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrManager_AvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            _lastValidPosInitialized = false;
        }

        /// <summary>
        ///     Called after all avatars have been updated. This is where the component is updated.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            UpdateFade();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the component using the current <see cref="UxrWallFadeMode" /> algorithm described by <see cref="Mode" />.
        /// </summary>
        private void UpdateFade()
        {
            if (_lastValidPosInitialized == false && Avatar != null && Avatar.transform.InverseTransformPoint(transform.position).y > CameraInitializationMinY)
            {
                // We assume the camera starts in a valid state
                _lastValidPosInitialized = true;
                _lastValidPos            = transform.position;
                _fadeAlpha               = 0.0f;
                IsInsideWall             = false;
            }

            if (_lastValidPosInitialized)
            {
                _fadeAlpha = 0.0f;

                // First check if we are inside a wall or not using the last valid position
                Vector3 cameraDeltaPos = transform.position - _lastValidPos;
                
                // We cast rays in both directions, from the last valid position to the current position. We will look for transitions
                // from inside to outside a wall or the other way around by looking at the number of intersections in both directions
                RaycastHit[] raycastHitsFromLasValidPos = Physics.RaycastAll(_lastValidPos,
                                                                             cameraDeltaPos.normalized,
                                                                             cameraDeltaPos.magnitude,
                                                                             _collisionLayers,
                                                                             _ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                RaycastHit[] raycastHitsToLastValidPos = Physics.RaycastAll(transform.position,
                                                                            -cameraDeltaPos.normalized,
                                                                            cameraDeltaPos.magnitude,
                                                                            _collisionLayers,
                                                                            _ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                int raycastCountFromLastValidPos = GetRaycastCount(raycastHitsFromLasValidPos);
                int raycastCountToLastValidPos   = GetRaycastCount(raycastHitsToLastValidPos);

                if (_mode == UxrWallFadeMode.AllowTraverse)
                {
                    if (IsInsideWall == false && raycastCountFromLastValidPos > raycastCountToLastValidPos)
                    {
                        // From outside a wall to inside a wall
                        IsInsideWall = true;
                    }
                    else if (IsInsideWall && raycastCountFromLastValidPos <= raycastCountToLastValidPos)
                    {
                        // From inside a wall to outside a wall
                        IsInsideWall = false;
                    }
                }
                else if (_mode == UxrWallFadeMode.Strict)
                {
                    IsInsideWall = raycastCountFromLastValidPos > 0 || raycastCountToLastValidPos > 0;
                }

                _fadeAlpha = IsInsideWall ? 1.0f : 0.0f;

                // If we are not inside a wall we will wrap the camera in a cylinder and get directions from which we will cast rays to check if they intersect with the scene
                // and fade the screen accordingly
                if (IsInsideWall == false)
                {
                    for (int heightSubdivision = 0; heightSubdivision < CameraCylinderVerticalSteps; ++heightSubdivision)
                    {
                        float height = Mathf.Lerp(-_fadeFarDistance, _fadeFarDistance, heightSubdivision / (CameraCylinderVerticalSteps - 1.0f));

                        for (int radiusIndex = 0; radiusIndex < CameraCylinderSides; ++radiusIndex)
                        {
                            float   offsetT   = 1.0f / CameraCylinderSides * 0.5f;
                            float   radians   = Mathf.PI * 2.0f * (radiusIndex * (1.0f / CameraCylinderSides) + offsetT);
                            Vector3 direction = new Vector3(Mathf.Cos(radians), height, Mathf.Sin(radians)).normalized;

                            RaycastHit[] raycastHits = Physics.RaycastAll(transform.position,
                                                                          transform.TransformDirection(direction),
                                                                          _fadeFarDistance,
                                                                          _collisionLayers,
                                                                          _ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                            if (GetClosestRaycast(raycastHits, out RaycastHit hit))
                            {
                                // We are close enough to a collider to start fading out
                                float interval = _fadeFarDistance - _fadeNearDistance;
                                float alpha = hit.distance < _fadeNearDistance ? 1.0f :
                                              interval > 0.0f                  ? 1.0f - (hit.distance - _fadeNearDistance) / interval : 1.0f;

                                if (alpha > _fadeAlpha)
                                {
                                    // We are close to a wall.
                                    _fadeAlpha = Mathf.Clamp01(alpha);
                                }
                            }
                        }
                    }
                }

                // Update last valid position if it is far enough from the wall to avoid floating point errors (we use 1cm threshold which is the minimum near clip plane for the camera)
                if (IsInsideWall == false)
                {
                    if (Physics.CheckSphere(transform.position,
                                            0.01f,
                                            _collisionLayers,
                                            _ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide) == false)
                    {
                        _lastValidPos = transform.position;
                    }
                }

                _quadObject.SetActive(_fadeAlpha > 0.0f);
                _fadeMaterial.color = _fadeColor.WithAlpha(_fadeAlpha);
            }
            else
            {
                _quadObject.SetActive(false);
                _fadeAlpha = 0.0f;
            }
        }

        /// <summary>
        ///     Creates the quad that is used to render the fullscreen fade.
        /// </summary>
        private void CreateCameraQuad()
        {
            Camera camera = GetComponent<Camera>();

            _quadObject = new GameObject("Fade");
            _quadObject.transform.SetParent(transform);
            _quadObject.transform.localPosition    = Vector3.forward * (camera.nearClipPlane + 0.01f);
            _quadObject.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);

            Mesh mesh = MeshExt.CreateQuad(2.0f);

            MeshFilter   meshFilter   = _quadObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = _quadObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = mesh;
            _fadeMaterial   = new Material(ShaderExt.UnlitOverlayFade);

            meshRenderer.sharedMaterial = _fadeMaterial;
            _quadObject.SetActive(false);
        }

        /// <summary>
        ///     Checks whether the given raycast collider hit is valid or should be ignored.
        /// </summary>
        /// <param name="colliderHit">Collider that was hit</param>
        /// <returns>Whether the given raycast collider hit is valid</returns>
        private bool IsValidRaycastHit(Collider colliderHit)
        {
            if (_ignoreDynamicObjects)
            {
                // Check for rigidbody and ignore if found
                if (colliderHit.gameObject.IsDynamic())
                {
                    return false;
                }
            }

            if (_ignoreGrabbedObjects)
            {
                UxrGrabbableObject grabbableObject = colliderHit.GetComponentInParent<UxrGrabbableObject>();

                if (grabbableObject && grabbableObject.IsBeingGrabbed)
                {
                    return false;
                }
            }

            return !colliderHit.gameObject.GetComponentInParent<UxrIgnoreWallFade>() && !colliderHit.gameObject.GetComponentInParent<UxrAvatar>();
        }

        /// <summary>
        ///     Gets the number of raycast hits that are valid from the given set.
        /// </summary>
        /// <param name="raycastHits">The set of raycast hits</param>
        /// <returns>The valid number of hits</returns>
        private int GetRaycastCount(RaycastHit[] raycastHits)
        {
            int count = 0;

            foreach (RaycastHit hit in raycastHits)
            {
                if (IsValidRaycastHit(hit.collider))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Gets the closest valid raycast hit from the set.
        /// </summary>
        /// <param name="raycastHits">The raycast to get the closest valid from</param>
        /// <param name="raycastHit">Returns the closest valid raycast</param>
        /// <returns>Whether a valid raycast was found</returns>
        private bool GetClosestRaycast(RaycastHit[] raycastHits, out RaycastHit raycastHit)
        {
            int   closestIndex    = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < raycastHits.Length; ++i)
            {
                if (IsValidRaycastHit(raycastHits[i].collider))
                {
                    // Keep closest hit

                    if (raycastHits[i].distance < closestDistance)
                    {
                        closestIndex    = i;
                        closestDistance = raycastHits[i].distance;
                    }
                }
            }

            if (closestIndex != -1)
            {
                raycastHit = raycastHits[closestIndex];
                return true;
            }

            raycastHit = new RaycastHit();
            return false;
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Used to avoid initialization being done before user has headset in correct position.
        /// </summary>
        private const float CameraInitializationMinY = 0.2f;

        private const float CameraCylinderSides         = 8;
        private const float CameraCylinderVerticalSteps = 2;

        private Vector3    _lastValidPos;
        private bool       _lastValidPosInitialized;
        private GameObject _quadObject;
        private Material   _fadeMaterial;
        private float      _fadeAlpha;

        #endregion
    }
}