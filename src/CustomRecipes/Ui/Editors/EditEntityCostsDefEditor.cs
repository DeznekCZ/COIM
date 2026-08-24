using System;
using System.Collections.Generic;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>edit_entity_costs</c> editor — pick any buildable entity, then
    /// retune what it costs. The target list is deliberately wide: <c>Costs</c>
    /// lives on <c>EntityProto</c>, so machines, buildings, trucks,
    /// excavators, locomotives, cargo wagons and ships are all valid.
    ///
    /// The form is ordered staffing → upkeep → build materials, and the
    /// staffing/upkeep rows only appear when the picked entity can actually
    /// use them: the game charges workers only to an
    /// <see cref="Mafi.Core.Population.IEntityWithWorkers"/> and maintenance
    /// only to an <see cref="Mafi.Core.Maintenance.IMaintainedEntity"/>.
    /// Offering those fields for, say, a conveyor would let a modder write a
    /// value the runtime then silently drops.
    public sealed class EditEntityCostsDefEditor : DefEditor<EditEntityCostsDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;
        private readonly Column m_currentCostHolder;
        private readonly Column m_staffingHolder;
        private readonly Column m_productsHolder;

        public EditEntityCostsDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_packModel = packModel;
            m_protosDb  = protosDb;

            EntityIdPicker entityPicker = new EntityIdPicker(
                m_packModel, m_protosDb,
                getId: () => value?.EntityId,
                setId: id => {
                    if (value != null)
                    {
                        value.EntityId = id;
                    }
                    MarkEdited();
                    refreshAll();
                },
                getOwnerDef: () => value,
                title: new LocStrFormatted("Pick entity to re-price"),
                includeAllEntities: true);
            AddField("entity (machine, building, vehicle, train car or ship)", entityPicker,
                onRefresh: () => {
                    entityPicker.RefreshDisplay();
                    refreshAll();
                });

            m_currentCostHolder = new Column();
            m_currentCostHolder.AlignItemsStretch().Gap(2.px());
            AddField("current cost in game (what this call overrides)", m_currentCostHolder,
                onRefresh: refreshCurrentCost);

            // Workers / maintenance / priority live in one rebuildable holder
            // rather than as fixed AddField rows, because which of them apply
            // depends on the entity the modder just picked.
            m_staffingHolder = new Column();
            m_staffingHolder.AlignItemsStretch().Gap(6.px());
            AddField("staffing and upkeep", m_staffingHolder, onRefresh: refreshStaffing);

            m_productsHolder = new Column();
            m_productsHolder.AlignItemsStretch().Gap(4.px());
            AddField("products (build materials — leave empty to keep the entity's own)",
                m_productsHolder, onRefresh: refreshProducts);

            AddNullableIntField(
                "multiplyPercent (scales the cost above; 100 = unchanged, 150 = 1.5x, 50 = half)",
                d => d.MultiplyPercent, (d, v) => d.MultiplyPercent = v);

            AddCommentField();
        }

        // ---- Target resolution ----------------------------------------------

        /// Resolve the stored id to a live proto. Mirrors EntityIdPicker's
        /// lookup chain so a target written as a typed ref
        /// (<c>Ids.Vehicles.TruckT2</c>) or as an in-file variable resolves the
        /// same way here as it does in the picker's own trigger.
        private EntityProto resolveEntity() {
            if (m_protosDb == null || value == null)
            {
                return null;
            }
            string id = value.EntityId;
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            EntityProto direct = lookup(id);
            if (direct != null)
            {
                return direct;
            }
            string viaTypedRef = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(viaTypedRef))
            {
                EntityProto byPath = lookup(viaTypedRef);
                if (byPath != null)
                {
                    return byPath;
                }
            }
            string viaVariable = resolveVariable(id);
            if (!string.IsNullOrEmpty(viaVariable))
            {
                return lookup(viaVariable);
            }
            return null;
        }

        /// Translate an in-file Python variable name to the id it was bound to.
        /// Returns null when the value isn't a known variable — callers treat
        /// that as "already an id".
        private string resolveVariable(string maybeVariableName) {
            if (string.IsNullOrEmpty(maybeVariableName) || value == null)
            {
                return null;
            }
            Dictionary<string, string> vars = value.SourceFileVariables;
            if (vars == null)
            {
                return null;
            }
            return vars.TryGetValue(maybeVariableName, out string id) ? id : null;
        }

        private EntityProto lookup(string id) {
            Option<EntityProto> found = m_protosDb.Get<EntityProto>(new Proto.ID(id));
            return found.HasValue ? found.Value : null;
        }

        private static bool supportsWorkers(EntityProto proto) {
            return proto != null
                && typeof(Mafi.Core.Population.IEntityWithWorkers).IsAssignableFrom(proto.EntityType);
        }

        private static bool supportsMaintenance(EntityProto proto) {
            return proto != null
                && typeof(Mafi.Core.Maintenance.IMaintainedEntity).IsAssignableFrom(proto.EntityType);
        }

        private static bool supportsPriority(EntityProto proto) {
            return proto != null
                && typeof(Mafi.Core.Entities.Priorities.IEntityWithGeneralPriority)
                    .IsAssignableFrom(proto.EntityType);
        }

        // ---- Sections --------------------------------------------------------

        private void refreshAll() {
            refreshCurrentCost();
            refreshStaffing();
            refreshProducts();
        }

        // Read-back of the target's live EntityCosts. Without it the modder is
        // editing blind — every field here is an override of a value they
        // otherwise have no way to see from inside the editor.
        private void refreshCurrentCost() {
            m_currentCostHolder.Clear();
            if (value == null)
            {
                return;
            }
            EntityProto target = resolveEntity();
            if (target == null)
            {
                m_currentCostHolder.Add(new Label(new LocStrFormatted(
                    string.IsNullOrEmpty(value.EntityId)
                        ? "(pick an entity to see what it currently costs)"
                        : "(entity '" + value.EntityId + "' could not be resolved)"))
                    .Color(ColorRgba.LightGray));
                return;
            }

            EntityCosts costs = target.Costs;
            m_currentCostHolder.Add(new Label(new LocStrFormatted(
                "build: " + formatAssetValue(costs.BaseConstructionCost))).TinyFontSize());
            m_currentCostHolder.Add(new Label(new LocStrFormatted(
                "workers: " + costs.Workers
                + "   ·   priority: " + costs.DefaultPriority
                + "   ·   maintenance: " + formatMaintenance(costs)))
                .TinyFontSize().Color(ColorRgba.LightGray));
        }

        private static string formatAssetValue(Mafi.Core.Economy.AssetValue cost) {
            if (cost.IsEmpty)
            {
                return "(free)";
            }
            List<string> parts = new List<string>();
            foreach (ProductQuantity pq in cost.Products)
            {
                parts.Add(pq.Quantity.Value + "x " + pq.Product.Id.Value);
            }
            return string.Join(", ", parts.ToArray());
        }

        private static string formatMaintenance(EntityCosts costs) {
            Mafi.Core.Maintenance.MaintenanceCosts maintenance = costs.Maintenance;
            if (maintenance.Product == null || maintenance.MaintenancePerMonth.IsNotPositive)
            {
                return "none";
            }
            return maintenance.MaintenancePerMonth + "/month of " + maintenance.Product.Id.Value;
        }

        // Workers, maintenance and priority — each row present only when the
        // picked entity is actually charged that way.
        private void refreshStaffing() {
            m_staffingHolder.Clear();
            if (value == null)
            {
                return;
            }
            EntityProto target = resolveEntity();
            if (target == null)
            {
                m_staffingHolder.Add(new Label(new LocStrFormatted(
                    "(pick an entity — which of these apply depends on what it is)"))
                    .Color(ColorRgba.LightGray));
                return;
            }

            bool anyShown = false;

            if (supportsWorkers(target))
            {
                anyShown = true;
                m_staffingHolder.Add(intRow("workers",
                    "people this entity occupies while it stands",
                    () => value.Workers, v => value.Workers = v));
            }

            if (supportsMaintenance(target))
            {
                anyShown = true;
                m_staffingHolder.Add(doubleRow("maintenance",
                    "units consumed per month; fractional values are allowed",
                    () => value.Maintenance, v => value.Maintenance = v));

                // Maintenance is billed in a VIRTUAL product (the MaintenanceT1
                // / T2 / T3 line), not a physical one, so the picker is typed
                // to that base rather than to ProductProto.
                ProtoPicker<Mafi.Core.Products.VirtualProductProto> maintenancePicker =
                    new ProtoPicker<Mafi.Core.Products.VirtualProductProto>(
                        m_protosDb,
                        getId: () => value?.MaintenanceProductId,
                        setId: id => {
                            if (value != null)
                            {
                                value.MaintenanceProductId = id;
                            }
                            MarkEdited();
                        },
                        emptyLabel: new LocStrFormatted("(keep the entity's current maintenance product)"),
                        title: new LocStrFormatted("Pick maintenance product"),
                        variableResolver: resolveVariable,
                        allowNone: true);
                m_staffingHolder.Add(labelled("maintenanceProduct",
                    "MaintenanceT1 / T2 / T3 — required when maintenance is set and the "
                        + "entity has none yet",
                    maintenancePicker));

                m_staffingHolder.Add(intRow("maintenanceBufferMonths",
                    "extra months of upkeep the entity stockpiles",
                    () => value.MaintenanceBufferMonths, v => value.MaintenanceBufferMonths = v));

                m_staffingHolder.Add(intRow("initialMaintenancePercent",
                    "upkeep boost while the entity is new (vanilla early game uses 180 or 260)",
                    () => value.InitialMaintenancePercent,
                    v => value.InitialMaintenancePercent = v));
            }

            if (supportsPriority(target))
            {
                anyShown = true;
                m_staffingHolder.Add(intRow("priority",
                    "default construction priority, 0 (highest) to 9 (lowest)",
                    () => value.Priority, v => value.Priority = v));
            }

            if (!anyShown)
            {
                m_staffingHolder.Add(new Label(new LocStrFormatted(
                    "'" + target.EntityType.Name + "' takes no workers, upkeep or priority — "
                    + "only its build materials can be changed."))
                    .Color(ColorRgba.LightGray).TinyFontSize());
            }
        }

        private void refreshProducts() {
            m_productsHolder.Clear();
            if (value == null)
            {
                return;
            }
            if (value.Products == null)
            {
                value.Products = new List<ProductRef>();
            }

            EntityProto target = resolveEntity();
            if (target != null && value.Products.Count == 0)
            {
                // Restating a whole price by hand is the tedious part of this
                // call, so offer the vanilla list as a starting point rather
                // than making the modder transcribe it from the game.
                m_productsHolder.Add(new ButtonText(
                    new LocStrFormatted("Copy this entity's current cost into the list"),
                    () => {
                        copyCurrentCostIntoProducts(target);
                        MarkEdited();
                        refreshProducts();
                    }));
            }

            // showPort: false — a port letter routes a recipe's products
            // through a machine's I/O, which has no meaning for a build price.
            m_productsHolder.Add(RecipeFormParts.BuildProductListEditor(
                m_protosDb, "product", isInput: true, machine: null,
                list: value.Products, onChanged: () => MarkEdited(), showPort: false));
        }

        private void copyCurrentCostIntoProducts(EntityProto target) {
            foreach (ProductQuantity pq in target.Costs.BaseConstructionCost.Products)
            {
                value.Products.Add(new ProductRef(pq.Product.Id.Value, pq.Quantity.Value));
            }
        }

        // ---- Row builders ----------------------------------------------------
        //
        // Built by hand rather than through AddField's typed helpers because
        // these rows are conditional — they're added to and cleared from a
        // holder as the target changes, which the fixed field list can't model.

        private Column labelled(string label, string hint, UiComponent field) {
            Column column = new Column();
            column.AlignItemsStretch().Gap(2.px());
            column.Add(new Label(new LocStrFormatted(label)).Class(Cls.groupHeader));
            if (!string.IsNullOrEmpty(hint))
            {
                column.Add(new Label(new LocStrFormatted(hint))
                    .TinyFontSize().Color(ColorRgba.LightGray));
            }
            column.Add(field);
            return column;
        }

        private Column intRow(string label, string hint, Func<int?> get, Action<int?> set) {
            TextField field = new TextField();
            int? current = get();
            field.Text(current.HasValue ? current.Value.ToString() : "");
            field.OnValueChanged(v => {
                if (value == null)
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(v))
                {
                    set(null);
                }
                else if (int.TryParse(v.Trim(), out int parsed))
                {
                    set(parsed);
                }
                MarkEdited();
            });
            return labelled(label, hint + " — leave blank to keep the entity's own value", field);
        }

        private Column doubleRow(string label, string hint, Func<double?> get, Action<double?> set) {
            TextField field = new TextField();
            double? current = get();
            field.Text(current.HasValue ? EditorHelpers.FormatDouble(current.Value) : "");
            field.OnValueChanged(v => {
                if (value == null)
                {
                    return;
                }
                set(EditorHelpers.ParseNullableDouble(v));
                MarkEdited();
            });
            return labelled(label, hint + " — leave blank to keep the entity's own value", field);
        }
    }
}
