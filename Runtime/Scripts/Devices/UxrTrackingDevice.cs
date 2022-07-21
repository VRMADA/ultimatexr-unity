// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrTrackingDevice.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components.Composite;
using UnityEngine.XR;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Base class for tracking devices.
    /// </summary>
    public abstract class UxrTrackingDevice : UxrAvatarComponent<UxrTrackingDevice>, IUxrTrackingDevice, IUxrTrackingUpdater
    {
        #region Public Types & Data

        /// <summary>
        ///     Default update order.
        /// </summary>
        public const int OrderStandard = 0;

        /// <summary>
        ///     Default update order for post-process tracking devices such as hand-tracking.
        /// </summary>
        public const int OrderPostprocess = 10;

        /// <summary>
        ///     Gets the headset device name.
        /// </summary>
        public static string HeadsetDeviceName
        {
            get
            {
                var inputDevices = new List<InputDevice>();
                InputDevices.GetDevices(inputDevices);

                foreach (var device in inputDevices)
                {
                    if (device.characteristics.HasFlag(InputDeviceCharacteristics.HeadMounted))
                    {
                        return device.name;
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     There are cases where more than one tracking device might be active. We use TrackingUpdateOrder
        ///     for cases where there is one that should be applied after the other(s). For example an Oculus Rift
        ///     together with a Leap Motion setup has one tracking component for each. But Leap Motion should
        ///     override the tracking values of the rift controllers if the Leap Motion component is active.
        ///     In this case Oculus, like most tracking devices, has a value of <see cref="OrderStandard" />
        ///     while Leap Motion has a value of <see cref="OrderPostprocess" /> so that the tracking
        ///     devices update the avatar in the correct order.
        /// </summary>
        public virtual int TrackingUpdateOrder => OrderStandard;

        #endregion

        #region Implicit IUxrDevice

        /// <inheritdoc />
        public abstract string SDKDependency { get; }

        /// <inheritdoc />
        public event EventHandler<UxrDeviceConnectEventArgs> DeviceConnected;

        #endregion

        #region Implicit IUxrTrackingDevice

        /// <inheritdoc />
        public event EventHandler SensorsUpdating;

        /// <inheritdoc />
        public event EventHandler SensorsUpdated;

        /// <inheritdoc />
        public event EventHandler AvatarUpdating;

        /// <inheritdoc />
        public event EventHandler AvatarUpdated;

        #endregion

        #region Explicit IUxrTrackingUpdater

        /// <inheritdoc />
        void IUxrTrackingUpdater.UpdateAvatar()
        {
            OnAvatarUpdating();
            UpdateAvatar();
            OnAvatarUpdated();
        }

        /// <inheritdoc />
        void IUxrTrackingUpdater.UpdateSensors()
        {
            OnSensorsUpdating();
            UpdateSensors();
            OnSensorsUpdated();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Tries to get the connected headset device.
        /// </summary>
        /// <param name="inputDevice">Returns the headset device if found</param>
        /// <returns>Whether the device was found</returns>
        public static bool GetHeadsetDevice(out InputDevice inputDevice)
        {
            var inputDevices = new List<InputDevice>();
            InputDevices.GetDevices(inputDevices);

            foreach (var device in inputDevices)
            {
                if (device.characteristics.HasFlag(InputDeviceCharacteristics.HeadMounted))
                {
                    inputDevice = device;
                    return true;
                }
            }

            inputDevice = new InputDevice();
            return false;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Sets events to null in order to help remove unused references.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            DeviceConnected = null;
            SensorsUpdating = null;
            SensorsUpdated  = null;
            AvatarUpdating  = null;
            AvatarUpdated   = null;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for the <see cref="DeviceConnected" /> event. Can be used to override in child classes in order to
        ///     use the event without subscribing to the parent.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <remarks>Calling the base implementation is required in child classes in order for the event to propagate correctly.</remarks>
        protected virtual void OnDeviceConnected(UxrDeviceConnectEventArgs e)
        {
            DeviceConnected?.Invoke(this, e);
        }

        /// <summary>
        ///     Event trigger for the <see cref="AvatarUpdating" /> event. Can be used to override in child classes in order to use
        ///     the event without subscribing to the parent.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <remarks>Calling the base implementation is required in child classes in order for the event to propagate correctly.</remarks>
        protected virtual void OnAvatarUpdating()
        {
            AvatarUpdating?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Event trigger for the <see cref="AvatarUpdated" /> event. Can be used to override in child classes in order to use
        ///     the event without subscribing to the parent.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <remarks>Calling the base implementation is required in child classes in order for the event to propagate correctly.</remarks>
        protected virtual void OnAvatarUpdated()
        {
            AvatarUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Event trigger for the <see cref="SensorsUpdating" /> event. Can be used to override in child classes in order to
        ///     use the event without subscribing to the parent.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <remarks>Calling the base implementation is required in child classes in order for the event to propagate correctly.</remarks>
        protected virtual void OnSensorsUpdating()
        {
            SensorsUpdating?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Event trigger for the <see cref="SensorsUpdated" /> event. Can be used to override in child classes in order to use
        ///     the event without subscribing to the parent.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <remarks>Calling the base implementation is required in child classes in order for the event to propagate correctly.</remarks>
        protected virtual void OnSensorsUpdated()
        {
            SensorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Overriden in child classes to implement the update of the current sensor data.
        /// </summary>
        protected virtual void UpdateSensors()
        {
        }

        /// <summary>
        ///     Overriden in child classes to implement the update of the avatar using the current sensor data.
        /// </summary>
        protected virtual void UpdateAvatar()
        {
        }

        #endregion
    }
}