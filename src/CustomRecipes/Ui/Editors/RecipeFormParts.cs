using System;
using System.Collections.Generic;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <summary>
    /// Shared static helpers for the recipe / edit-recipe forms — machine
    /// resolution, port-layout rendering, validation, and the
    /// product-list editor with its row, port dropdown, and validation
    /// column. Extracted from RecipeEditorWindow so RecipeDefEditor and
    /// EditRecipeDefEditor can both reach them without dragging the
    /// window in.
    ///
    /// Every method takes <c>ProtosDb</c> as a parameter where it needs to
    /// resolve ids — keeps the helpers stateless and avoids re-introducing
    /// the window dependency that the old non-static helpers carried.
    /// </summary>
    internal static class RecipeFormParts {

        // ---- Proto resolution -------------------------------------------------

        /// Resolve a machine id to its live MachineProto via ProtosDb.
        /// Tries the direct id first, then falls back to TypedRefResolver
        /// for typed-ref paths (Ids.Machines.*). Returns null when neither
        /// works — port dropdowns then show only "*" + the current stored
        /// value.
        /// Generic-proto variant of <see cref="ResolveMachine"/> used by
        /// the entity-clone editors (which work over various TSourceProto).
        /// Two-step lookup: direct id, then TypedRefResolver fallback.
        internal static T ResolveProtoSafe<T>(ProtosDb protosDb, string id) where T : Proto {
            if (protosDb == null || string.IsNullOrEmpty(id)) return null;
            Option<T> direct = protosDb.Get<T>(new Proto.ID(id));
            if (direct.HasValue) return direct.Value;
            string resolved = TypedRefResolver.ResolveOrNull(id);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<T> byPath = protosDb.Get<T>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        internal static MachineProto ResolveMachine(ProtosDb protosDb, string machineId) {
            if (protosDb == null || string.IsNullOrEmpty(machineId)) return null;
            Option<MachineProto> direct = protosDb.Get<MachineProto>(new Proto.ID(machineId));
            if (direct.HasValue) return direct.Value;
            string resolved = TypedRefResolver.ResolveOrNull(machineId);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<MachineProto> byPath = protosDb.Get<MachineProto>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        /// Resolve a ProductRef.ProductId to its live ProductProto. Same
        /// two-step strategy as <see cref="ResolveMachine"/>: direct
        /// ProtosDb hit, then typed-ref fallback. Returns null when
        /// neither resolves — validation then reports "unknown product".
        internal static ProductProto ResolveProduct(ProtosDb protosDb, string productId) {
            if (protosDb == null || string.IsNullOrEmpty(productId)) return null;
            Option<ProductProto> direct = protosDb.Get<ProductProto>(new Proto.ID(productId));
            if (direct.HasValue) return direct.Value;
            string resolved = TypedRefResolver.ResolveOrNull(productId);
            if (!string.IsNullOrEmpty(resolved)) {
                Option<ProductProto> byPath = protosDb.Get<ProductProto>(new Proto.ID(resolved));
                if (byPath.HasValue) return byPath.Value;
            }
            return null;
        }

        // ---- Product(...) expression parser / renderer -----------------------

        /// Try to extract <c>(productId, quantity)</c> from a Python
        /// <c>Product("id", N)</c> expression string. Accepts the
        /// canonical shape the emitter writes plus tolerance for
        /// optional named <c>port=</c> args (ignored here — the
        /// generator's input/output product doesn't carry a port).
        /// Returns false for anything else (variable refs, typed-ref
        /// productIds, unusual constructor shapes) so the caller can
        /// fall back to a raw text-edit surface.
        internal static bool TryParseProductExpression(
                string expr, out string productId, out int quantity) {
            productId = null;
            quantity = 0;
            if (string.IsNullOrWhiteSpace(expr)) return false;
            string s = expr.Trim();
            const string prefix = "Product(";
            if (!s.StartsWith(prefix, StringComparison.Ordinal)) return false;
            if (!s.EndsWith(")", StringComparison.Ordinal)) return false;
            string inner = s.Substring(prefix.Length, s.Length - prefix.Length - 1).Trim();
            int firstComma = inner.IndexOf(',');
            if (firstComma < 0) return false;
            string idPart = inner.Substring(0, firstComma).Trim();
            string rest = inner.Substring(firstComma + 1).Trim();
            // Allow an optional port= named arg after the quantity. We
            // split on the next comma so `Product("X", 3, port="A")`
            // still parses out (X, 3) for the picker; the port arg gets
            // dropped on re-emit since this picker doesn't expose it.
            int secondComma = rest.IndexOf(',');
            string qtyPart = secondComma < 0 ? rest : rest.Substring(0, secondComma).Trim();

            if (idPart.Length < 2) return false;
            char q0 = idPart[0];
            if (q0 != '"' && q0 != '\'') return false;
            if (idPart[idPart.Length - 1] != q0) return false;
            productId = idPart.Substring(1, idPart.Length - 2);

            if (!int.TryParse(qtyPart, System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out quantity)) {
                productId = null;
                return false;
            }
            return true;
        }

        /// Render a (productId, quantity) pair as the canonical
        /// <c>Product("id", N)</c> string. ProductId is always
        /// double-quoted; typed-ref shapes (<c>Ids.Products.X</c>) are
        /// out of scope here — modders who want those should use the
        /// raw-expression fallback on <see cref="Components.ProductExpressionPicker"/>.
        internal static string RenderProductExpression(string productId, int quantity) {
            if (string.IsNullOrEmpty(productId)) return null;
            if (quantity < 1) quantity = 1;
            return "Product(\"" + productId + "\", "
                + quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ")";
        }

        // ---- Port layout descriptions ----------------------------------------

        /// Read the valid ports off the machine and build a map from
        /// port-name string (the modder-facing `port=` argument) to a
        /// monospace-aligned layout description. Producing the layout
        /// text here — instead of letting the option factory walk the
        /// PortSpec at render time — means the dropdown popup only has
        /// to do dictionary lookups on a cold path.
        internal static Dictionary<string, string> CollectPortLayouts(
                MachineProto machine, bool isInput) {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (machine == null) return result;
            IoPortType target = isInput ? IoPortType.Input : IoPortType.Output;
            string dirLabel = isInput ? "IN " : "OUT";
            foreach (var port in machine.Ports) {
                if (port.Type != target) continue;
                string shapeName;
                if (port.Shape != null) {
                    string translated = port.Shape.Strings.Name.TranslatedString;
                    if (!string.IsNullOrEmpty(translated)) {
                        shapeName = translated;
                    } else if (!string.IsNullOrEmpty(port.Shape.Id.Value)) {
                        shapeName = port.Shape.Id.Value;
                    } else {
                        shapeName = port.Shape.LayoutChar.ToString();
                    }
                } else {
                    shapeName = "?";
                }
                string padded = shapeName.Length >= 10
                    ? shapeName
                    : shapeName + new string(' ', 10 - shapeName.Length);
                string layout = port.Name + " | " + dirLabel + " | " + padded;
                if (port.Spec.CanOnlyConnectToTransports) layout += " | conveyor-only";
                result[port.Name.ToString()] = layout;
            }
            return result;
        }

        /// Compute, per row, the port the Python runtime would auto-assign
        /// for that row's "*" selector. Mirrors COI's load-time
        /// distribution: explicit ports are subtracted from the pool;
        /// remaining wildcards consume the next available port of their
        /// product type in machine-declaration order.
        internal static List<string> ComputeAutoAssignedPorts(
                ProtosDb protosDb, List<ProductRef> list,
                MachineProto machine, bool isInput) {
            var result = new List<string>();
            if (list == null) return result;
            for (int i = 0; i < list.Count; i++) result.Add(null);
            if (machine == null) return result;

            Mafi.Collections.ImmutableCollections.ImmutableArray<IoPortTemplate> ports =
                isInput ? machine.InputPorts : machine.OutputPorts;

            var portsByType = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var pt in ports) {
                if (pt.Shape == null) continue;
                string typeKey = pt.Shape.AllowedProductType.ToString();
                if (!portsByType.TryGetValue(typeKey, out var bucket)) {
                    bucket = new List<string>();
                    portsByType[typeKey] = bucket;
                }
                bucket.Add(pt.Name.ToString());
            }

            var taken = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (ProductRef pr in list) {
                if (string.IsNullOrEmpty(pr.Port) || pr.Port == "*" || pr.Port == "VIRTUAL")
                    continue;
                ProductProto proto = ResolveProduct(protosDb, pr.ProductId);
                if (proto == null) continue;
                string typeKey = proto.Type.ToString();
                if (!taken.TryGetValue(typeKey, out var set)) {
                    set = new HashSet<string>(StringComparer.Ordinal);
                    taken[typeKey] = set;
                }
                set.Add(pr.Port);
            }

            var nextIdx = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < list.Count; i++) {
                ProductRef pr = list[i];
                if (!(string.IsNullOrEmpty(pr.Port) || pr.Port == "*")) continue;
                ProductProto proto = ResolveProduct(protosDb, pr.ProductId);
                if (proto == null) continue;
                string typeKey = proto.Type.ToString();
                if (!portsByType.TryGetValue(typeKey, out var bucket)) continue;
                nextIdx.TryGetValue(typeKey, out int idx);
                taken.TryGetValue(typeKey, out var takenSet);
                while (idx < bucket.Count && takenSet != null && takenSet.Contains(bucket[idx])) idx++;
                if (idx < bucket.Count) {
                    result[i] = bucket[idx];
                    idx++;
                }
                nextIdx[typeKey] = idx;
            }

            return result;
        }

        // ---- Port-option factory ---------------------------------------------

        internal static UiComponent PortOptionFactory(
                string option,
                Dictionary<string, string> portLayouts,
                string autoAssignedPort) {
            string text;
            if (option == "*") {
                text = !string.IsNullOrEmpty(autoAssignedPort)
                    ? "*       (auto -> " + autoAssignedPort + ")"
                    : "*       (any matching port)";
            } else if (option == "VIRTUAL") {
                text = "VIRTUAL (virtual products only)";
            } else if (portLayouts != null && portLayouts.TryGetValue(option, out string layout)) {
                text = layout;
            } else {
                text = (option ?? "") + "    (not on current machine)";
            }
            return new Label(new LocStrFormatted(text)).Class(Cls.fontMonospace);
        }

        // ---- Add-icon button -------------------------------------------------

        /// Icon-only "+" button reused by the ingredient and product list
        /// editors. Plus.svg + tooltip is the compact form of the older
        /// "+ add foo" text button which crowded the half-width rows.
        internal static UiComponent AddIconButton(string tooltip, Action onClick) {
            return new ButtonIcon(Mafi.Unity.UiToolkit.Library.Button.General,
                    "Assets/Unity/UserInterface/General/Plus.svg")
                .IconSize(16)
                .Tooltip(new LocStrFormatted(tooltip))
                .OnClick(onClick);
        }

        // ---- Drag-reorder state ----------------------------------------------

        /// Per-list mutable container holding the currently-grabbed row's
        /// index (null when no drag is in progress). Shared across every
        /// row in one ingredient/product list so a row's grip-down can
        /// hand the source index to a sibling row's mouse-up. One instance
        /// lives in a closure captured by <see cref="BuildProductListEditor"/>.
        ///
        /// Visual feedback (highlighting the grabbed row's grip) is the
        /// reason we keep a reference to the grip's Background helper here
        /// — we restore it to the resting color on either successful drop
        /// or cancelled drop (release on container whitespace).
        internal sealed class ReorderDragState {
            public int? SourceIndex;
            // Action that resets the grabbed row's grip to its resting
            // background. Captured at grab time so we can restore the
            // visual no matter how the drag ends.
            public Action ClearSourceVisual;
        }

        // ---- Product row + list editor ---------------------------------------

        /// Single row: [grip · Product picker — name+icon] · qty ·
        /// port-dropdown · ✕ remove. Mutations through the picker/qty/port
        /// write back into the captured <paramref name="p"/> ProductRef in
        /// place, so individual edits propagate without a separate sync
        /// step.
        ///
        /// Reorder is drag-and-drop via the leftmost grip handle:
        ///   • Mouse-down on the grip records this row as the drag source.
        ///   • Mouse-up anywhere on a (different) row commits the move —
        ///     the source row's data is inserted at the released row's
        ///     position.
        ///   • Mouse-up on the list's whitespace (e.g. the "+ add" footer)
        ///     cancels the drag without changing anything.
        /// No live ghost-row preview — Mafi doesn't surface a global mouse-
        /// move event we can hook from outside, and the press-release pair
        /// is enough to make the gesture feel natural.
        internal static UiComponent BuildProductRow(
                ProtosDb protosDb,
                ProductRef p,
                Dictionary<string, string> portLayouts,
                string autoAssignedPort,
                Action onRemove,
                Action onChanged,
                Action onStructureChanged,
                Action<Action> onGrabRow,
                Action onReleaseOnRow,
                bool showPort = true) {
            Row row = new Row();
            row.Gap(2.pt()).AlignItemsCenter();

            row.Add(buildDragGrip(onGrabRow));

            row.Add(new ProtoPicker<ProductProto>(
                protosDb,
                getId: () => p.ProductId,
                setId: id => { p.ProductId = id; onStructureChanged?.Invoke(); },
                emptyLabel: new LocStrFormatted("(pick product...)"),
                title: new LocStrFormatted("Pick product")).FlexGrow(1f));

            // Amount: a plain number, or an expression (`config.batch_size`,
            // `base_amount * 2`) authored through the fx composer. The expression wins
            // on emit and the number stays as the fallback — see ProductRef.
            row.Add(new ExpressionField(
                getNumber:     () => p.Quantity,
                setNumber:     v  => p.Quantity = v ?? 1,
                getExpression: () => p.QuantityExpression,
                setExpression: v  => p.QuantityExpression = v,
                onChanged:     () => onChanged?.Invoke(),
                minValue:      1));

            // In the split model a recipe's products carry no port — routing is
            // per machine binding — so the recipe form hides this column. It is
            // still shown for edit_recipe, which keeps the legacy shape.
            if (!showPort) {
                row.Add(new ButtonText(new LocStrFormatted("✕"), onRemove));
                row.OnMouseUp(_ => onReleaseOnRow?.Invoke());
                return row;
            }

            ProductProto resolved = ResolveProduct(protosDb, p.ProductId);
            bool isVirtualProduct = resolved != null
                && resolved.Type.Matches(VirtualProductProto.ProductType);
            List<string> options = new List<string> { "*" };
            if (portLayouts != null) {
                foreach (string n in portLayouts.Keys) options.Add(n);
            }
            if (isVirtualProduct) options.Add("VIRTUAL");
            string currentPort = string.IsNullOrEmpty(p.Port) ? "*" : p.Port;
            if (!options.Contains(currentPort)) options.Add(currentPort);

            Dictionary<string, string> layoutsForFactory =
                portLayouts ?? new Dictionary<string, string>();
            string capturedAuto = autoAssignedPort;
            Dropdown<string> portDropdown = new Dropdown<string>(
                    (option, index, isInDropdown) =>
                        PortOptionFactory(option, layoutsForFactory, capturedAuto))
                .SetOptions(options)
                .MinWidth(160.px());
            portDropdown.SetValue(currentPort);
            portDropdown.OnValueChanged((v, _) => {
                p.Port = (v == "*" || string.IsNullOrEmpty(v)) ? null : v;
                onStructureChanged?.Invoke();
            });
            row.Add(portDropdown);

            row.Add(new ButtonText(new LocStrFormatted("✕"), onRemove));

            // Releasing the mouse anywhere on the row counts as a drop
            // target for an active drag. Registered on the Row itself
            // (not individual children) so a drop over the picker, qty
            // field, or port dropdown still commits — UIElements MouseUp
            // bubbles up unless a child stops propagation, which none of
            // the standard controls do.
            row.OnMouseUp(_ => onReleaseOnRow?.Invoke());

            return row;
        }

        // ---- Port assignment editor (port-centric) ---------------------------

        private const string AutoOption = "(auto)";
        private const string NoneOption = "(unused)";

        /// Icon size in the port-assignment dropdowns. Smaller than the 32px the
        /// proto pickers use — these rows are dense and one per machine port.
        private static readonly Px PortOptionIconSize = 20.px();


        /// Port-centric editor for a binding's port map: ONE ROW PER MACHINE PORT
        /// (not per product), each with a dropdown choosing which of the recipe's
        /// products is routed through it. The recipe declares what the accepted
        /// products are; the binding decides which port each one lands on —
        /// automatically by default, or by hand here.
        ///
        /// Writes back into <paramref name="portMap"/> keyed by PRODUCT (that's the
        /// shape `bind_recipe(ports=…)` takes). Assigning one product to several
        /// ports merges into a single multi-port selector like "AB", because the
        /// runtime rejects two map entries for the same product.
        internal static UiComponent BuildPortAssignmentEditor(
                ProtosDb protosDb,
                MachineProto machine,
                List<ProductRef> recipeInputs,
                List<ProductRef> recipeOutputs,
                List<PortMapRef> portMap,
                Action onChanged) {
            Column col = new Column();
            col.AlignItemsStretch().Gap(1.pt());
            col.Add(new Label(new LocStrFormatted("ports (one row per machine port):")));

            if (machine == null) {
                col.Add(new Label(new LocStrFormatted("   (pick a machine first)")));
                return col;
            }

            col.Add(buildPortSection(protosDb, machine, isInput: true,
                sideProducts: recipeInputs, portMap: portMap, onChanged: onChanged));
            col.Add(buildPortSection(protosDb, machine, isInput: false,
                sideProducts: recipeOutputs, portMap: portMap, onChanged: onChanged));
            return col;
        }

        private static UiComponent buildPortSection(
                ProtosDb protosDb, MachineProto machine, bool isInput,
                List<ProductRef> sideProducts, List<PortMapRef> portMap, Action onChanged) {
            Column section = new Column();
            section.AlignItemsStretch().Gap(1.pt());
            section.Add(new Label(new LocStrFormatted(isInput ? "  Inputs:" : "  Outputs:")));

            IoPortType want = isInput ? IoPortType.Input : IoPortType.Output;
            List<ProductRef> products = sideProducts ?? new List<ProductRef>();

            // What the runtime would pick if this port were left on auto — shown
            // in the "(auto)" option so the default is visible, not a mystery.
            List<string> autoPorts = ComputeAutoAssignedPorts(protosDb, products, machine, isInput);
            Dictionary<string, string> autoByPort = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < products.Count && i < autoPorts.Count; i++) {
                if (!string.IsNullOrEmpty(autoPorts[i]) && !autoByPort.ContainsKey(autoPorts[i])) {
                    autoByPort[autoPorts[i]] = products[i].ProductId;
                }
            }

            bool anyPort = false;
            foreach (var port in machine.Ports) {
                if (port.Type != want) continue;
                anyPort = true;
                string portName = port.Name.ToString();

                // Only products whose type this port can actually carry.
                List<string> options = new List<string> { AutoOption, NoneOption };
                foreach (ProductRef p in products) {
                    if (string.IsNullOrEmpty(p.ProductId)) continue;
                    ProductProto resolved = ResolveProduct(protosDb, p.ProductId);
                    if (resolved != null && port.Shape != null
                            && !port.Shape.AllowedProductType.Matches(resolved.Type)) {
                        continue;
                    }
                    if (!options.Contains(p.ProductId)) options.Add(p.ProductId);
                }

                string current = findProductForPort(portMap, portName) ?? AutoOption;
                if (!options.Contains(current)) options.Add(current);

                Row row = new Row();
                row.Gap(2.pt()).AlignItemsCenter();

                string shapeName = port.Shape != null
                    ? (port.Shape.Strings.Name.TranslatedString ?? port.Shape.Id.Value)
                    : "?";
                row.Add(new Label(new LocStrFormatted(
                        "   " + portName + " | " + (isInput ? "IN " : "OUT")))
                    .Class(Cls.fontMonospace).Width(70.px()));

                // What the port physically accepts, as the game's own product-type
                // badge (the same icon the train-depot UI stamps on wagon tiles).
                // Port shapes have no UI icon of their own, so the type stands in.
                // When a badge exists the shape name moves into its tooltip — the
                // icon already says it, and the rows stay narrow; only fall back to
                // spelling it out when there is none.
                string typeIcon = port.Shape != null
                    ? Mafi.Unity.Ui.ProductTypesIcons.GetIconOrNull(port.Shape.AllowedProductType)
                    : null;
                if (!string.IsNullOrEmpty(typeIcon)) {
                    row.Add(new Icon(typeIcon)
                        .Size(PortOptionIconSize)
                        .Tooltip(new LocStrFormatted(shapeName)));
                } else {
                    row.Add(new Label(new LocStrFormatted(shapeName))
                        .Class(Cls.fontMonospace).Width(190.px()));
                }

                autoByPort.TryGetValue(portName, out string autoProd);
                string autoHint = autoProd != null ? AutoOption + " → " + autoProd : AutoOption;
                string capturedPort = portName;
                string capturedAutoProd = autoProd;
                Dropdown<string> pick = new Dropdown<string>(
                        (option, index, isInDropdown) => buildProductOption(
                            protosDb, option, autoHint, capturedAutoProd))
                    .SetOptions(options)
                    .FlexGrow(1f);
                pick.SetValue(current);
                pick.OnValueChanged((v, _) => {
                    assignProductToPort(portMap, capturedPort, v == AutoOption || v == NoneOption ? null : v);
                    onChanged?.Invoke();
                });
                row.Add(pick);
                section.Add(row);
            }

            if (!anyPort) {
                section.Add(new Label(new LocStrFormatted("   (machine has no ports of this direction)")));
            }
            return section;
        }

        /// One entry in a port's product dropdown: the product's icon (when it
        /// resolves) followed by its id. "(auto)" borrows the icon of whatever the
        /// runtime would pick, so the default reads at a glance; "(unused)" has none.
        private static UiComponent buildProductOption(
                ProtosDb protosDb, string option, string autoHint, string autoProductId) {
            Row row = new Row();
            row.Gap(2.pt()).AlignItemsCenter();

            string iconFor =
                option == AutoOption ? autoProductId :
                option == NoneOption ? null : option;

            ProductProto proto = string.IsNullOrEmpty(iconFor) ? null : ResolveProduct(protosDb, iconFor);
            if (proto != null && !string.IsNullOrEmpty(proto.IconPath)) {
                row.Add(new Icon(proto.IconPath).Size(PortOptionIconSize));
            }
            row.Add(new Label(new LocStrFormatted(option == AutoOption ? autoHint : option)));
            return row;
        }

        /// Which product is currently routed through <paramref name="portName"/>?
        /// A selector may list several ports ("AB"), so this checks membership.
        private static string findProductForPort(List<PortMapRef> portMap, string portName) {
            if (portMap == null) return null;
            foreach (PortMapRef m in portMap) {
                if (string.IsNullOrEmpty(m.Port) || m.Port == "*") continue;
                if (m.Port.IndexOf(portName[0]) >= 0) return m.ProductId;
            }
            return null;
        }

        /// Route <paramref name="portName"/> to <paramref name="productId"/> (null =
        /// back to auto). The port is first detached from whatever held it, then
        /// appended to the target product's selector — so one product ending up on
        /// two ports becomes a single "AB" entry rather than two conflicting ones.
        private static void assignProductToPort(List<PortMapRef> portMap, string portName, string productId) {
            if (portMap == null) return;
            char c = portName[0];

            for (int i = portMap.Count - 1; i >= 0; i--) {
                PortMapRef m = portMap[i];
                if (string.IsNullOrEmpty(m.Port) || m.Port == "*") continue;
                if (m.Port.IndexOf(c) < 0) continue;
                m.Port = m.Port.Replace(portName, "");
                if (string.IsNullOrEmpty(m.Port)) portMap.RemoveAt(i);
            }

            if (string.IsNullOrEmpty(productId)) return;

            foreach (PortMapRef m in portMap) {
                if (m.ProductId != productId) continue;
                if (string.IsNullOrEmpty(m.Port) || m.Port == "*") m.Port = portName;
                else if (m.Port.IndexOf(c) < 0) m.Port += portName;
                return;
            }
            portMap.Add(new PortMapRef(productId, portName));
        }

        // Leftmost drag handle for a reorderable row. 14px-wide column
        // showing a vertical "drag dots" glyph; pressing it captures the
        // row as the drag source. Visual feedback: the grip's background
        // switches to a highlight color while grabbed, and the row
        // editor calls back through the supplied restore action when
        // the drag ends so the grip returns to its resting color no
        // matter how the drag terminated.
        //
        // <paramref name="onGrabRow"/> is an Action&lt;Action&gt; — the
        // grip passes its own visual-restore Action up so the parent
        // list can invoke it from a sibling row's MouseUp / the list-
        // column's cancel handler. Keeps the visual lifecycle local to
        // the grip without forcing the list to know about its color
        // constants.
        private static UiComponent buildDragGrip(Action<Action> onGrabRow) {
            ColorRgba restColor      = new ColorRgba(48, 48, 52, 255);
            ColorRgba highlightColor = new ColorRgba(88, 132, 168, 255);
            Column grip = new Column();
            grip.Size(14.px(), 24.px())
                .Background(restColor)
                .Border(1.px(), ColorRgba.DarkGray, 2)
                .AlignItemsCenter();
            grip.Add(new Label(new LocStrFormatted("⋮⋮"))
                .Class(Cls.fontMonospace)
                .Color(ColorRgba.LightGray)
                .TinyFontSize());
            grip.Tooltip(new LocStrFormatted("Drag to reorder this row"));
            // Filter on left button so right-clicks and middle-clicks
            // don't accidentally initiate a drag. UnityEngine.UIElements
            // MouseDownEvent.button: 0 = left.
            grip.OnMouseDown(evt => {
                if (evt.button != 0) return;
                grip.Background(highlightColor);
                Action restore = () => grip.Background(restColor);
                onGrabRow?.Invoke(restore);
                evt.StopPropagation();
            });
            return grip;
        }

        /// Editable list of ingredient/product rows. The captured
        /// <paramref name="list"/> is the def's actual List&lt;ProductRef&gt;,
        /// so add/remove mutations apply directly to the model — no
        /// separate sync step needed before Save.
        internal static UiComponent BuildProductListEditor(
                ProtosDb protosDb,
                string label, bool isInput, MachineProto machine,
                List<ProductRef> list, Action onChanged, bool showPort = true) {
            Column listColumn = new Column();
            listColumn.Gap(1.pt()).AlignItemsStretch();

            Dictionary<string, string> portLayouts = CollectPortLayouts(machine, isInput);

            // Per-list drag state shared across every row. A row's grip-
            // down writes SourceIndex; a row's mouse-up reads it and
            // performs the swap, then clears. Releases on the container's
            // whitespace (between rows or on the "+ add" footer) bubble up
            // to listColumn's own MouseUp handler below, which also
            // clears — covers the "cancelled drag" path.
            ReorderDragState dragState = new ReorderDragState();
            listColumn.OnMouseUp(_ => {
                dragState.ClearSourceVisual?.Invoke();
                dragState.SourceIndex = null;
                dragState.ClearSourceVisual = null;
            });

            Action rebuild = null;
            rebuild = () => {
                listColumn.Clear();
                List<string> autoAssigned = ComputeAutoAssignedPorts(protosDb, list, machine, isInput);
                for (int i = 0; i < list.Count; i++) {
                    ProductRef p = list[i];
                    int captured = i;
                    string autoPort = i < autoAssigned.Count ? autoAssigned[i] : null;
                    Action onStructureChanged = () => {
                        onChanged?.Invoke();
                        rebuild();
                    };
                    Action<Action> onGrabRow = restoreVisual => {
                        // Clear any previous grab's visual before recording
                        // a new source (defensive — should be cleared by
                        // the prior release, but a stuck visual would look
                        // worse than this extra call).
                        dragState.ClearSourceVisual?.Invoke();
                        dragState.SourceIndex = captured;
                        dragState.ClearSourceVisual = restoreVisual;
                    };
                    Action onReleaseOnRow = () => {
                        if (!dragState.SourceIndex.HasValue) return;
                        int src = dragState.SourceIndex.Value;
                        int dst = captured;
                        // Clear state BEFORE the swap-driven rebuild so
                        // the new rows don't see a stale source index.
                        dragState.ClearSourceVisual?.Invoke();
                        dragState.SourceIndex = null;
                        dragState.ClearSourceVisual = null;
                        if (src == dst || src < 0 || src >= list.Count) return;
                        ProductRef moved = list[src];
                        list.RemoveAt(src);
                        // After RemoveAt, the original dst index shifts
                        // left by one when the source was above it.
                        int insertAt = src < dst ? dst - 1 : dst;
                        if (insertAt < 0) insertAt = 0;
                        if (insertAt > list.Count) insertAt = list.Count;
                        list.Insert(insertAt, moved);
                        onStructureChanged();
                    };
                    listColumn.Add(BuildProductRow(protosDb, p, portLayouts, autoPort, () => {
                        list.RemoveAt(captured);
                        rebuild();
                        onChanged?.Invoke();
                    }, onChanged, onStructureChanged, onGrabRow, onReleaseOnRow, showPort));
                }
                listColumn.Add(AddIconButton(
                    "Add " + label.TrimEnd('s'),
                    () => {
                        list.Add(new ProductRef { Quantity = 1, Port = "*" });
                        rebuild();
                        onChanged?.Invoke();
                    }));
            };
            rebuild();

            Column wrapper = new Column {
                new Label(new LocStrFormatted(label + ":")),
                listColumn
            };
            wrapper.AlignItemsStretch();
            return wrapper;
        }

        // ---- Machine ports view ----------------------------------------------

        /// Summarises the chosen machine's port layout. Renders the
        /// entity's ASCII layout grid plus a monospace inputs/outputs
        /// table on the side.
        internal static UiComponent BuildMachinePortsView(MachineProto machine, string machineId) {
            Column panelLayout = new Column();
            panelLayout.Gap(1.pt()).PaddingTopBottom(2.pt()).AlignItemsStretch();
            panelLayout.Add(new Label(new LocStrFormatted("Machine ports:")).FontBold());

            if (machine == null) {
                string msg = string.IsNullOrEmpty(machineId)
                    ? "  (no machine selected)"
                    : "  (machine '" + machineId + "' could not be resolved)";
                panelLayout.Add(new Label(new LocStrFormatted(msg))
                    .Color(ColorRgba.LightGray));
                return panelLayout;
            }

            if (machine.Layout != null && machine.Layout.LayoutSize.X > 0 && machine.Layout.LayoutSize.Y > 0) {
                panelLayout.Add(new Label(new LocStrFormatted("  Layout:")));
                panelLayout.Add(BuildMachineLayoutGrid(machine.Layout));
            }

            var inputs  = CollectPortLayouts(machine, isInput: true);
            var outputs = CollectPortLayouts(machine, isInput: false);

            Column panelPorts = new Column();
            panelPorts.Gap(1.pt()).PaddingTopBottom(2.pt()).AlignItemsStretch();

            panelPorts.Add(new Label(new LocStrFormatted("  Inputs:")).FontBold());
            if (inputs.Count == 0) {
                panelPorts.Add(new Label(new LocStrFormatted("    (none)")).Class(Cls.fontMonospace));
            } else {
                foreach (var kvp in inputs) {
                    panelPorts.Add(new Label(new LocStrFormatted("    " + kvp.Value))
                        .Class(Cls.fontMonospace));
                }
            }
            panelPorts.Add(new Label(new LocStrFormatted("  Outputs:")).FontBold());
            if (outputs.Count == 0) {
                panelPorts.Add(new Label(new LocStrFormatted("    (none)")).Class(Cls.fontMonospace));
            } else {
                foreach (var kvp in outputs) {
                    panelPorts.Add(new Label(new LocStrFormatted("    " + kvp.Value))
                        .Class(Cls.fontMonospace));
                }
            }
            return new Row(5.px()) {
                panelLayout, panelPorts
            }.Fill().AlignItemsStretch();
        }

        // GUI-grid version of the machine layout. Each occupied tile is a
        // square cell with a body-coloured background; port tiles get a
        // green (input) / blue (output) background with the port name
        // letter and an arrow pointing in the port's facing direction.
        // The grid uses screen-space top-down ordering (high-Y row first)
        // so the visual matches how the player sees the machine on the
        // map. Cells with no tile in the layout fall through as plain
        // spacers so the grid stays rectangular regardless of L-shapes /
        // notches.
        private static UiComponent BuildMachineLayoutGrid(EntityLayout layout) {
            const int CellPx = 24;
            const int GapPx = 1;
            int sizeX = layout.LayoutSize.X;
            int sizeY = layout.LayoutSize.Y;

            // Lookup: (x, y) -> true when occupied. Z is ignored; ports and
            // tiles on a non-zero Z get drawn on the same cell as their
            // ground projection (machines with stacked levels are rare and
            // the visual still reads as "there's something here").
            HashSet<long> occupied = new HashSet<long>();
            foreach (LayoutTile tile in layout.LayoutTiles) {
                occupied.Add(tileKey(tile.Coord.X, tile.Coord.Y));
            }

            // Lookup the port LABEL cell — the empty tile adjacent to each
            // port in the port's facing direction. We render the name + arrow
            // here (in the "outside" area) instead of overlaying the building
            // tile, so the player reads the port as "this is where a transport
            // connects on the X edge, named 'w'". Multiple ports sharing the
            // same label cell (rare; would require two ports stacked) are
            // resolved first-wins to keep a single readable indicator.
            //
            // The port's actual tile (port.RelativePosition) is also recorded
            // so we can stripe its building-side edge with the port color —
            // gives an at-a-glance link between the indicator and the
            // building edge it belongs to without cluttering the cell text.
            Dictionary<long, IoPortTemplate> portLabelAt = new Dictionary<long, IoPortTemplate>();
            Dictionary<long, IoPortTemplate> portEdgeAt  = new Dictionary<long, IoPortTemplate>();
            foreach (IoPortTemplate port in layout.Ports) {
                Vector2i dv = port.RelativeDirection.DirectionVector;
                int lx = port.RelativePosition.X + dv.X;
                int ly = port.RelativePosition.Y + dv.Y;
                long labelKey = tileKey(lx, ly);
                if (!portLabelAt.ContainsKey(labelKey)) portLabelAt[labelKey] = port;

                long edgeKey = tileKey(port.RelativePosition.X, port.RelativePosition.Y);
                if (!portEdgeAt.ContainsKey(edgeKey)) portEdgeAt[edgeKey] = port;
            }

            // Extended bounds: include every label cell so a port pointing
            // outward (e.g. +Y on the top row) gets a visible label cell
            // ABOVE the building footprint. Tracks lx/ly directly since
            // tileKey doesn't preserve sign for negative coordinates.
            int minX = 0, maxX = sizeX - 1;
            int minY = 0, maxY = sizeY - 1;
            foreach (IoPortTemplate port in layout.Ports) {
                Vector2i dv = port.RelativeDirection.DirectionVector;
                int lx = port.RelativePosition.X + dv.X;
                int ly = port.RelativePosition.Y + dv.Y;
                if (lx < minX) minX = lx;
                if (lx > maxX) maxX = lx;
                if (ly < minY) minY = ly;
                if (ly > maxY) maxY = ly;
            }

            ColorRgba bodyColor   = new ColorRgba(72, 72, 80, 255);
            ColorRgba emptyColor  = new ColorRgba(28, 28, 32, 255);
            ColorRgba inputColor  = new ColorRgba(58, 132, 64, 255);
            ColorRgba outputColor = new ColorRgba(58, 100, 168, 255);
            ColorRgba borderColor = ColorRgba.DarkGray;

            Column grid = new Column();
            grid.Gap(GapPx.px());
            // Iterate from high Y down to low Y so screen-space top is high
            // Y — matches how the player sees the building on the map and
            // makes "above the building" read intuitively in the editor.
            for (int y = maxY; y >= minY; y--) {
                Row tileRow = new Row();
                tileRow.Gap(GapPx.px());
                for (int x = minX; x <= maxX; x++) {
                    long key = tileKey(x, y);
                    bool hasTile = occupied.Contains(key);
                    portLabelAt.TryGetValue(key, out IoPortTemplate labelPort);
                    portEdgeAt.TryGetValue(key, out IoPortTemplate edgePort);
                    tileRow.Add(buildLayoutCell(CellPx, labelPort, edgePort, hasTile,
                        bodyColor, emptyColor, inputColor, outputColor, borderColor));
                }
                grid.Add(tileRow);
            }
            return grid;
        }

        // Single layout cell. Three roles map to three visuals:
        //   labelPort != null   -> port indicator cell: port-color fill,
        //                          name letter + arrow stacked inside.
        //                          Renders in the empty tile adjacent to a
        //                          port's RelativePosition, in the port's
        //                          facing direction.
        //   edgePort  != null   -> building tile that hosts a port: body
        //                          color, but the edge facing the port's
        //                          direction is striped with the port color
        //                          so the player can match indicator -> edge.
        //   hasTile              -> plain body color.
        //   else                 -> empty spacer color.
        // The cell is a Column so name/arrow stack vertically and the layout
        // engine centers them horizontally via AlignItemsCenter.
        private static UiComponent buildLayoutCell(int sizePx,
                IoPortTemplate labelPort, IoPortTemplate edgePort, bool hasTile,
                ColorRgba bodyColor, ColorRgba emptyColor,
                ColorRgba inputColor, ColorRgba outputColor,
                ColorRgba borderColor) {
            ColorRgba bg;
            if (labelPort != null) {
                bg = labelPort.Type == IoPortType.Input ? inputColor : outputColor;
            } else if (hasTile) {
                bg = bodyColor;
            } else {
                bg = emptyColor;
            }

            Column cell = new Column();
            cell.Size(sizePx.px(), sizePx.px())
                .Background(bg)
                .Border(1.px(), borderColor, 2)
                .AlignItemsCenter();

            if (labelPort != null) {
                // For outputs the arrow points outward (product leaves the
                // building); for inputs we flip with Rotated180 so the arrow
                // points INTO the building, matching the visual flow of
                // material on an in-game conveyor.
                Direction90 visualDir = labelPort.Type == IoPortType.Input
                    ? labelPort.RelativeDirection.Rotated180
                    : labelPort.RelativeDirection;
                Label arrow = new Label(new LocStrFormatted(visualDir.ToChar().ToString()));
                arrow.Class(Cls.fontMonospace).Color(ColorRgba.White).TinyFontSize();
                cell.Add(arrow);

                Label letter = new Label(new LocStrFormatted(labelPort.Name.ToString()));
                letter.Class(Cls.fontMonospace).Color(ColorRgba.White).FontBold();
                cell.Add(letter);
            }
            return cell;
        }

        // Pack (x, y) into a long key for hash-set / dictionary lookup.
        // Layout coordinates fit comfortably in 32 bits each; combining
        // into a long avoids the allocation overhead of ValueTuple keys
        // on .NET Framework 4.8.
        private static long tileKey(int x, int y) {
            return ((long)(uint)x << 32) | (uint)y;
        }

        // ---- Interactive port placement --------------------------------------

        /// Parse a PortRef.PositionExpression of the form "(x, y, z)" or
        /// "(x, y)" into integer components. Returns false for anything else
        /// (typed-refs, Vector3i(...), missing parens) so the caller can
        /// fall back to a "(0, 0, 0)" placeholder. Z defaults to 0 when
        /// only X/Y are supplied.
        internal static bool TryParsePortPosition(
                string expr, out int x, out int y, out int z) {
            x = 0; y = 0; z = 0;
            if (string.IsNullOrWhiteSpace(expr)) return false;
            string s = expr.Trim();
            if (!s.StartsWith("(", StringComparison.Ordinal)
                    || !s.EndsWith(")", StringComparison.Ordinal)) return false;
            string inner = s.Substring(1, s.Length - 2);
            string[] parts = inner.Split(',');
            if (parts.Length < 2 || parts.Length > 3) return false;
            if (!int.TryParse(parts[0].Trim(),
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out x)) return false;
            if (!int.TryParse(parts[1].Trim(),
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out y)) return false;
            if (parts.Length == 3) {
                if (!int.TryParse(parts[2].Trim(),
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out z)) return false;
            }
            return true;
        }

        // Pending port draft surfaced on the interactive grid. Bundles the
        // parsed position + direction so the renderer doesn't have to
        // re-parse PortRef text on every cell paint.
        internal struct PendingPort {
            public int X, Y;
            public Direction90 Direction;
            public IoPortType Type;
            public char Name;
            // Back-reference so click-to-remove can find which PortRef in
            // the editor's list this overlay represents. Null on render-
            // only call sites.
            public PortRef Source;
        }

        /// Convert the editor's draft <see cref="PortRef"/> list into the
        /// renderable struct form. Reads PositionX/Y directly from the
        /// model (the loader populates them straight from the AST), so a
        /// PortRef whose position is a raw typed-ref expression — with
        /// X/Y/Z left at their zero defaults — still renders at (0, 0)
        /// rather than being skipped. The modder can spot the misplaced
        /// indicator and either fix the X/Y/Z fields or clear the raw
        /// override.
        internal static List<PendingPort> ResolvePendingPorts(List<PortRef> refs) {
            List<PendingPort> result = new List<PendingPort>();
            if (refs == null) return result;
            foreach (PortRef pr in refs) {
                if (pr == null) continue;
                Direction90? dir = Direction90.FromString(pr.Direction ?? "");
                if (!dir.HasValue) continue;
                IoPortType type = string.Equals(pr.Type, "output",
                        StringComparison.OrdinalIgnoreCase)
                    ? IoPortType.Output : IoPortType.Input;
                char name = string.IsNullOrEmpty(pr.Name) ? '?' : pr.Name[0];
                result.Add(new PendingPort {
                    X = pr.PositionX, Y = pr.PositionY,
                    Direction = dir.Value, Type = type,
                    Name = name, Source = pr
                });
            }
            return result;
        }

        /// Interactive layout grid. Same visual shape as
        /// <see cref="BuildMachineLayoutGrid"/>, with three differences:
        ///   1. Always extends bounds by one cell on each side so the
        ///      modder has a clickable empty area around the entire
        ///      building (per the editor's UX brief).
        ///   2. Overlays the editor's <paramref name="pendingPorts"/> on
        ///      top of the source layout, so newly-added ports show up
        ///      live as the modder edits.
        ///   3. Empty cells adjacent to a building tile fire
        ///      <paramref name="onPlace"/> on click with the derived port
        ///      position (the building edge tile) + direction (pointing
        ///      from the building tile to the clicked cell). Click on a
        ///      pending port indicator removes that draft via
        ///      <paramref name="onRemove"/>.
        internal static UiComponent BuildInteractivePortLayoutGrid(
                EntityLayout layout,
                List<PendingPort> pendingPorts,
                Action<int, int, string> onPlace,
                Action<PortRef> onRemove,
                bool showSourcePorts = true) {
            const int CellPx = 24;
            const int GapPx = 1;
            int sizeX = layout.LayoutSize.X;
            int sizeY = layout.LayoutSize.Y;

            HashSet<long> occupied = new HashSet<long>();
            foreach (LayoutTile tile in layout.LayoutTiles) {
                occupied.Add(tileKey(tile.Coord.X, tile.Coord.Y));
            }

            // Combine source layout ports + pending overlay ports into the
            // label / edge lookup maps. Pending ports take priority on a
            // collision so the modder's in-progress edit is what shows.
            // <paramref name="showSourcePorts"/> false (build_machine with
            // copy_ports=false) skips the source ports entirely — the
            // runtime won't carry them forward, so showing them on the
            // preview would mislead the modder about which ports the new
            // machine actually has.
            Dictionary<long, IoPortTemplate> sourceLabelAt = new Dictionary<long, IoPortTemplate>();
            Dictionary<long, IoPortTemplate> sourceEdgeAt  = new Dictionary<long, IoPortTemplate>();
            if (showSourcePorts) {
                foreach (IoPortTemplate port in layout.Ports) {
                    Vector2i dv = port.RelativeDirection.DirectionVector;
                    long lk = tileKey(port.RelativePosition.X + dv.X, port.RelativePosition.Y + dv.Y);
                    if (!sourceLabelAt.ContainsKey(lk)) sourceLabelAt[lk] = port;
                    long ek = tileKey(port.RelativePosition.X, port.RelativePosition.Y);
                    if (!sourceEdgeAt.ContainsKey(ek)) sourceEdgeAt[ek] = port;
                }
            }
            Dictionary<long, PendingPort> pendingLabelAt = new Dictionary<long, PendingPort>();
            foreach (PendingPort pp in pendingPorts) {
                Vector2i dv = pp.Direction.DirectionVector;
                long lk = tileKey(pp.X + dv.X, pp.Y + dv.Y);
                if (!pendingLabelAt.ContainsKey(lk)) pendingLabelAt[lk] = pp;
            }

            // Bounds: building footprint extended by every label cell
            // (source + pending) and then padded by one in each direction
            // so the modder always sees a clickable empty border, even on a
            // building with no ports at all. Source-port bounds only count
            // when those ports are actually being shown — otherwise the
            // grid would expand to fit invisible labels.
            int minX = 0, maxX = sizeX - 1;
            int minY = 0, maxY = sizeY - 1;
            if (showSourcePorts) {
                foreach (IoPortTemplate port in layout.Ports) {
                    Vector2i dv = port.RelativeDirection.DirectionVector;
                    int lx = port.RelativePosition.X + dv.X;
                    int ly = port.RelativePosition.Y + dv.Y;
                    if (lx < minX) minX = lx;
                    if (lx > maxX) maxX = lx;
                    if (ly < minY) minY = ly;
                    if (ly > maxY) maxY = ly;
                }
            }
            foreach (PendingPort pp in pendingPorts) {
                Vector2i dv = pp.Direction.DirectionVector;
                int lx = pp.X + dv.X;
                int ly = pp.Y + dv.Y;
                if (lx < minX) minX = lx;
                if (lx > maxX) maxX = lx;
                if (ly < minY) minY = ly;
                if (ly > maxY) maxY = ly;
            }
            minX--; maxX++; minY--; maxY++;

            ColorRgba bodyColor    = new ColorRgba(72, 72, 80, 255);
            ColorRgba emptyColor   = new ColorRgba(28, 28, 32, 255);
            ColorRgba inputColor   = new ColorRgba(58, 132, 64, 255);
            ColorRgba outputColor  = new ColorRgba(58, 100, 168, 255);
            ColorRgba pendingInput = new ColorRgba(120, 200, 130, 255);  // brighter green
            ColorRgba pendingOutput= new ColorRgba(120, 160, 220, 255);  // brighter blue
            ColorRgba borderColor  = ColorRgba.DarkGray;

            Column grid = new Column();
            grid.Gap(GapPx.px());
            for (int y = maxY; y >= minY; y--) {
                Row tileRow = new Row();
                tileRow.Gap(GapPx.px());
                for (int x = minX; x <= maxX; x++) {
                    long key = tileKey(x, y);
                    bool hasTile = occupied.Contains(key);
                    sourceLabelAt.TryGetValue(key, out IoPortTemplate srcLabel);
                    sourceEdgeAt.TryGetValue(key, out IoPortTemplate srcEdge);
                    bool isPendingLabel = pendingLabelAt.TryGetValue(key, out PendingPort pending);

                    // Determine which neighbouring building tile (if any)
                    // would seed a port if this empty cell were clicked.
                    // Cell is clickable when:
                    //   • It has no occupied tile.
                    //   • It carries no existing or pending port label.
                    //   • It has a building tile in one of the four
                    //     cardinal directions.
                    bool clickable = !hasTile && srcLabel == null && !isPendingLabel
                                  && onPlace != null
                                  && hasAdjacentBuilding(occupied, x, y);

                    tileRow.Add(buildInteractiveCell(
                        CellPx, x, y,
                        srcLabel, srcEdge, isPendingLabel ? (PendingPort?)pending : null,
                        hasTile, clickable, occupied,
                        bodyColor, emptyColor, inputColor, outputColor,
                        pendingInput, pendingOutput, borderColor,
                        onPlace, onRemove));
                }
                grid.Add(tileRow);
            }
            return grid;
        }

        private static bool hasAdjacentBuilding(HashSet<long> occupied, int x, int y) {
            return occupied.Contains(tileKey(x - 1, y))
                || occupied.Contains(tileKey(x + 1, y))
                || occupied.Contains(tileKey(x, y - 1))
                || occupied.Contains(tileKey(x, y + 1));
        }

        private static UiComponent buildInteractiveCell(
                int sizePx, int cellX, int cellY,
                IoPortTemplate srcLabel, IoPortTemplate srcEdge,
                PendingPort? pendingLabel,
                bool hasTile, bool clickable, HashSet<long> occupied,
                ColorRgba bodyColor, ColorRgba emptyColor,
                ColorRgba inputColor, ColorRgba outputColor,
                ColorRgba pendingInput, ColorRgba pendingOutput,
                ColorRgba borderColor,
                Action<int, int, string> onPlace, Action<PortRef> onRemove) {
            // Background by precedence: pending overlay > source label >
            // building body > empty.
            ColorRgba bg;
            if (pendingLabel.HasValue) {
                bg = pendingLabel.Value.Type == IoPortType.Input ? pendingInput : pendingOutput;
            } else if (srcLabel != null) {
                bg = srcLabel.Type == IoPortType.Input ? inputColor : outputColor;
            } else if (hasTile) {
                bg = bodyColor;
            } else {
                bg = emptyColor;
            }

            Column cell = new Column();
            cell.Size(sizePx.px(), sizePx.px())
                .Background(bg)
                .Border(1.px(), borderColor, 2)
                .AlignItemsCenter();

            // Render port indicator (pending takes priority over source so
            // a draft port replacing an existing label reads correctly).
            if (pendingLabel.HasValue) {
                PendingPort pp = pendingLabel.Value;
                Direction90 visualDir = pp.Type == IoPortType.Input
                    ? pp.Direction.Rotated180 : pp.Direction;
                Label arrow = new Label(new LocStrFormatted(visualDir.ToChar().ToString()));
                arrow.Class(Cls.fontMonospace).Color(ColorRgba.White).TinyFontSize();
                cell.Add(arrow);
                Label letter = new Label(new LocStrFormatted(pp.Name.ToString()));
                letter.Class(Cls.fontMonospace).Color(ColorRgba.White).FontBold();
                cell.Add(letter);
                cell.Tooltip(new LocStrFormatted(
                    "Pending " + pp.Type.ToString().ToLowerInvariant() + " port '"
                    + pp.Name + "' facing " + pp.Direction.ToString()
                    + " — click to remove."));
                if (onRemove != null && pp.Source != null) {
                    PortRef capturedRef = pp.Source;
                    cell.OnMouseDown(evt => {
                        if (evt.button != 0) return;
                        onRemove(capturedRef);
                        evt.StopPropagation();
                    });
                }
            } else if (srcLabel != null) {
                Direction90 visualDir = srcLabel.Type == IoPortType.Input
                    ? srcLabel.RelativeDirection.Rotated180
                    : srcLabel.RelativeDirection;
                Label arrow = new Label(new LocStrFormatted(visualDir.ToChar().ToString()));
                arrow.Class(Cls.fontMonospace).Color(ColorRgba.White).TinyFontSize();
                cell.Add(arrow);
                Label letter = new Label(new LocStrFormatted(srcLabel.Name.ToString()));
                letter.Class(Cls.fontMonospace).Color(ColorRgba.White).FontBold();
                cell.Add(letter);
            } else if (clickable) {
                // Show a faint "+" hint so the modder knows the cell is
                // clickable. TinyFontSize keeps it from competing with
                // real port labels.
                Label hint = new Label(new LocStrFormatted("+"));
                hint.Class(Cls.fontMonospace).Color(ColorRgba.LightGray).TinyFontSize();
                cell.Add(hint);
                cell.Tooltip(new LocStrFormatted(
                    "Click to add a port on the adjacent building edge."));
                cell.OnMouseDown(evt => {
                    if (evt.button != 0) return;
                    // Find which side the building is on. Priority: +X,
                    // -X, +Y, -Y — matches the order modders write code
                    // in (X first, then Y).
                    int bx, by; string dir;
                    if (occupied.Contains(tileKey(cellX - 1, cellY))) {
                        bx = cellX - 1; by = cellY; dir = "+X";
                    } else if (occupied.Contains(tileKey(cellX + 1, cellY))) {
                        bx = cellX + 1; by = cellY; dir = "-X";
                    } else if (occupied.Contains(tileKey(cellX, cellY - 1))) {
                        bx = cellX; by = cellY - 1; dir = "+Y";
                    } else if (occupied.Contains(tileKey(cellX, cellY + 1))) {
                        bx = cellX; by = cellY + 1; dir = "-Y";
                    } else {
                        return;
                    }
                    onPlace?.Invoke(bx, by, dir);
                    evt.StopPropagation();
                });
            }
            return cell;
        }

        // ---- Validation -------------------------------------------------------

        /// Replicates the machine-I/O validation that RecipeProtoBuilder
        /// runs at registration time. See the inline comments in
        /// <see cref="validateDirection"/> for the per-rule reasoning;
        /// preserved verbatim from the legacy form so the round-trip
        /// behaviour is identical.
        internal static List<string> ValidateRecipeIo(
                ProtosDb protosDb, RecipeDef r, MachineProto machine) {
            var problems = new List<string>();
            if (machine == null) {
                if (!string.IsNullOrEmpty(r.MachineId)) {
                    problems.Add("Machine '" + r.MachineId + "' could not be resolved - "
                                 + "port validation skipped until dependency loads.");
                }
                return problems;
            }
            validateDirection(protosDb, r.Ingredients, machine.InputPorts, isInput: true, problems);
            validateDirection(protosDb, r.Products,    machine.OutputPorts, isInput: false, problems);
            return problems;
        }

        private static void validateDirection(
                ProtosDb protosDb,
                List<ProductRef> refs,
                Mafi.Collections.ImmutableCollections.ImmutableArray<IoPortTemplate> ports,
                bool isInput,
                List<string> problems) {
            if (refs == null || refs.Count == 0) return;

            var portByName = new Dictionary<string, IoPortTemplate>(StringComparer.Ordinal);
            var machinePortsByType = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var pt in ports) {
                if (pt.Shape == null) continue;
                portByName[pt.Name.ToString()] = pt;
                string key = pt.Shape.AllowedProductType.ToString();
                machinePortsByType.TryGetValue(key, out int existing);
                machinePortsByType[key] = existing + 1;
            }

            if (machinePortsByType.ContainsKey("ANY")) return;

            string dirNoun = isInput ? "input" : "output";
            string virtualKey = VirtualProductProto.ProductType.ToString();

            var takenPorts = new Dictionary<string, string>(StringComparer.Ordinal);
            var takenByType = new Dictionary<string, int>(StringComparer.Ordinal);
            var wildcardsByType = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (ProductRef pr in refs) {
                if (string.IsNullOrEmpty(pr.ProductId)) {
                    problems.Add("Recipe " + dirNoun + " row has no product selected.");
                    continue;
                }
                ProductProto proto = ResolveProduct(protosDb, pr.ProductId);
                if (proto == null) {
                    problems.Add("Recipe " + dirNoun + " '" + pr.ProductId
                                 + "' could not be resolved to a ProductProto.");
                    continue;
                }
                string protoTypeKey = proto.Type.ToString();

                if (protoTypeKey == virtualKey) {
                    string port = pr.Port;
                    if (!string.IsNullOrEmpty(port) && port != "*" && port != "VIRTUAL") {
                        problems.Add("Virtual product '" + pr.ProductId
                                     + "' has port '" + port + "' - virtual products "
                                     + "must use '*' or 'VIRTUAL'.");
                    }
                    continue;
                }
                if (pr.Port == "VIRTUAL") {
                    problems.Add("Product '" + pr.ProductId
                                 + "' has port 'VIRTUAL' but is not a virtual product.");
                    continue;
                }

                if (string.IsNullOrEmpty(pr.Port) || pr.Port == "*") {
                    wildcardsByType.TryGetValue(protoTypeKey, out int wc);
                    wildcardsByType[protoTypeKey] = wc + 1;
                    continue;
                }

                if (!portByName.TryGetValue(pr.Port, out var template)) {
                    problems.Add("Port '" + pr.Port + "' (row '" + pr.ProductId
                                 + "') does not exist on the machine's "
                                 + dirNoun + " ports.");
                    continue;
                }
                string portTypeKey = template.Shape.AllowedProductType.ToString();
                if (portTypeKey != "ANY" && portTypeKey != protoTypeKey) {
                    problems.Add("Port '" + pr.Port + "' accepts " + portTypeKey
                                 + " but row product '" + pr.ProductId + "' is "
                                 + protoTypeKey + " - incompatible.");
                    continue;
                }
                if (takenPorts.TryGetValue(pr.Port, out string firstClaimant)) {
                    problems.Add("Port '" + pr.Port + "' is claimed twice - by '"
                                 + firstClaimant + "' and '" + pr.ProductId
                                 + "'. Each explicit port may only be used once.");
                    continue;
                }
                takenPorts[pr.Port] = pr.ProductId;
                takenByType.TryGetValue(protoTypeKey, out int taken);
                takenByType[protoTypeKey] = taken + 1;
            }

            foreach (var kvp in wildcardsByType) {
                string needed = kvp.Key;
                int need = kvp.Value;
                machinePortsByType.TryGetValue(needed, out int have);
                takenByType.TryGetValue(needed, out int taken);
                int remaining = have - taken;
                if (remaining < need) {
                    if (have == 0) {
                        problems.Add("Not enough " + needed + " " + dirNoun
                                     + " ports - machine has no port for that product type.");
                    } else {
                        problems.Add("Not enough " + needed + " " + dirNoun
                                     + " ports - " + need + " wildcard row(s) need ports, "
                                     + remaining + " remain (machine has " + have
                                     + ", " + taken + " claimed by explicit assignments).");
                    }
                }
            }
        }

        /// Clear + repopulate the validation column with the latest issues.
        /// Called on every row mutation so feedback updates without
        /// rebuilding the form (which would steal focus from active
        /// fields).
        internal static void PopulateValidationColumn(
                ProtosDb protosDb, Column validation, RecipeDef r, MachineProto machine) {
            validation.Clear();
            validation.Add(new Label(new LocStrFormatted("Validation:")).FontBold());
            List<string> issues = ValidateRecipeIo(protosDb, r, machine);
            if (issues.Count == 0) {
                validation.Add(new Label(new LocStrFormatted("✓ All ports valid"))
                    .Color(ColorRgba.Green));
            } else {
                foreach (string msg in issues) {
                    validation.Add(new Label(new LocStrFormatted("⚠ " + msg))
                        .Color(ColorRgba.Red));
                }
            }
        }
    }
}
