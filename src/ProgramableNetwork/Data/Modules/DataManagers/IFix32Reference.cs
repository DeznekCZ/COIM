using Mafi;

namespace ProgramableNetwork
{
    public interface IReference<T>
        where T : new()
    {
        T this[string name, T defaultValue = default] { get; }

        T this[string name] { get; set; }
    }
}