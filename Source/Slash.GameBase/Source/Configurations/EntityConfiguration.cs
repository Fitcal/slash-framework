﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityConfiguration.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.GameBase.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Slash.Collections.AttributeTables;
    using Slash.Collections.Utils;
    using Slash.Reflection.Utils;
    using Slash.Serialization.Binary;

    /// <summary>
    ///   Contains all data to create and initialize an entity.
    /// </summary>
    [Serializable]
#if !WINDOWS_STORE
    public class EntityConfiguration : ICloneable, IBinarySerializable
#else
    public class EntityConfiguration : IBinarySerializable
#endif
    {
        #region Fields

        /// <summary>
        ///   Entity components that are added to the ones specified in the blueprint.
        /// </summary>
        private List<Type> additionalComponentTypes;

        /// <summary>
        ///   Configuration for the entity.
        /// </summary>
        private AttributeTable configuration;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Constructor.
        /// </summary>
        public EntityConfiguration()
        {
        }

        /// <summary>
        ///   Copy constructor.
        /// </summary>
        /// <param name="entityConfiguration">Configuration to copy.</param>
        public EntityConfiguration(EntityConfiguration entityConfiguration)
        {
            this.BlueprintId = entityConfiguration.BlueprintId;
            this.additionalComponentTypes = entityConfiguration.additionalComponentTypes != null
                                                ? new List<Type>(entityConfiguration.additionalComponentTypes)
                                                : null;
            this.configuration = entityConfiguration.configuration != null
                                     ? new AttributeTable(entityConfiguration.configuration)
                                     : null;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Entity components that are added to the ones specified in the blueprint.
        /// </summary>
        public List<Type> AdditionalComponentTypes
        {
            get
            {
                return this.additionalComponentTypes ?? (this.additionalComponentTypes = new List<Type>());
            }

            set
            {
                this.additionalComponentTypes = value;
            }
        }

        /// <summary>
        ///   Name of blueprint to use for the entity.
        /// </summary>
        public string BlueprintId { get; set; }

        /// <summary>
        ///   Configuration for the entity.
        /// </summary>
        public AttributeTable Configuration
        {
            get
            {
                return this.configuration ?? (this.configuration = new AttributeTable());
            }

            set
            {
                this.configuration = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///   A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new EntityConfiguration(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((EntityConfiguration)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.additionalComponentTypes != null ? this.additionalComponentTypes.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (this.configuration != null ? this.configuration.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.BlueprintId != null ? this.BlueprintId.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "BlueprintId: {0}, Configuration: {1}, AdditionalComponentTypes: {2}",
                this.BlueprintId,
                this.Configuration,
                this.AdditionalComponentTypes);
        }

        #endregion

        #region Methods

        protected bool Equals(EntityConfiguration other)
        {
            return CollectionUtils.SequenceEqual(this.AdditionalComponentTypes, other.AdditionalComponentTypes)
                   && Equals(this.Configuration, other.Configuration)
                   && string.Equals(this.BlueprintId, other.BlueprintId);
        }

        #endregion

        public void Deserialize(BinaryDeserializer serializer)
        {
            this.AdditionalComponentTypes = serializer.Deserialize<string[]>().Select(ReflectionUtils.FindType).ToList();
            this.BlueprintId = serializer.Deserialize<string>();
            this.Configuration = serializer.Deserialize<AttributeTable>();
        }

        public void Serialize(BinarySerializer serializer)
        {
            serializer.Serialize(
                this.AdditionalComponentTypes.Select(componentType => componentType.FullName).ToArray());
            serializer.Serialize(string.IsNullOrEmpty(this.BlueprintId) ? string.Empty : this.BlueprintId);
            serializer.Serialize(this.Configuration);
        }
    }
}