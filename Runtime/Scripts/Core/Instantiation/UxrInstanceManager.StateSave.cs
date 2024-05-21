// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManager.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.StateSave;
using UltimateXR.Core.Unique;
using UltimateXR.Extensions.System.Collections;
using UnityEngine;

namespace UltimateXR.Core.Instantiation
{
    public partial class UxrInstanceManager
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override int SerializationOrder => UxrConstants.Serialization.SerializationOrderInstanceManager;

        /// <inheritdoc />
        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            // Individual instantiations are already handled through events.
            // We save all generated instances in higher save levels.

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
                // When writing, update instance info first
                
                if (!isReading)
                {
                    foreach (KeyValuePair<Guid, InstanceInfo> pair in _currentInstancedPrefabs)
                    {
                        if (_currentInstances.TryGetValue(pair.Key, out GameObject instance) && instance != null)
                        {
                            pair.Value.UpdateInfoUsingObject(instance);
                        }
                    }
                }
                
                // We don't want to compare dictionaries, we save the instantiation info always by using null as name to avoid overhead.
                SerializeStateValue(level, options, null, ref _currentInstancedPrefabs);

                if (isReading)
                {
                    // When reading we need to update the scene:
                    //   -Destroy instances that do not exist anymore in the deserialized data
                    //   -Create instances that are not yet in the scene, from the deserialized data

                    // First destroy existing instances in the scene that are not present in the deserialized list anymore:

                    List<Guid> toRemove = null;

                    foreach (KeyValuePair<Guid, GameObject> pair in _currentInstances)
                    {
                        if (pair.Value == null)
                        {
                            continue;
                        }
                        
                        if (_currentInstancedPrefabs == null || !_currentInstancedPrefabs.ContainsKey(pair.Key))
                        {
                            // Before destroying the GameObject, unregister the components ahead of time too in case they are going to be re-created.
                            IUxrUniqueId[] components = pair.Value.GetComponentsInChildren<IUxrUniqueId>(true);
                            components.ForEach(c => c.Unregister());

                            Destroy(pair.Value);

                            if (toRemove == null)
                            {
                                toRemove = new List<Guid>();
                            }

                            toRemove.Add(pair.Key);
                        }
                        else if(_currentInstancedPrefabs != null && _currentInstancedPrefabs.TryGetValue(pair.Key, out InstanceInfo info))
                        {
                            // If the object is still present, update it to the current state
                            info.UpdateObjectUsingInfo(pair.Value);
                        }
                    }

                    if (toRemove != null)
                    {
                        foreach (Guid guid in toRemove)
                        {
                            _currentInstances.Remove(guid);
                        }
                    }

                    // Now instantiate prefabs that are present in the deserialized list but not in the scene:

                    List<(Guid CombineGuid, InstanceInfo Info)> toAdd = null;

                    if (_currentInstancedPrefabs != null)
                    {
                        foreach (KeyValuePair<Guid, InstanceInfo> pair in _currentInstancedPrefabs)
                        {
                            if (!_currentInstances.ContainsKey(pair.Key))
                            {
                                if (toAdd == null)
                                {
                                    toAdd = new List<(Guid, InstanceInfo)>();
                                }

                                toAdd.Add((pair.Key, pair.Value));
                            }
                        }
                    }

                    if (toAdd != null)
                    {
                        foreach ((Guid CombineGuid, InstanceInfo Info) pair in toAdd)
                        {
                            Vector3    position = pair.Info.Parent != null ? pair.Info.Parent.Transform.TransformPoint(pair.Info.Position) : pair.Info.Position;
                            Quaternion rotation = pair.Info.Parent != null ? pair.Info.Parent.Transform.rotation * pair.Info.Rotation : pair.Info.Rotation;
                            Vector3    scale    = pair.Info.Scale;

                            if (!string.IsNullOrEmpty(pair.Info.PrefabId))
                            {
                                // Prefab
                                GameObject newInstance = InstantiatePrefabInternal(pair.Info.PrefabId, pair.Info.Parent, position, rotation, 0, pair.CombineGuid);
                                newInstance.transform.localScale = scale;
                                CheckNetworkSpawnPostprocess(newInstance);
                            }
                            else
                            {
                                // Empty GameObject
                                GameObject newObject = InstantiateEmptyGameObjectInternal(pair.Info.Name, pair.Info.Parent, position, rotation, 0, pair.CombineGuid);
                                newObject.transform.localScale = scale;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}