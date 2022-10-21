// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkManagerWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Core;
using UltimateXR.Editor.Sdks;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Core
{
    /// <summary>
    ///     Editor class that allows to create/modify/delete hand poses that can be used for interaction or manipulation in
    ///     avatars.
    /// </summary>
    public class UxrSdkManagerWindow : EditorWindow
    {
        #region Public Methods

        /// <summary>
        ///     Shows the hand pose editor menu item.
        /// </summary>
        [MenuItem("Tools/UltimateXR/SDK Manager")]
        public static void ShowWindow()
        {
            GetWindow(typeof(UxrSdkManagerWindow), true, "UltimateXR SDK Manager");
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the window.
        /// </summary>
        private void OnEnable()
        {
            _foldouts = new Dictionary<UxrSdkLocator, bool>();
        }

        /// <summary>
        ///     Draws the UI and handles input events.
        /// </summary>
        private void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.LabelField("Compiling...");
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Supported SDKs:", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // SDK List

            foreach (UxrSdkLocator sdkLocator in UxrSdkManager.SDKLocators)
            {
                if (!_foldouts.ContainsKey(sdkLocator))
                {
                    _foldouts.Add(sdkLocator, true);
                }

                _foldouts[sdkLocator] = UxrEditorUtils.FoldoutStylish(sdkLocator.Name, _foldouts[sdkLocator]);

                if (_foldouts[sdkLocator])
                {
                    EditorGUI.indentLevel += 1;

                    EditorGUILayout.LabelField("Status: " + sdkLocator.CurrentStateString);

                    if (sdkLocator.CurrentState == UxrSdkLocator.State.NotInstalled)
                    {
                        if (UxrEditorUtils.CenteredButton(new GUIContent("Get SDK")))
                        {
                            sdkLocator.TryGet();
                        }
                    }
                    else if (sdkLocator.CurrentState == UxrSdkLocator.State.Available && sdkLocator.CanBeUpdated)
                    {
                        if (UxrEditorUtils.CenteredButton(new GUIContent("SDK Update Check")))
                        {
                            sdkLocator.TryUpdate();
                        }
                    }

                    GUI.enabled = UxrSdkManager.HasAnySymbols(sdkLocator);

                    if (UxrEditorUtils.CenteredButton(new GUIContent("Remove Symbols")))
                    {
                        UxrSdkManager.RemoveSymbols(sdkLocator);
                    }

                    GUI.enabled = true;

                    if (sdkLocator.CurrentState == UxrSdkLocator.State.Available)
                    {
                        sdkLocator.OnInspectorGUI();
                    }

                    EditorGUI.indentLevel -= 1;
                }
            }

            EditorGUILayout.Space();
        }

        #endregion

        #region Private Types & Data

        private Dictionary<UxrSdkLocator, bool> _foldouts;

        #endregion
    }
}