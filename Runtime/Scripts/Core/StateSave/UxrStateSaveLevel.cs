// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSaveLevel.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Enumerates the different levels of state saving.
    /// </summary>
    public enum UxrStateSaveLevel
    {
        /// <summary>
        ///     Don't save anything.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Save a snapshot with only the changes since the previous save using <see cref="ChangesSincePreviousSave" />,
        ///     <see cref="ChangesSinceBeginning" /> or <see cref="Complete" />.
        ///     This can be used in replay systems to save incremental changes only.
        /// </summary>
        ChangesSincePreviousSave,

        /// <summary>
        ///     Save a snapshot with all changes since the end of the first frame when the object is first enabled. The end of the
        ///     first frame is used because it represents the initial state of the scene when it's loaded, including logic that
        ///     might be executed during Awake() or Start().
        ///     This can be used to save space/bandwidth by avoiding sending redundant information.
        ///     It can be utilized in a networking environment to send an up-to-date scene state to new users upon joining.
        ///     Additionally, it can be employed in replay systems to save a keyframe.
        /// </summary>
        ChangesSinceBeginning,

        /// <summary>
        ///     Save complete data. It will serialize the state of all objects, regardless of whether any values changed since
        ///     the beginning. This can be used to ensure that the state of all objects is restored correctly, even if changes are
        ///     made in later versions where objects are moved to a different initial location or have a different initial state.
        /// </summary>
        Complete
    }
}