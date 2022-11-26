// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrFirearmWeapon.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
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
    public class UxrFirearmWeapon : UxrWeapon
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
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return false;
            }

            return IsLoaded(_triggers[triggerIndex]);
        }

        /// <summary>
        ///     Sets the given weapon trigger loaded state so that it is ready to shoot if there is ammo left.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        public void Reload(int triggerIndex)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return;
            }

            Reload(_triggers[triggerIndex]);
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

            return HasMagAttached(_triggers[triggerIndex]);
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

            return GetAmmoCapacity(_triggers[triggerIndex]);
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

            return GetAmmoLeft(_triggers[triggerIndex]);
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

            SetAmmoLeft(_triggers[triggerIndex], ammo);
        }

        /// <summary>
        ///     Sets the trigger pressed value.
        /// </summary>
        /// <param name="triggerIndex">Index in <see cref="_triggers" /></param>
        /// <param name="triggerValue">Pressed value between range [0.0, 1.0]</param>
        public void SetTriggerPressed(int triggerIndex, float triggerValue)
        {
            if (triggerIndex < 0 || triggerIndex >= _triggers.Count)
            {
                return;
            }

            SetTriggerPressed(_triggers[triggerIndex], triggerValue);
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

            return TryToShootRound(_triggers[triggerIndex]);
        }

        #endregion

        #region Unity

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

            foreach (UxrFirearmTrigger trigger in _triggers)
            {
                trigger.LastShotTimer = -1.0f;
                trigger.HasReloaded   = true;

                if (trigger.TriggerTransform)
                {
                    trigger.TriggerInitialLocalRotation = trigger.TriggerTransform.localRotation;
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
        ///     Called after the avatars have been updated. Updates the hand trigger blend value.
        /// </summary>
        private void UxrManager_AvatarsUpdated()
        {
            foreach (UxrFirearmTrigger trigger in _triggers)
            {
                // Check if we are grabbing the given grip

                if (trigger.TriggerGrabbable && UxrGrabManager.Instance.GetGrabbingHand(trigger.TriggerGrabbable, trigger.GrabbableGrabPointIndex, out UxrGrabber grabber))
                {
                    // Get the trigger press amount and use it to send it to the animation var that controls the hand trigger. Use it to rotate the trigger as well.

                    float triggerPressAmount = UxrAvatar.LocalAvatarInput.GetInput1D(grabber.Side, UxrInput1D.Trigger);

                    trigger.TriggerGrabbable.GetGrabPoint(trigger.GrabbableGrabPointIndex).GetGripPoseInfo(grabber.Avatar).PoseBlendValue = triggerPressAmount;

                    if (trigger.TriggerTransform != null)
                    {
                        trigger.TriggerTransform.localRotation = trigger.TriggerInitialLocalRotation * Quaternion.AngleAxis(trigger.TriggerRotationDegrees * triggerPressAmount, trigger.TriggerRotationAxis);
                    }

                    // Now depending on the weapon type check if we need to shoot

                    bool triggerPressed   = UxrAvatar.LocalAvatarInput.GetButtonsPress(grabber.Side, UxrInputButtons.Trigger);
                    bool triggerPressDown = UxrAvatar.LocalAvatarInput.GetButtonsPressDown(grabber.Side, UxrInputButtons.Trigger);
                    bool shoot            = false;

                    switch (trigger.CycleType)
                    {
                        case UxrShotCycle.ManualReload:
                        {
                            shoot = triggerPressDown && trigger.HasReloaded;

                            if (shoot)
                            {
                                trigger.HasReloaded = false;
                            }
                            break;
                        }

                        case UxrShotCycle.SemiAutomatic:

                            shoot = triggerPressDown;
                            break;

                        case UxrShotCycle.FullyAutomatic:

                            shoot = triggerPressed;
                            break;
                    }

                    if (triggerPressDown && GetAmmoLeft(trigger) == 0)
                    {
                        trigger.ShotAudioNoAmmo?.Play(trigger.TriggerGrabbable.GetGrabbedPointGrabAlignPosition(grabber, trigger.GrabbableGrabPointIndex));
                    }

                    if (shoot)
                    {
                        // Shoot!

                        if (TryToShootRound(trigger))
                        {
                            // Send haptic to the hand grabbing the grip

                            UxrAvatar.LocalAvatarInput.SendHapticFeedback(grabber.Side, trigger.ShotHapticClip);

                            // Send haptic to the other hand if it is also grabbing the weapon

                            if (UxrGrabManager.Instance.IsHandGrabbing(grabber.Avatar, trigger.TriggerGrabbable, grabber.OppositeSide))
                            {
                                UxrAvatar.LocalAvatarInput.SendHapticFeedback(grabber.OppositeSide, trigger.ShotHapticClip);
                            }
                        }
                    }
                }

                if (trigger.LastShotTimer > 0.0f)
                {
                    trigger.LastShotTimer -= Time.deltaTime;
                }

                if (trigger.RecoilTimer > 0.0f)
                {
                    trigger.RecoilTimer -= Time.deltaTime;
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

            foreach (UxrFirearmTrigger trigger in _triggers)
            {
                if (trigger.RecoilTimer > 0.0f)
                {
                    float recoilT = trigger.RecoilDurationSeconds > 0.0f ? (trigger.RecoilDurationSeconds - trigger.RecoilTimer) / trigger.RecoilDurationSeconds : 0.0f;

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
        ///     Checks whether a trigger is in a loaded state, meaning it is ready to shoot if pressed and there is any ammo left.
        /// </summary>
        /// <param name="trigger">Firearm trigger</param>
        /// <returns>Whether it is ready to shoot</returns>
        private static bool IsLoaded(UxrFirearmTrigger trigger)
        {
            return trigger.HasReloaded;
        }

        /// <summary>
        ///     Sets the given weapon trigger loaded state so that it is ready to shoot if there is ammo left.
        /// </summary>
        /// <param name="trigger">Firearm trigger</param>
        private static void Reload(UxrFirearmTrigger trigger)
        {
            trigger.HasReloaded = true;
        }

        /// <summary>
        ///     Checks whether there is a magazine attached that fires shots using the given trigger. It may or may not have ammo.
        /// </summary>
        /// <param name="trigger">Firearm trigger</param>
        /// <returns>Whether there is a magazine attached</returns>
        private static bool HasMagAttached(UxrFirearmTrigger trigger)
        {
            return trigger.AmmunitionMagAnchor != null && trigger.AmmunitionMagAnchor.CurrentPlacedObject != null;
        }

        /// <summary>
        ///     Gets the attached magazine maximum capacity.
        /// </summary>
        /// <param name="trigger">Firearm trigger</param>
        /// <returns>Maximum capacity of ammo in the attached magazine. If there isn't any magazine attached it returns 0</returns>
        private static int GetAmmoCapacity(UxrFirearmTrigger trigger)
        {
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
        /// <param name="trigger">Firearm trigger</param>
        /// <returns>Ammo left in the attached magazine. If there isn't any magazine attached it returns 0</returns>
        private static int GetAmmoLeft(UxrFirearmTrigger trigger)
        {
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
        /// <param name="trigger">Firearm trigger</param>
        /// <param name="ammo">New ammo</param>
        private static void SetAmmoLeft(UxrFirearmTrigger trigger, int ammo)
        {
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
        ///     Sets the trigger pressed value.
        /// </summary>
        /// <param name="trigger">Firearm trigger</param>
        /// <param name="triggerValue">Pressed value between range [0.0, 1.0]</param>
        private static void SetTriggerPressed(UxrFirearmTrigger trigger, float triggerValue)
        {
            trigger.TriggerTransform.localRotation = trigger.TriggerInitialLocalRotation * Quaternion.AngleAxis(trigger.TriggerRotationDegrees * triggerValue, trigger.TriggerRotationAxis);
        }

        /// <summary>
        ///     Tries to shoot a round using the given trigger.
        /// </summary>
        /// <param name="trigger">Firearm trigger</param>
        /// <returns>
        ///     Whether a round was shot. If no round was shot it can mean that:
        ///     <list type="bullet">
        ///         <item>The trigger index references an entry that doesn't exist.</item>
        ///         <item>The firearm isn't loaded.</item>
        ///         <item>The firearm doesn't have any ammo left or there is no magazine attached.</item>
        ///         <item>The shoot frequency doesn't allow to shoot again so quickly.</item>
        ///     </list>
        /// </returns>
        private bool TryToShootRound(UxrFirearmTrigger trigger)
        {
            if (GetAmmoLeft(trigger) > 0 && trigger.LastShotTimer <= 0.0f)
            {
                SetAmmoLeft(trigger, GetAmmoLeft(trigger) - 1);

                trigger.LastShotTimer = trigger.MaxShotFrequency > 0 ? 1.0f / trigger.MaxShotFrequency : -1.0f;

                // TODO: here we probably should add some randomization depending on recoil using the additional optional parameters
                _weaponSource.Shoot(trigger.ProjectileShotIndex);

                trigger.RecoilTimer = trigger.RecoilDurationSeconds;

                // Audio

                trigger.ShotAudio?.Play(_weaponSource.GetShotOrigin(trigger.ProjectileShotIndex));

                // Raise events

                int triggerIndex = _triggers.IndexOf(trigger);
                OnProjectileShot(triggerIndex);
                return true;
            }

            return false;
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

                    Transform          weaponRootGrabbableTransform = firstTriggerGrabbable.UsesGrabbableParentDependency ? firstTriggerGrabbable.GrabbableParentDependency : firstTriggerGrabbable.transform;
                    UxrGrabbableObject rootGrabbable                = weaponRootGrabbableTransform.GetComponent<UxrGrabbableObject>();

                    return rootGrabbable;
                }

                return null;
            }
        }

        private UxrProjectileSource _weaponSource;

        #endregion
    }
}