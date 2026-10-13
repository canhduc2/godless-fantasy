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

        // Pixel Rulers
        this.assemblyFrame = document.getElementById('canvas-assembly-frame');
        this.rulerCorner = document.getElementById('ruler-corner');
        this.rulerX = document.getElementById('ruler-x');
        this.rulerY = document.getElementById('ruler-y');
        this.showRulers = true;
        this.lastRulerCursorX = -1;
        this.lastRulerCursorY = -1;

        // Snap & Guides
        this.snapEnabled = true;
        this.activeGuides = [];

        // Assembler History (Undo / Redo)
        this.assemblerHistory = [];
        this.assemblerRedoStack = [];

        // Color Match / Harmonization State
        this.isColorMatchPicking = false;
        this.colorMatchSourcePartKey = null;
        this.colorHarmonizeHistory = [];

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
        this.peSelectedPixels = new Set();
        this.peIsSelecting = false;
        this.peMarqueeStart = null;
        this.peMarqueeCurrent = null;

        // Animation Workshop State
        this.animConfig = null;
        this.currentAnimState = 'idle';
        this.animZoom = 8;
        this.isAnimPlaying = true;
        this.animPlaybackSpeed = 1.0;
        this.animTimer = 0;
        this.lastAnimFrameTime = performance.now();
        this.animCanvas = document.getElementById('anim-canvas');
        this.animCtx = this.animCanvas ? this.animCanvas.getContext('2d') : null;
        if (this.animCtx) this.animCtx.imageSmoothingEnabled = false;
        this.showAnimShadow = true;
        this.showAnimPivots = false;

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
    // PIXEL RULERS (TOP X & LEFT Y)
    // ============================================================
    drawRulers(cursorX = undefined, cursorY = undefined) {
        if (!this.rulerX || !this.rulerY || !this.showRulers) return;
        if (cursorX === -1 && cursorY === -1) {
            this.lastRulerCursorX = -1;
            this.lastRulerCursorY = -1;
        } else if (cursorX !== undefined) {
            this.lastRulerCursorX = cursorX;
            this.lastRulerCursorY = cursorY;
        }

        const curX = this.lastRulerCursorX;
        const curY = this.lastRulerCursorY;

        const size = 64 * this.zoom;
        const rxCtx = this.rulerX.getContext('2d');
        const ryCtx = this.rulerY.getContext('2d');
        if (!rxCtx || !ryCtx) return;

        // Selected part bounds for range highlight
        const dirData = this.getCurrentDirData();
        let selRangeX = null, selRangeY = null;
        if (dirData && this.selectedPartKeys.size > 0) {
            let minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;
            for (const k of this.selectedPartKeys) {
                const p = dirData[k];
                if (!p || p.visible === false) continue;
                const img = this.getImage(p.file);
                const pw = img && img.complete ? img.naturalWidth : 16;
                const ph = img && img.complete ? img.naturalHeight : 16;
                minX = Math.min(minX, p.x);
                maxX = Math.max(maxX, p.x + pw);
                minY = Math.min(minY, p.y);
                maxY = Math.max(maxY, p.y + ph);
            }
            if (minX !== Infinity) {
                selRangeX = { min: minX, max: maxX };
                selRangeY = { min: minY, max: maxY };
            }
        }

        // ──────────────────────────────────────────
        // 1. TOP RULER (X-AXIS)
        // ──────────────────────────────────────────
        rxCtx.clearRect(0, 0, size, 22);
        rxCtx.fillStyle = '#16171a';
        rxCtx.fillRect(0, 0, size, 22);

        // Highlight selected part range on X
        if (selRangeX) {
            rxCtx.fillStyle = 'rgba(59, 130, 246, 0.22)';
            rxCtx.fillRect(selRangeX.min * this.zoom, 0, (selRangeX.max - selRangeX.min) * this.zoom, 22);
        }

        rxCtx.font = '9px "Inter", -apple-system, BlinkMacSystemFont, sans-serif';
        rxCtx.textAlign = 'center';
        rxCtx.textBaseline = 'top';

        for (let p = 0; p <= 64; p++) {
            const sx = Math.floor(p * this.zoom);
            const isCenter = (p === 32);
            const isMajor = (p % 8 === 0);
            const isMid = (p % 4 === 0);

            if (isCenter) {
                // Center Axis X = 32 Highlight (Cyan neon)
                rxCtx.fillStyle = '#38bdf8';
                rxCtx.fillRect(sx, 0, 1, 22);
                rxCtx.fillStyle = '#38bdf8';
                rxCtx.fillText('32', sx, 2);
            } else if (isMajor) {
                rxCtx.strokeStyle = '#71717a';
                rxCtx.lineWidth = 1;
                rxCtx.beginPath();
                rxCtx.moveTo(sx + 0.5, 12);
                rxCtx.lineTo(sx + 0.5, 22);
                rxCtx.stroke();

                rxCtx.fillStyle = '#a1a1aa';
                let labelX = sx;
                if (p === 0) labelX = sx + 7;
                else if (p === 64) labelX = sx - 7;
                rxCtx.fillText(p.toString(), labelX, 2);
            } else if (isMid) {
                rxCtx.strokeStyle = '#52525b';
                rxCtx.lineWidth = 1;
                rxCtx.beginPath();
                rxCtx.moveTo(sx + 0.5, 16);
                rxCtx.lineTo(sx + 0.5, 22);
                rxCtx.stroke();
            } else if (this.zoom >= 8) {
                // Minor tick every 1px
                rxCtx.strokeStyle = '#3f3f46';
                rxCtx.lineWidth = 1;
                rxCtx.beginPath();
                rxCtx.moveTo(sx + 0.5, 19);
                rxCtx.lineTo(sx + 0.5, 22);
                rxCtx.stroke();
            }
        }

        // Cursor tracker on X (aligned 100% to ruler tick mark at curX * this.zoom)
        if (curX >= 0 && curX <= 64) {
            const cx = Math.floor(curX * this.zoom);
            // Subtle 1-pixel cell highlight band
            if (this.zoom >= 4) {
                rxCtx.fillStyle = 'rgba(245, 158, 11, 0.20)';
                rxCtx.fillRect(cx, 0, this.zoom, 22);
            }

            // Crisp vertical indicator line matching ruler tick
            rxCtx.fillStyle = '#f59e0b';
            rxCtx.fillRect(cx, 0, 1, 22);

            // Little top arrow pointer centered on tick
            rxCtx.beginPath();
            rxCtx.moveTo(cx - 3, 0);
            rxCtx.lineTo(cx + 4, 0);
            rxCtx.lineTo(cx + 0.5, 4.5);
            rxCtx.closePath();
            rxCtx.fill();
        }

        // ──────────────────────────────────────────
        // 2. LEFT RULER (Y-AXIS)
        // ──────────────────────────────────────────
        ryCtx.clearRect(0, 0, 22, size);
        ryCtx.fillStyle = '#16171a';
        ryCtx.fillRect(0, 0, 22, size);

        // Highlight selected part range on Y
        if (selRangeY) {
            ryCtx.fillStyle = 'rgba(59, 130, 246, 0.22)';
            ryCtx.fillRect(0, selRangeY.min * this.zoom, 22, (selRangeY.max - selRangeY.min) * this.zoom);
        }

        ryCtx.font = '9px "Inter", -apple-system, BlinkMacSystemFont, sans-serif';
        ryCtx.textAlign = 'right';
        ryCtx.textBaseline = 'middle';

        for (let p = 0; p <= 64; p++) {
            const sy = Math.floor(p * this.zoom);
            const isCenter = (p === 32);
            const isMajor = (p % 8 === 0);
            const isMid = (p % 4 === 0);

            if (isCenter) {
                // Center Axis Y = 32 Highlight (Cyan neon)
                ryCtx.fillStyle = '#38bdf8';
                ryCtx.fillRect(0, sy, 22, 1);
                ryCtx.fillStyle = '#38bdf8';
                ryCtx.fillText('32', 18, sy);
            } else if (isMajor) {
                ryCtx.strokeStyle = '#71717a';
                ryCtx.lineWidth = 1;
                ryCtx.beginPath();
                ryCtx.moveTo(12, sy + 0.5);
                ryCtx.lineTo(22, sy + 0.5);
                ryCtx.stroke();

                ryCtx.fillStyle = '#a1a1aa';
                let labelY = sy;
                if (p === 0) labelY = sy + 5;
                else if (p === 64) labelY = sy - 5;
                ryCtx.fillText(p.toString(), 11, labelY);
            } else if (isMid) {
                ryCtx.strokeStyle = '#52525b';
                ryCtx.lineWidth = 1;
                ryCtx.beginPath();
                ryCtx.moveTo(16, sy + 0.5);
                ryCtx.lineTo(22, sy + 0.5);
                ryCtx.stroke();
            } else if (this.zoom >= 8) {
                // Minor tick every 1px
                ryCtx.strokeStyle = '#3f3f46';
                ryCtx.lineWidth = 1;
                ryCtx.beginPath();
                ryCtx.moveTo(19, sy + 0.5);
                ryCtx.lineTo(22, sy + 0.5);
                ryCtx.stroke();
            }
        }

        // Cursor tracker on Y (aligned 100% to ruler tick mark at curY * this.zoom)
        if (curY >= 0 && curY <= 64) {
            const cy = Math.floor(curY * this.zoom);
            // Subtle 1-pixel cell highlight band
            if (this.zoom >= 4) {
                ryCtx.fillStyle = 'rgba(245, 158, 11, 0.20)';
                ryCtx.fillRect(0, cy, 22, this.zoom);
            }

            // Crisp horizontal indicator line matching ruler tick
            ryCtx.fillStyle = '#f59e0b';
            ryCtx.fillRect(0, cy, 22, 1);

            // Little left arrow pointer centered on tick
            ryCtx.beginPath();
            ryCtx.moveTo(0, cy - 3);
            ryCtx.lineTo(0, cy + 4);
            ryCtx.lineTo(4.5, cy + 0.5);
            ryCtx.closePath();
            ryCtx.fill();
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
        if (this.assemblyFrame) {
            this.assemblyFrame.style.transform = `translate(${this.panX}px, ${this.panY}px)`;
        } else {
            this.wrapper.style.transform = `translate(${this.panX}px, ${this.panY}px)`;
        }
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
        this.setupAnimEditor();
        await this.loadPartsList();
        await this.loadConfig();
        await this.loadAnimConfig();
        this.updateZoom();
        this.renderAll();
        this.startAnimLoop();
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
        } else if (tabName === 'anim') {
            document.getElementById('view-anim').classList.add('active');
            document.getElementById('tab-btn-anim').classList.add('active');
            document.getElementById('header-gender-selector').style.display = 'flex';
            this.onSwitchToAnimTab();
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
        this.drawRulers();
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

        if (this.isColorMatchPicking && this.colorMatchSourcePartKey) {
            const dirData = this.getCurrentDirData();
            const part = dirData ? dirData[this.colorMatchSourcePartKey] : null;
            if (part && part.visible) {
                const img = this.getImage(part.file);
                const pw = img && img.complete ? img.naturalWidth : 16;
                const ph = img && img.complete ? img.naturalHeight : 16;
                this.selectionBox.style.display = 'block';
                this.selectionBox.classList.add('color-match-source-box');
                this.selectionBox.style.left = `${part.x * this.zoom}px`;
                this.selectionBox.style.top = `${part.y * this.zoom}px`;
                this.selectionBox.style.width = `${pw * this.zoom}px`;
                this.selectionBox.style.height = `${ph * this.zoom}px`;
                if (coordInd) coordInd.innerText = `🎯 ĐANG CHỌN MẪU MÀU CHO: ${PART_NAMES_VI[this.colorMatchSourcePartKey] || this.colorMatchSourcePartKey} (Nhấp vào sprite mẫu)`;
                return;
            }
        } else {
            this.selectionBox.classList.remove('color-match-source-box');
        }

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
                if (this.isColorMatchPicking) {
                    e.stopPropagation();
                    if (part.key === this.colorMatchSourcePartKey) {
                        this.showToast('⚠️ Vui lòng nhấp vào một mảnh khác để lấy mẫu màu!', '#f59e0b');
                        return;
                    }
                    const sourceKey = this.colorMatchSourcePartKey;
                    this.endColorMatchPickMode();
                    this.applyColorHarmonization(sourceKey, part.key);
                    return;
                }

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

        if (this.rulerX) {
            this.rulerX.width = size;
            this.rulerX.height = 22;
            this.rulerX.style.width = `${size}px`;
            this.rulerX.style.height = '22px';
        }
        if (this.rulerY) {
            this.rulerY.width = 22;
            this.rulerY.height = size;
            this.rulerY.style.width = '22px';
            this.rulerY.style.height = `${size}px`;
        }

        this.updateSelectionBox();
        this.drawRulers();
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

        // Step Brightness Buttons
        document.querySelectorAll('.btn-step[data-step]').forEach(btn => {
            btn.addEventListener('click', () => {
                const step = parseFloat(btn.dataset.step);
                this.peApplyStepBrightness(step);
            });
        });

        // Color & Brightness Adjustment Sliders
        const sB = document.getElementById('slider-pe-brightness');
        const sT = document.getElementById('slider-pe-temp');
        const sH = document.getElementById('slider-pe-hue');
        const sS = document.getElementById('slider-pe-sat');
        const sC = document.getElementById('slider-pe-contrast');
        if (sB) sB.addEventListener('input', (e) => {
            const v = e.target.value;
            document.getElementById('val-pe-brightness').innerText = (v > 0 ? `+${v}%` : `${v}%`);
        });
        if (sT) sT.addEventListener('input', (e) => {
            const v = parseInt(e.target.value);
            const valEl = document.getElementById('val-pe-temp');
            if (valEl) {
                if (v > 0) {
                    valEl.innerText = `+${v} (Ấm 🔥)`;
                    valEl.style.color = '#f97316';
                } else if (v < 0) {
                    valEl.innerText = `${v} (Lạnh ❄️)`;
                    valEl.style.color = '#38bdf8';
                } else {
                    valEl.innerText = '0 (Chuẩn)';
                    valEl.style.color = '#38bdf8';
                }
            }
        });
        if (sH) sH.addEventListener('input', (e) => {
            const v = e.target.value;
            document.getElementById('val-pe-hue').innerText = (v > 0 ? `+${v}°` : `${v}°`);
        });
        if (sS) sS.addEventListener('input', (e) => {
            const v = e.target.value;
            document.getElementById('val-pe-sat').innerText = (v > 0 ? `+${v}%` : `${v}%`);
        });
        if (sC) sC.addEventListener('input', (e) => {
            const v = e.target.value;
            document.getElementById('val-pe-contrast').innerText = (v > 0 ? `+${v}%` : `${v}%`);
        });

        // Quick Temperature step buttons
        const btnTempCool = document.getElementById('btn-pe-temp-cool');
        if (btnTempCool) btnTempCool.addEventListener('click', () => {
            const step = parseFloat(btnTempCool.dataset.temp) || -0.10;
            this.peApplyStepTemperature(step);
        });

        const btnTempWarm = document.getElementById('btn-pe-temp-warm');
        if (btnTempWarm) btnTempWarm.addEventListener('click', () => {
            const step = parseFloat(btnTempWarm.dataset.temp) || 0.10;
            this.peApplyStepTemperature(step);
        });

        const btnApplySliders = document.getElementById('btn-pe-apply-sliders');
        if (btnApplySliders) btnApplySliders.addEventListener('click', () => this.peApplyAdjustSliders());

        const btnResetSliders = document.getElementById('btn-pe-reset-sliders');
        if (btnResetSliders) btnResetSliders.addEventListener('click', () => this.peResetAdjustSliders());

        const btnTintColor = document.getElementById('btn-pe-tint-color');
        if (btnTintColor) btnTintColor.addEventListener('click', () => this.peApplyColorTint());

        const btnPeColorMatch = document.getElementById('btn-pe-color-match');
        if (btnPeColorMatch) btnPeColorMatch.addEventListener('click', () => this.promptPeColorMatch());

        // Selection Action Buttons
        const btnSelAll = document.getElementById('btn-pe-select-all');
        if (btnSelAll) btnSelAll.addEventListener('click', () => this.peSelectAll());

        const btnDeselect = document.getElementById('btn-pe-deselect');
        if (btnDeselect) btnDeselect.addEventListener('click', () => this.peDeselect());

        const btnInvertSel = document.getElementById('btn-pe-invert-sel');
        if (btnInvertSel) btnInvertSel.addEventListener('click', () => this.peInvertSelection());

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

    rgbToHsl(r, g, b) {
        r /= 255; g /= 255; b /= 255;
        const max = Math.max(r, g, b), min = Math.min(r, g, b);
        let h, s, l = (max + min) / 2;

        if (max === min) {
            h = s = 0;
        } else {
            const d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            switch (max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }
            h /= 6;
        }
        return [h, s, l];
    }

    hslToRgb(h, s, l) {
        let r, g, b;
        if (s === 0) {
            r = g = b = l;
        } else {
            const hue2rgb = (p, q, t) => {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1/6) return p + (q - p) * 6 * t;
                if (t < 1/2) return q;
                if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
                return p;
            };
            const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            const p = 2 * l - q;
            r = hue2rgb(p, q, h + 1/3);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1/3);
        }
        return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
    }

    hexToHsl(hex) {
        const c = parseInt(hex.replace('#', ''), 16);
        const r = (c >> 16) & 255;
        const g = (c >> 8) & 255;
        const b = c & 255;
        return this.rgbToHsl(r, g, b);
    }

    // ============================================================
    // LAB / XYZ / RGB COLOR ENGINE (FOR COLOR MATCHING & HARMONIZATION)
    // ============================================================
    rgbToXyz(r, g, b) {
        const toLin = (c) => {
            const v = c / 255;
            return v > 0.04045 ? Math.pow((v + 0.055) / 1.055, 2.4) : v / 12.92;
        };
        const rL = toLin(r);
        const gL = toLin(g);
        const bL = toLin(b);
        const x = rL * 0.4124564 + gL * 0.3575761 + bL * 0.1804375;
        const y = rL * 0.2126729 + gL * 0.7151522 + bL * 0.0721750;
        const z = rL * 0.0193339 + gL * 0.1191920 + bL * 0.9503041;
        return [x, y, z];
    }

    xyzToLab(x, y, z) {
        const xn = x / 0.95047;
        const yn = y / 1.00000;
        const zn = z / 1.08883;
        const delta = 6.0 / 29.0;
        const f = (t) => t > delta * delta * delta ? Math.cbrt(t) : (t / (3.0 * delta * delta)) + 4.0 / 29.0;
        const fx = f(xn);
        const fy = f(yn);
        const fz = f(zn);
        const L = 116.0 * fy - 16.0;
        const a = 500.0 * (fx - fy);
        const b = 200.0 * (fy - fz);
        return [L, a, b];
    }

    rgbToLab(r, g, b) {
        const [x, y, z] = this.rgbToXyz(r, g, b);
        return this.xyzToLab(x, y, z);
    }

    labToXyz(L, a, b) {
        const fy = (L + 16.0) / 116.0;
        const fx = fy + a / 500.0;
        const fz = fy - b / 200.0;
        const delta = 6.0 / 29.0;
        const finv = (t) => t > delta ? t * t * t : 3.0 * delta * delta * (t - 4.0 / 29.0);
        const x = finv(fx) * 0.95047;
        const y = finv(fy) * 1.00000;
        const z = finv(fz) * 1.08883;
        return [x, y, z];
    }

    xyzToRgb(x, y, z) {
        const rL = x * 3.2404542 + y * -1.5371385 + z * -0.4985314;
        const gL = x * -0.9692660 + y * 1.8760108 + z * 0.0415560;
        const bL = x * 0.0556434 + y * -0.2040259 + z * 1.0572252;
        const toSrgb = (c) => {
            const v = Math.max(0, Math.min(1, c));
            return v > 0.0031308 ? 1.055 * Math.pow(v, 1.0 / 2.4) - 0.055 : 12.92 * v;
        };
        return [
            Math.round(toSrgb(rL) * 255),
            Math.round(toSrgb(gL) * 255),
            Math.round(toSrgb(bL) * 255)
        ];
    }

    labToRgb(L, a, b) {
        const [x, y, z] = this.labToXyz(L, a, b);
        return this.xyzToRgb(x, y, z);
    }

    harmonizeImageData(sourceImgData, targetImgData, selectedPixelsSet = null, sourceWidth = 0) {
        const sD = sourceImgData.data;
        const tD = targetImgData.data;

        // 1. Gather Target Lab statistics
        const tLabs = [];
        for (let i = 0; i < tD.length; i += 4) {
            if (tD[i + 3] > 40) {
                const lab = this.rgbToLab(tD[i], tD[i + 1], tD[i + 2]);
                if (lab[0] >= 16) {
                    tLabs.push(lab);
                }
            }
        }
        if (tLabs.length < 5) {
            for (let i = 0; i < tD.length; i += 4) {
                if (tD[i + 3] > 40) {
                    tLabs.push(this.rgbToLab(tD[i], tD[i + 1], tD[i + 2]));
                }
            }
        }
        if (tLabs.length === 0) return 0;

        let meanLt = 0, meanAt = 0, meanBt = 0;
        for (const lab of tLabs) {
            meanLt += lab[0];
            meanAt += lab[1];
            meanBt += lab[2];
        }
        meanLt /= tLabs.length;
        meanAt /= tLabs.length;
        meanBt /= tLabs.length;

        let varLt = 0, varAt = 0, varBt = 0;
        for (const lab of tLabs) {
            varLt += (lab[0] - meanLt) * (lab[0] - meanLt);
            varAt += (lab[1] - meanAt) * (lab[1] - meanAt);
            varBt += (lab[2] - meanBt) * (lab[2] - meanBt);
        }
        const stdLt = Math.sqrt(varLt / tLabs.length) || 1;
        const stdAt = Math.sqrt(varAt / tLabs.length) || 1;
        const stdBt = Math.sqrt(varBt / tLabs.length) || 1;

        // 2. Gather Source Lab statistics
        const sLabs = [];
        for (let i = 0; i < sD.length; i += 4) {
            if (sD[i + 3] > 40) {
                const lab = this.rgbToLab(sD[i], sD[i + 1], sD[i + 2]);
                if (lab[0] >= 16) {
                    sLabs.push(lab);
                }
            }
        }
        if (sLabs.length < 5) {
            for (let i = 0; i < sD.length; i += 4) {
                if (sD[i + 3] > 40) {
                    sLabs.push(this.rgbToLab(sD[i], sD[i + 1], sD[i + 2]));
                }
            }
        }
        if (sLabs.length === 0) return 0;

        let meanLs = 0, meanAs = 0, meanBs = 0;
        for (const lab of sLabs) {
            meanLs += lab[0];
            meanAs += lab[1];
            meanBs += lab[2];
        }
        meanLs /= sLabs.length;
        meanAs /= sLabs.length;
        meanBs /= sLabs.length;

        let varLs = 0, varAs = 0, varBs = 0;
        for (const lab of sLabs) {
            varLs += (lab[0] - meanLs) * (lab[0] - meanLs);
            varAs += (lab[1] - meanAs) * (lab[1] - meanAs);
            varBs += (lab[2] - meanBs) * (lab[2] - meanBs);
        }
        const stdLs = Math.sqrt(varLs / sLabs.length) || 1;
        const stdAs = Math.sqrt(varAs / sLabs.length) || 1;
        const stdBs = Math.sqrt(varBs / sLabs.length) || 1;

        const scaleL = Math.min(2.2, Math.max(0.45, stdLt / (stdLs + 0.001)));
        const scaleA = Math.min(2.2, Math.max(0.45, stdAt / (stdAs + 0.001)));
        const scaleB = Math.min(2.2, Math.max(0.45, stdBt / (stdBs + 0.001)));

        // 3. Reinhard Transfer
        let modifiedCount = 0;
        const hasSelection = selectedPixelsSet && selectedPixelsSet.size > 0 && sourceWidth > 0;

        for (let i = 0; i < sD.length; i += 4) {
            if (sD[i + 3] <= 10) continue;

            if (hasSelection) {
                const pxIdx = i / 4;
                const px = pxIdx % sourceWidth;
                const py = Math.floor(pxIdx / sourceWidth);
                if (!selectedPixelsSet.has(`${px},${py}`)) continue;
            }

            const [curL, curA, curB] = this.rgbToLab(sD[i], sD[i + 1], sD[i + 2]);

            let newL = meanLt + (curL - meanLs) * scaleL;
            let newA = meanAt + (curA - meanAs) * scaleA;
            let newB = meanBt + (curB - meanBs) * scaleB;
            newL = Math.max(0, Math.min(100, newL));

            let finL = newL;
            let finA = newA;
            let finB = newB;
            if (curL < 18) {
                const w = Math.max(0, Math.min(1, curL / 18));
                finL = (1 - w) * curL + w * newL;
                finA = (1 - w) * curA + w * newA;
                finB = (1 - w) * curB + w * newB;
            }

            const [finR, finG, finB_rgb] = this.labToRgb(finL, finA, finB);
            sD[i] = finR;
            sD[i + 1] = finG;
            sD[i + 2] = finB_rgb;
            modifiedCount++;
        }

        return modifiedCount;
    }

    startColorMatchPickMode(sourceKey) {
        if (!sourceKey) return;
        this.isColorMatchPicking = true;
        this.colorMatchSourcePartKey = sourceKey;

        const banner = document.getElementById('color-match-banner');
        const nameEl = document.getElementById('cmb-source-name');
        if (banner) banner.style.display = 'flex';
        if (nameEl) nameEl.innerText = PART_NAMES_VI[sourceKey] || sourceKey;

        const frame = document.getElementById('canvas-assembly-frame');
        if (frame) frame.classList.add('picking-color-match');

        this.updateSelectionBox();
        this.showToast(`🎯 Đang căn màu cho ${PART_NAMES_VI[sourceKey] || sourceKey}. Nhấp vào sprite mẫu trên màn hình hoặc Layers!`, '#38bdf8');
    }

    endColorMatchPickMode() {
        this.isColorMatchPicking = false;
        this.colorMatchSourcePartKey = null;

        const banner = document.getElementById('color-match-banner');
        if (banner) banner.style.display = 'none';

        const frame = document.getElementById('canvas-assembly-frame');
        if (frame) frame.classList.remove('picking-color-match');

        this.updateSelectionBox();
    }

    async applyColorHarmonization(sourceKey, targetKey) {
        const dirData = this.getCurrentDirData();
        if (!dirData) return;

        const sourcePart = dirData[sourceKey];
        const targetPart = dirData[targetKey];
        if (!sourcePart || !targetPart) {
            this.showToast('❌ Không tìm thấy thông tin bộ phận!', '#ef4444');
            return;
        }

        const sourceNameVi = PART_NAMES_VI[sourceKey] || sourceKey;
        const targetNameVi = PART_NAMES_VI[targetKey] || targetKey;

        this.showToast(`⏳ Đang đồng bộ phong cách màu từ "${targetNameVi}" sang "${sourceNameVi}"...`, '#38bdf8');

        try {
            const ensureLoaded = (file) => new Promise((resolve, reject) => {
                const img = this.getImage(file);
                if (img && img.complete && img.naturalWidth > 0) return resolve(img);
                const temp = new Image();
                temp.crossOrigin = 'anonymous';
                temp.onload = () => resolve(temp);
                temp.onerror = () => reject(new Error(`Không thể nạp ảnh: ${file}`));
                temp.src = `/parts/${file}?t=${Date.now()}`;
            });

            const [sourceImg, targetImg] = await Promise.all([
                ensureLoaded(sourcePart.file),
                ensureLoaded(targetPart.file)
            ]);

            const sCanvas = document.createElement('canvas');
            sCanvas.width = sourceImg.naturalWidth;
            sCanvas.height = sourceImg.naturalHeight;
            const sCtx = sCanvas.getContext('2d');
            sCtx.drawImage(sourceImg, 0, 0);
            const sImgData = sCtx.getImageData(0, 0, sCanvas.width, sCanvas.height);

            const tCanvas = document.createElement('canvas');
            tCanvas.width = targetImg.naturalWidth;
            tCanvas.height = targetImg.naturalHeight;
            const tCtx = tCanvas.getContext('2d');
            tCtx.drawImage(targetImg, 0, 0);
            const tImgData = tCtx.getImageData(0, 0, tCanvas.width, tCanvas.height);

            const count = this.harmonizeImageData(sImgData, tImgData);
            if (count === 0) {
                this.showToast('⚠️ Không tìm thấy pixel hợp lệ để đồng bộ màu!', '#f59e0b');
                return;
            }

            sCtx.putImageData(sImgData, 0, 0);
            const newPngDataUrl = sCanvas.toDataURL('image/png');

            const res = await fetch('/api/save_part', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    part_name: sourcePart.file,
                    image_data: newPngDataUrl
                })
            });

            const result = await res.json();
            if (result.success) {
                this.invalidatePartCache(sourcePart.file);
                const refreshedImg = new Image();
                refreshedImg.src = `/parts/${sourcePart.file}?t=${Date.now()}`;
                await new Promise((res) => { refreshedImg.onload = res; refreshedImg.onerror = res; });
                this.imageCache.set(sourcePart.file, refreshedImg);

                this.renderAll();
                this.showToast(`✨ Đã đồng bộ màu cho "${sourceNameVi}" theo phong cách của "${targetNameVi}" (${count} px)!`, '#10b981');
            } else {
                this.showToast(`❌ Lỗi lưu ảnh: ${result.error}`, '#ef4444');
            }
        } catch (err) {
            console.error('Harmonization error:', err);
            this.showToast(`❌ Lỗi khi đồng bộ màu: ${err.message}`, '#ef4444');
        }
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

        this.peSelectedPixels = new Set();
        this.peIsSelecting = false;
        this.peMarqueeStart = null;
        this.peMarqueeCurrent = null;
        this.updatePeSelectionBadge();
        this.peResetAdjustSliders();

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
        const { x, y } = this.getPeCanvasCoords(e);

        if (this.peTool === 'select-rect') {
            this.peIsSelecting = true;
            this.peMarqueeStart = { x, y };
            this.peMarqueeCurrent = { x, y };
            if (!e.shiftKey) {
                this.peSelectedPixels.clear();
            }
            this.drawPixelCursor(x, y);
            return;
        }

        if (this.peTool === 'wand') {
            this.peSelectMagicWand(x, y, e.shiftKey);
            return;
        }

        this.peIsDrawing = true;
        this.pushPeHistory();
        this.applyPeTool(x, y);
    }

    onPeMouseMove(e) {
        if (this.currentTab !== 'pixel') return;
        const { x, y } = this.getPeCanvasCoords(e);

        document.getElementById('pe-coord').innerText = `Toạ độ: X: ${x}, Y: ${y}`;

        if (this.peIsSelecting) {
            this.peMarqueeCurrent = { x, y };
            this.drawPixelCursor(x, y);
            return;
        }

        this.drawPixelCursor(x, y);

        if (this.peIsDrawing) {
            if (x !== this.peLastDrawX || y !== this.peLastDrawY) {
                this.applyPeTool(x, y);
            }
        }
    }

    onPeMouseUp() {
        if (this.peIsSelecting) {
            this.peIsSelecting = false;
            if (this.peMarqueeStart && this.peMarqueeCurrent) {
                const minX = Math.min(this.peMarqueeStart.x, this.peMarqueeCurrent.x);
                const maxX = Math.max(this.peMarqueeStart.x, this.peMarqueeCurrent.x);
                const minY = Math.min(this.peMarqueeStart.y, this.peMarqueeCurrent.y);
                const maxY = Math.max(this.peMarqueeStart.y, this.peMarqueeCurrent.y);

                for (let py = minY; py <= maxY; py++) {
                    for (let px = minX; px <= maxX; px++) {
                        this.peSelectedPixels.add(`${px},${py}`);
                    }
                }
            }
            this.peMarqueeStart = null;
            this.peMarqueeCurrent = null;
            this.updatePeSelectionBadge();
            this.drawPixelCursor();
        }

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
        const hasSelection = this.peSelectedPixels && this.peSelectedPixels.size > 0;

        if (this.peTool === 'pencil') {
            for (let ox = 0; ox < this.peBrushSize; ox++) {
                for (let oy = 0; oy < this.peBrushSize; oy++) {
                    const tx = x + ox, ty = y + oy;
                    if (tx >= 0 && tx < this.peCanvas.width && ty >= 0 && ty < this.peCanvas.height) {
                        if (!hasSelection || this.peSelectedPixels.has(`${tx},${ty}`)) {
                            this.peCtx.fillStyle = this.peColor;
                            this.peCtx.fillRect(tx, ty, 1, 1);
                        }
                    }
                }
            }
        } else if (this.peTool === 'eraser') {
            for (let ox = 0; ox < this.peBrushSize; ox++) {
                for (let oy = 0; oy < this.peBrushSize; oy++) {
                    const tx = x + ox, ty = y + oy;
                    if (tx >= 0 && tx < this.peCanvas.width && ty >= 0 && ty < this.peCanvas.height) {
                        if (!hasSelection || this.peSelectedPixels.has(`${tx},${ty}`)) {
                            this.peCtx.clearRect(tx, ty, 1, 1);
                        }
                    }
                }
            }
        } else if (this.peTool === 'dodge') {
            for (let ox = 0; ox < this.peBrushSize; ox++) {
                for (let oy = 0; oy < this.peBrushSize; oy++) {
                    const tx = x + ox, ty = y + oy;
                    if (tx >= 0 && tx < this.peCanvas.width && ty >= 0 && ty < this.peCanvas.height) {
                        if (!hasSelection || this.peSelectedPixels.has(`${tx},${ty}`)) {
                            this.peAdjustPixelLightness(tx, ty, 0.08);
                        }
                    }
                }
            }
        } else if (this.peTool === 'burn') {
            for (let ox = 0; ox < this.peBrushSize; ox++) {
                for (let oy = 0; oy < this.peBrushSize; oy++) {
                    const tx = x + ox, ty = y + oy;
                    if (tx >= 0 && tx < this.peCanvas.width && ty >= 0 && ty < this.peCanvas.height) {
                        if (!hasSelection || this.peSelectedPixels.has(`${tx},${ty}`)) {
                            this.peAdjustPixelLightness(tx, ty, -0.08);
                        }
                    }
                }
            }
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

    peAdjustPixelLightness(x, y, delta) {
        const p = this.peCtx.getImageData(x, y, 1, 1);
        const d = p.data;
        if (d[3] > 0) {
            let [h, s, l] = this.rgbToHsl(d[0], d[1], d[2]);
            l = Math.max(0, Math.min(1, l + delta));
            const [nr, ng, nb] = this.hslToRgb(h, s, l);
            d[0] = nr;
            d[1] = ng;
            d[2] = nb;
            this.peCtx.putImageData(p, x, y);
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

        const hasSelection = this.peSelectedPixels && this.peSelectedPixels.size > 0;

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

            if (hasSelection && !this.peSelectedPixels.has(`${cx},${cy}`)) continue;

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

    peApplyStepBrightness(delta) {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        if (w === 0 || h === 0) return;

        this.pushPeHistory();
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const d = imgData.data;
        let count = 0;

        const hasSelection = this.peSelectedPixels && this.peSelectedPixels.size > 0;

        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                if (hasSelection && !this.peSelectedPixels.has(`${x},${y}`)) continue;
                const idx = (y * w + x) * 4;
                if (d[idx + 3] > 0) {
                    let [ph, ps, pl] = this.rgbToHsl(d[idx], d[idx + 1], d[idx + 2]);
                    pl = Math.max(0, Math.min(1, pl + delta));
                    const [nr, ng, nb] = this.hslToRgb(ph, ps, pl);
                    d[idx] = nr;
                    d[idx + 1] = ng;
                    d[idx + 2] = nb;
                    count++;
                }
            }
        }

        this.peCtx.putImageData(imgData, 0, 0);
        const percentText = (delta > 0 ? `+${Math.round(delta * 100)}% Sáng` : `${Math.round(delta * 100)}% Tối`);
        this.showToast(`${delta > 0 ? '☀️' : '🌙'} Đã tăng/giảm ${percentText} (${count} px)`);
    }

    peApplyStepTemperature(delta) {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        if (w === 0 || h === 0) return;

        this.pushPeHistory();
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const d = imgData.data;
        let count = 0;

        const hasSelection = this.peSelectedPixels && this.peSelectedPixels.size > 0;

        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                if (hasSelection && !this.peSelectedPixels.has(`${x},${y}`)) continue;
                const idx = (y * w + x) * 4;
                if (d[idx + 3] > 0) {
                    let r = d[idx], g = d[idx + 1], b = d[idx + 2];
                    if (delta > 0) {
                        r = r + (255 - r) * delta * 0.45;
                        g = g + (255 - g) * delta * 0.15;
                        b = b * (1.0 - delta * 0.35);
                    } else {
                        const c = Math.abs(delta);
                        b = b + (255 - b) * c * 0.45;
                        g = g + (255 - g) * c * 0.08;
                        r = r * (1.0 - c * 0.35);
                    }
                    d[idx] = Math.round(Math.max(0, Math.min(255, r)));
                    d[idx + 1] = Math.round(Math.max(0, Math.min(255, g)));
                    d[idx + 2] = Math.round(Math.max(0, Math.min(255, b)));
                    count++;
                }
            }
        }

        this.peCtx.putImageData(imgData, 0, 0);
        const label = delta > 0 ? `🔥 +${Math.round(delta * 100)}% Tông Ấm` : `❄️ ${Math.round(delta * 100)}% Tông Lạnh`;
        this.showToast(`Nhiệt độ màu: ${label} (${count} px)`);
    }

    peApplyAdjustSliders() {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        if (w === 0 || h === 0) return;

        const brightVal = parseInt(document.getElementById('slider-pe-brightness').value) || 0;
        const tempVal = parseInt(document.getElementById('slider-pe-temp').value) || 0;
        const hueVal = parseInt(document.getElementById('slider-pe-hue').value) || 0;
        const satVal = parseInt(document.getElementById('slider-pe-sat').value) || 0;
        const contrastVal = parseInt(document.getElementById('slider-pe-contrast').value) || 0;

        if (brightVal === 0 && tempVal === 0 && hueVal === 0 && satVal === 0 && contrastVal === 0) {
            this.showToast('ℹ️ Các thanh trượt đang ở mức 0', '#6b7280');
            return;
        }

        this.pushPeHistory();
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const d = imgData.data;

        const bDelta = brightVal / 100;
        const hDelta = hueVal / 360;
        const sDelta = satVal / 100;
        const cFactor = 1 + (contrastVal / 100);

        const hasSelection = this.peSelectedPixels && this.peSelectedPixels.size > 0;
        let count = 0;

        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                if (hasSelection && !this.peSelectedPixels.has(`${x},${y}`)) continue;
                const idx = (y * w + x) * 4;
                if (d[idx + 3] > 0) {
                    let r = d[idx], g = d[idx + 1], b = d[idx + 2];

                    // 1. Color Temperature (Ấm / Lạnh)
                    if (tempVal !== 0) {
                        const tf = tempVal / 100;
                        if (tf > 0) {
                            r = r + (255 - r) * tf * 0.45;
                            g = g + (255 - g) * tf * 0.15;
                            b = b * (1.0 - tf * 0.35);
                        } else {
                            const c = Math.abs(tf);
                            b = b + (255 - b) * c * 0.45;
                            g = g + (255 - g) * c * 0.08;
                            r = r * (1.0 - c * 0.35);
                        }
                        r = Math.max(0, Math.min(255, r));
                        g = Math.max(0, Math.min(255, g));
                        b = Math.max(0, Math.min(255, b));
                    }

                    // 2. Contrast
                    if (contrastVal !== 0) {
                        r = Math.max(0, Math.min(255, ((r / 255 - 0.5) * cFactor + 0.5) * 255));
                        g = Math.max(0, Math.min(255, ((g / 255 - 0.5) * cFactor + 0.5) * 255));
                        b = Math.max(0, Math.min(255, ((b / 255 - 0.5) * cFactor + 0.5) * 255));
                    }

                    // 3. Hue, Saturation, Lightness
                    let [ph, ps, pl] = this.rgbToHsl(r, g, b);
                    if (hDelta !== 0) ph = (ph + hDelta + 1) % 1;
                    if (sDelta !== 0) ps = Math.max(0, Math.min(1, ps + sDelta));
                    if (bDelta !== 0) pl = Math.max(0, Math.min(1, pl + bDelta));
                    const [nr, ng, nb] = this.hslToRgb(ph, ps, pl);
                    d[idx] = nr;
                    d[idx + 1] = ng;
                    d[idx + 2] = nb;
                    count++;
                }
            }
        }

        this.peCtx.putImageData(imgData, 0, 0);
        this.peResetAdjustSliders();
        this.showToast(`✨ Đã áp dụng chỉnh màu (${count} pixel)`);
    }

    peResetAdjustSliders() {
        const sB = document.getElementById('slider-pe-brightness');
        const sT = document.getElementById('slider-pe-temp');
        const sH = document.getElementById('slider-pe-hue');
        const sS = document.getElementById('slider-pe-sat');
        const sC = document.getElementById('slider-pe-contrast');
        if (sB) sB.value = 0;
        if (sT) sT.value = 0;
        if (sH) sH.value = 0;
        if (sS) sS.value = 0;
        if (sC) sC.value = 0;
        const vB = document.getElementById('val-pe-brightness');
        const vT = document.getElementById('val-pe-temp');
        const vH = document.getElementById('val-pe-hue');
        const vS = document.getElementById('val-pe-sat');
        const vC = document.getElementById('val-pe-contrast');
        if (vB) vB.innerText = '0%';
        if (vT) {
            vT.innerText = '0 (Chuẩn)';
            vT.style.color = '#38bdf8';
        }
        if (vH) vH.innerText = '0°';
        if (vS) vS.innerText = '0%';
        if (vC) vC.innerText = '0%';
    }


    peApplyColorTint() {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        if (w === 0 || h === 0) return;

        this.pushPeHistory();
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const d = imgData.data;

        const [th, ts, _] = this.hexToHsl(this.peColor);
        const hasSelection = this.peSelectedPixels && this.peSelectedPixels.size > 0;
        let count = 0;

        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                if (hasSelection && !this.peSelectedPixels.has(`${x},${y}`)) continue;
                const idx = (y * w + x) * 4;
                if (d[idx + 3] > 0) {
                    const [_, __, pl] = this.rgbToHsl(d[idx], d[idx + 1], d[idx + 2]);
                    const [nr, ng, nb] = this.hslToRgb(th, ts, pl);
                    d[idx] = nr;
                    d[idx + 1] = ng;
                    d[idx + 2] = nb;
                    count++;
                }
            }
        }

        this.peCtx.putImageData(imgData, 0, 0);
        this.showToast(`🎭 Đã nhuộm màu ${this.peColor} (${count} px)`);
    }

    async promptPeColorMatch() {
        if (!this.peCurrentPartName) return;
        const dirData = this.getCurrentDirData();
        if (!dirData) return;

        const options = [];
        for (const key of PART_KEYS) {
            const part = dirData[key];
            if (part && part.file && part.file !== this.peCurrentPartName) {
                options.push({
                    key,
                    name: `${PART_NAMES_VI[key] || key} (${part.file})`,
                    file: part.file
                });
            }
        }

        if (options.length === 0) {
            this.showToast('⚠️ Không có mảnh mẫu nào khác để đồng bộ màu!', '#f59e0b');
            return;
        }

        const promptMsg = 'Chọn số thứ tự của mảnh mẫu để lấy phong cách màu:\n\n' +
            options.map((opt, idx) => `${idx + 1}. ${opt.name}`).join('\n');

        const ans = window.prompt(promptMsg, '1');
        if (!ans) return;
        const idx = parseInt(ans.trim(), 10) - 1;
        if (isNaN(idx) || idx < 0 || idx >= options.length) {
            this.showToast('⚠️ Lựa chọn không hợp lệ!', '#f59e0b');
            return;
        }

        const chosenTarget = options[idx];
        this.showToast(`⏳ Đang đồng bộ màu theo ${chosenTarget.name}...`, '#38bdf8');

        try {
            const targetImg = new Image();
            targetImg.crossOrigin = 'anonymous';
            await new Promise((res, rej) => {
                targetImg.onload = res;
                targetImg.onerror = () => rej(new Error(`Không thể nạp ảnh: ${chosenTarget.file}`));
                targetImg.src = `/parts/${chosenTarget.file}?t=${Date.now()}`;
            });

            const tCanvas = document.createElement('canvas');
            tCanvas.width = targetImg.naturalWidth;
            tCanvas.height = targetImg.naturalHeight;
            const tCtx = tCanvas.getContext('2d');
            tCtx.drawImage(targetImg, 0, 0);
            const tImgData = tCtx.getImageData(0, 0, tCanvas.width, tCanvas.height);

            const w = this.peCanvas.width;
            const h = this.peCanvas.height;
            const sImgData = this.peCtx.getImageData(0, 0, w, h);

            this.pushPeHistory();
            const count = this.harmonizeImageData(sImgData, tImgData, this.peSelectedPixels, w);

            this.peCtx.putImageData(sImgData, 0, 0);
            this.showToast(`✨ Đã đồng bộ màu theo "${chosenTarget.name}" (${count} px)!`, '#10b981');
        } catch (e) {
            console.error('PE Harmonization error:', e);
            this.showToast(`❌ Lỗi: ${e.message}`, '#ef4444');
        }
    }

    peSelectAll() {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const d = imgData.data;
        this.peSelectedPixels.clear();

        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                const idx = (y * w + x) * 4;
                if (d[idx + 3] > 0) {
                    this.peSelectedPixels.add(`${x},${y}`);
                }
            }
        }
        this.updatePeSelectionBadge();
        this.drawPixelCursor();
        this.showToast(`🔲 Đã chọn tất cả (${this.peSelectedPixels.size} px)`);
    }

    peDeselect() {
        if (this.peSelectedPixels.size > 0) {
            this.peSelectedPixels.clear();
            this.updatePeSelectionBadge();
            this.drawPixelCursor();
            this.showToast('❌ Đã bỏ vùng chọn');
        }
    }

    peInvertSelection() {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const d = imgData.data;

        const newSet = new Set();
        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                const idx = (y * w + x) * 4;
                if (d[idx + 3] > 0) {
                    const key = `${x},${y}`;
                    if (!this.peSelectedPixels.has(key)) {
                        newSet.add(key);
                    }
                }
            }
        }
        this.peSelectedPixels = newSet;
        this.updatePeSelectionBadge();
        this.drawPixelCursor();
        this.showToast(`🔄 Đã đảo vùng chọn (${this.peSelectedPixels.size} px)`);
    }

    peSelectMagicWand(startX, startY, isAdd = false) {
        const w = this.peCanvas.width;
        const h = this.peCanvas.height;
        const imgData = this.peCtx.getImageData(0, 0, w, h);
        const data = imgData.data;

        const startIdx = (startY * w + startX) * 4;
        const sr = data[startIdx];
        const sg = data[startIdx + 1];
        const sb = data[startIdx + 2];
        const sa = data[startIdx + 3];

        if (!isAdd) {
            this.peSelectedPixels.clear();
        }

        const match = (idx) => {
            return Math.abs(data[idx] - sr) <= 8 &&
                   Math.abs(data[idx + 1] - sg) <= 8 &&
                   Math.abs(data[idx + 2] - sb) <= 8 &&
                   Math.abs(data[idx + 3] - sa) <= 8;
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

            this.peSelectedPixels.add(`${cx},${cy}`);

            if (cx > 0) queue.push([cx - 1, cy]);
            if (cx < w - 1) queue.push([cx + 1, cy]);
            if (cy > 0) queue.push([cx, cy - 1]);
            if (cy < h - 1) queue.push([cx, cy + 1]);
        }

        this.updatePeSelectionBadge();
        this.drawPixelCursor();
        this.showToast(`🪄 Đũa thần: chọn ${this.peSelectedPixels.size} px`);
    }

    updatePeSelectionBadge() {
        const badge = document.getElementById('pe-selection-badge');
        const text = document.getElementById('pe-selection-text');
        const icon = document.getElementById('pe-selection-icon');
        if (!badge || !text) return;

        if (this.peSelectedPixels && this.peSelectedPixels.size > 0) {
            badge.classList.add('active');
            if (icon) icon.innerText = '🎯';
            text.innerText = `Đang chọn: ${this.peSelectedPixels.size} px`;
        } else {
            badge.classList.remove('active');
            if (icon) icon.innerText = '⭕';
            text.innerText = 'Chưa chọn (Áp dụng cả ảnh)';
        }
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

        // Active Selection Highlight
        if (this.peSelectedPixels && this.peSelectedPixels.size > 0) {
            this.peCursorCtx.fillStyle = 'rgba(56, 189, 248, 0.28)';
            this.peCursorCtx.strokeStyle = 'rgba(56, 189, 248, 0.65)';
            this.peCursorCtx.lineWidth = 0.05;
            for (const key of this.peSelectedPixels) {
                const [px, py] = key.split(',').map(Number);
                this.peCursorCtx.fillRect(px, py, 1, 1);
                this.peCursorCtx.strokeRect(px, py, 1, 1);
            }
        }

        // Active Marquee Drag Box
        if (this.peIsSelecting && this.peMarqueeStart && this.peMarqueeCurrent) {
            const minX = Math.min(this.peMarqueeStart.x, this.peMarqueeCurrent.x);
            const maxX = Math.max(this.peMarqueeStart.x, this.peMarqueeCurrent.x);
            const minY = Math.min(this.peMarqueeStart.y, this.peMarqueeCurrent.y);
            const maxY = Math.max(this.peMarqueeStart.y, this.peMarqueeCurrent.y);
            const boxW = maxX - minX + 1;
            const boxH = maxY - minY + 1;

            this.peCursorCtx.fillStyle = 'rgba(56, 189, 248, 0.22)';
            this.peCursorCtx.fillRect(minX, minY, boxW, boxH);
            this.peCursorCtx.strokeStyle = '#38bdf8';
            this.peCursorCtx.lineWidth = 0.1;
            this.peCursorCtx.strokeRect(minX, minY, boxW, boxH);
        }

        // Hover Brush Cursor
        if (hoverX >= 0 && hoverY >= 0) {
            if (this.peTool === 'dodge') {
                this.peCursorCtx.strokeStyle = '#fbbf24';
            } else if (this.peTool === 'burn') {
                this.peCursorCtx.strokeStyle = '#818cf8';
            } else if (this.peTool === 'select-rect') {
                this.peCursorCtx.strokeStyle = '#38bdf8';
            } else if (this.peTool === 'wand') {
                this.peCursorCtx.strokeStyle = '#f472b6';
            } else {
                this.peCursorCtx.strokeStyle = '#ffffff';
            }
            this.peCursorCtx.lineWidth = 0.1;
            const cursorSize = (this.peTool === 'select-rect' || this.peTool === 'wand' || this.peTool === 'bucket' || this.peTool === 'eyedropper') ? 1 : this.peBrushSize;
            this.peCursorCtx.strokeRect(hoverX, hoverY, cursorSize, cursorSize);
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
        const tabBtnAnim = document.getElementById('tab-btn-anim');
        if (tabBtnAnim) tabBtnAnim.addEventListener('click', () => this.switchTab('anim'));

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
        this.canvas.addEventListener('mouseleave', () => this.drawRulers(-1, -1));

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

        const ctxColorMatch = document.getElementById('ctx-color-match');
        if (ctxColorMatch) {
            ctxColorMatch.addEventListener('click', () => {
                this.hideContextMenu();
                if (this.selectedPartKey) {
                    this.startColorMatchPickMode(this.selectedPartKey);
                }
            });
        }

        const btnCancelColorMatch = document.getElementById('btn-cancel-color-match');
        if (btnCancelColorMatch) {
            btnCancelColorMatch.addEventListener('click', () => {
                this.endColorMatchPickMode();
                this.showToast('Đã hủy chế độ căn màu', '#64748b');
            });
        }

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

        // Ruler Checkbox & Ruler Corner
        const chkRuler = document.getElementById('check-ruler');
        if (chkRuler) {
            chkRuler.addEventListener('change', (e) => {
                this.showRulers = e.target.checked;
                if (this.assemblyFrame) {
                    this.assemblyFrame.classList.toggle('hide-rulers', !this.showRulers);
                }
                this.drawRulers();
                this.showToast(this.showRulers ? '📏 Đã BẬT thước đo Pixel!' : '⚪ Đã TẮT thước đo!');
            });
        }
        if (this.rulerCorner) {
            this.rulerCorner.addEventListener('click', () => this.resetPan());
        }
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

        if (this.isColorMatchPicking) {
            if (hitKey) {
                if (hitKey === this.colorMatchSourcePartKey) {
                    this.showToast('⚠️ Vui lòng nhấp vào một mảnh khác để lấy mẫu màu!', '#f59e0b');
                    return;
                }
                const sourceKey = this.colorMatchSourcePartKey;
                this.endColorMatchPickMode();
                this.applyColorHarmonization(sourceKey, hitKey);
            } else {
                this.showToast('ℹ️ Hãy nhấp vào một sprite mẫu trên canvas hoặc danh sách Layers (hoặc Esc để hủy)', '#64748b');
            }
            return;
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
        if (this.isColorMatchPicking) {
            this.endColorMatchPickMode();
        }
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

        const pixelX = Math.floor(clientX / this.zoom);
        const pixelY = Math.floor(clientY / this.zoom);

        // Update Pixel Rulers Cursor Tracker
        if (clientX >= 0 && clientX < rect.width && clientY >= 0 && clientY < rect.height) {
            const pixelX = Math.floor(clientX / this.zoom);
            const pixelY = Math.floor(clientY / this.zoom);
            if (pixelX >= 0 && pixelX <= 64 && pixelY >= 0 && pixelY <= 64) {
                this.drawRulers(pixelX, pixelY);
            } else {
                this.drawRulers(-1, -1);
            }
        } else {
            this.drawRulers(-1, -1);
        }

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
        if (e.key === 'Escape') {
            if (this.isColorMatchPicking) {
                this.endColorMatchPickMode();
                this.showToast('Đã hủy chế độ căn màu', '#64748b');
                return;
            }
            this.hideContextMenu();
        }

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
                    } else if (this.currentTab === 'pixel') {
                        this.peSelectAll();
                    }
                    return;
                }
            } else if (k === 'd') {
                if (e.target.tagName !== 'INPUT' && e.target.tagName !== 'SELECT') {
                    if (this.currentTab === 'pixel') {
                        e.preventDefault();
                        this.peDeselect();
                        return;
                    }
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
            if (e.key === 'Escape') {
                this.peDeselect();
            } else if (e.key.toLowerCase() === 'b') {
                const btn = document.getElementById('tool-pencil');
                if (btn) btn.click();
            } else if (e.key.toLowerCase() === 'e') {
                const btn = document.getElementById('tool-eraser');
                if (btn) btn.click();
            } else if (e.key.toLowerCase() === 'g') {
                const btn = document.getElementById('tool-bucket');
                if (btn) btn.click();
            } else if (e.key.toLowerCase() === 'i') {
                const btn = document.getElementById('tool-eyedropper');
                if (btn) btn.click();
            } else if (e.key.toLowerCase() === 'd' && !e.ctrlKey) {
                const btn = document.getElementById('tool-dodge');
                if (btn) btn.click();
            } else if (e.key.toLowerCase() === 'k') {
                const btn = document.getElementById('tool-burn');
                if (btn) btn.click();
            } else if (e.key.toLowerCase() === 'm') {
                const btn = document.getElementById('tool-select-rect');
                if (btn) btn.click();
            } else if (e.key.toLowerCase() === 'w') {
                const btn = document.getElementById('tool-wand');
                if (btn) btn.click();
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

    // ============================================================
    // ANIMATION WORKSHOP ENGINE (VIEW 3)
    // ============================================================
    async loadAnimConfig() {
        try {
            const res = await fetch('/api/anim_config');
            this.animConfig = await res.json();
            this.updateAnimInputsFromConfig();
        } catch (e) {
            console.error('Failed to load anim config:', e);
        }
    }

    onSwitchToAnimTab() {
        this.updateAnimZoom();
        this.updateAnimHeaderBadges();
        this.updateAnimInputsFromConfig();
        this.syncAnimCompassButtons();
    }

    updateAnimZoom() {
        if (!this.animCanvas) return;
        const size = 64 * this.animZoom;
        this.animCanvas.style.width = `${size}px`;
        this.animCanvas.style.height = `${size}px`;
    }

    updateAnimHeaderBadges() {
        const bState = document.getElementById('anim-badge-state');
        const bDir = document.getElementById('anim-badge-dir');
        const pill = document.getElementById('anim-active-pill');

        const stateLabels = {
            idle: '🧘 Đứng Thở (Idle)',
            walk: '🚶 Đi Bộ (Walk)',
            run: '🏃 Chạy Nhanh (Run)',
            dash: '⚡ Lướt Gió (Dash)',
            attack: '⚔️ Chém Đao (Attack)',
            hurt: '💥 Bị Thương (Hurt)'
        };
        if (bState) bState.innerText = stateLabels[this.currentAnimState] || this.currentAnimState;
        if (bDir) bDir.innerText = DIR_LABELS_VI[this.currentDir] || this.currentDir;
        if (pill) pill.innerText = this.currentAnimState.toUpperCase();
    }

    syncAnimCompassButtons() {
        document.querySelectorAll('.anim-dir-btn').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.dir === this.currentDir);
        });
    }

    updateAnimInputsFromConfig() {
        if (!this.animConfig || !this.animConfig[this.currentAnimState]) return;
        const c = this.animConfig[this.currentAnimState];

        const setVal = (param, val) => {
            const slider = document.getElementById(`slider-anim-${param}`);
            const num = document.getElementById(`num-anim-${param}`);
            if (slider) slider.value = val !== undefined ? val : 0;
            if (num) num.value = val !== undefined ? val : 0;
        };

        setVal('speed', c.speed);
        setVal('bob', c.bobAmount);
        setVal('lean', c.bodyLeanAngle);
        setVal('head', c.headTiltAngle);
        setVal('arm', c.armSwingAngle);
        setVal('leg', c.legSwingAngle);
        setVal('squash', c.squashAmount);
        setVal('stretch', c.stretchAmount);

        this.updateAnimHeaderBadges();
    }

    setupAnimEditor() {
        // State selection buttons
        document.querySelectorAll('.anim-state-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                document.querySelectorAll('.anim-state-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                this.currentAnimState = btn.dataset.state;
                this.updateAnimInputsFromConfig();
            });
        });

        // Direction buttons
        document.querySelectorAll('.anim-dir-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                this.setDirection(btn.dataset.dir);
                this.syncAnimCompassButtons();
                this.updateAnimHeaderBadges();
            });
        });

        // Play / Pause button
        const btnPlay = document.getElementById('btn-anim-play-pause');
        if (btnPlay) {
            btnPlay.addEventListener('click', () => {
                this.isAnimPlaying = !this.isAnimPlaying;
                btnPlay.innerText = this.isAnimPlaying ? '⏸️ Tạm Dừng' : '▶️ Tiếp Tục';
                btnPlay.className = this.isAnimPlaying ? 'btn btn-primary' : 'btn btn-secondary';
                btnPlay.style.width = '100%';
                btnPlay.style.marginBottom = '8px';
            });
        }

        // Speed buttons
        document.querySelectorAll('.playback-speed-group .mini-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                document.querySelectorAll('.playback-speed-group .mini-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                this.animPlaybackSpeed = parseFloat(btn.dataset.speed) || 1.0;
            });
        });

        // Zoom dropdown
        const selectZoom = document.getElementById('select-anim-zoom');
        if (selectZoom) {
            selectZoom.addEventListener('change', (e) => {
                this.animZoom = parseInt(e.target.value) || 8;
                this.updateAnimZoom();
            });
        }

        // Checkboxes
        const checkShadow = document.getElementById('check-anim-shadow');
        if (checkShadow) {
            checkShadow.addEventListener('change', (e) => {
                this.showAnimShadow = e.target.checked;
            });
        }

        const checkPivots = document.getElementById('check-anim-pivots');
        if (checkPivots) {
            checkPivots.addEventListener('change', (e) => {
                this.showAnimPivots = e.target.checked;
            });
        }

        // Wire parameter sliders & number inputs
        const paramsMap = {
            'speed': 'speed',
            'bob': 'bobAmount',
            'lean': 'bodyLeanAngle',
            'head': 'headTiltAngle',
            'arm': 'armSwingAngle',
            'leg': 'legSwingAngle',
            'squash': 'squashAmount',
            'stretch': 'stretchAmount'
        };

        for (const [idKey, cfgKey] of Object.entries(paramsMap)) {
            const slider = document.getElementById(`slider-anim-${idKey}`);
            const num = document.getElementById(`num-anim-${idKey}`);

            if (slider && num) {
                const updateParam = (val) => {
                    if (!this.animConfig) return;
                    if (!this.animConfig[this.currentAnimState]) {
                        this.animConfig[this.currentAnimState] = {};
                    }
                    this.animConfig[this.currentAnimState][cfgKey] = val;
                };

                slider.addEventListener('input', (e) => {
                    const val = parseFloat(e.target.value);
                    num.value = val;
                    updateParam(val);
                });

                num.addEventListener('input', (e) => {
                    const val = parseFloat(e.target.value) || 0;
                    slider.value = val;
                    updateParam(val);
                });
            }
        }

        // Save Anim Config button
        const btnSave = document.getElementById('btn-save-anim');
        if (btnSave) {
            btnSave.addEventListener('click', () => this.saveAnimConfig());
        }

        // Reset current state button
        const btnResetState = document.getElementById('btn-reset-anim-state');
        if (btnResetState) {
            btnResetState.addEventListener('click', () => this.resetAnimCurrentState());
        }

        // Reset all anim button
        const btnResetAll = document.getElementById('btn-reset-all-anim');
        if (btnResetAll) {
            btnResetAll.addEventListener('click', () => this.resetAnimAll());
        }
    }

    async saveAnimConfig() {
        if (!this.animConfig) return;
        try {
            const res = await fetch('/api/save_anim_config', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.animConfig)
            });
            const data = await res.json();
            if (data.success) {
                this.showToast('💾 Đã lưu cấu hình hoạt ảnh & xuất ra Unity Resources! Nhấn [F5] trong Unity để nạp ngay.', '#065f46');
            } else {
                this.showToast('❌ Lỗi khi lưu cấu hình hoạt ảnh: ' + data.error, '#dc2626');
            }
        } catch (e) {
            this.showToast('❌ Lỗi kết nối máy chủ: ' + e.message, '#dc2626');
        }
    }

    async resetAnimCurrentState() {
        try {
            const res = await fetch('/api/default_anim_config');
            const defData = await res.json();
            if (defData && defData[this.currentAnimState]) {
                this.animConfig[this.currentAnimState] = { ...defData[this.currentAnimState] };
                this.updateAnimInputsFromConfig();
                this.showToast(`↺ Đã khôi phục trạng thái [${this.currentAnimState}] về mặc định!`);
            }
        } catch (e) {
            this.showToast('❌ Không thể tải cấu hình mặc định: ' + e.message, '#dc2626');
        }
    }

    async resetAnimAll() {
        try {
            const res = await fetch('/api/default_anim_config');
            const defData = await res.json();
            if (defData) {
                this.animConfig = JSON.parse(JSON.stringify(defData));
                this.updateAnimInputsFromConfig();
                this.showToast('🔄 Đã khôi phục toàn bộ hoạt ảnh về mặc định!');
            }
        } catch (e) {
            this.showToast('❌ Không thể tải cấu hình mặc định: ' + e.message, '#dc2626');
        }
    }

    startAnimLoop() {
        const loop = (timestamp) => {
            const dt = Math.min((timestamp - this.lastAnimFrameTime) / 1000, 0.1);
            this.lastAnimFrameTime = timestamp;

            if (this.currentTab === 'anim' && this.animConfig) {
                this.updateAndRenderAnim(dt);
            }
            requestAnimationFrame(loop);
        };
        requestAnimationFrame(loop);
    }

    updateAndRenderAnim(dt) {
        if (!this.animCtx || !this.animConfig) return;
        const c = this.animConfig[this.currentAnimState] || {};
        const speed = (c.speed !== undefined ? c.speed : 1.0) * this.animPlaybackSpeed;

        if (this.isAnimPlaying) {
            this.animTimer += dt * speed;
        }

        // Calculate motions matching Unity harmonic formulas
        let bobY = 0;
        let squashX = 1;
        let stretchY = 1;
        let rootLean = 0;
        let bodyAngle = 0;
        let headAngle = 0;
        let armLAngle = 0;
        let armRAngle = 0;
        let legLAngle = 0;
        let legRAngle = 0;

        const bobAmt = c.bobAmount || 0;
        const leanAmt = (c.bodyLeanAngle || 0) * Math.PI / 180;
        const headAmt = (c.headTiltAngle || 0) * Math.PI / 180;
        const armAmt = (c.armSwingAngle || 0) * Math.PI / 180;
        const legAmt = (c.legSwingAngle || 0) * Math.PI / 180;
        const squashAmt = c.squashAmount || 0;
        const stretchAmt = c.stretchAmount || 0;

        if (this.currentAnimState === 'idle') {
            const breath = Math.sin(this.animTimer);
            bobY = breath * bobAmt;
            squashX = 1 - breath * squashAmt;
            stretchY = 1 + breath * stretchAmt;
            rootLean = breath * leanAmt;
            headAngle = breath * headAmt;
            armLAngle = Math.sin(this.animTimer * 1.1) * armAmt;
            armRAngle = -armLAngle;
            legLAngle = breath * legAmt;
            legRAngle = -legLAngle;
        } else if (this.currentAnimState === 'walk' || this.currentAnimState === 'run') {
            const sinT = Math.sin(this.animTimer);
            const absSin = Math.abs(sinT);
            bobY = absSin * bobAmt;
            squashX = 1 - absSin * squashAmt;
            stretchY = 1 + absSin * stretchAmt;
            bodyAngle = sinT * leanAmt;
            headAngle = -sinT * headAmt;
            legLAngle = sinT * legAmt;
            legRAngle = -legLAngle;
            armLAngle = -sinT * armAmt;
            armRAngle = -armLAngle;
        } else if (this.currentAnimState === 'dash') {
            squashX = 1 + squashAmt;
            stretchY = 1 + stretchAmt;
            bobY = bobAmt;
            bodyAngle = leanAmt;
            headAngle = headAmt;
            armLAngle = armAmt;
            armRAngle = -armAmt;
            legLAngle = legAmt;
            legRAngle = -legAmt;
        } else if (this.currentAnimState === 'attack') {
            const progress = Math.sin(this.animTimer);
            bobY = Math.abs(progress) * bobAmt;
            squashX = 1 - Math.abs(progress) * squashAmt;
            stretchY = 1 + Math.abs(progress) * stretchAmt;
            bodyAngle = progress * leanAmt;
            headAngle = progress * headAmt;
            armRAngle = progress * armAmt;
            armLAngle = -progress * armAmt * 0.4;
            legLAngle = progress * legAmt;
            legRAngle = -legLAngle;
        } else if (this.currentAnimState === 'hurt') {
            const jerk = Math.sin(this.animTimer * 10);
            bobY = -Math.abs(jerk) * bobAmt;
            squashX = 1 + squashAmt;
            stretchY = 1 + stretchAmt;
            rootLean = jerk * leanAmt;
            headAngle = headAmt;
            armLAngle = armAmt;
            armRAngle = armAmt;
        }

        // Update live telemetry footer
        const telemetry = document.getElementById('anim-telemetry');
        if (telemetry) {
            telemetry.innerText = `Nhịp: ${(this.animTimer % 6.28).toFixed(2)}s | Bob: ${bobY.toFixed(2)}px | Thân: ${(bodyAngle * 180 / Math.PI).toFixed(1)}° | Đầu: ${(headAngle * 180 / Math.PI).toFixed(1)}° | Tay: ${(armLAngle * 180 / Math.PI).toFixed(1)}° | Chân: ${(legLAngle * 180 / Math.PI).toFixed(1)}°`;
        }

        // Draw Canvas (64x64)
        this.animCtx.clearRect(0, 0, 64, 64);

        // Ground shadow
        if (this.showAnimShadow) {
            this.animCtx.save();
            this.animCtx.fillStyle = 'rgba(0, 0, 0, 0.35)';
            this.animCtx.beginPath();
            this.animCtx.ellipse(32, 53, 10, 4, 0, 0, Math.PI * 2);
            this.animCtx.fill();
            this.animCtx.restore();
        }

        const dirData = this.getCurrentDirData();
        if (!dirData) return;

        const parts = PART_KEYS.map(key => ({
            key,
            ...dirData[key]
        })).filter(p => p && p.visible !== false);

        parts.sort((a, b) => (a.z || 10) - (b.z || 10));

        // Feet origin in 64x64 is at x = 32, y = 53
        const FEET_X = 32;
        const FEET_Y = 53;

        for (const part of parts) {
            const img = this.getImage(part.file);
            if (!img || !img.complete || img.naturalWidth === 0) continue;

            const px = Math.round(part.x || 0);
            const py = Math.round(part.y || 0);
            const pw = img.naturalWidth;
            const ph = img.naturalHeight;

            // Compute joint pivot for this part
            let jointPivotX = px + pw / 2;
            let jointPivotY = py + ph / 2;
            let partAngle = 0;

            if (part.key === 'idle_head') {
                jointPivotX = px + 8;
                jointPivotY = py + 19;
                partAngle = headAngle;
            } else if (part.key === 'idle_body') {
                jointPivotX = px + 6;
                jointPivotY = py + 12;
                partAngle = bodyAngle;
            } else if (part.key === 'idle_arm_left') {
                jointPivotX = px + 3;
                jointPivotY = py + 2;
                partAngle = armLAngle;
            } else if (part.key === 'idle_arm_right') {
                jointPivotX = px + 3;
                jointPivotY = py + 2;
                partAngle = armRAngle;
            } else if (part.key === 'idle_leg_left') {
                jointPivotX = px + 3;
                jointPivotY = py + 2;
                partAngle = legLAngle;
            } else if (part.key === 'idle_leg_right') {
                jointPivotX = px + 3;
                jointPivotY = py + 2;
                partAngle = legRAngle;
            }

            this.animCtx.save();

            // 1. Root Bobbing & Lean
            this.animCtx.translate(FEET_X, FEET_Y);
            this.animCtx.translate(0, -bobY);
            this.animCtx.rotate(rootLean);
            this.animCtx.scale(squashX, stretchY);
            this.animCtx.translate(-FEET_X, -FEET_Y);

            // 2. Part Joint Rotation
            this.animCtx.translate(jointPivotX, jointPivotY);
            this.animCtx.rotate(partAngle);
            this.animCtx.translate(-jointPivotX, -jointPivotY);

            // 3. Draw Part (handling flip_x / flip_y)
            if (part.flip_x || part.flip_y) {
                this.animCtx.save();
                this.animCtx.translate(px + (part.flip_x ? pw : 0), py + (part.flip_y ? ph : 0));
                this.animCtx.scale(part.flip_x ? -1 : 1, part.flip_y ? -1 : 1);
                this.animCtx.drawImage(img, 0, 0);
                this.animCtx.restore();
            } else {
                this.animCtx.drawImage(img, px, py);
            }

            // 4. Pivot visualization
            if (this.showAnimPivots) {
                this.animCtx.fillStyle = '#f43f5e';
                this.animCtx.fillRect(jointPivotX - 1, jointPivotY - 1, 2, 2);
            }

            this.animCtx.restore();
        }
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
