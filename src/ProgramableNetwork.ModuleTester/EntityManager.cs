using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Validators;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork.ModuleTester
{
    internal class EntityManager : IEntitiesManager
    {
        public IEvent<IEntity> EntityAdded => throw new NotImplementedException();

        public IEvent<IEntity, EntityAddReason> EntityAddedFull => throw new NotImplementedException();

        public IEvent<IEntity> EntityRemoved => throw new NotImplementedException();

        public IEvent<IEntity, EntityRemoveReason> EntityRemovedFull => throw new NotImplementedException();

        public IEvent<IStaticEntity> StaticEntityAdded => throw new NotImplementedException();

        public IEvent<IStaticEntity> StaticEntityRemoved => throw new NotImplementedException();

        public IEvent<IUpgradableEntity> OnUpgradeToBePerformed => throw new NotImplementedException();

        public IEvent<IUpgradableEntity, IEntityProto> OnUpgradeJustPerformed => throw new NotImplementedException();

        public IEvent<IEntity, bool> EntityPauseStateChanged => throw new NotImplementedException();

        public IEvent<IEntity, bool> EntityEnabledChanged => throw new NotImplementedException();

        public IEventNonSaveable<IEntity> OnEntityVisualChanged => throw new NotImplementedException();

        public IIndexable<IEntity> Entities => throw new NotImplementedException();

        public EntityValidationResult CanRemoveEntity(IEntity entity, EntityRemoveReason reasonToRemove)
        {
            throw new NotImplementedException();
        }

        public Option<IEntity> GetEntity(EntityId id)
        {
            throw new NotImplementedException();
        }

        public void InvokeOnEntityVisualChanged(IEntity entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveAndDestroyEntityNoChecks(IEntity entity, EntityRemoveReason reasonToRemove)
        {
            throw new NotImplementedException();
        }

        public void TryUpgradeEntity(IUpgradableEntity entity)
        {
            throw new NotImplementedException();
        }

        public void UpdatePropertiesForAllEntities(ImmutableArray<Type> types)
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IEntitiesManager.GetAllEntitiesOfType<T>()
        {
            throw new NotImplementedException();
        }

        Option<T> IEntitiesManager.GetEntity<T>(EntityId id)
        {
            throw new NotImplementedException();
        }

        bool IEntitiesManager.TryGetEntity<T>(EntityId id, out T entity)
        {
            entity = default(T);
            return false;
        }
    }
}