
class IComputingManager:
    def __init__(self):
        self.ComputingProductProto = None
        self.ProducedLastTick = None
        self.DemandedThisTick = None
        self.GenerationCapacityThisTick = None

class FluidIndicatorGfxParams:
    def __init__(self):
        self.SizePerTextureWidthMeters = 0.0
        self.DetailsScale = 0.0
        self.StillMovementScale = 0.0

class IEntityWithAssignedRecipes:
    def __init__(self):
        self.RecipesAssigned = None

class LoosePileTextureParams:
    Default = None
    def __init__(self):
        self.Scale = 0.0
        self.OffsetX = 0.0
        self.OffsetY = 0.0

class IEntityWithProductivityCounter:
    def __init__(self):
        self.OngoingMonthlyData = None
        self.ProductivityCounterHistory = None
        self.ProductivityCounterLabels = None
        self.Id = None
        self.Prototype = None
        self.Context = None
        self.IsEnabled = False
        self.IsPaused = False
        self.CanBePaused = False
        self.IsDestroyed = False
        self.DefaultTitle = None

class ProductivityCounter:
    def __init__(self):
        self.OngoingMonthlyData = None
        self.HistoricData = None

class ProductivityCounterHistory:
    MONTHLY_RECORDS_COUNT = 0
    YEARLY_RECORDS_COUNT = 0
    TOTAL_RECORDS_TICK_COUNT = 0
    def __init__(self):
        self.Version = 0
        self.MonthlyRecordsBuffer = None
        self.MonthlyRecordsBufferIndex = 0
        self.YearlyRolling = None
        self.YearlyRecordsBuffer = None
        self.YearlyRecordsBufferIndex = 0
        self.TotalRollingHalf = None

    class Data:
        def __init__(self):
            self.CategoryA = None
            self.CategoryB = None
            self.CategoryC = None
            self.CategoryD = None
            self.RawData = None

class ProductivityCounterLabels:
    def __init__(self):
        self.CategoryA = None
        self.CategoryB = None
        self.CategoryC = None
        self.CategoryD = None

class ProductivityLabelCategory:
    def __init__(self):
        self.Label = None
        self.Color = None
        self.IsProductiveMult = 0
