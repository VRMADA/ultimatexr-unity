﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalLogic.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Avatar;
using UltimateXR.CameraUtils;
using UltimateXR.Core;
using UltimateXR.Core.Components;
using UltimateXR.Core.StateSync;
using UltimateXR.Devices.Keyboard;
using UltimateXR.Examples.FullScene.Doors;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Locomotion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltimateXR.Examples.FullScene
{
    public class GlobalLogic : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [Header("Spawn positions")] [SerializeField] private Transform _spawnMain;
        [SerializeField]                             private Transform _spawnLab;
        [SerializeField]                             private Transform _spawnControllers;
        [SerializeField]                             private Transform _spawnShootingRange;

        [Header("Location volumes")] [SerializeField] private BoxCollider _boxSpawnRoomMirror;
        [SerializeField]                              private BoxCollider _boxSpawnRoomDoor;
        [SerializeField]                              private BoxCollider _boxCentralRoom;
        [SerializeField]                              private BoxCollider _boxLabRoom;
        [SerializeField]                              private BoxCollider _boxControllerRoom;
        [SerializeField]                              private BoxCollider _boxShootingRange;

        [Header("Relevant elements")] [SerializeField] private GameObject   _rootUnrestrictedArea;
        [SerializeField]                               private GameObject   _rootRestrictedArea;
        [SerializeField]                               private UxrComponent _mirrorComponent;
        [SerializeField]                               private GameObject   _controllerRoomElements;
        [SerializeField]                               private GameObject   _rootLabElements;
        [SerializeField]                               private ArmoredDoor  _armoredDoor;

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to avatar events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            UxrAvatar.LocalAvatarStarted += UxrAvatar_LocalAvatarStarted;
            UxrAvatar.GlobalAvatarMoved  += UxrAvatar_GlobalAvatarMoved;
            UxrManager.AvatarsUpdated    += UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Unsubscribes from avatar events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            UxrAvatar.LocalAvatarStarted -= UxrAvatar_LocalAvatarStarted;
            UxrAvatar.GlobalAvatarMoved  -= UxrAvatar_GlobalAvatarMoved;
            UxrManager.AvatarsUpdated    -= UxrManager_AvatarsUpdated;
        }

        /// <summary>
        ///     Handles some keyboard shortcuts to reset, quit or quick spawn to different places.
        /// </summary>
        private void Update()
        {
            if (UxrKeyboardInput.GetPressDown(UxrKey.Enter))
            {
                SceneManager.LoadScene(0);
            }
            else if (UxrKeyboardInput.GetPressDown(UxrKey.Q))
            {
                Application.Quit();
            }

            if (UxrKeyboardInput.GetPressDown(UxrKey.Digit1))
            {
                UxrManager.Instance.TeleportLocalAvatar(_spawnMain.position, _spawnMain.rotation, UxrTranslationType.Fade);
            }
            else if (UxrKeyboardInput.GetPressDown(UxrKey.Digit2))
            {
                UxrManager.Instance.TeleportLocalAvatar(_spawnLab.position, _spawnLab.rotation, UxrTranslationType.Fade);
            }
            else if (UxrKeyboardInput.GetPressDown(UxrKey.Digit3))
            {
                UxrManager.Instance.TeleportLocalAvatar(_spawnControllers.position, _spawnControllers.rotation, UxrTranslationType.Fade);
            }
            else if (UxrKeyboardInput.GetPressDown(UxrKey.Digit4))
            {
                UxrManager.Instance.TeleportLocalAvatar(_spawnShootingRange.position, _spawnShootingRange.rotation, UxrTranslationType.Fade);
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the local avatar called its Start(). Moves the avatar to the spawn point and initializes the visible
        ///     elements.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrAvatar_LocalAvatarStarted(object sender, UxrAvatarStartedEventArgs e)
        {
            UxrManager.Instance.MoveAvatarTo(UxrAvatar.LocalAvatar, _spawnMain);
            UpdateVisibility();
        }

        /// <summary>
        ///     Called when the avatar moved/teleported. We use it to enable/disable objects based on potential visibility.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event parameters</param>
        private void UxrAvatar_GlobalAvatarMoved(object sender, UxrAvatarMoveEventArgs e)
        {
            if (ReferenceEquals(sender, UxrAvatar.LocalAvatar))
            {
                UpdateVisibility();
            }
        }

        /// <summary>
        ///     Called each frame after all avatars have been updated.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            if (UxrAvatar.LocalAvatar == null || UxrCameraWallFade.IsAvatarPeekingThroughGeometry(UxrAvatar.LocalAvatar))
            {
                return;
            }

            _rootRestrictedArea.CheckSetActive(UxrAvatar.LocalAvatar.CameraPosition.IsInsideBox(_boxShootingRange) || _armoredDoor.OpenValue > 0.0f);
            _rootUnrestrictedArea.CheckSetActive(!UxrAvatar.LocalAvatar.CameraPosition.IsInsideBox(_boxShootingRange) || _armoredDoor.OpenValue > 0.0f);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Updates the visible elements based on the current avatar position.
        /// </summary>
        private void UpdateVisibility()
        {
            if (UxrAvatar.LocalAvatar == null || UxrCameraWallFade.IsAvatarPeekingThroughGeometry(UxrAvatar.LocalAvatar))
            {
                return;
            }

            EnableVisibilityGameObjects(UxrAvatar.LocalAvatar.CameraPosition);
        }

        /// <summary>
        ///     Updates the visibility GameObjects using the given view position.
        /// </summary>
        /// <param name="viewPosition">View position</param>
        private void EnableVisibilityGameObjects(Vector3 viewPosition)
        {
            BeginSync(UxrStateSyncOptions.Replay);

            _mirrorComponent.CheckSetEnabled(viewPosition.IsInsideBox(_boxSpawnRoomMirror));
            _rootRestrictedArea.CheckSetActive(viewPosition.IsInsideBox(_boxShootingRange) || _armoredDoor.OpenValue > 0.0f);
            _rootUnrestrictedArea.CheckSetActive(!viewPosition.IsInsideBox(_boxShootingRange) || _armoredDoor.OpenValue > 0.0f);

            if (viewPosition.IsInsideBox(_boxSpawnRoomMirror))
            {
                _controllerRoomElements.CheckSetActive(false);
                _rootLabElements.CheckSetActive(false);
            }
            else if (viewPosition.IsInsideBox(_boxSpawnRoomDoor))
            {
                _controllerRoomElements.CheckSetActive(false);
                _rootLabElements.CheckSetActive(true);
            }
            else if (viewPosition.IsInsideBox(_boxCentralRoom) || viewPosition.IsInsideBox(_boxLabRoom) || viewPosition.IsInsideBox(_boxControllerRoom))
            {
                _controllerRoomElements.CheckSetActive(true);
                _rootLabElements.CheckSetActive(true);
            }
            else if (viewPosition.IsInsideBox(_boxShootingRange))
            {
                _controllerRoomElements.CheckSetActive(false);
                _rootLabElements.CheckSetActive(false);
            }

            EndSyncMethod(new object[] { viewPosition });
        }

        #endregion
    }
}