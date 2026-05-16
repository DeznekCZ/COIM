namespace CustomAssets.IconStudio.Services;

// Compact text patch storing only the changed interval between two strings.
//
// Encoding: identical prefix length, identical suffix length, the chunk that
// disappears (Removed), the chunk that takes its place (Added). For a localized
// edit (single shape's color, a vertex move) the patch is just a few bytes —
// vs. ~10 KB for a full XML snapshot. Multiple non-contiguous edits collapse
// into the spanning range, which is still typically much smaller than the
// whole document.
//
// Patches are invertible (swap Removed / Added), which makes redo trivial: the
// inverse of the patch we popped from the undo stack is exactly the patch to
// push onto the redo stack.
public sealed record DiffPatch(int CommonPrefix, int CommonSuffix, string Removed, string Added)
{
    public DiffPatch Inverse() => new(CommonPrefix, CommonSuffix, Added, Removed);

    // Approximate in-memory cost of holding this patch (for telemetry).
    public int Size => Removed.Length + Added.Length;

    public string ApplyTo(string baseline)
    {
        if (CommonPrefix + Removed.Length + CommonSuffix != baseline.Length)
            throw new InvalidOperationException(
                $"DiffPatch baseline length mismatch: expected {CommonPrefix + Removed.Length + CommonSuffix}, got {baseline.Length}");
        if (baseline.AsSpan(CommonPrefix, Removed.Length).SequenceEqual(Removed.AsSpan()) == false)
            throw new InvalidOperationException("DiffPatch baseline middle does not match Removed");

        var sb = new System.Text.StringBuilder(CommonPrefix + Added.Length + CommonSuffix);
        sb.Append(baseline, 0, CommonPrefix);
        sb.Append(Added);
        sb.Append(baseline, CommonPrefix + Removed.Length, CommonSuffix);
        return sb.ToString();
    }

    // Computes the patch that turns `from` into `to`.
    public static DiffPatch Between(string from, string to)
    {
        int p = 0;
        int maxP = Math.Min(from.Length, to.Length);
        while (p < maxP && from[p] == to[p]) p++;

        int s = 0;
        int maxS = Math.Min(from.Length - p, to.Length - p);
        while (s < maxS && from[from.Length - 1 - s] == to[to.Length - 1 - s]) s++;

        return new DiffPatch(
            p, s,
            from.Substring(p, from.Length - p - s),
            to.Substring(p, to.Length - p - s));
    }
}
