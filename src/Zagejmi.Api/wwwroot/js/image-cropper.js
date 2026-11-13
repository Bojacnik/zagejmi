// Image cropper used by Components/Elements/ImageCropperModal.razor.
//
// The picked file is shown straight from the browser (object URL); nothing is resized here. The cropper only
// reports which part of the image to keep, as fractions of the (EXIF-rotated) image, and the server crops the
// original at full quality.
//
// Interaction: drag to pan, wheel / pinch / slider / +- buttons to zoom (zooming keeps the point under the cursor
// or fingers in place), arrow keys to pan, +/- keys to zoom. The image always covers the frame.

const MAX_ZOOM = 8;           // relative to the smallest zoom that still covers the frame
const MIN_CROP_SOURCE_PX = 32; // never zoom in so far that fewer source pixels than this fill the frame
const SLIDER_STEPS = 1000;

class Cropper {
    constructor(root, options, dotNet) {
        this.root = root;
        this.options = options;
        this.dotNet = dotNet;

        this.stage = root.querySelector('[data-crop-stage]');
        this.img = root.querySelector('[data-crop-image]');
        this.frame = root.querySelector('[data-crop-frame]');
        this.slider = root.querySelector('[data-crop-zoom]');
        this.warning = root.querySelector('[data-crop-warning]');
        this.previews = [...root.querySelectorAll('[data-crop-preview]')];

        this.pointers = new Map();
        this.gesture = null;
        this.frameRect = { x: 0, y: 0, w: 0, h: 0 };
        this.scale = 1;
        this.tx = 0;
        this.ty = 0;
        this.ready = false;
        this.raf = 0;
        this.hideGridTimer = 0;

        this.onPointerDown = this.onPointerDown.bind(this);
        this.onPointerMove = this.onPointerMove.bind(this);
        this.onPointerUp = this.onPointerUp.bind(this);
        this.onWheel = this.onWheel.bind(this);
        this.onKeyDown = this.onKeyDown.bind(this);
        this.onSlider = this.onSlider.bind(this);

        this.stage.addEventListener('pointerdown', this.onPointerDown);
        this.stage.addEventListener('pointermove', this.onPointerMove);
        this.stage.addEventListener('pointerup', this.onPointerUp);
        this.stage.addEventListener('pointercancel', this.onPointerUp);
        this.stage.addEventListener('wheel', this.onWheel, { passive: false });
        this.stage.addEventListener('keydown', this.onKeyDown);
        this.slider?.addEventListener('input', this.onSlider);

        this.resizeObserver = new ResizeObserver(() => this.relayout());
        this.resizeObserver.observe(this.stage);
    }

    load(url) {
        this.url = url;
        this.img.onload = () => {
            this.naturalW = this.img.naturalWidth;
            this.naturalH = this.img.naturalHeight;
            this.img.style.width = `${this.naturalW}px`;
            this.img.style.height = `${this.naturalH}px`;
            for (const preview of this.previews) {
                preview.querySelector('img').src = url;
            }
            this.ready = true;
            this.layoutFrame();
            this.reset();
            this.root.classList.add('is-ready');
            this.dotNet.invokeMethodAsync('OnImageReady', this.naturalW, this.naturalH);
        };
        this.img.onerror = () => this.dotNet.invokeMethodAsync('OnImageError');
        this.img.src = url;
    }

    // ----- geometry -------------------------------------------------------------------------------------------

    layoutFrame() {
        const stageW = this.stage.clientWidth;
        const stageH = this.stage.clientHeight;
        const padding = this.options.padding ?? 24;
        const ratio = this.options.aspectRatio;

        let w = stageW - padding * 2;
        let h = w / ratio;
        if (h > stageH - padding * 2) {
            h = stageH - padding * 2;
            w = h * ratio;
        }

        this.frameRect = { x: (stageW - w) / 2, y: (stageH - h) / 2, w, h };
        Object.assign(this.frame.style, {
            left: `${this.frameRect.x}px`,
            top: `${this.frameRect.y}px`,
            width: `${w}px`,
            height: `${h}px`,
        });
    }

    get minScale() {
        const f = this.frameRect;
        return Math.max(f.w / this.naturalW, f.h / this.naturalH);
    }

    get maxScale() {
        const f = this.frameRect;
        const bySourcePixels = Math.min(f.w, f.h) / MIN_CROP_SOURCE_PX;
        return Math.max(this.minScale, Math.min(this.minScale * MAX_ZOOM, bySourcePixels));
    }

    clamp() {
        const f = this.frameRect;
        this.scale = Math.min(Math.max(this.scale, this.minScale), this.maxScale);
        const w = this.naturalW * this.scale;
        const h = this.naturalH * this.scale;
        this.tx = Math.min(f.x, Math.max(f.x + f.w - w, this.tx));
        this.ty = Math.min(f.y, Math.max(f.y + f.h - h, this.ty));
    }

    /** Zooms to `scale` keeping the stage point (px, py) still. */
    zoomTo(scale, px, py) {
        const target = Math.min(Math.max(scale, this.minScale), this.maxScale);
        const k = target / this.scale;
        this.tx = px - (px - this.tx) * k;
        this.ty = py - (py - this.ty) * k;
        this.scale = target;
        this.clamp();
        this.render();
    }

    zoomBy(factor, px, py) {
        const f = this.frameRect;
        this.zoomTo(this.scale * factor, px ?? f.x + f.w / 2, py ?? f.y + f.h / 2);
    }

    reset() {
        const f = this.frameRect;
        this.scale = this.minScale;
        this.tx = f.x + (f.w - this.naturalW * this.scale) / 2;
        this.ty = f.y + (f.h - this.naturalH * this.scale) / 2;
        this.clamp();
        this.render();
    }

    /** The visible part of the image as fractions of its size. */
    getCrop() {
        const f = this.frameRect;
        const w = this.naturalW * this.scale;
        const h = this.naturalH * this.scale;
        const clamp01 = v => Math.min(1, Math.max(0, v));
        return {
            x: clamp01((f.x - this.tx) / w),
            y: clamp01((f.y - this.ty) / h),
            width: clamp01(f.w / w),
            height: clamp01(f.h / h),
        };
    }

    setCrop(crop) {
        const f = this.frameRect;
        this.scale = f.w / (crop.width * this.naturalW);
        this.tx = f.x - crop.x * this.naturalW * this.scale;
        this.ty = f.y - crop.y * this.naturalH * this.scale;
        this.clamp();
    }

    relayout() {
        if (!this.ready) {
            return;
        }
        const crop = this.getCrop();
        this.layoutFrame();
        this.setCrop(crop);
        this.render();
    }

    // ----- rendering ------------------------------------------------------------------------------------------

    render() {
        if (this.raf) {
            return;
        }
        this.raf = requestAnimationFrame(() => {
            this.raf = 0;
            this.img.style.transform = `translate3d(${this.tx}px, ${this.ty}px, 0) scale(${this.scale})`;

            if (this.slider) {
                const range = Math.log(this.maxScale / this.minScale);
                this.slider.value = range > 0
                    ? Math.round(Math.log(this.scale / this.minScale) / range * SLIDER_STEPS)
                    : 0;
                this.slider.disabled = range <= 0;
            }

            const crop = this.getCrop();
            for (const preview of this.previews) {
                const img = preview.querySelector('img');
                // Percentages of the preview box: the crop rectangle fills it exactly.
                img.style.width = `${100 / crop.width}%`;
                img.style.left = `${-crop.x / crop.width * 100}%`;
                img.style.top = `${-crop.y / crop.height * 100}%`;
            }

            if (this.warning) {
                const sourceWidth = crop.width * this.naturalW;
                this.warning.hidden = sourceWidth >= (this.options.recommendedMinWidth ?? 0);
            }
        });
    }

    showGrid() {
        this.frame.classList.add('show-grid');
        clearTimeout(this.hideGridTimer);
    }

    hideGridSoon() {
        clearTimeout(this.hideGridTimer);
        this.hideGridTimer = setTimeout(() => this.frame.classList.remove('show-grid'), 400);
    }

    // ----- input ----------------------------------------------------------------------------------------------

    stagePoint(e) {
        const r = this.stage.getBoundingClientRect();
        return { x: e.clientX - r.left, y: e.clientY - r.top };
    }

    onPointerDown(e) {
        if (!this.ready || (e.pointerType === 'mouse' && e.button !== 0)) {
            return;
        }
        this.stage.setPointerCapture(e.pointerId);
        this.pointers.set(e.pointerId, this.stagePoint(e));
        this.startGesture();
        this.stage.classList.add('is-dragging');
        this.showGrid();
        e.preventDefault();
    }

    startGesture() {
        const points = [...this.pointers.values()];
        if (points.length === 1) {
            this.gesture = { type: 'pan', start: points[0], tx: this.tx, ty: this.ty };
        } else if (points.length >= 2) {
            const [a, b] = points;
            this.gesture = {
                type: 'pinch',
                distance: Math.hypot(b.x - a.x, b.y - a.y),
                mid: { x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 },
                scale: this.scale,
                tx: this.tx,
                ty: this.ty,
            };
        }
    }

    onPointerMove(e) {
        if (!this.pointers.has(e.pointerId) || !this.gesture) {
            return;
        }
        this.pointers.set(e.pointerId, this.stagePoint(e));
        const points = [...this.pointers.values()];

        if (this.gesture.type === 'pan' && points.length === 1) {
            this.tx = this.gesture.tx + points[0].x - this.gesture.start.x;
            this.ty = this.gesture.ty + points[0].y - this.gesture.start.y;
            this.clamp();
            this.render();
        } else if (this.gesture.type === 'pinch' && points.length >= 2) {
            const [a, b] = points;
            const g = this.gesture;
            const mid = { x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 };
            const target = Math.min(Math.max(g.scale * Math.hypot(b.x - a.x, b.y - a.y) / g.distance, this.minScale), this.maxScale);
            const k = target / g.scale;
            // Zoom around the starting midpoint, then follow the fingers.
            this.scale = target;
            this.tx = g.mid.x - (g.mid.x - g.tx) * k + (mid.x - g.mid.x);
            this.ty = g.mid.y - (g.mid.y - g.ty) * k + (mid.y - g.mid.y);
            this.clamp();
            this.render();
        }
    }

    onPointerUp(e) {
        this.pointers.delete(e.pointerId);
        if (this.pointers.size > 0) {
            this.startGesture();
            return;
        }
        this.gesture = null;
        this.stage.classList.remove('is-dragging');
        this.hideGridSoon();
    }

    onWheel(e) {
        if (!this.ready) {
            return;
        }
        e.preventDefault();
        // Trackpads send many small deltas, mice few large ones; exp() keeps both smooth.
        const delta = e.deltaMode === 1 ? e.deltaY * 16 : e.deltaY;
        const p = this.stagePoint(e);
        this.showGrid();
        this.zoomBy(Math.exp(-delta * 0.0015), p.x, p.y);
        this.hideGridSoon();
    }

    onKeyDown(e) {
        if (!this.ready) {
            return;
        }
        const step = e.shiftKey ? 50 : 10;
        switch (e.key) {
            case 'ArrowLeft': this.tx += step; break;
            case 'ArrowRight': this.tx -= step; break;
            case 'ArrowUp': this.ty += step; break;
            case 'ArrowDown': this.ty -= step; break;
            case '+': case '=': this.zoomBy(1.1); e.preventDefault(); return;
            case '-': case '_': this.zoomBy(1 / 1.1); e.preventDefault(); return;
            default: return;
        }
        e.preventDefault();
        this.clamp();
        this.render();
    }

    onSlider() {
        const t = Number(this.slider.value) / SLIDER_STEPS;
        this.zoomTo(this.minScale * Math.pow(this.maxScale / this.minScale, t),
            this.frameRect.x + this.frameRect.w / 2, this.frameRect.y + this.frameRect.h / 2);
    }

    dispose() {
        cancelAnimationFrame(this.raf);
        clearTimeout(this.hideGridTimer);
        this.resizeObserver.disconnect();
        this.stage.removeEventListener('pointerdown', this.onPointerDown);
        this.stage.removeEventListener('pointermove', this.onPointerMove);
        this.stage.removeEventListener('pointerup', this.onPointerUp);
        this.stage.removeEventListener('pointercancel', this.onPointerUp);
        this.stage.removeEventListener('wheel', this.onWheel);
        this.stage.removeEventListener('keydown', this.onKeyDown);
        this.slider?.removeEventListener('input', this.onSlider);
    }
}

/**
 * Opens a cropper for the file currently selected in `fileInput`.
 * Returns a handle with zoomBy(), reset(), getCrop(), focus() and dispose().
 */
export function create(root, fileInput, options, dotNet) {
    const file = fileInput?.files?.[0];
    const cropper = new Cropper(root, options, dotNet);
    let url = null;

    if (file) {
        url = URL.createObjectURL(file);
        cropper.load(url);
    } else {
        dotNet.invokeMethodAsync('OnImageError');
    }

    return {
        zoomBy: factor => cropper.ready && cropper.zoomBy(factor),
        reset: () => cropper.ready && cropper.reset(),
        getCrop: () => (cropper.ready ? cropper.getCrop() : null),
        focus: () => cropper.stage.focus({ preventScroll: true }),
        dispose: () => {
            cropper.dispose();
            if (url) {
                URL.revokeObjectURL(url);
            }
        },
    };
}
