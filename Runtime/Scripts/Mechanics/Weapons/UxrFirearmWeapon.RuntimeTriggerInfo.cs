// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFirearmWeapon.RuntimeTriggerInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UltimateXR.Core.Serialization;
using UltimateXR.Core.StateSave;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    public partial class UxrFirearmWeapon
    {
        #region Private Types & Data

        private class RuntimeTriggerInfo : IUxrSerializable, ICloneable
        {
            #region Public Types & Data

            /// <summary>
            ///     Gets or sets whether the trigger is being pressed.
            /// </summary>
            public bool TriggerPressed
            {
                get => _triggerPressed;
                set => _triggerPressed = value;
            }

            /// <summary>
            ///     Gets or sets whether the trigger just started being pressed.
            /// </summary>
            public bool TriggerPressStarted
            {
                get => _triggerPressStarted;
                set => _triggerPressStarted = value;
            }

            /// <summary>
            ///     Gets or sets whether the trigger just finished being pressed.
            /// </summary>
            public bool TriggerPressEnded
            {
                get => _triggerPressEnded;
                set => _triggerPressEnded = value;
            }

            /// <summary>
            ///     Gets or sets the decreasing timer in seconds that will reach zero when the firearm is ready to shoot again.
            /// </summary>
            public float LastShotTimer
            {
                get => _lastShotTimer;
                set => _lastShotTimer = value;
            }

            /// <summary>
            ///     Gets or sets whether the weapon is currently loaded.
            /// </summary>
            public bool HasReloaded
            {
                get => _hasReloaded;
                set => _hasReloaded = value;
            }

            /// <summary>
            ///     Gets or sets the trigger's initial local rotation.
            /// </summary>
            public Quaternion TriggerInitialLocalRotation
            {
                get => _triggerInitialLocalRotation;
                set => _triggerInitialLocalRotation = value;
            }

            /// <summary>
            ///     Gets or sets the decreasing timer in seconds that will reach zero when the recoil animation finished.
            /// </summary>
            public float RecoilTimer
            {
                get => _recoilTimer;
                set => _recoilTimer = value;
            }

            #endregion

            #region Implicit ICloneable

            /// <summary>
            ///     Clones the object. Helps <see cref="UxrStateSaveImplementer{T}" /> avoid using serialization to create a deep copy.
            /// </summary>
            /// <returns>Copy</returns>
            public object Clone()
            {
                RuntimeTriggerInfo copy = new RuntimeTriggerInfo();

                copy._triggerPressed              = _triggerPressed;
                copy._triggerPressStarted         = _triggerPressStarted;
                copy._triggerPressEnded           = _triggerPressEnded;
                copy._lastShotTimer               = _lastShotTimer;
                copy._hasReloaded                 = _hasReloaded;
                copy._triggerInitialLocalRotation = _triggerInitialLocalRotation;
                copy._recoilTimer                 = _recoilTimer;

                return copy;
            }

            #endregion

            #region Implicit IUxrSerializable

            /// <inheritdoc />
            public int SerializationVersion => 0;

            /// <inheritdoc />
            public void Serialize(IUxrSerializer serializer, int serializationVersion)
            {
                serializer.Serialize(ref _triggerPressed);
                serializer.Serialize(ref _triggerPressStarted);
                serializer.Serialize(ref _triggerPressEnded);
                serializer.Serialize(ref _lastShotTimer);
                serializer.Serialize(ref _hasReloaded);
                serializer.Serialize(ref _triggerInitialLocalRotation);
                serializer.Serialize(ref _recoilTimer);
            }

            #endregion

            #region Public Overrides object

            /// <summary>
            ///     Compares this object to another. Helps <see cref="UxrStateSaveImplementer{T}" /> compare to avoid unnecessary
            ///     serialization.
            /// </summary>
            /// <param name="obj">Object to compare it to</param>
            /// <returns>Boolean telling whether the object is equal to <paramref name="obj" /></returns>
            public override bool Equals(object obj)
            {
                return Equals(obj as RuntimeTriggerInfo);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                // Use XOR (^) to combine hash codes for booleans that are used in Equals().
                return _triggerPressed.GetHashCode() ^ _triggerPressStarted.GetHashCode() ^ _triggerPressEnded.GetHashCode() ^ _hasReloaded.GetHashCode();
            }

            #endregion

            #region Public Methods

            /// <summary>
            ///     Compares this object to another. Helps <see cref="UxrStateSaveImplementer{T}" /> compare to avoid unnecessary
            ///     serialization.
            /// </summary>
            /// <param name="other">Object to compare it to</param>
            /// <returns>Boolean telling whether the object is equal to <paramref name="other" /></returns>
            public bool Equals(RuntimeTriggerInfo other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                // TODO: Check if we need to add the remaining fields in the comparison. We avoid it for now to avoid detecting unnecessary changes all the time.

                return _triggerPressed == other._triggerPressed &&
                       _triggerPressStarted == other._triggerPressStarted &&
                       _triggerPressEnded == other._triggerPressEnded &&
                       _hasReloaded == other._hasReloaded;
            }

            #endregion

            #region Private Types & Data

            private bool       _triggerPressed;
            private bool       _triggerPressStarted;
            private bool       _triggerPressEnded;
            private float      _lastShotTimer;
            private bool       _hasReloaded;
            private Quaternion _triggerInitialLocalRotation;
            private float      _recoilTimer;

            #endregion
        }

        #endregion
    }
}