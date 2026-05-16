using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CustomAssets.ModBuilder.Svg
{
    // Converts an SVG `<path d="...">` attribute into a System.Drawing GraphicsPath.
    //
    // Supports the full SVG 1.1 path command set:
    //   M/m  moveto
    //   L/l  lineto      H/h horizontal lineto   V/v vertical lineto
    //   C/c  cubic curve S/s smooth cubic curve
    //   Q/q  quadratic   T/t smooth quadratic
    //   A/a  elliptical arc
    //   Z/z  closepath
    internal static class SvgPath
    {
        // Tokenizes the d-attribute into (commandLetter, [args]) pairs.
        // A command letter implicitly repeats with the next set of args if numbers continue
        // (e.g. "M10,10 20,20 30,30" → MoveTo(10,10), LineTo(20,20), LineTo(30,30)).
        private static readonly Regex CommandLetter = new Regex(@"[MmLlHhVvCcSsQqTtAaZz]", RegexOptions.Compiled);

        public static GraphicsPath Parse(string d)
        {
            var path = new GraphicsPath(FillMode.Winding);
            if (string.IsNullOrWhiteSpace(d)) return path;

            // Current pen position, subpath start (used by Z), last cubic control,
            // last quadratic control. The last-control values are mirrored by S/T.
            float cx = 0, cy = 0;
            float subX = 0, subY = 0;
            float lastCubicCtrlX = 0, lastCubicCtrlY = 0;
            float lastQuadCtrlX = 0, lastQuadCtrlY = 0;
            char lastCmd = '\0';
            int i = 0;
            bool subStarted = false;

            while (i < d.Length)
            {
                while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
                if (i >= d.Length) break;

                char cmd;
                if (CommandLetter.IsMatch(d[i].ToString()))
                {
                    cmd = d[i];
                    i++;
                }
                else
                {
                    // Implicit repeat of the previous command (M repeats as L, m repeats as l).
                    if (lastCmd == 'M') cmd = 'L';
                    else if (lastCmd == 'm') cmd = 'l';
                    else cmd = lastCmd;
                    if (cmd == '\0') break;
                }

                switch (cmd)
                {
                    case 'M':
                    case 'm':
                    {
                        float x = ReadNumber(d, ref i);
                        float y = ReadNumber(d, ref i);
                        if (cmd == 'm' && subStarted) { x += cx; y += cy; }
                        cx = x; cy = y;
                        subX = cx; subY = cy;
                        path.StartFigure();
                        subStarted = true;
                        break;
                    }
                    case 'L':
                    case 'l':
                    {
                        float x = ReadNumber(d, ref i);
                        float y = ReadNumber(d, ref i);
                        if (cmd == 'l') { x += cx; y += cy; }
                        path.AddLine(cx, cy, x, y);
                        cx = x; cy = y;
                        break;
                    }
                    case 'H':
                    case 'h':
                    {
                        float x = ReadNumber(d, ref i);
                        if (cmd == 'h') x += cx;
                        path.AddLine(cx, cy, x, cy);
                        cx = x;
                        break;
                    }
                    case 'V':
                    case 'v':
                    {
                        float y = ReadNumber(d, ref i);
                        if (cmd == 'v') y += cy;
                        path.AddLine(cx, cy, cx, y);
                        cy = y;
                        break;
                    }
                    case 'C':
                    case 'c':
                    {
                        float x1 = ReadNumber(d, ref i);
                        float y1 = ReadNumber(d, ref i);
                        float x2 = ReadNumber(d, ref i);
                        float y2 = ReadNumber(d, ref i);
                        float x = ReadNumber(d, ref i);
                        float y = ReadNumber(d, ref i);
                        if (cmd == 'c')
                        {
                            x1 += cx; y1 += cy;
                            x2 += cx; y2 += cy;
                            x += cx; y += cy;
                        }
                        path.AddBezier(cx, cy, x1, y1, x2, y2, x, y);
                        lastCubicCtrlX = x2; lastCubicCtrlY = y2;
                        cx = x; cy = y;
                        break;
                    }
                    case 'S':
                    case 's':
                    {
                        // S/s reuses the reflection of the previous cubic control point.
                        // If the previous command was not C/c/S/s, the first control point
                        // is coincident with the current point (per SVG spec).
                        float x1, y1;
                        if (lastCmd == 'C' || lastCmd == 'c' || lastCmd == 'S' || lastCmd == 's')
                        {
                            x1 = 2 * cx - lastCubicCtrlX;
                            y1 = 2 * cy - lastCubicCtrlY;
                        }
                        else { x1 = cx; y1 = cy; }
                        float x2 = ReadNumber(d, ref i);
                        float y2 = ReadNumber(d, ref i);
                        float x = ReadNumber(d, ref i);
                        float y = ReadNumber(d, ref i);
                        if (cmd == 's')
                        {
                            x2 += cx; y2 += cy;
                            x += cx; y += cy;
                        }
                        path.AddBezier(cx, cy, x1, y1, x2, y2, x, y);
                        lastCubicCtrlX = x2; lastCubicCtrlY = y2;
                        cx = x; cy = y;
                        break;
                    }
                    case 'Q':
                    case 'q':
                    {
                        float qx = ReadNumber(d, ref i);
                        float qy = ReadNumber(d, ref i);
                        float x = ReadNumber(d, ref i);
                        float y = ReadNumber(d, ref i);
                        if (cmd == 'q')
                        {
                            qx += cx; qy += cy;
                            x += cx; y += cy;
                        }
                        AddQuadratic(path, cx, cy, qx, qy, x, y);
                        lastQuadCtrlX = qx; lastQuadCtrlY = qy;
                        cx = x; cy = y;
                        break;
                    }
                    case 'T':
                    case 't':
                    {
                        // T/t reflects previous quadratic control.
                        float qx, qy;
                        if (lastCmd == 'Q' || lastCmd == 'q' || lastCmd == 'T' || lastCmd == 't')
                        {
                            qx = 2 * cx - lastQuadCtrlX;
                            qy = 2 * cy - lastQuadCtrlY;
                        }
                        else { qx = cx; qy = cy; }
                        float x = ReadNumber(d, ref i);
                        float y = ReadNumber(d, ref i);
                        if (cmd == 't') { x += cx; y += cy; }
                        AddQuadratic(path, cx, cy, qx, qy, x, y);
                        lastQuadCtrlX = qx; lastQuadCtrlY = qy;
                        cx = x; cy = y;
                        break;
                    }
                    case 'A':
                    case 'a':
                    {
                        float rx = ReadNumber(d, ref i);
                        float ry = ReadNumber(d, ref i);
                        float xAxisRotation = ReadNumber(d, ref i);
                        // Per SVG: large-arc-flag and sweep-flag are single 0/1 digits.
                        // They MAY appear without separators, e.g. "0011" = 0,0,1,1.
                        float largeArc = ReadFlag(d, ref i);
                        float sweep = ReadFlag(d, ref i);
                        float x = ReadNumber(d, ref i);
                        float y = ReadNumber(d, ref i);
                        if (cmd == 'a') { x += cx; y += cy; }
                        AddArc(path, cx, cy, rx, ry, xAxisRotation, largeArc != 0f, sweep != 0f, x, y);
                        cx = x; cy = y;
                        break;
                    }
                    case 'Z':
                    case 'z':
                    {
                        if (subStarted)
                        {
                            path.CloseFigure();
                            cx = subX; cy = subY;
                        }
                        break;
                    }
                }
                lastCmd = cmd;
            }
            return path;
        }

        // Convert quadratic Bezier to a cubic Bezier so GraphicsPath (which only has
        // AddBezier for cubics) can handle it. The standard elevation formula is:
        //   CP1 = P0 + 2/3 * (Q - P0)
        //   CP2 = P2 + 2/3 * (Q - P2)
        private static void AddQuadratic(GraphicsPath p, float p0x, float p0y, float qx, float qy, float p2x, float p2y)
        {
            float cp1x = p0x + (qx - p0x) * 2f / 3f;
            float cp1y = p0y + (qy - p0y) * 2f / 3f;
            float cp2x = p2x + (qx - p2x) * 2f / 3f;
            float cp2y = p2y + (qy - p2y) * 2f / 3f;
            p.AddBezier(p0x, p0y, cp1x, cp1y, cp2x, cp2y, p2x, p2y);
        }

        // Convert an SVG endpoint-parameterization elliptical arc into a sequence of cubic
        // beziers. Algorithm from the SVG spec implementation notes (Appendix F.6) plus the
        // standard cubic-bezier-arc approximation.
        private static void AddArc(GraphicsPath p, float x0, float y0, float rx, float ry,
            float xAxisRotation, bool largeArcFlag, bool sweepFlag, float x1, float y1)
        {
            if (rx == 0 || ry == 0)
            {
                p.AddLine(x0, y0, x1, y1);
                return;
            }

            rx = Math.Abs(rx);
            ry = Math.Abs(ry);

            double sinPhi = Math.Sin(xAxisRotation * Math.PI / 180.0);
            double cosPhi = Math.Cos(xAxisRotation * Math.PI / 180.0);

            double dx = (x0 - x1) / 2.0;
            double dy = (y0 - y1) / 2.0;
            double x0p = cosPhi * dx + sinPhi * dy;
            double y0p = -sinPhi * dx + cosPhi * dy;

            // Ensure radii are large enough.
            double lambda = (x0p * x0p) / (rx * rx) + (y0p * y0p) / (ry * ry);
            if (lambda > 1)
            {
                double s = Math.Sqrt(lambda);
                rx = (float)(rx * s);
                ry = (float)(ry * s);
            }

            double rxSq = rx * rx;
            double rySq = ry * ry;
            double x0pSq = x0p * x0p;
            double y0pSq = y0p * y0p;

            double radicand = (rxSq * rySq - rxSq * y0pSq - rySq * x0pSq) / (rxSq * y0pSq + rySq * x0pSq);
            if (radicand < 0) radicand = 0;
            double coef = Math.Sqrt(radicand);
            if (largeArcFlag == sweepFlag) coef = -coef;

            double cxp = coef * rx * y0p / ry;
            double cyp = -coef * ry * x0p / rx;
            double cx = cosPhi * cxp - sinPhi * cyp + (x0 + x1) / 2.0;
            double cy = sinPhi * cxp + cosPhi * cyp + (y0 + y1) / 2.0;

            double theta1 = AngleBetween(1, 0, (x0p - cxp) / rx, (y0p - cyp) / ry);
            double deltaTheta = AngleBetween(
                (x0p - cxp) / rx, (y0p - cyp) / ry,
                (-x0p - cxp) / rx, (-y0p - cyp) / ry);
            if (!sweepFlag && deltaTheta > 0) deltaTheta -= 2 * Math.PI;
            else if (sweepFlag && deltaTheta < 0) deltaTheta += 2 * Math.PI;

            // Approximate the arc with a sequence of cubic beziers, ~90° each.
            int segments = (int)Math.Ceiling(Math.Abs(deltaTheta) / (Math.PI / 2.0));
            if (segments < 1) segments = 1;
            double delta = deltaTheta / segments;
            double t = (8.0 / 3.0) * Math.Sin(delta / 4.0) * Math.Sin(delta / 4.0) / Math.Sin(delta / 2.0);

            double px = x0;
            double py = y0;
            for (int s = 0; s < segments; s++)
            {
                double a1 = theta1 + s * delta;
                double a2 = theta1 + (s + 1) * delta;
                double cos1 = Math.Cos(a1), sin1 = Math.Sin(a1);
                double cos2 = Math.Cos(a2), sin2 = Math.Sin(a2);
                double e1x = cx + rx * (cosPhi * cos1 - sinPhi * sin1);
                double e1y = cy + ry * (sinPhi * cos1 + cosPhi * sin1);
                double e2x = cx + rx * (cosPhi * cos2 - sinPhi * sin2);
                double e2y = cy + ry * (sinPhi * cos2 + cosPhi * sin2);
                double q1x = e1x - t * (rx * (cosPhi * sin1 + sinPhi * cos1));
                double q1y = e1y - t * (ry * (sinPhi * sin1 - cosPhi * cos1));
                double q2x = e2x + t * (rx * (cosPhi * sin2 + sinPhi * cos2));
                double q2y = e2y + t * (ry * (sinPhi * sin2 - cosPhi * cos2));
                p.AddBezier((float)px, (float)py, (float)q1x, (float)q1y, (float)q2x, (float)q2y, (float)e2x, (float)e2y);
                px = e2x; py = e2y;
            }
        }

        private static double AngleBetween(double ux, double uy, double vx, double vy)
        {
            double dot = ux * vx + uy * vy;
            double len = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
            double c = dot / len;
            if (c < -1) c = -1;
            if (c > 1) c = 1;
            double sign = (ux * vy - uy * vx) < 0 ? -1 : 1;
            return sign * Math.Acos(c);
        }

        private static float ReadNumber(string d, ref int i)
        {
            while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
            if (i >= d.Length) return 0;
            int start = i;
            if (d[i] == '+' || d[i] == '-') i++;
            while (i < d.Length && (char.IsDigit(d[i]) || d[i] == '.')) i++;
            if (i < d.Length && (d[i] == 'e' || d[i] == 'E'))
            {
                i++;
                if (i < d.Length && (d[i] == '+' || d[i] == '-')) i++;
                while (i < d.Length && char.IsDigit(d[i])) i++;
            }
            if (i == start) return 0;
            string token = d.Substring(start, i - start);
            return float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;
        }

        // Reads a single 0/1 flag (used by A command for large-arc-flag and sweep-flag).
        private static float ReadFlag(string d, ref int i)
        {
            while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
            if (i >= d.Length) return 0;
            char ch = d[i];
            i++;
            return ch == '1' ? 1f : 0f;
        }
    }
}
