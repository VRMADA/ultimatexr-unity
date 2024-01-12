// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.Undo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor
{
    public static partial class UxrEditorUtils
    {
        #region Public Methods

        /// <summary>
        ///     Gets or adds a component. If the component is added, it's done using Undo.AddComponent<>.
        /// </summary>
        /// <param name="gameObject">Target GameObject</param>
        /// <typeparam name="T">Type of component to get or add</typeparam>
        /// <returns>The component</returns>
        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();

            if (component == null)
            {
                component = Undo.AddComponent<T>(gameObject);
            }

            return component;
        }

        #endregion
    }
}