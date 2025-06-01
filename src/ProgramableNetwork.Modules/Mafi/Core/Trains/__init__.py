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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class CarAndStationIndex:

    def __init__(self):
        pass

class CargoWagon:

    def __init__(self):
        self.Prototype = None
        self.Cargo = None
        from Mafi import Option
        self.OnlyAllowedProduct = Option()
        self.Capacity = None
        self.UsableCapacity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        from Mafi import Option
        self.AlignedStation = Option()
        self.Maintenance = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CanBePaused = False
        from Mafi import Option
        self.Train = Option()
        from Mafi import Option
        self.CustomTitle = Option()
        self.TrainIndex = int(0)
        self.IsBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        from Mafi import Option
        self.PreviousTrainCar = Option()
        from Mafi import Option
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
class CargoWagonLoose:

    def __init__(self):
        self.Prototype = None
        self.Cargo = None
        from Mafi import Option
        self.OnlyAllowedProduct = Option()
        self.Capacity = None
        self.UsableCapacity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        from Mafi import Option
        self.AlignedStation = Option()
        self.Maintenance = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CanBePaused = False
        from Mafi import Option
        self.Train = Option()
        from Mafi import Option
        self.CustomTitle = Option()
        self.TrainIndex = int(0)
        self.IsBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        from Mafi import Option
        self.PreviousTrainCar = Option()
        from Mafi import Option
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
class CargoWagonUnit:

    def __init__(self):
        self.Prototype = None
        self.Cargo = None
        from Mafi import Option
        self.OnlyAllowedProduct = Option()
        self.Capacity = None
        self.UsableCapacity = None
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.IsFull = False
        self.IsNotFull = False
        from Mafi import Option
        self.AlignedStation = Option()
        self.Maintenance = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.CanBePaused = False
        from Mafi import Option
        self.Train = Option()
        from Mafi import Option
        self.CustomTitle = Option()
        self.TrainIndex = int(0)
        self.IsBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        from Mafi import Option
        self.PreviousTrainCar = Option()
        from Mafi import Option
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class LayoutEntityWithTrainTrackBase:

    def __init__(self):
        self.Prototype = None
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
        self.IsTrackConstructed = False
        self.Waypoints = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.CanBePaused = False
        self.IsEnabled = False
        self.IsNotEnabled = False
        self.IsPaused = False
        self.IsNotPaused = False
        self.RendererData = None
class LevelCrossing:

    def __init__(self):
        self.Prototype = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsReservedByTrain = False
        self.IsOccupiedByTrain = False
        self.IsNotifiedByTrain = False
        self.IsRoadGloballyClosed = False
        self.IsRoadClosedSelf = False
        self.IsTrackConstructed = False
        self.CanBePaused = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.IsDefaultCritical = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Waypoints = None
        self.RoadLanesCount = int(0)
        self.NumberOfPassedTrains = int(0)
        self.HasBadConnection = False
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
        self.RoadTerrainConnectionsCount = int(0)
        self.IsRoadGloballyClosed = False
        self.IsRoadClosedSelf = False
        self.GateClosedPercentage = None
        self.RoadLanesCount = int(0)
        self.RoadProto = None
        self.HasBadConnection = False
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

    def __init__(self):
        self.Prototype = None
        self.NeedsRefueling = False
        self.IsFuelTankEmpty = False
        self.IsFuelTankFull = False
        self.CannotWorkDueToLowFuel = False
        self.CanRunWithNoFuel = False
        from Mafi import Option
        self.FuelTankProto = Option()
        from Mafi import Option
        self.FuelTank = Option()
        self.FuelConsumption = None
        self.PowerFactor = None
        self.Maintenance = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.IsEngineOn = False
        self.CanBePaused = False
        from Mafi import Option
        self.Train = Option()
        from Mafi import Option
        self.CustomTitle = Option()
        self.TrainIndex = int(0)
        self.IsBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        from Mafi import Option
        self.PreviousTrainCar = Option()
        from Mafi import Option
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
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class StartNodeMetadata:

    def __init__(self):
        self.RequiresTrainToReverse = False
class TenderWagon:

    def __init__(self):
        self.Prototype = None
        self.NeedsRefueling = False
        self.IsFuelTankEmpty = False
        self.IsFuelTankFull = False
        self.CannotWorkDueToLowFuel = False
        self.CanRunWithNoFuel = False
        from Mafi import Option
        self.FuelTankProto = Option()
        from Mafi import Option
        self.FuelTank = Option()
        self.FuelConsumption = None
        self.PowerFactor = None
        self.Maintenance = None
        self.GeneralPriority = int(0)
        self.IsCargoAffectedByGeneralPriority = False
        self.IsGeneralPriorityVisible = False
        self.IsEngineOn = False
        self.CanBePaused = False
        from Mafi import Option
        self.Train = Option()
        from Mafi import Option
        self.CustomTitle = Option()
        self.TrainIndex = int(0)
        self.IsBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        from Mafi import Option
        self.PreviousTrainCar = Option()
        from Mafi import Option
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
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class TrackPathRecord:

    def __init__(self):
        pass

class Train:

    def __init__(self):
        self.Name = str(0)
        self.DefaultTitle = None
        self.Position2f = None
        self.Position3f = None
        self.TrainCarsCount = int(0)
        self.TrainCars = None
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
        from Mafi import Fix64
        self.Acceleration = Fix64()
        self.BrakingDistance = None
        self.FreeSpaceEstimate = None
        self.ClearanceThrottle = None
        from Mafi import Fix32
        self.CurrentBrakingForceKn = Fix32()
        from Mafi import Fix32
        self.TrainHeadWaypointIndex = Fix32()
        from Mafi import Fix32
        self.TrainTailWaypointIndex = Fix32()
        self.IsSpawned = False
        from Mafi import Fix32
        self.GradeForceKn = Fix32()
        from Mafi import Fix32
        self.AirDragKn = Fix32()
        from Mafi import Fix32
        self.CurrentTractiveEffortKn = Fix32()
        from Mafi import Fix32
        self.AvailableTractiveEffortKn = Fix32()
        from Mafi import Fix32
        self.TractiveForceSmoothKn = Fix32()
        from Mafi import Fix32
        self.RollDragKn = Fix32()
        from Mafi import Fix32
        self.RollDragWhenMoving = Fix32()
        from Mafi import Fix32
        self.MassTons = Fix32()
        self.Waypoints = None
        self.OccupiedBlocks = None
        self.OccupiedBlocksCount = int(0)
        self.OccupiedWaypointsCount = int(0)
        self.ReservedBlocks = None
        self.ReservedBlocksCount = int(0)
        self.ReservedWaypointsCount = int(0)
        self.UnreservedBlocksCount = int(0)
        self.UnreservedWaypointsCount = int(0)
        self.TrainsManager = None
        self.TrainTracksGraphManager = None
        from Mafi import Option
        self.Depot = Option()
        self.IsEnteringDepot = False
        self.IsDespawning = False
        self.IsDestroyed = False
        self.IsPaused = False
        self.PreferDirection = None
        self.PathFindingTask = None
        self.LastUsedGoals = None
        from Mafi import Option
        self.Goal = Option()
        from Mafi import Option
        self.LastFoundGoalEntity = Option()
        self.PathDoesNotExist = False
        self.CurrentPath = None
        from Mafi import Option
        self.TrainLine = Option()
        from Mafi import Option
        self.CurrentScheduleItem = Option()
        self.ReservedStationGroupSlots = None
        self.DrivingMode = None
        self.NotifyingCannotScrap = False
        self.NotifyingIdle = False
        from Mafi import Option
        self.CurrentStation = Option()
        self.IsBeingScrapped = False
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
class TrainCarData:

    def __init__(self):
        pass

class TrainCarBase:

    def __init__(self):
        self.CanBePaused = False
        self.Prototype = None
        from Mafi import Option
        self.Train = Option()
        from Mafi import Option
        self.CustomTitle = Option()
        self.TrainIndex = int(0)
        self.IsBackwards = False
        self.FrontAxlePose = None
        self.RearAxlePose = None
        self.SpeedPerTick = None
        self.Color = None
        self.RendererData = None
        self.PercentFull = None
        from Mafi import Fix32
        self.MassTons = Fix32()
        from Mafi import Option
        self.PreviousTrainCar = Option()
        from Mafi import Option
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
class TrainColor:

    def __init__(self):
        pass

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

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
class TrainDepot:

    def __init__(self):
        self.Prototype = None
        self.CanBePaused = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.MaxTrainLength = None
        self.ExtensionOffset = None
        self.MaxStoredOfEachCarType = int(0)
        self.NumServiceLanes = int(0)
        self.PowerRequired = None
        from Mafi import Option
        self.ElectricityConsumer = Option()
        from Mafi import Option
        self.TrainConstructionProgress = Option()
        self.Buffers = None
        self.CarsWithoutTrain = None
        self.InternalTracks = None
        self.InternalTracksVersion = int(0)
        self.TrainBuildQueue = None
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
        self.IsTrackConstructed = False
        self.Waypoints = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.WorkersNeeded = int(0)
        self.HasWorkersCached = False
class TrainConstructionInfo:

    def __init__(self):
        self.IsCarFlipped = None
        self.FilteredProducts = None
        self.TrainLineId = None
class TrainDepotTrackSlot:

    def __init__(self):
        from Mafi import Option
        self.Train = Option()
        self.IsFree = False
        self.IsFull = False
class TrainDepotExtension:

    def __init__(self):
        self.Prototype = None
        self.ExtensionOffset = None
        self.CanBePaused = False
        self.Depot = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.ErrorMessage = str(0)
class TrainLine:

    def __init__(self):
        self.Color = None
        self.UseProductColor = False
        self.ApplyLineColorToTrains = False
        self.Name = None
        from Mafi import Option
        self.ProtoForIcon = Option()
        self.Schedule = None
        self.TrainsCount = int(0)
        self.Trains = None
class TrainLineColor:

    def __init__(self):
        pass

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
        from Mafi import Option
        self.LastFoundGoalEntity = Option()
        self.ActualStartNodeId = None
        from Mafi import Option
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
        self.PfInitCount = int(0)
        self.PfStartCount = int(0)
        self.PfPathFoundCount = int(0)
        self.PfCancelledCount = int(0)
class TrainScheduleDepartConditionBase:

    def __init__(self):
        self.CombineAsOrInsteadOfAnd = False
        self.LastEvalResult = False
class TrainScheduleItemId:

    def __init__(self):
        pass

class TrainsManager:

    def __init__(self):
        self.Trains = None
        self.TrainsDict = None
        self.TrainPausedStateChanged = None
        self.SlopeDifficultyMultiplier = None
        self.FuelConsumptionMultiplier = None
        self.TrainDepots = None
class TrainsPathFindingManager:

    def __init__(self):
        self.QueueSize = int(0)
        self.TotalEnqueuedTasksCount = None
class TrainsPathFindingManagerConfig:

    def __init__(self):
        pass

class TrainStationAlignment:

    def __init__(self):
        self.TrainCarOffset = int(0)
class TrainStationAlignmentPlan:

    def __init__(self):
        pass

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
        self.TrackProto = None
        self.TrainTrackId = None
        self.Direction = None
        self.TrackEntityId = None
        self.TrackTransform = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsTrackConstructed = False
        self.Waypoints = None
        from Mafi import Option
        self.CustomTitle = Option()
        self.GeneralPriority = int(0)
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
        self.WorkersNeeded = int(0)
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
        self.ErrorMessage = str(0)
class TrainStationGroup:

    def __init__(self):
        self.StationEntities = None
        self.ForwardEdgeStationEntities = None
        self.ReverseEdgeStationEntities = None
class StationAndDirection:

    def __init__(self):
        pass

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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
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
        self.ErrorMessage = str(0)
class TrainStationScheduleItem:

    def __init__(self):
        self.IndexInSchedule = int(0)
        self.StationRoots = None
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
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        self.IsTrackConstructed = False
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.CanBePaused = False
        self.PillarBlocksBitmap = None
        self.Pillars = None
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
class TrainTrackBlock:

    def __init__(self):
        self.BlockId = None
class TrainTrackBlockId:

    def __init__(self):
        self.BlockIndex = None
        self.TrainTrackId = None
class TrainTrackBlockRecord:

    def __init__(self):
        pass

class TrainTrackNodeDirection:

    def __init__(self):
        self.Dx = int(0)
        self.Dy = int(0)
        self.Direction = None
class TrainTrackPathFinderOptions:

    def __init__(self):
        pass

class TrainTrackPillar:

    def __init__(self):
        self.CanBePaused = False
        self.VehicleSurfaceHeights = None
        self.PfTargetTiles = None
        self.Value = None
        self.ConstructionCost = None
        self.OccupiedTiles = None
        self.OccupiedVertices = None
        self.OccupiedVerticesCombinedConstraint = None
        self.Height = None
        self.TopTileHeight = None
        self.BlockIndex = int(0)
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
class TrainTrackPillarInfo:

    def __init__(self):
        self.Position2f = None
class TrainTrackPillarInfoRel:

    def __init__(self):
        pass

class TrainTrackPlan:

    def __init__(self):
        self.IsEmpty = False
        self.IsNotEmpty = False
        self.BuildDirection = None
class TrainTrackPlanStep:

    def __init__(self):
        pass

class TrainTracksCollapseHelper:

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
        self.SuperBlocksCount = int(0)
        self.ClaimedSuperBlocksOfTrains = None
        self.GraphMutationsAllowed = False
        self.Initialized = False
        self.TrainGraphVersion = int(0)
        self.TrainGraphIdsValidityVersion = int(0)
        self.TrackEntitiesCount = int(0)
        self.TrackEntities = None
        self.GraphNodes = None
        self.NodesWithSingleEdge = None
        self.ChunkedTrackEntities = None
        self.GraphNodesCount = int(0)
        self.NodesStorageSize = int(0)
        self.GraphEdgesCount = int(0)
        self.EdgesStorageSize = int(0)
        self.TracksWithEndsRequired = None
        self.OnTrackSupportChanged = None
        self.OnSingleEdgeNodeAdded = None
        self.OnSingleEdgeNodeRemoved = None
        self.OnTrackGraphicsChangeNodeAdded = None
        self.OnTrackGraphicsChangeNodeRemoved = None
        self.Priority = None
class TrackEntityAndBlock:

    def __init__(self):
        pass

class TrainTrackAddRequestMetaData:

    def __init__(self):
        pass

class TrainTracksPillarManager:

    def __init__(self):
        pass

class CanBuildTrainTrackResult:

    def __init__(self):
        pass

class CargoWagonProto:

    def __init__(self):
        self.EntityType = None
        self.IconPath = str(0)
        self.BogiePivotsDistance = None
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class CargoWagonUnitProto:

    def __init__(self):
        self.EntityType = None
        self.IconPath = str(0)
        self.BogiePivotsDistance = None
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.MaxProductRenderCapacity = int(0)
        self.ProductRenderOffsets = None
        self.SideViewIconPath = str(0)
class CargoWagonLooseProto:

    def __init__(self):
        self.EntityType = None
        self.IconPath = str(0)
        self.BogiePivotsDistance = None
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.SideViewIconPath = str(0)
class TrainsDebugGameRenderer:

    def __init__(self):
        pass

class IEntityWithTrainTrackBaseProto:

    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.CanBeElevatedOnSupports = False
        self.TrackGraphics = None
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsInitialized = False
        self.Mod = None
class IEntityWithTrainTrackBaseProtoExtensions:

    def __init__(self):
        pass

class ITrainTrackGfx:

    def __init__(self):
        self.VisualStylePrefabsLods = None
        self.Ties = None
class TrackVisualStylePrefabs:

    def __init__(self):
        self.HasTies = False
class EntityWithTrainTrackBaseProto:

    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.CanBeElevatedOnSupports = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
class Gfx:

    def __init__(self):
        self.VisualStylePrefabsLods = None
        self.Ties = None
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.YawForGeneratedIcon = None
        self.VisualizedLayers = None
        self.Categories = None
class TrainTrackProtoHelper:

    def __init__(self):
        pass

class TrainTrackWaypointBlockData:

    def __init__(self):
        pass

class TrainTrackBlockDataRel:

    def __init__(self):
        pass

class IEntityWithTrainTrack:

    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
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
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
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
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
class TrainTrackTrajectoryDirection:

    def __init__(self):
        pass

class TrainTrackTrajectoryDirectionExtensions:

    def __init__(self):
        pass

class ITrainDepot:

    def __init__(self):
        self.InternalTracks = None
        self.DepartingInternalBlockId = None
        self.MaxTrainLength = None
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
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
        self.BlocksCount = int(0)
        self.CanBeElevatedOnSupports = False
        self.TrackGraphics = None
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsInitialized = False
        self.Mod = None
class ITrainTrackPathFinder:

    def __init__(self):
        self.Options = None
        self.Start = None
        self.Goal = None
        self.NodesProcessed = int(0)
        self.NodesInHeap = int(0)
        self.InvalidNodes = int(0)
class TrainTrackPathFinderFlags:

    def __init__(self):
        pass

class TrainTrackPfExploredTile:

    def __init__(self):
        pass

class ITrainTracksGraphPiece:

    def __init__(self):
        self.TrackProto = None
        self.IsTrackConstructed = False
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

class LevelCrossingProto:

    def __init__(self):
        self.EntityType = None
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.CanBeElevatedOnSupports = False
        self.TrajectoryData = None
        self.MaxSpeedTilesPerTick = None
        self.TrainTrackHelper = None
        self.MaxVehicleSpeedPerTick = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.TierData = None
        self.Graphics = None
        self.TrackGraphics = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.VisualStylePrefabsLods = None
        self.Ties = None
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.YawForGeneratedIcon = None
        self.VisualizedLayers = None
        self.Categories = None
class LevelCrossingEntranceProto:

    def __init__(self):
        self.EntityType = None
        self.Graphics = None
        self.TierData = None
        self.MaxVehicleSpeedPerTick = None
        self.LanesSpecs = None
        self.LanesData = None
        self.LanesTrajectories = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.PrefabPath = str(0)
        self.PrefabOrigin = None
        self.IconPath = str(0)
        self.YawForGeneratedIcon = None
        self.VisualizedLayers = None
        self.Categories = None
class LocomotiveProto:

    def __init__(self):
        self.EntityType = None
        from Mafi import Option
        self.FuelTankProto = Option()
        from Mafi import Option
        self.LocomotiveFuelTankProto = Option()
        self.IconPath = str(0)
        self.BogiePivotsDistance = None
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.SideViewIconPath = str(0)
class LocomotiveFuelTankProto:

    def __init__(self):
        self.PrimaryProduct = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class TenderWagonProto:

    def __init__(self):
        self.EntityType = None
        from Mafi import Option
        self.FuelTankProto = Option()
        from Mafi import Option
        self.LocomotiveFuelTankProto = Option()
        self.IconPath = str(0)
        self.BogiePivotsDistance = None
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class ITrain:

    def __init__(self):
        self.Name = str(0)
class TrainDrivingMode:

    def __init__(self):
        pass

class TrainStateForUi:

    def __init__(self):
        pass

class ITrainFriend:

    def __init__(self):
        self.IsSpawned = False
        self.Name = str(0)
        self.TrainCarsColorOverride = None
        from Mafi import Option
        self.TrainLine = Option()
        from Mafi import Option
        self.CurrentScheduleItem = Option()
class TrainCarBaseProto:

    def __init__(self):
        self.IconPath = str(0)
        self.BogiePivotsDistance = None
        self.EntityType = None
        self.Costs = None
        self.Id = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:

    def __init__(self):
        self.SideViewIconPath = str(0)
class PositionOnTrainTrack:

    def __init__(self):
        pass

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
        self.Value = None
        self.ConstructionCost = None
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
        self.DefaultTitle = None
        self.Id = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
class ITrainDepotExtensionParentProto:

    def __init__(self):
        self.ExtensionOffset = None
        self.Layout = None
        self.Ports = None
        self.CannotBeReflected = False
        self.IsUnique = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsAvailable = False
        self.IsNotAvailable = False
        self.IsInitialized = False
        self.Mod = None
class TrainDepotExtensionProto:

    def __init__(self):
        self.EntityType = None
        self.TierData = None
        self.ExtensionOffset = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class TrainDepotProto:

    def __init__(self):
        self.EntityType = None
        self.ElectricityConsumed = None
        self.CanBeElevatedOnSupports = False
        self.TierData = None
        self.ExtensionOffset = None
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
class ITrainLineMember:

    def __init__(self):
        from Mafi import Option
        self.TrainLine = Option()
        from Mafi import Option
        self.CurrentScheduleItem = Option()
        self.Name = str(0)
class ITrainLineMemberFriend:

    def __init__(self):
        self.TrainCarsColorOverride = None
        from Mafi import Option
        self.TrainLine = Option()
        from Mafi import Option
        self.CurrentScheduleItem = Option()
        self.Name = str(0)
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
        self.Train = None
        self.IsEnqueuedOrBeingProcessed = False
        self.Status = None
        self.LastPfResult = None
        self.PathView = None
        self.StartNodes = None
        self.StartNodesMetadata = None
        self.GoalNodes = None
class TrainPathFindingTaskStatus:

    def __init__(self):
        pass

class ITrainScheduleDepartCondition:

    def __init__(self):
        self.CombineAsOrInsteadOfAnd = False
        self.LastEvalResult = False
class TrainScheduleConditionsCommandsProcessor:

    def __init__(self):
        pass

class ITrainScheduleItem:

    def __init__(self):
        self.IndexInSchedule = int(0)
        self.StationRoots = None
        self.DepartConditions = None
        self.LoadOnlyProducts = None
        self.UnloadOnlyProducts = None
        self.DisableLoad = False
        self.DisableUnload = False
        self.SkipIfFuelHigherThan = None
class TrainEntityModuleLimits:

    def __init__(self):
        pass

class TrainEntityModuleLimitsExtensions:

    def __init__(self):
        pass

class TrainScheduleItemsCommandsProcessor:

    def __init__(self):
        pass

class PreferredTrainDirection:

    def __init__(self):
        pass

class ITrainsPathFinder:

    def __init__(self):
        pass

class TrainsPathFinder:

    def __init__(self):
        pass

class TrainsPathFinderResult:

    def __init__(self):
        pass

class TrainsPathFinderConfig:

    def __init__(self):
        pass

class TrainStaticData:

    def __init__(self):
        self.SlopeDifficultyMultiplier = None
        self.FuelConsumptionPer60 = None
        self.WasteProducedPer60 = None
        self.MaintenancePer60 = None
        self.Workers = int(0)
        self.CapacityPerProductType = None
        self.ForcesAtSpeeds = None
        self.DataVersion = int(0)
class TrainForcesAtSpeeds:

    def __init__(self):
        pass

class TrainForcesAtGrade:

    def __init__(self):
        pass

class TrainStationAlignmentStatus:

    def __init__(self):
        pass

class ITrainStationBase:

    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        from Mafi import Option
        self.CustomTitle = Option()
class ITrainStationRoot:

    def __init__(self):
        self.ModuleLimits = None
        self.TrainLimit = int(0)
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        from Mafi import Option
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
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        from Mafi import Option
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
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        from Mafi import Option
        self.CustomTitle = Option()
class ITrainStationFuel:

    def __init__(self):
        self.CanChangeRailTrackDirection = False
        self.CanChangeCriticality = False
        self.CanAddToSuperBlock = False
        self.CanRemoveFromSuperBlock = False
        self.IsDefaultCritical = False
        self.Waypoints = None
        self.OccupiedTiles = None
        self.Prototype = None
        self.Transform = None
        self.CenterTile = None
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
        self.AlwaysUseCustomPfTargetTiles = False
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
        self.TrackProto = None
        self.IsTrackConstructed = False
        self.TrackEntityId = None
        self.TrainTrackId = None
        self.TrackTransform = None
        self.Direction = None
        self.TrackCenterTile = None
        self.TrackPosition2f = None
        self.TrackPosition3f = None
        from Mafi import Option
        self.CustomTitle = Option()
class TrainStationBaseProto:

    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.CanBeElevatedOnSupports = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
class TrainStationModuleBaseProto:

    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.CanBeElevatedOnSupports = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
class TrainStationRootBaseProto:

    def __init__(self):
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.CanBeElevatedOnSupports = False
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
        self.EntityType = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
class TrainStationValidator:

    def __init__(self):
        self.Priority = None
class TrainTrackRadius:

    def __init__(self):
        pass

class TrainTrackGradeFactor:

    def __init__(self):
        pass

class TrainTrackGradeFactorExtensions:

    def __init__(self):
        pass

class TrainTrackBuilder:

    def __init__(self):
        pass

class TrainTrackConstants:

    def __init__(self):
        pass

class TrainTrackPathFinder:

    def __init__(self):
        self.ProcessedNodes = None
        self.Options = None
        self.Start = None
        self.Goal = None
        self.NodesProcessed = int(0)
        self.NodesInHeap = int(0)
        self.InvalidNodes = int(0)
class PieceInfo:

    def __init__(self):
        pass

class NodeConstraints:

    def __init__(self):
        self.AngleSinceStart = None
class Node:

    def __init__(self):
        self.EndOffset = None
        self.Piece = None
class ITrainTrackPillar:

    def __init__(self):
        self.TrainTrackEntityId = None
        self.BlockIndex = int(0)
class TrainTrackPillarAddRequest:

    def __init__(self):
        self.ReasonToAdd = None
class TrainTrackPillarEntityValidator:

    def __init__(self):
        self.Priority = None
class TrainTrackPillarRendererData:

    def __init__(self):
        self.IsValid = False
class TrainTrackPillarProto:

    def __init__(self):
        self.EntityType = None
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
class Gfx:
    Empty = None

    def __init__(self):
        pass

class TrainTrackPillarsBuilder:

    def __init__(self):
        pass

class TrainTrackProto:

    def __init__(self):
        self.EntityType = None
        self.CanBeElevatedOnSupports = False
        self.IsStraight = False
        from Mafi import Option
        self.ElevationFlippedProto = Option()
        self.TrajectoryLength = None
        self.BlocksCount = int(0)
        self.MaxSpeedTilesPerTick = None
        self.TrajectoryData = None
        self.TrainTrackHelper = None
        self.Layout = None
        self.Ports = None
        self.CloningDisabled = False
        self.IsUnique = False
        self.CannotBeReflected = False
        self.AutoBuildMiniZippers = False
        self.Graphics = None
        self.IconPath = str(0)
        self.Id = None
        self.Costs = None
        self.Strings = None
        self.IsNotPhantom = False
        self.IsInitialized = False
        self.Mod = None
        self.Tags = None
        self.IsNotAvailable = False
        self.IsAvailable = False
        self.IsObsolete = False
        self.TrackGraphics = None
class TrainTrackSegmentsRel:

    def __init__(self):
        self.Length = None
class TrainTrackTiesGfx:

    def __init__(self):
        pass

class TrainTrackWaypoint:

    def __init__(self):
        pass

class TrainTrackWaypointRel:

    def __init__(self):
        pass

class TrackOverlapStatus:

    def __init__(self):
        pass

class TrainGraphEdge:

    def __init__(self):
        pass

class TrainTrackBlockIdWithDirection:

    def __init__(self):
        pass

class TrainGraphEdgeInfo:

    def __init__(self):
        pass

class TrainTrackGraphNodeKey:

    def __init__(self):
        self.Position2i = None
        self.Position = None
class TrainGraphNodeWithEdge:

    def __init__(self):
        pass

class TrainTrackOccupancyData:

    def __init__(self):
        pass

class TrainTrackState:

    def __init__(self):
        pass

class TrainBlockState:

    def __init__(self):
        pass

class ITrainTrackManagedEntity:

    def __init__(self):
        pass

class TrainTrackGraphNodeKeyAndCrossSectionData:

    def __init__(self):
        pass

class TrainTrackTrajectoryData:

    def __init__(self):
        pass

