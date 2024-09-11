// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrReturnGrabbableObject.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Core.Components.Composite;
using UnityEngine;

namespace UltimateXR.Manipulation.Helpers
{
    /// <summary>
    ///     Component that will always return an <see cref="UxrGrabbableObject" /> to the
    ///     <see cref="UxrGrabbableObjectAnchor" /> it was grabbed from whenever it is released.
    /// </summary>
    [RequireComponent(typeof(UxrGrabbableObject))]
    public partial class UxrReturnGrabbableObject : UxrGrabbableObjectComponent<UxrReturnGrabbableObject>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool         _smoothTransition   = true;
        [SerializeField] private float        _returnDelaySeconds = -1.0f;
        [SerializeField] private ReturnPolicy _returnPolicy       = ReturnPolicy.LastAnchor;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Cancels a return if the given object has a <see cref="UxrReturnGrabbableObject" /> component and a return
        ///     programmed.
        /// </summary>
        /// <param name="grabbableObject">Object to try to cancel the return of</param>
        public static void CancelReturn(UxrGrabbableObject grabbableObject)
        {
            if (grabbableObject.gameObject.TryGetComponent<UxrReturnGrabbableObject>(out var returnComponent))
            {
                returnComponent.CancelReturn();
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        ///     Coroutine that returns the object to the original target after some time.
        /// </summary>
        /// <param name="grabbableObject">Object to return</param>
        /// <param name="propagateEvents">Whether to propagate manipulation events</param>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator ReturnCoroutine(UxrGrabbableObject grabbableObject, bool propagateEvents)
        {
            yield return new WaitForSeconds(_returnDelaySeconds);

            UxrGrabbableObjectAnchor anchor = GetReturnAnchor();

            if (anchor != null && anchor.CurrentPlacedObject == null)
            {
                UxrGrabManager.Instance.PlaceObject(grabbableObject, anchor, _smoothTransition ? UxrPlacementOptions.Smooth : UxrPlacementOptions.None, propagateEvents);
            }

            _returnCoroutine = null;
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Called by the base class whenever the object is grabbed.
        /// </summary>
        /// <param name="e">Contains all grab event parameters</param>
        protected override void OnObjectGrabbed(UxrManipulationEventArgs e)
        {
            base.OnObjectGrabbed(e);

            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
            }
        }

        /// <summary>
        ///     Called by the base class whenever the object is released.
        /// </summary>
        /// <param name="e">Contains all grab event parameters</param>
        protected override void OnObjectReleased(UxrManipulationEventArgs e)
        {
            base.OnObjectReleased(e);

            if (e.IsGrabbedStateChanged)
            {
                if (e.GrabbableAnchor != null)
                {
                    _lastObjectAnchor = e.GrabbableAnchor;
                }

                // Check also dependent grabs. We may be grabbing the object using another grip
                if (!UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, GrabbableObject, UxrHandSide.Left,  true) &&
                    !UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, GrabbableObject, UxrHandSide.Right, true))
                {
                    if (_returnDelaySeconds <= 0.0f)
                    {
                        // Return to original place
                        UxrGrabManager.Instance.PlaceObject(e.GrabbableObject, GetReturnAnchor(), _smoothTransition ? UxrPlacementOptions.Smooth : UxrPlacementOptions.None, true);
                    }
                    else
                    {
                        _returnCoroutine = StartCoroutine(ReturnCoroutine(e.GrabbableObject, true));
                    }
                }
            }
        }

        /// <summary>
        ///     Called by the base class whenever the object is placed.
        /// </summary>
        /// <param name="e">Contains all grab event parameters</param>
        protected override void OnObjectPlaced(UxrManipulationEventArgs e)
        {
            base.OnObjectPlaced(e);

            if (e.GrabbableAnchor != null)
            {
                _lastObjectAnchor = e.GrabbableAnchor;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the anchor where the object should be returned.
        /// </summary>
        /// <returns>Destination anchor</returns>
        private UxrGrabbableObjectAnchor GetReturnAnchor()
        {
            if (_returnPolicy == ReturnPolicy.LastAnchor && _lastObjectAnchor != null && _lastObjectAnchor.CurrentPlacedObject == null)
            {
                return _lastObjectAnchor;
            }

            return GrabbableObject.StartAnchor;
        }

        /// <summary>
        ///     Cancels a return if there was one programmed.
        /// </summary>
        private void CancelReturn()
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
        }

        #endregion

        #region Private Types & Data

        private UxrGrabbableObjectAnchor _lastObjectAnchor;
        private Coroutine                _returnCoroutine;

        #endregion
    }
}