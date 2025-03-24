using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using UnityEngine;

namespace ProgramableNetwork
{
    [GenerateSerializer(false, null, 0)]
    public partial class Module : IEntity, IEntityWithCloneableConfig
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

        public Module(ModuleProto prototype, EntityContext context, Controller entity)
        {
            this.Id = DateTime.UtcNow.Ticks;
            Prototype = prototype;
            Context = context;
            Controller = entity;
            Status = ModuleStatus.Init;
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
                if (Status == ModuleStatus.Init)
                {
                    Warning = true;
                    return;
                }

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
                //if (IsDebugging)
                    Debug.LogError(e);
            }
        }

        /// <summary>
        /// Resets all volatile data
        /// </summary>
        public void Reset()
        {
            Error = "";
            Warning = false;
            Info = false;
            Status = ModuleStatus.Init;
        }

        [DoNotSave(0, null)]
        public Dict<string, ModuleConnector> InputModules { get; private set; } // todo get by module id, cached
        protected void SerializeData(BlobWriter writer)
        {
            if (m_protoId == null)
                throw new NullReferenceException($"Prototype was not set valid");

            writer.WriteLong(Id);
            writer.WriteString(m_protoId);
            writer.WriteInt(/*Version*/ 2);
            writer.WriteBool(IsPaused);
            writer.WriteInt((int)Status);
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
            if (loadedVersion >= 2)
                Status = (ModuleStatus)reader.ReadInt();
            else
                Status = ModuleStatus.Running;
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
                if (alternative != null)
                    this.Prototype = Context.ProtosDb.Get<ModuleProto>(alternative ?? new ModuleProto.ID()).ValueOrThrow("Invalid module proto: " + m_protoId);
                else
                    this.Prototype = ModuleProto.Phantom;
            }

            if (loadedVersion == 0)
            {
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
                        Output.Integer[item.Id] = value.ToFix32().RawValue;
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

        [DoNotSave(0, null)]
        public InputData Input => new InputData(this);

        [DoNotSave(0, null)]
        public FieldOrInputData FieldOrInput => new FieldOrInputData(this);

        [DoNotSave(0, null)]
        public FieldData Field => new FieldData(this);

        [DoNotSave(0, null)]
        public DisplayData Display => new DisplayData(this);

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
    }
}