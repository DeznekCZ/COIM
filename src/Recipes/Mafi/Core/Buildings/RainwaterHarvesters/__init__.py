
class RainHarvestingHelper:
    def __init__(self):
        self.IsEnabled = False
        self.WaterCollectedPerDayFullRain = None

class RainwaterHarvester:
    def __init__(self):
        self.CanBePaused = False
        self.WaterCollectedPerDayFullRain = None
        self.StoredWater = None
        self.LogisticsOutputControl = None
        self.Prototype = None
        from Mafi import Option
        self.StoredProduct = Option()
        self.Capacity = None
        self.CurrentQuantity = None
        self.PercentFull = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.LogisticsInputControl = None
        self.IsLogisticsInputDisabled = False
        self.IsLogisticsOutputDisabled = False
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Value = None
        self.ConstructionCost = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
        self.ConstructionProgress = Option()
        self.DoNotAdjustTerrainDuringConstruction = False
        self.AreConstructionCubesDisabled = False
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None

class RainwaterHarvesterProto:
    def __init__(self):
        self.EntityType = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.WaterProto = None
        self.WaterCollectedPerDay = None
        self.Capacity = None
        self.TransferLimit = None
        self.TransferLimitDuration = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.CannotBeBuiltByPlayer = False
        self.ConstructionDurationPerProduct = None
        self.VehicleGoalHeightAllowedRange = None
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False
        self.IsInitialized = False

class RainwaterHarvesterProtoBuilder:
    def __init__(self):
        self.ProtosDb = None
        self.Registrator = None

    class State:
        def __init__(self):
            self.Builder = None
