// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LookAtWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Utilities
{
    /// <summary>
    ///     Custom tool window to implement a LookAt on an object transform.
    /// </summary>
    public class LookAtWindow : EditorWindow
    {
        #region Unity

        /// <summary>
        ///     Draws the inspector and gathers user input.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("This utility will make an object face a target. The look-at direction will be applied to the object's forward vector", MessageType.Info);

            EditorGUI.BeginChangeCheck();
            Transform objectToLookAt = _object;
            _object = EditorGUILayout.ObjectField(new GUIContent("Object", ""), _object, typeof(Transform), true) as Transform;
            if (EditorGUI.EndChangeCheck())
            {
                if (EditorUtility.IsPersistent(_object))
                {
                    _object = objectToLookAt;
                    EditorUtility.DisplayDialog("Error", "The object to process needs to be in the scene", "OK");
                }
            }

            _target = EditorGUILayout.ObjectField(new GUIContent("Target", ""), _target, typeof(Transform), true) as Transform;

            _invertForward = EditorGUILayout.Toggle(new GUIContent("Invert Forward"), _invertForward);

            GUI.enabled = _object && _target;

            if (UxrEditorUtils.CenteredButton(new GUIContent("Look At")))
            {
                Undo.RegisterCompleteObjectUndo(_object.transform, "Look at object");
                Vector3 forward = _target.position - _object.position;
                _object.rotation = Quaternion.LookRotation(_invertForward ? -forward : forward);
            }

            GUI.enabled = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Menu entry that invokes the tool.
        /// </summary>
        [MenuItem("Tools/UltimateXR/Utils/LookAt Object")]
        private static void Init()
        {
            LookAtWindow window = (LookAtWindow)GetWindow(typeof(LookAtWindow), true, "LookAt Object");
            window.Show();
        }

        #endregion

        #region Private Types & Data

        private Transform _object;
        private Transform _target;
        private bool      _invertForward;

        #endregion
    }
}