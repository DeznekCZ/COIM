using System;
using System.Globalization;

namespace Mafi
{
    /// <summary>
    /// Faithful clean-room reimplementation of Mafi's <c>Fix32</c> fixed-point number, used so the
    /// source-linked Python interpreter can run in the browser without the Mafi assembly.
    ///
    /// Q21.10: 10 fractional bits, <see cref="ONE_RAW"/> = 1024. Arithmetic semantics are copied
    /// from the game's Fix32 (verified against the decompiled source) so a simulated module produces
    /// the same per-tick values as in-game:
    ///   *  : (long)l*r >> 10, clamped to int range
    ///   /  : half-rounded division (negative-aware)
    ///   %  : raw C# remainder (sign of dividend) — matches Fix32.operator%
    ///   ToString : ToFloat().ToString(InvariantCulture)
    ///   Pow : FromDouble(Math.Pow(...))
    /// </summary>
    public readonly struct Fix32 : IEquatable<Fix32>, IComparable<Fix32>
    {
        public const int FRACTIONAL_BITS = 10;
        public const int FRACTION_RANGE = 1024;
        internal const int FRACTION_MASK = 1023;
        internal const int ONE_RAW = 1024;

        public static readonly Fix32 Zero = new Fix32(0);
        public static readonly Fix32 One = new Fix32(ONE_RAW);
        public static readonly Fix32 Half = new Fix32(ONE_RAW >> 1);
        public static readonly Fix32 MinValue = new Fix32(int.MinValue);
        public static readonly Fix32 MaxValue = new Fix32(int.MaxValue);

        public readonly int RawValue;

        internal Fix32(int rawValue)
        {
            RawValue = rawValue;
        }

        public bool IsZero => RawValue == 0;
        public bool IsNotZero => RawValue != 0;
        public bool IsPositive => RawValue > 0;
        public bool IsNegative => RawValue < 0;
        public bool IsNotNegative => RawValue >= 0;
        public bool IsInteger => (RawValue & FRACTION_MASK) == 0;

        /// <summary>Integer part discarding any fraction (floor toward zero), matching the game.</summary>
        public int IntegerPart => ((RawValue >= 0) ? RawValue : (RawValue + FRACTION_MASK)) >> FRACTIONAL_BITS;

        public Fix32 FractionalPart => new Fix32(RawValue - (((RawValue >= 0) ? RawValue : (RawValue + FRACTION_MASK)) & -ONE_RAW));

        public static Fix32 FromRaw(int rawValue) => new Fix32(rawValue);

        public static Fix32 FromInt(int value)
        {
            if (value < -2097152) { return MinValue; }
            if (value > 2097151) { return MaxValue; }
            return new Fix32(value << FRACTIONAL_BITS);
        }

        public static Fix32 FromFloat(float value)
        {
            if (float.IsNaN(value)) { return Zero; }
            double scaled = Math.Round((double)value * ONE_RAW);
            return ClampToRaw(scaled);
        }

        public static Fix32 FromDouble(double value)
        {
            if (double.IsNaN(value)) { return Zero; }
            return ClampToRaw(Math.Round(value * ONE_RAW));
        }

        private static Fix32 ClampToRaw(double scaled)
        {
            if (scaled < int.MinValue) { return MinValue; }
            if (scaled > int.MaxValue) { return MaxValue; }
            return new Fix32((int)scaled);
        }

        public float ToFloat() => (float)RawValue / ONE_RAW;
        public double ToDouble() => (double)RawValue / ONE_RAW;

        public int ToIntFloored() => RawValue >> FRACTIONAL_BITS;
        public int ToIntRounded() => (int)(((long)RawValue + 512L + ((RawValue < 0) ? -1 : 0)) >> FRACTIONAL_BITS);

        public Fix32 Abs() => new Fix32(Math.Abs(RawValue));
        public int Sign() => Math.Sign(RawValue);
        public Fix32 Min(Fix32 other) => RawValue <= other.RawValue ? this : other;
        public Fix32 Max(Fix32 other) => RawValue >= other.RawValue ? this : other;

        public Fix32 Pow(Fix32 exponent) => FromDouble(Math.Pow(ToDouble(), exponent.ToDouble()));
        public Fix32 Sqrt() => FromDouble(Math.Sqrt(ToDouble()));

        // --- unary ---
        public static Fix32 operator +(Fix32 v) => v;
        public static Fix32 operator -(Fix32 v) => new Fix32(-v.RawValue);

        // --- additive ---
        public static Fix32 operator +(Fix32 l, Fix32 r) => new Fix32(l.RawValue + r.RawValue);
        public static Fix32 operator -(Fix32 l, Fix32 r) => new Fix32(l.RawValue - r.RawValue);

        // --- multiplicative ---
        public static Fix32 operator *(Fix32 l, Fix32 r)
        {
            long num = (long)l.RawValue * r.RawValue >> FRACTIONAL_BITS;
            if (num > int.MaxValue) { return MaxValue; }
            if (num < int.MinValue) { return MinValue; }
            return new Fix32((int)num);
        }

        public static Fix32 operator *(Fix32 l, int r)
        {
            long num = (long)l.RawValue * r;
            if (num > int.MaxValue) { return MaxValue; }
            if (num < int.MinValue) { return MinValue; }
            return new Fix32((int)num);
        }

        public static Fix32 operator *(int l, Fix32 r) => r * l;

        public static Fix32 operator /(Fix32 lhs, Fix32 rhs)
        {
            if (rhs.IsZero) { return Zero; }
            if (rhs.IsNegative) { lhs = -lhs; rhs = -rhs; }
            long num = (long)lhs.RawValue << FRACTIONAL_BITS;
            num += (num > 0) ? (rhs.RawValue >> 1) : -(rhs.RawValue >> 1);
            long q = num / rhs.RawValue;
            if (q < int.MinValue) { return MinValue; }
            if (q > int.MaxValue) { return MaxValue; }
            return new Fix32((int)q);
        }

        public static Fix32 operator /(Fix32 lhs, int rhs)
        {
            if (rhs == 0) { return Zero; }
            if (rhs < 0) { lhs = -lhs; rhs = -rhs; }
            int adj = (lhs.RawValue < 0) ? -(rhs >> 1) : (rhs >> 1);
            return new Fix32((int)(((long)lhs.RawValue + adj) / rhs));
        }

        public static Fix32 operator /(int lhs, Fix32 rhs)
        {
            if (rhs.IsZero) { return Zero; }
            if (rhs.IsNegative) { lhs = -lhs; rhs = -rhs; }
            long num = (long)lhs << 20;
            num += (num > 0) ? (rhs.RawValue >> 1) : -(rhs.RawValue >> 1);
            long q = num / rhs.RawValue;
            if (q < int.MinValue) { return MinValue; }
            if (q > int.MaxValue) { return MaxValue; }
            return new Fix32((int)q);
        }

        public static Fix32 operator %(Fix32 l, Fix32 r) => new Fix32(l.RawValue % r.RawValue);

        public static Fix32 operator <<(Fix32 l, int r) => new Fix32(l.RawValue << r);
        public static Fix32 operator >>(Fix32 l, int r) => new Fix32(l.RawValue >> r);

        // --- comparisons ---
        public static bool operator ==(Fix32 l, Fix32 r) => l.RawValue == r.RawValue;
        public static bool operator !=(Fix32 l, Fix32 r) => l.RawValue != r.RawValue;
        public static bool operator >(Fix32 l, Fix32 r) => l.RawValue > r.RawValue;
        public static bool operator >=(Fix32 l, Fix32 r) => l.RawValue >= r.RawValue;
        public static bool operator <(Fix32 l, Fix32 r) => l.RawValue < r.RawValue;
        public static bool operator <=(Fix32 l, Fix32 r) => l.RawValue <= r.RawValue;

        public bool Equals(Fix32 other) => RawValue == other.RawValue;
        public int CompareTo(Fix32 other) => RawValue.CompareTo(other.RawValue);

        public override bool Equals(object obj) => obj is Fix32 f && f.RawValue == RawValue;
        public override int GetHashCode() => RawValue;

        public override string ToString() => ToFloat().ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Extension converters Mafi exposes (e.g. <c>5.ToFix32()</c>, <c>0.1.ToFix32()</c>).</summary>
    public static class Fix32Ext
    {
        public static Fix32 ToFix32(this int value) => Fix32.FromInt(value);
        public static Fix32 ToFix32(this short value) => Fix32.FromInt(value);
        public static Fix32 ToFix32(this byte value) => Fix32.FromInt(value);
        public static Fix32 ToFix32(this float value) => Fix32.FromFloat(value);
        public static Fix32 ToFix32(this double value) => Fix32.FromDouble(value);
    }
}
