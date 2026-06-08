using System.Collections.Generic;
using Mafi;

namespace ProgramableNetwork.Runtime.Sim
{
    /// <summary>Browser stand-in for a controller: holds the placed modules and the named buses.</summary>
    public sealed class SimController
    {
        public int Rows = 4;
        public int Columns = 18;

        public readonly List<SimModule> Modules = new List<SimModule>();
        public readonly List<SimBus> Buses = new List<SimBus>();

        public SimBus GetBus(string name)
        {
            foreach (SimBus b in Buses)
            {
                if (b.Name == name)
                {
                    return b;
                }
            }
            return null;
        }

        public SimBus GetBusById(long id)
        {
            foreach (SimBus b in Buses)
            {
                if (b.Id == id)
                {
                    return b;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Mirror of the mod's <c>ControllerBus</c> as the interpreter sees it: a strip of
    /// <see cref="PinCount"/> latching Fix32 pins addressable by name or index.
    /// </summary>
    public sealed class SimBus
    {
        public const int PinCount = 4;

        public long Id;
        public string Name;
        public readonly string[] PinNames = new string[PinCount];
        public readonly Fix32[] PinValues = new Fix32[PinCount];

        public int IndexOfPin(string pinName)
        {
            for (int i = 0; i < PinCount; i++)
            {
                if (PinNames[i] == pinName)
                {
                    return i;
                }
            }
            return -1;
        }

        public Fix32 Get(string pinName)
        {
            int i = IndexOfPin(pinName);
            return i >= 0 ? PinValues[i] : Fix32.Zero;
        }

        public void Set(string pinName, Fix32 value)
        {
            int i = IndexOfPin(pinName);
            if (i >= 0)
            {
                PinValues[i] = value;
            }
        }
    }
}
