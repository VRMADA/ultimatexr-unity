// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrController3DModelEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Devices.Visualization;
using UnityEditor;

namespace UltimateXR.Editor.Devices
{
    /// <summary>
    ///     Custom Unity editor for the <see cref="UxrController3DModel" /> component.
    /// </summary>
    [CustomEditor(typeof(UxrController3DModel))]
    public class UxrController3DModelEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties.
        /// </summary>
        public void OnEnable()
        {
            _propertyNeedsBothHands      = serializedObject.FindProperty("_needsBothHands");
            _propertyHandSide            = serializedObject.FindProperty("_handSide");
            _propertyControllerHand      = serializedObject.FindProperty("_controllerHand");
            _propertyControllerHandLeft  = serializedObject.FindProperty("_controllerHandLeft");
            _propertyControllerHandRight = serializedObject.FindProperty("_controllerHandRight");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_propertyNeedsBothHands);

            if (_propertyNeedsBothHands.boolValue)
            {
                EditorGUILayout.PropertyField(_propertyControllerHandLeft);
                EditorGUILayout.PropertyField(_propertyControllerHandRight);
            }
            else
            {
                EditorGUILayout.PropertyField(_propertyHandSide);
                EditorGUILayout.PropertyField(_propertyControllerHand);
            }

            // Rest of inspector

            DrawPropertiesExcluding(serializedObject, "m_Script", "_needsBothHands", "_handSide", "_controllerHand", "_controllerHandLeft", "_controllerHandRight");

            // Apply modified properties if necessary

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private SerializedProperty _propertyNeedsBothHands;
        private SerializedProperty _propertyHandSide;
        private SerializedProperty _propertyControllerHand;
        private SerializedProperty _propertyControllerHandLeft;
        private SerializedProperty _propertyControllerHandRight;

        #endregion
    }
}