using Mafi;
using Mafi.Collections;
using Mafi.Serialization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProgramableNetwork.Data.Variables
{
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    [GenerateSerializer(false, null, 0)]
    public class VariableManager
    {
        private readonly KeyBindings WindowKey = KeyBindings.FromKey(KbCategory.General, ShortcutMode.Game, KeyCode.V);

        [DoNotSave()]
        private Dict<string, Fix32> m_variables;
        [DoNotSave()]
        private IUnityInputMgr m_inputManager;

        public VariableManager(IUnityInputMgr inputManager)
        {
            m_variables = new Dict<string, Fix32>();
        }

        private void SerializeData(BlobWriter writer)
        {
            writer.WriteInt(/* Version */0);
            Dict<string, Fix32>.Serialize(m_variables, writer);
        }

        private void DeserializeData(BlobReader reader)
        {
            int version = reader.ReadInt();
            m_variables = Dict<string, Fix32>.Deserialize(reader);
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

        public void SetVariable(string name, Fix32 fix32)
        {
            if (fix32 == Fix32.Zero)
                m_variables.TryRemove(name, out var _);
            else
                m_variables[name] = fix32;
        }

        public Fix32 GetVariable(string name)
        {
            m_variables.TryGetValue(name, out Fix32 value);
            return value;
        }

        public Dict<string, Fix32> AllVariables => m_variables;
    }
}
