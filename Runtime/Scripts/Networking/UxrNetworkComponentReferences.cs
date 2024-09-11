// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkComponentReferences.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.Settings;
using UltimateXR.Extensions.System.Collections;
using UltimateXR.Extensions.Unity;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateXR.Networking
{
    /// <summary>
    ///     Component that stores added network components from different SDKs so that they can be tracked more easily.
    ///     They are mainly used by the UxrNetworkManagerEditor to delete added components.
    /// </summary>
    public partial class UxrNetworkComponentReferences : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private Origin           _origin;
        [SerializeField] private List<GameObject> _addedGameObjects;
        [SerializeField] private List<Component>  _addedComponents;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the origin of the components.
        /// </summary>
        public Origin ComponentOrigin => _origin;

        /// <summary>
        ///     Gets or sets the networking GameObjects that were registered by this component so that they can be deleted later
        ///     using <see cref="DestroyWithAddedComponents" />.
        /// </summary>
        public List<GameObject> AddedGameObjects
        {
            get => _addedGameObjects;
            set => _addedGameObjects = value;
        }

        /// <summary>
        ///     Gets or sets the networking components that were registered by this component so that they can be deleted later
        ///     using <see cref="DestroyWithAddedComponents" />.
        /// </summary>
        public List<Component> AddedComponents
        {
            get => _addedComponents;
            set => _addedComponents = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a <see cref="UxrNetworkComponentReferences" /> on a given GameObject that will keep track of all added
        ///     network components.
        /// </summary>
        /// <param name="gameObject">Object to process</param>
        /// <param name="addedGameObjects">Network GameObjects that were added</param>
        /// <param name="addedComponents">Network components that were added</param>
        /// <param name="origin">The origin of the components</param>
        /// <returns>The created <see cref="UxrNetworkComponentReferences" /></returns>
        public static UxrNetworkComponentReferences RegisterNetworkComponents(GameObject gameObject, IEnumerable<GameObject> addedGameObjects, IEnumerable<Component> addedComponents, Origin origin)
        {
#if UNITY_EDITOR
            UxrNetworkComponentReferences networkComponents = Undo.AddComponent<UxrNetworkComponentReferences>(gameObject);
            SerializedObject              serializedObject  = new SerializedObject(networkComponents);

            serializedObject.FindProperty(UxrConstants.Editor.PropertyObjectHideFlags).enumValueFlag = (int)ComponentHideFlags;
            serializedObject.FindProperty(nameof(_origin)).enumValueIndex                            = (int)origin;

            SerializedProperty propAddedGameObjects = serializedObject.FindProperty(nameof(_addedGameObjects));
            SerializedProperty propAddedComponents  = serializedObject.FindProperty(nameof(_addedComponents));
            propAddedGameObjects.arraySize = addedGameObjects.Count();
            propAddedComponents.arraySize  = addedComponents.Count();

            int i = 0;

            foreach (GameObject go in addedGameObjects)
            {
                propAddedGameObjects.GetArrayElementAtIndex(i++).objectReferenceValue = go;
            }

            i = 0;

            foreach (Component component in addedComponents)
            {
                propAddedComponents.GetArrayElementAtIndex(i++).objectReferenceValue = component;
            }

            serializedObject.ApplyModifiedProperties();

            return networkComponents;
#else
            return null;
#endif
        }

        /// <summary>
        ///     Destroys all added network components in a GameObject and any of its children.
        /// </summary>
        /// <param name="gameObject">GameObject to process</param>
        public static void DestroyNetworkComponentsRecursively(GameObject gameObject, Origin origin)
        {
            gameObject.GetComponentsInChildren<UxrNetworkComponentReferences>(true).Where(c => c._origin == origin).ForEach(c => c.DestroyWithAddedComponents());
        }

        /// <summary>
        ///     Destroys this component together with all the networking components that were added to this same GameObject.
        /// </summary>
        public void DestroyWithAddedComponents()
        {
            if (Application.isPlaying)
            {
                if (_addedComponents != null)
                {
                    foreach (Component c in _addedComponents.Where(c => c != null))
                    {
                        Destroy(c);
                    }

                    _addedComponents.Clear();
                }

                if (_addedGameObjects != null)
                {
                    foreach (GameObject g in _addedGameObjects.Where(g => g != null))
                    {
                        Destroy(g);
                    }

                    _addedGameObjects.Clear();
                }
                
                Destroy(this);
                return;
            }
            
#if UNITY_EDITOR
            for (int i = 0; i < _addedComponents.Count; ++i)
            {
                if (_addedComponents[i] != null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Destroying component {_addedComponents[i].GetType().Name} in {_addedComponents[i].GetPathUnderScene()}");
                    }

                    Undo.DestroyObjectImmediate(_addedComponents[i]);
                }
                else
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} component {i} is null");
                    }
                }
            }

            for (int i = 0; i < _addedGameObjects.Count; ++i)
            {
                if (_addedGameObjects[i] != null)
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} Destroying GameObject {_addedGameObjects[i].GetPathUnderScene()}");
                    }
                    
                    Undo.DestroyObjectImmediate(_addedGameObjects[i]);
                }
                else
                {
                    if (UxrGlobalSettings.Instance.LogLevelNetworking >= UxrLogLevel.Relevant)
                    {
                        Debug.Log($"{UxrConstants.NetworkingModule} GameObject {i} is null");
                    }
                }
            }

            _addedGameObjects.Clear();
            _addedComponents.Clear();

            if (this != null)
            {
                Undo.DestroyObjectImmediate(this);
            }
#endif
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the visibility.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            hideFlags = ComponentHideFlags;
        }

        /// <summary>
        ///     This is a workaround to make hideFlags work on prefabs. Maybe related:
        ///     https://forum.unity.com/threads/is-it-impossible-to-save-component-hideflags-in-a-prefab.976974/
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            hideFlags = ComponentHideFlags;
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the required hide flags for this component.
        /// </summary>
        private static HideFlags ComponentHideFlags => HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;

        #endregion
    }
}