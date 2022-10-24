// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrKeyboardKeyEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.UI.Helpers.Keyboard;
using UnityEditor;

namespace UltimateXR.Editor.UI.Helpers.Keyboard
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrKeyboardKeyEditor" />.
    /// </summary>
    [CustomEditor(typeof(UxrKeyboardKeyUI))]
    [CanEditMultipleObjects]
    public class UxrKeyboardKeyEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyKeyType                        = serializedObject.FindProperty("_keyType");
            _propertyLayout                         = serializedObject.FindProperty("_layout");
            _propertyPrintShift                     = serializedObject.FindProperty("_printShift");
            _propertyPrintNoShift                   = serializedObject.FindProperty("_printNoShift");
            _propertyPrintAltGr                     = serializedObject.FindProperty("_printAltGr");
            _propertyForceLabel                     = serializedObject.FindProperty("_forceLabel");
            _propertySingleLayoutValue              = serializedObject.FindProperty("_singleLayoutValue");
            _propertyMultipleLayoutValueTopLeft     = serializedObject.FindProperty("_multipleLayoutValueTopLeft");
            _propertyMultipleLayoutValueBottomLeft  = serializedObject.FindProperty("_multipleLayoutValueBottomLeft");
            _propertyMultipleLayoutValueBottomRight = serializedObject.FindProperty("_multipleLayoutValueBottomRight");
            _propertyToggleSymbols                  = serializedObject.FindProperty("_toggleSymbols");
            _propertyNameDirty                      = serializedObject.FindProperty("_nameDirty");
        }

        /// <summary>
        ///     Draws the inspector and handles user input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            PropertyFieldWithChangeCheck(_propertyKeyType);

            if (_propertyKeyType.enumValueIndex == (int)UxrKeyType.Printable)
            {
                PropertyFieldWithChangeCheck(_propertyLayout);
                PropertyFieldWithChangeCheck(_propertyPrintShift);
                PropertyFieldWithChangeCheck(_propertyPrintNoShift);
                PropertyFieldWithChangeCheck(_propertyPrintAltGr);
            }

            PropertyFieldWithChangeCheck(_propertyForceLabel);

            if (_propertyKeyType.enumValueIndex == (int)UxrKeyType.Printable)
            {
                PropertyFieldWithChangeCheck(_propertySingleLayoutValue);
                PropertyFieldWithChangeCheck(_propertyMultipleLayoutValueTopLeft);
                PropertyFieldWithChangeCheck(_propertyMultipleLayoutValueBottomLeft);
                PropertyFieldWithChangeCheck(_propertyMultipleLayoutValueBottomRight);
            }

            if (_propertyKeyType.enumValueIndex == (int)UxrKeyType.ToggleSymbols)
            {
                PropertyFieldWithChangeCheck(_propertyToggleSymbols);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Draws the serialized property field and sets a dirty flag when the value changed. The dirty flag will tell the
        ///     <see cref="UxrKeyboardKeyUI" /> component that it should check whether to update the GameObject's name based the
        ///     function assigned to the key. This is because in order to handle the edition of many keys it comes in handy to
        ///     handle the object naming automatically based on the key's function.
        /// </summary>
        /// <param name="serializedProperty">Serialized property to process</param>
        private void PropertyFieldWithChangeCheck(SerializedProperty serializedProperty)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedProperty, true);
            if (EditorGUI.EndChangeCheck())
            {
                _propertyNameDirty.boolValue = true;
            }
        }

        #endregion

        #region Private Types & Data

        private SerializedProperty _propertyKeyType;
        private SerializedProperty _propertyLayout;
        private SerializedProperty _propertyPrintShift;
        private SerializedProperty _propertyPrintNoShift;
        private SerializedProperty _propertyPrintAltGr;
        private SerializedProperty _propertyForceLabel;
        private SerializedProperty _propertySingleLayoutValue;
        private SerializedProperty _propertyMultipleLayoutValueTopLeft;
        private SerializedProperty _propertyMultipleLayoutValueBottomLeft;
        private SerializedProperty _propertyMultipleLayoutValueBottomRight;
        private SerializedProperty _propertyToggleSymbols;
        private SerializedProperty _propertyNameDirty;

        #endregion
    }
}