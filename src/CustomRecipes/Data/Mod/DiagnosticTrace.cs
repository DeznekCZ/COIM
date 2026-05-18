using System;
using System.IO;
using Mafi;
using Mafi.Logging;

namespace CustomAssets.Data.Mod
{
    /// <summary>
    /// Synchronous, file-flushed-every-write diagnostic trace. Used to localize hangs
    /// during mod load when Mafi.Log buffering causes the in-game log file to be
    /// truncated short of the actual stuck-point.
    ///
    /// Two sources of output land in the same trace.log file:
    ///   1. Explicit <see cref="Step"/> calls from our framework code.
    ///   2. A mirror subscription on Mafi.Log.LogReceived that captures every Info /
    ///      Warning / Error / Exception logged by any code (ours or Mafi's). Each
    ///      mirrored entry is flushed immediately so it persists past hangs.
    ///
    /// Output: &lt;COI_Mods&gt;/CustomAssets/Logs/trace.log (overwritten on every load).
    /// </summary>
    public static class DiagnosticTrace
    {
        private static string s_traceFilePath;
        private static readonly object s_lock = new object();
        private static bool s_initialized;
        private static Action<LogEntry> s_logMirror;

        public static void Initialize(string modBasePath)
        {
            if (string.IsNullOrEmpty(modBasePath)) return;
            try
            {
                string logsDir = Path.Combine(modBasePath, "Logs");
                Directory.CreateDirectory(logsDir);
                s_traceFilePath = Path.Combine(logsDir, "trace.log");
                // Overwrite (don't append) on each game load so the trace tracks
                // exactly the current load attempt.
                File.WriteAllText(s_traceFilePath,
                    $"==== Trace start: {DateTime.UtcNow:O} ====" + Environment.NewLine);
                s_initialized = true;
            }
            catch
            {
                // Tracing failures are non-fatal - if we can't write the trace file,
                // just go on without it. Don't bubble up.
                s_initialized = false;
                return;
            }

            // Mirror every Mafi.Log entry into trace.log so we capture what would
            // have gone to Mafi.log even when COI's logger buffer hasn't flushed.
            // Unsubscribe any previous mirror first - on hot reload the static
            // field persists and we'd double-log without this.
            try
            {
                if (s_logMirror != null)
                {
                    Log.LogReceived -= s_logMirror;
                }
                s_logMirror = OnLogReceived;
                Log.LogReceived += s_logMirror;
            }
            catch { /* non-fatal */ }
        }

        private static void OnLogReceived(LogEntry entry)
        {
            // Keep the formatting compact and avoid any heavy work (no stack-trace
            // expansion) since this fires on every Mafi.Log call - which can include
            // tight loops during simulation.
            try
            {
                Step($"[{entry.Type}] {entry.Message}");
            }
            catch { /* swallow - trace must never break logging */ }
        }

        /// <summary>
        /// Append one timestamped line + flush. Safe to call concurrently; no-op if
        /// <see cref="Initialize"/> hasn't been called yet or failed.
        /// </summary>
        public static void Step(string message)
        {
            if (!s_initialized || s_traceFilePath == null) return;
            try
            {
                lock (s_lock)
                {
                    // FileStream with Write+Flush forces the bytes to disk synchronously,
                    // so the last line written remains on disk even if the process hangs
                    // or crashes immediately after this call.
                    using (var fs = new FileStream(s_traceFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var w = new StreamWriter(fs))
                    {
                        w.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff}  {message}");
                        w.Flush();
                        fs.Flush(flushToDisk: true);
                    }
                }
            }
            catch
            {
                // Swallow - tracing must never crash the load itself.
            }
        }
    }
}
