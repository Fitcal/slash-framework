﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameBlueprintUtils.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.ECS.Blueprints
{
    using System.Collections.Generic;
    using System.Linq;

    using Slash.Collections.AttributeTables;
    using Slash.ECS.Components;

    /// <summary>
    ///   Utility methods for all issues related to blueprints of a game (e.g. entity creation,...).
    /// </summary>
    public static class GameBlueprintUtils
    {
        #region Public Methods and Operators

        public static List<int> CreateEntities(
            EntityManager entityManager,
            IBlueprintManager blueprintManager,
            IEnumerable<string> blueprintIds)
        {
            return
                blueprintIds.Select(
                    actionBlueprintId => CreateEntity(entityManager, blueprintManager, actionBlueprintId)).ToList();
        }

        /// <summary>
        ///   Searches for the blueprints with the specified ids and creates entities out of it.
        ///   If the blueprints are used several times, consider to fetch them from the game's
        ///   blueprint manager once and use them to create the entities.
        /// </summary>
        /// <param name="game">Game to get the blueprints from and create the entities in.</param>
        /// <param name="blueprintIds">Ids of blueprints to use.</param>
        /// <returns>Ids of created entities.</returns>
        public static List<int> CreateEntities(this Game game, IEnumerable<string> blueprintIds)
        {
            return blueprintIds.Select(actionBlueprintId => CreateEntity(game, actionBlueprintId)).ToList();
        }

        /// <summary>
        ///   Searches for the blueprint with the specified id and creates an entity out of it.
        ///   If the blueprint is used several times, consider to fetch the blueprint from the game's
        ///   blueprint manager once and use it to create the entities.
        /// </summary>
        /// <param name="game">Game to get the blueprint from and create the entity in.</param>
        /// <param name="blueprintId">Id of blueprint to use.</param>
        /// <returns>Id of created entity.</returns>
        public static int CreateEntity(this Game game, string blueprintId)
        {
            return CreateEntity(game, blueprintId, null);
        }

        public static int CreateEntity(
            EntityManager entityManager,
            IBlueprintManager blueprintManager,
            string blueprintId)
        {
            return CreateEntity(entityManager, blueprintManager, blueprintId, null);
        }

        /// <summary>
        ///   Searches for the blueprint with the specified id and creates an entity out of it.
        ///   If the blueprint is used several times, consider to fetch the blueprint from the game's
        ///   blueprint manager once and use it to create the entities.
        /// </summary>
        /// <param name="game">Game to get the blueprint from and create the entity in.</param>
        /// <param name="blueprintId">Id of blueprint to use.</param>
        /// <param name="configuration">Configuration to initialize the entity from.</param>
        /// <returns>Id of created entity.</returns>
        public static int CreateEntity(this Game game, string blueprintId, AttributeTable configuration)
        {
            return CreateEntity(game.EntityManager, game.BlueprintManager, blueprintId, configuration);
        }

        public static int CreateEntity(
            EntityManager entityManager,
            IBlueprintManager blueprintManager,
            string blueprintId,
            AttributeTable configuration)
        {
            var blueprint = blueprintManager.GetBlueprint(blueprintId);
            return entityManager.CreateEntity(blueprint, configuration);
        }

        #endregion
    }
}