// wwwroot/js/bg-animation.js
(function () {
    'use strict';

    /**
     * Initialisiert die Hintergrund-Animation auf dem übergebenen Canvas.
     * Transparenter Hintergrund, schimmernde Partikel, feine blaue Verbindungslinien.
     *
     * @param {HTMLCanvasElement} canvas
     * @returns {{destroy: () => void}} - optionaler Cleanup
     */
    window.initBgAnimation = function (canvas) {
        if (!canvas || !canvas.getContext) {
            console.warn('[bg-animation] Kein gültiges Canvas übergeben.');
            return { destroy() { } };
        }

        const ctx = canvas.getContext('2d', { alpha: true });

        const state = {
            running: true,
            dpr: Math.max(1, Math.min(3, window.devicePixelRatio || 1)),
            widthCss: 0,
            heightCss: 0,
            rafId: 0,
            mouseX: -1e6,
            mouseY: -1e6,
            time: 0,
        };

        // ---- Konfiguration ----
        const DENSITY = 0.00012;          // Partikel pro CSS-Pixel^2 (1920x1080 ≈ 250)
        const MAX_CONNECTION_DIST = 140;  // Basis-Distanz für Verbindungslinien
        const MOUSE_RADIUS = 180;         // Einflussradius der Maus
        const MOUSE_CONNECTION_BOOST = 1.2;
        const MOUSE_SLOW_ZONE = 0.3;      // Verlangsamung der Partikel in Mausnähe
        const SPEED_MIN = 0.08;
        const SPEED_MAX = 0.35;
        const POINT_SIZE = [1.2, 2.0];    // min/max Punkt-Radius in CSS-Pixeln
        const LINE_ALPHA = 0.55;
        const POINT_ALPHA = 0.9;
        const COLOR_NEAR = 'rgba(0, 99, 220, 1)';  // helles Blau
        const COLOR_FAR = 'rgba(89, 149, 228, 1)';   // dunkleres Blau
        const COLOR_HIGHLIGHT = 'rgba(100, 200, 255, 1)'; // Highlight-Farbe für Maus-Nähe

        /** @type {{x:number,y:number,vx:number,vy:number,size:number,phase:number}[]} */
        let particles = [];

        // ---- HiDPI + Size ----
        function resizeCanvas() {
            const { innerWidth, innerHeight } = window;
            state.widthCss = innerWidth;
            state.heightCss = innerHeight;

            const w = Math.floor(innerWidth * state.dpr);
            const h = Math.floor(innerHeight * state.dpr);

            if (canvas.width !== w || canvas.height !== h) {
                canvas.width = w;
                canvas.height = h;
                ctx.setTransform(state.dpr, 0, 0, state.dpr, 0, 0);
            }

            seedParticles();
            // Kein Füllen: Hintergrund bleibt transparent
            ctx.clearRect(0, 0, state.widthCss, state.heightCss);
        }

        function rand(min, max) {
            return Math.random() * (max - min) + min;
        }

        function seedParticles() {
            const targetCount = Math.max(
                60,
                Math.floor(state.widthCss * state.heightCss * DENSITY)
            );
            particles = new Array(targetCount).fill(0).map(() => makeParticle());
        }

        function makeParticle() {
            const angle = Math.random() * Math.PI * 2;
            const speed = rand(SPEED_MIN, SPEED_MAX);
            return {
                x: Math.random() * state.widthCss,
                y: Math.random() * state.heightCss,
                vx: Math.cos(angle) * speed,
                vy: Math.sin(angle) * speed,
                size: rand(POINT_SIZE[0], POINT_SIZE[1]),
                phase: Math.random() * Math.PI * 2, // Pulsieren
            };
        }

        // ---- Maus ----
        function onMouseMove(e) {
            // Globale Mausposition verwenden - funktioniert überall im Fenster
            state.mouseX = e.clientX;
            state.mouseY = e.clientY;
        }
        function onMouseLeave() {
            state.mouseX = -1e6;
            state.mouseY = -1e6;
        }

        // ---- Frame-Clear (ohne Trails) ----
        function clearFrame() {
            // Alles pro Frame löschen → keine Trails, transparenter BG
            ctx.clearRect(0, 0, state.widthCss, state.heightCss);
        }

        function updateParticles(dt) {
            const mx = state.mouseX, my = state.mouseY;

            for (let p of particles) {
                // Puls für Partikelgröße
                p.phase += dt * 2;
                const pulse = (Math.sin(p.phase) * 0.25 + 0.75); // 0.5..1.0
                let size = p.size * pulse;

                // Maus-Effekt: Partikel werden langsamer und heller in Mausnähe
                const dx = mx - p.x;
                const dy = my - p.y;
                const dist2 = dx * dx + dy * dy;
                let mouseInfluence = 0;
                let isNearMouse = false;

                if (dist2 < MOUSE_RADIUS * MOUSE_RADIUS) {
                    const dist = Math.sqrt(dist2) || 1;
                    mouseInfluence = 1 - dist / MOUSE_RADIUS; // 0..1
                    isNearMouse = true;

                    // Partikel werden größer und heller in Mausnähe
                    size *= (1 + mouseInfluence * 0.8);

                    // Verlangsamung statt Anziehung
                    const slowFactor = 1 - mouseInfluence * MOUSE_SLOW_ZONE;
                    p.vx *= slowFactor;
                    p.vy *= slowFactor;
                }

                // Normale Bewegung
                p.x += p.vx;
                p.y += p.vy;

                // Screen-Wrapping
                if (p.x < -10) p.x = state.widthCss + 10;
                if (p.x > state.widthCss + 10) p.x = -10;
                if (p.y < -10) p.y = state.heightCss + 10;
                if (p.y > state.heightCss + 10) p.y = -10;

                // Zeichnen (Punkt)
                const pointAlpha = (POINT_ALPHA * 0.8 + 0.2 * pulse) * (1 + mouseInfluence * 0.5);
                ctx.globalAlpha = Math.min(1, pointAlpha);

                // Farbe ändern bei Mausnähe
                if (isNearMouse) {
                    const r = 0 + mouseInfluence * 150;
                    const g = 99 + mouseInfluence * 151;
                    const b = 220 + mouseInfluence * 35;
                    ctx.fillStyle = `rgba(${r}, ${g}, ${b}, 1)`;
                } else {
                    ctx.fillStyle = COLOR_NEAR;
                }

                ctx.beginPath();
                ctx.arc(p.x, p.y, size, 0, Math.PI * 2);
                ctx.fill();
            }
            ctx.globalAlpha = 1;
        }

        function drawConnections() {
            const mx = state.mouseX, my = state.mouseY;

            for (let i = 0; i < particles.length; i++) {
                const a = particles[i];

                // Mausnähe verstärkt Verbindungen rund um Partikel a
                let distBoostA = 0;
                const amx = a.x - mx, amy = a.y - my;
                const ad2 = amx * amx + amy * amy;
                if (ad2 < MOUSE_RADIUS * MOUSE_RADIUS) {
                    distBoostA = MOUSE_CONNECTION_BOOST * (1 - Math.sqrt(ad2) / MOUSE_RADIUS);
                }

                for (let j = i + 1; j < particles.length; j++) {
                    const b = particles[j];
                    const dx = a.x - b.x;
                    const dy = a.y - b.y;
                    const d2 = dx * dx + dy * dy;
                    const maxD = MAX_CONNECTION_DIST + distBoostA * 100;
                    if (d2 > maxD * maxD) continue;

                    const d = Math.sqrt(d2);
                    let t = 1 - d / maxD; // 0..1
                    if (t <= 0) continue;

                    // zusätzlicher Boost, wenn b auch nahe Maus ist
                    let distBoostB = 0;
                    const bmx = b.x - mx, bmy = b.y - my;
                    const bd2 = bmx * bmx + bmy * bmy;
                    if (bd2 < MOUSE_RADIUS * MOUSE_RADIUS) {
                        distBoostB = MOUSE_CONNECTION_BOOST * (1 - Math.sqrt(bd2) / MOUSE_RADIUS);
                    }
                    const mouseFactor = 1 + 1.5 * Math.max(distBoostA, distBoostB); // 1..2.5

                    // Verlauf für subtilen Blau-Shift
                    const grad = ctx.createLinearGradient(a.x, a.y, b.x, b.y);
                    grad.addColorStop(0, COLOR_NEAR);
                    grad.addColorStop(1, COLOR_FAR);

                    ctx.strokeStyle = grad;
                    ctx.lineWidth = 1;
                    ctx.globalAlpha = Math.min(1, LINE_ALPHA * t * mouseFactor);

                    ctx.beginPath();
                    ctx.moveTo(a.x, a.y);
                    ctx.lineTo(b.x, b.y);
                    ctx.stroke();
                }
            }
            ctx.globalAlpha = 1;
        }

        // ---- Main Loop ----
        let lastTs = 0;
        function drawFrame(ts) {
            if (!state.running) return;

            const dt = Math.min(0.033, (ts - lastTs) / 1000 || 0.016); // clamp ~30–60 FPS
            lastTs = ts;
            state.time += dt;

            // Clear → keine Trails, transparenter BG
            clearFrame();

            // Partikel updaten & zeichnen
            updateParticles(dt);

            // Verbindungen zeichnen
            drawConnections();

            state.rafId = requestAnimationFrame(drawFrame);
        }

        // ---- Resize/Init ----
        let resizeRaf = 0;
        function onResize() {
            const newDpr = Math.max(1, Math.min(3, window.devicePixelRatio || 1));
            if (newDpr !== state.dpr) state.dpr = newDpr;

            if (resizeRaf) cancelAnimationFrame(resizeRaf);
            resizeRaf = requestAnimationFrame(resizeCanvas);
        }

        resizeCanvas();
        lastTs = performance.now();
        state.rafId = requestAnimationFrame(drawFrame);

        // ---- Events ----
        window.addEventListener('mousemove', onMouseMove);
        window.addEventListener('mouseleave', onMouseLeave);
        window.addEventListener('resize', onResize);

        // ---- Cleanup ----
        function destroy() {
            state.running = false;
            if (state.rafId) cancelAnimationFrame(state.rafId);
            window.removeEventListener('mousemove', onMouseMove);
            window.removeEventListener('mouseleave', onMouseLeave);
            window.removeEventListener('resize', onResize);
        }

        // Doppelte Initialisierung verhindern
        if (canvas.__bgAnimationDestroy) {
            try { canvas.__bgAnimationDestroy(); } catch { }
        }
        canvas.__bgAnimationDestroy = destroy;

        return { destroy };
    };
})();
