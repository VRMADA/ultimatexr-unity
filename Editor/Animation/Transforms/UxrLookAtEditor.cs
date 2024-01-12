// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLookAtEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Transforms;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.Transforms
{
    /// <summary>
    ///     Custom inspector for <see cref="UxrLookAt" />.
    /// </summary>
    [CustomEditor(typeof(UxrLookAt))]
    public class UxrLookAtEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyMode                   = serializedObject.FindProperty("_mode");
            _propertyTarget                 = serializedObject.FindProperty("_target");
            _propertyLookAxis               = serializedObject.FindProperty("_lookAxis");
            _propertyUpAxis                 = serializedObject.FindProperty("_upAxis");
            _propertyMatchDirection         = serializedObject.FindProperty("_matchDirection");
            _propertyAllowRotateAroundUp    = serializedObject.FindProperty("_allowRotateAroundUp");
            _propertyAllowRotateAroundRight = serializedObject.FindProperty("_allowRotateAroundRight");
            _propertyInvertedLookAxis       = serializedObject.FindProperty("_invertedLookAxis");
            _propertyOnlyOnce               = serializedObject.FindProperty("_onlyOnce");
        }

        /// <summary>
        ///     Draws the inspector and handles input.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_propertyMode, ContentMode);

            if (_propertyMode.enumValueIndex == (int)UxrLookAtMode.Target || _propertyMode.enumValueIndex == (int)UxrLookAtMode.MatchTargetDirection)
            {
                EditorGUILayout.PropertyField(_propertyTarget, ContentTarget);
            }

            EditorGUILayout.PropertyField(_propertyLookAxis, ContentLookAxis);
            EditorGUILayout.PropertyField(_propertyUpAxis,   ContentUpAxis);

            if (_propertyMode.enumValueIndex == (int)UxrLookAtMode.MatchWorldDirection || _propertyMode.enumValueIndex == (int)UxrLookAtMode.MatchTargetDirection)
            {
                EditorGUILayout.PropertyField(_propertyMatchDirection, ContentMatchDirection);
            }

            if (_propertyMode.enumValueIndex == (int)UxrLookAtMode.Target)
            {
                EditorGUILayout.PropertyField(_propertyAllowRotateAroundUp,    ContentAllowRotateAroundUp);
                EditorGUILayout.PropertyField(_propertyAllowRotateAroundRight, ContentAllowRotateAroundRight);
                EditorGUILayout.PropertyField(_propertyInvertedLookAxis,       ContentInvertedLookAxis);
            }

            EditorGUILayout.PropertyField(_propertyOnlyOnce, ContentOnlyOnce);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentMode                   { get; } = new GUIContent("Look-at Mode",                    "Selects which look-at mode to use");
        private GUIContent ContentTarget                 { get; } = new GUIContent("Target",                          "Selects the object the object will look at");
        private GUIContent ContentLookAxis               { get; } = new GUIContent("Look Axis",                       "Selects the object axis that will point towards the target");
        private GUIContent ContentUpAxis                 { get; } = new GUIContent("Up Axis",                         "Selects the object axis that points \"up\"");
        private GUIContent ContentMatchDirection         { get; } = new GUIContent("Direction To Match",              "Selects the direction to match \"up\"");
        private GUIContent ContentAllowRotateAroundUp    { get; } = new GUIContent("Allow Rotation Around \"up\"",    "Whether the look-at can rotate the object around the up axis");
        private GUIContent ContentAllowRotateAroundRight { get; } = new GUIContent("Allow Rotation Around \"right\"", "Whether the look-at can rotate the object around the right axis");
        private GUIContent ContentInvertedLookAxis       { get; } = new GUIContent("Inverted Look",                   "Whether to invert the look-at");
        private GUIContent ContentOnlyOnce               { get; } = new GUIContent("Only Once",                       "Whether to execute the look-at only the first frame");

        private SerializedProperty _propertyMode;
        private SerializedProperty _propertyTarget;
        private SerializedProperty _propertyLookAxis;
        private SerializedProperty _propertyUpAxis;
        private SerializedProperty _propertyMatchDirection;
        private SerializedProperty _propertyAllowRotateAroundUp;
        private SerializedProperty _propertyAllowRotateAroundRight;
        private SerializedProperty _propertyInvertedLookAxis;
        private SerializedProperty _propertyOnlyOnce;

        #endregion
    }
}