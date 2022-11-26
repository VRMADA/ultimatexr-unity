// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrConstants.Shaders.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    public static partial class UxrConstants
    {
        #region Public Types & Data

        /// <summary>
        ///     Contains constants used in shaders such as variable names, keywords, values, etc.
        /// </summary>
        public static class Shaders
        {
            #region Public Types & Data

            public const string EmissionKeyword           = "_EMISSION";
            public const string AlphaTestOnKeyword        = "_ALPHATEST_ON";
            public const string AlphaBlendOnKeyword       = "_ALPHABLEND_ON";
            public const string AlphaPremultiplyOnKeyword = "_ALPHAPREMULTIPLY_ON";

            public const string StandardMainTextureVarName            = "_MainTex";
            public const string StandardMainTextureScaleOffsetVarName = "_MainTex_ST";
            public const string StandardColorVarName                  = "_Color";
            public const string TintColorVarName                      = "_TintColor";
            public const string EmissionColorVarName                  = "_EmissionColor";
            public const string SrcBlendVarName                       = "_SrcBlend";
            public const string DstBlendVarName                       = "_DstBlend";
            public const string ZWriteVarName                         = "_ZWrite";

            public const string SurfaceModeVarName     = "_Surface";
            public const int    SurfaceModeOpaque      = 0;
            public const int    SurfaceModeTransparent = 1;

            public const string BlendModeVarName     = "_Blend";
            public const int    BlendModeAlpha       = 0;
            public const int    BlendModePremultiply = 1;
            public const int    BlendModeAdditive    = 2;
            public const int    BlendModeMultiply    = 3;

            #endregion
        }

        #endregion
    }
}