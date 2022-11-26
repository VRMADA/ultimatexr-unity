// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrReadmeEditor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UltimateXR.Editor.Readme
{
    /// <summary>
    ///     Custom inspector for the readme file. Based on Unity's Readme inspector.
    /// </summary>
    [CustomEditor(typeof(UxrReadme))]
    public class UxrReadmeEditor : UnityEditor.Editor
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private GUIStyle _linkStyle;
        [SerializeField] private GUIStyle _titleStyle;
        [SerializeField] private GUIStyle _headingStyle;
        [SerializeField] private GUIStyle _bodyStyle;

        #endregion

        #region Unity

        /// <summary>
        ///     Draws the <see cref="UxrReadme" /> graphics.
        /// </summary>
        public override void OnInspectorGUI()
        {
            UxrReadme readme = (UxrReadme)target;
            Initialize();

            if (readme.HeaderImage != null)
            {
                GUI.DrawTexture(new Rect(Mathf.Max(0.0f, (EditorGUIUtility.currentViewWidth - readme.HeaderImage.width) * 0.5f), 0.0f, readme.HeaderImage.width, readme.HeaderImage.height), readme.HeaderImage);
                GUILayout.Space(readme.HeaderImage.height + SectionSpacing);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Version {UxrConstants.Version}", _bodyStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(SectionSpacing);

            foreach (var section in readme.Sections)
            {
                if (!string.IsNullOrEmpty(section.Heading))
                {
                    GUILayout.Label(section.Heading, _headingStyle);
                }

                if (!string.IsNullOrEmpty(section.Text))
                {
                    GUILayout.Label(section.Text, _bodyStyle);
                }

                if (!string.IsNullOrEmpty(section.LinkText))
                {
                    if (LinkLabel(new GUIContent(section.LinkText)))
                    {
                        if (section.URL.EndsWith(".unity"))
                        {
                            EditorSceneManager.OpenScene(section.URL);
                        }
                        else
                        {
                            Application.OpenURL(section.URL);
                        }
                    }
                }

                GUILayout.Space(SectionSpacing);
            }

            if (readme.Credits.Count > 0)
            {
                GUILayout.Label("Credits", _headingStyle);

                foreach (var credit in readme.Credits)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{credit.Name}:", _bodyStyle, GUILayout.ExpandWidth(false));
                    GUILayout.Label($"{credit.Role}",  _bodyStyle, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates the GUI styles.
        /// </summary>
        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _bodyStyle          = new GUIStyle(EditorStyles.label);
            _bodyStyle.wordWrap = true;
            _bodyStyle.fontSize = 14;
            _bodyStyle.richText = true;

            _titleStyle          = new GUIStyle(_bodyStyle);
            _titleStyle.fontSize = 26;

            _headingStyle           = new GUIStyle(_bodyStyle);
            _headingStyle.fontStyle = FontStyle.Bold;
            _headingStyle.fontSize  = 18;

            _linkStyle          = new GUIStyle(_bodyStyle);
            _linkStyle.wordWrap = false;

            _linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            _linkStyle.stretchWidth     = false;

            _initialized = true;
        }

        /// <summary>
        ///     Draws and handles a link label as a button.
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="options">Layout options</param>
        /// <returns>Whether the link was clicked the current frame</returns>
        private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, _linkStyle, options);

            Handles.BeginGUI();
            Handles.color = _linkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, _linkStyle);
        }

        #endregion

        #region Private Types & Data

        private const float SectionSpacing = 16.0f;

        private bool _initialized;

        #endregion
    }
}