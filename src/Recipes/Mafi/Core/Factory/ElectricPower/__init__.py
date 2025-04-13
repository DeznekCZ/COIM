
class ElectricityConfig:
    def __init__(self):
        pass


class IElectricityConfig:
    def __init__(self):
        pass


class IElectricityConsumerReadonly:
    def __init__(self):
        self.IsEnabled = False
        self.Priority = 0
        self.NotEnoughPower = False
        self.DidConsumeLastTick = False
        self.Entity = None
        self.IsSurplusConsumer = False
        self.PowerCharged = None
        self.PowerRequired = None

class IElectricityConsumer:
    def __init__(self):
        self.IsEnabled = False
        self.Priority = 0
        self.NotEnoughPower = False
        self.DidConsumeLastTick = False
        self.Entity = None
        self.IsSurplusConsumer = False
        self.PowerCharged = None
        self.PowerRequired = None

class IElectricityConsumerInternal:
    def __init__(self):
        self.ProtoToken = 0
        self.IsEnabled = False
        self.Priority = 0
        self.NotEnoughPower = False
        self.DidConsumeLastTick = False
        self.Entity = None
        self.IsSurplusConsumer = False
        self.PowerCharged = None
        self.PowerRequired = None

class ElectricityConsumer:
    def __init__(self):
        self.ProtoToken = 0
        self.Priority = 0
        self.IsEnabled = False
        self.IsSurplusConsumer = False
        self.PowerCharged = None
        self.PowerRequired = None
        self.DidConsumeLastTick = False
        self.NotEnoughPower = False
        self.Entity = None

class ElectricityConsumerExtensions:
    def __init__(self):
        pass


class IElectricityConsumerFactory:
    def __init__(self):
        pass


class ElectricityConsumerFactory:
    def __init__(self):
        pass


class ElectricityConsumerFactoryExtensions:
    def __init__(self):
        pass


class ElectricityManager:
    MAX_PRIORITY_STEPS = 0
    def __init__(self):
        self.ElectricityProto = None
        self.MaxGenerationCapacity = None
        self.GenerationCapacityThisTick = None
        self.GeneratedThisTick = None
        self.ConsumedThisTick = None
        self.DemandedThisTick = None
        self.SurplusDemandedThisTick = None
        self.WastedElectricityThisTick = None
        self.GeneratorsCount = 0
        self.ConsumersCount = 0
        self.ProductionStats = None
        self.ConsumptionStats = None
        self.GenerationCapacityStats = None

    class ConsumptionPerProto:
        def __init__(self):
            self.ConsumerProto = None
            self.ConsumptionStats = None
            self.LastTick = None
            self.EntitiesTotal = 0

    class ProductionPerProto:
        def __init__(self):
            self.ProducerProto = None
            self.ProductionStats = None
            self.LastTick = None
            self.EntitiesTotal = 0

    class ConsumptionLastTick:
        Empty = None
        def __init__(self):
            self.Consumed = None
            self.Demand = None
            self.MaxPossibleConsumption = None

    class ProductionLastTick:
        Empty = None
        def __init__(self):
            self.ProduceAndWasted = None
            self.Produced = None
            self.Wasted = None
            self.MaxGenerationCapacity = None

class SetElectricityGenerationPriorityCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.EntityId = None
        self.Priority = 0

class SetIsElectricitySurplusGeneratorCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.EntityId = None
        self.IsSurplusGenerator = False

class SetIsElectricitySurplusConsumerCmd:
    def __init__(self):
        self.AffectsSaveState = False
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.IsVerificationCmd = False
        self.Result = False
        self.HasError = False
        self.ErrorMessage = ""
        self.EntityId = None
        self.IsSurplusConsumer = False

class IElectricityConsumingEntity:
    def __init__(self):
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.GeneralPriority = 0
        self.IsGeneralPriorityVisible = False
        self.IsCargoAffectedByGeneralPriority = False
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class IElectricityGeneratingEntity:
    def __init__(self):
        self.ElectricityGenerator = None
        self.MaxGenerationCapacity = None
        self.DefaultTitle = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class IElectricityGeneratingEntityGrouped:
    def __init__(self):
        self.GeneratorsTotal = 0
        self.Prototype = None
        self.MaxGenerationCapacity = None

class IElectricityGenerator:
    def __init__(self):
        self.MaxGenerationCapacity = None

class IElectricityGeneratorRegistrator:
    def __init__(self):
        self.GenerationPriority = 0
        self.IsSurplusGenerator = False
        self.MaxGenerationCapacity = None
        self.GenerationCapacityThisTick = None
        self.GeneratedThisTick = None

class IElectricityGeneratorRegistratorFactory:
    def __init__(self):
        pass


class IElectricityManager:
    def __init__(self):
        self.ElectricityProto = None
        self.GenerationCapacityThisTick = None
        self.DemandedThisTick = None
        self.ConsumedThisTick = None

class ISolarPanelEntity:
    def __init__(self):
        self.MaxOutputElectricity = None
        self.Prototype = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.Value = None
        self.ConstructionCost = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False

class ISolarPanelsManager:
    def __init__(self):
        pass


class SolarPanelsManager:
    def __init__(self):
        pass

