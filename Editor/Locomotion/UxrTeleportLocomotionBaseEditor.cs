// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTeleportLocomotionBaseEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Locomotion;
using UnityEditor;
using UnityEngine;

#pragma warning disable 0414

namespace UltimateXR.Editor.Locomotion
{
    /// <summary>
    ///     Base class for custom teleport locomotion components.
    /// </summary>
    public abstract class UxrTeleportLocomotionBaseEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Creates references to the serialized properties
        /// </summary>
        protected virtual void OnEnable()
        {
            _propControllerHand           = serializedObject.FindProperty("_controllerHand");
            _propUseControllerForward     = serializedObject.FindProperty("_useControllerForward");
            _propParentToDestination      = serializedObject.FindProperty("_parentToDestination");
            _propShakeFilter              = serializedObject.FindProperty("_shakeFilter");
            _propTranslationType          = serializedObject.FindProperty("_translationType");
            _propFadeTranslationColor     = serializedObject.FindProperty("_fadeTranslationColor");
            _propFadeTranslationSeconds   = serializedObject.FindProperty("_fadeTranslationSeconds");
            _propSmoothTranslationSeconds = serializedObject.FindProperty("_smoothTranslationSeconds");
            _propAllowJoystickBackStep    = serializedObject.FindProperty("_allowJoystickBackStep");
            _propBackStepDistance         = serializedObject.FindProperty("_backStepDistance");
            _propRotationType             = serializedObject.FindProperty("_rotationType");
            _propRotationStepDegrees      = serializedObject.FindProperty("_rotationStepDegrees");
            _propFadeRotationColor        = serializedObject.FindProperty("_fadeRotationColor");
            _propFadeRotationSeconds      = serializedObject.FindProperty("_fadeRotationSeconds");
            _propSmoothRotationSeconds    = serializedObject.FindProperty("_smoothRotationSeconds");
            _propReorientationType        = serializedObject.FindProperty("_reorientationType");

            _propTarget                      = serializedObject.FindProperty("_target");
            _propTargetPlacementAboveHit     = serializedObject.FindProperty("_targetPlacementAboveHit");
            _propShowTargetAlsoWhenInvalid   = serializedObject.FindProperty("_showTargetAlsoWhenInvalid");
            _propValidMaterialColorTargets   = serializedObject.FindProperty("_validMaterialColorTargets");
            _propInvalidMaterialColorTargets = serializedObject.FindProperty("_invalidMaterialColorTargets");

            _propTriggerCollidersInteraction = serializedObject.FindProperty("_triggerCollidersInteraction");
            _propMaxAllowedDistance          = serializedObject.FindProperty("_maxAllowedDistance");
            _propMaxAllowedHeightDifference  = serializedObject.FindProperty("_maxAllowedHeightDifference");
            _propMaxAllowedSlopeDegrees      = serializedObject.FindProperty("_maxAllowedSlopeDegrees");
            _propDestinationValidationRadius = serializedObject.FindProperty("_destinationValidationRadius");
            _propValidTargetLayers           = serializedObject.FindProperty("_validTargetLayers");
            _propBlockingTargetLayers        = serializedObject.FindProperty("_blockingTargetLayers");
        }

        /// <summary>
        ///     Draws the custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            _foldoutGeneral = UxrEditorUtils.FoldoutStylish("General Parameters", _foldoutGeneral);

            if (_foldoutGeneral)
            {
                EditorGUILayout.PropertyField(_propControllerHand,       ContentControllerHand);
                EditorGUILayout.PropertyField(_propUseControllerForward, ContentUseControllerForward);
                EditorGUILayout.PropertyField(_propParentToDestination,  ContentParentToDestination);
                EditorGUILayout.Slider(_propShakeFilter, 0.0f, 1.0f, ContentShakeFilter);
            }

            _foldoutTranslation = UxrEditorUtils.FoldoutStylish("Translation", _foldoutTranslation);

            if (_foldoutTranslation)
            {
                EditorGUILayout.PropertyField(_propTranslationType, ContentTranslationType);

                if (_propTranslationType.enumValueIndex == (int)UxrTranslationType.Smooth && _propReorientationType.enumValueIndex != (int)UxrReorientationType.KeepOrientation)
                {
                    EditorGUILayout.HelpBox("For smooth translation it is recommended to use Keep Orientation as Reorient After Teleport parameter in the Rotation settings", MessageType.Warning);
                }

                if (_propTranslationType.enumValueIndex == (int)UxrTranslationType.Fade)
                {
                    EditorGUILayout.PropertyField(_propFadeTranslationColor, ContentFadeTranslationColor);
                    EditorGUILayout.Slider(_propFadeTranslationSeconds, 0.01f, 2.0f, ContentFadeTranslationSeconds);
                }
                else if (_propTranslationType.enumValueIndex == (int)UxrTranslationType.Smooth)
                {
                    EditorGUILayout.Slider(_propSmoothTranslationSeconds, 0.01f, 2.0f, ContentSmoothTranslationSeconds);
                }

                EditorGUILayout.PropertyField(_propAllowJoystickBackStep, ContentAllowJoystickBackStep);
                EditorGUILayout.PropertyField(_propBackStepDistance,      ContentBackStepDistance);
            }

            _foldoutRotation = UxrEditorUtils.FoldoutStylish("Rotation", _foldoutRotation);

            if (_foldoutRotation)
            {
                EditorGUILayout.PropertyField(_propRotationType, ContentRotationType);

                if (_propRotationType.enumValueIndex != (int)UxrRotationType.NotAllowed)
                {
                    EditorGUILayout.Slider(_propRotationStepDegrees, 10.0f, 180.0f, ContentRotationStepDegrees);
                }

                if (_propRotationType.enumValueIndex == (int)UxrRotationType.Fade)
                {
                    EditorGUILayout.PropertyField(_propFadeRotationColor, ContentFadeRotationColor);
                    EditorGUILayout.Slider(_propFadeRotationSeconds, 0.01f, 2.0f, ContentFadeRotationSeconds);
                }

                if (_propRotationType.enumValueIndex == (int)UxrRotationType.Smooth)
                {
                    EditorGUILayout.Slider(_propSmoothRotationSeconds, 0.01f, 2.0f, ContentSmoothRotationSeconds);
                }

                EditorGUILayout.PropertyField(_propReorientationType, ContentReorientationType);
            }

            EditorGUILayout.Space();

            _foldoutTarget = UxrEditorUtils.FoldoutStylish("Target", _foldoutTarget);

            if (_foldoutTarget)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_propTarget, ContentTarget);

                EditorGUILayout.Slider(_propTargetPlacementAboveHit, 0.0f, 1.0f, ContentTargetPlacementAboveHit);
                EditorGUILayout.PropertyField(_propShowTargetAlsoWhenInvalid, ContentShowTargetAlsoWhenInvalid);

                if (_propShowTargetAlsoWhenInvalid.boolValue)
                {
                    EditorGUILayout.PropertyField(_propValidMaterialColorTargets,   ContentValidMaterialColorTargets);
                    EditorGUILayout.PropertyField(_propInvalidMaterialColorTargets, ContentInvalidMaterialColorTargets);
                }
            }

            EditorGUILayout.Space();

            _foldoutConstraints = UxrEditorUtils.FoldoutStylish("Constraints", _foldoutConstraints);

            if (_foldoutConstraints)
            {
                EditorGUILayout.PropertyField(_propTriggerCollidersInteraction, ContentTriggerCollidersInteraction);
                EditorGUILayout.PropertyField(_propMaxAllowedDistance,          ContentMaxAllowedDistance);
                EditorGUILayout.PropertyField(_propMaxAllowedHeightDifference,  ContentMaxAllowedHeightDifference);
                EditorGUILayout.Slider(_propMaxAllowedSlopeDegrees, 0.0f, 90.0f, ContentMaxAllowedSlopeDegrees);
                EditorGUILayout.PropertyField(_propDestinationValidationRadius, ContentDestinationValidationRadius);
                EditorGUILayout.PropertyField(_propValidTargetLayers,           ContentValidTargetLayers);
                EditorGUILayout.PropertyField(_propBlockingTargetLayers,        ContentBlockingTargetLayers);
            }

            OnTeleportInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Event Trigger Methods

        protected abstract void OnTeleportInspectorGUI();

        #endregion

        #region Private Types & Data

        private GUIContent ContentControllerHand              { get; } = new GUIContent("Controller Hand",               "Which hand controls the input");
        private GUIContent ContentUseControllerForward        { get; } = new GUIContent("Use Controller Forward",        "Will the teleport use the controller's forward vector instead of its own transform forward?");
        private GUIContent ContentParentToDestination         { get; } = new GUIContent("Parent To Destination",         "Whether to parent the avatar to the destination object after teleport. Use it when building applications with moving vehicles or platforms the avatar can move on, so that the avatar keeps the relative position/orientation after teleporting.");
        private GUIContent ContentShakeFilter                 { get; } = new GUIContent("Shake Filter",                  "The amount of filtering to apply to the hand movement to smooth it out");
        private GUIContent ContentTranslationType             { get; } = new GUIContent("Translation Type",              "Which translation method to use");
        private GUIContent ContentFadeTranslationColor        { get; } = new GUIContent("Translation Fade Color",        "The fade color when Fade translation type is used");
        private GUIContent ContentFadeTranslationSeconds      { get; } = new GUIContent("Translation Fade Seconds",      "The fade transition in seconds when Fade translation type is used");
        private GUIContent ContentSmoothTranslationSeconds    { get; } = new GUIContent("Smooth Translation Seconds",    "The translation duration in seconds when Smooth translation type is used");
        private GUIContent ContentAllowJoystickBackStep       { get; } = new GUIContent("Allow Joystick Back Step",      "Whether to allow back steps by pressing the joystick down");
        private GUIContent ContentBackStepDistance            { get; } = new GUIContent("Back Step Distance",            "The distance of each back step");
        private GUIContent ContentRotationType                { get; } = new GUIContent("Rotation Type",                 "Which rotation method to use");
        private GUIContent ContentRotationStepDegrees         { get; } = new GUIContent("Rotation Step Degrees",         "The amount of degrees of each turn for Immediate and Fade rotations");
        private GUIContent ContentFadeRotationColor           { get; } = new GUIContent("Rotation Fade Color",           "The fade color when Fade rotation is used");
        private GUIContent ContentFadeRotationSeconds         { get; } = new GUIContent("Rotation Fade Seconds",         "The fade transition in seconds when Fade rotation is used");
        private GUIContent ContentSmoothRotationSeconds       { get; } = new GUIContent("Smooth Rotation Seconds",       "The rotation duration in seconds when Smooth rotation is used");
        private GUIContent ContentReorientationType           { get; } = new GUIContent("Reorient After Teleport",       "How to orient the view right after teleporting");
        private GUIContent ContentTarget                      { get; } = new GUIContent("Target",                        "Which teleport target to use. Can be either a prefab or an already instantiated object");
        private GUIContent ContentTargetPlacementAboveHit     { get; } = new GUIContent("Target Placement Above Floor",  "Offset applied to the teleport target to help placing it a little above the floor");
        private GUIContent ContentShowTargetAlsoWhenInvalid   { get; } = new GUIContent("Show Target Also When Invalid", "Whether to show the target object also when the destination is not valid");
        private GUIContent ContentValidMaterialColorTargets   { get; } = new GUIContent("Target Color When Valid",       "Target color to use when the destination is valid");
        private GUIContent ContentInvalidMaterialColorTargets { get; } = new GUIContent("Target Color When Invalid",     "Target color to use when the destination is not valid");
        private GUIContent ContentTriggerCollidersInteraction { get; } = new GUIContent("Trigger Colliders Interaction", "Whether colliders with the trigger property set will interact with the teleport raycasts");
        private GUIContent ContentMaxAllowedDistance          { get; } = new GUIContent("Max Allowed Distance Travel",   "Maximum allowed distance that can be travelled using each teleport");
        private GUIContent ContentMaxAllowedHeightDifference  { get; } = new GUIContent("Max Allowed Height Difference", "Maximum allowed height difference to be able to teleport");
        private GUIContent ContentMaxAllowedSlopeDegrees      { get; } = new GUIContent("Max Allowed Slope Degrees",     "Maximum allowed slope degrees at destination");
        private GUIContent ContentDestinationValidationRadius { get; } = new GUIContent("Destination Validation Radius", "Radius of a cylinder that will be used to validate the destination surroundings to allow teleporting");
        private GUIContent ContentValidTargetLayers           { get; } = new GUIContent("Valid Target Layers",           "Valid layers for teleporting destination objects");
        private GUIContent ContentBlockingTargetLayers        { get; } = new GUIContent("Blocking Target Layers",        "Objects that will block teleporting raycasts");

        private SerializedProperty _propControllerHand;
        private SerializedProperty _propUseControllerForward;
        private SerializedProperty _propParentToDestination;
        private SerializedProperty _propShakeFilter;
        private SerializedProperty _propTranslationType;
        private SerializedProperty _propFadeTranslationColor;
        private SerializedProperty _propFadeTranslationSeconds;
        private SerializedProperty _propSmoothTranslationSeconds;
        private SerializedProperty _propAllowJoystickBackStep;
        private SerializedProperty _propBackStepDistance;
        private SerializedProperty _propRotationType;
        private SerializedProperty _propRotationStepDegrees;
        private SerializedProperty _propFadeRotationColor;
        private SerializedProperty _propFadeRotationSeconds;
        private SerializedProperty _propSmoothRotationSeconds;
        private SerializedProperty _propReorientationType;

        private SerializedProperty _propTarget;
        private SerializedProperty _propTargetPlacementAboveHit;
        private SerializedProperty _propShowTargetAlsoWhenInvalid;
        private SerializedProperty _propValidMaterialColorTargets;
        private SerializedProperty _propInvalidMaterialColorTargets;

        private SerializedProperty _propTriggerCollidersInteraction;
        private SerializedProperty _propMaxAllowedDistance;
        private SerializedProperty _propMaxAllowedHeightDifference;
        private SerializedProperty _propMaxAllowedSlopeDegrees;
        private SerializedProperty _propDestinationValidationRadius;
        private SerializedProperty _propValidTargetLayers;
        private SerializedProperty _propBlockingTargetLayers;

        private bool _foldoutGeneral     = true;
        private bool _foldoutTranslation = true;
        private bool _foldoutRotation    = true;
        private bool _foldoutTarget      = true;
        private bool _foldoutConstraints = true;

        #endregion
    }
}

#pragma warning restore 0414