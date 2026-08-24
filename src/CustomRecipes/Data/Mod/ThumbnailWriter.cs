using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CustomAssets.ModBuilder.Svg;
using Mafi;

namespace CustomAssets.Data.Mod;

/// <summary>
/// Writes a pack's <c>Thumbnail.png</c> — the image COI's mods panel and the
/// editor's pack switcher show for the pack (see
/// <see cref="CustomAssets.Ui.PackThumbnailCache"/>, which reads exactly that
/// file name from the pack root).
///
/// Three source shapes are accepted:
///   • <c>.png</c> — copied verbatim.
///   • <c>.jpg</c> / <c>.jpeg</c> — decoded and re-encoded as PNG.
///   • <c>.svg</c> — copied to the pack root as <c>thumbnail.svg</c> AND
///     rasterized to <c>Thumbnail.png</c> at <see cref="ThumbnailSize"/>.
///
/// The SVG source is kept alongside the PNG on purpose: the generated pack
/// csproj already runs <c>ModBuilder svg2png</c> over a pack-root
/// <c>thumbnail.svg</c> at build time, so keeping the vector next to the
/// raster means a later rebuild reproduces the same image instead of
/// silently dropping back to whatever PNG happened to be there.
///
/// Rasterization shares ModBuilder's <see cref="SvgRenderer"/>, which is
/// linked into this assembly (see CustomAssets.csproj). It is built on
/// System.Drawing/GDI+; the game ships System.Drawing.dll, but if GDI+ is
/// unavailable at runtime the failure is caught and reported as an error the
/// UI shows verbatim rather than taking down the editor.
/// </summary>
public static class ThumbnailWriter {

    /// Square edge, in pixels, of the generated PNG. Matches the existing
    /// hand-authored pack thumbnails (CoalLiquefaction ships 256×256) and
    /// the size the mods panel renders at without upscaling.
    public const int ThumbnailSize = 256;

    /// Supersample factor handed to the renderer — render at 2× then
    /// downscale, same default the build-time svg2png command uses.
    private const int Supersample = 2;

    public const string ThumbnailFileName = "Thumbnail.png";
    public const string SvgSourceFileName = "thumbnail.svg";

    public sealed class Result {
        public bool Success;
        /// Absolute path of the written PNG. Populated on success only.
        public string WrittenPath;
        /// True when the source was an SVG that we rasterized, so callers
        /// can mention the conversion in their status line.
        public bool ConvertedFromSvg;
        /// Human-readable failure reason, phrased as a sentence so the UI
        /// can show it verbatim.
        public string Error;

        public static Result Ok(string path, bool converted) {
            return new Result { Success = true, WrittenPath = path, ConvertedFromSvg = converted };
        }

        public static Result Fail(string error) {
            return new Result { Success = false, Error = error };
        }
    }

    /// <summary>True when <paramref name="path"/> has an extension this
    /// writer can turn into a thumbnail. Used by the picker to filter the
    /// files it offers.</summary>
    public static bool IsSupportedSource(string path) {
        string ext = safeExtension(path);
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".svg";
    }

    public static bool IsSvg(string path) {
        return safeExtension(path) == ".svg";
    }

    /// <summary>Install <paramref name="sourcePath"/> as the thumbnail of
    /// the pack rooted at <paramref name="packRoot"/>. Overwrites any
    /// existing thumbnail — replacing one is the same operation as setting
    /// the first.</summary>
    public static Result Apply(string packRoot, string sourcePath) {
        if (string.IsNullOrWhiteSpace(packRoot) || !Directory.Exists(packRoot)) {
            return Result.Fail("Pack folder not found - cannot write the thumbnail.");
        }
        if (string.IsNullOrWhiteSpace(sourcePath)) {
            return Result.Fail("No image selected.");
        }
        sourcePath = sourcePath.Trim().Trim('"');
        if (!File.Exists(sourcePath)) {
            return Result.Fail("Image not found: '" + sourcePath + "'.");
        }
        if (!IsSupportedSource(sourcePath)) {
            return Result.Fail("Unsupported image type '" + safeExtension(sourcePath)
                + "'. Use a .png, .jpg, or .svg file.");
        }

        string destination = Path.Combine(packRoot, ThumbnailFileName);
        try {
            if (IsSvg(sourcePath)) {
                return applySvg(packRoot, sourcePath, destination);
            }
            if (safeExtension(sourcePath) == ".png") {
                copyIfDistinct(sourcePath, destination);
            } else {
                convertToPng(sourcePath, destination);
            }
            invalidateCaches();
            return Result.Ok(destination, converted: false);
        } catch (Exception ex) {
            Log.Warning("ThumbnailWriter: failed to write '" + destination + "' - " + ex.Message);
            return Result.Fail("Could not write the thumbnail: " + ex.Message);
        }
    }

    // SVG path: keep the vector source in the pack (so a later ModBuilder
    // build re-renders it) and rasterize it now so the thumbnail is visible
    // without a rebuild.
    private static Result applySvg(string packRoot, string sourcePath, string destination) {
        string svgInPack = Path.Combine(packRoot, SvgSourceFileName);
        copyIfDistinct(sourcePath, svgInPack);

        try {
            SvgRenderer renderer = new SvgRenderer(ThumbnailSize, ThumbnailSize, Supersample);
            renderer.RenderToFile(svgInPack, destination);
        } catch (Exception ex) {
            // Most likely cause is GDI+ being unavailable in the player's
            // runtime. The .svg is already in the pack at this point, so
            // tell the modder the one thing that still works: a build-time
            // conversion, which produces the same file.
            Log.Warning("ThumbnailWriter: SVG rasterization failed - " + ex.Message);
            return Result.Fail("Copied the SVG into the pack, but could not convert it to PNG here: "
                + ex.Message
                + " Run 'ModBuilder svg2png \"" + svgInPack + "\" --size " + ThumbnailSize
                + "' to generate Thumbnail.png.");
        }

        invalidateCaches();
        return Result.Ok(destination, converted: true);
    }

    // Re-encode a non-PNG raster (jpg) as PNG. Kept separate from the .png
    // branch so the common case stays a plain file copy with no GDI+
    // dependency at all.
    private static void convertToPng(string sourcePath, string destination) {
        using (Bitmap source = new Bitmap(sourcePath)) {
            source.Save(destination, ImageFormat.Png);
        }
    }

    private static void copyIfDistinct(string sourcePath, string destination) {
        // Re-applying the pack's own current thumbnail would otherwise throw
        // "source and destination are the same file".
        if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destination),
                StringComparison.OrdinalIgnoreCase)) {
            return;
        }
        File.Copy(sourcePath, destination, overwrite: true);
    }

    // Both caches key on paths/ids that just changed meaning on disk. They
    // are session-lifetime caches, so without this the editor keeps showing
    // the previous thumbnail (or the fallback) until the game restarts.
    private static void invalidateCaches() {
        try {
            CustomAssets.Ui.PackThumbnailCache.Clear();
            CustomAssets.Ui.AssetThumbnailCache.Clear();
        } catch (Exception ex) {
            Log.Warning("ThumbnailWriter: cache invalidation failed - " + ex.Message);
        }
    }

    private static string safeExtension(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return "";
        }
        try {
            return (Path.GetExtension(path) ?? "").ToLowerInvariant();
        } catch (ArgumentException) {
            // Path.GetExtension throws on invalid path characters — a
            // half-typed path in the picker's text field hits this.
            return "";
        }
    }
}
