using Mafi;
using Mafi.Core;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
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
}