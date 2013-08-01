﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityManager.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.GameBase
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Slash.Collections.AttributeTables;
    using Slash.Collections.ObjectModel;
    using Slash.GameBase.EventData;

    /// <summary>
    ///   Creates and removes game entities. Holds references to all component
    ///   managers, delegating all calls for adding or removing components.
    /// </summary>
    public class EntityManager
    {
        #region Fields

        /// <summary>
        ///   Managers that are mapping entity ids to specific components.
        /// </summary>
        private readonly Dictionary<Type, ComponentManager> componentManagers;

        /// <summary>
        ///   All active entity ids.
        /// </summary>
        private readonly HashSet<int> entities;

        /// <summary>
        ///   Game this manager controls the entities of.
        /// </summary>
        private readonly Game game;

        /// <summary>
        ///   Ids of all entities that have been removed in this tick.
        /// </summary>
        private readonly HashSet<int> removedEntities;

        /// <summary>
        ///   Id that will be assigned to the next entity created.
        /// </summary>
        private int nextEntityId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Constructs a new entity manager without any initial entities.
        /// </summary>
        /// <param name="game"> Game to manage the entities for. </param>
        public EntityManager(Game game)
        {
            this.game = game;
            this.nextEntityId = 1;
            this.entities = new HashSet<int>();
            this.removedEntities = new HashSet<int>();
            this.componentManagers = new Dictionary<Type, ComponentManager>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Read-only collection of all entities.
        /// </summary>
        public IEnumerable<int> Entities
        {
            get
            {
                return new ReadOnlyCollection<int>(this.entities);
            }
        }

        /// <summary>
        ///   Total number of entities managed by this EntityManager instance.
        /// </summary>
        public int EntityCount
        {
            get
            {
                return this.entities.Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///   Attaches the passed component to the entity with the specified id.
        /// </summary>
        /// <param name="entityId"> Id of the entity to attach the component to. </param>
        /// <param name="entityComponent"> Component to attach. </param>
        /// <exception cref="ArgumentOutOfRangeException">Entity id is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Entity id has not yet been assigned.</exception>
        /// <exception cref="ArgumentException">Entity with the specified id has already been removed.</exception>
        /// <exception cref="ArgumentNullException">Passed component is null.</exception>
        /// <exception cref="InvalidOperationException">There is already a component of the same type attached.</exception>
        public void AddComponent(int entityId, IEntityComponent entityComponent)
        {
            this.CheckEntityId(entityId);

            Type componentType = entityComponent.GetType();

            if (!this.componentManagers.ContainsKey(componentType))
            {
                this.componentManagers.Add(componentType, new ComponentManager(this.game));
            }

            this.componentManagers[entityComponent.GetType()].AddComponent(entityId, entityComponent);
        }

        /// <summary>
        ///   Removes all entities that have been issued for removal during the
        ///   current tick, detaching all components.
        /// </summary>
        public void CleanUpEntities()
        {
            foreach (int id in this.removedEntities)
            {
                foreach (ComponentManager manager in this.componentManagers.Values)
                {
                    manager.RemoveComponent(id);
                }

                this.entities.Remove(id);
            }

            this.removedEntities.Clear();
        }

        /// <summary>
        ///   Returns an iterator over all components of the specified type.
        /// </summary>
        /// <param name="type"> Type of the components to get. </param>
        /// <returns> Components of the specified type. </returns>
        /// <exception cref="ArgumentNullException">Specified type is null.</exception>
        public IEnumerable ComponentsOfType(Type type)
        {
            ComponentManager componentManager;

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!this.componentManagers.TryGetValue(type, out componentManager))
            {
                yield break;
            }

            foreach (KeyValuePair<int, IEntityComponent> component in componentManager.Components())
            {
                yield return component;
            }
        }

        /// <summary>
        ///   Creates a new entity.
        /// </summary>
        /// <returns> Unique id of the new entity. </returns>
        public int CreateEntity()
        {
            int id = this.nextEntityId++;
            this.entities.Add(id);
            this.game.EventManager.QueueEvent(FrameworkEventType.EntityCreated, id);
            return id;
        }

        /// <summary>
        ///   Creates a new entity, adding components matching the passed
        ///   blueprint and initializing these with the data stored in the
        ///   blueprint and the specified configuration. Configuration data
        ///   is preferred over blueprint data.
        /// </summary>
        /// <param name="blueprint"> Blueprint describing the entity to create. </param>
        /// <param name="configuration"> Data for initializing the entity. </param>
        /// <returns> Unique id of the new entity. </returns>
        public int CreateEntity(Blueprint blueprint, IAttributeTable configuration)
        {
            int id = this.CreateEntity();
            this.InitEntity(id, blueprint, configuration);
            return id;
        }

        /// <summary>
        ///   Checks whether the entity with the passed id has been removed or
        ///   not.
        /// </summary>
        /// <param name="entityId"> Id of the entity to check. </param>
        /// <returns>
        ///   <c>false</c> , if the entity has been removed, and <c>true</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Entity id is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Entity id has not yet been assigned.</exception>
        public bool EntityIsAlive(int entityId)
        {
            if (entityId < 0)
            {
                throw new ArgumentOutOfRangeException("entityId", "Entity ids are always non-negative.");
            }

            if (entityId >= this.nextEntityId)
            {
                throw new ArgumentOutOfRangeException(
                    "entityId", "Entity id " + entityId + " has not yet been assigned.");
            }

            return this.entities.Contains(entityId);
        }

        /// <summary>
        ///   Gets a component of the passed type attached to the entity with the specified id.
        /// </summary>
        /// <param name="entityId"> Id of the entity to get the component of. </param>
        /// <param name="componentType"> Type of the component to get. </param>
        /// <returns> The component, if there is one of the specified type attached to the entity, and null otherwise. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Entity id is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Entity id has not yet been assigned.</exception>
        /// <exception cref="ArgumentException">Entity with the specified id has already been removed.</exception>
        /// <exception cref="ArgumentNullException">Passed component type is null.</exception>
        public IEntityComponent GetComponent(int entityId, Type componentType)
        {
            this.CheckEntityId(entityId);

            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }

            // Get component manager.
            ComponentManager componentManager;
            return this.componentManagers.TryGetValue(componentType, out componentManager)
                       ? componentManager.GetComponent(entityId)
                       : null;
        }

        /// <summary>
        ///   Gets a component of the passed type attached to the entity with the specified id.
        /// </summary>
        /// <param name="entityId"> Id of the entity to get the component of. </param>
        /// <typeparam name="T"> Type of the component to get. </typeparam>
        /// <returns> The component, if there is one of the specified type attached to the entity, and null otherwise. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Entity id is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Entity id has not yet been assigned.</exception>
        /// <exception cref="ArgumentException">Entity with the specified id has already been removed.</exception>
        /// <exception cref="ArgumentNullException">Passed component type is null.</exception>
        /// <exception cref="ArgumentException">A component of the passed type has never been added before.</exception>
        public T GetComponent<T>(int entityId) where T : IEntityComponent
        {
            return (T)this.GetComponent(entityId, typeof(T));
        }

        /// <summary>
        ///   Retrieves an array containing the ids of all living entities in
        ///   O(n).
        /// </summary>
        /// <returns> Array containing the ids of all entities that haven't been removed yet. </returns>
        public int[] GetEntities()
        {
            if (this.entities.Count == 0)
            {
                return null;
            }

            int[] entityArray = new int[this.entities.Count];
            this.entities.CopyTo(entityArray);
            return entityArray;
        }

        /// <summary>
        ///   Returns the entity ids of all entities which fulfill the specified predicate.
        /// </summary>
        /// <param name="predicate"> Predicate to fulfill. </param>
        /// <returns> Collection of ids of all entities which fulfill the specified predicate. </returns>
        public IEnumerable<int> GetEntities(Func<int, bool> predicate)
        {
            return this.entities.Count == 0 ? null : this.entities.Where(predicate);
        }

        /// <summary>
        ///   Convenience method for retrieving components from two entities
        ///   in case the order of the entities is unknown.
        /// </summary>
        /// <typeparam name="TComponentTypeA">Type of the first component to get.</typeparam>
        /// <typeparam name="TComponentTypeB">Type of the second component to get.</typeparam>
        /// <param name="data">Data for the event that affected two entities.</param>
        /// <param name="entityIdA">Id of the entity having the first component attached.</param>
        /// <param name="entityIdB">Id of the entity having the second component attached.</param>
        /// <param name="componentA">First component.</param>
        /// <param name="componentB">Second component.</param>
        /// <returns>
        ///   <c>true</c>, if one of the entities has a <typeparamref name="TComponentTypeA" />
        ///   and the other one a <typeparamref name="TComponentTypeB" /> attached,
        ///   and <c>false</c> otherwise.
        /// </returns>
        public bool GetEntityComponents<TComponentTypeA, TComponentTypeB>(
            Entity2Data data,
            out int entityIdA,
            out int entityIdB,
            out TComponentTypeA componentA,
            out TComponentTypeB componentB) where TComponentTypeA : class, IEntityComponent
            where TComponentTypeB : class, IEntityComponent
        {
            entityIdA = data.First;
            entityIdB = data.Second;

            componentA = this.GetComponent<TComponentTypeA>(entityIdA);
            componentB = this.GetComponent<TComponentTypeB>(entityIdB);

            if (componentA == null || componentB == null)
            {
                // Check other way round.
                entityIdA = data.Second;
                entityIdB = data.First;

                componentA = this.GetComponent<TComponentTypeA>(entityIdA);
                componentB = this.GetComponent<TComponentTypeB>(entityIdB);

                return componentA != null && componentB != null;
            }

            return true;
        }

        /// <summary>
        ///   Initializes the specified entity, adding components matching the
        ///   passed blueprint and initializing these with the data stored in
        ///   the blueprint and the specified configuration. Configuration
        ///   data is preferred over blueprint data.
        /// </summary>
        /// <param name="entityId">Id of the entity to initialize.</param>
        /// <param name="blueprint"> Blueprint describing the entity to create. </param>
        /// <param name="configuration"> Data for initializing the entity. </param>
        public void InitEntity(int entityId, Blueprint blueprint, IAttributeTable configuration)
        {
            foreach (Type type in blueprint.ComponentTypes)
            {
                // Create component.
                IEntityComponent entityComponent = (IEntityComponent)Activator.CreateInstance(type);
                this.AddComponent(entityId, entityComponent);

                // Initialize component with the attribute table data.
                HierarchicalAttributeTable attributeTable = new HierarchicalAttributeTable();
                if (configuration != null)
                {
                    attributeTable.AddParent(configuration);
                }

                if (blueprint.AttributeTable != null)
                {
                    attributeTable.AddParent(blueprint.AttributeTable);
                }

                entityComponent.InitComponent(attributeTable);
            }

            this.game.EventManager.QueueEvent(FrameworkEventType.EntityInitialized, entityId);
        }

        /// <summary>
        ///   Removes a component of the passed type from the entity with the specified id.
        /// </summary>
        /// <param name="entityId"> Id of the entity to remove the component from. </param>
        /// <param name="componentType"> Type of the component to remove. </param>
        /// <returns> Whether a component has been removed, or not. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Entity id is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Entity id has not yet been assigned.</exception>
        /// <exception cref="ArgumentException">Entity with the specified id has already been removed.</exception>
        /// <exception cref="ArgumentNullException">Passed component type is null.</exception>
        /// <exception cref="ArgumentException">A component of the passed type has never been added before.</exception>
        public bool RemoveComponent(int entityId, Type componentType)
        {
            this.CheckEntityId(entityId);

            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }

            ComponentManager componentManager;

            if (!this.componentManagers.TryGetValue(componentType, out componentManager))
            {
                throw new ArgumentException(
                    "A component of type " + componentType + " has never been added before.", "componentType");
            }

            return componentManager.RemoveComponent(entityId);
        }

        /// <summary>
        ///   Issues the entity with the specified id for removal at the end of
        ///   the current tick.
        /// </summary>
        /// <param name="id"> Id of the entity to remove. </param>
        /// <exception cref="ArgumentOutOfRangeException">Entity id is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Entity id has not yet been assigned.</exception>
        /// <exception cref="ArgumentException">Entity with the specified id has already been removed.</exception>
        public void RemoveEntity(int id)
        {
            this.CheckEntityId(id);

            this.game.EventManager.QueueEvent(FrameworkEventType.EntityRemoved, id);

            this.removedEntities.Add(id);
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Checks whether the passed entity is valid, throwing an exception if not.
        /// </summary>
        /// <param name="id"> Entity id to check. </param>
        /// <exception cref="ArgumentOutOfRangeException">Entity id is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Entity id has not yet been assigned.</exception>
        /// <exception cref="ArgumentException">Entity with the specified id has already been removed.</exception>
        private void CheckEntityId(int id)
        {
            if (!this.EntityIsAlive(id))
            {
                throw new ArgumentException("id", "The entity with id " + id + " has already been removed.");
            }
        }

        #endregion
    }
}