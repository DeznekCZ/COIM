using Mafi.Core.Entities;

namespace CustomRecipes
{
    public interface IEntitySelector<TEntity>
        where TEntity : IEntity
    {
        bool EntityFilter(TEntity e);
    }
}