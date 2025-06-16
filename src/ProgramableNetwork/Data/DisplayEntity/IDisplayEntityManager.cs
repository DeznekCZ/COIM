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
        Ui.DisplayEntity.IDisplayEntityInspector Inspector { get; }

        void Init(DisplayEntityMb mb);
        void RenderUpdate(GameTime time);
        void SyncUpdate(GameTime time);
    }
}