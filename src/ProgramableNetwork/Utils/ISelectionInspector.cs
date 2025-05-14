using Mafi.Core.Entities;
using Mafi.Unity.Camera;
using Mafi.Unity.Entities;

namespace ProgramableNetwork
{
    public interface ISelectionInspector<TEntity, TEntitySelector, TTarget>
        where TEntity : IEntity
        where TTarget : IEntity
        where TEntitySelector : IEntitySelector<TEntity>
    {
        TEntitySelector EntitySelectionInput { get; set; }
        TTarget Entity { get; }
        EntityHighlighter EntityHighlighter { get; }
        CameraController CameraController { get; }
    }
}