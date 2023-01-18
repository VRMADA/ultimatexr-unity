// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrPointerInputModule.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Extensions.Unity;
using UltimateXR.Haptics;
using UltimateXR.UI.UnityInputModule.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateXR.UI.UnityInputModule
{
    /// <summary>
    ///     Input module for Unity that enables interaction in virtual reality using either touch gestures (via
    ///     <see cref="UxrFingerTip" />) or laser pointers (via <see cref="UxrLaserPointer" />).
    ///     Using <see cref="AutoEnableOnWorldCanvases" /> it is possible to automatically set up existing Unity canvases (
    ///     <see cref="Canvas" /> components), otherwise it is required to add a <see cref="UxrCanvas" /> component on each
    ///     GameObject having a <see cref="Canvas" /> to enable interaction.
    /// </summary>
    /// <remarks>
    ///     <see cref="Canvas" /> components instantiated at runtime should have a <see cref="UxrCanvas" /> already present in
    ///     order for VR interaction to work, since <see cref="AutoEnableOnWorldCanvases" /> only works for objects present in
    ///     the scene after loading.
    /// </remarks>
    public class UxrPointerInputModule : PointerInputModule
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] protected bool               _disableOtherInputModules    = true;
        [SerializeField] protected bool               _autoEnableOnWorldCanvases   = true;
        [SerializeField] protected bool               _autoAssignEventCamera       = true;
        [SerializeField] protected UxrInteractionType _interactionTypeOnAutoEnable = UxrInteractionType.FingerTips;
        [SerializeField] protected float              _fingerTipMinHoverDistance   = UxrFingerTipRaycaster.FingerTipMinHoverDistanceDefault;
        [SerializeField] protected int                _dragThreshold               = 40;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Gets the singleton instance.
        /// </summary>
        public static UxrPointerInputModule Instance { get; private set; }

        /// <summary>
        ///     Gets whether the input module will try to find all <see cref="Canvas" /> components after loading a scene, in order
        ///     to add a <see cref="UxrCanvas" /> component to those that have not been set up.
        /// </summary>
        public bool AutoEnableOnWorldCanvases => _autoEnableOnWorldCanvases;

        /// <summary>
        ///     Gets whether to assign the event camera to the <see cref="Canvas" /> components to that they use the local
        ///     <see cref="UxrAvatar" />.
        /// </summary>
        public bool AutoAssignEventCamera => _autoAssignEventCamera;

        /// <summary>
        ///     Gets, for those canvases that have been set up automatically using <see cref="AutoEnableOnWorldCanvases" />, the
        ///     type of interaction that will be used.
        /// </summary>
        public UxrInteractionType InteractionTypeOnAutoEnable => _interactionTypeOnAutoEnable;

        /// <summary>
        ///     Gets the minimum distance from a finger tip to a canvas in order to generate hovering events, when
        ///     <see cref="InteractionTypeOnAutoEnable" /> is <see cref="UxrInteractionType.FingerTips" />,
        /// </summary>
        public float FingerTipMinHoverDistance => _fingerTipMinHoverDistance;

        #endregion

        #region Public Overrides BaseInputModule

        /// <inheritdoc />
        public override bool IsModuleSupported()
        {
            return true;
        }

        /// <inheritdoc />
        public override void Process()
        {
            if (UxrManager.Instance == null)
            {
                return;
            }

            bool usedEvent = SendUpdateEventToSelectedObject();

            foreach (UxrFingerTip fingerTip in UxrFingerTip.EnabledComponentsInLocalAvatar)
            {
                ProcessPointerEvents(GetFingerTipPointerEventData(fingerTip));
            }

            foreach (UxrLaserPointer laserPointer in UxrLaserPointer.EnabledComponentsInLocalAvatar)
            {
                ProcessPointerEvents(GetLaserPointerEventData(laserPointer));
            }

            /*
             TODO: Create navigation events using controller input? 
             
            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                {
                    usedEvent |= SendMoveEventToSelectedObject();
                }

                if (!usedEvent)
                {
                    SendSubmitEventToSelectedObject();
                }
            }*/
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Checks if the given GameObject is interactive. An object is considered interactive when it is able to handle either
        ///     pointer down or pointer drag events.
        ///     Since other non-interactive objects may be in front of interactive objects, the whole hierarchy is checked up to
        ///     the first <see cref="UxrCanvas" /> found.
        /// </summary>
        /// <param name="uiGameObject">UI GameObject to check</param>
        /// <returns>Whether the given GameObject is interactive or not</returns>
        public static bool IsInteractive(GameObject uiGameObject)
        {
            if (uiGameObject == null)
            {
                return false;
            }

            UxrCanvas canvas           = uiGameObject.GetComponentInParent<UxrCanvas>();
            Transform currentTransform = uiGameObject.transform;

            if (canvas == null)
            {
                return false;
            }

            while (currentTransform != canvas.transform)
            {
                if (ExecuteEvents.CanHandleEvent<IPointerDownHandler>(currentTransform.gameObject) || ExecuteEvents.CanHandleEvent<IDragHandler>(currentTransform.gameObject))
                {
                    return true;
                }

                currentTransform = currentTransform.parent;
            }

            return false;
        }

        /// <summary>
        ///     Gets the pointer event data of a given <see cref="UxrFingerTip" /> if it exists.
        /// </summary>
        /// <param name="fingerTip">Finger tip to get the event data of</param>
        /// <returns>Pointer event data if it exists or null if not</returns>
        public UxrPointerEventData GetPointerEventData(UxrFingerTip fingerTip)
        {
            return _fingerTipEventData.TryGetValue(fingerTip, out UxrPointerEventData pointerEventData) ? pointerEventData : null;
        }

        /// <summary>
        ///     Gets the pointer event data of a given <see cref="UxrLaserPointer" /> if it exists.
        /// </summary>
        /// <param name="laserPointer">Laser pointer to get the event data of</param>
        /// <returns>Pointer event data if it exists or null if not</returns>
        public UxrPointerEventData GetPointerEventData(UxrLaserPointer laserPointer)
        {
            return _laserPointerEventData.TryGetValue(laserPointer, out UxrPointerEventData pointerEventData) ? pointerEventData : null;
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Initializes the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Instance != null)
            {
                Debug.LogError($"There is already an active {nameof(UxrPointerInputModule)} in the scene. Only one {nameof(UxrPointerInputModule)} can be used.");
            }
            else
            {
                Instance = this;

                if (_disableOtherInputModules)
                {
                    BaseInputModule[] baseInputModules = FindObjectsOfType<BaseInputModule>();

                    foreach (BaseInputModule inputModule in baseInputModules)
                    {
                        if (inputModule != this)
                        {
                            inputModule.enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Subscribes to events and sets up the haptics coroutine.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            UxrControlInput.GlobalPressed  += UxrControlInput_GlobalPressed;
            UxrControlInput.GlobalReleased += UxrControlInput_GlobalReleased;
            UxrControlInput.GlobalClicked  += UxrControlInput_GlobalClicked;

            _coroutineDragHaptics = StartCoroutine(CoroutineDragHaptics());
        }

        /// <summary>
        ///     Unsubscribes from events and stops the haptics coroutine.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            UxrControlInput.GlobalPressed  -= UxrControlInput_GlobalPressed;
            UxrControlInput.GlobalReleased -= UxrControlInput_GlobalReleased;
            UxrControlInput.GlobalClicked  -= UxrControlInput_GlobalClicked;

            StopCoroutine(_coroutineDragHaptics);
        }

        /// <summary>
        ///     Sets the drag threshold.
        /// </summary>
        protected override void Start()
        {
            if (eventSystem != null)
            {
                eventSystem.pixelDragThreshold = _dragThreshold;
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever a <see cref="UxrControlInput" /> was pressed.
        /// </summary>
        /// <param name="controlInput">Control that was interacted with</param>
        /// <param name="eventData">Event parameters</param>
        private void UxrControlInput_GlobalPressed(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (eventData is UxrPointerEventData pointerEventData)
            {
                TrySendButtonFeedback(controlInput, controlInput.FeedbackOnDown, pointerEventData);
            }
        }

        /// <summary>
        ///     Called whenever a <see cref="UxrControlInput" /> was released after being pressed.
        /// </summary>
        /// <param name="controlInput">Control that was interacted with</param>
        /// <param name="eventData">Event parameters</param>
        private void UxrControlInput_GlobalReleased(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (eventData is UxrPointerEventData pointerEventData)
            {
                TrySendButtonFeedback(controlInput, controlInput.FeedbackOnUp, pointerEventData);
            }
        }

        /// <summary>
        ///     Called whenever a <see cref="UxrControlInput" /> was clicked. Depending on the operating mode, a click is either a
        ///     release after a press or a press.
        /// </summary>
        /// <param name="controlInput">Control that was interacted with</param>
        /// <param name="eventData">Event parameters</param>
        private void UxrControlInput_GlobalClicked(UxrControlInput controlInput, PointerEventData eventData)
        {
            if (eventData is UxrPointerEventData pointerEventData)
            {
                TrySendButtonFeedback(controlInput, controlInput.FeedbackOnClick, pointerEventData);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Finds the raycast that will be used to find out which UI element the user interacted with.
        /// </summary>
        /// <param name="candidates">List of candidates, sorted in increasing distance order</param>
        /// <param name="pointerEventData">Pointer data</param>
        /// <returns>The raycast that describes the UI element that the user interacted with</returns>
        protected static RaycastResult FindFirstRaycast(List<RaycastResult> candidates, UxrPointerEventData pointerEventData)
        {
            int first           = -1;
            int candidatesCount = candidates.Count;

            // First search for the first raycast that shares canvas with the pointerEnter event

            UxrCanvas initialCanvas = pointerEventData.pointerEnter != null ? pointerEventData.pointerEnter.GetTopmostCanvas() : null;

            for (int i = 0; i < candidatesCount; ++i)
            {
                if (candidates[i].gameObject == null)
                {
                    continue;
                }

                if (first == -1)
                {
                    first = i;
                }

                if (candidates[i].gameObject.GetTopmostCanvas() == initialCanvas)
                {
                    return candidates[i];
                }
            }

            // Not found? return first

            if (first != -1)
            {
                return candidates[first];
            }

            // No results? return empty

            return new RaycastResult();
        }

        /// <summary>
        ///     Processes the pointer events.
        /// </summary>
        /// <param name="pointerEventData">Pointer event data</param>
        protected virtual void ProcessPointerEvents(UxrPointerEventData pointerEventData)
        {
            // Handle events

            bool pressedBefore = pointerEventData.pointerPress != null;

            ProcessPointerPressRelease(pointerEventData);
            ProcessMove(pointerEventData);
            ProcessDrag(pointerEventData);

            bool pressedNow = pointerEventData.pointerPress != null;

            // Default haptic feedback if we don't have UxrControlInput

            if (pressedNow && !pressedBefore && pointerEventData.pointerPress.GetComponent<UxrControlInput>() == null)
            {
                if (UxrAvatar.LocalAvatarInput)
                {
                    UxrAvatar.LocalAvatarInput.SendHapticFeedback(pointerEventData.HandSide, UxrHapticClipType.Click, 0.2f);
                }
            }

            if (pointerEventData.GameObjectClicked)
            {
                if (pointerEventData.GameObjectClicked.GetComponent<UxrControlInput>() == null)
                {
                    if (UxrAvatar.LocalAvatarInput)
                    {
                        UxrAvatar.LocalAvatarInput.SendHapticFeedback(pointerEventData.HandSide, UxrHapticClipType.Click, 0.6f);
                    }
                }

                pointerEventData.GameObjectClicked = null;
            }

            pointerEventData.Speed = pointerEventData.delta.magnitude / Time.deltaTime;
        }

        /// <summary>
        ///     Processes the pointer press and release events.
        /// </summary>
        /// <param name="pointerEventData">Pointer event data</param>
        protected virtual void ProcessPointerPressRelease(UxrPointerEventData pointerEventData)
        {
            GameObject currentOverGo = pointerEventData.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pointerEventData.PressedThisFrame)
            {
                pointerEventData.eligibleForClick    = true;
                pointerEventData.delta               = Vector2.zero;
                pointerEventData.dragging            = false;
                pointerEventData.useDragThreshold    = true;
                pointerEventData.pressPosition       = pointerEventData.position;
                pointerEventData.pointerPressRaycast = pointerEventData.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEventData);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                GameObject newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEventData, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
                }

                float time = Time.unscaledTime;

                if (newPressed == pointerEventData.lastPress)
                {
                    float diffTime = time - pointerEventData.clickTime;

                    if (diffTime < 0.3f)
                    {
                        ++pointerEventData.clickCount;
                    }
                    else
                    {
                        pointerEventData.clickCount = 1;
                    }

                    pointerEventData.clickTime = time;
                }
                else
                {
                    pointerEventData.clickCount = 1;
                }

                pointerEventData.pointerPress    = newPressed;
                pointerEventData.rawPointerPress = currentOverGo;
                pointerEventData.clickTime       = time;
                pointerEventData.pointerDrag     = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEventData.pointerDrag != null)
                {
                    ExecuteEvents.Execute(pointerEventData.pointerDrag, pointerEventData, ExecuteEvents.initializePotentialDrag);
                }

                // If the UI element has scrolling, click will require press+release to support dragging.
                // If not, it's a little more user friendly in VR to require just a press to avoid missing clicks.
                // TODO: Be able to control if this feature is enabled via an inspector parameter.
                // TODO: Check compatibility with drag&drop. 

                if (pointerEventData.pointerPress && !RequiresScrolling(pointerEventData.pointerPress))
                {
                    // UI element doesn't require scrolling. Perform a click on press instead of a click on release.
                    pointerEventData.eligibleForClick = false;
                    ExecuteEvents.Execute(pointerEventData.pointerPress, pointerEventData, ExecuteEvents.pointerClickHandler);
                    pointerEventData.GameObjectClicked = pointerEventData.pointerPress;
                }
            }

            // PointerUp notification
            if (pointerEventData.ReleasedThisFrame)
            {
                ExecuteEvents.Execute(pointerEventData.pointerPress, pointerEventData, ExecuteEvents.pointerUpHandler);

                // see if the release is on the same element that was pressed...
                GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEventData.pointerPress == pointerUpHandler && pointerEventData.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEventData.pointerPress, pointerEventData, ExecuteEvents.pointerClickHandler);
                    pointerEventData.GameObjectClicked = pointerEventData.pointerPress;
                }
                else if (pointerEventData.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEventData, ExecuteEvents.dropHandler);
                }

                pointerEventData.eligibleForClick = false;
                pointerEventData.pointerPress     = null;
                pointerEventData.rawPointerPress  = null;
                pointerEventData.pointerClick     = null;

                if (pointerEventData.pointerDrag != null && pointerEventData.dragging)
                {
                    ExecuteEvents.Execute(pointerEventData.pointerDrag, pointerEventData, ExecuteEvents.endDragHandler);
                }

                pointerEventData.dragging    = false;
                pointerEventData.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over something that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != pointerEventData.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEventData, null);
                    HandlePointerExitAndEnter(pointerEventData, currentOverGo);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets whether a ray-casted UI element requires auto-enabling the laser pointer.
        /// </summary>
        /// <param name="raycast">Raycast to check</param>
        /// <param name="laserPointer">The laser pointer</param>
        /// <returns>Whether the UI element will auto-enable the laser pointer</returns>
        private bool DoesAutoEnableLaserPointer(RaycastResult raycast, UxrLaserPointer laserPointer)
        {
            if (laserPointer.IgnoreAutoEnable)
            {
                return false;
            }

            if (raycast.isValid && raycast.gameObject)
            {
                UxrCanvas[] canvasVR = raycast.gameObject.GetComponentsInParent<UxrCanvas>();

                foreach (UxrCanvas canvas in canvasVR)
                {
                    if (canvas.CanvasInteractionType == UxrInteractionType.LaserPointers &&
                        canvas.AutoEnableLaserPointer &&
                        canvas.IsCompatible(laserPointer.HandSide) &&
                        raycast.distance <= canvas.AutoEnableDistance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Coroutine that sends haptic feedback when elements are being dragged.
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator CoroutineDragHaptics()
        {
            void SendDragHapticFeedback(UxrHandSide handSide, float dragSpeed)
            {
                float quantityPos  = (dragSpeed - HapticsMinSpeed) / (HapticsMaxSpeed - HapticsMinSpeed);
                float frequencyPos = Mathf.Lerp(HapticsMinFrequency, HapticsMaxFrequency, Mathf.Clamp01(quantityPos));
                float amplitudePos = Mathf.Lerp(HapticsMinAmplitude, HapticsMaxAmplitude, Mathf.Clamp01(quantityPos));
                UxrAvatar.LocalAvatarInput.SendHapticFeedback(handSide, frequencyPos, amplitudePos, HapticsSampleDurationSeconds);
            }

            while (true)
            {
                float maxLeftSpeed  = Mathf.Max(GetMaxDragSpeed(_fingerTipEventData, UxrHandSide.Left),  GetMaxDragSpeed(_laserPointerEventData, UxrHandSide.Left));
                float maxRightSpeed = Mathf.Max(GetMaxDragSpeed(_fingerTipEventData, UxrHandSide.Right), GetMaxDragSpeed(_laserPointerEventData, UxrHandSide.Right));

                if (maxLeftSpeed >= HapticsMinSpeed)
                {
                    SendDragHapticFeedback(UxrHandSide.Left, maxLeftSpeed);
                }

                if (maxRightSpeed >= HapticsMinSpeed)
                {
                    SendDragHapticFeedback(UxrHandSide.Right, maxRightSpeed);
                }

                yield return new WaitForSeconds(HapticsSampleDurationSeconds);
            }
        }

        /// <summary>
        ///     Returns whether the given GameObject is part of a UI that requires scrolling.
        /// </summary>
        /// <param name="pointerPress">UI GameObject to test</param>
        /// <returns>Whether the GameObject requires scrolling</returns>
        private bool RequiresScrolling(GameObject pointerPress)
        {
            ScrollRect scrollRect = pointerPress.GetComponentInParent<ScrollRect>();

            if (scrollRect != null)
            {
                if (scrollRect.viewport == null || scrollRect.content == null)
                {
                    // We've found cases where null viewports may mean special handling of scroll views to virtualize visible area (SRDebugger)
                    return true;
                }

                if (scrollRect.horizontal && scrollRect.content.rect.width > scrollRect.viewport.rect.width)
                {
                    // Scrollable content is wider than viewport.
                    return true;
                }

                if (scrollRect.vertical && scrollRect.content.rect.height > scrollRect.viewport.rect.height)
                {
                    // Scrollable content is taller than viewport.
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks whether to send UpdateSelected events.
        /// </summary>
        /// <returns>Whether an UpdateSelected event was processed</returns>
        private bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
            {
                return false;
            }

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        /// <summary>
        ///     Sends button audio/haptic feedback depending on the events processed.
        /// </summary>
        private void TrySendButtonFeedback(UxrControlInput controlInput, UxrControlFeedback controlFeedback, UxrPointerEventData pointerEventData)
        {
            if (UxrAvatar.LocalAvatarInput)
            {
                UxrAvatar.LocalAvatarInput.SendHapticFeedback(pointerEventData.HandSide, controlFeedback.HapticClip);
            }

            if (controlFeedback.AudioClip != null && controlInput.Interactable)
            {
                Vector3 audioPosition = !controlFeedback.UseAudio3D && UxrAvatar.LocalAvatarCamera ? UxrAvatar.LocalAvatar.CameraPosition : controlInput.transform.position;
                AudioSource.PlayClipAtPoint(controlFeedback.AudioClip, audioPosition, controlFeedback.AudioVolume);
            }
        }

        /// <summary>
        ///     Gets or creates the pointer event data for a finger tip.
        /// </summary>
        /// <param name="fingerTip">Finger tip to process</param>
        /// <param name="data">Returns the pointer event data for the given finger tip</param>
        /// <param name="create">Whether to create the data if an entry for the given finger tip doesn't exist</param>
        private void FetchPointerEventData(UxrFingerTip fingerTip, out UxrPointerEventData data, bool create)
        {
            if (!_fingerTipEventData.TryGetValue(fingerTip, out data) && create)
            {
                data = new UxrPointerEventData(eventSystem, fingerTip);
                _fingerTipEventData.Add(fingerTip, data);
            }
        }

        /// <summary>
        ///     Gets or creates the pointer event data for a laser pointer.
        /// </summary>
        /// <param name="laserPointer">Laser pointer to process</param>
        /// <param name="data">Returns the pointer event data for the given laser pointer</param>
        /// <param name="create">Whether to create the data if an entry for the given laser pointer doesn't exist</param>
        private void FetchPointerEventData(UxrLaserPointer laserPointer, out UxrPointerEventData data, bool create)
        {
            if (!_laserPointerEventData.TryGetValue(laserPointer, out data) && create)
            {
                data = new UxrPointerEventData(eventSystem, laserPointer);
                _laserPointerEventData.Add(laserPointer, data);
            }
        }

        /// <summary>
        ///     Computes the event data for the given finger tip.
        /// </summary>
        /// <param name="fingerTip">Finger tip to compute the event data for</param>
        /// <returns>Pointer event data</returns>
        private UxrPointerEventData GetFingerTipPointerEventData(UxrFingerTip fingerTip)
        {
            // Get/create the event data
            FetchPointerEventData(fingerTip, out UxrPointerEventData data, true);
            data.Reset();

            // TODO: Add scroll support using thumbstick?
            // leftData.scrollDelta = ...

            data.button           = PointerEventData.InputButton.Left;
            data.useDragThreshold = true;

            // Finger tip worldpos/previousworldpos initialization

            data.PreviousWorldPos = data.WorldPos;
            data.WorldPos         = fingerTip.WorldPos;

            if (!data.WorldPosInitialized)
            {
                data.WorldPosInitialized = true;
                return data;
            }

            // Raycast

            RaycastResult raycastResult = data.pointerCurrentRaycast;
            raycastResult.worldPosition = fingerTip.WorldPos;
            raycastResult.worldNormal   = fingerTip.WorldDir;
            data.pointerCurrentRaycast  = raycastResult;

            eventSystem.RaycastAll(data, m_RaycastResultCache);
            RaycastResult raycast = FindFirstRaycast(m_RaycastResultCache, data);

            // Check if it is a compatible hand using our UxrCanvas.
            // TODO: Support 2D/3D objects in the UxrFingerTipRaycaster?

            data.IgnoredGameObject = null;
            data.GameObject2D      = raycast.isValid && raycast.depth == UxrConstants.UI.Depth2DObject ? raycast.gameObject : null;
            data.GameObject3D      = raycast.isValid && raycast.depth == UxrConstants.UI.Depth3DObject ? raycast.gameObject : null;
            data.IsInteractive     = raycast.isValid && !data.IsNonUI && IsInteractive(raycast.gameObject);

            bool isHandCompatible = IsHandCompatible(fingerTip.Side, raycast.gameObject);

            if (data.IsNonUI || !isHandCompatible)
            {
                // If finger tip should be ignored, null the raycast object so that events still gets processed as if nothing was hit.
                // This is mainly to process things correctly if the finger tip is invalidated at runtime.
                data.IgnoredGameObject = raycast.gameObject;
                raycast.gameObject     = null;
            }

            data.pointerCurrentRaycast = raycast;

            // Try to find our ray casting module

            UxrFingerTipRaycaster raycaster = raycast.module as UxrFingerTipRaycaster;

            if (raycaster)
            {
                // Get screen position generated by our module
                data.position = raycast.screenPosition;
            }

            // First force a release if some conditions are met. We basically want to avoid using fingertips whenever a user tries to grab something

            bool fingerTipValid = fingerTip.gameObject.activeInHierarchy && fingerTip.enabled && fingerTip.Avatar.AvatarController.CanHandInteractWithUI(fingerTip.Side);

            data.PressedThisFrame = false;

            data.ReleasedThisFrame = data.pointerPress != null && !fingerTipValid;

            // Check for presses/releases by comparing the finger tip current/last positions against the UI object's plane

            if (data.pointerEnter && fingerTipValid)
            {
                if (!IsFingerTipOutside(data, data.pointerEnter) && WasFingerTipPreviousPosOutside(data, data.pointerEnter))
                {
                    data.PressedThisFrame = true;
                }
                else if (IsFingerTipOutside(data, data.pointerEnter) && !WasFingerTipPreviousPosOutside(data, data.pointerEnter))
                {
                    data.ReleasedThisFrame = true;
                }
            }

            // Make sure here that UI events will get called appropriately

            if (data.pointerCurrentRaycast.gameObject == null && data.pointerEnter != null)
            {
                data.ReleasedThisFrame = true;
            }

            return data;
        }

        /// <summary>
        ///     Computes the event data for the laser pointer.
        /// </summary>
        /// <param name="laserPointer">Laser pointer to compute the event data for</param>
        /// <returns>Pointer event data</returns>
        private UxrPointerEventData GetLaserPointerEventData(UxrLaserPointer laserPointer)
        {
            // Get/create the event data
            FetchPointerEventData(laserPointer, out UxrPointerEventData data, true);
            data.Reset();

            // TODO: Add scroll support using thumbstick?
            // leftData.scrollDelta = ...

            data.button           = PointerEventData.InputButton.Left;
            data.useDragThreshold = true;

            // Raycast

            RaycastResult raycastResult = data.pointerCurrentRaycast;
            raycastResult.worldPosition = laserPointer.LaserPos;
            raycastResult.worldNormal   = laserPointer.LaserDir;
            data.pointerCurrentRaycast  = raycastResult;
            data.IgnoredGameObject      = null;

            eventSystem.RaycastAll(data, m_RaycastResultCache);
            RaycastResult raycast = FindFirstRaycast(m_RaycastResultCache, data);

            // Raycasts are performed using length to canvas. If no raycast was found, raycast using laser length first.

            bool colliderRaycastProcessed = false;

            if (!raycast.isValid)
            {
                if (laserPointer.TargetTypes.HasFlag(UxrLaserPointerTargetTypes.Colliders3D))
                {
                    if (Physics.Raycast(new Ray(laserPointer.LaserPos, laserPointer.LaserDir), out RaycastHit hit, laserPointer.MaxRayLength, laserPointer.BlockingMask, laserPointer.TriggerCollidersInteraction))
                    {
                        raycast.gameObject       = hit.collider.gameObject;
                        raycast.distance         = hit.distance;
                        data.GameObject2D        = null;
                        data.GameObject3D        = hit.collider.gameObject;
                        data.IsInteractive       = false;
                        colliderRaycastProcessed = true;
                    }
                }
                else if (laserPointer.TargetTypes.HasFlag(UxrLaserPointerTargetTypes.Colliders2D))
                {
                    RaycastHit2D hit = Physics2D.Raycast(laserPointer.LaserPos, laserPointer.LaserDir, laserPointer.MaxRayLength, laserPointer.BlockingMask);
                    
                    if (hit.collider)
                    {
                        raycast.gameObject       = hit.collider.gameObject;
                        raycast.distance         = hit.distance;
                        data.GameObject2D        = hit.collider.gameObject;
                        data.GameObject3D        = null;
                        data.IsInteractive       = false;
                        colliderRaycastProcessed = true;
                    }
                }
            }

            if (!colliderRaycastProcessed)
            {
                // Check if the current ray-casted element is interactive

                data.GameObject2D  = raycast.isValid && raycast.depth == UxrConstants.UI.Depth2DObject ? raycast.gameObject : null;
                data.GameObject3D  = raycast.isValid && raycast.depth == UxrConstants.UI.Depth3DObject ? raycast.gameObject : null;
                data.IsInteractive = raycast.isValid && !data.IsNonUI && IsInteractive(raycast.gameObject);
            }

            laserPointer.IsAutoEnabled = !data.IsNonUI && DoesAutoEnableLaserPointer(raycast, laserPointer);

            bool isHandCompatible = IsHandCompatible(laserPointer.HandSide, raycast.gameObject);

            if (data.IsNonUI || !laserPointer.IsLaserEnabled || !isHandCompatible)
            {
                // If laser should be ignored, null the raycast object so that events still gets processed as if nothing was hit.
                // TODO: Make sure that this is called after controller input processing during this frame

                data.IgnoredGameObject = raycast.gameObject;
                raycast.gameObject     = null;
                data.IsInteractive     = false;
            }

            data.pointerCurrentRaycast = raycast;

            // Try to find our ray casting module

            UxrLaserPointerRaycaster raycaster = raycast.module as UxrLaserPointerRaycaster;

            if (raycaster)
            {
                // Get screen position generated by our module
                data.position = raycast.screenPosition;
            }

            // Make sure here that UI events will get called appropriately

            data.PressedThisFrame = isHandCompatible && laserPointer.IsLaserEnabled && laserPointer.IsClickedThisFrame();

            if (data.pointerPress != null && !laserPointer.IsLaserEnabled)
            {
                data.ReleasedThisFrame = true;
            }
            else
            {
                data.ReleasedThisFrame = laserPointer.IsLaserEnabled && laserPointer.IsReleasedThisFrame();
            }

            if (data.pointerCurrentRaycast.gameObject == null && data.pointerEnter != null)
            {
                data.ReleasedThisFrame = true;
            }

            return data;
        }

        /// <summary>
        ///     Gets whether the given hand is compatible with the target GameObject.
        /// </summary>
        /// <param name="handSide">Which hand</param>
        /// <param name="uiGameObject">UI element GameObject</param>
        /// <returns>Whether the hand is compatible</returns>
        /// <seealso cref="UxrCanvas.IsCompatible" />
        private bool IsHandCompatible(UxrHandSide handSide, GameObject uiGameObject)
        {
            if (uiGameObject == null)
            {
                return false;
            }

            UxrCanvas canvas = uiGameObject.GetComponentInParent<UxrCanvas>();

            return !canvas || canvas.IsCompatible(handSide);
        }

        /// <summary>
        ///     Gets the maximum drag speed of the pointers that are interacting with a given control.
        /// </summary>
        /// <param name="eventData">Event data for all pointers</param>
        /// <param name="handSide">Which hand</param>
        /// <typeparam name="T">Pointer type</typeparam>
        /// <returns>Maximum drag speed in units/second</returns>
        private float GetMaxDragSpeed<T>(Dictionary<T, UxrPointerEventData> eventData, UxrHandSide handSide)
        {
            return eventData.Where(d => d.Value.HandSide == handSide && d.Value.dragging && d.Value.Avatar.AvatarController.CanHandInteractWithUI(handSide))
                            .Select(d => d.Value.Speed)
                            .DefaultIfEmpty(0.0f)
                            .Max();
        }

        /// <summary>
        ///     Gets whether a finger tip is on the front side of the plane where the control lies.
        /// </summary>
        /// <param name="pointerEventData">Pointer event data</param>
        /// <param name="uiGameObject">Control</param>
        /// <returns>Whether the finger tip is on the front side</returns>
        private bool IsFingerTipOutside(UxrPointerEventData pointerEventData, GameObject uiGameObject)
        {
            return Vector3.Dot(uiGameObject.transform.position - pointerEventData.WorldPos, uiGameObject.transform.forward) > 0.0f;
        }

        /// <summary>
        ///     Gets whether a finger tip was on the front side of the plane where the control lies, during the previous frame.
        /// </summary>
        /// <param name="pointerEventData">Pointer event data</param>
        /// <param name="uiGameObject">Control</param>
        /// <returns>Whether the finger tip was on the front side the previous frame</returns>
        private bool WasFingerTipPreviousPosOutside(UxrPointerEventData pointerEventData, GameObject uiGameObject)
        {
            return Vector3.Dot(uiGameObject.transform.position - pointerEventData.PreviousWorldPos, uiGameObject.transform.forward) > 0.0f;
        }

        #endregion

        #region Private Types & Data

        private const float HapticsSampleDurationSeconds = 0.1f;
        private const float HapticsMinAmplitude          = 0.01f;
        private const float HapticsMaxAmplitude          = 0.2f;
        private const float HapticsMinFrequency          = 200.0f;
        private const float HapticsMaxFrequency          = 200.0f;
        private const float HapticsMinSpeed              = 30.0f;
        private const float HapticsMaxSpeed              = 12000.0f;

        private readonly Dictionary<UxrFingerTip, UxrPointerEventData>    _fingerTipEventData    = new Dictionary<UxrFingerTip, UxrPointerEventData>();
        private readonly Dictionary<UxrLaserPointer, UxrPointerEventData> _laserPointerEventData = new Dictionary<UxrLaserPointer, UxrPointerEventData>();

        private Coroutine _coroutineDragHaptics;

        #endregion
    }
}