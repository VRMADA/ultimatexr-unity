using System;

namespace UltimateXR.Devices.Integrations.WebXR
{
    public class UxrWebXRTracking : UxrWebXRControllerTracking
    {
        #region Public Overrides UxrControllerTracking
        public override Type RelatedControllerInputType => typeof(UxrWebXRInput);
        #endregion
    }
}
