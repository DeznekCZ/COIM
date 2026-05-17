// IconStudio canvas controller.
//
// Owns all painting and pointer handling for the editor surface. Blazor pushes
// scene state via `setState({ width, height, layers, selectedShapeId, zoom })`
// and calls invokeMethodAsync(...) callbacks for selects / shape commits / vertex
// edits via a DotNetObjectReference; Blazor is the source of truth.
//
// Drag state machine:
//   drag.tool === "rect"|"circle"|"line"  — rubber-band a primitive
//   drag.tool === "polygon"               — accumulating vertices for a new polygon
//   drag.tool === "vertex"                — moving an existing polygon vertex
(function () {
    const studio = (window.iconStudio = window.iconStudio || {});
    const imageCache = new Map();

    // Hit-test tolerance for clicking near a vertex, expressed in screen pixels.
    const HIT_PX = 8;

    let canvas = null;
    let ctx = null;
    let dotnet = null;
    let state = null;
    let drag = null;
    let activeTool = "select";
    let stageEl = null;
    let resizeObserver = null;
    // When the eyedropper tool is active, reference image layers are painted at
    // full opacity so the user can sample original colors. This flag is consulted
    // by paint() without touching the persisted layer.opacity.
    let pickerOverride = false;

    // Canvas viewport. Identity = icon centered + fit to canvas with letterbox.
    // panX/panY are in canvas-pixel space, applied after the base fit transform.
    const viewport = { zoom: 1, panX: 0, panY: 0 };
    let lastSceneW = 0, lastSceneH = 0;

    // Window-level wheel listener installed once. Suppresses Ctrl+wheel browser
    // zoom anywhere in the app. Must be `passive: false` for preventDefault to work.
    function killCtrlWheel(e) { if (e.ctrlKey) e.preventDefault(); }
    if (!window.__iconStudioCtrlWheelInstalled) {
        window.addEventListener("wheel", killCtrlWheel, { passive: false });
        // Also suppress the keyboard ctrl-+ / ctrl-- / ctrl-0 page zoom shortcuts.
        window.addEventListener("keydown", function (e) {
            if (e.ctrlKey && (e.key === "+" || e.key === "-" || e.key === "=" || e.key === "0")) {
                e.preventDefault();
            }
        });
        window.__iconStudioCtrlWheelInstalled = true;
    }

    studio.attach = function (canvasEl, dotnetRef, initialTool) {
        canvas = canvasEl;
        ctx = canvas.getContext("2d");
        dotnet = dotnetRef;
        activeTool = initialTool || "select";

        // Find the surrounding flex container that bounds available drawing space.
        // The canvas resizes to fit that container; the wrap inherits the canvas's
        // size so the checkerboard background stays glued to the canvas edges.
        stageEl = canvasEl.closest(".canvas-stage") || canvasEl.parentElement;

        canvas.addEventListener("pointerdown", onPointerDown);
        canvas.addEventListener("pointermove", onPointerMove);
        canvas.addEventListener("pointerup", onPointerUp);
        canvas.addEventListener("pointercancel", onPointerCancel);
        canvas.addEventListener("dblclick", onDblClick);
        canvas.addEventListener("contextmenu", onContextMenu);
        canvas.addEventListener("wheel", onWheel, { passive: false });
        window.addEventListener("keydown", onKeyDown);

        if (window.ResizeObserver && stageEl) {
            resizeObserver = new ResizeObserver(() => fitCanvas());
            resizeObserver.observe(stageEl);
        }
    };

    studio.detach = function () {
        if (!canvas) return;
        canvas.removeEventListener("pointerdown", onPointerDown);
        canvas.removeEventListener("pointermove", onPointerMove);
        canvas.removeEventListener("pointerup", onPointerUp);
        canvas.removeEventListener("pointercancel", onPointerCancel);
        canvas.removeEventListener("dblclick", onDblClick);
        canvas.removeEventListener("contextmenu", onContextMenu);
        canvas.removeEventListener("wheel", onWheel);
        window.removeEventListener("keydown", onKeyDown);
        if (resizeObserver) { resizeObserver.disconnect(); resizeObserver = null; }
        canvas = ctx = dotnet = state = drag = stageEl = null;
    };

    // Sizes the canvas (pixel buffer) and its surrounding wrap to the largest
    // rectangle that fits the stage while preserving the scene's aspect ratio.
    // Also accounts for HiDPI: the buffer is DPR-scaled so drawing stays crisp.
    function fitCanvas() {
        if (!canvas || !stageEl || !state) return;
        const padding = 24; // breathing room around the icon canvas
        const availW = Math.max(64, stageEl.clientWidth - padding);
        const availH = Math.max(64, stageEl.clientHeight - padding);
        const aspect = state.width / state.height;

        let cssW, cssH;
        if (availW / availH > aspect) {
            cssH = availH;
            cssW = cssH * aspect;
        } else {
            cssW = availW;
            cssH = cssW / aspect;
        }

        const dpr = window.devicePixelRatio || 1;
        canvas.width = Math.round(cssW * dpr);
        canvas.height = Math.round(cssH * dpr);
        canvas.style.width = cssW + "px";
        canvas.style.height = cssH + "px";

        const wrap = canvas.parentElement;
        if (wrap) {
            wrap.style.width = cssW + "px";
            wrap.style.height = cssH + "px";
        }

        paint();
    }

    studio.setTool = function (tool) {
        activeTool = tool;
        // Cancel an in-progress polygon/polyline when switching tools.
        if (drag && (drag.tool === "polygon" || drag.tool === "polyline") && tool !== drag.tool) {
            drag = null;
        }
        const newOverride = (tool === "eyedropper");
        if (newOverride !== pickerOverride) {
            pickerOverride = newOverride;
        }
        if (canvas) canvas.style.cursor = (tool === "eyedropper") ? "crosshair" : "";
        paint();
    };

    studio.setState = function (newState) {
        const sceneDimsChanged = !state ||
            newState.width !== lastSceneW || newState.height !== lastSceneH;
        state = newState;
        if (sceneDimsChanged) {
            viewport.zoom = 1;
            viewport.panX = 0;
            viewport.panY = 0;
            lastSceneW = state.width;
            lastSceneH = state.height;
        }
        for (const layer of state.layers) {
            if (layer.kind === "image" && !imageCache.has(layer.dataUrl)) {
                const img = new Image();
                img.onload = paint;
                img.src = layer.dataUrl;
                imageCache.set(layer.dataUrl, img);
            }
        }
        // First state push (or one that changes aspect) needs a re-fit; subsequent
        // ones just repaint at the current size.
        if (sceneDimsChanged) fitCanvas(); else paint();
    };

    studio.resetZoom = function () {
        viewport.zoom = 1;
        viewport.panX = 0;
        viewport.panY = 0;
        paint();
    };

    // Returns the icon→canvas-pixel base transform (no viewport). Uniform scale so
    // non-square icons letterbox rather than distort.
    function baseFit() {
        const baseScale = Math.min(canvas.width / state.width, canvas.height / state.height);
        return {
            scale: baseScale,
            fitX: (canvas.width - state.width * baseScale) / 2,
            fitY: (canvas.height - state.height * baseScale) / 2,
        };
    }

    // Converts a pointer event's screen coordinates to icon-space, accounting for
    // both the base fit transform and the user's pan / zoom viewport.
    function iconCoords(e) {
        const r = canvas.getBoundingClientRect();
        const cx = (e.clientX - r.left) * (canvas.width / r.width);
        const cy = (e.clientY - r.top) * (canvas.height / r.height);
        const f = baseFit();
        const x = (cx - f.fitX - viewport.panX) / (viewport.zoom * f.scale);
        const y = (cy - f.fitY - viewport.panY) / (viewport.zoom * f.scale);
        return { x, y };
    }

    // 1 icon unit in screen (CSS) pixels at the current zoom. Used for hit-testing
    // tolerances expressed in screen pixels.
    function iconToScreenScale() {
        const r = canvas.getBoundingClientRect();
        const f = baseFit();
        return viewport.zoom * f.scale * (r.width / canvas.width);
    }

    function onPointerDown(e) {
        if (!state) return;
        if (e.button === 2) return; // right-click handled by onContextMenu
        canvas.setPointerCapture(e.pointerId);
        const p = iconCoords(e);

        // ── Eyedropper: read pixel under cursor, send to C# as hex, stop here.
        // Shift held → picks into the brush's stroke channel instead of fill.
        if (activeTool === "eyedropper") {
            const r = canvas.getBoundingClientRect();
            const cx = Math.round((e.clientX - r.left) * (canvas.width / r.width));
            const cy = Math.round((e.clientY - r.top) * (canvas.height / r.height));
            try {
                const pixel = ctx.getImageData(cx, cy, 1, 1).data;
                if (pixel[3] === 0) return; // transparent pixel — ignore
                const hex = "#" + [pixel[0], pixel[1], pixel[2]]
                    .map(c => c.toString(16).padStart(2, "0")).join("");
                const target = e.shiftKey ? "stroke" : "fill";
                dotnet.invokeMethodAsync("OnColorPicked", hex, target);
            } catch (err) {
                console.warn("eyedropper read failed:", err);
            }
            return;
        }

        // ── Polygon / polyline in progress ────────────────────────────
        // Both are multi-click tools. Polygon offers a click-near-first-vertex
        // close shortcut; polyline doesn't (it stays open — use dblclick/Enter).
        if (activeTool === "polygon" || activeTool === "polyline") {
            if (!drag || drag.tool !== activeTool) {
                drag = { tool: activeTool, points: [{ x: p.x, y: p.y }], cursor: { x: p.x, y: p.y } };
                paint();
                return;
            }
            if (activeTool === "polygon") {
                const first = drag.points[0];
                const screenScale = iconToScreenScale();
                const screenDist = Math.hypot((p.x - first.x) * screenScale, (p.y - first.y) * screenScale);
                if (drag.points.length >= 3 && screenDist < HIT_PX) {
                    commitDrag(first);
                    return;
                }
            }
            drag.points.push({ x: p.x, y: p.y });
            paint();
            return;
        }

        // ── Alt+click on edge of a polygon/polyline: toggle segment type
        // (Line ↔ Cubic). Works on the selected shape first; if no edge of
        // the selected shape is under the cursor (or nothing is selected),
        // fall back to scanning every polygon/polyline so the user doesn't
        // have to "select first, then convert."
        if (e.altKey && activeTool === "select") {
            const hit = hitAnyEdge(p, state.selectedShapeId);
            if (hit) {
                if (hit.shapeId !== state.selectedShapeId) {
                    dotnet.invokeMethodAsync("OnSelect", hit.shapeId, false);
                }
                dotnet.invokeMethodAsync("OnToggleSegment", hit.shapeId, hit.afterIndex);
                return;
            }
        }

        // ── Handle / vertex / control-point drag on the selected shape ─
        if (activeTool === "select" && state.selectedShapeId) {
            const sel = findShape(state.selectedShapeId);
            if (sel) {
                if (sel.kind === "polygon" || sel.kind === "polyline") {
                    // Control points first (sit on top of vertices visually).
                    const cp = hitControlPoint(sel, p);
                    if (cp) {
                        drag = { tool: "control", shapeId: sel.id, segIdx: cp.segIdx, ctrlIdx: cp.ctrlIdx };
                        return;
                    }
                    const idx = hitVertex(sel, p);
                    if (idx >= 0) {
                        drag = { tool: "vertex", shapeId: sel.id, vertexIndex: idx };
                        return;
                    }
                } else {
                    const h = hitHandle(sel, p);
                    if (h) {
                        drag = { tool: "handle", shapeId: sel.id, handle: h };
                        return;
                    }
                }
            }
        }

        // ── Move shape(s) by dragging the body ────────────────────────
        // Plain click on a shape: enter "move-or-select" — if pointer moves more
        // than a few pixels we treat it as a drag-to-move; otherwise pointerup
        // treats it as a click-to-select. Ctrl+click skips this path so the user
        // can toggle multi-select without accidentally translating shapes.
        if (activeTool === "select" && !(e.ctrlKey || e.metaKey)) {
            const hit = hitTest(p.x, p.y);
            if (hit) {
                const currentlySelected =
                    (state.selectedShapeIds && state.selectedShapeIds.length > 0)
                        ? state.selectedShapeIds.slice()
                        : (state.selectedShapeId ? [state.selectedShapeId] : []);
                const moveIds = currentlySelected.includes(hit.id)
                    ? currentlySelected
                    : [hit.id];
                const snapshot = moveIds
                    .map(id => {
                        const s = findShape(id);
                        if (!s) return null;
                        return {
                            id,
                            x: s.x, y: s.y,
                            points: s.points ? s.points.slice() : null,
                        };
                    })
                    .filter(Boolean);
                drag = {
                    tool: "move-or-select",
                    startX: p.x, startY: p.y,
                    shapeIds: moveIds,
                    hitId: hit.id,
                    snapshot,
                    moving: false,
                };
                return;
            }
        }

        // ── New primitive (rect / circle / line) ──────────────────────
        drag = {
            tool: activeTool,
            startX: p.x, startY: p.y,
            x: p.x, y: p.y,
        };
    }

    function onPointerMove(e) {
        if (!state) return;
        const p = iconCoords(e);

        if (drag && drag.tool === "polygon") { drag.cursor = p; paint(); return; }

        if (drag && drag.tool === "vertex") {
            const sel = findShape(drag.shapeId);
            if (sel) {
                // Mutate locally for smooth preview; Blazor will overwrite on pointerup.
                sel.points[drag.vertexIndex * 2] = p.x;
                sel.points[drag.vertexIndex * 2 + 1] = p.y;
                paint();
            }
            return;
        }

        if (drag && drag.tool === "handle") {
            const sel = findShape(drag.shapeId);
            if (sel) {
                applyHandleDrag(sel, drag.handle, p);
                paint();
            }
            return;
        }

        if (drag && drag.tool === "control") {
            const sel = findShape(drag.shapeId);
            if (sel && sel.segments && sel.segments[drag.segIdx]) {
                const seg = sel.segments[drag.segIdx];
                if (drag.ctrlIdx === 1) { seg.c1x = p.x; seg.c1y = p.y; }
                else                    { seg.c2x = p.x; seg.c2y = p.y; }
                paint();
            }
            return;
        }

        if (drag && drag.tool === "move-or-select") {
            const dx = p.x - drag.startX;
            const dy = p.y - drag.startY;
            // Always render the live preview on any movement, no matter how small.
            // The threshold for "this was a drag, not a click" lives in pointerup
            // — keeps preview snappy regardless of HiDPI scaling math.
            if (dx !== 0 || dy !== 0) {
                drag.moving = true;
                for (const orig of drag.snapshot) {
                    const s = findShape(orig.id);
                    if (!s) continue;
                    if (orig.points) {
                        for (let i = 0; i < orig.points.length; i++)
                            s.points[i] = orig.points[i] + (i % 2 === 0 ? dx : dy);
                    } else {
                        s.x = orig.x + dx;
                        s.y = orig.y + dy;
                    }
                }
                canvas.style.cursor = "move";
                paint();
            }
            return;
        }

        if (drag) {
            drag.x = p.x;
            drag.y = p.y;
            paint();
        }
    }

    function onPointerUp(e) {
        if (!state || !drag) return;
        const p = iconCoords(e);

        if (drag.tool === "polygon") return; // closed via dblclick / first-vertex / Enter

        if (drag.tool === "vertex") {
            const sel = findShape(drag.shapeId);
            if (sel) {
                dotnet.invokeMethodAsync("OnUpdatePolygon", sel.id, [...sel.points]);
            }
            drag = null;
            return;
        }

        if (drag.tool === "handle") {
            const sel = findShape(drag.shapeId);
            if (sel) {
                dotnet.invokeMethodAsync("OnUpdateBounds", sel.id, sel.x, sel.y, sel.w, sel.h);
            }
            drag = null;
            return;
        }

        if (drag.tool === "control") {
            const sel = findShape(drag.shapeId);
            if (sel && sel.segments && sel.segments[drag.segIdx]) {
                const seg = sel.segments[drag.segIdx];
                const cx = drag.ctrlIdx === 1 ? seg.c1x : seg.c2x;
                const cy = drag.ctrlIdx === 1 ? seg.c1y : seg.c2y;
                dotnet.invokeMethodAsync(
                    "OnUpdateControlPoint", sel.id, drag.segIdx, drag.ctrlIdx, cx, cy);
            }
            drag = null;
            return;
        }

        if (drag.tool === "move-or-select") {
            const dx = p.x - drag.startX;
            const dy = p.y - drag.startY;
            // Distinguish "real drag" from "shaky click" using total screen-pixel
            // travel — a sub-pixel jitter shouldn't commit a translation.
            const screenDist = Math.hypot(dx, dy) * iconToScreenScale();
            canvas.style.cursor = "";
            if (drag.moving && screenDist > 1) {
                dotnet.invokeMethodAsync("OnMoveShapes", drag.shapeIds, dx, dy);
            } else {
                // No real movement — treat as a select click on the hit shape.
                const addMode = !!(e.ctrlKey || e.metaKey);
                dotnet.invokeMethodAsync("OnSelect", drag.hitId, addMode);
            }
            drag = null;
            paint();
            return;
        }

        if (drag.tool === "select") {
            const hit = hitTest(p.x, p.y);
            const addMode = !!(e.ctrlKey || e.metaKey);
            dotnet.invokeMethodAsync("OnSelect", hit ? hit.id : 0, addMode);
            drag = null;
            paint();
            return;
        }
        commitDrag(p);
    }

    function onPointerCancel() {
        drag = null;
        paint();
    }

    function onDblClick(e) {
        // Finish polygon/polyline drawing on dblclick.
        if (drag && (drag.tool === "polygon" || drag.tool === "polyline")) {
            const minPoints = drag.tool === "polygon" ? 3 : 2;
            if (drag.points.length >= minPoints) {
                drag.points.pop();
                if (drag.points.length >= minPoints) {
                    commitDrag(iconCoords(e));
                } else {
                    drag = null;
                    paint();
                }
            }
            return;
        }

        // Double-click on an edge of the selected polygon/polyline inserts a new
        // vertex on that edge at the click's projection point.
        if (activeTool === "select" && state && state.selectedShapeId) {
            const sel = findShape(state.selectedShapeId);
            const isPoly = sel && (sel.kind === "polygon" || sel.kind === "polyline");
            if (isPoly && sel.points && sel.points.length >= 4) {
                const p = iconCoords(e);
                const hit = hitEdge(sel, p);
                if (hit) {
                    // Explicit insert keeps existing segment kinds intact;
                    // only the split edge becomes two straight lines.
                    dotnet.invokeMethodAsync(
                        "OnInsertVertex", sel.id, hit.afterIndex, hit.x, hit.y);
                }
            }
        }
    }

    // Returns { afterIndex, x, y } for the closest polygon/polyline edge to `p`
    // (within HIT_PX screen pixels), or null. Polylines don't have a closing edge,
    // so the wrap-around segment is skipped.
    function hitEdge(s, p) {
        const screenScale = iconToScreenScale();
        const tol = HIT_PX / screenScale;
        const pts = s.points;
        const isOpen = s.kind === "polyline";
        let best = null;
        for (let i = 0; i < pts.length; i += 2) {
            if (isOpen && i + 2 >= pts.length) break;
            const j = (i + 2) % pts.length;
            const dx = pts[j] - pts[i];
            const dy = pts[j + 1] - pts[i + 1];
            const len2 = dx * dx + dy * dy;
            if (len2 < 1e-9) continue;
            let t = ((p.x - pts[i]) * dx + (p.y - pts[i + 1]) * dy) / len2;
            if (t <= 0.05 || t >= 0.95) continue;
            const px = pts[i] + t * dx;
            const py = pts[i + 1] + t * dy;
            const d = Math.hypot(p.x - px, p.y - py);
            if (d < tol && (best === null || d < best.d)) {
                best = { d, x: px, y: py, afterIndex: i / 2 };
            }
        }
        return best;
    }

    // Scans every polygon/polyline edge in the scene and returns the closest one
    // to `p`, or null. Used for Alt+click on an edge without the shape having to
    // be selected first. Prefers the currently-selected shape when it ties, so
    // the user's focus isn't yanked away.
    function hitAnyEdge(p, preferredShapeId) {
        let best = null;
        for (const layer of state.layers) {
            if (!layer.visible || layer.kind !== "shape") continue;
            for (const s of layer.shapes) {
                if (s.kind !== "polygon" && s.kind !== "polyline") continue;
                const edge = hitEdge(s, p);
                if (!edge) continue;
                const isPreferred = s.id === preferredShapeId;
                if (best === null ||
                    edge.d < best.d ||
                    (isPreferred && Math.abs(edge.d - best.d) < 1e-6)) {
                    best = { d: edge.d, x: edge.x, y: edge.y, afterIndex: edge.afterIndex, shapeId: s.id };
                }
            }
        }
        return best;
    }

    // Right-click. Either pops the last polygon vertex (during drawing) or removes
    // a vertex from a selected polygon (when in select mode). Otherwise just
    // suppresses the native menu.
    function onContextMenu(e) {
        e.preventDefault();
        if (!state) return;
        const p = iconCoords(e);

        if (drag && drag.tool === "polygon") {
            drag.points.pop();
            if (drag.points.length === 0) drag = null;
            paint();
            return;
        }

        if (activeTool === "select" && state.selectedShapeId) {
            const sel = findShape(state.selectedShapeId);
            if (sel && (sel.kind === "polygon" || sel.kind === "polyline")) {
                // Polygon needs at least 3 vertices, polyline at least 2.
                if (sel.points.length > (sel.kind === "polygon" ? 6 : 4)) {
                    const idx = hitVertex(sel, p);
                    if (idx >= 0) {
                        // Explicit remove keeps the rest of the segment list
                        // (and therefore the curves on every other edge) intact.
                        dotnet.invokeMethodAsync("OnRemoveVertex", sel.id, idx);
                    }
                }
            }
        }
    }

    function onKeyDown(e) {
        // ESC cancels polygon-in-progress; Enter commits.
        if (drag && drag.tool === "polygon") {
            if (e.key === "Escape") {
                drag = null;
                paint();
                e.preventDefault();
                return;
            }
            if (e.key === "Enter" && drag.points.length >= 3) {
                commitDrag(drag.points[drag.points.length - 1]);
                e.preventDefault();
                return;
            }
        }

        // Below shortcuts are suppressed when a text field has focus so typing
        // doesn't trigger destructive actions or interfere with property edits.
        const ae = document.activeElement;
        const tag = (ae && ae.tagName) || "";
        const editable = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT" ||
                         (ae && ae.isContentEditable);
        if (editable) return;
        if (!dotnet) return;

        if (e.key === "Delete" || e.key === "Backspace") {
            if (state && state.selectedShapeId) {
                dotnet.invokeMethodAsync("OnDeleteSelected");
                e.preventDefault();
            }
            return;
        }
        if (e.ctrlKey || e.metaKey) {
            const k = e.key.toLowerCase();
            if (k === "z" && !e.shiftKey) { dotnet.invokeMethodAsync("OnUndo"); e.preventDefault(); return; }
            if (k === "y" || (k === "z" && e.shiftKey)) { dotnet.invokeMethodAsync("OnRedo"); e.preventDefault(); return; }
            if (k === "c") { dotnet.invokeMethodAsync("OnCopy"); e.preventDefault(); return; }
            if (k === "v") { dotnet.invokeMethodAsync("OnPaste"); e.preventDefault(); return; }
        }
    }

    // Plain wheel zooms the canvas centered on the cursor. Ctrl+wheel is suppressed
    // (the window-level handler kills the browser's page-zoom shortcut).
    function onWheel(e) {
        if (!state) return;
        e.preventDefault();
        if (e.ctrlKey) return;

        // Capture the icon-space point under the cursor BEFORE changing zoom so
        // we can re-pin it to the same canvas position afterwards.
        const before = iconCoords(e);

        const factor = e.deltaY < 0 ? 1.15 : 1 / 1.15;
        const newZoom = Math.max(0.1, Math.min(32, viewport.zoom * factor));
        if (newZoom === viewport.zoom) return;

        const r = canvas.getBoundingClientRect();
        const cx = (e.clientX - r.left) * (canvas.width / r.width);
        const cy = (e.clientY - r.top) * (canvas.height / r.height);
        const f = baseFit();

        viewport.zoom = newZoom;
        viewport.panX = cx - f.fitX - newZoom * f.scale * before.x;
        viewport.panY = cy - f.fitY - newZoom * f.scale * before.y;

        paint();
    }

    function commitDrag(p) {
        if (!drag) return;
        const payload = {
            tool: drag.tool,
            x: drag.startX ?? 0, y: drag.startY ?? 0,
            endX: p.x, endY: p.y,
            points: drag.points ? drag.points.map(pt => [pt.x, pt.y]).flat() : [],
        };
        drag = null;
        dotnet.invokeMethodAsync("OnCommit", payload);
        paint();
    }

    function hitTest(x, y) {
        if (!state) return null;
        for (let li = state.layers.length - 1; li >= 0; li--) {
            const layer = state.layers[li];
            if (!layer.visible || layer.kind !== "shape") continue;
            for (let si = layer.shapes.length - 1; si >= 0; si--) {
                const s = layer.shapes[si];
                if (pointInShape(s, x, y)) return s;
            }
        }
        return null;
    }

    // Returns the handle ID of a selection handle near `p`, or null. Handles are:
    //   rect / circle : "nw" "ne" "sw" "se" (corners of bounding box)
    //   line          : "p1" "p2" (start / end endpoints)
    // Polygons use hitVertex() instead.
    function hitHandle(s, p) {
        const screenScale = iconToScreenScale();
        const tol = HIT_PX / screenScale;
        if (s.kind === "line") {
            if (Math.hypot(s.x - p.x, s.y - p.y) < tol) return "p1";
            if (Math.hypot((s.x + s.w) - p.x, (s.y + s.h) - p.y) < tol) return "p2";
            return null;
        }
        if (s.kind === "rect" || s.kind === "circle") {
            const corners = {
                "nw": [s.x, s.y],
                "ne": [s.x + s.w, s.y],
                "sw": [s.x, s.y + s.h],
                "se": [s.x + s.w, s.y + s.h],
            };
            for (const key in corners) {
                const [cx, cy] = corners[key];
                if (Math.hypot(cx - p.x, cy - p.y) < tol) return key;
            }
        }
        return null;
    }

    // Mutates `s` in place based on the handle being dragged and the new pointer
    // position. Width/height are kept positive by normalising — if the user drags
    // past the opposite edge, the shape flips but its bbox stays well-formed.
    function applyHandleDrag(s, h, p) {
        if (s.kind === "line") {
            if (h === "p1") {
                const ex = s.x + s.w, ey = s.y + s.h;
                s.x = p.x; s.y = p.y;
                s.w = ex - s.x; s.h = ey - s.y;
            } else if (h === "p2") {
                s.w = p.x - s.x; s.h = p.y - s.y;
            }
            return;
        }
        if (s.kind === "rect" || s.kind === "circle") {
            let nx1 = s.x, ny1 = s.y;
            let nx2 = s.x + s.w, ny2 = s.y + s.h;
            if (h.includes("w")) nx1 = p.x;
            if (h.includes("e")) nx2 = p.x;
            if (h.includes("n")) ny1 = p.y;
            if (h.includes("s")) ny2 = p.y;
            s.x = Math.min(nx1, nx2);
            s.y = Math.min(ny1, ny2);
            s.w = Math.max(0.5, Math.abs(nx2 - nx1));
            s.h = Math.max(0.5, Math.abs(ny2 - ny1));
        }
    }

    // Returns { segIdx, ctrlIdx (1 or 2) } for the control point under `p`, or
    // null. Only cubic segments have control points.
    function hitControlPoint(s, p) {
        if (!s.segments) return null;
        const screenScale = iconToScreenScale();
        const tol = HIT_PX / screenScale;
        for (let i = 0; i < s.segments.length; i++) {
            const seg = s.segments[i];
            if (!seg || seg.kind !== "cubic") continue;
            if (Math.hypot(seg.c1x - p.x, seg.c1y - p.y) < tol) return { segIdx: i, ctrlIdx: 1 };
            if (Math.hypot(seg.c2x - p.x, seg.c2y - p.y) < tol) return { segIdx: i, ctrlIdx: 2 };
        }
        return null;
    }

    // Returns the index (0-based, in vertex units) of a polygon / polyline point
    // near `p`, measured in screen pixels so the tolerance feels consistent
    // regardless of zoom.
    function hitVertex(s, p) {
        if ((s.kind !== "polygon" && s.kind !== "polyline") || !s.points) return -1;
        const screenScale = iconToScreenScale();
        for (let i = 0; i < s.points.length; i += 2) {
            const dx = (s.points[i] - p.x) * screenScale;
            const dy = (s.points[i + 1] - p.y) * screenScale;
            if (Math.hypot(dx, dy) < HIT_PX) return i / 2;
        }
        return -1;
    }

    function pointInShape(s, x, y) {
        switch (s.kind) {
            case "rect":
                return x >= s.x && y >= s.y && x <= s.x + s.w && y <= s.y + s.h;
            case "circle": {
                const cx = s.x + s.w / 2, cy = s.y + s.h / 2;
                const rx = s.w / 2, ry = s.h / 2;
                if (rx <= 0 || ry <= 0) return false;
                const dx = (x - cx) / rx, dy = (y - cy) / ry;
                return dx * dx + dy * dy <= 1;
            }
            case "line": {
                const x2 = s.x + s.w, y2 = s.y + s.h;
                const dx = x2 - s.x, dy = y2 - s.y;
                const len2 = dx * dx + dy * dy;
                if (len2 < 1e-6) return Math.hypot(x - s.x, y - s.y) < 3;
                let t = ((x - s.x) * dx + (y - s.y) * dy) / len2;
                t = Math.max(0, Math.min(1, t));
                const px = s.x + t * dx, py = s.y + t * dy;
                return Math.hypot(x - px, y - py) < 3;
            }
            case "polygon":
                return pointInPolygon(s.points, x, y);
            case "polyline":
                return pointNearPolyline(s.points, x, y);
        }
        return false;
    }

    // Open path — hit if within ~6 screen pixels of any segment.
    function pointNearPolyline(points, x, y) {
        if (!points || points.length < 4) return false;
        const tol = 6 / iconToScreenScale();
        for (let i = 0; i + 3 < points.length; i += 2) {
            const dx = points[i + 2] - points[i];
            const dy = points[i + 3] - points[i + 1];
            const len2 = dx * dx + dy * dy;
            if (len2 < 1e-9) continue;
            let t = ((x - points[i]) * dx + (y - points[i + 1]) * dy) / len2;
            t = Math.max(0, Math.min(1, t));
            const px = points[i] + t * dx, py = points[i + 1] + t * dy;
            if (Math.hypot(x - px, y - py) < tol) return true;
        }
        return false;
    }

    function pointInPolygon(points, x, y) {
        let inside = false;
        for (let i = 0, j = points.length - 2; i < points.length; j = i, i += 2) {
            const xi = points[i],     yi = points[i + 1];
            const xj = points[j],     yj = points[j + 1];
            const intersect = ((yi > y) !== (yj > y)) &&
                (x < (xj - xi) * (y - yi) / (yj - yi + 1e-9) + xi);
            if (intersect) inside = !inside;
        }
        return inside;
    }

    function paint() {
        if (!ctx || !state) return;
        const cw = canvas.width, ch = canvas.height;
        ctx.save();
        ctx.clearRect(0, 0, cw, ch);

        // canvas-pixel = fitOffset + pan + zoom * baseScale * iconPoint
        const f = baseFit();
        ctx.translate(f.fitX + viewport.panX, f.fitY + viewport.panY);
        const k = viewport.zoom * f.scale;
        ctx.scale(k, k);

        // Outline the icon canvas so the user can see its bounds at any zoom.
        ctx.save();
        ctx.strokeStyle = "rgba(96,165,250,0.4)";
        ctx.lineWidth = 1 / k;
        ctx.strokeRect(0, 0, state.width, state.height);
        ctx.restore();

        for (const layer of state.layers) {
            if (!layer.visible) continue;
            // Eyedropper override: reference images are painted fully opaque so the
            // sampled pixel matches what the user sees in the source image.
            const isImageBoosted = pickerOverride && layer.kind === "image";
            ctx.globalAlpha = isImageBoosted ? 1.0 : (layer.opacity ?? 1);
            if (layer.kind === "image") {
                const img = imageCache.get(layer.dataUrl);
                if (img && img.complete) {
                    ctx.drawImage(img, layer.x, layer.y, layer.w, layer.h);
                }
            } else if (layer.kind === "shape") {
                for (const s of layer.shapes) {
                    drawShape(s);
                }
            }
        }
        ctx.globalAlpha = 1;

        if (drag) drawDragPreview();
        ctx.restore();

        // Render selection handles for every selected shape. Primary selection
        // (state.selectedShapeId) gets the full handle treatment; secondary
        // selections get a simpler dashed bounding box so the user can see what
        // else is in the set without visual clutter.
        const selectedIds = (state.selectedShapeIds && state.selectedShapeIds.length > 0)
            ? state.selectedShapeIds
            : (state.selectedShapeId ? [state.selectedShapeId] : []);
        for (const id of selectedIds) {
            const s = findShape(id);
            if (!s) continue;
            if (id === state.selectedShapeId) drawSelectionHandles(s);
            else drawSecondarySelection(s);
        }
    }

    function drawSecondarySelection(s) {
        const bb = shapeBounds(s);
        if (!bb) return;
        const tl = iconToCanvas(bb.x, bb.y);
        const br = iconToCanvas(bb.x + bb.w, bb.y + bb.h);
        ctx.save();
        ctx.strokeStyle = "#60a5fa";
        ctx.lineWidth = 1;
        ctx.setLineDash([2, 3]);
        ctx.strokeRect(tl.x, tl.y, br.x - tl.x, br.y - tl.y);
        ctx.restore();
    }

    // Maps icon-space (ix, iy) to canvas-pixel coordinates using the current viewport.
    function iconToCanvas(ix, iy) {
        const f = baseFit();
        return {
            x: f.fitX + viewport.panX + viewport.zoom * f.scale * ix,
            y: f.fitY + viewport.panY + viewport.zoom * f.scale * iy,
        };
    }

    function drawShape(s) {
        ctx.fillStyle = s.fill || "transparent";
        ctx.strokeStyle = s.stroke || "transparent";
        ctx.lineWidth = s.strokeWidth || 0;
        ctx.beginPath();
        switch (s.kind) {
            case "rect":
                roundRect(ctx, s.x, s.y, s.w, s.h, s.cornerRadius || 0);
                break;
            case "circle": {
                const cx = s.x + s.w / 2, cy = s.y + s.h / 2;
                ctx.ellipse(cx, cy, Math.max(0.01, s.w / 2), Math.max(0.01, s.h / 2), 0, 0, Math.PI * 2);
                break;
            }
            case "line":
                ctx.moveTo(s.x, s.y);
                ctx.lineTo(s.x + s.w, s.y + s.h);
                break;
            case "polygon":
            case "polyline":
                if (s.points && s.points.length >= 4) {
                    drawPolyPath(s, s.kind === "polygon");
                }
                break;
        }
        if (s.fill && s.fill !== "none") ctx.fill();
        if (s.strokeWidth > 0 && s.stroke && s.stroke !== "none") ctx.stroke();
    }

    // Shared path builder for polygon (closed=true) and polyline (closed=false).
    // Honors per-segment `kind: "cubic"` to emit bezierCurveTo, otherwise lineTo.
    function drawPolyPath(s, closed) {
        const pts = s.points;
        const segs = s.segments;
        const n = pts.length / 2;
        const segCount = closed ? n : n - 1;
        ctx.moveTo(pts[0], pts[1]);
        for (let i = 0; i < segCount; i++) {
            const ni = ((i + 1) % n) * 2;
            const ex = pts[ni], ey = pts[ni + 1];
            const seg = segs && segs[i];
            if (seg && seg.kind === "cubic") {
                ctx.bezierCurveTo(seg.c1x, seg.c1y, seg.c2x, seg.c2y, ex, ey);
            } else {
                ctx.lineTo(ex, ey);
            }
        }
        if (closed) ctx.closePath();
    }

    function roundRect(c, x, y, w, h, r) {
        r = Math.min(r, w / 2, h / 2);
        c.moveTo(x + r, y);
        c.lineTo(x + w - r, y);
        c.quadraticCurveTo(x + w, y, x + w, y + r);
        c.lineTo(x + w, y + h - r);
        c.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        c.lineTo(x + r, y + h);
        c.quadraticCurveTo(x, y + h, x, y + h - r);
        c.lineTo(x, y + r);
        c.quadraticCurveTo(x, y, x + r, y);
    }

    function drawDragPreview() {
        if (drag.tool === "vertex") return; // live-edit handled by paint()
        // Inverse of the current paint transform — gives us "one icon unit = how
        // many user units after scaling". We divide 1 by this to get a 1-pixel line.
        const k = viewport.zoom * baseFit().scale;
        ctx.save();
        ctx.fillStyle = "rgba(96, 165, 250, 0.25)";
        ctx.strokeStyle = "#60a5fa";
        ctx.lineWidth = 1 / k;
        ctx.setLineDash([4 * ctx.lineWidth, 4 * ctx.lineWidth]);
        ctx.beginPath();
        if (drag.tool === "rect") {
            const x = Math.min(drag.startX, drag.x);
            const y = Math.min(drag.startY, drag.y);
            const w = Math.abs(drag.x - drag.startX);
            const h = Math.abs(drag.y - drag.startY);
            ctx.rect(x, y, w, h);
        } else if (drag.tool === "circle") {
            const x = Math.min(drag.startX, drag.x);
            const y = Math.min(drag.startY, drag.y);
            const w = Math.abs(drag.x - drag.startX);
            const h = Math.abs(drag.y - drag.startY);
            ctx.ellipse(x + w / 2, y + h / 2, Math.max(0.01, w / 2), Math.max(0.01, h / 2), 0, 0, Math.PI * 2);
        } else if (drag.tool === "line") {
            ctx.moveTo(drag.startX, drag.startY);
            ctx.lineTo(drag.x, drag.y);
        } else if ((drag.tool === "polygon" || drag.tool === "polyline") && drag.points.length > 0) {
            ctx.moveTo(drag.points[0].x, drag.points[0].y);
            for (let i = 1; i < drag.points.length; i++)
                ctx.lineTo(drag.points[i].x, drag.points[i].y);
            if (drag.cursor) ctx.lineTo(drag.cursor.x, drag.cursor.y);
        }
        ctx.stroke();
        if (drag.tool !== "line" && drag.tool !== "polygon" && drag.tool !== "polyline") ctx.fill();
        // Polygon preview also shows a translucent fill so the user sees what
        // will be filled on commit; polyline is stroke-only.
        if (drag.tool === "polygon") ctx.fill();
        ctx.restore();

        // Highlight the first polygon vertex so the user can see where to click to close.
        if (drag.tool === "polygon" && drag.points.length >= 3) {
            const first = drag.points[0];
            const screenScale = iconToScreenScale();
            ctx.save();
            ctx.fillStyle = "#fcd34d";
            ctx.strokeStyle = "#0a0a0c";
            ctx.lineWidth = 1 / k;
            ctx.beginPath();
            ctx.arc(first.x, first.y, HIT_PX / screenScale, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
            ctx.restore();
        }
    }

    function findShape(id) {
        if (!state) return null;
        for (const layer of state.layers) {
            if (layer.kind !== "shape") continue;
            for (const s of layer.shapes) if (s.id === id) return s;
        }
        return null;
    }

    function drawSelectionHandles(s) {
        const bb = shapeBounds(s);
        if (!bb) return;
        const tl = iconToCanvas(bb.x, bb.y);
        const br = iconToCanvas(bb.x + bb.w, bb.y + bb.h);
        ctx.save();
        ctx.strokeStyle = "#60a5fa";
        ctx.lineWidth = 1;
        ctx.setLineDash([4, 3]);
        ctx.strokeRect(tl.x, tl.y, br.x - tl.x, br.y - tl.y);
        ctx.setLineDash([]);
        ctx.fillStyle = "#60a5fa";

        // Pick handle positions matching the hit-test in hitHandle().
        let handles;
        if (s.kind === "line") {
            handles = [[s.x, s.y], [s.x + s.w, s.y + s.h]];
        } else {
            handles = [
                [bb.x, bb.y], [bb.x + bb.w, bb.y],
                [bb.x, bb.y + bb.h], [bb.x + bb.w, bb.y + bb.h],
            ];
        }
        for (const [hx, hy] of handles) {
            const p = iconToCanvas(hx, hy);
            ctx.fillRect(p.x - 4, p.y - 4, 8, 8);
        }

        // Polygon / polyline vertex handles — yellow squares, distinct from the
        // blue bounding-box corner handles.
        if ((s.kind === "polygon" || s.kind === "polyline") && s.points) {
            // Bezier control points first, so vertex squares overlap them when adjacent.
            if (s.segments) {
                const n = s.points.length / 2;
                ctx.strokeStyle = "rgba(245, 158, 11, 0.55)";
                ctx.setLineDash([3, 3]);
                ctx.lineWidth = 1;
                for (let i = 0; i < s.segments.length; i++) {
                    const seg = s.segments[i];
                    if (!seg || seg.kind !== "cubic") continue;
                    const ni = (i + 1) % n;
                    const v1 = iconToCanvas(s.points[i * 2], s.points[i * 2 + 1]);
                    const v2 = iconToCanvas(s.points[ni * 2], s.points[ni * 2 + 1]);
                    const c1 = iconToCanvas(seg.c1x, seg.c1y);
                    const c2 = iconToCanvas(seg.c2x, seg.c2y);
                    ctx.beginPath();
                    ctx.moveTo(v1.x, v1.y); ctx.lineTo(c1.x, c1.y);
                    ctx.moveTo(v2.x, v2.y); ctx.lineTo(c2.x, c2.y);
                    ctx.stroke();
                }
                ctx.setLineDash([]);
                // Round orange handles for control points.
                ctx.fillStyle = "#f59e0b";
                ctx.strokeStyle = "#0a0a0c";
                for (let i = 0; i < s.segments.length; i++) {
                    const seg = s.segments[i];
                    if (!seg || seg.kind !== "cubic") continue;
                    const c1 = iconToCanvas(seg.c1x, seg.c1y);
                    const c2 = iconToCanvas(seg.c2x, seg.c2y);
                    ctx.beginPath(); ctx.arc(c1.x, c1.y, 5, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
                    ctx.beginPath(); ctx.arc(c2.x, c2.y, 5, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
                }
            }

            // Vertex squares — yellow.
            ctx.fillStyle = "#fcd34d";
            ctx.strokeStyle = "#0a0a0c";
            ctx.lineWidth = 1;
            for (let i = 0; i < s.points.length; i += 2) {
                const p = iconToCanvas(s.points[i], s.points[i + 1]);
                ctx.beginPath();
                ctx.rect(p.x - 4, p.y - 4, 8, 8);
                ctx.fill();
                ctx.stroke();
            }
        }
        ctx.restore();
    }

    function shapeBounds(s) {
        if (s.kind === "polygon" || s.kind === "polyline") {
            if (!s.points || s.points.length < 2) return null;
            let minX = s.points[0], maxX = s.points[0];
            let minY = s.points[1], maxY = s.points[1];
            for (let i = 2; i < s.points.length; i += 2) {
                minX = Math.min(minX, s.points[i]);
                maxX = Math.max(maxX, s.points[i]);
                minY = Math.min(minY, s.points[i + 1]);
                maxY = Math.max(maxY, s.points[i + 1]);
            }
            // Cubic segments can bulge past the endpoint polygon. The control
            // points sit on the convex hull that bounds the curve, so expanding
            // by them is a safe (slightly loose) bound — and matches what the
            // user can grab with the orange handles.
            if (s.segments) {
                for (let i = 0; i < s.segments.length; i++) {
                    const seg = s.segments[i];
                    if (!seg || seg.kind !== "cubic") continue;
                    if (typeof seg.c1x === "number") {
                        minX = Math.min(minX, seg.c1x); maxX = Math.max(maxX, seg.c1x);
                        minY = Math.min(minY, seg.c1y); maxY = Math.max(maxY, seg.c1y);
                    }
                    if (typeof seg.c2x === "number") {
                        minX = Math.min(minX, seg.c2x); maxX = Math.max(maxX, seg.c2x);
                        minY = Math.min(minY, seg.c2y); maxY = Math.max(maxY, seg.c2y);
                    }
                }
            }
            return { x: minX, y: minY, w: maxX - minX, h: maxY - minY };
        }
        if (s.kind === "line") {
            return {
                x: Math.min(s.x, s.x + s.w),
                y: Math.min(s.y, s.y + s.h),
                w: Math.abs(s.w),
                h: Math.abs(s.h),
            };
        }
        return { x: s.x, y: s.y, w: s.w, h: s.h };
    }

    // ────────────────────────────────────────────────────────────
    // File pickers — used by Blazor for Open / Save / Export. Tries the modern
    // File System Access API first (Chromium-native, surfaces a real native
    // dialog and writes directly to disk), then falls back to the legacy
    // <input type=file> / <a download> mechanism which always works.
    //
    // The return shape from openFilePicker is { name, base64 } so the C# side
    // can decode bytes without depending on the path. saveFilePicker returns the
    // chosen filename (or null on cancel, "<name> (Downloads)" on fallback).

    // Convert an ArrayBuffer → base64 in 32 KB chunks (String.fromCharCode.apply
    // blows up on large inputs).
    function bufToB64(buf) {
        const bytes = new Uint8Array(buf);
        let bin = "";
        const CHUNK = 0x8000;
        for (let i = 0; i < bytes.length; i += CHUNK) {
            bin += String.fromCharCode.apply(null, bytes.subarray(i, Math.min(i + CHUNK, bytes.length)));
        }
        return btoa(bin);
    }

    studio.openFilePicker = async function (filterDesc, filterExt) {
        const accept = "." + filterExt;
        if (window.showOpenFilePicker) {
            try {
                const [handle] = await window.showOpenFilePicker({
                    types: [{
                        description: filterDesc,
                        accept: { "*/*": [accept] }
                    }],
                    excludeAcceptAllOption: false,
                    multiple: false
                });
                const file = await handle.getFile();
                const buf = await file.arrayBuffer();
                return { name: handle.name, base64: bufToB64(buf) };
            } catch (e) {
                if (e && e.name === "AbortError") return null;
                console.warn("showOpenFilePicker failed, falling back:", e);
            }
        }
        return new Promise(resolve => {
            const input = document.createElement("input");
            input.type = "file";
            input.accept = accept;
            input.onchange = async () => {
                const f = input.files && input.files[0];
                if (!f) { resolve(null); return; }
                const buf = await f.arrayBuffer();
                resolve({ name: f.name, base64: bufToB64(buf) });
            };
            // Some browsers don't fire change if user cancels; resolve(null) after
            // a sane timeout if the picker hasn't fired by the time the body regains
            // focus.
            input.click();
        });
    };

    studio.saveFilePicker = async function (suggestedName, filterDesc, filterExt, contentBase64) {
        const accept = "." + filterExt;
        const bin = atob(contentBase64);
        const buf = new Uint8Array(bin.length);
        for (let i = 0; i < bin.length; i++) buf[i] = bin.charCodeAt(i);

        if (window.showSaveFilePicker) {
            try {
                const handle = await window.showSaveFilePicker({
                    suggestedName: suggestedName,
                    types: [{
                        description: filterDesc,
                        accept: { "*/*": [accept] }
                    }]
                });
                const writable = await handle.createWritable();
                await writable.write(buf);
                await writable.close();
                return handle.name;
            } catch (e) {
                if (e && e.name === "AbortError") return null;
                console.warn("showSaveFilePicker failed, falling back:", e);
            }
        }
        // Fallback: trigger a browser download — goes to Downloads folder.
        const blob = new Blob([buf]);
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = suggestedName;
        document.body.appendChild(a);
        a.click();
        setTimeout(() => { URL.revokeObjectURL(url); a.remove(); }, 100);
        return suggestedName + " (Downloads)";
    };

    studio.pickFile = function (accept) {
        return new Promise(resolve => {
            const input = document.createElement("input");
            input.type = "file";
            input.accept = accept || "";
            input.onchange = () => {
                const f = input.files && input.files[0];
                if (!f) { resolve(null); return; }
                const reader = new FileReader();
                reader.onload = () => resolve(reader.result);
                reader.onerror = () => resolve(null);
                reader.readAsDataURL(f);
            };
            input.click();
        });
    };

    studio.download = function (fileName, mime, base64) {
        const bin = atob(base64);
        const buf = new Uint8Array(bin.length);
        for (let i = 0; i < bin.length; i++) buf[i] = bin.charCodeAt(i);
        const blob = new Blob([buf], { type: mime });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        setTimeout(() => { URL.revokeObjectURL(url); a.remove(); }, 100);
    };
})();
