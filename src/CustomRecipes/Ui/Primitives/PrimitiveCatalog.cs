using System;
using System.Collections.Generic;

namespace CustomAssets.Ui.Primitives {

    /// One entry in the preset shape picker. Includes the bounding-box
    /// dimensions, a stackable hint (does this shape pack into a COI tile
    /// without looking ugly?), and a generator delegate. The generator is
    /// passed the entry's own dimensions for fixed presets and overridden
    /// values for the "custom box" entry.
    public sealed class PrimitivePreset {

        public string Id;
        public string DisplayName;
        public string Description;
        public double Width;
        public double Height;
        public double Depth;
        public bool StacksWell;
        public Func<double, double, double, ShapeResult> Build;

        /// Resolves to the OBJ filename (without extension) when this preset
        /// is generated for the first time. The dialog suffixes _0, _1, …
        /// when the file already exists.
        public string DefaultBaseName;
    }

    /// Built-in preset library. The user picks one (or "Custom" for free
    /// dimensions); the dialog hands the picked dimensions to the build
    /// delegate and writes the resulting OBJ + wireframe PNG into the
    /// loaded pack. New presets can be added here without touching the UI.
    public static class PrimitiveCatalog {

        public const string CustomId = "custom_box";

        public static readonly IReadOnlyList<PrimitivePreset> All = new List<PrimitivePreset> {
            new PrimitivePreset {
                Id = "wide_box",
                DisplayName = "Wide box",
                Description = "1.0 × 0.3 × 0.3 — flat, wide rectangular box. Stacks evenly.",
                Width = 1.0, Height = 0.3, Depth = 0.3,
                StacksWell = true,
                DefaultBaseName = "wide_box",
                Build = (w, h, d) => PrimitiveShapes.Box(w, h, d),
            },
            new PrimitivePreset {
                Id = "tall_box",
                DisplayName = "Tall box (triangle layout)",
                Description = "0.3 × 1.0 × 0.3 — narrow upright box, fits 3-up in a tile.",
                Width = 0.3, Height = 1.0, Depth = 0.3,
                StacksWell = true,
                DefaultBaseName = "tall_box",
                Build = (w, h, d) => PrimitiveShapes.Box(w, h, d),
            },
            new PrimitivePreset {
                Id = "flat_plate",
                DisplayName = "Flat plate",
                Description = "1.0 × 0.1 × 1.0 — thin square plate. Stacks cleanly.",
                Width = 1.0, Height = 0.1, Depth = 1.0,
                StacksWell = true,
                DefaultBaseName = "flat_plate",
                Build = (w, h, d) => PrimitiveShapes.Box(w, h, d),
            },
            new PrimitivePreset {
                Id = "cylinder_h",
                DisplayName = "Cylinder (horizontal)",
                Description = "Long axis X (length 1.0), diameter 0.3. Lies on its side.",
                Width = 1.0, Height = 0.3, Depth = 0.3,
                StacksWell = false,
                DefaultBaseName = "cylinder_h",
                Build = (w, h, d) => PrimitiveShapes.CylinderHorizontal(Math.Min(h, d), w),
            },
            new PrimitivePreset {
                Id = "cylinder_v",
                DisplayName = "Cylinder (vertical)",
                Description = "Long axis Y (height 1.0), diameter 0.3. Stands on its end.",
                Width = 0.3, Height = 1.0, Depth = 0.3,
                StacksWell = false,
                DefaultBaseName = "cylinder_v",
                Build = (w, h, d) => PrimitiveShapes.CylinderVertical(Math.Min(w, d), h),
            },
            new PrimitivePreset {
                Id = "container_heap",
                DisplayName = "Container + heap",
                Description = "0.8 × 0.3 × 0.8 box with a small loose-product heap on top.",
                Width = 0.8, Height = 0.3, Depth = 0.8,
                StacksWell = true,
                DefaultBaseName = "container_heap",
                Build = (w, h, d) => PrimitiveShapes.ContainerWithHeap(w, h, d),
            },
            new PrimitivePreset {
                Id = CustomId,
                DisplayName = "Custom box…",
                Description = "Pick your own width / height / depth. Built as a plain box.",
                Width = 0.5, Height = 0.5, Depth = 0.5,
                StacksWell = true,
                DefaultBaseName = "custom_box",
                Build = (w, h, d) => PrimitiveShapes.Box(w, h, d),
            },
        };

        public static PrimitivePreset FindById(string id) {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (PrimitivePreset p in All) {
                if (p.Id == id) return p;
            }
            return null;
        }
    }
}
