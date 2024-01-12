// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponentInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace UltimateXR.Editor
{
    /// <summary>
    ///     Used by <see cref="UxrComponentProcessor{T}" /> to send information about the component that needs to be processed.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    public class UxrComponentInfo<T> where T : Component
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the target component that needs to be processed.
        /// </summary>
        public T TargetComponent { get; }

        /// <summary>
        ///     Gets, if <see cref="TargetComponent" /> is located inside a prefab, its root GameObject.
        ///     It is null if <see cref="TargetComponent" /> is in a GameObject in a scene.
        /// </summary>
        public GameObject TargetPrefab { get; }

        /// <summary>
        ///     Gets whether the component is the original source of data or it exists in a parent prefab instead.
        ///     This can be used to process components only once and have the source value come from the original source component.
        ///     <list type="bullet">
        ///         <item>For prefabs, it is true in the original prefab and false in prefab variants</item>
        ///         <item>
        ///             For GameObjects in the scene, it is true if they are not instantiated prefabs and false if they are the
        ///             result of instantiating a prefab in the scene
        ///         </item>
        ///     </list>
        ///     When processing components, this enables setting values only when <see cref="IsOriginalSource" /> is true,
        ///     and setting the <see cref="SerializedProperty.prefabOverride" /> to false for non-original components.
        /// </summary>
        /// <remarks>
        ///     This property doesn't check if the data is actually overriden or not, it just tells whether the component
        ///     comes from a source prefab or not.
        /// </remarks>
        public bool IsOriginalSource { get; }

        /// <summary>
        ///     <para>
        ///         Similar to <see cref="IsOriginalSource" /> but when working with specific project paths.
        ///         If <see cref="IsOriginalSource" /> is true, <see cref="IsInnermostInValidChain" /> will always be true.
        ///         If <see cref="IsOriginalSource" /> is false, it will be true if the parent prefab is not located in a valid
        ///         project path. This means that the instance or the prefab in question is the last element in the chain that
        ///         is still in a valid project path.
        ///     </para>
        ///     <para>
        ///         Here are some examples when <see cref="IsOriginalSource" /> might be false but
        ///         <see cref="IsInnermostInValidChain" /> is true:
        ///         <list type="bullet">
        ///             <item>
        ///                 You want to apply changes to prefabs or instances in an application (for example the
        ///                 /Assets/Application folder) but not in the root prefabs that lie in a framework folder in
        ///                 /Assets/Framework
        ///             </item>
        ///             <item>
        ///                 You want to apply changes in prefab variants where the original prefab is in UltimateXR, for example
        ///                 avatars. This will ensure that your modifications are applied to your prefabs but not to UltimateXR
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>
        ///         The main use of <see cref="IsInnermostInValidChain" /> is to help component processors that target only
        ///         the innermost prefab. Usually these component processors' goal is to add or modify a component on the innermost
        ///         prefab, letting all child prefabs inherit these changes. Sometimes the innermost prefab or the prefab instance
        ///         is located outside the target path, but it is still desired to modify the one that is still inside the target
        ///         path so that all child prefabs that are also inside can inherit the changes. In the case of a prefab instance
        ///         it allows the prefab instance to have the changes.
        ///     </para>
        /// </summary>
        public bool IsInnermostInValidChain { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="component">Component to process</param>
        public UxrComponentInfo(T component)
        {
            TargetComponent = component;
            T componentInParent = PrefabUtility.GetCorrespondingObjectFromSource(component);
            if (componentInParent != null)
            {
                TargetPrefab = componentInParent.transform.root.gameObject;
            }

            IsOriginalSource        = componentInParent == null;
            IsInnermostInValidChain = false;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="component">Component to process</param>
        /// <param name="prefab">
        ///     Root GameObject of the prefab to process if <paramref name="component" /> is located inside a prefab.
        ///     It is null if the <paramref name="component" /> being processed is in a scene.
        /// </param>
        /// <param name="isOriginalSource">
        ///     If <paramref name="component" /> is the original source of data (true) or it is instantiated from a parent prefab
        ///     (false)
        /// </param>
        /// <param name="isInnermostInValidChain">See <see cref="IsInnermostInValidChain" /></param>
        public UxrComponentInfo(T component, GameObject prefab, bool isOriginalSource, bool isInnermostInValidChain)
        {
            TargetComponent         = component;
            TargetPrefab            = prefab;
            IsOriginalSource        = isOriginalSource;
            IsInnermostInValidChain = isInnermostInValidChain;
        }

        #endregion
    }
}