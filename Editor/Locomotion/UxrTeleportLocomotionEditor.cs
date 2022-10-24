// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotionEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Locomotion;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Locomotion
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrTeleportLocomotion" />.
    /// </summary>
    [CustomEditor(typeof(UxrTeleportLocomotion))]
    [CanEditMultipleObjects]
    public class UxrTeleportLocomotionEditor : UxrTeleportLocomotionBaseEditor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _propArcSegments           = serializedObject.FindProperty("_arcSegments");
            _propArcWidth              = serializedObject.FindProperty("_arcWidth");
            _propArcScrollSpeedValid   = serializedObject.FindProperty("_arcScrollSpeedValid");
            _propArcScrollSpeedInvalid = serializedObject.FindProperty("_arcScrollSpeedInvalid");
            _propArcMaterialValid      = serializedObject.FindProperty("_arcMaterialValid");
            _propArcMaterialInvalid    = serializedObject.FindProperty("_arcMaterialInvalid");
            _propArcFadeLength         = serializedObject.FindProperty("_arcFadeLength");
            _propRaycastStepsQuality   = serializedObject.FindProperty("_raycastStepsQuality");
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Draws the custom inspector part for specific <see cref="UxrTeleportLocomotion" /> properties. Parent properties are
        ///     drawn by <see cref="UxrTeleportLocomotion" />
        /// </summary>
        protected override void OnTeleportInspectorGUI()
        {
            EditorGUILayout.Space();

            _foldoutArc = UxrEditorUtils.FoldoutStylish("Arc", _foldoutArc);

            if (_foldoutArc)
            {
                EditorGUILayout.IntSlider(_propArcSegments, 2, 1000, ContentArcSegments);
                EditorGUILayout.Slider(_propArcWidth, 0.01f, 0.4f, ContentArcWidth);
                EditorGUILayout.PropertyField(_propArcScrollSpeedValid,   ContentArcScrollSpeedValid);
                EditorGUILayout.PropertyField(_propArcScrollSpeedInvalid, ContentArcScrollSpeedInvalid);
                EditorGUILayout.PropertyField(_propArcMaterialValid,      ContentArcMaterialValid);
                EditorGUILayout.PropertyField(_propArcMaterialInvalid,    ContentArcMaterialInvalid);
                EditorGUILayout.Slider(_propArcFadeLength, 0.01f, 8.0f, ContentArcFadeLength);
                EditorGUILayout.PropertyField(_propRaycastStepsQuality, ContentRaycastStepsQuality);
            }
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentArcSegments           { get; } = new GUIContent("Arc Segments",                      "The number of subdivisions to use to render the arc");
        private GUIContent ContentArcWidth              { get; } = new GUIContent("Arc Width",                         "The width of the arc");
        private GUIContent ContentArcScrollSpeedValid   { get; } = new GUIContent("Arc Scroll Speed (Valid Target)",   "The arc material scroll speed when the destination is valid");
        private GUIContent ContentArcScrollSpeedInvalid { get; } = new GUIContent("Arc Scroll Speed (Invalid Target)", "The arc material scroll speed when the destination is invalid");
        private GUIContent ContentArcMaterialValid      { get; } = new GUIContent("Arc Material (Valid Target)",       "The arc material used when the destination is valid");
        private GUIContent ContentArcMaterialInvalid    { get; } = new GUIContent("Arc Material (Invalid Target)",     "The arc material used when the destination is invalid");
        private GUIContent ContentArcFadeLength         { get; } = new GUIContent("Arc Fade Length",                   "The fade length used when the arc exits the hand or collides with a target to hide intersections");
        private GUIContent ContentRaycastStepsQuality   { get; } = new GUIContent("Blocking Raycast Steps Quality",    "How many subdivisions along the arc to perform to raycasts against the scenario");

        private SerializedProperty _propArcSegments;
        private SerializedProperty _propArcWidth;
        private SerializedProperty _propArcScrollSpeedValid;
        private SerializedProperty _propArcScrollSpeedInvalid;
        private SerializedProperty _propArcMaterialValid;
        private SerializedProperty _propArcMaterialInvalid;
        private SerializedProperty _propArcFadeLength;
        private SerializedProperty _propRaycastStepsQuality;

        private bool _foldoutArc = true;

        #endregion
    }
}