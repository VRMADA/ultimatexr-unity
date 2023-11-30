// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrManager.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UltimateXR.Animation.Interpolation;
using UltimateXR.Avatar;
using UltimateXR.Avatar.Controllers;
using UltimateXR.Core.Caching;
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Extensions.System.Threading;
using UltimateXR.Extensions.Unity;
using UltimateXR.Extensions.Unity.Math;
using UltimateXR.Extensions.Unity.Render;
using UltimateXR.Locomotion;
using UltimateXR.Manipulation;
using UltimateXR.Mechanics.Weapons;
using UltimateXR.UI.UnityInputModule;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltimateXR.Core
{
    /// <summary>
    ///     <para>
    ///         Main manager in the UltimateXR framework. As a <see cref="UxrSingleton{T}">UxrSingleton</see> it can be
    ///         accessed at any point in the application through <see cref="UxrSingleton{T}.Instance">UxrManager.Instance</see>
    ///         . It can be pre-instantiated in the scene in order to change default parameters through the inspector but it is
    ///         not required. When accessing the global <see cref="UxrSingleton{T}.Instance">UxrManager.Instance</see>, if no
    ///         <see cref="UxrManager" /> is currently available, one will be instantiated in the scene as the global
    ///         Singleton.
    ///     </para>
    ///     <para>
    ///         <see cref="UxrManager" /> is responsible for updating all key framework entities such as avatars each frame in
    ///         the correct order. Events and callbacks are provided so that custom updates can be executed at appropriate
    ///         stages of the updating process.
    ///     </para>
    ///     <para>
    ///         <see cref="UxrManager" /> also provides commonly required functionality:
    ///         <list type="bullet">
    ///             <item>Pre-caching prefabs when scenes are loaded to eliminate hiccups.</item>
    ///             <item>Moving/rotating/teleporting avatars.</item>
    ///             <item>Provide events to notify when avatars have been moved/rotated/teleported.</item>
    ///             <item>
    ///                 Provide events to notify before and after updating a frame and at different stages of the updating
    ///                 process for finer control.
    ///             </item>
    ///         </list>
    ///     </para>
    /// </summary>
    public sealed class UxrManager : UxrSingleton<UxrManager>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private UxrPostUpdateMode _postUpdateMode     = UxrPostUpdateMode.LateUpdate;
        [SerializeField] private bool              _usePrecaching      = true;
        [SerializeField] private int               _precacheFrameCount = 50;

        #endregion

        #region Public Types & Data

        // SDK strings

        public const string SdkUnityInputSystem    = "Unity Input System";
        public const string SdkOculus              = "Oculus";
        public const string SdkSteamVR             = "SteamVR";
        public const string SdkWindowsMixedReality = "Windows Mixed Reality";
        public const string SdkUltraleap           = "Ultraleap";
        public const string SdkWaveXR              = "WaveXR";
        public const string SdkPicoXR              = "PicoXR";
        public const string SdkMagicLeap           = "Magic Leap";

        // Events

        /// <summary>
        ///     Called right before precaching is about to start. It's called on the first frame that is displayed black.
        ///     See <see cref="UsePrecaching"/>.
        /// </summary>
        public static event Action PrecachingStarting;

        /// <summary>
        ///     Called right after precaching finished. It's called on the first frame that starts to fade-in from black.
        ///     See <see cref="UsePrecaching"/>.
        /// </summary>
        public static event Action PrecachingFinished;

        /// <summary>
        ///     Called right before processing all update stages in the current frame. Equivalent to <see cref="StageUpdating" />
        ///     for <see cref="UxrUpdateStage.Update" />
        /// </summary>
        public static event Action AvatarsUpdating;

        /// <summary>
        ///     Called right after processing all update stages in the current frame. Equivalent to <see cref="StageUpdated" /> for
        ///     <see cref="UxrUpdateStage.PostProcess" />
        /// </summary>
        public static event Action AvatarsUpdated;

        /// <summary>
        ///     Called right before an update stage in the current frame. See <see cref="UxrUpdateStage" />.
        /// </summary>
        public static event Action<UxrUpdateStage> StageUpdating;

        /// <summary>
        ///     Called right after an update stage in the current frame. See <see cref="UxrUpdateStage" />.
        /// </summary>
        public static event Action<UxrUpdateStage> StageUpdated;

        /// <summary>
        ///     Called right before an <see cref="UxrAvatar" /> is about to be moved.
        /// </summary>
        public static event EventHandler<UxrAvatarMoveEventArgs> AvatarMoving;

        /// <summary>
        ///     Called right after an <see cref="UxrAvatar" /> was moved.
        /// </summary>
        public static event EventHandler<UxrAvatarMoveEventArgs> AvatarMoved;

        /// <summary>
        ///     Gets whether the manager is currently pre-caching. This happens right after the local avatar is enabled and
        ///     <see cref="UsePrecaching" /> is set.
        /// </summary>
        public bool IsPrecaching => _precacheCoroutine != null;

        // Properties

        /// <summary>
        ///     Gets or sets when to perform the post-update. The post-update updates among others the avatar animation (hand
        ///     poses, manipulation mechanics and Inverse Kinematics).
        ///     It is <see cref="UxrPostUpdateMode.LateUpdate" /> by default to make sure they are played on top of any animation
        ///     generated by Unity built-in animation components like <see cref="Animator" />.
        /// </summary>
        public UxrPostUpdateMode PostUpdateMode
        {
            get => _postUpdateMode;
            set => _postUpdateMode = value;
        }

        /// <summary>
        ///     Gets or sets whether the manager uses pre-caching. Pre-caching happens right after the local avatar is enabled and
        ///     consists of instantiating objects described in all <see cref="IUxrPrecacheable" /> components in the scene. These
        ///     objects are placed right in front of the camera while it is faded black, so that they can't be seen, which forces
        ///     their resources to be loaded in order to reduce hiccups when they need to be instantiated during the session. After
        ///     that they are deleted and the scene is faded in.
        /// </summary>
        public bool UsePrecaching
        {
            get => _usePrecaching;
            set => _usePrecaching = value;
        }

        /// <summary>
        ///     Gets or sets the number of frames pre-cached objects are shown. These frames are drawn in black and right after the
        ///     scene will fade in, so that pre-caching is hidden to the user.
        /// </summary>
        public int PrecacheFrameCount
        {
            get => _precacheFrameCount;
            set => _precacheFrameCount = value;
        }

        /// <summary>
        ///     Gets or sets the color used when teleporting using screen fading transitions.
        /// </summary>
        public Color TeleportFadeColor { get; set; } = default;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Given a component that requires an <see cref="UxrAvatar" /> component in the hierarchy in order to work, logs an
        ///     error indicating that it's missing.
        /// </summary>
        /// <param name="component">Component that requires an <see cref="UxrAvatar" /> on its GameObject or any of its parents.</param>
        public static void LogMissingAvatarInHierarchyError(Component component)
        {
            Debug.LogError($"{component.GetType().Name} requires to be part of an {nameof(UxrAvatar)} in order to work correctly. GameObject is {component.GetPathUnderScene()}.");
        }

        /// <summary>
        ///     Given a component that requires an <see cref="UxrAvatar" /> component in the scene in order to work, logs an error
        ///     indicating that it's missing.
        /// </summary>
        /// <param name="component">Component that requires an <see cref="UxrAvatar" /> in the scene.</param>
        public static void LogMissingAvatarInScene(Component component)
        {
            Debug.LogError($"{component.GetType().Name} requires an avatar in the scene to work correctly. GameObject is {component.GetPathUnderScene()}.");
        }

        /// <summary>
        ///     Translates an avatar.
        /// </summary>
        /// <param name="avatar">The avatar to translate</param>
        /// <param name="translation">Translation offset</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        public void TranslateAvatar(UxrAvatar avatar, Vector3 translation, bool propagateEvents = true)
        {
            MoveAvatarTo(avatar, avatar.CameraFloorPosition + translation, avatar.ProjectedCameraForward, propagateEvents);
        }

        /// <summary>
        ///     Moves an avatar to a new position on the floor, keeping the same viewing direction. The eye level is maintained.
        /// </summary>
        /// <param name="avatar">The avatar to move</param>
        /// <param name="newFloorPosition">
        ///     The position on the floor above which the avatar's camera will be positioned.
        ///     Coordinates need to be specified at ground level since the eye camera level over the floor will be maintained.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        public void MoveAvatarTo(UxrAvatar avatar, Vector3 newFloorPosition, bool propagateEvents = true)
        {
            MoveAvatarTo(avatar, newFloorPosition, avatar.ProjectedCameraForward, propagateEvents);
        }

        /// <summary>
        ///     Moves an avatar to a new position on the floor and a viewing direction. The eye level is maintained.
        /// </summary>
        /// <param name="avatar">The avatar to move</param>
        /// <param name="newFloorPosition">
        ///     The position on the floor above which the avatar's camera will be positioned.
        ///     Coordinates need to be specified at ground level since the eye camera level over the floor will be maintained.
        /// </param>
        /// <param name="newForward">The new viewing direction of the avatar, including the camera.</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        public void MoveAvatarTo(UxrAvatar avatar, Vector3 newFloorPosition, Vector3 newForward, bool propagateEvents = true)
        {
            Transform avatarTransform = avatar.transform;

            Vector3    oldPosition = avatarTransform.position;
            Quaternion oldRotation = avatarTransform.rotation;
            Vector3    newPosition = oldPosition;
            Quaternion newRotation = oldRotation;

            TransformExt.ApplyAlignment(ref newPosition, ref newRotation, avatar.CameraFloorPosition, Quaternion.LookRotation(avatar.ProjectedCameraForward), newFloorPosition, Quaternion.LookRotation(newForward), true, true);

            if (propagateEvents)
            {
                OnAvatarMoving(new UxrAvatarMoveEventArgs(avatar, oldPosition, oldRotation, newPosition, newRotation));
            }

            avatarTransform.SetPositionAndRotation(newPosition, newRotation);

            if (propagateEvents)
            {
                OnAvatarMoved(new UxrAvatarMoveEventArgs(avatar, oldPosition, oldRotation, newPosition, newRotation));
            }
        }

        /// <summary>
        ///     See <see cref="MoveAvatarTo(UxrAvatar,UnityEngine.Vector3,UnityEngine.Vector3)">MoveAvatarTo</see>.
        /// </summary>
        /// <param name="avatar">The avatar to move</param>
        /// <param name="destination">The position and orientation on the floor</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        public void MoveAvatarTo(UxrAvatar avatar, Transform destination, bool propagateEvents = true)
        {
            if (avatar && destination)
            {
                MoveAvatarTo(avatar, destination.position, destination.forward, propagateEvents);
            }
        }

        /// <summary>
        ///     Moves the avatar to a new floor level.
        /// </summary>
        /// <param name="avatar">The avatar to move</param>
        /// <param name="floorLevel">The new floor level (Y)</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        public void MoveAvatarTo(UxrAvatar avatar, float floorLevel, bool propagateEvents = true)
        {
            if (avatar)
            {
                Vector3 newPosition = avatar.CameraFloorPosition;
                newPosition.y = floorLevel;
                MoveAvatarTo(avatar, newPosition, propagateEvents);
            }
        }

        /// <summary>
        ///     Rotates the avatar around its vertical axis, where a positive angle turns it to the right and a negative angle to
        ///     the left.
        /// </summary>
        /// <param name="avatar">The avatar to rotate</param>
        /// <param name="degrees">The degrees to rotate</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        public void RotateAvatar(UxrAvatar avatar, float degrees, bool propagateEvents = true)
        {
            Transform avatarTransform = avatar.transform;
            MoveAvatarTo(avatar, avatar.CameraFloorPosition, avatar.ProjectedCameraForward.GetRotationAround(avatarTransform.up, degrees), propagateEvents);
        }

        /// <summary>
        ///     Teleports the local <see cref="UxrAvatar" />. The local avatar is the avatar controlled by the user using the
        ///     headset and input controllers. Non-local avatars are other avatars instantiated in the scene but not controlled by
        ///     the user, either other users through the network or other scenarios such as automated replays.
        /// </summary>
        /// <param name="newFloorPosition">
        ///     World-space floor-level position the avatar will be teleported over. The camera position will be on top of the
        ///     floor position, keeping the original eye-level.
        /// </param>
        /// <param name="newRotation">
        ///     World-space rotation the avatar will be teleported to. The camera will point in the rotation's forward direction.
        /// </param>
        /// <param name="translationType">The type of translation to use. By default it will teleport immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="translationType" /> has a duration, it will specify how long the
        ///     teleport transition will take in seconds. By default it is <see cref="UxrConstants.TeleportTranslationSeconds" />
        /// </param>
        /// <param name="teleportedCallback">
        ///     Optional callback executed depending on the teleportation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrTranslationType.Immediate" />: Right after finishing the teleportation.</item>
        ///         <item>
        ///             <see cref="UxrTranslationType.Fade" />: When the screen is completely faded out and the avatar has been
        ///             moved, before fading back in. This can be used to enable/disable/change GameObjects in the scene since the
        ///             screen at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrTranslationType.Smooth" />: Right after finishing the teleportation.</item>
        ///     </list>
        /// </param>
        /// <param name="finishedCallback">
        ///     Optional callback executed right after the teleportation finished. It will receive a boolean parameter telling
        ///     whether the teleport finished completely (true) or was cancelled (false). If a fade effect has been requested, the
        ///     callback is executed right after the screen has faded back in.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Coroutine enumerator</returns>
        /// <remarks>
        ///     If <see cref="UxrTranslationType.Fade" /> translation mode was specified, the default black fade color can be
        ///     changed using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public void TeleportLocalAvatar(Vector3            newFloorPosition,
                                        Quaternion         newRotation,
                                        UxrTranslationType translationType    = UxrTranslationType.Immediate,
                                        float              transitionSeconds  = UxrConstants.TeleportTranslationSeconds,
                                        Action             teleportedCallback = null,
                                        Action<bool>       finishedCallback   = null,
                                        bool               propagateEvents    = true)
        {
            if (_teleportCoroutine != null)
            {
                StopCoroutine(_teleportCoroutine);
            }

            bool hasFinished = false;
            _teleportCoroutine = StartCoroutine(TeleportLocalAvatarCoroutine(newFloorPosition, newRotation, translationType, transitionSeconds, teleportedCallback, () => hasFinished = true, propagateEvents));
            finishedCallback?.Invoke(hasFinished);
        }

        /// <summary>
        ///     Teleports the local <see cref="UxrAvatar" /> while making sure to keep relative position/orientation on moving
        ///     objects. Some <paramref name="translationType" /> values have a transition before the teleport to avoid motion
        ///     sickness. On worlds with moving platforms it is important to specify the destination transform so that:
        ///     <list type="bullet">
        ///         <item>Relative position/orientation to the destination is preserved.</item>
        ///         <item>Optionally the local avatar can be parented to the new destination.</item>
        ///     </list>
        ///     The local avatar is the avatar controlled by the user using the headset and input controllers. Non-local avatars
        ///     are other avatars instantiated in the scene but not controlled by the user, either other users through the network
        ///     or other scenarios such as automated replays.
        /// </summary>
        /// <param name="referenceTransform">
        ///     The object the avatar should keep relative position/orientation to. This should be the moving object the avatar has
        ///     teleported on top of
        /// </param>
        /// <param name="parentToReference">
        ///     Whether to parent the avatar to <paramref name="referenceTransform" />. The avatar should be parented if it's being
        ///     teleported to a moving hierarchy it is not part of
        /// </param>
        /// <param name="newFloorPosition">
        ///     World-space floor-level position the avatar will be teleported over. The camera position will be on top of the
        ///     floor position, keeping the original eye-level.
        /// </param>
        /// <param name="newRotation">
        ///     World-space rotation the avatar will be teleported to. The camera will point in the rotation's forward direction.
        /// </param>
        /// <param name="translationType">The type of translation to use. By default it will teleport immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="translationType" /> has a duration, it will specify how long the
        ///     teleport transition will take in seconds. By default it is <see cref="UxrConstants.TeleportTranslationSeconds" />
        /// </param>
        /// <param name="teleportedCallback">
        ///     Optional callback executed depending on the teleportation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrTranslationType.Immediate" />: Right after finishing the teleportation.</item>
        ///         <item>
        ///             <see cref="UxrTranslationType.Fade" />: When the screen is completely faded out and the avatar has been
        ///             moved, before fading back in. This can be used to enable/disable/change GameObjects in the scene since the
        ///             screen at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrTranslationType.Smooth" />: Right after finishing the teleportation.</item>
        ///     </list>
        /// </param>
        /// <param name="finishedCallback">
        ///     Optional callback executed right after the teleportation finished. It will receive a boolean parameter telling
        ///     whether the teleport finished completely (true) or was cancelled (false). If a fade effect has been requested, the
        ///     callback is executed right after the screen has faded back in.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Coroutine enumerator</returns>
        /// <remarks>
        ///     If <see cref="UxrTranslationType.Fade" /> translation mode was specified, the default black fade color can be
        ///     changed using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public void TeleportLocalAvatarRelative(Transform          referenceTransform,
                                                bool               parentToReference,
                                                Vector3            newFloorPosition,
                                                Quaternion         newRotation,
                                                UxrTranslationType translationType    = UxrTranslationType.Immediate,
                                                float              transitionSeconds  = UxrConstants.TeleportTranslationSeconds,
                                                Action             teleportedCallback = null,
                                                Action<bool>       finishedCallback   = null,
                                                bool               propagateEvents    = true)
        {
            if (_teleportCoroutine != null)
            {
                StopCoroutine(_teleportCoroutine);
            }

            Vector3    newRelativeFloorPosition = referenceTransform != null ? referenceTransform.InverseTransformPoint(newFloorPosition) : newFloorPosition;
            Quaternion newRelativeRotation      = referenceTransform != null ? Quaternion.Inverse(referenceTransform.rotation) * newRotation : newRotation;
            bool       hasFinished              = false;

            _teleportCoroutine = StartCoroutine(TeleportLocalAvatarRelativeCoroutine(referenceTransform, parentToReference, newRelativeFloorPosition, newRelativeRotation, translationType, transitionSeconds, teleportedCallback, () => hasFinished = true, propagateEvents));

            finishedCallback?.Invoke(hasFinished);
        }

        /// <summary>
        ///     <para>
        ///         Asynchronous version of <see cref="TeleportLocalAvatar"> TeleportLocalAvatar</see>.
        ///     </para>
        ///     Teleports the local <see cref="UxrAvatar" />. The local avatar is the avatar controlled by the user using the
        ///     headset and input controllers. Non-local avatars are other avatars instantiated in the scene but not controlled by
        ///     the user, either other users through the network or other scenarios such as automated replays.
        /// </summary>
        /// <param name="newFloorPosition">
        ///     World-space floor-level position the avatar will be teleported over. The camera position will be on top of the
        ///     floor position, keeping the original eye-level.
        /// </param>
        /// <param name="newRotation">
        ///     World-space rotation the avatar will be teleported to. The camera will point in the rotation's forward direction.
        /// </param>
        /// <param name="translationType">The type of translation to use. By default it will teleport immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="translationType" /> has a duration, it will specify how long the
        ///     teleport transition will take in seconds. By default it is <see cref="UxrConstants.TeleportTranslationSeconds" />
        /// </param>
        /// <param name="teleportedCallback">
        ///     Optional callback executed depending on the teleportation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrTranslationType.Immediate" />: Right after finishing the teleportation.</item>
        ///         <item>
        ///             <see cref="UxrTranslationType.Fade" />: When the screen is completely faded out and the avatar has been
        ///             moved, before fading back in. This can be used to enable/disable/change GameObjects in the scene since the
        ///             screen at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrTranslationType.Smooth" />: Right after finishing the teleportation.</item>
        ///     </list>
        /// </param>
        /// <param name="ct">Optional cancellation token that can be used to cancel the task</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Awaitable <see cref="Task" /> that will finish after the avatar was teleported or if it was cancelled</returns>
        /// <exception cref="TaskCanceledException">Task was canceled using <paramref name="ct" /></exception>
        /// <remarks>
        ///     If <see cref="UxrTranslationType.Fade" /> translation mode was specified, the default black fade color can be
        ///     changed using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public async Task TeleportLocalAvatarAsync(Vector3            newFloorPosition,
                                                   Quaternion         newRotation,
                                                   UxrTranslationType translationType    = UxrTranslationType.Immediate,
                                                   float              transitionSeconds  = UxrConstants.TeleportTranslationSeconds,
                                                   Action             teleportedCallback = null,
                                                   CancellationToken  ct                 = default,
                                                   bool               propagateEvents    = true)
        {
            bool finished = false;

            Coroutine teleportCoroutine = StartCoroutine(TeleportLocalAvatarCoroutine(newFloorPosition, newRotation, translationType, transitionSeconds, teleportedCallback, () => finished = true, propagateEvents));
            await TaskExt.WaitUntil(() => finished, ct);

            if (ct.IsCancellationRequested)
            {
                StopCoroutine(teleportCoroutine);
            }
        }

        /// <summary>
        ///     <para>
        ///         Asynchronous version of <see cref="TeleportLocalAvatarRelative"> TeleportLocalAvatar</see>.
        ///     </para>
        ///     Teleports the local <see cref="UxrAvatar" />. The local avatar is the avatar controlled by the user using the
        ///     headset and input controllers. Non-local avatars are other avatars instantiated in the scene but not controlled by
        ///     the user, either other users through the network or other scenarios such as automated replays.
        /// </summary>
        /// <param name="referenceTransform">
        ///     The object the avatar should keep relative position/orientation to. This should be the moving object the avatar has
        ///     teleported on top of
        /// </param>
        /// <param name="parentToReference">
        ///     Whether to parent the avatar to <paramref name="referenceTransform" />. The avatar should be parented if it's being
        ///     teleported to a moving hierarchy it is not part of
        /// </param>
        /// <param name="newFloorPosition">
        ///     World-space floor-level position the avatar will be teleported over. The camera position will be on top of the
        ///     floor position, keeping the original eye-level.
        /// </param>
        /// <param name="newRotation">
        ///     World-space rotation the avatar will be teleported to. The camera will point in the rotation's forward direction.
        /// </param>
        /// <param name="translationType">The type of translation to use. By default it will teleport immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="translationType" /> has a duration, it will specify how long the
        ///     teleport transition will take in seconds. By default it is <see cref="UxrConstants.TeleportTranslationSeconds" />
        /// </param>
        /// <param name="teleportedCallback">
        ///     Optional callback executed depending on the teleportation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrTranslationType.Immediate" />: Right after finishing the teleportation.</item>
        ///         <item>
        ///             <see cref="UxrTranslationType.Fade" />: When the screen is completely faded out and the avatar has been
        ///             moved, before fading back in. This can be used to enable/disable/change GameObjects in the scene since the
        ///             screen at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrTranslationType.Smooth" />: Right after finishing the teleportation.</item>
        ///     </list>
        /// </param>
        /// <param name="ct">Optional cancellation token that can be used to cancel the task</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Awaitable <see cref="Task" /> that will finish after the avatar was teleported or if it was cancelled</returns>
        /// <exception cref="TaskCanceledException">Task was canceled using <paramref name="ct" /></exception>
        /// <remarks>
        ///     If <see cref="UxrTranslationType.Fade" /> translation mode was specified, the default black fade color can be
        ///     changed using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public async Task TeleportLocalAvatarRelativeAsync(Transform          referenceTransform,
                                                           bool               parentToReference,
                                                           Vector3            newFloorPosition,
                                                           Quaternion         newRotation,
                                                           UxrTranslationType translationType    = UxrTranslationType.Immediate,
                                                           float              transitionSeconds  = UxrConstants.TeleportTranslationSeconds,
                                                           Action             teleportedCallback = null,
                                                           CancellationToken  ct                 = default,
                                                           bool               propagateEvents    = true)
        {
            Vector3    newRelativeFloorPosition = referenceTransform != null ? referenceTransform.InverseTransformPoint(newFloorPosition) : newFloorPosition;
            Quaternion newRelativeRotation      = referenceTransform != null ? Quaternion.Inverse(referenceTransform.rotation) * newRotation : newRotation;
            bool       hasFinished              = false;

            Coroutine teleportCoroutine = StartCoroutine(TeleportLocalAvatarRelativeCoroutine(referenceTransform, parentToReference, newRelativeFloorPosition, newRelativeRotation, translationType, transitionSeconds, teleportedCallback, () => hasFinished = true, propagateEvents));
            await TaskExt.WaitUntil(() => hasFinished, ct);

            if (ct.IsCancellationRequested)
            {
                StopCoroutine(teleportCoroutine);
            }
        }

        /// <summary>
        ///     Rotates the local avatar around its vertical axis, where a positive angle turns it to the right and a negative
        ///     angle to the left. The rotation can be performed in different ways using <paramref name="rotationType" />.
        /// </summary>
        /// <param name="degrees">The degrees to rotate</param>
        /// <param name="rotationType">The type of rotation to use. By default it will rotate immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="rotationType" /> has a duration, it will specify how long the
        ///     rotation transition will take in seconds. By default it is <see cref="UxrConstants.TeleportRotationSeconds" />
        /// </param>
        /// <param name="rotatedCallback">
        ///     Optional callback executed depending on the rotation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrRotationType.Immediate" />: Right after finishing the rotation.</item>
        ///         <item>
        ///             <see cref="UxrRotationType.Fade" />: When the screen is completely faded out and the avatar has rotated,
        ///             before fading back in. This can be used to enable/disable/change GameObjects in the scene since the screen
        ///             at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrRotationType.Smooth" />: Right after finishing the rotation.</item>
        ///     </list>
        /// </param>
        /// <param name="finishedCallback">
        ///     Optional callback executed right after the teleportation finished. It will receive a boolean parameter telling
        ///     whether the teleport finished completely (true) or was cancelled (false). If a fade effect has been requested, the
        ///     callback is executed right after the screen has faded back in.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <remarks>
        ///     If <see cref="UxrTranslationType.Fade" /> translation mode was specified, the default black fade color can be
        ///     changed using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public void RotateLocalAvatar(float           degrees,
                                      UxrRotationType rotationType      = UxrRotationType.Immediate,
                                      float           transitionSeconds = UxrConstants.TeleportRotationSeconds,
                                      Action          rotatedCallback   = null,
                                      Action<bool>    finishedCallback  = null,
                                      bool            propagateEvents   = true)
        {
            if (_teleportCoroutine != null)
            {
                StopCoroutine(_teleportCoroutine);
            }

            bool hasFinished = false;
            _teleportCoroutine = StartCoroutine(RotateLocalAvatarCoroutine(degrees, rotationType, transitionSeconds, rotatedCallback, () => hasFinished = true, propagateEvents));
            finishedCallback?.Invoke(hasFinished);
        }

        /// <summary>
        ///     <para>Asynchronous version of <see cref="RotateLocalAvatar" />.</para>
        ///     <para>
        ///         Rotates the local avatar around its vertical axis, where a positive angle turns it to the right and a
        ///         negative angle to the left. The rotation can be performed in different ways using
        ///         <paramref name="rotationType" />.
        ///     </para>
        /// </summary>
        /// <param name="degrees">The degrees to rotate</param>
        /// <param name="rotationType">The type of rotation to use. By default it will rotate immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="rotationType" /> has a duration, it will specify how long the
        ///     rotation transition will take in seconds. By default it is <see cref="UxrConstants.TeleportRotationSeconds" />
        /// </param>
        /// <param name="rotatedCallback">
        ///     Optional callback executed depending on the rotation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrRotationType.Immediate" />: Right after finishing the rotation.</item>
        ///         <item>
        ///             <see cref="UxrRotationType.Fade" />: When the screen is completely faded out and the avatar has rotated,
        ///             before fading back in. This can be used to enable/disable/change GameObjects in the scene since the screen
        ///             at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrRotationType.Smooth" />: Right after finishing the rotation.</item>
        ///     </list>
        /// </param>
        /// <param name="ct">Optional cancellation token to cancel the operation</param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Awaitable <see cref="Task" /> that will finish when the rotation finished</returns>
        public async Task RotateLocalAvatarAsync(float             degrees,
                                                 UxrRotationType   rotationType      = UxrRotationType.Immediate,
                                                 float             transitionSeconds = UxrConstants.TeleportRotationSeconds,
                                                 Action            rotatedCallback   = null,
                                                 CancellationToken ct                = default,
                                                 bool              propagateEvents   = true)
        {
            bool finished = false;

            Coroutine rotateCoroutine = StartCoroutine(RotateLocalAvatarCoroutine(degrees, rotationType, transitionSeconds, rotatedCallback, () => finished = true, propagateEvents));
            await TaskExt.WaitUntil(() => finished, ct);

            if (ct.IsCancellationRequested)
            {
                StopCoroutine(rotateCoroutine);
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Subscribes to global events.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            UxrAvatar.GlobalEnabled    += Avatar_Enabled;
            SceneManager.sceneLoaded   += SceneManager_SceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
        }

        /// <summary>
        ///     Unsubscribes from global events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            UxrAvatar.GlobalEnabled    -= Avatar_Enabled;
            SceneManager.sceneLoaded   -= SceneManager_SceneLoaded;
            SceneManager.sceneUnloaded -= SceneManager_SceneUnloaded;

            DestroyPrecachedInstances();
        }

        /// <summary>
        ///     Tries to find Unity canvases (<see cref="Canvas" /> components) and automatically set them up so that they can be
        ///     used by the framework using <see cref="UxrCanvas" />.
        /// </summary>
        protected override void Start()
        {
            SetupCanvases();
        }

        /// <summary>
        ///     Updates the key entities to the current frame. If the <see cref="PostUpdateMode" /> is set to
        ///     <see cref="UxrPostUpdateMode.Update" />, all the animation (hand poses, manipulation mechanics and Inverse
        ///     Kinematics) will also be updated.
        /// </summary>
        private void Update()
        {
            OnUpdating();
            OnUpdatingStage(UxrUpdateStage.Update);

            foreach (UxrAvatarController avatarController in LocalAvatarControllers)
            {
                OnAvatarUpdating(avatarController.Avatar, new UxrAvatarUpdateEventArgs(avatarController.Avatar, UxrUpdateStage.Update));
                ((IUxrAvatarControllerUpdater)avatarController).UpdateAvatar();
                OnAvatarUpdated(avatarController.Avatar, new UxrAvatarUpdateEventArgs(avatarController.Avatar, UxrUpdateStage.Update));
            }

            OnStageUpdated(UxrUpdateStage.Update);

            if (PostUpdateMode == UxrPostUpdateMode.Update)
            {
                PostUpdate();
            }
        }

        /// <summary>
        ///     Updates the key entities to the current frame. If the <see cref="PostUpdateMode" /> is set to
        ///     <see cref="UxrPostUpdateMode.LateUpdate" />, all the animation (hand poses, manipulation mechanics and Inverse
        ///     Kinematics) will also be updated.
        /// </summary>
        private void LateUpdate()
        {
            if (PostUpdateMode == UxrPostUpdateMode.LateUpdate)
            {
                PostUpdate();
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Public teleporting coroutine that can be yielded from an external coroutine.
        ///     Teleports the local <see cref="UxrAvatar" />. The local avatar is the avatar controlled by the user using the
        ///     headset and input controllers. Non-local avatars are other avatars instantiated in the scene but not controlled by
        ///     the user, either other users through the network or other scenarios such as automated replays.
        /// </summary>
        /// <param name="newFloorPosition">
        ///     Floor-level position the avatar will be teleported over. The camera position will be on top of the floor position,
        ///     keeping the original eye-level.
        /// </param>
        /// <param name="newRotation">
        ///     Rotation the avatar will be teleported to. The camera will point in the rotation's forward
        ///     direction
        /// </param>
        /// <param name="translationType">The type of translation to use. By default it will teleport immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="translationType" /> has a duration, it will specify how long the
        ///     teleport transition will take in seconds. By default it is <see cref="UxrConstants.TeleportTranslationSeconds" />
        /// </param>
        /// <param name="teleportedCallback">
        ///     Optional callback executed depending on the teleportation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrTranslationType.Immediate" />: Right after finishing the teleportation.</item>
        ///         <item>
        ///             <see cref="UxrTranslationType.Fade" />: When the screen is completely faded out and the avatar has been
        ///             moved, before fading back in. This can be used to enable/disable/change GameObjects in the scene since the
        ///             screen at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrTranslationType.Smooth" />: Right after finishing the teleportation.</item>
        ///     </list>
        /// </param>
        /// <param name="finishedCallback">
        ///     Optional callback executed right after the teleportation finished. If a fade effect has been requested, the
        ///     callback is executed right after the screen has faded back in.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Coroutine enumerator</returns>
        /// <remarks>
        ///     If <see cref="UxrTranslationType.Fade" /> translation mode was specified, the default black fade color can be
        ///     changed using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public IEnumerator TeleportLocalAvatarCoroutine(Vector3            newFloorPosition,
                                                        Quaternion         newRotation,
                                                        UxrTranslationType translationType    = UxrTranslationType.Immediate,
                                                        float              transitionSeconds  = UxrConstants.TeleportTranslationSeconds,
                                                        Action             teleportedCallback = null,
                                                        Action             finishedCallback   = null,
                                                        bool               propagateEvents    = true)
        {
            yield return TeleportLocalAvatarRelativeCoroutine(null, false, newFloorPosition, newRotation, translationType, transitionSeconds, teleportedCallback, finishedCallback, propagateEvents);
        }

        /// <summary>
        ///     Public teleporting coroutine that can be yielded from an external coroutine.
        ///     Teleports the local <see cref="UxrAvatar" /> while making sure to keep relative position/orientation on moving
        ///     objects. Some <paramref name="translationType" /> values have a transition before the teleport to avoid motion
        ///     sickness. On worlds with moving platforms it is important to specify the destination transform so that:
        ///     <list type="bullet">
        ///         <item>Relative position/orientation to the destination is preserved.</item>
        ///         <item>Optionally the local avatar can be parented to the new destination.</item>
        ///     </list>
        ///     The local avatar is the avatar controlled by the user using the headset and input controllers. Non-local avatars
        ///     are other avatars instantiated in the scene but not controlled by the user, either other users through the network
        ///     or other scenarios such as automated replays.
        /// </summary>
        /// <param name="referenceTransform">
        ///     The object the avatar should keep relative position/orientation to. This should be the moving object the avatar has
        ///     teleported on top of. If null, <paramref name="newRelativeFloorPosition" /> and
        ///     <paramref name="newRelativeRotation" /> will be interpreted as world coordinates.
        /// </param>
        /// <param name="parentToReference">
        ///     Whether to parent the avatar to <paramref name="referenceTransform" />. The avatar should be parented if it's being
        ///     teleported to a moving hierarchy it is not part of
        /// </param>
        /// <param name="newRelativeFloorPosition">
        ///     New floor-level position the avatar will be teleported over in <paramref name="referenceTransform" /> local
        ///     coordinates. If <paramref name="referenceTransform" /> is null, coordinates will be interpreted as being in
        ///     world-space. The camera position will be on top of the floor position, keeping the original eye-level.
        /// </param>
        /// <param name="newRelativeRotation">
        ///     Local rotation the avatar will be teleported to with respect to <see cref="referenceTransform" />. If
        ///     <paramref name="referenceTransform" /> is null, rotation will be in world-space. The camera will point in the
        ///     rotation's forward direction.
        /// </param>
        /// <param name="translationType">The type of translation to use. By default it will teleport immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="translationType" /> has a duration, it will specify how long the
        ///     teleport transition will take in seconds. By default it is <see cref="UxrConstants.TeleportTranslationSeconds" />
        /// </param>
        /// <param name="teleportedCallback">
        ///     Optional callback executed depending on the teleportation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrTranslationType.Immediate" />: Right after finishing the teleportation.</item>
        ///         <item>
        ///             <see cref="UxrTranslationType.Fade" />: When the screen is completely faded out and the avatar has been
        ///             moved, before fading back in. This can be used to enable/disable/change GameObjects in the scene since the
        ///             screen at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrTranslationType.Smooth" />: Right after finishing the teleportation.</item>
        ///     </list>
        /// </param>
        /// <param name="finishedCallback">
        ///     Optional callback executed right after the teleportation finished. If a fade effect has been requested, the
        ///     callback is executed right after the screen has faded back in.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Coroutine enumerator</returns>
        /// <remarks>
        ///     If <see cref="UxrTranslationType.Fade" /> translation mode was specified, the default black fade color can be
        ///     changed using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public IEnumerator TeleportLocalAvatarRelativeCoroutine(Transform          referenceTransform,
                                                                bool               parentToReference,
                                                                Vector3            newRelativeFloorPosition,
                                                                Quaternion         newRelativeRotation,
                                                                UxrTranslationType translationType    = UxrTranslationType.Immediate,
                                                                float              transitionSeconds  = UxrConstants.TeleportTranslationSeconds,
                                                                Action             teleportedCallback = null,
                                                                Action             finishedCallback   = null,
                                                                bool               propagateEvents    = true)
        {
            if (UxrAvatar.LocalAvatar)
            {
                Vector3    oldFloorPosition         = UxrAvatar.LocalAvatar.CameraFloorPosition;
                Quaternion oldFloorRotation         = Quaternion.LookRotation(UxrAvatar.LocalAvatar.ProjectedCameraForward);
                Quaternion inverseReferenceRotation = referenceTransform != null ? Quaternion.Inverse(referenceTransform.rotation) : Quaternion.identity;
                Matrix4x4  inverseReferenceMatrix   = referenceTransform != null ? referenceTransform.localToWorldMatrix.inverse : Matrix4x4.identity;
                Vector3    oldRelativePosition      = inverseReferenceMatrix * oldFloorPosition;
                Quaternion oldRelativeRotation      = inverseReferenceRotation * oldFloorRotation;

                void TranslateAvatarInternal(float t = 1.0f)
                {
                    Vector3    newPos = Vector3.Lerp(oldRelativePosition, newRelativeFloorPosition, t);
                    Quaternion newRot = oldRelativeRotation;

                    if (Mathf.Approximately(t, 1.0f))
                    {
                        newRot = newRelativeRotation;
                    }

                    if (referenceTransform != null)
                    {
                        newPos = referenceTransform.TransformPoint(newPos);
                        newRot = referenceTransform.rotation * newRot;
                    }

                    MoveAvatarTo(UxrAvatar.LocalAvatar, newPos, newRot * Vector3.forward, propagateEvents);
                }

                switch (translationType)
                {
                    case UxrTranslationType.Immediate:

                        TranslateAvatarInternal();
                        teleportedCallback?.Invoke();
                        break;

                    case UxrTranslationType.Fade:

                        yield return UxrAvatar.LocalAvatar.CameraFade.StartFadeCoroutine(transitionSeconds * 0.5f, TeleportFadeColor.WithAlpha(0.0f), TeleportFadeColor.WithAlpha(1.0f));

                        TranslateAvatarInternal();
                        teleportedCallback?.Invoke();
                        yield return null;
                        yield return UxrAvatar.LocalAvatar.CameraFade.StartFadeCoroutine(transitionSeconds * 0.5f, TeleportFadeColor.WithAlpha(1.0f), TeleportFadeColor.WithAlpha(0.0f));

                        break;

                    case UxrTranslationType.Smooth:

                        yield return this.LoopCoroutine(transitionSeconds, TranslateAvatarInternal, UxrEasing.Linear, true);

                        break;
                }

                if (parentToReference && referenceTransform != null)
                {
                    UxrAvatar.LocalAvatar.transform.SetParent(referenceTransform);
                }
            }

            _teleportCoroutine = null;
            finishedCallback?.Invoke();
        }

        /// <summary>
        ///     Public avatar rotation coroutine that can be yielded from an external coroutine.
        ///     Rotates the avatar around its vertical axis, where a positive angle turns it to the right and a negative angle to
        ///     the left.
        /// </summary>
        /// <param name="degrees">The degrees to rotate</param>
        /// <param name="rotationType">The type of rotation to use. By default it will rotate immediately</param>
        /// <param name="transitionSeconds">
        ///     If <paramref name="rotationType" /> has a duration, it will specify how long the
        ///     rotation transition will take in seconds. By default it is <see cref="UxrConstants.TeleportRotationSeconds" />
        /// </param>
        /// <param name="rotatedCallback">
        ///     Optional callback executed depending on the rotation mode:
        ///     <list type="bullet">
        ///         <item><see cref="UxrRotationType.Immediate" />: Right after finishing the rotation.</item>
        ///         <item>
        ///             <see cref="UxrRotationType.Fade" />: When the screen is completely faded out and the avatar has rotated,
        ///             before fading back in. This can be used to enable/disable/change GameObjects in the scene since the screen
        ///             at this point is fully rendered using the fade color.
        ///         </item>
        ///         <item><see cref="UxrRotationType.Smooth" />: Right after finishing the rotation.</item>
        ///     </list>
        /// </param>
        /// <param name="finishedCallback">
        ///     Optional callback executed right after the rotation finished. If a fade effect has been requested, the callback is
        ///     executed right after the screen has faded back in.
        /// </param>
        /// <param name="propagateEvents">Whether to propagate <see cref="AvatarMoving" />/<see cref="AvatarMoved" /> events</param>
        /// <returns>Coroutine enumerator</returns>
        /// <remarks>
        ///     If <see cref="UxrRotationType.Fade" /> translation mode was specified, the default black fade color can be changed
        ///     using <see cref="TeleportFadeColor" />.
        /// </remarks>
        public IEnumerator RotateLocalAvatarCoroutine(float           degrees,
                                                      UxrRotationType rotationType      = UxrRotationType.Immediate,
                                                      float           transitionSeconds = UxrConstants.TeleportRotationSeconds,
                                                      Action          rotatedCallback   = null,
                                                      Action          finishedCallback  = null,
                                                      bool            propagateEvents   = true)
        {
            if (UxrAvatar.LocalAvatar)
            {
                void RotateAvatarInternal(float t = 1.0f)
                {
                    Transform avatarTransform = UxrAvatar.LocalAvatar.transform;
                    Vector3   initialForward  = UxrAvatar.LocalAvatar.ProjectedCameraForward;

                    MoveAvatarTo(UxrAvatar.LocalAvatar, UxrAvatar.LocalAvatar.CameraFloorPosition, initialForward.GetRotationAround(avatarTransform.up, degrees * t), propagateEvents);
                }

                switch (rotationType)
                {
                    case UxrRotationType.Immediate:

                        RotateAvatarInternal();
                        rotatedCallback?.Invoke();
                        break;

                    case UxrRotationType.Fade:

                        yield return UxrAvatar.LocalAvatar.CameraFade.StartFadeCoroutine(transitionSeconds * 0.5f, TeleportFadeColor.WithAlpha(0.0f), TeleportFadeColor.WithAlpha(1.0f));

                        RotateAvatarInternal();
                        rotatedCallback?.Invoke();
                        yield return null;
                        yield return UxrAvatar.LocalAvatar.CameraFade.StartFadeCoroutine(transitionSeconds * 0.5f, TeleportFadeColor.WithAlpha(1.0f), TeleportFadeColor.WithAlpha(0.0f));

                        break;

                    case UxrRotationType.Smooth:

                        yield return this.LoopCoroutine(transitionSeconds, RotateAvatarInternal, UxrEasing.Linear, true);

                        break;
                }
            }

            _teleportCoroutine = null;
            finishedCallback?.Invoke();
        }

        /// <summary>
        ///     Precaching coroutine. It will try to find all <see cref="IUxrPrecacheable" /> components in the scene and
        ///     pre-instantiate their objects in front of the camera while the screen is still faded black.
        ///     The goal is to make sure all resources (meshes, textures) are in memory so that when they are instantiated at any
        ///     point, the resources are already available lowering the chances of rendering hiccups.
        ///     The scene is rendered black on top during a pre-determined number of frames (<see cref="PrecacheFrameCount" />)
        ///     after which the pre-instantiated objects will be destroyed and the scene will be faded in.
        /// </summary>
        /// <param name="onStarting">Optional callback called when precaching is right about to start</param>
        /// <param name="onFinished">Optional callback called right after precaching finished</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator PrecacheCoroutine(Action onStarting = null, Action onFinished = null)
        {
            UxrAvatar avatar = UxrAvatar.LocalAvatar;

            while (avatar == null)
            {
                yield return null;

                avatar = UxrAvatar.LocalAvatar;
            }
            
            onStarting?.Invoke();

            DestroyPrecachedInstances();

            _dynamicInstances = new Dictionary<int, GameObject>();

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                AddScenePrecachedInstances(_dynamicInstances, scene, avatar);
            }

            AddScenePrecachedInstances(_dynamicInstances, Instance.gameObject.scene, avatar);

            for (int frame = 0; frame < _precacheFrameCount; ++frame)
            {
                avatar.CameraFade.EnableFadeColor(Color.black, 1.0f);
                yield return null;
            }

            DestroyPrecachedInstances();

            onFinished?.Invoke();

            float startFadeTime = Time.time;
            float fadeDuration  = 0.5f;

            while (Time.time - startFadeTime < fadeDuration)
            {
                avatar.CameraFade.EnableFadeColor(Color.black, 1.0f - (Time.time - startFadeTime) / fadeDuration);
                yield return null;
            }

            avatar.CameraFade.DisableFadeColor();

            _precacheCoroutine = null;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when an <see cref="UxrAvatar" /> is enabled. If the avatar is the local avatar, it is used as a signal to
        ///     set up canvases in the scene and start the pre-caching process.
        /// </summary>
        /// <param name="avatar">Avatar that was enabled</param>
        private void Avatar_Enabled(UxrAvatar avatar)
        {
            if (avatar.AvatarMode == UxrAvatarMode.Local)
            {
                if (UxrPointerInputModule.Instance != null && UxrPointerInputModule.Instance.AutoAssignEventCamera)
                {
                    foreach (UxrCanvas canvas in UxrCanvas.AllComponents)
                    {
                        if (canvas.UnityCanvas)
                        {
                            canvas.UnityCanvas.worldCamera = avatar.CameraComponent;
                        }
                    }
                }

                TryPrecaching();
            }
        }

        /// <summary>
        ///     Called when a Unity scene was loaded. It is used to try to automatically set up the canvases in the scene so that
        ///     they can be used with UltimateXR.
        /// </summary>
        /// <param name="scene">Scene that was loaded.</param>
        /// <param name="loadSceneMode">The mode used to load the scene.</param>
        private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SetupCanvases();
        }

        /// <summary>
        ///     Called when a Unity scene was unloaded. It is used to try to automatically set up the canvases in the scene so that
        ///     they can be used with UltimateXR.
        /// </summary>
        /// <param name="scene">Scene that was unloaded.</param>
        private void SceneManager_SceneUnloaded(Scene scene)
        {
            SetupCanvases();
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     <see cref="PrecachingStarting"/> event trigger.
        /// </summary>
        private void OnPrecachingStarting()
        {
            PrecachingStarting?.Invoke();
        }

        /// <summary>
        ///     <see cref="PrecachingFinished"/> event trigger.
        /// </summary>
        private void OnPrecachingFinished()
        {
            PrecachingFinished?.Invoke();
        }

        /// <summary>
        ///     <see cref="AvatarMoving" /> event trigger.
        /// </summary>
        /// <param name="args">Event parameters</param>
        private void OnAvatarMoving(UxrAvatarMoveEventArgs args)
        {
            AvatarMoving?.Invoke(this, args);
        }

        /// <summary>
        ///     <see cref="AvatarMoved" /> event trigger.
        /// </summary>
        /// <param name="args">Event parameters</param>
        private void OnAvatarMoved(UxrAvatarMoveEventArgs args)
        {
            AvatarMoved?.Invoke(this, args);
            args.Avatar.NotifyAvatarMoved(args);
        }

        /// <summary>
        ///     <see cref="AvatarsUpdating" /> event trigger.
        /// </summary>
        private void OnUpdating()
        {
            AvatarsUpdating?.Invoke();
        }

        /// <summary>
        ///     <see cref="AvatarsUpdated" /> event trigger.
        /// </summary>
        private void OnUpdated()
        {
            AvatarsUpdated?.Invoke();
        }

        /// <summary>
        ///     <see cref="StageUpdating" /> event trigger.
        /// </summary>
        private void OnUpdatingStage(UxrUpdateStage stage)
        {
            StageUpdating?.Invoke(stage);
        }

        /// <summary>
        ///     <see cref="StageUpdated" /> event trigger.
        /// </summary>
        private void OnStageUpdated(UxrUpdateStage stage)
        {
            StageUpdated?.Invoke(stage);
        }

        /// <summary>
        ///     <see cref="UxrAvatar.AvatarUpdating">UxrAvatar.AvatarUpdating</see> event trigger.
        /// </summary>
        private void OnAvatarUpdating(UxrAvatar avatar, UxrAvatarUpdateEventArgs e)
        {
            avatar.RaiseAvatarUpdating(e);
        }

        /// <summary>
        ///     <see cref="UxrAvatar.AvatarUpdated">UxrAvatar.AvatarUpdated</see> event trigger.
        /// </summary>
        private void OnAvatarUpdated(UxrAvatar avatar, UxrAvatarUpdateEventArgs e)
        {
            avatar.RaiseAvatarUpdated(e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Performs the post-update: Updates the animation and interaction of all key entities, while sending all related
        ///     events during the process.
        ///     Main updates are:
        ///     <list type="bullet">
        ///         <item>Avatar animation.</item>
        ///         <item>Manipulation mechanics and constraints.</item>
        ///         <item>Other managers in the framework such as the <see cref="UxrWeaponManager" />.</item>
        ///         <item>Inverse kinematics.</item>
        ///     </list>
        /// </summary>
        private void PostUpdate()
        {
            // Avatar bones that are tracked
            
            OnUpdatingStage(UxrUpdateStage.AvatarUsingTracking);

            foreach (UxrAvatar avatar in UxrAvatar.EnabledComponents)
            {
                if (avatar.AvatarMode == UxrAvatarMode.Local && avatar.AvatarController is { enabled: true } avatarController)
                {
                    OnAvatarUpdating(avatar, new UxrAvatarUpdateEventArgs(avatar, UxrUpdateStage.AvatarUsingTracking));
                    ((IUxrAvatarControllerUpdater)avatarController).UpdateAvatarUsingTrackingDevices();
                    OnAvatarUpdated(avatar, new UxrAvatarUpdateEventArgs(avatar, UxrUpdateStage.AvatarUsingTracking));
                }
            }

            OnStageUpdated(UxrUpdateStage.AvatarUsingTracking);

            // Update managers

            OnUpdatingStage(UxrUpdateStage.Manipulation);

            foreach (UxrAvatarController avatarController in LocalAvatarControllers)
            {
                OnAvatarUpdating(avatarController.Avatar, new UxrAvatarUpdateEventArgs(avatarController.Avatar, UxrUpdateStage.Manipulation));
            }

            UxrGrabManager.Instance.UpdateManager();
            UxrWeaponManager.Instance.UpdateManager();

            foreach (UxrAvatarController avatarController in LocalAvatarControllers)
            {
                ((IUxrAvatarControllerUpdater)avatarController).UpdateAvatarManipulation();
            }

            foreach (UxrAvatarController avatarController in LocalAvatarControllers)
            {
                OnAvatarUpdated(avatarController.Avatar, new UxrAvatarUpdateEventArgs(avatarController.Avatar, UxrUpdateStage.Manipulation));
            }

            OnStageUpdated(UxrUpdateStage.Manipulation);

            // Update animation

            OnUpdatingStage(UxrUpdateStage.Animation);

            foreach (UxrAvatar avatar in UxrAvatar.EnabledComponents)
            {
                if (avatar.AvatarMode == UxrAvatarMode.Local && avatar.AvatarController is { enabled: true } avatarController)
                {
                    OnAvatarUpdating(avatar, new UxrAvatarUpdateEventArgs(avatar, UxrUpdateStage.Animation));
                    ((IUxrAvatarControllerUpdater)avatarController).UpdateAvatarAnimation();
                    OnAvatarUpdated(avatar, new UxrAvatarUpdateEventArgs(avatar, UxrUpdateStage.Animation));
                }
                else if (avatar.AvatarMode == UxrAvatarMode.UpdateExternally)
                {
                    // This makes sure that hand poses are updated 
                    OnAvatarUpdating(avatar, new UxrAvatarUpdateEventArgs(avatar, UxrUpdateStage.Animation));
                    avatar.UpdateHandPoseTransforms();
                    OnAvatarUpdated(avatar, new UxrAvatarUpdateEventArgs(avatar, UxrUpdateStage.Animation));
                }
            }

            OnStageUpdated(UxrUpdateStage.Animation);

            // Update post-process

            OnUpdatingStage(UxrUpdateStage.PostProcess);

            foreach (UxrAvatarController avatarController in LocalAvatarControllers)
            {
                OnAvatarUpdating(avatarController.Avatar, new UxrAvatarUpdateEventArgs(avatarController.Avatar, UxrUpdateStage.PostProcess));
                ((IUxrAvatarControllerUpdater)avatarController).UpdateAvatarPostProcess();
                avatarController.Avatar.AvatarRigInfo.UpdateInfo();
                OnAvatarUpdated(avatarController.Avatar, new UxrAvatarUpdateEventArgs(avatarController.Avatar, UxrUpdateStage.PostProcess));
            }

            OnStageUpdated(UxrUpdateStage.PostProcess);
            OnUpdated();
        }

        /// <summary>
        ///     Processes all <see cref="IUxrPrecacheable" /> components in a scene and instantiates all required prefabs in front
        ///     of the avatar camera. The goal is to make sure all their resources are loaded into memory afterwards.
        /// </summary>
        /// <param name="dynamicInstances">List of loaded instances.</param>
        /// <param name="scene">Scene to get the components from.</param>
        /// <param name="avatar">Current avatar.</param>
        private void AddScenePrecachedInstances(Dictionary<int, GameObject> dynamicInstances, Scene scene, UxrAvatar avatar)
        {
            for (int rootIndex = 0; rootIndex < scene.rootCount; ++rootIndex)
            {
                MonoBehaviour[] behaviours = scene.GetRootGameObjects()[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);

                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; ++behaviourIndex)
                {
                    if (behaviours[behaviourIndex] is IUxrPrecacheable)
                    {
                        IUxrPrecacheable precacheable = behaviours[behaviourIndex] as IUxrPrecacheable;

                        foreach (GameObject precachedInstance in precacheable.PrecachedInstances)
                        {
                            if (precachedInstance != null && dynamicInstances.ContainsKey(precachedInstance.GetInstanceID()) == false)
                            {
                                // Instantiate
                                GameObject dynamicInstance = Instantiate(precachedInstance,
                                                                         avatar.CameraTransform.position + avatar.CameraTransform.forward * 5.0f,
                                                                         avatar.CameraTransform.rotation,
                                                                         Instance.transform);

                                dynamicInstances.Add(precachedInstance.GetInstanceID(), dynamicInstance);

                                // Avoid sounds
                                AudioSource[] audioSources = dynamicInstance.GetComponentsInChildren<AudioSource>(true);
                                for (int audio = 0; audio < audioSources.Length; ++audio)
                                {
                                    audioSources[audio].enabled = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Destroys the currently loaded pre-cached instances.
        /// </summary>
        private void DestroyPrecachedInstances()
        {
            if (_dynamicInstances != null)
            {
                foreach (KeyValuePair<int, GameObject> dynamicInstancePair in _dynamicInstances)
                {
                    if (dynamicInstancePair.Value != null)
                    {
                        Destroy(dynamicInstancePair.Value);
                    }
                }

                _dynamicInstances.Clear();
            }
        }

        /// <summary>
        ///     Starts the pre-caching process. If a pre-caching process is currently running, it will be stopped before starting
        ///     again.
        /// </summary>
        private void TryPrecaching()
        {
            if (_precacheCoroutine != null)
            {
                StopCoroutine(_precacheCoroutine);
            }

            if (UsePrecaching)
            {
                _precacheCoroutine = StartCoroutine(PrecacheCoroutine(OnPrecachingStarting, OnPrecachingFinished));
            }
        }

        /// <summary>
        ///     Tries to set up all <see cref="Canvas" /> components currently in the scene so that they can work with UltimateXR
        ///     through the <see cref="UxrCanvas" /> component.
        /// </summary>
        private void SetupCanvases()
        {
            if (UxrPointerInputModule.Instance)
            {
                foreach (Canvas canvas in ComponentExt.GetAllComponentsInOpenScenes<Canvas>(true))
                {
                    if (canvas.renderMode == RenderMode.WorldSpace && canvas.GetComponent<UxrIgnoreCanvas>() == null)
                    {
                        if (!canvas.TryGetComponent<UxrCanvas>(out var canvasXR))
                        {
                            if (UxrPointerInputModule.Instance.AutoEnableOnWorldCanvases)
                            {
                                canvasXR = canvas.gameObject.AddComponent<UxrCanvas>();
                                canvasXR.SetupCanvas(UxrPointerInputModule.Instance);
                            }
                        }

                        if (canvasXR && UxrPointerInputModule.Instance.AutoAssignEventCamera && UxrAvatar.LocalAvatar)
                        {
                            canvas.worldCamera = UxrAvatar.LocalAvatarCamera;
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the enabled <see cref="UxrAvatarController" /> components that belong to enabled <see cref="UxrAvatar" />
        ///     components whose <see cref="UxrAvatar.AvatarMode" /> is <see cref="UxrAvatarMode.Local" />. This should be either
        ///     none or one. The property allows to iterate over avatar controllers that require updating each frame.
        /// </summary>
        private IEnumerable<UxrAvatarController> LocalAvatarControllers
        {
            get
            {
                foreach (UxrAvatar avatar in UxrAvatar.EnabledComponents)
                {
                    if (avatar.AvatarMode == UxrAvatarMode.Local && avatar.AvatarController != null && avatar.AvatarController.enabled)
                    {
                        yield return avatar.AvatarController;
                    }
                }
            }
        }

        private Coroutine                   _precacheCoroutine;
        private Dictionary<int, GameObject> _dynamicInstances;
        private Coroutine                   _teleportCoroutine;

        #endregion
    }
}