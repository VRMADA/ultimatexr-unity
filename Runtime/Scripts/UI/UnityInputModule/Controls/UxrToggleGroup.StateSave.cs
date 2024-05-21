// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrToggleGroup.StateSave.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UltimateXR.Core;
using UltimateXR.Core.StateSave;

namespace UltimateXR.UI.UnityInputModule.Controls
{
    public partial class UxrToggleGroup
    {
        #region Protected Overrides UxrComponent

        protected override int SerializationOrder => UxrConstants.Serialization.SerializationOrderDefault + 1;

        protected override void SerializeState(bool isReading, int stateSerializationVersion, UxrStateSaveLevel level, UxrStateSaveOptions options)
        {
            base.SerializeState(isReading, stateSerializationVersion, level, options);

            if (level >= UxrStateSaveLevel.ChangesSinceBeginning)
            {
                int currentSelectionIndex = _toggles?.IndexOf(CurrentSelection) ?? -1;

                SerializeStateValue(level, options, nameof(currentSelectionIndex), ref currentSelectionIndex);

                if (isReading)
                {
                    CurrentSelection = _toggles != null && currentSelectionIndex >= 0 && currentSelectionIndex < _toggles.Count ? _toggles[currentSelectionIndex] : null;

                    if (CurrentSelection != null)
                    {
                        CurrentSelection.IsSelected = true;
                    }
                    else
                    {
                        if (_toggles != null)
                        {
                            foreach (UxrToggleControlInput t in _toggles)
                            {
                                t.CanBeToggled = true;
                                t.SetIsSelected(false, false);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}