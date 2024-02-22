// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSyncResult.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Contains the result of executing a state change synchronization.
    /// </summary>
    public class UxrStateSyncResult
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets whether there was an error executing the state change.
        /// </summary>
        public bool IsError => ErrorMessage != null;

        /// <summary>
        ///     Gets the target of the state change.
        /// </summary>
        public IUxrStateSync Target { get; }

        /// <summary>
        ///     Gets the state change data.
        /// </summary>
        public UxrSyncEventArgs EventArgs { get; }

        /// <summary>
        ///     Gets the error message if there was an error executing the state change. Otherwise it returns null.
        /// </summary>
        public string ErrorMessage { get; }

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="target">Target of the state change</param>
        /// <param name="eventArgs">Event data containing the state change</param>
        /// <param name="errorMessage">Error message if there was an error executing the state change, otherwise null</param>
        public UxrStateSyncResult(IUxrStateSync target, UxrSyncEventArgs eventArgs, string errorMessage)
        {
            Target       = target;
            EventArgs    = eventArgs;
            ErrorMessage = errorMessage;
        }

        #endregion

        #region Public Overrides object

        /// <inheritdoc />
        public override string ToString()
        {
            string error = !string.IsNullOrEmpty(ErrorMessage) ? $" Error is: {ErrorMessage}" : string.Empty;
            
            if (IsValid)
            {
                string result = !string.IsNullOrEmpty(ErrorMessage) ? "Could not execute state change" : "Successful state change";
                return $"{result} event for {Target.Component.name}, {EventArgs}.{error}";
            }

            if (Target != null)
            {
                return $"Unknown event for {Target.Component.name}.{error}";
            }

            if (EventArgs != null)
            {
                return $"Event for unresolved target: {EventArgs}.{error}";
            }

            return error;
        }

        #endregion

        #region Private Types & Data

        /// <summary>
        ///     Gets whether the result contains valid data.
        /// </summary>
        private bool IsValid => Target != null && EventArgs != null;

        #endregion
    }
}