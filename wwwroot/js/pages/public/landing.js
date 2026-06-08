/* ═══════════════════════════════════════════════════
   Landing — Wave Grid 3D + Aurora + GSAP
   ═══════════════════════════════════════════════════ */
(function () {
    'use strict';
    gsap.registerPlugin(ScrollTrigger);

    /* ── wait for THREE to be available ── */
    function ready(fn) {
        if (document.readyState !== 'loading') fn();
        else document.addEventListener('DOMContentLoaded', fn);
    }

    ready(function () {
        initAurora();
        initWebGL();
        initCursor();
        initMagnetic();
        initGsap();
    });

    /* ══════════════════════════════════════════════
       AURORA — 2D blobs for sections below hero
    ══════════════════════════════════════════════ */
    function initAurora() {
        const c = document.getElementById('bg-canvas');
        if (!c) return;
        const ctx = c.getContext('2d');
        const resize = () => { c.width = window.innerWidth; c.height = window.innerHeight; };
        resize();
        window.addEventListener('resize', resize);

        const orbs = [
            { bx:.22, by:.4,  r:.5,  rgb:[255,107,53], px:0,   py:0,   sp:.00010 },
            { bx:.75, by:.55, r:.55, rgb:[100,60,240],  px:2.1, py:1.4, sp:.00008 },
            { bx:.5,  by:.15, r:.4,  rgb:[170,40,220],  px:4.8, py:3.2, sp:.00013 },
            { bx:.12, by:.8,  r:.45, rgb:[30,120,255],  px:1.7, py:5.9, sp:.00010 },
            { bx:.88, by:.28, r:.35, rgb:[255,160,30],  px:3.4, py:2.8, sp:.00012 },
        ];
        const mood = { pu:0, bl:0, sc:1 };
        [
            { s:'.stats-section',    t:{ pu:.3,  bl:.1,  sc:1.1 } },
            { s:'.section-dark',     t:{ pu:.1,  bl:.4,  sc:1.15} },
            { s:'.cta-section',      t:{ pu:.6,  bl:.2,  sc:1.3 } },
        ].forEach(({ s, t }) => {
            const el = document.querySelector(s);
            if (!el) return;
            ScrollTrigger.create({ trigger:el, start:'top 60%', end:'bottom 40%',
                onEnter:    ()=>gsap.to(mood,{...t,             duration:2}),
                onLeave:    ()=>gsap.to(mood,{pu:0,bl:0,sc:1,  duration:2}),
                onEnterBack:()=>gsap.to(mood,{...t,             duration:2}),
                onLeaveBack:()=>gsap.to(mood,{pu:0,bl:0,sc:1,  duration:2}),
            });
        });

        const t0 = performance.now();
        (function loop() {
            requestAnimationFrame(loop);
            const t = (performance.now() - t0) * 0.001;
            const W = c.width, H = c.height;

            ctx.fillStyle = 'rgba(7,7,26,.15)';
            ctx.fillRect(0, 0, W, H);

            orbs.forEach(o => {
                const x = (o.bx + .26*Math.sin(t*o.sp*2   +o.px)) * W;
                const y = (o.by + .20*Math.cos(t*o.sp*1.55 +o.py)) * H;
                const r = o.r * Math.max(W,H) * mood.sc;
                let [rv,gv,bv] = o.rgb;
                bv = Math.min(255, bv + mood.bl*150);
                rv = Math.max(0,   rv - mood.bl*70);
                bv = Math.min(255, bv + mood.pu*110);
                gv = Math.max(0,   gv - mood.pu*50);

                const g = ctx.createRadialGradient(x,y,0, x,y,r);
                g.addColorStop(0,  `rgba(${rv},${gv},${bv},.17)`);
                g.addColorStop(.5, `rgba(${rv},${gv},${bv},.07)`);
                g.addColorStop(1,  `rgba(${rv},${gv},${bv},0)`);
                ctx.beginPath();
                ctx.arc(x,y,r,0,Math.PI*2);
                ctx.fillStyle = g;
                ctx.fill();
            });
        })();
    }

    /* ══════════════════════════════════════════════
       THREE.JS — 3D WAVE GRID
       Grid of glowing dots displaced by sine waves
       + mouse ripple + scroll camera
    ══════════════════════════════════════════════ */
    function initWebGL() {
        const canvas = document.getElementById('hero-canvas');
        if (!canvas) return;
        if (typeof THREE === 'undefined') {
            console.error('[landing] THREE not loaded');
            return;
        }

        /* ── Shaders ── */
        const VERT = `
            attribute float aSize;
            uniform   float uTime;
            uniform   vec2  uMouse;
            varying   float vElev;

            void main(){
                vec3 p = position;

                // multi-layer sine wave surface
                float z = sin(p.x*0.22 + uTime*0.70)*2.2
                        + sin(p.y*0.28 + uTime*0.55)*1.8
                        + sin((p.x+p.y)*0.11 + uTime*0.40)*1.4
                        + sin(p.x*0.07 - uTime*0.32)*0.9;

                // mouse ripple
                float d = distance(p.xy, uMouse * 30.0);
                z += sin(d*0.38 - uTime*3.5) * exp(-d*0.09) * 4.0;

                p.z    = z;
                vElev  = z;

                vec4 mv = modelViewMatrix * vec4(p, 1.0);
                gl_PointSize = aSize * (280.0 / -mv.z);
                gl_Position  = projectionMatrix * mv;
            }
        `;
        const FRAG = `
            varying float vElev;
            void main(){
                float d = distance(gl_PointCoord, vec2(.5));
                if(d > .5) discard;
                float a = pow(1.0 - smoothstep(0.,.5,d), 1.6);

                // valley = indigo, peak = orange
                float t  = clamp((vElev + 3.5) / 8.0, 0.0, 1.0);
                vec3 col = mix(vec3(0.26,0.16,0.92), vec3(1.0,0.42,0.12), t);

                gl_FragColor = vec4(col, a * 0.92);
            }
        `;

        /* ── Scene & renderer ── */
        const W = window.innerWidth, H = window.innerHeight;
        const scene    = new THREE.Scene();
        scene.fog      = new THREE.FogExp2(0x07071a, 0.010);

        const camera   = new THREE.PerspectiveCamera(52, W/H, 0.1, 300);
        camera.position.set(0, -20, 22);
        camera.lookAt(0, 2, 0);

        const renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        renderer.setSize(W, H);
        renderer.setClearColor(0x07071a, 1);

        /* ── Grid geometry ── */
        const isMobile = W < 768;
        const COLS = isMobile ? 55 : 110;
        const ROWS = isMobile ? 38 : 72;
        const TOTAL = COLS * ROWS;
        const SX = 70 / COLS, SY = 48 / ROWS;

        const pos  = new Float32Array(TOTAL * 3);
        const size = new Float32Array(TOTAL);

        for (let r = 0; r < ROWS; r++) {
            for (let c = 0; c < COLS; c++) {
                const i = r * COLS + c;
                pos[i*3]   = (c - COLS/2) * SX;
                pos[i*3+1] = (r - ROWS/2) * SY;
                pos[i*3+2] = 0;
                size[i]    = 1.2 + Math.random() * 1.6;
            }
        }

        const geom = new THREE.BufferGeometry();
        geom.setAttribute('position', new THREE.BufferAttribute(pos,  3));
        geom.setAttribute('aSize',    new THREE.BufferAttribute(size, 1));

        const uTime  = { value: 0 };
        const uMouse = { value: new THREE.Vector2(0, 0) };

        const mat = new THREE.ShaderMaterial({
            vertexShader:   VERT,
            fragmentShader: FRAG,
            uniforms: { uTime, uMouse },
            transparent: true,
            depthWrite:  false,
            blending:    THREE.AdditiveBlending,
        });

        scene.add(new THREE.Points(geom, mat));

        /* ── Ambient floating dust ── */
        const DUST = 180;
        const dp = new Float32Array(DUST*3), ds = new Float32Array(DUST);
        for(let i=0;i<DUST;i++){
            dp[i*3]  =(Math.random()-.5)*80;
            dp[i*3+1]=(Math.random()-.5)*60;
            dp[i*3+2]= Math.random()*8;
            ds[i] = .8 + Math.random()*.8;
        }
        const dg = new THREE.BufferGeometry();
        dg.setAttribute('position', new THREE.BufferAttribute(dp,3));
        dg.setAttribute('aSize',    new THREE.BufferAttribute(ds,1));
        scene.add(new THREE.Points(dg, new THREE.ShaderMaterial({
            vertexShader:`
                attribute float aSize;
                uniform float uTime;
                void main(){
                    vec3 p=position;
                    p.z += sin(uTime*.4+p.x*.3)*0.5;
                    vec4 mv=modelViewMatrix*vec4(p,1.);
                    gl_PointSize=aSize*(200./-mv.z);
                    gl_Position=projectionMatrix*mv;
                }`,
            fragmentShader:`
                void main(){
                    float d=distance(gl_PointCoord,vec2(.5));
                    if(d>.5) discard;
                    float a=pow(1.-smoothstep(0.,.5,d),2.);
                    gl_FragColor=vec4(.6,.35,.95,a*.4);
                }`,
            uniforms:{ uTime },
            transparent:true, depthWrite:false, blending:THREE.AdditiveBlending
        })));

        /* ── GSAP scroll ── */
        const hero = document.querySelector('.hero');
        if(hero){
            gsap.to(camera.position, {
                z:10, y:-8,
                ease:'none',
                scrollTrigger:{ trigger:hero, start:'top top', end:'bottom top', scrub:1.8 }
            });
            gsap.to(camera.rotation, {
                x:.22,
                ease:'none',
                scrollTrigger:{ trigger:hero, start:'top top', end:'bottom top', scrub:1.8 }
            });
        }

        /* ── Mouse: ripple + camera shift ── */
        document.addEventListener('mousemove', e => {
            if(window.scrollY > window.innerHeight) return;
            const nx = (e.clientX/W - .5)*2;
            const ny = (e.clientY/H - .5)*2;
            gsap.to(uMouse.value, { x:nx, y:-ny, duration:1.2, ease:'power2.out' });
            gsap.to(camera.position, { x:nx*3, duration:2.5, ease:'power2.out', overwrite:'auto' });
        });

        /* ── Resize ── */
        window.addEventListener('resize', () => {
            const w=window.innerWidth, h=window.innerHeight;
            camera.aspect=w/h;
            camera.updateProjectionMatrix();
            renderer.setSize(w,h);
        });

        /* ── Render loop ── */
        const clock = new THREE.Clock();
        (function loop() {
            requestAnimationFrame(loop);
            uTime.value = clock.getElapsedTime();
            renderer.render(scene, camera);
        })();
    }

    /* ══════════════════════════════════════════════
       CUSTOM CURSOR
    ══════════════════════════════════════════════ */
    function initCursor() {
        const dot=document.getElementById('cursor-dot');
        const ring=document.getElementById('cursor-ring');
        if(!dot||!ring) return;
        document.addEventListener('mousemove',e=>{
            gsap.to(dot, { x:e.clientX,y:e.clientY,duration:.08 });
            gsap.to(ring,{ x:e.clientX,y:e.clientY,duration:.30,ease:'power2.out' });
        });
        document.querySelectorAll('a,button,.tilt-card,.cat-chip').forEach(el=>{
            el.addEventListener('mouseenter',()=>gsap.to(ring,{scale:2.2,opacity:.4,duration:.2}));
            el.addEventListener('mouseleave',()=>gsap.to(ring,{scale:1,opacity:1,duration:.2}));
        });
        document.addEventListener('mousedown',()=>gsap.to(dot,{scale:.5,duration:.1}));
        document.addEventListener('mouseup',  ()=>gsap.to(dot,{scale:1, duration:.1}));
    }

    /* ══════════════════════════════════════════════
       MAGNETIC BUTTONS
    ══════════════════════════════════════════════ */
    function initMagnetic() {
        document.querySelectorAll('.btn-magnetic').forEach(btn=>{
            btn.addEventListener('mousemove',e=>{
                const r=btn.getBoundingClientRect();
                gsap.to(btn,{
                    x:(e.clientX-r.left-r.width/2)*.38,
                    y:(e.clientY-r.top-r.height/2)*.38,
                    duration:.5, ease:'power2.out'
                });
            });
            btn.addEventListener('mouseleave',()=>
                gsap.to(btn,{x:0,y:0,duration:.9,ease:'elastic.out(1,.4)'}));
        });
    }

    /* ══════════════════════════════════════════════
       GSAP PAGE ANIMATIONS
    ══════════════════════════════════════════════ */
    function initGsap() {
        /* hero */
        gsap.timeline({delay:.1})
            .from('.hero-badge', {opacity:0,y:20,scale:.88,duration:.6,ease:'back.out(2)'})
            .from('.hero-title', {opacity:0,y:55,duration:.95,ease:'power3.out'},'-=.1')
            .from('.hero-sub',   {opacity:0,y:30,duration:.85,ease:'power3.out'},'-=.6')
            .from('.hero-btns',  {opacity:0,y:18,scale:.95,duration:.7,ease:'back.out(1.5)'},'-=.5')
            .from('.hero-scroll',{opacity:0,duration:.5},'-=.3');

        /* reveals */
        gsap.utils.toArray('.reveal').forEach(el=>
            gsap.from(el,{opacity:0,y:45,duration:.9,ease:'power3.out',
                scrollTrigger:{trigger:el,start:'top 88%'}}));

        /* stagger */
        gsap.utils.toArray('.stagger-group').forEach(g=>{
            const items=g.querySelectorAll('.stagger-item');
            if(!items.length) return;
            gsap.from(items,{opacity:0,y:45,scale:.93,duration:.75,
                stagger:{amount:.5,ease:'power2.out'},ease:'power3.out',
                scrollTrigger:{trigger:g,start:'top 84%'}});
        });

        /* counters */
        document.querySelectorAll('.counter').forEach(el=>{
            const target=parseInt(el.getAttribute('data-target')||'0',10);
            ScrollTrigger.create({trigger:el,start:'top 88%',once:true,
                onEnter(){
                    const o={v:0};
                    gsap.to(o,{v:target,duration:2.5,ease:'power3.out',
                        onUpdate(){el.textContent=Math.floor(o.v).toLocaleString('fa-IR');}});
                }
            });
        });

        /* tilt */
        document.querySelectorAll('.tilt-card').forEach(card=>{
            card.addEventListener('mousemove',e=>{
                const r=card.getBoundingClientRect();
                gsap.to(card,{
                    rotateY:((e.clientX-r.left)/r.width -.5)*14,
                    rotateX:-((e.clientY-r.top) /r.height-.5)*14,
                    transformPerspective:700,scale:1.04,duration:.35,ease:'power2.out'
                });
            });
            card.addEventListener('mouseleave',()=>
                gsap.to(card,{rotateX:0,rotateY:0,scale:1,duration:.8,ease:'elastic.out(1,.4)'}));
        });

        /* section label flicker */
        gsap.utils.toArray('.section-label').forEach(el=>{
            ScrollTrigger.create({trigger:el,start:'top 86%',once:true,onEnter(){
                gsap.fromTo(el,{opacity:.1},
                    {opacity:1,duration:.45,ease:'power2.out',repeat:3,yoyo:true,
                     onComplete(){gsap.set(el,{opacity:1,textShadow:'0 0 8px rgba(255,107,53,.35)'});}});
            }});
        });

        /* badge parallax */
        gsap.to('.hero-badge',{y:-35,
            scrollTrigger:{trigger:'.hero',start:'top top',end:'bottom top',scrub:1.2}});
    }

})();
