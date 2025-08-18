window.dashboard = (() => {
	function clamp(n, min, max) { return Math.max(min, Math.min(max, n)); }

	function applyGridPosition(el, x, y, w, h) {
		el.style.gridColumn = `${x} / span ${w}`;
		el.style.gridRow = `${y} / span ${h}`;
	}

	function loadLayout(board) {
		const key = `layout:${location.pathname}`;
		try {
			const saved = JSON.parse(localStorage.getItem(key) || "[]");
			const byId = Object.fromEntries(saved.map(x => [x.id, x]));
			board.querySelectorAll(".card[id]").forEach(el => {
				const s = byId[el.id];
				if (s) applyGridPosition(el, s.x, s.y, s.w, s.h);
			});
		} catch {}
	}

	function saveLayout(board) {
		const key = `layout:${location.pathname}`;
		const layout = [];
		board.querySelectorAll(".card[id]").forEach(el => {
			const [x, , w] = parseGrid(el.style.gridColumn) || [];
			const [y, , h] = parseGrid(el.style.gridRow) || [];
			if (x && y && w && h) layout.push({ id: el.id, x, y, w, h });
		});
		localStorage.setItem(key, JSON.stringify(layout));
	}

	function parseGrid(str) {
		// "x / span w"
		if (!str) return null;
		const m = str.match(/^\s*(\d+)\s*\/\s*span\s*(\d+)\s*$/i);
		if (!m) return null;
		return [parseInt(m[1],10), "span", parseInt(m[2],10)];
	}

	function placeFromData(el) {
		const x = parseInt(el.dataset.x || "1", 10);
		const y = parseInt(el.dataset.y || "1", 10);
		const w = parseInt(el.dataset.w || "4", 10);
		const h = parseInt(el.dataset.h || "6", 10);
		applyGridPosition(el, x, y, w, h);
	}

	function initBoard(selector, opts) {
		const board = document.querySelector(selector);
		if (!board) return;

		const cols = opts?.cols || 12;
		const gridSize = opts?.gridSize || 16;
		const gap = opts?.gap || 8;

		board.style.setProperty("--board-cols", cols);
		board.style.setProperty("--grid-size", `${gridSize}px`);
		board.style.setProperty("--gap", `${gap}px`);

		// Startpositionen setzen (falls kein Layout gespeichert)
		board.querySelectorAll(".card").forEach(placeFromData);
		loadLayout(board);

		let dragging = null;
		let resizing = null;
		let start = null;

		function cellFromPx(px) {
			// Zelle = 1fr -> wir rechnen über Boardbreite
			const boardRect = board.getBoundingClientRect();
			const totalGap = (cols - 1) * gap;
			const colWidth = (boardRect.width - totalGap) / cols;
			return { colWidth, boardRect };
		}

		function getCurrentGrid(el) {
			const [x,,w] = parseGrid(el.style.gridColumn) || [1,"span",4];
			const [y,,h] = parseGrid(el.style.gridRow) || [1,"span",6];
			return { x, y, w, h };
		}

		function pointerDownDrag(e, el) {
			el.classList.add("dragging");
			const { x, y, w, h } = getCurrentGrid(el);
			start = { pointerId: e.pointerId, x0: e.clientX, y0: e.clientY, x, y, w, h };
			dragging = el;
			el.setPointerCapture(e.pointerId);
		}

		function pointerDownResize(e, el) {
			el.classList.add("resizing");
			const { x, y, w, h } = getCurrentGrid(el);
			start = { pointerId: e.pointerId, x0: e.clientX, y0: e.clientY, x, y, w, h };
			resizing = el;
			el.setPointerCapture(e.pointerId);
		}

		function pointerMove(e) {
			if (!dragging && !resizing) return;
			const { colWidth } = cellFromPx();
			const dx = e.clientX - start.x0;
			const dy = e.clientY - start.y0;
			const stepX = Math.round(dx / (colWidth + gap));
			const stepY = Math.round(dy / (gridSize + gap));

			if (dragging) {
				const w = start.w;
				const x = clamp(start.x + stepX, 1, cols - w + 1);
				const y = clamp(start.y + stepY, 1, 200); // großzügig viele Zeilen
				applyGridPosition(dragging, x, y, w, start.h);
			} else if (resizing) {
				const w = clamp(start.w + stepX, 1, cols - start.x + 1);
				const h = clamp(start.h + stepY, 1, 200);
				applyGridPosition(resizing, start.x, start.y, w, h);
			}
		}

		function pointerUp(e) {
			if (dragging && e.pointerId === start.pointerId) {
				dragging.classList.remove("dragging");
				dragging.releasePointerCapture(e.pointerId);
				dragging = null;
				saveLayout(board);
			}
			if (resizing && e.pointerId === start.pointerId) {
				resizing.classList.remove("resizing");
				resizing.releasePointerCapture(e.pointerId);
				resizing = null;
				saveLayout(board);
			}
		}

		board.querySelectorAll(".card").forEach(el => {
			const header = el.querySelector(".card-header") || el;
			header.addEventListener("pointerdown", (e) => {
				if (e.button !== 0) return;
				// Klick auf Resize-Handle nicht als Drag
				if (e.target.closest(".resize-handle")) return;
				pointerDownDrag(e, el);
			});
			const handle = el.querySelector(".resize-handle");
			if (handle) {
				handle.addEventListener("pointerdown", (e) => {
					if (e.button !== 0) return;
					pointerDownResize(e, el);
					e.stopPropagation();
				});
			}
		});

		window.addEventListener("pointermove", pointerMove);
		window.addEventListener("pointerup", pointerUp);

		// Re-Snap bei Resize des Boards
		const ro = new ResizeObserver(() => saveLayout(board));
		ro.observe(board);
	}

	return { initBoard };
})();
