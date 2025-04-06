using Mafi.Core.Entities;

namespace Mafi.Unity
{
    public interface IEntitySelector<TEntity>
        where TEntity : IEntity
    {
        bool EntityFilter(TEntity e);
    }
}