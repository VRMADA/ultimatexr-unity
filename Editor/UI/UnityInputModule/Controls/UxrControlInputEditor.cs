// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControlInputEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEditor;
using UnityEditor.EventSystems;

namespace UltimateXR.Editor.UI.UnityInputModule.Controls
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrControlInput" />. Needs to inherit from <see cref="EventTriggerEditor" />
    ///     because <see cref="UxrControlInput" /> is an EventTrigger-derived component.
    /// </summary>
    [CustomEditor(typeof(UxrControlInput))]
    [CanEditMultipleObjects]
    public class UxrControlInputEditor : EventTriggerEditor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties.
        /// </summary>
        protected override void OnEnable()
        {
            _propertyPressAndHoldDuration = serializedObject.FindProperty("_pressAndHoldDuration");
            _propertyFeedbackOnDown       = serializedObject.FindProperty("_feedbackOnPress");
            _propertyFeedbackOnUp         = serializedObject.FindProperty("_feedbackOnRelease");
            _propertyFeedbackOnClick      = serializedObject.FindProperty("_feedbackOnClick");
        }

        /// <summary>
        ///     Draws the custom inspector, including the one implemented in child classes.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_propertyPressAndHoldDuration);
            EditorGUILayout.PropertyField(_propertyFeedbackOnDown,  true);
            EditorGUILayout.PropertyField(_propertyFeedbackOnUp,    true);
            EditorGUILayout.PropertyField(_propertyFeedbackOnClick, true);

            // Child properties
            OnControlInputInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Overridable method to draw child properties.
        /// </summary>
        protected virtual void OnControlInputInspectorGUI()
        {
        }

        #endregion

        #region Private Types & Data

        private SerializedProperty _propertyPressAndHoldDuration;
        private SerializedProperty _propertyFeedbackOnDown;
        private SerializedProperty _propertyFeedbackOnUp;
        private SerializedProperty _propertyFeedbackOnClick;

        #endregion
    }
}