using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;

namespace ControlledTransport
{
    public static class ImmutableArrayExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
        }
    }
}
