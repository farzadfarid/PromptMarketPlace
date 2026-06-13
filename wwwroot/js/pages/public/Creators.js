(function () {
    var c = document.getElementById('creators-canvas');
    if (!c) return;
    var cx = c.getContext('2d');

    function resize() { c.width = innerWidth; c.height = innerHeight; }
    resize();
    window.addEventListener('resize', resize);

    var COLS = [
        '249,115,22',
        '251,146,60',
        '253,186,116',
        '255,255,255',
    ];

    var pool = [];
    var N = 45;

    function mkEmber(fromBottom) {
        var x = Math.random() * c.width;
        return {
            x: x,
            y: fromBottom ? c.height + 10 : Math.random() * c.height,
            baseX: x,
            vy: -(Math.random() * 0.55 + 0.2),
            wobbleAmp: Math.random() * 18 + 6,
            wobbleFreq: Math.random() * 0.018 + 0.008,
            wobbleOff: Math.random() * Math.PI * 2,
            r: Math.random() * 1.8 + 0.5,
            col: COLS[Math.floor(Math.random() * COLS.length)],
            maxOp: Math.random() * 0.55 + 0.25,
            op: 0,
            age: 0,
        };
    }

    for (var i = 0; i < N; i++) pool.push(mkEmber(false));

    var t = 0;
    function frame() {
        cx.clearRect(0, 0, c.width, c.height);
        t += 1;

        for (var i = 0; i < pool.length; i++) {
            var p = pool[i];
            p.age++;

            /* fade in over 60 frames */
            var fadeIn  = Math.min(1, p.age / 60);
            /* fade out top 15% of screen */
            var fadeOut = Math.min(1, p.y / (c.height * 0.15));
            p.op = p.maxOp * fadeIn * fadeOut;

            if (p.op > 0.01) {
                /* soft glow */
                var gr = cx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.r * 5);
                gr.addColorStop(0, 'rgba(' + p.col + ',' + (p.op * 0.6) + ')');
                gr.addColorStop(1, 'rgba(' + p.col + ',0)');
                cx.beginPath();
                cx.arc(p.x, p.y, p.r * 5, 0, 6.2832);
                cx.fillStyle = gr;
                cx.fill();

                /* core */
                cx.beginPath();
                cx.arc(p.x, p.y, p.r, 0, 6.2832);
                cx.fillStyle = 'rgba(' + p.col + ',' + p.op + ')';
                cx.fill();
            }

            p.y += p.vy;
            p.x  = p.baseX + Math.sin(t * p.wobbleFreq + p.wobbleOff) * p.wobbleAmp;

            /* respawn when off top */
            if (p.y < -15) {
                var fresh = mkEmber(true);
                pool[i] = fresh;
            }
        }

        requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);
})();
