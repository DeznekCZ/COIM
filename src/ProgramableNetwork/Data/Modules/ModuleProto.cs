﻿using Mafi.Serialization;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Base;
using Mafi.Unity.UserInterface.Style;
using Mafi.Unity.UiFramework.Components;
using Mafi.Core.Entities.Static;
using System.Linq;

namespace ProgramableNetwork
{
    public class ModuleProto : EntityProto, IProtoWithIcon
    {

        [DebuggerStepThrough]
        [DebuggerDisplay("{Value,nq}")]
        [ManuallyWrittenSerialization]
        public new readonly struct ID : IEquatable<ID>, IComparable<ID>
        {
            //
            // Souhrn:
            //     Underlying string value of this Id.
            public readonly string Value;

            public ID(string value)
            {
                Value = value;
            }

            public static bool operator ==(ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(Proto.ID lhs, ID rhs)
            {
                return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator ==(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public static bool operator !=(ID lhs, Proto.ID rhs)
            {
                return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
            }

            public override bool Equals(object other)
            {
                if (other is ID)
                {
                    ID other2 = (ID)other;
                    return Equals(other2);
                }

                return false;
            }

            public bool Equals(ID other)
            {
                return string.Equals(Value, other.Value, StringComparison.Ordinal);
            }

            public int CompareTo(ID other)
            {
                return string.CompareOrdinal(Value, other.Value);
            }

            public override string ToString()
            {
                return Value ?? string.Empty;
            }

            public override int GetHashCode()
            {
                return Value?.GetHashCode() ?? 0;
            }

            public static void Serialize(ID value, BlobWriter writer)
            {
                writer.WriteString(value.Value);
            }

            public static ID Deserialize(BlobReader reader)
            {
                return new ID(reader.ReadString());
            }

            public static implicit operator Proto.ID(ID id)
            {
                return new Proto.ID(id.Value);
            }

            public static implicit operator EntityProto.ID(ID id)
            {
                return new EntityProto.ID(id.Value);
            }
        }


        public new class Gfx : EntityProto.Gfx
        {
            public static new readonly Gfx Empty;

            public string IconPath { get; }

            public Gfx(string iconPath, ColorRgba? color = null)
                : base(color ?? ColorRgba.White)
            {
                this.IconPath = iconPath;
            }

            static Gfx()
            {
                Empty = new Gfx(new UiStyle().Icons.Empty, ColorRgba.Empty);
            }
        }

        public new ID Id { get; }
        public Func<Module, ModuleStatus> Action { get; }
        public Func<Module, ModuleStatus> Init { get; }
        public bool IsInputModule { get; }
        public bool IsOutputModule { get; }
        public List<ModuleConnectorProto> Inputs { get; }
        public List<ModuleConnectorProto> Outputs { get; }
        public List<ModuleConnectorProto> Displays { get; }
        public List<Category> Categories { get; }
        public List<IField> Fields { get; }
        public Electricity UsedPower { get; }
        public PartialQuantity UsedComputing { get; }
        public Action<Module, StackContainer> DisplayFunction { get; }
        public Func<Module, int> WidthFunction { get; }

        public override Type EntityType => typeof(Module);

        public new Gfx Graphics { get; }
        public string IconPath => Graphics.IconPath;

        public string Symbol { get; }
        public List<StaticEntityProto.ID> AllowedDevices { get; }

        public static readonly ModuleProto Phantom;
        protected static readonly ID PHANTOM_PRODUCT_ID;

        static ModuleProto() {
            try
            {
                PHANTOM_PRODUCT_ID = new ID("__PHANTOM__MODULE__");
                Phantom = new Builder(null, PHANTOM_PRODUCT_ID)
                    .SetDescritpion("Module replacement for already nonexsiting module")
                    .SetGfx(Assets.Base.Products.Icons.Vegetables_svg)
                    .SetSymbol("!!")
                    .SetName("[Removed module]")
                    .Action(m => ModuleStatus.Error)
                    //.AddDisplay("__display__", "Index", 1)
                    //.AddDisplay("__display__", "Display of keys", 5)
                    //.Action((module) =>
                    //{
                    //    if (module.IsDebugging)
                    //    {
                    //        var stringKeys = module.StringData
                    //            .Where(k => k != "display____display__")
                    //            .ToArray();
                    //
                    //        var intKeys = module.NumberData
                    //            .Where(k => k != "display____display__")
                    //            .ToArray();
                    //    }
                    //    return ModuleStatus.Error;
                    //})
                    .Build();
                Proto.RegisterPhantom(Phantom);
                typeof(ModuleProto)
                    .GetField("<WidthFunction>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(Phantom, (Func<Module, int>)((m) =>
                    {
                        if (m.NumberData.TryGetValue("phantom__width", out int width))
                        {
                            return width;
                        }
                        bool rowHit = false;
                        foreach (var row in m.Controller.Rows)
                        {
                            foreach (var column in row)
                            {
                                if (column.ModuleId == m.Id)
                                {
                                    rowHit = true;
                                    width++;
                                    continue;
                                }
                                if (rowHit) break;
                            }
                            if (rowHit) break;
                        }
                        m.NumberData["phantom__width"] = width;
                        return width;
                    }));

            }
            catch (Exception e)
            {
                Log.Exception(e);
                throw;
            }
        }

        public ModuleProto(ID id, Str strings, EntityCosts costs, Gfx gfx, IEnumerable<Tag> tags, Func<Module, ModuleStatus> action, Func<Module, ModuleStatus> m_init, bool isInputModule, bool isOutputModule, Electricity usedPower, PartialQuantity usedComputing,
            List<ModuleConnectorProto> m_inputs, List<ModuleConnectorProto> m_outputs, List<ModuleConnectorProto> m_displays, List<IField> m_fields,
            Action<Module, StackContainer> m_displayFunction, Func<Module, int> m_widthFunction, string m_symbol, List<StaticEntityProto.ID> m_allowedDevices, List<Category> m_categories) : base(id, strings, costs, gfx, tags)
        {
            Id = id;
            Symbol = m_symbol;
            Action = action;
            Init = m_init;
            IsInputModule = isInputModule;
            IsOutputModule = isOutputModule;
            Inputs = m_inputs;
            Outputs = m_outputs;
            Displays = m_displays;
            Fields = m_fields;
            UsedPower = usedPower;
            UsedComputing = usedComputing;
            Graphics = gfx;
            DisplayFunction = m_displayFunction;
            WidthFunction = m_widthFunction;
            AllowedDevices = m_allowedDevices;
            Categories = m_categories;
            SetAvailability(false);
        }

        public class Builder
        {
            private readonly List<Tag> m_tags = new List<Tag>();
            private ProtoRegistrator m_registrator;
            private readonly ID m_id;
            private string m_name;
            private string m_description;
            private Func<Module, ModuleStatus> m_action;
            private Func<Module, ModuleStatus> m_init;
            private bool m_isOutputModule = false;
            private bool m_isInputModule = false;
            private readonly List<ModuleConnectorProto> m_inputs = new List<ModuleConnectorProto>();
            private readonly List<ModuleConnectorProto> m_outputs = new List<ModuleConnectorProto>();
            private readonly List<ModuleConnectorProto> m_displays = new List<ModuleConnectorProto>();
            private Electricity m_usedPower;
            private PartialQuantity m_usedComputing;
            private EntityCostsTpl.Builder m_costs;
            private Gfx m_gfx;
            private string m_symbol;
            private readonly List<IField> m_fields = new List<IField>();
            private readonly List<StaticEntityProto.ID> m_allowedDevices;
            private bool m_customBuild;
            private bool m_customMaintenance;
            private List<Category> m_categories = new List<Category>();

            public Action<Module, StackContainer> m_displayFunction { get; }
            public Func<Module, int> m_widthFunction { get; private set; }

            public Builder(ProtoRegistrator registrator, string id, string name, string description, string symbol, Gfx gfx)
            {
                m_registrator = registrator;
                m_id = new ID(id.ModuleId());
                m_name = name;
                m_description = description;
                m_tags = new List<Tag>();
                m_usedPower = 1.Kw();
                m_costs = new EntityCostsTpl.Builder();
                m_gfx = gfx;
                m_symbol = symbol;
                m_allowedDevices = new List<StaticEntityProto.ID>();
            }

            public Builder(ProtoRegistrator registrator, ID id)
            {
                m_registrator = registrator;
                m_id = id;
                m_tags = new List<Tag>();
                m_usedPower = 1.Kw();
                m_costs = new EntityCostsTpl.Builder();
                m_allowedDevices = new List<StaticEntityProto.ID>();
            }

            public Builder(ProtoRegistrator registrator, string id)
            {
                m_registrator = registrator;
                m_id = new ID(id.ModuleId());
                m_tags = new List<Tag>();
                m_usedPower = 1.Kw();
                m_costs = new EntityCostsTpl.Builder();
                m_allowedDevices = new List<StaticEntityProto.ID>();
            }

            public ModuleProto Build()
            {
                if (!m_customMaintenance)
                    UseDefaultMaintenance();
                if (!m_customBuild)
                    BuildDefault();

                return new ModuleProto(
                    m_id,
                    CreateStr(m_id, m_name, m_description),
                    m_registrator == null ? new EntityCosts() : ((EntityCostsTpl)m_costs).MapToEntityCosts(m_registrator),
                    m_gfx,
                    m_tags,
                    m_action ?? (m => ModuleStatus.Running),
                    m_init ?? (m => ModuleStatus.Running),
                    m_isInputModule,
                    m_isOutputModule,
                    m_usedPower,
                    m_usedComputing,
                    m_inputs,
                    m_outputs,
                    m_displays,
                    m_fields,
                    m_displayFunction,
                    m_widthFunction,
                    m_symbol,
                    m_allowedDevices,
                    m_categories
                );
            }

            public ModuleProto BuildAndAdd()
            {
                return BuildAndAdd(NewIds.Research.ProgramableNetwork_Stage1);
            }

            public ModuleProto BuildAndAdd(ResearchNodeProto.ID researchStage)
            {
                Research.AddModule(m_id, researchStage);
                return m_registrator.PrototypesDb.Add(Build());
            }

            public Builder SetName(string name)
            {
                this.m_name = name;
                return this;
            }

            public Builder SetDescritpion(string description)
            {
                this.m_description = description;
                return this;
            }

            public Builder SetSymbol(string symbol)
            {
                this.m_symbol = symbol;
                return this;
            }

            public Builder SetGfx(string gfx)
            {
                this.m_gfx = new Gfx(gfx);
                return this;
            }

            public Builder AddTag(Tag tag)
            {
                m_tags.Add(tag);
                return this;
            }

            public Builder AddInput(string id, string name)
            {
                m_inputs.Add(new ModuleConnectorProto(id, m_id.Input(id, name)));
                return this;
            }

            public Builder AddOutput(string id, string name)
            {
                m_outputs.Add(new ModuleConnectorProto(id, m_id.Output(id, name)));
                return this;
            }

            public Builder AddDevice(StaticEntityProto.ID device)
            {
                m_allowedDevices.Add(device);
                return this;
            }

            public Builder AddControllerDevice()
            {
                return AddDevice(NewIds.Controllers.Controller);
            }

            public Builder UsePower(Electricity usedPower)
            {
                m_usedPower = usedPower;
                return this;
            }

            public Builder UseMaintenance(VirtualProductProto.ID maintenance, int count)
            {
                m_customMaintenance = true;
                m_costs.Maintenance(count, maintenance);
                return this;
            }

            public Builder UseDefaultMaintenance()
            {
                return UseMaintenance(Ids.Products.MaintenanceT1, 1);
            }

            public Builder BuildProduct(ProductProto.ID product, int count)
            {
                m_customBuild = true;
                m_costs.Product(count, product);
                return this;
            }

            public Builder BuildElectronicsT1(int count = 1)
            {
                return BuildProduct(Ids.Products.Electronics, count);
            }

            public Builder BuildElectronicsT2(int count = 1)
            {
                return BuildProduct(Ids.Products.Electronics2, count);
            }

            public Builder BuildElectronicsT3(int count = 1)
            {
                return BuildProduct(Ids.Products.Electronics3, count);
            }

            public Builder BuildDefault()
            {
                return BuildElectronicsT1(1);
            }

            /// <summary>
            /// Displays will be shown on the name, the name will be overriden
            /// </summary>
            /// <param name="id">indexing name in Module.Field[id]</param>
            /// <param name="name">Displayerd tooltip value</param>
            /// <param name="width">taken module width</param>
            /// <returns></returns>
            public Builder AddDisplay(string id, string name, int width, bool image = false, string[] toggle = null, bool entity = false)
            {
                m_displays.Add(new ModuleConnectorProto(id, m_id.Display(id, name), width,
                    image ? "[image]" :
                    toggle != null ? "[toggle]" + WriteToggleArray(toggle) :
                    (new string('0', width * 2) + "|")
                    ));
                return this;
            }

            private string WriteToggleArray(string[] toggle)
            {
                char separator = "|`#,".AsEnumerable().Where(c => !toggle.SelectMany(t => t).Any(t => t == c)).First();
                return separator + string.Join("" + separator, toggle);
            }

            public Builder Action(Action<Module> action)
            {
                m_action = (m) =>
                {
                    action(m);
                    return ModuleStatus.Running;
                };
                return this;
            }

            public Builder Action(Func<Module, ModuleStatus> action)
            {
                m_action = action;
                return this;
            }

            public Builder AddCategory(Category category)
            {
                m_categories.Add(category);
                return this;
            }

            public Builder Width(int slots)
            {
                m_widthFunction = (m) => slots;
                return this;
            }

            public Builder Init(Action<Module> init)
            {
                m_init = (m) => { init(m); return ModuleStatus.Running; };
                return this;
            }

            public Builder Init(Func<Module, ModuleStatus> init)
            {
                m_init = init;
                return this;
            }

            public Builder AddBooleanField(string id, string name, string shortDesc = "", bool defaultValue = false)
            {
                m_fields.Add(new BooleanField(id, name, shortDesc, defaultValue));
                return this;
            }

            public Builder AddInt32Field(string id, string name, string shortDesc = "", int defaultValue = 0)
            {
                m_fields.Add(new NumberField<int>(id, name, shortDesc, defaultValue));
                return this;
            }

            public Builder AddInt64Field(string id, string name, string shortDesc = "", long defaultValue = 0)
            {
                m_fields.Add(new NumberField<long>(id, name, shortDesc, defaultValue));
                return this;
            }

            public Builder AddFix32Field(string id, string name, string shortDesc = "", Fix32? defaultValue = null)
            {
                m_fields.Add(new NumberField<Fix32>(id, name, shortDesc, defaultValue ?? Fix32.Zero));
                return this;
            }

            public Builder AddStringField(string id, string name, string shortDesc = "", string defaultValue = "")
            {
                m_fields.Add(new StringField(id, name, shortDesc, defaultValue));
                return this;
            }

            public Builder AddEntityField(string id, string name, Func<Module, IEntity, bool> entitySelector = null, Fix32? distance = null)
            {
                m_fields.Add(new EntityField(id, name, null, entitySelector, distance ?? 5.ToFix32()));
                return this;
            }

            public Builder AddEntityField<T>(string id, string name, Fix32? distance = null)
                where T : IEntity
            {
                m_fields.Add(new EntityField(id, name, null, (module, entity) => entity is T, distance ?? 5.ToFix32()));
                return this;
            }

            public Builder AddEntityField<T>(string id, string name, string shortDesc, Fix32? distance = null)
                where T : IEntity
            {
                m_fields.Add(new EntityField(id, name, shortDesc, (module, entity) => entity is T, distance ?? 5.ToFix32()));
                return this;
            }

            public Builder AddEntityField(string id, string name, string shortDesc, Func<Module, IEntity, bool> filter = null, Fix32? distance = null)
            {
                m_fields.Add(new EntityField(id, name, shortDesc, filter, distance ?? 5.ToFix32()));
                return this;
            }

            public Builder AddEntityField<T>(string id, string name, string shortDesc, Func<Module, IEntity, bool> filter = null, Fix32? distance = null)
                where T : IEntity
            {
                m_fields.Add(new EntityField(id, name, shortDesc, (module, entity) => entity is T && (filter?.Invoke(module, entity) ?? true), distance ?? 5.ToFix32()));
                return this;
            }

            public Builder AddEntityField(Type t, string id, string name, string shortDesc, Func<Module, IEntity, bool> filter = null, Fix32? distance = null)
            {
                m_fields.Add(new EntityField(id, name, shortDesc, (module, entity) => entity?.GetType()?.IsAssignableTo(t) ?? false && (filter?.Invoke(module, entity) ?? true), distance ?? 5.ToFix32()));
                return this;
            }

            public Builder AddEntityTypeField<T>(string id, string name, string shortDesc = null, Func<Module, T, bool> filter = null)
                where T : EntityProto, IProtoWithIcon
            {
                m_fields.Add(new EntityTypeField<T>(id, name, shortDesc ?? "", filter ?? ((m, proto) => true)));
                return this;
            }

            public Builder AddProductField(string id, string name, string shortDesc = null, Func<Module, ProductProto, bool> filter = null)
            {
                m_fields.Add(new ProductField(id, name, shortDesc ?? "", filter ?? ((m, proto) => true)));
                return this;
            }

            public Builder AddCustomField(string id, string name, Func<int> size, Action<CustomField> ui, Action<CustomField> data = null)
            {
                m_fields.Add(new CustomField(id, name, null, size, ui, data ?? ((field) => { })));
                return this;
            }

            public Builder AddCustomField(string id, string name, string shortDesc, Func<int> size, Action<CustomField> ui, Action<CustomField> data = null)
            {
                m_fields.Add(new CustomField(id, name, shortDesc, size, ui, data ?? ((field) => { })));
                return this;
            }

            public Builder UseComputation(PartialQuantity quantity)
            {
                m_usedComputing = quantity;
                return this;
            }
        }

        public void ExecuteInit(Module m)
        {
            foreach (var field in Fields)
            {
                field.InitData(m);
            }
            m.SetStatus(Init.Invoke(m));
            Log.Info($"Module initialized: {m.Id} ({m.Prototype.Id.Value}) with status {m.Status}");
        }
    }

    public static class ModuleProtoExtensions
    {
        public static ModuleProto.Builder ModuleBuilderStart(this ProtoRegistrator registrator, string id, string name, string symbol, string gfx, string description = "")
        {
            return new ModuleProto.Builder(registrator, id, name, description, symbol, new ModuleProto.Gfx(gfx));
        }

        public static ModuleProto.Builder ModuleBuilderStart(this ProtoRegistrator registrator, string id)
        {
            return new ModuleProto.Builder(registrator, id);
        }

        public static Proto.Str Input(this ModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__input__" + name), text, description);
        }

        public static Proto.Str Output(this ModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__output__" + name), text, description);
        }

        public static Proto.Str Display(this ModuleProto.ID operation, string name, string text, string description = "")
        {
            return Proto.CreateStr(new Proto.ID(operation.Value + "__display__" + name), text, description);
        }
    }
}
