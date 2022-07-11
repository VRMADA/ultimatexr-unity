// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabPointInfo.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Defines a <see cref="UxrGrabbableObject" /> grab point. A grab point describes a point of an object which
    ///     can be grabbed. Objects can have multiple grab points to allow it to be grabbed from different angles.
    ///     Grab points can be further expanded by using a <see cref="UxrGrabPointShape" />, which gives flexibility
    ///     by allowing it to be grabbed around or along an axis passing through that point, for example.
    /// </summary>
    [Serializable]
    public class UxrGrabPointInfo
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool                  _editorFoldout         = true;
        [SerializeField] private string                _editorName            = "";
        [SerializeField] private UxrGrabMode           _grabMode              = UxrGrabMode.GrabWhilePressed;
        [SerializeField] private bool                  _useDefaultGrabButtons = true;
        [SerializeField] private bool                  _bothHandsCompatible   = true;
        [SerializeField] private UxrHandSide           _handSide              = UxrHandSide.Left;
        [SerializeField] private UxrInputButtons       _inputButtons          = UxrInputButtons.Grip;
        [SerializeField] private bool                  _hideHandGrabberRenderer;
        [SerializeField] private UxrGripPoseInfo       _defaultGripPoseInfo   = new UxrGripPoseInfo(null);
        [SerializeField] private UxrSnapToHandMode     _snapMode              = UxrSnapToHandMode.PositionAndRotation;
        [SerializeField] private UxrHandSnapDirection  _snapDirection         = UxrHandSnapDirection.ObjectToHand;
        [SerializeField] private UxrSnapReference      _snapReference         = UxrSnapReference.UseOtherTransform;
        [SerializeField] private List<UxrGripPoseInfo> _avatarGripPoseEntries = new List<UxrGripPoseInfo>();
        [SerializeField] private bool                  _alignToController;
        [SerializeField] private Transform             _alignToControllerAxes;
        [SerializeField] private UxrGrabProximityMode  _grabProximityMode = UxrGrabProximityMode.UseProximity;
        [SerializeField] private BoxCollider           _grabProximityBox;
        [SerializeField] private float                 _maxDistanceGrab               = 0.2f;
        [SerializeField] private bool                  _grabProximityTransformUseSelf = true;
        [SerializeField] private Transform             _grabProximityTransform;
        [SerializeField] private bool                  _grabberProximityUseDefault = true;
        [SerializeField] private int                   _grabberProximityIndex;
        [SerializeField] private GameObject            _enableOnHandNear;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the <see cref="UxrGrabber" /> proximity index used to compute the distance to the object. -1 for default (the
        ///     grabber itself) or any other value for additional transforms in the <see cref="UxrGrabber" /> component.
        /// </summary>
        public int GrabberProximityTransformIndex => GrabberProximityUseDefault ? -1 : GrabberProximityIndex;

        /// <summary>
        ///     Gets how many grip pose entries there are. 1 (the default grip pose info) plus all the registered avatar ones.
        /// </summary>
        public int GripPoseInfoCount => 1 + _avatarGripPoseEntries.Count;

        /// <summary>
        ///     Gets the registered avatars for specific grip poses and properties.
        /// </summary>
        public List<UxrGripPoseInfo> AvatarGripPoseEntries => _avatarGripPoseEntries;

        /// <summary>
        ///     Gets or sets whether foldout control for a given grab point is folded out or not. We use this in the
        ///     editor to check if we need to render the preview grab pose meshes for a given grab point.
        ///     Grab points that are not folded out are not rendered.
        /// </summary>
        public bool IsEditorFoldedOut
        {
            get => _editorFoldout;
            set => _editorFoldout = value;
        }

        /// <summary>
        ///     Gets or sets the grab point display name in the inspector.
        /// </summary>
        public string EditorName
        {
            get => _editorName;
            set => _editorName = value;
        }

        /// <summary>
        ///     Gets or sets the grab mode.
        /// </summary>
        public UxrGrabMode GrabMode
        {
            get => _grabMode;
            set => _grabMode = value;
        }

        /// <summary>
        ///     Gets or sets whether to use the default grab buttons to grab the object using the grab point.
        /// </summary>
        public bool UseDefaultGrabButtons
        {
            get => _useDefaultGrabButtons;
            set => _useDefaultGrabButtons = value;
        }

        /// <summary>
        ///     Gets or sets whether both hands are compatible with the grab point.
        /// </summary>
        public bool BothHandsCompatible
        {
            get => _bothHandsCompatible;
            set => _bothHandsCompatible = value;
        }

        /// <summary>
        ///     If <see cref="BothHandsCompatible" /> is false, tells which hand is used to grab the object using the grab point.
        /// </summary>
        public UxrHandSide HandSide
        {
            get => _handSide;
            set => _handSide = value;
        }

        /// <summary>
        ///     If <see cref="UseDefaultGrabButtons" /> is false, tells which buttons are used to grab the object using the grab
        ///     point.
        /// </summary>
        public UxrInputButtons InputButtons
        {
            get => _inputButtons;
            set => _inputButtons = value;
        }

        /// <summary>
        ///     Gets or sets whether to hide the hand while it is grabbing the object using the grab point.
        /// </summary>
        public bool HideHandGrabberRenderer
        {
            get => _hideHandGrabberRenderer;
            set => _hideHandGrabberRenderer = value;
        }

        /// <summary>
        ///     Gets or sets the default grip pose info, which is the grip pose info used when an avatar interacts with an object
        ///     and is not registered to have specific properties.
        /// </summary>
        public UxrGripPoseInfo DefaultGripPoseInfo
        {
            get => _defaultGripPoseInfo;
            set => _defaultGripPoseInfo = value;
        }

        /// <summary>
        ///     Gets or sets how the object will snap to the hand when it is grabbed using the grab point.
        /// </summary>
        public UxrSnapToHandMode SnapMode
        {
            get => _snapMode;
            set => _snapMode = value;
        }

        /// <summary>
        ///     Gets or sets whether the object will snap to the hand or the hand will snap to the object when it is grabbed using
        ///     the grab point. Only used when any kind of snapping is enabled.
        /// </summary>
        public UxrHandSnapDirection SnapDirection
        {
            get => _snapDirection;
            set => _snapDirection = value;
        }

        /// <summary>
        ///     Gets or sets which reference to use for snapping when the object is grabbed using the grab point.
        /// </summary>
        public UxrSnapReference SnapReference
        {
            get => _snapReference;
            set => _snapReference = value;
        }

        /// <summary>
        ///     Gets or sets whether to align the grab to the controller axes, useful when grabbing objects that require aiming,
        ///     such as weapons.
        /// </summary>
        public bool AlignToController
        {
            get => _alignToController;
            set => _alignToController = value;
        }

        /// <summary>
        ///     Gets or sets the transform in the grabbable object to use that will align to the controller axes (x = right, y =
        ///     up, z = forward).
        /// </summary>
        public Transform AlignToControllerAxes
        {
            get => _alignToControllerAxes;
            set => _alignToControllerAxes = value;
        }

        /// <summary>
        ///     Gets or sets the proximity mode to use.
        /// </summary>
        public UxrGrabProximityMode GrabProximityMode
        {
            get => _grabProximityMode;
            set => _grabProximityMode = value;
        }

        /// <summary>
        ///     Gets or sets the box collider used when <see cref="GrabProximityMode" /> is
        ///     <see cref="UxrGrabProximityMode.BoxConstrained" />.
        /// </summary>
        public BoxCollider GrabProximityBox
        {
            get => _grabProximityBox;
            set => _grabProximityBox = value;
        }

        /// <summary>
        ///     Gets or sets the maximum distance the object can be grabbed using this the grab point.
        /// </summary>
        public float MaxDistanceGrab
        {
            get => _maxDistanceGrab;
            set => _maxDistanceGrab = value;
        }

        /// <summary>
        ///     Gets or sets whether to use the own <see cref="UxrGrabbableObject" /> transform when computing the distance to
        ///     <see cref="UxrGrabber" /> components.
        /// </summary>
        public bool GrabProximityTransformUseSelf
        {
            get => _grabProximityTransformUseSelf;
            set => _grabProximityTransformUseSelf = value;
        }

        /// <summary>
        ///     Gets or sets the <see cref="Transform" /> that will be used to compute the distance to <see cref="UxrGrabber" />
        ///     components when <see cref="GrabberProximityUseDefault" /> is false.
        /// </summary>
        public Transform GrabProximityTransform
        {
            get => _grabProximityTransform;
            set => _grabProximityTransform = value;
        }

        /// <summary>
        ///     Gets or sets whether to use the <see cref="UxrGrabber" /> transform when computing the distance to the grab point.
        ///     Otherwise it can specify additional proximity transforms using <see cref="GrabberProximityIndex" />.
        /// </summary>
        public bool GrabberProximityUseDefault
        {
            get => _grabberProximityUseDefault;
            set => _grabberProximityUseDefault = value;
        }

        /// <summary>
        ///     Gets or sets which additional proximity transform from <see cref="UxrGrabber" /> to use when
        ///     <see cref="GrabberProximityUseDefault" /> is false.
        /// </summary>
        public int GrabberProximityIndex
        {
            get => _grabberProximityIndex;
            set => _grabberProximityIndex = value;
        }

        /// <summary>
        ///     Gets or sets the <see cref="GameObject" /> to enable or disable when the object is grabbed or not using the grab
        ///     point.
        /// </summary>
        public GameObject EnableOnHandNear
        {
            get => _enableOnHandNear;
            set => _enableOnHandNear = value;
        }

        #endregion

        #region Internal Types & Data

        /// <summary>
        ///     Gets or sets the runtime grab info dictionary.
        /// </summary>
        internal Dictionary<UxrGrabber, UxrRuntimeGripInfo> RuntimeGrabs { get; set; } = new Dictionary<UxrGrabber, UxrRuntimeGripInfo>();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether to create a grip pose entry for the given avatar prefab.
        /// </summary>
        /// <param name="avatarGuid">Prefab GUID to generate a grip pose entry for</param>
        public void CheckAddGripPoseInfo(string avatarGuid)
        {
            if (string.IsNullOrEmpty(avatarGuid))
            {
                return;
            }

            // Only add if the given avatar prefab isn't found. If any parent prefabs are also registered, the new registered prefab will prevail over the parent prefab entries.

            if (_avatarGripPoseEntries.All(e => e.AvatarPrefabGuid != avatarGuid))
            {
                _avatarGripPoseEntries.Add(new UxrGripPoseInfo(avatarGuid));
            }
        }

        /// <summary>
        ///     Gets a given grip pose info entry.
        /// </summary>
        /// <param name="i">Index to retrieve</param>
        /// <returns>Grip pose info. If the index is 0 or not valid, it will return the default grip pose info</returns>
        public UxrGripPoseInfo GetGripPoseInfo(int i)
        {
            if (i == 0)
            {
                return DefaultGripPoseInfo;
            }

            if (i > 0 && i <= _avatarGripPoseEntries.Count)
            {
                return _avatarGripPoseEntries[i - 1];
            }

            return null;
        }

        /// <summary>
        ///     Gets a given grip pose info entry.
        /// </summary>
        /// <param name="prefabGuid">Prefab Guid whose info to retrieve</param>
        /// <returns>Grip pose info or null if it wasn't found</returns>
        public UxrGripPoseInfo GetGripPoseInfo(string prefabGuid)
        {
            return _avatarGripPoseEntries.FirstOrDefault(i => i.AvatarPrefabGuid == prefabGuid);
        }

        /// <summary>
        ///     Gets the grip pose info for the given avatar instance or prefab.
        /// </summary>
        /// <param name="avatar">Avatar to get the grip pose info for</param>
        /// <param name="usePrefabInheritance">
        ///     If the given avatar prefab info wasn't found, whether to look for the pose info for any prefab above the first
        ///     prefab in the hierarchy. This allows child prefabs to inherit poses and manipulation settings of parent prefabs
        /// </param>
        /// <returns>
        ///     Grip pose info. If <see cref="usePrefabInheritance" /> is false it will return null if the given prefab wasn't
        ///     found. If <see cref="usePrefabInheritance" /> is true, it will return <see cref="DefaultGripPoseInfo" /> if nor the
        ///     prefab nor a parent prefab entry were found
        /// </returns>
        public UxrGripPoseInfo GetGripPoseInfo(UxrAvatar avatar, bool usePrefabInheritance = true)
        {
            foreach (string avatarPrefabGuid in avatar.GetPrefabGuidChain())
            {
                foreach (UxrGripPoseInfo gripPoseInfo in _avatarGripPoseEntries)
                {
                    if (gripPoseInfo.AvatarPrefabGuid == avatarPrefabGuid)
                    {
                        return gripPoseInfo;
                    }
                }

                if (!usePrefabInheritance)
                {
                    return null;
                }
            }

            return DefaultGripPoseInfo;
        }

        /// <summary>
        ///     Gets all the grip pose infos that can be used with the given avatar.
        /// </summary>
        /// <param name="avatar">The avatar to check</param>
        /// <param name="usePrefabInheritance">Whether to check for compatibility using all the parents in the prefab hierarchy</param>
        /// <returns>List of <see cref="UxrGripPoseInfo" /> that are potentially compatible with the given avatar</returns>
        public IEnumerable<UxrGripPoseInfo> GetCompatibleGripPoseInfos(UxrAvatar avatar, bool usePrefabInheritance = true)
        {
            foreach (string avatarPrefabGuid in avatar.GetPrefabGuidChain())
            {
                foreach (UxrGripPoseInfo gripPoseInfo in _avatarGripPoseEntries)
                {
                    if (gripPoseInfo.AvatarPrefabGuid == avatarPrefabGuid)
                    {
                        yield return gripPoseInfo;
                    }
                }

                if (!usePrefabInheritance)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        ///     Removes the grip pose entry of a given avatar prefab.
        /// </summary>
        /// <param name="avatarPrefabGuid">Prefab GUID whose information to remove</param>
        public void RemoveGripPoseInfo(string avatarPrefabGuid)
        {
            if (string.IsNullOrEmpty(avatarPrefabGuid))
            {
                return;
            }

            _avatarGripPoseEntries.RemoveAll(e => e.AvatarPrefabGuid == avatarPrefabGuid);
        }

        #endregion
    }
}