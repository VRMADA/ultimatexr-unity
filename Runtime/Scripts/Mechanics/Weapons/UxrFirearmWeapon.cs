// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFirearmWeapon.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Devices;
using UltimateXR.Manipulation;
using UnityEngine;

namespace UltimateXR.Mechanics.Weapons
{
    /// <summary>
    ///     Type of weapon that shoots projectiles. A firearm has one or more <see cref="UxrFirearmTrigger" /> entries. Each
    ///     trigger allows to shoot a different type of projectile, and determines properties such as the shot cycle, shot
    ///     frequency, ammunition, recoil and grabbing.
    ///     A <see cref="UxrFirearmWeapon" /> requires a <see cref="UxrProjectileSource" /> component that defines the
    ///     projectiles being shot. If a firearm has more than one trigger (for instance, a rifle that shoots bullets and has a
    ///     grenade launcher), the <see cref="UxrProjectileSource" /> will require the same amount of entries in
    ///     <see cref="UxrProjectileSource.ShotTypes" />.
    /// </summary>
    [RequireComponent(typeof(UxrProjectileSource))]
    public partial class UxrFirearmWeapon : UxrWeapon
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] protected Transform               _recoilAxes;
        [SerializeField] private   List<UxrFirearmTrigger> _triggers;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called right after the weapon shot a projectile using the given trigger index.
        /// </summary>
        public event Action<int> ProjectileShot;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks whether a trigger is in a loaded state, meaning it is ready to shoot if pressed and there is any ammo left.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <returns>Whether it is ready to shoot</returns>
        public bool IsLoaded(int triggerIndex)
        {
            if (_runtimeTriggers.TryGetValue(triggerIndex, out RuntimeTriggerInfo runtimeTrigger))
            {
                return runtimeTrigger.HasReloaded;
            }

            return false;
        }

        /// <summary>
        ///     Sets the given weapon trigger loaded state so that it is ready to shoot if there is ammo left.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        public void Reload(int triggerIndex)
        {
            if (!_runtimeTriggers.TryGetValue(triggerIndex, out RuntimeTriggerInfo runtimeTrigger))
            {
                return;
            }

            BeginSync();
            runtimeTrigger.HasReloaded = true;
            EndSyncMethod(new object[] { triggerIndex });
        }

        /// <summary>
        ///     Checks whether there is a magazine attached that fires shots using the given trigger. It may or may not have ammo.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <returns>Whether there is a magazine attached</returns>
        public bool HasMagAttached(int triggerIndex)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return false;
            }

            UxrFirearmTrigger trigger = _triggers[triggerIndex];
            return trigger.AmmunitionMagAnchor != null && trigger.AmmunitionMagAnchor.CurrentPlacedObject != null;
        }

        /// <summary>
        ///     Gets the attached magazine maximum capacity.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <returns>Maximum capacity of ammo in the attached magazine. If there isn't any magazine attached it returns 0</returns>
        public int GetAmmoCapacity(int triggerIndex)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return 0;
            }

            UxrFirearmTrigger trigger = _triggers[triggerIndex];

            if (trigger.AmmunitionMagAnchor != null && trigger.AmmunitionMagAnchor.CurrentPlacedObject != null)
            {
                UxrFirearmMag mag = trigger.AmmunitionMagAnchor.CurrentPlacedObject.GetCachedComponent<UxrFirearmMag>();
                return mag != null ? mag.Capacity : 0;
            }

            return 0;
        }

        /// <summary>
        ///     Gets the ammo left in the attached magazine.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <returns>Ammo left in the attached magazine. If there isn't any magazine attached it returns 0</returns>
        public int GetAmmoLeft(int triggerIndex)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return 0;
            }

            UxrFirearmTrigger trigger = _triggers[triggerIndex];

            if (trigger.AmmunitionMagAnchor != null)
            {
                if (trigger.AmmunitionMagAnchor.CurrentPlacedObject != null)
                {
                    UxrFirearmMag mag = trigger.AmmunitionMagAnchor.CurrentPlacedObject.GetCachedComponent<UxrFirearmMag>();

                    return mag != null ? mag.Rounds : 0;
                }
            }
            else
            {
                return int.MaxValue;
            }

            return 0;
        }

        /// <summary>
        ///     Sets the ammo left in the attached magazine.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <param name="ammo">New ammo</param>
        public void SetAmmoLeft(int triggerIndex, int ammo)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return;
            }

            UxrFirearmTrigger trigger = _triggers[triggerIndex];

            if (trigger.AmmunitionMagAnchor != null && trigger.AmmunitionMagAnchor.CurrentPlacedObject != null)
            {
                UxrFirearmMag mag = trigger.AmmunitionMagAnchor.CurrentPlacedObject.GetCachedComponent<UxrFirearmMag>();

                if (mag != null)
                {
                    mag.Rounds = ammo;
                }
            }
        }

        /// <summary>
        ///     Sets the trigger pressed amount.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <param name="amount">Pressed amount between range [0.0, 1.0]</param>
        public void SetTriggerPressedAmount(int triggerIndex, float amount)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return;
            }

            if (!_runtimeTriggers.TryGetValue(triggerIndex, out RuntimeTriggerInfo runtimeTrigger))
            {
                return;
            }

            UxrFirearmTrigger trigger = _triggers[triggerIndex];

            if (trigger.TriggerTransform)
            {
                trigger.TriggerTransform.localRotation = runtimeTrigger.TriggerInitialLocalRotation * Quaternion.AngleAxis(trigger.TriggerRotationDegrees * amount, trigger.TriggerRotationAxis);
            }
        }

        /// <summary>
        ///     Tries to shoot a round using the given trigger.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <returns>
        ///     Whether a round was shot. If no round was shot it can mean that:
        ///     <list type="bullet">
        ///         <item>The trigger index references an entry that doesn't exist.</item>
        ///         <item>The firearm isn't loaded.</item>
        ///         <item>The firearm doesn't have any ammo left or there is no magazine attached.</item>
        ///         <item>The shoot frequency doesn't allow to shoot again so quickly.</item>
        ///     </list>
        /// </returns>
        public bool TryToShootRound(int triggerIndex)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return false;
            }

            if (!_runtimeTriggers.TryGetValue(triggerIndex, out RuntimeTriggerInfo runtimeTrigger))
            {
                return false;
            }

            UxrFirearmTrigger trigger = _triggers[triggerIndex];

            if (GetAmmoLeft(triggerIndex) > 0 && runtimeTrigger.LastShotTimer <= 0.0f)
            {
                SetAmmoLeft(triggerIndex, GetAmmoLeft(triggerIndex) - 1);

                runtimeTrigger.LastShotTimer = trigger.MaxShotFrequency > 0 ? 1.0f / trigger.MaxShotFrequency : -1.0f;

                // TODO: here we probably should add some randomization depending on recoil using the additional optional parameters
                _weaponSource.Shoot(trigger.ProjectileShotIndex);

                runtimeTrigger.RecoilTimer = trigger.RecoilDurationSeconds;

                // Audio

                trigger.ShotAudio?.Play(_weaponSource.GetShotOrigin(trigger.ProjectileShotIndex));

                // Raise events

                OnProjectileShot(triggerIndex);
                return true;
            }

            return false;
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < _triggers.Count; ++i)
            {
                if (!_runtimeTriggers.ContainsKey(i))
                {
                    _runtimeTriggers.Add(i, new RuntimeTriggerInfo());
                }
            }
        }

        /// <summary>
        ///     Subscribes to events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (RootGrabbable)
            {
                RootGrabbable.ConstraintsApplied += RootGrabbable_ConstraintsApplied;
            }

            UxrManager.AvatarsUpdated += UxrManager_AvatarsUpdated;

            foreach (UxrFirearmTrigger trigger in _triggers)
            {
                if (trigger.TriggerGrabbable != null)
                {
                    trigger.TriggerGrabbable.Released += Trigger_Released;
                    trigger.TriggerGrabbable.Placed   += Trigger_Placed;
                }

                if (trigger.AmmunitionMagAnchor != null)
                {
                    trigger.AmmunitionMagAnchor.Placed  += MagTarget_Placed;
                    trigger.AmmunitionMagAnchor.Removed += MagTarget_Removed;
                }
            }
        }

        /// <summary>
        ///     Unsubscribes from events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (RootGrabbable)
            {
                RootGrabbable.ConstraintsApplied -= RootGrabbable_ConstraintsApplied;
            }

            UxrManager.AvatarsUpdated -= UxrManager_AvatarsUpdated;

            foreach (UxrFirearmTrigger trigger in _triggers)
            {
                if (trigger.TriggerGrabbable != null)
                {
                    trigger.TriggerGrabbable.Released -= Trigger_Released;
                    trigger.TriggerGrabbable.Placed   -= Trigger_Placed;
                }

                if (trigger.AmmunitionMagAnchor != null)
                {
                    trigger.AmmunitionMagAnchor.Placed  -= MagTarget_Placed;
                    trigger.AmmunitionMagAnchor.Removed -= MagTarget_Removed;
                }
            }
        }

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _weaponSource = GetCachedComponent<UxrProjectileSource>();

            for (int i = 0; i < _triggers.Count; i++)
            {
                UxrFirearmTrigger trigger = _triggers[i];

                if (_runtimeTriggers.TryGetValue(i, out RuntimeTriggerInfo info))
                {
                    info.LastShotTimer = -1.0f;
                    info.HasReloaded   = true;

                    if (trigger.TriggerTransform)
                    {
                        info.TriggerInitialLocalRotation = trigger.TriggerTransform.localRotation;
                    }
                }

                if (trigger.AmmunitionMagAnchor != null)
                {
                    if (trigger.AmmunitionMagAnchor.CurrentPlacedObject != null)
                    {
                        // Disable mag collider while it is attached

                        Collider magCollider = trigger.AmmunitionMagAnchor.CurrentPlacedObject.GetComponentInChildren<Collider>();

                        if (magCollider != null)
                        {
                            magCollider.enabled = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called when the grip of a grabbable object for a given trigger was released.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event parameters</param>
        private void Trigger_Released(object sender, UxrManipulationEventArgs e)
        {
            if (e.Grabber != null && e.Grabber.Avatar.AvatarMode == UxrAvatarMode.Local)
            {
                SyncAmmoLeft(e.GrabbableObject);
            }
        }

        /// <summary>
        ///     Called when the grip of a grabbable object for a given trigger was placed.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event parameters</param>
        private void Trigger_Placed(object sender, UxrManipulationEventArgs e)
        {
            if (e.Grabber != null && e.Grabber.Avatar.AvatarMode == UxrAvatarMode.Local)
            {
                SyncAmmoLeft(e.GrabbableObject);
            }
        }

        /// <summary>
        ///     Called after the avatars have been updated. Updates the hand trigger blend value.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            for (int i = 0; i < _triggers.Count; i++)
            {
                UxrFirearmTrigger trigger = _triggers[i];

                if (!_runtimeTriggers.TryGetValue(i, out RuntimeTriggerInfo runtimeTrigger))
                {
                    continue;
                }

                // Check if we are grabbing the given grip using the local avatar

                if (trigger.TriggerGrabbable && UxrGrabManager.Instance.GetGrabbingHand(trigger.TriggerGrabbable, trigger.GrabbableGrabPointIndex, out UxrGrabber grabber))
                {
                    if (grabber.Avatar.AvatarMode == UxrAvatarMode.Local)
                    {
                        // Get the trigger press amount and use it to send it to the animation var that controls the hand trigger. Use it to rotate the trigger as well.

                        float triggerPressAmount = UxrAvatar.LocalAvatarInput.GetInput1D(grabber.Side, UxrInput1D.Trigger);
                        trigger.TriggerGrabbable.GetGrabPoint(trigger.GrabbableGrabPointIndex).GetGripPoseInfo(grabber.Avatar).PoseBlendValue = triggerPressAmount;
                        SetTriggerPressedAmount(i, triggerPressAmount);

                        // Now depending on the weapon type check if we need to shoot

                        SyncTriggerPressStates(i,
                                               UxrAvatar.LocalAvatarInput.GetButtonsPress(grabber.Side, UxrInputButtons.Trigger),
                                               UxrAvatar.LocalAvatarInput.GetButtonsPressDown(grabber.Side, UxrInputButtons.Trigger),
                                               UxrAvatar.LocalAvatarInput.GetButtonsPressUp(grabber.Side, UxrInputButtons.Trigger));
                    }
                    else
                    {
                        // Remote avatars will get the trigger pressed amount from the avatar pose blend amount, because poses are synchronized. 
                        SetTriggerPressedAmount(i, grabber.Avatar.GetCurrentHandPoseBlendValue(grabber.Side));
                    }

                    bool shoot = false;

                    switch (trigger.CycleType)
                    {
                        case UxrShotCycle.ManualReload:
                        {
                            shoot = runtimeTrigger.TriggerPressStarted && runtimeTrigger.HasReloaded;

                            if (shoot)
                            {
                                runtimeTrigger.HasReloaded = false;
                            }
                            break;
                        }

                        case UxrShotCycle.SemiAutomatic:

                            shoot = runtimeTrigger.TriggerPressStarted;
                            break;

                        case UxrShotCycle.FullyAutomatic:

                            shoot = runtimeTrigger.TriggerPressed;
                            break;
                    }

                    if (runtimeTrigger.TriggerPressStarted && GetAmmoLeft(i) == 0)
                    {
                        trigger.ShotAudioNoAmmo?.Play(trigger.TriggerTransform != null ? trigger.TriggerTransform.position : trigger.TriggerGrabbable.GetGrabPointGrabProximityTransform(grabber, trigger.GrabbableGrabPointIndex).position);
                    }

                    if (shoot)
                    {
                        // Shoot!

                        if (TryToShootRound(i))
                        {
                            if (grabber.Avatar.AvatarMode == UxrAvatarMode.Local)
                            {
                                // Send haptic to the hand grabbing the grip

                                UxrAvatar.LocalAvatarInput.SendHapticFeedback(grabber.Side, trigger.ShotHapticClip);

                                // Send haptic to the other hand if it is also grabbing the weapon

                                if (UxrGrabManager.Instance.IsHandGrabbing(grabber.Avatar, trigger.TriggerGrabbable, grabber.OppositeSide, true))
                                {
                                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(grabber.OppositeSide, trigger.ShotHapticClip);
                                }
                            }
                        }
                    }

                    if (grabber.Avatar.AvatarMode == UxrAvatarMode.Local && runtimeTrigger.TriggerPressEnded)
                    {
                        // Sync ammo after shooting a fully automatic weapon to make sure the ammo left is the same.
                        if (trigger.CycleType == UxrShotCycle.FullyAutomatic)
                        {
                            SyncAmmoLeft(i, GetAmmoLeft(i));
                        }
                    }
                }

                runtimeTrigger.TriggerPressStarted = false;
                runtimeTrigger.TriggerPressEnded   = false;

                if (runtimeTrigger.LastShotTimer > 0.0f)
                {
                    runtimeTrigger.LastShotTimer -= Time.deltaTime;
                }

                if (runtimeTrigger.RecoilTimer > 0.0f)
                {
                    runtimeTrigger.RecoilTimer -= Time.deltaTime;
                }
            }
        }

        /// <summary>
        ///     Called when a mag is removed.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void MagTarget_Removed(object sender, UxrManipulationEventArgs e)
        {
            foreach (UxrFirearmTrigger trigger in _triggers)
            {
                if (e.GrabbableAnchor == trigger.AmmunitionMagAnchor)
                {
                    Collider magCollider = e.GrabbableObject.GetComponentInChildren<Collider>();

                    if (magCollider != null)
                    {
                        magCollider.enabled = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Called when a mag is attached.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void MagTarget_Placed(object sender, UxrManipulationEventArgs e)
        {
            foreach (UxrFirearmTrigger trigger in _triggers)
            {
                if (e.GrabbableAnchor == trigger.AmmunitionMagAnchor)
                {
                    Collider magCollider = e.GrabbableObject.GetComponentInChildren<Collider>();

                    if (magCollider != null)
                    {
                        magCollider.enabled = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Called right after applying constraints to the main grabbable object. It is used to apply the recoil after the
        ///     constraints to do it in the appropriate order.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event parameters</param>
        private void RootGrabbable_ConstraintsApplied(object sender, UxrApplyConstraintsEventArgs e)
        {
            // Get the grabbable object and apply recoil depending on the number of hands that are grabbing the gun

            UxrGrabbableObject grabbableObject    = sender as UxrGrabbableObject;
            Transform          grabbableTransform = grabbableObject.transform;
            int                grabbingHandCount  = UxrGrabManager.Instance.GetGrabbingHandCount(grabbableObject);

            for (int i = 0; i < _triggers.Count; i++)
            {
                UxrFirearmTrigger trigger = _triggers[i];
                
                if (_runtimeTriggers.TryGetValue(i, out RuntimeTriggerInfo runtimeTrigger) && runtimeTrigger.RecoilTimer > 0.0f)
                {
                    float recoilT = trigger.RecoilDurationSeconds > 0.0f ? (trigger.RecoilDurationSeconds - runtimeTrigger.RecoilTimer) / trigger.RecoilDurationSeconds : 0.0f;

                    Vector3 recoilRight    = _recoilAxes != null ? _recoilAxes.right : grabbableTransform.right;
                    Vector3 recoilUp       = _recoilAxes != null ? _recoilAxes.up : grabbableTransform.up;
                    Vector3 recoilForward  = _recoilAxes != null ? _recoilAxes.forward : grabbableTransform.forward;
                    Vector3 recoilPosition = _recoilAxes != null ? _recoilAxes.position : grabbableTransform.position;

                    float   amplitude    = 1.0f - recoilT;
                    Vector3 recoilOffset = grabbingHandCount == 1 ? amplitude * trigger.RecoilOffsetOneHand : amplitude * trigger.RecoilOffsetTwoHands;

                    grabbableTransform.position += recoilRight * recoilOffset.x + recoilUp * recoilOffset.y + recoilForward * recoilOffset.z;
                    grabbableTransform.RotateAround(recoilPosition, -recoilRight, grabbingHandCount == 1 ? amplitude * trigger.RecoilAngleOneHand : amplitude * trigger.RecoilAngleTwoHands);
                }
            }
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="ProjectileShot" />.
        /// </summary>
        /// <param name="triggerIndex">The weapon trigger index</param>
        protected virtual void OnProjectileShot(int triggerIndex)
        {
            ProjectileShot?.Invoke(triggerIndex);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sets the trigger pressed state, to sync multiplayer.
        /// </summary>
        /// <param name="triggerIndex">The trigger index</param>
        /// <param name="pressed">Whether the trigger is pressed</param>
        /// <param name="pressDown">Whether the trigger just started being pressed down</param>
        /// <param name="pressUp">Whether the trigger just started being released</param>
        private void SyncTriggerPressStates(int triggerIndex, bool pressed, bool pressDown, bool pressUp)
        {
            if (_runtimeTriggers.TryGetValue(triggerIndex, out RuntimeTriggerInfo runtimeTrigger))
            {
                if (runtimeTrigger.TriggerPressed != pressed || runtimeTrigger.TriggerPressStarted != pressDown || runtimeTrigger.TriggerPressEnded != pressUp)
                {
                    BeginSync();

                    runtimeTrigger.TriggerPressed      = pressed;
                    runtimeTrigger.TriggerPressStarted = pressDown;
                    runtimeTrigger.TriggerPressEnded   = pressUp;

                    EndSyncMethod(new object[] { triggerIndex, pressed, pressDown, pressUp });
                }
            }
        }

        /// <summary>
        ///     See <see cref="SyncAmmoLeft(int, int)" />.
        /// </summary>
        /// <param name="grabbableTrigger">The grabbable object for the trigger</param>
        private void SyncAmmoLeft(UxrGrabbableObject grabbableTrigger)
        {
            UxrFirearmTrigger trigger = _triggers.FirstOrDefault(t => t.TriggerGrabbable == grabbableTrigger);

            if (trigger != null && trigger.CycleType == UxrShotCycle.FullyAutomatic)
            {
                int index = _triggers.IndexOf(trigger);

                if (index != -1)
                {
                    SyncAmmoLeft(index, GetAmmoLeft(index));
                }
            }
        }

        /// <summary>
        ///     Sets the ammo left, to sync multiplayer after a fully automatic gun stopped firing.
        /// </summary>
        /// <param name="triggerIndex"> The trigger index</param>
        /// <param name="ammo">The ammo left</param>
        private void SyncAmmoLeft(int triggerIndex, int ammo)
        {
            BeginSync();

            SetAmmoLeft(triggerIndex, ammo);

            EndSyncMethod(new object[] { triggerIndex, ammo });
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets the root grabbable object.
        /// </summary>
        private UxrGrabbableObject RootGrabbable
        {
            get
            {
                UxrGrabbableObject firstTriggerGrabbable = _triggers.Count > 0 && _triggers[0].TriggerGrabbable != null ? _triggers[0].TriggerGrabbable : null;

                if (firstTriggerGrabbable)
                {
                    // A normal setup will have just one grabbable point but for rifles and weapons with multiple parts we may have different grabbable objects.
                    // We will just get the root one so that we can subscribe to its ApplyConstraints event to apply recoil effects

                    Transform          weaponRootGrabbableTransform = firstTriggerGrabbable.UsesGrabbableParentDependency ? firstTriggerGrabbable.GrabbableParent.transform : firstTriggerGrabbable.transform;
                    UxrGrabbableObject rootGrabbable                = weaponRootGrabbableTransform.GetComponent<UxrGrabbableObject>();

                    return rootGrabbable;
                }

                return null;
            }
        }

        private UxrProjectileSource                 _weaponSource;
        private Dictionary<int, RuntimeTriggerInfo> _runtimeTriggers = new Dictionary<int, RuntimeTriggerInfo>();

        #endregion
    }
}