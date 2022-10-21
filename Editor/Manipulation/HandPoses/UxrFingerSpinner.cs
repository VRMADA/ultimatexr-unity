// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFingerSpinner.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Extensions.Unity.Render;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation.HandPoses
{
    /// <summary>
    ///     Finger spinner widget that allows to handle the rotation of finger nodes. It is placed inside a hand image together
    ///     with all other finger spinners.
    /// </summary>
    [Serializable]
    public class UxrFingerSpinner
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrFingerAngleType    _angleType;
        [SerializeField] private Texture2D             _texMouse;
        [SerializeField] private Color32               _color;
        [SerializeField] private Transform             _target;
        [SerializeField] private Transform             _parent;
        [SerializeField] private UxrUniversalLocalAxes _targetLocalAxes;
        [SerializeField] private UxrUniversalLocalAxes _parentLocalAxes;
        [SerializeField] private float                 _minAngle;
        [SerializeField] private float                 _maxAngle;
        [SerializeField] private UxrHandSide           _handSide;
        [SerializeField] private bool                  _isThumbProximal;
        [SerializeField] private Vector2               _quadMax;
        [SerializeField] private Vector2               _quadMin;
        [SerializeField] private float                 _offset;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the type of angle controlled by the spinner.
        /// </summary>
        public UxrFingerAngleType Angle => _angleType;

        /// <summary>
        ///     Gets the finger bone transform controlled by the spinner.
        /// </summary>
        public Transform Target => _target;

        /// <summary>
        ///     Gets which hand the spinner belongs to.
        /// </summary>
        public UxrHandSide HandSide => _handSide;

        /// <summary>
        ///     Gets the <see cref="Rect" /> of the UI control in local hand image coordinates.
        /// </summary>
        public Rect MouseRect => new Rect(_quadMin.x, _quadMin.y, _quadMax.x - _quadMin.x, _quadMax.y - _quadMin.y);

        /// <summary>
        ///     Gets or sets the angle value in degrees.
        /// </summary>
        public float Value
        {
            get => GetValueFromObject();
            set
            {
                float oldValue = GetValueFromObject();
                float newValue = Mathf.Clamp(value, _minAngle, _maxAngle);

                if (_angleType == UxrFingerAngleType.Spread)
                {
                    _target.Rotate(_targetLocalAxes.LocalUp, newValue - oldValue, Space.Self);
                }
                else if (_angleType == UxrFingerAngleType.Curl)
                {
                    _target.Rotate(_targetLocalAxes.LocalRight, newValue - oldValue, Space.Self);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the offset applied to the spinner. It is used to draw the widget instead of using <see cref="Value" />
        ///     which sometimes propagates slightly from one spinner to another since transforms are related.
        /// </summary>
        public float Offset
        {
            get => _offset;
            set
            {
                float forgiveness = 0.1f;

                if (Value > _minAngle + forgiveness && Value < _maxAngle - forgiveness)
                {
                    _offset = value;
                }
            }
        }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="angleType">The type of angle this control will rotate (spread or curl)</param>
        /// <param name="texMouse">The image that has the clickable parts encoded in colors</param>
        /// <param name="color">The color this control has inside the <paramref name="texMouse" /> texture</param>
        /// <param name="target">The transform that will be rotated</param>
        /// <param name="parent">The parent transform</param>
        /// <param name="targetLocalAxes">The universal right/up/forward axes of the target transform</param>
        /// <param name="parentLocalAxes">The universal right/up/forward axes of the parent transform</param>
        /// <param name="minAngle">The minimum allowed value</param>
        /// <param name="maxAngle">The maximum allowed angle</param>
        /// <param name="handSide">Which hand</param>
        /// <param name="isThumbProximal">Is target a proximal bone from the thumb?</param>
        public UxrFingerSpinner(UxrFingerAngleType    angleType,
                                Texture2D             texMouse,
                                Color32               color,
                                Transform             target,
                                Transform             parent,
                                UxrUniversalLocalAxes targetLocalAxes,
                                UxrUniversalLocalAxes parentLocalAxes,
                                float                 minAngle,
                                float                 maxAngle,
                                UxrHandSide           handSide,
                                bool                  isThumbProximal = false)
        {
            _angleType       = angleType;
            _texMouse        = texMouse;
            _color           = color;
            _target          = target;
            _parent          = parent;
            _targetLocalAxes = targetLocalAxes;
            _parentLocalAxes = parentLocalAxes;
            _minAngle        = minAngle;
            _maxAngle        = maxAngle;
            _handSide        = handSide;
            _isThumbProximal = isThumbProximal;

            if (handSide == UxrHandSide.Left && s_boundsLeftHand == null)
            {
                s_boundsLeftHand = GetMouseTextureBounds(texMouse);
            }

            if (handSide == UxrHandSide.Right && s_boundsRightHand == null)
            {
                s_boundsRightHand = GetMouseTextureBounds(texMouse);
            }

            _quadMin = GetBoundsMin(handSide == UxrHandSide.Left ? s_boundsLeftHand : s_boundsRightHand, color);
            _quadMax = GetBoundsMax(handSide == UxrHandSide.Left ? s_boundsLeftHand : s_boundsRightHand, color);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether the mouse position lies within the control bounds.
        /// </summary>
        /// <param name="mousePos">Mouse position in local hand image coordinates</param>
        /// <returns>Whether the mouse position lies within the control bounds</returns>
        public bool ContainsMousePos(Vector2 mousePos)
        {
            return mousePos.x >= _quadMin.x && mousePos.x <= _quadMax.x && mousePos.y >= _quadMin.y && mousePos.y <= _quadMax.y;

            /*
             * Alternative: Get from mouse texture.
             * 
            if (mousePos.x >= 0 && mousePos.x < _texMouse.width && mousePos.y >= 0 && mousePos.y < _texMouse.height)
            {
                Color32 texColor = _texMouse.GetPixels32(0)[((_texMouse.height - Mathf.RoundToInt(mousePos.y)) * _texMouse.width) + Mathf.RoundToInt(mousePos.x)];
                return texColor.r == _color.r && texColor.g == _color.g && texColor.b == _color.b && texColor.a == _color.a;
            }

            return false;
            */
        }

        /// <summary>
        ///     Computes the rotation angle from the object's current transform.
        /// </summary>
        /// <returns>Rotation value in degrees</returns>
        public float GetValueFromObject()
        {
            return GetValueFromObject(_angleType);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Computes a dictionary where for a given color a minimum and maximum rect value is stored, representing the corners
        ///     where the color appears in the texture.
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <returns>Dictionary</returns>
        private static Dictionary<int, Bounds> GetMouseTextureBounds(Texture2D texture)
        {
            Dictionary<int, Bounds> colorBounds = new Dictionary<int, Bounds>();

            Color32[] pixels = texture.GetPixels32(0);

            for (int x = 0; x < texture.width; ++x)
            {
                for (int y = 0; y < texture.height; ++y)
                {
                    int color = pixels[(texture.height - y - 1) * texture.width + x].ToInt();

                    if (!colorBounds.ContainsKey(color))
                    {
                        colorBounds.Add(color, new Bounds(new Vector3(x, y, 0.0f), Vector3.zero));
                    }

                    Bounds  bounds = colorBounds[color];
                    Vector3 min    = bounds.min;
                    Vector3 max    = bounds.max;

                    if (x < min.x)
                    {
                        min.x = x;
                    }
                    if (x > max.x)
                    {
                        max.x = x;
                    }
                    if (y < min.y)
                    {
                        min.y = y;
                    }
                    if (y > max.y)
                    {
                        max.y = y;
                    }

                    bounds.min = min;
                    bounds.max = max;

                    colorBounds[color] = bounds;
                }
            }

            return colorBounds;
        }

        /// <summary>
        ///     Gets the minimum value of a rect for a given color.
        /// </summary>
        /// <param name="colorBounds">Bounds dictionary</param>
        /// <param name="color">Color to get the max bounds value of</param>
        /// <returns>Maximum rect x and y for the given color</returns>
        private static Vector2 GetBoundsMin(Dictionary<int, Bounds> colorBounds, Color32 color)
        {
            if (colorBounds.TryGetValue(color.ToInt(), out Bounds bounds))
            {
                return new Vector2(bounds.min.x, bounds.min.y);
            }

            return Vector2.zero;
        }

        /// <summary>
        ///     Gets the maximum value of a rect for a given color.
        /// </summary>
        /// <param name="colorBounds">Bounds dictionary</param>
        /// <param name="color">Color to get the max bounds value of</param>
        /// <returns>Maximum rect x and y for the given color</returns>
        private static Vector2 GetBoundsMax(Dictionary<int, Bounds> colorBounds, Color32 color)
        {
            if (colorBounds.TryGetValue(color.ToInt(), out Bounds bounds))
            {
                return new Vector2(bounds.max.x, bounds.max.y);
            }

            return Vector2.zero;
        }

        /// <summary>
        ///     Computes a rotation angle from the object's current transform.
        /// </summary>
        /// <param name="angleType">Angle to compute (spread or curl)</param>
        /// <returns>Angle in degrees</returns>
        private float GetValueFromObject(UxrFingerAngleType angleType)
        {
            // Compute roll value by rotating "right" vector from the current forward direction to the initial forward direction. Roll value will be
            // the angle between initial right and current right.
            // The parent reference directions are different for the proximal thumb bone which is a special case.

            Vector3 parentRight = _isThumbProximal
                                              ? _handSide == UxrHandSide.Left ? _parent.TransformDirection(-_parentLocalAxes.LocalForward) : _parent.TransformDirection(_parentLocalAxes.LocalForward)
                                              : _parent.TransformDirection(_parentLocalAxes.LocalRight);
            Vector3 parentUp = _parent.TransformDirection(_parentLocalAxes.LocalUp);
            Vector3 parentForward = _isThumbProximal
                                                ? _handSide == UxrHandSide.Left ? _parent.TransformDirection(_parentLocalAxes.LocalRight) : _parent.TransformDirection(-_parentLocalAxes.LocalRight)
                                                : _parent.TransformDirection(_parentLocalAxes.LocalForward);

            Vector3 targetRight   = _target.TransformDirection(_targetLocalAxes.LocalRight);
            Vector3 targetUp      = _target.TransformDirection(_targetLocalAxes.LocalUp);
            Vector3 targetForward = _target.TransformDirection(_targetLocalAxes.LocalForward);

            Quaternion toForward     = Quaternion.FromToRotation(targetForward, parentForward);
            Vector3    rightOnlyRoll = toForward * targetRight;
            float      roll          = Vector3.SignedAngle(parentRight, rightOnlyRoll, parentForward);

            // Compute rotation to remove roll component

            Quaternion removeRoll = Quaternion.AngleAxis(-roll, targetForward);

            if (angleType == UxrFingerAngleType.Spread)
            {
                // Compute correct spread angle

                Vector3 spreadAxis          = removeRoll * targetUp;
                Vector3 planeRightNormal    = parentRight;
                Vector3 spreadVectorOnPlane = Vector3.Cross(planeRightNormal, Vector3.ProjectOnPlane(spreadAxis, planeRightNormal));

                return Vector3.SignedAngle(spreadVectorOnPlane, targetForward, spreadAxis);
            }
            if (angleType == UxrFingerAngleType.Curl)
            {
                // Compute correct curl angle

                Vector3 curlAxis          = removeRoll * targetRight;
                Vector3 planeUpNormal     = parentUp;
                Vector3 curlVectorOnPlane = Vector3.Cross(Vector3.ProjectOnPlane(curlAxis, planeUpNormal), planeUpNormal);

                return Vector3.SignedAngle(curlVectorOnPlane, targetForward, curlAxis);
            }

            return 0.0f;
        }

        #endregion

        #region Private Types & Data

        private static Dictionary<int, Bounds> s_boundsLeftHand;
        private static Dictionary<int, Bounds> s_boundsRightHand;

        #endregion
    }
}