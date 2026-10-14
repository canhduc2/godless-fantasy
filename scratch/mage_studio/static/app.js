/**
 * MAGE 8-DIRECTION SPRITE STUDIO
 * High-performance Pixel Art Canvas & 60fps Live Animation Preview
 */

(function () {
    'use strict';

    // ================= CONFIG & CONSTANTS =================
    const CANVAS_WIDTH = 160;
    const CANVAS_HEIGHT = 160;
    const DIRECTIONS = [
        { id: 'south', name: 'Nam (S)', code: 'S' },
        { id: 'south_west', name: 'Tây Nam (SW)', code: 'SW' },
        { id: 'west', name: 'Tây (W)', code: 'W' },
        { id: 'north_west', name: 'Tây Bắc (NW)', code: 'NW' },
        { id: 'north', name: 'Bắc (N)', code: 'N' },
        { id: 'north_east', name: 'Đông Bắc (NE)', code: 'NE' },
        { id: 'east', name: 'Đông (E)', code: 'E' },
        { id: 'south_east', name: 'Đông Nam (SE)', code: 'SE' }
    ];

    // ================= STATE =================
    const state = {
        gender: 'male',
        direction: 'south',
        frame: 0,
        tool: 'pencil',
        brushSize: 1,
        color: '#9E2532',
        symmetry: false,
        onionSkin: true,
        showGrid: true,
        orthoLock: false,
        shiftKey: false,
        drawStartX: null,
        drawStartY: null,
        lockedAxis: null,
        zoom: 4,
        autoSave: true,
        isDrawing: false,
        lastX: null,
        lastY: null,
        rectStartX: null,
        rectStartY: null,
        // Selection & Move state
        selection: null, // { x, y, w, h }
        floatingSelection: null, // { x, y, w, h, canvas, originX, originY }
        clipboard: null, // { w, h, canvas }
        isSelecting: false,
        isDraggingSelection: false,
        selectStartX: null,
        selectStartY: null,
        dragSelectStartX: null,
        dragSelectStartY: null,
        dragSelectOriginX: 0,
        dragSelectOriginY: 0,
        lastHoverX: null,
        lastHoverY: null,
        recentColors: ['#9E2532', '#7D1A25', '#FFC83B', '#1B161D', '#FFD700'],
        undoStack: [],
        redoStack: [],
        maxHistory: 30,
        // Live Preview state
        previewMode: 'walk', // 'walk' or 'idle'
        previewFps: 8.5,
        previewScale: 2,
        previewBg: 'checker',
        animTimer: 0,
        lastFrameTime: performance.now(),
        frameImagesCache: {}, // key: `${gender}_${dir}_${frame}` -> Image or Canvas
        dirtyFrames: new Set(),
        autoSaveTimeout: null
    };

    // ================= DOM ELEMENTS =================
    const paintCanvas = document.getElementById('paintCanvas');
    const paintCtx = paintCanvas.getContext('2d', { willReadFrequently: true });

    const onionCanvas = document.getElementById('onionCanvas');
    const onionCtx = onionCanvas.getContext('2d');

    const overlayCanvas = document.getElementById('overlayCanvas');
    const overlayCtx = overlayCanvas.getContext('2d');

    const previewCanvas = document.getElementById('previewCanvas');
    const previewCtx = previewCanvas.getContext('2d');

    const canvasContainer = document.getElementById('canvasContainer');
    const canvasViewport = document.getElementById('canvasViewport');
    const toast = document.getElementById('toast');

    // ================= INITIALIZATION =================
    async function init() {
        setupCanvases();
        bindEvents();
        updateUI();
        await loadAllDirectionFrames(state.gender, state.direction);
        loadCurrentFrameToCanvas();
        requestAnimationFrame(previewAnimationLoop);
        showToast('🧙‍♂️ Mage Sprite Studio đã sẵn sàng! 64 Frame Nam/Nữ đồng bộ Unity.');
    }

    function setupCanvases() {
        paintCanvas.width = CANVAS_WIDTH;
        paintCanvas.height = CANVAS_HEIGHT;
        onionCanvas.width = CANVAS_WIDTH;
        onionCanvas.height = CANVAS_HEIGHT;
        overlayCanvas.width = CANVAS_WIDTH;
        overlayCanvas.height = CANVAS_HEIGHT;
        previewCanvas.width = CANVAS_WIDTH;
        previewCanvas.height = CANVAS_HEIGHT;

        applyZoom();
    }

    const ZOOM_PRESETS = [1, 2, 3, 4, 5, 6, 8, 10, 12, 16];

    // ================= CANVAS ZOOM & PAN =================
    function applyZoom() {
        const displayW = CANVAS_WIDTH * state.zoom;
        const displayH = CANVAS_HEIGHT * state.zoom;

        canvasContainer.style.width = `${displayW}px`;
        canvasContainer.style.height = `${displayH}px`;

        paintCanvas.style.width = `${displayW}px`;
        paintCanvas.style.height = `${displayH}px`;

        onionCanvas.style.width = `${displayW}px`;
        onionCanvas.style.height = `${displayH}px`;

        // overlayCanvas có độ phân giải bằng displayW x displayH để vẽ lưới pixel, con trỏ và viền sắc nét từng pixel màn hình!
        overlayCanvas.width = displayW;
        overlayCanvas.height = displayH;
        overlayCanvas.style.width = `${displayW}px`;
        overlayCanvas.style.height = `${displayH}px`;

        document.getElementById('zoomLevelLabel').textContent = `${Math.round(state.zoom * 100)}%`;
        renderOverlay(state.lastHoverX, state.lastHoverY);
    }

    function zoomIn() {
        const idx = ZOOM_PRESETS.findIndex(z => z >= state.zoom);
        if (idx !== -1 && idx < ZOOM_PRESETS.length - 1) {
            state.zoom = ZOOM_PRESETS[idx + 1];
        } else if (idx === -1) {
            state.zoom = ZOOM_PRESETS[ZOOM_PRESETS.length - 1];
        }
        applyZoom();
    }

    function zoomOut() {
        const idx = ZOOM_PRESETS.findIndex(z => z >= state.zoom);
        if (idx > 0) {
            state.zoom = ZOOM_PRESETS[idx - 1];
        } else if (idx === -1) {
            state.zoom = ZOOM_PRESETS[0];
        }
        applyZoom();
    }

    function zoomReset() {
        state.zoom = 4;
        applyZoom();
    }

    // ================= DATA LOADING & CACHING =================
    function getCacheKey(gender, dir, frame) {
        return `${gender}_${dir}_${frame}`;
    }

    async function fetchFrameImage(gender, dir, frame) {
        return new Promise((resolve) => {
            const img = new Image();
            img.crossOrigin = 'anonymous';
            img.onload = () => resolve(img);
            img.onerror = () => {
                // Return empty 160x160 canvas if image fails to load
                const blank = document.createElement('canvas');
                blank.width = CANVAS_WIDTH;
                blank.height = CANVAS_HEIGHT;
                resolve(blank);
            };
            img.src = `/api/get_frame?gender=${gender}&direction=${dir}&frame=${frame}&t=${Date.now()}`;
        });
    }

    async function loadAllDirectionFrames(gender, dir) {
        const promises = [];
        for (let f = 0; f < 8; f++) {
            const key = getCacheKey(gender, dir, f);
            if (!state.frameImagesCache[key]) {
                promises.push(
                    fetchFrameImage(gender, dir, f).then(img => {
                        state.frameImagesCache[key] = img;
                    })
                );
            }
        }
        await Promise.all(promises);
        renderFilmstripThumbnails();
    }

    function loadCurrentFrameToCanvas() {
        commitSelection();
        const key = getCacheKey(state.gender, state.direction, state.frame);
        const cached = state.frameImagesCache[key];

        paintCtx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        if (cached) {
            paintCtx.drawImage(cached, 0, 0);
        }

        // Reset Undo / Redo for new frame
        state.undoStack = [];
        state.redoStack = [];
        saveHistoryState();

        renderOnionSkin();
        renderOverlay();
        updateStatus();
    }

    function renderOnionSkin() {
        onionCtx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        if (!state.onionSkin) return;

        // Lấy frame liền trước để làm bóng mờ
        const prevFrame = (state.frame + 7) % 8;
        const prevKey = getCacheKey(state.gender, state.direction, prevFrame);
        const prevImg = state.frameImagesCache[prevKey];

        if (prevImg) {
            onionCtx.globalAlpha = 0.38;
            onionCtx.drawImage(prevImg, 0, 0);
            onionCtx.globalAlpha = 1.0;
        }
    }

    function renderOverlay(hoverX = null, hoverY = null) {
        overlayCtx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);
        const zm = state.zoom;
        const dispW = CANVAS_WIDTH * zm;
        const dispH = CANVAS_HEIGHT * zm;

        // 1. Pixel Grid (Lưới chuẩn từng pixel 1x1 khi phóng to >= 300%)
        if (state.showGrid && zm >= 3) {
            overlayCtx.fillStyle = 'rgba(255, 255, 255, 0.08)';
            for (let x = 1; x < CANVAS_WIDTH; x++) {
                overlayCtx.fillRect(x * zm, 0, 1, dispH);
            }
            for (let y = 1; y < CANVAS_HEIGHT; y++) {
                overlayCtx.fillRect(0, y * zm, dispW, 1);
            }
        }

        // 2. Vertical Symmetry Line (x = 80)
        // Trục đối xứng chuẩn xác tuyệt đối nằm giữa cột 79 và cột 80 (đốm 1px sắc nét, không canvas stroke)
        if (state.symmetry) {
            overlayCtx.fillStyle = '#e5ab2e';
            const symScreenX = 80 * zm;
            for (let sy = 0; sy < dispH; sy += 8) {
                overlayCtx.fillRect(symScreenX, sy, 1, 4);
            }
        }

        // 3. Floating Selection Preview (khi đang di chuyển vùng chọn)
        if (state.floatingSelection) {
            const fsX = state.floatingSelection.x * zm;
            const fsY = state.floatingSelection.y * zm;
            const fsW = state.floatingSelection.w * zm;
            const fsH = state.floatingSelection.h * zm;
            overlayCtx.imageSmoothingEnabled = false;
            overlayCtx.drawImage(state.floatingSelection.canvas, fsX, fsY, fsW, fsH);
        }

        // 4. Active Selection Marquee (100% viền integer pixel, ZERO canvas stroke, kiến bò nhấp nháy chuẩn nét)
        if (state.selection) {
            const selX0 = state.selection.x * zm;
            const selY0 = state.selection.y * zm;
            const selX1 = (state.selection.x + state.selection.w) * zm;
            const selY1 = (state.selection.y + state.selection.h) * zm;
            const timeDash = Math.floor(Date.now() / 80) % 8;

            // Đường ngang trên & dưới
            for (let px = selX0; px < selX1; px++) {
                const isWhite = Math.floor((px + timeDash) / 4) % 2 === 0;
                overlayCtx.fillStyle = isWhite ? '#ffffff' : '#000000';
                overlayCtx.fillRect(px, selY0, 1, 1);
                overlayCtx.fillRect(px, selY1 - 1, 1, 1);
            }

            // Đường dọc trái & phải
            for (let py = selY0; py < selY1; py++) {
                const isWhite = Math.floor((py + timeDash) / 4) % 2 === 0;
                overlayCtx.fillStyle = isWhite ? '#ffffff' : '#000000';
                overlayCtx.fillRect(selX0, py, 1, 1);
                overlayCtx.fillRect(selX1 - 1, py, 1, 1);
            }
        }

        // 5. Rect Tool Preview (Khung viền chuẩn pixel integer 1px, không tô màu mờ che pixel bên dưới)
        if (state.tool === 'rect' && state.rectStartX !== null && hoverX !== null && hoverY !== null) {
            const rx = Math.min(state.rectStartX, hoverX);
            const ry = Math.min(state.rectStartY, hoverY);
            const rw = Math.abs(hoverX - state.rectStartX) + 1;
            const rh = Math.abs(hoverY - state.rectStartY) + 1;
            const srx = rx * zm;
            const sry = ry * zm;
            const srw = rw * zm;
            const srh = rh * zm;

            overlayCtx.fillStyle = '#38bdf8';
            overlayCtx.fillRect(srx, sry, srw, 1);
            if (srh > 1) overlayCtx.fillRect(srx, sry + srh - 1, srw, 1);
            if (srh > 2) {
                overlayCtx.fillRect(srx, sry + 1, 1, srh - 2);
                if (srw > 1) overlayCtx.fillRect(srx + srw - 1, sry + 1, 1, srh - 2);
            }

            // Preview đối xứng nếu bật đối xứng
            if (state.symmetry) {
                const symRx = CANVAS_WIDTH - (rx + rw);
                const symSrx = symRx * zm;
                overlayCtx.fillStyle = '#e5ab2e';
                overlayCtx.fillRect(symSrx, sry, srw, 1);
                if (srh > 1) overlayCtx.fillRect(symSrx, sry + srh - 1, srw, 1);
                if (srh > 2) {
                    overlayCtx.fillRect(symSrx, sry + 1, 1, srh - 2);
                    if (srw > 1) overlayCtx.fillRect(symSrx + srw - 1, sry + 1, 1, srh - 2);
                }
            }
        }

        // 6. Brush Cursor Box (Khung viền 1px siêu nét, hoàn toàn trong suốt không làm mờ/lệch màu pixel bên dưới)
        if (state.tool !== 'select' && hoverX !== null && hoverY !== null) {
            const sz = state.brushSize;
            const half = Math.floor(sz / 2);
            const startX = hoverX - half;
            const startY = hoverY - half;

            const sx = startX * zm;
            const sy = startY * zm;
            const sw = sz * zm;
            const sh = sz * zm;

            // Khung viền 1px màn hình bao quanh đúng phạm vi cọ vẽ, không tô mờ bên trong
            overlayCtx.fillStyle = state.tool === 'eraser' ? '#ff4d4f' : '#ffffff';
            overlayCtx.fillRect(sx, sy, sw, 1);
            overlayCtx.fillRect(sx, sy + sh - 1, sw, 1);
            if (sh > 2) {
                overlayCtx.fillRect(sx, sy + 1, 1, sh - 2);
                overlayCtx.fillRect(sx + sw - 1, sy + 1, 1, sh - 2);
            }

            // Con trỏ đối xứng nếu bật đối xứng
            if (state.symmetry) {
                const symStartX = CANVAS_WIDTH - (startX + sz);
                const symSx = symStartX * zm;

                overlayCtx.fillStyle = '#e5ab2e';
                overlayCtx.fillRect(symSx, sy, sw, 1);
                overlayCtx.fillRect(symSx, sy + sh - 1, sw, 1);
                if (sh > 2) {
                    overlayCtx.fillRect(symSx, sy + 1, 1, sh - 2);
                    overlayCtx.fillRect(symSx + sw - 1, sy + 1, 1, sh - 2);
                }
            }
        }
    }

    // ================= PIXEL DRAWING LOGIC =================
    function getCanvasCoordinates(e) {
        const rect = paintCanvas.getBoundingClientRect();
        const clientX = e.clientX !== undefined ? e.clientX : (e.touches && e.touches[0].clientX);
        const clientY = e.clientY !== undefined ? e.clientY : (e.touches && e.touches[0].clientY);

        const isInside = clientX >= rect.left && clientX < rect.right && clientY >= rect.top && clientY < rect.bottom;

        const x = Math.floor((clientX - rect.left) / (rect.width / CANVAS_WIDTH));
        const y = Math.floor((clientY - rect.top) / (rect.height / CANVAS_HEIGHT));

        return {
            x: Math.max(0, Math.min(CANVAS_WIDTH - 1, x)),
            y: Math.max(0, Math.min(CANVAS_HEIGHT - 1, y)),
            isInside: isInside
        };
    }

    function saveHistoryState() {
        const imgData = paintCtx.getImageData(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        state.undoStack.push(imgData);
        if (state.undoStack.length > state.maxHistory) {
            state.undoStack.shift();
        }
        state.redoStack = [];
    }

    function undo() {
        if (state.undoStack.length > 1) {
            const current = state.undoStack.pop();
            state.redoStack.push(current);
            const prev = state.undoStack[state.undoStack.length - 1];
            paintCtx.putImageData(prev, 0, 0);
            syncLivePreviewAndThumb();
            scheduleAutoSave();
        }
    }

    function redo() {
        if (state.redoStack.length > 0) {
            const next = state.redoStack.pop();
            state.undoStack.push(next);
            paintCtx.putImageData(next, 0, 0);
            syncLivePreviewAndThumb();
            scheduleAutoSave();
        }
    }

    function clearCanvas() {
        saveHistoryState();
        paintCtx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        syncLivePreviewAndThumb();
        scheduleAutoSave();
        showToast('Đã xóa sạch frame hiện tại.');
    }

    function hexToRgba(hex) {
        const c = hex.replace('#', '');
        const r = parseInt(c.substring(0, 2), 16);
        const g = parseInt(c.substring(2, 4), 16);
        const b = parseInt(c.substring(4, 6), 16);
        return [r, g, b, 255];
    }

    function plotPixel(x, y, colorRgba) {
        if (x < 0 || x >= CANVAS_WIDTH || y < 0 || y >= CANVAS_HEIGHT) return;

        const sz = state.brushSize;
        const half = Math.floor(sz / 2);

        function paintAt(px, py) {
            if (state.tool === 'eraser') {
                paintCtx.clearRect(px, py, 1, 1);
            } else if (state.tool === 'dodge') {
                adjustBrightness(px, py, 1.2);
            } else if (state.tool === 'burn') {
                adjustBrightness(px, py, 0.8);
            } else {
                paintCtx.fillStyle = `rgba(${colorRgba[0]}, ${colorRgba[1]}, ${colorRgba[2]}, ${colorRgba[3] / 255})`;
                paintCtx.fillRect(px, py, 1, 1);
            }
        }

        for (let dy = 0; dy < sz; dy++) {
            for (let dx = 0; dx < sz; dx++) {
                const px = x - half + dx;
                const py = y - half + dy;

                if (px >= 0 && px < CANVAS_WIDTH && py >= 0 && py < CANVAS_HEIGHT) {
                    paintAt(px, py);
                    // Đối xứng trục giữa: phản chiếu chính xác từng pixel sang bên đối diện
                    if (state.symmetry) {
                        const symPx = CANVAS_WIDTH - 1 - px;
                        if (symPx >= 0 && symPx < CANVAS_WIDTH) {
                            paintAt(symPx, py);
                        }
                    }
                }
            }
        }
    }

    function adjustBrightness(px, py, factor) {
        const p = paintCtx.getImageData(px, py, 1, 1);
        if (p.data[3] === 0) return;
        p.data[0] = Math.min(255, Math.max(0, Math.round(p.data[0] * factor)));
        p.data[1] = Math.min(255, Math.max(0, Math.round(p.data[1] * factor)));
        p.data[2] = Math.min(255, Math.max(0, Math.round(p.data[2] * factor)));
        paintCtx.putImageData(p, px, py);
    }

    // Bresenham's line algorithm for smooth continuous strokes
    function drawLine(x0, y0, x1, y1, colorRgba) {
        const dx = Math.abs(x1 - x0);
        const dy = Math.abs(y1 - y0);
        const sx = (x0 < x1) ? 1 : -1;
        const sy = (y0 < y1) ? 1 : -1;
        let err = dx - dy;

        while (true) {
            plotPixel(x0, y0, colorRgba);
            if (x0 === x1 && y0 === y1) break;
            const e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    // Flood fill (Bucket tool) - Khớp màu pixel art chính xác 100%
    function floodFill(startX, startY, fillRgba) {
        const imgData = paintCtx.getImageData(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        const data = imgData.data;

        const idx = (startY * CANVAS_WIDTH + startX) * 4;
        const targetR = data[idx];
        const targetG = data[idx + 1];
        const targetB = data[idx + 2];
        const targetA = data[idx + 3];

        if (targetR === fillRgba[0] && targetG === fillRgba[1] && targetB === fillRgba[2] && targetA === fillRgba[3]) {
            return;
        }

        const stack = [[startX, startY]];
        const visited = new Uint8Array(CANVAS_WIDTH * CANVAS_HEIGHT);

        function match(x, y) {
            const i = (y * CANVAS_WIDTH + x) * 4;
            return data[i] === targetR &&
                   data[i + 1] === targetG &&
                   data[i + 2] === targetB &&
                   data[i + 3] === targetA;
        }

        while (stack.length > 0) {
            const [x, y] = stack.pop();
            const pos = y * CANVAS_WIDTH + x;
            if (visited[pos]) continue;
            visited[pos] = 1;

            if (match(x, y)) {
                const i = pos * 4;
                data[i] = fillRgba[0];
                data[i + 1] = fillRgba[1];
                data[i + 2] = fillRgba[2];
                data[i + 3] = fillRgba[3];

                if (x > 0) stack.push([x - 1, y]);
                if (x < CANVAS_WIDTH - 1) stack.push([x + 1, y]);
                if (y > 0) stack.push([x, y - 1]);
                if (y < CANVAS_HEIGHT - 1) stack.push([x, y + 1]);
            }
        }

        paintCtx.putImageData(imgData, 0, 0);
    }

    // Eyedropper tool
    function pickColor(x, y) {
        const p = paintCtx.getImageData(x, y, 1, 1).data;
        if (p[3] > 0) {
            const hex = '#' + ((1 << 24) + (p[0] << 16) + (p[1] << 8) + p[2]).toString(16).slice(1).toUpperCase();
            setColor(hex);
            showToast(`Đã chọn màu: ${hex}`);
        }
    }

    // ================= MARQUEE SELECTION & MOVE FUNCTIONS =================
    function updateSelectionUI() {
        const sec = document.getElementById('selectionActionSection');
        if (!sec) return;
        if (state.selection) {
            sec.classList.remove('hidden');
            const dims = document.getElementById('selectionDims');
            if (dims) dims.textContent = `${state.selection.w}x${state.selection.h}`;
        } else {
            sec.classList.add('hidden');
        }
    }

    function commitSelection() {
        if (state.floatingSelection) {
            paintCtx.drawImage(state.floatingSelection.canvas, state.floatingSelection.x, state.floatingSelection.y);
            state.floatingSelection = null;
            syncLivePreviewAndThumb();
            scheduleAutoSave();
        }
        state.selection = null;
        updateSelectionUI();
        renderOverlay(state.lastHoverX, state.lastHoverY);
    }

    function liftSelection() {
        if (!state.selection || state.floatingSelection) return;
        saveHistoryState();
        const { x, y, w, h } = state.selection;
        const liftedCanvas = document.createElement('canvas');
        liftedCanvas.width = w;
        liftedCanvas.height = h;
        const liftedCtx = liftedCanvas.getContext('2d');
        liftedCtx.drawImage(paintCanvas, x, y, w, h, 0, 0, w, h);

        // Xóa sạch vùng cũ trên canvas để nâng lên vùng nổi di chuyển
        paintCtx.clearRect(x, y, w, h);

        state.floatingSelection = {
            x: x,
            y: y,
            w: w,
            h: h,
            canvas: liftedCanvas,
            originX: x,
            originY: y
        };
        syncLivePreviewAndThumb();
    }

    function deleteSelection() {
        if (!state.selection) return;
        saveHistoryState();
        if (state.floatingSelection) {
            state.floatingSelection = null;
        } else {
            paintCtx.clearRect(state.selection.x, state.selection.y, state.selection.w, state.selection.h);
        }
        state.selection = null;
        updateSelectionUI();
        syncLivePreviewAndThumb();
        scheduleAutoSave();
        renderOverlay(state.lastHoverX, state.lastHoverY);
        showToast('🗑️ Đã xóa vùng chọn.');
    }

    function copySelection() {
        if (!state.selection) return;
        const { x, y, w, h } = state.selection;
        const clipCanvas = document.createElement('canvas');
        clipCanvas.width = w;
        clipCanvas.height = h;
        const clipCtx = clipCanvas.getContext('2d');

        if (state.floatingSelection) {
            clipCtx.drawImage(state.floatingSelection.canvas, 0, 0);
        } else {
            clipCtx.drawImage(paintCanvas, x, y, w, h, 0, 0, w, h);
        }

        state.clipboard = { w, h, canvas: clipCanvas };
        showToast(`📋 Đã sao chép vùng chọn (${w}x${h}px)`);
    }

    function cutSelection() {
        if (!state.selection) return;
        copySelection();
        deleteSelection();
        showToast('✂️ Đã cắt vùng chọn.');
    }

    function pasteSelection() {
        if (!state.clipboard) {
            showToast('Bộ nhớ tạm đang trống.', true);
            return;
        }
        commitSelection();
        saveHistoryState();

        const w = state.clipboard.w;
        const h = state.clipboard.h;
        let px = state.lastHoverX !== null ? Math.floor(state.lastHoverX - w / 2) : Math.floor((CANVAS_WIDTH - w) / 2);
        let py = state.lastHoverY !== null ? Math.floor(state.lastHoverY - h / 2) : Math.floor((CANVAS_HEIGHT - h) / 2);
        px = Math.max(0, Math.min(CANVAS_WIDTH - w, px));
        py = Math.max(0, Math.min(CANVAS_HEIGHT - h, py));

        const pastedCanvas = document.createElement('canvas');
        pastedCanvas.width = w;
        pastedCanvas.height = h;
        pastedCanvas.getContext('2d').drawImage(state.clipboard.canvas, 0, 0);

        state.floatingSelection = {
            x: px,
            y: py,
            w: w,
            h: h,
            canvas: pastedCanvas,
            originX: px,
            originY: py
        };
        state.selection = { x: px, y: py, w, h };
        setTool('select');
        updateSelectionUI();
        renderOverlay(state.lastHoverX, state.lastHoverY);
        showToast(`📋 Đã dán (${w}x${h}px). Kéo chuột để di chuyển!`);
    }

    // Flip Horizontal Frame (hoặc Lật Ngang Vùng Chọn nếu đang có selection)
    function flipHorizontal() {
        if (state.selection) {
            saveHistoryState();
            if (!state.floatingSelection) {
                liftSelection();
            }
            const w = state.floatingSelection.w;
            const h = state.floatingSelection.h;
            const tmp = document.createElement('canvas');
            tmp.width = w;
            tmp.height = h;
            const tmpCtx = tmp.getContext('2d');
            tmpCtx.translate(w, 0);
            tmpCtx.scale(-1, 1);
            tmpCtx.drawImage(state.floatingSelection.canvas, 0, 0);

            state.floatingSelection.canvas = tmp;
            renderOverlay(state.lastHoverX, state.lastHoverY);
            showToast('↔️ Đã lật ngang vùng chọn.');
            return;
        }

        saveHistoryState();
        const tmp = document.createElement('canvas');
        tmp.width = CANVAS_WIDTH;
        tmp.height = CANVAS_HEIGHT;
        const tmpCtx = tmp.getContext('2d');
        tmpCtx.drawImage(paintCanvas, 0, 0);

        paintCtx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        paintCtx.save();
        paintCtx.translate(CANVAS_WIDTH, 0);
        paintCtx.scale(-1, 1);
        paintCtx.drawImage(tmp, 0, 0);
        paintCtx.restore();

        syncLivePreviewAndThumb();
        scheduleAutoSave();
        showToast('Đã lật ngang frame thành công.');
    }

    // ================= SYNC & AUTO-SAVE =================
    function syncLivePreviewAndThumb() {
        const key = getCacheKey(state.gender, state.direction, state.frame);

        // Update cache copy (nếu đang di chuyển selection, vẽ kèm cả vùng đang bay để preview không bị thủng lỗ)
        const copyCanvas = document.createElement('canvas');
        copyCanvas.width = CANVAS_WIDTH;
        copyCanvas.height = CANVAS_HEIGHT;
        const copyCtx = copyCanvas.getContext('2d');
        copyCtx.drawImage(paintCanvas, 0, 0);
        if (state.floatingSelection) {
            copyCtx.drawImage(state.floatingSelection.canvas, state.floatingSelection.x, state.floatingSelection.y);
        }
        state.frameImagesCache[key] = copyCanvas;

        // Update active thumbnail
        const thumbImg = document.getElementById(`thumbImg_${state.frame}`);
        if (thumbImg) {
            thumbImg.src = copyCanvas.toDataURL('image/png');
        }
    }

    function scheduleAutoSave() {
        if (!state.autoSave) return;
        clearTimeout(state.autoSaveTimeout);
        const statusEl = document.getElementById('saveStatusIndicator');
        statusEl.className = 'status-indicator saving';
        statusEl.textContent = 'Đang tự lưu...';

        state.autoSaveTimeout = setTimeout(() => {
            saveCurrentFrame(true);
        }, 600);
    }

    async function saveCurrentFrame(isAuto = false) {
        // Tự động chốt vùng chọn nổi vào canvas trước khi lưu
        if (state.floatingSelection) {
            commitSelection();
        }

        const statusEl = document.getElementById('saveStatusIndicator');
        statusEl.className = 'status-indicator saving';
        statusEl.textContent = 'Đang lưu vào Unity...';

        const dataUrl = paintCanvas.toDataURL('image/png');
        try {
            const resp = await fetch('/api/save_frame', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    gender: state.gender,
                    direction: state.direction,
                    frame: state.frame,
                    image_data: dataUrl
                })
            });
            const res = await resp.json();
            if (res.success) {
                statusEl.className = 'status-indicator saved';
                statusEl.textContent = '✓ Đã đồng bộ Unity';
                if (!isAuto) showToast(`💾 ${res.message}`);
            } else {
                statusEl.className = 'status-indicator';
                statusEl.textContent = 'Lỗi lưu';
                showToast(`❌ ${res.error || 'Lỗi khi lưu'}`, true);
            }
        } catch (err) {
            statusEl.className = 'status-indicator';
            statusEl.textContent = 'Lỗi kết nối';
            showToast('❌ Không thể kết nối tới server.', true);
        }
    }

    // ================= FILMSTRIP / TIMELINE =================
    function renderFilmstripThumbnails() {
        const container = document.getElementById('frameThumbnails');
        container.innerHTML = '';

        for (let f = 0; f < 8; f++) {
            const card = document.createElement('div');
            card.className = `frame-card ${f === state.frame ? 'active' : ''}`;
            card.dataset.frame = f;

            const key = getCacheKey(state.gender, state.direction, f);
            const cached = state.frameImagesCache[key];
            const imgSrc = cached ? (cached.src || cached.toDataURL?.() || '') : '';

            card.innerHTML = `
                <div class="frame-thumb-box">
                    <img id="thumbImg_${f}" class="frame-thumb-img" src="${imgSrc}" alt="Frame ${f}">
                </div>
                <span class="frame-label">Frame ${f}</span>
                <span class="frame-badge">${f === 0 ? 'IDLE' : `WALK ${f}`}</span>
            `;

            card.addEventListener('click', () => {
                selectFrame(f);
            });

            container.appendChild(card);
        }
    }

    function selectFrame(f) {
        if (state.frame === f) return;
        commitSelection();
        state.frame = f;

        document.querySelectorAll('.frame-card').forEach(c => {
            c.classList.toggle('active', parseInt(c.dataset.frame) === f);
        });

        loadCurrentFrameToCanvas();
    }

    // ================= DIRECTION & GENDER SWITCHING =================
    async function setDirection(dir) {
        if (state.direction === dir) return;
        commitSelection();
        state.direction = dir;

        document.querySelectorAll('.dir-tab').forEach(t => {
            t.classList.toggle('active', t.dataset.dir === dir);
        });
        document.querySelectorAll('.compass-btn').forEach(b => {
            b.classList.toggle('active', b.dataset.dir === dir);
        });

        const dirObj = DIRECTIONS.find(d => d.id === dir);
        if (dirObj) {
            document.getElementById('compassDirCode').textContent = dirObj.code;
        }

        await loadAllDirectionFrames(state.gender, state.direction);
        loadCurrentFrameToCanvas();
        showToast(`Đã chuyển sang hướng: ${dirObj ? dirObj.name : dir}`);
    }

    async function setGender(gender) {
        if (state.gender === gender) return;
        commitSelection();
        state.gender = gender;

        document.querySelectorAll('.gender-btn').forEach(b => {
            b.classList.toggle('active', b.dataset.gender === gender);
        });

        const copyBtn = document.getElementById('btnCopyGender');
        copyBtn.textContent = gender === 'male' ? '📋 Nhân Bản Sang Nữ' : '📋 Nhân Bản Sang Nam';

        await loadAllDirectionFrames(state.gender, state.direction);
        loadCurrentFrameToCanvas();
        showToast(`Đã chuyển sang nhân vật: ${gender === 'male' ? 'Nam (Male)' : 'Nữ (Female)'}`);
    }

    // ================= COLOR & TOOLS =================
    function setColor(hex) {
        state.color = hex;
        document.getElementById('nativeColorPicker').value = hex;
        document.getElementById('currentColorBox').style.backgroundColor = hex;
        document.getElementById('hexColorLabel').textContent = hex;

        if (!state.recentColors.includes(hex)) {
            state.recentColors.unshift(hex);
            if (state.recentColors.length > 8) state.recentColors.pop();
            renderRecentColors();
        }
    }

    function renderRecentColors() {
        const container = document.getElementById('recentSwatches');
        container.innerHTML = '';
        state.recentColors.forEach(col => {
            const s = document.createElement('span');
            s.className = 'swatch';
            s.style.backgroundColor = col;
            s.dataset.color = col;
            s.addEventListener('click', () => setColor(col));
            container.appendChild(s);
        });
    }

    function setTool(toolName) {
        if (state.tool === 'select' && toolName !== 'select') {
            commitSelection();
        }
        state.tool = toolName;
        document.querySelectorAll('.tool-btn').forEach(b => {
            b.classList.toggle('active', b.dataset.tool === toolName);
        });
        if (toolName !== 'select') {
            paintCanvas.style.cursor = 'crosshair';
        }
    }

    // ================= LIVE PREVIEW LOOP =================
    function previewAnimationLoop(now) {
        const delta = (now - state.lastFrameTime) / 1000;
        state.lastFrameTime = now;

        previewCtx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        let activeFrameIdx = 0;
        let scaleOffsetY = 0;

        if (state.previewMode === 'walk') {
            state.animTimer += delta * state.previewFps;
            activeFrameIdx = Math.floor(state.animTimer) % 8;
            document.getElementById('previewBadge').textContent = `WALKING (${state.previewFps.toFixed(1)} FPS)`;
        } else {
            // Idle Mode: lock to Frame 0 and add breathing oscillation (Unity Match)
            activeFrameIdx = 0;
            const breath = Math.sin((now / 1000) * 2.4) * 0.015;
            scaleOffsetY = breath;
            document.getElementById('previewBadge').textContent = 'IDLE (FRAME 0)';
        }

        // Draw character frame
        const key = getCacheKey(state.gender, state.direction, activeFrameIdx);
        let imgToDraw = state.frameImagesCache[key];

        // If currently editing this frame on canvas, render live canvas!
        if (activeFrameIdx === state.frame) {
            imgToDraw = (state.floatingSelection && state.frameImagesCache[key]) ? state.frameImagesCache[key] : paintCanvas;
        }

        if (imgToDraw) {
            previewCtx.save();
            if (scaleOffsetY !== 0) {
                previewCtx.translate(80, 80);
                previewCtx.scale(1 - scaleOffsetY * 0.5, 1 + scaleOffsetY);
                previewCtx.translate(-80, -80);
            }
            previewCtx.drawImage(imgToDraw, 0, 0);
            previewCtx.restore();
        }

        // Nếu đang có vùng chọn, tự động render lại overlay để hiệu ứng kiến bò (marching ants) chuyển động liên tục
        if (state.selection) {
            renderOverlay(state.lastHoverX, state.lastHoverY);
        }

        requestAnimationFrame(previewAnimationLoop);
    }

    // ================= EVENT BINDINGS =================
    function bindEvents() {
        // Pointer drawing on Paint Canvas
        paintCanvas.addEventListener('mousedown', (e) => {
            const coords = getCanvasCoordinates(e);

            // Thao tác với công cụ Vùng chọn (Select / Move)
            if (state.tool === 'select') {
                const inSelection = state.selection &&
                    coords.x >= state.selection.x && coords.x < state.selection.x + state.selection.w &&
                    coords.y >= state.selection.y && coords.y < state.selection.y + state.selection.h;

                if (inSelection) {
                    // Click trong vùng chọn: Nhấc pixel lên & kéo di chuyển!
                    if (!state.floatingSelection) {
                        liftSelection();
                    }
                    state.isDraggingSelection = true;
                    state.dragSelectStartX = coords.x;
                    state.dragSelectStartY = coords.y;
                    state.dragSelectOriginX = state.floatingSelection.x;
                    state.dragSelectOriginY = state.floatingSelection.y;
                } else {
                    // Click ra ngoài: Áp dụng vùng chọn cũ, bắt đầu tạo vùng chọn mới!
                    commitSelection();
                    state.isSelecting = true;
                    state.selectStartX = coords.x;
                    state.selectStartY = coords.y;
                    state.selection = { x: coords.x, y: coords.y, w: 1, h: 1 };
                    updateSelectionUI();
                }
                renderOverlay(coords.x, coords.y);
                return;
            }

            state.isDrawing = true;
            saveHistoryState();

            state.drawStartX = coords.x;
            state.drawStartY = coords.y;
            state.lockedAxis = null;

            if (state.tool === 'bucket') {
                floodFill(coords.x, coords.y, hexToRgba(state.color));
                if (state.symmetry) {
                    const symX = CANVAS_WIDTH - 1 - coords.x;
                    floodFill(symX, coords.y, hexToRgba(state.color));
                }
                syncLivePreviewAndThumb();
                scheduleAutoSave();
            } else if (state.tool === 'eyedropper') {
                pickColor(coords.x, coords.y);
            } else if (state.tool === 'rect') {
                state.rectStartX = coords.x;
                state.rectStartY = coords.y;
            } else {
                plotPixel(coords.x, coords.y, hexToRgba(state.color));
                state.lastX = coords.x;
                state.lastY = coords.y;
                syncLivePreviewAndThumb();
            }
        });

        window.addEventListener('mousemove', (e) => {
            let coords = getCanvasCoordinates(e);
            if (!coords.isInside && !state.isDrawing && !state.isSelecting && !state.isDraggingSelection) {
                state.lastHoverX = null;
                state.lastHoverY = null;
                updateCoordinates('-', '-');
                renderOverlay(null, null);
                return;
            }
            state.lastHoverX = coords.x;
            state.lastHoverY = coords.y;

            if (state.tool === 'select') {
                if (state.isSelecting) {
                    const minX = Math.min(state.selectStartX, coords.x);
                    const minY = Math.min(state.selectStartY, coords.y);
                    const maxX = Math.max(state.selectStartX, coords.x);
                    const maxY = Math.max(state.selectStartY, coords.y);
                    state.selection = {
                        x: minX,
                        y: minY,
                        w: maxX - minX + 1,
                        h: maxY - minY + 1
                    };
                    updateSelectionUI();
                    updateCoordinates(coords.x, coords.y);
                    renderOverlay(coords.x, coords.y);
                    return;
                }

                if (state.isDraggingSelection && state.floatingSelection) {
                    const dx = coords.x - state.dragSelectStartX;
                    const dy = coords.y - state.dragSelectStartY;
                    let nx = state.dragSelectOriginX + dx;
                    let ny = state.dragSelectOriginY + dy;
                    nx = Math.max(0, Math.min(CANVAS_WIDTH - state.floatingSelection.w, nx));
                    ny = Math.max(0, Math.min(CANVAS_HEIGHT - state.floatingSelection.h, ny));
                    state.floatingSelection.x = nx;
                    state.floatingSelection.y = ny;
                    state.selection.x = nx;
                    state.selection.y = ny;
                    updateCoordinates(coords.x, coords.y);
                    renderOverlay(coords.x, coords.y);
                    return;
                }

                const inSel = state.selection &&
                    coords.x >= state.selection.x && coords.x < state.selection.x + state.selection.w &&
                    coords.y >= state.selection.y && coords.y < state.selection.y + state.selection.h;
                paintCanvas.style.cursor = inSel ? 'move' : 'crosshair';

                updateCoordinates(coords.x, coords.y);
                renderOverlay(coords.x, coords.y);
                return;
            }

            // Tự động căn di chuyển chuột theo hàng/cột (Orthogonal Lock chống lệch nét)
            const isOrtho = state.orthoLock || e.shiftKey || state.shiftKey;
            if (state.isDrawing && isOrtho && state.drawStartX !== null && state.drawStartY !== null) {
                if (!state.lockedAxis) {
                    const dx = Math.abs(coords.x - state.drawStartX);
                    const dy = Math.abs(coords.y - state.drawStartY);
                    if (dx >= 2 || dy >= 2) {
                        state.lockedAxis = dx >= dy ? 'x' : 'y';
                    }
                }
                if (state.lockedAxis === 'x') {
                    coords.y = state.drawStartY; // Khóa chặt hàng ngang
                } else if (state.lockedAxis === 'y') {
                    coords.x = state.drawStartX; // Khóa chặt cột dọc
                }
            }

            updateCoordinates(coords.x, coords.y);
            renderOverlay(coords.x, coords.y);

            if (!state.isDrawing) return;

            if (state.tool === 'rect') {
                // renderOverlay đã preview khung khít pixel hoàn hảo
            } else if (state.tool === 'pencil' || state.tool === 'eraser' || state.tool === 'dodge' || state.tool === 'burn') {
                if (state.lastX !== null && state.lastY !== null) {
                    drawLine(state.lastX, state.lastY, coords.x, coords.y, hexToRgba(state.color));
                }
                state.lastX = coords.x;
                state.lastY = coords.y;
                syncLivePreviewAndThumb();
            }
        });

        window.addEventListener('mouseup', (e) => {
            const coords = getCanvasCoordinates(e);
            if (state.tool === 'select') {
                if (state.isSelecting) {
                    state.isSelecting = false;
                    // Nếu click chuột một điểm mà không kéo tạo hộp chọn -> bỏ chọn
                    if (state.selection && state.selectStartX === coords.x && state.selectStartY === coords.y && state.selection.w <= 1 && state.selection.h <= 1) {
                        state.selection = null;
                        updateSelectionUI();
                    }
                }
                if (state.isDraggingSelection) {
                    state.isDraggingSelection = false;
                    syncLivePreviewAndThumb();
                }
                renderOverlay(coords.x, coords.y);
                return;
            }

            if (state.isDrawing) {
                if (state.tool === 'rect' && state.rectStartX !== null) {
                    const coords = getCanvasCoordinates(e);
                    const rx = Math.min(state.rectStartX, coords.x);
                    const ry = Math.min(state.rectStartY, coords.y);
                    const rw = Math.abs(coords.x - state.rectStartX) + 1;
                    const rh = Math.abs(coords.y - state.rectStartY) + 1;
                    paintCtx.fillStyle = state.color;
                    paintCtx.fillRect(rx, ry, rw, rh);
                    if (state.symmetry) {
                        const symRx = CANVAS_WIDTH - (rx + rw);
                        paintCtx.fillRect(symRx, ry, rw, rh);
                    }
                    state.rectStartX = null;
                    state.rectStartY = null;
                }
                state.isDrawing = false;
                state.lastX = null;
                state.lastY = null;
                state.drawStartX = null;
                state.drawStartY = null;
                state.lockedAxis = null;
                syncLivePreviewAndThumb();
                scheduleAutoSave();
            }
        });

        // Zoom controls
        document.getElementById('btnZoomIn').addEventListener('click', zoomIn);
        document.getElementById('btnZoomOut').addEventListener('click', zoomOut);
        document.getElementById('btnZoomReset').addEventListener('click', zoomReset);

        canvasViewport.addEventListener('wheel', (e) => {
            if (e.ctrlKey || e.metaKey || e.altKey) {
                e.preventDefault();
                if (e.deltaY < 0) zoomIn();
                else zoomOut();
            }
        });

        // Direction buttons
        document.querySelectorAll('.dir-tab').forEach(tab => {
            tab.addEventListener('click', () => setDirection(tab.dataset.dir));
        });
        document.querySelectorAll('.compass-btn').forEach(btn => {
            btn.addEventListener('click', () => setDirection(btn.dataset.dir));
        });

        // Gender buttons
        document.getElementById('btnGenderMale').addEventListener('click', () => setGender('male'));
        document.getElementById('btnGenderFemale').addEventListener('click', () => setGender('female'));

        // Tool buttons
        document.querySelectorAll('.tool-btn').forEach(btn => {
            btn.addEventListener('click', () => setTool(btn.dataset.tool));
        });

        // Brush size
        document.querySelectorAll('.size-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                state.brushSize = parseInt(btn.dataset.size);
                document.querySelectorAll('.size-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
            });
        });

        // Symmetry toggle
        const symBtn = document.getElementById('btnToggleSymmetry');
        symBtn.addEventListener('click', () => {
            state.symmetry = !state.symmetry;
            symBtn.classList.toggle('active', state.symmetry);
            document.getElementById('symStatus').textContent = state.symmetry ? 'BẬT' : 'TẮT';
            renderOverlay();
            showToast(state.symmetry ? '🪞 Đã BẬT vẽ đối xứng trục giữa.' : '🪞 Đã TẮT vẽ đối xứng.');
        });

        // Flip Horizontal
        document.getElementById('btnFlipH').addEventListener('click', flipHorizontal);

        // Undo & Redo & Clear
        document.getElementById('btnUndo').addEventListener('click', undo);
        document.getElementById('btnRedo').addEventListener('click', redo);
        document.getElementById('btnClearCanvas').addEventListener('click', clearCanvas);

        // Save button
        document.getElementById('btnSaveFrame').addEventListener('click', () => saveCurrentFrame(false));

        // Auto Save toggle
        document.getElementById('chkAutoSave').addEventListener('change', (e) => {
            state.autoSave = e.target.checked;
        });

        // Grid & Onion Skin & Crosshair & OrthoLock
        document.getElementById('chkGrid').addEventListener('change', (e) => {
            state.showGrid = e.target.checked;
            renderOverlay();
        });
        document.getElementById('chkOnionSkin').addEventListener('change', (e) => {
            state.onionSkin = e.target.checked;
            renderOnionSkin();
        });
        const chkOrtho = document.getElementById('chkOrthoLock');
        if (chkOrtho) {
            chkOrtho.addEventListener('change', (e) => {
                state.orthoLock = e.target.checked;
                showToast(state.orthoLock ? '📐 Đã BẬT Khóa hàng/cột (chống lệch nét).' : '📐 Đã TẮT Khóa hàng/cột.');
            });
        }

        // Color palette swatches
        document.querySelectorAll('.swatch').forEach(sw => {
            sw.addEventListener('click', () => {
                document.querySelectorAll('.swatch').forEach(s => s.classList.remove('active'));
                sw.classList.add('active');
                setColor(sw.dataset.color);
            });
        });

        // Native Color Picker
        document.getElementById('nativeColorPicker').addEventListener('input', (e) => {
            setColor(e.target.value);
        });

        // Preview Controls
        document.getElementById('btnModeWalk').addEventListener('click', () => {
            state.previewMode = 'walk';
            document.getElementById('btnModeWalk').classList.add('active');
            document.getElementById('btnModeIdle').classList.remove('active');
        });
        document.getElementById('btnModeIdle').addEventListener('click', () => {
            state.previewMode = 'idle';
            document.getElementById('btnModeIdle').classList.add('active');
            document.getElementById('btnModeWalk').classList.remove('active');
        });

        const rngFps = document.getElementById('rngFps');
        rngFps.addEventListener('input', (e) => {
            state.previewFps = parseFloat(e.target.value);
            document.getElementById('fpsValueLabel').textContent = `${state.previewFps.toFixed(1)} FPS`;
        });

        // Preview Backgrounds
        document.querySelectorAll('.bg-opt').forEach(btn => {
            btn.addEventListener('click', () => {
                document.querySelectorAll('.bg-opt').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                const box = document.getElementById('previewBox');
                box.className = 'preview-stage-box';
                if (btn.dataset.bg !== 'checker') {
                    box.classList.add(`bg-${btn.dataset.bg}`);
                }
            });
        });

        // Preview Scales
        document.querySelectorAll('.scale-opt').forEach(btn => {
            btn.addEventListener('click', () => {
                document.querySelectorAll('.scale-opt').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                const sc = parseInt(btn.dataset.scale);
                previewCanvas.style.transform = `scale(${sc})`;
            });
        });

        // Copy Gender
        document.getElementById('btnCopyGender').addEventListener('click', async () => {
            const targetGender = state.gender === 'male' ? 'female' : 'male';
            const confirmMsg = `Bạn có chắc muốn sao chép toàn bộ 64 frame từ ${state.gender === 'male' ? 'Nam' : 'Nữ'} sang ${targetGender === 'male' ? 'Nam' : 'Nữ'}? (Hành động này sẽ ghi đè)`;
            if (!confirm(confirmMsg)) return;

            try {
                const resp = await fetch('/api/copy_gender', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        source_gender: state.gender,
                        target_gender: targetGender
                    })
                });
                const res = await resp.json();
                if (res.success) {
                    showToast(`✓ ${res.message}`);
                } else {
                    showToast(`❌ ${res.error}`, true);
                }
            } catch (err) {
                showToast('❌ Lỗi khi sao chép giới tính.', true);
            }
        });

        // Mirror Opposite Direction
        document.getElementById('btnMirrorOpposite').addEventListener('click', async () => {
            const pairs = {
                'west': 'east',
                'east': 'west',
                'south_west': 'south_east',
                'south_east': 'south_west',
                'north_west': 'north_east',
                'north_east': 'north_west'
            };
            const targetDir = pairs[state.direction];
            if (!targetDir) {
                showToast('⚠️ Hướng South và North đối xứng tâm nên không có hướng đối diện.', true);
                return;
            }

            const confirmMsg = `Sao chép đối xứng cả 8 frame từ [${state.direction}] sang [${targetDir}] cho nhân vật ${state.gender}?`;
            if (!confirm(confirmMsg)) return;

            try {
                const resp = await fetch('/api/mirror_direction', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        gender: state.gender,
                        source_direction: state.direction,
                        target_direction: targetDir
                    })
                });
                const res = await resp.json();
                if (res.success) {
                    showToast(`✓ ${res.message}`);
                } else {
                    showToast(`❌ ${res.error}`, true);
                }
            } catch (err) {
                showToast('❌ Lỗi khi sao chép đối xứng hướng.', true);
            }
        });

        // Restore Backup
        document.getElementById('btnRestoreBackup').addEventListener('click', async () => {
            if (!confirm(`Khôi phục Frame ${state.frame} hướng ${state.direction} (${state.gender}) về bản gốc an toàn?`)) return;

            try {
                const resp = await fetch('/api/restore_backup', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        gender: state.gender,
                        direction: state.direction,
                        frame: state.frame
                    })
                });
                const res = await resp.json();
                if (res.success) {
                    delete state.frameImagesCache[getCacheKey(state.gender, state.direction, state.frame)];
                    await loadAllDirectionFrames(state.gender, state.direction);
                    loadCurrentFrameToCanvas();
                    showToast(`✓ ${res.message}`);
                }
            } catch (err) {
                showToast('❌ Lỗi khi khôi phục.', true);
            }
        });

        // Export Sprite Sheet
        document.getElementById('btnExportSheet').addEventListener('click', async () => {
            showToast('⏳ Đang xuất Sprite Sheet 8 Hướng (1280x1280)...');
            try {
                const resp = await fetch('/api/export_sheet', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ gender: state.gender })
                });
                const res = await resp.json();
                if (res.success) {
                    showToast(`✓ ${res.message}`);
                }
            } catch (err) {
                showToast('❌ Lỗi khi xuất Sprite Sheet.', true);
            }
        });

        // Selection Action Buttons
        const btnDeselect = document.getElementById('btnDeselect');
        if (btnDeselect) btnDeselect.addEventListener('click', commitSelection);
        const btnCutSel = document.getElementById('btnCutSelection');
        if (btnCutSel) btnCutSel.addEventListener('click', cutSelection);
        const btnCopySel = document.getElementById('btnCopySelection');
        if (btnCopySel) btnCopySel.addEventListener('click', copySelection);
        const btnDeleteSel = document.getElementById('btnDeleteSelection');
        if (btnDeleteSel) btnDeleteSel.addEventListener('click', deleteSelection);

        // Keyboard Shortcuts
        window.addEventListener('keydown', (e) => {
            if (e.key === 'Shift') {
                state.shiftKey = true;
            }

            // Check if user is typing in an input
            if (e.target.tagName === 'INPUT') return;

            // Arrow Keys Nudge khi đang có vùng chọn (Di chuyển pixel từng nấc 1px hoặc 4px với Shift)
            if (state.selection && (e.key === 'ArrowUp' || e.key === 'ArrowDown' || e.key === 'ArrowLeft' || e.key === 'ArrowRight')) {
                e.preventDefault();
                if (!state.floatingSelection) {
                    liftSelection();
                }
                const step = e.shiftKey ? 4 : 1;
                let nx = state.floatingSelection.x;
                let ny = state.floatingSelection.y;
                if (e.key === 'ArrowUp') ny -= step;
                if (e.key === 'ArrowDown') ny += step;
                if (e.key === 'ArrowLeft') nx -= step;
                if (e.key === 'ArrowRight') nx += step;
                nx = Math.max(0, Math.min(CANVAS_WIDTH - state.floatingSelection.w, nx));
                ny = Math.max(0, Math.min(CANVAS_HEIGHT - state.floatingSelection.h, ny));
                state.floatingSelection.x = nx;
                state.floatingSelection.y = ny;
                state.selection.x = nx;
                state.selection.y = ny;
                renderOverlay(state.lastHoverX, state.lastHoverY);
                syncLivePreviewAndThumb();
                return;
            }

            // Delete / Backspace khi đang có vùng chọn: Xóa vùng chọn
            if (state.selection && (e.key === 'Delete' || e.key === 'Backspace')) {
                e.preventDefault();
                deleteSelection();
                return;
            }

            // Enter / Escape khi đang có vùng chọn: Áp dụng / Bỏ chọn
            if (state.selection && (e.key === 'Enter' || e.key === 'Escape')) {
                e.preventDefault();
                commitSelection();
                showToast('✔️ Đã áp dụng vùng chọn.');
                return;
            }

            // Ctrl + C (Copy selection)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'c') {
                if (state.selection) {
                    e.preventDefault();
                    copySelection();
                    return;
                }
            }

            // Ctrl + X (Cut selection)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'x') {
                if (state.selection) {
                    e.preventDefault();
                    cutSelection();
                    return;
                }
            }

            // Ctrl + V (Paste selection)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'v') {
                if (state.clipboard) {
                    e.preventDefault();
                    pasteSelection();
                    return;
                }
            }


            // Ctrl + S (Save)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
                e.preventDefault();
                saveCurrentFrame(false);
                return;
            }

            // Ctrl + Z (Undo)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'z' && !e.shiftKey) {
                e.preventDefault();
                undo();
                return;
            }

            // Ctrl + Y or Ctrl + Shift + Z (Redo)
            if (((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'y') ||
                ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key.toLowerCase() === 'z')) {
                e.preventDefault();
                redo();
                return;
            }

            // Direction Keys (WASD / Arrows khi KHÔNG có selection)
            if (!state.selection) {
                if (e.key === 'ArrowDown' || e.key.toLowerCase() === 's' && !e.ctrlKey) {
                    setDirection('south');
                } else if (e.key === 'ArrowUp' || e.key.toLowerCase() === 'w') {
                    setDirection('north');
                } else if (e.key === 'ArrowLeft' || e.key.toLowerCase() === 'a') {
                    setDirection('west');
                } else if (e.key === 'ArrowRight' || e.key.toLowerCase() === 'd') {
                    setDirection('east');
                }
            }

            // 1..8 number keys for 8 directions
            const num = parseInt(e.key);
            if (num >= 1 && num <= 8) {
                setDirection(DIRECTIONS[num - 1].id);
            }

            // Tool shortcuts
            if (e.key.toLowerCase() === 'p') setTool('pencil');
            if (e.key.toLowerCase() === 'e') setTool('eraser');
            if (e.key.toLowerCase() === 'm' || (e.key.toLowerCase() === 's' && !e.ctrlKey && !state.selection)) setTool('select');
            if (e.key.toLowerCase() === 'b') setTool('bucket');
            if (e.key.toLowerCase() === 'i') setTool('eyedropper');
            if (e.key.toLowerCase() === 'r') setTool('rect');

            // H key: Lật ngang frame hoặc lật ngang vùng chọn
            if (e.key.toLowerCase() === 'h' && !e.ctrlKey && !e.metaKey && !e.altKey) {
                flipHorizontal();
            }
        });

        window.addEventListener('keyup', (e) => {
            if (e.key === 'Shift') {
                state.shiftKey = false;
            }
        });
    }

    // ================= HELPERS & TOAST =================
    function updateCoordinates(x, y) {
        document.getElementById('statusCoordinates').textContent = `X: ${x}, Y: ${y}`;
    }

    function updateStatus() {
        document.getElementById('statusFrameInfo').textContent = 
            `mage_${state.direction}_${state.frame}.png (160x160 RGBA · ${state.gender === 'male' ? 'Nam' : 'Nữ'})`;
    }

    function updateUI() {
        setColor(state.color);
        renderRecentColors();
        previewCanvas.style.transform = `scale(${state.previewScale})`;
    }

    let toastTimer = null;
    function showToast(msg, isError = false) {
        clearTimeout(toastTimer);
        toast.textContent = msg;
        toast.className = `toast ${isError ? 'error' : ''}`;
        toastTimer = setTimeout(() => {
            toast.className = 'toast hidden';
        }, 3200);
    }

    // Start on DOM ready
    window.addEventListener('DOMContentLoaded', init);
})();
