// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkManagerWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Editor.Sdks;
using UltimateXR.Extensions.System;
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
        ///     Shows the SDK Manager window.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathSdks + "SDK Manager", priority = UxrConstants.Editor.PriorityMenuPathSdks + 100)]
        public static void ShowWindow()
        {
            ShowWindow(UxrSdkLocator.SupportType.InputTracking);
        }

        /// <summary>
        ///     Shows the SDK Manager window.
        /// </summary>
        /// <param name="supportType">SDK type tab to show</param>
        public static void ShowWindow(UxrSdkLocator.SupportType supportType)
        {
            UxrSdkManagerWindow managerWindow = GetWindow(typeof(UxrSdkManagerWindow), true, "UltimateXR SDK Manager") as UxrSdkManagerWindow;

            if (GetRegisteredSdkSupportTypes().Contains(supportType))
            {
                managerWindow._selectedType = (int)supportType;
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the window.
        /// </summary>
        private void OnEnable()
        {
            _foldouts        = new Dictionary<UxrSdkLocator, bool>();
            _registeredTypes = GetRegisteredSdkSupportTypes();
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

            if (_registeredTypes.Count == 0)
            {
                EditorGUILayout.LabelField("No SDK locators have been registered");
                return;
            }

            // SDK type tabs

            if (_registeredTypes.Count > 1)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                _selectedType = GUILayout.Toolbar(_selectedType, _registeredTypes.Select(t => t.ToString().SplitCamelCase()).ToArray());
            }

            // SDK list

            IEnumerable<UxrSdkLocator> locatorsOfCurrentType = UxrSdkManager.SDKLocators.Where(l => l.Support == _registeredTypes[_selectedType]);

            if (!locatorsOfCurrentType.Any())
            {
                EditorGUILayout.LabelField("Support coming in next versions. Stay tuned!");
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Supported SDKs:", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (UxrSdkLocator sdkLocator in locatorsOfCurrentType)
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

        #region Private Methods

        /// <summary>
        ///     Gets the different SDK types that have been registered
        /// </summary>
        /// <returns>SDK types</returns>
        private static List<UxrSdkLocator.SupportType> GetRegisteredSdkSupportTypes()
        {
            return Enum.GetValues(typeof(UxrSdkLocator.SupportType)).OfType<UxrSdkLocator.SupportType>().ToList();
        }

        #endregion

        #region Private Types & Data

        private List<UxrSdkLocator.SupportType> _registeredTypes = new List<UxrSdkLocator.SupportType>();
        private int                             _selectedType;
        private Dictionary<UxrSdkLocator, bool> _foldouts;

        #endregion
    }
}