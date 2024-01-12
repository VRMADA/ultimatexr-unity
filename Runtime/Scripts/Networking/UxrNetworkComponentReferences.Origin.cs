// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrNetworkComponentReferences.Origin.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Networking
{
    public partial class UxrNetworkComponentReferences
    {
        #region Public Types & Data

        /// <summary>
        ///     Enumerates where the components come from.
        /// </summary>
        public enum Origin
        {
            None = 0,

            /// <summary>
            ///     The source of the components is a <see cref="UxrNetworkImplementation" />.
            /// </summary>
            Network,

            /// <summary>
            ///     The source of the components is a <see cref="UxrNetworkVoiceImplementation" />.
            /// </summary>
            NetworkVoice,
        }

        #endregion
    }
}