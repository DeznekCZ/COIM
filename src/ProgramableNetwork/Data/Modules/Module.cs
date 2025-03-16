using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgramableNetwork
{
    [GenerateSerializer(false, null, 0)]
    public class Module : IEntity, IEntityWithCloneableConfig
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((Module)obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((Module)obj).DeserializeData(reader);
        };
        public static void Serialize(Module value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static Module Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out Module obj))
            {
                reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
            }

            return obj;
        }

        private ModuleProto m_proto;
        private string m_protoId;
        private int loadedVersion;
        private ModuleLayout m_layout;

        private static readonly int SerializerVersion = 1;

        public Module(ModuleProto prototype, EntityContext context, Controller entity)
        {
            this.Id = DateTime.UtcNow.Ticks;
            Prototype = prototype;
            Context = context;
            Controller = entity;
            NumberData = new Dict<string, int>();
            StringData = new Dict<string, string>();
            InputModules = new Dict<string, ModuleConnector>();
        }

        public Controller Controller { get; set; }

        public ModuleStatus Status { get; private set; }
        public string Error { get; private set; } = "";
        public long Id { get; private set; }

        public ModuleProto Prototype {
            get => m_proto;
            set
            {
                m_protoId = value.Id.Value;
                m_proto = value;
            }
        }

        public EntityContext Context { get; set; }
        public ModuleLayout Layout => new ModuleLayout(Prototype);

        public void Execute()
        {
            try
            {
                Status = Prototype.Action(this);
                if (Status != ModuleStatus.Error)
                {
                    Error = "";
                }
            }
            catch (System.Exception e)
            {
                Error = e.Message;
                Status = ModuleStatus.Error;
                if (IsDebugging)
                    Debug.LogError(e);
            }
        }

        /// <summary>
        /// Resets all volatile data
        /// </summary>
        public void Reset()
        {
            try
            {
                Error = "";
                Prototype.Reset(this);
                Status = ModuleStatus.Running;
            }
            catch (System.Exception e)
            {
                Error = e.Message;
                Status = ModuleStatus.Error;
            }
        }

        [DoNotSave(0, null)]
        public Dict<string, ModuleConnector> InputModules { get; private set; } // todo get by module id, cached
        protected void SerializeData(BlobWriter writer)
        {
            if (m_protoId == null)
                throw new NullReferenceException($"Prototype was not set valid");

            writer.WriteLong(Id);
            writer.WriteString(m_protoId);
            writer.WriteInt(SerializerVersion);
            writer.WriteBool(IsPaused);
            Dict<string, int>.Serialize(NumberData, writer);
            Dict<string, string>.Serialize(StringData, writer);
            Dict<string, ModuleConnector>.Serialize(InputModules, writer);
        }

        protected void DeserializeData(BlobReader reader)
        {
            Id = reader.ReadLong();
            m_protoId = reader.ReadString();
            loadedVersion = reader.ReadInt();
            IsPaused = reader.ReadBool();
            NumberData = Dict<string, int>.Deserialize(reader);
            StringData = Dict<string, string>.Deserialize(reader);
            InputModules = Dict<string, ModuleConnector>.Deserialize(reader);

            Log.Info($"Instance: {GetHashCode()}({Id}), version: {loadedVersion}");
        }

        [InitAfterLoad(InitPriority.High)]
        [OnlyForSaveCompatibility(null)]
        internal void initContexts(int saveVersion)
        {
            var Prototype = Context.ProtosDb.Get<ModuleProto>(new ModuleProto.ID(m_protoId));
            Log.Info($"Instance: {GetHashCode()}({Id}), version: {loadedVersion}");

            if (Prototype.HasValue)
            {
                this.Prototype = Prototype.Value;
            }
            else
            {
                var alternative = Deprecation.GetAlternative(new ModuleProto.ID(m_protoId));
                this.Prototype = Context.ProtosDb.Get<ModuleProto>(alternative).ValueOrThrow("Invalid module proto: " + m_protoId);
            }

            if (loadedVersion == 0)
            {
                loadedVersion = SerializerVersion;
                foreach (IField item in this.Prototype.Fields)
                {
                    if (!(item is EntityField) && NumberData.TryGetValue("field__" + item.Id, out var value))
                    {
                        NumberData["field__" + item.Id] = value.ToFix32().RawValue;
                    }
                }
                foreach (ModuleConnectorProto item in this.Prototype.Inputs)
                {
                    if (NumberData.TryGetValue("in__" + item.Id, out var value))
                        Input[item.Id] = value.ToFix32().RawValue;
                }
                foreach (ModuleConnectorProto item in this.Prototype.Outputs)
                {
                    if (NumberData.TryGetValue("out__" + item.Id, out var value))
                        Output[item.Id] = value.ToFix32().RawValue;
                }
            }
        }

        public void AddToConfig(EntityConfigData data)
        {
            // clone proto id, position and cofigured fields
            // the position on grid is hold by computer
        }

        public void ApplyConfig(EntityConfigData data)
        {
            // applies prototype and accessible fields
        }

        public void UpdateIsEnabled()
        {
            //throw new NotImplementedException();
        }

        public void UpdateIsBroken()
        {
            //throw new NotImplementedException();
        }

        public void UpdateProperties()
        {
            //throw new NotImplementedException();
        }

        public void SetPaused(bool isPaused)
        {
            //throw new NotImplementedException();
        }

        public void AddObserver(IEntityObserver observer)
        {
            //throw new NotImplementedException();
        }

        public void RemoveObserver(IEntityObserver observer)
        {
            //throw new NotImplementedException();
        }

        public void SetStatus(ModuleStatus status)
        {
            Status = status;
        }

        public void SetError(string message)
        {
            Error = message;
        }

        [DoNotSave(0, null)]
        public Dict<string, int> NumberData { get; private set; }
        [DoNotSave(0, null)]
        public Dict<string, string> StringData { get; private set; }


        [DoNotSave(0, null)]
        public OutputData Output => new OutputData(this);

        public class OutputData
        {
            private Module module;

            internal OutputData(Module module)
            {
                this.module = module;
            }

            public Fix32 this[string name, int defaultValue = 0]
            {
                get => module.NumberData.TryGetValue("out__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue.ToFix32();
                set => module.NumberData["out__" + name] = value.RawValue;
            }

            public Fix32 this[string name, Fix32 defaultValue]
            {
                get => module.NumberData.TryGetValue("out__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue;
            }
        }


        [DoNotSave(0, null)]
        public InputData Input => new InputData(this);

        public class InputData
        {
            private Module module;

            internal InputData(Module module)
            {
                this.module = module;
            }

            public Fix32 this[string name, int defaultValue = 0]
            {
                get => module.NumberData.TryGetValue("in__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue.ToFix32();
                set => module.NumberData["in__" + name] = value.RawValue;
            }

            public Fix32 this[string name, Fix32 defaultValue]
            {
                get => module.NumberData.TryGetValue("in__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue;
            }
            public ProductProto Product(string name)
            {
                module.NumberData.TryGetValue("in__" + name, out int slimId);

                if (slimId == 0)
                {
                    return null;
                }

                module.StringData.TryGetValue("in__" + name, out string cache);
                if (!string.IsNullOrEmpty(cache))
                { // try get entity by name and check slimId
                    Option<ProductProto> product = module.Context.ProtosDb.Get<ProductProto>(new Mafi.Core.Prototypes.Proto.ID(cache));
                    if (product.HasValue && product.Value.SlimId.Value == slimId)
                    {
                        return product.Value;
                    }
                }
                // else
                { // try get entity by slimId
                    Option<ProductProto> product = module.Context.ProtosDb.First<ProductProto>(p => p.SlimId.Value == slimId);
                    if (product.HasValue && product.Value.SlimId.Value == slimId)
                    {
                        module.StringData["in__" + name] = product.Value.Id.Value;
                        return product.Value;
                    }
                }

                module.NumberData.TryRemove("in__" + name, out slimId);
                module.StringData.TryRemove("in__" + name, out cache);
                return null;
            }

            public T Entity<T>(string name)
                where T : class, IEntity
            {
                if (module.NumberData.TryGetValue("in__" + name, out int data))
                {
                    module.Context.EntitiesManager.TryGetEntity(new EntityId(data), out T entity);
                    return entity;
                }
                return default;
            }

            public IProtoWithIcon EntityProtoIconified(string name)
            {
                module.NumberData.TryGetValue("in__" + name, out int slimId);

                if (slimId == 0)
                {
                    return default;
                }

                module.StringData.TryGetValue("in__" + name, out string cache);
                if (!string.IsNullOrEmpty(cache))
                { // try get entity by name and check slimId
                    Option<Proto> product = module.Context.ProtosDb.Get<Proto>(new Mafi.Core.Prototypes.Proto.ID(cache));
                    if (product.HasValue && FixSavedGames.GetPrototypeString(product.Value.Id.Value).RawValue == slimId)
                    {
                        return product.Value as IProtoWithIcon;
                    }
                }
                // else
                { // try get entity by slimId
                    Option<Proto> product = module.Context.ProtosDb.First<Proto>(p => FixSavedGames.GetPrototypeString(p.Id.Value).RawValue == slimId);
                    if (product.HasValue)
                    {
                        module.StringData["in__" + name] = product.Value.Id.Value;
                        return product.Value as IProtoWithIcon;
                    }
                }

                module.NumberData.TryRemove("in__" + name, out slimId);
                return default;
            }
        }

        [DoNotSave(0, null)]
        public FieldOrInputData FieldOrInput => new FieldOrInputData(this);

        public class FieldOrInputData
        {
            private Module module;

            internal FieldOrInputData(Module module)
            {
                this.module = module;
            }

            public Fix32 this[string name, int defaultValue]
            {
                get => module.Field["field_" + name, Fix32.Zero] > Fix32.Zero
                    ? module.Field[name, defaultValue]
                    : module.Input[name, defaultValue];
            }

            public Fix32 this[string name, Fix32 defaultValue]
            {
                get => module.Field["field_" + name, Fix32.Zero] > Fix32.Zero
                    ? module.Field[name, defaultValue]
                    : module.Input[name, defaultValue];
            }

            public T Entity<T>(string name)
                where T : class, IEntity
            {
                return module.Field["field_" + name, Fix32.Zero] > Fix32.Zero
                    ? module.Field.Entity<T>(name)
                    : module.Input.Entity<T>(name);
            }

            public ProductProto Product(string name)
            {
                return module.Field["field_" + name, Fix32.Zero] > Fix32.Zero
                    ? module.Field.Product(name)
                    : module.Input.Product(name);
            }

            public IProtoWithIcon EntityProtoIconified(string name)
            {
                return module.Field["field_" + name, Fix32.Zero] > Fix32.Zero
                    ? module.Field.EntityProtoIconified(name)
                    : module.Input.EntityProtoIconified(name);
            }
        }

        [DoNotSave(0, null)]
        public FieldData Field => new FieldData(this);

        public class FieldData
        {
            private Module module;

            internal FieldData(Module module)
            {
                this.module = module;
            }

            public string this[string name, string defaultValue]
            {
                get => module.StringData.TryGetValue("field__" + name, out string data)
                    ? data : defaultValue;
                set => module.StringData["field__" + name] = value ?? defaultValue;
            }

            public Fix32 this[string name, int defaultValue]
            {
                get => module.NumberData.TryGetValue("field__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue.ToFix32();
            }

            public Fix32 this[string name, Fix32 defaultValue]
            {
                get => module.NumberData.TryGetValue("field__" + name, out int data)
                    ? Fix32.FromRaw(data) : defaultValue;
            }

            public T Entity<T>(string name)
                where T : class, IEntity
            {
                if (module.NumberData.TryGetValue("field__" + name, out int data))
                {
                    module.Context.EntitiesManager.TryGetEntity(new EntityId(data), out T entity);
                    if (!module.StringData.ContainsKey("field__" + name))
                    {
                        Entity(name, entity);
                    }
                    return entity;
                }
                return default;
            }

            public void Entity<T>(string name, T entity)
                where T : class, IEntity
            {
                if (entity is null)
                {
                    module.NumberData.TryRemove("field__" + name, out _);
                    module.StringData.TryRemove("field__" + name, out _);
                    return;
                }

                entity.HasPosition(out Tile2f posA);
                var relativePosition = module.Controller.Position2f - posA;

                module.NumberData["field__" + name] = entity.Id.Value;
                module.StringData["field__" + name] = JsonConvert.SerializeObject(new EntityInfo(entity, relativePosition));
            }

            public ProductProto Product(string name)
            {
                module.NumberData.TryGetValue("field__" + name, out int slimId);

                if (slimId == 0)
                {
                    return null;
                }

                module.StringData.TryGetValue("field__" + name, out string cache);
                if (!string.IsNullOrEmpty(cache))
                { // try get entity by name and check slimId
                    Option<ProductProto> product = module.Context.ProtosDb.Get<ProductProto>(new Mafi.Core.Prototypes.Proto.ID(cache));
                    if (product.HasValue && product.Value.SlimId.Value == slimId)
                    {
                        return product.Value;
                    }
                }
                // else
                { // try get entity by slimId
                    Option<ProductProto> product = module.Context.ProtosDb.First<ProductProto>(p => p.SlimId.Value == slimId);
                    if (product.HasValue && product.Value.SlimId.Value == slimId)
                    {
                        module.StringData["field__" + name] = product.Value.Id.Value;
                        return product.Value;
                    }
                }

                module.NumberData.TryRemove("field__" + name, out slimId);
                module.StringData.TryRemove("field__" + name, out cache);
                return null;
            }

            public Fix32 this[string name]
            {
                set
                {
                    module.NumberData["field__" + name] = value.RawValue;
                }
            }

            public IProtoWithIcon EntityProtoIconified(string name)
            {
                module.NumberData.TryGetValue("field__" + name, out int slimId);

                if (slimId == 0)
                {
                    return default;
                }

                module.StringData.TryGetValue("field__" + name, out string cache);
                if (!string.IsNullOrEmpty(cache))
                { // try get entity by name and check slimId
                    Option<Proto> product = module.Context.ProtosDb.Get<Proto>(new Mafi.Core.Prototypes.Proto.ID(cache));
                    if (product.HasValue && FixSavedGames.GetPrototypeString(product.Value.Id.Value).RawValue == slimId)
                    {
                        return product.Value as IProtoWithIcon;
                    }
                }
                // else
                { // try get entity by slimId
                    Option<Proto> product = module.Context.ProtosDb.First<Proto>(p => FixSavedGames.GetPrototypeString(p.Id.Value).RawValue == slimId);
                    if (product.HasValue)
                    {
                        module.StringData["field__" + name] = product.Value.Id.Value;
                        return product.Value as IProtoWithIcon;
                    }
                }

                module.NumberData.TryRemove("field__" + name, out slimId);
                return default;
            }
        }

        [DoNotSave(0, null)]
        public DisplayData Display => new DisplayData(this);

        public class DisplayData
        {
            private Module module;

            internal DisplayData(Module module)
            {
                this.module = module;
            }

            public string this[string name, string defaultValue]
            {
                get => module.StringData.TryGetValue("display__" + name, out string data)
                    ? data ?? defaultValue : defaultValue;
            }

            public string this[string name]
            {
                set
                {
                    module.StringData["display__" + name] = value;
                }
            }
        }


        [DoNotSave(0, null)]
        public StatusData StatusIn => new StatusData(this, "in__");


        [DoNotSave(0, null)]
        public StatusData StatusOut => new StatusData(this, "out__");

        public LocStrFormatted DefaultTitle => throw new NotImplementedException();

        EntityId IEntity.Id => new EntityId((int)(Id&0xFFFFFFFF));

        EntityProto IEntity.Prototype => Prototype;

        public bool IsEnabled => true;

        public bool IsPaused { get; set; }

        public bool CanBePaused => false;

        public bool IsDestroyed => false;

        public bool IsDebugging { get; set; } = false;

        public bool Info {
            get => NumberData.TryGetValue("__info", out int value) && value > 0;
            set => NumberData["__info"] = value ? 1 : 0;
        }

        public bool Warning {
            get => NumberData.TryGetValue("__warning", out int value) && value > 0;
            set => NumberData["__warning"] = value ? 1 : 0;
        }

        public class StatusData
        {
            private readonly Module module;
            private readonly string direction;

            internal StatusData(Module module, string direction)
            {
                this.module = module;
                this.direction = direction;
            }

            public ModuleStatus this[string name]
            {
                get => module.NumberData.TryGetValue("status__" + direction + name, out int data)
                    ? (ModuleStatus)data : ModuleStatus.Init;
                set => module.NumberData["status__" + direction + name] = (int)value;
            }
        }
    }

    public class EntityInfo
    {
        public int Id { get; set; }
        public string Prototype { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        [JsonIgnore]
        public RelTile2f Relative => new RelTile2f(Fix32.FromRaw(X), Fix32.FromRaw(Y));

        public EntityInfo() { } // serializer constructor

        public EntityInfo(IEntity entity, RelTile2f position)
        {
            Id = entity.Id.Value;
            Prototype = entity.Prototype.Id.Value;
            X = position.X.RawValue;
            Y = position.Y.RawValue;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityInfo other &&
                   Id == other.Id &&
                   Prototype == other.Prototype &&
                   X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = -1866251748;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Prototype);
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }
}