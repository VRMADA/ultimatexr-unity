// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MirrorWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Editor.Core.Math;
using UltimateXR.Extensions.Unity;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Utilities.TransformUtils
{
    /// <summary>
    ///     Custom tool window that will mirror an object's position/orientation with reference to another.
    /// </summary>
    public class MirrorWindow : EditorWindow
    {
        #region Unity

        /// <summary>
        ///     Draws the inspector and gathers user input.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("This utility will mirror an object. The mirror position is defined by a transform and the mirror plane by the transform's forward vector.\nThe Mirror Type option controls which vectors from the object will be mirrored, the remaining one being computed using the cross-product of the other two.",
                                    MessageType.Info);

            EditorGUI.BeginChangeCheck();
            UnityEngine.Transform objectToAlign = _objectToMirror;
            _objectToMirror = EditorGUILayout.ObjectField(new GUIContent("Object to Mirror", "The object that will be mirrored"), _objectToMirror, typeof(UnityEngine.Transform), true) as UnityEngine.Transform;
            if (EditorGUI.EndChangeCheck())
            {
                if (EditorUtility.IsPersistent(_objectToMirror))
                {
                    _objectToMirror = objectToAlign;
                    EditorUtility.DisplayDialog(UxrConstants.Editor.Error, "The object to mirror needs to be in the scene", UxrConstants.Editor.Ok);
                }
            }

            _useSelfSourceTransform = EditorGUILayout.Toggle(new GUIContent("Use Object As Source", "Whether to use the object to be mirrored as the source position/orientation"), _useSelfSourceTransform);

            if (!_useSelfSourceTransform)
            {
                _sourceTransform = EditorGUILayout.ObjectField(new GUIContent("Source Reference", "The transform that will be used as reference for the start position/orientation"), _sourceTransform, typeof(UnityEngine.Transform), true) as UnityEngine.Transform;
            }

            _mirrorPlane = EditorGUILayout.ObjectField(new GUIContent("Mirror Plane", "A point where the mirror plane lies"), _mirrorPlane, typeof(UnityEngine.Transform), true) as UnityEngine.Transform;
            _mirrorAxis  = UxrAxisPropertyDrawer.EditorGuiLayout(new GUIContent("Mirror Axis", "The normal of the axis plane"), _mirrorAxis);
            _reposition  = EditorGUILayout.Toggle(new GUIContent("Reposition",                 "Change position?"),    _reposition);
            _reorient    = EditorGUILayout.Toggle(new GUIContent("Reorient",                   "Change orientation?"), _reorient);

            GUI.enabled = _reorient;

            _mirrorType = (TransformExt.MirrorType)EditorGUILayout.EnumPopup("Mirror Type", _mirrorType);

            GUI.enabled = _objectToMirror != null && _mirrorPlane != null;

            if (UxrEditorUtils.CenteredButton(new GUIContent("Mirror")))
            {
                Undo.RegisterCompleteObjectUndo(_objectToMirror.transform, "Mirror object");

                if (!_useSelfSourceTransform && _sourceTransform)
                {
                    _objectToMirror.SetPositionAndRotation(_sourceTransform);
                }

                _objectToMirror.ApplyMirroring(_mirrorPlane, _mirrorAxis, _mirrorType, _reorient, _reposition);
            }

            GUI.enabled = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Menu entry that invokes the tool.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathUtils + "Mirror Object", priority = UxrConstants.Editor.PriorityMenuPathUtils + 3)]
        private static void Init()
        {
            MirrorWindow window = (MirrorWindow)GetWindow(typeof(MirrorWindow), true, "Mirror Object");
            window.Show();
        }

        #endregion

        #region Private Types & Data

        private UnityEngine.Transform   _objectToMirror;
        private bool                    _useSelfSourceTransform = true;
        private UnityEngine.Transform   _sourceTransform;
        private UnityEngine.Transform   _mirrorPlane;
        private UxrAxis                 _mirrorAxis = UxrAxis.Z;
        private TransformExt.MirrorType _mirrorType = TransformExt.MirrorType.MirrorYZ;
        private bool                    _reposition = true;
        private bool                    _reorient   = true;

        #endregion
    }
}