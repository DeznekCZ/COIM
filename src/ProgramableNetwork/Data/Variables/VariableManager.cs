using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Serialization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using System;

namespace ProgramableNetwork.Data.Variables
{
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    [ManuallyWrittenSerialization]
    public class VariableManager
    {
        // Version 1 added m_writers (per-variable writer controller EntityId) so the
        // UI can offer goto / open-inspector buttons without scanning every module
        // on every click.  Saved variables from v0 deserialize with an empty
        // writers dict — buttons gracefully no-op until the producer ticks again.
        private const int VERSION = 1;

        [DoNotSave()]
        private Dict<string, Fix32> m_variables;
        [DoNotSave()]
        private Dict<string, EntityId> m_writers;
        [DoNotSave()]
        private IUnityInputMgr m_inputManager;

        public VariableManager(IUnityInputMgr inputManager)
        {
            m_variables = new Dict<string, Fix32>();
            m_writers = new Dict<string, EntityId>();
        }

        private void SerializeData(BlobWriter writer)
        {
            writer.WriteInt(/* Version */VERSION);
            Dict<string, Fix32>.Serialize(m_variables, writer);
            Dict<string, EntityId>.Serialize(m_writers, writer);
        }

        private void DeserializeData(BlobReader reader)
        {
            int version = reader.ReadInt();
            m_variables = Dict<string, Fix32>.Deserialize(reader);
            // v1+ persists the writer-controller map alongside the values.
            // Older saves restart with an empty writers dict; entries get filled
            // back in as each Variable_Write module ticks once after load.
            m_writers = version >= 1
                ? Dict<string, EntityId>.Deserialize(reader)
                : new Dict<string, EntityId>();
        }

        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((VariableManager)obj).SerializeData(writer);
        };

        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((VariableManager)obj).DeserializeData(reader);
        };

        public static void Serialize(VariableManager value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static VariableManager Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out VariableManager value, null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        /// <summary>
        /// Stores <paramref name="value"/> under <paramref name="name"/> and records
        /// <paramref name="writer"/> as its producer for the inspector goto/open
        /// shortcuts. Setting Zero clears both the value and the writer link so a
        /// dropped variable doesn't leave a stale writer pointer behind.
        /// </summary>
        public void SetVariable(string name, Fix32 value, EntityId writer)
        {
            if (value == Fix32.Zero)
            {
                m_variables.TryRemove(name, out _);
                m_writers.TryRemove(name, out _);
            }
            else
            {
                m_variables[name] = value;
                m_writers[name] = writer;
            }
        }

        public Fix32 GetVariable(string name)
        {
            m_variables.TryGetValue(name, out Fix32 value);
            return value;
        }

        /// <summary>
        /// Returns the controller EntityId that last wrote <paramref name="name"/>,
        /// or <c>default</c> (an invalid EntityId, <c>.IsValid == false</c>) when
        /// the variable has never been written or its writer cleared its value.
        /// </summary>
        public EntityId GetVariableWriter(string name)
        {
            return m_writers.TryGetValue(name, out EntityId id) ? id : default;
        }

        /// <summary>
        /// Explicit removal of a variable (value + writer link) without going
        /// through <see cref="SetVariable"/>'s zero-as-clear convention. Targeted
        /// at the Variables window's stale-cleanup button via <see cref="VariableRemoveCmd"/>;
        /// no-op when the name isn't present.
        /// </summary>
        public void RemoveVariable(string name)
        {
            m_variables.TryRemove(name, out _);
            m_writers.TryRemove(name, out _);
        }

        public Dict<string, Fix32> AllVariables => m_variables;
    }
}
