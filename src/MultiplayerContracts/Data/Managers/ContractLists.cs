using System;
using System.Collections.Generic;

namespace MultiplayerContracts
{
    public class ContractLists
    {
        public readonly DateTime UpdatedAt = DateTime.Now;
        public readonly Dictionary<ContractId, ContractParameters> Owned = new Dictionary<ContractId, ContractParameters>();
        public readonly List<ContractId> Claimable = new List<ContractId>();
        public readonly Dictionary<ContractId, ContractParameters> Available = new Dictionary<ContractId, ContractParameters>();
    }
}