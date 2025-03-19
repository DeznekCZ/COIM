class ElectricityConfig:

    def __init__(self):
        pass

class IElectricityConfig:

    def __init__(self):
        pass

class IElectricityConsumerReadonly:

    def __init__(self):
        self.IsEnabled = False
        self.Priority = int(0)
        self.NotEnoughPower = False
        self.DidConsumeLastTick = False
        self.Entity = None
        self.IsSurplusConsumer = False
        self.PowerCharged = None
        self.PowerRequired = None
class IElectricityConsumer:

    def __init__(self):
        self.IsEnabled = False
        self.Priority = int(0)
        self.NotEnoughPower = False
        self.DidConsumeLastTick = False
        self.Entity = None
        self.IsSurplusConsumer = False
        self.PowerCharged = None
        self.PowerRequired = None
class IElectricityConsumerInternal:

    def __init__(self):
        self.ProtoToken = int(0)
        self.IsEnabled = False
        self.Priority = int(0)
        self.NotEnoughPower = False
        self.DidConsumeLastTick = False
        self.Entity = None
        self.IsSurplusConsumer = False
        self.PowerCharged = None
        self.PowerRequired = None
class ElectricityConsumer:

    def __init__(self):
        self.ProtoToken = int(0)
        self.Priority = int(0)
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

    def __init__(self):
        self.ElectricityProto = None
        self.MaxGenerationCapacity = None
        self.GenerationCapacityThisTick = None
        self.GeneratedThisTick = None
        self.ConsumedThisTick = None
        self.DemandedThisTick = None
        self.SurplusDemandedThisTick = None
        self.WastedElectricityThisTick = None
        self.GeneratorsCount = int(0)
        self.ConsumersCount = int(0)
class ConsumptionPerProto:

    def __init__(self):
        pass

class ProductionPerProto:

    def __init__(self):
        pass

class ConsumptionLastTick:

    def __init__(self):
        pass

class ProductionLastTick:

    def __init__(self):
        self.ProduceAndWasted = None
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class IElectricityConsumingEntity:

    def __init__(self):
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.GeneralPriority = int(0)
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
        self.GeneratorsTotal = int(0)
        self.Prototype = None
        self.MaxGenerationCapacity = None
class IElectricityGenerator:

    def __init__(self):
        self.MaxGenerationCapacity = None
class IElectricityGeneratorRegistrator:

    def __init__(self):
        self.GenerationPriority = int(0)
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

