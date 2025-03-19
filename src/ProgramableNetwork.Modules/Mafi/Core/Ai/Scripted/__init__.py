class IScriptedAiPlayerAction:

    def __init__(self):
        self.Description = str(0)
        self.ActionCoreType = None
class IScriptedAiPlayerActionCore:

    def __init__(self):
        pass

class ScriptedAiPlayer:

    def __init__(self):
        self.Stage = int(0)
        self.IsDoneWithAllActions = False
        self.CurrentActionIndex = int(0)
        self.ActionsCount = int(0)
        self.CurrentActionDescription = str(0)
        self.IsInstaBuildEnabled = False
class ScriptedAiPlayerConfig:

    def __init__(self):
        self.Actions = None
        self.FirstBuildingPosition = None
        self.FirstBuildingPositionAlt = None
        self.VehicleDepotPosition = None
        self.BuildingsGridSpacing = None
        self.StartAtStage = int(0)
        self.TerminateAfterStage = int(0)
