// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleGroup.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Core.Components;
using UnityEngine;

namespace UltimateXR.UI.UnityInputModule.Controls
{
    /// <summary>
    ///     Component that, added to a GameObject, will make all child <see cref="UxrToggleControlInput" /> components behave
    ///     like a single group where only one selection can be active at a time.
    /// </summary>
    public partial class UxrToggleGroup : UxrComponent
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private InitState _initialToggleState = InitState.DontChange;

        #endregion

        #region Public Types & Data

        /// <summary>
        ///     Event called whenever the currently selected toggle control was changed.
        /// </summary>
        public event Action<UxrToggleControlInput> SelectionChanged;

        /// <summary>
        ///     Gets all the current toggle components.
        /// </summary>
        public IList<UxrToggleControlInput> Toggles => _toggles;

        /// <summary>
        ///     Gets the currently selected toggle component.
        /// </summary>
        public UxrToggleControlInput CurrentSelection { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Refreshes the internal list of children toggle components. This can be used to refresh the list whenever a new
        ///     child toggle component was added dynamically, or was removed.
        /// </summary>
        public void RefreshToggleChildrenList()
        {
            _toggles.ForEach(t => t.Toggled -= Toggle_Toggled);
            _toggles.Clear();

            _toggles = new List<UxrToggleControlInput>();

            for (int i = 0; i < transform.childCount; ++i)
            {
                UxrToggleControlInput toggle = transform.GetChild(i).GetComponent<UxrToggleControlInput>();
                
                if (toggle != null && !toggle.IsBeingDestroyed && toggle.isActiveAndEnabled)
                {
                    _toggles.Add(toggle);
                    toggle.Toggled += Toggle_Toggled;
                }
            }

            CurrentSelection = _toggles.FirstOrDefault(t => t.IsSelected);

            _toggles.ForEach(t => t.CanBeToggled = t != CurrentSelection);
        }

        /// <summary>
        ///     Clears the current selection if there is any.
        /// </summary>
        public void ClearCurrentSelection()
        {
            BeginSync();

            if (CurrentSelection != null)
            {
                CurrentSelection.IsSelected   = false;
                CurrentSelection.CanBeToggled = true;
                CurrentSelection              = null;
            }

            EndSyncMethod();
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Refreshes the toggle list and subscribes to toggle events.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (!_toggles.Any())
            {
                RefreshToggleChildrenList();
            }
        }

        /// <summary>
        ///     Unsubscribes from toggle events.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            _toggles.ForEach(t => t.Toggled -= Toggle_Toggled);
        }

        /// <summary>
        ///     Initializes the current selection if required.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            switch (_initialToggleState)
            {
                case InitState.DontChange: break;

                case InitState.FirstChild:

                    if (_toggles.Any())
                    {
                        _toggles.First().IsSelected = true;
                    }

                    break;
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Called whenever a toggle component was toggled.
        /// </summary>
        /// <param name="toggle">The toggle component that was toggled</param>
        private void Toggle_Toggled(UxrToggleControlInput toggle)
        {
            NotifyToggle(_toggles.IndexOf(toggle), true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Notifies that a toggle component was toggled.
        /// </summary>
        /// <param name="index">The index of the toggle</param>
        /// <param name="isFromEvent">Whether the call comes from handling a toggle event</param>
        private void NotifyToggle(int index, bool isFromEvent)
        {
            BeginSync();

            UxrToggleControlInput toggle = index >= 0 && index < _toggles.Count ? _toggles[index] : null;

            foreach (UxrToggleControlInput t in _toggles)
            {
                t.CanBeToggled = t != toggle;

                if (t != null && t != toggle)
                {
                    t.SetIsSelected(false, false);
                }
            }

            CurrentSelection = toggle;

            if (CurrentSelection != null && !isFromEvent)
            {
                // Since it doesn't come from an event, we need to update the selection manually.
                CurrentSelection.SetIsSelected(true, false);
            }

            SelectionChanged?.Invoke(toggle);

            EndSyncMethod(new object[] { index, false });
        }

        #endregion

        #region Private Types & Data

        private List<UxrToggleControlInput> _toggles = new List<UxrToggleControlInput>();

        #endregion
    }
}