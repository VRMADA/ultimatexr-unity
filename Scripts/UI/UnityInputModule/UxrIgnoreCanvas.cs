// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrIgnoreCanvas.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Component that, added to a <see cref="GameObject" /> with a Unity <see cref="Canvas" /> component, will ignore it
    ///     when the <see cref="UxrPointerInputModule" /> is set to automatically add <see cref="UxrCanvas" /> components that
    ///     enable user interaction with UltimateXR. See <see cref="UxrPointerInputModule.AutoEnableOnWorldCanvases" />.
    /// </summary>
    public class UxrIgnoreCanvas : UxrComponent
    {
    }
}