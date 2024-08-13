import { ContractParameters } from "./ContractParameters";

interface ContractList {
    Claimable: Array<number>;
    Owned: Array<number>;
    Available: Array<number>;
    Entries: Array<{ Id: number; Params: ContractParameters }>;
}

export { ContractList }