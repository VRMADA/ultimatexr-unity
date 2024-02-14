// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentProcessorWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltimateXR.Editor.Utilities
{
    /// <summary>
    ///     Base editor window to create tools that process a type of component on a selection or even the whole project.
    /// </summary>
    public abstract partial class ComponentProcessorWindow<T> : EditorWindow where T : Component
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private TargetObjects _targetObjects       = TargetObjects.ProjectFolder;
        [SerializeField] private T             _targetSingleObject  = null;
        [SerializeField] private string        _startPath           = "";
        [SerializeField] private bool          _ignoreUxrAssets     = true;
        [SerializeField] private LogOptions    _logOptions          = LogOptions.Processed;
        [SerializeField] private bool          _onlyCheck           = false;
        [SerializeField] private bool          _recurseIntoChildren = true;
        [SerializeField] private bool          _recurseIntoPrefabs  = true;

        #endregion

        #region Unity

        /// <summary>
        ///     Loads the editor prefs
        /// </summary>
        protected virtual void OnEnable()
        {
            // TODO: Load editor prefs using a key based on typeof(T).FullName
        }

        /// <summary>
        ///     Saves the editor prefs
        /// </summary>
        protected virtual void OnDisable()
        {
            // TODO: Save editor prefs
        }

        /// <summary>
        ///     Draws the inspector
        /// </summary>
        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(HelpBoxMessage))
            {
                EditorGUILayout.HelpBox(HelpBoxMessage, HelpBoxMessageType);
            }

            bool buttonEnabled       = false;
            bool showIgnoreUxrOption = CanProcessUltimateXRAssets;
            
            _targetObjects = (TargetObjects)EditorGUILayout.EnumPopup(ContentTargetObjects, _targetObjects);

            if (_targetObjects == TargetObjects.SingleComponent)
            {
                _targetSingleObject  = EditorGUILayout.ObjectField(ContentTargetSingleObject, _targetSingleObject, typeof(T), true) as T;
                _recurseIntoChildren = EditorGUILayout.Toggle(ContentRecurseIntoChildren, _recurseIntoChildren);
                _recurseIntoPrefabs  = EditorGUILayout.Toggle(ContentRecurseIntoPrefabs,  _recurseIntoPrefabs);

                buttonEnabled = _targetSingleObject != null;
            }
            else if (_targetObjects == TargetObjects.CurrentSelection)
            {
                GUI.enabled          = Selection.gameObjects.Length > 0;
                _recurseIntoChildren = EditorGUILayout.Toggle(ContentRecurseIntoChildren, _recurseIntoChildren);
                _recurseIntoPrefabs  = EditorGUILayout.Toggle(ContentRecurseIntoPrefabs,  _recurseIntoPrefabs);
                GUI.enabled          = true;
                buttonEnabled        = Selection.gameObjects.Length > 0;
            }
            else if (_targetObjects == TargetObjects.CurrentScene)
            {
                buttonEnabled        = SceneManager.sceneCount > 0;
                _recurseIntoChildren = EditorGUILayout.Toggle(ContentRecurseIntoChildren, _recurseIntoChildren);
                _recurseIntoPrefabs  = EditorGUILayout.Toggle(ContentRecurseIntoPrefabs,  _recurseIntoPrefabs);
            }
            else if (_targetObjects == TargetObjects.ProjectFolder)
            {
                EditorGUILayout.BeginHorizontal();
                
                _startPath = EditorGUILayout.TextField(ContentPathStart, _startPath);

                if (GUILayout.Button(ContentChooseFolder, GUILayout.ExpandWidth(false)) && UxrEditorUtils.OpenFolderPanel(out string path))
                {
                    _startPath = path;
                    Repaint();
                }

                EditorGUILayout.EndHorizontal();

                showIgnoreUxrOption = false;
                buttonEnabled       = true;
            }

            if (showIgnoreUxrOption)
            {
                EditorGUI.BeginChangeCheck();
                _ignoreUxrAssets = EditorGUILayout.Toggle(ContentIgnoreUxrAssets, _ignoreUxrAssets);

                if (EditorGUI.EndChangeCheck() && _ignoreUxrAssets == false && !string.IsNullOrEmpty(DontIgnoreUxrAssetsWarningMessage))
                {
                    EditorUtility.DisplayDialog(UxrConstants.Editor.Warning, DontIgnoreUxrAssetsWarningMessage, UxrConstants.Editor.Ok);
                }
            }

            _logOptions = (LogOptions)EditorGUILayout.EnumFlagsField(ContentLogOptions, _logOptions);
            _onlyCheck  = EditorGUILayout.Toggle(ContentOnlyCheck, _onlyCheck);

            // Draw processor GUI if necessary

            OnProcessorGUI();

            // Bottom part

            GUILayout.Space(30);
            GUI.enabled = buttonEnabled && ProcessButtonEnabled;

            if (UxrEditorUtils.CenteredButton(new GUIContent(_onlyCheck ? "Check" : ProcessButtonText)))
            {
                if (OnProcessStarting())
                {
                    if (_targetObjects == TargetObjects.SingleComponent)
                    {
                        UxrEditorUtils.ModifyComponent<T>(_targetSingleObject,
                                                          CurrentComponentProcessingOptions,
                                                          ComponentProcessor,
                                                          progressInfo => EditorUtility.DisplayCancelableProgressBar(progressInfo.Title, progressInfo.Info, progressInfo.Progress),
                                                          out bool _,
                                                          _onlyCheck);
                    }
                    else if (_targetObjects == TargetObjects.CurrentSelection)
                    {
                        foreach (GameObject gameObject in Selection.gameObjects)
                        {
                            UxrEditorUtils.ModifyComponent<T>(gameObject,
                                                              CurrentComponentProcessingOptions,
                                                              ComponentProcessor,
                                                              progressInfo => EditorUtility.DisplayCancelableProgressBar(progressInfo.Title, progressInfo.Info, progressInfo.Progress),
                                                              out bool _,
                                                              _onlyCheck);
                        }
                    }
                    else if (_targetObjects == TargetObjects.CurrentScene)
                    {
                        for (int i = 0; i < SceneManager.sceneCount; ++i)
                        {
                            foreach (GameObject gameObject in SceneManager.GetSceneAt(i).GetRootGameObjects())
                            {
                                UxrEditorUtils.ModifyComponent<T>(gameObject,
                                                                  CurrentComponentProcessingOptions,
                                                                  ComponentProcessor,
                                                                  progressInfo => EditorUtility.DisplayCancelableProgressBar(progressInfo.Title, progressInfo.Info, progressInfo.Progress),
                                                                  out bool _,
                                                                  _onlyCheck);
                            }
                        }
                    }
                    else if (_targetObjects == TargetObjects.ProjectFolder)
                    {
                        UxrEditorUtils.ProcessAllProjectComponents<T>(_startPath,
                                                                      ComponentProcessor,
                                                                      progressInfo => EditorUtility.DisplayCancelableProgressBar(progressInfo.Title, progressInfo.Info, progressInfo.Progress),
                                                                      out bool _,
                                                                      false,
                                                                      _onlyCheck);
                    }

                    OnProcessEnded();

                    if (_targetObjects != TargetObjects.SingleComponent)
                    {
                        ShowResultsDialog(_prefabComponents.Sum(c => c.Value), _prefabComponents.Count, _sceneComponents.Sum(c => c.Value), _sceneComponents.Count);
                    }
                }
            }

            GUI.enabled = true;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Overridable method called right before starting processing.
        /// </summary>
        /// <remarks>When overriden, the base needs to be called</remarks>
        protected virtual bool OnProcessStarting()
        {
            _prefabComponents = new Dictionary<string, int>();
            _sceneComponents  = new Dictionary<string, int>();

            return true;
        }

        /// <summary>
        ///     Overridable method called right after processing finished.
        /// </summary>
        /// <remarks>When overriden, the base needs to be called</remarks>
        protected virtual void OnProcessEnded()
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        ///     Draws the specific processor GUI.
        /// </summary>
        /// <remarks>When overriden, the base needs to be called</remarks>
        protected virtual void OnProcessorGUI()
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Processes the component.
        /// </summary>
        /// <param name="info">Information of component to process</param>
        /// <param name="onlyCheck">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <param name="isChanged">Returns whether the component was changed</param>
        /// <param name="forceNoLog">Returns whether to force the result not be logged</param>
        protected abstract void ProcessComponent(UxrComponentInfo<T> info, bool onlyCheck, out bool isChanged, out bool forceNoLog);

        /// <summary>
        ///     Overridable method that shows the results dialog.
        /// </summary>
        /// <param name="prefabComponentCount">Number of components in prefabs processed</param>
        /// <param name="prefabCount">Number of prefabs processed</param>
        /// <param name="sceneComponentCount">Number of components in scenes processed</param>
        /// <param name="sceneCount">Scenes processed</param>
        protected virtual void ShowResultsDialog(int prefabComponentCount, int prefabCount, int sceneComponentCount, int sceneCount)
        {
            string action = _onlyCheck ? "Found" : "Processed";
            EditorUtility.DisplayDialog("Finished", $"{action} {prefabComponentCount} components in {prefabCount} prefabs and {sceneComponentCount} components in {sceneCount} scenes", UxrConstants.Editor.Ok);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Component processor.
        /// </summary>
        /// <param name="info">Contains the component to process</param>
        /// <param name="onlyCheck">
        ///     Whether to only check if components should be processed, without making any changes. This
        ///     can be used to get how many elements would be changed without modifying any data
        /// </param>
        /// <returns>Whether the component required to be changed</returns>
        private bool ComponentProcessor(UxrComponentInfo<T> info, bool onlyCheck)
        {
            ProcessComponent(info, onlyCheck, out bool isChanged, out bool ignoreLog);

            if (!ignoreLog)
            {
                LogProcessing(info, isChanged);
            }

            return isChanged;
        }

        /// <summary>
        ///     Logs changes to the console.
        /// </summary>
        /// <param name="info">Change information</param>
        /// <param name="isChanged">Whether the component was changed</param>
        private void LogProcessing(UxrComponentInfo<T> info, bool isChanged)
        {
            string action        = _onlyCheck ? "Found" : "Processed";
            string processAction = isChanged ? action : _onlyCheck ? "Found to ignore" : "Ignored";
            bool   shouldLog     = (isChanged && _logOptions.HasFlag(LogOptions.Processed)) || (!isChanged && _logOptions.HasFlag(LogOptions.Ignored));

            if (info.TargetPrefab != null)
            {
                string path = AssetDatabase.GetAssetPath(info.TargetPrefab);

                if (isChanged)
                {
                    if (_prefabComponents.ContainsKey(path))
                    {
                        _prefabComponents[path]++;
                    }
                    else
                    {
                        _prefabComponents.Add(path, 1);
                    }
                }

                if (shouldLog)
                {
                    string dataType = info.IsOriginalSource ? "original data source" : "not original data source";
                    Debug.Log($"{processAction} component {info.TargetComponent.GetPathUnderScene()} in prefab {info.TargetPrefab.name} ({dataType})");
                }
            }
            else
            {
                string scenePath = info.TargetComponent.gameObject.scene.path;

                if (isChanged)
                {
                    if (_sceneComponents.ContainsKey(scenePath))
                    {
                        _sceneComponents[scenePath]++;
                    }
                    else
                    {
                        _sceneComponents.Add(scenePath, 1);
                    }
                }

                if (shouldLog)
                {
                    string componentType = info.IsOriginalSource ? "original" : "instantiated";
                    Debug.Log($"{processAction} {componentType} component {info.TargetComponent.GetPathUnderScene()} in scene {scenePath}");
                }
            }
        }

        #endregion

        #region Protected Types & Data

        /// <summary>
        ///     Gets whether the component processor can process components from assets in UltimateXR folders.
        /// </summary>
        protected virtual bool CanProcessUltimateXRAssets => false;

        /// <summary>
        ///     Gets the message to show in the help box. Null or empty for no message.
        /// </summary>
        protected virtual string HelpBoxMessage => string.Empty;

        /// <summary>
        ///     Gets the type of message to show in the help box.
        /// </summary>
        protected virtual MessageType HelpBoxMessageType => MessageType.Info;

        /// <summary>
        ///     Gets the message to show in the help box. Null or empty for no message.
        /// </summary>
        protected virtual string DontIgnoreUxrAssetsWarningMessage => "All assets in UltimateXR come with a predefined configuration. Changing it may have unwanted results";

        /// <summary>
        ///     Gets the text to show on the process button.
        /// </summary>
        protected virtual string ProcessButtonText => "Process";

        /// <summary>
        ///     Gets whether the process button is available.
        /// </summary>
        protected virtual bool ProcessButtonEnabled => true;

        #endregion

        #region Private Types & Data

        private GUIContent ContentTargetObjects       => new GUIContent("Target Objects",           "The objects to change: objects in the current scene or prefabs in the whole project");
        private GUIContent ContentTargetSingleObject  => new GUIContent("Object To Process",        "The object to process");
        private GUIContent ContentPathStart           => new GUIContent("Path Start",               "If empty, it will process the whole /Assets folder. Use Assets/Application/Prefabs/ to start from this folder for example");
        private GUIContent ContentChooseFolder        => new GUIContent("...",                      "Selects the root folder to process");
        private GUIContent ContentIgnoreUxrAssets     => new GUIContent("Ignore UltimateXR assets", "Ignores processing assets in UltimateXR folders");
        private GUIContent ContentLogOptions          => new GUIContent("Log Options",              "Whether to log components that were processed and components that were not processed (ignored)");
        private GUIContent ContentOnlyCheck           => new GUIContent("Only Log, Don't Modify",   "Scared to proceed and make changes? This option will not make any modifications and instead will only log on the console which objects would be changed");
        private GUIContent ContentRecurseIntoChildren => new GUIContent("Recurse Into Children",    "Whether to process also child objects");
        private GUIContent ContentRecurseIntoPrefabs  => new GUIContent("Recurse Into Prefabs",     "Whether to process also the same components in all parent prefabs if they exist");

        /// <summary>
        ///     Gets whether to ignore components in assets in UltimateXR folders.
        /// </summary>
        private bool IgnoreUxrAssets => !CanProcessUltimateXRAssets || _ignoreUxrAssets;

        /// <summary>
        ///     Gets the current component processing options flags.
        /// </summary>
        private UxrComponentProcessingOptions CurrentComponentProcessingOptions
        {
            get
            {
                UxrComponentProcessingOptions options = UxrComponentProcessingOptions.All;

                if (IgnoreUxrAssets)
                {
                    options &= ~UxrComponentProcessingOptions.ProcessUltimateXRAssetComponents;
                }

                if (_targetObjects == TargetObjects.ProjectFolder)
                {
                    return options;
                }

                if (!_recurseIntoChildren)
                {
                    options &= ~UxrComponentProcessingOptions.RecurseIntoChildren;
                }

                if (!_recurseIntoPrefabs)
                {
                    options &= ~(UxrComponentProcessingOptions.RecurseIntoPrefabs);
                }

                return options;
            }
        }

        private Dictionary<string, int> _prefabComponents = new Dictionary<string, int>();
        private Dictionary<string, int> _sceneComponents  = new Dictionary<string, int>();

        #endregion
    }
}