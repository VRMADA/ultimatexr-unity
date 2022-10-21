// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLaserPointerEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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
            _propertyHandSide   = serializedObject.FindProperty("_handSide");
            _propertyClickInput = serializedObject.FindProperty("_clickInput");

            _propertyShowLaserInput            = serializedObject.FindProperty("_showLaserInput");
            _propertyShowLaserButtonEvent      = serializedObject.FindProperty("_showLaserButtonEvent");
            _propertyOptionalEnableWhenLaserOn = serializedObject.FindProperty("_optionalEnableWhenLaserOn");

            _propertyUseControllerForward   = serializedObject.FindProperty("_useControllerForward");
            _propertyInvisible              = serializedObject.FindProperty("_invisible");
            _propertyRayLength              = serializedObject.FindProperty("_rayLength");
            _propertyRayWidth               = serializedObject.FindProperty("_rayWidth");
            _propertyRayColorInteractive    = serializedObject.FindProperty("_rayColorInteractive");
            _propertyRayColorNonInteractive = serializedObject.FindProperty("_rayColorNonInteractive");
            _propertyRayHitMaterial         = serializedObject.FindProperty("_rayHitMaterial");
            _propertyRayHitSize             = serializedObject.FindProperty("_rayHitSize");

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

            EditorGUILayout.LabelField("General properties:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propertyHandSide,   new GUIContent("Hand",        "Selects which controller will be used to control the laser"));
            EditorGUILayout.PropertyField(_propertyClickInput, new GUIContent("Click Input", "Tells which controller button will be used to perform clicks on UI elements"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Laser enabling:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propertyShowLaserInput,            new GUIContent("Enable Laser Input",        "Selects which controller button will be used to enable the laser"));
            EditorGUILayout.PropertyField(_propertyShowLaserButtonEvent,      new GUIContent("Enable Laser Button Event", "Tells which controller button input event will be needed to enable the laser"));
            EditorGUILayout.PropertyField(_propertyOptionalEnableWhenLaserOn, new GUIContent("Optionally Enable Object",  "Optional additional object that will be enabled/disabled at the same time the laser is enabled or disabled"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Laser rendering:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propertyUseControllerForward, new GUIContent("Use Controller Forward", "When the avatar is rendered in controllers mode, will the laser use the controller's forward vector instead of its own?"));
            EditorGUILayout.PropertyField(_propertyInvisible,            new GUIContent("Invisible",              "Should the laser be invisible? This does not affect any raycasts, they still would be performed but the ray itself will be invisible"));

            if (_propertyInvisible.boolValue == false)
            {
                EditorGUILayout.PropertyField(_propertyRayLength,              new GUIContent("Ray Length",                "Laser ray length"));
                EditorGUILayout.PropertyField(_propertyRayWidth,               new GUIContent("Ray Width",                 "Laser ray width"));
                EditorGUILayout.PropertyField(_propertyRayColorInteractive,    new GUIContent("Ray Color Interactive",     "Laser color when hovering over interactive UI elements"));
                EditorGUILayout.PropertyField(_propertyRayColorNonInteractive, new GUIContent("Ray Color Non-Interactive", "Laser color when hovering over non-interactive UI elements"));
                EditorGUILayout.PropertyField(_propertyRayHitMaterial,         new GUIContent("Ray Hit Material",          "Material that will be used to render the quad representing the hit with the scenario or UI elements"));
                EditorGUILayout.PropertyField(_propertyRayHitSize,             new GUIContent("Ray Hit Size",              "Size of the quad representing the hit with the scenario or UI elements"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private const string LaserDotMaterialGuid = "5796fb89729c636469d8bd446231c1a0";

        private SerializedProperty _propertyHandSide;
        private SerializedProperty _propertyClickInput;

        private SerializedProperty _propertyShowLaserInput;
        private SerializedProperty _propertyShowLaserButtonEvent;
        private SerializedProperty _propertyOptionalEnableWhenLaserOn;

        private SerializedProperty _propertyUseControllerForward;
        private SerializedProperty _propertyInvisible;
        private SerializedProperty _propertyRayLength;
        private SerializedProperty _propertyRayWidth;
        private SerializedProperty _propertyRayColorInteractive;
        private SerializedProperty _propertyRayColorNonInteractive;
        private SerializedProperty _propertyRayHitMaterial;
        private SerializedProperty _propertyRayHitSize;

        #endregion
    }
}