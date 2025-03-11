using Mafi.Core.Entities;

namespace ProgramableNetwork
{
    public interface IDataBandChannel
    {
        Antena Antena { get; set; }
        void Update();
    }
}