from Core.categories import DefaultCategories
from Core.fields import EntityField, BooleanField
from Core.io import Input, Output
from Core.module import DefaultControllers, Module, ModuleStatus
from Mafi.Core.Factory.Machines import IEntityWithBoost

class Connection_Boost_Set(Module):
    name = "Connection: Unity Boost"
    symbol = "BOST"
    inputs = [
        Input("boost", "Boost")
    ]
    outputs = [
        Output("boosted", "Is Boosted")
    ]
    fields = [
        EntityField(IEntityWithBoost, "entity", "Boostable machine",
            "Machine or building that supports Unity boosting of production speed"),
        BooleanField("boost", "Boost", "Enable or disable Unity boost")
    ]
    width = 1
    categories = [DefaultCategories.Connection, DefaultCategories.ConnectionWrite]
    controllers = [DefaultControllers.Controller]

    def action(self):
        entity = self.Field.get_ent("entity")
        if entity is not IEntityWithBoost:
            return ModuleStatus.Error

        boost = self.FieldOrInput.get_bool("boost", False)
        entity.SetBoosted(boost)
        self.Output.set_bool("boosted", entity.IsBoosted)
        return ModuleStatus.Running
