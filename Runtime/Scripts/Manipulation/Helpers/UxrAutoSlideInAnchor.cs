// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrAutoSlideInAnchor.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.Manipulation.Helpers
{
    /// <summary>
    ///     Anchor component for <see cref="UxrAutoSlideInAnchor" />. Grabbable objects with the
    ///     <see cref="UxrAutoSlideInObject" /> component will automatically attach/detach from this anchor.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObjectAnchor))]
    public class UxrAutoSlideInAnchor : UxrComponent<UxrAutoSlideInAnchor>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets the anchor.
        /// </summary>
        public UxrGrabbableObjectAnchor Anchor
        {
            get
            {
                if (_anchor == null)
                {
                    _anchor = GetComponent<UxrGrabbableObjectAnchor>();
                }

                return _anchor;
            }
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (_anchor == null)
            {
                _anchor = GetComponent<UxrGrabbableObjectAnchor>();
            }
        }

        #endregion

        #region Private Types & Data

        private UxrGrabbableObjectAnchor _anchor;

        #endregion
    }
}