// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrProgressInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor
{
    /// <summary>
    ///     Used by <see cref="UxrProgressUpdater" /> to send information about the current progress.
    /// </summary>
    public class UxrProgressInfo
    {
        #region Public Types & Data

        /// <summary>
        ///     Title of the current progress
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Current information of the current progress
        /// </summary>
        public string Info { get; }

        /// <summary>
        ///     Current progress in [0.0, 1.0] range
        /// </summary>
        public float Progress { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="title">Progress title</param>
        /// <param name="info">Progress info</param>
        /// <param name="progress">Progress [0.0, 1.0]</param>
        public UxrProgressInfo(string title, string info, float progress)
        {
            Title    = title;
            Info     = info;
            Progress = progress;
        }

        #endregion
    }
}