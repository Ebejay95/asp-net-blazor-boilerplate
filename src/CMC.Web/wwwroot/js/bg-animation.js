window.addEventListener("DOMContentLoaded", function () {
    this.setTimeout(function () {
        const canvas = document.getElementById('canvas');
        console.log(canvas)
        const ctx = canvas.getContext('2d');

        let width, height;
        let mouseX = 0, mouseY = 0;
        let verticalLines = [];
        let crossConnections = [];
        let shimmerPoints = [];
        let time = 0;

        function resize() {
            width = canvas.width = window.innerWidth;
            height = canvas.height = window.innerHeight;
        }

        class VerticalLine {
            constructor(x) {
                this.x = x;
                this.originalX = x;
                this.shimmer = Math.random() * Math.PI * 2;
                this.shimmerSpeed = 0.01 + Math.random() * 0.02;
                this.mouseInfluence = 0;
                this.sway = Math.random() * 0.5 + 0.2;
                this.swayOffset = Math.random() * Math.PI * 2;
            }

            update() {
                // Sanfte Schwankung
                this.x = this.originalX + Math.sin(time * this.sway + this.swayOffset) * 2;

                // Maus-Einfluss
                const dx = mouseX - this.x;
                const distance = Math.abs(dx);
                const maxDistance = 150;

                if (distance < maxDistance) {
                    const force = (maxDistance - distance) / maxDistance;
                    this.mouseInfluence = force;
                    this.x += (dx > 0 ? 1 : -1) * force * 15;
                } else {
                    this.mouseInfluence *= 0.95;
                }

                this.shimmer += this.shimmerSpeed;
                if (this.shimmer > Math.PI * 2) this.shimmer = 0;
            }

            draw() {
                const alpha = 0.4 + Math.sin(this.shimmer) * 0.2;
                const glowAlpha = this.mouseInfluence * 0.6;

                // Hauptlinie
                ctx.beginPath();
                ctx.moveTo(this.x, 0);
                ctx.lineTo(this.x, height);
                ctx.strokeStyle = `rgba(102, 153, 204, ${alpha})`;
                ctx.lineWidth = 1.5;
                ctx.stroke();

                // Glow-Effekt bei Mausinteraktion
                if (this.mouseInfluence > 0.1) {
                    ctx.beginPath();
                    ctx.moveTo(this.x, 0);
                    ctx.lineTo(this.x, height);
                    ctx.strokeStyle = `rgba(173, 216, 230, ${glowAlpha})`;
                    ctx.lineWidth = 3;
                    ctx.stroke();

                    // Zusätzlicher innerer Glow
                    ctx.beginPath();
                    ctx.moveTo(this.x, 0);
                    ctx.lineTo(this.x, height);
                    ctx.strokeStyle = `rgba(200, 230, 255, ${glowAlpha * 0.5})`;
                    ctx.lineWidth = 1;
                    ctx.stroke();
                }
            }
        }

        class CrossConnection {
            constructor(y, startIndex, endIndex) {
                this.y = y;
                this.originalY = y;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
                this.shimmer = Math.random() * Math.PI * 2;
                this.shimmerSpeed = 0.015 + Math.random() * 0.025;
                this.mouseInfluence = 0;
                this.pulse = Math.random() * Math.PI * 2;
                this.pulseSpeed = 0.02 + Math.random() * 0.03;
            }

            update() {
                // Sanfte Y-Bewegung
                this.y = this.originalY + Math.sin(time * 0.3 + this.pulse) * 1;

                // Maus-Einfluss
                const dy = mouseY - this.y;
                const distance = Math.abs(dy);
                const maxDistance = 100;

                if (distance < maxDistance) {
                    const force = (maxDistance - distance) / maxDistance;
                    this.mouseInfluence = force;
                    this.y += (dy > 0 ? 1 : -1) * force * 8;
                } else {
                    this.mouseInfluence *= 0.9;
                }

                this.shimmer += this.shimmerSpeed;
                this.pulse += this.pulseSpeed;
            }

            draw() {
                if (this.startIndex >= verticalLines.length || this.endIndex >= verticalLines.length) return;

                const startX = verticalLines[this.startIndex].x;
                const endX = verticalLines[this.endIndex].x;

                const alpha = 0.3 + Math.sin(this.shimmer) * 0.15;
                const glowAlpha = this.mouseInfluence * 0.5;

                // Hauptverbindung
                ctx.beginPath();
                ctx.moveTo(startX, this.y);
                ctx.lineTo(endX, this.y);
                ctx.strokeStyle = `rgba(102, 153, 204, ${alpha})`;
                ctx.lineWidth = 1;
                ctx.stroke();

                // Glow-Effekt
                if (this.mouseInfluence > 0.1) {
                    ctx.beginPath();
                    ctx.moveTo(startX, this.y);
                    ctx.lineTo(endX, this.y);
                    ctx.strokeStyle = `rgba(173, 216, 230, ${glowAlpha})`;
                    ctx.lineWidth = 2.5;
                    ctx.stroke();
                }

                // Schimmer-Punkte an Kreuzungen
                if (Math.random() < 0.005) {
                    shimmerPoints.push({
                        x: startX,
                        y: this.y,
                        life: 1,
                        decay: 0.02
                    });
                    shimmerPoints.push({
                        x: endX,
                        y: this.y,
                        life: 1,
                        decay: 0.02
                    });
                }
            }
        }

        class ShimmerPoint {
            constructor(x, y) {
                this.x = x;
                this.y = y;
                this.life = 1;
                this.decay = 0.015;
                this.size = Math.random() * 3 + 1;
                this.pulse = Math.random() * Math.PI * 2;
            }

            update() {
                this.life -= this.decay;
                this.pulse += 0.2;
            }

            draw() {
                if (this.life <= 0) return;

                const pulseSize = this.size + Math.sin(this.pulse) * 0.5;

                ctx.save();
                ctx.globalAlpha = this.life;

                // Äußerer Glow
                ctx.fillStyle = 'rgba(173, 216, 230, 0.3)';
                ctx.beginPath();
                ctx.arc(this.x, this.y, pulseSize * 2, 0, Math.PI * 2);
                ctx.fill();

                // Innerer Punkt
                ctx.fillStyle = '#add8e6';
                ctx.beginPath();
                ctx.arc(this.x, this.y, pulseSize, 0, Math.PI * 2);
                ctx.fill();

                ctx.restore();
            }
        }

        function initNetwork() {
            verticalLines = [];
            crossConnections = [];

            // Vertikale Linien erstellen
            const lineSpacing = 80;
            const numLines = Math.ceil(width / lineSpacing) + 2;

            for (let i = 0; i < numLines; i++) {
                const x = i * lineSpacing - lineSpacing / 2;
                verticalLines.push(new VerticalLine(x));
            }

            // Querverbindungen erstellen
            const connectionSpacing = 60;
            const numConnections = Math.ceil(height / connectionSpacing);

            for (let i = 0; i < numConnections; i++) {
                const y = i * connectionSpacing + 30;

                // Verschiedene Verbindungstypen
                for (let j = 0; j < verticalLines.length - 1; j++) {
                    if (Math.random() < 0.7) { // Benachbarte Linien verbinden
                        crossConnections.push(new CrossConnection(y, j, j + 1));
                    }
                    if (Math.random() < 0.3 && j < verticalLines.length - 2) { // Überspringen einer Linie
                        crossConnections.push(new CrossConnection(y + 15, j, j + 2));
                    }
                }
            }
        }

        function animate() {
            // Hintergrund mit leichtem Trails-Effekt
            ctx.fillStyle = 'rgba(10, 14, 26, 0.05)';
            ctx.fillRect(0, 0, width, height);

            time += 0.016;

            // Update und zeichne vertikale Linien
            verticalLines.forEach(line => {
                line.update();
                line.draw();
            });

            // Update und zeichne Querverbindungen
            crossConnections.forEach(connection => {
                connection.update();
                connection.draw();
            });

            // Update und zeichne Schimmerpunkte
            for (let i = shimmerPoints.length - 1; i >= 0; i--) {
                const point = shimmerPoints[i];
                point.life -= point.decay;

                if (point.life <= 0) {
                    shimmerPoints.splice(i, 1);
                } else {
                    const pulseSize = point.size + Math.sin(time * 10 + point.x * 0.01) * 0.5;

                    ctx.save();
                    ctx.globalAlpha = point.life;

                    ctx.fillStyle = 'rgba(173, 216, 230, 0.3)';
                    ctx.beginPath();
                    ctx.arc(point.x, point.y, pulseSize * 2, 0, Math.PI * 2);
                    ctx.fill();

                    ctx.fillStyle = '#add8e6';
                    ctx.beginPath();
                    ctx.arc(point.x, point.y, pulseSize, 0, Math.PI * 2);
                    ctx.fill();

                    ctx.restore();
                }
            }

            requestAnimationFrame(animate);
        }

        // Event listeners
        window.addEventListener('resize', () => {
            resize();
            initNetwork();
        });

        canvas.addEventListener('mousemove', (e) => {
            mouseX = e.clientX;
            mouseY = e.clientY;
        });

        // Schimmerpunkte bei Mausklick hinzufügen
        canvas.addEventListener('click', (e) => {
            for (let i = 0; i < 5; i++) {
                shimmerPoints.push({
                    x: e.clientX + (Math.random() - 0.5) * 20,
                    y: e.clientY + (Math.random() - 0.5) * 20,
                    life: 1,
                    decay: 0.01,
                    size: Math.random() * 4 + 2
                });
            }
        });

        // Initialize
        resize();
        initNetwork();
        animate();
    }, 1000)
})
