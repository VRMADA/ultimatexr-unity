// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrGrabPointIndex.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace UltimateXR.Manipulation
{
    /// <summary>
    ///     Enables nicer formatting of the grab point this shape is bound to in the editor. It will show strings like Main,
    ///     Additional 0,
    ///     Additional 1... etc. because there is an CustomPropertyDrawer for this class (see GrabPointIndexDrawer).
    /// </summary>
    [Serializable]
    public class UxrGrabPointIndex
    {
        #region Inspector Properties/Serialized Fields

        [SerializeField] private int _index;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="index">Grab point index</param>
        public UxrGrabPointIndex(int index)
        {
            _index = index;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the display name of a given grabbable object index.
        /// </summary>
        /// <param name="grabbableObject">Grabbable object</param>
        /// <param name="index">Grab point index</param>
        /// <returns>Display name</returns>
        public static string GetIndexDisplayName(UxrGrabbableObject grabbableObject, int index)
        {
            // If we have a custom name set up in the editor, use it

            UxrGrabPointInfo grabPointInfo = grabbableObject.GetGrabPoint(index);

            if (grabPointInfo != null && !string.IsNullOrEmpty(grabPointInfo.EditorName))
            {
                return grabPointInfo.EditorName;
            }

            // Use better formatted default name

            if (index == 0)
            {
                return MainGrabPointName;
            }

            return AdditionalGrabPointPrefix + " " + (index - 1);
        }

        /// <summary>
        ///     Gets the grab point index of a given display name.
        /// </summary>
        /// <param name="name">Display name to get the index for</param>
        /// <returns>Grab point index</returns>
        public static int GetIndexFromDisplayName(string name)
        {
            if (name == MainGrabPointName)
            {
                return 0;
            }
            if (name.StartsWith(AdditionalGrabPointPrefix))
            {
                return int.Parse(name.Remove(0, AdditionalGrabPointPrefix.Length + 1)) + 1;
            }

            return -1;
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        ///     Converts from <see cref="UxrGrabPointIndex" /> to integer.
        /// </summary>
        /// <param name="grabPointIndex">Grab point index object</param>
        /// <returns>Integer</returns>
        public static implicit operator int(UxrGrabPointIndex grabPointIndex)
        {
            return grabPointIndex._index;
        }

        /// <summary>
        ///     Converts from integer to <see cref="UxrGrabPointIndex" />.
        /// </summary>
        /// <param name="index">Grab point index</param>
        /// <returns>Grab point index object</returns>
        public static implicit operator UxrGrabPointIndex(int index)
        {
            return new UxrGrabPointIndex(index);
        }

        #endregion

        #region Private Types & Data

        private const string MainGrabPointName         = "Main Grab Point";
        private const string AdditionalGrabPointPrefix = "Additional Grab Point";

        #endregion
    }
}