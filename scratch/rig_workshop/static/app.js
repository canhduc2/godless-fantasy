// ============================================================
// CHIBI RIG WORKSHOP & PIXEL STUDIO • COMPLETE CLIENT ENGINE
// ============================================================

const PART_NAMES_VI = {
    "idle_head": "Đầu (Head)",
    "idle_body": "Thân / Áo (Body)",
    "idle_arm_left": "Tay Trái (Arm Left)",
    "idle_arm_right": "Tay Phải (Arm Right)",
    "idle_leg_left": "Chân Trái (Leg Left)",
    "idle_leg_right": "Chân Phải (Leg Right)"
};

const DIR_LABELS_VI = {
    "south": "South (Trước - Front)",
    "south_east": "South-East (Chếch Trước Phải)",
    "east": "East (Phải - Right)",
    "north_east": "North-East (Chếch Sau Phải)",
    "north": "North (Sau - Back)",
    "north_west": "North-West (Chếch Sau Trái)",
    "west": "West (Trái - Left)",
    "south_west": "South-West (Chếch Trước Trái)"
};

const GALLERY_DIRS = ['south', 'south_east', 'east', 'north_east', 'north', 'north_west', 'west', 'south_west'];
const PART_KEYS = ['idle_head', 'idle_body', 'idle_arm_left', 'idle_arm_right', 'idle_leg_left', 'idle_leg_right'];

const CHIBI_PALETTE = [
    '#ffdfd2', '#fed8c8', '#f6beaa', '#e29b85', '#b8745f', '#8d4a36',
    '#fa9166', '#f27b50', '#d45934', '#9b371b', '#6d210b', '#421104',
    '#686065', '#524c50', '#443e41', '#302b2d', '#201c1e', '#121011',
    '#b88262', '#9c694a', '#805036', '#653d27', '#4a2c1b', '#2f1a0e',
    '#86efac', '#4ade80', '#16a34a', '#166534', '#ffffff', '#000000'
];

class RigWorkshopApp {
    constructor() {
        this.currentTab = 'assembler';
        this.config = null;
        this.currentGender = 'female';
        this.currentDir = 'south';
        this._selectedPartKeys = new Set(['idle_body']);
        this.zoom = 8;
        this.showGrid = true;
        this.showBbox = true;
        this.availableParts = [];
        this.imageCache = new Map();

        // Assembler Canvas & Overlays
        this.canvas = document.getElementById('main-canvas');
        this.ctx = this.canvas.getContext('2d');
        this.ctx.imageSmoothingEnabled = false;
        this.wrapper = document.getElementById('canvas-wrapper');
        this.selectionBox = document.getElementById('selection-box');
        this.selectionBoxesContainer = document.getElementById('selection-boxes-container');
        this.marqueeBox = document.getElementById('marquee-box');
        this.guideCanvas = document.getElementById('guide-canvas');
        this.guideCtx = this.guideCanvas ? this.guideCanvas.getContext('2d') : null;
        if (this.guideCtx) this.guideCtx.imageSmoothingEnabled = false;
        this.contextMenu = document.getElementById('context-menu');

        // Snap & Guides
        this.snapEnabled = true;
        this.activeGuides = [];

        // Assembler History (Undo / Redo)
        this.assemblerHistory = [];
        this.assemblerRedoStack = [];

        // Dragging, Panning & Marquee selection state
        this.toolMode = 'select'; // 'select' or 'pan'
        this.panX = 0;
        this.panY = 0;
        this.isDragging = false;
        this.isPanning = false;
        this.isMarqueeSelecting = false;
        this.isSpacePressed = false;
        this.panStartX = 0;
        this.panStartY = 0;
        this.panStartOffsetX = 0;
        this.panStartOffsetY = 0;
        this.dragStartX = 0;
        this.dragStartY = 0;
        this.dragStartPositions = new Map(); // key -> { x, y }
        this.marqueeStartX = 0;
        this.marqueeStartY = 0;
        this.marqueeInitialSelection = new Set();

        // Pixel Editor State
        this.peCurrentPartName = '';
        this.peCanvas = document.getElementById('pe-canvas');
        this.peCtx = this.peCanvas.getContext('2d');
        this.peCtx.imageSmoothingEnabled = false;
        this.peCursorCanvas = document.getElementById('pe-cursor-canvas');
        this.peCursorCtx = this.peCursorCanvas.getContext('2d');
        this.peZoom = 20;
        this.peTool = 'pencil';
        this.peBrushSize = 1;
        this.peColor = '#f27b50';
        this.peHistory = [];
        this.peRedoStack = [];
        this.peIsDrawing = false;
        this.peLastDrawX = -1;
        this.peLastDrawY = -1;

        this.init();
    }

    get selectedPartKeys() {
        return this._selectedPartKeys;
    }
    set selectedPartKeys(val) {
        this._selectedPartKeys = new Set(val);
    }

    get selectedPartKey() {
        const it = this._selectedPartKeys.values().next();
        return it.done ? null : it.value;
    }
    set selectedPartKey(key) {
        this._selectedPartKeys.clear();
        if (key) this._selectedPartKeys.add(key);
    }

    // ============================================================
    // SNAP GUIDES & SMART ALIGNMENT
    // ============================================================
    drawSnapGuides() {
        if (!this.guideCanvas || !this.guideCtx) return;
        this.guideCtx.clearRect(0, 0, 64, 64);
        if (!this.snapEnabled || !this.activeGuides || this.activeGuides.length === 0) return;

        this.guideCtx.save();
        for (const g of this.activeGuides) {
            if (g.type === 'vert') {
                this.guideCtx.strokeStyle = '#f43f5e'; // Neon pink vertical center line
                this.guideCtx.lineWidth = 1;
                this.guideCtx.setLineDash([2, 2]);
                this.guideCtx.beginPath();
                this.guideCtx.moveTo(g.pos + 0.5, 0);
                this.guideCtx.lineTo(g.pos + 0.5, 64);
                this.guideCtx.stroke();
            } else if (g.type === 'horiz') {
                this.guideCtx.strokeStyle = '#06b6d4'; // Neon cyan horizontal center line
                this.guideCtx.lineWidth = 1;
                this.guideCtx.setLineDash([2, 2]);
                this.guideCtx.beginPath();
                this.guideCtx.moveTo(0, g.pos + 0.5);
                this.guideCtx.lineTo(64, g.pos + 0.5);
                this.guideCtx.stroke();
            }
        }
        this.guideCtx.restore();
    }

    clearSnapGuides() {
        this.activeGuides = [];
        if (this.guideCtx) {
            this.guideCtx.clearRect(0, 0, 64, 64);
        }
    }

    // ============================================================
    // ARRANGE & ALIGN ENGINE
    // ============================================================
    alignSelected(alignment) {
        const dirData = this.getCurrentDirData();
        if (!dirData || this.selectedPartKeys.size === 0) return;
        this.pushAssemblerHistory();

        const parts = Array.from(this.selectedPartKeys).map(k => {
            const p = dirData[k];
            const img = this.getImage(p.file);
            return {
                key: k,
                part: p,
                w: img && img.complete ? img.naturalWidth : 16,
                h: img && img.complete ? img.naturalHeight : 16
            };
        });

        const minX = Math.min(...parts.map(p => p.part.x));
        const maxX = Math.max(...parts.map(p => p.part.x + p.w));
        const minY = Math.min(...parts.map(p => p.part.y));
        const maxY = Math.max(...parts.map(p => p.part.y + p.h));
        const groupW = maxX - minX;
        const groupH = maxY - minY;

        switch (alignment) {
            case 'left':
                if (parts.length > 1) {
                    parts.forEach(p => p.part.x = minX);
                } else {
                    parts[0].part.x = 0;
                }
                this.showToast('⇤ Đã căn mép Trái!');
                break;
            case 'center_x':
                if (parts.length > 1) {
                    const targetLeft = Math.round(32 - groupW / 2);
                    const diff = targetLeft - minX;
                    parts.forEach(p => p.part.x += diff);
                } else {
                    parts[0].part.x = Math.round(32 - parts[0].w / 2);
                }
                this.showToast('⇋ Đã căn giữa theo trục X = 32!');
                break;
            case 'right':
                if (parts.length > 1) {
                    parts.forEach(p => p.part.x = maxX - p.w);
                } else {
                    parts[0].part.x = 64 - parts[0].w;
                }
                this.showToast('⇥ Đã căn mép Phải!');
                break;
            case 'top':
                if (parts.length > 1) {
                    parts.forEach(p => p.part.y = minY);
                } else {
                    parts[0].part.y = 0;
                }
                this.showToast('⤒ Đã căn mép Trên!');
                break;
            case 'center_y':
                if (parts.length > 1) {
                    const targetTop = Math.round(32 - groupH / 2);
                    const diff = targetTop - minY;
                    parts.forEach(p => p.part.y += diff);
                } else {
                    parts[0].part.y = Math.round(32 - parts[0].h / 2);
                }
                this.showToast('⇅ Đã căn giữa theo trục Y = 32!');
                break;
            case 'bottom':
                if (parts.length > 1) {
                    parts.forEach(p => p.part.y = maxY - p.h);
                } else {
                    parts[0].part.y = 64 - parts[0].h;
                }
                this.showToast('⤓ Đã căn mép Dưới!');
                break;
        }

        this.renderAll();
    }

    distributeSelected(axis) {
        const dirData = this.getCurrentDirData();
        if (!dirData || this.selectedPartKeys.size < 3) {
            this.showToast('ℹ️ Cần chọn ít nhất 3 bộ phận để dãn đều khoảng cách!', '#3b82f6');
            return;
        }
        this.pushAssemblerHistory();

        const parts = Array.from(this.selectedPartKeys).map(k => {
            const p = dirData[k];
            const img = this.getImage(p.file);
            return {
                key: k,
                part: p,
                w: img && img.complete ? img.naturalWidth : 16,
                h: img && img.complete ? img.naturalHeight : 16
            };
        });

        if (axis === 'horizontal') {
            parts.sort((a, b) => a.part.x - b.part.x);
            const first = parts[0];
            const last = parts[parts.length - 1];
            const totalWidthOfParts = parts.reduce((sum, p) => sum + p.w, 0);
            const span = (last.part.x + last.w) - first.part.x;
            const availableSpace = span - totalWidthOfParts;
            const gap = availableSpace / (parts.length - 1);

            let currentX = first.part.x;
            for (let i = 0; i < parts.length; i++) {
                parts[i].part.x = Math.round(currentX);
                currentX += parts[i].w + gap;
            }
            this.showToast('↔️ Đã dãn đều khoảng cách ngang!');
        } else if (axis === 'vertical') {
            parts.sort((a, b) => a.part.y - b.part.y);
            const first = parts[0];
            const last = parts[parts.length - 1];
            const totalHeightOfParts = parts.reduce((sum, p) => sum + p.h, 0);
            const span = (last.part.y + last.h) - first.part.y;
            const availableSpace = span - totalHeightOfParts;
            const gap = availableSpace / (parts.length - 1);

            let currentY = first.part.y;
            for (let i = 0; i < parts.length; i++) {
                parts[i].part.y = Math.round(currentY);
                currentY += parts[i].h + gap;
            }
            this.showToast('↕️ Đã dãn đều khoảng cách dọc!');
        }

        this.renderAll();
    }

    bringSelectedToFront() {
        const dirData = this.getCurrentDirData();
        if (!dirData || this.selectedPartKeys.size === 0) return;
        this.pushAssemblerHistory();

        let maxZ = 0;
        for (const k of PART_KEYS) {
            if (dirData[k]) maxZ = Math.max(maxZ, dirData[k].z || 10);
        }

        let offset = 1;
        for (const k of this.selectedPartKeys) {
            if (dirData[k]) {
                dirData[k].z = maxZ + offset;
                offset++;
            }
        }
        this.renderAll();
        this.showToast('⏫ Đã đưa nhóm bộ phận lên đỉnh ngoài cùng!');
    }

    sendSelectedToBack() {
        const dirData = this.getCurrentDirData();
        if (!dirData || this.selectedPartKeys.size === 0) return;
        this.pushAssemblerHistory();

        let minZ = 100;
        for (const k of PART_KEYS) {
            if (dirData[k]) minZ = Math.min(minZ, dirData[k].z || 10);
        }

        const baseZ = Math.max(0, minZ - this.selectedPartKeys.size);
        let offset = 0;
        for (const k of this.selectedPartKeys) {
            if (dirData[k]) {
                dirData[k].z = Math.max(0, baseZ + offset);
                offset++;
            }
        }
        this.renderAll();
        this.showToast('⏬ Đã đưa nhóm bộ phận xuống đáy trong cùng!');
    }

    // ============================================================
    // UNDO & REDO ENGINE FOR ASSEMBLER
    // ============================================================
    pushAssemblerHistory() {
        if (!this.config) return;
        const snapshot = JSON.stringify(this.config);
        if (this.assemblerHistory.length > 0 && this.assemblerHistory[this.assemblerHistory.length - 1] === snapshot) {
            return;
        }
        this.assemblerHistory.push(snapshot);
        if (this.assemblerHistory.length > 50) this.assemblerHistory.shift();
        this.assemblerRedoStack = [];
    }

    undoAssembler() {
        if (this.assemblerHistory.length > 0) {
            const current = JSON.stringify(this.config);
            this.assemblerRedoStack.push(current);
            const prev = this.assemblerHistory.pop();
            this.config = JSON.parse(prev);
            this.renderAll();
            this.showToast('↩️ Hoàn tác thao tác (Undo)', '#4b5563');
        } else {
            this.showToast('ℹ️ Không còn thao tác nào để hoàn tác', '#3b82f6');
        }
    }

    redoAssembler() {
        if (this.assemblerRedoStack.length > 0) {
            const current = JSON.stringify(this.config);
            this.assemblerHistory.push(current);
            const next = this.assemblerRedoStack.pop();
            this.config = JSON.parse(next);
            this.renderAll();
            this.showToast('↪️ Làm lại thao tác (Redo)', '#4b5563');
        } else {
            this.showToast('ℹ️ Không có thao tác nào để làm lại', '#3b82f6');
        }
    }

    // ============================================================
    // TOOL MODES & VIEWPORT PANNING
    // ============================================================
    setToolMode(mode) {
        this.toolMode = mode;
        const btnSelect = document.getElementById('btn-mode-select');
        const btnPan = document.getElementById('btn-mode-pan');
        if (btnSelect) btnSelect.classList.toggle('active', mode === 'select');
        if (btnPan) btnPan.classList.toggle('active', mode === 'pan');
        this.updateCursorMode();
    }

    updateCursorMode() {
        if (this.toolMode === 'pan' || this.isSpacePressed) {
            this.canvas.style.cursor = this.isPanning ? 'grabbing' : 'grab';
            this.wrapper.style.cursor = this.isPanning ? 'grabbing' : 'grab';
        } else {
            this.canvas.style.cursor = 'crosshair';
            this.wrapper.style.cursor = 'default';
        }
    }

    updatePanTransform() {
        this.wrapper.style.transform = `translate(${this.panX}px, ${this.panY}px)`;
    }

    resetPan() {
        this.panX = 0;
        this.panY = 0;
        this.updatePanTransform();
        this.showToast('🎯 Đã căn giữa lại góc nhìn canvas!');
    }

    async init() {
        this.setupEventListeners();
        this.setupPixelEditor();
        await this.loadPartsList();
        await this.loadConfig();
        this.updateZoom();
        this.renderAll();
    }

    // ============================================================
    // DATA & ASSETS
    // ============================================================
    async loadPartsList() {
        try {
            const res = await fetch('/api/parts_list');
            this.availableParts = await res.json();
            for (const p of this.availableParts) {
                this.getImage(p.file);
            }
        } catch (e) {
            console.error('Failed to load parts list:', e);
        }
    }

    async loadConfig() {
        try {
            const res = await fetch('/api/config');
            this.config = await res.json();
        } catch (e) {
            console.error('Failed to load config:', e);
        }
    }

    getImage(fileName, forceReload = false) {
        if (!fileName) return null;
        if (this.imageCache.has(fileName) && !forceReload) {
            return this.imageCache.get(fileName);
        }
        const img = new Image();
        img.src = `/parts/${fileName}.png?t=${Date.now()}`;
        img.onload = () => {
            if (this.currentTab === 'assembler') {
                this.renderMainCanvas();
                this.renderGallery();
            }
        };
        this.imageCache.set(fileName, img);
        return img;
    }

    invalidatePartCache(fileName) {
        this.imageCache.delete(fileName);
        this.getImage(fileName, true);
    }

    getCurrentDirData() {
        if (!this.config) return null;
        if (!this.config[this.currentGender]) this.config[this.currentGender] = {};
        if (!this.config[this.currentGender][this.currentDir]) {
            this.config[this.currentGender][this.currentDir] = {};
        }
        return this.config[this.currentGender][this.currentDir];
    }

    // ============================================================
    // WORKSPACE TABS SWITCHING
    // ============================================================
    switchTab(tabName) {
        this.currentTab = tabName;
        document.querySelectorAll('.tab-view').forEach(v => v.classList.remove('active'));
        document.querySelectorAll('.ws-tab-btn').forEach(b => b.classList.remove('active'));

        if (tabName === 'assembler') {
            document.getElementById('view-assembler').classList.add('active');
            document.getElementById('tab-btn-assembler').classList.add('active');
            document.getElementById('header-gender-selector').style.display = 'flex';
            this.renderAll();
        } else if (tabName === 'pixel') {
            document.getElementById('view-pixel-editor').classList.add('active');
            document.getElementById('tab-btn-pixel').classList.add('active');
            document.getElementById('header-gender-selector').style.display = 'none';
            this.refreshPixelEditorCanvas();
        }
    }

    // ============================================================
    // ASSEMBLER RENDERING
    // ============================================================
    renderAll() {
        this.updateHeaderBadges();
        this.renderMainCanvas();
        this.renderLayersPanel();
        this.renderInspector();
        this.renderGallery();
    }

    renderMainCanvas() {
        const dirData = this.getCurrentDirData();
        if (!dirData) return;

        this.ctx.clearRect(0, 0, 64, 64);

        const parts = PART_KEYS.map(key => ({
            key,
            ...dirData[key]
        })).filter(p => p && p.visible !== false);

        parts.sort((a, b) => (a.z || 10) - (b.z || 10));

        for (const part of parts) {
            const img = this.getImage(part.file);
            if (!img || !img.complete || img.naturalWidth === 0) continue;

            const px = Math.round(part.x || 0);
            const py = Math.round(part.y || 0);
            const pw = img.naturalWidth;
            const ph = img.naturalHeight;

            if (part.flip_x || part.flip_y) {
                this.ctx.save();
                this.ctx.translate(px + (part.flip_x ? pw : 0), py + (part.flip_y ? ph : 0));
                this.ctx.scale(part.flip_x ? -1 : 1, part.flip_y ? -1 : 1);
                this.ctx.drawImage(img, 0, 0);
                this.ctx.restore();
            } else {
                this.ctx.drawImage(img, px, py);
            }
        }

        this.updateSelectionBox();
    }

    updateSelectionBox() {
        if (this.selectionBoxesContainer) {
            this.selectionBoxesContainer.innerHTML = '';
        }

        const coordInd = document.getElementById('coord-indicator');
        const badgePart = document.getElementById('badge-selected-part');
        const arrangeHint = document.getElementById('arrange-count-hint');

        if (!this.showBbox || this.selectedPartKeys.size === 0) {
            this.selectionBox.style.display = 'none';
            if (badgePart) badgePart.innerText = 'Chưa chọn bộ phận';
            if (coordInd) coordInd.innerText = 'Chưa chọn bộ phận nào (Click chuột hoặc quét chọn)';
            if (arrangeHint) arrangeHint.innerText = 'Chưa chọn';
            return;
        }

        const dirData = this.getCurrentDirData();
        if (!dirData) return;

        if (this.selectedPartKeys.size === 1) {
            const key = this.selectedPartKey;
            const part = dirData[key];
            if (!part || !part.visible) {
                this.selectionBox.style.display = 'none';
                return;
            }

            const img = this.getImage(part.file);
            const pw = img && img.complete ? img.naturalWidth : 16;
            const ph = img && img.complete ? img.naturalHeight : 16;

            this.selectionBox.style.display = 'block';
            this.selectionBox.style.left = `${part.x * this.zoom}px`;
            this.selectionBox.style.top = `${part.y * this.zoom}px`;
            this.selectionBox.style.width = `${pw * this.zoom}px`;
            this.selectionBox.style.height = `${ph * this.zoom}px`;

            if (coordInd) coordInd.innerText = `Toạ độ: X: ${part.x}, Y: ${part.y} | Z-Order: ${part.z} | Kích thước: ${pw}x${ph}`;
            if (badgePart) badgePart.innerText = PART_NAMES_VI[key] || key;
            if (arrangeHint) arrangeHint.innerText = 'Đang chọn: 1 mảnh';
        } else {
            // Multiple parts selected
            let minX = Infinity, maxX = -Infinity;
            let minY = Infinity, maxY = -Infinity;

            for (const key of this.selectedPartKeys) {
                const part = dirData[key];
                if (!part || !part.visible) continue;
                const img = this.getImage(part.file);
                const pw = img && img.complete ? img.naturalWidth : 16;
                const ph = img && img.complete ? img.naturalHeight : 16;

                minX = Math.min(minX, part.x);
                maxX = Math.max(maxX, part.x + pw);
                minY = Math.min(minY, part.y);
                maxY = Math.max(maxY, part.y + ph);

                // Add individual item box
                if (this.selectionBoxesContainer) {
                    const itemBox = document.createElement('div');
                    itemBox.className = 'multi-selection-item';
                    itemBox.style.left = `${part.x * this.zoom}px`;
                    itemBox.style.top = `${part.y * this.zoom}px`;
                    itemBox.style.width = `${pw * this.zoom}px`;
                    itemBox.style.height = `${ph * this.zoom}px`;
                    this.selectionBoxesContainer.appendChild(itemBox);
                }
            }

            if (minX !== Infinity) {
                this.selectionBox.style.display = 'block';
                this.selectionBox.style.left = `${minX * this.zoom}px`;
                this.selectionBox.style.top = `${minY * this.zoom}px`;
                this.selectionBox.style.width = `${(maxX - minX) * this.zoom}px`;
                this.selectionBox.style.height = `${(maxY - minY) * this.zoom}px`;

                if (coordInd) {
                    coordInd.innerText = `Đang chọn ${this.selectedPartKeys.size} bộ phận | Khung nhóm: X: ${minX}, Y: ${minY} (${maxX - minX}x${maxY - minY} px)`;
                }
                if (badgePart) badgePart.innerText = `Nhóm ${this.selectedPartKeys.size} bộ phận`;
                if (arrangeHint) arrangeHint.innerText = `Đang chọn: ${this.selectedPartKeys.size} mảnh`;
            } else {
                this.selectionBox.style.display = 'none';
            }
        }
    }

    renderGallery() {
        const strip = document.getElementById('gallery-strip');
        if (!strip || !this.config) return;

        strip.innerHTML = '';
        const gData = this.config[this.currentGender] || {};

        for (const dir of GALLERY_DIRS) {
            const item = document.createElement('div');
            item.className = `gallery-item ${dir === this.currentDir ? 'active' : ''}`;
            item.title = DIR_LABELS_VI[dir] || dir;

            const wrap = document.createElement('div');
            wrap.className = 'gallery-canvas-wrap';

            const c = document.createElement('canvas');
            c.width = 64;
            c.height = 64;
            const ctx = c.getContext('2d');
            ctx.imageSmoothingEnabled = false;

            const dData = gData[dir] || {};
            const parts = PART_KEYS.map(key => ({ key, ...dData[key] })).filter(p => p && p.visible !== false);
            parts.sort((a, b) => (a.z || 10) - (b.z || 10));

            for (const part of parts) {
                const img = this.getImage(part.file);
                if (!img || !img.complete || img.naturalWidth === 0) continue;
                const px = Math.round(part.x || 0);
                const py = Math.round(part.y || 0);
                const pw = img.naturalWidth;
                const ph = img.naturalHeight;
                if (part.flip_x || part.flip_y) {
                    ctx.save();
                    ctx.translate(px + (part.flip_x ? pw : 0), py + (part.flip_y ? ph : 0));
                    ctx.scale(part.flip_x ? -1 : 1, part.flip_y ? -1 : 1);
                    ctx.drawImage(img, 0, 0);
                    ctx.restore();
                } else {
                    ctx.drawImage(img, px, py);
                }
            }

            wrap.appendChild(c);
            item.appendChild(wrap);

            const label = document.createElement('span');
            label.className = 'gallery-label';
            label.innerText = dir.toUpperCase().replace('_', ' ');
            item.appendChild(label);

            item.addEventListener('click', () => this.setDirection(dir));
            strip.appendChild(item);
        }
    }

    renderLayersPanel() {
        const list = document.getElementById('layers-list');
        if (!list) return;

        const dirData = this.getCurrentDirData();
        if (!dirData) { list.innerHTML = ''; return; }

        const parts = PART_KEYS.map(key => ({
            key,
            ...dirData[key]
        })).sort((a, b) => (b.z || 10) - (a.z || 10));

        list.innerHTML = '';

        parts.forEach((part) => {
            const item = document.createElement('div');
            item.className = `layer-item ${this.selectedPartKeys.has(part.key) ? 'active' : ''}`;

            const thumbWrap = document.createElement('div');
            thumbWrap.className = 'layer-thumb-wrap';
            const thumb = document.createElement('img');
            thumb.src = `/parts/${part.file}.png`;
            thumbWrap.appendChild(thumb);

            const info = document.createElement('div');
            info.className = 'layer-info';
            info.innerHTML = `
                <div class="layer-name">${PART_NAMES_VI[part.key] || part.key}</div>
                <div class="layer-coords">X: ${part.x}, Y: ${part.y}</div>
            `;

            const controls = document.createElement('div');
            controls.className = 'layer-controls';

            const zBadge = document.createElement('span');
            zBadge.className = 'z-badge';
            zBadge.innerText = `Z: ${part.z}`;
            controls.appendChild(zBadge);

            // Button to open pixel editor for this part
            const btnEdit = document.createElement('button');
            btnEdit.className = 'icon-btn icon-btn-edit';
            btnEdit.title = 'Vẽ sửa pixel mảnh này';
            btnEdit.innerText = '🎨';
            btnEdit.addEventListener('click', (e) => {
                e.stopPropagation();
                this.selectPart(part.key);
                this.openPixelEditorForPart(part.file);
            });
            controls.appendChild(btnEdit);

            const btnUp = document.createElement('button');
            btnUp.className = 'icon-btn';
            btnUp.title = 'Lên trên (+1 Z)';
            btnUp.innerText = '▲';
            btnUp.addEventListener('click', (e) => {
                e.stopPropagation();
                this.pushAssemblerHistory();
                part.z = (part.z || 10) + 1;
                dirData[part.key].z = part.z;
                this.renderAll();
            });
            controls.appendChild(btnUp);

            const btnDown = document.createElement('button');
            btnDown.className = 'icon-btn';
            btnDown.title = 'Xuống dưới (-1 Z)';
            btnDown.innerText = '▼';
            btnDown.addEventListener('click', (e) => {
                e.stopPropagation();
                this.pushAssemblerHistory();
                part.z = Math.max(0, (part.z || 10) - 1);
                dirData[part.key].z = part.z;
                this.renderAll();
            });
            controls.appendChild(btnDown);

            const btnVis = document.createElement('button');
            btnVis.className = 'icon-btn';
            btnVis.title = part.visible ? 'Ẩn' : 'Hiện';
            btnVis.innerText = part.visible ? '👁️' : '🚫';
            btnVis.addEventListener('click', (e) => {
                e.stopPropagation();
                this.pushAssemblerHistory();
                part.visible = !part.visible;
                dirData[part.key].visible = part.visible;
                this.renderAll();
            });
            controls.appendChild(btnVis);

            item.appendChild(thumbWrap);
            item.appendChild(info);
            item.appendChild(controls);

            item.addEventListener('click', (e) => {
                if (e.ctrlKey || e.metaKey || e.shiftKey) {
                    if (this.selectedPartKeys.has(part.key)) {
                        this.selectedPartKeys.delete(part.key);
                    } else {
                        this.selectedPartKeys.add(part.key);
                    }
                    this.renderAll();
                } else {
                    this.selectPart(part.key);
                }
            });

            item.addEventListener('contextmenu', (e) => {
                e.preventDefault();
                if (!this.selectedPartKeys.has(part.key)) {
                    this.selectPart(part.key);
                }
                this.showContextMenu(e.clientX, e.clientY);
            });

            list.appendChild(item);
        });
    }

    renderInspector() {
        const body = document.getElementById('inspector-body');
        if (!body) return;

        const dirData = this.getCurrentDirData();
        if (!dirData || this.selectedPartKeys.size === 0) {
            body.innerHTML = `<p class="empty-hint">Chưa chọn bộ phận nào (Click chuột hoặc quét vùng để chọn)</p>`;
            return;
        }

        // Multi-selection Inspector Card
        if (this.selectedPartKeys.size > 1) {
            body.innerHTML = `
                <div class="batch-inspector-card">
                    <div class="batch-title">📦 Đang chọn nhóm (${this.selectedPartKeys.size} bộ phận)</div>
                    <div class="batch-part-chips">
                        ${Array.from(this.selectedPartKeys).map(k => `<span class="part-chip">${PART_NAMES_VI[k] || k}</span>`).join('')}
                    </div>
                    <div style="display:flex; gap:6px; margin-bottom:8px;">
                        <button class="btn btn-secondary" id="btn-batch-flip-x" style="flex:1; justify-content:center;">↔️ Lật ngang nhóm</button>
                        <button class="btn btn-secondary" id="btn-batch-flip-y" style="flex:1; justify-content:center;">↕️ Lật dọc nhóm</button>
                    </div>
                    <div style="display:flex; gap:6px; margin-bottom:8px;">
                        <button class="btn btn-secondary" id="btn-batch-vis" style="flex:1; justify-content:center;">👁️ Ẩn/Hiện nhóm</button>
                        <button class="btn btn-secondary" id="btn-batch-deselect" style="flex:1; justify-content:center;">❌ Bỏ chọn</button>
                    </div>
                    <div style="display:flex; gap:6px;">
                        <button class="btn btn-secondary" id="btn-batch-z-up" style="flex:1; justify-content:center;">▲ Tăng Z (+1)</button>
                        <button class="btn btn-secondary" id="btn-batch-z-down" style="flex:1; justify-content:center;">▼ Giảm Z (-1)</button>
                    </div>
                </div>
            `;

            document.getElementById('btn-batch-flip-x').addEventListener('click', () => {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].flip_x = !dirData[k].flip_x;
                }
                this.renderAll();
                this.showToast(`↔️ Đã lật ngang ${this.selectedPartKeys.size} bộ phận!`);
            });

            document.getElementById('btn-batch-flip-y').addEventListener('click', () => {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].flip_y = !dirData[k].flip_y;
                }
                this.renderAll();
                this.showToast(`↕️ Đã lật dọc ${this.selectedPartKeys.size} bộ phận!`);
            });

            document.getElementById('btn-batch-vis').addEventListener('click', () => {
                this.pushAssemblerHistory();
                const firstKey = this.selectedPartKey;
                const newVis = !dirData[firstKey].visible;
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].visible = newVis;
                }
                this.renderAll();
                this.showToast(`👁️ Đã ${newVis ? 'hiện' : 'ẩn'} nhóm ${this.selectedPartKeys.size} bộ phận!`);
            });

            document.getElementById('btn-batch-deselect').addEventListener('click', () => {
                this.selectedPartKeys.clear();
                this.renderAll();
            });

            document.getElementById('btn-batch-z-up').addEventListener('click', () => {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].z = (dirData[k].z || 10) + 1;
                }
                this.renderAll();
                this.showToast('▲ Đã tăng +1 Z cho cả nhóm!');
            });

            document.getElementById('btn-batch-z-down').addEventListener('click', () => {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].z = Math.max(0, (dirData[k].z || 10) - 1);
                }
                this.renderAll();
                this.showToast('▼ Đã giảm -1 Z cho cả nhóm!');
            });
            return;
        }

        const part = dirData[this.selectedPartKey];
        if (!part) {
            body.innerHTML = `<p class="empty-hint">Chọn 1 bộ phận để chỉnh sửa thông số chi tiết</p>`;
            return;
        }

        const partTitle = PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey;

        body.innerHTML = `
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom: 8px;">
                <span style="font-size: 13px; font-weight: 800; color: #fff;">${partTitle}</span>
                <button class="btn btn-secondary" id="btn-inspect-edit-part" style="padding: 4px 8px; font-size: 11px;">
                    🎨 Vẽ sửa Pixel
                </button>
            </div>
            <div class="prop-grid">
                <div class="prop-field">
                    <label>Toạ độ X (Pixel)</label>
                    <div class="number-stepper">
                        <button class="stepper-btn" id="btn-x-minus">-</button>
                        <input type="number" id="input-part-x" value="${part.x}">
                        <button class="stepper-btn" id="btn-x-plus">+</button>
                    </div>
                </div>
                <div class="prop-field">
                    <label>Toạ độ Y (Pixel)</label>
                    <div class="number-stepper">
                        <button class="stepper-btn" id="btn-y-minus">-</button>
                        <input type="number" id="input-part-y" value="${part.y}">
                        <button class="stepper-btn" id="btn-y-plus">+</button>
                    </div>
                </div>
                <div class="prop-field">
                    <label>Thứ tự lớp (Z-Order)</label>
                    <div class="number-stepper">
                        <button class="stepper-btn" id="btn-z-minus">-</button>
                        <input type="number" id="input-part-z" value="${part.z}">
                        <button class="stepper-btn" id="btn-z-plus">+</button>
                    </div>
                </div>
                <div class="prop-field">
                    <label>Lật ngang (Flip X)</label>
                    <button class="btn ${part.flip_x ? 'btn-primary' : 'btn-secondary'}" id="btn-flip-x" style="width:100%; justify-content:center;">
                        ${part.flip_x ? '↔️ Đang lật ngang' : '↔️ Bình thường'}
                    </button>
                </div>
                <div class="prop-field">
                    <label>Lật dọc (Flip Y)</label>
                    <button class="btn ${part.flip_y ? 'btn-primary' : 'btn-secondary'}" id="btn-flip-y" style="width:100%; justify-content:center;">
                        ${part.flip_y ? '↕️ Đang lật dọc' : '↕️ Bình thường'}
                    </button>
                </div>
            </div>

            <div class="prop-field" style="margin-top: 10px;">
                <label>File ảnh bộ phận</label>
                <select id="select-part-file" style="width: 100%; background: var(--bg-card); color: var(--text-main); border: 1px solid var(--border); padding: 7px; border-radius: 6px; font-size: 11px;">
                    ${this.availableParts.map(p => `
                        <option value="${p.file}" ${p.file === part.file ? 'selected' : ''}>
                            ${p.file} (${p.width}x${p.height})
                        </option>
                    `).join('')}
                </select>
            </div>
        `;

        document.getElementById('btn-inspect-edit-part').addEventListener('click', () => {
            this.openPixelEditorForPart(part.file);
        });

        const inX = document.getElementById('input-part-x');
        const inY = document.getElementById('input-part-y');
        const inZ = document.getElementById('input-part-z');

        inX.addEventListener('change', () => { this.pushAssemblerHistory(); part.x = parseInt(inX.value) || 0; this.renderAll(); });
        inY.addEventListener('change', () => { this.pushAssemblerHistory(); part.y = parseInt(inY.value) || 0; this.renderAll(); });
        inZ.addEventListener('change', () => { this.pushAssemblerHistory(); part.z = parseInt(inZ.value) || 0; this.renderAll(); });

        document.getElementById('btn-x-minus').addEventListener('click', () => { this.pushAssemblerHistory(); part.x--; inX.value = part.x; this.renderAll(); });
        document.getElementById('btn-x-plus').addEventListener('click', () => { this.pushAssemblerHistory(); part.x++; inX.value = part.x; this.renderAll(); });
        document.getElementById('btn-y-minus').addEventListener('click', () => { this.pushAssemblerHistory(); part.y--; inY.value = part.y; this.renderAll(); });
        document.getElementById('btn-y-plus').addEventListener('click', () => { this.pushAssemblerHistory(); part.y++; inY.value = part.y; this.renderAll(); });
        document.getElementById('btn-z-minus').addEventListener('click', () => { this.pushAssemblerHistory(); part.z--; inZ.value = part.z; this.renderAll(); });
        document.getElementById('btn-z-plus').addEventListener('click', () => { this.pushAssemblerHistory(); part.z++; inZ.value = part.z; this.renderAll(); });

        document.getElementById('btn-flip-x').addEventListener('click', () => {
            this.pushAssemblerHistory();
            part.flip_x = !part.flip_x;
            this.renderAll();
            this.showToast(`↔️ Đã ${part.flip_x ? 'lật ngang' : 'hủy lật ngang'} ${partTitle}!`);
        });

        const btnFlipY = document.getElementById('btn-flip-y');
        if (btnFlipY) btnFlipY.addEventListener('click', () => {
            this.pushAssemblerHistory();
            part.flip_y = !part.flip_y;
            this.renderAll();
            this.showToast(`↕️ Đã ${part.flip_y ? 'lật dọc' : 'hủy lật dọc'} ${partTitle}!`);
        });

        document.getElementById('select-part-file').addEventListener('change', (e) => {
            this.pushAssemblerHistory();
            part.file = e.target.value;
            this.renderAll();
        });
    }

    selectPart(key) {
        this.selectedPartKey = key;
        const badge = document.getElementById('badge-selected-part');
        if (badge) {
            badge.innerText = key ? (PART_NAMES_VI[key] || key) : 'Chưa chọn';
        }
        this.renderAll();
    }

    selectAllParts() {
        const dirData = this.getCurrentDirData();
        if (!dirData) return;
        this._selectedPartKeys.clear();
        for (const key of PART_KEYS) {
            if (dirData[key] && dirData[key].visible !== false) {
                this._selectedPartKeys.add(key);
            }
        }
        this.renderLayersPanel();
        this.renderInspector();
        this.updateSelectionBox();
        this.showToast(`✨ Đã chọn tất cả ${this._selectedPartKeys.size} bộ phận! [Ctrl+A]`);
    }

    deselectAllParts() {
        this._selectedPartKeys.clear();
        this.renderLayersPanel();
        this.renderInspector();
        this.updateSelectionBox();
        this.showToast('⚪ Đã hủy chọn tất cả!');
    }

    setGender(g) {
        this.currentGender = g;
        document.querySelectorAll('.gender-btn').forEach(b => {
            b.classList.toggle('active', b.dataset.gender === g);
        });
        this.renderAll();
    }

    setDirection(dir) {
        this.currentDir = dir;
        document.querySelectorAll('.dir-btn').forEach(b => {
            b.classList.toggle('active', b.dataset.dir === dir);
        });
        this.renderAll();
    }

    updateHeaderBadges() {
        const bChar = document.getElementById('badge-character');
        const bDir = document.getElementById('badge-direction');
        if (bChar) bChar.innerText = this.currentGender === 'female' ? '👩 Nữ (Female)' : '👨 Nam (Male)';
        if (bDir) bDir.innerText = DIR_LABELS_VI[this.currentDir] || this.currentDir;
    }

    updateZoom() {
        const size = 64 * this.zoom;
        this.canvas.style.width = `${size}px`;
        this.canvas.style.height = `${size}px`;
        if (this.guideCanvas) {
            this.guideCanvas.style.width = `${size}px`;
            this.guideCanvas.style.height = `${size}px`;
        }
        this.wrapper.style.width = `${size}px`;
        this.wrapper.style.height = `${size}px`;
        this.updateSelectionBox();
    }

    // ============================================================
    // CONTEXT MENU
    // ============================================================
    showContextMenu(x, y) {
        const dirData = this.getCurrentDirData();
        if (!dirData || !this.selectedPartKey) return;
        const part = dirData[this.selectedPartKey];

        const title = document.getElementById('ctx-part-title');
        title.innerText = `Mảnh: ${PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey} (${part.file})`;

        this.contextMenu.style.display = 'block';
        this.contextMenu.style.left = `${Math.min(window.innerWidth - 240, x)}px`;
        this.contextMenu.style.top = `${Math.min(window.innerHeight - 280, y)}px`;
    }

    hideContextMenu() {
        this.contextMenu.style.display = 'none';
    }

    // ============================================================
    // PIXEL ART STUDIO ENGINE
    // ============================================================
    setupPixelEditor() {
        // Palette swatches
        const swatchContainer = document.getElementById('palette-swatches');
        swatchContainer.innerHTML = '';
        CHIBI_PALETTE.forEach(c => {
            const btn = document.createElement('button');
            btn.className = 'swatch-btn';
            btn.style.backgroundColor = c;
            btn.addEventListener('click', () => this.setPeColor(c));
            swatchContainer.appendChild(btn);
        });

        // Color Picker & Hex input
        const colorIn = document.getElementById('input-color-picker');
        const hexIn = document.getElementById('input-hex-code');
        colorIn.addEventListener('input', (e) => this.setPeColor(e.target.value));
        hexIn.addEventListener('change', (e) => this.setPeColor(e.target.value));

        // Tools Buttons
        document.querySelectorAll('.pe-tool-btn[data-tool]').forEach(btn => {
            btn.addEventListener('click', () => {
                document.querySelectorAll('.pe-tool-btn[data-tool]').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                this.peTool = btn.dataset.tool;
            });
        });

        // Brush Size
        document.querySelectorAll('.pe-size-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                document.querySelectorAll('.pe-size-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                this.peBrushSize = parseInt(btn.dataset.size);
            });
        });

        // Zoom & Grid
        document.getElementById('select-pe-zoom').addEventListener('change', (e) => {
            this.peZoom = parseInt(e.target.value);
            this.refreshPixelEditorCanvas();
        });
        document.getElementById('check-pe-grid').addEventListener('change', () => {
            this.drawPixelCursor();
        });

        // Actions
        document.getElementById('tool-undo').addEventListener('click', () => this.peUndo());
        document.getElementById('tool-redo').addEventListener('click', () => this.peRedo());
        document.getElementById('tool-clear').addEventListener('click', () => this.peClear());

        const toolFlipX = document.getElementById('tool-flip-x');
        if (toolFlipX) toolFlipX.addEventListener('click', () => this.peFlipCanvasX());
        const toolFlipY = document.getElementById('tool-flip-y');
        if (toolFlipY) toolFlipY.addEventListener('click', () => this.peFlipCanvasY());

        // Mouse events on Pixel Canvas
        this.peCanvas.addEventListener('mousedown', (e) => this.onPeMouseDown(e));
        window.addEventListener('mousemove', (e) => this.onPeMouseMove(e));
        window.addEventListener('mouseup', () => this.onPeMouseUp());

        // Pixel Editor Header Actions
        document.getElementById('btn-pe-back').addEventListener('click', () => this.switchTab('assembler'));
        document.getElementById('btn-pe-save').addEventListener('click', () => this.savePixelPartAndReturn());
        document.getElementById('btn-pe-revert').addEventListener('click', () => this.revertPixelPart());
        document.getElementById('btn-pe-download').addEventListener('click', () => this.downloadPixelPartPng());
    }

    setPeColor(hex) {
        this.peColor = hex;
        document.getElementById('current-color-preview').style.backgroundColor = hex;
        document.getElementById('input-color-picker').value = hex.startsWith('#') ? hex : '#' + hex;
        document.getElementById('input-hex-code').value = hex.toUpperCase();
    }

    openPixelEditorForPart(partFile) {
        this.peCurrentPartName = partFile;
        document.getElementById('pe-current-part-name').innerText = `Đang vẽ: ${partFile}`;
        this.switchTab('pixel');
        this.loadPartIntoPixelEditor(partFile);
    }

    loadPartIntoPixelEditor(partFile) {
        const img = this.getImage(partFile);
        if (!img || !img.complete) {
            setTimeout(() => this.loadPartIntoPixelEditor(partFile), 100);
            return;
        }

        const w = img.naturalWidth || 16;
        const h = img.naturalHeight || 16;

        this.peCanvas.width = w;
        this.peCanvas.height = h;
        this.peCursorCanvas.width = w;
        this.peCursorCanvas.height = h;

        this.peCtx.clearRect(0, 0, w, h);
        this.peCtx.drawImage(img, 0, 0);

        document.getElementById('pe-dimensions').innerText = `${w} x ${h} px`;

        this.peHistory = [];
        this.peRedoStack = [];
        this.pushPeHistory();

        this.refreshPixelEditorCanvas();
    }

    refreshPixelEditorCanvas() {
        const w = this.peCanvas.width || 16;
        const h = this.peCanvas.height || 16;
        const dispW = w * this.peZoom;
        const dispH = h * this.peZoom;

        this.peCanvas.style.width = `${dispW}px`;
        this.peCanvas.style.height = `${dispH}px`;
        this.peCursorCanvas.style.width = `${dispW}px`;
        this.peCursorCanvas.style.height = `${dispH}px`;

        const wrapper = document.getElementById('pe-canvas-wrapper');
        wrapper.style.width = `${dispW}px`;
        wrapper.style.height = `${dispH}px`;

        this.drawPixelCursor();
    }

    pushPeHistory() {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        this.peHistory.push(imgData);
        if (this.peHistory.length > 30) this.peHistory.shift();
        this.peRedoStack = [];
    }

    peUndo() {
        if (this.peHistory.length > 1) {
            const current = this.peHistory.pop();
            this.peRedoStack.push(current);
            const prev = this.peHistory[this.peHistory.length - 1];
            this.peCtx.putImageData(prev, 0, 0);
            this.showToast('↩️ Hoàn tác (Undo)', '#4b5563');
        }
    }

    peRedo() {
        if (this.peRedoStack.length > 0) {
            const next = this.peRedoStack.pop();
            this.peHistory.push(next);
            this.peCtx.putImageData(next, 0, 0);
            this.showToast('↪️ Làm lại (Redo)', '#4b5563');
        }
    }

    peClear() {
        if (!confirm('Bạn có chắc muốn xóa trắng mảnh này không?')) return;
        this.pushPeHistory();
        this.peCtx.clearRect(0, 0, this.peCanvas.width, this.peCanvas.height);
        this.showToast('🗑️ Đã xóa trắng canvas', '#4b5563');
    }

    onPeMouseDown(e) {
        if (this.currentTab !== 'pixel') return;
        this.peIsDrawing = true;
        this.pushPeHistory();

        const { x, y } = this.getPeCanvasCoords(e);
        this.applyPeTool(x, y);
    }

    onPeMouseMove(e) {
        if (this.currentTab !== 'pixel') return;
        const { x, y } = this.getPeCanvasCoords(e);

        document.getElementById('pe-coord').innerText = `Toạ độ: X: ${x}, Y: ${y}`;
        this.drawPixelCursor(x, y);

        if (this.peIsDrawing) {
            if (x !== this.peLastDrawX || y !== this.peLastDrawY) {
                this.applyPeTool(x, y);
            }
        }
    }

    onPeMouseUp() {
        this.peIsDrawing = false;
        this.peLastDrawX = -1;
        this.peLastDrawY = -1;
    }

    getPeCanvasCoords(e) {
        const rect = this.peCanvas.getBoundingClientRect();
        const clientX = e.clientX - rect.left;
        const clientY = e.clientY - rect.top;
        const x = Math.floor(clientX / this.peZoom);
        const y = Math.floor(clientY / this.peZoom);
        return {
            x: Math.max(0, Math.min(this.peCanvas.width - 1, x)),
            y: Math.max(0, Math.min(this.peCanvas.height - 1, y))
        };
    }

    applyPeTool(x, y) {
        this.peLastDrawX = x;
        this.peLastDrawY = y;

        if (this.peTool === 'pencil') {
            this.peCtx.fillStyle = this.peColor;
            this.peCtx.fillRect(x, y, this.peBrushSize, this.peBrushSize);
        } else if (this.peTool === 'eraser') {
            this.peCtx.clearRect(x, y, this.peBrushSize, this.peBrushSize);
        } else if (this.peTool === 'eyedropper') {
            const p = this.peCtx.getImageData(x, y, 1, 1).data;
            if (p[3] > 0) {
                const hex = '#' + ((1 << 24) + (p[0] << 16) + (p[1] << 8) + p[2]).toString(16).slice(1);
                this.setPeColor(hex);
            }
        } else if (this.peTool === 'bucket') {
            this.floodFill(x, y, this.peColor);
        }
    }

    floodFill(startX, startY, fillHex) {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const data = imgData.data;

        // Parse target color
        const startIndex = (startY * w + startX) * 4;
        const sr = data[startIndex];
        const sg = data[startIndex + 1];
        const sb = data[startIndex + 2];
        const sa = data[startIndex + 3];

        // Parse fill color
        const fc = parseInt(fillHex.replace('#', ''), 16);
        const fr = (fc >> 16) & 255;
        const fg = (fc >> 8) & 255;
        const fb = fc & 255;
        const fa = 255;

        if (sr === fr && sg === fg && sb === fb && sa === fa) return;

        const match = (idx) => {
            return data[idx] === sr && data[idx + 1] === sg && data[idx + 2] === sb && data[idx + 3] === sa;
        };

        const queue = [[startX, startY]];
        const visited = new Uint8Array(w * h);

        while (queue.length > 0) {
            const [cx, cy] = queue.pop();
            const pos = cy * w + cx;
            if (visited[pos]) continue;
            visited[pos] = 1;

            const idx = pos * 4;
            if (!match(idx)) continue;

            data[idx] = fr;
            data[idx + 1] = fg;
            data[idx + 2] = fb;
            data[idx + 3] = fa;

            if (cx > 0) queue.push([cx - 1, cy]);
            if (cx < w - 1) queue.push([cx + 1, cy]);
            if (cy > 0) queue.push([cx, cy - 1]);
            if (cy < h - 1) queue.push([cx, cy + 1]);
        }

        this.peCtx.putImageData(imgData, 0, 0);
    }

    drawPixelCursor(hoverX = -1, hoverY = -1) {
        const w = this.peCursorCanvas.width;
        const h = this.peCursorCanvas.height;
        this.peCursorCtx.clearRect(0, 0, w, h);

        const showGrid = document.getElementById('check-pe-grid').checked;
        if (showGrid) {
            this.peCursorCtx.strokeStyle = 'rgba(255, 255, 255, 0.08)';
            this.peCursorCtx.lineWidth = 0.05;
            for (let x = 0; x <= w; x++) {
                this.peCursorCtx.beginPath();
                this.peCursorCtx.moveTo(x, 0);
                this.peCursorCtx.lineTo(x, h);
                this.peCursorCtx.stroke();
            }
            for (let y = 0; y <= h; y++) {
                this.peCursorCtx.beginPath();
                this.peCursorCtx.moveTo(0, y);
                this.peCursorCtx.lineTo(w, y);
                this.peCursorCtx.stroke();
            }
        }

        if (hoverX >= 0 && hoverY >= 0) {
            this.peCursorCtx.strokeStyle = '#ffffff';
            this.peCursorCtx.lineWidth = 0.1;
            this.peCursorCtx.strokeRect(hoverX, hoverY, this.peBrushSize, this.peBrushSize);
        }
    }

    async savePixelPartAndReturn() {
        if (!this.peCurrentPartName) return;
        this.showToast('⏳ Đang lưu mảnh & cập nhật hệ thống Rig...', '#2563eb');

        const dataUrl = this.peCanvas.toDataURL('image/png');
        try {
            const res = await fetch('/api/save_part', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    part_name: this.peCurrentPartName,
                    image_data: dataUrl
                })
            });
            const data = await res.json();
            if (data.success) {
                this.imageCache.clear();
                this.showToast(`✅ ${data.message}`);
                this.switchTab('assembler');
                this.renderAll();
            } else {
                this.showToast(`❌ Lỗi: ${data.error}`, '#dc2626');
            }
        } catch (e) {
            this.showToast(`❌ Lỗi kết nối: ${e.message}`, '#dc2626');
        }
    }

    async revertPixelPart() {
        if (!this.peCurrentPartName) return;
        if (!confirm(`Bạn có chắc muốn khôi phục mảnh ${this.peCurrentPartName} về ảnh gốc không?`)) return;

        try {
            const res = await fetch('/api/revert_part', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ part_name: this.peCurrentPartName })
            });
            const data = await res.json();
            if (data.success) {
                this.invalidatePartCache(this.peCurrentPartName);
                this.loadPartIntoPixelEditor(this.peCurrentPartName);
                this.showToast(`↺ ${data.message}`);
            } else {
                this.showToast(`❌ Lỗi: ${data.error}`, '#dc2626');
            }
        } catch (e) {
            this.showToast(`❌ Lỗi kết nối: ${e.message}`, '#dc2626');
        }
    }

    downloadPixelPartPng() {
        const a = document.createElement('a');
        a.href = this.peCanvas.toDataURL('image/png');
        a.download = `${this.peCurrentPartName}.png`;
        a.click();
        this.showToast(`📥 Đã tải ${this.peCurrentPartName}.png về máy!`);
    }

    peFlipCanvasX() {
        this.pushPeHistory();
        const w = this.peCanvas.width, h = this.peCanvas.height;
        const temp = document.createElement('canvas');
        temp.width = w; temp.height = h;
        const tCtx = temp.getContext('2d');
        tCtx.drawImage(this.peCanvas, 0, 0);
        this.peCtx.clearRect(0, 0, w, h);
        this.peCtx.save();
        this.peCtx.translate(w, 0);
        this.peCtx.scale(-1, 1);
        this.peCtx.drawImage(temp, 0, 0);
        this.peCtx.restore();
        this.showToast('↔️ Đã lật ngang toàn bộ ảnh!');
    }

    peFlipCanvasY() {
        this.pushPeHistory();
        const w = this.peCanvas.width, h = this.peCanvas.height;
        const temp = document.createElement('canvas');
        temp.width = w; temp.height = h;
        const tCtx = temp.getContext('2d');
        tCtx.drawImage(this.peCanvas, 0, 0);
        this.peCtx.clearRect(0, 0, w, h);
        this.peCtx.save();
        this.peCtx.translate(0, h);
        this.peCtx.scale(1, -1);
        this.peCtx.drawImage(temp, 0, 0);
        this.peCtx.restore();
        this.showToast('↕️ Đã lật dọc toàn bộ ảnh!');
    }

    // ============================================================
    // MOUSE & KEYBOARD EVENTS FOR ASSEMBLER
    // ============================================================
    setupEventListeners() {
        // Tab Buttons
        document.getElementById('tab-btn-assembler').addEventListener('click', () => this.switchTab('assembler'));
        document.getElementById('tab-btn-pixel').addEventListener('click', () => {
            const dirData = this.getCurrentDirData();
            const part = dirData && this.selectedPartKey ? dirData[this.selectedPartKey] : null;
            this.openPixelEditorForPart(part ? part.file : 'female_torso_front');
        });

        // Gender Buttons
        document.getElementById('btn-gender-female').addEventListener('click', () => this.setGender('female'));
        document.getElementById('btn-gender-male').addEventListener('click', () => this.setGender('male'));

        // Direction Compass Buttons
        document.querySelectorAll('.dir-btn').forEach(btn => {
            btn.addEventListener('click', () => this.setDirection(btn.dataset.dir));
        });

        // Zoom & Background
        document.getElementById('select-zoom').addEventListener('change', (e) => {
            this.zoom = parseInt(e.target.value);
            this.updateZoom();
        });

        document.getElementById('select-bg').addEventListener('change', (e) => {
            const val = e.target.value;
            this.wrapper.className = 'canvas-wrapper';
            if (val === 'checker') this.wrapper.classList.add('checker-bg');
            else if (val === 'dark') this.wrapper.style.backgroundColor = '#18181b';
            else if (val === 'light') this.wrapper.style.backgroundColor = '#e4e4e7';
            else if (val === 'green') this.wrapper.style.backgroundColor = '#00ff00';
            else if (val === 'grass') this.wrapper.style.backgroundColor = '#527a4d';
        });

        document.getElementById('check-bbox').addEventListener('change', (e) => {
            this.showBbox = e.target.checked;
            this.updateSelectionBox();
        });

        // Open pixel editor button in viewport header
        document.getElementById('btn-open-pixel-editor').addEventListener('click', () => {
            const dirData = this.getCurrentDirData();
            const part = dirData && this.selectedPartKey ? dirData[this.selectedPartKey] : null;
            if (part && part.file) {
                this.openPixelEditorForPart(part.file);
            }
        });

        // Mouse Dragging on Canvas
        this.canvas.addEventListener('mousedown', (e) => this.onMouseDown(e));
        window.addEventListener('mousemove', (e) => this.onMouseMove(e));
        window.addEventListener('mouseup', () => this.onMouseUp());

        // Context Menu handling
        this.canvas.addEventListener('contextmenu', (e) => {
            e.preventDefault();
            this.onCanvasRightClick(e);
        });

        window.addEventListener('click', (e) => {
            if (!this.contextMenu.contains(e.target)) {
                this.hideContextMenu();
            }
        });

        // Wire context menu actions
        document.getElementById('ctx-edit-pixel').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            const part = dirData && this.selectedPartKey ? dirData[this.selectedPartKey] : null;
            if (part) this.openPixelEditorForPart(part.file);
        });

        document.getElementById('ctx-move-up').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            if (dirData && this.selectedPartKey) {
                this.pushAssemblerHistory();
                dirData[this.selectedPartKey].z = (dirData[this.selectedPartKey].z || 10) + 1;
                this.renderAll();
            }
        });

        document.getElementById('ctx-move-down').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            if (dirData && this.selectedPartKey) {
                this.pushAssemblerHistory();
                dirData[this.selectedPartKey].z = Math.max(0, (dirData[this.selectedPartKey].z || 10) - 1);
                this.renderAll();
            }
        });

        document.getElementById('ctx-toggle-vis').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            if (dirData && this.selectedPartKey) {
                this.pushAssemblerHistory();
                dirData[this.selectedPartKey].visible = !dirData[this.selectedPartKey].visible;
                this.renderAll();
            }
        });

        document.getElementById('ctx-flip-x').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            if (dirData && this.selectedPartKey) {
                this.pushAssemblerHistory();
                dirData[this.selectedPartKey].flip_x = !dirData[this.selectedPartKey].flip_x;
                this.renderAll();
                this.showToast(`↔️ Đã ${dirData[this.selectedPartKey].flip_x ? 'lật ngang' : 'hủy lật ngang'} bộ phận!`);
            }
        });

        const ctxFlipY = document.getElementById('ctx-flip-y');
        if (ctxFlipY) {
            ctxFlipY.addEventListener('click', () => {
                this.hideContextMenu();
                const dirData = this.getCurrentDirData();
                if (dirData && this.selectedPartKey) {
                    this.pushAssemblerHistory();
                    dirData[this.selectedPartKey].flip_y = !dirData[this.selectedPartKey].flip_y;
                    this.renderAll();
                    this.showToast(`↕️ Đã ${dirData[this.selectedPartKey].flip_y ? 'lật dọc' : 'hủy lật dọc'} bộ phận!`);
                }
            });
        }

        document.getElementById('ctx-reset-pos').addEventListener('click', () => {
            this.hideContextMenu();
            this.pushAssemblerHistory();
            this.resetCurrentDir();
        });

        // Tool Mode selection [V] Select, [H] Pan
        const btnModeSelect = document.getElementById('btn-mode-select');
        const btnModePan = document.getElementById('btn-mode-pan');
        if (btnModeSelect) btnModeSelect.addEventListener('click', () => this.setToolMode('select'));
        if (btnModePan) {
            btnModePan.addEventListener('click', () => this.setToolMode('pan'));
            btnModePan.addEventListener('dblclick', () => this.resetPan());
        }

        // Undo & Redo buttons
        const btnUndo = document.getElementById('btn-undo');
        const btnRedo = document.getElementById('btn-redo');
        const btnUndoMini = document.getElementById('btn-undo-mini');
        const btnRedoMini = document.getElementById('btn-redo-mini');
        if (btnUndo) btnUndo.addEventListener('click', () => this.undoAssembler());
        if (btnRedo) btnRedo.addEventListener('click', () => this.redoAssembler());
        if (btnUndoMini) btnUndoMini.addEventListener('click', () => this.undoAssembler());
        if (btnRedoMini) btnRedoMini.addEventListener('click', () => this.redoAssembler());

        // Alt + Mouse Wheel Zoom on Viewports
        const vpContainer = document.querySelector('.viewport-container');
        if (vpContainer) {
            vpContainer.addEventListener('wheel', (e) => {
                if (e.altKey) {
                    e.preventDefault();
                    const delta = e.deltaY < 0 ? 1 : -1;
                    this.zoom = Math.max(2, Math.min(24, this.zoom + delta));
                    const selZoom = document.getElementById('select-zoom');
                    if (selZoom) selZoom.value = this.zoom;
                    this.updateZoom();
                }
            }, { passive: false });

            // Allow panning by dragging on background of viewport-container
            vpContainer.addEventListener('mousedown', (e) => {
                if (e.button === 1 || this.isSpacePressed || this.toolMode === 'pan') {
                    if (e.target !== this.canvas) {
                        this.onMouseDown(e);
                    }
                }
            });
        }

        const peViewport = document.querySelector('.pe-viewport');
        if (peViewport) {
            peViewport.addEventListener('wheel', (e) => {
                if (e.altKey) {
                    e.preventDefault();
                    const delta = e.deltaY < 0 ? 2 : -2;
                    this.peZoom = Math.max(4, Math.min(48, this.peZoom + delta));
                    const selPeZoom = document.getElementById('select-pe-zoom');
                    if (selPeZoom) selPeZoom.value = this.peZoom;
                    this.refreshPixelEditorCanvas();
                }
            }, { passive: false });
        }

        // Keyboard Shortcuts
        window.addEventListener('keydown', (e) => this.onKeyDown(e));
        window.addEventListener('keyup', (e) => {
            if (e.code === 'Space') {
                this.isSpacePressed = false;
                this.updateCursorMode();
            }
        });

        // Save Button (Unity Export)
        document.getElementById('btn-save-unity').addEventListener('click', () => this.saveAndExport());
        document.getElementById('btn-export-json').addEventListener('click', () => this.exportJson());
        document.getElementById('file-import-json').addEventListener('change', (e) => this.importJson(e));
        document.getElementById('btn-reset-default').addEventListener('click', () => this.resetDefault());
        document.getElementById('btn-reset-current-dir').addEventListener('click', () => this.resetCurrentDir());

        // Symmetry & Alignment Tools
        const symCenterX = document.getElementById('btn-sym-center-x');
        if (symCenterX) symCenterX.addEventListener('click', () => this.symmetryCenterX());
        const symCenterY = document.getElementById('btn-sym-center-y');
        if (symCenterY) symCenterY.addEventListener('click', () => this.symmetryCenterY());
        const symMirrorX = document.getElementById('btn-sym-mirror-x');
        if (symMirrorX) symMirrorX.addEventListener('click', () => this.symmetryMirrorX());
        const symMirrorY = document.getElementById('btn-sym-mirror-y');
        if (symMirrorY) symMirrorY.addEventListener('click', () => this.symmetryMirrorY());
        const symArms = document.getElementById('btn-sym-arms');
        if (symArms) symArms.addEventListener('click', () => this.symmetryArms());
        const symLegs = document.getElementById('btn-sym-legs');
        if (symLegs) symLegs.addEventListener('click', () => this.symmetryLegs());
        const symOpposite = document.getElementById('btn-sym-copy-opposite');
        if (symOpposite) symOpposite.addEventListener('click', () => this.symmetryCopyOpposite());

        // Arrange & Alignment Buttons
        const bindBtn = (id, fn) => {
            const b = document.getElementById(id);
            if (b) b.addEventListener('click', fn);
        };
        bindBtn('btn-align-left', () => this.alignSelected('left'));
        bindBtn('btn-align-center-x', () => this.alignSelected('center_x'));
        bindBtn('btn-align-right', () => this.alignSelected('right'));
        bindBtn('btn-align-top', () => this.alignSelected('top'));
        bindBtn('btn-align-center-y', () => this.alignSelected('center_y'));
        bindBtn('btn-align-bottom', () => this.alignSelected('bottom'));
        bindBtn('btn-distribute-x', () => this.distributeSelected('horizontal'));
        bindBtn('btn-distribute-y', () => this.distributeSelected('vertical'));
        bindBtn('btn-bring-front', () => this.bringSelectedToFront());
        bindBtn('btn-send-back', () => this.sendSelectedToBack());

        // Smart Snap Checkbox
        const chkSnap = document.getElementById('check-snap');
        if (chkSnap) {
            chkSnap.addEventListener('change', (e) => {
                this.snapEnabled = e.target.checked;
                this.showToast(this.snapEnabled ? '🧲 Đã BẬT gợi ý căn chỉnh giữa thông minh!' : '⚪ Đã TẮT gợi ý căn chỉnh!');
            });
        }

        // Select All & Deselect All Buttons
        bindBtn('btn-select-all', () => this.selectAllParts());
        bindBtn('btn-deselect-all', () => this.deselectAllParts());
        const ctxSelectAll = document.getElementById('ctx-select-all');
        if (ctxSelectAll) ctxSelectAll.addEventListener('click', () => {
            this.hideContextMenu();
            this.selectAllParts();
        });
    }

    // ============================================================
    // SYMMETRY & ALIGNMENT METHODS
    // ============================================================
    symmetryCenterX() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !this.selectedPartKey) return;
        this.pushAssemblerHistory();
        const part = dirData[this.selectedPartKey];
        const img = this.getImage(part.file);
        const w = img && img.complete ? img.naturalWidth : 16;
        part.x = Math.round(32 - w / 2);
        this.renderAll();
        this.showToast(`🎯 Đã căn giữa ${PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey} theo trục X = 32!`);
    }

    symmetryCenterY() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !this.selectedPartKey) return;
        this.pushAssemblerHistory();
        const part = dirData[this.selectedPartKey];
        const img = this.getImage(part.file);
        const h = img && img.complete ? img.naturalHeight : 16;
        part.y = Math.round(32 - h / 2);
        this.renderAll();
        this.showToast(`🎯 Đã căn giữa ${PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey} theo trục Y = 32!`);
    }

    symmetryMirrorX() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !this.selectedPartKey) return;
        this.pushAssemblerHistory();
        const part = dirData[this.selectedPartKey];
        const img = this.getImage(part.file);
        const w = img && img.complete ? img.naturalWidth : 16;
        part.x = 64 - part.x - w;
        part.flip_x = !part.flip_x;
        this.renderAll();
        this.showToast(`↔️ Đã lật đối xứng ${PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey} qua tâm X!`);
    }

    symmetryMirrorY() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !this.selectedPartKey) return;
        this.pushAssemblerHistory();
        const part = dirData[this.selectedPartKey];
        const img = this.getImage(part.file);
        const h = img && img.complete ? img.naturalHeight : 16;
        part.y = 64 - part.y - h;
        part.flip_y = !part.flip_y;
        this.renderAll();
        this.showToast(`↕️ Đã lật đối xứng ${PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey} qua tâm Y!`);
    }

    symmetryArms() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !dirData['idle_arm_left'] || !dirData['idle_arm_right']) return;
        this.pushAssemblerHistory();
        const al = dirData['idle_arm_left'];
        const ar = dirData['idle_arm_right'];
        const imgL = this.getImage(al.file);
        const wL = imgL && imgL.complete ? imgL.naturalWidth : 16;
        ar.x = 64 - al.x - wL;
        ar.y = al.y;
        ar.z = al.z;
        ar.flip_x = !al.flip_x;
        this.renderAll();
        this.showToast('👥 Đã đồng bộ đối xứng Tay Phải theo Tay Trái!');
    }

    symmetryLegs() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !dirData['idle_leg_left'] || !dirData['idle_leg_right']) return;
        this.pushAssemblerHistory();
        const ll = dirData['idle_leg_left'];
        const lr = dirData['idle_leg_right'];
        const imgL = this.getImage(ll.file);
        const wL = imgL && imgL.complete ? imgL.naturalWidth : 16;
        lr.x = 64 - ll.x - wL;
        lr.y = ll.y;
        lr.z = ll.z;
        lr.flip_x = !ll.flip_x;
        this.renderAll();
        this.showToast('👥 Đã đồng bộ đối xứng Chân Phải theo Chân Trái!');
    }

    symmetryCopyOpposite() {
        if (!this.config || !this.config[this.currentGender]) return;
        const gData = this.config[this.currentGender];
        const currentData = gData[this.currentDir];
        if (!currentData) return;

        const OPPOSITE_MAP = {
            'east': 'west', 'west': 'east',
            'south_east': 'south_west', 'south_west': 'south_east',
            'north_east': 'north_west', 'north_west': 'north_east'
        };

        const targetDir = OPPOSITE_MAP[this.currentDir];
        if (!targetDir) {
            this.showToast('ℹ️ Hướng South và North là hướng chính diện, không có hướng đối lập!', '#3b82f6');
            return;
        }

        this.pushAssemblerHistory();
        const mirrored = {};
        for (const [key, part] of Object.entries(currentData)) {
            let outKey = key;
            if (key === 'idle_arm_left') outKey = 'idle_arm_right';
            else if (key === 'idle_arm_right') outKey = 'idle_arm_left';
            else if (key === 'idle_leg_left') outKey = 'idle_leg_right';
            else if (key === 'idle_leg_right') outKey = 'idle_leg_left';

            const img = this.getImage(part.file);
            const w = img && img.complete ? img.naturalWidth : 16;
            mirrored[outKey] = {
                ...part,
                x: 64 - part.x - w,
                flip_x: !part.flip_x
            };
        }

        gData[targetDir] = mirrored;
        this.renderAll();
        this.showToast(`🌐 Đã sao chép đối xứng từ ${DIR_LABELS_VI[this.currentDir]} sang ${DIR_LABELS_VI[targetDir]}!`);
    }

    // ============================================================
    // MOUSE DRAGGING & VIEWPORT PANNING
    // ============================================================
    onMouseDown(e) {
        this.hideContextMenu();

        // 1. Pan Viewport (Middle Click, or Space key held, or Pan Tool mode)
        if (e.button === 1 || this.isSpacePressed || this.toolMode === 'pan') {
            e.preventDefault();
            this.isPanning = true;
            this.panStartX = e.clientX;
            this.panStartY = e.clientY;
            this.panStartOffsetX = this.panX;
            this.panStartOffsetY = this.panY;
            this.updateCursorMode();
            return;
        }

        // 2. Left click part dragging or marquee selection
        if (e.button !== 0) return;

        const rect = this.canvas.getBoundingClientRect();
        const clientX = e.clientX - rect.left;
        const clientY = e.clientY - rect.top;

        const pixelX = Math.floor(clientX / this.zoom);
        const pixelY = Math.floor(clientY / this.zoom);

        const dirData = this.getCurrentDirData();
        if (!dirData) return;

        const parts = PART_KEYS.map(key => ({
            key,
            ...dirData[key]
        })).filter(p => p && p.visible !== false);

        parts.sort((a, b) => (b.z || 10) - (a.z || 10));

        let hitKey = null;
        for (const p of parts) {
            const img = this.getImage(p.file);
            if (!img || !img.complete) continue;
            const pw = img.naturalWidth;
            const ph = img.naturalHeight;

            if (pixelX >= p.x && pixelX < p.x + pw &&
                pixelY >= p.y && pixelY < p.y + ph) {
                hitKey = p.key;
                break;
            }
        }

        const isCtrl = e.ctrlKey || e.metaKey || e.shiftKey;

        if (hitKey) {
            if (isCtrl) {
                // Toggle clicked part in selection
                if (this._selectedPartKeys.has(hitKey)) {
                    this._selectedPartKeys.delete(hitKey);
                } else {
                    this._selectedPartKeys.add(hitKey);
                }
            } else {
                // If clicked part is NOT already selected, select ONLY it
                if (!this._selectedPartKeys.has(hitKey)) {
                    this._selectedPartKeys.clear();
                    this._selectedPartKeys.add(hitKey);
                }
                // If it IS already in multi-selection, keep the group intact so user can drag the entire group!
            }

            if (this._selectedPartKeys.size > 0) {
                this.pushAssemblerHistory();
                this.isDragging = true;
                this.dragStartX = pixelX;
                this.dragStartY = pixelY;

                // Cache start positions for ALL selected parts
                this.dragStartPositions.clear();
                for (const k of this._selectedPartKeys) {
                    const p = dirData[k];
                    if (p) {
                        this.dragStartPositions.set(k, { x: p.x, y: p.y });
                    }
                }
            }

            this.renderLayersPanel();
            this.renderInspector();
            this.updateSelectionBox();
        } else {
            // Clicked on EMPTY canvas space
            if (!isCtrl) {
                // Deselect all when clicking on empty canvas without Ctrl
                this._selectedPartKeys.clear();
                this.renderLayersPanel();
                this.renderInspector();
                this.updateSelectionBox();
            }

            // Start Marquee Selection box
            this.isMarqueeSelecting = true;
            this.marqueeStartX = clientX;
            this.marqueeStartY = clientY;
            this.marqueeInitialSelection = new Set(this._selectedPartKeys);

            if (this.marqueeBox) {
                this.marqueeBox.style.display = 'block';
                this.marqueeBox.style.left = `${clientX}px`;
                this.marqueeBox.style.top = `${clientY}px`;
                this.marqueeBox.style.width = '0px';
                this.marqueeBox.style.height = '0px';
            }
        }
    }

    onCanvasRightClick(e) {
        const rect = this.canvas.getBoundingClientRect();
        const clientX = e.clientX - rect.left;
        const clientY = e.clientY - rect.top;

        const pixelX = Math.floor(clientX / this.zoom);
        const pixelY = Math.floor(clientY / this.zoom);

        const dirData = this.getCurrentDirData();
        if (!dirData) return;

        const parts = PART_KEYS.map(key => ({
            key,
            ...dirData[key]
        })).filter(p => p && p.visible !== false);

        parts.sort((a, b) => (b.z || 10) - (a.z || 10));

        for (const p of parts) {
            const img = this.getImage(p.file);
            if (!img || !img.complete) continue;
            const pw = img.naturalWidth;
            const ph = img.naturalHeight;

            if (pixelX >= p.x && pixelX < p.x + pw &&
                pixelY >= p.y && pixelY < p.y + ph) {
                this.selectPart(p.key);
                break;
            }
        }

        this.showContextMenu(e.clientX, e.clientY);
    }

    onMouseMove(e) {
        if (this.isPanning) {
            const dx = e.clientX - this.panStartX;
            const dy = e.clientY - this.panStartY;
            this.panX = this.panStartOffsetX + dx;
            this.panY = this.panStartOffsetY + dy;
            this.updatePanTransform();
            return;
        }

        if (this.currentTab !== 'assembler') return;

        const rect = this.canvas.getBoundingClientRect();
        const clientX = e.clientX - rect.left;
        const clientY = e.clientY - rect.top;

        // 1. Marquee Selection update
        if (this.isMarqueeSelecting) {
            const curX = Math.max(0, Math.min(rect.width, clientX));
            const curY = Math.max(0, Math.min(rect.height, clientY));

            const left = Math.min(this.marqueeStartX, curX);
            const top = Math.min(this.marqueeStartY, curY);
            const width = Math.abs(curX - this.marqueeStartX);
            const height = Math.abs(curY - this.marqueeStartY);

            if (this.marqueeBox) {
                this.marqueeBox.style.left = `${left}px`;
                this.marqueeBox.style.top = `${top}px`;
                this.marqueeBox.style.width = `${width}px`;
                this.marqueeBox.style.height = `${height}px`;
            }

            // Determine intersecting parts in pixel coordinates
            const boxMinPxX = left / this.zoom;
            const boxMaxPxX = (left + width) / this.zoom;
            const boxMinPxY = top / this.zoom;
            const boxMaxPxY = (top + height) / this.zoom;

            const dirData = this.getCurrentDirData();
            if (dirData) {
                const newSelection = new Set(this.marqueeInitialSelection);
                for (const key of PART_KEYS) {
                    const p = dirData[key];
                    if (!p || p.visible === false) continue;
                    const img = this.getImage(p.file);
                    const pw = img && img.complete ? img.naturalWidth : 16;
                    const ph = img && img.complete ? img.naturalHeight : 16;

                    // AABB intersection check
                    const pMaxX = p.x + pw;
                    const pMaxY = p.y + ph;
                    if (boxMinPxX < pMaxX && boxMaxPxX > p.x &&
                        boxMinPxY < pMaxY && boxMaxPxY > p.y) {
                        newSelection.add(key);
                    }
                }
                this._selectedPartKeys = newSelection;
                this.updateSelectionBox();
                this.renderLayersPanel();
                this.renderInspector();
            }
            return;
        }

        // 2. Dragging Selected Parts with Smart Snap & Guidelines
        const pixelX = Math.floor(clientX / this.zoom);
        const pixelY = Math.floor(clientY / this.zoom);

        if (this.isDragging && this._selectedPartKeys.size > 0) {
            const dirData = this.getCurrentDirData();
            if (!dirData) return;

            let dx = pixelX - this.dragStartX;
            let dy = pixelY - this.dragStartY;

            // SMART SNAP & NEON CENTER GUIDES (Trục X=32, Trục Y=32)
            this.activeGuides = [];
            if (this.snapEnabled) {
                let minX = Infinity, maxX = -Infinity;
                let minY = Infinity, maxY = -Infinity;

                for (const [key, startPos] of this.dragStartPositions.entries()) {
                    const p = dirData[key];
                    if (!p) continue;
                    const img = this.getImage(p.file);
                    const pw = img && img.complete ? img.naturalWidth : 16;
                    const ph = img && img.complete ? img.naturalHeight : 16;

                    const curTrialX = startPos.x + dx;
                    const curTrialY = startPos.y + dy;
                    minX = Math.min(minX, curTrialX);
                    maxX = Math.max(maxX, curTrialX + pw);
                    minY = Math.min(minY, curTrialY);
                    maxY = Math.max(maxY, curTrialY + ph);
                }

                if (minX !== Infinity) {
                    const groupCenterX = (minX + maxX) / 2;
                    const groupCenterY = (minY + maxY) / 2;

                    // Snap Group Center X to 32 (Tolerance: 1.5 pixels)
                    if (Math.abs(groupCenterX - 32) <= 1.5) {
                        const snapAdjustmentX = Math.round(32 - groupCenterX);
                        dx += snapAdjustmentX;
                        this.activeGuides.push({ type: 'vert', pos: 32 });
                    }

                    // Snap Group Center Y to 32 (Tolerance: 1.5 pixels)
                    if (Math.abs(groupCenterY - 32) <= 1.5) {
                        const snapAdjustmentY = Math.round(32 - groupCenterY);
                        dy += snapAdjustmentY;
                        this.activeGuides.push({ type: 'horiz', pos: 32 });
                    }
                }
            }

            // Draw visual neon center guides
            this.drawSnapGuides();

            // Apply dx, dy to all selected parts
            for (const [key, startPos] of this.dragStartPositions.entries()) {
                const part = dirData[key];
                if (part) {
                    part.x = startPos.x + dx;
                    part.y = startPos.y + dy;
                }
            }

            this.renderMainCanvas();
            this.renderSmallPreviews();
            this.updateSelectionBox();
        }
    }

    onMouseUp() {
        if (this.isPanning) {
            this.isPanning = false;
            this.updateCursorMode();
        }
        if (this.isMarqueeSelecting) {
            this.isMarqueeSelecting = false;
            if (this.marqueeBox) this.marqueeBox.style.display = 'none';
            this.renderAll();
        }
        if (this.isDragging) {
            this.isDragging = false;
            this.clearSnapGuides();
            this.renderAll();
        }
    }

    // ============================================================
    // KEYBOARD ENGINE (CTRL+S, CTRL+Z, CTRL+Y, FLIPS & NUDGE)
    // ============================================================
    onKeyDown(e) {
        // Space key activation for pan tool
        if (e.code === 'Space' && e.target.tagName !== 'INPUT' && e.target.tagName !== 'SELECT') {
            e.preventDefault();
            this.isSpacePressed = true;
            this.updateCursorMode();
        }

        // Global shortcuts with Ctrl / Cmd
        if (e.ctrlKey || e.metaKey) {
            const k = e.key.toLowerCase();
            if (k === 's') {
                e.preventDefault();
                if (this.currentTab === 'assembler') {
                    this.saveAndExport();
                } else if (this.currentTab === 'pixel') {
                    this.savePixelPartAndReturn();
                }
                return;
            } else if (k === 'z') {
                e.preventDefault();
                if (e.shiftKey) {
                    if (this.currentTab === 'assembler') this.redoAssembler();
                    else this.peRedo();
                } else {
                    if (this.currentTab === 'assembler') this.undoAssembler();
                    else this.peUndo();
                }
                return;
            } else if (k === 'y') {
                e.preventDefault();
                if (this.currentTab === 'assembler') this.redoAssembler();
                else this.peRedo();
                return;
            } else if (k === 'a') {
                if (e.target.tagName !== 'INPUT' && e.target.tagName !== 'SELECT') {
                    e.preventDefault();
                    if (this.currentTab === 'assembler') {
                        this.selectAllParts();
                    }
                    return;
                }
            }
        }

        if (e.target.tagName === 'INPUT' || e.target.tagName === 'SELECT') return;

        if (this.currentTab === 'assembler') {
            const dirData = this.getCurrentDirData();

            // Tool modes [V] Select, [H] Pan, [R] Reset Pan
            if (e.key.toLowerCase() === 'v') {
                this.setToolMode('select');
                return;
            } else if (e.key.toLowerCase() === 'h') {
                this.setToolMode('pan');
                return;
            } else if (e.key.toLowerCase() === 'r') {
                this.resetPan();
                return;
            }

            // Flip shortcuts [X] Horizontal Flip, [Y] Vertical Flip for ALL selected parts
            if (this.selectedPartKeys.size > 0 && e.key.toLowerCase() === 'x') {
                e.preventDefault();
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData && dirData[k]) dirData[k].flip_x = !dirData[k].flip_x;
                }
                this.renderAll();
                this.showToast(`↔️ Đã lật ngang ${this.selectedPartKeys.size} bộ phận!`);
                return;
            } else if (this.selectedPartKeys.size > 0 && e.key.toLowerCase() === 'y') {
                e.preventDefault();
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData && dirData[k]) dirData[k].flip_y = !dirData[k].flip_y;
                }
                this.renderAll();
                this.showToast(`↕️ Đã lật dọc ${this.selectedPartKeys.size} bộ phận!`);
                return;
            }

            if (this.selectedPartKeys.size === 0 || !dirData) return;

            const step = e.shiftKey ? 5 : 1;
            let handled = false;

            if (e.key === 'ArrowLeft' || e.key.toLowerCase() === 'a') {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].x -= step;
                }
                handled = true;
            } else if (e.key === 'ArrowRight' || e.key.toLowerCase() === 'd') {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].x += step;
                }
                handled = true;
            } else if (e.key === 'ArrowUp' || e.key.toLowerCase() === 'w') {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].y -= step;
                }
                handled = true;
            } else if (e.key === 'ArrowDown' || e.key.toLowerCase() === 's') {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].y += step;
                }
                handled = true;
            } else if (e.key === 'PageUp') {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].z = (dirData[k].z || 10) + 1;
                }
                handled = true;
            } else if (e.key === 'PageDown') {
                this.pushAssemblerHistory();
                for (const k of this.selectedPartKeys) {
                    if (dirData[k]) dirData[k].z = Math.max(0, (dirData[k].z || 10) - 1);
                }
                handled = true;
            }

            if (handled) {
                e.preventDefault();
                this.renderAll();
            }
        } else if (this.currentTab === 'pixel') {
            if (e.key.toLowerCase() === 'b') {
                document.getElementById('tool-pencil').click();
            } else if (e.key.toLowerCase() === 'e') {
                document.getElementById('tool-eraser').click();
            } else if (e.key.toLowerCase() === 'g') {
                document.getElementById('tool-bucket').click();
            } else if (e.key.toLowerCase() === 'i') {
                document.getElementById('tool-eyedropper').click();
            } else if (e.key.toLowerCase() === 'x') {
                this.peFlipCanvasX();
            } else if (e.key.toLowerCase() === 'y' && !e.ctrlKey) {
                this.peFlipCanvasY();
            }
        }
    }

    // ============================================================
    // API ACTIONS
    // ============================================================
    async saveAndExport() {
        this.showToast('⏳ Đang lưu cấu hình và xuất ra Unity Resources...', '#2563eb');
        try {
            const res = await fetch('/api/save', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.config)
            });
            const data = await res.json();
            if (data.success) {
                this.showToast('✅ Đã lưu cấu hình & xuất thành công toàn bộ 16 bộ rig vào Unity Assets!');
            } else {
                this.showToast(`❌ Lỗi: ${data.error}`, '#dc2626');
            }
        } catch (e) {
            this.showToast(`❌ Lỗi kết nối máy chủ: ${e.message}`, '#dc2626');
        }
    }

    async resetDefault() {
        if (!confirm('Bạn có chắc chắn muốn khôi phục toàn bộ cấu hình về mặc định ban đầu không?')) return;
        try {
            const res = await fetch('/api/reset', { method: 'POST' });
            const data = await res.json();
            if (data.success) {
                this.config = data.config;
                this.renderAll();
                this.showToast('🔄 Đã khôi phục cài đặt mặc định thành công!');
            }
        } catch (e) {
            this.showToast(`❌ Lỗi: ${e.message}`, '#dc2626');
        }
    }

    async resetCurrentDir() {
        try {
            const res = await fetch('/api/default_config');
            const defData = await res.json();
            if (defData && defData[this.currentGender] && defData[this.currentGender][this.currentDir]) {
                this.config[this.currentGender][this.currentDir] = JSON.parse(JSON.stringify(defData[this.currentGender][this.currentDir]));
                this.renderAll();
                this.showToast(`↺ Đã đặt lại hướng ${DIR_LABELS_VI[this.currentDir]} về mặc định!`);
            }
        } catch (e) {
            console.error(e);
        }
    }

    exportJson() {
        const str = JSON.stringify(this.config, null, 2);
        const blob = new Blob([str], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `rig_workshop_config.json`;
        a.click();
        URL.revokeObjectURL(url);
        this.showToast('📥 Đã tải file JSON về máy!');
    }

    importJson(e) {
        const file = e.target.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = (event) => {
            try {
                this.config = JSON.parse(event.target.result);
                this.renderAll();
                this.showToast('📤 Đã nạp cấu hình JSON thành công!');
            } catch (err) {
                this.showToast('❌ File JSON không hợp lệ!', '#dc2626');
            }
        };
        reader.readAsText(file);
    }

    showToast(message, bgColor = '#065f46') {
        const toast = document.getElementById('toast');
        if (!toast) return;
        toast.innerText = message;
        toast.style.backgroundColor = bgColor;
        toast.classList.add('show');
        clearTimeout(this.toastTimer);
        this.toastTimer = setTimeout(() => {
            toast.classList.remove('show');
        }, 3500);
    }
}

window.addEventListener('DOMContentLoaded', () => {
    window.workshopApp = new RigWorkshopApp();
});
