﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InspectorComponentAttribute.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.ECS.Inspector.Attributes
{
    /// <summary>
    ///   Exposes the component to the inspector.
    /// </summary>
    public class InspectorComponentAttribute : InspectorTypeAttribute
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Constructor.
        /// </summary>
        public InspectorComponentAttribute()
        {
            this.CanBeRemoved = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Whether this component can be removed from a blueprint in the inspector, or not.
        ///   Default: true.
        /// </summary>
        public bool CanBeRemoved { get; set; }

        /// <summary>
        ///   Whether to skip initializing this component with reflection.
        /// </summary>
        public bool InitExplicitly { get; set; }

        /// <summary>
        ///   Component priority. Specifies the position the component appears within lists.
        ///   A lower value indicates a higher priority.
        /// </summary>
        public int Priority { get; set; }

        #endregion
    }
}