namespace ProgramableNetwork.Python
{
    public enum EntityType
    {
        Unknown = 0,
        Entity,
        StaticEntity,
        StorageBase,
        Controller,
        Antena,
        SettlementHousingModule,
        SettlementFoodModule,
        SettlementTransformer,
        SettlementWasteModule,
        SettlementServiceModule,
        Machine,
        IEntityWithWorkers,
        IElectricityConsumingEntity,
        IUnityConsumingEntity
    }
}