// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFlipbookAnimationMode.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Animation.Materials
{
    /// <summary>
    ///     The different animation modes available in <see cref="UxrAnimatedTextureFlipbook" />
    /// </summary>
    public enum UxrFlipbookAnimationMode
    {
        /// <summary>
        ///     Frames are played back in a sequence, ending with the last frame.
        /// </summary>
        SingleSequence,

        /// <summary>
        ///     Frames are played back in a sequence up to the last frame. The sequence starts again from the beginning
        ///     indefinitely.
        /// </summary>
        Loop,

        /// <summary>
        ///     Frames are played back in a sequence up to the last frame and then back to the beginning again. This process is
        ///     repeated indefinitely.
        /// </summary>
        PingPong,

        /// <summary>
        ///     Random frames are played indefinitely.
        /// </summary>
        RandomFrame,

        /// <summary>
        ///     Random frames are played indefinitely but there are never two same frames played one after the other.
        /// </summary>
        RandomFrameNoRepetition,
    }
}