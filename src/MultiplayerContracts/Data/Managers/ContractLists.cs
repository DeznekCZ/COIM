using Mafi;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Newtonsoft.Json;
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

        public class PQDes
        {
            public string Product;
            public int Quantity;
        }

        public class CPDes
        {
            public PQDes Supply;
            public PQDes Demand;
        }

        public class ICPDes
        {
            public long Id;
            public CPDes Params;
        }

        public class CLDes
        {
            public List<long> Owned;
            public List<long> Claimable;
            public List<long> Available;
            public List<ICPDes> Entries;
        }

        public static ContractLists ParseJSON(string jsonString, ProtosDb protosDb)
        {
            CLDes d = JsonConvert.DeserializeObject<CLDes>(jsonString);
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