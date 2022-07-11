// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkLocator.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Reflection;
using UltimateXR.Core;
using UnityEditor;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     Base class for SDK locators. SDK locators are classes instantiated when Unity is loaded or changes are made and the
    ///     projects is recompiled.
    ///     They are used to automatically add/remove scripting symbols that allow to use different SDKs without the need of
    ///     manual user setup.
    /// </summary>
    public abstract partial class UxrSdkLocator
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the SDK name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Gets the minimum required Unity version as a string.
        /// </summary>
        public abstract string MinimumUnityVersion { get; }

        /// <summary>
        ///     Gets list of scripting symbols that will be added to the project when this SDK is available. Usually
        ///     <see cref="TryLocate" /> will set up an internal list that can be accessed through this array.
        /// </summary>
        public abstract string[] AvailableSymbols { get; }

        /// <summary>
        ///     Gets list of all scripting symbols that this SDK can add to the project.
        ///     It is used to remove all symbols when it is required.
        /// </summary>
        public abstract string[] AllSymbols { get; }

        /// <summary>
        ///     Gets whether the SDK can be updated. If so, <see cref="TryUpdate" /> is available.
        /// </summary>
        public virtual bool CanBeUpdated => false;

        /// <summary>
        ///     Gets the package name if the SDK is distributed through the package manager.
        /// </summary>
        public virtual string PackageName => null;

        /// <summary>
        ///     Gets the current SDK state as a string.
        /// </summary>
        public string CurrentStateString
        {
            get
            {
                switch (CurrentState)
                {
                    case State.Unknown:                   return StringUnknown;
                    case State.NeedsHigherUnityVersion:   return StringNeedsHigherUnityVersion;
                    case State.CurrentTargetNotSupported: return $"{StringCurrentTargetNotSupported} ({EditorUserBuildSettings.activeBuildTarget})";
                    case State.NotInstalled:              return StringNotInstalled;
                    case State.SoonSupported:             return StringSoonSupported;
                    case State.Available:                 return StringAvailable;
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     Gets whether the SDK is a package. This will simplify dependency handling since Unity's Assembly system can take
        ///     care of it.
        /// </summary>
        public bool IsPackage => !string.IsNullOrEmpty(PackageName);

        /// <summary>
        ///     Gets the current SDK state.
        /// </summary>
        public State CurrentState { get; protected set; } = State.Unknown;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Tries to find the given type name in the current assemblies. It is used to check for given types to see if an SDK
        ///     is installed or not.
        /// </summary>
        /// <param name="typeName">Type name to look for</param>
        /// <returns>Boolean telling whether the type was found or not</returns>
        public static bool IsTypeInAssemblies(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(typeName) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Child SDK locators should implement this method. It will try to locate the SDK and update the internal state making
        ///     it available through <see cref="CurrentState" /> or <see cref="CurrentStateString" />.
        /// </summary>
        public abstract void TryLocate();

        /// <summary>
        ///     Child SDK locators can implement this method. It will try to get the SDK from somewhere (usually opening a specific
        ///     URL).
        /// </summary>
        public virtual void TryGet()
        {
        }

        /// <summary>
        ///     Child SDK locators can implement this method if <see cref="CanBeUpdated" /> returns true. It will try to update the
        ///     SDK from somewhere (usually opening a specific URL).
        /// </summary>
        public virtual void TryUpdate()
        {
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Child SDK locators can implement Unity Editor functionality here. The inspector for <see cref="UxrManager" /> will
        ///     iterate through all SDKs and for the installed ones, will allow to draw custom inspector UI underneath.
        /// </summary>
        public virtual void OnInspectorGUI()
        {
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Gets or sets the incremental version number. It enables backwards compatibility across different SDKs.
        /// </summary>
        protected int CurrentVersion { get; set; } = 0;

        #endregion

        #region Private Types & Data

        private const string StringUnknown                   = "Unknown (not processed)";
        private const string StringNeedsHigherUnityVersion   = "Needs a higher Unity version";
        private const string StringCurrentTargetNotSupported = "Current build target not supported by SDK";
        private const string StringNotInstalled              = "Not installed";
        private const string StringSoonSupported             = "Soon supported";
        private const string StringAvailable                 = "Available";

        #endregion
    }
}