// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AlignWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Extensions.Unity;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Utilities
{
    /// <summary>
    ///     Custom tool window that will reposition/reorient an object so that the source transform would match the target
    ///     transform.
    /// </summary>
    public class AlignWindow : EditorWindow
    {
        #region Unity

        /// <summary>
        ///     Draws the inspector and gathers user input.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("This utility will reposition/reorient an object in a way so that it would make source match target", MessageType.Info);

            EditorGUI.BeginChangeCheck();
            Transform objectToAlign = _objectToAlign;
            _objectToAlign = EditorGUILayout.ObjectField(new GUIContent("Object To Align", ""), _objectToAlign, typeof(Transform), true) as Transform;
            if (EditorGUI.EndChangeCheck())
            {
                if (EditorUtility.IsPersistent(_objectToAlign))
                {
                    _objectToAlign = objectToAlign;
                    EditorUtility.DisplayDialog("Error", "The object to align needs to be in the scene", "OK");
                }
            }

            _referenceSource = EditorGUILayout.ObjectField(new GUIContent("Reference Source", ""), _referenceSource, typeof(Transform), true) as Transform;
            _referenceTarget = EditorGUILayout.ObjectField(new GUIContent("Reference Target", ""), _referenceTarget, typeof(Transform), true) as Transform;

            _reposition = EditorGUILayout.Toggle(new GUIContent("Reposition", ""), _reposition);
            _reorient   = EditorGUILayout.Toggle(new GUIContent("Reorient",   ""), _reorient);

            GUI.enabled = _objectToAlign && _referenceSource && _referenceTarget;

            if (UxrEditorUtils.CenteredButton(new GUIContent("Align")))
            {
                Undo.RegisterCompleteObjectUndo(_objectToAlign.transform, "Align object");
                _objectToAlign.ApplyAlignment(_referenceSource, _referenceTarget, _reorient, _reposition);
            }

            GUI.enabled = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Menu entry that invokes the tool.
        /// </summary>
        [MenuItem("Tools/UltimateXR/Utils/Align Object")]
        private static void Init()
        {
            AlignWindow window = (AlignWindow)GetWindow(typeof(AlignWindow), true, "Align Object");
            window.Show();
        }

        #endregion

        #region Private Types & Data

        private Transform _objectToAlign;
        private Transform _referenceSource;
        private Transform _referenceTarget;
        private bool      _reposition = true;
        private bool      _reorient   = true;

        #endregion
    }
}