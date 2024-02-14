// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddAvatarToSceneMenu.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Avatar
{
    /// <summary>
    ///     Utility class with menu items to add built-in UltimateXR avatars to the scene.
    /// </summary>
    public static class AddAvatarToSceneMenu
    {
        #region Public Methods

        [MenuItem(UxrConstants.Editor.MenuPathAddAvatar + "Small Hands Avatar (BRP)", priority = UxrConstants.Editor.PriorityMenuPathAvatar)]
        public static void AddSmallHandsAvatarBrp()
        {
            AddAvatar(SmallHandsAvatarBrpAssetGuid);
        }

        [MenuItem(UxrConstants.Editor.MenuPathAddAvatar + "Small Hands Avatar (URP)", priority = UxrConstants.Editor.PriorityMenuPathAvatar + 1)]
        public static void AddSmallHandsAvatarUrp()
        {
            AddAvatar(SmallHandsAvatarUrpAssetGuid);
        }

        [MenuItem(UxrConstants.Editor.MenuPathAddAvatar + "Big Hands Avatar (BRP)", priority = UxrConstants.Editor.PriorityMenuPathAvatar + 2)]
        public static void AddBigHandsAvatarBrp()
        {
            AddAvatar(BigHandsAvatarBrpAssetGuid);
        }

        [MenuItem(UxrConstants.Editor.MenuPathAddAvatar + "Big Hands Avatar (URP)", priority = UxrConstants.Editor.PriorityMenuPathAvatar + 3)]
        public static void AddBigHandsAvatarUrp()
        {
            AddAvatar(BigHandsAvatarUrpAssetGuid);
        }

        [MenuItem(UxrConstants.Editor.MenuPathAddAvatar + "Cyborg Avatar (BRP)", priority = UxrConstants.Editor.PriorityMenuPathAvatar + 4)]
        public static void AddCyborgAvatarBrp()
        {
            AddAvatar(CyborgAvatarBrpAssetGuid);
        }

        [MenuItem(UxrConstants.Editor.MenuPathAddAvatar + "Cyborg Avatar (URP)", priority = UxrConstants.Editor.PriorityMenuPathAvatar + 5)]
        public static void AddCyborgAvatarUrp()
        {
            AddAvatar(CyborgAvatarUrpAssetGuid);
        }

        #endregion

        #region Private Types & Data

        private static void AddAvatar(string assetGuid)
        {
            GameObject avatarAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assetGuid));

            if (avatarAsset != null)
            {
                GameObject newInstance = PrefabUtility.InstantiatePrefab(avatarAsset) as GameObject;
                newInstance.transform.position = Vector3.zero;
                newInstance.transform.rotation = Quaternion.identity;
                newInstance.name               = avatarAsset.name;
                Selection.activeGameObject     = newInstance;
            }
        }

        private const string SmallHandsAvatarBrpAssetGuid = "c311edb378c22084c9abc1d944113176";
        private const string SmallHandsAvatarUrpAssetGuid = "7195d091dac462e4db9ba0793d9a6727";
        private const string BigHandsAvatarBrpAssetGuid   = "e7c39828446955c4eb062c0ef1f5eb71";
        private const string BigHandsAvatarUrpAssetGuid   = "dc011efb41f2d4a419389aefb4b4270d";
        private const string CyborgAvatarBrpAssetGuid     = "d12ff67997e4a1547890dc7efeb1b11c";
        private const string CyborgAvatarUrpAssetGuid     = "e7468ec6b7af89c4d94d451ec3c1807b";

        #endregion
    }
}