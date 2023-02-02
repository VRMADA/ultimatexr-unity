// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGraphicRaycaster.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR
{
    /// <summary>
    ///     Base Unity UI Graphic raycaster class.
    /// </summary>
    public abstract class UxrGraphicRaycaster : GraphicRaycaster
    {
        #region Public Methods

        /// <summary>
        ///     Checks whether the raycast result describes a collision against a 2D or 3D object.
        /// </summary>
        /// <param name="result">Raycast result</param>
        /// <returns>Whether collision is against a 2D or 3D object</returns>
        public static bool Is2DOr3DRaycastResult(RaycastResult result)
        {
            return result.depth == UxrConstants.UI.Depth2DObject || result.depth == UxrConstants.UI.Depth3DObject;
        }

        /// <summary>
        ///     Compares two raycast results.
        /// </summary>
        /// <param name="a">Raycast result 1</param>
        /// <param name="b">Raycast result 2</param>
        /// <returns>Less than 0 if a is closer than b, 0 if they are at the same distance and greater than 0 if b is closer than a</returns>
        public static int CompareDepth(RaycastResult a, RaycastResult b)
        {
            if (Is2DOr3DRaycastResult(a) || Is2DOr3DRaycastResult(b))
            {
                return a.distance.CompareTo(b.distance);
            }

            return a.depth.CompareTo(b.depth);
        }

        #endregion
    }
}