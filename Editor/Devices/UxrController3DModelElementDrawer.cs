// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrController3DModelElementDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Devices.Visualization;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Devices
{
    /// <summary>
    ///     Custom UI property drawer for the <see cref="UxrElement" /> type.
    /// </summary>
    [CustomPropertyDrawer(typeof(UxrElement))]
    public class UxrController3DModelElementDrawer : PropertyDrawer
    {
        #region Public Overrides PropertyDrawer

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            UxrController3DModel controller3DModel = property.serializedObject.targetObject as UxrController3DModel;
            int                  enumIndex         = property.FindPropertyRelative(PropertyElementType).enumValueIndex;
            int                  lineCount         = 1;

            if (enumIndex == (int)UxrElementType.NotSet)
            {
                lineCount = 2;
            }
            else if (enumIndex == (int)UxrElementType.Button)
            {
                lineCount = 8;
            }
            else if (enumIndex == (int)UxrElementType.Input1DRotate)
            {
                lineCount = 8;
            }
            else if (enumIndex == (int)UxrElementType.Input1DPush)
            {
                lineCount = 8;
            }
            else if (enumIndex == (int)UxrElementType.Input2DJoystick)
            {
                lineCount = 9;
            }
            else if (enumIndex == (int)UxrElementType.Input2DTouch)
            {
                lineCount = 9;
            }
            else if (enumIndex == (int)UxrElementType.DPad)
            {
                lineCount = 11;
            }

            if (controller3DModel && !controller3DModel.NeedsBothHands && enumIndex != (int)UxrElementType.NotSet)
            {
                // Doesn't need hand parameter
                lineCount -= 1;
            }

            return lineCount * EditorGUIUtility.singleLineHeight;
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UxrController3DModel controller3DModel = property.serializedObject.targetObject as UxrController3DModel;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel += 1;

            int posY = 1;

            EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyElementType), ContentElementType);

            int enumIndex = property.FindPropertyRelative(PropertyElementType).enumValueIndex;

            if (enumIndex != (int)UxrElementType.NotSet)
            {
                if (controller3DModel && controller3DModel.NeedsBothHands)
                {
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyHand), ContentHand);
                }
                else
                {
                    property.FindPropertyRelative(PropertyHand).enumValueIndex = (int)controller3DModel.HandSide;
                }

                EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyElement),            ContentElement);
                EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyGameObject),         ContentGameObject);
                EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyFinger),             ContentFinger);
                EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyFingerContactPoint), ContentFingerContactPoint);

                if (enumIndex == (int)UxrElementType.Button)
                {
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyButtonPressedOffset), ContentButtonPressedOffset);
                }
                else if (enumIndex == (int)UxrElementType.Input1DRotate)
                {
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyInput1DPressedOffsetAngle), ContentInput1DPressedOffsetAngle);
                }
                else if (enumIndex == (int)UxrElementType.Input1DPush)
                {
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyInput1DPressedOffset), ContentInput1DPressedOffset);
                }
                else if (enumIndex == (int)UxrElementType.Input2DJoystick)
                {
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyInput2DFirstAxisOffsetAngle),  ContentInput2DFirstAxisOffsetAngle);
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyInput2DSecondAxisOffsetAngle), ContentInput2DSecondAxisOffsetAngle);
                }
                else if (enumIndex == (int)UxrElementType.Input2DTouch)
                {
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyInput2DFirstAxisOffset),  ContentInput2DFirstAxisOffset);
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyInput2DSecondAxisOffset), ContentInput2DSecondAxisOffset);
                }
                else if (enumIndex == (int)UxrElementType.DPad)
                {
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyDpadFirstAxisOffset),       ContentDpadFirstAxisOffset);
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyDpadSecondAxisOffset),      ContentDpadSecondAxisOffset);
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyDpadFirstAxisOffsetAngle),  ContentDpadFirstAxisOffsetAngle);
                    EditorGUI.PropertyField(GetRect(position, posY++), property.FindPropertyRelative(PropertyDpadSecondAxisOffsetAngle), ContentDpadSecondAxisOffsetAngle);
                }
            }

            EditorGUI.indentLevel -= 1;
            EditorGUI.EndProperty();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Helper method that returns the rect for a given line number.
        /// </summary>
        /// <param name="position"><see cref="OnGUI" /> position parameter</param>
        /// <param name="line">Line number</param>
        /// <returns>Rect to draw the given UI line</returns>
        private Rect GetRect(Rect position, int line)
        {
            return new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * line, position.width, EditorGUIUtility.singleLineHeight);
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentElementType                  { get; } = new GUIContent("Type",                      "");
        private GUIContent ContentHand                         { get; } = new GUIContent("Hand",                      "");
        private GUIContent ContentElement                      { get; } = new GUIContent("Element",                   "");
        private GUIContent ContentGameObject                   { get; } = new GUIContent("GameObject",                "");
        private GUIContent ContentFinger                       { get; } = new GUIContent("Finger Used",               "");
        private GUIContent ContentFingerContactPoint           { get; } = new GUIContent("Finger Contact Pos",        "");
        private GUIContent ContentButtonPressedOffset          { get; } = new GUIContent("Pressed Offset",            "");
        private GUIContent ContentInput1DPressedOffsetAngle    { get; } = new GUIContent("Pressed Angle Offset",      "");
        private GUIContent ContentInput1DPressedOffset         { get; } = new GUIContent("Pressed Offset",            "");
        private GUIContent ContentInput2DFirstAxisOffsetAngle  { get; } = new GUIContent("1st Axis Angle Amplitude",  "");
        private GUIContent ContentInput2DSecondAxisOffsetAngle { get; } = new GUIContent("2nd Axis Angle Amplitude",  "");
        private GUIContent ContentInput2DFirstAxisOffset       { get; } = new GUIContent("1st Axis Offset Amplitude", "");
        private GUIContent ContentInput2DSecondAxisOffset      { get; } = new GUIContent("2nd Axis Offset Amplitude", "");
        private GUIContent ContentDpadFirstAxisOffsetAngle     { get; } = new GUIContent("1st Axis Angle Amplitude",  "");
        private GUIContent ContentDpadSecondAxisOffsetAngle    { get; } = new GUIContent("2nd Axis Angle Amplitude",  "");
        private GUIContent ContentDpadFirstAxisOffset          { get; } = new GUIContent("1st Axis Offset Amplitude", "");
        private GUIContent ContentDpadSecondAxisOffset         { get; } = new GUIContent("2nd Axis Offset Amplitude", "");

        private const string PropertyElementType                  = "_elementType";
        private const string PropertyHand                         = "_hand";
        private const string PropertyElement                      = "_element";
        private const string PropertyGameObject                   = "_gameObject";
        private const string PropertyFinger                       = "_finger";
        private const string PropertyFingerContactPoint           = "_fingerContactPoint";
        private const string PropertyButtonPressedOffset          = "_buttonPressedOffset";
        private const string PropertyInput1DPressedOffsetAngle    = "_input1DPressedOffsetAngle";
        private const string PropertyInput1DPressedOffset         = "_input1DPressedOffset";
        private const string PropertyInput2DFirstAxisOffsetAngle  = "_input2DFirstAxisOffsetAngle";
        private const string PropertyInput2DSecondAxisOffsetAngle = "_input2DSecondAxisOffsetAngle";
        private const string PropertyInput2DFirstAxisOffset       = "_input2DFirstAxisOffset";
        private const string PropertyInput2DSecondAxisOffset      = "_input2DSecondAxisOffset";
        private const string PropertyDpadFirstAxisOffsetAngle     = "_dpadFirstAxisOffsetAngle";
        private const string PropertyDpadSecondAxisOffsetAngle    = "_dpadSecondAxisOffsetAngle";
        private const string PropertyDpadFirstAxisOffset          = "_dpadFirstAxisOffset";
        private const string PropertyDpadSecondAxisOffset         = "_dpadSecondAxisOffset";

        #endregion
    }
}