// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEditorUtils.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Manipulation;
using Object = UnityEngine.Object;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     Static class containing utilities for use in the Unity Editor.
    /// </summary>
    public static partial class UxrEditorUtils
    {
        #region Public Types & Data

        public const int ButtonWidth = 200;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks for the presence of <see cref="UxrManager" /> in scene.
        /// </summary>
        /// <returns>Boolean telling the result</returns>
        public static bool CheckManagerInScene()
        {
            return Object.FindObjectOfType<UxrManager>() != null;
        }

        /// <summary>
        ///     Checks for the presence of an <see cref="UxrAvatar" /> in scene.
        /// </summary>
        /// <returns>Boolean telling the result</returns>
        public static bool CheckAvatarInScene()
        {
            return Object.FindObjectOfType<UxrAvatar>() != null;
        }

        /// <summary>
        ///     Checks for the presence of an <see cref="UxrAvatar" /> in scene that has <see cref="UxrGrabber" />
        ///     components set up.
        /// </summary>
        /// <returns>Boolean telling the result</returns>
        public static bool CheckAvatarInSceneWithGrabbing()
        {
            UxrAvatar avatar = Object.FindObjectOfType<UxrAvatar>();

            if (avatar == null)
            {
                return false;
            }

            return avatar.GetComponentInChildren<UxrGrabber>() != null;
        }

        /// <summary>
        ///     Checks for the presence of an <see cref="UxrAvatar" /> in scene that has <see cref="UxrGrabber" />
        ///     components set up and a controller that has grab events.
        /// </summary>
        /// <returns>Boolean telling the result</returns>
        public static bool CheckAvatarInSceneWithGrabController()
        {
            UxrAvatar avatar = Object.FindObjectOfType<UxrAvatar>();

            if (avatar == null)
            {
                return false;
            }

            if (avatar.GetComponentInChildren<UxrGrabber>() == null)
            {
                return false;
            }

            UxrStandardAvatarController controller = avatar.GetComponentInChildren<UxrStandardAvatarController>();

            foreach (UxrAvatarControllerEvent controllerEvent in controller.LeftControllerEvents.Concat(controller.RightControllerEvents))
            {
                if (controllerEvent.TypeOfAnimation == UxrAnimationType.LeftHandGrab || controllerEvent.TypeOfAnimation == UxrAnimationType.RightHandGrab)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns a list of button names to be used in inspector components (EditorGUI.MaskField specifically).
        /// </summary>
        /// <returns>List with names of available buttons</returns>
        public static List<string> GetControllerButtonNames()
        {
            List<string> buttonNames = new List<string>(Enum.GetNames(typeof(UxrInputButtons)));
            buttonNames.Remove(UxrInputButtons.None.ToString());
            buttonNames.Remove(UxrInputButtons.Any.ToString());
            buttonNames.Remove(UxrInputButtons.Everything.ToString());

            return buttonNames;
        }

        /// <summary>
        ///     Returns a list of avatar render modes removing composition flags
        /// </summary>
        /// <returns>List of available buttons</returns>
        public static List<string> GetAvatarRenderModeNames()
        {
            List<string> renderModeNames = new List<string>(Enum.GetNames(typeof(UxrAvatarRenderModes)));
            renderModeNames.Remove(UxrAvatarRenderModes.None.ToString());
            renderModeNames.Remove(UxrAvatarRenderModes.AllControllers.ToString());
            renderModeNames.Remove(UxrAvatarRenderModes.AllControllersAndAvatar.ToString());
            return renderModeNames;
        }

        #endregion
    }
}