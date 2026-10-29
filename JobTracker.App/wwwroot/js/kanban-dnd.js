// kanban-dnd.js — arrastre de tarjetas del tablero (BRIEF-001 / PLAN-001).
//
// El JS solo detecta el gesto; la decisión y la escritura viven en
// KanbanStatusMover (C#). Delegación en document para sobrevivir a los
// re-render de Blazor. Un único puntero activo. Todas las salidas pasan por
// finish(), que libera captura, limpia clases y deja el guard de clic.

const THRESHOLD = 8;   // px para considerar que es un arrastre y no un clic
const CLICK_GUARD = 300; // ms para que soltar no navegue

const state = {
    ref: null,
    pointerId: null,
    card: null,
    startX: 0,
    startY: 0,
    dragging: false,
    source: null,
    hovered: null,
    blockClickUntil: 0,
};

export function init(dotNetRef) {
    if (state.ref) return; // idempotente
    state.ref = dotNetRef;
    document.addEventListener('pointerdown', onPointerDown, true);
    document.addEventListener('pointermove', onPointerMove, true);
    document.addEventListener('pointerup', onPointerUp, true);
    document.addEventListener('pointercancel', onPointerCancel, true);
    document.addEventListener('lostpointercapture', onPointerCancel, true);
    document.addEventListener('click', onClickCapture, true);
    document.addEventListener('dragstart', onDragStart, true);
}

export function dispose() {
    document.removeEventListener('pointerdown', onPointerDown, true);
    document.removeEventListener('pointermove', onPointerMove, true);
    document.removeEventListener('pointerup', onPointerUp, true);
    document.removeEventListener('pointercancel', onPointerCancel, true);
    document.removeEventListener('lostpointercapture', onPointerCancel, true);
    document.removeEventListener('click', onClickCapture, true);
    document.removeEventListener('dragstart', onDragStart, true);
    finish();
    state.ref = null;
}

function columnOf(el) {
    return el ? el.closest('.kanban-column') : null;
}

function columnAt(x, y) {
    const el = document.elementFromPoint(x, y);
    return columnOf(el);
}

function highlight(column) {
    if (state.hovered === column) return;
    if (state.hovered) state.hovered.classList.remove('drop-target');
    state.hovered = column;
    if (column) column.classList.add('drop-target');
}

function onPointerDown(e) {
    if (!state.ref || state.pointerId !== null) return;      // un solo puntero
    if (e.button !== 0) return;                               // solo botón primario
    const card = e.target && e.target.closest ? e.target.closest('.kanban-card') : null;
    if (!card) return;

    state.pointerId = e.pointerId;
    state.card = card;
    state.startX = e.clientX;
    state.startY = e.clientY;
    state.dragging = false;
    state.source = card.dataset.status || null;
}

function onPointerMove(e) {
    if (e.pointerId !== state.pointerId || !state.card) return;

    if (!state.dragging) {
        const dx = e.clientX - state.startX;
        const dy = e.clientY - state.startY;
        if (Math.hypot(dx, dy) < THRESHOLD) return;

        state.dragging = true;
        state.card.classList.add('dragging');
        try {
            state.card.setPointerCapture(e.pointerId);
        } catch (_) {
            // sin captura seguimos: los listeners están en document
        }
    }

    highlight(columnAt(e.clientX, e.clientY));
}

function onPointerUp(e) {
    if (e.pointerId !== state.pointerId) return;

    if (state.dragging && state.card) {
        const target = columnAt(e.clientX, e.clientY);
        const status = target ? target.dataset.status : null;
        const appId = state.card.dataset.appId;
        if (status && appId && status !== state.source) {
            state.blockClickUntil = Date.now() + CLICK_GUARD;
            const ref = state.ref;
            if (ref) {
                ref.invokeMethodAsync('OnCardDroppedAsync', appId, status)
                    .catch(() => { /* el componente puede estar disponible */ });
            }
        }
    }

    finish();
}

function onPointerCancel(e) {
    if (e.pointerId !== state.pointerId) return;
    finish(); // cancela sin invocar al componente
}

function onClickCapture(e) {
    if (Date.now() >= state.blockClickUntil) return;
    // Venimos de un arrastre: el clic no debe navegar al formulario.
    e.preventDefault();
    e.stopPropagation();
}

function onDragStart(e) {
    // Que el navegador no arranque el DnD HTML5 nativo (el que ya falló aquí).
    e.preventDefault();
}

function finish() {
    if (state.card) {
        if (state.dragging) {
            try {
                state.card.releasePointerCapture(state.pointerId);
            } catch (_) {
                // ya liberada
            }
        }
        state.card.classList.remove('dragging');
    }
    if (state.hovered) state.hovered.classList.remove('drop-target');

    state.pointerId = null;
    state.card = null;
    state.dragging = false;
    state.source = null;
    state.hovered = null;
}
