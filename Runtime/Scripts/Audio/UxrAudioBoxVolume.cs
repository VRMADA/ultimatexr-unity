// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAudioBoxVolume.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity.Math;
using UnityEngine;

namespace UltimateXR.Audio
{
    /// <summary>
    ///     Component that allows to define inside and outside volumes for different audio sources.
    /// </summary>
    public class UxrAudioBoxVolume : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private AudioSource[] _audioSources;
        [SerializeField] private BoxCollider[] _boxVolumes;
        [SerializeField] private float         _volumeWhenOutside = 1.0f;
        [SerializeField] private float         _volumeWhenInside  = 0.5f;

        #endregion

        #region Unity

        /// <summary>
        ///     Checks whether the avatar is inside any of the box colliders and adjusts the volumes accordingly.
        /// </summary>
        private void Update()
        {
            if (UxrAvatar.LocalAvatar == null)
            {
                return;
            }

            bool isInside = _boxVolumes.Any(boxCollider => UxrAvatar.LocalAvatar.CameraPosition.IsInsideBox(boxCollider));

            _audioSources.ForEach(a => a.volume = isInside ? _volumeWhenInside : _volumeWhenOutside);
        }

        #endregion
    }
}