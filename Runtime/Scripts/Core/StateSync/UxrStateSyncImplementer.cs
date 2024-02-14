// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrStateSyncImplementer.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace UltimateXR.Core.StateSync
{
    /// <summary>
    ///     Base class for <see cref="UxrStateSyncImplementer{T}" />.
    /// </summary>
    public class UxrStateSyncImplementer
    {
        #region Public Types & Data

        /// <summary>
        ///     <para>
        ///         Gets the current call depth of BeginSync/EndSync calls, which are responsible for helping synchronize calls
        ///         over the network.
        ///         To avoid redundant synchronization, nested calls (where <see cref="SyncCallDepth" /> is greater than 1),
        ///         need to be ignored.
        ///     </para>
        ///     <para>
        ///         State synchronization, for networking or other functionality like saving gameplay replays, can be done
        ///         by subscribing to <see cref="UxrManager.ComponentStateChanged" />. By default, only top level calls will
        ///         trigger the event. This can be changed using <see cref="UxrManager.UseTopLevelStateChangesOnly" />.
        ///     </para>
        ///     <para>
        ///         In the following code, only PlayerShoot() needs to be synchronized. This will not only save bandwidth, but also
        ///         make sure that only a single particle system gets instantiated and the shot audio doesn't get played twice.
        ///     </para>
        ///     <code>
        ///         void PlayerShoot(int parameter1, bool parameter2)
        ///         {
        ///             BeginSync(); 
        ///             ShowParticles(parameter1);
        ///             PlayAudioShot(parameter2); 
        ///             EndSyncMethod(new object[] {parameter1, parameter2});
        ///         }
        ///         
        ///         void ShowParticles(int parameter);
        ///         {
        ///             BeginSync();
        ///             Instantiate(ParticleSystem);
        ///             EndSyncMethod(new object[] {parameter});
        ///         }
        ///         
        ///         void PlayAudioShot(bool parameter);
        ///         {
        ///             BeginSync();
        ///             _audio.Play();
        ///             EndSyncMethod(new object[] {parameter});
        ///         }
        ///     </code>
        /// </summary>
        public static int SyncCallDepth { get; protected set; }

        #endregion
    }
}