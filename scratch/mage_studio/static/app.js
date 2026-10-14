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
        zoom: 3.5,
        autoSave: true,
        isDrawing: false,
        lastX: null,
        lastY: null,
        rectStartX: null,
        rectStartY: null,
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

    // ================= CANVAS ZOOM & PAN =================
    function applyZoom() {
        const displayW = CANVAS_WIDTH * state.zoom;
        const displayH = CANVAS_HEIGHT * state.zoom;

        canvasContainer.style.width = `${displayW}px`;
        canvasContainer.style.height = `${displayH}px`;

        [paintCanvas, onionCanvas, overlayCanvas].forEach(c => {
            c.style.width = `${displayW}px`;
            c.style.height = `${displayH}px`;
        });

        document.getElementById('zoomLevelLabel').textContent = `${Math.round(state.zoom * 100)}%`;
        renderOverlay();
    }

    function zoomIn() {
        if (state.zoom < 14) {
            state.zoom = Math.min(14, state.zoom + 0.5);
            applyZoom();
        }
    }

    function zoomOut() {
        if (state.zoom > 1) {
            state.zoom = Math.max(1, state.zoom - 0.5);
            applyZoom();
        }
    }

    function zoomReset() {
        state.zoom = 3.5;
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
        overlayCtx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // 1. Pixel Grid
        if (state.showGrid && state.zoom >= 3.0) {
            overlayCtx.strokeStyle = 'rgba(255, 255, 255, 0.08)';
            overlayCtx.lineWidth = 0.5;
            overlayCtx.beginPath();
            for (let x = 0; x <= CANVAS_WIDTH; x += 4) {
                overlayCtx.moveTo(x, 0);
                overlayCtx.lineTo(x, CANVAS_HEIGHT);
            }
            for (let y = 0; y <= CANVAS_HEIGHT; y += 4) {
                overlayCtx.moveTo(0, y);
                overlayCtx.lineTo(CANVAS_WIDTH, y);
            }
            overlayCtx.stroke();
        }

        // 2. Vertical Symmetry Line (x = 80)
        if (state.symmetry) {
            overlayCtx.save();
            overlayCtx.strokeStyle = '#e5ab2e';
            overlayCtx.lineWidth = 1;
            overlayCtx.setLineDash([4, 4]);
            overlayCtx.beginPath();
            overlayCtx.moveTo(80, 0);
            overlayCtx.lineTo(80, CANVAS_HEIGHT);
            overlayCtx.stroke();
            overlayCtx.restore();
        }

        // 3. Brush Cursor Hover Box
        if (hoverX !== null && hoverY !== null) {
            const sz = state.brushSize;
            const startX = Math.floor(hoverX - sz / 2);
            const startY = Math.floor(hoverY - sz / 2);

            overlayCtx.strokeStyle = state.tool === 'eraser' ? '#ff4d4f' : '#38bdf8';
            overlayCtx.lineWidth = 1;
            overlayCtx.strokeRect(startX + 0.5, startY + 0.5, sz, sz);

            if (state.symmetry) {
                const symX = CANVAS_WIDTH - 1 - hoverX;
                const symStartX = Math.floor(symX - sz / 2);
                overlayCtx.strokeStyle = 'rgba(229, 171, 46, 0.8)';
                overlayCtx.strokeRect(symStartX + 0.5, startY + 0.5, sz, sz);
            }
        }
    }

    // ================= PIXEL DRAWING LOGIC =================
    function getCanvasCoordinates(e) {
        const rect = paintCanvas.getBoundingClientRect();
        const clientX = e.clientX || (e.touches && e.touches[0].clientX);
        const clientY = e.clientY || (e.touches && e.touches[0].clientY);

        const x = Math.floor(((clientX - rect.left) / rect.width) * CANVAS_WIDTH);
        const y = Math.floor(((clientY - rect.top) / rect.height) * CANVAS_HEIGHT);

        return {
            x: Math.max(0, Math.min(CANVAS_WIDTH - 1, x)),
            y: Math.max(0, Math.min(CANVAS_HEIGHT - 1, y))
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

        for (let dy = 0; dy < sz; dy++) {
            for (let dx = 0; dx < sz; dx++) {
                const px = x - half + dx;
                const py = y - half + dy;

                if (px >= 0 && px < CANVAS_WIDTH && py >= 0 && py < CANVAS_HEIGHT) {
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
            }
        }

        // Symmetry draw
        if (state.symmetry) {
            const symX = CANVAS_WIDTH - 1 - x;
            for (let dy = 0; dy < sz; dy++) {
                for (let dx = 0; dx < sz; dx++) {
                    const px = symX - half + dx;
                    const py = y - half + dy;
                    if (px >= 0 && px < CANVAS_WIDTH && py >= 0 && py < CANVAS_HEIGHT) {
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

    // Flood fill (Bucket tool)
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
            return Math.abs(data[i] - targetR) < 10 &&
                   Math.abs(data[i + 1] - targetG) < 10 &&
                   Math.abs(data[i + 2] - targetB) < 10 &&
                   Math.abs(data[i + 3] - targetA) < 10;
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

    // Flip Horizontal Frame
    function flipHorizontal() {
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

        // Update cache copy
        const copyCanvas = document.createElement('canvas');
        copyCanvas.width = CANVAS_WIDTH;
        copyCanvas.height = CANVAS_HEIGHT;
        copyCanvas.getContext('2d').drawImage(paintCanvas, 0, 0);
        state.frameImagesCache[key] = copyCanvas;

        // Update active thumbnail
        const thumbImg = document.getElementById(`thumbImg_${state.frame}`);
        if (thumbImg) {
            thumbImg.src = paintCanvas.toDataURL('image/png');
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
        state.frame = f;

        document.querySelectorAll('.frame-card').forEach(c => {
            c.classList.toggle('active', parseInt(c.dataset.frame) === f);
        });

        loadCurrentFrameToCanvas();
    }

    // ================= DIRECTION & GENDER SWITCHING =================
    async function setDirection(dir) {
        if (state.direction === dir) return;
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
        state.tool = toolName;
        document.querySelectorAll('.tool-btn').forEach(b => {
            b.classList.toggle('active', b.dataset.tool === toolName);
        });
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
            imgToDraw = paintCanvas;
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

        requestAnimationFrame(previewAnimationLoop);
    }

    // ================= EVENT BINDINGS =================
    function bindEvents() {
        // Pointer drawing on Paint Canvas
        paintCanvas.addEventListener('mousedown', (e) => {
            state.isDrawing = true;
            saveHistoryState();
            const coords = getCanvasCoordinates(e);

            if (state.tool === 'bucket') {
                floodFill(coords.x, coords.y, hexToRgba(state.color));
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
            const coords = getCanvasCoordinates(e);
            updateCoordinates(coords.x, coords.y);
            renderOverlay(coords.x, coords.y);

            if (!state.isDrawing) return;

            if (state.tool === 'rect') {
                // Preview rect outline on overlay
                renderOverlay(coords.x, coords.y);
                const rx = Math.min(state.rectStartX, coords.x);
                const ry = Math.min(state.rectStartY, coords.y);
                const rw = Math.abs(coords.x - state.rectStartX) + 1;
                const rh = Math.abs(coords.y - state.rectStartY) + 1;
                overlayCtx.strokeStyle = '#38bdf8';
                overlayCtx.strokeRect(rx, ry, rw, rh);
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

        // Grid & Onion Skin
        document.getElementById('chkGrid').addEventListener('change', (e) => {
            state.showGrid = e.target.checked;
            renderOverlay();
        });
        document.getElementById('chkOnionSkin').addEventListener('change', (e) => {
            state.onionSkin = e.target.checked;
            renderOnionSkin();
        });

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

        // Keyboard Shortcuts
        window.addEventListener('keydown', (e) => {
            // Check if user is typing in an input
            if (e.target.tagName === 'INPUT') return;

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

            // Direction Keys (WASD / Arrows)
            if (e.key === 'ArrowDown' || e.key.toLowerCase() === 's' && !e.ctrlKey) {
                setDirection('south');
            } else if (e.key === 'ArrowUp' || e.key.toLowerCase() === 'w') {
                setDirection('north');
            } else if (e.key === 'ArrowLeft' || e.key.toLowerCase() === 'a') {
                setDirection('west');
            } else if (e.key === 'ArrowRight' || e.key.toLowerCase() === 'd') {
                setDirection('east');
            }

            // 1..8 number keys for 8 directions
            const num = parseInt(e.key);
            if (num >= 1 && num <= 8) {
                setDirection(DIRECTIONS[num - 1].id);
            }

            // Tool shortcuts
            if (e.key.toLowerCase() === 'p') setTool('pencil');
            if (e.key.toLowerCase() === 'e') setTool('eraser');
            if (e.key.toLowerCase() === 'b') setTool('bucket');
            if (e.key.toLowerCase() === 'i') setTool('eyedropper');
            if (e.key.toLowerCase() === 'r') setTool('rect');
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
