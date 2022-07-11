// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatar.HandState.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

namespace UltimateXR.Avatar
{
    public partial class UxrAvatar
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores the state of an <see cref="UxrAvatar" /> hand.
        /// </summary>
        private class HandState
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets the current hand pose name or null if there is no current hand pose set.
            /// </summary>
            public string CurrentHandPoseName => CurrentHandPose?.PoseName;

            /// <summary>
            ///     Gets the current hand pose.
            /// </summary>
            public UxrRuntimeHandPose CurrentHandPose => _currentHandPose;

            /// <summary>
            ///     Gets the current blend value, if <see cref="CurrentHandPose" /> is a <see cref="UxrHandPoseType.Blend" /> pose.
            /// </summary>
            public float CurrentBlendValue => _currentBlendValue;

            #endregion

            #region Public Methods

            /// <summary>
            ///     Checks whether a given event would change the current state.
            /// </summary>
            /// <param name="e">Event arguments</param>
            /// <returns>Whether the event would change the current state</returns>
            public bool IsChange(UxrAvatarHandPoseChangeEventArgs e)
            {
                if (CurrentHandPose == null || e.PoseName != CurrentHandPoseName)
                {
                    return true;
                }

                if (CurrentHandPose.PoseType == UxrHandPoseType.Blend && Mathf.Abs(e.BlendValue - CurrentBlendValue) > BlendEpsilon)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Changes the current pose.
            /// </summary>
            /// <param name="handPose">New hand pose</param>
            /// <param name="blendValue">New blend value, if the given pose is <see cref="UxrHandPoseType.Blend" /></param>
            public void SetPose(UxrRuntimeHandPose handPose, float blendValue)
            {
                if (CurrentHandPose != handPose)
                {
                    _needsBlend = true;

                    if (CurrentHandPose != null)
                    {
                        // Start smooth transition to new pose.

                        _currentHandPoseFrom = CurrentHandPose;
                        _currentHandPose     = handPose;
                        _blendTimer          = PoseTransitionSeconds;
                    }
                    else
                    {
                        // First initialization: transition immediately.

                        _currentHandPoseFrom = handPose;
                        _currentHandPose     = handPose;
                        _blendTimer          = -1.0f;
                    }
                }

                _currentBlendValueFrom = CurrentBlendValue;

                if (!Mathf.Approximately(_currentBlendValue, blendValue) && CurrentHandPose != null && CurrentHandPose.PoseType == UxrHandPoseType.Blend)
                {
                    _currentBlendValue = blendValue;
                    _needsBlend        = true;
                }
            }

            /// <summary>
            ///     Updates the hand, mainly transitioning smoothly between poses.
            /// </summary>
            /// <param name="avatar">The avatar to update</param>
            /// <param name="handSide">The hand to update</param>
            /// <param name="deltaTime">Delta time in seconds</param>
            public void Update(UxrAvatar avatar, UxrHandSide handSide, float deltaTime)
            {
                if (_blendTimer > 0.0f)
                {
                    _blendTimer -= deltaTime;
                }

                if (_blendTimer < 0.0f)
                {
                    _blendTimer = -1.0f;
                }

                if (_needsBlend)
                {
                    // Blend the poses

                    float t = Mathf.Clamp01(1.0f - _blendTimer / PoseTransitionSeconds);
                    BlendPoses(avatar, handSide, t);
                }

                if (_blendTimer < 0.0f)
                {
                    _needsBlend = false;
                }
            }

            #endregion

            #region Private Methods

            /// <summary>
            ///     Updates the avatar hand, interpolating between the "from" and "to" poses.
            /// </summary>
            /// <param name="avatar">Avatar to update</param>
            /// <param name="handSide">Hand to update</param>
            /// <param name="t">Interpolation value [0.0, 1.0]</param>
            private void BlendPoses(UxrAvatar avatar, UxrHandSide handSide, float t)
            {
                if (_currentHandPoseFrom == null || _currentHandPose == null)
                {
                    return;
                }

                // Compute the hand descriptors to interpolate

                _currentDescriptorFrom.CopyFrom(_currentHandPoseFrom.GetHandDescriptor(handSide, UxrBlendPoseType.OpenGrip));
                _currentDescriptor.CopyFrom(_currentHandPose.GetHandDescriptor(handSide, UxrBlendPoseType.OpenGrip));

                // If any of the hand poses has a blend type, compute the blended pose first

                if (_currentHandPoseFrom.PoseType == UxrHandPoseType.Blend)
                {
                    _currentDescriptorFrom.InterpolateTo(_currentHandPoseFrom.GetHandDescriptor(handSide, UxrBlendPoseType.ClosedGrip), _currentBlendValueFrom);
                }

                if (_currentHandPose.PoseType == UxrHandPoseType.Blend)
                {
                    _currentDescriptor.InterpolateTo(_currentHandPose.GetHandDescriptor(handSide, UxrBlendPoseType.ClosedGrip), _currentBlendValue);
                }

                // Now interpolate between the two and update the hand transforms

                UxrAvatarRig.UpdateHandUsingRuntimeDescriptor(avatar, handSide, _currentDescriptorFrom, _currentDescriptor, t);
            }

            #endregion

            #region Private Types & Data

            /// <summary>
            ///     Time in seconds it takes to transition from one pose to another.
            /// </summary>
            private const float PoseTransitionSeconds = 0.1f;

            /// <summary>
            ///      Value that will be used as epsilon to compare two pose blend values.
            /// </summary>
            private const float BlendEpsilon = 0.005f;

            private readonly UxrRuntimeHandDescriptor _currentDescriptorFrom = new UxrRuntimeHandDescriptor();
            private readonly UxrRuntimeHandDescriptor _currentDescriptor     = new UxrRuntimeHandDescriptor();
            private          UxrRuntimeHandPose       _currentHandPoseFrom;
            private          UxrRuntimeHandPose       _currentHandPose;
            private          float                    _currentBlendValueFrom;
            private          float                    _currentBlendValue;
            private          float                    _blendTimer = -1.0f;
            private          bool                     _needsBlend;

            #endregion
        }

        #endregion
    }
}