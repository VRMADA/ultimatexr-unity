// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLaserPointerRaycaster.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Raycaster compatible with Unity UI to use <see cref="UxrLaserPointer" /> components on canvases, enabling
    ///     interaction using laser pointers from a distance.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class UxrLaserPointerRaycaster : GraphicRaycaster
    {
        #region Public Overrides GraphicRaycaster

        /// <inheritdoc />
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_canvas == null)
            {
                _canvas      = gameObject.GetComponent<Canvas>();
                _canvasGroup = gameObject.GetComponent<CanvasGroup>();

                if (_canvas == null)
                {
                    return;
                }
            }

            if (_canvasGroup != null && _canvasGroup.interactable == false)
            {
                return;
            }
            
            UxrPointerEventData pointerEventData = eventData as UxrPointerEventData;

            // Raycast against the canvas, gather all results and append to the list

            _raycastResults.Clear();

            var ray = new Ray(eventData.pointerCurrentRaycast.worldPosition, eventData.pointerCurrentRaycast.worldNormal);
            Raycast(_canvas, pointerEventData?.LaserPointer, eventCamera, ray, ref _raycastResults, ref resultAppendList, out float _);

            // Assign correct indices and get closest raycast

            RaycastResult? raycastNearest = null;

            for (int i = 0; i < _raycastResults.Count; ++i)
            {
                RaycastResult rayCastResult = _raycastResults[i];
                rayCastResult.index = resultAppendList.Count + i;

                if (!raycastNearest.HasValue || rayCastResult.distance < raycastNearest.Value.distance)
                {
                    raycastNearest = rayCastResult;
                }
            }

            // If a closest raycast was found, use it to compute the event delta

            if (raycastNearest.HasValue)
            {
                eventData.position = raycastNearest.Value.screenPosition;
                eventData.delta    = eventData.position - _lastPosition;
                _lastPosition      = eventData.position;

                eventData.pointerCurrentRaycast = raycastNearest.Value;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Performs a raycast to check which elements in a canvas were potentially interacted with.
        /// </summary>
        /// <param name="canvas">Canvas to check</param>
        /// <param name="laserPointer">Source laser pointer or null if it could not be retrieved</param>
        /// <param name="cam">Camera from which to perform the checks</param>
        /// <param name="ray">Ray to use for intersection detection</param>
        /// <param name="results">Returns the list of results sorted by increasing distance</param>
        /// <param name="resultAppendList">Returns a list where the results have been appended to the existing content</param>
        /// <param name="occluderDistance">Returns the occluder distance</param>
        private void Raycast(Canvas canvas, UxrLaserPointer laserPointer, Camera cam, Ray ray, ref List<RaycastResult> results, ref List<RaycastResult> resultAppendList, out float occluderDistance)
        {
            occluderDistance = -1.0f;

            if (cam == null)
            {
                return;
            }

            float      hitDistance = float.MaxValue;
            GameObject hitObject   = null;
            Vector3    hitPosition = Vector3.zero;
            int        hitDepth    = 0;

            // Check for objects in the path. Set hitDistance to the ray length to the closest hit.

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && blockingObjects != BlockingObjects.None)
            {
                RectTransform rectTransformCanvas = canvas.GetComponent<RectTransform>();

                float maxDistance = Vector3.Distance(ray.origin, canvas.transform.position) +
                                    Mathf.Max(rectTransformCanvas.localScale.x, rectTransformCanvas.localScale.y, rectTransformCanvas.localScale.z) *
                                    Mathf.Max(rectTransformCanvas.rect.width,   rectTransformCanvas.rect.height);

                bool blocking3D = laserPointer != null ? laserPointer.TargetTypes.HasFlag(UxrLaserPointerTargetTypes.Colliders3D) : blockingObjects.HasFlag(BlockingObjects.ThreeD);

                if (blocking3D)
                {
                    if (Physics.Raycast(ray,
                                        out RaycastHit hit,
                                        maxDistance,
                                        laserPointer != null ? laserPointer.BlockingMask : m_BlockingMask,
                                        laserPointer != null ? laserPointer.TriggerCollidersInteraction : QueryTriggerInteraction.Ignore))
                    {
                        hitObject        = hit.collider.gameObject;
                        hitPosition      = hit.point;
                        hitDepth         = UxrConstants.UI.Depth3DObject;
                        hitDistance      = hit.distance;
                        occluderDistance = hitDistance;
                    }
                }

                bool blocking2D = laserPointer != null ? laserPointer.TargetTypes.HasFlag(UxrLaserPointerTargetTypes.Colliders2D) : blockingObjects.HasFlag(BlockingObjects.TwoD);

                if (blocking2D)
                {
                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxDistance, laserPointer != null ? laserPointer.BlockingMask : m_BlockingMask);

                    if (hit.collider != null && hit.distance < hitDistance)
                    {
                        hitObject        = hit.collider.gameObject;
                        hitPosition      = hit.point;
                        hitDepth         = UxrConstants.UI.Depth2DObject;
                        hitDistance      = hit.fraction * maxDistance;
                        occluderDistance = hitDistance;
                    }
                }
            }

            if (hitObject != null)
            {
                var result = new RaycastResult
                             {
                                         gameObject     = hitObject,
                                         module         = this,
                                         distance       = hitDistance,
                                         screenPosition = cam.WorldToScreenPoint(hitPosition),
                                         worldPosition  = hitPosition,
                                         depth          = hitDepth,
                                         sortingLayer   = 0,
                                         sortingOrder   = 0,
                             };

                results.Add(result);
            }

            // Iterate over all canvas graphics

            bool processUI = laserPointer == null || laserPointer.TargetTypes.HasFlag(UxrLaserPointerTargetTypes.UI);

            if (processUI)
            {
                IList<Graphic> listGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

                for (int i = 0; i < listGraphics.Count; i++)
                {
                    Graphic graphic = listGraphics[i];
                    if (graphic.depth == -1 || !graphic.raycastTarget)
                    {
                        continue;
                    }

                    float distance = Vector3.Dot(graphic.transform.forward, graphic.transform.position - ray.origin) / Vector3.Dot(graphic.transform.forward, ray.direction);

                    if (distance < 0.0f)
                    {
                        continue;
                    }

                    if (distance - HitDistanceThreshold > hitDistance)
                    {
                        continue;
                    }

                    Vector3 position        = ray.GetPoint(distance);
                    Vector2 pointerPosition = cam.WorldToScreenPoint(position);

                    if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, cam))
                    {
                        continue;
                    }

                    if (graphic.Raycast(pointerPosition, cam))
                    {
                        var result = new RaycastResult
                                     {
                                                 gameObject     = graphic.gameObject,
                                                 module         = this,
                                                 distance       = distance,
                                                 screenPosition = pointerPosition,
                                                 worldPosition  = position,
                                                 depth          = graphic.depth,
                                                 sortingLayer   = canvas.sortingLayerID,
                                                 sortingOrder   = canvas.sortingOrder,
                                     };

                        results.Add(result);
                    }
                }
            }

            results.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            resultAppendList.AddRange(results);
        }

        #endregion

        #region Private Types & Data

        private const float HitDistanceThreshold = 0.001f;

        private Canvas              _canvas;
        private CanvasGroup         _canvasGroup;
        private List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private Vector2             _lastPosition;

        #endregion
    }
}