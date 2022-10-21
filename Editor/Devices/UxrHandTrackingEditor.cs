// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandTrackingEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Devices
{
    /// <summary>
    ///     Custom Unity editor for hand tracking components.
    /// </summary>
    [CustomEditor(typeof(UxrHandTracking), true)]
    public class UxrHandTrackingEditor : UnityEditor.Editor
    {
        #region Unity

        /// <summary>
        ///     Caches the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _propertyCalibrationPose      = serializedObject.FindProperty("_calibrationPose");
            _propertyLeftCalibrationData  = serializedObject.FindProperty("_leftCalibrationData");
            _propertyRightCalibrationData = serializedObject.FindProperty("_rightCalibrationData");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrHandTracking handTracking = serializedObject.targetObject as UxrHandTracking;

            if (handTracking != null)
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");
            }
            else
            {
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                if (!handTracking.HasCalibrationData)
                {
                    EditorGUILayout.HelpBox("Hand tracking can be calibrated for this avatar in play mode. Do not re-calibrate data if the avatar is already calibrated correctly or you don't know what you're doing", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("Component contains calibration data. Calibration can be readjusted at runtime using this inspector.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Calibration for this avatar is performed by adopting, using your real hand, the same hand pose as the reference pose specified in the inspector, regardless of how the hand is currently being rendered. " +
                                        "Once your real hand has roughly the same pose as the reference hand pose, click on the calibrate button.\n" +
                                        "Calibration doesn't need to be performed per user or per session, only once at edit-time. The only goal is to correct the mismatch between the avatar's hand rigging and the tracking data for this device.",
                                        MessageType.Info);
            }

            GUI.enabled = EditorApplication.isPlaying;

            if (UxrEditorUtils.CenteredButton(ContentCalibrateLeft))
            {
                handTracking.CollectCalibrationData(UxrHandSide.Left);
            }

            if (UxrEditorUtils.CenteredButton(ContentCalibrateRight))
            {
                handTracking.CollectCalibrationData(UxrHandSide.Right);
            }

            serializedObject.ApplyModifiedProperties();

            if (UxrEditorUtils.CenteredButton(ContentClearLeft))
            {
                handTracking.ClearCalibrationData(UxrHandSide.Left);
            }

            if (UxrEditorUtils.CenteredButton(ContentClearRight))
            {
                handTracking.ClearCalibrationData(UxrHandSide.Right);
            }

            GUI.enabled = true;
        }

        #endregion

        #region Private Types & Data

        private GUIContent ContentCalibrateLeft  { get; } = new GUIContent("Calibrate Left Hand",     "");
        private GUIContent ContentCalibrateRight { get; } = new GUIContent("Calibrate Right Hand",    "");
        private GUIContent ContentClearLeft      { get; } = new GUIContent("Clear Left Calibration",  "");
        private GUIContent ContentClearRight     { get; } = new GUIContent("Clear Right Calibration", "");

        private SerializedProperty _propertyCalibrationPose;
        private SerializedProperty _propertyLeftCalibrationData;
        private SerializedProperty _propertyRightCalibrationData;

        #endregion
    }
}