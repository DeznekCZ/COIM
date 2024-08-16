using Mafi;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MultiplayerContracts
{
    public class ContractLists
    {
        public readonly DateTime UpdatedAt = DateTime.Now;
        public readonly List<long> Owned;
        public readonly List<long> Claimable;
        public readonly List<long> Available;
        public readonly Dictionary<long, ContractParameters> Entries;

        public ContractLists() : this(new List<long>(), new List<long>(), new List<long>(), new Dictionary<long, ContractParameters>()) {

        }

        public ContractLists(List<long> owned, List<long> claimable, List<long> available, Dictionary<long, ContractParameters> entries)
        {
            this.Owned = owned;
            this.Claimable = claimable;
            this.Available = available;
            this.Entries = entries;
        }

        [DataContract()]
        public class PQDes
        {
            [DataMember] public string Product;
            [DataMember] public int Quantity;
        }

        [DataContract()]
        public class CPDes
        {
            [DataMember] public PQDes Supply;
            [DataMember] public PQDes Demand;
        }

        [DataContract()]
        public class ICPDes
        {
            [DataMember] public long Id;
            [DataMember] public CPDes Params;
        }

        [DataContract()]
        public class CLDes
        {
            [DataMember] public List<long> Owned;
            [DataMember] public List<long> Claimable;
            [DataMember] public List<long> Available;
            [DataMember] public List<ICPDes> Entries;
        }

        public static ContractLists ParseJSON(string jsonString, ProtosDb protosDb)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);
            writer.Write(jsonString);
            writer.Flush();
            memoryStream.Position = 0;

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CLDes));

            CLDes d = (CLDes)serializer.ReadObject(memoryStream);
            ContractLists cl = new ContractLists();
            cl.Owned.AddRange(d.Owned);
            cl.Claimable.AddRange(d.Claimable);
            cl.Available.AddRange(d.Available);
            foreach (var entry in d.Entries)
            {
                ProductProto supplyProduct = protosDb.Get<ProductProto>(
                            new Proto.ID(entry.Params.Supply.Product))
                        .ValueOr(ProductProto.Phantom);

                ProductProto demandProduct = protosDb.Get<ProductProto>(
                            new Proto.ID(entry.Params.Demand.Product))
                        .ValueOr(ProductProto.Phantom);

                cl.Entries.Add(entry.Id, new ContractParameters(
                    new Mafi.Core.ProductQuantity(
                        supplyProduct,
                        entry.Params.Supply.Quantity.Quantity()
                    ),
                    new Mafi.Core.ProductQuantity(
                        demandProduct,
                        entry.Params.Demand.Quantity.Quantity()
                    )
                ));
            }

            return cl;
        }
    }
}