using Mafi.Core.Entities;

namespace CustomRecipes
{
    public interface ISelectionInspector<TEntity, TEntitySelector, TTarget>
        where TEntity : IEntity
        where TTarget : IEntity
        where TEntitySelector : IEntitySelector<TEntity>
    {
        TEntitySelector EntitySelectionInput { get; set; }
        TTarget SelectedEntity { get; }
    }
}