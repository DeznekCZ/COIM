using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ProgramableNetwork
{
	/// <summary>
	/// Defers text application on UI components until the host is shown.
	///
	/// Why: <see cref="LocStr"/> snapshots its <c>TranslatedString</c> at construction. If a UI
	/// component is built before the underlying string is correctly translated, the rendered text
	/// is permanently the wrong (usually English) value. Capturing a *getter* and re-invoking it on
	/// every <c>OnShow</c> refreshes whatever the component displays.
	///
	/// Recommended usage pattern:
	/// 1. Declare <c>static readonly LocStr</c> fields **inside the UI class that uses them**
	///    (not centrally in <c>NewTr</c>). The class's static cctor runs the first time the class is
	///    referenced — typically when the UI is built, which is well after
	///    <c>ModTranslations.Load</c> has spliced translations into <c>s_data</c>. So the LocStr is
	///    born already-translated.
	/// 2. Pass that field through a getter to <c>LaterText</c>, with the host (window/dialog) as the
	///    second argument. On each show the host re-runs the getter against the component.
	///
	/// <code>
	/// public class MyDialog : FloatingColumn {
	///     private static readonly LocStr WindowTitle =
	///         Loc.Str("ProgramableNetwork_MyDialog_Title", "Configure", "");
	///
	///     public MyDialog() {
	///         this.Header.Add(new Label().LaterText(() => WindowTitle, this));
	///     }
	/// }
	/// </code>
	/// </summary>
	public static class LaterTextExtensions
	{
		// ConditionalWeakTable keys off the host so registrations are GC'd with it; no leak when
		// transient windows close. The list-of-actions doubles as the "is OnShow hooked yet" flag —
		// presence in the table means we've already attached the OnShow callback for this host.
		private static readonly ConditionalWeakTable<UiComponent, List<Action>> s_registry
			= new ConditionalWeakTable<UiComponent, List<Action>>();

		// Stable monotonic id assigned to each host the first time it shows up here; lets logs
		// correlate Register/OnShow/apply events even when ToString() collides.
		private static int s_nextHostId;
		private static readonly ConditionalWeakTable<UiComponent, HostTag> s_hostTags
			= new ConditionalWeakTable<UiComponent, HostTag>();

		private sealed class HostTag { public int Id; public string TypeName; }

		/// <summary>
		/// Registers a deferred text setter for a <see cref="Label"/>. Returns the label so it can be
		/// chained in builder-style code.
		/// </summary>
		public static Label LaterText(this Label label, Func<LocStrFormatted> getter, UiComponent host)
		{
			Register(host, () => label.Value(getter()));
			return label;
		}

		/// <summary>
		/// Overload accepting a <see cref="LocStr"/>-returning getter. Convenient when the source is a
		/// <c>NewTr</c>-style field/property whose return type is <c>LocStr</c>; the implicit conversion
		/// to <c>LocStrFormatted</c> happens inside the lambda.
		/// </summary>
		public static Label LaterText(this Label label, Func<LocStr> getter, UiComponent host)
		{
			Register(host, () => label.Value(getter()));
			return label;
		}

		/// <summary>
		/// Generic overload for any component type that has its own value-setter signature. Caller
		/// supplies the <paramref name="setter"/> that knows how to apply a <see cref="LocStrFormatted"/>
		/// to <typeparamref name="T"/> (e.g. <c>(d, v) =&gt; d.Value(v)</c> for a Display).
		/// </summary>
		public static T LaterText<T>(this T component, Func<LocStrFormatted> getter, UiComponent host,
			Action<T, LocStrFormatted> setter) where T : UiComponent
		{
			Register(host, () => setter(component, getter()));
			return component;
		}

		/// <summary>
		/// Generic overload accepting a <see cref="LocStr"/>-returning getter — implicit conversion
		/// from LocStr to LocStrFormatted is applied when invoking the setter.
		/// </summary>
		public static T LaterText<T>(this T component, Func<LocStr> getter, UiComponent host,
			Action<T, LocStrFormatted> setter) where T : UiComponent
		{
			Register(host, () => setter(component, getter()));
			return component;
		}

		private static void Register(UiComponent host, Action apply)
		{
			HostTag tag = GetTag(host);
			bool freshHost = false;
			List<Action> list = s_registry.GetValue(host, h =>
			{
				freshHost = true;
				List<Action> newList = new List<Action>();
				// Hook OnShow exactly once per host; subsequent registrations append into the same
				// list. The OnShow callback fires the pending applies once, then clears the list so
				// nothing re-evaluates on later shows — the component already has its final text.
				h.OnShow(() =>
				{
					int count = newList.Count;
					Log.Info($"[LaterText] OnShow fired host={tag.TypeName}#{tag.Id} pending={count}");
					for (int i = 0; i < count; i++)
					{
						try { newList[i](); }
						catch (Exception ex) { Log.Exception(ex, $"[LaterText] apply #{i} threw on host={tag.TypeName}#{tag.Id}"); }
					}
					newList.Clear();
					Log.Info($"[LaterText] OnShow finished host={tag.TypeName}#{tag.Id} cleared");
				});
				Log.Info($"[LaterText] Registry created host={tag.TypeName}#{tag.Id}");
				return newList;
			});
			list.Add(apply);
			Log.Info($"[LaterText] Registered host={tag.TypeName}#{tag.Id} freshHost={freshHost} totalPending={list.Count}");
			// Apply once immediately so the component isn't blank until first show. If translations
			// aren't loaded yet, this initial call captures English; OnShow will refresh later.
			try { apply(); }
			catch (Exception ex) { Log.Exception(ex, $"[LaterText] initial apply threw on host={tag.TypeName}#{tag.Id}"); }
		}

		private static HostTag GetTag(UiComponent host)
		{
			return s_hostTags.GetValue(host, h => new HostTag
			{
				Id = Interlocked.Increment(ref s_nextHostId),
				TypeName = h.GetType().Name
			});
		}
	}
}
