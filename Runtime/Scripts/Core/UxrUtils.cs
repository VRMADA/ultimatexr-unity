// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrUtils.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    public static class UxrUtils
    {
        #region Public Methods

        /// <summary>
        ///     Gets the opposite side.
        /// </summary>
        /// <param name="handSide">Side</param>
        /// <returns>Opposite side</returns>
        public static UxrHandSide GetOppositeSide(UxrHandSide handSide)
        {
            return handSide == UxrHandSide.Left ? UxrHandSide.Right : UxrHandSide.Left;
        }

        /// <summary>
        ///     Builds a <see cref="UxrTransformations" /> flags enum using booleans.
        /// </summary>
        /// <param name="translate">Whether to add the <see cref="UxrTransformations.Translate" /> translation flag</param>
        /// <param name="rotate">Whether to add the <see cref="UxrTransformations.Rotate" /> rotate flag</param>
        /// <param name="scale">Whether to add the <see cref="UxrTransformations.Scale" /> scale flag</param>
        /// <returns>Flags</returns>
        public static UxrTransformations BuildTransformations(bool translate = false, bool rotate = false, bool scale = false)
        {
            UxrTransformations transformations = UxrTransformations.None;

            if (translate)
            {
                transformations |= UxrTransformations.Translate;
            }

            if (rotate)
            {
                transformations |= UxrTransformations.Rotate;
            }

            if (scale)
            {
                transformations |= UxrTransformations.Scale;
            }

            return transformations;
        }

        #endregion
    }
}