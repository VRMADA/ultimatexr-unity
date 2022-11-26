// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabbableObjectAnchorEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Manipulation;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Manipulation
{
    /// <summary>
    ///     Custom editor for <see cref="UxrGrabbableObjectAnchor" />.
    /// </summary>
    [CustomEditor(typeof(UxrGrabbableObjectAnchor))]
    [CanEditMultipleObjects]
    public class UxrGrabbableObjectAnchorEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties
        /// </summary>
        private void OnEnable()
        {
            _propCompatibleTags                 = serializedObject.FindProperty("_compatibleTags");
            _propMaxPlaceDistance               = serializedObject.FindProperty("_maxPlaceDistance");
            _propAlignTransformUseSelf          = serializedObject.FindProperty("_alignTransformUseSelf");
            _propAlignTransform                 = serializedObject.FindProperty("_alignTransform");
            _propDropProximityTransformUseSelf  = serializedObject.FindProperty("_dropProximityTransformUseSelf");
            _propDropProximityTransform         = serializedObject.FindProperty("_dropProximityTransform");
            _propActivateOnCompatibleNear       = serializedObject.FindProperty("_activateOnCompatibleNear");
            _propActivateOnCompatibleNotNear    = serializedObject.FindProperty("_activateOnCompatibleNotNear");
            _propActivateOnHandNearAndGrabbable = serializedObject.FindProperty("_activateOnHandNearAndGrabbable");
            _propActivateOnPlaced               = serializedObject.FindProperty("_activateOnPlaced");
            _propActivateOnEmpty                = serializedObject.FindProperty("_activateOnEmpty");
        }

        /// <summary>
        ///     Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int popup = -1;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General parameters:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propCompatibleTags,   ContentCompatibleTags, true);
            EditorGUILayout.PropertyField(_propMaxPlaceDistance, ContentMaxPlaceDistance);

            popup = EditorGUILayout.Popup(ContentDropAlignmentOptions, _propAlignTransformUseSelf.boolValue ? 0 : 1, new[] { new GUIContent("Use self transform"), new GUIContent("Use other transform") });

            if (popup == 1)
            {
                EditorGUILayout.PropertyField(_propAlignTransform, ContentAlignTransform);
                _propAlignTransformUseSelf.boolValue = false;
            }
            else
            {
                _propAlignTransformUseSelf.boolValue = true;
            }

            popup = EditorGUILayout.Popup(ContentDropProximityOptions, _propDropProximityTransformUseSelf.boolValue ? 0 : 1, new[] { new GUIContent("Use self transform"), new GUIContent("Use other transform") });

            if (popup == 1)
            {
                EditorGUILayout.PropertyField(_propDropProximityTransform, ContentDropProximityTransform);
                _propDropProximityTransformUseSelf.boolValue = false;
            }
            else
            {
                _propDropProximityTransformUseSelf.boolValue = true;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Activation/deactivation of objects for visual feedback:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propActivateOnCompatibleNear,       ContentActivateOnCompatibleNear);
            EditorGUILayout.PropertyField(_propActivateOnCompatibleNotNear,    ContentActivateOnCompatibleNotNear);
            EditorGUILayout.PropertyField(_propActivateOnHandNearAndGrabbable, ContentActivateOnHandNearAndGrabbable);
            EditorGUILayout.PropertyField(_propActivateOnPlaced,               ContentActivateOnPlaced);
            EditorGUILayout.PropertyField(_propActivateOnEmpty,                ContentActivateOnEmpty);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentCompatibleTags                 { get; } = new GUIContent("Compatible Tags",                     $"List of {nameof(UxrGrabbableObject)} tags that can be placed here. Other tags will not be able to be placed");
        private GUIContent ContentMaxPlaceDistance               { get; } = new GUIContent("Max Place Distance",                  "Maximum distance the grabbable object needs to be from the anchor to be placed");
        private GUIContent ContentDropAlignmentOptions           { get; } = new GUIContent("Drop Snap Transform",                 $"The {nameof(UxrGrabbableObject)} Place Snap Transform will try to align to these axes if the {nameof(UxrGrabbableObject)} options are enabled (AlignToHandGrabAxes and/or PlaceInHandGrabPivot are active)");
        private GUIContent ContentAlignTransform                 { get; } = new GUIContent("Snap Transform",                      $"The {nameof(UxrGrabbableObject)} Place Snap Transform will try to align to these axes if the {nameof(UxrGrabbableObject)} options are enabled (AlignToHandGrabAxes and/or PlaceInHandGrabPivot are active)");
        private GUIContent ContentDropProximityOptions           { get; } = new GUIContent("Drop Proximity Transform",            $"The distance from the {nameof(UxrGrabbableObject)} Place Proximity Position to this transform will be compared to know if it is close enough to be placed here");
        private GUIContent ContentDropProximityTransform         { get; } = new GUIContent("Proximity Transform",                 $"The distance from the {nameof(UxrGrabbableObject)} Place Proximity Position to this transform will be compared to know if it is close enough to be placed here");
        private GUIContent ContentActivateOnCompatibleNear       { get; } = new GUIContent("Activate On Compatible Near",         $"GameObject that will be enabled/disabled depending on if there is a grabbed compatible {nameof(UxrGrabbableObject)} near enough to be placed on it");
        private GUIContent ContentActivateOnCompatibleNotNear    { get; } = new GUIContent("Activate On Compatible Not Near",     $"GameObject that will be enabled/disabled depending on if there is a grabbed compatible {nameof(UxrGrabbableObject)} NOT near enough to be placed on it");
        private GUIContent ContentActivateOnHandNearAndGrabbable { get; } = new GUIContent("Activate On Hand Near And Grabbable", $"GameObject that will be enabled/disabled depending on if there is a {nameof(UxrGrabbableObject)} currently placed and a {nameof(UxrGrabber)} is close enough to grab it");
        private GUIContent ContentActivateOnPlaced               { get; } = new GUIContent("Activate On Placed",                  $"GameObject that will be enabled/disabled depending on if there is a {nameof(UxrGrabbableObject)} currently placed on it");
        private GUIContent ContentActivateOnEmpty                { get; } = new GUIContent("Activate On Empty",                   $"GameObject that will be enabled/disabled depending on if there is a {nameof(UxrGrabbableObject)} currently NOT placed on it");

        private SerializedProperty _propCompatibleTags;
        private SerializedProperty _propMaxPlaceDistance;
        private SerializedProperty _propAlignTransformUseSelf;
        private SerializedProperty _propAlignTransform;
        private SerializedProperty _propDropProximityTransformUseSelf;
        private SerializedProperty _propDropProximityTransform;
        private SerializedProperty _propActivateOnCompatibleNear;
        private SerializedProperty _propActivateOnCompatibleNotNear;
        private SerializedProperty _propActivateOnHandNearAndGrabbable;
        private SerializedProperty _propActivateOnPlaced;
        private SerializedProperty _propActivateOnEmpty;

        #endregion
    }
}