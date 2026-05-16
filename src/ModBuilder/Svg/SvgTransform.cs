using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CustomAssets.ModBuilder.Svg
{
    // Parses the SVG `transform="..."` attribute syntax into a System.Drawing.Drawing2D.Matrix.
    // Multiple transform functions on the same attribute compose left-to-right (i.e. the
    // leftmost transform is applied first to a point), which is the order GDI+ matrix
    // multiplication produces with MatrixOrder.Append on each step.
    internal static class SvgTransform
    {
        private static readonly Regex FunctionRx = new Regex(
            @"(?<name>\w+)\s*\(\s*(?<args>[^)]*)\)",
            RegexOptions.Compiled);

        public static Matrix Parse(string attr)
        {
            var m = new Matrix();
            if (string.IsNullOrWhiteSpace(attr)) return m;
            foreach (Match match in FunctionRx.Matches(attr))
            {
                string name = match.Groups["name"].Value;
                double[] args = SplitNumbers(match.Groups["args"].Value);
                ApplyFunction(m, name, args);
            }
            return m;
        }

        private static void ApplyFunction(Matrix m, string name, double[] a)
        {
            switch (name)
            {
                case "translate":
                {
                    float tx = a.Length > 0 ? (float)a[0] : 0f;
                    float ty = a.Length > 1 ? (float)a[1] : 0f;
                    m.Translate(tx, ty, MatrixOrder.Append);
                    break;
                }
                case "scale":
                {
                    float sx = a.Length > 0 ? (float)a[0] : 1f;
                    float sy = a.Length > 1 ? (float)a[1] : sx;
                    m.Scale(sx, sy, MatrixOrder.Append);
                    break;
                }
                case "rotate":
                {
                    float angle = a.Length > 0 ? (float)a[0] : 0f;
                    if (a.Length >= 3)
                    {
                        float cx = (float)a[1];
                        float cy = (float)a[2];
                        m.Translate(-cx, -cy, MatrixOrder.Append);
                        m.Rotate(angle, MatrixOrder.Append);
                        m.Translate(cx, cy, MatrixOrder.Append);
                    }
                    else
                    {
                        m.Rotate(angle, MatrixOrder.Append);
                    }
                    break;
                }
                case "skewX":
                {
                    float angle = a.Length > 0 ? (float)a[0] : 0f;
                    double t = Math.Tan(angle * Math.PI / 180.0);
                    var skew = new Matrix(1f, 0f, (float)t, 1f, 0f, 0f);
                    m.Multiply(skew, MatrixOrder.Append);
                    break;
                }
                case "skewY":
                {
                    float angle = a.Length > 0 ? (float)a[0] : 0f;
                    double t = Math.Tan(angle * Math.PI / 180.0);
                    var skew = new Matrix(1f, (float)t, 0f, 1f, 0f, 0f);
                    m.Multiply(skew, MatrixOrder.Append);
                    break;
                }
                case "matrix":
                {
                    if (a.Length == 6)
                    {
                        var mtx = new Matrix(
                            (float)a[0], (float)a[1],
                            (float)a[2], (float)a[3],
                            (float)a[4], (float)a[5]);
                        m.Multiply(mtx, MatrixOrder.Append);
                    }
                    break;
                }
            }
        }

        private static double[] SplitNumbers(string s)
        {
            var list = new List<double>();
            foreach (Match m in NumberToken.Matches(s))
            {
                if (double.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    list.Add(v);
            }
            return list.ToArray();
        }

        // SVG numbers may be separated by commas, whitespace, or implicit sign change
        // ("1.5-2.0" is two numbers). The regex below captures any optionally-signed float.
        public static readonly Regex NumberToken = new Regex(
            @"[-+]?(\d+\.\d*|\.\d+|\d+)([eE][-+]?\d+)?",
            RegexOptions.Compiled);
    }
}
