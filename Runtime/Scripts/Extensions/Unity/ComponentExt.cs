// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateXR.Extensions.Unity
{
    /// <summary>
    ///     <see cref="Component" /> extensions.
    /// </summary>
    public static class ComponentExt
    {
        #region Public Methods

        /// <summary>
        ///     Controls whether to show a given component in the inspector.
        /// </summary>
        /// <param name="self">The component to show</param>
        /// <param name="show">Whether to show the component or now</param>
        public static void ShowInInspector(this Component self, bool show = true)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                SerializedObject   so   = new SerializedObject(self);
                SerializedProperty prop = so.FindProperty(UxrConstants.Editor.PropertyObjectHideFlags);

                if (show)
                {
                    prop.intValue &= (int)~HideFlags.HideInInspector;
                }
                else
                {
                    prop.intValue |= (int)HideFlags.HideInInspector;
                }

                so.ApplyModifiedProperties();
                return;
            }
#endif
            if (show)
            {
                self.hideFlags &= ~HideFlags.HideInInspector;
            }
            else
            {
                self.hideFlags |= HideFlags.HideInInspector;
            }
        }

        /// <summary>
        ///     Controls whether to show a given component in the inspector and whether it is editable.
        /// </summary>
        /// <param name="self">The object to set</param>
        /// <param name="show">Whether to show it in the inspector</param>
        /// <param name="editable">Whether it is editable</param>
        public static void ShowInInspector(this Component self, bool show, bool editable)
        {
#if UNITY_EDITOR
            SerializedObject   so   = new SerializedObject(self);
            SerializedProperty prop = so.FindProperty(UxrConstants.Editor.PropertyObjectHideFlags);

            if (show)
            {
                prop.intValue &= (int)~HideFlags.HideInInspector;
            }
            else
            {
                prop.intValue |= (int)HideFlags.HideInInspector;
            }

            if (editable)
            {
                prop.intValue &= (int)~HideFlags.NotEditable;
            }
            else
            {
                prop.intValue |= (int)HideFlags.NotEditable;
            }

            so.ApplyModifiedProperties();
#else
            if (show)
            {
                self.hideFlags &= ~HideFlags.HideInInspector;
            }
            else
            {
                self.hideFlags |= HideFlags.HideInInspector;
            }

            if (editable)
            {
                self.hideFlags &= ~HideFlags.NotEditable;
            }
            else
            {
                self.hideFlags |= HideFlags.NotEditable;
            }
#endif
        }

        /// <summary>
        ///     Checks whether the component is in a prefab.
        /// </summary>
        /// <param name="self">Component to check</param>
        /// <returns>Whether the component is in a prefab</returns>
        public static bool IsInPrefab(this Component self)
        {
            return self.gameObject.IsInPrefab();
        }

#if UNITY_EDITOR

       /// <summary>
        ///     Gets the GUID of the prefab the component is in, if it is in a prefab, or the GUID of the prefab the component was
        ///     instantiated from, if it was instantiated from a prefab.
        ///     If the component is not in a prefab and doesn't have a source prefab either, it will return string.Empty.
        /// </summary>
        /// <param name="prefabGuid">If the call was successful, returns the GUID or string.Empty</param>
        /// <returns>Whether the prefab GUID could be retrieved</returns>
        /// <remarks>
        ///     The reason the call can be unsuccessful is because Unity for some reason will report
        ///     a null/empty asset path even though PrefabUtility.IsPartOfPrefabAsset() returns true.
        ///     This behaviour happens when in prefab isolation/context mode in the editor
        /// </remarks>
        public static bool GetPrefabGuid(this Component self, out string prefabGuid)
        {
            return self.gameObject.GetPrefabGuid(out prefabGuid, out string _);
        }

        /// <summary>
        ///     Same as <see cref="GetPrefabGuid(UnityEngine.Component)" /> but it also returns the asset path if it exists.
        /// </summary>
        /// <param name="prefabGuid">If the call was successful, returns the GUID or string.Empty</param>
        /// <param name="assetPath">If the call was successful, returns the asset path or string.Empty</param>
        /// <returns>Whether the prefab GUID could be retrieved</returns>
        /// <remarks>
        ///     The reason the call can be unsuccessful is because Unity for some reason will report
        ///     a null/empty asset path even though PrefabUtility.IsPartOfPrefabAsset() returns true.
        ///     This behaviour happens when in prefab isolation/context mode in the editor
        /// </remarks>
        public static bool GetPrefabGuid(this Component self, out string prefabGuid, out string assetPath)
        {
            return self.gameObject.GetPrefabGuid(out prefabGuid, out assetPath);
        }

#endif

        /// <summary>
        ///     Gets the Component of a given type. If it doesn't exist, it is added to the GameObject.
        /// </summary>
        /// <param name="self">Component whose GameObject will be used to retrieve or add the given component type to</param>
        /// <typeparam name="T">Component type to get or add</typeparam>
        /// <returns>Existing component or newly added if it didn't exist before</returns>
        public static T GetOrAddComponent<T>(this Component self) where T : Component
        {
            T component = self.GetComponent<T>();

            if (component == null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    component = self.gameObject.AddComponent<T>();
                }
                else
                {
                    component = Undo.AddComponent<T>(self.gameObject);
                }
#else
                component = self.gameObject.AddComponent<T>();
#endif
            }

            return component;

        }

        /// <summary>
        ///     Gets the Component of a given type in the GameObject or any of its parents. It also works on prefabs, where regular
        ///     <see cref="Component.GetComponentInParent" /> will not work:
        ///     https://issuetracker.unity3d.com/issues/getcomponentinparent-is-returning-null-when-the-gameobject-is-a-prefab
        /// </summary>
        /// <typeparam name="T"><see cref="Component" /> type to get</typeparam>
        /// <returns>Component in same GameObject or any of its parents. Null if it wasn't found</returns>
        public static T SafeGetComponentInParent<T>(this Component self)
        {
            return self.GetComponentInParent<T>() ?? self.GetComponentsInParent<T>(true).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the full path under current scene, including all parents, but scene name, for the given component.
        /// </summary>
        /// <remarks>
        ///     The path generated might not be unique. If that is the purpose, use <see cref="GetUniqueScenePath" /> instead.
        /// </remarks>
        /// <param name="self"><see cref="Component" /> to get the path for</param>
        /// <param name="relativeTo">
        ///     Optional Transform to get the path relative to. If it's not the same Transform or a Transform up in the hierarchy
        ///     it will return the full path
        /// </param>
        /// <returns>Component path string</returns>
        public static string GetPathUnderScene(this Component self, Transform relativeTo = null)
        {
            string path = self.transform.GetPathUnderScene(relativeTo);
            return self is Transform ? path : $"{path}/{self.GetType().Name}";
        }

        /// <summary>
        ///     Gets an unique path in the scene for the given component. It will include scene name, sibling and component indices
        ///     to make it unique.
        /// </summary>
        /// <param name="self"><see cref="Component" /> to get the unique path for</param>
        /// <param name="relativeTo">
        ///     Optional Transform to get the path relative to. If it's not the same Transform or a Transform up in the hierarchy
        ///     it will return the full path
        /// </param>
        /// <returns>Unique component path string</returns>
        public static string GetUniqueScenePath(this Component self, Transform relativeTo = null)
        {
            string path = self.transform.GetUniqueScenePath(relativeTo);
            return self is Transform ? path : $"{path}/{Array.IndexOf(self.GetComponents<Component>(), self):00}{self.GetType().Name}";
        }

        /// <summary>
        ///     Gets an unique identifier string for the given component.
        /// </summary>
        /// <remarks>Generates an 8 characters hexadecimal hash code of <see cref="GetUniqueScenePath" />.</remarks>
        /// <param name="self"><see cref="Component" /> to get UID for.</param>
        /// <returns>8 characters hexadecimal unique identifier <see cref="string" /></returns>
        public static string GetSceneUid(this Component self)
        {
            return self.GetUniqueScenePath().GetHashCode().ToString("x8");
        }

        /// <summary>
        ///     Gets a list of all components of the given type in the open scenes
        /// </summary>
        /// <typeparam name="T">Type of component to look for</typeparam>
        /// <param name="includeInactive">Whether to include inactive components or not</param>
        /// <returns>List of components</returns>
        public static List<T> GetAllComponentsInOpenScenes<T>(bool includeInactive) where T : Component
        {
            List<T> listResult = new List<T>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    GameObject[] rootGameObjects = s.GetRootGameObjects();
                    foreach (GameObject go in rootGameObjects)
                    {
                        listResult.AddRange(go.GetComponentsInChildren<T>(includeInactive));
                    }
                }
            }

            return listResult;
        }

        /// <summary>
        ///     From a set of components, returns which one of them has a transform that is a common root of all.
        ///     The transform must be the transform of a component in the list.
        /// </summary>
        /// <param name="components">Components whose transforms to check</param>
        /// <returns>
        ///     Returns which transform from all the components passed as parameters is a common root of all. If no component has a
        ///     transform that is a common root it returns null.
        /// </returns>
        public static T GetCommonRootComponentFromSet<T>(params T[] components) where T : Component
        {
            T commonRoot = null;

            for (int i = 0; i < components.Length; i++)
            {
                if (i == 0)
                {
                    commonRoot = components[i];
                }
                else
                {
                    if (commonRoot == null || (components[i] != commonRoot && components[i].transform.HasParent(commonRoot.transform) == false))
                    {
                        bool found = true;

                        for (int j = 0; j < i - 1; j++)
                        {
                            if (components[i] != components[j] && components[j].transform.HasParent(components[i].transform) == false)
                            {
                                found = false;
                            }
                        }

                        commonRoot = found ? components[i] : null;
                    }
                }
            }

            return commonRoot;
        }

        #endregion
    }
}