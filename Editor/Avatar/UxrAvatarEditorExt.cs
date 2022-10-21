// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAvatarEditorExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using UltimateXR.Avatar;
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor.Avatar
{
    /// <summary>
    ///     <see cref="UxrAvatar" /> extensions available only for editor scripts.
    /// </summary>
    public static class UxrAvatarEditorExt
    {
        #region Public Methods

        /// <summary>
        ///     Gets an <see cref="UxrAvatar" />'s prefab, based on the Guid stored.
        /// </summary>
        /// <param name="self">Avatar to get the source prefab of</param>
        /// <returns>Prefab or null if it could not be found</returns>
        public static GameObject GetPrefab(this UxrAvatar self)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(self.PrefabGuid));
        }

        /// <summary>
        ///     Gets an <see cref="UxrAvatar" /> prefab's <see cref="UxrAvatar" /> component, based on the Guid stored.
        /// </summary>
        /// <param name="self">Avatar to get the source prefab of</param>
        /// <returns>Prefab or null if it could not be found</returns>
        public static UxrAvatar GetAvatarPrefab(this UxrAvatar self)
        {
            return GetFromGuid(self.PrefabGuid);
        }

        /// <summary>
        ///     Gets a prefab's GUID.
        /// </summary>
        /// <param name="gameObject">Prefab to get the GUID of</param>
        /// <returns>GUID or empty string if it could not be found</returns>
        public static string GetGuid(GameObject gameObject)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(gameObject));
        }

        /// <summary>
        ///     Gets an <see cref="UxrAvatar" /> prefab, based on an asset GUID.
        /// </summary>
        /// <param name="avatarPrefabGuid">Asset GUID</param>
        /// <returns>Prefab or null if it could not be found</returns>
        public static UxrAvatar GetFromGuid(string avatarPrefabGuid)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(avatarPrefabGuid));
            return prefab != null ? prefab.GetComponent<UxrAvatar>() : null;
        }

        /// <summary>
        ///     Gets the avatar prefab chain. This is the source prefab followed by all parent prefabs up to the root parent
        ///     prefab.
        /// </summary>
        /// <returns>
        ///     Upwards prefab chain. If the avatar is a prefab itself, it will be the first item in the list.
        /// </returns>
        public static IEnumerable<UxrAvatar> GetPrefabChain(this UxrAvatar self)
        {
            UxrAvatar avatarPrefab = self.GetAvatarPrefab();

            if (avatarPrefab != null)
            {
                yield return avatarPrefab;
            }

            UxrAvatar current = self.ParentAvatarPrefab;

            while (current != null)
            {
                yield return current;

                current = current.ParentAvatarPrefab;
            }
        }

        /// <summary>
        ///     Checks whether the avatar is or comes from the given prefab.
        /// </summary>
        /// <param name="self">Avatar</param>
        /// <param name="prefab">Prefab</param>
        /// <returns>Whether the avatar is or comes from the given prefab</returns>
        public static bool IsInPrefabChain(this UxrAvatar self, GameObject prefab)
        {
            foreach (UxrAvatar avatarPrefab in self.GetPrefabChain())
            {
                if (avatarPrefab != null && avatarPrefab.gameObject == prefab)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}