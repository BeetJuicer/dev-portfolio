// ─── LINKS ───
// Paste the City of Faith Google Play listing URL here. Empty = Play links fall back to itch.io.
const PLAY_STORE_URL = '';

function initPlayLinks() {
  document.querySelectorAll('[data-play-link]').forEach(a => {
    if (PLAY_STORE_URL) a.href = PLAY_STORE_URL;
    else if (a.hasAttribute('data-play-only')) a.remove();
  });
}

// Media panels marked data-optional-media disappear if their video file is missing
function initOptionalMedia() {
  document.querySelectorAll('[data-optional-media]').forEach(panel => {
    const source = panel.querySelector('source');
    const hide = () => panel.closest('.featured-game').classList.add('no-media');
    const video = panel.querySelector('video');
    if (!source || video.networkState === HTMLMediaElement.NETWORK_NO_SOURCE) return hide();
    source.addEventListener('error', hide);
  });
}

// ─── HERO REEL ───
// Clips the hero cycles through. Replace with a single demo reel once you have one.
const heroReel = [
  { src: 'preview-boids.mp4',   label: 'Boids · compute shaders' },
  { src: 'preview-bird.mp4',    label: 'Bird flight · FSM + splines' },
  { src: 'preview-time.mp4',    label: 'Time rewind · command pattern' },
  { src: 'preview-bullet.mp4',  label: 'Bullet hell · data-driven patterns' },
  { src: 'preview-possess.mp4', label: 'AI possession · actor / controller' },
];
const HERO_CLIP_SECONDS = 6;

function initHeroReel() {
  const video = document.getElementById('hero-video');
  const label = document.getElementById('hero-label');
  if (!video || !heroReel.length) return;

  let i = 0;
  let timer;
  const show = () => {
    const clip = heroReel[i];
    video.src = clip.src;
    label.textContent = clip.label;
    video.play().catch(() => {});
    clearTimeout(timer);
    timer = setTimeout(next, HERO_CLIP_SECONDS * 1000);
  };
  const next = () => { i = (i + 1) % heroReel.length; show(); };

  video.addEventListener('ended', next);
  show();
}

// ─── SHOWCASE GRID ───
function showcaseCard({ title, tags, description, youtubeId, projectKey, video }) {
  const tagPills = tags.map(t => `<span class="tag-pill">${t}</span>`).join('');

  const media = video
    ? `<video muted loop playsinline preload="none" poster="https://i.ytimg.com/vi/${youtubeId}/hqdefault.jpg" data-src="${video}"></video>`
    : `<img src="https://i.ytimg.com/vi/${youtubeId}/hqdefault.jpg" alt="${title}" loading="lazy" />`;

  return `
    <article class="showcase-card">
      <a class="showcase-media" href="https://www.youtube.com/watch?v=${youtubeId}" target="_blank" rel="noopener noreferrer" aria-label="Watch ${title} on YouTube">
        ${media}
        <span class="showcase-play">▶ Full video</span>
      </a>
      <div class="showcase-body">
        <h3 class="card-title">${title}</h3>
        <p class="card-desc">${description}</p>
        <div class="card-tags">${tagPills}</div>
        <button class="toggle-btn" onclick="openCode('${projectKey}', this)">
          <span class="scripts-icon">{&nbsp;}</span> View code
        </button>
      </div>
    </article>`;
}

function initShowcaseCards(data) {
  // Cards with a local clip first, so the grid opens on motion
  const ordered = [...data].sort((a, b) => !a.video - !b.video);
  document.getElementById('showcase-cards').innerHTML = ordered.map(showcaseCard).join('');

  const videos = document.querySelectorAll('.showcase-media video');
  const hoverCapable = window.matchMedia('(hover: hover)').matches;

  const load = v => {
    if (v.dataset.src) { v.src = v.dataset.src; v.removeAttribute('data-src'); }
  };

  if (hoverCapable) {
    // Desktop: play on hover
    videos.forEach(v => {
      const card = v.closest('.showcase-card');
      card.addEventListener('mouseenter', () => { load(v); v.play().catch(() => {}); });
      card.addEventListener('mouseleave', () => v.pause());
    });
  } else {
    // Touch: play while on screen
    const obs = new IntersectionObserver(entries => {
      entries.forEach(e => {
        if (e.isIntersecting) { load(e.target); e.target.play().catch(() => {}); }
        else e.target.pause();
      });
    }, { threshold: 0.6 });
    videos.forEach(v => obs.observe(v));
  }
}

// ─── CODE DIALOG ───
const codeCache = {};

function openCode(projectKey, btn) {
  const dialog = document.getElementById('code-dialog');
  const panel = document.getElementById('code-dialog-panel');
  const tabBar = panel.querySelector('.file-tabs');
  document.getElementById('code-dialog-title').textContent =
    btn.closest('.showcase-card').querySelector('.card-title').textContent;

  tabBar.innerHTML = '';
  panel.querySelectorAll('.file-content').forEach(c => c.remove());
  dialog.showModal();

  const base = `cs-scripts/${projectKey}`;
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
        code.className = 'language-csharp';
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

        const url = `${base}/${filename}`;
        const text = codeCache[url]
          ? Promise.resolve(codeCache[url])
          : fetch(url).then(r => r.ok ? r.text() : Promise.reject());
        text
          .then(t => {
            codeCache[url] = t;
            code.textContent = t;
            hljs.highlightElement(code);
          })
          .catch(() => { code.textContent = `// Could not load ${filename}`; });
      });
    })
    .catch(() => { tabBar.textContent = '// No manifest found'; });
}

// Click on the backdrop closes the dialog
document.addEventListener('click', e => {
  if (e.target.id === 'code-dialog') e.target.close();
});

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
  document.querySelectorAll('.featured-game, .game-card, .showcase-card').forEach(el => {
    el.classList.add('reveal');
    // Stagger siblings in the same grid row
    el.style.setProperty('--i', [...el.parentElement.children].indexOf(el) % 2);
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

