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
        this.selectedPartKey = 'idle_body';
        this.zoom = 8;
        this.showGrid = true;
        this.showBbox = true;
        this.availableParts = [];
        this.imageCache = new Map();

        // Assembler Canvas
        this.canvas = document.getElementById('main-canvas');
        this.ctx = this.canvas.getContext('2d');
        this.ctx.imageSmoothingEnabled = false;
        this.wrapper = document.getElementById('canvas-wrapper');
        this.selectionBox = document.getElementById('selection-box');
        this.contextMenu = document.getElementById('context-menu');

        // Dragging state
        this.isDragging = false;
        this.dragStartX = 0;
        this.dragStartY = 0;
        this.dragStartPartX = 0;
        this.dragStartPartY = 0;

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

            if (part.flip_x) {
                this.ctx.save();
                this.ctx.translate(px + pw, py);
                this.ctx.scale(-1, 1);
                this.ctx.drawImage(img, 0, 0);
                this.ctx.restore();
            } else {
                this.ctx.drawImage(img, px, py);
            }
        }

        this.updateSelectionBox();
    }

    updateSelectionBox() {
        if (!this.showBbox || !this.selectedPartKey) {
            this.selectionBox.style.display = 'none';
            return;
        }

        const dirData = this.getCurrentDirData();
        const part = dirData ? dirData[this.selectedPartKey] : null;
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

        const coordInd = document.getElementById('coord-indicator');
        if (coordInd) {
            coordInd.innerText = `Toạ độ: X: ${part.x}, Y: ${part.y} | Z-Order: ${part.z} | Kích thước: ${pw}x${ph}`;
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
                if (part.flip_x) {
                    ctx.save();
                    ctx.translate(px + pw, py);
                    ctx.scale(-1, 1);
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
            item.className = `layer-item ${part.key === this.selectedPartKey ? 'active' : ''}`;

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
                part.visible = !part.visible;
                dirData[part.key].visible = part.visible;
                this.renderAll();
            });
            controls.appendChild(btnVis);

            item.appendChild(thumbWrap);
            item.appendChild(info);
            item.appendChild(controls);

            item.addEventListener('click', () => this.selectPart(part.key));
            item.addEventListener('contextmenu', (e) => {
                e.preventDefault();
                this.selectPart(part.key);
                this.showContextMenu(e.clientX, e.clientY);
            });

            list.appendChild(item);
        });
    }

    renderInspector() {
        const body = document.getElementById('inspector-body');
        if (!body) return;

        const dirData = this.getCurrentDirData();
        const part = dirData && this.selectedPartKey ? dirData[this.selectedPartKey] : null;

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
                    <button class="btn btn-secondary" id="btn-flip-x" style="width:100%; justify-content:center;">
                        ${part.flip_x ? '↔️ Đang lật' : '➡️ Bình thường'}
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

        inX.addEventListener('change', () => { part.x = parseInt(inX.value) || 0; this.renderAll(); });
        inY.addEventListener('change', () => { part.y = parseInt(inY.value) || 0; this.renderAll(); });
        inZ.addEventListener('change', () => { part.z = parseInt(inZ.value) || 0; this.renderAll(); });

        document.getElementById('btn-x-minus').addEventListener('click', () => { part.x--; inX.value = part.x; this.renderAll(); });
        document.getElementById('btn-x-plus').addEventListener('click', () => { part.x++; inX.value = part.x; this.renderAll(); });
        document.getElementById('btn-y-minus').addEventListener('click', () => { part.y--; inY.value = part.y; this.renderAll(); });
        document.getElementById('btn-y-plus').addEventListener('click', () => { part.y++; inY.value = part.y; this.renderAll(); });
        document.getElementById('btn-z-minus').addEventListener('click', () => { part.z--; inZ.value = part.z; this.renderAll(); });
        document.getElementById('btn-z-plus').addEventListener('click', () => { part.z++; inZ.value = part.z; this.renderAll(); });

        document.getElementById('btn-flip-x').addEventListener('click', () => {
            part.flip_x = !part.flip_x;
            this.renderAll();
        });

        document.getElementById('select-part-file').addEventListener('change', (e) => {
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
                this.invalidatePartCache(this.peCurrentPartName);
                this.showToast(`✅ ${data.message}`);
                this.switchTab('assembler');
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
                dirData[this.selectedPartKey].z = (dirData[this.selectedPartKey].z || 10) + 1;
                this.renderAll();
            }
        });

        document.getElementById('ctx-move-down').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            if (dirData && this.selectedPartKey) {
                dirData[this.selectedPartKey].z = Math.max(0, (dirData[this.selectedPartKey].z || 10) - 1);
                this.renderAll();
            }
        });

        document.getElementById('ctx-toggle-vis').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            if (dirData && this.selectedPartKey) {
                dirData[this.selectedPartKey].visible = !dirData[this.selectedPartKey].visible;
                this.renderAll();
            }
        });

        document.getElementById('ctx-flip-x').addEventListener('click', () => {
            this.hideContextMenu();
            const dirData = this.getCurrentDirData();
            if (dirData && this.selectedPartKey) {
                dirData[this.selectedPartKey].flip_x = !dirData[this.selectedPartKey].flip_x;
                this.renderAll();
            }
        });

        document.getElementById('ctx-reset-pos').addEventListener('click', () => {
            this.hideContextMenu();
            this.resetCurrentDir();
        });

        // Keyboard Nudging
        window.addEventListener('keydown', (e) => this.onKeyDown(e));

        // Save Button (Unity Export)
        document.getElementById('btn-save-unity').addEventListener('click', () => this.saveAndExport());
        document.getElementById('btn-export-json').addEventListener('click', () => this.exportJson());
        document.getElementById('file-import-json').addEventListener('change', (e) => this.importJson(e));
        document.getElementById('btn-reset-default').addEventListener('click', () => this.resetDefault());
        document.getElementById('btn-reset-current-dir').addEventListener('click', () => this.resetCurrentDir());

        // Symmetry & Alignment Tools
        const symCenter = document.getElementById('btn-sym-center-x');
        if (symCenter) symCenter.addEventListener('click', () => this.symmetryCenterX());
        const symMirror = document.getElementById('btn-sym-mirror-x');
        if (symMirror) symMirror.addEventListener('click', () => this.symmetryMirrorX());
        const symArms = document.getElementById('btn-sym-arms');
        if (symArms) symArms.addEventListener('click', () => this.symmetryArms());
        const symLegs = document.getElementById('btn-sym-legs');
        if (symLegs) symLegs.addEventListener('click', () => this.symmetryLegs());
        const symOpposite = document.getElementById('btn-sym-copy-opposite');
        if (symOpposite) symOpposite.addEventListener('click', () => this.symmetryCopyOpposite());
    }

    // ============================================================
    // SYMMETRY & ALIGNMENT METHODS
    // ============================================================
    symmetryCenterX() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !this.selectedPartKey) return;
        const part = dirData[this.selectedPartKey];
        const img = this.getImage(part.file);
        const w = img && img.complete ? img.naturalWidth : 16;
        part.x = Math.round(32 - w / 2);
        this.renderAll();
        this.showToast(`🎯 Đã căn giữa ${PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey} theo trục X = 32!`);
    }

    symmetryMirrorX() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !this.selectedPartKey) return;
        const part = dirData[this.selectedPartKey];
        const img = this.getImage(part.file);
        const w = img && img.complete ? img.naturalWidth : 16;
        part.x = 64 - part.x - w;
        part.flip_x = !part.flip_x;
        this.renderAll();
        this.showToast(`🪞 Đã lật đối xứng ${PART_NAMES_VI[this.selectedPartKey] || this.selectedPartKey} qua tâm!`);
    }

    symmetryArms() {
        const dirData = this.getCurrentDirData();
        if (!dirData || !dirData['idle_arm_left'] || !dirData['idle_arm_right']) return;
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

    onMouseDown(e) {
        if (e.button !== 0) return; // Only left click for dragging
        this.hideContextMenu();

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

        if (hitKey) {
            this.selectPart(hitKey);
            this.isDragging = true;
            this.dragStartX = pixelX;
            this.dragStartY = pixelY;
            this.dragStartPartX = dirData[hitKey].x;
            this.dragStartPartY = dirData[hitKey].y;
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
        if (this.currentTab !== 'assembler') return;

        const rect = this.canvas.getBoundingClientRect();
        const clientX = e.clientX - rect.left;
        const clientY = e.clientY - rect.top;

        const pixelX = Math.floor(clientX / this.zoom);
        const pixelY = Math.floor(clientY / this.zoom);

        if (this.isDragging && this.selectedPartKey) {
            const dirData = this.getCurrentDirData();
            const part = dirData[this.selectedPartKey];
            if (part) {
                const dx = pixelX - this.dragStartX;
                const dy = pixelY - this.dragStartY;
                part.x = this.dragStartPartX + dx;
                part.y = this.dragStartPartY + dy;
                this.renderAll();
            }
        }
    }

    onMouseUp() {
        this.isDragging = false;
    }

    onKeyDown(e) {
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'SELECT') return;

        if (this.currentTab === 'assembler') {
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
                e.preventDefault();
                this.saveAndExport();
                return;
            }

            const dirData = this.getCurrentDirData();
            if (!dirData || !this.selectedPartKey) return;
            const part = dirData[this.selectedPartKey];
            if (!part) return;

            const step = e.shiftKey ? 5 : 1;
            let handled = false;

            if (e.key === 'ArrowLeft' || e.key.toLowerCase() === 'a') {
                part.x -= step; handled = true;
            } else if (e.key === 'ArrowRight' || e.key.toLowerCase() === 'd') {
                part.x += step; handled = true;
            } else if (e.key === 'ArrowUp' || e.key.toLowerCase() === 'w') {
                part.y -= step; handled = true;
            } else if (e.key === 'ArrowDown' || e.key.toLowerCase() === 's') {
                part.y += step; handled = true;
            } else if (e.key === 'PageUp') {
                part.z = (part.z || 10) + 1; handled = true;
            } else if (e.key === 'PageDown') {
                part.z = Math.max(0, (part.z || 10) - 1); handled = true;
            }

            if (handled) {
                e.preventDefault();
                this.renderAll();
            }
        } else if (this.currentTab === 'pixel') {
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'z') {
                e.preventDefault();
                this.peUndo();
            } else if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'y') {
                e.preventDefault();
                this.peRedo();
            } else if (e.key.toLowerCase() === 'b') {
                document.getElementById('tool-pencil').click();
            } else if (e.key.toLowerCase() === 'e') {
                document.getElementById('tool-eraser').click();
            } else if (e.key.toLowerCase() === 'g') {
                document.getElementById('tool-bucket').click();
            } else if (e.key.toLowerCase() === 'i') {
                document.getElementById('tool-eyedropper').click();
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
