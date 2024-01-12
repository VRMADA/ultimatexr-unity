// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniqueIdGenerationWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Xml;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Extensions.Unity;
using UnityEditor;

namespace UltimateXR.Editor.Utilities.MultiplayerUtils
{
    /// <summary>
    ///     Custom tool window that will make sure that all UXR elements in the project have correct
    ///     <see cref="UniqueId" /> values.
    /// </summary>
    public sealed class UniqueIdGenerationWindow : ComponentProcessorWindow<UxrComponent>
    {
        #region Public Methods

        /// <summary>
        ///     Fixes the given component Unique ID information if necessary.
        /// </summary>
        /// <param name="info">Information about the component</param>
        /// <param name="forceRegenerateId">Whether to force the regeneration of the unique Id.</param>
        /// <param name="onlyCheck">
        ///     Whether to only check, and return a value telling if the component requires changes, but not
        ///     perform any modifications on it
        /// </param>
        /// <returns>Whether the component required changes</returns>
        public static bool FixComponentUniqueIdInformation(UxrComponentInfo<UxrComponent> info, bool forceRegenerateId, bool onlyCheck)
        {
            SerializedObject serializedObject = new SerializedObject(info.TargetComponent);
            serializedObject.Update();
            SerializedProperty uniqueIdProperty   = serializedObject.FindProperty(UxrEditorUtils.PropertyUniqueId);
            SerializedProperty prefabGuidProperty = serializedObject.FindProperty(UxrEditorUtils.PropertyPrefabGuid);
            SerializedProperty isInPrefabProperty = serializedObject.FindProperty(UxrEditorUtils.PropertyIsInPrefab);

            if (!info.TargetComponent.GetPrefabGuid(out string prefabGuid))
            {
                return false;
            }
            
            bool isInPrefab  = info.TargetComponent.IsInPrefab();
            bool needsChange = false;

            if (prefabGuidProperty.stringValue != prefabGuid || (!info.IsOriginalSource && !prefabGuidProperty.prefabOverride))
            {
                if (!onlyCheck)
                {
                    // By changing values this way, we ensure that the property is marked as overriden.
                    // We should be able to use prefabGuidProperty.prefabOverride = true, but it gives unknown errors in some cases.
                    prefabGuidProperty.stringValue = "AA";
                    serializedObject.ApplyModifiedProperties();
                    prefabGuidProperty.stringValue = prefabGuid;
                }
                
                needsChange = true;
            }

            if (isInPrefabProperty.boolValue != isInPrefab || (!info.IsOriginalSource && !isInPrefabProperty.prefabOverride))
            {
                if (!onlyCheck)
                {
                    // By changing values this way, we ensure that the property is marked as overriden.
                    // We should be able to use isInPrefabProperty.prefabOverride = true, but it gives unknown errors in some cases.
                    isInPrefabProperty.boolValue = !isInPrefab;
                    serializedObject.ApplyModifiedProperties();
                    isInPrefabProperty.boolValue = isInPrefab;
                }

                needsChange = true;
            }

            if (forceRegenerateId || string.IsNullOrEmpty(uniqueIdProperty.stringValue) || (!info.IsOriginalSource && !uniqueIdProperty.prefabOverride))
            {
                if (!onlyCheck)
                {
                    string newUniqueID = UxrComponent.GetNewUniqueId();
                    uniqueIdProperty.stringValue = "AA";
                    serializedObject.ApplyModifiedProperties();
                    uniqueIdProperty.stringValue = newUniqueID;
                }

                needsChange = true;
            }

            if (!onlyCheck && needsChange)
            {
                serializedObject.ApplyModifiedProperties();
            }

            return needsChange;
        }

        #endregion

        #region Event Trigger Methods

        /// <inheritdoc />
        protected override bool OnProcessStarting()
        {
            if (!base.OnProcessStarting())
            {
                return false;
            }

            EditorPrefs.SetBool(UxrConstants.Editor.AutomaticIdGenerationPrefs, false);

            return true;
        }

        /// <inheritdoc />
        protected override void OnProcessEnded()
        {
            base.OnProcessEnded();

            EditorPrefs.SetBool(UxrConstants.Editor.AutomaticIdGenerationPrefs, true);
        }

        #endregion

        #region Protected Overrides ComponentProcessorWindow<UxrComponent>

        /// <inheritdoc />
        protected override bool CanProcessUltimateXRAssets => true;

        /// <inheritdoc />
        protected override string HelpBoxMessage => "This generates missing unique IDs for UltimateXR components. Unique IDs are used to identify objects so that functionality such as multi-user and save states can work correctly. UltimateXR versions prior to 0.10.0 might have missing IDs since unique ID information was first introduced in 0.10.0. Use this tool to generate missing unique ID information in projects created with older versions.";

        /// <inheritdoc />
        protected override string DontIgnoreUxrAssetsWarningMessage => "All assets in UltimateXR come already with unique ID information. Changing it may have unwanted results";

        /// <inheritdoc />
        protected override string ProcessButtonText => "Generate";

        /// <inheritdoc />
        protected override bool ProcessComponent(UxrComponentInfo<UxrComponent> info, bool onlyCheck)
        {
            return FixComponentUniqueIdInformation(info, false, onlyCheck);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Menu entry that invokes the tool.
        /// </summary>
        [MenuItem(UxrConstants.Editor.MenuPathNetworking + "Check Unique IDs", priority = UxrConstants.Editor.PriorityMenuPathNetworking + 1)]
        private static void Init()
        {
            UniqueIdGenerationWindow window = (UniqueIdGenerationWindow)GetWindow(typeof(UniqueIdGenerationWindow));
            window.Show();
        }

        #endregion
    }
}