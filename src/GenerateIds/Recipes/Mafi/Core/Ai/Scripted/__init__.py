
class IScriptedAiPlayerAction:
    def __init__(self):
        self.Description = ""
        self.ActionCoreType = None

class IScriptedAiPlayerActionCore:
    def __init__(self):
        pass


class ScriptedAiPlayer:
    DOCK_ENTITY_NAME = ""
    SETTLEMENT_NAME = ""
    def __init__(self):
        self.Stage = 0
        self.IsDoneWithAllActions = False
        self.CurrentActionIndex = 0
        self.ActionsCount = 0
        self.CurrentActionDescription = ""
        self.IsInstaBuildEnabled = False
        self.SimLoopEvents = None
        self.ProtosDb = None

class ScriptedAiPlayerConfig:
    def __init__(self):
        self.Actions = None
        self.FirstBuildingPosition = None
        self.FirstBuildingPositionAlt = None
        self.VehicleDepotPosition = None
        self.BuildingsGridSpacing = None
        self.StartAtStage = 0
        self.TerminateAfterStage = 0
