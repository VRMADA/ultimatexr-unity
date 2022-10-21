// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrIKSolverCcdLinkDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.IK;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.IK
{
    /// <summary>
    ///     Custom property drawer for <see cref="UxrCcdLink" />.
    /// </summary>
    [CustomPropertyDrawer(typeof(UxrCcdLink))]
    public class UxrIKSolverCcdLinkDrawer : PropertyDrawer
    {
        #region Public Overrides PropertyDrawer

        /// <summary>
        ///     Returns the height in pixels required to draw the property.
        /// </summary>
        /// <param name="property">Serialized property to draw</param>
        /// <param name="label">UI label</param>
        /// <returns>Height in pixels</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = 7;

            if (property.FindPropertyRelative(PropertyAxis1HasLimits).boolValue)
            {
                lines += 2;
            }

            int enumIndex = property.FindPropertyRelative(PropertyConstraint).enumValueIndex;

            if (enumIndex == (int)UxrCcdConstraintType.TwoAxes)
            {
                if (property.FindPropertyRelative(PropertyAxis2HasLimits).boolValue)
                {
                    lines += 4;
                }
                else
                {
                    lines += 2;
                }
            }

            return lines * EditorGUIUtility.singleLineHeight;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Draws the inspector and handles input.
        /// </summary>
        /// <param name="position">Position where to draw the serialized property</param>
        /// <param name="property">Serialized property</param>
        /// <param name="label">UI label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel += 1;

            int line = 1;

            property.FindPropertyRelative(PropertyWeight).floatValue = EditorGUI.Slider(UxrEditorUtils.GetRect(position, line++),
                                                                                        ContentWeight,
                                                                                        property.FindPropertyRelative(PropertyWeight).floatValue,
                                                                                        0.0f,
                                                                                        1.0f);

            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyBone), ContentBone);

            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyConstraint),     ContentConstraint);
            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyRotationAxis1),  ContentRotationAxis1);
            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAxis1HasLimits), ContentAxis1HasLimits);

            if (property.FindPropertyRelative(PropertyAxis1HasLimits).boolValue)
            {
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAxis1AngleMin), ContentAxis1AngleMin);
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAxis1AngleMax), ContentAxis1AngleMax);
            }

            int enumIndex = property.FindPropertyRelative(PropertyConstraint).enumValueIndex;

            if (enumIndex == (int)UxrCcdConstraintType.TwoAxes)
            {
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyRotationAxis2),  ContentRotationAxis2);
                EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAxis2HasLimits), ContentAxis2HasLimits);

                if (property.FindPropertyRelative(PropertyAxis2HasLimits).boolValue)
                {
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAxis2AngleMin), ContentAxis2AngleMin);
                    EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAxis2AngleMax), ContentAxis2AngleMax);
                }
            }

            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, line++), property.FindPropertyRelative(PropertyAlignToGoal), ContentAlignToGoal);

            EditorGUI.indentLevel -= 1;
            EditorGUI.EndProperty();
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentBone           { get; } = new GUIContent("Bone",                   "");
        private GUIContent ContentWeight         { get; } = new GUIContent("Weight",                 "");
        private GUIContent ContentConstraint     { get; } = new GUIContent("Constraint",             "");
        private GUIContent ContentRotationAxis1  { get; } = new GUIContent("Rotation Axis1",         "");
        private GUIContent ContentRotationAxis2  { get; } = new GUIContent("Rotation Axis2",         "");
        private GUIContent ContentAxis1HasLimits { get; } = new GUIContent("Axis1 Has Angle Limits", "");
        private GUIContent ContentAxis1AngleMin  { get; } = new GUIContent("Axis1 Angle Min",        "");
        private GUIContent ContentAxis1AngleMax  { get; } = new GUIContent("Axis1 Angle Max",        "");
        private GUIContent ContentAxis2HasLimits { get; } = new GUIContent("Axis2 Has Angle Limits", "");
        private GUIContent ContentAxis2AngleMin  { get; } = new GUIContent("Axis2 Angle Min",        "");
        private GUIContent ContentAxis2AngleMax  { get; } = new GUIContent("Axis2 Angle Max",        "");
        private GUIContent ContentAlignToGoal    { get; } = new GUIContent("Align To Goal",          "Tries to align this link to the same axes as the goal");

        private const string PropertyBone           = "_bone";
        private const string PropertyWeight         = "_weight";
        private const string PropertyConstraint     = "_constraint";
        private const string PropertyRotationAxis1  = "_rotationAxis1";
        private const string PropertyRotationAxis2  = "_rotationAxis2";
        private const string PropertyAxis1HasLimits = "_axis1HasLimits";
        private const string PropertyAxis1AngleMin  = "_axis1AngleMin";
        private const string PropertyAxis1AngleMax  = "_axis1AngleMax";
        private const string PropertyAxis2HasLimits = "_axis2HasLimits";
        private const string PropertyAxis2AngleMin  = "_axis2AngleMin";
        private const string PropertyAxis2AngleMax  = "_axis2AngleMax";
        private const string PropertyAlignToGoal    = "_alignToGoal";

        #endregion
    }
}