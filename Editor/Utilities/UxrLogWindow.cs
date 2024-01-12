// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrLogWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Utilities
{
    /// <summary>
    ///     Simple log window.
    /// </summary>
    public class UxrLogWindow : EditorWindow
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private List<string> _infoLines = new List<string>();
        [SerializeField] private Vector2      _scrollPos;

        #endregion

        #region Public Types & Data

        public const int DefaultWindowWidth  = 1400;
        public const int DefaultWindowHeight = 600;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Shows the window with the log.
        /// </summary>
        /// <param name="infoLines"></param>
        public static void ShowLog(IEnumerable<string> infoLines, int windowWidth = DefaultWindowWidth, int windowHeight = DefaultWindowHeight)
        {
            UxrLogWindow window = (UxrLogWindow)GetWindow(typeof(UxrLogWindow));

            int x = (Screen.currentResolution.width - windowWidth) / 2;
            int y = (Screen.currentResolution.height - windowHeight) / 2;

            window.position   = new Rect(x, y, windowWidth, windowHeight);
            window._infoLines = infoLines.ToList();
            window.Show();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Draws the window.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - EditorGUIUtility.singleLineHeight * 2));

            foreach (string line in _infoLines)
            {
                GUILayout.Label(line);
            }

            EditorGUILayout.EndScrollView();

            if (UxrEditorUtils.CenteredButton(new GUIContent("Close")))
            {
                Close();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}