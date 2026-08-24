// ObjEditor — hand-rolled WebGL viewer.
//
// Coordinate convention: C# stores Unity-axis positions (+X right, +Y up,
// +Z forward, left-handed). WebGL is right-handed with +Z toward the camera,
// so we mirror Z when uploading vertices. The visible scene matches Unity's
// "+Z is forward" intuition.
//
// State flow: C# is authoritative. setState() rebuilds GPU buffers from the
// snapshot. No interactive picking yet — that lands in the next stage.
(function () {
    const editor = (window.objEditor = window.objEditor || {});

    // -----------------------------------------------------------------------
    // Multi-viewport registry. Each canvas the C# side attaches gets its own
    // `vp` state object (GL context, buffers, camera, etc). The module-level
    // `V` is a rebindable pointer to the *active* viewport — every entry
    // point (draw loop, event handler, public API call) sets V to the right
    // vp before falling through to the existing single-viewport logic, so
    // the bulk of the renderer is unchanged.
    // -----------------------------------------------------------------------
    const VIEWPORTS = new Map(); // canvas → vp
    let V = null;

    // Decoded-once cache of every material texture the C# side has pushed.
    // GL handles aren't shareable across contexts and per-viewport `textures`
    // maps get wiped on detach, so when a new viewport attaches (tab swap,
    // quad-view toggle, etc.) we re-upload from these cached Images.
    const textureCache = new Map(); // matId → HTMLImageElement (already decoded)

    function makeVpState(viewMode) {
        return {
            canvas: null, host: null, dotnet: null,
            gl: null,
            // Mode: 'perspective' (default), 'top', 'front', 'side'. Ortho
            // modes use a fixed camera direction + an orthographic projection
            // sized off camera.distance.
            viewMode: viewMode || 'perspective',
            meshProg: null, lineProg: null, markerProg: null,
            meshBuf: null, meshCount: 0,
            wireBuf: null, wireCount: 0,
            connEdgeBuf: null, connEdgeCount: 0,
            axisBuf: null, axisCount: 0,
            pointBuf: null, pointCount: 0,
            placeLineBuf: null, placeLineCount: 0,
            placePointBuf: null, placePointCount: 0,
            gizmoLineBuf: null, gizmoLineCount: 0,
            gizmoTipBuf: null, gizmoTipCount: 0,
            gizmoTips: [],
            camera: { theta: 0.6, phi: 0.5, distance: 4, target: [0, 0, 0] },
            // Multi-light state: flat 12-float arrays (4 lights × xyz) +
            // ambient floor. The shader expects pre-normalized directions
            // and zero-color slots for empty entries; default is one key
            // light pointing at (0.5, 0.9, 0.4)/|len| so a fresh viewport
            // renders the same as the old single-light look.
            lightDirs: new Float32Array([
                0.4525, 0.8145, 0.3620,  0, 0, 0,  0, 0, 0,  0, 0, 0,
            ]),
            lightCols: new Float32Array([1, 1, 1,  0, 0, 0,  0, 0, 0,  0, 0, 0]),
            ambient: 0.25,
            meshBatches: new Map(),
            textures: new Map(),
            defaultTex: null,
            drag: null,
            state: null,
            resizeObserver: null,
            raf: 0,
            dpr: 1,
            gridBuf: null,
            gridCount: 0,
        };
    }

    // Set the active viewport pointer. All event handlers and per-frame
    // entry points must call this before touching V.* so the right state is
    // in play. Returns the resolved vp (or null if the canvas is unknown).
    function activate(canvas) {
        V = VIEWPORTS.get(canvas) || null;
        return V;
    }

    editor.attachViewport = function (canvasEl, hostEl, dotnetRef, viewMode) {
        try { return _attachViewport(canvasEl, hostEl, dotnetRef, viewMode); }
        catch (e) {
            // Without this trap, a shader-compile failure here would just
            // surface as a generic JSException on the C# side with no body.
            // We want the real stack visible in objeditor.log via LogFromJs.
            const msg = 'attachViewport failed: ' + (e && e.stack ? e.stack : e);
            console.error(msg);
            try {
                if (dotnetRef && dotnetRef.invokeMethodAsync) {
                    dotnetRef.invokeMethodAsync('LogFromJs', msg).catch(() => {});
                }
            } catch (_) {}
            throw e;
        }
    };

    function _attachViewport(canvasEl, hostEl, dotnetRef, viewMode) {
        // Replacing an existing entry for the same canvas is a stale remount
        // (e.g. tab swap): tear it down first so the new GL context owns
        // fresh buffers. Other viewports keep running untouched.
        const existing = VIEWPORTS.get(canvasEl);
        if (existing) {
            V = existing;
            teardownViewport();
            VIEWPORTS.delete(canvasEl);
        }

        const vp = makeVpState(viewMode);
        VIEWPORTS.set(canvasEl, vp);
        V = vp;
        V.canvas = canvasEl;
        V.host = hostEl;
        V.dotnet = dotnetRef;
        const gl = canvasEl.getContext('webgl2', { antialias: true })
                || canvasEl.getContext('webgl', { antialias: true });
        if (!gl) {
            console.error('WebGL is not available in this WebView.');
            return;
        }
        V.gl = gl;

        V.meshProg = buildProgram(gl, MESH_VS, MESH_FS, ['aPos', 'aNormal', 'aColor', 'aUv']);
        // Mesh-only uniforms — buildProgram pre-caches uMVP/uModel only, so
        // fetch the rest ad-hoc here.
        V.meshProg.uTex = gl.getUniformLocation(V.meshProg.program, 'uTex');
        V.meshProg.uLightDirs = gl.getUniformLocation(V.meshProg.program, 'uLightDirs');
        V.meshProg.uLightCols = gl.getUniformLocation(V.meshProg.program, 'uLightCols');
        V.meshProg.uAmbient = gl.getUniformLocation(V.meshProg.program, 'uAmbient');
        V.lineProg = buildProgram(gl, LINE_VS, LINE_FS, ['aPos', 'aColor']);
        V.markerProg = buildProgram(gl, MARKER_VS, LINE_FS, ['aPos', 'aColor']);

        V.meshBuf = gl.createBuffer();
        V.wireBuf = gl.createBuffer();
        V.connEdgeBuf = gl.createBuffer();
        V.axisBuf = gl.createBuffer();
        V.pointBuf = gl.createBuffer();
        V.placeLineBuf = gl.createBuffer();
        V.placePointBuf = gl.createBuffer();
        V.gizmoLineBuf = gl.createBuffer();
        V.gizmoTipBuf = gl.createBuffer();

        // Single-sided rendering, Unity convention.
        // Unity defines "front-facing" as CW winding viewed from the front,
        // and the model stores faces that way. After our Z-mirror, that CW
        // winding still appears as CW in WebGL screen space, so set
        // frontFace = CW (overriding the WebGL default CCW). The cross
        // products derived from this winding point *inward*, so we negate
        // them when emitting normals — see rebuildMeshBuffers and
        // rebuildPlacementBuffers below. With those negations the shader
        // lights the right side and the placement arrow points outward.
        gl.enable(gl.CULL_FACE);
        gl.cullFace(gl.BACK);
        gl.frontFace(gl.CW);

        // 1x1 white default texture, used by the untextured mesh pass so the
        // single mesh shader can always sample uTex without branching.
        V.defaultTex = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, V.defaultTex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0, gl.RGBA, gl.UNSIGNED_BYTE,
            new Uint8Array([255, 255, 255, 255]));
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

        // Re-upload every cached texture image into this viewport's GL context.
        // Without this, a tab swap / quad-view toggle would lose all textures
        // because each new viewport starts with an empty `textures` map.
        for (const [matId, img] of textureCache) {
            uploadTextureToVp(V, matId, img);
        }

        // Axis gizmo is static. Build once.
        buildAxisBuffer();

        sizeToHost();
        installMouse();
        // sizeToHost and the resize observer both reference V — rebind it
        // to this vp before the observer fires (otherwise a resize on a
        // backgrounded viewport would mis-size whichever V was active).
        const localVp = vp;
        V.resizeObserver = new ResizeObserver(() => { V = localVp; sizeToHost(); });
        V.resizeObserver.observe(hostEl);

        // Each viewport runs its own RAF loop. The loop's only job is to
        // make THIS vp active, then call the shared draw() so any code
        // touching V is operating on the right state.
        const loop = () => {
            if (!localVp.gl) return; // torn down
            V = localVp;
            localVp.raf = requestAnimationFrame(loop);
            draw();
        };
        loop();
    }

    // Called from C# DisposeAsync. Looks up the registered viewport for the
    // canvas and tears it down. Other viewports keep running.
    editor.detachViewport = function (canvasEl) {
        const vp = VIEWPORTS.get(canvasEl);
        if (!vp) return;
        V = vp;
        teardownViewport();
        VIEWPORTS.delete(canvasEl);
        if (V === vp) V = VIEWPORTS.values().next().value || null;
    };

    function teardownViewport() {
        cancelAnimationFrame(V.raf);
        if (V.resizeObserver) V.resizeObserver.disconnect();
        if (V.canvas) removeMouse();
        const gl = V.gl;
        if (gl) {
            [V.meshBuf, V.wireBuf, V.connEdgeBuf, V.axisBuf, V.pointBuf,
             V.placeLineBuf, V.placePointBuf,
             V.gizmoLineBuf, V.gizmoTipBuf, V.gridBuf].forEach(b => b && gl.deleteBuffer(b));
            if (V.meshProg) gl.deleteProgram(V.meshProg.program);
            if (V.lineProg) gl.deleteProgram(V.lineProg.program);
            if (V.markerProg) gl.deleteProgram(V.markerProg.program);
        }
        if (gl) {
            for (const t of V.textures.values()) gl.deleteTexture(t);
            if (V.defaultTex) gl.deleteTexture(V.defaultTex);
            for (const b of V.meshBatches.values()) gl.deleteBuffer(b.buf);
        }
        V.gridBuf = null;
        V.gridCount = 0;
        V.textures.clear();
        V.meshBatches.clear();
        V.defaultTex = null;
        V.canvas = V.host = V.dotnet = V.gl = null;
        V.meshProg = V.lineProg = V.markerProg = null;
        V.meshBuf = V.wireBuf = V.connEdgeBuf = V.axisBuf = V.pointBuf = null;
        V.placeLineBuf = V.placePointBuf = null;
        V.gizmoLineBuf = V.gizmoTipBuf = null;
        V.gizmoTips = [];
        V.state = null;
        V.resizeObserver = null;
    }

    // Material texture API — bytes are passed as base64 to avoid binary
    // marshaling through JSInterop. We decode via an HTMLImageElement so the
    // browser handles PNG/JPEG/GIF transparently. The upload is asynchronous;
    // until it completes the renderer falls back to the default white texture
    // (faces still draw, just with the diffuse color only).
    editor.setMaterialTexture = function (matId, base64) {
        // Decode once into an HTMLImageElement; cache for future re-uploads
        // and push to every currently-attached viewport. (Caching even works
        // when there are no viewports yet — they'll pick it up on attach.)
        const bin = atob(base64);
        const bytes = new Uint8Array(bin.length);
        for (let i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
        const url = URL.createObjectURL(new Blob([bytes]));
        const img = new Image();
        img.onload = function () {
            URL.revokeObjectURL(url);
            textureCache.set(matId, img);
            for (const vp of VIEWPORTS.values()) {
                if (vp.gl) uploadTextureToVp(vp, matId, img);
            }
        };
        img.onerror = function () {
            URL.revokeObjectURL(url);
            console.warn('setMaterialTexture decode failed for material ' + matId);
        };
        img.src = url;
    };

    function uploadTextureToVp(vp, matId, img) {
        const gl = vp.gl;
        const existing = vp.textures.get(matId);
        const tex = existing || gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, tex);
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.REPEAT);
        vp.textures.set(matId, tex);
    }

    editor.clearMaterialTexture = function (matId) {
        // Drop the cached image AND each viewport's GL texture. Skip the
        // cache step and a re-attached viewport would resurrect the old
        // texture from the cache.
        textureCache.delete(matId);
        for (const vp of VIEWPORTS.values()) {
            const t = vp.textures.get(matId);
            if (t && vp.gl) vp.gl.deleteTexture(t);
            vp.textures.delete(matId);
        }
    };

    editor.setState = function (snapshot) {
        // The C# snapshot is identical for every viewport; only the camera
        // and per-context GL buffers differ. Push to each viewport in turn.
        for (const vp of VIEWPORTS.values()) {
            V = vp;
            try {
                V.state = snapshot;
                rebuildMeshBuffers();
                rebuildPlacementBuffers();
            } catch (e) {
                console.error('objEditor.setState failed:', e && e.stack ? e.stack : e);
            }
        }
    };

    editor.frameAll = function () {
        // Re-frame every attached viewport so adding a new pane immediately
        // gets a sensible camera target/distance.
        for (const vp of VIEWPORTS.values()) {
            V = vp;
            if (!V.state || V.state.vertices.length === 0) continue;
            let cx = 0, cy = 0, cz = 0;
            let minX = +Infinity, maxX = -Infinity, minY = +Infinity, maxY = -Infinity, minZ = +Infinity, maxZ = -Infinity;
            for (const v of V.state.vertices) {
                const z = -v.z; // Unity → WebGL
                cx += v.x; cy += v.y; cz += z;
                if (v.x < minX) minX = v.x; if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y; if (v.y > maxY) maxY = v.y;
                if (z   < minZ) minZ = z;   if (z   > maxZ) maxZ = z;
            }
            const n = V.state.vertices.length;
            V.camera.target = [cx / n, cy / n, cz / n];
            const dx = maxX - minX, dy = maxY - minY, dz = maxZ - minZ;
            V.camera.distance = Math.max(2, Math.sqrt(dx*dx + dy*dy + dz*dz) * 1.4);
        }
    };

    // -----------------------------------------------------------------------
    // Shaders.
    //
    // The mesh shader does per-vertex flat-ish shading (we duplicate vertices
    // per face so each triangle's normal is constant). The line shader is
    // used for the axis gizmo and wireframe overlay.
    // -----------------------------------------------------------------------
    const MESH_VS = `
        attribute vec3 aPos;
        attribute vec3 aNormal;
        attribute vec3 aColor;
        attribute vec2 aUv;
        uniform mat4 uMVP;
        uniform mat4 uModel;
        varying vec3 vNormal;
        varying vec3 vColor;
        varying vec2 vUv;
        void main() {
            gl_Position = uMVP * vec4(aPos, 1.0);
            // Model rotation is identity here; pass the raw normal through.
            vNormal = aNormal;
            vColor = aColor;
            vUv = aUv;
        }`;
    const MESH_FS = `
        precision mediump float;
        #define MAX_LIGHTS 4
        varying vec3 vNormal;
        varying vec3 vColor;
        varying vec2 vUv;
        uniform sampler2D uTex;
        // Lights are pre-normalized on the CPU and unused slots are zero-color,
        // so the shader can always sum MAX_LIGHTS contributions without a
        // dynamic break. (Backticks deliberately omitted from this comment:
        // the host source is a JS template literal and backticks would close
        // it mid-way.) Some WebGL 1 drivers reject break inside arrays
        // indexed by a loop variable in fragment shaders.
        uniform vec3 uLightDirs[MAX_LIGHTS];
        uniform vec3 uLightCols[MAX_LIGHTS];
        uniform float uAmbient;
        void main() {
            vec3 N = normalize(vNormal);
            vec4 tex = texture2D(uTex, vUv);
            vec3 albedo = vColor * tex.rgb;
            vec3 lit = albedo * uAmbient;
            for (int i = 0; i < MAX_LIGHTS; i++) {
                float k = max(dot(N, uLightDirs[i]), 0.0);
                lit += albedo * k * uLightCols[i];
            }
            gl_FragColor = vec4(lit, 1.0);
        }`;

    const LINE_VS = `
        attribute vec3 aPos;
        attribute vec3 aColor;
        uniform mat4 uMVP;
        varying vec3 vColor;
        void main() {
            gl_Position = uMVP * vec4(aPos, 1.0);
            // Used only when drawing as gl.POINTS; harmless for gl.LINES.
            // Bigger than the visible disc so the hit-test radius below
            // (PICK_PX) matches what the user actually sees.
            gl_PointSize = 9.0;
            vColor = aColor;
        }`;
    const LINE_FS = `
        precision mediump float;
        varying vec3 vColor;
        void main() { gl_FragColor = vec4(vColor, 1.0); }`;

    // Oversized point shader for placement markers — same geometry as the
    // line program, just a fatter gl_PointSize so the click order reads
    // cleanly on top of regular vertex handles.
    const MARKER_VS = `
        attribute vec3 aPos;
        attribute vec3 aColor;
        uniform mat4 uMVP;
        varying vec3 vColor;
        void main() {
            gl_Position = uMVP * vec4(aPos, 1.0);
            gl_PointSize = 16.0;
            vColor = aColor;
        }`;

    function buildProgram(gl, vsSrc, fsSrc, attribNames) {
        const vs = compile(gl, gl.VERTEX_SHADER, vsSrc);
        const fs = compile(gl, gl.FRAGMENT_SHADER, fsSrc);
        const program = gl.createProgram();
        gl.attachShader(program, vs);
        gl.attachShader(program, fs);
        attribNames.forEach((n, i) => gl.bindAttribLocation(program, i, n));
        gl.linkProgram(program);
        if (!gl.getProgramParameter(program, gl.LINK_STATUS))
            throw new Error('link: ' + gl.getProgramInfoLog(program));
        return {
            program,
            attribs: Object.fromEntries(attribNames.map((n, i) => [n, i])),
            uMVP: gl.getUniformLocation(program, 'uMVP'),
            uModel: gl.getUniformLocation(program, 'uModel'),
        };
    }
    function compile(gl, type, src) {
        const sh = gl.createShader(type);
        gl.shaderSource(sh, src);
        gl.compileShader(sh);
        if (!gl.getShaderParameter(sh, gl.COMPILE_STATUS))
            throw new Error('shader: ' + gl.getShaderInfoLog(sh));
        return sh;
    }

    // -----------------------------------------------------------------------
    // Buffer building: convert the C# snapshot to GPU-friendly arrays.
    //
    // Faces are fan-triangulated (good for convex polygons; n-gons that aren't
    // convex will look wrong — we triangulate properly later). Each triangle
    // gets its own copy of the three vertices so we can give it a constant
    // face normal for flat shading.
    // -----------------------------------------------------------------------
    function rebuildMeshBuffers() {
        const gl = V.gl;
        if (!gl || !V.state) return;

        const vertById = new Map(V.state.vertices.map(v => [v.id, v]));
        const matById = new Map(V.state.materials.map(m => [m.id, m]));
        const uvById = new Map((V.state.uvs || []).map(u => [u.id, u]));

        // Per-material triangle arrays keyed by `matId` for textured faces.
        // Untextured / texture-less-material faces accumulate into `triArr`.
        // Each entry is 11 floats/vertex (pos[3] + normal[3] + color[3] + uv[2]).
        const triArr = [];
        const texTriArr = new Map();
        const wireArr = [];  // 6 floats/vertex (pos + color)
        const ptArr = [];    // 6 floats/vertex (pos + color)
        const selVerts = new Set(V.state.selectedVertexIds);
        const selFaces = new Set(V.state.selectedFaceIds);

        // First pass: compute each face's outward normal (Unity CW → flip).
        const faceNormals = new Map();
        for (const f of V.state.faces) {
            if (f.v.length < 3) continue;
            const a = unityToGl(vertById.get(f.v[0]));
            const b = unityToGl(vertById.get(f.v[1]));
            const c = unityToGl(vertById.get(f.v[2]));
            if (!a || !b || !c) continue;
            const ni = triNormal(a, b, c);
            faceNormals.set(f.id, [-ni[0], -ni[1], -ni[2]]);
        }

        // Second pass: smoothing-group-aware normal accumulator. Faces in
        // group 0 are flat (no contribution); each non-zero group has its
        // own per-vertex accumulator keyed `vid|group`. A vertex shared by
        // two different groups gets two separate smoothed normals — that's
        // how hard edges between groups fall out automatically.
        const smoothNormalByKey = new Map();
        function smoothKey(vid, group) { return vid + '|' + group; }
        for (const f of V.state.faces) {
            const group = f.s | 0;
            if (group === 0) continue;
            const fn = faceNormals.get(f.id);
            if (!fn) continue;
            for (const vid of f.v) {
                const key = smoothKey(vid, group);
                const acc = smoothNormalByKey.get(key);
                if (acc) { acc[0] += fn[0]; acc[1] += fn[1]; acc[2] += fn[2]; }
                else smoothNormalByKey.set(key, [fn[0], fn[1], fn[2]]);
            }
        }
        for (const [_, acc] of smoothNormalByKey) {
            const len = Math.hypot(acc[0], acc[1], acc[2]);
            if (len > 1e-9) { acc[0] /= len; acc[1] /= len; acc[2] /= len; }
        }

        for (const f of V.state.faces) {
            if (f.v.length < 3) continue;
            const mat = (f.m != null) ? matById.get(f.m) : null;
            const baseColor = mat ? [mat.r, mat.g, mat.b] : [0.6, 0.62, 0.7];
            const color = selFaces.has(f.id)
                ? [Math.min(1, baseColor[0] * 0.5 + 0.25),
                   Math.min(1, baseColor[1] * 0.5 + 0.45),
                   Math.min(1, baseColor[2] * 0.5 + 0.85)]
                : baseColor;

            const corners = f.v.map(id => unityToGl(vertById.get(id)));
            const fn = faceNormals.get(f.id) || [0, 1, 0];
            const group = f.s | 0;
            const cornerNormals = f.v.map(id => {
                if (group !== 0) {
                    const sn = smoothNormalByKey.get(smoothKey(id, group));
                    if (sn) return sn;
                }
                return fn;
            });
            // Resolve per-corner UVs from id → (u,v). Missing or unknown
            // ids fall back to (0,0) so the untextured pass still has valid
            // values for the always-sample fragment shader.
            const cornerUvs = (f.uv || []).map(id => {
                if (id == null) return [0, 0];
                const u = uvById.get(id);
                return u ? [u.u, u.v] : [0, 0];
            });
            while (cornerUvs.length < corners.length) cornerUvs.push([0, 0]);

            // Faces whose material has a texture go into a per-material
            // batch; the rest into the shared untextured array.
            const target = (mat && mat.tex) ? bucketFor(f.m) : triArr;
            // Fan from corner 0.
            for (let i = 1; i < corners.length - 1; i++) {
                const a = corners[0], b = corners[i], c = corners[i + 1];
                if (!a || !b || !c) continue;
                pushTri(target, a, cornerNormals[0], color, cornerUvs[0]);
                pushTri(target, b, cornerNormals[i], color, cornerUvs[i]);
                pushTri(target, c, cornerNormals[i + 1], color, cornerUvs[i + 1]);
            }
            // Wireframe edges (always drawn over the mesh).
            for (let i = 0; i < corners.length; i++) {
                const a = corners[i], b = corners[(i + 1) % corners.length];
                if (!a || !b) continue;
                const wc = selFaces.has(f.id) ? [0.4, 0.7, 1.0] : [0.0, 0.0, 0.0];
                wireArr.push(a[0], a[1], a[2], wc[0], wc[1], wc[2]);
                wireArr.push(b[0], b[1], b[2], wc[0], wc[1], wc[2]);
            }
        }
        function bucketFor(matId) {
            let arr = texTriArr.get(matId);
            if (!arr) { arr = []; texTriArr.set(matId, arr); }
            return arr;
        }

        // Vertex point handles (rendered as gl.POINTS).
        for (const v of V.state.vertices) {
            const p = unityToGl(v);
            const c = selVerts.has(v.id) ? [1.0, 0.55, 0.25] : [0.95, 0.95, 0.95];
            ptArr.push(p[0], p[1], p[2], c[0], c[1], c[2]);
        }

        // Helper: only upload when there's data. A 0-byte gl.bufferData call
        // is technically valid per spec but trips up some WebGL paths, and
        // the matching draw is already guarded by `count > 0`.
        function upload(buf, arr) {
            if (!buf) return 0;
            if (arr.length === 0) return 0;
            gl.bindBuffer(gl.ARRAY_BUFFER, buf);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(arr), gl.STATIC_DRAW);
            return arr.length;
        }
        // Vertex layout now has UVs → 11 floats per vertex.
        V.meshCount = upload(V.meshBuf, triArr) / 11;
        V.wireCount = upload(V.wireBuf, wireArr) / 6;
        V.pointCount = upload(V.pointBuf, ptArr) / 6;

        // Per-material textured batches. Reuse existing buffers when the
        // material is still present; delete buffers for materials that
        // dropped out (texture removed or material deleted).
        const keepMats = new Set(texTriArr.keys());
        for (const [matId, batch] of [...V.meshBatches]) {
            if (!keepMats.has(matId)) {
                gl.deleteBuffer(batch.buf);
                V.meshBatches.delete(matId);
            }
        }
        // Drop GL textures for materials no longer present in the snapshot
        // (deleted material, or Replace() after Open). Leaks otherwise pile up.
        const allMatIds = new Set(V.state.materials.map(m => m.id));
        for (const [matId, tex] of [...V.textures]) {
            if (!allMatIds.has(matId)) {
                gl.deleteTexture(tex);
                V.textures.delete(matId);
            }
        }
        for (const [matId, arr] of texTriArr) {
            let batch = V.meshBatches.get(matId);
            if (!batch) {
                batch = { buf: gl.createBuffer(), count: 0 };
                V.meshBatches.set(matId, batch);
            }
            batch.count = upload(batch.buf, arr) / 11;
        }

        // Build connected-edge highlight in face mode.
        const connArr = [];
        if (V.state.mode === 'face' && selFaces.size > 0) {
            const selectedVerts = new Set();
            for (const f of V.state.faces) {
                if (selFaces.has(f.id)) for (const id of f.v) selectedVerts.add(id);
            }
            const hc = [1.0, 0.85, 0.30]; // amber
            for (const f of V.state.faces) {
                if (selFaces.has(f.id)) continue;
                if (f.v.length < 2) continue;
                for (let i = 0; i < f.v.length; i++) {
                    const aId = f.v[i];
                    const bId = f.v[(i + 1) % f.v.length];
                    if (!selectedVerts.has(aId) && !selectedVerts.has(bId)) continue;
                    const a = vertById.get(aId);
                    const b = vertById.get(bId);
                    if (!a || !b) continue;
                    const ap = unityToGl(a), bp = unityToGl(b);
                    connArr.push(ap[0], ap[1], ap[2], hc[0], hc[1], hc[2]);
                    connArr.push(bp[0], bp[1], bp[2], hc[0], hc[1], hc[2]);
                }
            }
        }
        if (connArr.length > 0 && V.connEdgeBuf) {
            gl.bindBuffer(gl.ARRAY_BUFFER, V.connEdgeBuf);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(connArr), gl.DYNAMIC_DRAW);
        }
        V.connEdgeCount = connArr.length / 6;
    }

    // Build the placement overlay buffers from V.state.placingVertexIds.
    //
    // Visual contract:
    //   - First vertex marker is green (the user can see where the polygon starts).
    //   - Latest vertex marker is yellow (so the user can see what they just placed).
    //   - Other markers are pale blue.
    //   - A polyline connects them in pick order. When ≥3 vertices are placed,
    //     a "closing" dashed-looking segment connects last→first (we just draw
    //     it as a normal line — a real dash needs a shader).
    //   - When ≥3 vertices are placed, an arrow from the polygon centroid shows
    //     the predicted front-face normal (computed from the first three picks).
    function rebuildPlacementBuffers() {
        const gl = V.gl;
        const s = V.state;
        if (!gl) return;
        if (!s || !s.placing || !s.placingVertexIds || s.placingVertexIds.length === 0) {
            V.placeLineCount = 0;
            V.placePointCount = 0;
            return;
        }
        const vertById = new Map(s.vertices.map(v => [v.id, v]));
        const picked = s.placingVertexIds
            .map(id => vertById.get(id))
            .filter(v => v != null)
            .map(unityToGl);

        const lineArr = [];
        const pointArr = [];

        // Markers.
        picked.forEach((p, i) => {
            let c;
            if (i === 0) c = [0.30, 1.00, 0.45];          // green start
            else if (i === picked.length - 1) c = [1.00, 0.90, 0.30]; // yellow latest
            else c = [0.55, 0.80, 1.00];                  // pale blue mid
            pointArr.push(p[0], p[1], p[2], c[0], c[1], c[2]);
        });

        // Polyline through picks in order.
        const lineColor = [0.55, 0.80, 1.00];
        for (let i = 0; i + 1 < picked.length; i++) {
            const a = picked[i], b = picked[i + 1];
            lineArr.push(a[0], a[1], a[2], lineColor[0], lineColor[1], lineColor[2]);
            lineArr.push(b[0], b[1], b[2], lineColor[0], lineColor[1], lineColor[2]);
        }
        // Closing edge (last → first) — drawn dimmer to read as "preview".
        if (picked.length >= 3) {
            const a = picked[picked.length - 1], b = picked[0];
            const dim = [0.30, 0.45, 0.65];
            lineArr.push(a[0], a[1], a[2], dim[0], dim[1], dim[2]);
            lineArr.push(b[0], b[1], b[2], dim[0], dim[1], dim[2]);
        }

        // Predicted-normal arrow from polygon centroid. Uses the first three
        // picks to determine winding — same as how the eventual face's first
        // triangle will be wound. Negate the cross product so the arrow points
        // OUTWARD from the front face under the Unity (CW=front) convention.
        if (picked.length >= 3) {
            const c = centroid(picked);
            const ni = triNormal(picked[0], picked[1], picked[2]);
            const n = [-ni[0], -ni[1], -ni[2]];
            const len = Math.max(0.2, polygonBoundLen(picked) * 0.35);
            const tip = [c[0] + n[0] * len, c[1] + n[1] * len, c[2] + n[2] * len];
            const arrowColor = [0.40, 0.70, 1.00];
            // shaft
            lineArr.push(c[0], c[1], c[2], arrowColor[0], arrowColor[1], arrowColor[2]);
            lineArr.push(tip[0], tip[1], tip[2], arrowColor[0], arrowColor[1], arrowColor[2]);
            // Arrowhead — two short segments perpendicular to the shaft. Pick
            // any vector not parallel to n, take the cross, and scale.
            const side = arrowHeadSide(n);
            const head1 = [tip[0] - n[0] * len * 0.18 + side[0] * len * 0.10,
                           tip[1] - n[1] * len * 0.18 + side[1] * len * 0.10,
                           tip[2] - n[2] * len * 0.18 + side[2] * len * 0.10];
            const head2 = [tip[0] - n[0] * len * 0.18 - side[0] * len * 0.10,
                           tip[1] - n[1] * len * 0.18 - side[1] * len * 0.10,
                           tip[2] - n[2] * len * 0.18 - side[2] * len * 0.10];
            lineArr.push(tip[0], tip[1], tip[2], arrowColor[0], arrowColor[1], arrowColor[2]);
            lineArr.push(head1[0], head1[1], head1[2], arrowColor[0], arrowColor[1], arrowColor[2]);
            lineArr.push(tip[0], tip[1], tip[2], arrowColor[0], arrowColor[1], arrowColor[2]);
            lineArr.push(head2[0], head2[1], head2[2], arrowColor[0], arrowColor[1], arrowColor[2]);
        }

        if (lineArr.length > 0 && V.placeLineBuf) {
            gl.bindBuffer(gl.ARRAY_BUFFER, V.placeLineBuf);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(lineArr), gl.DYNAMIC_DRAW);
        }
        V.placeLineCount = lineArr.length / 6;

        if (pointArr.length > 0 && V.placePointBuf) {
            gl.bindBuffer(gl.ARRAY_BUFFER, V.placePointBuf);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(pointArr), gl.DYNAMIC_DRAW);
        }
        V.placePointCount = pointArr.length / 6;
    }

    function centroid(pts) {
        let cx = 0, cy = 0, cz = 0;
        for (const p of pts) { cx += p[0]; cy += p[1]; cz += p[2]; }
        const n = pts.length || 1;
        return [cx / n, cy / n, cz / n];
    }
    function polygonBoundLen(pts) {
        let minX = +Infinity, maxX = -Infinity, minY = +Infinity, maxY = -Infinity, minZ = +Infinity, maxZ = -Infinity;
        for (const p of pts) {
            if (p[0] < minX) minX = p[0]; if (p[0] > maxX) maxX = p[0];
            if (p[1] < minY) minY = p[1]; if (p[1] > maxY) maxY = p[1];
            if (p[2] < minZ) minZ = p[2]; if (p[2] > maxZ) maxZ = p[2];
        }
        return Math.hypot(maxX - minX, maxY - minY, maxZ - minZ);
    }
    function arrowHeadSide(n) {
        // Any vector not parallel to n; take cross to get a perpendicular.
        const upish = (Math.abs(n[1]) < 0.9) ? [0, 1, 0] : [1, 0, 0];
        const sx = n[1] * upish[2] - n[2] * upish[1];
        const sy = n[2] * upish[0] - n[0] * upish[2];
        const sz = n[0] * upish[1] - n[1] * upish[0];
        const l = Math.hypot(sx, sy, sz) || 1;
        return [sx / l, sy / l, sz / l];
    }

    // -----------------------------------------------------------------------
    // Translate gizmo at the selected vertex.
    //
    // Shows three axis lines (red X, green Y, blue Z) anchored at the selected
    // vertex's WebGL position. The Z arm points along Unity +Z (which is
    // WebGL -Z because of the mirror). Each axis has a tip marker that's the
    // drag handle. Length scales with camera distance so the gizmo stays a
    // similar size on screen at any zoom.
    //
    // Drag math (see onPointerMove): project the world axis onto screen space
    // at drag start, then convert the mouse pointer's pixel motion along that
    // 2D direction into world-distance-along-the-axis. Send the new vertex
    // position back to C# via OnVertexMoved every move (real-time edit).
    // -----------------------------------------------------------------------
    function rebuildGizmoBuffers() {
        const gl = V.gl;
        const s = V.state;
        V.gizmoTips = [];
        V.gizmoLineCount = 0;
        V.gizmoTipCount = 0;
        if (!gl || !s || s.placing) return;

        // Determine gizmo anchor: a single selected vertex, or the centroid
        // of all unique vertices in the selected faces. The drag handler
        // captures `affectedVertexIds` so a face drag moves every attached
        // vertex together (and any neighbour that happens to share them).
        let center = null;
        let affectedVertexIds = null;
        if (s.mode === 'vertex' && s.selectedVertexIds.length === 1) {
            const v = s.vertices.find(vv => vv.id === s.selectedVertexIds[0]);
            if (!v) return;
            center = unityToGl(v);
            affectedVertexIds = [v.id];
        } else if (s.mode === 'face' && s.selectedFaceIds.length > 0) {
            const selSet = new Set(s.selectedFaceIds);
            const ids = new Set();
            for (const f of s.faces) {
                if (selSet.has(f.id)) for (const id of f.v) ids.add(id);
            }
            if (ids.size === 0) return;
            let cx = 0, cy = 0, cz = 0, n = 0;
            for (const id of ids) {
                const v = s.vertices.find(vv => vv.id === id);
                if (!v) continue;
                const p = unityToGl(v);
                cx += p[0]; cy += p[1]; cz += p[2]; n++;
            }
            if (n === 0) return;
            center = [cx / n, cy / n, cz / n];
            affectedVertexIds = Array.from(ids);
        } else {
            return;
        }
        const len = V.camera.distance * 0.18;

        // Unity axis directions in WebGL world space. +Z (forward in Unity)
        // is -Z after the mirror, so the blue handle goes that way.
        const axes = [
            { axis: 'x', dir: [1, 0, 0],  color: [1.00, 0.30, 0.35] },
            { axis: 'y', dir: [0, 1, 0],  color: [0.35, 0.85, 0.35] },
            { axis: 'z', dir: [0, 0, -1], color: [0.40, 0.55, 1.00] },
        ];
        const lineArr = [];
        const tipArr = [];
        for (const a of axes) {
            const tip = [center[0] + a.dir[0]*len, center[1] + a.dir[1]*len, center[2] + a.dir[2]*len];
            lineArr.push(center[0], center[1], center[2], a.color[0], a.color[1], a.color[2]);
            lineArr.push(tip[0],    tip[1],    tip[2],    a.color[0], a.color[1], a.color[2]);
            tipArr.push(tip[0], tip[1], tip[2], a.color[0], a.color[1], a.color[2]);
            V.gizmoTips.push({
                axis: a.axis,
                world: tip,
                axisDir: a.dir,
                center: center.slice(),
                vertexIds: affectedVertexIds.slice(),
            });
        }
        gl.bindBuffer(gl.ARRAY_BUFFER, V.gizmoLineBuf);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(lineArr), gl.DYNAMIC_DRAW);
        V.gizmoLineCount = lineArr.length / 6;

        gl.bindBuffer(gl.ARRAY_BUFFER, V.gizmoTipBuf);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(tipArr), gl.DYNAMIC_DRAW);
        V.gizmoTipCount = tipArr.length / 6;
    }

    // Return the axis name if the cursor is over a gizmo tip (within ~12 CSS
    // px), or null otherwise. Used in onPointerDown to decide whether the
    // click should grab the gizmo or fall through to orbit.
    function hitGizmo(ddx, ddy) {
        if (V.gizmoTips.length === 0) return null;
        const vp = buildVP();
        const w = V.canvas.width, h = V.canvas.height;
        const r2 = (12 * V.dpr) * (12 * V.dpr);
        let best = null, bestD2 = r2;
        for (const t of V.gizmoTips) {
            const p = projectToScreen(vp, t.world, w, h);
            if (!p) continue;
            const dx = p[0] - ddx, dy = p[1] - ddy;
            const d2 = dx*dx + dy*dy;
            if (d2 < bestD2) { bestD2 = d2; best = t; }
        }
        return best;
    }

    function pushTri(arr, p, n, c, uv) {
        arr.push(p[0], p[1], p[2], n[0], n[1], n[2], c[0], c[1], c[2],
                 uv ? uv[0] : 0, uv ? uv[1] : 0);
    }
    function unityToGl(v) {
        if (!v) return null;
        return [v.x, v.y, -v.z];
    }
    function triNormal(a, b, c) {
        const ux = b[0] - a[0], uy = b[1] - a[1], uz = b[2] - a[2];
        const vx = c[0] - a[0], vy = c[1] - a[1], vz = c[2] - a[2];
        let nx = uy * vz - uz * vy;
        let ny = uz * vx - ux * vz;
        let nz = ux * vy - uy * vx;
        const len = Math.hypot(nx, ny, nz) || 1;
        return [nx / len, ny / len, nz / len];
    }

    function buildAxisBuffer() {
        // Three lines from origin: +X red, +Y green, +Z blue. Unity's +Z is
        // "forward" — in our WebGL world that's the -z direction, so the
        // blue arm goes to (0,0,-1). Same color convention as Unity's
        // gizmo so the user's mental model lines up.
        const data = [
            // pos.xyz                  // color.rgb
            0, 0, 0,    1.0, 0.30, 0.35, // +X start
            1, 0, 0,    1.0, 0.30, 0.35, // +X end
            0, 0, 0,    0.35, 0.85, 0.35, // +Y start
            0, 1, 0,    0.35, 0.85, 0.35, // +Y end
            0, 0, 0,    0.40, 0.55, 1.0,  // +Z start (Unity +Z forward = WebGL -Z)
            0, 0, -1,   0.40, 0.55, 1.0,  // +Z end
        ];
        const gl = V.gl;
        gl.bindBuffer(gl.ARRAY_BUFFER, V.axisBuf);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(data), gl.STATIC_DRAW);
        V.axisCount = 6;
    }

    // -----------------------------------------------------------------------
    // Matrix math (column-major mat4 stored as Float32Array(16)).
    // -----------------------------------------------------------------------
    function mat4Identity() {
        return new Float32Array([1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1]);
    }
    function mat4Perspective(fovY, aspect, near, far) {
        const f = 1 / Math.tan(fovY / 2);
        const nf = 1 / (near - far);
        const m = new Float32Array(16);
        m[0] = f / aspect; m[5] = f;
        m[10] = (far + near) * nf;
        m[11] = -1;
        m[14] = 2 * far * near * nf;
        return m;
    }
    function mat4Ortho(l, r, b, t, n, f) {
        const m = new Float32Array(16);
        m[0] = 2 / (r - l);
        m[5] = 2 / (t - b);
        m[10] = -2 / (f - n);
        m[12] = -(r + l) / (r - l);
        m[13] = -(t + b) / (t - b);
        m[14] = -(f + n) / (f - n);
        m[15] = 1;
        return m;
    }
    function mat4LookAt(eye, center, up) {
        const z0 = eye[0] - center[0], z1 = eye[1] - center[1], z2 = eye[2] - center[2];
        let zl = Math.hypot(z0, z1, z2) || 1;
        const zx = z0 / zl, zy = z1 / zl, zz = z2 / zl;
        let xx = up[1] * zz - up[2] * zy;
        let xy = up[2] * zx - up[0] * zz;
        let xz = up[0] * zy - up[1] * zx;
        let xl = Math.hypot(xx, xy, xz) || 1;
        xx /= xl; xy /= xl; xz /= xl;
        const yx = zy * xz - zz * xy;
        const yy = zz * xx - zx * xz;
        const yz = zx * xy - zy * xx;
        const m = new Float32Array(16);
        m[0] = xx; m[1] = yx; m[2] = zx; m[3] = 0;
        m[4] = xy; m[5] = yy; m[6] = zy; m[7] = 0;
        m[8] = xz; m[9] = yz; m[10] = zz; m[11] = 0;
        m[12] = -(xx * eye[0] + xy * eye[1] + xz * eye[2]);
        m[13] = -(yx * eye[0] + yy * eye[1] + yz * eye[2]);
        m[14] = -(zx * eye[0] + zy * eye[1] + zz * eye[2]);
        m[15] = 1;
        return m;
    }
    function mat4Mul(a, b) {
        const out = new Float32Array(16);
        for (let i = 0; i < 4; i++) for (let j = 0; j < 4; j++) {
            let s = 0;
            for (let k = 0; k < 4; k++) s += a[i + k * 4] * b[k + j * 4];
            out[i + j * 4] = s;
        }
        return out;
    }

    function cameraEye() {
        const t = V.camera.theta, p = V.camera.phi, d = V.camera.distance;
        const cx = V.camera.target[0], cy = V.camera.target[1], cz = V.camera.target[2];
        return [
            cx + d * Math.cos(p) * Math.cos(t),
            cy + d * Math.sin(p),
            cz + d * Math.cos(p) * Math.sin(t),
        ];
    }

    function buildVP() {
        const aspect = (V.canvas.width || 1) / (V.canvas.height || 1);
        const mode = V.viewMode || 'perspective';
        if (mode === 'perspective') {
            const proj = mat4Perspective(Math.PI / 4, aspect, 0.01, 1000);
            const view = mat4LookAt(cameraEye(), V.camera.target, [0, 1, 0]);
            return mat4Mul(proj, view);
        }
        // Orthographic views: fixed camera direction, distance controls the
        // half-extent so the wheel still zooms. The target is shared with the
        // perspective camera so panning carries across.
        const t = V.camera.target;
        const d = V.camera.distance;
        const half = d * 0.5;
        const proj = mat4Ortho(-half * aspect, half * aspect, -half, half, -100, 100);
        let eye, up;
        if (mode === 'top') {
            // Looking straight down. +Y → +Y; up is Unity's +Z (forward) →
            // in GL the Z axis is negated, so up here is (0,0,-1).
            eye = [t[0], t[1] + 10, t[2]];
            up = [0, 0, -1];
        } else if (mode === 'front') {
            // Looking along Unity's +Z (which is GL's -Z) toward origin.
            // Camera sits at z = target.z - 10 in GL, looking +Z direction.
            eye = [t[0], t[1], t[2] - 10];
            up = [0, 1, 0];
        } else { // 'side'
            // Looking along +X toward origin.
            eye = [t[0] + 10, t[1], t[2]];
            up = [0, 1, 0];
        }
        const view = mat4LookAt(eye, t, up);
        return mat4Mul(proj, view);
    }

    // -----------------------------------------------------------------------
    // Draw.
    // -----------------------------------------------------------------------
    function draw() {
        const gl = V.gl;
        if (!gl) return;
        gl.viewport(0, 0, V.canvas.width, V.canvas.height);
        gl.clearColor(0.087, 0.087, 0.106, 1);
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
        gl.enable(gl.DEPTH_TEST);

        const vp = buildVP();

        // Grid first (just lines on the Y=0 plane) — drawn with line program
        // and a temporary buffer. We draw it directly here to keep state small.
        drawGrid(vp);

        // Mesh.
        if (V.meshCount > 0 || V.meshBatches.size > 0) {
            gl.useProgram(V.meshProg.program);
            gl.uniformMatrix4fv(V.meshProg.uMVP, false, vp);
            if (V.meshProg.uTex) gl.uniform1i(V.meshProg.uTex, 0);
            if (V.meshProg.uLightDirs) gl.uniform3fv(V.meshProg.uLightDirs, V.lightDirs);
            if (V.meshProg.uLightCols) gl.uniform3fv(V.meshProg.uLightCols, V.lightCols);
            if (V.meshProg.uAmbient) gl.uniform1f(V.meshProg.uAmbient, V.ambient);
            gl.activeTexture(gl.TEXTURE0);

            const stride = 11 * 4;
            const setupAttribs = () => {
                gl.enableVertexAttribArray(0);
                gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
                gl.enableVertexAttribArray(1);
                gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
                gl.enableVertexAttribArray(2);
                gl.vertexAttribPointer(2, 3, gl.FLOAT, false, stride, 6 * 4);
                gl.enableVertexAttribArray(3);
                gl.vertexAttribPointer(3, 2, gl.FLOAT, false, stride, 9 * 4);
            };

            // Untextured pass: default white texture so the shader's multiply
            // is identity. Faces here keep their per-material diffuse color.
            if (V.meshCount > 0) {
                gl.bindTexture(gl.TEXTURE_2D, V.defaultTex);
                gl.bindBuffer(gl.ARRAY_BUFFER, V.meshBuf);
                setupAttribs();
                gl.drawArrays(gl.TRIANGLES, 0, V.meshCount);
            }

            // Textured pass: bind each material's texture and draw its batch.
            // If the bytes haven't decoded yet, the texture map won't have an
            // entry — fall back to default so the face still draws.
            for (const [matId, batch] of V.meshBatches) {
                if (batch.count === 0) continue;
                const tex = V.textures.get(matId) || V.defaultTex;
                gl.bindTexture(gl.TEXTURE_2D, tex);
                gl.bindBuffer(gl.ARRAY_BUFFER, batch.buf);
                setupAttribs();
                gl.drawArrays(gl.TRIANGLES, 0, batch.count);
            }
            gl.disableVertexAttribArray(3);
        }

        // Wireframe overlay on top.
        if (V.wireCount > 0) {
            gl.useProgram(V.lineProg.program);
            gl.uniformMatrix4fv(V.lineProg.uMVP, false, vp);
            gl.bindBuffer(gl.ARRAY_BUFFER, V.wireBuf);
            const stride = 6 * 4;
            gl.enableVertexAttribArray(0);
            gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
            gl.enableVertexAttribArray(1);
            gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
            gl.disableVertexAttribArray(2);
            gl.drawArrays(gl.LINES, 0, V.wireCount);
        }

        // Connected-edge highlight in face mode — drawn over the wireframe
        // so the amber edges read clearly against the black wireframe.
        if (V.connEdgeCount > 0) {
            gl.useProgram(V.lineProg.program);
            gl.uniformMatrix4fv(V.lineProg.uMVP, false, vp);
            gl.bindBuffer(gl.ARRAY_BUFFER, V.connEdgeBuf);
            const stride = 6 * 4;
            gl.enableVertexAttribArray(0);
            gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
            gl.enableVertexAttribArray(1);
            gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
            gl.lineWidth(2);
            gl.drawArrays(gl.LINES, 0, V.connEdgeCount);
        }

        // Vertex handles (points). Disable depth so they're always visible.
        if (V.pointCount > 0) {
            gl.useProgram(V.lineProg.program);
            gl.uniformMatrix4fv(V.lineProg.uMVP, false, vp);
            gl.bindBuffer(gl.ARRAY_BUFFER, V.pointBuf);
            const stride = 6 * 4;
            gl.enableVertexAttribArray(0);
            gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
            gl.enableVertexAttribArray(1);
            gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
            gl.disable(gl.DEPTH_TEST);
            gl.drawArrays(gl.POINTS, 0, V.pointCount);
            gl.enable(gl.DEPTH_TEST);
        }

        // Translate gizmo at the selected vertex. Rebuilt every frame so its
        // size tracks camera zoom. Depth disabled so it's always visible.
        rebuildGizmoBuffers();
        if (V.gizmoLineCount > 0) {
            gl.disable(gl.DEPTH_TEST);
            gl.useProgram(V.lineProg.program);
            gl.uniformMatrix4fv(V.lineProg.uMVP, false, vp);
            gl.bindBuffer(gl.ARRAY_BUFFER, V.gizmoLineBuf);
            const stride = 6 * 4;
            gl.enableVertexAttribArray(0);
            gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
            gl.enableVertexAttribArray(1);
            gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
            gl.lineWidth(3);
            gl.drawArrays(gl.LINES, 0, V.gizmoLineCount);

            gl.useProgram(V.markerProg.program);
            gl.uniformMatrix4fv(V.markerProg.uMVP, false, vp);
            gl.bindBuffer(gl.ARRAY_BUFFER, V.gizmoTipBuf);
            gl.enableVertexAttribArray(0);
            gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
            gl.enableVertexAttribArray(1);
            gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
            gl.drawArrays(gl.POINTS, 0, V.gizmoTipCount);
            gl.enable(gl.DEPTH_TEST);
        }

        // Placement overlay (drawn before axis so the axis still wins).
        // Depth disabled so the markers are always visible.
        if (V.placeLineCount > 0 || V.placePointCount > 0) {
            gl.disable(gl.DEPTH_TEST);
            if (V.placeLineCount > 0) {
                gl.useProgram(V.lineProg.program);
                gl.uniformMatrix4fv(V.lineProg.uMVP, false, vp);
                gl.bindBuffer(gl.ARRAY_BUFFER, V.placeLineBuf);
                const stride = 6 * 4;
                gl.enableVertexAttribArray(0);
                gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
                gl.enableVertexAttribArray(1);
                gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
                gl.lineWidth(2);
                gl.drawArrays(gl.LINES, 0, V.placeLineCount);
            }
            if (V.placePointCount > 0) {
                gl.useProgram(V.markerProg.program);
                gl.uniformMatrix4fv(V.markerProg.uMVP, false, vp);
                gl.bindBuffer(gl.ARRAY_BUFFER, V.placePointBuf);
                const stride = 6 * 4;
                gl.enableVertexAttribArray(0);
                gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
                gl.enableVertexAttribArray(1);
                gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
                gl.drawArrays(gl.POINTS, 0, V.placePointCount);
            }
            gl.enable(gl.DEPTH_TEST);
        }

        // Axis gizmo (always drawn on top — disable depth + draw last).
        gl.disable(gl.DEPTH_TEST);
        gl.useProgram(V.lineProg.program);
        gl.uniformMatrix4fv(V.lineProg.uMVP, false, vp);
        gl.bindBuffer(gl.ARRAY_BUFFER, V.axisBuf);
        const stride = 6 * 4;
        gl.enableVertexAttribArray(0);
        gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
        gl.enableVertexAttribArray(1);
        gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
        gl.lineWidth(2);
        gl.drawArrays(gl.LINES, 0, V.axisCount);
        gl.enable(gl.DEPTH_TEST);

        drawGimbal();
    }

    // Static top-right axis indicator. Reuses the world axis buffer but draws
    // it through a rotation-only view matrix and orthographic projection in a
    // small viewport corner so the user always knows which way the camera
    // points. Axes match the world gizmo: +X red, +Y green, +Z (Unity forward) blue.
    function drawGimbal() {
        const gl = V.gl;
        const sizePx = Math.round(80 * V.dpr);
        const marginPx = Math.round(10 * V.dpr);
        const gx = V.canvas.width - sizePx - marginPx;
        const gy = V.canvas.height - sizePx - marginPx;
        gl.viewport(gx, gy, sizePx, sizePx);
        gl.disable(gl.DEPTH_TEST);

        // Camera direction (target → eye), normalized. We place a synthetic
        // eye 4 units along this direction and look at origin so only rotation
        // affects the gimbal — its position never changes.
        const eye = cameraEye();
        const dx = eye[0] - V.camera.target[0];
        const dy = eye[1] - V.camera.target[1];
        const dz = eye[2] - V.camera.target[2];
        const dl = Math.hypot(dx, dy, dz) || 1;
        const e = [dx / dl * 4, dy / dl * 4, dz / dl * 4];
        const view = mat4LookAt(e, [0, 0, 0], [0, 1, 0]);
        const proj = mat4Ortho(-1.4, 1.4, -1.4, 1.4, 0.01, 100);
        const gvp = mat4Mul(proj, view);

        gl.useProgram(V.lineProg.program);
        gl.uniformMatrix4fv(V.lineProg.uMVP, false, gvp);
        gl.bindBuffer(gl.ARRAY_BUFFER, V.axisBuf);
        const stride = 6 * 4;
        gl.enableVertexAttribArray(0);
        gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
        gl.enableVertexAttribArray(1);
        gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
        gl.lineWidth(3);
        gl.drawArrays(gl.LINES, 0, V.axisCount);

        // Tip dots — render with the marker program so they're chunkier than
        // the line endpoints and the user can read orientation at a glance.
        gl.useProgram(V.markerProg.program);
        gl.uniformMatrix4fv(V.markerProg.uMVP, false, gvp);
        gl.drawArrays(gl.POINTS, 1, 1); // +X tip
        gl.drawArrays(gl.POINTS, 3, 1); // +Y tip
        gl.drawArrays(gl.POINTS, 5, 1); // +Z tip

        gl.viewport(0, 0, V.canvas.width, V.canvas.height);
        gl.enable(gl.DEPTH_TEST);
    }

    function drawGrid(vp) {
        const gl = V.gl;
        // Grid buffer is per-viewport: each WebGL context needs its own,
        // GL handles aren't shared across contexts in WebGL 1.
        if (!V.gridBuf) {
            const lines = [];
            const range = 10, step = 1;
            const c1 = [0.16, 0.16, 0.20];
            const c2 = [0.10, 0.10, 0.13];
            for (let i = -range; i <= range; i++) {
                const col = (i === 0) ? c1 : c2;
                lines.push(-range, 0, i, col[0], col[1], col[2]);
                lines.push( range, 0, i, col[0], col[1], col[2]);
                lines.push(i, 0, -range, col[0], col[1], col[2]);
                lines.push(i, 0,  range, col[0], col[1], col[2]);
            }
            V.gridBuf = gl.createBuffer();
            gl.bindBuffer(gl.ARRAY_BUFFER, V.gridBuf);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(lines), gl.STATIC_DRAW);
            V.gridCount = lines.length / 6;
        }
        gl.useProgram(V.lineProg.program);
        gl.uniformMatrix4fv(V.lineProg.uMVP, false, vp);
        gl.bindBuffer(gl.ARRAY_BUFFER, V.gridBuf);
        const stride = 6 * 4;
        gl.enableVertexAttribArray(0);
        gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
        gl.enableVertexAttribArray(1);
        gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
        gl.drawArrays(gl.LINES, 0, V.gridCount);
    }

    // -----------------------------------------------------------------------
    // Sizing + mouse.
    // -----------------------------------------------------------------------
    function sizeToHost() {
        if (!V.host || !V.canvas) return;
        const w = Math.max(1, V.host.clientWidth);
        const h = Math.max(1, V.host.clientHeight);
        V.dpr = window.devicePixelRatio || 1;
        V.canvas.width = Math.round(w * V.dpr);
        V.canvas.height = Math.round(h * V.dpr);
        V.canvas.style.width = w + 'px';
        V.canvas.style.height = h + 'px';
    }

    function installMouse() {
        const vp = V;
        // Wrap each handler so V is rebound to this viewport before the shared
        // handler runs. Anonymous closures can't be removed via name, so we
        // stash the bound copies on vp for removeMouse to use.
        vp._mouseListeners = {
            down:  e => { V = vp; onPointerDown(e); },
            move:  e => { V = vp; onPointerMove(e); },
            up:    e => { V = vp; onPointerUp(e); },
            wheel: e => { V = vp; onWheel(e); },
            ctx:   e => { V = vp; onContextMenu(e); },
        };
        const m = vp._mouseListeners;
        vp.canvas.addEventListener('pointerdown', m.down);
        vp.canvas.addEventListener('pointermove', m.move);
        vp.canvas.addEventListener('pointerup', m.up);
        vp.canvas.addEventListener('pointercancel', m.up);
        vp.canvas.addEventListener('wheel', m.wheel, { passive: false });
        vp.canvas.addEventListener('contextmenu', m.ctx);
    }
    function removeMouse() {
        const vp = V;
        if (!vp || !vp.canvas || !vp._mouseListeners) return;
        const m = vp._mouseListeners;
        vp.canvas.removeEventListener('pointerdown', m.down);
        vp.canvas.removeEventListener('pointermove', m.move);
        vp.canvas.removeEventListener('pointerup', m.up);
        vp.canvas.removeEventListener('pointercancel', m.up);
        vp.canvas.removeEventListener('wheel', m.wheel);
        vp.canvas.removeEventListener('contextmenu', m.ctx);
        vp._mouseListeners = null;
    }

    // Right-click opens a Blazor-rendered context menu. We pre-pick both a
    // vertex and a face so the C# side can decide which kind of menu to show.
    // preventDefault() suppresses the WebView2 native menu so the custom one
    // is the only thing the user sees.
    function onContextMenu(e) {
        // preventDefault alone isn't always enough for the Photino/WebView2
        // native menu — stopPropagation + stopImmediatePropagation kills any
        // bubbling listeners (including the host-level one) so only our
        // Blazor-rendered menu shows over the canvas.
        e.preventDefault();
        e.stopPropagation();
        if (typeof e.stopImmediatePropagation === 'function') e.stopImmediatePropagation();
        if (!V || !V.state || !V.dotnet) return;
        const rect = V.canvas.getBoundingClientRect();
        const cssX = e.clientX - rect.left;
        const cssY = e.clientY - rect.top;
        const ddx = cssX * V.dpr;
        const ddy = cssY * V.dpr;
        const vId = pickVertex(ddx, ddy);
        const fId = vId === 0 ? pickFace(cssX, cssY, rect.width, rect.height) : 0;
        invokeDotnet('OnContextMenu', vId, fId, cssX, cssY);
    }

    // Fire-and-forget JS→.NET call with logging. Critically, the .catch
    // prevents an unhandled Promise rejection from short-circuiting the
    // calling code; without it a single failed pointer-move could leave
    // the C# side out of sync and the next state push would revert the
    // face back to its pre-drag position (the "face resets" bug).
    function invokeDotnet(name) {
        if (!V.dotnet) return;
        const args = Array.prototype.slice.call(arguments, 1);
        V.dotnet.invokeMethodAsync.apply(V.dotnet, [name].concat(args))
            .catch(err => console.error('invokeMethodAsync ' + name + ' failed:', err));
    }

    // Forward a one-shot diagnostic note to the C# log file. Throttled
    // implicitly by being called only on key user actions (drag start,
    // drag end), not per pointermove.
    function jsLog(msg) {
        if (!V.dotnet) return;
        V.dotnet.invokeMethodAsync('LogFromJs', msg).catch(() => {});
    }

    function onPointerDown(e) {
        V.canvas.setPointerCapture(e.pointerId);
        const rect = V.canvas.getBoundingClientRect();
        const cssX = e.clientX - rect.left;
        const cssY = e.clientY - rect.top;

        // Left button → check gizmo before falling through to orbit/pick.
        if (e.button === 0 && !e.shiftKey) {
            const hit = hitGizmo(cssX * V.dpr, cssY * V.dpr);
            if (hit) {
                // Cache the screen-space axis vector so each move just dots
                // the mouse delta against it — see onPointerMove (gizmo).
                const vp = buildVP();
                const w = V.canvas.width, h = V.canvas.height;
                const sCenter = projectToScreen(vp, hit.center, w, h);
                const sTip = projectToScreen(vp, hit.world, w, h);
                let screenAxis = [0, 0], screenAxisLen2 = 0;
                if (sCenter && sTip) {
                    screenAxis = [sTip[0] - sCenter[0], sTip[1] - sCenter[1]];
                    // length is in DRAWING-buffer pixels; scale below by dpr.
                    screenAxisLen2 = screenAxis[0]*screenAxis[0] + screenAxis[1]*screenAxis[1];
                }
                // Snapshot every affected vertex's original Unity position so
                // each frame can apply an absolute delta from the click point
                // rather than accumulating per-move rounding error.
                const startUnity = hit.vertexIds.map(id => {
                    const v = V.state.vertices.find(vv => vv.id === id);
                    return v ? [v.x, v.y, v.z] : null;
                }).filter(p => p);
                const startIds = hit.vertexIds.filter((_, i) => {
                    const v = V.state.vertices.find(vv => vv.id === hit.vertexIds[i]);
                    return !!v;
                });
                V.drag = {
                    id: e.pointerId,
                    startX: e.clientX, startY: e.clientY,
                    x: e.clientX, y: e.clientY,
                    mode: 'gizmo',
                    moved: false,
                    button: e.button, shiftKey: e.shiftKey,
                    vertexIds: startIds,
                    startUnity,
                    axisDir: hit.axisDir,
                    screenAxis, screenAxisLen2,
                    movesSent: 0,
                };
                jsLog('gizmo drag start axis=' + hit.axis
                    + ' vertexIds=[' + startIds.join(',') + ']'
                    + ' axisDir=[' + hit.axisDir.join(',') + ']'
                    + ' screenAxisLen2=' + screenAxisLen2.toFixed(3));
                return;
            }
        }

        V.drag = {
            id: e.pointerId,
            startX: e.clientX, startY: e.clientY,
            x: e.clientX, y: e.clientY,
            // Middle button or shift+left = pan; else orbit. Plain left-click
            // also doubles as a pick if the pointer never moved — see onPointerUp.
            mode: (e.button === 1 || (e.shiftKey && e.button === 0)) ? 'pan' : 'orbit',
            moved: false,
            button: e.button,
            shiftKey: e.shiftKey,
        };
    }
    function onPointerMove(e) {
        if (!V.drag || V.drag.id !== e.pointerId) return;
        const dx = e.clientX - V.drag.x;
        const dy = e.clientY - V.drag.y;
        V.drag.x = e.clientX;
        V.drag.y = e.clientY;
        if (!V.drag.moved) {
            // Movement threshold: anything below ~3 CSS px is treated as a click,
            // not a drag — clicks pick, drags orbit/pan.
            const tdx = e.clientX - V.drag.startX;
            const tdy = e.clientY - V.drag.startY;
            if (tdx * tdx + tdy * tdy > 9) V.drag.moved = true;
        }
        if (!V.drag.moved) return;
        if (V.drag.mode === 'gizmo') {
            // Project total mouse motion onto the cached screen-axis vector.
            // Total (not incremental) so accumulated rounding doesn't drift.
            const totalDxCss = e.clientX - V.drag.startX;
            const totalDyCss = e.clientY - V.drag.startY;
            const totalDx = totalDxCss * V.dpr;
            const totalDy = totalDyCss * V.dpr;
            const sa = V.drag.screenAxis;
            const sa2 = V.drag.screenAxisLen2;
            if (sa2 > 1e-6) {
                const along = totalDx * sa[0] + totalDy * sa[1];
                const worldDelta = along / sa2;
                // axisDir is in WebGL space; convert to Unity-space delta by
                // mirroring Z (so the blue handle increments Unity +Z).
                const du = [
                    V.drag.axisDir[0] * worldDelta,
                    V.drag.axisDir[1] * worldDelta,
                    -V.drag.axisDir[2] * worldDelta,
                ];
                const ids = V.drag.vertexIds;
                const xs = new Array(ids.length);
                const ys = new Array(ids.length);
                const zs = new Array(ids.length);
                for (let i = 0; i < ids.length; i++) {
                    const o = V.drag.startUnity[i];
                    xs[i] = o[0] + du[0];
                    ys[i] = o[1] + du[1];
                    zs[i] = o[2] + du[2];
                    // Patch the local snapshot so the next frame's mesh /
                    // gizmo / wireframe / connected-edge overlay all reflect
                    // the move without waiting for the C# round-trip.
                    const v = V.state.vertices.find(vv => vv.id === ids[i]);
                    if (v) { v.x = xs[i]; v.y = ys[i]; v.z = zs[i]; }
                }
                rebuildMeshBuffers();
                const moves = new Array(ids.length);
                for (let i = 0; i < ids.length; i++) {
                    moves[i] = { id: ids[i], x: xs[i], y: ys[i], z: zs[i] };
                }
                invokeDotnet('OnVerticesMoved', moves);
                V.drag.movesSent++;
                if (V.drag.movesSent === 1) {
                    jsLog('first move sent worldDelta=' + worldDelta.toFixed(3)
                        + ' first move id=' + moves[0].id
                        + ' (' + moves[0].x.toFixed(3)
                        + ',' + moves[0].y.toFixed(3)
                        + ',' + moves[0].z.toFixed(3) + ')');
                }
            }
            return;
        }
        if (V.drag.mode === 'orbit') {
            // Ortho views have a fixed camera direction — drag without
            // shift/middle behaves as pan instead of orbit (the user's
            // intuition in a top/front/side view).
            if ((V.viewMode || 'perspective') !== 'perspective') {
                V.drag.mode = 'pan';
            } else {
                V.camera.theta -= dx * 0.01;
                V.camera.phi   += dy * 0.01;
                const halfPi = Math.PI / 2 - 0.01;
                if (V.camera.phi > halfPi)  V.camera.phi = halfPi;
                if (V.camera.phi < -halfPi) V.camera.phi = -halfPi;
            }
        }
        if (V.drag.mode === 'pan') {
            // Ortho views have a known fixed view direction → use hard-coded
            // right/up vectors so a top view pans across XZ, side across YZ,
            // etc. Falling through to the perspective branch with cameraEye()
            // would compute a screen basis from the unused theta/phi.
            const mode = V.viewMode || 'perspective';
            if (mode !== 'perspective') {
                const scale = V.camera.distance * 0.0015;
                let rx = 0, ry = 0, rz = 0, ux = 0, uy = 0, uz = 0;
                if (mode === 'top')   { rx = 1; uz = -1; } // screen-right=+X, screen-up=-Z (Unity +Z forward → flipped in GL)
                else if (mode === 'front') { rx = 1; uy = 1; }
                else /* side */       { rz = 1; uy = 1; }
                V.camera.target[0] -= rx * dx * scale - ux * dy * scale;
                V.camera.target[1] -= ry * dx * scale - uy * dy * scale;
                V.camera.target[2] -= rz * dx * scale - uz * dy * scale;
                V.drag.lastX = e.clientX;
                V.drag.lastY = e.clientY;
                return;
            }
            // Pan in screen space: move target along camera right/up.
            const eye = cameraEye();
            const fx = V.camera.target[0] - eye[0];
            const fy = V.camera.target[1] - eye[1];
            const fz = V.camera.target[2] - eye[2];
            const fl = Math.hypot(fx, fy, fz) || 1;
            const fnx = fx / fl, fny = fy / fl, fnz = fz / fl;
            // right = forward × up
            let rx = fnz * 1 - fny * 0;
            let ry = fnx * 0 - fnz * 0;
            let rz = fny * 0 - fnx * 1;
            // Recompute via proper cross with world up (0,1,0):
            rx = fny * 0 - fnz * 1;
            ry = fnz * 0 - fnx * 0;
            rz = fnx * 1 - fny * 0;
            // Cleaner explicit form:
            rx = -fnz; ry = 0; rz = fnx;
            const rl = Math.hypot(rx, ry, rz) || 1;
            rx /= rl; ry /= rl; rz /= rl;
            // up = right × forward
            const ux = ry * fnz - rz * fny;
            const uy = rz * fnx - rx * fnz;
            const uz = rx * fny - ry * fnx;
            const scale = V.camera.distance * 0.0015;
            V.camera.target[0] -= rx * dx * scale - ux * dy * scale;
            V.camera.target[1] -= ry * dx * scale - uy * dy * scale;
            V.camera.target[2] -= rz * dx * scale - uz * dy * scale;
        }
    }
    function onPointerUp(e) {
        const drag = V.drag;
        if (drag && drag.id === e.pointerId) V.drag = null;
        try { V.canvas.releasePointerCapture(e.pointerId); } catch {}
        if (drag && drag.mode === 'gizmo') {
            jsLog('gizmo drag end moved=' + drag.moved
                + ' movesSent=' + (drag.movesSent || 0));
        }
        // Click (no drag) → fire a pick. Left button only; modifier flag
        // becomes the "additive" parameter so Shift-click multi-selects.
        if (drag && !drag.moved && drag.button === 0) {
            tryPick(e.clientX, e.clientY, drag.shiftKey);
        }
    }

    // -----------------------------------------------------------------------
    // Picking.
    //
    // Vertex pick: project each vertex through the current MVP, take whichever
    // is closest in screen space within PICK_PX. Cheap and accurate as long
    // as the camera matrix is up to date with the rendered frame.
    //
    // Face pick: Möller–Trumbore ray/triangle intersection against the
    // fan-triangulated faces, closest-hit wins.
    // -----------------------------------------------------------------------
    const PICK_PX = 10;

    function tryPick(clientX, clientY, additive) {
        if (!V.state || !V.dotnet) return;
        const rect = V.canvas.getBoundingClientRect();
        const px = (clientX - rect.left);
        const py = (clientY - rect.top);
        // Convert to GL drawing-buffer pixel coords (devicePixelRatio).
        const ddx = px * V.dpr;
        const ddy = py * V.dpr;

        if (V.state.mode === 'vertex') {
            const id = pickVertex(ddx, ddy);
            invokeDotnet('OnPickVertex', id, !!additive);
        } else {
            const id = pickFace(clientX - rect.left, clientY - rect.top, rect.width, rect.height);
            invokeDotnet('OnPickFace', id, !!additive);
        }
    }

    function pickVertex(ddx, ddy) {
        const vp = buildVP();
        const w = V.canvas.width, h = V.canvas.height;
        let bestId = 0, bestDist = (PICK_PX * V.dpr) * (PICK_PX * V.dpr);
        for (const v of V.state.vertices) {
            const p = projectToScreen(vp, [v.x, v.y, -v.z], w, h);
            if (!p) continue;
            const dx = p[0] - ddx, dy = p[1] - ddy;
            const d2 = dx * dx + dy * dy;
            if (d2 < bestDist) { bestDist = d2; bestId = v.id; }
        }
        return bestId;
    }

    function projectToScreen(mvp, p, w, h) {
        // mvp is column-major: m[col*4 + row].
        const x = p[0], y = p[1], z = p[2];
        const cx = mvp[0]*x + mvp[4]*y + mvp[8]*z  + mvp[12];
        const cy = mvp[1]*x + mvp[5]*y + mvp[9]*z  + mvp[13];
        // const cz = mvp[2]*x + mvp[6]*y + mvp[10]*z + mvp[14]; (depth, unused)
        const cw = mvp[3]*x + mvp[7]*y + mvp[11]*z + mvp[15];
        if (cw <= 0) return null; // behind the camera
        const ndcX = cx / cw, ndcY = cy / cw;
        return [
            (ndcX * 0.5 + 0.5) * w,
            (1 - (ndcY * 0.5 + 0.5)) * h, // flip Y for screen-space pixel coord
        ];
    }

    // Möller–Trumbore ray-triangle intersection. Returns the parametric t
    // along `dir` of the hit point, or null if no hit. Accepts hits from
    // either side — we rely on closest-t to give the visible (front) face
    // for closed meshes.
    function rayTri(orig, dir, v0, v1, v2) {
        const EPS = 1e-7;
        const e1 = [v1[0]-v0[0], v1[1]-v0[1], v1[2]-v0[2]];
        const e2 = [v2[0]-v0[0], v2[1]-v0[1], v2[2]-v0[2]];
        const h = [dir[1]*e2[2] - dir[2]*e2[1],
                   dir[2]*e2[0] - dir[0]*e2[2],
                   dir[0]*e2[1] - dir[1]*e2[0]];
        const a = e1[0]*h[0] + e1[1]*h[1] + e1[2]*h[2];
        if (Math.abs(a) < EPS) return null;
        const f = 1 / a;
        const s = [orig[0]-v0[0], orig[1]-v0[1], orig[2]-v0[2]];
        const u = f * (s[0]*h[0] + s[1]*h[1] + s[2]*h[2]);
        if (u < 0 || u > 1) return null;
        const q = [s[1]*e1[2] - s[2]*e1[1],
                   s[2]*e1[0] - s[0]*e1[2],
                   s[0]*e1[1] - s[1]*e1[0]];
        const v = f * (dir[0]*q[0] + dir[1]*q[1] + dir[2]*q[2]);
        if (v < 0 || u + v > 1) return null;
        const t = f * (e2[0]*q[0] + e2[1]*q[1] + e2[2]*q[2]);
        return (t > EPS) ? t : null;
    }

    // Build a world-space ray from the camera through a CSS-pixel cursor
    // position. The math is the camera-space frustum reconstruction —
    // forward + nx*aspect*tan(fovY/2)*right + ny*tan(fovY/2)*up — then
    // re-projected to world coordinates.
    function cursorWorldRay(cssX, cssY, cssW, cssH) {
        const nx = (2 * cssX) / cssW - 1;
        const ny = 1 - (2 * cssY) / cssH;
        const aspect = (V.canvas.width || 1) / (V.canvas.height || 1);
        const mode = V.viewMode || 'perspective';
        if (mode !== 'perspective') {
            // Orthographic: all rays are parallel to the view direction, with
            // the origin offset by the cursor's projected world position.
            const t = V.camera.target;
            const half = V.camera.distance * 0.5;
            const wx = t[0] + nx * half * aspect;
            const wy = t[1] + ny * half;
            const wz = t[2];
            let origin, dir;
            if (mode === 'top') {
                // Cursor X → world X; cursor Y → world -Z (Unity +Z is forward = GL -Z).
                origin = [t[0] + nx * half * aspect, 100, t[2] - ny * half];
                dir = [0, -1, 0];
            } else if (mode === 'front') {
                origin = [wx, wy, t[2] - 100];
                dir = [0, 0, 1];
            } else { // 'side'
                origin = [t[0] + 100, wy, t[2] + nx * half * aspect];
                dir = [-1, 0, 0];
            }
            return { origin, direction: dir };
        }
        const tanHalfFov = Math.tan((Math.PI / 4) / 2);
        const eye = cameraEye();
        const fwd = normalize([
            V.camera.target[0] - eye[0],
            V.camera.target[1] - eye[1],
            V.camera.target[2] - eye[2],
        ]);
        const right = normalize(cross3(fwd, [0, 1, 0]));
        const up = cross3(right, fwd);
        const dir = normalize([
            fwd[0] + right[0]*nx*aspect*tanHalfFov + up[0]*ny*tanHalfFov,
            fwd[1] + right[1]*nx*aspect*tanHalfFov + up[1]*ny*tanHalfFov,
            fwd[2] + right[2]*nx*aspect*tanHalfFov + up[2]*ny*tanHalfFov,
        ]);
        return { origin: eye, direction: dir };
    }

    function cross3(a, b) {
        return [a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0]];
    }
    function normalize(v) {
        const l = Math.hypot(v[0], v[1], v[2]) || 1;
        return [v[0]/l, v[1]/l, v[2]/l];
    }

    function pickFace(cssX, cssY, cssW, cssH) {
        const ray = cursorWorldRay(cssX, cssY, cssW, cssH);
        const vertById = new Map(V.state.vertices.map(v => [v.id, v]));
        let bestT = +Infinity, bestId = 0;
        for (const f of V.state.faces) {
            if (f.v.length < 3) continue;
            const corners = f.v.map(id => unityToGl(vertById.get(id)));
            // Fan triangulation matches what we render. Bad for non-convex
            // n-gons but consistent with the visual representation.
            for (let i = 1; i < corners.length - 1; i++) {
                const a = corners[0], b = corners[i], c = corners[i + 1];
                if (!a || !b || !c) continue;
                const t = rayTri(ray.origin, ray.direction, a, b, c);
                if (t !== null && t < bestT) { bestT = t; bestId = f.id; }
            }
        }
        return bestId;
    }
    function onWheel(e) {
        e.preventDefault();
        const factor = Math.pow(1.0015, e.deltaY);
        V.camera.distance = Math.max(0.1, Math.min(500, V.camera.distance * factor));
    }

    // Sun direction set from Unity-space azimuth/elevation (degrees). Azimuth 0
    // points along +X; positive rotates toward +Z (Unity forward). Elevation
    // 90° is directly above (+Y). We convert and Z-mirror into GL space here so
    // the renderer doesn't need to know about the Unity convention.
    // payload = { ambient: number, lights: [{ dir:[x,y,z], color:[r,g,b], intensity }] }
    // We pre-normalize each direction and zero-out unused slots so the
    // fragment shader can iterate MAX lights unconditionally — that pattern
    // is the most portable in WebGL 1.
    editor.setLights = function (payload) {
        const MAX = 4;
        const lights = (payload && payload.lights) || [];
        const dirs = new Float32Array(MAX * 3);
        const cols = new Float32Array(MAX * 3);
        const count = Math.min(MAX, lights.length);
        for (let i = 0; i < count; i++) {
            const l = lights[i];
            const d = l.dir || [0, 1, 0];
            const c = l.color || [1, 1, 1];
            const k = (l.intensity == null) ? 1 : l.intensity;
            const dl = Math.hypot(d[0], d[1], d[2]) || 1;
            dirs[i * 3]     = d[0] / dl;
            dirs[i * 3 + 1] = d[1] / dl;
            dirs[i * 3 + 2] = d[2] / dl;
            cols[i * 3]     = c[0] * k;
            cols[i * 3 + 1] = c[1] * k;
            cols[i * 3 + 2] = c[2] * k;
        }
        const ambient = (payload && typeof payload.ambient === 'number') ? payload.ambient : 0.25;
        for (const vp of VIEWPORTS.values()) {
            vp.lightDirs = dirs;
            vp.lightCols = cols;
            vp.ambient = ambient;
        }
    };

    // Backwards-compat shim — older C# call paths might still go through this.
    editor.setSunDir = function (azimDeg, elevDeg) {
        const az = azimDeg * Math.PI / 180;
        const el = elevDeg * Math.PI / 180;
        const ce = Math.cos(el);
        editor.setLights({
            ambient: 0.25,
            lights: [{
                dir: [ce * Math.cos(az), Math.sin(el), -ce * Math.sin(az)],
                color: [1, 1, 1],
                intensity: 1,
            }],
        });
    };

    // ────────────────────────────────────────────────────────────
    // File pickers — Photino's native dialogs proved unreliable in WebView2
    // (silent failures, dialog never returning), so we use the in-WebView
    // File System Access API and fall back to <input type=file> / <a download>.
    // openFilePicker returns { name, base64 }; saveFilePicker returns the
    // chosen filename (or null on cancel, "<name> (Downloads)" on fallback).

    function bufToB64(buf) {
        const bytes = new Uint8Array(buf);
        let bin = "";
        const CHUNK = 0x8000;
        for (let i = 0; i < bytes.length; i += CHUNK) {
            bin += String.fromCharCode.apply(null, bytes.subarray(i, Math.min(i + CHUNK, bytes.length)));
        }
        return btoa(bin);
    }

    // filterExts: array of extensions without leading dot, e.g. ["obj"] or ["png","jpg"].
    editor.openFilePicker = async function (filterDesc, filterExts) {
        const exts = Array.isArray(filterExts) ? filterExts : [filterExts];
        const accept = exts.map(e => "." + e);
        if (window.showOpenFilePicker) {
            try {
                const [handle] = await window.showOpenFilePicker({
                    types: [{ description: filterDesc, accept: { "*/*": accept } }],
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
            input.accept = accept.join(",");
            input.onchange = async () => {
                const f = input.files && input.files[0];
                if (!f) { resolve(null); return; }
                const buf = await f.arrayBuffer();
                resolve({ name: f.name, base64: bufToB64(buf) });
            };
            input.click();
        });
    };

    // Cache of FileSystemFileHandle keyed by a caller-supplied slot name
    // ("obj", "mtl"). Lets Ctrl+S re-write the same file without prompting.
    const fileHandleSlots = new Map();

    editor.saveFilePicker = async function (suggestedName, filterDesc, filterExts, contentBase64, slot) {
        const exts = Array.isArray(filterExts) ? filterExts : [filterExts];
        const accept = exts.map(e => "." + e);
        const bin = atob(contentBase64);
        const buf = new Uint8Array(bin.length);
        for (let i = 0; i < bin.length; i++) buf[i] = bin.charCodeAt(i);

        if (window.showSaveFilePicker) {
            try {
                const handle = await window.showSaveFilePicker({
                    suggestedName: suggestedName,
                    types: [{ description: filterDesc, accept: { "*/*": accept } }]
                });
                const writable = await handle.createWritable();
                await writable.write(buf);
                await writable.close();
                // Remember the handle so Ctrl+S can re-write without prompting.
                if (slot) fileHandleSlots.set(slot, handle);
                return handle.name;
            } catch (e) {
                if (e && e.name === "AbortError") return null;
                console.warn("showSaveFilePicker failed, falling back:", e);
            }
        }
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

    // Silent re-save to the FileSystemFileHandle cached during a previous
    // saveFilePicker call for this slot. Returns the file name on success
    // or null when the slot is empty / permission is denied / not supported.
    editor.saveToCachedSlot = async function (slot, contentBase64) {
        const handle = fileHandleSlots.get(slot);
        if (!handle) return null;
        try {
            if (handle.queryPermission) {
                const perm = await handle.queryPermission({ mode: "readwrite" });
                if (perm !== "granted") {
                    const next = await handle.requestPermission({ mode: "readwrite" });
                    if (next !== "granted") return null;
                }
            }
            const bin = atob(contentBase64);
            const buf = new Uint8Array(bin.length);
            for (let i = 0; i < bin.length; i++) buf[i] = bin.charCodeAt(i);
            const writable = await handle.createWritable();
            await writable.write(buf);
            await writable.close();
            return handle.name;
        } catch (e) {
            console.warn("saveToCachedSlot(" + slot + ") failed:", e);
            return null;
        }
    };

    editor.hasCachedSlot = function (slot) {
        return fileHandleSlots.has(slot);
    };

    // Bind a single page-level keydown listener that captures Ctrl+S and
    // routes it to the Editor's OnCtrlS JSInvokable. Idempotent: calling it
    // again with a different ref just swaps the target. The listener stays
    // installed once added — the Editor is a long-lived top-level page.
    let _ctrlSRef = null;
    editor.installCtrlS = function (dotnetRef) {
        _ctrlSRef = dotnetRef;
        if (editor._ctrlSInstalled) return;
        editor._ctrlSInstalled = true;
        window.addEventListener("keydown", function (e) {
            if ((e.ctrlKey || e.metaKey) && (e.key === "s" || e.key === "S")) {
                e.preventDefault();
                if (_ctrlSRef && _ctrlSRef.invokeMethodAsync) {
                    _ctrlSRef.invokeMethodAsync("OnCtrlS").catch(err =>
                        console.warn("OnCtrlS failed:", err));
                }
            }
        });
    };

    // Tiny helper used by the UV editor. The Blazor MouseEventArgs.OffsetX
    // is relative to the *event target* (which becomes a child handle once
    // the cursor lands on it), so we measure the SVG's screen rect once and
    // do the math ourselves via ClientX/Y. Returns [left, top, width, height].
    editor.measureRect = function (el) {
        if (!el || !el.getBoundingClientRect) return [0, 0, 0, 0];
        const r = el.getBoundingClientRect();
        return [r.left, r.top, r.width, r.height];
    };
})();
