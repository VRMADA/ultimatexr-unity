// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrConstants.Paths.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core
{
    public static partial class UxrConstants
    {
        #region Public Types & Data

        /// <summary>
        ///     Contains constants describing file paths in the framework.
        /// </summary>
        public static class Paths
        {
            #region Public Types & Data

            public const string Base               = "UltimateXR/";
            public const string SingletonResources = "Singletons/";
			
            public static readonly string HandPosePresetsRelativePath = $"{Base}HandPosePresets";

            #endregion
        }

        #endregion
    }
}