// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPointerInputModuleEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI.UnityInputModule;
using UnityEditor;

namespace UltimateXR.Editor.UI
{
    /// <summary>
    ///     Custom editor for <see cref="UxrPointerInputModule" />.
    /// </summary>
    [CustomEditor(typeof(UxrPointerInputModule))]
    public class UxrPointerInputModuleEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyDisableOtherInputModules    = serializedObject.FindProperty("_disableOtherInputModules");
            _propertyAutoEnableOnWorldCanvases   = serializedObject.FindProperty("_autoEnableOnWorldCanvases");
            _propertyAutoAssignEventCamera       = serializedObject.FindProperty("_autoAssignEventCamera");
            _propertyInteractionTypeOnAutoEnable = serializedObject.FindProperty("_interactionTypeOnAutoEnable");
            _propertyFingerTipMinHoverDistance   = serializedObject.FindProperty("_fingerTipMinHoverDistance");
            _propertyDragThreshold               = serializedObject.FindProperty("_dragThreshold");
        }

        /// <summary>
        ///     Draws the custom inspector to show additional help.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("If Auto Enable World Canvases is enabled below, the input module will automatically enable VR interaction on world-space canvases that have not been manually set up by adding them a UxrCanvas component.\n\n" +
                                    "Two types of interaction are supported: finger tips, where the user will be able to interact with the elements by touching them with the hands, and laser pointers, where the user will be able to interact with the elements by pointing at them with a laser and pressing a controller button.",
                                    MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_propertyDisableOtherInputModules);
            EditorGUILayout.PropertyField(_propertyAutoEnableOnWorldCanvases);
            EditorGUILayout.PropertyField(_propertyAutoAssignEventCamera);
            EditorGUILayout.PropertyField(_propertyInteractionTypeOnAutoEnable);

            if (_propertyInteractionTypeOnAutoEnable.enumNames[_propertyInteractionTypeOnAutoEnable.enumValueIndex] == UxrInteractionType.FingerTips.ToString())
            {
                EditorGUILayout.PropertyField(_propertyFingerTipMinHoverDistance);
            }

            EditorGUILayout.PropertyField(_propertyDragThreshold);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private SerializedProperty _propertyDisableOtherInputModules;
        private SerializedProperty _propertyAutoEnableOnWorldCanvases;
        private SerializedProperty _propertyAutoAssignEventCamera;
        private SerializedProperty _propertyInteractionTypeOnAutoEnable;
        private SerializedProperty _propertyFingerTipMinHoverDistance;
        private SerializedProperty _propertyDragThreshold;

        #endregion
    }
}