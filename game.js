class Game2048 {
    constructor() {
        this.score = 0;
        this.board = [];
        this.tiles = []; 
        this.isMoving = false; 
        this.moveSpeed = 140; // Синхронизировано с CSS (--animation-speed)

        this.initDOM();
        this.loadSettingsInputs(); 
        this.applySettings(true);  
        this.setupInput();
    }

    initDOM() {
        this.gridContainer = document.getElementById('grid-container');
        this.scoreEl = document.getElementById('score');
        this.bestScoreEl = document.getElementById('best-score');
        this.overlay = document.getElementById('game-overlay');
        this.overlayText = document.getElementById('overlay-text');
    }

    loadSettingsInputs() {
        const syncSliderText = (sliderId, valId, isPowerOfTwo = false) => {
            const input = document.getElementById(sliderId);
            const display = document.getElementById(valId);
            
            const update = () => {
                let v = input.value;
                if (isPowerOfTwo) {
                    display.innerText = Math.pow(2, v);
                } else {
                    display.innerText = v + (sliderId === 'setup-chance' ? '%' : '');
                }
            };

            input.addEventListener('input', () => {
                update();
                if (sliderId === 'setup-hue') {
                    document.documentElement.style.setProperty('--base-hue', input.value);
                    this.updateTileColorsCSS();
                }
            });

            update(); 
        };

        syncSliderText('setup-cols', 'val-cols');
        syncSliderText('setup-rows', 'val-rows');
        syncSliderText('setup-chance', 'val-chance');
        syncSliderText('setup-max', 'val-max', true);
        syncSliderText('setup-hue', 'val-hue');
    }

    applySettings(isFirstRun = false) {
        this.cols = parseInt(document.getElementById('setup-cols').value);
        this.rows = parseInt(document.getElementById('setup-rows').value);
        this.chanceOf2 = parseInt(document.getElementById('setup-chance').value) / 100;
        this.targetValue = Math.pow(2, parseInt(document.getElementById('setup-max').value));
        this.baseHue = parseInt(document.getElementById('setup-hue').value);
        
        document.documentElement.style.setProperty('--base-hue', this.baseHue);
        
        this.restart();
    }

    restart() {
        this.score = 0;
        this.updateScore(0);
        this.loadBestScore();
        this.overlay.classList.remove('active');
        this.isMoving = false;
        
        this.buildGridSystem();
        this.board = Array(this.rows).fill(null).map(() => Array(this.cols).fill(0));
        
        this.tiles.forEach(t => t.el.remove());
        this.tiles = [];

        this.addRandomTile();
        this.addRandomTile();
    }

    buildGridSystem() {
        this.gridContainer.querySelectorAll('.grid-cell').forEach(c => c.remove());
        this.gridContainer.style.gridTemplateColumns = `repeat(${this.cols}, 1fr)`;
        this.gridContainer.style.gridTemplateRows = `repeat(${this.rows}, 1fr)`;
        
        for (let i = 0; i < this.rows * this.cols; i++) {
            const cell = document.createElement('div');
            cell.classList.add('grid-cell');
            this.gridContainer.appendChild(cell);
        }
    }

    getLayout() {
        const padding = 12;
        const gap = 12;
        const width = this.gridContainer.clientWidth;
        const height = this.gridContainer.clientHeight;
        const cellWidth = (width - padding * 2 - gap * (this.cols - 1)) / this.cols;
        const cellHeight = (height - padding * 2 - gap * (this.rows - 1)) / this.rows;
        return { padding, gap, cellWidth, cellHeight };
    }

    getPos(r, c) {
        const layout = this.getLayout();
        const x = layout.padding + c * (layout.cellWidth + layout.gap);
        const y = layout.padding + r * (layout.cellHeight + layout.gap);
        return { x, y };
    }

    addRandomTile() {
        const emptyCells = [];
        for (let r = 0; r < this.rows; r++) {
            for (let c = 0; c < this.cols; c++) {
                if (this.board[r][c] === 0) emptyCells.push({r, c});
            }
        }
        if (emptyCells.length > 0) {
            const {r, c} = emptyCells[Math.floor(Math.random() * emptyCells.length)];
            const value = Math.random() < this.chanceOf2 ? 2 : 4;
            this.board[r][c] = value;
            this.createTileDOM(r, c, value, true);
        }
    }

    createTileDOM(r, c, value, isNew = false) {
        const layout = this.getLayout();
        const pos = this.getPos(r, c);
        
        const tileEl = document.createElement('div');
        tileEl.classList.add('tile');
        if (isNew) tileEl.classList.add('spawn');
        tileEl.innerText = value;
        
        tileEl.style.setProperty('--target-x', `${pos.x}px`);
        tileEl.style.setProperty('--target-y', `${pos.y}px`);
        tileEl.style.transform = `translate(${pos.x}px, ${pos.y}px)`;
        
        tileEl.style.width = `${layout.cellWidth}px`;
        tileEl.style.height = `${layout.cellHeight}px`;
        this.updateTileStyle(tileEl, value);

        this.gridContainer.appendChild(tileEl);
        
        const tileObj = { el: tileEl, r, c, value, merged: false };
        this.tiles.push(tileObj);

        if(isNew) {
            setTimeout(() => tileEl.classList.remove('spawn'), 150);
        }
    }

    updateTileStyle(el, value) {
        const power = Math.log2(value);
        const hueShift = (power - 1) * 18; 
        const currentHue = (this.baseHue + hueShift) % 360;
        const saturation = 70 + (power * 2); 
        const lightness = Math.max(25, 65 - (power * 3.5));

        el.style.backgroundColor = `hsl(${currentHue}, ${Math.min(saturation, 100)}%, ${lightness}%)`;
        el.style.color = lightness > 50 ? '#111' : '#fff';
        el.style.boxShadow = power > 4 ? `0 0 ${power * 4}px hsl(${currentHue}, 90%, 60%)` : 'none';
        
        if (value >= 10000) el.style.fontSize = '1.2rem';
        else if (value >= 1000) el.style.fontSize = '1.5rem';
        else el.style.fontSize = '2rem';
    }

    updateTileColorsCSS() {
        this.tiles.forEach(tile => this.updateTileStyle(tile.el, tile.value));
    }

    getTileAt(r, c) {
        return this.tiles.find(t => t.r === r && t.c === c && !t.merged);
    }

    animateTileMove(tileObj, newR, newC) {
        const pos = this.getPos(newR, newC);
        tileObj.el.style.setProperty('--target-x', `${pos.x}px`);
        tileObj.el.style.setProperty('--target-y', `${pos.y}px`);
        tileObj.el.style.transform = `translate(${pos.x}px, ${pos.y}px)`;
        tileObj.r = newR;
        tileObj.c = newC;
    }

    recalculateLayout() {
        const layout = this.getLayout();
        this.tiles.forEach(tile => {
            tile.el.style.width = `${layout.cellWidth}px`;
            tile.el.style.height = `${layout.cellHeight}px`;
            this.animateTileMove(tile, tile.r, tile.c);
        });
    }

    setupInput() {
        // --- 1. ОБРАБОТКА КЛАВИАТУРЫ ---
        window.addEventListener('keydown', (e) => {
            if (this.isMoving || this.overlay.classList.contains('active')) return;
            let moved = false;
            switch(e.key) {
                case 'ArrowUp':    case 'w': case 'W': moved = this.move('up'); break;
                case 'ArrowDown':  case 's': case 'S': moved = this.move('down'); break;
                case 'ArrowLeft':  case 'a': case 'A': moved = this.move('left'); break;
                case 'ArrowRight': case 'd': case 'D': moved = this.move('right'); break;
                default: return;
            }
            if (moved) this.handleTurnCompletion();
        });

        // --- 2. ОБРАБОТКА СВАЙПОВ (МОБИЛЬНЫЕ ТЕЛЕФОНЫ) ---
        let touchStartX = 0;
        let touchStartY = 0;

        // Вешаем слушатель именно на игровое поле
        this.gridContainer.addEventListener('touchstart', (e) => {
            touchStartX = e.changedTouches[0].screenX;
            touchStartY = e.changedTouches[0].screenY;
        }, { passive: false });

        // Блокируем скролл страницы, пока палец движется по игровому полю
        this.gridContainer.addEventListener('touchmove', (e) => {
            e.preventDefault(); 
        }, { passive: false });

        this.gridContainer.addEventListener('touchend', (e) => {
            if (this.isMoving || this.overlay.classList.contains('active')) return;
            
            let touchEndX = e.changedTouches[0].screenX;
            let touchEndY = e.changedTouches[0].screenY;
            
            const diffX = touchEndX - touchStartX;
            const diffY = touchEndY - touchStartY;
            
            const absX = Math.abs(diffX);
            const absY = Math.abs(diffY);
            
            // Если свайп короче 30 пикселей - игнорируем (защита от случайных касаний)
            if (Math.max(absX, absY) < 30) return;
            
            let moved = false;

            // Определяем, по какой оси был свайп (где больше расстояние)
            if (absX > absY) {
                // Горизонтальный свайп
                moved = diffX > 0 ? this.move('right') : this.move('left');
            } else {
                // Вертикальный свайп
                moved = diffY > 0 ? this.move('down') : this.move('up');
            }
            
            if (moved) this.handleTurnCompletion();
        });
    }

    // Вынес завершение хода в отдельный метод, чтобы не дублировать код для клавиш и свайпов
    handleTurnCompletion() {
        this.isMoving = true;
        setTimeout(() => {
            this.addRandomTile();
            this.checkGameState();
            this.isMoving = false;
        }, this.moveSpeed);
    }

    move(direction) {
        let moved = false;
        const isVertical = direction === 'up' || direction === 'down';
        const isForward = direction === 'right' || direction === 'down';
        
        const primarySize = isVertical ? this.cols : this.rows;
        const secondarySize = isVertical ? this.rows : this.cols;

        this.tiles.forEach(t => t.merged = false);

        for (let i = 0; i < primarySize; i++) {
            for (let j = isForward ? secondarySize - 2 : 1; isForward ? j >= 0 : j < secondarySize; isForward ? j-- : j++) {
                
                const r = isVertical ? j : i;
                const c = isVertical ? i : j;
                
                if (this.board[r][c] === 0) continue;

                let currentR = r;
                let currentC = c;
                
                while (true) {
                    let nextR = currentR + (direction === 'up' ? -1 : direction === 'down' ? 1 : 0);
                    let nextC = currentC + (direction === 'left' ? -1 : direction === 'right' ? 1 : 0);

                    if (nextR < 0 || nextR >= this.rows || nextC < 0 || nextC >= this.cols) break;

                    const currentTile = this.getTileAt(currentR, currentC);
                    const nextValue = this.board[nextR][nextC];

                    if (nextValue === 0) {
                        this.board[nextR][nextC] = this.board[currentR][currentC];
                        this.board[currentR][currentC] = 0;
                        this.animateTileMove(currentTile, nextR, nextC);
                        currentR = nextR;
                        currentC = nextC;
                        moved = true;
                    } else if (nextValue === this.board[currentR][currentC]) {
                        const nextTile = this.getTileAt(nextR, nextC);
                        if (nextTile && !nextTile.merged) {
                            const newValue = nextValue * 2;
                            this.board[nextR][nextC] = newValue;
                            this.board[currentR][currentC] = 0;
                            
                            this.animateTileMove(currentTile, nextR, nextC);
                            currentTile.merged = true;
                            
                            setTimeout(() => {
                                nextTile.value = newValue;
                                nextTile.el.innerText = newValue;
                                nextTile.el.classList.add('merged');
                                this.updateTileStyle(nextTile.el, newValue);
                                
                                currentTile.el.remove();
                                this.tiles = this.tiles.filter(t => !t.merged);
                                
                                setTimeout(() => nextTile.el.classList.remove('merged'), 150);
                            }, this.moveSpeed);

                            this.updateScore(newValue);
                            moved = true;
                        }
                        break;
                    } else {
                        break;
                    }
                }
            }
        }
        return moved;
    }

    updateScore(points) {
        this.score += points;
        this.scoreEl.innerText = this.score;
        if (points > 0) {
            const container = document.getElementById('score-pop-container');
            const pop = document.createElement('div');
            pop.classList.add('score-pop');
            pop.innerText = `+${points}`;
            container.appendChild(pop);
            setTimeout(() => pop.remove(), 600);
        }
        if (this.score > this.bestScore) {
            this.bestScore = this.score;
            this.bestScoreEl.innerText = this.bestScore;
            localStorage.setItem(`2048_best_${this.rows}x${this.cols}`, this.bestScore);
        }
    }

    loadBestScore() {
        const saved = localStorage.getItem(`2048_best_${this.rows}x${this.cols}`);
        this.bestScore = saved ? parseInt(saved) : 0;
        this.bestScoreEl.innerText = this.bestScore;
    }

    checkGameState() {
        for (let r = 0; r < this.rows; r++)
            for (let c = 0; c < this.cols; c++)
                if (this.board[r][c] >= this.targetValue) { this.showOverlay("Победа!"); return; }

        for (let r = 0; r < this.rows; r++)
            for (let c = 0; c < this.cols; c++)
                if (this.board[r][c] === 0) return;

        for (let r = 0; r < this.rows; r++)
            for (let c = 0; c < this.cols; c++) {
                if (c < this.cols - 1 && this.board[r][c] === this.board[r][c + 1]) return;
                if (r < this.rows - 1 && this.board[r][c] === this.board[r + 1][c]) return;
            }
        this.showOverlay("Game Over");
    }

    showOverlay(text) {
        this.overlayText.innerText = text;
        this.overlayText.style.color = text === "Победа!" ? "var(--accent-color)" : "#ff4a4a";
        this.overlay.classList.add('active');
    }
}

// Глобальный запуск игры
let game;
window.onload = () => { game = new Game2048(); };
window.onresize = () => { if(game) game.recalculateLayout(); };