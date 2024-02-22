// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrInstanceManager.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.StateSave;
using UnityEngine;

namespace UltimateXR.Core.Instantiation
{
    public partial class UxrInstanceManager
    {
        #region Protected Overrides UxrComponent

        /// <inheritdoc />
        protected override void SerializeStateInternal(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeStateInternal(isReading, stateSerializationVersion, level, options);

            // Individual instantiations are already handled through events.
            // We save all generated instances in higher save levels.

            if (level > UxrStateSaveLevel.ChangesSincePreviousSave)
            {
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
                        if (_currentInstancedPrefabs == null || !_currentInstancedPrefabs.ContainsKey(pair.Key))
                        {
                            Destroy(pair.Value);

                            if (toRemove == null)
                            {
                                toRemove = new List<Guid>();
                            }

                            toRemove.Add(pair.Key);
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
                            Vector3    position    = pair.Info.Parent != null ? pair.Info.Parent.Transform.TransformPoint(pair.Info.Position) : pair.Info.Position;
                            Quaternion rotation    = pair.Info.Parent != null ? pair.Info.Parent.Transform.rotation * pair.Info.Rotation : pair.Info.Rotation;
                            GameObject newInstance = InstantiateGameObjectInternal(pair.Info.PrefabId, pair.Info.Parent, position, rotation, pair.CombineGuid);
                            CheckNetworkSpawnPostprocess(newInstance);
                        }
                    }
                }
            }
        }

        #endregion
    }
}