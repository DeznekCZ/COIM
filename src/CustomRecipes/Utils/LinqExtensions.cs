using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAssets.Utils;
public static class LinqExtensions {
	public static void Loop<TElement>(this IEnumerable<TElement> enumerable) {
		foreach (TElement e in enumerable) {
			// DO nothing, only invoke the loop
		}
	}
	public static IEnumerable<string> ElementsToString<TElement>(this IEnumerable<TElement> enumerable) {
		foreach (TElement e in enumerable) {
			yield return e as string ?? (e?.ToString() ?? "");
		}
	}
}
