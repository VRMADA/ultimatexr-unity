// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrControllerInputEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Devices;
using UltimateXR.Editor.Core;
using UltimateXR.Editor.Sdks;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Devices
{
    /// <summary>
    ///     Custom Unity editor for the device input components. Checks for SDK availability.
    /// </summary>
    [CustomEditor(typeof(UxrControllerInput), true)]
    public class UxrControllerInputEditor : UnityEditor.Editor
    {
        #region Public Methods

        /// <summary>
        ///     Draws the UI related to checking for the required SDK.
        /// </summary>
        /// <param name="controllerInput">The controller input component to draw the UI for</param>
        public static void DrawSDKCheckInspectorGUI(UxrControllerInput controllerInput)
        {
            if (string.IsNullOrEmpty(controllerInput.SDKDependency) == false)
            {
                if (UxrSdkManager.IsAvailable(controllerInput.SDKDependency) == false)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("In order to work properly this component needs the following SDK installed and active: " + controllerInput.SDKDependency, MessageType.Warning);

                    if (UxrEditorUtils.CenteredButton(new GUIContent("Check", "Go to the SDK Manager to check the SDK")))
                    {
                        UxrSdkManagerWindow.ShowWindow();
                    }

                    EditorGUILayout.Space();
                }
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Checks if the given input component needs an SDK installed and available. Then draws the component itself.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UxrControllerInput controllerInput = serializedObject.targetObject as UxrControllerInput;
            DrawSDKCheckInspectorGUI(controllerInput);

            if (controllerInput)
            {
                if (controllerInput.SetupType == UxrControllerSetupType.Single)
                {
                    DrawPropertiesExcluding(serializedObject, "m_Script", "_leftController", "_rightController", "_enableObjectListLeft", "_enableObjectListRight");
                }
                else if (controllerInput.SetupType == UxrControllerSetupType.Dual)
                {
                    DrawPropertiesExcluding(serializedObject, "m_Script", "_controller", "_enableObjectList");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}