// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SteamVRActionsExporter.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#if ULTIMATEXR_USE_STEAMVR_SDK
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UltimateXR.Devices;
using UltimateXR.Devices.Integrations.SteamVR;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using Valve.Newtonsoft.Json;
#endif

namespace UltimateXR.Editor.Sdks
{
#if ULTIMATEXR_USE_STEAMVR_SDK
    /// <summary>
    ///     Class with functionality to create and remove the required SteamVR actions to interface with UltimateXR.
    /// </summary>
    public static partial class SteamVRActionsExporter
    {
        #region Public Methods

        /// <summary>
        ///     If SteamVR has the actions already set up, checks if UltimateXR actions need to be setup.
        /// </summary>
        /// <returns>True if SteamVR actions are present but UltimateXR custom actions are missing</returns>
        public static bool NeedsActionsSetup()
        {
            if (SteamVR_Input.DoesActionsFileExist() && SteamVR_Input.actionFile != null && SteamVR_Input.actionFile.action_sets != null)
            {
                // Action set registered?

                SteamVR_Input_ActionFile_ActionSet actionSet = SteamVR_Input.actionFile.action_sets.FirstOrDefault(a => a.name == ActionSetName);

                if (actionSet == null)
                {
                    return true;
                }

                if (!string.Equals(actionSet.usage, SteamVR_Input_ActionFile_ActionSet_Usages.single))
                {
                    return true;
                }

                // All actions registered?

                foreach (SteamVR_Input_ActionFile_Action action in EnumerateCustomActionsToBeAdded(actionSet))
                {
                    if (!IsActionPresent(actionSet, action))
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        /// <summary>
        ///     Tries to set up UltimateXR SteamVR actions to interface with the input system.
        /// </summary>
        public static void TrySetupActions()
        {
            if (SteamVR_Input.DoesActionsFileExist())
            {
                // Action file not loaded?

                if (SteamVR_Input.actionFile == null)
                {
                    SteamVR_Input.InitializeFile(false, false);
                }

                // Only process if SteamVR actions are available

                if (SteamVR_Input.actionFile != null && SteamVR_Input.actionFile.action_sets != null)
                {
                    // Try to find our action set

                    SteamVR_Input_ActionFile_ActionSet actionSet = SteamVR_Input.actionFile.action_sets.FirstOrDefault(a => a.name == ActionSetName);

                    // If not present, create:

                    if (actionSet == null)
                    {
                        actionSet = new SteamVR_Input_ActionFile_ActionSet
                                    {
                                                name  = ActionSetName,
                                                usage = SteamVR_Input_ActionFile_ActionSet_Usages.single
                                    };

                        SteamVR_Input.actionFile.action_sets.Add(actionSet);
                    }
                    else
                    {
                        actionSet.usage = SteamVR_Input_ActionFile_ActionSet_Usages.single;
                    }

                    // Clear list and add actions. This will remove deprecated actions in the future.

                    actionSet.actionsInList  = new List<SteamVR_Input_ActionFile_Action>();
                    actionSet.actionsOutList = new List<SteamVR_Input_ActionFile_Action>();

                    foreach (SteamVR_Input_ActionFile_Action action in EnumerateCustomActionsToBeAdded(actionSet))
                    {
                        if (!IsActionPresent(actionSet, action))
                        {
                            if (action.direction == SteamVR_ActionDirections.In)
                            {
                                actionSet.actionsInList.Add(action);
                            }
                            else
                            {
                                actionSet.actionsOutList.Add(action);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Warning. Action {action.name} already exists. Skipping...");
                        }
                    }

                    // Generate bindings

                    TryGenerateBindings();

                    // Save at the end

                    EditorApplication.delayCall += SaveSteamVRActions;
                }
            }
        }

        /// <summary>
        ///     Tries to remove SteamVR actions to interface with the UltimateXR input system.
        /// </summary>
        public static void TryRemoveActions()
        {
            if (SteamVR_Input.DoesActionsFileExist())
            {
                // Action file not loaded?

                if (SteamVR_Input.actionFile == null)
                {
                    SteamVR_Input.InitializeFile(false, false);
                }

                // Only process if SteamVR actions are available

                if (SteamVR_Input.actionFile != null && SteamVR_Input.actionFile.action_sets != null)
                {
                    // Try to find action set and remove it.
                    // TODO: SteamVR has a bug where the action set is not removed but at least the actions are

                    int index = SteamVR_Input.actionFile.action_sets.FindIndex(a => a.name == ActionSetName);

                    if (index != -1)
                    {
                        SteamVR_Input.actionFile.action_sets.RemoveAt(index);
                        EditorApplication.delayCall += SaveSteamVRActions;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Saves the SteamVR actions to disk.
        /// </summary>
        private static void SaveSteamVRActions()
        {
            SteamVR_Input.actionFile.SaveHelperLists();
            SteamVR_Input.actionFile.Save(SteamVR_Input.GetActionsFilePath());
            SteamVR_Input_ActionManifest_Manager.CleanBindings(true);
            SteamVR_Input_Generator.BeginGeneration();
        }

        /// <summary>
        ///     Checks if an action is present in a SteamVR action set.
        /// </summary>
        /// <param name="actionSet">Action set to process</param>
        /// <param name="action">Action to look for</param>
        /// <returns>True if the action was found</returns>
        private static bool IsActionPresent(SteamVR_Input_ActionFile_ActionSet actionSet, SteamVR_Input_ActionFile_Action action)
        {
            if (action.direction == SteamVR_ActionDirections.In)
            {
                SteamVR_Input_ActionFile_Action existingAction = actionSet.actionsInList.FirstOrDefault(a => string.Equals(a.name, action.name));
                return existingAction != null && existingAction.Equals(action);
            }
            else
            {
                SteamVR_Input_ActionFile_Action existingAction = actionSet.actionsOutList.FirstOrDefault(a => string.Equals(a.name, action.name));
                return existingAction != null && existingAction.Equals(action);
            }
        }

        /// <summary>
        ///     Enumerates all the SteamVR actions that need to be added in order to translate SteamVR input to UltimateXR input.
        /// </summary>
        /// <param name="actionSet">Action set that the actions will belong to</param>
        /// <returns>Enumerable collection with required SteamVR actions</returns>
        private static IEnumerable<SteamVR_Input_ActionFile_Action> EnumerateCustomActionsToBeAdded(SteamVR_Input_ActionFile_ActionSet actionSet)
        {
            // Buttons

            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Joystick);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Joystick2);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Trigger);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Trigger2);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Grip);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Button1);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Button2);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Button3);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Button4);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Bumper);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Bumper2);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Back);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.Menu);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.ThumbCapSense);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.IndexCapSense);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.MiddleCapSense);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.RingCapSense);
            yield return GetNewActionButtonClick(actionSet, UxrInputButtons.LittleCapSense);

            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Joystick);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Joystick2);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Trigger);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Trigger2);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Grip);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Button1);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Button2);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Button3);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Button4);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Bumper);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Bumper2);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Back);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.Menu);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.ThumbCapSense);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.IndexCapSense);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.MiddleCapSense);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.RingCapSense);
            yield return GetNewActionButtonTouch(actionSet, UxrInputButtons.LittleCapSense);

            // UxrInput1D

            yield return GetNewAction(actionSet, UxrInput1D.Grip.ToString(),     SteamVR_ActionDirections.In, UxrSteamVRConstants.BindingVarVector1);
            yield return GetNewAction(actionSet, UxrInput1D.Trigger.ToString(),  SteamVR_ActionDirections.In, UxrSteamVRConstants.BindingVarVector1);
            yield return GetNewAction(actionSet, UxrInput1D.Trigger2.ToString(), SteamVR_ActionDirections.In, UxrSteamVRConstants.BindingVarVector1);

            // UxrInput2D

            yield return GetNewAction(actionSet, UxrInput2D.Joystick.ToString(),  SteamVR_ActionDirections.In, UxrSteamVRConstants.BindingVarVector2);
            yield return GetNewAction(actionSet, UxrInput2D.Joystick2.ToString(), SteamVR_ActionDirections.In, UxrSteamVRConstants.BindingVarVector2);

            // Hand skeletons

            yield return GetNewActionSkeleton(actionSet, UxrSteamVRConstants.ActionNameHandSkeletonLeft,  true);
            yield return GetNewActionSkeleton(actionSet, UxrSteamVRConstants.ActionNameHandSkeletonRight, false);

            // Haptic feedback

            yield return GetNewActionHaptic(actionSet, UxrSteamVRConstants.ActionNameHandHaptics);
        }

        /// <summary>
        ///     Creates a new SteamVR action object.
        /// </summary>
        /// <param name="actionSet">Action set this action will belong to</param>
        /// <param name="actionName">Action name</param>
        /// <param name="direction">Input or output action?</param>
        /// <param name="varType">Variable type associated to the action</param>
        /// <returns>New SteamVR action object</returns>
        private static SteamVR_Input_ActionFile_Action GetNewAction(SteamVR_Input_ActionFile_ActionSet actionSet, string actionName, SteamVR_ActionDirections direction, string varType)
        {
            SteamVR_Input_ActionFile_Action action = new SteamVR_Input_ActionFile_Action
                                                     {
                                                                 name        = SteamVR_Input_ActionFile_Action.CreateNewName(actionSet.shortName, direction, actionName.ToLower()) + "_" + varType,
                                                                 type        = varType,
                                                                 requirement = SteamVR_Input_ActionFile_Action_Requirements.optional.ToString()
                                                     };

            return action;
        }

        /// <summary>
        ///     Simplified method to create a new SteamVR action object for a button click.
        /// </summary>
        /// <param name="actionSet">Action set this action will belong to</param>
        /// <param name="button">Button to generate action for</param>
        /// <returns>Action object representing the action of a button click</returns>
        private static SteamVR_Input_ActionFile_Action GetNewActionButtonClick(SteamVR_Input_ActionFile_ActionSet actionSet, UxrInputButtons button)
        {
            return GetNewAction(actionSet, $"{button}_{UxrSteamVRConstants.BindingInputClick}", SteamVR_ActionDirections.In, UxrSteamVRConstants.BindingVarBool);
        }

        /// <summary>
        ///     Simplified method to create a new SteamVR action object for a button touch.
        /// </summary>
        /// <param name="actionSet">Action set this action will belong to</param>
        /// <param name="button">Button to generate action for</param>
        /// <returns>Action object representing the action of a button touch</returns>
        private static SteamVR_Input_ActionFile_Action GetNewActionButtonTouch(SteamVR_Input_ActionFile_ActionSet actionSet, UxrInputButtons button)
        {
            return GetNewAction(actionSet, $"{button}_{UxrSteamVRConstants.BindingInputTouch}", SteamVR_ActionDirections.In, UxrSteamVRConstants.BindingVarBool);
        }

        /// <summary>
        ///     Simplified method to create a new SteamVR action object for skeleton tracking.
        /// </summary>
        /// <param name="actionSet">Action set this action will belong to</param>
        /// <param name="actionName">Name of the action</param>
        /// <param name="isLeft">Is it for the left hand or right hand?</param>
        /// <returns>Action object representing the action of hand skeleton tracking</returns>
        private static SteamVR_Input_ActionFile_Action GetNewActionSkeleton(SteamVR_Input_ActionFile_ActionSet actionSet, string actionName, bool isLeft)
        {
            SteamVR_Input_ActionFile_Action action = new SteamVR_Input_ActionFile_Action
                                                     {
                                                                 name        = SteamVR_Input_ActionFile_Action.CreateNewName(actionSet.shortName, SteamVR_ActionDirections.In, actionName),
                                                                 type        = SteamVR_Input_ActionFile_ActionTypes.skeleton,
                                                                 skeleton    = SteamVR_Input_ActionFile_ActionTypes.listSkeletons[isLeft ? 0 : 1].Replace("\\", "/"),
                                                                 requirement = SteamVR_Input_ActionFile_Action_Requirements.optional.ToString()
                                                     };

            return action;
        }

        /// <summary>
        ///     Simplified method to create a new SteamVR action object for haptic feedback.
        /// </summary>
        /// <param name="actionSet">Action set this action will belong to</param>
        /// <param name="actionName">Name of the action</param>
        /// <returns>Action object representing the action</returns>
        private static SteamVR_Input_ActionFile_Action GetNewActionHaptic(SteamVR_Input_ActionFile_ActionSet actionSet, string actionName)
        {
            SteamVR_Input_ActionFile_Action action = new SteamVR_Input_ActionFile_Action
                                                     {
                                                                 name = SteamVR_Input_ActionFile_Action.CreateNewName(actionSet.shortName, SteamVR_ActionDirections.Out, actionName),
                                                                 type = SteamVR_Input_ActionFile_ActionTypes.vibration
                                                     };

            return action;
        }


        /// <summary>
        ///     Tries to generate bindings for all the registered SteamVR binding files.
        ///     Files are deserialized, processed and if necessary changes are saved back to disk.
        /// </summary>
        private static void TryGenerateBindings()
        {
            // Iterate over binding files

            foreach (SteamVR_Input_ActionFile_DefaultBinding binding in SteamVR_Input.actionFile.default_bindings)
            {
                // Compose full path

                string bindingsFilePath = Path.Combine(SteamVR_Input.GetActionsFileFolder(), binding.binding_url);

                if (File.Exists(bindingsFilePath))
                {
                    // Try to process

                    try
                    {
                        SteamVR_Input_BindingFile bindingFile = JsonConvert.DeserializeObject<SteamVR_Input_BindingFile>(File.ReadAllText(bindingsFilePath));

                        // Find action list from our action set. If it's not present, create it:

                        if (!bindingFile.bindings.TryGetValue(ActionSetName, out SteamVR_Input_BindingFile_ActionList actionList))
                        {
                            actionList = new SteamVR_Input_BindingFile_ActionList();
                            bindingFile.bindings.Add(ActionSetName, actionList);
                        }

                        // Reset data

                        actionList.sources.Clear();
                        actionList.skeleton.Clear();
                        actionList.haptics.Clear();

                        // Try to process it. If it's been modified we need to save it back.

                        bool modified = false;

                        switch (bindingFile.controller_type)
                        {
                            case BindingControllerTypeOculusTouch:
                                modified = GenerateBindingsControllerOculusTouch(actionList);
                                break;

                            case BindingControllerTypeHtcVive:
                                modified = GenerateBindingsControllerHtcVive(actionList);
                                break;

                            case BindingControllerTypeHtcViveCosmos:
                                modified = GenerateBindingsControllerHtcViveCosmos(actionList);
                                break;

                            case BindingControllerTypeValveKnuckles:
                                modified = GenerateBindingsControllerValveKnuckles(actionList);
                                break;

                            case BindingControllerTypeWindowsMixedReality:
                                modified = GenerateBindingsControllerWindowsMixedReality(actionList);
                                break;
                        }

                        // Haptics, which are common

                        if (modified)
                        {
                            AddHapticsBindings(actionList);
                        }

                        // Need to Save?

                        if (modified)
                        {
                            File.WriteAllText(bindingsFilePath, JsonConvert.SerializeObject(bindingFile, Formatting.Indented));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error creating custom bindings to {nameof(SteamVR_Input_BindingFile)} in {bindingsFilePath} : {e.Message}, {e.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        ///     Generates necessary custom bindings for the Oculus Touch controllers.
        /// </summary>
        /// <param name="actionList">The action list to register the bindings in</param>
        /// <returns>True if the action list was modified</returns>
        private static bool GenerateBindingsControllerOculusTouch(SteamVR_Input_BindingFile_ActionList actionList)
        {
            AddButtonBindings(actionList.sources, "system", UxrInputButtons.Menu,    SideFlags.Left,  ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "x",      UxrInputButtons.Button1, SideFlags.Left,  ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "y",      UxrInputButtons.Button2, SideFlags.Left,  ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "a",      UxrInputButtons.Button1, SideFlags.Right, ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "b",      UxrInputButtons.Button2, SideFlags.Right, ButtonUsageFlags.All);
            AddVector2Bindings(actionList.sources, "joystick", BindingModeJoystick, UxrInput2D.Joystick, UxrInputButtons.Joystick, SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector1Bindings(actionList.sources, "grip",    UxrInput1D.Grip,    UxrInputButtons.Grip,    SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector1Bindings(actionList.sources, "trigger", UxrInput1D.Trigger, UxrInputButtons.Trigger, SideFlags.BothSides, ButtonUsageFlags.All);

            return true;
        }

        /// <summary>
        ///     Generates necessary custom bindings for the HTC Vive controllers.
        /// </summary>
        /// <param name="actionList">The action list to register the bindings in</param>
        /// <returns>True if the action list was modified</returns>
        private static bool GenerateBindingsControllerHtcVive(SteamVR_Input_BindingFile_ActionList actionList)
        {
            AddButtonBindings(actionList.sources, "menu", UxrInputButtons.Menu, SideFlags.BothSides, ButtonUsageFlags.Click);
            AddButtonBindings(actionList.sources, "grip", UxrInputButtons.Grip, SideFlags.BothSides, ButtonUsageFlags.Click);
            AddVector2Bindings(actionList.sources, "trackpad", BindingModeTrackpad, UxrInput2D.Joystick, UxrInputButtons.Joystick, SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector1Bindings(actionList.sources, "trigger", UxrInput1D.Trigger, UxrInputButtons.Trigger, SideFlags.BothSides, ButtonUsageFlags.Click);

            return true;
        }

        /// <summary>
        ///     Generates necessary custom bindings for the HTC Vive Cosmos controllers.
        /// </summary>
        /// <param name="actionList">The action list to register the bindings in</param>
        /// <returns>True if the action list was modified</returns>
        private static bool GenerateBindingsControllerHtcViveCosmos(SteamVR_Input_BindingFile_ActionList actionList)
        {
            AddButtonBindings(actionList.sources, "system", UxrInputButtons.Menu,    SideFlags.BothSides, ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "x",      UxrInputButtons.Button1, SideFlags.Left,      ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "y",      UxrInputButtons.Button2, SideFlags.Left,      ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "a",      UxrInputButtons.Button1, SideFlags.Right,     ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "b",      UxrInputButtons.Button2, SideFlags.Right,     ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "bumper", UxrInputButtons.Bumper,  SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector2Bindings(actionList.sources, "joystick", BindingModeJoystick, UxrInput2D.Joystick, UxrInputButtons.Joystick, SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector1Bindings(actionList.sources, "grip",    UxrInput1D.Grip,    UxrInputButtons.Grip,    SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector1Bindings(actionList.sources, "trigger", UxrInput1D.Trigger, UxrInputButtons.Trigger, SideFlags.BothSides, ButtonUsageFlags.All);

            return true;
        }

        /// <summary>
        ///     Generates necessary custom bindings for the Valve Knuckles controllers.
        /// </summary>
        /// <param name="actionList">The action list to register the bindings in</param>
        /// <returns>True if the action list was modified</returns>
        private static bool GenerateBindingsControllerValveKnuckles(SteamVR_Input_BindingFile_ActionList actionList)
        {
            AddButtonBindings(actionList.sources, "system", UxrInputButtons.Menu,    SideFlags.BothSides, ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "a",      UxrInputButtons.Button1, SideFlags.BothSides, ButtonUsageFlags.All);
            AddButtonBindings(actionList.sources, "b",      UxrInputButtons.Button2, SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector2Bindings(actionList.sources, "thumbstick", BindingModeJoystick, UxrInput2D.Joystick,  UxrInputButtons.Joystick,  SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector2Bindings(actionList.sources, "trackpad",   BindingModeJoystick, UxrInput2D.Joystick2, UxrInputButtons.Joystick2, SideFlags.BothSides, ButtonUsageFlags.Touch);
            AddVector1Bindings(actionList.sources, "grip",    UxrInput1D.Grip,    UxrInputButtons.Grip,    SideFlags.BothSides, ButtonUsageFlags.Touch);
            AddVector1Bindings(actionList.sources, "trigger", UxrInput1D.Trigger, UxrInputButtons.Trigger, SideFlags.BothSides, ButtonUsageFlags.All);

            AddSkeletalBindings(actionList);

            return true;
        }

        /// <summary>
        ///     Generates necessary custom bindings for the Windows Mixed Reality controllers.
        /// </summary>
        /// <param name="actionList">The action list to register the bindings in</param>
        /// <returns>True if the action list was modified</returns>
        private static bool GenerateBindingsControllerWindowsMixedReality(SteamVR_Input_BindingFile_ActionList actionList)
        {
            AddButtonBindings(actionList.sources, "menu", UxrInputButtons.Menu,    SideFlags.BothSides, ButtonUsageFlags.Click);
            AddButtonBindings(actionList.sources, "grip", UxrInputButtons.Button1, SideFlags.BothSides, ButtonUsageFlags.Click);
            AddVector2Bindings(actionList.sources, "joystick", BindingModeJoystick, UxrInput2D.Joystick,  UxrInputButtons.Joystick,  SideFlags.BothSides, ButtonUsageFlags.Click);
            AddVector2Bindings(actionList.sources, "trackpad", BindingModeJoystick, UxrInput2D.Joystick2, UxrInputButtons.Joystick2, SideFlags.BothSides, ButtonUsageFlags.All);
            AddVector1Bindings(actionList.sources, "trigger", UxrInput1D.Trigger, UxrInputButtons.Trigger, SideFlags.BothSides, ButtonUsageFlags.None);

            return true;
        }

        /// <summary>
        ///     Adds necessary binding information for a controller button.
        /// </summary>
        /// <param name="sources">List to add the necessary information to</param>
        /// <param name="deviceElement">Name of the device button in SteamVR to generate the binding for</param>
        /// <param name="button">UltimateXR button that it will be mapped to</param>
        /// <param name="sideFlags">
        ///     Flags telling which sides need to be registered. Some buttons may be
        ///     available for both sides, some only for one side (menu, system...)
        /// </param>
        /// <param name="usageFlags">Which actions to register (click, touch...)</param>
        private static void AddButtonBindings(List<SteamVR_Input_BindingFile_Source> sources,
                                              string                                 deviceElement,
                                              UxrInputButtons                        button,
                                              SideFlags                              sideFlags,
                                              ButtonUsageFlags                       usageFlags)
        {
            if (sideFlags.HasFlag(SideFlags.Left))
            {
                sources.Add(GetControllerButtonBindingSource(deviceElement, BindingLeft, button, usageFlags));
            }

            if (sideFlags.HasFlag(SideFlags.Right))
            {
                sources.Add(GetControllerButtonBindingSource(deviceElement, BindingRight, button, usageFlags));
            }
        }

        /// <summary>
        ///     Adds necessary binding information for a vector1 controller element, which also can be used as a button.
        /// </summary>
        /// <param name="sources">List to add the necessary information to</param>
        /// <param name="deviceElement">Name of the device element in SteamVR to generate the binding for</param>
        /// <param name="input1D">The UltimateXR 1D controller that it will be mapped to</param>
        /// <param name="button">UltimateXR button that it will be mapped to</param>
        /// <param name="sideFlags">
        ///     Flags telling which sides need to be registered. Some buttons may be available for both sides,
        ///     some only for one side
        /// </param>
        /// <param name="buttonUsageFlags">Which actions to register (click, touch...)</param>
        private static void AddVector1Bindings(List<SteamVR_Input_BindingFile_Source> sources,
                                               string                                 deviceElement,
                                               UxrInput1D                             input1D,
                                               UxrInputButtons                        button,
                                               SideFlags                              sideFlags,
                                               ButtonUsageFlags                       buttonUsageFlags)
        {
            if (sideFlags.HasFlag(SideFlags.Left))
            {
                // Create button

                SteamVR_Input_BindingFile_Source source = GetControllerButtonBindingSource(deviceElement, BindingLeft, button, buttonUsageFlags);

                // Override mode with trigger and add trigger action

                source.mode = BindingModeTrigger;
                source.inputs.Add(BindingInputPull, GetVector1BindingDictionary(input1D));
                sources.Add(source);
            }

            if (sideFlags.HasFlag(SideFlags.Right))
            {
                // Create button

                SteamVR_Input_BindingFile_Source source = GetControllerButtonBindingSource(deviceElement, BindingRight, button, buttonUsageFlags);

                // Override mode with trigger and add trigger action

                source.mode = BindingModeTrigger;
                source.inputs.Add(BindingInputPull, GetVector1BindingDictionary(input1D));
                sources.Add(source);
            }
        }

        /// <summary>
        ///     Adds necessary binding information for a vector2 controller element, which also can be used as a button.
        /// </summary>
        /// <param name="sources">List to add the necessary information to</param>
        /// <param name="deviceElement">Name of the device element in SteamVR to generate the binding for</param>
        /// <param name="bindingMode">Input bindingMode (trackpad, joystick). Use string constants!</param>
        /// <param name="input2D">The UltimateXR 2D controller that it will be mapped to</param>
        /// <param name="button">UltimateXR button that it will be mapped to</param>
        /// <param name="sideFlags">
        ///     Flags telling which sides need to be registered. Some buttons may be available for both sides,
        ///     some only for one side
        /// </param>
        /// <param name="buttonUsageFlags">Which actions to register (click, touch...)</param>
        private static void AddVector2Bindings(List<SteamVR_Input_BindingFile_Source> sources,
                                               string                                 deviceElement,
                                               string                                 bindingMode,
                                               UxrInput2D                             input2D,
                                               UxrInputButtons                        button,
                                               SideFlags                              sideFlags,
                                               ButtonUsageFlags                       buttonUsageFlags)
        {
            if (sideFlags.HasFlag(SideFlags.Left))
            {
                // Create button

                SteamVR_Input_BindingFile_Source source = GetControllerButtonBindingSource(deviceElement, BindingLeft, button, buttonUsageFlags);

                // Override mode and add position action

                source.mode = bindingMode;
                source.inputs.Add(BindingInputPosition, GetVector2BindingDictionary(input2D));
                sources.Add(source);
            }

            if (sideFlags.HasFlag(SideFlags.Right))
            {
                // Create button

                SteamVR_Input_BindingFile_Source source = GetControllerButtonBindingSource(deviceElement, BindingRight, button, buttonUsageFlags);

                // Override mode and add position action

                source.mode = bindingMode;
                source.inputs.Add(BindingInputPosition, GetVector2BindingDictionary(input2D));
                sources.Add(source);
            }
        }

        /// <summary>
        ///     Adds the skeleton bindings to the given action list. This will enable access to skeleton data.
        /// </summary>
        /// <param name="actionList">Action list to add the bindings to</param>
        private static void AddSkeletalBindings(SteamVR_Input_BindingFile_ActionList actionList)
        {
            actionList.skeleton.Add(new SteamVR_Input_BindingFile_Skeleton
                                    {
                                                output = $"/actions/{UxrSteamVRConstants.ActionSetName}/in/{UxrSteamVRConstants.ActionNameHandSkeletonLeft}",
                                                path   = BindingSkeletonPathLeft
                                    });

            actionList.skeleton.Add(new SteamVR_Input_BindingFile_Skeleton
                                    {
                                                output = $"/actions/{UxrSteamVRConstants.ActionSetName}/in/{UxrSteamVRConstants.ActionNameHandSkeletonRight}",
                                                path   = BindingSkeletonPathRight
                                    });
        }

        /// <summary>
        ///     Adds the haptic bindings to the given action list. This will enable sending haptic feedback.
        /// </summary>
        /// <param name="actionList">Action list to add the bindings to</param>
        private static void AddHapticsBindings(SteamVR_Input_BindingFile_ActionList actionList)
        {
            actionList.haptics.Add(new SteamVR_Input_BindingFile_Haptic
                                   {
                                               output = $"/actions/{UxrSteamVRConstants.ActionSetName}/out/{UxrSteamVRConstants.ActionNameHandHaptics}",
                                               path   = BindingHapticsPathLeft
                                   });

            actionList.haptics.Add(new SteamVR_Input_BindingFile_Haptic
                                   {
                                               output = $"/actions/{UxrSteamVRConstants.ActionSetName}/out/{UxrSteamVRConstants.ActionNameHandHaptics}",
                                               path   = BindingHapticsPathRight
                                   });
        }

        /// <summary>
        ///     Creates a <see cref="SteamVR_Input_BindingFile_Source" /> object describing a binding for a specific SteamVR
        ///     controller button of one side, to an well-known action.
        /// </summary>
        /// <param name="deviceElement">SteamVR device element name</param>
        /// <param name="side">Left or right side. Use string constants.</param>
        /// <param name="button">UltimateXR button mapped to</param>
        /// <param name="usageFlags">Which actions to register (click, touch...)</param>
        /// <returns><see cref="SteamVR_Input_BindingFile_Source" /> object</returns>
        private static SteamVR_Input_BindingFile_Source GetControllerButtonBindingSource(string           deviceElement,
                                                                                         string           side,
                                                                                         UxrInputButtons  button,
                                                                                         ButtonUsageFlags usageFlags)
        {
            SteamVR_Input_BindingFile_Source source = new SteamVR_Input_BindingFile_Source
                                                      {
                                                                  path = GetControllerBindingSourcePath(side, deviceElement),
                                                                  mode = BindingModeButton
                                                      };

            if (usageFlags.HasFlag(ButtonUsageFlags.Click))
            {
                source.inputs.Add(UxrSteamVRConstants.BindingInputClick, GetButtonBindingDictionary(button, UxrSteamVRConstants.BindingInputClick));
            }

            if (usageFlags.HasFlag(ButtonUsageFlags.Touch))
            {
                source.inputs.Add(UxrSteamVRConstants.BindingInputTouch, GetButtonBindingDictionary(button, UxrSteamVRConstants.BindingInputTouch));
            }

            return source;
        }

        /// <summary>
        ///     Gets a string that can be used as a path to a given SteamVR device element.
        /// </summary>
        /// <param name="side">Left or right side. Use string constants.</param>
        /// <param name="deviceElement">The SteamVR controller element name</param>
        /// <returns>Full path that can be used in a <see cref="SteamVR_Input_BindingFile_Source" /> object</returns>
        private static string GetControllerBindingSourcePath(string side, string deviceElement)
        {
            return $"/user/hand/{side}/input/{deviceElement}";
        }

        /// <summary>
        ///     Gets a string that describes a path to a given button action.
        /// </summary>
        /// <param name="button">UltimateXR button that the action will be mapped to</param>
        /// <param name="inputType">The type of input (click, touch...)</param>
        /// <returns>Full action path</returns>
        private static string GetControllerBindingOutputButton(UxrInputButtons button, string inputType)
        {
            return $"/actions/{UxrSteamVRConstants.ActionSetName}/in/{button.ToString().ToLower()}_{inputType}_{UxrSteamVRConstants.BindingVarBool}";
        }

        /// <summary>
        ///     Gets a string that describes a path to a given vector1 action.
        /// </summary>
        /// <param name="input1D">UltimateXR input1D that the action will be mapped to</param>
        /// <returns>Full action path</returns>
        private static string GetControllerBindingOutputInput1D(UxrInput1D input1D)
        {
            return $"/actions/{UxrSteamVRConstants.ActionSetName}/in/{input1D.ToString().ToLower()}_{UxrSteamVRConstants.BindingVarVector1}";
        }

        /// <summary>
        ///     Gets a string that describes a path to a given vector2 action.
        /// </summary>
        /// <param name="input2D">UltimateXR input2D that the action will be mapped to</param>
        /// <returns>Full action path</returns>
        private static string GetControllerBindingOutputInput2D(UxrInput2D input2D)
        {
            return $"/actions/{UxrSteamVRConstants.ActionSetName}/in/{input2D.ToString().ToLower()}_{UxrSteamVRConstants.BindingVarVector2}";
        }

        /// <summary>
        ///     Gets a dictionary describing a button input type to an action.
        /// </summary>
        /// <param name="button">UltimateXR button to map</param>
        /// <param name="inputType">Input type (click, touch...)</param>
        /// <returns>Dictionary describing the mapping</returns>
        private static SteamVR_Input_BindingFile_Source_Input_StringDictionary GetButtonBindingDictionary(UxrInputButtons button, string inputType)
        {
            return new SteamVR_Input_BindingFile_Source_Input_StringDictionary
                   {
                               { BindingOutput, GetControllerBindingOutputButton(button, inputType) }
                   };
        }

        /// <summary>
        ///     Gets a dictionary describing a vector1 type to an action.
        /// </summary>
        /// <param name="input1D">UltimateXR input1D to map</param>
        /// <returns>Dictionary describing the mapping</returns>
        private static SteamVR_Input_BindingFile_Source_Input_StringDictionary GetVector1BindingDictionary(UxrInput1D input1D)
        {
            return new SteamVR_Input_BindingFile_Source_Input_StringDictionary
                   {
                               { BindingOutput, GetControllerBindingOutputInput1D(input1D) }
                   };
        }

        /// <summary>
        ///     Gets a dictionary describing a vector2 type to an action.
        /// </summary>
        /// <param name="input2D">UltimateXR input2D to map</param>
        /// <returns>Dictionary describing the mapping</returns>
        private static SteamVR_Input_BindingFile_Source_Input_StringDictionary GetVector2BindingDictionary(UxrInput2D input2D)
        {
            return new SteamVR_Input_BindingFile_Source_Input_StringDictionary
                   {
                               { BindingOutput, GetControllerBindingOutputInput2D(input2D) }
                   };
        }

        #endregion

        #region Private Types & Data

        private const string ActionSetName                            = "/actions/" + UxrSteamVRConstants.ActionSetName;
        private const string BindingModeButton                        = "button";
        private const string BindingModeTrigger                       = "trigger";
        private const string BindingModeTrackpad                      = "trackpad";
        private const string BindingModeJoystick                      = "joystick";
        private const string BindingInputPull                         = "pull";
        private const string BindingInputPosition                     = "position";
        private const string BindingLeft                              = "left";
        private const string BindingRight                             = "right";
        private const string BindingOutput                            = "output";
        private const string BindingSkeletonPathLeft                  = "/user/hand/left/input/skeleton/left";
        private const string BindingSkeletonPathRight                 = "/user/hand/right/input/skeleton/right";
        private const string BindingHapticsPathLeft                   = "/user/hand/left/output/haptic";
        private const string BindingHapticsPathRight                  = "/user/hand/right/output/haptic";
        private const string BindingControllerTypeOculusTouch         = "oculus_touch";
        private const string BindingControllerTypeHtcVive             = "vive_controller";
        private const string BindingControllerTypeHtcViveCosmos       = "vive_cosmos_controller";
        private const string BindingControllerTypeValveKnuckles       = "knuckles";
        private const string BindingControllerTypeWindowsMixedReality = "holographic_controller";

        #endregion
    }

#endif // ULTIMATEXR_USE_STEAMVR_SDK
}