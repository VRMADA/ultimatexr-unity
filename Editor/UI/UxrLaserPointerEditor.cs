// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLaserPointerEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Extensions.System.Math;
using UltimateXR.UI;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.UI
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrLaserPointer" />.
    /// </summary>
    [CustomEditor(typeof(UxrLaserPointer))]
    [CanEditMultipleObjects]
    public class UxrLaserPointerEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            // General parameters
            _propertyHandSide             = serializedObject.FindProperty("_handSide");
            _propertyUseControllerForward = serializedObject.FindProperty("_useControllerForward");

            // Interaction
            _propertyTargetTypes                 = serializedObject.FindProperty("_targetTypes");
            _propertyBlockingMask                = serializedObject.FindProperty("_blockingMask");
            _propertyTriggerCollidersInteraction = serializedObject.FindProperty("_triggerCollidersInteraction");
            
            // Input parameters
            _propertyClickInput                = serializedObject.FindProperty("_clickInput");
            _propertyShowLaserInput            = serializedObject.FindProperty("_showLaserInput");
            _propertyShowLaserButtonEvent      = serializedObject.FindProperty("_showLaserButtonEvent");
            
            // Laser appearance
            _propertyInvisible                   = serializedObject.FindProperty("_invisible");
            _propertyRayLength                   = serializedObject.FindProperty("_rayLength");
            _propertyRayWidth                    = serializedObject.FindProperty("_rayWidth");
            _propertyRayColorInteractive         = serializedObject.FindProperty("_rayColorInteractive");
            _propertyRayColorNonInteractive      = serializedObject.FindProperty("_rayColorNonInteractive");
            _propertyRayHitMaterial              = serializedObject.FindProperty("_rayHitMaterial");
            _propertyRayHitSize                  = serializedObject.FindProperty("_rayHitSize");
            _propertyOptionalEnableWhenLaserOn   = serializedObject.FindProperty("_optionalEnableWhenLaserOn");

            if (_propertyRayHitMaterial.objectReferenceValue == null)
            {
                string laserDotMaterialAssetPath = AssetDatabase.GUIDToAssetPath(LaserDotMaterialGuid);

                if (!string.IsNullOrEmpty(laserDotMaterialAssetPath))
                {
                    Material dotMaterial = AssetDatabase.LoadAssetAtPath<Material>(laserDotMaterialAssetPath);
                    _propertyRayHitMaterial.objectReferenceValue = dotMaterial;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        /// <summary>
        ///     Draws the UI and gathers user input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            _foldoutGeneral = UxrEditorUtils.FoldoutStylish("General", _foldoutGeneral);

            if (_foldoutGeneral)
            {
                EditorGUILayout.PropertyField(_propertyHandSide,             new GUIContent("Hand",                   "Selects which controller will be used to control the laser"));
                EditorGUILayout.PropertyField(_propertyUseControllerForward, new GUIContent("Use Controller Forward", "When the avatar is rendered in controllers mode, whether to use the controller's forward vector instead of the GameObject's forward vector for the laser direction"));
            }

            _foldoutInteraction = UxrEditorUtils.FoldoutStylish("Interaction", _foldoutInteraction);

            if (_foldoutInteraction)
            {
                EditorGUILayout.PropertyField(_propertyTargetTypes, new GUIContent("Target Types", "Selects which target types the laser pointer will interact with"));

                if (_propertyTargetTypes.intValue.HasFlags((int)UxrLaserPointerTargetTypes.Colliders3D))
                {
                    EditorGUILayout.PropertyField(_propertyTriggerCollidersInteraction, new GUIContent("Trigger Colliders Interaction", "Whether colliders with the trigger property set will interact with the laser pointer"));
                }

                if (_propertyTargetTypes.intValue.HasFlags((int)UxrLaserPointerTargetTypes.Colliders2D) ||
                    _propertyTargetTypes.intValue.HasFlags((int)UxrLaserPointerTargetTypes.Colliders3D))
                {
                    EditorGUILayout.PropertyField(_propertyBlockingMask, new GUIContent("Blocking Mask", "Which layers will block the laser pointer for 2D/3D GameObjects"));
                }
            }

            _foldoutInput = UxrEditorUtils.FoldoutStylish("Input", _foldoutInput);

            if (_foldoutInput)
            {
                EditorGUILayout.PropertyField(_propertyClickInput,           new GUIContent("Click Input",               "Tells which controller button will be used to perform clicks on UI elements"));
                EditorGUILayout.PropertyField(_propertyShowLaserInput,       new GUIContent("Enable Laser Input",        "Selects which controller button will be used to enable the laser"));
                EditorGUILayout.PropertyField(_propertyShowLaserButtonEvent, new GUIContent("Enable Laser Button Event", "Tells which controller button input event will be needed to enable the laser"));
            }

            _foldoutAppearance = UxrEditorUtils.FoldoutStylish("Appearance", _foldoutAppearance);

            if (_foldoutAppearance)
            {
                EditorGUILayout.PropertyField(_propertyInvisible, new GUIContent("Invisible", "Whether not to render the ray but still perform raycasts and interaction"));

                if (_propertyInvisible.boolValue == false)
                {
                    EditorGUILayout.PropertyField(_propertyRayLength,              new GUIContent("Ray Length",                "Laser ray length"));
                    EditorGUILayout.PropertyField(_propertyRayWidth,               new GUIContent("Ray Width",                 "Laser ray width"));
                    EditorGUILayout.PropertyField(_propertyRayColorInteractive,    new GUIContent("Ray Color Interactive",     "Laser color when hovering over interactive UI elements"));
                    EditorGUILayout.PropertyField(_propertyRayColorNonInteractive, new GUIContent("Ray Color Non-Interactive", "Laser color when hovering over non-interactive UI elements"));
                    EditorGUILayout.PropertyField(_propertyRayHitMaterial,         new GUIContent("Ray Hit Material",          "Material that will be used to render the quad representing the hit with the scenario or UI elements"));
                    EditorGUILayout.PropertyField(_propertyRayHitSize,             new GUIContent("Ray Hit Size",              "Size of the quad representing the hit with the scenario or UI elements"));
                }

                EditorGUILayout.PropertyField(_propertyOptionalEnableWhenLaserOn, new GUIContent("Optionally Enable Object", "Optional additional object that will be enabled/disabled at the same time the laser is enabled or disabled"));
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private const string LaserDotMaterialGuid = "5796fb89729c636469d8bd446231c1a0";

        private SerializedProperty _propertyHandSide;
        private SerializedProperty _propertyUseControllerForward;

        private SerializedProperty _propertyTargetTypes;
        private SerializedProperty _propertyBlockingMask;
        private SerializedProperty _propertyTriggerCollidersInteraction;
        
        private SerializedProperty _propertyClickInput;
        private SerializedProperty _propertyShowLaserInput;
        private SerializedProperty _propertyShowLaserButtonEvent;
        
        private SerializedProperty _propertyInvisible;
        private SerializedProperty _propertyRayLength;
        private SerializedProperty _propertyRayWidth;
        private SerializedProperty _propertyRayColorInteractive;
        private SerializedProperty _propertyRayColorNonInteractive;
        private SerializedProperty _propertyRayHitMaterial;
        private SerializedProperty _propertyRayHitSize;
        private SerializedProperty _propertyOptionalEnableWhenLaserOn;

        private bool _foldoutGeneral     = true;
        private bool _foldoutInteraction = true;
        private bool _foldoutInput       = true;
        private bool _foldoutAppearance  = true;

        #endregion
    }
}