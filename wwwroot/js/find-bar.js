(function () {
    if (window.__tbFindActive) {
        const inp = document.getElementById('tb-find-input');
        if (inp) { inp.focus(); inp.select(); }
        return;
    }
    window.__tbFindActive = true;

    // 1. Create Shadow DOM Host (Protects UI from website CSS)
    const host = document.createElement('div');
    host.id = 'tb-find-host';
    host.style.cssText = 'position:fixed;top:0;right:0;z-index:2147483647;pointer-events:none;';
    document.documentElement.appendChild(host);
    const shadow = host.attachShadow({ mode: 'open' });

    // 2. Encapsulated Styles
    const style = document.createElement('style');
    style.textContent = `
        :host { all: initial; font-family: system-ui, -apple-system, sans-serif; }
        .bar { pointer-events: auto; position: fixed; top: 12px; right: 12px; width: 340px; height: 48px; display: flex; align-items: center; padding: 0 12px; background: var(--bg-tab-hover, #1a1b24); border: 1px solid var(--border-crisp, #252636); border-radius: 8px; box-shadow: 0 8px 24px rgba(0,0,0,0.5); box-sizing: border-box; gap: 8px; }
        input { flex: 1; height: 32px; background: transparent; border: none; outline: none; color: var(--text-main, #c8cdd8); font-size: 14px; }
        .counter { font-size: 12px; color: var(--text-muted, #6b7280); min-width: 50px; text-align: center; }
        button { width: 28px; height: 28px; border: none; border-radius: 4px; background: transparent; color: var(--text-muted, #6b7280); cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 12px; }
        button:hover { background: rgba(255,255,255,0.1); color: var(--text-main, #c8cdd8); }
        .close-btn:hover { color: #ff5f56; }
    `;
    shadow.appendChild(style);

    // 3. Build UI
    const bar = document.createElement('div');
    bar.className = 'bar';
    bar.innerHTML = `
        <input id="tb-find-input" type="text" placeholder="Find in page..." />
        <span class="counter" id="tb-find-counter">0 of 0</span>
        <button id="tb-find-prev" title="Previous (Shift+Enter)">&#9650;</button>
        <button id="tb-find-next" title="Next (Enter)">&#9660;</button>
        <button class="close-btn" id="tb-find-close" title="Close (Esc)">&#10005;</button>
    `;
    shadow.appendChild(bar);

    const inp = shadow.getElementById('tb-find-input');
    const cnt = shadow.getElementById('tb-find-counter');

    // 4. Event Listeners & Search Logic
    inp.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') { e.preventDefault(); e.shiftKey ? findPrev() : findNext(); }
        if (e.key === 'Escape') { e.preventDefault(); closeBar(); }
    });
    inp.addEventListener('input', () => search(inp.value));
    shadow.getElementById('tb-find-prev').addEventListener('click', findPrev);
    shadow.getElementById('tb-find-next').addEventListener('click', findNext);
    shadow.getElementById('tb-find-close').addEventListener('click', closeBar);

    let marks = [], cur = -1;

    function search(t) {
        document.querySelectorAll('.tb-find-wrap').forEach(w => {
            w.parentNode.replaceChild(document.createTextNode(w.textContent), w);
        });
        marks = []; cur = -1;
        if (!t) { cnt.textContent = '0 of 0'; return; }

        const rx = new RegExp(t.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), 'gi');
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, null);
        let node;
        while (node = walker.nextNode()) {
            if (node.parentElement.closest('#tb-find-host') || node.parentElement.tagName === 'SCRIPT' || node.parentElement.tagName === 'STYLE') continue;
            rx.lastIndex = 0;
            if (rx.test(node.textContent)) {
                const frag = document.createDocumentFragment();
                let last = 0, m;
                rx.lastIndex = 0;
                while (m = rx.exec(node.textContent)) {
                    if (m.index > last) frag.appendChild(document.createTextNode(node.textContent.substring(last, m.index)));
                    const mk = document.createElement('mark');
                    mk.className = 'tb-find-wrap';
                    mk.style.cssText = 'background: var(--accent, #5b9cf6); opacity: 0.3; color: inherit; border-radius: 2px;';
                    mk.textContent = m[0];
                    marks.push(mk);
                    frag.appendChild(mk);
                    last = rx.lastIndex;
                }
                if (last < node.textContent.length) frag.appendChild(document.createTextNode(node.textContent.substring(last)));
                const wrap = document.createElement('span');
                wrap.className = 'tb-find-wrap';
                wrap.style.display = 'inline';
                wrap.appendChild(frag);
                node.parentNode.replaceChild(wrap, node);
            }
        }
        if (marks.length > 0) { cur = 0; marks[0].style.opacity = '0.9'; }
        cnt.textContent = marks.length > 0 ? `${cur + 1} of ${marks.length}` : '0 of 0';
    }

    function findNext() {
        if (!marks.length) return;
        marks[cur].style.opacity = '0.3';
        cur = (cur + 1) % marks.length;
        marks[cur].style.opacity = '0.9';
        marks[cur].scrollIntoView({ behavior: 'smooth', block: 'center' });
        cnt.textContent = `${cur + 1} of ${marks.length}`;
    }

    function findPrev() {
        if (!marks.length) return;
        marks[cur].style.opacity = '0.3';
        cur = (cur - 1 + marks.length) % marks.length;
        marks[cur].style.opacity = '0.9';
        marks[cur].scrollIntoView({ behavior: 'smooth', block: 'center' });
        cnt.textContent = `${cur + 1} of ${marks.length}`;
    }

    function closeBar() {
        document.querySelectorAll('.tb-find-wrap').forEach(w => {
            if (w.tagName === 'MARK' || w.tagName === 'SPAN') {
                w.parentNode.replaceChild(document.createTextNode(w.textContent), w);
            }
        });
        window.__tbFindActive = false;
        host.remove();
    }

    window.__findNext = findNext;
    window.__findPrev = findPrev;
    window.__closeFindBar = closeBar;

    inp.focus();
})();

