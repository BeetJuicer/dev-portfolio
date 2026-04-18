// ─── SHOWCASE CARD EXPAND/COLLAPSE ───
function toggleCard(header) {
  const card = header.closest('.showcase-card');
  card.classList.toggle('open');

  const body = card.querySelector('.card-body');
  if (card.classList.contains('open')) {
    body.addEventListener('transitionend', () => {
      body.style.overflow = 'visible';
    }, { once: true });
  } else {
    body.style.overflow = 'hidden';
  }

  // After opening, load any unloaded videos inside
  const video = header.nextElementSibling.querySelector('video');
  if (video) {
    const source = video.querySelector('source[data-src]');
    if (source) {
      source.src = source.dataset.src;     // set the real src
      source.removeAttribute('data-src');  // mark as loaded
      video.load();                        // trigger load
      video.play();
    }
  }
}

// ─── SCRIPTS PANEL TOGGLE ───
function toggleScripts(btn) {
  btn.classList.toggle('open');
  btn.closest('.scripts-toggle').nextElementSibling.classList.toggle('visible');
}

function initShowcase() {
  // ─── AUTO-BUILD SCRIPT TABS ───
  document.querySelectorAll('.scripts-panel[data-project]').forEach(panel => {
    const project = panel.getAttribute('data-project');
    const base = `cs-scripts/${project}`;
    const tabBar = panel.querySelector('.file-tabs');

    fetch(`${base}/manifest.json`)
      .then(r => r.json())
      .then(files => {
        files.forEach((filename, i) => {
          const tab = document.createElement('button');
          tab.className = 'file-tab' + (i === 0 ? ' active' : '');
          tab.textContent = filename;

          const content = document.createElement('div');
          content.className = 'file-content' + (i === 0 ? ' active' : '');
          const pre = document.createElement('pre');
          const code = document.createElement('code');
          code.textContent = 'Loading...';
          pre.appendChild(code);
          content.appendChild(pre);

          tab.onclick = () => {
            panel.querySelectorAll('.file-tab').forEach(t => t.classList.remove('active'));
            panel.querySelectorAll('.file-content').forEach(c => c.classList.remove('active'));
            tab.classList.add('active');
            content.classList.add('active');
          };

          tabBar.appendChild(tab);
          panel.appendChild(content);

          fetch(`${base}/${filename}`)
            .then(r => r.ok ? r.text() : Promise.reject())
            .then(text => {
              code.textContent = text;
              hljs.highlightElement(code);
            })
            .catch(() => { code.textContent = `// Could not load ${filename}`; });
        });
      })
      .catch(() => { tabBar.textContent = '// No manifest found'; });
  });
}

function initReveal(){

  // ─── ACTIVE NAV HIGHLIGHT ───
  const navObs = new IntersectionObserver(entries => {
    entries.forEach(e => {
      if (e.isIntersecting) {
        document.querySelectorAll('nav a').forEach(a => a.classList.remove('active'));
        const active = document.querySelector(`nav a[href="#${e.target.id}"]`);
        if (active) active.classList.add('active');
      }
    });
  }, { threshold: 0.3 });
  document.querySelectorAll('section[id]').forEach(s => navObs.observe(s));
  
  // ─── SCROLL REVEAL ───
  document.querySelectorAll('.project-row').forEach((el, i) => {
    el.classList.add('reveal');
    el.style.setProperty('--i', i);
  });
  document.querySelectorAll('.showcase-card').forEach((el, i) => {
    el.classList.add('reveal');
    el.style.setProperty('--i', i);
  });
  document.querySelectorAll('.section-head').forEach(el => el.classList.add('reveal'));
  
  const revealObs = new IntersectionObserver(entries => {
    entries.forEach(e => { if (e.isIntersecting) e.target.classList.add('visible'); });
  }, { threshold: 0.12 });
  document.querySelectorAll('.reveal').forEach(el => revealObs.observe(el));
}
  
  // ─── BACKGROUND CANVAS ANIMATION ───
  (function () {
    const canvas = document.getElementById('bg-canvas');
    const ctx = canvas.getContext('2d');
    const isDark = () => window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    const BALLS = 8;
    let W, H, balls = [];
    
    function resize() {
      W = canvas.width = window.innerWidth;
      H = canvas.height = window.innerHeight;
    }
    
    function makeBall() {
      const r = 40 + Math.random() * 90;
      return {
        x: Math.random() * W,
        y: Math.random() * H,
        r,
        vx: (Math.random() - 0.5) * 0.7,
        vy: (Math.random() - 0.5) * 0.7,
        hue: Math.random() < 0.5
          ? 150 + Math.random() * 30
          : 190 + Math.random() * 40,
        alpha: 0.06 + Math.random() * 0.08,
        phase: Math.random() * Math.PI * 2,
        speed: 0.004 + Math.random() * 0.003
      };
    }

    resize();
    for (let i = 0; i < BALLS; i++) balls.push(makeBall());
    window.addEventListener('resize', resize);

    let frame = 0;
    function draw() {
      ctx.clearRect(0, 0, W, H);
      const dark = isDark();
      frame++;
      balls.forEach(b => {
        b.x += b.vx;
        b.y += b.vy;
        if (b.x < -b.r) b.x = W + b.r;
        if (b.x > W + b.r) b.x = -b.r;
        if (b.y < -b.r) b.y = H + b.r;
        if (b.y > H + b.r) b.y = -b.r;

        const pulse = 1 + 0.06 * Math.sin(frame * b.speed + b.phase);
        const rad = b.r * pulse;
        const alpha = b.alpha * (dark ? 0.7 : 1);

        const grad = ctx.createRadialGradient(
          b.x - rad * 0.25, b.y - rad * 0.25, rad * 0.05,
          b.x, b.y, rad
        );
        const sat = dark ? '35%' : '55%';
        const light = dark ? '55%' : '72%';
        grad.addColorStop(0,   `hsla(${b.hue},${sat},${light},${alpha * 1.6})`);
        grad.addColorStop(0.5, `hsla(${b.hue},${sat},${light},${alpha})`);
        grad.addColorStop(1,   `hsla(${b.hue},${sat},${light},0)`);

        ctx.beginPath();
        ctx.arc(b.x, b.y, rad, 0, Math.PI * 2);
        ctx.fillStyle = grad;
        ctx.fill();
      });
      requestAnimationFrame(draw);
    }
    draw();
  })();

  