class ComputingConsumer:

    def __init__(self):
        self.Priority = int(0)
        self.ProtoToken = int(0)
        self.IsEnabled = False
        self.ComputingCharged = None
        self.ComputingRequired = None
        self.DidConsumeLastTick = False
        self.NotEnoughComputing = False
        self.Entity = None
class IComputingConsumerFactory:

    def __init__(self):
        pass

class ComputingConsumerFactory:

    def __init__(self):
        pass

class ComputingConsumerFactoryExtensions:

    def __init__(self):
        pass

class ComputingManager:

    def __init__(self):
        self.ComputingProductProto = None
        self.ProvidedProducts = None
        self.ProducedLastTick = None
        self.DemandedThisTick = None
        self.GenerationCapacityThisTick = None
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
        pass

class IComputingConsumerReadonly:

    def __init__(self):
        self.IsEnabled = False
        self.Priority = int(0)
        self.NotEnoughComputing = False
        self.Entity = None
        self.DidConsumeLastTick = False
        self.ComputingCharged = None
        self.ComputingRequired = None
class IComputingConsumer:

    def __init__(self):
        self.IsEnabled = False
        self.Priority = int(0)
        self.NotEnoughComputing = False
        self.Entity = None
        self.DidConsumeLastTick = False
        self.ComputingCharged = None
        self.ComputingRequired = None
class IComputingConsumerInternal:

    def __init__(self):
        self.ProtoToken = int(0)
        self.IsEnabled = False
        self.Priority = int(0)
        self.NotEnoughComputing = False
        self.Entity = None
        self.DidConsumeLastTick = False
        self.ComputingCharged = None
        self.ComputingRequired = None
class ComputingConsumerExtensions:

    def __init__(self):
        pass

class IComputingConsumingEntity:

    def __init__(self):
        self.ComputingRequired = None
        from Mafi import Option
        self.ComputingConsumer = Option()
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
class IComputingGenerator:

    def __init__(self):
        self.MaxComputingGenerationCapacity = None
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
