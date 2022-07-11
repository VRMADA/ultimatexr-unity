// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSdkManager.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEditor;

namespace UltimateXR.Editor.Sdks
{
    /// <summary>
    ///     Static class that will store all SDK locators through auto-registration. Each <see cref="UxrSdkLocator" />
    ///     implementation will register itself through this class.
    /// </summary>
    public static partial class UxrSdkManager
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the global list of registered SDK locators. SDK locators will auto-register every time Unity is installed or
        ///     the project is updated.
        /// </summary>
        public static IReadOnlyList<UxrSdkLocator> SDKLocators => s_locators;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Registers a new SDK locator if it is not already registered.
        ///     The locator then is used to update the project symbols adding the necessary symbols if the SDK was found or
        ///     removing them if it wasn't.
        /// </summary>
        /// <param name="locator">SDK locator interface</param>
        public static void RegisterLocator(UxrSdkLocator locator)
        {
            if (s_locators == null)
            {
                s_locators = new List<UxrSdkLocator>();
            }

            // Check if it was already registered

            bool locatorAlreadyRegistered = false;

            foreach (UxrSdkLocator registeredLocator in s_locators)
            {
                if (registeredLocator.Name == locator.Name)
                {
                    locatorAlreadyRegistered = true;
                    break;
                }
            }

            // Register if not found

            if (locatorAlreadyRegistered == false)
            {
                s_locators.Add(locator);
            }

            // Try to locate SDK and set up symbols

            locator.TryLocate();

            if (!locator.IsPackage)
            {
                SetupSymbols(locator);
            }
        }

        /// <summary>
        ///     Checks if a given SDK is present and available.
        /// </summary>
        /// <typeparam name="T">Type of the SDK locator</typeparam>
        /// <returns>True if installed and available, false if not</returns>
        public static bool IsAvailable<T>() where T : UxrSdkLocator
        {
            if (s_locators != null)
            {
                foreach (UxrSdkLocator locator in s_locators)
                {
                    if (locator.GetType() == typeof(T))
                    {
                        return locator.CurrentState == UxrSdkLocator.State.Available;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if a given SDK is present and available.
        /// </summary>
        /// <param name="name">The SDK name (looks to match any UxrSdkLocator.Name)</param>
        /// <returns>True if installed and available, false if not</returns>
        public static bool IsAvailable(string name)
        {
            if (s_locators != null)
            {
                foreach (UxrSdkLocator locator in s_locators)
                {
                    if (locator.Name == name)
                    {
                        return locator.CurrentState == UxrSdkLocator.State.Available;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Updates the project symbols removing the symbols of the SDK locator given as argument.
        /// </summary>
        /// <param name="locator">The SDK locator to remove the symbols from</param>
        public static void RemoveSymbols(UxrSdkLocator locator)
        {
            if (!locator.IsPackage)
            {
                SetupSymbols(locator, SetupSymbolsMode.ForceRemove);
            }
        }


        /// <summary>
        ///     Checks if currently the project has any symbols defined for the given SDK locator.
        /// </summary>
        /// <param name="locator">The SDK locator to check the symbols for</param>
        public static bool HasAnySymbols(UxrSdkLocator locator)
        {
            string[] targetGroupNames = Enum.GetNames(typeof(BuildTargetGroup));
            int      targetGroupIndex = 0;

            foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                // Get the BuildTargetGroup name through targetGroupNames. targetGroup.ToString() does not work because there are
                // enum entries with the same numerical value

                string targetGroupName = targetGroupNames[targetGroupIndex];
                targetGroupIndex++;

                // Ignore target groups to avoid scripting errors (thank you Ludiq!)

                if (targetGroup == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                if (typeof(BuildTargetGroup).GetField(targetGroupName).IsDefined(typeof(ObsoleteAttribute), true))
                {
                    continue;
                }

                // Get trimmed target symbol list

                List<string> currentSymbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';'));

                for (int currentSymbolIndex = 0; currentSymbolIndex < currentSymbols.Count; ++currentSymbolIndex)
                {
                    currentSymbols[currentSymbolIndex] = currentSymbols[currentSymbolIndex].Trim();
                }

                // Look for symbols

                foreach (string sdkSymbolString in locator.AllSymbols)
                {
                    if (currentSymbols.IndexOf(sdkSymbolString) != -1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the project symbols adding the necessary symbols if the SDK is present or removing them if it is not. Also
        ///     allows to remove the symbols if necessary.
        /// </summary>
        /// <param name="locator">The SDK locator</param>
        /// <param name="setupSymbolsMode">
        ///     If <see cref="SetupSymbolsMode.AddOrRemove" /> is specified then it will update the symbols depending on the SDK
        ///     presence (add if SDK is present, remove if it is not present).
        ///     In <see cref="SetupSymbolsMode.ForceRemove" /> mode it will remove all symbols linked to the SDK locator.
        /// </param>
        private static void SetupSymbols(UxrSdkLocator locator, SetupSymbolsMode setupSymbolsMode = SetupSymbolsMode.AddOrRemove)
        {
            string[] targetGroupNames = Enum.GetNames(typeof(BuildTargetGroup));
            int      targetGroupIndex = 0;

            foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                // Get the BuildTargetGroup name through targetGroupNames. targetGroup.ToString() does not work because there are
                // enum entries with the same numerical value

                string targetGroupName = targetGroupNames[targetGroupIndex];
                targetGroupIndex++;

                // Ignore target groups to avoid scripting errors (thank you Ludiq!)

                if (targetGroup == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                if (typeof(BuildTargetGroup).GetField(targetGroupName).IsDefined(typeof(ObsoleteAttribute), true))
                {
                    continue;
                }

                // Get trimmed target symbol list

                List<string> currentSymbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';'));

                for (int currentSymbolIndex = 0; currentSymbolIndex < currentSymbols.Count; ++currentSymbolIndex)
                {
                    currentSymbols[currentSymbolIndex].Trim();
                }

                // Look for symbols, adding or removing if necessary

                bool updated = false;

                List<string> availableSymbols = new List<string>(locator.AvailableSymbols);

                foreach (string sdkSymbolString in locator.AllSymbols)
                {
                    if (setupSymbolsMode == SetupSymbolsMode.AddOrRemove)
                    {
                        // Add

                        if (availableSymbols.Contains(sdkSymbolString) && currentSymbols.IndexOf(sdkSymbolString) == -1)
                        {
                            currentSymbols.Add(sdkSymbolString);
                            updated = true;
                        }
                        else
                        {
                            while (currentSymbols.IndexOf(sdkSymbolString) != currentSymbols.LastIndexOf(sdkSymbolString))
                            {
                                // Remove duplicates
                                currentSymbols.Remove(sdkSymbolString);
                                updated = true;
                            }
                        }
                    }
                    else
                    {
                        // Remove

                        while (currentSymbols.IndexOf(sdkSymbolString) != -1)
                        {
                            currentSymbols.Remove(sdkSymbolString);
                            updated = true;
                        }
                    }
                }

                // Update target symbol list

                if (updated)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", currentSymbols.ToArray()));
                }
            }
        }

        #endregion

        #region Private Types & Data

        private static List<UxrSdkLocator> s_locators;

        #endregion
    }
}