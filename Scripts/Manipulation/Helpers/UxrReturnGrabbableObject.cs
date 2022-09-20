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
    public class UxrReturnGrabbableObject : UxrGrabbableObjectComponent<UxrReturnGrabbableObject>
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private bool  _smoothTransition   = true;
        [SerializeField] private float _returnDelaySeconds = -1.0f;

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
        /// <param name="anchor">Anchor to return the object to</param>
        /// <param name="propagateEvents">Whether to propagate manipulation events</param>
        /// <returns>Coroutine IEnumerator</returns>
        private IEnumerator ReturnCoroutine(UxrGrabbableObject grabbableObject, UxrGrabbableObjectAnchor anchor, bool propagateEvents)
        {
            yield return new WaitForSeconds(_returnDelaySeconds);

            if (anchor != null && anchor.CurrentPlacedObject == null)
            {
                UxrGrabManager.Instance.PlaceObject(grabbableObject, anchor, _smoothTransition ? UxrPlacementType.Smooth : UxrPlacementType.Immediate, propagateEvents);
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

            if (e.IsOwnershipChanged)
            {
                if (e.GrabbableAnchor != null)
                {
                    _grabbableObjectAnchor = e.GrabbableAnchor;
                }

                // Check also dependent grabs. We may be grabbing the object using another grip
                if (!UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, GrabbableObject, UxrHandSide.Left) &&
                    !UxrGrabManager.Instance.IsHandGrabbing(UxrAvatar.LocalAvatar, GrabbableObject, UxrHandSide.Right))
                {
                    if (_returnDelaySeconds <= 0.0f)
                    {
                        // Return to original place
                        UxrGrabManager.Instance.PlaceObject(e.GrabbableObject, _grabbableObjectAnchor, _smoothTransition ? UxrPlacementType.Smooth : UxrPlacementType.Immediate, true);
                    }
                    else
                    {
                        _returnCoroutine = StartCoroutine(ReturnCoroutine(e.GrabbableObject, _grabbableObjectAnchor, true));
                    }
                }
            }
        }

        #endregion

        #region Private Methods

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

        private UxrGrabbableObjectAnchor _grabbableObjectAnchor;
        private Coroutine                _returnCoroutine;

        #endregion
    }
}