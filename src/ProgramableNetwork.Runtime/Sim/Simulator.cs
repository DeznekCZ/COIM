using System;
using System.Collections.Generic;
using Mafi;
using ProgramableNetwork.Python;

namespace ProgramableNetwork.Runtime.Sim
{
    /// <summary>
    /// Per-tick execution engine for a <see cref="SimController"/>. Resolves each module's inputs from
    /// its connections, then runs Init (once) → action → Display, exactly through the source-linked
    /// interpreter (same code path as in-game). Modules run in dependency order; cycles fall back to
    /// placement order with one-tick latency on the back edges (the source's previous output).
    /// </summary>
    public sealed class Simulator
    {
        private readonly SimController controller;
        private readonly Dictionary<long, SimModule> byId = new Dictionary<long, SimModule>();
        private List<SimModule> order = new List<SimModule>();

        public long Tick { get; private set; }

        public Simulator(SimController controller)
        {
            this.controller = controller;
            Rebuild();
        }

        /// <summary>Recompute the id index + execution order. Call after modules/connections change.</summary>
        public void Rebuild()
        {
            byId.Clear();
            foreach (SimModule m in controller.Modules)
            {
                byId[m.Id] = m;
                m.Controller = controller;
            }
            order = TopologicalOrder();
        }

        /// <summary>Clear runtime state so a run starts fresh (keeps fields, connections, placement).</summary>
        public void Reset()
        {
            Tick = 0;
            foreach (SimModule m in controller.Modules)
            {
                m.Outputs.Clear();
                m.Inputs.Clear();
                m.Displays.Clear();
                m.NumberData.Clear();
                m.StringData.Clear();
                m.Array.Clear();
                m.Status = ModuleStatus.Init;
                m.Error = "";
                m.Info = false;
                m.Warning = false;
                m.HasInitialised = false;
            }
        }

        public void Step()
        {
            foreach (SimModule m in order)
            {
                ResolveInputs(m);
                Run(m);
            }
            Tick++;
        }

        private void ResolveInputs(SimModule m)
        {
            int count = m.EffectiveInputCount;
            for (int i = 0; i < count; i++)
            {
                string pin = m.EffectiveInputId(i);
                if (string.IsNullOrEmpty(pin))
                {
                    continue;
                }
                m.Inputs[pin] = ResolveValue(m, pin);
            }
        }

        private Fix32 ResolveValue(SimModule m, string pin)
        {
            if (!m.Connections.TryGetValue(pin, out Connection c) || c == null)
            {
                return Fix32.Zero;
            }
            switch (c.Kind)
            {
                case ConnectionKind.Constant:
                    return c.ConstantValue;
                case ConnectionKind.Module:
                    return byId.TryGetValue(c.SourceModuleId, out SimModule src)
                        ? src.GetOutput(c.SourceOutputId, Fix32.Zero)
                        : Fix32.Zero;
                case ConnectionKind.Bus:
                    SimBus bus = controller.GetBusById(c.BusId);
                    return bus != null && c.BusPinIndex >= 0 && c.BusPinIndex < SimBus.PinCount
                        ? bus.PinValues[c.BusPinIndex]
                        : Fix32.Zero;
                default:
                    return Fix32.Zero;
            }
        }

        private void Run(SimModule m)
        {
            ModuleWrapper wrapper = new ModuleWrapper(m, m.Definition.PythonClass);
            try
            {
                if (!m.HasInitialised)
                {
                    if (m.Definition.InitMethod != null)
                    {
                        Invoke(m.Definition.InitMethod, wrapper);
                    }
                    m.HasInitialised = true;
                }

                if (m.Definition.ActionMethod != null)
                {
                    object ret = Invoke(m.Definition.ActionMethod, wrapper);
                    if (m.Status != ModuleStatus.Error)
                    {
                        m.Status = ret is ModuleStatus s ? s : ModuleStatus.Running;
                    }
                }
                else
                {
                    m.Status = ModuleStatus.Running;
                }

                if (m.Definition.DisplayMethod != null && m.Status != ModuleStatus.Error)
                {
                    Invoke(m.Definition.DisplayMethod, wrapper);
                }
            }
            catch (Exception ex)
            {
                m.Status = ModuleStatus.Error;
                m.Error = ex.Message;
            }
        }

        private static object Invoke(Method method, ModuleWrapper self)
        {
            method.Self = self;
            return Expressions.__call__(method, new List<(string name, object value)>());
        }

        private List<SimModule> TopologicalOrder()
        {
            // Kahn's algorithm over module→module connections; back edges of cycles are ignored for
            // ordering and resolve with one-tick latency at run time (reading last tick's output).
            Dictionary<long, int> indegree = new Dictionary<long, int>();
            Dictionary<long, List<long>> outgoing = new Dictionary<long, List<long>>();
            foreach (SimModule m in controller.Modules)
            {
                indegree[m.Id] = 0;
                outgoing[m.Id] = new List<long>();
            }

            HashSet<(long, long)> edges = new HashSet<(long, long)>();
            foreach (SimModule m in controller.Modules)
            {
                foreach (Connection c in m.Connections.Values)
                {
                    if (c == null || c.Kind != ConnectionKind.Module || !byId.ContainsKey(c.SourceModuleId))
                    {
                        continue;
                    }
                    (long, long) edge = (c.SourceModuleId, m.Id);
                    if (c.SourceModuleId == m.Id || edges.Contains(edge))
                    {
                        continue;
                    }
                    edges.Add(edge);
                    outgoing[c.SourceModuleId].Add(m.Id);
                    indegree[m.Id]++;
                }
            }

            Queue<long> ready = new Queue<long>();
            foreach (SimModule m in controller.Modules)
            {
                if (indegree[m.Id] == 0)
                {
                    ready.Enqueue(m.Id);
                }
            }

            List<SimModule> result = new List<SimModule>();
            HashSet<long> placed = new HashSet<long>();
            while (ready.Count > 0)
            {
                long id = ready.Dequeue();
                result.Add(byId[id]);
                placed.Add(id);
                foreach (long next in outgoing[id])
                {
                    if (--indegree[next] == 0)
                    {
                        ready.Enqueue(next);
                    }
                }
            }

            // Any modules left are part of a cycle — append in placement order.
            foreach (SimModule m in controller.Modules)
            {
                if (!placed.Contains(m.Id))
                {
                    result.Add(m);
                }
            }
            return result;
        }
    }
}
