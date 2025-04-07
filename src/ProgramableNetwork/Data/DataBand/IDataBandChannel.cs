using Mafi.Core.Entities;
using Mafi.Unity.UiToolkit.Component;
using System;

namespace ProgramableNetwork
{
    public interface IDataBandChannel
    {
        UiComponent CreateUI(Antena antena, IDataBand databand, IDataBandChannel channel, Action remove);
        void Update();
    }
}