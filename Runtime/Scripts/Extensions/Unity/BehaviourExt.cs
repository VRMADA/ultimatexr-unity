// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BehaviourExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     <see cref="Component" /> extensions.
    /// </summary>
    public static class BehaviourExt
    {
        #region Public Methods

        /// <summary>
        ///     Controls the enabled state, using serialized properties when called from the Unity Editor to support Undo
        ///     correctly.
        /// </summary>
        /// <param name="self">The behaviour to enable or disable</param>
        /// <param name="enabled">Whether to enable the behaviour or disable it</param>
        public static void SetEnabled(this Behaviour self, bool enabled)
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                SerializedObject so = new SerializedObject(self);
                so.FindProperty(UxrConstants.Editor.PropertyBehaviourEnabled).boolValue = enabled;
                so.ApplyModifiedProperties();
                return;
            }
#endif
            self.enabled = enabled;

        }

        #endregion
    }
}