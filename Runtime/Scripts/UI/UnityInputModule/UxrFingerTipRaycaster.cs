// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFingerTipRaycaster.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Raycaster compatible with Unity UI to use <see cref="UxrFingerTip" /> components on canvases, enabling touch
    ///     interaction.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class UxrFingerTipRaycaster : UxrGraphicRaycaster
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private float _fingerTipMinHoverDistance = FingerTipMinHoverDistanceDefault;
        [SerializeField] private float _fingerTipMaxAllowedAngle  = FingerTipMaxAllowedTipAngle;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Default maximum distance a <see cref="UxrFingerTip" /> can have to a canvas in order to start generating hovering
        ///     events.
        /// </summary>
        public const float FingerTipMinHoverDistanceDefault = 0.05f;

        /// <summary>
        ///     Default maximum angle between the finger and the canvas for a <see cref="UxrFingerTip" /> component to generate
        ///     input events.
        /// </summary>
        public const float FingerTipMaxAllowedTipAngle = 65.0f;

        /// <summary>
        ///     Gets or sets the maximum distance a <see cref="UxrFingerTip" /> can have to a canvas in order to generate hovering
        ///     events.
        /// </summary>
        public float FingerTipMinHoverDistance
        {
            get => _fingerTipMinHoverDistance;
            set => _fingerTipMinHoverDistance = value;
        }

        #endregion

        #region Public Overrides GraphicRaycaster

        /// <inheritdoc />
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            // Check if it should be ray-casted
            
            UxrPointerEventData pointerEventData = eventData as UxrPointerEventData;

            if (pointerEventData == null || pointerEventData.FingerTip == null)
            {
                return;
            }

            // Initialize if necessary
            
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

            // Raycast against the canvas, gather all results and append to the list

            _raycastResults.Clear();

            // First check finger angle. This helps avoiding unwanted clicks.

            if (Vector3.Angle(pointerEventData.FingerTip.WorldDir, _canvas.transform.forward) > _fingerTipMaxAllowedAngle)
            {
                return;
            }

            // Raycast

            var ray = new Ray(pointerEventData.FingerTip.WorldPos, pointerEventData.FingerTip.WorldDir);
            Raycast(_canvas, eventCamera, ray, ref _raycastResults, ref resultAppendList);

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
        /// <param name="cam">Camera from which to perform the checks</param>
        /// <param name="ray">Ray to use for intersection detection</param>
        /// <param name="results">Returns the list of results sorted by increasing distance</param>
        /// <param name="resultAppendList">Returns a list where the results have been appended to the existing content</param>
        private void Raycast(Canvas canvas, Camera cam, Ray ray, ref List<RaycastResult> results, ref List<RaycastResult> resultAppendList)
        {
            if (cam == null)
            {
                return;
            }

            // Iterate over all canvas graphics

            IList<Graphic> listGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            for (int i = 0; i < listGraphics.Count; ++i)
            {
                if (listGraphics[i].depth == -1 || !listGraphics[i].raycastTarget)
                {
                    continue;
                }

                float   distance        = Vector3.Dot(listGraphics[i].transform.forward, listGraphics[i].transform.position - ray.origin) / Vector3.Dot(listGraphics[i].transform.forward, ray.direction);
                Vector3 position        = ray.GetPoint(distance);
                Vector2 pointerPosition = cam.WorldToScreenPoint(position);

                if (distance > _fingerTipMinHoverDistance)
                {
                    continue;
                }

                if (!RectTransformUtility.RectangleContainsScreenPoint(listGraphics[i].rectTransform, pointerPosition, cam))
                {
                    continue;
                }

                if (listGraphics[i].Raycast(pointerPosition, cam))
                {
                    var result = new RaycastResult
                                 {
                                             gameObject     = listGraphics[i].gameObject,
                                             module         = this,
                                             distance       = distance,
                                             screenPosition = pointerPosition,
                                             worldPosition  = position,
                                             depth          = listGraphics[i].depth,
                                             sortingLayer   = canvas.sortingLayerID,
                                             sortingOrder   = canvas.sortingOrder,
                                 };

                    results.Add(result);
                }
            }

            results.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            resultAppendList.AddRange(results);
        }

        #endregion

        #region Private Types & Data

        private Canvas              _canvas;
        private CanvasGroup         _canvasGroup;
        private List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private Vector2             _lastPosition;

        #endregion
    }
}