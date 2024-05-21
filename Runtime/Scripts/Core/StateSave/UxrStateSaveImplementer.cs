// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSaveImplementer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UltimateXR.Core.Components.Singleton;
using UltimateXR.Core.Instantiation;
using UltimateXR.Core.Serialization;

namespace UltimateXR.Core.StateSave
{
    /// <summary>
    ///     Base class for <see cref="UxrStateSaveImplementer{T}" />.
    ///     It provides access to global state serialization events and components.
    /// </summary>
    public abstract class UxrStateSaveImplementer
    {
        #region Public Types & Data

        /// <summary>
        ///     The name assigned to the Transform when an object's Transform is being serialized.
        /// </summary>
        public const string SelfTransformVarName = "this.transform";

        /// <summary>
        ///     Event called when the state is about to be serialized using
        ///     <see cref="UxrStateSaveImplementer{T}.SerializeState" />. The sender is the <see cref="IUxrStateSave" /> that is
        ///     about to be serialized.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> StateSerializing;

        /// <summary>
        ///     Event called when the state finished serializing using <see cref="UxrStateSaveImplementer{T}.SerializeState" />.
        ///     The sender is the <see cref="IUxrStateSave" /> that was serialized.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> StateSerialized;

        /// <summary>
        ///     Event called when a state variable is about to be serialized inside
        ///     <see cref="UxrStateSaveImplementer{T}.SerializeState" />. The sender is the <see cref="IUxrStateSave" /> that is
        ///     about to be serialized.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> VarSerializing;

        /// <summary>
        ///     Event called when a state variable finished serializing inside
        ///     <see cref="UxrStateSaveImplementer{T}.SerializeState" />. The sender is the <see cref="IUxrStateSave" /> that was
        ///     serialized.
        /// </summary>
        public static event EventHandler<UxrStateSaveEventArgs> VarSerialized;

        /// <summary>
        ///     Gets all the components with an <see cref="IUxrStateSave" /> interface that save any data.
        ///     The order will ensure that the first component will be the <see cref="UxrInstanceManager" /> if it exists, then all
        ///     the components with a <see cref="IUxrSingleton" /> interface and then the rest.<br />
        ///     To get the components sorted by <see cref="IUxrStateSave.SerializationOrder" /> use
        ///     <c>AllSerializableComponents.OrderBy(s => s.SerializationOrder);</c>
        /// </summary>
        public static IEnumerable<IUxrStateSave> AllSerializableComponents
        {
            get
            {
                if (UxrInstanceManager.HasInstance)
                {
                    yield return UxrInstanceManager.Instance;
                }

                foreach (IUxrStateSave stateSave in s_allSingletons)
                {
                    yield return stateSave;
                }

                foreach (IUxrStateSave stateSave in s_allComponents)
                {
                    yield return stateSave;
                }
            }
        }

        /// <summary>
        ///     Gets all the enabled components with an <see cref="IUxrStateSave" /> interface that save any data.
        ///     The order will ensure that the first component will be the <see cref="UxrInstanceManager" /> if it exists, then all
        ///     the components with a <see cref="IUxrSingleton" /> interface and then the rest.<br />
        ///     To get the components sorted by <see cref="IUxrStateSave.SerializationOrder" /> use
        ///     <c>EnabledSerializableComponents.OrderBy(s => s.SerializationOrder);</c>
        /// </summary>
        public static IEnumerable<IUxrStateSave> EnabledSerializableComponents
        {
            get
            {
                if (UxrInstanceManager.HasInstance && UxrInstanceManager.Instance.isActiveAndEnabled)
                {
                    yield return UxrInstanceManager.Instance;
                }

                foreach (IUxrStateSave stateSave in s_enabledSingletons)
                {
                    yield return stateSave;
                }

                foreach (IUxrStateSave stateSave in s_enabledComponents)
                {
                    yield return stateSave;
                }
            }
        }

        /// <summary>
        ///     Gets all the components that should be saved when saving the current state of the scene, such as
        ///     when using <see cref="UxrManager.SaveStateChanges" />.
        ///     This includes all the enabled components and the components with <see cref="IUxrStateSave.SaveStateWhenDisabled" />
        ///     set.<br />
        ///     To get the components sorted by <see cref="IUxrStateSave.SerializationOrder" /> use
        ///     <c>SaveRequiredComponents.OrderBy(s => s.SerializationOrder);</c>
        /// </summary>
        public static IEnumerable<IUxrStateSave> SaveRequiredComponents
        {
            get
            {
                if (UxrInstanceManager.HasInstance)
                {
                    yield return UxrInstanceManager.Instance;
                }

                foreach (IUxrStateSave stateSave in s_saveRequiredSingletons)
                {
                    yield return stateSave;
                }

                foreach (IUxrStateSave stateSave in s_saveRequiredComponents)
                {
                    yield return stateSave;
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Notifies the end of frame in the UltimateXR update process.
        /// </summary>
        internal static void NotifyEndOfFrame()
        {
            foreach (UxrStateSaveImplementer implementer in s_pendingStoreInitialStates)
            {
                implementer.StoreInitialState();
            }

            s_pendingStoreInitialStates.Clear();
        }

        #endregion

        #region Event Trigger Methods

        /// <summary>
        ///     Event trigger for <see cref="StateSerializing" />.
        /// </summary>
        /// <param name="stateSave">The <see cref="IUxrStateSave" /> that is about to be serialized</param>
        /// <param name="e">Event parameters</param>
        protected virtual void OnStateSerializing(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            StateSerializing?.Invoke(stateSave, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="StateSerialized" />.
        /// </summary>
        /// <param name="stateSave">The <see cref="IUxrStateSave" /> that was serialized</param>
        /// <param name="e">Event parameters</param>
        protected virtual void OnStateSerialized(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            StateSerialized?.Invoke(stateSave, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="VarSerializing" />.
        /// </summary>
        /// <param name="stateSave">The <see cref="IUxrStateSave" /> that is about to be serialized</param>
        /// <param name="e">Event parameters</param>
        protected virtual void OnVarSerializing(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            VarSerializing?.Invoke(stateSave, e);
        }

        /// <summary>
        ///     Event trigger for <see cref="VarSerialized" />.
        /// </summary>
        /// <param name="stateSave">The <see cref="IUxrStateSave" /> that was serialized</param>
        /// <param name="e">Event parameters</param>
        protected virtual void OnVarSerialized(IUxrStateSave stateSave, UxrStateSaveEventArgs e)
        {
            VarSerialized?.Invoke(stateSave, e);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Stores the initial state of a component.
        /// </summary>
        protected abstract void StoreInitialState();

        /// <summary>
        ///     Registers the component.
        /// </summary>
        protected void RegisterComponent(IUxrStateSave stateSave)
        {
            if (stateSave is UxrInstanceManager)
            {
                return;
            }

            // Do a serialization test and check if there is any state saving. If not we can ignore it because this component doesn't save any data.

            if (!stateSave.SerializeState(UxrDummySerializer.WriteModeSerializer, stateSave.StateSerializationVersion, UxrStateSaveLevel.Complete, UxrStateSaveOptions.DontSerialize | UxrStateSaveOptions.DontCacheChanges))
            {
                return;
            }

            if (stateSave is IUxrSingleton)
            {
                s_allSingletons.Add(stateSave);
                s_saveRequiredSingletons.Add(stateSave);
            }
            else
            {
                s_allComponents.Add(stateSave);
                s_saveRequiredComponents.Add(stateSave);
            }

            s_pendingStoreInitialStates.Add(this);
        }

        /// <summary>
        ///     Unregisters the component
        /// </summary>
        protected void UnregisterComponent(IUxrStateSave stateSave)
        {
            if (stateSave is UxrInstanceManager)
            {
                return;
            }

            if (stateSave is IUxrSingleton)
            {
                s_allSingletons.Remove(stateSave);
                s_saveRequiredSingletons.Remove(stateSave);
            }
            else
            {
                s_allComponents.Remove(stateSave);
                s_saveRequiredComponents.Remove(stateSave);
            }
        }

        /// <summary>
        ///     Notifies the component has been enabled.
        /// </summary>
        protected void NotifyOnEnable(IUxrStateSave stateSave)
        {
            if (stateSave is UxrInstanceManager)
            {
                return;
            }

            // We only register components that have been filtered by RegisterComponent, we want to discard components that don't save data.

            if (stateSave is IUxrSingleton)
            {
                if (s_allSingletons.Contains(stateSave))
                {
                    s_enabledSingletons.Add(stateSave);
                }

                if (!stateSave.SaveStateWhenDisabled)
                {
                    s_saveRequiredSingletons.Add(stateSave);
                }
            }
            else
            {
                if (s_allComponents.Contains(stateSave))
                {
                    s_enabledComponents.Add(stateSave);
                }

                if (!stateSave.SaveStateWhenDisabled)
                {
                    s_saveRequiredComponents.Add(stateSave);
                }
            }
        }

        /// <summary>
        ///     Notifies the component has been disabled.
        /// </summary>
        protected void NotifyOnDisable(IUxrStateSave stateSave)
        {
            if (stateSave is UxrInstanceManager)
            {
                return;
            }

            if (stateSave is IUxrSingleton)
            {
                s_enabledSingletons.Remove(stateSave);

                if (!stateSave.SaveStateWhenDisabled)
                {
                    s_saveRequiredSingletons.Remove(stateSave);
                }
            }
            else
            {
                s_enabledComponents.Remove(stateSave);

                if (!stateSave.SaveStateWhenDisabled)
                {
                    s_saveRequiredComponents.Remove(stateSave);
                }
            }
        }

        #endregion

        #region Private Types & Data

        private static readonly HashSet<IUxrStateSave>           s_allComponents             = new HashSet<IUxrStateSave>();
        private static readonly HashSet<IUxrStateSave>           s_enabledComponents         = new HashSet<IUxrStateSave>();
        private static readonly HashSet<IUxrStateSave>           s_saveRequiredComponents    = new HashSet<IUxrStateSave>();
        private static readonly HashSet<IUxrStateSave>           s_allSingletons             = new HashSet<IUxrStateSave>();
        private static readonly HashSet<IUxrStateSave>           s_enabledSingletons         = new HashSet<IUxrStateSave>();
        private static readonly HashSet<IUxrStateSave>           s_saveRequiredSingletons    = new HashSet<IUxrStateSave>();
        private static readonly HashSet<UxrStateSaveImplementer> s_pendingStoreInitialStates = new HashSet<UxrStateSaveImplementer>();

        #endregion
    }
}