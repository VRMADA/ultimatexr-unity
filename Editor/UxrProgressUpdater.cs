// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrProgressUpdater.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Editor
{
    /// <summary>
    ///     Progress updater delegate used by <see cref="UxrEditorUtils.ProcessAllProjectComponents{T}" />.
    ///     The progress updater is responsible for giving feedback about the current processing progress.
    /// </summary>
    /// <returns>
    ///     Boolean telling whether the progress should cancel because the user pressed the cancel button.
    /// </returns>
    public delegate bool UxrProgressUpdater(UxrProgressInfo progressInfo);
}