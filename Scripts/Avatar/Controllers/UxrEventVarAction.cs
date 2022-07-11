// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrEventVarAction.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace UltimateXR.Avatar.Controllers
{
    /// <summary>
    ///     Enumerates possible actions over an <see cref="Animator" /> component. Used by
    ///     <see cref="UxrStandardAvatarController" />.
    /// </summary>
    public enum UxrEventVarAction
    {
        DoNothing,
        ToggleBool,
        SetBool,
        SetInt,
        SetFloat,
        Trigger
    }
}