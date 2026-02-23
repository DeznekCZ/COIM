
class AddEntityToScheduleItemCmd:
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
        self.ScheduleItemId = None
        self.EntityId = None
        self.Priority = 0

class AddNewScheduleItemToTrainLineCmd:
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
        self.TrainLineId = None
        self.EntityId = None
        self.Index = 0
        self.Priority = 0

class AddRemoveTrainScheduleItemProductFilterCmd:
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
        self.ScheduleItemId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()

        self.IsRemove = False
        self.IsUnload = False

class AddTrainTrackPillarCmd:
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
        self.BlockIdx = 0

class AssignTrainLineToTrainCmd:
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
        self.TrainId = None
        self.TrainLineId = None

class AssignTrainLineToUnfinishedTrainCmd:
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
        self.DepotId = None
        self.QueueIndex = 0
        self.TrainLineId = None

class BuildTrainCmd:
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
        self.DepotId = None
        self.CarDesigns = None
        self.TrainLineId = None
        self.TrainToReplace = None

class CancelTrainBuildCmd:
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
        self.DepotId = None
        self.QueueIndex = 0

class CancelTrainReplacementCmd:
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
        self.TrainId = None

class CarAndStationIndex:
    def __init__(self):
        self.TrainCarIndex = 0
        self.TrainStationIndex = 0

class CargoWagon:
    def __init__(self):
        self.Prototype = None
        self.Cargo = None
        from Mafi import Option
        self.OnlyAllowedProduct = Option()
        self.CargoQuantity = None
        self.Capacity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CanBePaused = False
        self.Train = Option()
        self.CustomTitle = Option()
        self.TrainIndex = 0
        self.IsBackwards = False
        self.IsConstructedBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.PreviousTrainCar = Option()
        self.NextTrainCar = Option()
        self.Position3f = None
        self.Position2f = None
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.SubCars = None

    class SubCargoWagon:
        def __init__(self):
            self.Cargo = None
            from Mafi import Option
            self.OnlyAllowedProduct = Option()
            self.Capacity = None
            self.UsableCapacity = None
            self.IsEmpty = False
            self.IsNotEmpty = False
            self.IsFull = False
            self.IsNotFull = False
            self.PercentFull = None
            self.AlignedStation = Option()
            self.CargoWagon = None
            self.TrainCar = None
            self.SubCarIndex = 0

class CargoWagonLoose:
    def __init__(self):
        self.Prototype = None
        self.Cargo = None
        from Mafi import Option
        self.OnlyAllowedProduct = Option()
        self.CargoQuantity = None
        self.Capacity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CanBePaused = False
        self.Train = Option()
        self.CustomTitle = Option()
        self.TrainIndex = 0
        self.IsBackwards = False
        self.IsConstructedBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.PreviousTrainCar = Option()
        self.NextTrainCar = Option()
        self.Position3f = None
        self.Position2f = None
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.SubCars = None

class CargoWagonMolten:
    def __init__(self):
        self.Prototype = None
        self.Cargo = None
        from Mafi import Option
        self.OnlyAllowedProduct = Option()
        self.CargoQuantity = None
        self.Capacity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CanBePaused = False
        self.Train = Option()
        self.CustomTitle = Option()
        self.TrainIndex = 0
        self.IsBackwards = False
        self.IsConstructedBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.PreviousTrainCar = Option()
        self.NextTrainCar = Option()
        self.Position3f = None
        self.Position2f = None
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.SubCars = None

    class SubCargoWagonMolten:
        def __init__(self):
            self.LadleAngle = None
            self.Cargo = None
            from Mafi import Option
            self.OnlyAllowedProduct = Option()
            self.Capacity = None
            self.UsableCapacity = None
            self.IsEmpty = False
            self.IsNotEmpty = False
            self.IsFull = False
            self.IsNotFull = False
            self.PercentFull = None
            self.AlignedStation = Option()
            self.MoltenWagon = None
            self.CargoWagon = None
            self.TrainCar = None
            self.SubCarIndex = 0

class CargoWagonUnit:
    def __init__(self):
        self.Prototype = None
        self.Cargo = None
        from Mafi import Option
        self.OnlyAllowedProduct = Option()
        self.CargoQuantity = None
        self.Capacity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CanBePaused = False
        self.Train = Option()
        self.CustomTitle = Option()
        self.TrainIndex = 0
        self.IsBackwards = False
        self.IsConstructedBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.PreviousTrainCar = Option()
        self.NextTrainCar = Option()
        self.Position3f = None
        self.Position2f = None
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.SubCars = None

class CreateNewTrainLineCmd:
    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = ""

class CreateTrainTrackEntityWithDirectionCmd:
    def __init__(self):
        self.IsProcessed = False
        self.IsProcessedAndSynced = False
        self.ProcessedAtStep = None
        self.ResultSet = False
        self.AffectsSaveState = False
        self.IsVerificationCmd = False
        self.Result = None
        self.HasError = False
        self.ErrorMessage = ""
        self.Direction = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.ProtoId = StaticEntityProto.ID()

        self.Transform = None
        self.IsFree = False
        self.AllowValidationSuppression = False

class CreateTrainTrackFromPlanCmd:
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
        self.Plan = None

class DestroyTrainImmediateCmd:
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
        self.TrainId = None

class EditDepartConditionCmd:
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
        self.ScheduleItemId = None
        self.RootId = None
        self.ConditionIndex = 0

class FailedPathVisitedNodes:
    def __init__(self):
        self.TrainGraphIdsValidityVersion = 0
        self.PathfindingId = 0
        self.Bitmap = None

class LayoutEntityWithTrainTrackBase:
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.TrainTrackId = None
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.Poles = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.Waypoints = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None

class LevelCrossing:
    DELAY_BEFORE_OPEN = None
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsReservedByTrain = False
        self.IsOccupiedByTrain = False
        self.IsNotifiedByTrain = False
        self.IsRoadGloballyClosed = False
        self.IsRoadClosedSelf = False
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.CanBePaused = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Waypoints = None
        self.RoadLanesCount = 0
        self.NumberOfPassedTrains = 0
        self.HasBadConnection = False
        self.ClosedForTrainCrossingPercentage = None
        self.AnimationParams = None
        self.AnimationStatesProvider = None
        self.EmissionIntensity = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
        from Mafi import Option
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
        self.TrackProto = None
        self.RoadProto = None

class LevelCrossingEntrance:
    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.RoadTerrainConnectionsCount = 0
        self.IsRoadGloballyClosed = False
        self.IsRoadClosedSelf = False
        self.GateClosedPercentage = None
        self.RoadLanesCount = 0
        self.RoadProto = None
        self.HasBadConnection = False
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
        from Mafi import Option
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

class LevelCrossingsManager:
    def __init__(self):
        self.LevelCrossingTrainApproaching = None

class Locomotive:
    POWER_WHEN_BROKEN = None
    POWER_ON_LOW_FUEL = None
    ENGINE_OFF_WHEN_STOPPED = None
    IDLE_FUEL_CONSUMPTION = None
    def __init__(self):
        self.Prototype = None
        self.IsFuelTankEmpty = False
        self.IsFuelTankFull = False
        self.CannotWorkDueToLowFuel = False
        self.CanRunWithNoFuel = False
        from Mafi import Option
        self.FuelTankProto = Option()
        self.FuelTank = Option()
        self.FuelConsumption = None
        self.PowerFactor = None
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.AlignedStation = Option()
        self.UseFuelTankToMakePower = False
        self.IsEngineOn = False
        self.CanBePaused = False
        self.Train = Option()
        self.CustomTitle = Option()
        self.TrainIndex = 0
        self.IsBackwards = False
        self.IsConstructedBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.PreviousTrainCar = Option()
        self.NextTrainCar = Option()
        self.Position3f = None
        self.Position2f = None
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.SubCars = None

class NavigateTrainToCmd:
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
        self.TrainId = None
        self.TargetEntityId = None

class QuickBuildCurrentTrainCmd:
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
        self.DepotId = None

class QuickRepairTrainCmd:
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
        self.TrainId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()


class RecoverTrainCmd:
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
        self.TrainId = None
        self.ForcePause = False
        self.TargetDepot = None

class RefuelTrainCmd:
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
        self.TrainId = None

class RemoveDepartConditionCmd:
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
        self.ScheduleItemId = None
        self.RootId = None
        self.ConditionIndex = 0

class RemoveEntityFromScheduleItemCmd:
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
        self.ScheduleItemId = None
        self.EntityId = None

class RemoveTrainLineCmd:
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
        self.TrainLineId = None

class RemoveTrainLineScheduleItemCmd:
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
        self.ScheduleItem = None

class RemoveTrainTrackPillarCmd:
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
        self.PillarId = None

class RenameTrainLineCmd:
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
        self.TrainLineId = None
        self.Name = ""

class ReorderDepartConditionCmd:
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
        self.ScheduleItemId = None
        self.RootId = None
        self.ConditionIndex = 0
        self.NewConditionIndex = 0

class ReorderTrainLineScheduleItemCmd:
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
        self.ScheduleItem = None
        self.NewIndex = 0

class ReplaceEntityInScheduleCmd:
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
        self.ScheduleItemId = None
        self.OldEntityId = None
        self.NewEntityId = None

class ReverseTracksCmd:
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
        self.TrackIds = None

class ReverseTrainCmd:
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
        self.TrainId = None

class ScrapTrainCarCmd:
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
        self.DepotId = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.CarProtoId = DynamicEntityProto.ID()


class SetDepartConditionCombineMethodCmd:
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
        self.ScheduleItemId = None
        self.RootId = None
        self.ConditionIndex = 0
        self.CombineAsOr = False

class SetTrainDrivingModeCmd:
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
        self.TrainId = None
        self.Mode = None

class SetTrainLineColorApplicationCmd:
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
        self.TrainLineId = None
        self.ApplyLineColorToTrains = False

class SetTrainLineColorCmd:
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
        self.TrainLineId = None
        self.Color = None

class SetTrainLineColorSourceCmd:
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
        self.TrainLineId = None
        self.UseProductColor = False

class SetTrainLineIcon:
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
        self.TrainLineId = None
        from Mafi import Option
        self.ProtoForIcon = Option()

class SetTrainPreferredDirectionCmd:
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
        self.TrainId = None
        self.Direction = None

class SetTrainScheduleItemProductFilterCmd:
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
        self.ScheduleItemId = None
        self.ProductProto = None
        self.IsRemove = False
        self.IsUnload = False
        self.Disable = False

class SetTrainSchedulePriority:
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
        self.ScheduleItemId = None
        self.Priority = 0
        self.RootId = None

class SetTrainScheduleSkipIfHighFuelCmd:
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
        self.ScheduleItemId = None
        self.SkipIfFuelHigherThan = None

class SetTrainSpeedCmd:
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
        self.TrainId = None
        self.NewSpeed = None

class SetTrainStationModuleLimitsCmd:
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
        self.ModuleLimits = None
        self.TrainLimit = None

class SetTrainTrackCriticalBlockStateCmd:
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
        self.BlockIds = None
        self.IsCritical = False

class SetTrainTrackSuperBlockStateCmd:
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
        self.BlockIds = None
        self.IsSuperBlock = False

class SetWagonProductFilterCmd:
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
        self.TrainId = None
        self.WagonIndex = 0
        self.SubCarIndex = 0
        self.ProductId = None

class SkipScheduleItemCmd:
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
        self.TrainId = None

class StartNodeMetadata:
    def __init__(self):
        self.RequiresTrainToReverse = False
        self.Entity = None
        self.IsAtTrainRear = False
        self.IsTrainReversed = False

class TenderWagon:
    def __init__(self):
        self.Prototype = None
        self.UseFuelTankToMakePower = False
        self.IsFuelTankEmpty = False
        self.IsFuelTankFull = False
        self.CannotWorkDueToLowFuel = False
        self.CanRunWithNoFuel = False
        from Mafi import Option
        self.FuelTankProto = Option()
        self.FuelTank = Option()
        self.FuelConsumption = None
        self.PowerFactor = None
        self.Maintenance = None
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.AlignedStation = Option()
        self.IsEngineOn = False
        self.CanBePaused = False
        self.Train = Option()
        self.CustomTitle = Option()
        self.TrainIndex = 0
        self.IsBackwards = False
        self.IsConstructedBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.PreviousTrainCar = Option()
        self.NextTrainCar = Option()
        self.Position3f = None
        self.Position2f = None
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.MaintenanceCosts = None
        self.IsIdleForMaintenance = False
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.SubCars = None

class ToggleBidirectionalCommand:
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
        self.TrackIds = None

class ToggleLoadUnloadTrainScheduleItemCmd:
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
        self.ScheduleItemId = None
        self.IsUnload = False
        self.Disable = False

class ToggleScrapTrainAtNearestDepotCmd:
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
        self.TrainId = None

class ToggleTrainFullEmptyDebugCmd:
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
        self.TrainId = None

class ToggleTrainPausedCmd:
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
        self.TrainId = None

class ToggleTrainStationModuleLoadUnloadCmd:
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
        self.Load = None

class TrackPathRecord:
    def __init__(self):
        self.TrackEntity = None
        self.IsBackwardsEdge = False

class Train:
    IDLE_DURATION_FOR_WARNING = None
    IDLE_DURATION_FOR_ERROR = None
    RESET_INCREMENTAL_RESERVATION_PERIOD = None
    TIME_BEFORE_NO_PATH_NOTIF = None
    RECOVERY_COST_PER_CAR = None
    MAX_SPEED = None
    MAX_ROLL_DRAG_SPEED = None
    NOTIFY_APPROACHING_DISTANCE_MULT = 0
    NOTIFY_APPROACHING_MAX_INTERVAL = None
    NOTIFY_APPROACHING_TICKS_REDUCED_PER_TILE = None
    NOT_APPROACHING_TIMEOUT = None
    IDLE_TIME_BEFORE_DEPART_NO_CONDITIONS = None
    IDLE_TIME_BEFORE_DEPART_WITH_CONDITIONS = None
    PATH_RETRY_COOLDOWN = None
    def __init__(self):
        self.Name = ""
        self.DefaultTitle = None
        self.Position2f = None
        self.Position3f = None
        self.TrainId = None
        self.TrainCarsCount = 0
        self.TrainSubCarsCount = 0
        self.TrainCars = None
        self.TrainSubCars = None
        self.Locomotives = None
        self.CargoWagons = None
        self.TrainCarsDataInDriveOrder = None
        self.Length = None
        self.TrainCarsColorOverride = None
        self.IsReversed = False
        self.Data = None
        from Mafi import Fix64
        self.LifetimeDistanceTraveled = Fix64()
        self.LifetimeLoadedQuantity = None
        self.ThrottlePercent = None
        self.BrakesPercent = None
        self.Speed = None
        self.TargetSpeed = None
        self.MaxSpeed = None
        self.SpeedLimit = None
        self.Acceleration = Fix64()
        self.BrakingDistance = None
        self.FreeSpaceEstimate = None
        self.SpeedLimitAhead = None
        self.ClearanceThrottle = None
        from Mafi import Fix32
        self.BrakingForceKnCurrent = Fix32()
        self.TrainHeadWaypointIndex = Fix32()
        self.TrainTailWaypointIndex = Fix32()
        self.IsSpawned = False
        self.GradeForceKn = Fix32()
        self.AirDragKn = Fix32()
        self.AccelerationForceKnCurrent = Fix32()
        self.AccelerationForceKnAvailable = Fix32()
        self.TractiveForceKnCombinedSmooth = Fix32()
        self.RollDragKn = Fix32()
        self.RollDragWhenMoving = Fix32()
        self.MassTons = Fix32()
        self.Waypoints = None
        self.OccupiedBlocks = None
        self.OccupiedBlocksCount = 0
        self.OccupiedWaypointsCount = 0
        self.ReservedBlocks = None
        self.OverlapReservedBlocks = None
        self.ReservedBlocksCount = 0
        self.ReservedWaypointsCount = 0
        self.UnreservedBlocksCount = 0
        self.UnreservedWaypointsCount = 0
        self.TrainsManager = None
        self.TrainTracksGraphManager = None
        from Mafi import Option
        self.Depot = Option()
        self.IsEnteringDepot = False
        self.NoPathFromTrainDepot = False
        self.IsDespawning = False
        self.IsDestroyed = False
        self.IsPaused = False
        self.PreferDirection = None
        self.PathFindingTask = None
        self.LastUsedGoals = None
        self.Goal = Option()
        self.LastFoundGoalEntity = Option()
        self.PathDoesNotExist = False
        self.CurrentPath = None
        self.TrainLine = Option()
        self.CurrentScheduleItem = Option()
        self.ReservedStationGroupSlots = None
        self.DrivingMode = None
        self.ReservationWaitTime = None
        self.NotifyingCannotScrap = False
        self.CurrentStation = Option()
        self.IsBeingScrapped = False
        self.HasReplacementReady = False
        self.CurrentPathGoalEntities = None
        self.StationInactiveDuration = None
        self.StationaryDuration = None
        self.StationaryDurationInvoluntary = None
        self.ForcedScheduleIndex = None
        self.TimeAtCurrentStation = None
        self.RecoveryCost = None
        self.LastBlockingTrainIdOrNone = None
        self.AttemptedReservationDistance = None
        self.LastFailedBlockReservation = None
        self.FailedPathVisitedNodes = Option()
        self.DepotToReplaceAt = Option()
        self.CannotWorkDueToLowFuel = False
        self.StateForUi = None
        self.WarningForUi = None

    class TrainCarData:
        def __init__(self):
            self.TrainCar = None
            from Mafi import Fix32
            self.FrontAxleWaypointIndex = Fix32()
            self.RearAxleWaypointIndex = Fix32()

class TrainCarBase:
    def __init__(self):
        self.CanBePaused = False
        self.Prototype = None
        from Mafi import Option
        self.Train = Option()
        self.CustomTitle = Option()
        self.TrainIndex = 0
        self.IsBackwards = False
        self.IsConstructedBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.PreviousTrainCar = Option()
        self.NextTrainCar = Option()
        self.Position3f = None
        self.Position2f = None
        self.Id = None
        self.DefaultTitle = None
        self.Context = None
        self.IsDestroyed = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.SubCars = None

    class TrainSubCarBase:
        def __init__(self):
            self.TrainCar = None
            self.SubCarIndex = 0

class TrainCarDesign:
    def __init__(self):
        self.AllowsPrimaryFilter = False
        self.AllowsSecondaryFilter = False
        self.Proto = None
        self.IsFlipped = False
        from Mafi import Option
        self.PrimaryProduct = Option()
        self.SecondaryProduct = Option()

class TrainCarDesignWithId:
    def __init__(self):
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.ProtoId = DynamicEntityProto.ID()

        self.IsFlipped = False
        self.SecondaryProduct = None
        self.PrimaryProduct = None

class TrainColor:
    def __init__(self):
        self.Primary = None
        self.Secondary = None
        self.Raw = None

class TrainContainsDepartCondition:
    def __init__(self):
        self.UseLessThanInsteadOfSuperiorTo = False
        self.Mode = None
        self.Percent = None
        from Mafi import Option
        self.Product = Option()
        self.IsSimplified = False
        self.IsFullOfCondition = False
        self.IsEmptyOfCondition = False
        self.CombineAsOrInsteadOfAnd = False
        self.LastEvalResult = False

    class ComparisonMode:
        AllProducts = None
        AnyProduct = None
        SpecificProduct = None
        def __init__(self):
            self.value__ = 0

class TrainContainsDepartConditionEditCmd:
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
        self.UseInferiorToInsteadOfSuperiorTo = None
        self.Mode = None
        self.Percent = None
        self.ProductId = None
        self.ProductIdSpecified = False
        self.IsSimplified = None
        self.ScheduleItemId = None
        self.RootId = None
        self.ConditionIndex = 0

class TrainDepot:
    def __init__(self):
        self.Prototype = None
        self.UpgradableProto = None
        self.CanBePaused = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.MaxTrainLength = None
        self.ExtensionOffset = None
        self.MaxStoredOfEachCarType = 0
        self.NumServiceLanes = 0
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.TrainConstructionProgress = Option()
        self.Buffers = None
        self.CarsWithoutTrain = None
        self.InternalTracks = None
        self.InternalTracksVersion = 0
        self.TrainsQueuedToSpawn = None
        self.TrainBuildQueue = None
        self.ConstructedTrainsWaitingForReplacement = None
        self.IsBuildingTrain = False
        self.DoorInOpenPerc = None
        self.DoorOutOpenPerc = None
        self.ArrivingExternalBlockId = None
        self.ArrivingInternalBlockId = None
        self.CentreBlockId = None
        self.DepartingInternalBlockId = None
        self.DepartingExternalBlockId = None
        self.CanDisableLogisticsInput = False
        self.CanDisableLogisticsOutput = False
        self.LogisticsInputMode = None
        self.LogisticsOutputMode = None
        self.TrackProto = None
        self.TrainTrackId = None
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.Poles = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.Waypoints = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False

    class TrainConstructionInfo:
        def __init__(self):
            self.TrainLineId = None
            from Mafi import Option
            self.TrainToReplace = Option()
            self.ReusedWagons = None
            self.IsReplacement = False
            self.Design = None

    class TrainDepotTrackSlot:
        def __init__(self):
            from Mafi import Option
            self.Train = Option()
            self.IsFree = False
            self.IsFull = False
            self.IsReservedForReplace = False
            self.ReservedForConstruction = False
            self.ReplacementDesign = Option()

class TrainDepotExtension:
    def __init__(self):
        self.Prototype = None
        self.ExtensionOffset = None
        self.CanBePaused = False
        self.Depot = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
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

class TrainDurationDepartCondition:
    def __init__(self):
        self.Delay = None
        self.CombineAsOrInsteadOfAnd = False
        self.LastEvalResult = False

class TrainDurationDepartConditionEditCmd:
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
        self.Delay = None
        self.ScheduleItemId = None
        self.RootId = None
        self.ConditionIndex = 0

class TrainLine:
    COLOR_PALETTE = None
    def __init__(self):
        self.Color = None
        self.UseProductColor = False
        self.ApplyLineColorToTrains = False
        self.Name = None
        from Mafi import Option
        self.ProtoForIcon = Option()
        self.Schedule = None
        self.TrainsCount = 0
        self.Trains = None
        self.Id = None
        self.Manager = None

class TrainLineColor:
    def __init__(self):
        self.Primary = None
        self.Secondary = None
        self.TrainColor = None

class TrainLinesManager:
    def __init__(self):
        self.Lines = None

class TrainPathFindingTask:
    def __init__(self):
        self.Train = None
        self.IsEnqueuedOrBeingProcessed = False
        self.IsPathImprovementTask = False
        self.CustomStart = None
        self.Goals = None
        self.Status = None
        self.PathWasAcceptedByTrain = False
        from Mafi import Option
        self.ActualGoalEntity = Option()
        self.LastAttemptedGoals = None
        self.LastFoundGoalEntity = Option()
        self.ActualStartNodeId = None
        self.PathFindingManager = Option()
        self.IsReversible = False
        self.Path = None
        self.PathView = None
        self.LastPfResult = None
        self.StartNodes = None
        self.StartNodesMetadata = None
        self.LastPathFailedStep = None
        self.GoalNodes = None
        self.ForwardMaxSpeed = None
        self.BackwardMaxSpeed = None
        self.MaxDistanceForOccupancyCostPenalties = None
        self.RequireElectrified = False
        self.PfInitCount = 0
        self.PfStartCount = 0
        self.PfPathFoundCount = 0
        self.PfCancelledCount = 0

class TrainScheduleDepartConditionBase:
    def __init__(self):
        self.CombineAsOrInsteadOfAnd = False
        self.LastEvalResult = False

class TrainScheduleItemId:
    def __init__(self):
        self.TrainLineId = None
        self.ScheduleIndex = 0

class TrainsManager:
    def __init__(self):
        self.Trains = None
        self.TrainsDict = None
        self.LargestBlockWaypointCount = 0
        self.TrainPausedStateChanged = None
        self.SlopeDifficultyMultiplier = None
        self.FuelConsumptionMultiplier = None
        self.TrainDepots = None
        self.ForceMaxRepath = False
        self.TrainGraphManager = None
        self.TrainStationManager = None
        self.EntitiesManager = None
        self.EntityContext = None
        self.TrainsPathFindingManager = None
        self.SimLoopEvents = None

class TrainsPathFindingManager:
    def __init__(self):
        self.QueueSize = 0
        self.TotalEnqueuedTasksCount = None

class TrainsPathFindingManagerConfig:
    def __init__(self):
        self.PerformWorkInBackgroundThread = False
        self.MaxStepsPerTick = 0

class TrainStationAlignment:
    def __init__(self):
        self.TrainCarOffset = 0
        self.TrainCarIndex = 0
        self.TrainStationIndex = 0
        self.AllAlignments = None

class TrainStationAlignmentPlan:
    EMPTY_PLAN = None
    def __init__(self):
        self.Alignments = None
        self.StationGroupId = None
        self.TrainIsInReverse = False
        self.StationIsInReverse = False

class TrainStationAlignmentState:
    def __init__(self):
        self.AlignmentStatus = None
        self.StationGroupId = None
        self.IsComplete = False
        self.HasMoreAlignments = False
        self.HasAnyAlignments = False

class TrainStationBase:
    def __init__(self):
        self.Prototype = None
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.CanBePaused = False
        from Mafi import Option
        self.ElectricityConsumer = Option()
        self.CanWorkOnLowPower = False
        self.UpgradableProto = None
        self.TrackProto = None
        self.TrainTrackId = None
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.Poles = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.Waypoints = None
        self.CustomTitle = Option()
        self.GeneralPriority = 0
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.Ports = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.WorkersNeeded = 0
        self.HasWorkersCached = False
        self.PowerRequired = None

class TrainStationCheatAssignedProductCmd:
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
        self.ModuleId = None

class TrainStationGroup:
    MAX_GROUP_SIZE = 0
    def __init__(self):
        self.StationEntities = None
        self.ForwardEdgeStationEntities = None
        self.ReverseEdgeStationEntities = None
        self.StationGroupId = None
        self.StationRootEntity = None

    class StationAndDirection:
        def __init__(self):
            self.Station = None
            self.Direction = None

class TrainStationManager:
    def __init__(self):
        self.TrainStationEntities = None
        self.TrainStationGroups = None

class TrainStationModuleClearProductCmd:
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
        self.ModuleId = None

class TrainStationModuleQuickRemoveProductCmd:
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
        self.ModuleId = None

class TrainStationModuleSetProductCmd:
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
        self.ModuleId = None
        from Mafi.Core.Products import ProductProto
        self.ProductId = ProductProto.ID()


class TrainStationScheduleItem:
    DEFAULT_PRIORITY = 0
    def __init__(self):
        self.IndexInSchedule = 0
        self.StationRoots = None
        self.StationPriorities = None
        self.SkipIfFuelHigherThan = None
        self.DisableLoad = False
        self.DisableUnload = False
        self.Id = None
        self.DepartConditions = None
        self.LoadOnlyProducts = None
        self.UnloadOnlyProducts = None

class TrainTrack:
    def __init__(self):
        self.Prototype = None
        self.TrackProto = None
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.CanBePaused = False
        self.PillarBlocksBitmap = None
        self.Pillars = None
        self.UpgradableProto = None
        self.Poles = None
        self.Transform = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
        from Mafi import Option
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

class TrainTrackBlock:
    def __init__(self):
        self.BlockId = None
        self.Entity = None
        self.BlockIndex = None

class TrainTrackBlockId:
    def __init__(self):
        self.BlockIndex = None
        self.TrainTrackId = None
        self.RawData = None

class TrainTrackBlockRecord:
    def __init__(self):
        self.BlockId = None
        self.WaypointsCount = None
        self.IsBackwardsEdge = False

class TrainTrackNodeDirection:
    DIR_MASK = 0
    def __init__(self):
        self.Dx = 0
        self.Dy = 0
        self.Direction = None
        self.DirectionPacked = None
        self.GradeFactor = None

class TrainTrackPathFinderOptions:
    def __init__(self):
        self.PreferredHeight = None
        self.ForcedStartDirectionA = None
        self.ForcedStartDirectionB = None
        self.ForcedEndDirectionA = None
        self.ForcedEndDirectionB = None
        self.BuildDirection = None
        self.Flags = None

class TrainTrackPillar:
    def __init__(self):
        self.CanBePaused = False
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.Height = None
        self.TopTileHeight = None
        self.BlockIndex = 0
        self.PillarInfoRel = None
        self.TrainTrack = None
        self.TrainTrackEntityId = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.Prototype = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
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
        self.PillarPosition = None
        self.PillarInfo = None

class TrainTrackPillarInfo:
    def __init__(self):
        self.Position2f = None
        self.Offset = None
        self.Height = None
        self.InfoRel = None

class TrainTrackPillarInfoRel:
    def __init__(self):
        self.Position = None
        self.Direction = None
        self.RelHeight = None
        self.OccupancyMask = None
        self.BlockIndex = 0

class TrainTrackPlan:
    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.BuildDirection = None
        self.Steps = None
        self.PillarsValid = False
        self.PolesValid = False
        self.PoleAtStartIndex = None
        self.PoleAtEndIndex = None
        self.LastStepEndPosition = None
        self.Options = None
        self.IsStartPlaceHolder = False

class TrainTrackPlanStep:
    def __init__(self):
        self.Proto = None
        self.EndDirection = None
        self.Transform = None
        self.PillarInfos = None
        self.PoleInfos = None
        self.TrackDirection = None
        self.Aabb = None
        self.OccupiedTilesRelative = None
        self.OccupiedVerticesRelative = None

class TrainTrackPole:
    def __init__(self):
        self.CanBePaused = False
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.BlockIndex = 0
        self.PoleInfoRel = None
        self.TrainTrack = None
        self.TrainTrackEntityId = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.Prototype = None
        self.CenterTile = None
        self.Position2f = None
        self.Position3f = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.ConstructionState = None
        self.IsConstructed = False
        self.IsNotConstructed = False
        self.IsBeingUpgraded = False
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
        self.PolePosition = None
        self.PoleInfo = None

class TrainTrackPoleInfo:
    def __init__(self):
        self.TrackPosition3f = None
        self.Position2f = None
        self.Position3f = None
        self.TrackPosition = None
        self.InfoRel = None

class TrainTrackPoleInfoRel:
    def __init__(self):
        self.Position = None
        self.BlockIndex = 0
        self.PoleIndex = 0
        self.IsClose = False
        self.IsStartEnd = False
        self.Rotation = None

class TrainTracksCollapseHelper:
    TIME_TO_COLLAPSE = None
    def __init__(self):
        self.AnyGoingToCollapse = False

class TrainTracksGraphManager:
    def __init__(self):
        self.EntityAdded = None
        self.EntityRemoved = None
        self.OnTrackSuperBlockChanged = None
        self.OnTrackCriticalChanged = None
        self.OnTrackReservedChanged = None
        self.OnTrackOccupiedChanged = None
        self.OnTrackSuperBlockReservationChanged = None
        self.OnTrackOverlappingChanged = None
        self.OnTrackDirectionChanged = None
        self.GraphEdgeAdded = None
        self.SuperBlocksCount = 0
        self.ClaimedSuperBlocksOfTrains = None
        self.GraphMutationsAllowed = False
        self.IgnoreForCollisionsPredicate = None
        self.Initialized = False
        self.TrainGraphVersion = 0
        self.TrainGraphIdsValidityVersion = 0
        self.TrackEntitiesCount = 0
        self.TrackEntities = None
        self.GraphNodes = None
        self.NodesWithSingleEdge = None
        self.NodesWithSingleElectricEdge = None
        self.ChunkedTrackEntities = None
        self.GraphNodesCount = 0
        self.NodesStorageSize = 0
        self.GraphEdgesCount = 0
        self.EdgesStorageSize = 0
        self.TracksWithEndsRequired = None
        self.OnTrackSupportChanged = None
        self.OnTrackSupportChangedV2 = None
        self.OnTrackElectrificationChanged = None
        self.OnSingleEdgeNodeAdded = None
        self.OnSingleEdgeNodeRemoved = None
        self.OnTrackGraphicsChangeNodeAdded = None
        self.OnTrackGraphicsChangeNodeRemoved = None
        self.OnTrackElectricChangeNodeAdded = None
        self.OnTracElectricChangeNodeRemoved = None
        self.Priority = None
        self.PillarManager = None
        from Mafi import Option
        self.PolesManager = Option()

    class TrackEntityAndBlock:
        def __init__(self):
            self.Entity = None
            self.BlockIndex = 0

    class TrainTrackAddRequestMetaData:
        def __init__(self):
            self.IsForStartChecking = False
            self.UpgradingId = None

class TrainTracksPillarManager:
    def __init__(self):
        pass


class CanBuildTrainTrackResult:
    def __init__(self):
        self.RequestStartDirection = None
        self.RequestEndDirection = None
        from Mafi import Option
        self.NewPlan = Option()

class CargoWagonProto:
    def __init__(self):
        self.EntityType = None
        self.Capacity = None
        self.SubCarCapacity = None
        self.IconPath = ""
        self.BogiePivotsDistance = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.ProductType = None
        self.CarLength = None
        self.BogiePivotsOffset = None
        self.SubCarCount = 0
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.BogiePivotToCarEnd = None
        self.BogieWheelBase = None
        self.MaxSpeed = None
        from Mafi import Fix32
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.BrakingForceKn = Fix32()
        self.RollingResistanceCoefficientTimesThousand = Fix32()
        self.FrontalAreaM2 = Fix32()
        self.LengthDragAsExtraFrontalArea = Fix32()
        self.DragCoefficientStandalone = Fix32()
        self.DragCoefficientInline = Fix32()
        self.OnlyAllowedAtFront = None
        self.OnlyAllowedAtRear = None
        self.Graphics = None
        self.IsPhantom = False

class CargoWagonLooseProto:
    def __init__(self):
        self.EntityType = None
        self.Capacity = None
        self.SubCarCapacity = None
        self.IconPath = ""
        self.BogiePivotsDistance = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.ProductType = None
        self.CarLength = None
        self.BogiePivotsOffset = None
        self.SubCarCount = 0
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.BogiePivotToCarEnd = None
        self.BogieWheelBase = None
        self.MaxSpeed = None
        from Mafi import Fix32
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.BrakingForceKn = Fix32()
        self.RollingResistanceCoefficientTimesThousand = Fix32()
        self.FrontalAreaM2 = Fix32()
        self.LengthDragAsExtraFrontalArea = Fix32()
        self.DragCoefficientStandalone = Fix32()
        self.DragCoefficientInline = Fix32()
        self.OnlyAllowedAtFront = None
        self.OnlyAllowedAtRear = None
        self.IsPhantom = False

    class Gfx:
        def __init__(self):
            self.SideViewIconPath = ""
            self.IconPath = ""
            self.PilesData = None
            self.PrefabPath = ""
            self.FrontBogieModelName = ""
            self.RearBogieModelName = ""
            self.FrontCouplerName = ""
            self.RearCouplerName = ""
            self.FrontCarConnectorName = ""
            self.RearCarConnectorName = ""
            self.FrontCarConnectorSize = 0.0
            self.RearCarConnectorSize = 0.0
            self.WheelModelPrefix = ""
            self.WheelRadiusMeters = 0.0
            self.CarWidthMeters = 0.0
            self.CarHeightMeters = 0.0
            self.WheelCircumferenceMeters = 0.0
            self.UseAnimationForWheelMovement = False
            self.OrderInBuildMenu = 0.0
            self.DefaultColor = None
            self.SideViewIconIsCustom = False
            from Mafi import Option
            self.ExhaustParticlesSpec = Option()
            self.MotionSoundSpec = None
            self.BrakingSoundSpec = None
            self.StoppedSoundSpec = None
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0

        class WagonPileData:
            def __init__(self):
                self.AnimationStateName = ""
                self.PileObjectPath = ""
                self.PileTextureParams = None

class CargoWagonMoltenProto:
    def __init__(self):
        self.EntityType = None
        self.Capacity = None
        self.SubCarCapacity = None
        self.IconPath = ""
        self.BogiePivotsDistance = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.LadleAngleStartUnload = None
        self.LadleAngleEndUnload = None
        self.LadleAngleSpeedPerTick = None
        self.ProductType = None
        self.CarLength = None
        self.BogiePivotsOffset = None
        self.SubCarCount = 0
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.BogiePivotToCarEnd = None
        self.BogieWheelBase = None
        self.MaxSpeed = None
        from Mafi import Fix32
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.BrakingForceKn = Fix32()
        self.RollingResistanceCoefficientTimesThousand = Fix32()
        self.FrontalAreaM2 = Fix32()
        self.LengthDragAsExtraFrontalArea = Fix32()
        self.DragCoefficientStandalone = Fix32()
        self.DragCoefficientInline = Fix32()
        self.OnlyAllowedAtFront = None
        self.OnlyAllowedAtRear = None
        self.IsPhantom = False

    class Gfx:
        def __init__(self):
            self.SideViewIconPath = ""
            self.IconPath = ""
            self.LadleObjectPrefix = ""
            self.HasMoltenSurfaceAnimation = False
            self.ParticlesParamsForUnloadingLeft = None
            self.ParticlesParamsForUnloadingRight = None
            self.PrefabPath = ""
            self.FrontBogieModelName = ""
            self.RearBogieModelName = ""
            self.FrontCouplerName = ""
            self.RearCouplerName = ""
            self.FrontCarConnectorName = ""
            self.RearCarConnectorName = ""
            self.FrontCarConnectorSize = 0.0
            self.RearCarConnectorSize = 0.0
            self.WheelModelPrefix = ""
            self.WheelRadiusMeters = 0.0
            self.CarWidthMeters = 0.0
            self.CarHeightMeters = 0.0
            self.WheelCircumferenceMeters = 0.0
            self.UseAnimationForWheelMovement = False
            self.OrderInBuildMenu = 0.0
            self.DefaultColor = None
            self.SideViewIconIsCustom = False
            from Mafi import Option
            self.ExhaustParticlesSpec = Option()
            self.MotionSoundSpec = None
            self.BrakingSoundSpec = None
            self.StoppedSoundSpec = None
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0

class CargoWagonUnitProto:
    def __init__(self):
        self.EntityType = None
        self.Capacity = None
        self.SubCarCapacity = None
        self.IconPath = ""
        self.BogiePivotsDistance = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.ProductType = None
        self.CarLength = None
        self.BogiePivotsOffset = None
        self.SubCarCount = 0
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.BogiePivotToCarEnd = None
        self.BogieWheelBase = None
        self.MaxSpeed = None
        from Mafi import Fix32
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.BrakingForceKn = Fix32()
        self.RollingResistanceCoefficientTimesThousand = Fix32()
        self.FrontalAreaM2 = Fix32()
        self.LengthDragAsExtraFrontalArea = Fix32()
        self.DragCoefficientStandalone = Fix32()
        self.DragCoefficientInline = Fix32()
        self.OnlyAllowedAtFront = None
        self.OnlyAllowedAtRear = None
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.SideViewIconPath = ""
            self.IconPath = ""
            self.ProductsData = None
            self.PrefabPath = ""
            self.FrontBogieModelName = ""
            self.RearBogieModelName = ""
            self.FrontCouplerName = ""
            self.RearCouplerName = ""
            self.FrontCarConnectorName = ""
            self.RearCarConnectorName = ""
            self.FrontCarConnectorSize = 0.0
            self.RearCarConnectorSize = 0.0
            self.WheelModelPrefix = ""
            self.WheelRadiusMeters = 0.0
            self.CarWidthMeters = 0.0
            self.CarHeightMeters = 0.0
            self.WheelCircumferenceMeters = 0.0
            self.UseAnimationForWheelMovement = False
            self.OrderInBuildMenu = 0.0
            self.DefaultColor = None
            self.SideViewIconIsCustom = False
            from Mafi import Option
            self.ExhaustParticlesSpec = Option()
            self.MotionSoundSpec = None
            self.BrakingSoundSpec = None
            self.StoppedSoundSpec = None
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0

        class WagonProductsData:
            def __init__(self):
                self.MaxProductRenderCapacity = 0
                self.ProductRenderOffsets = None
                self.ShelfName = ""
                self.Shelf2Name = ""

class DebugGameRendererTrains:
    COLOR_TRACK_BASE = None
    COLOR_TRACK_IN_SB = None
    COLOR_TRACK_CRITICAL = None
    COLOR_TRACK_RESERVED = None
    COLOR_TRACK_RESERVED_SB = None
    COLOR_TRACK_OCCUPIED = None
    COLOR_TRACK_BLOCKED = None
    TIE_WIDTH = None
    TIE_LENGTH = None
    def __init__(self):
        pass


class IEntityWithTrainTrackBaseProto:
    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.IsStraight = False
        self.IsElevated = False
        self.CanBeElevatedOnSupports = False
        self.TrackGraphics = None
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.ElectrificationType = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None
        self.Upgrade = None
        self.TierData = None
        self.IconPath = ""

class IEntityWithTrainTrackBaseProtoExtensions:
    def __init__(self):
        pass


class ITrainTrackGfx:
    def __init__(self):
        self.VisualStylePrefabsLods = None
        self.Ties = None
        self.PrefabRotation = None

class TrackVisualStylePrefabs:
    def __init__(self):
        self.HasTies = False
        self.PixelsPerMeter = 0.0
        self.TrajectoryExtrusionStep = 0
        from Mafi import Option
        self.RailCrossSectionPrefab = Option()
        self.TrackBaseCrossSectionPrefab = Option()
        self.TrackBase2CrossSectionPrefab = Option()
        self.TrackLipCrossSectionPrefab = Option()
        self.TrackBaseEndPrefab = Option()
        self.TrackLipEndPrefab = Option()
        self.TiePrefabs = None
        self.TieLipPrefabs = None

class EntityWithTrainTrackBaseProto:
    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.ElectrificationType = None
        self.IsStraight = False
        self.IsElevated = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.VisualStylePrefabsLods = None
            self.PrefabRotation = None
            self.Ties = None
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class TrainTrackProtoHelper:
    def __init__(self):
        self.WaypointsPerBlock = 0
        self.TransformedTrajectoriesCache = None
        self.TransformedWaypointsCache = None
        self.TransformedPillarLocationsCache = None
        self.TransformedPoleLocationsCache = None
        self.TransformedClosePoleLocationsCache = None
        self.TransformedStartEndPoleLocationsCache = None
        self.TransformedBlocksDataCache = None
        self.WaypointBlocksData = None

class TrainTrackWaypointBlockData:
    def __init__(self):
        self.WaypointStartIndex = None
        self.WaypointsCount = None

class TrainTrackBlockDataRel:
    def __init__(self):
        self.BoundingBoxStartLeft = None
        self.BoundingBoxStartRight = None
        self.BoundingBoxEndLeft = None
        self.BoundingBoxEndRight = None
        self.CentreHeight = None

class TrainTrackStartEndPoleInfos:
    def __init__(self):
        self.Length = 0
        self.Item = None
        self.StartPoleA = None
        self.StartPoleB = None
        self.EndPoleA = None
        self.EndPoleB = None

class ElectrificationType:
    None = None
    WithPoles = None
    NoPoles = None
    def __init__(self):
        self.value__ = 0

class PoleGenerationMode:
    None = None
    All = None
    OnlyOnRightSide = None
    def __init__(self):
        self.value__ = 0

class IEntityWithTrainTrack:
    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None

class INotifyTrainApproachingEntity:
    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None

class IEntityWithTrainTrackExtensions:
    def __init__(self):
        pass


class IEntityWithTrainTrackFriend:
    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None

class TrainTrackTrajectoryDirection:
    Unknown = None
    Forward = None
    Backward = None
    Bidirectional = None
    def __init__(self):
        self.value__ = None

class TrainTrackTrajectoryDirectionExtensions:
    def __init__(self):
        pass


class ITrainDepot:
    def __init__(self):
        self.InternalTracks = None
        self.DepartingInternalBlockId = None
        self.MaxTrainLength = None
        self.ConstructedTrainsWaitingForReplacement = None
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None

class ITrainStationProto:
    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.IsStraight = False
        self.IsElevated = False
        self.CanBeElevatedOnSupports = False
        self.TrackGraphics = None
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.ElectrificationType = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None
        self.Upgrade = None
        self.TierData = None
        self.IconPath = ""

class ITrainTrackPathFinder:
    def __init__(self):
        self.Options = None
        self.Start = None
        self.Goal = None
        self.NodesProcessed = 0
        self.NodesInHeap = 0
        self.InvalidNodes = 0

class TrainTrackPathFinderFlags:
    None = None
    IgnoreCollisions = None
    GoalMustBeFlat = None
    Precomputation = None
    AllowNonExactPointMatch = None
    AllowOnlyStraight = None
    DisallowR14 = None
    DisallowR22 = None
    DisallowR30 = None
    DisallowG4 = None
    DisallowG8 = None
    AlternativeMode = None
    Electrified = None
    TryToConnectToGround = None
    PrecomputationExitToGround = None
    UnguidedSearch = None
    def __init__(self):
        self.value__ = 0

class ITrainTracksGraphPiece:
    def __init__(self):
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.OccupiedTiles = None

class ITrainTracksGraphPieceExtensions:
    def __init__(self):
        pass


class ITrainTracksPolesManager:
    def __init__(self):
        pass


class LevelCrossingProto:
    def __init__(self):
        self.EntityType = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.ElectrificationType = None
        self.IsStraight = False
        self.IsElevated = False
        self.IgnoreMissingSupport = False
        from Mafi import Option
        self.ElevationFlippedProto = Option()
        self.UseTerrainHeightForVehicles = False
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.MaxVehicleSpeedPerTick = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.AnimationParams = None
        self.RoadTotalWidth = None
        self.EmissionIntensity = None
        self.Upgrade = None
        self.TierData = None
        self.Graphics = None
        self.TrackGraphics = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.CloseTrainArrivalDistance = None
        self.CloseTrainArrivalDuration = None
        self.AnimationChangePerStep = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        def __init__(self):
            self.VisualStylePrefabsLods = None
            self.Ties = None
            self.PrefabRotation = None
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class LevelCrossingEntranceProto:
    def __init__(self):
        self.EntityType = None
        self.Graphics = None
        self.TierData = None
        self.MaxVehicleSpeedPerTick = None
        self.UseTerrainHeightForVehicles = False
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.RoadTotalWidth = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TerrainConnections = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.SoundPrefabPath = ""
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class LocomotiveProto:
    def __init__(self):
        self.EntityType = None
        from Mafi import Option
        self.FuelTankProto = Option()
        self.LocomotiveFuelTankProto = Option()
        self.PowerRequired = None
        self.IconPath = ""
        self.BogiePivotsDistance = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.RequiresAlignment = False
        self.EnginePowerKw = None
        from Mafi import Fix32
        self.StartingTractiveEffort = Fix32()
        self.IgnoreFuelCostDuringConstruction = False
        self.OnlyRefuelIfUnder = None
        self.Graphics = None
        self.CarLength = None
        self.BogiePivotsOffset = None
        self.SubCarCount = 0
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.BogiePivotToCarEnd = None
        self.BogieWheelBase = None
        self.MaxSpeed = None
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.BrakingForceKn = Fix32()
        self.RollingResistanceCoefficientTimesThousand = Fix32()
        self.FrontalAreaM2 = Fix32()
        self.LengthDragAsExtraFrontalArea = Fix32()
        self.DragCoefficientStandalone = Fix32()
        self.DragCoefficientInline = Fix32()
        self.OnlyAllowedAtFront = None
        self.OnlyAllowedAtRear = None
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.SideViewIconPath = ""
            self.IconPath = ""
            self.EngineIdleSoundSpec = None
            self.EngineMovingSoundSpec = None
            self.HornSoundSpec = None
            self.UseAnimationForEngineThrottle = False
            self.ThrottleAnimationSpeedIdle = 0.0
            self.ThrottleAnimationSpeedFullPower = 0.0
            self.FallbackFuelIconPath = ""
            self.AlterLocoAtAnimPercent = None
            self.PrefabPath = ""
            self.FrontBogieModelName = ""
            self.RearBogieModelName = ""
            self.FrontCouplerName = ""
            self.RearCouplerName = ""
            self.FrontCarConnectorName = ""
            self.RearCarConnectorName = ""
            self.FrontCarConnectorSize = 0.0
            self.RearCarConnectorSize = 0.0
            self.WheelModelPrefix = ""
            self.WheelRadiusMeters = 0.0
            self.CarWidthMeters = 0.0
            self.CarHeightMeters = 0.0
            self.WheelCircumferenceMeters = 0.0
            self.UseAnimationForWheelMovement = False
            self.OrderInBuildMenu = 0.0
            self.DefaultColor = None
            self.SideViewIconIsCustom = False
            from Mafi import Option
            self.ExhaustParticlesSpec = Option()
            self.MotionSoundSpec = None
            self.BrakingSoundSpec = None
            self.StoppedSoundSpec = None
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0

class LocoSoundSpecs:
    def __init__(self):
        self.MotionSoundSpec = None
        self.BrakingSoundSpec = None
        self.StoppedSoundSpec = None
        self.EngineIdleSoundSpec = None
        self.EngineMovingSoundSpec = None
        self.HornSoundSpec = None

class LocomotiveFuelTankProto:
    def __init__(self):
        self.PrimaryProduct = None
        from Mafi.Core.Prototypes import Proto
        self.Id = Proto.ID()

        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.PrimaryProductAmount = None
        self.SecondaryProductAmount = None
        from Mafi import Option
        self.SecondaryProduct = Option()
        self.Product = None
        self.WasteProduct = Option()
        self.PollutionPercent = None
        self.Capacity = None
        self.Duration = None
        self.ReserveDuration = None
        self.IdleFuelConsumption = None
        self.OneQuantityDuration = None
        self.OneQuantityPollution = None
        self.QuickRefuelCostPerQuantity = None
        self.QuickRefuelHandlingCost = None
        self.IsPhantom = False

class TenderWagonProto:
    def __init__(self):
        self.EntityType = None
        from Mafi import Option
        self.FuelTankProto = Option()
        self.LocomotiveFuelTankProto = Option()
        self.PowerRequired = None
        self.IconPath = ""
        self.BogiePivotsDistance = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.RequiresAlignment = False
        self.EnginePowerKw = None
        from Mafi import Fix32
        self.StartingTractiveEffort = Fix32()
        self.IgnoreFuelCostDuringConstruction = False
        self.OnlyRefuelIfUnder = None
        self.CarLength = None
        self.BogiePivotsOffset = None
        self.SubCarCount = 0
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.BogiePivotToCarEnd = None
        self.BogieWheelBase = None
        self.MaxSpeed = None
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.BrakingForceKn = Fix32()
        self.RollingResistanceCoefficientTimesThousand = Fix32()
        self.FrontalAreaM2 = Fix32()
        self.LengthDragAsExtraFrontalArea = Fix32()
        self.DragCoefficientStandalone = Fix32()
        self.DragCoefficientInline = Fix32()
        self.OnlyAllowedAtFront = None
        self.OnlyAllowedAtRear = None
        self.IsPhantom = False

    class Gfx:
        def __init__(self):
            self.SideViewIconPath = ""
            self.IconPath = ""
            self.PileObjectPath = ""
            self.AnimationStateName = ""
            self.PileTextureParams = None
            self.EngineIdleSoundSpec = None
            self.EngineMovingSoundSpec = None
            self.HornSoundSpec = None
            self.UseAnimationForEngineThrottle = False
            self.ThrottleAnimationSpeedIdle = 0.0
            self.ThrottleAnimationSpeedFullPower = 0.0
            self.FallbackFuelIconPath = ""
            self.AlterLocoAtAnimPercent = None
            self.PrefabPath = ""
            self.FrontBogieModelName = ""
            self.RearBogieModelName = ""
            self.FrontCouplerName = ""
            self.RearCouplerName = ""
            self.FrontCarConnectorName = ""
            self.RearCarConnectorName = ""
            self.FrontCarConnectorSize = 0.0
            self.RearCarConnectorSize = 0.0
            self.WheelModelPrefix = ""
            self.WheelRadiusMeters = 0.0
            self.CarWidthMeters = 0.0
            self.CarHeightMeters = 0.0
            self.WheelCircumferenceMeters = 0.0
            self.UseAnimationForWheelMovement = False
            self.OrderInBuildMenu = 0.0
            self.DefaultColor = None
            self.SideViewIconIsCustom = False
            from Mafi import Option
            self.ExhaustParticlesSpec = Option()
            self.MotionSoundSpec = None
            self.BrakingSoundSpec = None
            self.StoppedSoundSpec = None
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0

class ITrain:
    def __init__(self):
        self.Name = ""
        self.TrainId = None

class TrainDrivingMode:
    None = None
    FollowingSchedule = None
    DrivingToExplicitGoal = None
    PlayerIsDriving = None
    DrivingToScrap = None
    DrivingToReplace = None
    def __init__(self):
        self.value__ = 0

class TrainStateForUi:
    Unknown = None
    Paused = None
    Driving = None
    NoLineSet = None
    LineHasNoStations = None
    NoValidGoals = None
    WaitingForFreeTrack = None
    LoadingOrUnloading = None
    Arriving = None
    Departing = None
    NoPower = None
    WaitingForDepotDoors = None
    CannotFindPath = None
    WaitingForSuperBlock = None
    WaitingForBidirectionalSuperBlock = None
    ArrivalConditionsNotMet = None
    SelfIntersect = None
    AtOnlyStationOnLine = None
    DrivingToDepot = None
    def __init__(self):
        self.value__ = 0

class ITrainFriend:
    def __init__(self):
        self.IsSpawned = False
        self.Name = ""
        self.TrainId = None
        self.TrainCarsColorOverride = None
        from Mafi import Option
        self.TrainLine = Option()
        self.CurrentScheduleItem = Option()

class TrainCarBaseProto:
    T1_CAR_LENGTH = None
    T2_CAR_LENGTH = None
    T1_CAR_WIDTH_METERS = 0.0
    T2_CAR_WIDTH_METERS = 0.0
    T1_CAR_HEIGHT_METERS = 0.0
    T2_CAR_HEIGHT_METERS = 0.0
    WHEEL_RADIUS = None
    def __init__(self):
        self.IconPath = ""
        self.BogiePivotsDistance = None
        from Mafi.Core.Entities.Dynamic import DynamicEntityProto
        self.Id = DynamicEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.CarLength = None
        self.BogiePivotsOffset = None
        self.SubCarCount = 0
        self.BuildDurationPerProduct = None
        self.BuildExtraDuration = None
        self.BogiePivotToCarEnd = None
        self.BogieWheelBase = None
        self.MaxSpeed = None
        from Mafi import Fix32
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.BrakingForceKn = Fix32()
        self.RollingResistanceCoefficientTimesThousand = Fix32()
        self.FrontalAreaM2 = Fix32()
        self.LengthDragAsExtraFrontalArea = Fix32()
        self.DragCoefficientStandalone = Fix32()
        self.DragCoefficientInline = Fix32()
        self.OnlyAllowedAtFront = None
        self.OnlyAllowedAtRear = None
        self.Graphics = None
        self.IsPhantom = False

    class Gfx:
        Empty = None
        FRONT_COUPLER_DEFAULT_NAME = ""
        REAR_COUPLER_DEFAULT_NAME = ""
        WHEEL_DEFAULT_MODEL_PREFIX = ""
        def __init__(self):
            self.SideViewIconPath = ""
            self.IconPath = ""
            self.PrefabPath = ""
            self.FrontBogieModelName = ""
            self.RearBogieModelName = ""
            self.FrontCouplerName = ""
            self.RearCouplerName = ""
            self.FrontCarConnectorName = ""
            self.RearCarConnectorName = ""
            self.FrontCarConnectorSize = 0.0
            self.RearCarConnectorSize = 0.0
            self.WheelModelPrefix = ""
            self.WheelRadiusMeters = 0.0
            self.CarWidthMeters = 0.0
            self.CarHeightMeters = 0.0
            self.WheelCircumferenceMeters = 0.0
            self.UseAnimationForWheelMovement = False
            self.OrderInBuildMenu = 0.0
            self.DefaultColor = None
            self.SideViewIconIsCustom = False
            from Mafi import Option
            self.ExhaustParticlesSpec = Option()
            self.MotionSoundSpec = None
            self.BrakingSoundSpec = None
            self.StoppedSoundSpec = None
            self.IconIsCustom = False
            self.Color = None
            self.RendererIndex = 0

class TrainCarSoundSpecs:
    def __init__(self):
        self.MotionSoundSpec = None
        self.BrakingSoundSpec = None
        self.StoppedSoundSpec = None

class ITrainDepotExtensionParent:
    def __init__(self):
        self.ExtensionOffset = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class ITrainDepotExtensionParentProto:
    def __init__(self):
        self.ExtensionOffset = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None

class TrainDepotExtensionProto:
    def __init__(self):
        self.EntityType = None
        self.TierData = None
        self.ExtensionOffset = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.MaxStoredOfEachCarType = 0
        self.TrainLengthLimit = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class TrainDepotProto:
    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.ElectrificationType = None
        self.ExtensionOffset = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.IsStraight = False
        self.IsElevated = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
        self.MaxStoredOfEachCarType = 0
        self.NumServiceLanes = 0
        self.TrainLengthLimit = None
        self.DoorOpenDuration = None
        self.ArrivingExternalBlockId = None
        self.ArrivingInternalBlockId = None
        self.CenterBlockId = None
        self.DepartingInternalBlockId = None
        self.DepartingExternalBlockId = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class ITrainLineMember:
    def __init__(self):
        from Mafi import Option
        self.TrainLine = Option()
        self.CurrentScheduleItem = Option()
        self.Name = ""
        self.TrainId = None

class ITrainLineMemberFriend:
    def __init__(self):
        self.TrainCarsColorOverride = None
        from Mafi import Option
        self.TrainLine = Option()
        self.CurrentScheduleItem = Option()
        self.Name = ""
        self.TrainId = None

class TrainLineWarning:
    None = None
    NoStops = None
    DestroyedStations = None
    OnlyOneStop = None
    OnlyFuelStops = None
    OnlyOneStopNotRefuel = None
    def __init__(self):
        self.value__ = 0

class ITrainPathFindingTask:
    def __init__(self):
        self.Train = None
        self.IsEnqueuedOrBeingProcessed = False
        self.Status = None
        self.LastPfResult = None
        self.PathView = None
        self.StartNodes = None
        self.StartNodesMetadata = None
        self.GoalNodes = None

class ITrainPfTaskManaged:
    def __init__(self):
        self.ForwardMaxSpeed = None
        self.BackwardMaxSpeed = None
        self.MaxDistanceForOccupancyCostPenalties = None
        self.RequireElectrified = False
        self.Train = None
        self.IsEnqueuedOrBeingProcessed = False
        self.Status = None
        self.LastPfResult = None
        self.PathView = None
        self.StartNodes = None
        self.StartNodesMetadata = None
        self.GoalNodes = None

class TrainPathFindingTaskStatus:
    Unknown = None
    Initialized = None
    WaitingInPfQueue = None
    PathFinding = None
    Done = None
    def __init__(self):
        self.value__ = 0

class ITrainScheduleDepartCondition:
    def __init__(self):
        self.CombineAsOrInsteadOfAnd = False
        self.LastEvalResult = False

class TrainScheduleConditionsCommandsProcessor:
    def __init__(self):
        pass


class ITrainScheduleItem:
    def __init__(self):
        self.IndexInSchedule = 0
        self.StationRoots = None
        self.DepartConditions = None
        self.StationPriorities = None
        self.LoadOnlyProducts = None
        self.UnloadOnlyProducts = None
        self.DisableLoad = False
        self.DisableUnload = False
        self.SkipIfFuelHigherThan = None

class TrainEntityModuleLimits:
    Unrestricted = None
    AnyCompatibleStationModulesEmpty = None
    AllCompatibleStationModulesEmpty = None
    AnyCompatibleStationModulesFull = None
    AllCompatibleStationModulesFull = None
    def __init__(self):
        self.value__ = 0

class TrainEntityModuleLimitsExtensions:
    def __init__(self):
        pass


class TrainScheduleItemsCommandsProcessor:
    def __init__(self):
        pass


class PreferredTrainDirection:
    Straight = None
    Left = None
    Right = None
    Random = None
    def __init__(self):
        self.value__ = 0

class ITrainsPathFinder:
    def __init__(self):
        self.CurrentPfId = 0

class TrainsPathFinder:
    def __init__(self):
        self.CurrentPfId = 0

class TrainsPathFinderResult:
    Unknown = None
    InvalidState = None
    StillSearching = None
    PathFound = None
    PathWasInvalid = None
    PathDoesNotExist = None
    NoValidStart = None
    NoValidGoal = None
    ExceptionWasThrown = None
    def __init__(self):
        self.value__ = 0

class TrainsPathFinderConfig:
    def __init__(self):
        self.MaxDistanceForOccupancyCostPenalties = None
        from Mafi import Fix32
        self.PositiveGradeCostMult = Fix32()
        self.GradeFactorExponent = Fix32()
        self.GradeCostHeuristic = Fix32()
        self.ModuleCostPerTile = Fix32()
        self.BlockedCostPerTile = Fix32()
        self.BlockedCostPerTileMovingFastMultiplier = Fix32()
        self.CriticalCostPerTile = Fix32()
        self.SpeedCostPerTilePerTickLost = Fix32()

class TrainStaticData:
    TIME_TO_TRAVEL_TILES = None
    MAX_SPEED_ESTIMATION_MAX_DURATION = None
    MIN_TRAIN_SPEED = None
    PUSH_PULL_WAGONS_MULT = None
    PUSHED_WAGON_POWER_PENALTY = None
    ACCELERATION_FUN_FACTOR = None
    Empty = None
    def __init__(self):
        self.SlopeDifficultyMultiplier = None
        self.FuelConsumptionPer60 = None
        self.WasteProducedPer60 = None
        self.MaintenancePer60 = None
        self.Workers = 0
        self.CapacityPerProductType = None
        self.ForcesAtSpeeds = None
        self.DataVersion = 0
        self.AllLocosElectrified = False
        self.TrainCars = None
        self.TrainSubCarsCount = 0
        self.Locomotives = None
        self.CargoWagons = None
        self.TotalLength = None
        from Mafi import Fix32
        self.MassTonsWhenEmpty = Fix32()
        self.MassTonsWhenFull = Fix32()
        self.StartingTractiveEffortKn = Fix32()
        self.MaxBrakingForceKn = Fix32()
        self.RollDragKnWhenEmpty = Fix32()
        self.RollDragKnWhenFull = Fix32()
        self.AirDragCoefficient = Fix32()
        self.AirDragMultiplier = Fix32()
        self.MaxSpeedBasedOnConstruction = None
        self.MaxForwardsSpeedCombined = None
        self.MaxBackwardSpeedCombined = None
        self.MaxSpeedAtGrade0 = None
        self.MaxSpeedAtGrade12 = None
        self.MaxSpeedAtGrade25 = None
        self.MaxSpeedAtGrade0Backwards = None
        self.MaxSpeedAtGrade0Unrestricted = None
        self.TimeToTravelNTilesAtGrade0 = None
        self.TimeToTravelNTilesAtGrade12 = None
        self.TimeToTravelNTilesAtGrade25 = None
        self.SpeedAtNTilesAtGrade0 = None
        self.SpeedAtNTilesAtGrade12 = None
        self.SpeedAtNTilesAtGrade25 = None
        self.TimeToTravelNTilesAtGrade0Backwards = None
        self.SpeedAtNTilesAtGrade0Backwards = None
        self.LocosMaxPowerFactorsForwards = None
        self.LocosMaxPowerFactorsBackwards = None
        self.TotalMaxPower = None
        self.TotalMaxPowerForwards = None
        self.TotalMaxPowerBackwards = None

class TrainForcesAtSpeeds:
    def __init__(self):
        self.Speed = None
        self.TractiveEffortKn = None
        self.AirDragKn = None
        self.ForcesAtGrade0 = None
        self.ForcesAtGrade12 = None
        self.ForcesAtGrade25 = None

class TrainForcesAtGrade:
    def __init__(self):
        self.MaxSpeedWhenEmpty = None
        self.MaxSpeedWhenFull = None
        self.TotalDragKnWhenEmpty = None
        self.TotalDragKnWhenFull = None
        self.BrakingDistanceWhenEmpty = None
        self.BrakingDistanceWhenFull = None

class TrainStationAlignmentStatus:
    None = None
    Aligning = None
    Aligned = None
    Complete = None
    Skipped = None
    def __init__(self):
        self.value__ = 0

class ITrainStationBase:
    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.CustomTitle = Option()

class ITrainStationRoot:
    def __init__(self):
        self.ModuleLimits = None
        self.TrainLimit = 0
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.CustomTitle = Option()

class ITrainStationModule:
    def __init__(self):
        self.ProductType = None
        self.IsForLoading = False
        self.CanReleaseWagon = False
        self.IsFull = False
        self.IsEmpty = False
        from Mafi import Option
        self.StoredProduct = Option()
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.CustomTitle = Option()

class ITrainStationModuleFriend:
    def __init__(self):
        self.ProductType = None
        self.IsForLoading = False
        self.CanReleaseWagon = False
        self.IsFull = False
        self.IsEmpty = False
        from Mafi import Option
        self.StoredProduct = Option()
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.CustomTitle = Option()

class ITrainStationFuel:
    def __init__(self):
        self.CanReleaseAlignedLocomotive = False
        self.LoadPercent = None
        self.RequiresAlignment = False
        from Mafi import Option
        self.PrimaryProduct = Option()
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.CustomTitle = Option()

class TrainStationBaseProto:
    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.ElectrificationType = None
        self.IsStraight = False
        self.IsElevated = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
        self.PowerConsumption = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class TrainStationModuleBaseProto:
    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.ElectrificationType = None
        self.IsStraight = False
        self.IsElevated = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
        self.ProductType = None
        self.PowerConsumption = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class TrainStationRootBaseProto:
    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.ElectrificationType = None
        self.IsStraight = False
        self.IsElevated = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
        self.PowerConsumption = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class TrainStationValidator:
    def __init__(self):
        self.Priority = None

class TrainStationWaypointBaseProto:
    def __init__(self):
        from Mafi import Option
        self.ElevationFlippedProto = Option()
        self.IsElevated = False
        self.SmallerInterTrackLayout = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.ElectrificationType = None
        self.IsStraight = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class ITrainTrackMayBeElevatedFriend:
    def __init__(self):
        self.TrackProto = None
        self.PillarBlocksBitmap = None
        self.Pillars = None
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None

class IDiagonalTrainTrack:
    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Poles = None
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.VehicleSurfaceHeights = None
        self.ConstructionState = None
        from Mafi import Option
        self.ConstructionProgress = Option()
        self.IsConstructed = False
        self.PfTargetTiles = None
        self.AlwaysUseCustomPfTargetTiles = False
        self.AreConstructionCubesDisabled = False
        self.DoNotAdjustTerrainDuringConstruction = False
        self.Position2f = None
        self.Position3f = None
        self.RendererData = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None
        self.UpgradableProto = None
        self.TrackProto = None
        self.IsTrackDestroyed = False
        self.IsTrackEnabled = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None

class TrainTrackRadius:
    Inf = None
    R14 = None
    R22 = None
    R30 = None
    def __init__(self):
        self.value__ = 0

class TrainTrackGradeFactor:
    G0 = None
    G4 = None
    G8 = None
    G16 = None
    G24 = None
    GMinus4 = None
    GMinus8 = None
    GMinus16 = None
    GMinus24 = None
    def __init__(self):
        self.value__ = None

class TrainTrackGradeFactorExtensions:
    def __init__(self):
        pass


class TrainTrackBuilder:
    def __init__(self):
        pass


class TrainTrackConstants:
    POLE_HEIGHT = None
    GAUGE = None
    LAYOUT_WIDTH = None
    TRAIN_OCCUPANCY_WIDTH = None
    LAYOUT_HEIGHT = None
    LAYOUT_HEIGHT_ELECTRIC = None
    TRACK_RIDE_HEIGHT = None
    TRACK_CABLE_HEIGHT = None
    POLE_DISTANCE = None
    POLE_DISTANCE_CLOSE = None
    TIE_SPACING = None
    TRACK_RAIL_WIDTH = None
    TRACK_LIP_WIDTH = None
    MAX_BLOCKS_PER_TRACK = 0
    WAYPOINTS_COUNT_PER_BLOCK = 0
    MAX_POLES_PER_TRACK = 0
    from Mafi import Fix32
    WAYPOINTS_PER_TILE = Fix32()
    WAYPOINT_SPACING = None
    HALF_WAYPOINT_SPACING = None
    START_SLOPE_CTRL_DIST = None
    END_SLOPE_CTRL_DIST = None
    GROUND_TOLERANCE = None
    PILLAR_SUPPORT_DISTANCE = None
    POLE_SUPPORT_DISTANCE = None
    PILLAR_EXTENTS_ALONG_TRACK = None
    PILLAR_EXTENTS_ACROSS_TRACK = None
    PILLAR_SIZE_ALONG_TRACK = None
    PILLAR_SIZE_ACROSS_TRACK = None
    PILLAR_COLLIDER_SIZE_ALONG_TRACK = None
    PILLAR_COLLIDER_SIZE_ACROSS_TRACK = None
    SUPPORT_NOT_COMPUTED_VAL = None
    STD_GRADE_FACTOR = 0
    STD_GRADE_FACTOR_2X = 0
    STD_GRADE_FACTOR_3X = 0
    STD_GRADE_FACTOR_4X = 0
    STD_GRADE_1X_APPROX = 0
    STD_GRADE_2X_APPROX = 0
    STD_GRADE_3X_APPROX = 0
    STD_GRADE_4X_APPROX = 0
    def __init__(self):
        pass


class TrainTrackPathFinder:
    PRECOMPUTED_DATA_FILE_HEADER_SIZE = 0
    PRECOMPUTED_DATA_FILE_EXTENSION = ""
    MAX_SEARCH_RANGE = 0
    PRECOMPUTED_HEURISTICS_RANGE = 0
    PRECOMPUTED_HEURISTICS_HEIGHT = 0
    PRECOMPUTED_HEURISTICS_DIR_NAME = ""
    MAX_PRECOMPUTED_HEURISTICS_KEY = 0
    def __init__(self):
        self.ProcessedNodes = None
        self.Options = None
        self.Start = None
        self.Goal = None
        self.NodesProcessed = 0
        self.NodesInHeap = 0
        self.InvalidNodes = 0

    class PieceInfo:
        def __init__(self):
            self.Proto = None
            self.AllPiecesIndex = None
            self.Transform = None
            self.Trajectory = None
            self.IsTrajectoryReversed = False
            self.AngleTraversed = None
            self.EndOffsetFromStart = None
            self.OccupiedTilesRelative = None
            self.OccupiedVerticesRelative = None
            self.MaxGradient = 0
            from Mafi import Fix32
            self.Cost = Fix32()
            self.BoundingBox = None
            self.UnpenalizedCost = Fix32()

    class NodeConstraints:
        def __init__(self):
            self.AngleSinceStart = None
            self.NumDirectionChanges = None

    class Node:
        def __init__(self):
            self.EndOffset = None
            self.Piece = None
            self.StartOffset = None
            self.PieceIndex = 0
            from Mafi import Fix32
            self.CurrentCost = Fix32()
            self.ParentNodeIndex = 0
            self.UnsupportedSpanAtEnd = None
            self.UnsupportedSpanAtLastBlock = None
            self.NodeConstraints = None

class TrainTrackPathFinderPrecomputation:
    def __init__(self):
        pass


class ITrainTrackPillar:
    def __init__(self):
        self.TrainTrackEntityId = None
        self.BlockIndex = 0

class TrainTrackPillarAddRequest:
    Instance = None
    def __init__(self):
        self.ReasonToAdd = None

class TrainTrackPillarEntityValidator:
    def __init__(self):
        self.Priority = None

class TrainTrackPillarRendererData:
    def __init__(self):
        self.IsValid = False
        self.ChunkIndex = None
        self.PartsIds = None

class TrainTrackPillarProto:
    MAX_PILLAR_HEIGHT = None
    OCCUPANCY_MASK_SIZE = None
    OCCUPANCY_MASK_CENTRE = None
    def __init__(self):
        self.EntityType = None
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.Graphics = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.BasePrefabPath = ""
            self.TowerPrefabPath = ""
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class TrainTrackPillarsBuilder:
    def __init__(self):
        self.PillarProto = None

class ITrainTrackPole:
    def __init__(self):
        self.TrainTrackEntityId = None
        self.BlockIndex = 0

class TrainTrackPoleAddRequest:
    Instance = None
    def __init__(self):
        self.ReasonToAdd = None

class TrainTrackPoleRendererData:
    def __init__(self):
        self.IsValid = False
        self.ChunkIndex = None
        self.PartsIds = None

class TrainTrackPoleProto:
    def __init__(self):
        self.EntityType = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

    class Gfx:
        Empty = None
        def __init__(self):
            self.PrefabPath = ""
            self.PrefabOrigin = None
            self.IconPath = ""
            self.YawForGeneratedIcon = None
            self.VisualizedLayers = None
            self.Categories = None
            self.AnimationDataAssetPathBase = ""
            self.BasePrefabPath = ""
            self.TowerPrefabPath = ""
            self.IconIsCustom = False
            self.UseInstancedRendering = False
            self.UseSemiInstancedRendering = False
            self.SemiInstancedRenderingExcludedObjects = None
            self.MaxRenderedLod = 0
            self.DisableEmptyChildrenStripping = False
            self.InstancedRendererIndex = None
            self.AnimatedGameObjects = None
            self.AnimationLength = 0.0
            self.RemoveUndergroundVertices = False
            self.HideBlockedPortsIcon = False
            self.Color = None
            self.RendererIndex = 0

class TrainTrackPolesFactory:
    def __init__(self):
        from Mafi import Option
        self.PoleProto = Option()

class ITrainTrackWithSmallerInterTrackLayoutProto:
    def __init__(self):
        self.SmallerInterTrackLayout = None
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.IsStraight = False
        self.IsElevated = False
        self.CanBeElevatedOnSupports = False
        self.TrackGraphics = None
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.ElectrificationType = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None
        self.Upgrade = None
        self.TierData = None
        self.IconPath = ""

class ITrainTrackMayBeElevatedProto:
    def __init__(self):
        from Mafi import Option
        self.ElevationFlippedProto = Option()
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.IsStraight = False
        self.IsElevated = False
        self.CanBeElevatedOnSupports = False
        self.TrackGraphics = None
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.ElectrificationType = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.Graphics = None
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsInitialized = False
        self.IsObsolete = False
        self.Mod = None
        self.Upgrade = None
        self.TierData = None
        self.IconPath = ""

class TrainTrackProto:
    def __init__(self):
        self.EntityType = None
        self.IsElevated = False
        self.IsStraight = False
        self.ElectrificationType = None
        from Mafi import Option
        self.StationWaypointProto = Option()
        self.ElevationFlippedProto = Option()
        self.TrajectoryLength = None
        self.BlocksCount = 0
        self.CanBeElevatedOnSupports = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Upgrade = None
        self.TierData = None
        self.IgnoreMissingSupport = False
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = ""
        self.CanMoveUpDownWhenInvalidPlacement = False
        self.IsUnique = False
        self.CloningDisabled = False
        from Mafi.Core.Entities.Static import StaticEntityProto
        self.Id = StaticEntityProto.ID()

        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsLocked = False
        self.IsUnlocked = False
        self.IsUnlockedAndAvailable = False
        self.IsLockedOrUnavailable = False
        self.IsLockedButAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
        self.HasElevationChange = False
        self.IgnoreInPathFinder = False
        self.CurveRadius = None
        self.BoostCost = None
        self.InputPorts = None
        self.OutputPorts = None
        self.ConstructionDurationPerProduct = None
        self.CollapseRubbleScale = None
        self.CustomBuriedTolerance = None
        self.CustomSuspendedTolerance = None
        self.VehicleGoalHeightAllowedRange = None
        self.CannotBeBuiltByPlayer = False
        self.CannotBeDestroyedByFlood = False
        self.DoNotStartConstructionAutomatically = False
        self.IsPhantom = False

class TrainTrackSegmentsRel:
    def __init__(self):
        self.Length = None
        self.Positions = None
        self.DirectionsNormalized = None
        self.SegmentLengthsPrefixSums = None
        self.StartDirection = None
        self.EndDirection = None

class TrainTrackTiesGfx:
    EMPTY = None
    def __init__(self):
        self.Positions = None
        self.DirectionsNormalized = None

class TrainTrackWaypoint:
    def __init__(self):
        self.Position = None
        self.Rotation = None

class TrainTrackWaypointRel:
    def __init__(self):
        self.Position = None
        self.Rotation = None

class TrackOverlapStatus:
    None = None
    Left = None
    Right = None
    Both = None
    def __init__(self):
        self.value__ = 0

class TrainGraphEdge:
    def __init__(self):
        self.Start = None
        self.End = None

class TrainTrackBlockIdWithDirection:
    def __init__(self):
        self.BlockId = None
        self.IsInReverse = False

class TrainGraphEdgeInfo:
    def __init__(self):
        self.Entity = None
        self.EntityDataIndex = 0
        self.StartNodeId = 0
        self.EndNodeId = 0
        self.IsBackwards = False

class TrainTrackGraphNodeKey:
    def __init__(self):
        self.Position = None
        self.Direction = None
        self.Data = None

class TrainGraphNodeWithEdge:
    def __init__(self):
        self.EndNodeId = None
        self.EdgeId = None

class TrainTrackOccupancyData:
    def __init__(self):
        self.ReservedBlocksCount = None
        self.BlockedEntriesCount = None

class TrainTrackState:
    Free = None
    Reserved = None
    Occupied = None
    Blocked = None
    def __init__(self):
        self.value__ = 0

class TrainBlockState:
    Free = None
    Reserved = None
    ClaimedBySuperBlock = None
    Occupied = None
    Blocked = None
    def __init__(self):
        self.value__ = 0

class ITrainTrackManagedEntity:
    def __init__(self):
        pass


class TrainTrackGraphNodeKeyAndCrossSectionData:
    def __init__(self):
        self.Key = None
        self.Prefabs = None

class TrainTrackTrajectoryData:
    def __init__(self):
        self.TrajectoryCurve = None
        self.CurveRightOffset = None
        self.HeightCurve = None
        self.Segments = None
        self.Waypoints = None
        self.StartDirection = None
        self.EndDirection = None
        self.HalfWaypointCount = 0
