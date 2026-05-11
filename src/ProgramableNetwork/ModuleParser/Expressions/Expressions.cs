using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ProgramableNetwork.Python
{
    public class Expressions
    {
        public static Dictionary<Type, Func<IArgumentValue[], object>> Initializers =
        new ()
        {
            { typeof(ColorRgba), (args) => new ColorRgba(
                (byte)(int)args[0].Value,
                (byte)(int)args[1].Value,
                (byte)(int)args[2].Value,
                (byte)(int)(args.Length == 4 ? args[3].Value : 255)
            ) }
        };

        public static bool __bool__(object v)
        {
            return v is bool b ? b
                : v is int i ? i != 0
                : v is float f ? f != 0
                : v is Fix32 fix ? fix.RawValue != 0
                : v is byte by ? by != 0
                : v is short sh ? sh != 0
                : throw new NotImplementedException("Can not convert to __bool__");
        }

        public static int __int__(object v)
        {
            return v is int i ? i
                : v is float f ? (int)f
                : v is Fix32 fix ? fix.IntegerPart
                : v is bool b ? (b ? 1 : 0)
                : v is byte by ? by
                : v is short sh ? sh
                : throw new NotImplementedException("Can not convert to __int__");
        }

        public static float __float__(object v)
        {
            return v is float f ? f
                : v is Fix32 fix ? fix.ToFloat()
                : v is int i ? i
                : v is bool b ? (b ? 1 : 0)
                : v is byte by ? by
                : v is short sh ? sh
                : throw new NotImplementedException("Can not convert to __float__");
        }

        public static Fix32 __fix__(object v)
        {
            return v is Fix32 fix ? fix
                : v is float f ? f.ToFix32()
                : v is int i ? i.ToFix32()
                : v is bool b ? (b ? 1.ToFix32() : 0.ToFix32())
                : v is byte by ? ((int)by).ToFix32()
                : v is short sh ? ((int)sh).ToFix32()
                : throw new NotImplementedException("Can not convert to __fix__");
        }

        // Raw fixed-point representation of a Fix32 (the underlying int the
        // type stores internally).  Inverse of Fix32.FromRaw — pairs with it
        // for save/load or precise arithmetic that needs to bypass Fix32's
        // normal scaling.  Other numeric types pass through their integer
        // value so `raw(5)` doesn't surprise a player who expected a number.
        public static int __raw__(object v)
        {
            return v is Fix32 fix ? fix.RawValue
                : v is int i ? i
                : v is bool b ? (b ? 1 : 0)
                : v is byte by ? by
                : v is short sh ? sh
                : throw new NotImplementedException("Can not convert to __raw__");
        }

        public static string __str__(object v)
        {
            if (v is null) {
				return "None";
			}
			if (v is string s) {
				return s;
			}
			if (v is int i) {
				return i.ToString();
			}
			if (v is float f) {
				return f.ToString();
			}
			if (v is Fix32 fix) {
				return fix.ToString();
			}
			if (v is byte by) {
				return by.ToString();
			}
			if (v is short sh) {
				return sh.ToString();
			}
			System.Reflection.MethodInfo m;
            if ((m = v.GetType().GetMethod("__str__")) != null) {
				return (string)m.Invoke(v, null);
			}
			return v.ToString(); // nouzovka
        }

        public static object __neg__(object v)
        {
            return v is float f ? -f
                : v is Fix32 fix ? (object)(-fix)
                : v is int i ? -i
                : v is bool b ? (b ? -1 : 0)
                : v is byte by ? -by
                : v is short sh ? -sh
                : throw new NotImplementedException("Can not call __neg__");
        }

        public static object __pos__(object v)
        {
            return v is float f ? f
                : v is Fix32 fix ? (object)(fix)
                : v is int i ? i
                : v is bool b ? (b ? 1 : 0)
                : v is byte by ? by
                : v is short sh ? sh
                : throw new NotImplementedException("Can not call __pos__");
        }

        public static object __or__(object v1, object v2)
        {
            if (v1 is null && v2 is null) {
				return 0;
			}
			return __int__(v1) | __int__(v2);
        }

        public static object __xor__(object v1, object v2)
        {
            if (v1 is null && v2 is null) {
				return 0;
			}
			return __int__(v1) ^ __int__(v2);
        }

        public static object __and__(object v1, object v2)
        {
            if (v1 is null && v2 is null) {
				return 0;
			}
			return __int__(v1) & __int__(v2);
        }

        public static object __call__(object executable, List<(string name, object value)> arguments)
        {
            if (executable is Constructor constructor)
            {
                return constructor.Invoke(arguments.Select(a =>
                    a.name == null
                        ? (IArgumentValue)new OrderedValue(a.value)
                        : (IArgumentValue)new NamedValue(a.name, a.value))
                    .ToArray());
            }
            if (executable is MemberCall member)
            {
                object[] values = arguments
                    .Select(a => a.value)
                    .ToArray();

                // Pick the overload whose parameter count matches the call
                // site.  Was previously always invoking member.Type[0], which
                // meant any method with multiple overloads (e.g. ArraySetter
                // .resize(size) vs .resize(size, fill_new)) ran the first
                // registered one and threw "argument count does not match"
                // for every other shape.  Exact arity match wins; if none
                // matches, fall through to the first overload so the original
                // mismatch error still surfaces with a meaningful message.
                MethodInfo chosen = null;
                for (int i = 0; i < member.Type.Length; i++)
                {
                    if (member.Type[i].GetParameters().Length == values.Length)
                    {
                        chosen = member.Type[i];
                        break;
                    }
                }
                if (chosen == null) {
                    chosen = member.Type[0];
                }
                return chosen.Invoke(member.Target, values);
            }
            if (executable is Method function)
            {
                // Class-method dotted access (`obj.foo(arg)`) sets
                // function.Self to the receiver in PropertyExpression.GetReference,
                // and the declared signature includes the conventional
                // `self` first parameter — so we prepend Self here to bind
                // it.  Free functions registered by RegisterPreamble never
                // set Self (their declared parameters don't include it),
                // so prepending a null would shift every player-supplied
                // arg one slot to the right and bind null to the first
                // declared name — which surfaces as a downstream
                // "Cannot add null values" / null-deref the moment the
                // body uses that parameter.  Skipping the prepend when
                // Self is null keeps both shapes working.
                IEnumerable<IArgumentValue> mapped = arguments.Select(a =>
                    a.name == null
                        ? (IArgumentValue)new OrderedValue(a.value)
                        : (IArgumentValue)new NamedValue(a.name, a.value));
                IArgumentValue[] callArgs = function.Self != null
                    ? new IArgumentValue[] { new OrderedValue(function.Self) }.Concat(mapped).ToArray()
                    : mapped.ToArray();
                return function.Invoke(callArgs);
            }
            if (executable is Type type)
            {
                try
                {
                    return Expressions.__init__(type, arguments.Select(a =>
                        a.name == null
                            ? (IArgumentValue)new OrderedValue(a.value)
                            : (IArgumentValue)new NamedValue(a.name, a.value)));
                }
                catch (Exception e)
                {
                    throw new NotImplementedException($"Try to call constructor of {type.Name}", e);
                }
            }
            throw new NotImplementedException("Invocation is not defined");
        }

        public static object __init__(Type type, IEnumerable<IArgumentValue> enumerable)
        {
            if (Initializers.TryGetValue(type, out var initializer))
            {
                return initializer([.. enumerable]);
            }

            return Activator.CreateInstance(type, [.. enumerable.Select(e => e.Value)]);
        }

        public static bool __eq__(object left, object right)
        {
            if (left is null && right is null) {
				return true;
			}
			if (left is null || right is null) {
				return false;
			}
			if (left.GetType() != right.GetType())
            {
                if (__eq__base(left, right, out bool result)) {
					return result;
				}
				throw new NotImplementedException($"Types has no comparison yet or never (different type)");
            }
            if (left is Fix32 fix) {
				return fix == (Fix32)right;
			}
			if (left is int i) {
				return i == (int)right;
			}
			if (left is float f) {
				return f == (float)right;
			}
			if (left is bool b) {
				return b == (bool)right;
			}
			throw new NotImplementedException($"Types has no comparison yet or never (same type)");
        }

        private static bool __eq__base(object left, object right, out bool result)
        {
            if (left is int leftI && right is Fix32 right32)
            {
                result = leftI.ToFix32() == right32;
                return true;
            }
            if (left is Fix32 left32 && right is int rightI)
            {
                result = left32 == rightI.ToFix32();
                return true;
            }
            if (left is float leftF && right is Fix32 right32_)
            {
                result = leftF.ToFix32() == right32_;
                return true;
            }
            if (left is Fix32 left32_ && right is float rightF)
            {
                result = left32_ == rightF.ToFix32();
                return true;
            }
            if (left is float leftF_ && right is int rightI_)
            {
                result = leftF_ == rightI_;
                return true;
            }
            if (left is int leftI_ && right is float rightF_)
            {
                result = leftI_ == rightF_;
                return true;
            }
            result = false;
            return false;
        }

        public static bool __ne__(object left, object right)
        {
            if (left is null && right is null) {
				return false;
			}
			if (left is null || right is null) {
				return true;
			}
			if (left.GetType() != right.GetType())
            {
                if (__ne__base(left, right, out bool result)) {
					return result;
				}
				throw new NotImplementedException($"Types has no comparison yet or never (different type)");
            }
            if (left is Fix32 fix) {
				return fix != (Fix32)right;
			}
			if (left is int i) {
				return i != (int)right;
			}
			if (left is float f) {
				return f != (float)right;
			}
			if (left is bool b) {
				return b != (bool)right;
			}
			throw new NotImplementedException($"Types has no comparison yet or never (same type)");
        }

        private static bool __ne__base(object left, object right, out bool result)
        {
            if (left is int leftI && right is Fix32 right32)
            {
                result = leftI.ToFix32() != right32;
                return true;
            }
            if (left is Fix32 left32 && right is int rightI)
            {
                result = left32 != rightI.ToFix32();
                return true;
            }
            if (left is float leftF && right is Fix32 right32_)
            {
                result = leftF.ToFix32() != right32_;
                return true;
            }
            if (left is Fix32 left32_ && right is float rightF)
            {
                result = left32_ != rightF.ToFix32();
                return true;
            }
            result = false;
            return false;
        }

        public static bool __ge__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot compare null values");
			}
			if (left.GetType() != right.GetType())
            {
                if (__ge__base(left, right, out bool result)) {
					return result;
				}
				throw new NotImplementedException($"Types has no comparison yet or never (different type)");
            }
            if (left is Fix32 fix) {
				return fix >= (Fix32)right;
			}
			if (left is int i) {
				return i >= (int)right;
			}
			if (left is float f) {
				return f >= (float)right;
			}
			if (left is bool b) {
				return true;
			}
			throw new NotImplementedException($"Types has no comparison yet or never (same type)");
        }

        private static bool __ge__base(object left, object right, out bool result)
        {
            if (left is int leftI && right is Fix32 right32)
            {
                result = leftI.ToFix32() >= right32;
                return true;
            }
            if (left is Fix32 left32 && right is int rightI)
            {
                result = left32 >= rightI.ToFix32();
                return true;
            }
            if (left is float leftF && right is Fix32 right32_)
            {
                result = leftF.ToFix32() >= right32_;
                return true;
            }
            if (left is Fix32 left32_ && right is float rightF)
            {
                result = left32_ >= rightF.ToFix32();
                return true;
            }
            result = false;
            return false;
        }

        public static bool __gt__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot compare null values");
			}
			if (left.GetType() != right.GetType())
            {
                if (__gt__base(left, right, out bool result)) {
					return result;
				}
				throw new NotImplementedException($"Types has no comparison yet or never (different type)");
            }
            if (left is Fix32 fix) {
				return fix > (Fix32)right;
			}
			if (left is int i) {
				return i > (int)right;
			}
			if (left is float f) {
				return f > (float)right;
			}
			if (left is bool b) {
				return b == true && (bool)right == false;
			}
			throw new NotImplementedException($"Types has no comparison yet or never (same type)");
        }

        private static bool __gt__base(object left, object right, out bool result)
        {
            if (left is int leftI && right is Fix32 right32)
            {
                result = leftI.ToFix32() > right32;
                return true;
            }
            if (left is Fix32 left32 && right is int rightI)
            {
                result = left32 > rightI.ToFix32();
                return true;
            }
            if (left is float leftF && right is Fix32 right32_)
            {
                result = leftF.ToFix32() > right32_;
                return true;
            }
            if (left is Fix32 left32_ && right is float rightF)
            {
                result = left32_ > rightF.ToFix32();
                return true;
            }
            result = false;
            return false;
        }

        public static bool __le__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot compare null values");
			}
			if (left.GetType() != right.GetType())
            {
                if (__le__base(left, right, out bool result)) {
					return result;
				}
				throw new NotImplementedException($"Types has no comparison yet or never (different type)");
            }
            if (left is Fix32 fix) {
				return fix <= (Fix32)right;
			}
			if (left is int i) {
				return i <= (int)right;
			}
			if (left is float f) {
				return f <= (float)right;
			}
			if (left is bool b) {
				return true;
			}
			throw new NotImplementedException($"Types has no comparison yet or never (same type)");
        }

        private static bool __le__base(object left, object right, out bool result)
        {
            if (left is int leftI && right is Fix32 right32)
            {
                result = leftI.ToFix32() <= right32;
                return true;
            }
            if (left is Fix32 left32 && right is int rightI)
            {
                result = left32 <= rightI.ToFix32();
                return true;
            }
            if (left is float leftF && right is Fix32 right32_)
            {
                result = leftF.ToFix32() <= right32_;
                return true;
            }
            if (left is Fix32 left32_ && right is float rightF)
            {
                result = left32_ <= rightF.ToFix32();
                return true;
            }
            result = false;
            return false;
        }

        public static bool __lt__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot compare null values");
			}
			if (left.GetType() != right.GetType())
            {
                if (__lt__base(left, right, out bool result)) {
					return result;
				}
				throw new NotImplementedException($"Types has no comparison yet or never (different type)");
            }
            if (left is Fix32 fix) {
				return fix < (Fix32)right;
			}
			if (left is int i) {
				return i < (int)right;
			}
			if (left is float f) {
				return f < (float)right;
			}
			if (left is bool b) {
				return b == false && (bool)right == true;
			}
			throw new NotImplementedException($"Types has no comparison yet or never (same type)");
        }

        private static bool __lt__base(object left, object right, out bool result)
        {
            if (left is int leftI && right is Fix32 right32)
            {
                result = leftI.ToFix32() < right32;
                return true;
            }
            if (left is Fix32 left32 && right is int rightI)
            {
                result = left32 < rightI.ToFix32();
                return true;
            }
            if (left is float leftF && right is Fix32 right32_)
            {
                result = leftF.ToFix32() < right32_;
                return true;
            }
            if (left is Fix32 left32_ && right is float rightF)
            {
                result = left32_ < rightF.ToFix32();
                return true;
            }
            result = false;
            return false;
        }

        public static object __range__(object left, Range range)
        {
            throw new NotImplementedException("Range operator is not defined");
        }

        public static void __setitem__(object target, object index, object value)
        {
            if (target is List<object> list)
            {
                list[__int__(index)] = value;
                return;
            }
            if (target is IDictionary<string, object> dict)
            {
                dict[__str__(index)] = value;
                return;
            }
            // Wrappers (ArraySetter, NumberDataSetter, ...) declare their
            // own __setitem__ — pick that up via reflection BEFORE the
            // generic IList path so the wrapper's coercion / clamping
            // logic still runs (an ArraySetter is not an IList itself,
            // but its `set` writes through Module.Array which has its
            // own resize semantics).
            if (target?.GetType().GetMethod("__setitem__") is MethodInfo methodInfo)
            {
                methodInfo.Invoke(target, [index, value]);
                return;
            }
            // Generic IList covers List<int>, List<Fix32>, T[], anything
            // range() / list literals / pin enumeration produces.  Index
            // is coerced through __int__ so a Fix32 / float index resolves
            // to int the same way IList expects; the value is coerced to
            // the list's element type when knowable (generic List<T> and
            // arrays expose it cheaply).
            if (target is System.Collections.IList ilist)
            {
                ilist[__int__(index)] = CoerceToElementType(target, value);
                return;
            }
            // Generic IList<T> — Mafi.Collections.Lyst<T>, ReadWriteSwapLyst<T>,
            // and any other type that implements only the generic interface
            // (List<T> / T[] implement BOTH IList<T> AND non-generic IList, so
            // they're caught by the branch above; this catches the strictly-
            // generic case).  Routes through the IList<T> indexer property
            // (resolved against the interface, not the concrete type) so an
            // explicit interface implementation that hides "Item" under a
            // different name still works.
            {
                System.Reflection.PropertyInfo genericIndexer = TryGetGenericIListIndexer(target);
                if (genericIndexer != null && genericIndexer.CanWrite)
                {
                    genericIndexer.SetValue(target, CoerceToElementType(target, value),
                        new object[] { __int__(index) });
                    return;
                }
            }
            // System.Array fallback for multi-dim arrays (which DON'T implement
            // IList).  Single-dim T[] arrives via the IList branch above; this
            // is here so a 2D array (rare but possible from a Mafi API) still
            // works rather than throwing NotImplementedException.
            if (target is System.Array arr)
            {
                arr.SetValue(CoerceToElementType(target, value), __int__(index));
                return;
            }
            // Last resort — types that don't implement any of the standard
            // collection interfaces but do expose a `this[int]` indexer.
            // Mafi.ImmutableArray<T> is read-only (getter only) so this
            // path is a no-op for it; it would already have thrown via the
            // GetProperty lookup returning a get-only property and CanWrite
            // being false.  We leave the error message as
            // NotImplementedException rather than synthesize a custom one
            // because the player's runtime catch in PlcPy.Action surfaces
            // the message directly to the error strip.
            if (target != null)
            {
                Type t = target.GetType();
                var prop = t.GetProperty("Item", new[] { typeof(int) });
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(target, CoerceToElementType(target, value),
                        new object[] { __int__(index) });
                    return;
                }
            }
            throw new NotImplementedException("__setitem__");
        }

        public static object __getitem__(object target, object index)
        {
            if (target is List<object> list)
            {
                return list[__int__(index)];
            }
            if (target is IDictionary<string, object> dict)
            {
                return dict.TryGetValue(__str__(index), out object value) ? value : null;
            }
            // Reflection-bound __getitem__ wins over the generic IList path
            // for the same reason as __setitem__ above (wrapper semantics).
            if (target?.GetType().GetMethod("__getitem__") is MethodInfo methodInfo)
            {
                return methodInfo.Invoke(target, [index]);
            }
            // Strings index by character so `"abc"[1]` works; without this
            // the player would get the "__getitem__ not implemented" error
            // for the most common indexable type.
            if (target is string s)
            {
                int si = __int__(index);
                if (si < 0 || si >= s.Length) {
                    throw new System.ArgumentOutOfRangeException(
                        $"string index {si} out of range [0,{s.Length})");
                }
                return s[si].ToString();
            }
            if (target is System.Collections.IList ilist)
            {
                return ilist[__int__(index)];
            }
            // Generic IList<T> — Mafi.Lyst<T> / ReadWriteSwapLyst<T>, anything
            // that implements only the BCL generic interface.  See the matching
            // comment in __setitem__ for why this is a separate branch from the
            // non-generic IList check above.
            {
                System.Reflection.PropertyInfo genericIndexer = TryGetGenericIListIndexer(target);
                if (genericIndexer != null && genericIndexer.CanRead)
                {
                    return genericIndexer.GetValue(target, new object[] { __int__(index) });
                }
            }
            // Multi-dim arrays (T[,], T[,,]) don't implement IList but do
            // support index access through Array.GetValue.  Single-dim T[]
            // is caught by the IList branch above.
            if (target is System.Array arr)
            {
                return arr.GetValue(__int__(index));
            }
            // Mafi.ImmutableArray<T> / ReadOnlyArray<T> / LystStruct<T> /
            // SmallImmutableArray<T> — none of these implement IList, but
            // they all expose a `T this[int]` indexer.  Reflect once and
            // route through it so the player can index any Mafi-style
            // collection a wrapper might surface (e.g. an ImmutableArray of
            // recipe ids exposed via self.Prototype.Recipes).
            if (target != null)
            {
                Type t = target.GetType();
                var prop = t.GetProperty("Item", new[] { typeof(int) });
                if (prop != null && prop.CanRead)
                {
                    return prop.GetValue(target, new object[] { __int__(index) });
                }
            }
            throw new NotImplementedException("__getitem__");
        }

        // Returns the IList<T> indexer property for the most-derived `IList<T>`
        // interface `target` implements, or null when the target doesn't
        // implement the generic IList<T>.  Used so we can route through the
        // standard generic indexer for types like Mafi.Collections.Lyst<T>
        // that implement only IList<T> and not the non-generic IList — those
        // would otherwise fall through to the property-reflection fallback,
        // which works but is less semantic and would also pick up unrelated
        // `this[int]` indexers (e.g. an int-keyed lookup that happens to live
        // on a non-list type).  GetInterfaces() is a one-time call per
        // __getitem__/__setitem__ — for sealed Mafi collection types this
        // is a small fixed set, and the runtime caches interface metadata
        // internally so repeat lookups stay cheap.
        private static System.Reflection.PropertyInfo TryGetGenericIListIndexer(object target)
        {
            if (target == null) {
                return null;
            }
            foreach (Type i in target.GetType().GetInterfaces())
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    // IList<T>.Item — single int parameter, both get and set.
                    // GetProperty against the interface (not the concrete type)
                    // so an explicit-interface impl that hides "Item" still
                    // resolves cleanly.
                    return i.GetProperty("Item");
                }
            }
            return null;
        }

        // Helper for IList __setitem__: cast `value` to the list's element
        // type when we can derive it (generic List<T> or an array).  Used to
        // route an int from `range()` into a List<Fix32> (and vice versa)
        // without making the player wrap every assignment in fix(...).
        // Returns the value unchanged if the element type is unknown or the
        // value is already assignable — IList will throw a clear cast error
        // in that case, which is what we want for a genuine type mismatch.
        private static object CoerceToElementType(object target, object value)
        {
            Type elementType = null;
            Type targetType = target.GetType();
            if (targetType.IsArray)
            {
                elementType = targetType.GetElementType();
            }
            else if (targetType.IsGenericType)
            {
                Type[] args = targetType.GetGenericArguments();
                if (args.Length == 1) {
                    elementType = args[0];
                }
            }
            if (elementType == null || value == null) {
                return value;
            }
            if (elementType.IsInstanceOfType(value)) {
                return value;
            }
            if (elementType == typeof(int)) {
                return __int__(value);
            }
            if (elementType == typeof(Fix32)) {
                return __fix__(value);
            }
            if (elementType == typeof(string)) {
                return __str__(value);
            }
            return value;
        }

        public static bool __contains__(object target, object key)
        {
            if (target is null) {
				throw new NullReferenceException("Target is None");
			}
			if (target is IDictionary<string, object> dict) {
				return dict.ContainsKey(__str__(key));
			}
			if (target is List<object> list) {
				return list.Contains(key);
			}
			throw new NotImplementedException("__contains__");
        }

        public static object __invert__(object v)
        {
            throw new NotImplementedException("__invert__");
        }

        public static object __mul__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot multiply null values");
			}
			if (left.GetType() != right.GetType()) {
				throw new NotImplementedException($"Types has no multiply yet or never (different type)");
			}
			if (left is Fix32 fix) {
				return fix * (Fix32)right;
			}
			if (left is int i) {
				return i * (int)right;
			}
			if (left is float f) {
				return f * (float)right;
			}
			throw new NotImplementedException($"Types has no multiply yet or never (same type)");
        }

        public static object __div__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot divide null values");
			}
			if (left.GetType() != right.GetType()) {
				throw new NotImplementedException($"Types has no divide yet or never (different type)");
			}
			if (left is Fix32 fix) {
				return fix / (Fix32)right;
			}
			if (left is int i) {
				return i / (int)right;
			}
			if (left is float f) {
				return f / (float)right;
			}
			throw new NotImplementedException($"Types has no divide yet or never (same type)");
        }

        public static object __divint__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot divide null values");
			}
			if (left.GetType() != right.GetType()) {
				throw new NotImplementedException($"Types has no multiply yet or never (different type)");
			}
			if (left is Fix32 fix) {
				return fix / (Fix32)right;
			}
			if (left is int i) {
				return i / (int)right;
			}
			if (left is float f) {
				return f / (float)right;
			}
			throw new NotImplementedException($"Types has no divide yet or never (same type)");
        }

        public static object __mod__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot divide null values");
			}
			if (left.GetType() != right.GetType()) {
				throw new NotImplementedException($"Types has no divide yet or never (different type)");
			}
			if (left is Fix32 fix) {
				return fix % (Fix32)right;
			}
			if (left is int i) {
				return i % (int)right;
			}
			if (left is float f) {
				return f % (float)right;
			}
			throw new NotImplementedException($"Types has no divide yet or never (same type)");
        }

        public static bool __not__(object v)
        {
            if (v is bool b) {
				return !b;
			}
			throw new NotImplementedException("__not__");
        }

        public static Fix32 __pow__(object left, object right)
        {
            Fix32 fixLeft = __fix__(left);
            Fix32 fixRight = __fix__(right);
            // todo
            return fixLeft.Pow(fixRight);
        }

        public static object __lshift__(object left, object right)
        {
            if (left is null && right is null) {
				return 0;
			}
			return __int__(left) << __int__(right);
        }

        public static object __rshift__(object left, object right)
        {
            if (left is null && right is null) {
				return 0;
			}
			return __int__(left) >> __int__(right);
        }

        public static object __add__(object left, object right)
        {
            if (left is null || right is null) {
				throw new NotImplementedException($"Cannot add null values");
			}
			if (left is string ls) {
				return ls + (right is string rs ? rs : right?.ToString());
			}
			if (left.GetType() != right.GetType()) {
				throw new NotImplementedException($"Types has no divide yet or never (different type)");
			}
			if (left is Fix32 fix) {
				return fix + (Fix32)right;
			}
			if (left is int i) {
				return i + (int)right;
			}
			if (left is float f) {
				return f + (float)right;
			}
			throw new NotImplementedException($"Types has no divide yet or never (same type)");
        }
    }
}