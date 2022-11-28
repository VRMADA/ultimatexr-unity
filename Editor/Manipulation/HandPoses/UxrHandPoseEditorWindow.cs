// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrHandPoseEditorWindow.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

//#define SHOW_AUTOSAVE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Rig;
using UltimateXR.Core;
using UltimateXR.Core.Math;
using UltimateXR.Editor.Avatar;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UltimateXR.Manipulation;
using UltimateXR.Manipulation.HandPoses;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateXR.Editor.Manipulation.HandPoses
{
    /// <summary>
    ///     Editor class that allows to create/modify/delete hand poses that can be used for interaction or manipulation in
    ///     avatars.
    /// </summary>
    public partial class UxrHandPoseEditorWindow : EditorWindow
    {
        #region Inspector Properties/Serialized Fields

        // Internal vars

        [SerializeField] private UxrAvatar        _avatar;
        [SerializeField] private bool             _foldoutMainMenu    = true;
        [SerializeField] private bool             _foldoutPoseMenu    = true;
        [SerializeField] private bool             _foldoutHands       = true;
        [SerializeField] private bool             _foldoutPresets     = true;
        [SerializeField] private bool             _showInheritedPoses = true;
        [SerializeField] private UxrHandPoseAsset _currentHandPose;
        [SerializeField] private bool             _updateHandTransforms;
        [SerializeField] private int              _poseIndex;
        [SerializeField] private string           _poseName;
        [SerializeField] private int              _poseTypeIndex;
        [SerializeField] private bool             _autoSave;
        [SerializeField] private float            _blendValue;
        [SerializeField] private bool             _selectGameObjectOnClick;
        [SerializeField] private float            _mouseRotationSpeed;

        [SerializeField] private List<UxrFingerSpinner> _fingerSpinners;
        [SerializeField] private UxrFingerSpinner       _selectedFingerSpinner;
        [SerializeField] private Vector2                _selectedFingerSpinnerMouseStart;
        [SerializeField] private float                  _selectedFingerSpinnerStartValue;

        [SerializeField] private Texture2D _leftHandTex;
        [SerializeField] private Texture2D _rightHandTex;
        [SerializeField] private Texture2D _leftMouseTex;
        [SerializeField] private Texture2D _rightMouseTex;
        [SerializeField] private Texture2D _horizontalSpinnerTex;
        [SerializeField] private Texture2D _verticalSpinnerTex;
        [SerializeField] private Texture2D _spinnerArrowLeftTex;
        [SerializeField] private Texture2D _spinnerArrowRightTex;
        [SerializeField] private Texture2D _spinnerArrowUpTex;
        [SerializeField] private Texture2D _spinnerArrowDownTex;

        [SerializeField] private Vector2                 _presetsScrollPosition;
        [SerializeField] private List<UxrHandPosePreset> _handPosePresets;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets whether a <see cref="UxrHandPoseEditorWindow" /> window is open.
        /// </summary>
        public static bool IsVisible => s_openWindowCount > 0;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Shows the hand pose editor menu item.
        /// </summary>
        [MenuItem("Tools/UltimateXR/Hand Pose Editor")]
        public static void ShowWindow()
        {
            EditorWindow handPoseWindow = GetWindow(typeof(UxrHandPoseEditorWindow), true, "UltimateXR Hand Pose Editor");
            //handPoseWindow.autoRepaintOnSceneChange = true;
        }

        /// <summary>
        ///     Returns the type of a pose.
        /// </summary>
        /// <param name="poseName">Pose name to check</param>
        /// <param name="avatar">Avatar being processed</param>
        /// <returns><see cref="UxrHandPoseType" /> value. <see cref="UxrHandPoseType.None" /> if it could not be found</returns>
        public static UxrHandPoseType GetPoseType(string poseName, UxrAvatar avatar)
        {
            if (avatar == null)
            {
                return UxrHandPoseType.None;
            }

            UxrHandPoseAsset handPose = avatar.GetHandPose(poseName);

            return handPose != null ? handPose.PoseType : UxrHandPoseType.None;
        }

        /// <summary>
        ///     Gets a list of all the pose names in an avatar.
        /// </summary>
        /// <param name="avatar">Avatar to process</param>
        /// <param name="includedInheritedPoses">Whether to include inherited poses</param>
        /// <returns>List of all available pose names</returns>
        public static IReadOnlyList<string> GetAvatarPoseNames(UxrAvatar avatar, bool includedInheritedPoses = true)
        {
            if (avatar == null)
            {
                return new List<string>();
            }

            return includedInheritedPoses ? avatar.GetAllHandPoses().Select(p => p.name).ToList() : avatar.GetHandPoses().Select(p => p.name).ToList();
        }

        /// <summary>
        ///     Opens the hand pose editor window to edit the given avatar instance. Optionally it can start with a specific pose.
        /// </summary>
        /// <param name="avatar">Avatar instance. It can't be a prefab</param>
        /// <param name="handPose">Optional hand pose to start editing or null to load the default pose</param>
        public static void Open(UxrAvatar avatar, UxrHandPoseAsset handPose = null)
        {
            if (avatar == null || avatar.gameObject.IsPrefab())
            {
                return;
            }

            UxrHandPoseEditorWindow handPoseWindow = GetWindow(typeof(UxrHandPoseEditorWindow)) as UxrHandPoseEditorWindow;
            handPoseWindow.LoadAvatar(avatar, handPose);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Sets up the window.
        /// </summary>
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += EditorApplication_PlaymodeStateChanged;
            SceneView.duringSceneGui               += SceneView_DuringSceneGUI;
            Undo.undoRedoPerformed                 += Undo_OnUndoRedo;

            _updateHandTransforms  = false;
            _currentPoseNames      = new List<string>();
            _fingerSpinners        = new List<UxrFingerSpinner>();
            _selectedFingerSpinner = null;

            // Load persistent parameters

            _autoSave                = EditorPrefs.GetBool(PrefAutoSave,                true);
            _selectGameObjectOnClick = EditorPrefs.GetBool(PrefSelectGameObjectOnClick, false);
            _mouseRotationSpeed      = EditorPrefs.GetFloat(PrefMouseRotationSpeed, 1.0f);

            // Load UI textures

            MonoScript script = MonoScript.FromScriptableObject(this);
            string     path   = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script));
            _leftHandTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(path + LeftHandTextureRelativePath);
            _rightHandTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path + RightHandTextureRelativePath);

            _leftMouseTex         = AssetDatabase.LoadAssetAtPath<Texture2D>(path + LeftHandMouseTextureRelativePath);
            _rightMouseTex        = AssetDatabase.LoadAssetAtPath<Texture2D>(path + RightHandMouseTextureRelativePath);
            _horizontalSpinnerTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path + HorizontalSpinnerTextureRelativePath);
            _verticalSpinnerTex   = AssetDatabase.LoadAssetAtPath<Texture2D>(path + VerticalSpinnerTextureRelativePath);

            _spinnerArrowLeftTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(path + SpinnerArrowLeftTextureRelativePath);
            _spinnerArrowRightTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path + SpinnerArrowRightTextureRelativePath);
            _spinnerArrowUpTex    = AssetDatabase.LoadAssetAtPath<Texture2D>(path + SpinnerArrowUpTextureRelativePath);
            _spinnerArrowDownTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(path + SpinnerArrowDownTextureRelativePath);

            _horizontalSpinnerTex.wrapMode = TextureWrapMode.Repeat;
            _verticalSpinnerTex.wrapMode   = TextureWrapMode.Repeat;

            // Load presets

            RefreshHandPosePresets();

            s_handPoseEditorWindow = this;
            s_openWindowCount++;
        }

        /// <summary>
        ///     Resets states when closing the window.
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorApplication_PlaymodeStateChanged;
            SceneView.duringSceneGui               -= SceneView_DuringSceneGUI;
            Undo.undoRedoPerformed                 -= Undo_OnUndoRedo;

            // Save persistent parameters

            EditorPrefs.SetBool(PrefAutoSave,                _autoSave);
            EditorPrefs.SetBool(PrefSelectGameObjectOnClick, _selectGameObjectOnClick);
            EditorPrefs.SetFloat(PrefMouseRotationSpeed, _mouseRotationSpeed);

            // Reset hands

            if (_avatar != null)
            {
                if (_autoSave)
                {
                    AutoSave(SaveHandPoseFlags.All);
                }

                CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.All);

                ResetHandTransforms(_avatar, UxrHandSide.Left);
                ResetHandTransforms(_avatar, UxrHandSide.Right);
            }

            s_openWindowCount--;
        }

        /// <summary>
        ///     Draws the UI and handles input events.
        /// </summary>
        private void OnGUI()
        {
            int  lastY                    = 0;
            bool registerUndoOnPoseUpdate = true;

            // This is mainly to handle the preview blend slider appropriately

            if (_avatar != null && _currentHandPose != null)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (_autoSave)
                    {
                        AutoSave(SaveHandPoseFlags.HandDescriptors);
                    }
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    _blendValue           = _blendValue < BlendPoseOpenThreshold ? 0.0f : 1.0f;
                    _updateHandTransforms = true;
                }
            }

            // Read-only pose?

            bool isReadOnlyPose = false;

            if (_avatar != null && _currentHandPose != null && _currentPoseNames.Count > 0)
            {
                if (_avatar.GetHandPose(_currentHandPose.name, false) == null)
                {
                    isReadOnlyPose = true;
                }
            }

            // Main menu ////////////////////////////////////

            EditorGUILayout.BeginVertical("box", GUILayout.Width(HandMarginHorizontal * 2 + _leftHandTex.width + _rightHandTex.width));

            _foldoutMainMenu = UxrEditorUtils.FoldoutStylish("Main menu:", _foldoutMainMenu);

            if (_foldoutMainMenu)
            {
                EditorGUI.BeginChangeCheck();
                UxrAvatar newAvatar = EditorGUILayout.ObjectField(new GUIContent("Target Avatar", "Drop an avatar from the scene here to edit its hand poses"), _avatar, typeof(UxrAvatar), true, GUILayout.Width(FieldWidth)) as UxrAvatar;
                if (EditorGUI.EndChangeCheck())
                {
                    bool isValidAvatar = true;

                    if (newAvatar == null)
                    {
                        _avatar       = null;
                        isValidAvatar = false;
                    }
                    else if (newAvatar.GetAvatarPrefab() == null)
                    {
                        EditorUtility.DisplayDialog("Avatar must have prefab", "The avatar must be a prefab in order to store poses. Please create a prefab from the avatar first.", "OK");
                        isValidAvatar = false;
                    }

                    if (isValidAvatar)
                    {
                        if (_autoSave)
                        {
                            AutoSave(SaveHandPoseFlags.HandDescriptors);
                        }

                        if (CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors))
                        {
                            if (EditorUtility.IsPersistent(newAvatar))
                            {
                                EditorUtility.DisplayDialog("Avatar must be in the scene",
                                                            "Please drop the prefab into the scene in order to preview/edit poses.\nThe poses will be saved in the prefab, making them available to all avatar instances and prefab variants",
                                                            "OK");
                            }
                            else
                            {
                                // Reset old avatar

                                if (_avatar != null)
                                {
                                    ResetHandTransforms(_avatar, UxrHandSide.Left);
                                    ResetHandTransforms(_avatar, UxrHandSide.Right);
                                }

                                // Set up current avatar

                                LoadAvatar(newAvatar);
                            }
                        }
                    }
                }

                if (_avatar == null)
                {
                    _poseIndex       = 0;
                    _poseTypeIndex   = -1;
                    _blendValue      = 0.0f;
                    _poseName        = null;
                    _currentHandPose = null;
                }

                EditorGUILayout.Space();
                lastY = (int)GUILayoutUtility.GetLastRect().yMax;
                GUILayout.Space(VerticalHeightSingleLine * 6);

                GUI.Label(GetPosesMenuButtonRect(PosesMenuColumnCreate, lastY), new GUIContent("Create", "Creation options"), EditorStyles.boldLabel);
                GUI.Label(GetPosesMenuButtonRect(PosesMenuColumnDelete, lastY), new GUIContent("Delete", "Delete options"),   EditorStyles.boldLabel);
                GUI.Label(GetPosesMenuButtonRect(PosesMenuColumnMisc,   lastY), new GUIContent("Misc",   "Misc options"),     EditorStyles.boldLabel);

                if (_avatar != null && _avatar.IsPrefabVariant)
                {
                    GUI.Label(GetPosesMenuButtonRect(PosesMenuColumnPrefabVariant, lastY), new GUIContent("Prefab hierarchy:", "List of parent prefabs"), EditorStyles.boldLabel);

                    int prefabIndex = 0;

                    foreach (UxrAvatar avatarPrefab in _avatar.GetPrefabChain().Reverse())
                    {
                        int posY = lastY + (prefabIndex + 1) * VerticalHeightSingleLine;
                        GUI.Label(GetPosesMenuButtonRect(PosesMenuColumnPrefabVariant, posY), $"-{avatarPrefab.name}");
                        prefabIndex++;
                    }
                }

                lastY += VerticalHeightSingleLine;

                GUI.enabled = _avatar != null;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnCreate, lastY), new GUIContent("Create New Pose...", "Creates a new pose for the currently selected avatar animator")))
                {
                    if (_autoSave)
                    {
                        AutoSave(SaveHandPoseFlags.HandDescriptors);
                    }

                    if (CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors))
                    {
                        UxrHandPoseAsset newHandPoseAsset = CreatePose(_avatar);

                        if (newHandPoseAsset != null)
                        {
                            SwitchToPose(newHandPoseAsset);

                            if (_currentPoseNames.Count == 1)
                            {
                                SetPoseAsDefault(_avatar, newHandPoseAsset.name);
                            }

                            SaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors);

                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newHandPoseAsset));
                            AssetDatabase.SaveAssets();
                        }
                    }
                }

                GUI.enabled = _currentHandPose != null && !isReadOnlyPose;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnDelete, lastY), new GUIContent("Delete Current Pose...", "Deletes the currently selected pose")))
                {
                    if (EditorUtility.DisplayDialog("Delete Pose?", "Delete pose " + _currentPoseNames[_poseIndex] + "?", "Yes", "Cancel"))
                    {
                        DeletePose(_avatar, _currentPoseNames[_poseIndex], true);
                        LoadDefaultPoseData();

                        if (_currentHandPose == null && _currentPoseNames.Count > 0)
                        {
                            _poseIndex       = 0;
                            _currentHandPose = _avatar.GetHandPose(_currentPoseNames[_poseIndex]);
                            SwitchToPose(_currentHandPose);
                        }
                    }
                }

                GUI.enabled = _currentHandPose != null && _avatar != null && _avatar.DefaultHandPose != _currentHandPose;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnMisc, lastY), new GUIContent("Set Current Pose As Default", "The default pose is the one that the avatar will have when it is in idle state")))
                {
                    SetPoseAsDefault(_avatar, _currentHandPose.name);
                }

                lastY += VerticalHeightSingleLine;

                GUI.enabled = _poseIndex >= 0 && _poseIndex < _currentPoseNames.Count;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnCreate, lastY), new GUIContent("Copy Current Pose To New...", "Copies the current pose to a new pose asset file")))
                {
                    if (_autoSave)
                    {
                        AutoSave(SaveHandPoseFlags.HandDescriptors);
                    }

                    if (CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors))
                    {
                        UxrHandPoseAsset newHandPoseAsset = CopyPoseAsNew(_avatar, _currentHandPose);

                        if (newHandPoseAsset != null)
                        {
                            SwitchToPose(newHandPoseAsset);
                            SaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors);

                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newHandPoseAsset));
                            AssetDatabase.SaveAssets();
                        }
                    }
                }

                GUI.enabled = _currentPoseNames.Count > 0;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnDelete, lastY), new GUIContent("Delete All Poses...", "Deletes all avatar poses")))
                {
                    string message = _avatar.IsPrefabVariant ? $"Delete all poses in {_avatar.GetAvatarPrefab().name}? Inherited poses from parent prefabs are read-only and will not be deleted." : "Delete all poses?";

                    if (EditorUtility.DisplayDialog("Confirm", message, "Yes", "Cancel"))
                    {
                        foreach (string poseName in _avatar.GetHandPoses().Select(p => p.name))
                        {
                            DeletePose(_avatar, poseName, true);
                        }

                        LoadDefaultPoseData();
                    }
                }

                GUI.enabled = _currentHandPose != null;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnMisc, lastY), new GUIContent("Select Current Pose Asset File", "Selects the current pose asset file in the project window")))
                {
                    Selection.activeObject = _currentHandPose;
                }

                lastY += VerticalHeightSingleLine;

                GUI.enabled = _avatar != null;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnCreate, lastY), new GUIContent("Add Pose From File...", "Creates a new pose using an external pose file")))
                {
                    string path = EditorUtility.OpenFilePanel("Open existing pose", CurrentFolder, "asset");

                    if (!string.IsNullOrEmpty(path))
                    {
                        string sourceFile = path;
                        path = UxrEditorUtils.ToHandPoseAssetPath(path);

                        UxrHandPoseAsset srcHandPoseAsset = AssetDatabase.LoadAssetAtPath<UxrHandPoseAsset>(path);

                        if (srcHandPoseAsset == null)
                        {
                            EditorUtility.DisplayDialog("Error", "Could not load asset " + path + " as pose.", "OK");
                        }
                        else
                        {
                            path = EditorUtility.SaveFilePanel("Select pose file to save", CurrentFolder, srcHandPoseAsset.name, "asset");

                            if (string.IsNullOrEmpty(path))
                            {
                            }
                            else if (!UxrEditorUtils.PathIsInCurrentProject(path))
                            {
                                DisplayPathNotFromThisProjectError(path);
                            }
                            else if (string.Equals(sourceFile, path))
                            {
                                EditorUtility.DisplayDialog("Error", "Source and destination files cannot be the same.", "OK");
                            }
                            else
                            {
                                string poseName = Path.GetFileNameWithoutExtension(path);
                                bool   save     = true;

                                if (GetPoseType(poseName, _avatar) != UxrHandPoseType.None)
                                {
                                    // Already exists
                                    save = EditorUtility.DisplayDialog("Overwrite?", "Pose " + poseName + " already exists. Overwrite?", "Yes", "Cancel");
                                }

                                if (save)
                                {
                                    UxrHandPoseAsset newHandPoseAsset = CreatePose(_avatar, false, path);

                                    if (newHandPoseAsset != null)
                                    {
                                        SetPoseFromAsset(_avatar, srcHandPoseAsset, newHandPoseAsset);
                                        SaveHandPose(newHandPoseAsset, SaveHandPoseFlags.HandDescriptors);
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newHandPoseAsset));
                                        SwitchToPose(newHandPoseAsset);
                                    }
                                }
                            }
                        }
                    }
                }

                GUI.enabled = _currentHandPose != null && _avatar != null;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnMisc, lastY), new GUIContent("Bake Pose In Avatar", "Sets the hand in the avatar prefab to the current pose")))
                {
                    BakeCurrentPoseInAvatar(_avatar);
                }

                lastY += VerticalHeightSingleLine;

                GUI.enabled = _avatar != null;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnCreate, lastY), new GUIContent("Add All Poses From Folder...", "Adds all pose files from a folder to the current avatar")))
                {
                    string pathSrc = EditorUtility.OpenFolderPanel("Pose Assets Source Folder", CurrentFolder, string.Empty);

                    if (string.IsNullOrEmpty(pathSrc))
                    {
                    }
                    else if (!UxrEditorUtils.PathIsInCurrentProject(pathSrc))
                    {
                        DisplayPathNotFromThisProjectError(pathSrc);
                    }
                    else
                    {
                        List<UxrHandPoseAsset> handPosesToAdd = new List<UxrHandPoseAsset>();
                        string[]               files          = UxrEditorUtils.GetHandPosePresetFiles();

                        foreach (string file in files)
                        {
                            if (AssetDatabase.GetMainAssetTypeAtPath(file) == typeof(UxrHandPoseAsset))
                            {
                                handPosesToAdd.Add((UxrHandPoseAsset)AssetDatabase.LoadMainAssetAtPath(file));
                            }
                        }

                        if (handPosesToAdd.Count == 0)
                        {
                            EditorUtility.DisplayDialog("No hand poses", "Source folder doesn't contain any hand pose assets.", "OK");
                        }
                        else
                        {
                            string pathDst = EditorUtility.SaveFolderPanel("Pose Assets Destination Folder", CurrentFolder, string.Empty);

                            if (string.IsNullOrEmpty(pathDst))
                            {
                            }
                            else if (string.Equals(pathSrc, pathDst))
                            {
                                EditorUtility.DisplayDialog("Path error", "Hand pose source and destination paths cannot be the same.", "OK");
                            }
                            else if (!UxrEditorUtils.PathIsInCurrentProject(pathDst))
                            {
                                DisplayPathNotFromThisProjectError(pathDst);
                            }
                            else
                            {
                                foreach (UxrHandPoseAsset srcHandPoseAsset in handPosesToAdd)
                                {
                                    UxrHandPoseAsset newHandPoseAsset = CreatePose(_avatar, false, pathDst + "/" + srcHandPoseAsset.name + ".asset");

                                    if (newHandPoseAsset != null)
                                    {
                                        SetPoseFromAsset(_avatar, srcHandPoseAsset, newHandPoseAsset);
                                        SaveHandPose(newHandPoseAsset, SaveHandPoseFlags.HandDescriptors);
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newHandPoseAsset));
                                    }
                                }

                                if (_currentHandPose != null)
                                {
                                    SwitchToPose(_currentHandPose);
                                }
                                else
                                {
                                    LoadDefaultPoseData();
                                }
                            }
                        }
                    }
                }

                lastY += VerticalHeightSingleLine;

                if (GUI.Button(GetPosesMenuButtonRect(PosesMenuColumnCreate, lastY), new GUIContent("Add All Preset Poses", "Creates all presets in the current avatar")))
                {
                    CreateAllHandPosePresets(_avatar);
                }
            }

            EditorGUILayout.EndVertical();

            // End Main Menu ////////////////////////////////

            EditorGUILayout.Space();

            // Pose menu ////////////////////////////////////

            EditorGUILayout.BeginVertical("box", GUILayout.Width(HandMarginHorizontal * 2 + _leftHandTex.width + _rightHandTex.width));

            _foldoutPoseMenu = UxrEditorUtils.FoldoutStylish("Pose menu:", _foldoutPoseMenu);

            if (_foldoutPoseMenu)
            {
                GUI.enabled = _avatar != null;

                if (_avatar != null)
                {
                    if (_avatar.IsPrefabVariant)
                    {
                        EditorGUI.BeginChangeCheck();
                        _showInheritedPoses = EditorGUILayout.Toggle(new GUIContent("Show inherited poses", ""), _showInheritedPoses);
                        if (EditorGUI.EndChangeCheck())
                        {
                            _currentPoseNames = GetAvatarPoseNames(_avatar, _showInheritedPoses);

                            if (_showInheritedPoses == false && _currentHandPose != null)
                            {
                                // Show inherited poses has been disabled. Check if the selected pose isn't there anymore.

                                UxrHandPoseAsset handPose = _avatar.GetHandPose(_currentHandPose.name, false);

                                if (_currentHandPose != handPose)
                                {
                                    if (_currentPoseNames.Count == 0)
                                    {
                                        // No poses available: reset hands

                                        _poseIndex       = 0;
                                        _poseTypeIndex   = -1;
                                        _blendValue      = 0.0f;
                                        _poseName        = null;
                                        _currentHandPose = null;

                                        ResetHandTransforms(_avatar, UxrHandSide.Left);
                                        ResetHandTransforms(_avatar, UxrHandSide.Right);
                                    }
                                    else
                                    {
                                        // Switch to first pose in the list

                                        _poseIndex       = 0;
                                        _currentHandPose = _avatar.GetHandPose(_currentPoseNames[0], false);

                                        SwitchToPose(_currentHandPose);
                                    }
                                }
                            }
                            else if (_showInheritedPoses && _currentHandPose == null)
                            {
                                // Show inherited poses has been enabled and no pose is currently selected: Load default pose.

                                LoadDefaultPoseData();
                                _updateHandTransforms = true;
                            }
                        }
                    }
                }

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();

                int oldPoseIndex = _poseIndex;
                _poseIndex = EditorGUILayout.Popup(new GUIContent("Pose", "Selects the pose currently being edited"),
                                                   _poseIndex,
                                                   UxrEditorUtils.ToGUIContentArray(GetFormattedAvatarPoseNames(_currentPoseNames, _avatar != null ? _avatar.DefaultHandPoseName : string.Empty)),
                                                   GUILayout.Width(FieldWidth));

                if (EditorGUI.EndChangeCheck())
                {
                    if (_autoSave)
                    {
                        AutoSave(SaveHandPoseFlags.HandDescriptors);
                    }

                    if (CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors))
                    {
                        SwitchToPose(_poseIndex >= 0 && _poseIndex < _currentPoseNames.Count ? _avatar.GetHandPose(_currentPoseNames[_poseIndex]) : null);
                    }
                    else
                    {
                        _poseIndex = oldPoseIndex;
                    }
                }

                if (_poseName == null && _poseIndex >= 0 && _poseIndex < _currentPoseNames.Count)
                {
                    _poseName = _currentPoseNames[_poseIndex];
                }

                if (_poseTypeIndex == -1)
                {
                    // Usually when something is reset.

                    _currentPoseNames = GetAvatarPoseNames(_avatar, _showInheritedPoses);

                    if (_poseIndex >= 0 && _poseIndex < _currentPoseNames.Count)
                    {
                        _poseTypeIndex = GetPoseTypes().IndexOf(GetPoseType(_currentPoseNames[_poseIndex], _avatar).ToString());
                        _blendValue    = 0.0f;
                    }
                }

                if (_avatar != null && _currentHandPose != null && _currentPoseNames.Count > 0)
                {
                    GUIStyle textStyle = EditorStyles.label;
                    textStyle.wordWrap = true;
                    Color color = GUI.color;
                    GUI.color = Color.yellow;

                    if (isReadOnlyPose && _avatar.GetParentPrefab(_currentHandPose) != null)
                    {
                        EditorGUILayout.LabelField($"Inherited poses are read-only to prevent modifying data from a parent prefab.\nLoad {_avatar.GetParentPrefab(_currentHandPose).name} instead if you want to modify the pose.", textStyle);
                    }

                    if (_avatar.IsHandPoseOverriden(_currentHandPose.name, out UxrAvatar originalAvatar))
                    {
                        EditorGUILayout.LabelField($"Pose overrides \"{_currentHandPose.name}\" in parent prefab {originalAvatar.name}", textStyle);
                    }

                    GUI.color = color;

                    if (!string.IsNullOrEmpty(_avatar.DefaultHandPoseName) && _avatar.DefaultHandPoseName == _currentHandPose.name)
                    {
                        EditorGUILayout.LabelField($"{_currentHandPose.name} is the default pose, adopted when the avatar is not using any other pose.", textStyle);
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUI.enabled = _currentHandPose != null && _currentPoseNames.Count > 0 && _poseTypeIndex != -1 && !isReadOnlyPose;

                EditorGUI.BeginChangeCheck();
                int oldPoseTypeIndex = _poseTypeIndex;
                _poseTypeIndex = EditorGUILayout.Popup(new GUIContent("Pose Type",
                                                                      "Use fixed pose types for hand gestures or ad-hoc grip poses. Blend poses have an open grip and closed grip and allow to use the same pose to grab objects of different sizes by adjusting the blend parameter on each."),
                                                       _poseTypeIndex,
                                                       UxrEditorUtils.ToGUIContentArray(GetPoseTypes()),
                                                       GUILayout.Width(FieldWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    if (_autoSave)
                    {
                        AutoSave(SaveHandPoseFlags.HandDescriptors);
                    }

                    if (CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors))
                    {
                        _currentHandPose.PoseType = CurrentPoseType;
                        _updateHandTransforms     = true;
                        _blendValue               = 0.0f;
                    }
                    else
                    {
                        _poseTypeIndex = oldPoseTypeIndex;
                    }
                }

                if (CurrentPoseType == UxrHandPoseType.Blend)
                {
                    EditorGUI.BeginChangeCheck();
                    int blendPoseIndex = EditorGUILayout.Popup(new GUIContent("Edit Blend Pose", "The blend pose currently being edited"),
                                                               GetBlendPoseTypes().IndexOf(CurrentBlendPoseType.ToString()),
                                                               UxrEditorUtils.ToGUIContentArray(GetBlendPoseTypes()),
                                                               GUILayout.Width(FieldWidth));

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (_autoSave)
                        {
                            AutoSave(SaveHandPoseFlags.HandDescriptors);
                        }

                        if (CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors))
                        {
                            UxrBlendPoseType blendPoseType = blendPoseIndex != -1 ? (UxrBlendPoseType)Enum.Parse(typeof(UxrBlendPoseType), GetBlendPoseTypes()[blendPoseIndex]) : UxrBlendPoseType.None;
                            _blendValue           = blendPoseType == UxrBlendPoseType.ClosedGrip ? 1.0f : 0.0f;
                            _updateHandTransforms = true;
                        }
                    }

                    GUI.enabled = true;

                    EditorGUI.BeginChangeCheck();
                    float newBlendValue = EditorGUILayout.Slider(new GUIContent("Preview Blend", "Previews in the scene window how the hand transitions from an open grip to a closed grip using the blend parameter"),
                                                                 _blendValue,
                                                                 0.0f,
                                                                 1.0f,
                                                                 GUILayout.Width(FieldWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        bool allow = true;

                        if (IsOneOrZero(_blendValue))
                        {
                            if (_autoSave)
                            {
                                AutoSave(SaveHandPoseFlags.HandDescriptors);
                            }

                            allow = CheckWarnSaveHandPose(_currentHandPose, SaveHandPoseFlags.HandDescriptors);
                        }

                        if (allow)
                        {
                            _blendValue              = newBlendValue;
                            _updateHandTransforms    = true;
                            registerUndoOnPoseUpdate = false;
                        }
                    }
                }

                GUI.enabled = _currentHandPose != null && !isReadOnlyPose;

                GUILayout.BeginHorizontal();

                _poseName = EditorGUILayout.TextField(new GUIContent("Pose Name", "The name of the pose. Use the Update Pose Name button afterwards."), _poseName ?? string.Empty, GUILayout.Width(FieldWidth));

                bool isNewNameAvailable = _currentHandPose != null && !string.IsNullOrEmpty(_poseName) && _poseName != _currentHandPose.name;
                bool isNewNameValid     = isNewNameAvailable && _poseName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && _avatar.GetHandPose(_poseName, false) == null;

                if (isNewNameValid && GUILayout.Button(new GUIContent("Update Pose Name", "Updates the pose name from the content in the 'Pose Name' field"), GUILayout.Width(ButtonWidth)))
                {
                    if (RenamePose(_avatar, _currentHandPose, _poseName))
                    {
                        SaveHandPose(_currentHandPose, SaveHandPoseFlags.All);

                        _currentPoseNames = GetAvatarPoseNames(_avatar, _showInheritedPoses);
                        _poseIndex        = _currentPoseNames.IndexOf(_poseName);
                    }
                }

                if (isNewNameAvailable)
                {
                    if (_avatar.GetHandPose(_poseName, false) != null)
                    {
                        EditorGUILayout.LabelField($"The avatar already has a pose named {_poseName}");
                    }
                    else if (!isNewNameValid)
                    {
                        EditorGUILayout.LabelField("Invalid pose name");
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            // End pose menu ////////////////////////////////

            // Hands ////////////////////////////////////////

            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(HandMarginHorizontal * 2 + _leftHandTex.width + _rightHandTex.width));

            _foldoutHands = UxrEditorUtils.FoldoutStylish("Hand controls:", _foldoutHands);

            if (_foldoutHands)
            {
#if SHOW_AUTOSAVE
                // Leave commented for now since Auto-save sometimes was found to corrupt the animation data for some reason
                _autoSave = GUILayout.Toggle(_autoSave, new GUIContent("Auto-save"));
#endif
                _selectGameObjectOnClick = GUILayout.Toggle(_selectGameObjectOnClick, new GUIContent("Select finger bone in Hierarchy Window when interacting with spinner"));
                _mouseRotationSpeed      = EditorGUILayout.Slider(new GUIContent("Spinner Sensitivity"), _mouseRotationSpeed, 0.1f, 3.0f, GUILayout.Width(FieldWidth));

                GUI.enabled = _currentHandPose != null && _currentPoseNames.Count > 0 && _poseTypeIndex != -1 && !isReadOnlyPose;

                lastY = (int)GUILayoutUtility.GetLastRect().yMax;

                Vector2 leftHandTexturePos  = new Vector2(HandMarginHorizontal,                           lastY + HandMarginVertical / 2);
                Vector2 rightHandTexturePos = new Vector2(HandMarginHorizontal * 2 + _rightHandTex.width, lastY + HandMarginVertical / 2);

                GUI.DrawTexture(new Rect(leftHandTexturePos.x,  leftHandTexturePos.y,  _leftHandTex.width,  _leftHandTex.height),  _leftHandTex);
                GUI.DrawTexture(new Rect(rightHandTexturePos.x, rightHandTexturePos.y, _rightHandTex.width, _rightHandTex.height), _rightHandTex);

                GUILayout.Space(_rightHandTex.height + HandMarginVertical / 2 - EditorGUIUtility.singleLineHeight * 2 - EditorGUIUtility.standardVerticalSpacing * 2);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(HandMarginHorizontal + _leftHandTex.width + HandMarginHorizontal / 2 - ButtonWidth / 2);

                if (GUILayout.Button(new GUIContent("Copy Left To Right >", "Mirrors the current left hand's finger transforms to the right hand"), GUILayout.Width(ButtonWidth)))
                {
                    RegisterHandsUndo("Copy hand");
                    SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));

                    // Mirror hand descriptor
                    UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, CurrentPoseType, CurrentBlendPoseType, UxrHandSide.Left).Mirrored();
                    // Recompute relative matrices
                    newHandDescriptor.Compute(_avatar, UxrHandSide.Right, true);
                    // Set as new hand descriptor
                    SetHandDescriptor(_currentHandPose, CurrentPoseType, CurrentBlendPoseType, UxrHandSide.Right, newHandDescriptor);
                    UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Right, newHandDescriptor);
                    SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                    _updateHandTransforms = true;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(HandMarginHorizontal + _leftHandTex.width + HandMarginHorizontal / 2 - ButtonWidth / 2);

                if (GUILayout.Button(new GUIContent("< Copy Right To Left", "Mirrors the current right hand's finger transforms to the left hand"), GUILayout.Width(ButtonWidth)))
                {
                    RegisterHandsUndo("Copy hand");
                    SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                    // Mirror hand descriptor
                    UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, CurrentPoseType, CurrentBlendPoseType, UxrHandSide.Right).Mirrored();
                    // Recompute relative matrices
                    newHandDescriptor.Compute(_avatar, UxrHandSide.Left, true);
                    // Set as new hand descriptor
                    SetHandDescriptor(_currentHandPose, CurrentPoseType, CurrentBlendPoseType, UxrHandSide.Left, newHandDescriptor);
                    UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Left, newHandDescriptor);
                    SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                    _updateHandTransforms = true;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(HandMarginVertical / 2);

                lastY = (int)GUILayoutUtility.GetLastRect().yMax;
                int buttonsStartY = lastY;

                string resetButtonString = "Reset...";

                if (CurrentPoseType == UxrHandPoseType.Blend)
                {
                    resetButtonString = CurrentBlendPoseType == UxrBlendPoseType.ClosedGrip ? "Reset Closed Grip..." : "Reset Open Grip...";
                }

                if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent(resetButtonString, "Resets the left hand's finger transforms to their default position/orientation")))
                {
                    if (EditorUtility.DisplayDialog("Override data", "Override current with default pose?", "Yes", "Cancel"))
                    {
                        RegisterHandsUndo("Load Default Hand");
                        ResetHandTransforms(_avatar, UxrHandSide.Left);
                    }
                }

                if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent(resetButtonString, "Resets the right hand's finger transforms to their default position/orientation")))
                {
                    if (EditorUtility.DisplayDialog("Override data", "Override current with default pose?", "Yes", "Cancel"))
                    {
                        RegisterHandsUndo("Load Default Hand");
                        ResetHandTransforms(_avatar, UxrHandSide.Right);
                    }
                }

                lastY += VerticalHeightSingleLine;

                if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Load External Left...", "Sets the left hand's finger transforms using an external hand pose asset file")))
                {
                    PromptLoadExternalPose(true, false);
                }

                if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Load External Right...", "Sets the right hand's finger transforms using an external hand pose asset file")))
                {
                    PromptLoadExternalPose(false, true);
                }

                if (CurrentPoseType == UxrHandPoseType.Blend && _currentHandPose != null)
                {
                    lastY += VerticalHeightSingleLine;

                    if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Copy Fixed Pose To Open Grip", "Copies the left hand fingers from the fixed pose to the open grip")))
                    {
                        RegisterHandsUndo("Copy fixed to open grip");
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, UxrHandPoseType.Fixed, UxrBlendPoseType.None, UxrHandSide.Left);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.OpenGrip, UxrHandSide.Left, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Left, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.OpenGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }

                    if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Copy Fixed Pose To Open Grip", "Copies the right hand fingers from the fixed pose to the open grip")))
                    {
                        RegisterHandsUndo("Copy fixed to open grip");
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, UxrHandPoseType.Fixed, UxrBlendPoseType.None, UxrHandSide.Right);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.OpenGrip, UxrHandSide.Right, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Right, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.OpenGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }

                    lastY += VerticalHeightSingleLine;

                    if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Copy Fixed Pose To Closed Grip", "Copies the left hand fingers from the fixed pose to the closed grip")))
                    {
                        RegisterHandsUndo("Copy fixed to closed grip");
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, UxrHandPoseType.Fixed, UxrBlendPoseType.None, UxrHandSide.Left);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.ClosedGrip, UxrHandSide.Left, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Left, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.ClosedGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }

                    if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Copy Fixed Pose To Closed Grip", "Copies the right hand fingers from the fixed pose to the closed grip")))
                    {
                        RegisterHandsUndo("Copy fixed to closed grip");
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, UxrHandPoseType.Fixed, UxrBlendPoseType.None, UxrHandSide.Right);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.ClosedGrip, UxrHandSide.Right, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Right, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.ClosedGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }

                    lastY += VerticalHeightSingleLine;

                    if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Copy Open Grip To Closed Grip", "Copies the left hand fingers from the open grip to the closed grip")))
                    {
                        RegisterHandsUndo("Copy open to closed grip");
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.OpenGrip, UxrHandSide.Left);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.ClosedGrip, UxrHandSide.Left, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Left, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.ClosedGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }

                    if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Copy Open Grip To Closed Grip", "Copies the right hand fingers from the open grip to the closed grip")))
                    {
                        RegisterHandsUndo("Copy open to closed grip");
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.OpenGrip, UxrHandSide.Right);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.ClosedGrip, UxrHandSide.Right, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Right, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.ClosedGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }

                    lastY += VerticalHeightSingleLine;

                    if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Copy Closed Grip To Open Grip", "Copies the left hand fingers from the closed grip to the open grip")))
                    {
                        RegisterHandsUndo("Copy closed to open grip");
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.ClosedGrip, UxrHandSide.Left);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.OpenGrip, UxrHandSide.Left, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Left, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Left, CurrentPoseType, UxrBlendPoseType.OpenGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.OpenGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }

                    if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Copy Closed Grip To Open Grip", "Copies the right hand fingers from the closed grip to the open grip")))
                    {
                        RegisterHandsUndo("Copy closed to open grip");
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        UxrHandDescriptor newHandDescriptor = GetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.ClosedGrip, UxrHandSide.Right);
                        SetHandDescriptor(_currentHandPose, CurrentPoseType, UxrBlendPoseType.OpenGrip, UxrHandSide.Right, newHandDescriptor);
                        UxrAvatarRig.UpdateHandUsingDescriptor(_avatar, UxrHandSide.Right, newHandDescriptor);
                        SavePose(_currentHandPose, UxrHandSide.Right, CurrentPoseType, UxrBlendPoseType.OpenGrip, SaveHandPoseFlags.HandDescriptors);

                        if (CurrentBlendPoseType == UxrBlendPoseType.OpenGrip)
                        {
                            _updateHandTransforms = true;
                        }
                    }
                }

                // Place snap buttons

                lastY       += VerticalHeightSingleLine;
                GUI.enabled =  !string.IsNullOrEmpty(_poseName) && _avatar != null;

                if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Place Snap On Grabbable Object")))
                {
                    TryPlaceSnapTransform(_avatar, _poseName, UxrHandSide.Left);
                }

                if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Place Snap On Grabbable Object")))
                {
                    TryPlaceSnapTransform(_avatar, _poseName, UxrHandSide.Right);
                }

                GUI.enabled = _currentHandPose != null && _currentPoseNames.Count > 0 && _poseTypeIndex != -1 && !isReadOnlyPose;

                // Save buttons
#if SHOW_AUTOSAVE
                GUI.color = Color.green;
                lastY += VerticalHeightSingleLine;

                if (_currentHandPose != null && IsOneOrZero(_blendValue) && IsCurrentHandDifferentFromPose(_currentHandPose, UxrHandSide.Left, CurrentBlendPoseType))
                {
                    if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Save Left Hand")))
                    {
                        SavePose(_currentHandPose, UxrHandSide.Left, poseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        AssetDatabase.SaveAssets();
                    }
                }

                if (_currentHandPose != null && IsOneOrZero(_blendValue) && IsCurrentHandDifferentFromPose(_currentHandPose, UxrHandSide.Right, CurrentBlendPoseType))
                {
                    if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Save Right Hand")))
                    {
                        SavePose(_currentHandPose, UxrHandSide.Right, poseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        AssetDatabase.SaveAssets();
                    }
                }

                GUI.color = guiColor;
#endif
                // Temporal dev buttons to set pose using current HandDescriptor. Used when developing auto-save and fixing bugs.
/*
                lastY  += VerticalHeightSingleLine;

                if (_currentHandPose != null && IsOneOrZero(_blendValue))
                {
                    if (GUI.Button(GetLeftHandButtonRect(lastY), new GUIContent("Restore Left Hand (use HandDescriptor)")))
                    {
                        UpdateHandUsingCurrentDescriptor(_avatar, UxrHandSide.Left);
                    }
                }

                if (_currentHandPose != null && IsOneOrZero(_blendValue))
                {
                    if (GUI.Button(GetRightHandButtonRect(lastY), new GUIContent("Restore Right Hand (use HandDescriptor)")))
                    {
                        UpdateHandUsingCurrentDescriptor(_avatar, UxrHandSide.Right);
                    }
                }
*/
                GUILayout.Space(lastY - buttonsStartY + VerticalHeightSingleLine);

                // Handle finger spinners

                if (_avatar != null && GUI.enabled)
                {
                    HandleFingerSpinners(leftHandTexturePos, rightHandTexturePos, _selectGameObjectOnClick, CurrentPoseType, CurrentBlendPoseType);
                }
            }

            EditorGUILayout.EndVertical();

            lastY = (int)GUILayoutUtility.GetLastRect().yMax;
            int minWindowHeight = lastY;

            // Presets ////////////////////////////////////

            int  rightAreaX      = HandMarginHorizontal * 2 + _rightHandTex.width * 2 + HandMarginHorizontal + 10;
            Rect areaPresetsRect = new Rect(rightAreaX, 4, position.width - rightAreaX, position.height);

            GUI.enabled = true;

            GUILayout.BeginArea(areaPresetsRect);
            EditorGUILayout.BeginVertical("box", GUILayout.Width(areaPresetsRect.width));

            _foldoutPresets = UxrEditorUtils.FoldoutStylish("Hand pose presets:", _foldoutPresets);

            if (_foldoutPresets)
            {
                int presetMargin = 10;
                int presetWidth  = 200;
                int presetHeight = 140;
                int columns      = Mathf.Max(1, (int)(areaPresetsRect.width - EditorGUIUtility.singleLineHeight - presetMargin) / (presetMargin + presetWidth));

                minSize = new Vector2(rightAreaX + presetWidth * 1.2f, minWindowHeight);

                _presetsScrollPosition = EditorGUILayout.BeginScrollView(_presetsScrollPosition, false, false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                GUI.enabled = _currentHandPose != null && _currentPoseNames.Count > 0 && _poseTypeIndex != -1 && !isReadOnlyPose;

                if (_handPosePresets != null)
                {
                    bool finished = false;

                    for (int y = 0; finished == false; ++y)
                    {
                        EditorGUILayout.BeginHorizontal();
                        for (int x = 0; x < columns; ++x)
                        {
                            int index = y * columns + x;

                            if (index >= _handPosePresets.Count)
                            {
                                finished = true;
                                break;
                            }

                            Texture2D buttonNormal  = GUI.skin.button.normal.background;
                            Texture2D buttonHover   = GUI.skin.button.hover.background;
                            Texture2D buttonActive  = GUI.skin.button.active.background;
                            Texture2D buttonFocused = GUI.skin.button.focused.background;

                            GUI.skin.button.normal.background  = _handPosePresets[index].Thumbnail;
                            GUI.skin.button.hover.background   = _handPosePresets[index].Thumbnail;
                            GUI.skin.button.active.background  = _handPosePresets[index].Thumbnail;
                            GUI.skin.button.focused.background = _handPosePresets[index].Thumbnail;

                            GUI.skin.button.alignment = TextAnchor.LowerCenter;

                            string poseSuffix = "";
                            string tooltip    = $"Load {_handPosePresets[index].Pose.name} pose";

                            if (_handPosePresets[index].Pose.PoseType == UxrHandPoseType.Fixed)
                            {
                                poseSuffix =  " (F)";
                                tooltip    += " (Fixed pose type)";
                            }
                            else if (_handPosePresets[index].Pose.PoseType == UxrHandPoseType.Blend)
                            {
                                poseSuffix =  " (B)";
                                tooltip    += " (Blend pose type)";
                            }

                            if (GUILayout.Button(new GUIContent(_handPosePresets[index].Pose.name + poseSuffix, tooltip), GUILayout.Width(presetWidth), GUILayout.Height(presetHeight)))
                            {
                                if (EditorUtility.DisplayDialog("Override data", "Override current pose with preset " + _handPosePresets[index].Pose.name + "?", "Yes", "Cancel"))
                                {
                                    if (_handPosePresets[index].Pose.Version > UxrHandPoseAsset.CurrentVersion)
                                    {
                                        EditorUtility.DisplayDialog("Warning", "File was saved using a newer version of the Hand Pose Editor. Pose may not be loaded correctly.", "OK");
                                    }

                                    RegisterHandsUndo("Load Preset " + _handPosePresets[index].Pose.name);
                                    SetPoseFromAsset(_avatar, _handPosePresets[index].Pose, _currentHandPose);
                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                                    AssetDatabase.SaveAssets();

                                    _poseTypeIndex = GetPoseTypes().IndexOf(_currentHandPose.PoseType.ToString());
                                }
                            }

                            GUI.skin.button.normal.background  = buttonNormal;
                            GUI.skin.button.hover.background   = buttonHover;
                            GUI.skin.button.active.background  = buttonActive;
                            GUI.skin.button.focused.background = buttonFocused;

                            GUILayout.Space(presetMargin);
                        }
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(presetMargin);
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            // Update current pose?

            if (_updateHandTransforms && _poseIndex >= 0 && _poseIndex < _currentPoseNames.Count)
            {
                if (_currentHandPose != null && registerUndoOnPoseUpdate)
                {
                    Undo.RecordObject(_currentHandPose, "Update pose");
                }

                // This hasn't worked sometimes: UpdateHandTransforms(_avatar, poseList[_poseIndex], _blendValue);
                UpdateHandUsingCurrentDescriptor(_avatar, UxrHandSide.Left);
                UpdateHandUsingCurrentDescriptor(_avatar, UxrHandSide.Right);
                _updateHandTransforms = false;

                // Force update of skin meshes, sometimes they don't get updated even if the transforms are changed

                foreach (SkinnedMeshRenderer skin in _avatar.GetAllAvatarRendererComponents())
                {
                    bool enabled = skin.enabled;
                    skin.enabled = false;
                    skin.enabled = true;
                    skin.enabled = enabled;
                }

                // Repaint scene windows

                SceneView.RepaintAll();

                // Update other editor pose meshes

                if (_avatar != null && _currentHandPose != null)
                {
                    UxrGrabbableObjectEditor.RefreshGrabPoseMeshes(_avatar, _currentHandPose);
                    UxrGrabbableObjectSnapTransformEditor.RefreshGrabPoseMeshes(_avatar, _currentHandPose);
                }
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called by Unity when the handles can be drawn.
        /// </summary>
        /// <param name="sceneView">Scene information</param>
        private void SceneView_DuringSceneGUI(SceneView sceneView)
        {
            if (_avatar != null)
            {
                // Do something with Unity's Handles class
            }
        }

        /// <summary>
        ///     Called by Unity when the scripts finished compiling. Reload data to avoid losing references and state.
        /// </summary>
        [DidReloadScripts]
        private static void Unity_OnScriptsReloaded()
        {
            if (s_handPoseEditorWindow != null && s_handPoseEditorWindow._avatar != null)
            {
                // These things are lost when scripts are recompiled
                s_handPoseEditorWindow._currentPoseNames = GetAvatarPoseNames(s_handPoseEditorWindow._avatar, s_handPoseEditorWindow._showInheritedPoses);
                s_handPoseEditorWindow.BuildFingerSpinners();

                // Force update transforms in scene view
                s_handPoseEditorWindow._updateHandTransforms = true;

                // Refresh presets
                s_handPoseEditorWindow.RefreshHandPosePresets();
            }
        }

        /// <summary>
        ///     Called by Unity when the play mode state changed.
        /// </summary>
        /// <param name="playModeStateChange">State change</param>
        private void EditorApplication_PlaymodeStateChanged(PlayModeStateChange playModeStateChange)
        {
        }

        /// <summary>
        ///     Called by Unity when an undo/redo was performed.
        /// </summary>
        private void Undo_OnUndoRedo()
        {
            if (_autoSave)
            {
                AutoSave(SaveHandPoseFlags.HandDescriptors);
            }

            if (_avatar != null && _currentHandPose != null)
            {
                UxrGrabbableObjectEditor.RefreshGrabPoseMeshes(_avatar, _currentHandPose);
                UxrGrabbableObjectSnapTransformEditor.RefreshGrabPoseMeshes(_avatar, _currentHandPose);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the asset path of a pose if the asset and the pose exist.
        /// </summary>
        /// <param name="handPose">Pose to get the path of</param>
        /// <returns>path relative to the Assets folder in which the pose (UxrHandPoseAsset ScriptableObject) is stored</returns>
        private static string GetPoseAssetPath(UxrHandPoseAsset handPose)
        {
            if (handPose == null)
            {
                return null;
            }

            return AssetDatabase.GetAssetPath(handPose);
        }

        /// <summary>
        ///     Displays an error telling the given path does not belong to the current project.
        /// </summary>
        /// <param name="path">Path that doesn't belong to the current project</param>
        private static void DisplayPathNotFromThisProjectError(string path)
        {
            EditorUtility.DisplayDialog("Error", "Path " + path + " cannot be outside the project's Assets folder", "OK");
        }

        // Initialization

        /// <summary>
        ///     Loads the avatar and optionally a pose.
        /// </summary>
        /// <param name="avatar">Avatar to load</param>
        /// <param name="handPose">Pose to load or null to load the default hand pose</param>
        private void LoadAvatar(UxrAvatar avatar, UxrHandPoseAsset handPose = null)
        {
            if (avatar != null)
            {
                if (_avatar != avatar || _fingerSpinners == null || _fingerSpinners.Count == 0 || handPose == null)
                {
                    _avatar = avatar;

                    if (!avatar.AvatarRig.HasFingerData())
                    {
                        _fingerSpinners = new List<UxrFingerSpinner>();
                        EditorUtility.DisplayDialog("Warning", $"Avatar hands and finger nodes could not be resolved correctly. Please try setting them up manually in the {nameof(UxrAvatar)} Rig component", "OK");
                    }
                    else
                    {
                        BuildFingerSpinners();
                    }
                }

                if (handPose == null)
                {
                    LoadDefaultPoseData();
                }
                else
                {
                    SwitchToPose(handPose);
                }

                _updateHandTransforms = true;
            }
        }

        /// <summary>
        ///     Loads all default transform values of a hand, coming from the values defined in the 3d file or prefab.
        /// </summary>
        /// <param name="avatar">Avatar to reset</param>
        /// <param name="handSide">Hand to reset</param>
        private void ResetHandTransforms(UxrAvatar avatar, UxrHandSide handSide)
        {
            UxrAvatarHand hand = avatar.GetHand(handSide);

            ResetFinger(hand.Wrist, hand.Index);
            ResetFinger(hand.Wrist, hand.Middle);
            ResetFinger(hand.Wrist, hand.Ring);
            ResetFinger(hand.Wrist, hand.Little);
            ResetFinger(hand.Wrist, hand.Thumb);
        }

        /// <summary>
        ///     Loads all default transform values of a finger.
        /// </summary>
        /// <param name="hand">Hand being processed</param>
        /// <param name="finger">Finger to reset</param>
        private void ResetFinger(Transform hand, UxrAvatarFinger finger)
        {
            Transform current = finger.Distal;

            while (current != hand && current != null)
            {
                PrefabUtility.RevertObjectOverride(current, InteractionMode.AutomatedAction);
                current = current.parent;
            }

            ResetFingerSpinners(finger);
            _updateHandTransforms = true;
        }

        /// <summary>
        ///     Tries to load the default pose configuration.
        /// </summary>
        private void LoadDefaultPoseData()
        {
            _poseIndex        = 0;
            _poseTypeIndex    = -1;
            _blendValue       = 0.0f;
            _poseName         = null;
            _currentHandPose  = null;
            _currentPoseNames = GetAvatarPoseNames(_avatar, _showInheritedPoses);

            if (_avatar.DefaultHandPoseName != null)
            {
                _currentHandPose = _avatar.GetHandPose(_avatar.DefaultHandPoseName, _showInheritedPoses);

                if (_currentHandPose != null)
                {
                    _poseName  = _avatar.DefaultHandPoseName;
                    _poseIndex = _currentPoseNames.IndexOf(_poseName);
                }
            }
            else if (_currentPoseNames.Count > 0)
            {
                _currentHandPose = _avatar.GetHandPose(_currentPoseNames[0], _showInheritedPoses);

                if (_currentHandPose)
                {
                    _poseName  = _currentPoseNames[0];
                    _poseIndex = 0;
                }
            }

            ResetFingerSpinners();
        }

        // Poses

        /// <summary>
        ///     Switches the currently active pose.
        /// </summary>
        /// <param name="handPose">The new pose to switch to</param>
        private void SwitchToPose(UxrHandPoseAsset handPose)
        {
            _currentHandPose      = handPose;
            _currentPoseNames     = GetAvatarPoseNames(_avatar, _showInheritedPoses);
            _poseIndex            = handPose == null ? 0 : _currentPoseNames.IndexOf(_currentHandPose.name);
            _poseTypeIndex        = -1;
            _blendValue           = 0.0f;
            _poseName             = handPose == null ? null : handPose.name;
            _updateHandTransforms = true;

            ResetFingerSpinners();
        }

        /// <summary>
        ///     Creates a list with the different enum PoseTypes values except for None.
        /// </summary>
        /// <returns>List with different enum PoseTypes values. None is not included</returns>
        private List<string> GetPoseTypes()
        {
            List<string> listTypes = new List<string>();
            string[]     types     = Enum.GetNames(typeof(UxrHandPoseType));

            foreach (string poseType in types)
            {
                if (poseType != UxrHandPoseType.None.ToString())
                {
                    listTypes.Add(poseType);
                }
            }

            return listTypes;
        }

        /// <summary>
        ///     Creates a list with the different enum BlendPoseTypes values except for None.
        /// </summary>
        /// <returns>List with different enum BlendPoseTypes values. None is not included</returns>
        private List<string> GetBlendPoseTypes()
        {
            List<string> listTypes = new List<string>();
            string[]     types     = Enum.GetNames(typeof(UxrBlendPoseType));

            foreach (string blendPoseType in types)
            {
                if (blendPoseType != UxrBlendPoseType.None.ToString())
                {
                    listTypes.Add(blendPoseType);
                }
            }

            return listTypes;
        }

        /// <summary>
        ///     Sets the pose as the default animator state.
        /// </summary>
        /// <param name="avatar">The avatar with the pose</param>
        /// <param name="poseName">The pose name set to default</param>
        private void SetPoseAsDefault(UxrAvatar avatar, string poseName)
        {
            if (avatar != null)
            {
                SerializedObject serializedObject = new SerializedObject(avatar.GetAvatarPrefab());
                serializedObject.Update();
                SerializedProperty defaultHandPose = serializedObject.FindProperty(UxrAvatarEditor.PropertyDefaultHandPose);
                defaultHandPose.objectReferenceValue = avatar.GetHandPose(poseName);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(avatar.GetAvatarPrefab());
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        ///     Sets the pose as the default animator state.
        /// </summary>
        /// <param name="avatar">The avatar with the pose</param>
        private void BakeCurrentPoseInAvatar(UxrAvatar avatar)
        {
            UxrAvatar avatarPrefab = avatar.GetAvatarPrefab();
            UpdateHandUsingCurrentDescriptor(avatarPrefab, UxrHandSide.Left);
            UpdateHandUsingCurrentDescriptor(avatarPrefab, UxrHandSide.Right);

            avatarPrefab.GetHand(UxrHandSide.Left).Transforms.ForEach(EditorUtility.SetDirty);
            avatarPrefab.GetHand(UxrHandSide.Right).Transforms.ForEach(EditorUtility.SetDirty);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        ///     Compares the current hand pose to another.
        /// </summary>
        /// <param name="handPose">Hand pose to compare it to</param>
        /// <param name="handSide">Which hand to compare</param>
        /// <returns>Whether the two hands are different</returns>
        private bool IsCurrentHandDifferentFromPose(UxrHandPoseAsset handPose, UxrHandSide handSide)
        {
            if (handPose == null)
            {
                return false;
            }

            UxrHandDescriptor handDescriptorTmp = new UxrHandDescriptor(_avatar, handSide);

            // Point to correct hand descriptor

            switch (handPose.PoseType)
            {
                case UxrHandPoseType.Fixed:
                {
                    UxrHandDescriptor handDescriptor = handPose.GetHandDescriptor(handSide);
                    return !handDescriptorTmp.Equals(handDescriptor);
                }

                case UxrHandPoseType.Blend:
                {
                    UxrHandDescriptor handDescriptorOpen   = handPose.GetHandDescriptor(handSide, UxrBlendPoseType.OpenGrip);
                    UxrHandDescriptor handDescriptorClosed = handPose.GetHandDescriptor(handSide, UxrBlendPoseType.ClosedGrip);

                    if (Mathf.Approximately(_blendValue, 0.0f))
                    {
                        return !handDescriptorTmp.Equals(handDescriptorOpen);
                    }
                    if (Mathf.Approximately(_blendValue, 1.0f))
                    {
                        return !handDescriptorTmp.Equals(handDescriptorClosed);
                    }
                    break;
                }
            }

            return false;
        }

        /// <summary>
        ///     Compares the current hand pose to another.
        /// </summary>
        /// <param name="handPose">Hand pose to compare it to</param>
        /// <param name="handSide">Which hand to compare</param>
        /// <param name="blendPoseType">Blend pose type to compare</param>
        /// <returns>Whether the two hands are different</returns>
        private bool IsCurrentHandDifferentFromPose(UxrHandPoseAsset handPose, UxrHandSide handSide, UxrBlendPoseType blendPoseType)
        {
            if (handPose == null)
            {
                return false;
            }

            UxrHandDescriptor handDescriptor    = handPose.GetHandDescriptor(handSide, blendPoseType);
            UxrHandDescriptor handDescriptorTmp = new UxrHandDescriptor(_avatar, handSide);

            return !handDescriptorTmp.Equals(handDescriptor);
        }

        /// <summary>
        ///     Checks if the given hand pose has currently unsaved changes to present the user the possibility
        ///     to save them first. A dialog box is presented to the user if the current hand pose has unsaved changes.
        ///     <list type="bullet">
        ///         <item>If the user decides to save the current changes, changes are saved and the method will return true.</item>
        ///         <item>
        ///             If the user decides to not save the current changes, changes are not saved and the method will return
        ///             true.
        ///         </item>
        ///         <item>
        ///             If the user decides to cancel, changes are not saved and the method will return false, indicating that
        ///             whatever process that was started should not continue.
        ///         </item>
        ///         <item>If there are no unsaved changes the method will simply return true.</item>
        ///     </list>
        /// </summary>
        /// <param name="handPose">The hand pose to check</param>
        /// <param name="saveFlags">Flags with the elements to save</param>
        /// <returns>
        ///     True if whatever process that was started, should continue. False if the process should not start.
        /// </returns>
        private bool CheckWarnSaveHandPose(UxrHandPoseAsset handPose, SaveHandPoseFlags saveFlags)
        {
            if (handPose == null)
            {
                return true;
            }

            bool isReadOnlyPose    = _avatar && _avatar.GetHandPose(handPose.name, false) != handPose;
            bool hasUnsavedChanges = IsCurrentHandDifferentFromPose(handPose, UxrHandSide.Left) || IsCurrentHandDifferentFromPose(handPose, UxrHandSide.Right);

            if (!isReadOnlyPose && hasUnsavedChanges)
            {
                int option = EditorUtility.DisplayDialogComplex("Unsaved Changes",
                                                                "Do you want to save the pose changes you made before continuing?",
                                                                "Save",
                                                                "Cancel",
                                                                "Don't Save");

                switch (option)
                {
                    case 0:
                        // Save
                        SaveHandPose(handPose, saveFlags);
                        return true;

                    case 1:
                        // Do not continue, user cancelled.
                        return false;

                    case 2:
                        // Ignore changes
                        return true;
                }
            }

            return true;
        }

        /// <summary>
        ///     Saves the given hand pose.
        /// </summary>
        /// <param name="handPose">The hand pose to save</param>
        /// <param name="saveFlags">Flags with the elements to save</param>
        private void SaveHandPose(UxrHandPoseAsset handPose, SaveHandPoseFlags saveFlags)
        {
            bool isReadOnlyPose = _avatar && handPose && _avatar.GetHandPose(handPose.name, false) != handPose;

            if (handPose && !isReadOnlyPose)
            {
                if (handPose.PoseType == UxrHandPoseType.Fixed)
                {
                    SavePose(handPose, UxrHandSide.Left,  handPose.PoseType, UxrBlendPoseType.None, SaveHandPoseFlags.HandDescriptors);
                    SavePose(handPose, UxrHandSide.Right, handPose.PoseType, UxrBlendPoseType.None, SaveHandPoseFlags.HandDescriptors);
                }
                else
                {
                    if (Mathf.Approximately(_blendValue, 0.0f) || Mathf.Approximately(_blendValue, 1.0f))
                    {
                        SavePose(handPose, UxrHandSide.Left,  handPose.PoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                        SavePose(handPose, UxrHandSide.Right, handPose.PoseType, CurrentBlendPoseType, SaveHandPoseFlags.HandDescriptors);
                    }
                }

                if (saveFlags.HasFlag(SaveHandPoseFlags.Assets))
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(handPose));
                    AssetDatabase.SaveAssets();
                }
            }
        }

        /// <summary>
        ///     Saves the given hand pose.
        /// </summary>
        /// <param name="handPose">The hand pose being saved</param>
        /// <param name="handSide">Which hand needs to be saved</param>
        /// <param name="poseType">The current pose type that needs to be saved</param>
        /// <param name="blendPoseType">The current blend pose (open/closed) that needs to be saved if it is a blend pose</param>
        /// <param name="saveFlags">Flags with the elements to save</param>
        private void SavePose(UxrHandPoseAsset handPose, UxrHandSide handSide, UxrHandPoseType poseType, UxrBlendPoseType blendPoseType, SaveHandPoseFlags saveFlags)
        {
            if (handPose)
            {
                Undo.RecordObject(handPose, "Update pose");
            }

            // Point to correct data

            UxrHandDescriptor handDescriptorDst = handPose.GetHandDescriptor(handSide, poseType, blendPoseType);

            // Check if the pose is the same and we can skip re-saving. Otherwise the revision control system gets changes all the time.

            UxrHandDescriptor handDescriptorTmp = new UxrHandDescriptor(_avatar, handSide);

            if (handDescriptorTmp.Equals(handDescriptorDst))
            {
                return;
            }

            // Export pose data

            if (saveFlags.HasFlag(SaveHandPoseFlags.HandDescriptors))
            {
                handDescriptorDst?.Compute(_avatar, handSide);
                EditorUtility.SetDirty(handPose);
            }

            // Save asset?

            if (saveFlags.HasFlag(SaveHandPoseFlags.Assets))
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(handPose));
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        ///     Renames a pose.
        /// </summary>
        /// <param name="avatar">Avatar being processed</param>
        /// <param name="handPose">Hand pose to rename</param>
        /// <param name="newPoseName">New pose name</param>
        /// <returns>True if it was successfully renamed, false if not</returns>
        private bool RenamePose(UxrAvatar avatar, UxrHandPoseAsset handPose, string newPoseName)
        {
            if (avatar == null || handPose == null || string.IsNullOrEmpty(newPoseName))
            {
                return false;
            }

            string errorMessage = AssetDatabase.RenameAsset(GetPoseAssetPath(handPose), newPoseName);

            if (string.IsNullOrEmpty(errorMessage))
            {
                return true;
            }

            EditorUtility.DisplayDialog("Error", "Asset could not be renamed. Error: " + errorMessage, "OK");
            return false;
        }

        /// <summary>
        ///     Deletes a pose if the pose is not an inherited pose.
        /// </summary>
        /// <param name="avatar">Avatar being processed</param>
        /// <param name="poseName">Pose to delete</param>
        /// <param name="deleteAsset">Also delete the asset from disk if it exists?</param>
        private void DeletePose(UxrAvatar avatar, string poseName, bool deleteAsset)
        {
            if (avatar == null)
            {
                return;
            }

            UxrHandPoseAsset handPoseAsset = avatar.GetHandPose(poseName, false);

            if (handPoseAsset != null)
            {
                SerializedObject   serializedObject  = new SerializedObject(avatar.GetAvatarPrefab());
                SerializedProperty propertyHandPoses = serializedObject.FindProperty(UxrAvatarEditor.PropertyHandPoses);

                for (int i = 0; i < propertyHandPoses.arraySize; ++i)
                {
                    if (propertyHandPoses.GetArrayElementAtIndex(i).objectReferenceValue == handPoseAsset)
                    {
                        propertyHandPoses.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                string assetPath = GetPoseAssetPath(handPoseAsset);

                if (deleteAsset && !string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        /// <summary>
        ///     Creates a new pose asset to store a pose.
        /// </summary>
        /// <param name="avatar">Avatar the pose is for</param>
        /// <param name="reset">Reset the hands to the default position?</param>
        /// <param name="path">
        ///     If null, user will be required to specify output file path. If not null, the path value will be used
        ///     as destination file
        /// </param>
        /// <returns>New UxrHandPoseAsset object</returns>
        private UxrHandPoseAsset CreatePose(UxrAvatar avatar, bool reset = true, string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanel("Create new pose", CurrentFolder, GetUniquePoseName(avatar), "asset");
            }

            if (!string.IsNullOrEmpty(path))
            {
                if (!UxrEditorUtils.PathIsInCurrentProject(path))
                {
                    DisplayPathNotFromThisProjectError(path);
                    return null;
                }

                string poseName = Path.GetFileNameWithoutExtension(path);

                if (avatar.GetAvatarPrefab() && avatar.GetAvatarPrefab().GetHandPose(poseName, false) != null)
                {
                    // Already exists in the parent prefab

                    EditorUtility.DisplayDialog("Error", "Pose " + poseName + " already exists in the Avatar", "OK");
                    return null;
                }

                UxrAvatar parentPrefab = avatar.GetParentPrefab(poseName);

                if (parentPrefab && !EditorUtility.DisplayDialog("Use new pose?",
                                                                 $"A pose with the name {poseName} is already present in parent prefab {parentPrefab.name}. The new pose will not delete the old one, but will hide it when using this avatar prefab ({avatar.GetAvatarPrefab().name}).",
                                                                 "Yes",
                                                                 "Cancel"))
                {
                    return null;
                }

                if (reset)
                {
                    ResetHandTransforms(avatar, UxrHandSide.Left);
                    ResetHandTransforms(avatar, UxrHandSide.Right);
                }

                UxrHandPoseAsset handPose = CreateInstance<UxrHandPoseAsset>();

                handPose.Version                   = UxrHandPoseAsset.CurrentVersion;
                handPose.PoseType                  = UxrHandPoseType.Fixed;
                handPose.HandDescriptorLeft        = new UxrHandDescriptor(_avatar, UxrHandSide.Left);
                handPose.HandDescriptorRight       = new UxrHandDescriptor(_avatar, UxrHandSide.Right);
                handPose.HandDescriptorOpenLeft    = new UxrHandDescriptor(_avatar, UxrHandSide.Left);
                handPose.HandDescriptorOpenRight   = new UxrHandDescriptor(_avatar, UxrHandSide.Right);
                handPose.HandDescriptorClosedLeft  = new UxrHandDescriptor(_avatar, UxrHandSide.Left);
                handPose.HandDescriptorClosedRight = new UxrHandDescriptor(_avatar, UxrHandSide.Right);

                string file = UxrEditorUtils.GetProjectRelativePath(path);
                AssetDatabase.CreateAsset(handPose, file);

                s_currentFolder = Path.GetDirectoryName(file);

                SerializedObject serializedObject = new SerializedObject(avatar.GetAvatarPrefab());
                serializedObject.Update();
                SerializedProperty handPoses = serializedObject.FindProperty(UxrAvatarEditor.PropertyHandPoses);
                handPoses.InsertArrayElementAtIndex(0);
                handPoses.GetArrayElementAtIndex(0).objectReferenceValue = handPose;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(avatar.GetAvatarPrefab());
                AssetDatabase.SaveAssets();

                return handPose;
            }

            return null;
        }

        /// <summary>
        ///     Creates a new pose based on the currently selected pose.
        /// </summary>
        /// <param name="avatar">The avatar to create the pose for</param>
        /// <param name="handPoseSrc">
        ///     The UxrHandPoseAsset that will be used to replicate the data from.
        /// </param>
        /// <returns>
        ///     A new <see cref="UxrHandPoseAsset" /> ScriptableObject stored in disk containing all data or null if the user
        ///     cancelled
        /// </returns>
        private UxrHandPoseAsset CopyPoseAsNew(UxrAvatar avatar, UxrHandPoseAsset handPoseSrc)
        {
            UxrHandPoseAsset newHandPoseAsset = CreatePose(avatar, false);

            if (newHandPoseAsset != null)
            {
                if (handPoseSrc != null)
                {
                    SetPoseFromAsset(avatar, handPoseSrc, newHandPoseAsset);
                }

                return newHandPoseAsset;
            }

            return null;
        }

        /// <summary>
        ///     Prompts the user to select an external hand pose asset to override the current hand(s) with it.
        /// </summary>
        /// <param name="loadLeft">Overwrite current left hand?</param>
        /// <param name="loadRight">Overwrite current right hand?</param>
        private void PromptLoadExternalPose(bool loadLeft, bool loadRight)
        {
            string path = EditorUtility.OpenFilePanel("Open existing pose", CurrentFolder, "asset");

            if (!string.IsNullOrEmpty(path))
            {
                path = UxrEditorUtils.ToHandPoseAssetPath(path);

                bool load = true;

                UxrHandPoseAsset externalPose = AssetDatabase.LoadAssetAtPath<UxrHandPoseAsset>(path);

                if (externalPose == null)
                {
                    EditorUtility.DisplayDialog("Error", "Could not load asset " + path + " as pose.", "OK");
                }
                else
                {
                    if (externalPose.Version > UxrHandPoseAsset.CurrentVersion)
                    {
                        if (!EditorUtility.DisplayDialog("Warning", "File was saved using a newer version of the Hand Pose Editor. Load anyway?", "Yes", "Cancel"))
                        {
                            load = false;
                        }
                    }

                    if (externalPose.PoseType != _currentHandPose.PoseType)
                    {
                        EditorUtility.DisplayDialog("Pose type mismatch",
                                                    "Current pose type is " + _currentHandPose.PoseType + " and external pose type is " + externalPose.PoseType + ". To load the external pose please change the current pose type first.",
                                                    "OK");
                        load = false;
                    }

                    if (load)
                    {
                        RegisterHandsUndo("Load Pose " + externalPose.name);
                        SetPoseFromAsset(_avatar, externalPose, _currentHandPose, loadLeft, loadRight);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_currentHandPose));
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        /// <summary>
        ///     Changes a pose based on another pose. This enables to quickly set up new poses using others that already exist.
        ///     Since we use universal coordinates through the UxrLocalAxes struct, we can exchange data between different
        ///     coordinate systems, meaning that no matter how the transforms were authored, we will get the same results.
        /// </summary>
        /// <param name="avatar">Avatar to create the new pose for</param>
        /// <param name="handPoseSrc">The source hand pose asset</param>
        /// <param name="handPoseDst">The destination hand pose asset to copy the data to</param>
        /// <param name="loadLeft">Whether to load the left hand</param>
        /// <param name="loadRight">Whether to load the right hand</param>
        private void SetPoseFromAsset(UxrAvatar avatar, UxrHandPoseAsset handPoseSrc, UxrHandPoseAsset handPoseDst, bool loadLeft = true, bool loadRight = true)
        {
            // Save animation clips

            if (loadLeft)
            {
                UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Left, handPoseSrc.HandDescriptorLeft);
                SavePose(handPoseDst, UxrHandSide.Left, UxrHandPoseType.Fixed, UxrBlendPoseType.None, SaveHandPoseFlags.HandDescriptors);

                UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Left, handPoseSrc.HandDescriptorOpenLeft);
                SavePose(handPoseDst, UxrHandSide.Left, UxrHandPoseType.Blend, UxrBlendPoseType.OpenGrip, SaveHandPoseFlags.HandDescriptors);

                UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Left, handPoseSrc.HandDescriptorClosedLeft);
                SavePose(handPoseDst, UxrHandSide.Left, UxrHandPoseType.Blend, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);
            }

            if (loadRight)
            {
                UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Right, handPoseSrc.HandDescriptorRight);
                SavePose(handPoseDst, UxrHandSide.Right, UxrHandPoseType.Fixed, UxrBlendPoseType.None, SaveHandPoseFlags.HandDescriptors);

                UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Right, handPoseSrc.HandDescriptorOpenRight);
                SavePose(handPoseDst, UxrHandSide.Right, UxrHandPoseType.Blend, UxrBlendPoseType.OpenGrip, SaveHandPoseFlags.HandDescriptors);

                UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Right, handPoseSrc.HandDescriptorClosedRight);
                SavePose(handPoseDst, UxrHandSide.Right, UxrHandPoseType.Blend, UxrBlendPoseType.ClosedGrip, SaveHandPoseFlags.HandDescriptors);
            }

            // Parameters after, because SavePose updates them as well.

            handPoseDst.PoseType = handPoseSrc.PoseType;

            // Set initial state 

            if (handPoseSrc.PoseType == UxrHandPoseType.Fixed)
            {
                if (loadLeft)
                {
                    UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Left, handPoseSrc.HandDescriptorLeft);
                }

                if (loadRight)
                {
                    UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Right, handPoseSrc.HandDescriptorRight);
                }
            }
            else if (handPoseSrc.PoseType == UxrHandPoseType.Blend)
            {
                if (loadLeft)
                {
                    UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Left, handPoseSrc.HandDescriptorOpenLeft);
                }

                if (loadRight)
                {
                    UxrAvatarRig.UpdateHandUsingDescriptor(avatar, UxrHandSide.Right, handPoseSrc.HandDescriptorOpenRight);
                }

                _blendValue = 0.0f;
            }

            SaveHandPose(handPoseDst, SaveHandPoseFlags.All);
        }

        /// <summary>
        ///     Creates all preset poses in the given avatar.
        /// </summary>
        /// <param name="avatar">Avatar to create all the preset poses for</param>
        private void CreateAllHandPosePresets(UxrAvatar avatar)
        {
            bool tryFindDefault = GetAvatarPoseNames(avatar).Count == 0;

            string path = EditorUtility.SaveFolderPanel("Pose Assets Destination Folder", CurrentFolder, string.Empty);

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!UxrEditorUtils.PathIsInCurrentProject(path))
            {
                DisplayPathNotFromThisProjectError(path);
                return;
            }

            foreach (UxrHandPosePreset preset in _handPosePresets)
            {
                UxrHandPoseAsset newHandPoseAsset = CreatePose(avatar, false, path + "/" + preset.Pose.name + ".asset");

                if (newHandPoseAsset != null)
                {
                    SetPoseFromAsset(avatar, preset.Pose, newHandPoseAsset);

                    if (tryFindDefault && preset.Pose.name.ToLower().Contains("default"))
                    {
                        SetPoseAsDefault(avatar, newHandPoseAsset.name);
                        _currentHandPose = newHandPoseAsset;
                        UpdateHandTransforms(avatar, newHandPoseAsset, 0.0f);
                    }

                    SaveHandPose(newHandPoseAsset, SaveHandPoseFlags.HandDescriptors);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newHandPoseAsset));
                }
            }

            AssetDatabase.SaveAssets();
        }

        // Hand descriptors

        /// <summary>
        ///     Updates all hand/finger transforms to a given pose. Optionally the blend value for a blend pose
        ///     can also be specified.
        /// </summary>
        /// <param name="avatar">Avatar to process</param>
        /// <param name="handPose">Pose to update all transforms to</param>
        /// <param name="blend">If the pose is a blend pose, it tells which interpolation value to use [0, 1]</param>
        private void UpdateHandTransforms(UxrAvatar avatar, UxrHandPoseAsset handPose, float blend)
        {
            // Depending on the Unity version sometimes we need to do tricky stuff to force it to update the view with the new animation state

            bool active = avatar.gameObject.activeSelf;

            avatar.gameObject.SetActive(false);
            avatar.gameObject.SetActive(true);
            avatar.gameObject.SetActive(active);

            EditorApplication.Step();
        }

        /// <summary>
        ///     Gets the hand descriptor of a hand. The hand descriptor contains all the data needed to describe the
        ///     transforms of a given pose.
        /// </summary>
        /// <param name="handPose">Hand pose asset to get the descriptor for</param>
        /// <param name="poseType">The pose type</param>
        /// <param name="blendPoseType">If the pose is a blend pose, tells if the open or closed pose is being requested</param>
        /// <param name="handSide">Tells if the left hand or right hand is being requested</param>
        /// <returns>HandDescriptor object or null if it could not be found</returns>
        private UxrHandDescriptor GetHandDescriptor(UxrHandPoseAsset handPose, UxrHandPoseType poseType, UxrBlendPoseType blendPoseType, UxrHandSide handSide)
        {
            return handPose.GetHandDescriptor(handSide, poseType, blendPoseType);
        }

        /// <summary>
        ///     Sets the hand descriptor for a pose.
        /// </summary>
        /// <param name="handPose">Hand pose to set the descriptor for</param>
        /// <param name="poseType">The pose type to copy it to</param>
        /// <param name="blendPoseType">If the pose is a blend pose, tells whether to copy it to the open or closed pose</param>
        /// <param name="handSide">Tells whether to copy it to the left hand or right hand</param>
        /// <param name="handDescriptorSrc">The source hand descriptor</param>
        private void SetHandDescriptor(UxrHandPoseAsset handPose, UxrHandPoseType poseType, UxrBlendPoseType blendPoseType, UxrHandSide handSide, UxrHandDescriptor handDescriptorSrc)
        {
            GetHandDescriptor(handPose, poseType, blendPoseType, handSide).CopyFrom(handDescriptorSrc);
        }

        /// <summary>
        ///     Sets the current hand transforms using the current hand pose descriptors.
        /// </summary>
        /// <param name="avatar">Avatar to update</param>
        /// <param name="handSide">Hand to update</param>
        private void UpdateHandUsingCurrentDescriptor(UxrAvatar avatar, UxrHandSide handSide)
        {
            if (_currentHandPose == null)
            {
                return;
            }

            // Point to correct hand descriptor

            if (_currentHandPose.PoseType == UxrHandPoseType.Fixed)
            {
                // Update hand using fixed HandDescriptor

                UxrAvatarRig.UpdateHandUsingDescriptor(avatar, handSide, handSide == UxrHandSide.Left ? _currentHandPose.HandDescriptorLeft : _currentHandPose.HandDescriptorRight);
            }
            else if (_currentHandPose.PoseType == UxrHandPoseType.Blend)
            {
                // Update hand using blend HandDescriptors (open and closed poses), plus a blend value

                UxrAvatarRig.UpdateHandUsingDescriptor(avatar,
                                                       handSide,
                                                       handSide == UxrHandSide.Left ? _currentHandPose.HandDescriptorOpenLeft : _currentHandPose.HandDescriptorOpenRight,
                                                       handSide == UxrHandSide.Left ? _currentHandPose.HandDescriptorClosedLeft : _currentHandPose.HandDescriptorClosedRight,
                                                       _blendValue);
            }
        }

        // Presets

        /// <summary>
        ///     Refreshes the hand pose presets, looking for new entries.
        /// </summary>
        private void RefreshHandPosePresets()
        {
            try
            {
                _handPosePresets = new List<UxrHandPosePreset>();

                string[] files = UxrEditorUtils.GetHandPosePresetFiles();

                foreach (string file in files)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(file) == typeof(UxrHandPoseAsset))
                    {
                        _handPosePresets.Add(new UxrHandPosePreset(file, files));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error loading hand pose presets: " + e.Message);
            }
        }

        // Finger spinners

        /// <summary>
        ///     Initializes the finger UI spinner components to handle the rotation of finger transforms.
        /// </summary>
        private void BuildFingerSpinners()
        {
            UxrUniversalLocalAxes leftHandAxes    = _avatar.AvatarRigInfo.GetArmInfo(UxrHandSide.Left).HandUniversalLocalAxes;
            UxrUniversalLocalAxes rightHandAxes   = _avatar.AvatarRigInfo.GetArmInfo(UxrHandSide.Right).HandUniversalLocalAxes;
            UxrUniversalLocalAxes leftFingerAxes  = _avatar.AvatarRigInfo.GetArmInfo(UxrHandSide.Left).FingerUniversalLocalAxes;
            UxrUniversalLocalAxes rightFingerAxes = _avatar.AvatarRigInfo.GetArmInfo(UxrHandSide.Right).FingerUniversalLocalAxes;
            UxrAvatarHand         leftHand        = _avatar.LeftHand;
            UxrAvatarHand         rightHand       = _avatar.RightHand;
            Transform             leftHandBone    = _avatar.LeftHandBone;
            Transform             rightHandBone   = _avatar.RightHandBone;
            UxrFingerAngleType    typeSpread      = UxrFingerAngleType.Spread;
            UxrFingerAngleType    typeCurl        = UxrFingerAngleType.Curl;

            _fingerSpinners = new List<UxrFingerSpinner>();

            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _leftMouseTex, new Color32(128, 255, 255, 255), leftHand.Thumb.Proximal,      leftHandBone,                 leftFingerAxes, leftHandAxes,   -50, 5,   UxrHandSide.Left, true));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _leftMouseTex, new Color32(128, 0,   255, 255), leftHand.Index.Proximal,      leftHandBone,                 leftFingerAxes, leftHandAxes,   -45, 45,  UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _leftMouseTex, new Color32(128, 0,   0,   255), leftHand.Middle.Proximal,     leftHandBone,                 leftFingerAxes, leftHandAxes,   -45, 45,  UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _leftMouseTex, new Color32(0,   0,   0,   255), leftHand.Ring.Proximal,       leftHandBone,                 leftFingerAxes, leftHandAxes,   -45, 45,  UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _leftMouseTex, new Color32(128, 0,   128, 255), leftHand.Little.Proximal,     leftHandBone,                 leftFingerAxes, leftHandAxes,   -45, 45,  UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(0,   0,   128, 255), leftHand.Thumb.Proximal,      leftHandBone,                 leftFingerAxes, leftHandAxes,   -10, 40,  UxrHandSide.Left, true));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(128, 128, 128, 255), leftHand.Index.Proximal,      leftHandBone,                 leftFingerAxes, leftHandAxes,   -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(128, 255, 128, 255), leftHand.Middle.Proximal,     leftHandBone,                 leftFingerAxes, leftHandAxes,   -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(255, 128, 255, 255), leftHand.Ring.Proximal,       leftHandBone,                 leftFingerAxes, leftHandAxes,   -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(255, 0,   0,   255), leftHand.Little.Proximal,     leftHandBone,                 leftFingerAxes, leftHandAxes,   -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(128, 128, 0,   255), leftHand.Thumb.Intermediate,  leftHand.Thumb.Proximal,      leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(128, 128, 255, 255), leftHand.Index.Intermediate,  leftHand.Index.Proximal,      leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(128, 255, 0,   255), leftHand.Middle.Intermediate, leftHand.Middle.Proximal,     leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(255, 128, 128, 255), leftHand.Ring.Intermediate,   leftHand.Ring.Proximal,       leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(255, 255, 0,   255), leftHand.Little.Intermediate, leftHand.Little.Proximal,     leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(0,   128, 0,   255), leftHand.Thumb.Distal,        leftHand.Thumb.Intermediate,  leftFingerAxes, leftFingerAxes, -10, 80,  UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(0,   128, 255, 255), leftHand.Index.Distal,        leftHand.Index.Intermediate,  leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(0,   255, 0,   255), leftHand.Middle.Distal,       leftHand.Middle.Intermediate, leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(0,   0,   255, 255), leftHand.Ring.Distal,         leftHand.Ring.Intermediate,   leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _leftMouseTex, new Color32(255, 255, 255, 255), leftHand.Little.Distal,       leftHand.Little.Intermediate, leftFingerAxes, leftFingerAxes, -30, 120, UxrHandSide.Left));

            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _rightMouseTex, new Color32(128, 255, 255, 255), rightHand.Thumb.Proximal,      rightHandBone,                 rightFingerAxes, rightHandAxes,   -5,  50,  UxrHandSide.Right, true));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _rightMouseTex, new Color32(128, 0,   255, 255), rightHand.Index.Proximal,      rightHandBone,                 rightFingerAxes, rightHandAxes,   -45, 45,  UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _rightMouseTex, new Color32(128, 0,   0,   255), rightHand.Middle.Proximal,     rightHandBone,                 rightFingerAxes, rightHandAxes,   -45, 45,  UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _rightMouseTex, new Color32(0,   0,   0,   255), rightHand.Ring.Proximal,       rightHandBone,                 rightFingerAxes, rightHandAxes,   -45, 45,  UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeSpread, _rightMouseTex, new Color32(128, 0,   128, 255), rightHand.Little.Proximal,     rightHandBone,                 rightFingerAxes, rightHandAxes,   -45, 45,  UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(0,   0,   128, 255), rightHand.Thumb.Proximal,      rightHandBone,                 rightFingerAxes, rightHandAxes,   -10, 40,  UxrHandSide.Right, true));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(128, 128, 128, 255), rightHand.Index.Proximal,      rightHandBone,                 rightFingerAxes, rightHandAxes,   -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(128, 255, 128, 255), rightHand.Middle.Proximal,     rightHandBone,                 rightFingerAxes, rightHandAxes,   -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(255, 128, 255, 255), rightHand.Ring.Proximal,       rightHandBone,                 rightFingerAxes, rightHandAxes,   -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(255, 0,   0,   255), rightHand.Little.Proximal,     rightHandBone,                 rightFingerAxes, rightHandAxes,   -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(128, 128, 0,   255), rightHand.Thumb.Intermediate,  rightHand.Thumb.Proximal,      rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(128, 128, 255, 255), rightHand.Index.Intermediate,  rightHand.Index.Proximal,      rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(128, 255, 0,   255), rightHand.Middle.Intermediate, rightHand.Middle.Proximal,     rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(255, 128, 128, 255), rightHand.Ring.Intermediate,   rightHand.Ring.Proximal,       rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(255, 255, 0,   255), rightHand.Little.Intermediate, rightHand.Little.Proximal,     rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(0,   128, 0,   255), rightHand.Thumb.Distal,        rightHand.Thumb.Intermediate,  rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(0,   128, 255, 255), rightHand.Index.Distal,        rightHand.Index.Intermediate,  rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(0,   255, 0,   255), rightHand.Middle.Distal,       rightHand.Middle.Intermediate, rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(0,   0,   255, 255), rightHand.Ring.Distal,         rightHand.Ring.Intermediate,   rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));
            _fingerSpinners.Add(new UxrFingerSpinner(typeCurl,   _rightMouseTex, new Color32(255, 255, 255, 255), rightHand.Little.Distal,       rightHand.Little.Intermediate, rightFingerAxes, rightFingerAxes, -30, 120, UxrHandSide.Right));

            _selectedFingerSpinner = null;
        }

        /// <summary>
        ///     Handles all finger rotation operations.
        /// </summary>
        /// <param name="leftHandTexturePos">
        ///     Top-left position of the left hand texture in window coordinates, so that we can transform mouse coordinates
        ///     to coordinates relative to the hand image
        /// </param>
        /// <param name="rightHandTexturePos">
        ///     Top-left position of the right hand texture in window coordinates, so that we can transform mouse coordinates
        ///     to coordinates relative to the hand image
        /// </param>
        /// <param name="selectGameObjectOnClick">
        ///     Does the Unity Editor select the finger GameObject in the hierarchy window when the user
        ///     modifies a finger node?
        /// </param>
        /// <param name="poseType">Pose type currently being edited</param>
        /// <param name="blendPoseType">Blend pose type currently being edited</param>
        private void HandleFingerSpinners(Vector2 leftHandTexturePos, Vector2 rightHandTexturePos, bool selectGameObjectOnClick, UxrHandPoseType poseType, UxrBlendPoseType blendPoseType)
        {
            // Events

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                foreach (UxrFingerSpinner spinner in _fingerSpinners)
                {
                    Vector2 mouseRelativePos = Event.current.mousePosition - (spinner.HandSide == UxrHandSide.Left ? leftHandTexturePos : rightHandTexturePos);
                    if (spinner.ContainsMousePos(mouseRelativePos))
                    {
                        _selectedFingerSpinner           = spinner;
                        _selectedFingerSpinnerStartValue = spinner.GetValueFromObject();
                        _selectedFingerSpinnerMouseStart = mouseRelativePos;

                        if (selectGameObjectOnClick)
                        {
                            Selection.objects = new[] { spinner.Target.gameObject };
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                if (_currentHandPose != null && _currentHandPose.PoseType == UxrHandPoseType.Blend)
                {
                    _blendValue           = blendPoseType == UxrBlendPoseType.OpenGrip ? 0.0f : 1.0f;
                    _updateHandTransforms = _autoSave; // = true
                }

                if (_selectedFingerSpinner != null)
                {
                    _selectedFingerSpinner = null;

                    if (_autoSave && _avatar != null && _currentHandPose != null)
                    {
                        SavePose(_currentHandPose, UxrHandSide.Right, poseType, blendPoseType, SaveHandPoseFlags.HandDescriptors);
                        SavePose(_currentHandPose, UxrHandSide.Left,  poseType, blendPoseType, SaveHandPoseFlags.HandDescriptors);
                    }
                }
            }
            else if (Event.current.type == EventType.MouseDrag && _selectedFingerSpinner != null)
            {
                if (_selectedFingerSpinner != null)
                {
                    Vector2 mouseRelativePos = Event.current.mousePosition - (_selectedFingerSpinner.HandSide == UxrHandSide.Left ? leftHandTexturePos : rightHandTexturePos);

                    Undo.RecordObject(_selectedFingerSpinner.Target, _selectedFingerSpinner.Target.name + " Rotation");
                    Vector2 mouseOffset = mouseRelativePos - _selectedFingerSpinnerMouseStart;
                    float   offset      = _selectedFingerSpinner.Angle == UxrFingerAngleType.Spread ? mouseOffset.x : -mouseOffset.y;
                    _selectedFingerSpinner.Value  = _selectedFingerSpinnerStartValue + offset * _mouseRotationSpeed;
                    _selectedFingerSpinner.Offset = _selectedFingerSpinnerStartValue + offset * _mouseRotationSpeed;
                    Repaint();

                    // This part below is to have the grabbable object preview meshes in sync while dragging too.
                    // The key part is we save only the HandDescriptors to make it more responsive, since saving the animation clips
                    // doesn't seem to be very fast. The full hand rig is saved on the MouseUp event so everything keeps in sync.

                    if (_autoSave && _avatar != null && _currentHandPose != null)
                    {
                        SavePose(_currentHandPose, UxrHandSide.Right, poseType, blendPoseType, SaveHandPoseFlags.HandDescriptors);
                        SavePose(_currentHandPose, UxrHandSide.Left,  poseType, blendPoseType, SaveHandPoseFlags.HandDescriptors);
                    }

                    if (_avatar != null && _currentHandPose != null)
                    {
                        UxrGrabbableObjectEditor.RefreshGrabPoseMeshes(_avatar, _currentHandPose);
                        UxrGrabbableObjectSnapTransformEditor.RefreshGrabPoseMeshes(_avatar, _currentHandPose);
                    }
                }
            }

            // Draw spinners

            foreach (UxrFingerSpinner spinner in _fingerSpinners)
            {
                Vector2 texturePos        = spinner.HandSide == UxrHandSide.Left ? leftHandTexturePos : rightHandTexturePos;
                float   spinnerWidth      = spinner.Angle == UxrFingerAngleType.Spread ? _horizontalSpinnerTex.width : _verticalSpinnerTex.width;
                float   spinnerHeight     = spinner.Angle == UxrFingerAngleType.Spread ? _horizontalSpinnerTex.height : _verticalSpinnerTex.height;
                int     halfSpinnerWidth  = (int)(spinnerWidth * 0.5f);
                int     halfSpinnerHeight = (int)(spinnerHeight * 0.5f);

                Rect spinnerRect = new Rect(spinner.MouseRect.center.x - halfSpinnerWidth + texturePos.x,                       spinner.MouseRect.center.y - halfSpinnerHeight + texturePos.y,                    spinnerWidth, spinnerHeight);
                Rect texCoords   = new Rect(spinner.Angle == UxrFingerAngleType.Spread ? spinner.Offset * -SpinnerSpeed : 0.0f, spinner.Angle == UxrFingerAngleType.Curl ? spinner.Offset * -SpinnerSpeed : 0.0f, 1.0f,         1.0f);

                // Spinner

                GUI.DrawTextureWithTexCoords(spinnerRect, spinner.Angle == UxrFingerAngleType.Curl ? _verticalSpinnerTex : _horizontalSpinnerTex, texCoords);

                // Arrows

                int arrowMargin = 1;

                if (spinner.Angle == UxrFingerAngleType.Spread)
                {
                    Rect leftArrowRect = new Rect(spinner.MouseRect.center.x - halfSpinnerWidth - _spinnerArrowLeftTex.width - arrowMargin + texturePos.x,
                                                  spinner.MouseRect.center.y - (int)(_spinnerArrowLeftTex.height / 2.0f) + texturePos.y,
                                                  _spinnerArrowLeftTex.width,
                                                  _spinnerArrowLeftTex.height);
                    Rect rightArrowRect = new Rect(spinner.MouseRect.center.x + halfSpinnerWidth + arrowMargin + texturePos.x,
                                                   spinner.MouseRect.center.y - (int)(_spinnerArrowRightTex.height / 2.0f) + texturePos.y,
                                                   _spinnerArrowRightTex.width,
                                                   _spinnerArrowRightTex.height);

                    GUI.DrawTexture(leftArrowRect,  _spinnerArrowLeftTex);
                    GUI.DrawTexture(rightArrowRect, _spinnerArrowRightTex);
                }
                else
                {
                    Rect upArrowRect = new Rect(spinner.MouseRect.center.x - (int)(_spinnerArrowUpTex.width / 2.0f) + texturePos.x,
                                                spinner.MouseRect.center.y - halfSpinnerHeight - _spinnerArrowUpTex.height - arrowMargin + texturePos.y,
                                                _spinnerArrowUpTex.width,
                                                _spinnerArrowUpTex.height);
                    Rect downArrowRect = new Rect(spinner.MouseRect.center.x - (int)(_spinnerArrowDownTex.width / 2.0f) + texturePos.x,
                                                  spinner.MouseRect.center.y + halfSpinnerHeight + arrowMargin + texturePos.y,
                                                  _spinnerArrowDownTex.width,
                                                  _spinnerArrowDownTex.height);

                    GUI.DrawTexture(upArrowRect,   _spinnerArrowUpTex);
                    GUI.DrawTexture(downArrowRect, _spinnerArrowDownTex);
                }
            }

            // Mouse cursor rects

            if (_selectedFingerSpinner == null)
            {
                foreach (UxrFingerSpinner spinner in _fingerSpinners)
                {
                    Vector2 texturePos = spinner.HandSide == UxrHandSide.Left ? leftHandTexturePos : rightHandTexturePos;
                    Rect    mouseRect  = new Rect(spinner.MouseRect.x + texturePos.x, spinner.MouseRect.y + texturePos.y, spinner.MouseRect.width, spinner.MouseRect.height);

                    EditorGUIUtility.AddCursorRect(mouseRect, spinner.Angle == UxrFingerAngleType.Curl ? MouseCursor.ResizeVertical : MouseCursor.ResizeHorizontal);
                }
            }
            else
            {
                EditorGUIUtility.AddCursorRect(new Rect(int.MinValue / 4, int.MinValue / 4, int.MaxValue, int.MaxValue), _selectedFingerSpinner.Angle == UxrFingerAngleType.Curl ? MouseCursor.ResizeVertical : MouseCursor.ResizeHorizontal);
            }
        }

        /// <summary>
        ///     Resets all the finger spinners or those of a specific finger.
        /// </summary>
        /// <param name="finger">Finger to reset the spinners of or null to reset all spinners</param>
        private void ResetFingerSpinners(UxrAvatarFinger finger = null)
        {
            if (_fingerSpinners == null)
            {
                return;
            }

            foreach (UxrFingerSpinner spinner in _fingerSpinners)
            {
                if (finger == null || finger.Metacarpal == spinner.Target || finger.Proximal == spinner.Target || finger.Intermediate == spinner.Target || finger.Distal == spinner.Target)
                {
                    spinner.Offset = 0.0f;
                }
            }
        }

        // Undo

        /// <summary>
        ///     Registers an undo operation for the whole hand bones.
        /// </summary>
        /// <param name="undoName">Name to show in the editor undo section</param>
        private void RegisterHandsUndo(string undoName)
        {
            RegisterFingerUndo(undoName, _avatar.LeftHand.Thumb);
            RegisterFingerUndo(undoName, _avatar.LeftHand.Index);
            RegisterFingerUndo(undoName, _avatar.LeftHand.Middle);
            RegisterFingerUndo(undoName, _avatar.LeftHand.Ring);
            RegisterFingerUndo(undoName, _avatar.LeftHand.Little);

            RegisterFingerUndo(undoName, _avatar.RightHand.Thumb);
            RegisterFingerUndo(undoName, _avatar.RightHand.Index);
            RegisterFingerUndo(undoName, _avatar.RightHand.Middle);
            RegisterFingerUndo(undoName, _avatar.RightHand.Ring);
            RegisterFingerUndo(undoName, _avatar.RightHand.Little);
        }

        /// <summary>
        ///     Registers an undo operation for a finger.
        /// </summary>
        /// <param name="undoName">Name to show in the editor undo section</param>
        /// <param name="finger">Finger to process</param>
        private void RegisterFingerUndo(string undoName, UxrAvatarFinger finger)
        {
            if (finger.Metacarpal)
            {
                Undo.RecordObject(finger.Metacarpal, undoName);
            }
            if (finger.Proximal)
            {
                Undo.RecordObject(finger.Proximal, undoName);
            }
            if (finger.Intermediate)
            {
                Undo.RecordObject(finger.Intermediate, undoName);
            }
            if (finger.Distal)
            {
                Undo.RecordObject(finger.Distal, undoName);
            }
        }

        // Misc

        /// <summary>
        ///     Gets a list of formatted pose names for the dropdown list.
        /// </summary>
        /// <param name="poseNames">Input pose names</param>
        /// <param name="defaultPoseName">Name of the default pose name</param>
        /// <returns>Formatted pose names for the dropdown list</returns>
        private List<string> GetFormattedAvatarPoseNames(IEnumerable<string> poseNames, string defaultPoseName)
        {
            if (poseNames == null)
            {
                return new List<string>();
            }

            List<string> formattedPoseNames = new List<string>(poseNames.Count());

            foreach (string poseName in poseNames)
            {
                formattedPoseNames.Add(poseName == defaultPoseName ? $"{poseName} (Default pose)" : poseName);
            }

            return formattedPoseNames;
        }

        /// <summary>
        ///     Checks if a given value is either 0.0 or 1.0
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>Boolean telling whether the given value is either 0.0 or 1.0</returns>
        private bool IsOneOrZero(float value)
        {
            return Mathf.Approximately(value, 0.0f) || Mathf.Approximately(value, 1.0f);
        }

        /// <summary>
        ///     Saves all current pose unsaved changes.
        /// </summary>
        /// <param name="saveFlags">Flags with the elements to save</param>
        private void AutoSave(SaveHandPoseFlags saveFlags)
        {
            SaveHandPose(_currentHandPose, saveFlags);
        }

        /// <summary>
        ///     Gets a unique new pose name.
        /// </summary>
        /// <param name="avatar">Avatar being processed</param>
        /// <returns>Unique pose name</returns>
        private string GetUniquePoseName(UxrAvatar avatar)
        {
            for (int i = 1; i < 100; ++i)
            {
                if (GetPoseType(NewPoseName + i, avatar) == UxrHandPoseType.None)
                {
                    return NewPoseName + i;
                }
            }

            return NewPoseName + (int)(Random.value * 65535);
        }

        /// <summary>
        ///     Tries to place a snap transform GameObject on the selected grabbable object if there is any.
        ///     A snap transform is the transform that will tell how the object will be snapped to the hand. The snap
        ///     transform will be aligned to the <see cref="UxrGrabber" /> transform while the object is being grabbed.
        /// </summary>
        /// <param name="avatar">The avatar being processed</param>
        /// <param name="poseName">The name of the selected pose</param>
        /// <param name="handSide">Which hand to create the snap transform for</param>
        private void TryPlaceSnapTransform(UxrAvatar avatar, string poseName, UxrHandSide handSide)
        {
            UxrGrabbableObject grabbableObjectSelected = Selection.objects != null && Selection.objects.Length == 1 && Selection.objects[0].GetType() == typeof(GameObject)
                                                                     ? ((GameObject)Selection.objects[0]).GetComponent<UxrGrabbableObject>()
                                                                     : null;

            if (grabbableObjectSelected == null)
            {
                EditorUtility.DisplayDialog($"No {nameof(UxrGrabbableObject)} selected",
                                            $"A GameObject with an {nameof(UxrGrabbableObject)} component needs to be selected in the Hierarchy Window",
                                            "OK");
            }
            else if (EditorUtility.IsPersistent(grabbableObjectSelected.gameObject))
            {
                EditorUtility.DisplayDialog("Object is not in scene", "The selected grabbable object needs to be in the scene, it cannot be a prefab", "OK");
            }
            else
            {
                UxrGrabber[] grabbers = _avatar.GetComponentsInChildren<UxrGrabber>();

                for (int i = 0; i < grabbers.Length; ++i)
                {
                    if (grabbers[i].Side == UxrHandSide.Left)
                    {
                        if (EditorUtility.DisplayDialog("Create snap transform?", "Create snap transform on " + grabbableObjectSelected.name + " for pose " + poseName + "?", "Yes", "Cancel"))
                        {
                            GameObject                      snapObject              = new GameObject(poseName + (handSide == UxrHandSide.Left ? "Left" : "Right"));
                            UxrGrabbableObjectSnapTransform alignTransformComponent = snapObject.AddComponent<UxrGrabbableObjectSnapTransform>();
                            snapObject.transform.SetPositionAndRotation(grabbers[i].transform.position, grabbers[i].transform.rotation);
                            snapObject.transform.SetParent(grabbableObjectSelected.transform, true);

                            Undo.RegisterCreatedObjectUndo(snapObject, "Create " + snapObject.name);
                        }

                        return;
                    }
                }

                EditorUtility.DisplayDialog($"{nameof(UxrGrabber)} not found",
                                            $"The avatar needs to have a GameObject with an {nameof(UxrGrabber)} component for this hand. This will tell where the object will be snapped to the hand when it is being grabbed.",
                                            "OK");
            }
        }

        /// <summary>
        ///     Gets the <see cref="Rect" /> for the given poses menu button.
        /// </summary>
        /// <param name="column">Column</param>
        /// <param name="y">Y position</param>
        /// <returns>Rect for the button</returns>
        private Rect GetPosesMenuButtonRect(int column, int y)
        {
            float totalWidth  = HandMarginHorizontal * 2 + _leftHandTex.width + _rightHandTex.width;
            float columnWidth = totalWidth / PosesMenuTotalColumns;
            float posMiddle   = columnWidth * column + columnWidth * 0.5f;
            float posButton   = posMiddle - ButtonWidth * 0.5f;

            return new Rect(posButton, y, ButtonWidth, EditorGUIUtility.singleLineHeight);
        }

        /// <summary>
        ///     Gets the <see cref="Rect" /> for the given left hand action button.
        /// </summary>
        /// <param name="y">Y position</param>
        /// <returns>Rect for the button</returns>
        private Rect GetLeftHandButtonRect(int y)
        {
            float leftHandButtonPos = HandMarginHorizontal + _leftHandTex.width / 2 - ButtonWidth / 2;
            return new Rect(leftHandButtonPos, y, ButtonWidth, EditorGUIUtility.singleLineHeight);
        }

        /// <summary>
        ///     Gets the <see cref="Rect" /> for the given right hand action button.
        /// </summary>
        /// <param name="y">Y position</param>
        /// <returns>Rect for the button</returns>
        private Rect GetRightHandButtonRect(int y)
        {
            float rightHandButtonPos = HandMarginHorizontal * 2 + _leftHandTex.width * 1.5f - ButtonWidth / 2;
            return new Rect(rightHandButtonPos, y, ButtonWidth, EditorGUIUtility.singleLineHeight);
        }

        #endregion

        #region Private Types & Data

        // Internal properties

        /// <summary>
        ///     Gets the folder to show then opening file/folder dialogs.
        /// </summary>
        private string CurrentFolder
        {
            get
            {
                if (string.IsNullOrEmpty(s_currentFolder))
                {
                    return string.Empty;
                }

                string folder = s_currentFolder + "/";
                folder = folder.Substring("Assets/".Length);

                return folder;
            }
        }

        /// <summary>
        ///     Gets the number of columns in the poses menu.
        /// </summary>
        private int PosesMenuTotalColumns => _avatar != null && _avatar.IsPrefabVariant ? 4 : 3;

        /// <summary>
        ///     Gets the height in pixels of a text line in the editor.
        /// </summary>
        private int VerticalHeightSingleLine => (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

        /// <summary>
        ///     Gets the current pose type based on the current selected pose type.
        /// </summary>
        private UxrHandPoseType CurrentPoseType => _poseTypeIndex != -1 ? (UxrHandPoseType)Enum.Parse(typeof(UxrHandPoseType), GetPoseTypes()[_poseTypeIndex]) : UxrHandPoseType.None;

        /// <summary>
        ///     Gets the current blend pose type based on the current blend value.
        /// </summary>
        private UxrBlendPoseType CurrentBlendPoseType => _blendValue < BlendPoseOpenThreshold ? UxrBlendPoseType.OpenGrip : UxrBlendPoseType.ClosedGrip;

        // Constants

        private const int FieldWidth           = 400;
        private const int ButtonWidth          = 200;
        private const int HandMarginHorizontal = 0;
        private const int HandMarginVertical   = 50;

        private const int PosesMenuColumnCreate        = 0;
        private const int PosesMenuColumnDelete        = 1;
        private const int PosesMenuColumnMisc          = 2;
        private const int PosesMenuColumnPrefabVariant = 3;

        private const float SpinnerSpeed = 0.01f;

        private const string NewPoseName                          = "HandPose";
        private const float  BlendPoseOpenThreshold               = 0.5f;
        private const string LeftHandTextureRelativePath          = "/Textures/LeftHand.png";
        private const string RightHandTextureRelativePath         = "/Textures/RightHand.png";
        private const string LeftHandMouseTextureRelativePath     = "/Textures/LeftHandMouse.png";
        private const string RightHandMouseTextureRelativePath    = "/Textures/RightHandMouse.png";
        private const string HorizontalSpinnerTextureRelativePath = "/Textures/SpinnerHorizontal.png";
        private const string VerticalSpinnerTextureRelativePath   = "/Textures/SpinnerVertical.png";
        private const string SpinnerArrowLeftTextureRelativePath  = "/Textures/SpinnerArrowLeft.png";
        private const string SpinnerArrowRightTextureRelativePath = "/Textures/SpinnerArrowRight.png";
        private const string SpinnerArrowUpTextureRelativePath    = "/Textures/SpinnerArrowUp.png";
        private const string SpinnerArrowDownTextureRelativePath  = "/Textures/SpinnerArrowDown.png";
        private const string PrefAutoSave                         = "VRMADA.UltimateXR.Manipulation.Editor.HandPoseEditor.UxrHandPoseEditorWindow.AutoSave";
        private const string PrefSelectGameObjectOnClick          = "VRMADA.UltimateXR.Manipulation.Editor.HandPoseEditor.UxrHandPoseEditorWindow.SelectGameObjectOnClick";
        private const string PrefMouseRotationSpeed               = "VRMADA.UltimateXR.Manipulation.Editor.HandPoseEditor.UxrHandPoseEditorWindow.MouseRotationSpeed";

        private static UxrHandPoseEditorWindow s_handPoseEditorWindow;
        private static int                     s_openWindowCount;
        private static string                  s_currentFolder;

        private IReadOnlyList<string> _currentPoseNames;

        #endregion
    }
}