using Mafi.Unity.UiToolkit.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Data.DataBand
{
    public abstract class DataBandChannelUiComponent<TDataBand, TDataBandChannel> : UiComponent
        where TDataBand : IDataBand
        where TDataBandChannel : IDataBandChannel
    {
        protected readonly Antena m_antena;
        protected readonly IDataBand m_dataBand;
        protected readonly IDataBandChannel m_channel;

        public DataBandChannelUiComponent(Antena antena, IDataBand dataBand, IDataBandChannel channel) : base(new UnityEngine.UIElements.VisualElement())
        {
            this.m_antena = antena;
            this.m_dataBand = dataBand;
            this.m_channel = channel;
        }

        public abstract void InitializeUI();
    }
}
