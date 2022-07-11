// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUxrControllerTracking.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Devices
{
    /// <summary>
    ///     Controller tracking interface for all VR input devices, supporting single controllers and dual controller setups.
    /// </summary>
    public interface IUxrControllerTracking
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the type of the input controller component that handles input for the same kind of controller this component
        ///     handles the tracking for.
        /// </summary>
        Type RelatedControllerInputType { get; }

        /// <summary>
        ///     Gets whether the camera of the tracking setup has 6 degrees of freedom
        /// </summary>
        bool HeadsetIs6Dof { get; }

        /// <summary>
        ///     Gets if the left hand sensor in the component inspector has been set up
        /// </summary>
        bool HasLeftHandSensorSetup { get; }

        /// <summary>
        ///     Gets if the right hand sensor in the component inspector has been set up
        /// </summary>
        bool HasRightHandSensorSetup { get; }

        /// <summary>
        ///     Gets the world-space position of the left controller sensor.
        /// </summary>
        Vector3 SensorLeftPos { get; }

        /// <summary>
        ///     Gets the world-space position of the right controller sensor.
        /// </summary>
        Vector3 SensorRightPos { get; }

        /// <summary>
        ///     Gets the world-space rotation of the left controller sensor.
        /// </summary>
        Quaternion SensorLeftRot { get; }

        /// <summary>
        ///     Gets the world-space rotation of the right controller sensor.
        /// </summary>
        Quaternion SensorRightRot { get; }

        /// <summary>
        ///     Gets the world-space position where the left hand bone should be, using the left sensor data.
        /// </summary>
        Vector3 SensorLeftHandPos { get; }

        /// <summary>
        ///     Gets the world-space position where the right hand bone should be, using the right sensor data.
        /// </summary>
        Vector3 SensorRightHandPos { get; }

        /// <summary>
        ///     Gets the world-space rotation that the left hand bone should have using the left sensor data.
        /// </summary>
        Quaternion SensorLeftHandRot { get; }

        /// <summary>
        ///     Gets the world-space rotation that the right hand bone should have using the right sensor data.
        /// </summary>
        Quaternion SensorRightHandRot { get; }

        #endregion
    }
}