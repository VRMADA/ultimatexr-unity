// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEasingDrawer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Animation.Interpolation;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity.Render;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Animation.Interpolation
{
    /// <summary>
    ///     Custom inspector drawer for <see cref="UxrEasing" />.
    /// </summary>
    [CustomPropertyDrawer(typeof(UxrEasing))]
    public class UxrEasingDrawer : PropertyDrawer
    {
        #region Public Types & Data

        /// <summary>
        ///     This constant determines the graph height in pixels.
        /// </summary>
        public const int GraphHeight = 80;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Creates the temporal material to draw the graph.
        /// </summary>
        public UxrEasingDrawer()
        {
            var shader = Shader.Find(UxrConstants.Shaders.HiddenInternalColoredShader);
            _lineMaterial = new Material(shader);
        }

        /// <summary>
        ///     Destroys the temporal material to draw the graph.
        /// </summary>
        ~UxrEasingDrawer()
        {
            Object.DestroyImmediate(_lineMaterial);
        }

        #endregion

        #region Public Overrides PropertyDrawer

        /// <summary>
        ///     Gets the height in pixels required to draw the property.
        /// </summary>
        /// <param name="property">Serialized property describing an <see cref="UxrEasing" /></param>
        /// <param name="label">UI label</param>
        /// <returns>Height in pixels</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + GraphHeight;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Draws the easing graph.
        /// </summary>
        /// <param name="rect">Target rect</param>
        /// <param name="material">Material used</param>
        /// <param name="color">The line color</param>
        /// <param name="easing">The easing used</param>
        /// <param name="loopMode">The loop mode</param>
        /// <param name="loops">The number of loops to draw</param>
        public static void DrawGraph(Rect rect, Material material, Color color, UxrEasing easing, UxrLoopMode loopMode = UxrLoopMode.None, int loops = 1)
        {
            // Make coordinates relative to the rect.
            GUI.BeginClip(rect);

            // Enable the internal material.
            material.SetPass(0);

            // Draw background. Use alpha to avoid getting too dark.
            GL.Begin(GL.QUADS);
            GL.Color(Color.black.WithAlpha(0.4f));
            GL.Vertex3(0,          rect.height, 0);
            GL.Vertex3(rect.width, rect.height, 0);
            GL.Vertex3(rect.width, 0,           0);
            GL.Vertex3(0,          0,           0);
            GL.End();

            // Now draw the graph as a connected set of points. 
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color);

            // Get the min/max graph values.
            // This is important because some interpolation curves go out the [0, 1] range. 
            GetGraphRange(easing, out float min, out float max);

            // Iterate over points and draw vertices.
            for (int i = 0; i < CurveSegments + 1; ++i)
            {
                float t           = (float)i / CurveSegments;
                float value       = UxrInterpolator.Interpolate(Vector4.one, Vector4.zero, 1.0f, 0.0f, t * loops, easing, loopMode).x;
                float valueScaled = Mathf.InverseLerp(min, max, value);
                GL.Vertex3(t * rect.width, rect.height * valueScaled, 0);
            }

            GL.End();
            GUI.EndClip();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Draws the inspector and handles input.
        /// </summary>
        /// <param name="position">Position where to draw the inspector</param>
        /// <param name="property">Serialized property to draw</param>
        /// <param name="label">UI label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Draw the property label and value
            EditorGUI.PropertyField(UxrEditorUtils.GetRect(position, 0), property, label);

            // Get the graph rect. Overwrite with graph height in pixels and indentation so that it is drawn below the values only and not taking the whole inspector width.
            Rect rect = UxrEditorUtils.GetRect(position, 1);
            rect.height =  GraphHeight;
            rect.xMin   += EditorGUIUtility.labelWidth;

            // Get our easing value from the property.
            UxrEasing easing = (UxrEasing)property.enumValueIndex;

            // Draw the graph!
            if (Event.current.type == EventType.Repaint)
            {
                DrawGraph(rect, _lineMaterial, Color.green, easing);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the min and max values for a type of interpolation.
        /// </summary>
        /// <param name="easing">Easing</param>
        /// <param name="min">Returns the min graph value</param>
        /// <param name="max">Returns the max graph value</param>
        private static void GetGraphRange(UxrEasing easing, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < CurveSegments + 1; ++i)
            {
                float t     = (float)i / CurveSegments;
                float value = UxrInterpolator.Interpolate(1.0f, 0.0f, t, easing);

                if (value < min)
                {
                    min = value;
                }

                if (value > max)
                {
                    max = value;
                }
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Determines the amount of segments to draw the graph with.
        /// </summary>
        private const int CurveSegments = 200;

        private readonly Material _lineMaterial;

        #endregion
    }
}