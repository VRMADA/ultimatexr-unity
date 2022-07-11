// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrSingleton.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.IO;
using System.Threading;
using UltimateXR.Core.Threading;
using UnityEngine;

namespace UltimateXR.Core.Components.Singleton
{
    /// <summary>
    ///     <para>
    ///         An improved singleton implementation over <see cref="UxrAbstractSingleton{T}" /> for non-abstract classes.
    ///         <see cref="UxrSingleton{T}" /> guarantees that an <see cref="Instance" /> will always be available by
    ///         instantiating the singleton if it wasn't found in the scene. Additionally, it can instantiate a prefab
    ///         if there is one available in a well-known location.
    ///     </para>
    ///     <para>
    ///         The steps followed by this singleton implementation to assign the instance are the:
    ///     </para>
    ///     <para>
    ///         <list type="number">
    ///             <item>
    ///                 The singleton component is searched in the scene to see if it was pre-instantiated or is already
    ///                 available.
    ///             </item>
    ///             <item>
    ///                 If not found, the component tries to be instantiated in the scene using a prefab in a well known
    ///                 Resources folder. The well known path is <see cref="UxrConstants.Paths.SingletonResources" /> in any
    ///                 Resources folder and the prefab name is the singleton class name.
    ///                 A prefab allows to assign initial properties to the component and also hang additional resources
    ///                 (meshes, textures) from the GameObject if needed.
    ///             </item>
    ///             <item>
    ///                 If not found, a new <see cref="GameObject" /> is instantiated and the singleton is added using
    ///                 <see cref="GameObject.AddComponent{T}" />.
    ///             </item>
    ///         </list>
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     The singleton can be a pre-existing component in a scene. If not, <see cref="Instance" /> takes care of
    ///     instancing it and make it the singleton.
    ///     This singleton can only be used in sealed classes. For use in abstract classes check
    ///     <see cref="UxrAbstractSingleton{T}" /> instead.
    /// </remarks>
    /// <typeparam name="T">
    ///     Type the singleton is for. This template can only be used with a hierarchy where <typeparamref name="T" /> is
    ///     specified at its lowers level (sealed). For use in abstract classes, check <see cref="UxrAbstractSingleton{T}" />.
    /// </typeparam>
    public abstract class UxrSingleton<T> : UxrAbstractSingleton<T> where T : UxrSingleton<T>
    {
        #region Public Types & Data

        /// <inheritdoc cref="UxrAbstractSingleton{T}.Instance" />
        public new static T Instance
        {
            get
            {
                if (GetInstance() is null)
                {
                    UxrMonoDispatcher.RunOnMainThread(FindOrAddInstance);
                    while (GetInstance() is null && !UxrMonoDispatcher.IsCurrentThreadMain)
                    {
                        Thread.Sleep(25);
                    }
                }

                return GetInstance();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Dummy method forcing <see cref="Instance" /> to run the instance finding/creation process.
        /// </summary>
        public void Poke()
        {
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Tries to find a pre-existing instance in the scene and set it as the singleton instance.
        /// </summary>
        /// <returns></returns>
        private static bool TryFindInstance()
        {
            return TrySetInstance(FindObjectOfType<T>());
        }

        /// <summary>
        ///     <para>
        ///         Tries to find the singleton instance in the scene. If it was not found, a new singleton is instantiated
        ///         and assigned as the singleton instance. The following steps are used:
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             The singleton component is searched in the scene to see if it was pre-instantiated or is already
        ///             available.
        ///         </item>
        ///         <item>
        ///             If not found, the component tries to be instantiated in the scene using a prefab in a well known
        ///             Resources folder. The well known path is <see cref="UxrConstants.Paths.SingletonResources" /> in any
        ///             Resources
        ///             folder and the prefab name is the singleton class name.
        ///             A prefab can be used to assign initial properties to the component and also hang additional resources
        ///             (meshes, textures) from the GameObject.
        ///         </item>
        ///         <item>
        ///             If not found, a new <see cref="GameObject" /> is instantiated and the singleton is added using
        ///             <see cref="GameObject.AddComponent{T}" />.
        ///         </item>
        ///     </list>
        /// </summary>
        private static void FindOrAddInstance()
        {
            // First: Try to find instance in scene

            if (TryFindInstance())
            {
                return;
            }

            // Second: Try to instantiate it from Resources singleton folder

            T prefab   = Resources.Load<T>(ResourcesPrefabPath);
            T instance = null;

            if (prefab != null)
            {
                Debug.Log($"{typeof(T).Name} was able to load prefab from Resources folder: {ResourcesPrefabPath}");

                instance = Instantiate(prefab);

                if (instance == null)
                {
                    Debug.LogError($"[{typeof(T).Name}] Unable to instantiate prefab. Instance is null. Path is {ResourcesPrefabPath}", prefab);
                }
            }

            // Third: Try to instantiate it by creating a GameObject and adding the component

            if (instance == null)
            {
                instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }

            // Try to set it as the current singleton instance

            if (!TrySetInstance(instance))
            {
                Debug.LogError($"[{typeof(T).Name}] singleton failed to initialize", instance);
            }
        }

        #endregion

        #region Private Types & Data

        private static string ResourcesPrefabPath => Path.Combine(UxrConstants.Paths.SingletonResources, typeof(T).Name);

        #endregion
    }
}