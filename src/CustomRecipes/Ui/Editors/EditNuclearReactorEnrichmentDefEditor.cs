using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>edit_nuclear_reactor_enrichment</c> editor — typed picker for
    /// the target NuclearReactorProto + the same <see cref="EnrichmentRefEditor"/>
    /// component the build-reactor editor uses. Mirrors
    /// <see cref="EditNuclearReactorFuelsDefEditor"/>: the modder only
    /// configures the enrichment override, not the whole reactor.
    public sealed class EditNuclearReactorEnrichmentDefEditor : DefEditor<EditNuclearReactorEnrichmentDef> {

        private readonly ProtosDb m_protosDb;

        private readonly Column m_reactorHolder;
        private readonly Column m_enrichmentHolder;

        public EditNuclearReactorEnrichmentDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_protosDb = protosDb;

            // Reactor picker — typed ProtoPicker<NuclearReactorProto>
            // restricts the choice to real reactor protos in the DB.
            m_reactorHolder = new Column();
            m_reactorHolder.AlignItemsStretch();
            AddField("reactor (target NuclearReactorProto to patch)",
                m_reactorHolder, onRefresh: () => {
                    m_reactorHolder.Clear();
                    if (value == null) return;
                    m_reactorHolder.Add(new ProtoPicker<NuclearReactorProto>(
                        m_protosDb,
                        getId: () => value?.ReactorId,
                        setId: id => {
                            if (value != null) value.ReactorId = id;
                            // Rebuild the enrichment block so the "Load
                            // existing" affordance toggles based on whether
                            // the newly-picked reactor carries Enrichment.
                            rebuildEnrichment(m_enrichmentHolder);
                        },
                        title: new LocStrFormatted("Pick target reactor"),
                        variableResolver: EditorHelpers.VariableResolverFor(value)));
                });

            // Enrichment override — same Add/Clear button pair pattern as
            // NuclearReactorDefEditor (avoids the Toggle.Value(bool)
            // round-trip that silently nuked Enrichment state on every
            // Value() swap). The C# runtime ctor for
            // edit_nuclear_reactor_enrichment requires the enrichment
            // arg, so MissingMandatoryFields on the def gates save until
            // the modder fills one in.
            m_enrichmentHolder = new Column();
            m_enrichmentHolder.AlignItemsStretch();
            AddField("enrichment (breeding chemistry to patch onto the target)",
                m_enrichmentHolder, onRefresh: () => rebuildEnrichment(m_enrichmentHolder));

            AddCommentField();
        }

        private void rebuildEnrichment(Column holder) {
            holder.Clear();
            if (value == null) return;

            // Look the target reactor up so the "Load existing" affordance
            // can decide whether to render. Resolved fresh on every rebuild
            // so picking a different reactor immediately updates the button
            // state without a full editor reload.
            NuclearReactorProto targetReactor = string.IsNullOrEmpty(value.ReactorId)
                ? null
                : RecipeFormParts.ResolveProtoSafe<NuclearReactorProto>(m_protosDb, value.ReactorId);
            bool targetHasEnrichment = targetReactor != null && targetReactor.Enrichment.HasValue;

            if (value.Enrichment == null) {
                holder.Add(new ButtonText(
                    new LocStrFormatted("+ Add enrichment override"),
                    () => {
                        if (value == null) return;
                        value.Enrichment = new EnrichmentRef();
                        value.Dirty = true;
                        rebuildEnrichment(holder);
                    }));
                if (targetHasEnrichment) {
                    holder.Add(new ButtonText(
                        new LocStrFormatted("⇩ Load existing from target"),
                        () => {
                            if (value == null) return;
                            EnrichmentRef loaded = EnrichmentRefEditor.CreateFromProto(targetReactor);
                            if (loaded != null) {
                                value.Enrichment = loaded;
                                value.Dirty = true;
                                rebuildEnrichment(holder);
                            }
                        })
                        .Tooltip(new LocStrFormatted(
                            "Populate every field from the target reactor's current Enrichment data, then edit deltas.")));
                }
                return;
            }

            holder.Add(new EnrichmentRefEditor(
                value.Enrichment,
                onChanged: () => { if (value != null) value.Dirty = true; },
                m_protosDb));
            if (targetHasEnrichment) {
                holder.Add(new ButtonText(
                    new LocStrFormatted("⇩ Reload from target"),
                    () => {
                        if (value == null) return;
                        EnrichmentRef loaded = EnrichmentRefEditor.CreateFromProto(targetReactor);
                        if (loaded != null) {
                            value.Enrichment = loaded;
                            value.Dirty = true;
                            rebuildEnrichment(holder);
                        }
                    })
                    .Tooltip(new LocStrFormatted(
                        "Overwrite all fields with the target reactor's current Enrichment data.")));
            }
            holder.Add(new ButtonText(
                new LocStrFormatted("✕ Clear enrichment override"),
                () => {
                    if (value == null) return;
                    value.Enrichment = null;
                    value.Dirty = true;
                    rebuildEnrichment(holder);
                }));
        }
    }
}
