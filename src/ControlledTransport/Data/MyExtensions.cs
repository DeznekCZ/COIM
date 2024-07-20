using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;

namespace ControlledTransport
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
        }

        public static T AppendTo<T>(this T element, StackContainer stackContainer) where T : IUiElement
        {
            stackContainer.Append(element, element.GetSize(), default, default, false);
            return element;
        }
    }
}
