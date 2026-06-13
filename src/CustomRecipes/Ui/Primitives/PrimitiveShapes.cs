using System;
using System.Collections.Generic;

namespace CustomAssets.Ui.Primitives {

    /// Rectangular cell in the wireframe / UV layout image. Used both for UV
    /// assignment on the mesh and for drawing the wireframe PNG template that
    /// ships next to the .obj. All coordinates are in *layout pixel* space —
    /// the layout has its own intrinsic units (computed from the shape's
    /// width / height / depth in metres) and is normalised to UV [0,1] at the
    /// last step.
    public struct LayoutCell {
        public double X;      // left edge
        public double Y;      // top edge (Y grows DOWN in layout coords)
        public double W;
        public double H;

        public LayoutCell(double x, double y, double w, double h) {
            X = x; Y = y; W = w; H = h;
        }
    }

    /// Polygon outline in layout space (for wireframe PNG drawing). Wraps a
    /// rectangular cell or an arbitrary closed loop (used by the cylinder
    /// caps and the container's heap triangles).
    public sealed class LayoutPolygon {
        public List<(double X, double Y)> Points = new List<(double, double)>();
        public string Label;
    }

    /// Output of a shape generator: the mesh itself plus the layout polygons
    /// for the wireframe PNG and the layout's overall pixel size. The mesh's
    /// UVs are already in [0,1]; the layout polygons stay in layout-pixel
    /// space so the PNG renderer can apply its own pixel scale.
    public sealed class ShapeResult {
        public PrimitiveMesh Mesh;
        public List<LayoutPolygon> Layout = new List<LayoutPolygon>();
        public double LayoutW;
        public double LayoutH;
    }

    /// Procedural builders for every preset shape. All shapes sit with their
    /// base on the XZ plane (y=0 is the floor) and are centred on the X/Z
    /// origin — matches the convention vanilla COI unit prefabs use, so the
    /// generated mesh drops straight into a `mesh=` argument without an
    /// import-time pivot fix.
    public static class PrimitiveShapes {

        /// Number of segments around a cylinder's circumference. 16 keeps
        /// silhouettes acceptable for a unit-size prop while keeping the
        /// generated OBJ small enough to hand-edit.
        public const int CylinderSegments = 16;

        // ---- Box (dice-cross unwrap) -----------------------------------------

        /// Axis-aligned box sitting on y=0, dimensions w (X) × h (Y) × d (Z).
        /// UV layout: standard dice-cross unwrap — Top / [Left Front Right
        /// Back] / Bottom. Each face's rectangle in the layout has its 3D
        /// "natural right" mapped to layout-+X and "natural up" mapped to
        /// layout-+V; see comments at face emission for the per-face
        /// orientation.
        public static ShapeResult Box(double w, double h, double d) {
            ShapeResult r = new ShapeResult();
            PrimitiveMesh m = new PrimitiveMesh { Name = "box" };
            r.Mesh = m;

            double hw = w * 0.5;
            double hd = d * 0.5;

            // 8 corner positions. Naming: b=bottom(y=0), t=top(y=h), l/r=−X/+X,
            // f=+Z (front), n=−Z (back / "near the camera" was rejected since
            // unity convention has +Z forward; n here just means "negative Z").
            int blf = m.AddVertex(new V3(-hw, 0, +hd));
            int brf = m.AddVertex(new V3(+hw, 0, +hd));
            int bln = m.AddVertex(new V3(-hw, 0, -hd));
            int brn = m.AddVertex(new V3(+hw, 0, -hd));
            int tlf = m.AddVertex(new V3(-hw, h, +hd));
            int trf = m.AddVertex(new V3(+hw, h, +hd));
            int tln = m.AddVertex(new V3(-hw, h, -hd));
            int trn = m.AddVertex(new V3(+hw, h, -hd));

            // Layout grid (pixel units = metre units; rescaled at PNG time):
            //         +-----+
            //         | Top |
            // +---+-----+---+-----+
            // | L | Frt | R | Back|
            // +---+-----+---+-----+
            //         | Bot |
            //         +-----+
            // Column widths: d (Left), w (Front), d (Right), w (Back).
            // Row heights:   d (Top),  h (middle row),       d (Bottom).
            double layoutW = 2 * d + 2 * w;
            double layoutH = h + 2 * d;
            r.LayoutW = layoutW;
            r.LayoutH = layoutH;

            LayoutCell top    = new LayoutCell(d,           0,         w, d);
            LayoutCell left   = new LayoutCell(0,           d,         d, h);
            LayoutCell front  = new LayoutCell(d,           d,         w, h);
            LayoutCell right  = new LayoutCell(d + w,       d,         d, h);
            LayoutCell back   = new LayoutCell(2 * d + w,   d,         w, h);
            LayoutCell bottom = new LayoutCell(d,           d + h,     w, d);

            int[] frontUv  = uvRect(m, front,  layoutW, layoutH);
            int[] backUv   = uvRect(m, back,   layoutW, layoutH);
            int[] leftUv   = uvRect(m, left,   layoutW, layoutH);
            int[] rightUv  = uvRect(m, right,  layoutW, layoutH);
            int[] topUv    = uvRect(m, top,    layoutW, layoutH);
            int[] bottomUv = uvRect(m, bottom, layoutW, layoutH);

            // Each face: CCW winding viewed from along the outward normal,
            // with corners in (BL, BR, TR, TL) order so the UV indices match
            // the rectangle's corner ordering returned by uvRect.
            m.AddFace(new[] { blf, brf, trf, tlf }, frontUv,  "front");
            m.AddFace(new[] { brn, bln, tln, trn }, backUv,   "back");
            m.AddFace(new[] { bln, blf, tlf, tln }, leftUv,   "left");
            m.AddFace(new[] { brf, brn, trn, trf }, rightUv,  "right");
            m.AddFace(new[] { tlf, trf, trn, tln }, topUv,    "top");
            m.AddFace(new[] { blf, bln, brn, brf }, bottomUv, "bottom");

            r.Layout.Add(rectPolygon(front,  "FRONT"));
            r.Layout.Add(rectPolygon(back,   "BACK"));
            r.Layout.Add(rectPolygon(left,   "LEFT"));
            r.Layout.Add(rectPolygon(right,  "RIGHT"));
            r.Layout.Add(rectPolygon(top,    "TOP"));
            r.Layout.Add(rectPolygon(bottom, "BOTTOM"));

            return r;
        }

        // ---- Cylinder --------------------------------------------------------

        /// Cylinder lying on its side along the X axis (long axis = X,
        /// length = lengthAxisX, diameter = diameter). Caps are at x=±L/2;
        /// y ranges from 0 to diameter. UV layout: side rectangle (perimeter
        /// wide × length tall) and two end-cap circles below it.
        public static ShapeResult CylinderHorizontal(double diameter, double lengthAxisX) {
            return cylinder(diameter, lengthAxisX, horizontal: true);
        }

        /// Cylinder standing on its end along the Y axis (long axis = Y,
        /// height = lengthAxisY, diameter = diameter). Caps at y=0 and
        /// y=height. Same UV layout shape as horizontal.
        public static ShapeResult CylinderVertical(double diameter, double lengthAxisY) {
            return cylinder(diameter, lengthAxisY, horizontal: false);
        }

        private static ShapeResult cylinder(double diameter, double length, bool horizontal) {
            ShapeResult r = new ShapeResult();
            PrimitiveMesh m = new PrimitiveMesh { Name = horizontal ? "cylinder_h" : "cylinder_v" };
            r.Mesh = m;

            int seg = CylinderSegments;
            double radius = diameter * 0.5;

            // Two rings (one per cap) — duplicate ring vertex at the seam so
            // the side UV strip doesn't wrap from u=1 back to u=0 inside a
            // single quad (which would smear the texture). 17 verts per ring.
            int[] ringA = new int[seg + 1];
            int[] ringB = new int[seg + 1];
            for (int i = 0; i <= seg; i++) {
                double t = (double)i / seg;
                double ang = t * Math.PI * 2.0;
                double cos = Math.Cos(ang);
                double sin = Math.Sin(ang);
                if (horizontal) {
                    // Cylinder along X. Cap at x=−L/2 (ringA) and x=+L/2 (ringB).
                    // y ranges 0..diameter so the cylinder sits ON the floor.
                    ringA[i] = m.AddVertex(new V3(-length * 0.5, radius + radius * cos, radius * sin));
                    ringB[i] = m.AddVertex(new V3(+length * 0.5, radius + radius * cos, radius * sin));
                } else {
                    // Cylinder along Y. Cap at y=0 (ringA) and y=length (ringB).
                    ringA[i] = m.AddVertex(new V3(radius * cos, 0, radius * sin));
                    ringB[i] = m.AddVertex(new V3(radius * cos, length, radius * sin));
                }
            }

            // Cap centres (one each).
            int capACenter, capBCenter;
            if (horizontal) {
                capACenter = m.AddVertex(new V3(-length * 0.5, radius, 0));
                capBCenter = m.AddVertex(new V3(+length * 0.5, radius, 0));
            } else {
                capACenter = m.AddVertex(new V3(0, 0,      0));
                capBCenter = m.AddVertex(new V3(0, length, 0));
            }

            // Layout: side rectangle on top (perimeter × length), two cap
            // circles below side by side. Layout pixel units mirror metres
            // so the dice-cross box presets and the cylinder presets render
            // at the same DPI in the wireframe PNG.
            double circumference = Math.PI * diameter;
            double sideW = circumference;
            double sideH = length;
            double capCellW = diameter;
            double capCellH = diameter;

            double layoutW = Math.Max(sideW, 2 * capCellW);
            double layoutH = sideH + capCellH;
            r.LayoutW = layoutW;
            r.LayoutH = layoutH;

            LayoutCell sideCell = new LayoutCell(0, 0, sideW, sideH);

            // Side UVs: 17 columns (seam duplicated) × 2 rows.
            int[] sideUvBottom = new int[seg + 1];
            int[] sideUvTop    = new int[seg + 1];
            for (int i = 0; i <= seg; i++) {
                double u = (double)i / seg * sideW / layoutW;
                double vBottom = 1.0 - sideCell.H / layoutH;   // bottom of side rect
                double vTop    = 1.0;                          // top of side rect (layout y=0)
                sideUvBottom[i] = m.AddUv(new V2(u, vBottom));
                sideUvTop[i]    = m.AddUv(new V2(u, vTop));
            }

            for (int i = 0; i < seg; i++) {
                // Side quad: CCW from outside (which here means looking in
                // toward the cylinder's axis along the radial outward
                // normal). For the horizontal cylinder, ringA is at -X and
                // ringB at +X, so the outward radial is unchanged; the quad
                // wraps around as i grows. Same applies vertically.
                m.AddFace(
                    new[] { ringA[i], ringB[i], ringB[i + 1], ringA[i + 1] },
                    new[] { sideUvBottom[i], sideUvTop[i], sideUvTop[i + 1], sideUvBottom[i + 1] },
                    "side");
            }

            // Cap A (lower-y for vertical / -X for horizontal). Outward
            // normal: −Y (vert) or −X (horiz). Triangle fan from centre.
            double capAcx = capCellW * 0.5;
            double capAcy = sideH + capCellH * 0.5;
            double capBcx = capCellW + capCellW * 0.5;
            double capBcy = sideH + capCellH * 0.5;

            int capACenterUv = m.AddUv(new V2(capAcx / layoutW, 1.0 - capAcy / layoutH));
            int capBCenterUv = m.AddUv(new V2(capBcx / layoutW, 1.0 - capBcy / layoutH));
            int[] capARimUv = new int[seg];
            int[] capBRimUv = new int[seg];
            LayoutPolygon capAPoly = new LayoutPolygon { Label = "CAP A" };
            LayoutPolygon capBPoly = new LayoutPolygon { Label = "CAP B" };
            for (int i = 0; i < seg; i++) {
                double ang = (double)i / seg * Math.PI * 2.0;
                double cosA = Math.Cos(ang);
                double sinA = Math.Sin(ang);
                double aX = capAcx + radius * cosA;
                double aY = capAcy + radius * sinA;
                double bX = capBcx + radius * cosA;
                double bY = capBcy + radius * sinA;
                capARimUv[i] = m.AddUv(new V2(aX / layoutW, 1.0 - aY / layoutH));
                capBRimUv[i] = m.AddUv(new V2(bX / layoutW, 1.0 - bY / layoutH));
                capAPoly.Points.Add((aX, aY));
                capBPoly.Points.Add((bX, bY));
            }

            for (int i = 0; i < seg; i++) {
                int next = (i + 1) % seg;
                // Cap A: outward normal points away from ringB. Triangle wound
                // so that the outward normal matches; for the horizontal
                // cylinder this is -X, for vertical -Y. The winding below
                // gives that in both cases because of how the rings were laid
                // down (ringA at smaller axis coordinate).
                m.AddFace(
                    new[] { capACenter, ringA[next], ringA[i] },
                    new[] { capACenterUv, capARimUv[next], capARimUv[i] },
                    "cap_a");
                m.AddFace(
                    new[] { capBCenter, ringB[i], ringB[next] },
                    new[] { capBCenterUv, capBRimUv[i], capBRimUv[next] },
                    "cap_b");
            }

            r.Layout.Add(rectPolygon(sideCell, "SIDE"));
            r.Layout.Add(capAPoly);
            r.Layout.Add(capBPoly);

            return r;
        }

        // ---- Container with heap --------------------------------------------

        /// Open-top-looking solid cuboid (the "container") with a small
        /// 4-sided pyramid heap sitting on top in the middle. Footprint
        /// w × h × d with heap height = 0.5 × h above the container top and
        /// heap base half the container's smaller XZ dimension.
        public static ShapeResult ContainerWithHeap(double w, double h, double d) {
            ShapeResult r = new ShapeResult();
            PrimitiveMesh m = new PrimitiveMesh { Name = "container_heap" };
            r.Mesh = m;

            // Container: same vertex layout as Box. Reuse the dice-cross
            // unwrap so the modder can still texture the outside cleanly.
            double hw = w * 0.5;
            double hd = d * 0.5;

            int blf = m.AddVertex(new V3(-hw, 0, +hd));
            int brf = m.AddVertex(new V3(+hw, 0, +hd));
            int bln = m.AddVertex(new V3(-hw, 0, -hd));
            int brn = m.AddVertex(new V3(+hw, 0, -hd));
            int tlf = m.AddVertex(new V3(-hw, h, +hd));
            int trf = m.AddVertex(new V3(+hw, h, +hd));
            int tln = m.AddVertex(new V3(-hw, h, -hd));
            int trn = m.AddVertex(new V3(+hw, h, -hd));

            // Heap: 4 triangles meeting at apex. Heap base sits at the
            // container's top surface (y=h). Base side = 0.6 × min(w,d) so
            // it visibly leaves room around the rim. Apex height = 0.5×h
            // above the container top — looks like a small pile.
            double heapBaseHalf = 0.5 * 0.6 * Math.Min(w, d);
            double heapHeight = 0.5 * h;
            int hbl = m.AddVertex(new V3(-heapBaseHalf, h, +heapBaseHalf));
            int hbr = m.AddVertex(new V3(+heapBaseHalf, h, +heapBaseHalf));
            int hnl = m.AddVertex(new V3(-heapBaseHalf, h, -heapBaseHalf));
            int hnr = m.AddVertex(new V3(+heapBaseHalf, h, -heapBaseHalf));
            int apex = m.AddVertex(new V3(0, h + heapHeight, 0));

            // Layout: container dice-cross on top, heap triangles in a row
            // below. The user spec says "container outer and inner pile on
            // same PNG" — that's exactly this layout.
            double crossW = 2 * d + 2 * w;
            double crossH = h + 2 * d;
            double heapTriW = 2 * heapBaseHalf;
            double heapTriH = Math.Sqrt(heapHeight * heapHeight + heapBaseHalf * heapBaseHalf);
            double heapStripW = 4 * heapTriW;
            double heapStripH = heapTriH;
            double gap = Math.Max(crossH, crossW) * 0.05;

            double layoutW = Math.Max(crossW, heapStripW);
            double layoutH = crossH + gap + heapStripH;
            r.LayoutW = layoutW;
            r.LayoutH = layoutH;

            LayoutCell top    = new LayoutCell(d,           0,         w, d);
            LayoutCell left   = new LayoutCell(0,           d,         d, h);
            LayoutCell front  = new LayoutCell(d,           d,         w, h);
            LayoutCell right  = new LayoutCell(d + w,       d,         d, h);
            LayoutCell back   = new LayoutCell(2 * d + w,   d,         w, h);
            LayoutCell bottom = new LayoutCell(d,           d + h,     w, d);

            int[] frontUv  = uvRect(m, front,  layoutW, layoutH);
            int[] backUv   = uvRect(m, back,   layoutW, layoutH);
            int[] leftUv   = uvRect(m, left,   layoutW, layoutH);
            int[] rightUv  = uvRect(m, right,  layoutW, layoutH);
            int[] topUv    = uvRect(m, top,    layoutW, layoutH);
            int[] bottomUv = uvRect(m, bottom, layoutW, layoutH);

            m.AddFace(new[] { blf, brf, trf, tlf }, frontUv,  "container_front");
            m.AddFace(new[] { brn, bln, tln, trn }, backUv,   "container_back");
            m.AddFace(new[] { bln, blf, tlf, tln }, leftUv,   "container_left");
            m.AddFace(new[] { brf, brn, trn, trf }, rightUv,  "container_right");
            m.AddFace(new[] { tlf, trf, trn, tln }, topUv,    "container_top");
            m.AddFace(new[] { blf, bln, brn, brf }, bottomUv, "container_bottom");

            r.Layout.Add(rectPolygon(front,  "FRONT"));
            r.Layout.Add(rectPolygon(back,   "BACK"));
            r.Layout.Add(rectPolygon(left,   "LEFT"));
            r.Layout.Add(rectPolygon(right,  "RIGHT"));
            r.Layout.Add(rectPolygon(top,    "TOP"));
            r.Layout.Add(rectPolygon(bottom, "BOTTOM"));

            // Heap triangles: 4 walls, no base / no top. Winding CCW from
            // outside each triangle (apex is the shared "back" of every
            // wall, so apex is the third vertex after the two adjacent
            // base corners going around the heap CCW from above).
            double stripY = crossH + gap;
            string[] heapLabels = new[] { "HEAP F", "HEAP R", "HEAP B", "HEAP L" };
            (int A, int B)[] heapEdges = new (int, int)[] {
                (hbl, hbr),  // front edge
                (hbr, hnr),  // right edge
                (hnr, hnl),  // back edge
                (hnl, hbl),  // left edge
            };
            for (int i = 0; i < 4; i++) {
                double triX = i * heapTriW;
                double triY = stripY;
                // Triangle in layout: base on bottom, apex on top centre.
                double blX = triX;
                double blY = triY + heapTriH;
                double brX = triX + heapTriW;
                double brY = triY + heapTriH;
                double apX = triX + heapTriW * 0.5;
                double apY = triY;

                int uvA  = m.AddUv(new V2(blX / layoutW, 1.0 - blY / layoutH));
                int uvB  = m.AddUv(new V2(brX / layoutW, 1.0 - brY / layoutH));
                int uvAp = m.AddUv(new V2(apX / layoutW, 1.0 - apY / layoutH));

                m.AddFace(
                    new[] { heapEdges[i].A, heapEdges[i].B, apex },
                    new[] { uvA, uvB, uvAp },
                    "heap");

                LayoutPolygon tri = new LayoutPolygon { Label = heapLabels[i] };
                tri.Points.Add((blX, blY));
                tri.Points.Add((brX, brY));
                tri.Points.Add((apX, apY));
                r.Layout.Add(tri);
            }

            return r;
        }

        // ---- Helpers ---------------------------------------------------------

        /// Allocate 4 UVs (BL, BR, TR, TL of <paramref name="cell"/>) on
        /// <paramref name="mesh"/> and return their indices. Layout cells use
        /// Y-down; UV space uses Y-up — the conversion (1.0 − y/layoutH) is
        /// applied here in one place so face writers don't have to.
        private static int[] uvRect(PrimitiveMesh mesh, LayoutCell cell, double layoutW, double layoutH) {
            double l = cell.X / layoutW;
            double rg = (cell.X + cell.W) / layoutW;
            double bv = 1.0 - (cell.Y + cell.H) / layoutH; // bottom in UV space
            double tv = 1.0 - cell.Y / layoutH;            // top in UV space
            int bl = mesh.AddUv(new V2(l,  bv));
            int br = mesh.AddUv(new V2(rg, bv));
            int tr = mesh.AddUv(new V2(rg, tv));
            int tl = mesh.AddUv(new V2(l,  tv));
            return new[] { bl, br, tr, tl };
        }

        private static LayoutPolygon rectPolygon(LayoutCell cell, string label) {
            LayoutPolygon p = new LayoutPolygon { Label = label };
            p.Points.Add((cell.X,          cell.Y));
            p.Points.Add((cell.X + cell.W, cell.Y));
            p.Points.Add((cell.X + cell.W, cell.Y + cell.H));
            p.Points.Add((cell.X,          cell.Y + cell.H));
            return p;
        }
    }
}
