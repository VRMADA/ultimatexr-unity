using System;
using System.Collections;
using System.Collections.Generic;
using UltimateXR.Devices.Integrations.Valve;
using UnityEngine;

namespace UltimateXR.Devices.Integrations.WebXR
{
    public class UxrWebXRTracking : UxrWebXRControllerTracking
    {
        #region Public Overrides UxrControllerTracking
        public override Type RelatedControllerInputType => typeof(UxrWebXRInput);
        #endregion
    }
}
