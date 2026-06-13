(function(){
    var c = document.getElementById('creator-canvas');
    if (!c) return;
    var cx = c.getContext('2d');

    function resize(){ c.width = innerWidth; c.height = innerHeight; }
    resize();
    window.addEventListener('resize', resize);

    var N = 65;
    var LINK = 155;
    var ps = [];

    function mkP() {
        var orange = Math.random() < 0.38;
        return {
            x:  Math.random() * c.width,
            y:  Math.random() * c.height,
            vx: (Math.random() - 0.5) * 0.28,
            vy: (Math.random() - 0.5) * 0.28,
            r:  Math.random() * 1.6 + 0.5,
            col: orange ? '249,115,22' : '180,210,255',
            op: Math.random() * 0.45 + 0.25,
            pulse: Math.random() * Math.PI * 2,
        };
    }

    for (var i = 0; i < N; i++) ps.push(mkP());

    var t = 0;
    function frame() {
        cx.clearRect(0, 0, c.width, c.height);
        t += 0.008;

        for (var i = 0; i < ps.length; i++) {
            for (var j = i + 1; j < ps.length; j++) {
                var dx = ps[i].x - ps[j].x;
                var dy = ps[i].y - ps[j].y;
                var d  = Math.sqrt(dx*dx + dy*dy);
                if (d < LINK) {
                    var a = (1 - d / LINK) * 0.18;
                    cx.beginPath();
                    cx.moveTo(ps[i].x, ps[i].y);
                    cx.lineTo(ps[j].x, ps[j].y);
                    cx.strokeStyle = 'rgba(249,115,22,' + a + ')';
                    cx.lineWidth   = 0.55;
                    cx.stroke();
                }
            }
        }

        for (var i = 0; i < ps.length; i++) {
            var p   = ps[i];
            var brt = p.op * (0.75 + 0.25 * Math.sin(t * 1.4 + p.pulse));

            var gr = cx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.r * 5);
            gr.addColorStop(0, 'rgba('+p.col+','+(brt * 0.5)+')');
            gr.addColorStop(1, 'rgba('+p.col+',0)');
            cx.beginPath(); cx.arc(p.x, p.y, p.r * 5, 0, 6.2832);
            cx.fillStyle = gr; cx.fill();

            cx.beginPath(); cx.arc(p.x, p.y, p.r, 0, 6.2832);
            cx.fillStyle = 'rgba('+p.col+','+brt+')'; cx.fill();

            p.x += p.vx; p.y += p.vy;
            if (p.x < -25) p.x = c.width  + 25;
            if (p.x > c.width  + 25) p.x = -25;
            if (p.y < -25) p.y = c.height + 25;
            if (p.y > c.height + 25) p.y = -25;
        }

        requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);
})();
