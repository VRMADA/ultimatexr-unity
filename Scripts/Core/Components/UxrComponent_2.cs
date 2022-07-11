// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UxrComponent_2.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Avatar;
using UltimateXR.Core.Components.Composite;
using UnityEngine;

namespace UltimateXR.Core.Components
{
    /// <summary>
    ///     <para>
    ///         Like <see cref="UxrComponent{T}" /> but the component belongs to a hierarchy
    ///         with a parent that has a component of a certain type <typeparamref name="TP" />.
    ///         This allows to enumerate and keep track of only the components that hang from the hierarchy
    ///         under each parent component separately.
    ///     </para>
    ///     <para>
    ///         In the case of keeping track of all components of a same type that are in or hang from an avatar (
    ///         <see cref="UxrAvatar" />) there is a special component for that in <see cref="UxrAvatarComponent{T}" />.
    ///     </para>
    /// </summary>
    /// <typeparam name="TP">Parent component type</typeparam>
    /// <typeparam name="TC">Component type</typeparam>
    public abstract class UxrComponent<TP, TC> : UxrComponent<TC>
                where TP : Component
                where TC : UxrComponent<TC>
    {
        #region Public Types & Data

        /// <summary>
        ///     Gets all the components, enabled of not, of this specific type that hang from the same parent.
        /// </summary>
        /// <remarks>
        ///     Components that have never been enabled are not returned. Components are automatically registered through their
        ///     Awake() call, which is never called if the object has never been enabled. In this case it is recommended to resort
        ///     to <see cref="GameObject.GetComponentsInChildren{T}(bool)" />.
        /// </remarks>
        public IEnumerable<TC> AllChildrenFromParent => GetParentChildren(Parent, true);

        /// <summary>
        ///     Gets only the enabled components of this specific type that hang from the same parent.
        /// </summary>
        public IEnumerable<TC> EnabledChildrenFromParent => GetParentChildren(Parent);

        /// <summary>
        ///     Parent the component belongs to.
        /// </summary>
        public TP Parent
        {
            get
            {
                if (_parent == null)
                {
                    _parent = GetComponentInParent<TP>();
                }
                return _parent;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the children from a specific parent.
        /// </summary>
        /// <param name="parent">Parent to get the components from</param>
        /// <param name="includeDisabled">Whether to include disabled components or not</param>
        /// <returns>Components meeting the criteria</returns>
        /// <remarks>
        ///     When using the <paramref name="includeDisabled" /> parameter, components that have never been enabled are not
        ///     returned. Components are automatically registered through their Awake() call, which is never called if the object
        ///     has never been enabled. In this case it is recommended to resort to
        ///     <see cref="GameObject.GetComponentsInChildren{T}(bool)" />.
        /// </remarks>
        public static IEnumerable<TC> GetParentChildren(TP parent, bool includeDisabled = false)
        {
            if (includeDisabled)
            {
                return AllComponents.Where(c => c is UxrComponent<TP, TC> child && child.Parent == parent);
            }
            return AllComponents.Where(c => c.isActiveAndEnabled && c is UxrComponent<TP, TC> child && child.Parent == parent);
        }

        #endregion

        #region Unity

        /// <summary>
        ///     Pre-caches the root parent's component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _parent = GetComponentInParent<TP>();
        }

        #endregion

        #region Private Types & Data

        private TP _parent;

        #endregion
    }
}