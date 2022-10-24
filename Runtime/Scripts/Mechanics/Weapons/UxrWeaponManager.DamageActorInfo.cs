// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrWeaponManager.DamageActorInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    public partial class UxrWeaponManager
    {
        #region Private Types & Data

        /// <summary>
        ///     Stores information about an actor in the
        /// </summary>
        private class ActorInfo
        {
            #region Public Types & Data

            public Transform Transform { get; }

            #endregion

            #region Constructors & Finalizer

            public ActorInfo(UxrActor target)
            {
                Transform = target.GetComponent<Transform>();
            }

            #endregion
        }

        #endregion
    }
}