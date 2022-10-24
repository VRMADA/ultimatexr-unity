// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleControlInputEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEditor;

namespace UltimateXR.Editor.UI.UnityInputModule.Controls
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrToggleControlInput" />. Needs to inherit from
    ///     <see cref="UxrControlInputEditor" />.
    /// </summary>
    [CustomEditor(typeof(UxrToggleControlInput))]
    [CanEditMultipleObjects]
    public class UxrToggleControlInputEditor : UxrControlInputEditor
    {
        #region Unity

        /// <summary>
        ///     Caches serialized properties.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _propertyInitialStateIsSelected = serializedObject.FindProperty("_initialStateIsSelected");
            _propertyCanToggleOnlyOnce      = serializedObject.FindProperty("_canToggleOnlyOnce");
            _propertyText                   = serializedObject.FindProperty("_text");
            _propertyEnableWhenSelected     = serializedObject.FindProperty("_enableWhenSelected");
            _propertyEnableWhenNotSelected  = serializedObject.FindProperty("_enableWhenNotSelected");
            _propertyTextColorChanges       = serializedObject.FindProperty("_textColorChanges");
            _propertyAudioToggleOn          = serializedObject.FindProperty("_audioToggleOn");
            _propertyAudioToggleOff         = serializedObject.FindProperty("_audioToggleOff");
            _propertyAudioToggleOnVolume    = serializedObject.FindProperty("_audioToggleOnVolume");
            _propertyAudioToggleOffVolume   = serializedObject.FindProperty("_audioToggleOffVolume");
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Draws the inspector for the child properties.
        /// </summary>
        protected override void OnControlInputInspectorGUI()
        {
            EditorGUILayout.PropertyField(_propertyInitialStateIsSelected);
            EditorGUILayout.PropertyField(_propertyCanToggleOnlyOnce);
            EditorGUILayout.PropertyField(_propertyText);
            EditorGUILayout.PropertyField(_propertyEnableWhenSelected);
            EditorGUILayout.PropertyField(_propertyEnableWhenNotSelected);
            EditorGUILayout.PropertyField(_propertyTextColorChanges);
            EditorGUILayout.PropertyField(_propertyAudioToggleOn);
            EditorGUILayout.PropertyField(_propertyAudioToggleOff);
            EditorGUILayout.PropertyField(_propertyAudioToggleOnVolume);
        }

        #endregion

        #region Private Types & Data

        private SerializedProperty _propertyInitialStateIsSelected;
        private SerializedProperty _propertyCanToggleOnlyOnce;
        private SerializedProperty _propertyText;
        private SerializedProperty _propertyEnableWhenSelected;
        private SerializedProperty _propertyEnableWhenNotSelected;
        private SerializedProperty _propertyTextColorChanges;
        private SerializedProperty _propertyAudioToggleOn;
        private SerializedProperty _propertyAudioToggleOff;
        private SerializedProperty _propertyAudioToggleOnVolume;
        private SerializedProperty _propertyAudioToggleOffVolume;

        #endregion
    }
}