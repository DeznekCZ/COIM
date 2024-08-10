using Mafi;
using Mafi.Core;
using Mafi.Core.World.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerContracts
{
    public class MultiplayerContractProto : ContractProto
    {
        public MultiplayerContractProto(ID id, ProductQuantity productToBuy, ProductQuantity productToPayWith, Upoints upointsPerMonth, Upoints upointsPer100ProductsBought, int minReputationRequired)
            : base(id, productToBuy, productToPayWith, upointsPerMonth, upointsPer100ProductsBought, minReputationRequired)
        {
        }
    }
}
