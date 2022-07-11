// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Laser.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Devices;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Extensions.Unity.Render;
using UltimateXR.Haptics.Helpers;
using UltimateXR.Manipulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateXR.Examples.FullScene.Lab
{
    /// <summary>
    ///     Component that handles the laser in the lab room.
    /// </summary>
    public partial class Laser : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Transform              _laserSource;
        [SerializeField] private LayerMask              _collisionMask                        = -1;
        [SerializeField] private float                  _laserRayWidth                        = 0.003f;
        [SerializeField] private float                  _laserRayLength                       = 5.0f;
        [SerializeField] private Color                  _laserColor                           = Color.red;
        [SerializeField] private float                  _laserBurnDelaySeconds                = 0.2f; // Time the laser needs to be travelling at a speed less than LaserBurnParticlesMaxSpeed to create burn FX
        [SerializeField] private float                  _laserBurnSpeedThreshold              = 0.4f; // Maximum speed the laser can travel before stopping particles emission
        [SerializeField] private Color                  _laserBurnColor                       = new Color(0.0f, 0.0f, 0.0f, 0.6f);
        [SerializeField] private float                  _laserBurnVertexDistance              = 0.03f;
        [SerializeField] private float                  _laserBurnHeightOffset                = 0.001f;
        [SerializeField] private float                  _laserBurnDurationFadeStart           = 3.0f;
        [SerializeField] private float                  _laserBurnDurationFadeEnd             = 6.0f;
        [SerializeField] private Color                  _laserBurnIncandescentColor           = new Color(0.7f, 0.7f, 0.1f, 1.0f);
        [SerializeField] private float                  _laserBurnIncandescentDurationFadeEnd = 1.0f;
        [SerializeField] private ParticleSystem         _laserBurnParticles;
        [SerializeField] private float                  _laserBurnParticlesHeightOffset;
        [SerializeField] private bool                   _laserBurnReflectParticles;
        [SerializeField] private GameObject             _enableWhenLaserActive;
        [SerializeField] private UxrGrabbableObject     _triggerGrabbable;
        [SerializeField] private Transform              _trigger;
        [SerializeField] private Vector3                _triggerOffset;
        [SerializeField] private UxrFixedHapticFeedback _laserHaptics;

        #endregion

        #region Unity

        /// <summary>
        ///     Sets up internal data.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _triggerInitialOffset = _trigger.localPosition;

            // Line renderer setup

            _laserLineRenderer               = gameObject.AddComponent<LineRenderer>();
            _laserLineRenderer.useWorldSpace = false;

            SetLaserLineRendererMesh(_laserRayLength);

            _laserLineRenderer.material             = new Material(ShaderExt.UnlitTransparentColor);
            _laserLineRenderer.material.renderQueue = (int)RenderQueue.Overlay + 1;
            _laserLineRenderer.material.color       = _laserColor;
            _laserLineRenderer.loop                 = true;
            _laserLineRenderer.enabled              = false;

            _laserBurns = new List<LaserBurn>();

            ParticleSystem.EmissionModule emission = _laserBurnParticles.emission;
            emission.enabled = false;

            _createBurnTimer = _laserBurnDelaySeconds;
        }

        /// <summary>
        ///     Subscribe to avatar updated event.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribe from avatar updated event.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     We update the laser after all VR avatars have been updated to make sure it's processed after all manipulation.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            // Check if there is a hand grabbing the laser

            if (UxrGrabManager.Instance.GetGrabbingHand(_triggerGrabbable, 0, out UxrGrabber grabber))
            {
                // There is! see which hand to check for a trigger squeeze

                float trigger = UxrAvatar.LocalAvatarInput.GetInput1D(grabber.Side, UxrInput1D.Trigger);

                _trigger.localPosition = _triggerInitialOffset + _triggerOffset * trigger;

                _triggerGrabbable.GetGrabPoint(0).GetGripPoseInfo(grabber.Avatar).PoseBlendValue = trigger;

                if (UxrAvatar.LocalAvatarInput.GetButtonsPress(grabber.Side, UxrInputButtons.Trigger))
                {
                    // Trigger is squeezed

                    if (_laserLineRenderer.enabled == false)
                    {
                        // Start a new empty laser burn (it will be setup later, we use null to force to set up a new one)
                        _laserBurns.Add(null);
                        _laserLineRenderer.enabled = true;
                    }
                }
                else
                {
                    _laserLineRenderer.enabled = false;
                }
            }
            else
            {
                _laserLineRenderer.enabled = false;
            }

            // Check laser raycast

            if (_laserLineRenderer.enabled)
            {
                float currentRayLength = _laserRayLength;

                if (Physics.Raycast(_laserSource.position, _laserSource.forward, out RaycastHit hitInfo, currentRayLength, _collisionMask, QueryTriggerInteraction.Ignore))
                {
                    if (CurrentLaserBurn == null)
                    {
                        // This is a new burn -> initialize
                        _laserBurns[_laserBurns.Count - 1] = CreateNewLaserBurn(hitInfo.collider);
                        _createBurnTimer                   = _laserBurnDelaySeconds;
                    }
                    else if (CurrentLaserBurn.Collider != hitInfo.collider)
                    {
                        // If we hit another object we create a new laser burn entry
                        _laserBurns.Add(CreateNewLaserBurn(hitInfo.collider));
                        _createBurnTimer = _laserBurnDelaySeconds;
                    }

                    // Check if laser travel speed is below threshold to create a burn. If so decrement timer which will start a burn if it reaches 0.

                    if (Vector3.Distance(_lastLaserHitPosition, hitInfo.point) / Time.deltaTime < _laserBurnSpeedThreshold)
                    {
                        _createBurnTimer -= Time.deltaTime;
                    }
                    else
                    {
                        // Not enough speed -> new burn
                        _laserBurns.Add(CreateNewLaserBurn(hitInfo.collider));
                        _createBurnTimer = _laserBurnDelaySeconds;
                    }

                    _lastLaserHitPosition = hitInfo.point;

                    // Needs to start burn FX?

                    ParticleSystem.EmissionModule emission = _laserBurnParticles.emission;
                    emission.enabled                       = _createBurnTimer < 0.0f;
                    _laserBurnParticles.transform.position = hitInfo.point + hitInfo.normal * _laserBurnParticlesHeightOffset;
                    _laserBurnParticles.transform.rotation = _laserBurnReflectParticles ? Quaternion.LookRotation(Vector3.Reflect(_laserSource.forward, hitInfo.normal)) : Quaternion.Slerp(Quaternion.LookRotation(hitInfo.normal, Vector3.right), Quaternion.LookRotation(Vector3.up, Vector3.right), 0.9f);

                    if (_createBurnTimer < 0.0f)
                    {
                        // Burn trail

                        float segmentDistance = CurrentLaserBurn.PathPositions.Count == 0 ? 0.0f : Vector3.Distance(hitInfo.point, CurrentLaserBurn.LastWorldPathPosition);

                        if (CurrentLaserBurn.PathPositions.Count == 0 || segmentDistance > _laserBurnVertexDistance)
                        {
                            // Here we should create a new segment since the burn has travelled enough distance to create a new one, but first we are going to check if we somehow
                            // went over a hole, bump or depression in the geometry from the last point to this one. We do not want a burn strip to appear over gaps on the geometry so in that case
                            // what we will do is just create a new laser burn and skip this segment

                            if (CurrentLaserBurn.PathPositions.Count > 0 && segmentDistance > BurnGapCheckMinDistance)
                            {
                                bool startNewBurn = false;

                                Vector3 vectorAB = hitInfo.point - CurrentLaserBurn.LastWorldPathPosition;

                                for (int checkStep = 0; checkStep < BurnGapCheckSteps; ++checkStep)
                                {
                                    // Perform a series of steps casting rays from the laser source to intermediate points between the last burn segment and the current one looking for changes in depth

                                    float   t             = (checkStep + 1.0f) / (BurnGapCheckSteps + 1.0f);
                                    Vector3 pointCheck    = hitInfo.point + vectorAB * t;
                                    Vector3 perpendicular = Vector3.Cross(Vector3.Cross(vectorAB.normalized, ((CurrentLaserBurn.LastWorldNormal + hitInfo.normal) * 0.5f).normalized), vectorAB.normalized);
                                    Vector3 raySource     = CurrentLaserBurn.LastWorldPathPosition + vectorAB * 0.5f + perpendicular * BurnGapCheckRaySourceDistance;

                                    if (Physics.Raycast(raySource, (pointCheck - raySource).normalized, out RaycastHit hitInfoBurnGapCheck, _laserRayLength, _collisionMask, QueryTriggerInteraction.Ignore))
                                    {
                                        float distanceToLine = hitInfoBurnGapCheck.point.DistanceToLine(CurrentLaserBurn.LastWorldPathPosition, hitInfo.point);

                                        if (distanceToLine > BurnGapCheckLineDistanceThreshold)
                                        {
                                            // Depth change too big -> create new laser burn
                                            startNewBurn = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // Raycast did not find intersection -> create new laser burn
                                        startNewBurn = true;
                                        break;
                                    }
                                }

                                if (startNewBurn)
                                {
                                    _laserBurns.Add(CreateNewLaserBurn(hitInfo.collider));
                                }
                            }

                            // Add last point and offset a little bit from the geometry to draw correctly

                            CurrentLaserBurn.PathPositions.Add(CurrentLaserBurn.Transform.InverseTransformPoint(hitInfo.point + hitInfo.normal * _laserBurnHeightOffset));
                            CurrentLaserBurn.PathCreationTimes.Add(Time.time);
                            CurrentLaserBurn.LastNormal = CurrentLaserBurn.Transform.InverseTransformVector(hitInfo.normal);
                            UpdateLaserBurnLineRenderer(CurrentLaserBurn);
                            UpdateLaserBurns(0, _laserBurns.Count - 1);
                        }
                        else
                        {
                            UpdateLaserBurns(0, _laserBurns.Count);
                        }
                    }
                    else
                    {
                        UpdateLaserBurns(0, _laserBurns.Count);
                    }

                    currentRayLength = hitInfo.distance;
                }
                else
                {
                    ParticleSystem.EmissionModule emission = _laserBurnParticles.emission;
                    emission.enabled = false;
                    _createBurnTimer = _laserBurnDelaySeconds;

                    UpdateLaserBurns(0, _laserBurns.Count);
                }

                SetLaserLineRendererMesh(currentRayLength);
            }
            else
            {
                ParticleSystem.EmissionModule emission = _laserBurnParticles.emission;
                emission.enabled = false;
                _createBurnTimer = _laserBurnDelaySeconds;

                UpdateLaserBurns(0, _laserBurns.Count);
            }

            if (_laserHaptics)
            {
                _laserHaptics.enabled = _laserLineRenderer.enabled;

                if (grabber)
                {
                    _laserHaptics.HandSide = grabber.Side;
                }
            }

            if (_enableWhenLaserActive && !_enableWhenLaserActive.activeSelf && _laserLineRenderer.enabled)
            {
                _enableWhenLaserActive.SetActive(true);
            }
            else if (_enableWhenLaserActive && _enableWhenLaserActive.activeSelf && !_laserLineRenderer.enabled)
            {
                _enableWhenLaserActive.SetActive(false);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the laser line renderer
        /// </summary>
        /// <param name="rayLength">Current laser ray length</param>
        private void SetLaserLineRendererMesh(float rayLength)
        {
            _laserLineRenderer.startWidth = _laserRayWidth;
            _laserLineRenderer.endWidth   = _laserRayWidth;

            Vector3[] positions =
            {
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, rayLength > LaserGradientLength ? LaserGradientLength : rayLength * 0.33f),
                        new Vector3(0.0f, 0.0f, rayLength < LaserGradientLength * 2.0f ? rayLength * 0.66f : rayLength - LaserGradientLength),
                        new Vector3(0.0f, 0.0f, rayLength)
            };

            for (int i = 0; i < positions.Length; ++i)
            {
                positions[i] = _laserLineRenderer.transform.InverseTransformPoint(_laserSource.TransformPoint(positions[i]));
            }

            _laserLineRenderer.positionCount = 4;
            _laserLineRenderer.SetPositions(positions);

            Gradient colorGradient = new Gradient();

            colorGradient.colorKeys = new[]
                                      {
                                                  new GradientColorKey(Color.white, 0.0f),
                                                  new GradientColorKey(Color.white, rayLength > LaserGradientLength ? LaserGradientLength / rayLength : 0.33f),
                                                  new GradientColorKey(Color.white, rayLength < LaserGradientLength * 2.0f ? 0.66f : 1.0f - LaserGradientLength / rayLength),
                                                  new GradientColorKey(Color.white, 1.0f)
                                      };

            colorGradient.alphaKeys = new[]
                                      {
                                                  new GradientAlphaKey(0.0f, 0.0f),
                                                  new GradientAlphaKey(1.0f, rayLength > LaserGradientLength ? LaserGradientLength / rayLength : 0.3f),
                                                  new GradientAlphaKey(1.0f, rayLength < LaserGradientLength * 2.0f ? 0.66f : 1.0f - LaserGradientLength / rayLength),
                                                  new GradientAlphaKey(0.0f, 1.0f)
                                      };

            _laserLineRenderer.colorGradient = colorGradient;
        }

        /// <summary>
        ///     Creates a new laser burn entry
        /// </summary>
        /// <param name="collider">Collider the laser burn is attached to</param>
        /// <returns>New laser burn object</returns>
        private LaserBurn CreateNewLaserBurn(Collider collider)
        {
            LaserBurn newLaserBurn = new LaserBurn();

            newLaserBurn.Collider = collider;

            newLaserBurn.GameObject                         = new GameObject("LaserBurn");
            newLaserBurn.GameObject.transform.parent        = collider.transform;
            newLaserBurn.GameObject.transform.localPosition = Vector3.zero;
            newLaserBurn.GameObject.transform.localRotation = Quaternion.identity;
            newLaserBurn.LineRenderer                       = newLaserBurn.GameObject.AddComponent<LineRenderer>();
            newLaserBurn.LineRenderer.receiveShadows        = true;
            newLaserBurn.LineRenderer.shadowCastingMode     = ShadowCastingMode.Off;
            newLaserBurn.LineRenderer.useWorldSpace         = false;
            newLaserBurn.LineRenderer.material              = new Material(ShaderExt.UnlitTransparentColor);
            newLaserBurn.LineRenderer.material.renderQueue  = (int)RenderQueue.Overlay + 1;
            newLaserBurn.LineRenderer.material.color        = _laserBurnColor;
            newLaserBurn.LineRenderer.loop                  = false;
            newLaserBurn.LineRenderer.positionCount         = 0;

            newLaserBurn.GameObjectIncandescent                         = new GameObject("LaserBurnIncandescent");
            newLaserBurn.GameObjectIncandescent.transform.parent        = collider.transform;
            newLaserBurn.GameObjectIncandescent.transform.localPosition = Vector3.zero;
            newLaserBurn.GameObjectIncandescent.transform.localRotation = Quaternion.identity;
            newLaserBurn.IncandescentLineRenderer                       = newLaserBurn.GameObjectIncandescent.AddComponent<LineRenderer>();
            newLaserBurn.IncandescentLineRenderer.receiveShadows        = false;
            newLaserBurn.IncandescentLineRenderer.shadowCastingMode     = ShadowCastingMode.Off;
            newLaserBurn.IncandescentLineRenderer.useWorldSpace         = false;
            newLaserBurn.IncandescentLineRenderer.material              = new Material(ShaderExt.UnlitAdditiveColor);
            newLaserBurn.IncandescentLineRenderer.material.renderQueue  = (int)RenderQueue.Overlay + 2;
            newLaserBurn.IncandescentLineRenderer.material.color        = _laserBurnIncandescentColor;
            newLaserBurn.IncandescentLineRenderer.loop                  = false;
            newLaserBurn.IncandescentLineRenderer.positionCount         = 0;

            newLaserBurn.PathPositions     = new List<Vector3>();
            newLaserBurn.PathCreationTimes = new List<float>();

            _createBurnTimer = _laserBurnDelaySeconds;

            return newLaserBurn;
        }

        /// <summary>
        ///     Updates the given range of laser burns. Unused laser burns will be deleted
        /// </summary>
        /// <param name="startIndex">The start index</param>
        /// <param name="count">The number of items to update</param>
        private void UpdateLaserBurns(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count && i < _laserBurns.Count; ++i)
            {
                UpdateLaserBurnLineRenderer(_laserBurns[i]);

                if (_laserBurns[i].PathPositions.Count <= 1 && _laserBurns[i] != CurrentLaserBurn)
                {
                    Destroy(_laserBurns[i].GameObject);
                    Destroy(_laserBurns[i].GameObjectIncandescent);
                    _laserBurns.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        ///     Updates the laser burn line renderer
        /// </summary>
        /// <param name="laserBurn">LaserBurn object whose LineRenderer to update</param>
        private void UpdateLaserBurnLineRenderer(LaserBurn laserBurn)
        {
            if (laserBurn == null || laserBurn.GameObject == null)
            {
                return;
            }

            laserBurn.LineRenderer.startWidth             = _laserRayWidth * 2.5f;
            laserBurn.LineRenderer.endWidth               = _laserRayWidth * 2.5f;
            laserBurn.IncandescentLineRenderer.startWidth = _laserRayWidth * 1.5f;
            laserBurn.IncandescentLineRenderer.endWidth   = _laserRayWidth * 1.5f;

            // Remove segment positions that have already faded out. Keep just 1 if all need to be deleted to have LastPathPosition accessible.

            int lastIndexToDelete = -1;

            for (int i = 0; i < laserBurn.PathCreationTimes.Count; ++i)
            {
                if (Time.time - laserBurn.PathCreationTimes[i] >= _laserBurnDurationFadeEnd)
                {
                    lastIndexToDelete = i;
                }
                else
                {
                    break;
                }
            }

            if (lastIndexToDelete >= 0 && laserBurn.PathPositions.Count > 1)
            {
                laserBurn.PathCreationTimes.RemoveRange(0, lastIndexToDelete + 1);
                laserBurn.PathPositions.RemoveRange(0, lastIndexToDelete + 1);
            }

            // Inside the burn range, compute the start index for the incandescent part

            int incandescentIndexStart = 0;

            for (int i = 0; i < laserBurn.PathCreationTimes.Count; ++i)
            {
                if (Time.time - laserBurn.PathCreationTimes[i] < _laserBurnIncandescentDurationFadeEnd)
                {
                    incandescentIndexStart = i;
                    break;
                }
            }

            // Create color and alpha gradients. Maximum number of entries for Unity's LineRenderer are 8.

            if (laserBurn.PathCreationTimes.Count > 0)
            {
                Gradient colorGradient             = new Gradient();
                Gradient colorGradientIncandescent = new Gradient();

                colorGradient.colorKeys             = new[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) };
                colorGradientIncandescent.colorKeys = new[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) };

                GradientAlphaKey[] alphaKeys             = new GradientAlphaKey[8];
                GradientAlphaKey[] alphaKeysIncandescent = new GradientAlphaKey[8];

                for (int i = 0; i < alphaKeys.Length; ++i)
                {
                    float t         = i / (alphaKeys.Length - 1.0f);
                    int   timeIndex = Mathf.Clamp(Mathf.RoundToInt(t * (laserBurn.PathCreationTimes.Count - 1.0f)), 0, laserBurn.PathCreationTimes.Count - 1);
                    float life      = Time.time - laserBurn.PathCreationTimes[timeIndex];
                    alphaKeys[i].alpha = life < _laserBurnDurationFadeStart ? 1.0f : 1.0f - Mathf.Clamp01((life - _laserBurnDurationFadeStart) / (_laserBurnDurationFadeEnd - _laserBurnDurationFadeStart));
                    alphaKeys[i].time  = t;
                }

                for (int i = 0; i < alphaKeysIncandescent.Length; ++i)
                {
                    float t                      = i / (alphaKeysIncandescent.Length - 1.0f);
                    int   numIncandescentEntries = laserBurn.PathCreationTimes.Count - incandescentIndexStart;
                    int   timeIndex              = Mathf.Clamp(Mathf.RoundToInt(t * (numIncandescentEntries - 1.0f)), 0, numIncandescentEntries - 1);
                    float life                   = Time.time - laserBurn.PathCreationTimes[incandescentIndexStart + timeIndex];
                    alphaKeysIncandescent[i].alpha = 1.0f - Mathf.Clamp01(life / _laserBurnIncandescentDurationFadeEnd);
                    alphaKeysIncandescent[i].time  = t;
                }

                colorGradient.alphaKeys             = alphaKeys;
                colorGradientIncandescent.alphaKeys = alphaKeysIncandescent;

                laserBurn.LineRenderer.colorGradient             = colorGradient;
                laserBurn.IncandescentLineRenderer.colorGradient = colorGradientIncandescent;
            }

            // Update positions

            if (laserBurn.PathPositions.Count > 1)
            {
                laserBurn.LineRenderer.positionCount = laserBurn.PathPositions.Count;
                laserBurn.LineRenderer.SetPositions(laserBurn.PathPositions.ToArray());
            }
            else
            {
                laserBurn.LineRenderer.positionCount = 0;
            }

            if (incandescentIndexStart < laserBurn.PathPositions.Count - 1)
            {
                int count = laserBurn.PathPositions.Count - incandescentIndexStart;
                laserBurn.IncandescentLineRenderer.positionCount = count;
                laserBurn.IncandescentLineRenderer.SetPositions(laserBurn.PathPositions.GetRange(incandescentIndexStart, count).ToArray());
            }
            else
            {
                laserBurn.IncandescentLineRenderer.positionCount = 0;
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the current laser burn being generated.
        /// </summary>
        private LaserBurn CurrentLaserBurn => _laserBurns[_laserBurns.Count - 1];

        private const float LaserGradientLength               = 0.4f;   // Laser is rendered with a gradient of this length to make it a little less dull
        private const float BurnGapCheckSteps                 = 4;      // Number of steps to check for gaps in the geometry when computing laser burns
        private const float BurnGapCheckMinDistance           = 0.02f;  // Only perform gap checks if the separation between two consecutive laser burn segments is greater than this value
        private const float BurnGapCheckRaySourceDistance     = 0.1f;   // The raycasts performed on each gap check step will be casted from this distance to the geometry
        private const float BurnGapCheckLineDistanceThreshold = 0.002f; // Allow this amount of depth variation when checking for gaps or bumps in the geometry

        private Vector3         _triggerInitialOffset;
        private LineRenderer    _laserLineRenderer;
        private List<LaserBurn> _laserBurns;
        private Vector3         _lastLaserHitPosition;
        private float           _createBurnTimer;

        #endregion
    }
}