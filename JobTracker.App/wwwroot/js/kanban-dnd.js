// kanban-dnd.js — arrastre de tarjetas del tablero (BRIEF-001 / BRIEF-002).
//
// El JS solo detecta el gesto; la decisión y la escritura viven en
// KanbanStatusMover (C#). Delegación en document para sobrevivir a los
// re-render de Blazor. Un único puntero activo. Todas las salidas pasan por
// finish(), que libera captura, limpia clases, borra la copia y deja el guard
// de clic.

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
    ghost: null,
    offsetX: 0,
    offsetY: 0,
};

export function init(dotNetRef) {
    if (state.ref) return; // idempotente
    state.ref = dotNetRef;
    document.addEventListener('pointerdown', onPointerDown, true);
    document.addEventListener('pointermove', onPointerMove, true);
    document.addEventListener('pointerup', onPointerUp, true);
    document.addEventListener('pointercancel', onPointerCancel, true);
    document.addEventListener('lostpointercapture', onLostPointerCapture, true);
    document.addEventListener('pointerout', onPointerOut, true);
    document.addEventListener('click', onClickCapture, true);
    document.addEventListener('dragstart', onDragStart, true);
    window.addEventListener('blur', onWindowBlur);
}

export function dispose() {
    document.removeEventListener('pointerdown', onPointerDown, true);
    document.removeEventListener('pointermove', onPointerMove, true);
    document.removeEventListener('pointerup', onPointerUp, true);
    document.removeEventListener('pointercancel', onPointerCancel, true);
    document.removeEventListener('lostpointercapture', onLostPointerCapture, true);
    document.removeEventListener('pointerout', onPointerOut, true);
    document.removeEventListener('click', onClickCapture, true);
    document.removeEventListener('dragstart', onDragStart, true);
    window.removeEventListener('blur', onWindowBlur);
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

// --- copia que viaja con el puntero (BRIEF-002) ---

function createGhost(rect) {
    if (state.ghost || !state.card) return;

    const clone = state.card.cloneNode(true);
    clone.classList.add('kanban-card-ghost');
    clone.removeAttribute('href');      // no debe navegar ni recibir foco
    clone.setAttribute('aria-hidden', 'true');
    clone.style.width = rect.width + 'px';
    document.body.appendChild(clone);

    state.ghost = clone;
}

function moveGhost(x, y) {
    if (!state.ghost) return;
    state.ghost.style.transform =
        `translate(${x - state.offsetX}px, ${y - state.offsetY}px)`;
}

function destroyGhost() {
    if (state.ghost) {
        state.ghost.remove();
        state.ghost = null;
    }
}

// --- gesto ---

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

        // El agarre se mide en ESTE instante: así la copia nace bajo el cursor
        // sin saltar (el puntero ya no está en startX/startY).
        const rect = state.card.getBoundingClientRect();
        state.offsetX = e.clientX - rect.left;
        state.offsetY = e.clientY - rect.top;
        createGhost(rect);

        try {
            state.card.setPointerCapture(e.pointerId);
        } catch (_) {
            // sin captura seguimos: los listeners están en document
        }
    }

    if (state.dragging) moveGhost(e.clientX, e.clientY);
    highlight(columnAt(e.clientX, e.clientY));
}

function onPointerUp(e) {
    if (e.pointerId !== state.pointerId) return;

    if (state.dragging && state.card) {
        // Un arrastre nunca acaba en clic: se suelte donde se suelte. Antes el
        // guard solo se armaba si cambiaba la columna, así que soltar en la
        // misma columna (o fuera de todas) abría la tarjeta como un tap.
        state.blockClickUntil = Date.now() + CLICK_GUARD;

        const target = columnAt(e.clientX, e.clientY);
        const status = target ? target.dataset.status : null;
        const appId = state.card.dataset.appId;
        if (status && appId && status !== state.source) {
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

function onLostPointerCapture(e) {
    // finish() suelta la captura y este evento puede llegar después, incluso
    // con otro gesto empezado: solo cancela si el arrastre sigue vivo.
    if (!state.dragging) return;
    if (e.pointerId !== state.pointerId) return;
    finish();
}

function onPointerOut(e) {
    // Salir de la ventana puede no dejar llegar nunca el pointerup.
    if (!state.dragging || e.relatedTarget) return;
    finish();
}

function onWindowBlur() {
    if (state.dragging) finish();
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
    destroyGhost();

    state.pointerId = null;
    state.card = null;
    state.dragging = false;
    state.source = null;
    state.hovered = null;
}
