using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Ports;
using Mafi.Core.Ports.Io;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mafi.Unity.UiFramework;
using Mafi.Serialization;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Factory.Transports;

namespace ControlledTransport
{
    [GenerateSerializer(false, null, 0)]
    public class TransportControl : LayoutEntity, IEntityWithPorts, IAreaSelectableEntity, IEntityWithCloneableConfig
    {
        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate(object obj, BlobWriter writer)
	    {
		    ((TransportControl) obj).SerializeData(writer);
        };
        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
	    {
		    ((TransportControl) obj).DeserializeData(reader);
        };

        public TransportControl(EntityId id, TransportControlProto proto, TileTransform transform, EntityContext context)
            : base(id, proto, transform, context)
        {

        }

        public new TransportControlProto Prototype => (TransportControlProto)base.Prototype;

        public override bool CanBePaused => true;

        public int MinimumTroughput { get; set; } = 0;
        public int MaximumTroughput { get; set; } = 20;

        public IEntityWithPorts InputStorage { get; set; }
        public bool TransportActive { get; private set; }
        public string Error { get; private set; }

        public void AddToConfig(EntityConfigData data)
        {
            data.SetInt("minimum", MinimumTroughput);
            data.SetInt("maximum", MaximumTroughput);
        }

        public void ApplyConfig(EntityConfigData data)
        {
            MinimumTroughput = data.GetInt("minimum") ?? 0;
            MaximumTroughput = data.GetInt("maximum") ?? 20;
        }

        public Quantity ReceiveAsMuchAsFromPort(ProductQuantity pq, IoPortToken sourcePort)
        {
            if (IsPaused || !CanSend(sourcePort))
            {
                return pq.Quantity;
            }

            ProductQuantity partialQuantity = pq;
            foreach(var output in ConnectedOutputPorts)
            {
                Quantity nonTaken = output.SendAsMuchAs(partialQuantity);
                partialQuantity = partialQuantity.WithNewQuantity(nonTaken);
            }

            return partialQuantity.Quantity;
        }

        private bool CanSend(IoPortToken sourcePort)
        {
            IoPort port = Ports.Where(p => p.Name == sourcePort.Name)
                .ToLyst()[0];

            Error = "";

            if (port.ConnectedPort.ValueOrNull.OwnerEntity is StaticEntity staticEntity)
            {
                if (staticEntity is Storage storage)
                {
                    InputStorage = storage;
                    Quantity quantity = storage.CurrentQuantity;

                    return ValidateQuantity(quantity);
                }
                else if (staticEntity is Transport pipe)
                {
                    InputStorage = pipe;
                    Quantity quantity = Quantity.Zero;

                    foreach (var product in pipe.TransportedProducts)
                    {
                        quantity += product.Quantity;
                    }

                    return ValidateQuantity(quantity);
                }
            }

            Error = "Pipe is not connected to valid output!";
            return false;
        }

        private bool ValidateQuantity(Quantity quantity)
        {
            if (quantity >= MaximumTroughput.Quantity())
            {
                return TransportActive = true;
            }
            else if (quantity <= MinimumTroughput.Quantity())
            {
                return TransportActive = false;
            }
            else
            {
                return TransportActive;
            }
        }

        public static void Serialize(TransportControl value, BlobWriter writer)
        {
            if (writer.TryStartClassSerialization(value))
            {
                writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
            }
        }

        public static TransportControl Deserialize(BlobReader reader)
        {
            if (reader.TryStartClassDeserialization(out TransportControl value, (Func<BlobReader, Type, TransportControl>)null))
            {
                reader.EnqueueDataDeserialization(value, s_deserializeDataDelayedAction);
            }
            return value;
        }

        private readonly int SerializerVersion = 0;
        protected override void SerializeData(BlobWriter writer)
        {
            base.SerializeData(writer);
            writer.WriteInt(SerializerVersion);
            writer.WriteInt(MinimumTroughput);
            writer.WriteInt(MaximumTroughput);
            writer.WriteBool(TransportActive);
        }

        protected override void DeserializeData(BlobReader reader)
        {
            base.DeserializeData(reader);
            int version = reader.ReadInt();
            if (version != SerializerVersion)
            {
                // todo handle update
            }
            else
            {
                MinimumTroughput = reader.ReadInt();
                MaximumTroughput = reader.ReadInt();
                TransportActive = reader.ReadBool();
            }
        }
    }
}
