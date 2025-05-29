using Mafi.Core;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Data.DisplayEntity
{
    public interface IDisplayEntityManager
    {
        DisplayEntity Entity { get; }
        DisplayEntityProto Proto { get; }
        DisplayEntityMb Mb { get; }

        void Init(DisplayEntityMb mb);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="panel"></param>
        /// <returns>Clearing function</returns>
        Action Inspector(DisplayEntityInspector panel);
        void RenderUpdate(GameTime time);
        void SyncUpdate(GameTime time);
    }

    public class SimpleDisplayManager<TContext> : IDisplayEntityManager
    {
        private readonly Func<DisplayEntity, DisplayEntityMb, TContext> m_initFunction;
        private readonly Action<DisplayEntity, DisplayEntityMb, GameTime, TContext> m_renderFunction;
        private readonly Action<DisplayEntity, DisplayEntityMb, GameTime, TContext> m_syncFunction;
        private readonly Func<DisplayEntity, DisplayEntityMb, DisplayEntityInspector, TContext, Action> m_inspectorFunction;

        public SimpleDisplayManager(DisplayEntity display,
            Func<DisplayEntity, DisplayEntityMb, TContext> initFunction,
            Action<DisplayEntity, DisplayEntityMb, GameTime, TContext> renderFunction = null,
            Action<DisplayEntity, DisplayEntityMb, GameTime, TContext> syncFunction = null,
            Func<DisplayEntity, DisplayEntityMb, DisplayEntityInspector, TContext, Action> inspectorFunction = null
        )
        {
            m_initFunction = initFunction;
            m_renderFunction = renderFunction;
            m_syncFunction = syncFunction;
            m_inspectorFunction = inspectorFunction;
        }

        public DisplayEntity Entity { get; }

        public DisplayEntityProto Proto => Entity.Prototype;

        public DisplayEntityMb Mb { get; private set; }

        private TContext m_context;

        public void Init(DisplayEntityMb mb)
        {
            Mb = mb;
            m_context = m_initFunction(Entity, Mb);
        }

        public void RenderUpdate(GameTime time)
        {
            m_renderFunction?.Invoke(Entity, Mb, time, m_context);
        }

        public void SyncUpdate(GameTime time)
        {
            m_syncFunction?.Invoke(Entity, Mb, time, m_context);
        }

        public Action Inspector(DisplayEntityInspector inspector)
        {
            return m_inspectorFunction?.Invoke(Entity, Mb, inspector, m_context);
        }
    }
}