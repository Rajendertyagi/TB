console.log("JS: Script loaded");

const urlInput = document.getElementById('url-input');
const ctxMenu = document.getElementById('context-menu');
let originalUrl = "";

function resolveOmnibar(input) {
    const val = input.trim();
    if (!val) return null;
    if (/^[a-zA-Z]+:\/\//.test(val)) return val;
    if (/^[^\s]+\.[^\s]+$/.test(val) || val === 'localhost') return 'https://' + val;
    return 'https://www.google.com/search?q=' + encodeURIComponent(val);
}

if (urlInput) {
    const clearSelection = () => {
        const len = urlInput.value.length;
        urlInput.setSelectionRange(len, len);
    };

    urlInput.addEventListener('blur', clearSelection);
    urlInput.addEventListener('mouseup', clearSelection);

    urlInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            let val = urlInput.value.trim();
            if (!val) return;

            if (e.ctrlKey) {
                if (!val.startsWith('http://') && !val.startsWith('https://')) {
                    if (!val.includes('.')) val = val + '.com';
                    if (!val.startsWith('www.')) val = 'www.' + val;
                }
            }

            const url = resolveOmnibar(val);
            if (url) {
                clearSelection();
                urlInput.blur();
                window.chrome.webview.postMessage({ action: 'NAVIGATE', url: url });
            }
        }
    });
}

document.addEventListener('keydown', (e) => {
    const ctrl = e.ctrlKey || e.metaKey, shift = e.shiftKey, alt = e.altKey;
    const send = (action, data = {}) => window.chrome.webview.postMessage({ action, ...data });

    if (ctrl && /^[1-9]$/.test(e.key)) { e.preventDefault(); send('SWITCH_TAB_INDEX', { index: parseInt(e.key) }); }
    else if (ctrl && (e.key === '+' || e.key === '=')) { e.preventDefault(); send('ZOOM_IN'); }
    else if (ctrl && e.key === '-') { e.preventDefault(); send('ZOOM_OUT'); }
    else if (ctrl && e.key === '0') { e.preventDefault(); send('ZOOM_RESET'); }
    else if ((shift && e.key === 'F5') || (ctrl && shift && e.key.toLowerCase() === 'r')) { e.preventDefault(); send('HARD_RELOAD'); }
    else if (ctrl && e.key.toLowerCase() === 'p') { e.preventDefault(); send('PRINT'); }
    else if (ctrl && e.key.toLowerCase() === 'u') { e.preventDefault(); send('VIEW_SOURCE'); }
    else if (ctrl && e.key.toLowerCase() === 'n') { e.preventDefault(); send('NEW_WINDOW'); }
    else if (ctrl && e.key.toLowerCase() === 't') { e.preventDefault(); send('NEW_TAB'); }
    else if (ctrl && e.key.toLowerCase() === 'w') { e.preventDefault(); send('CLOSE_ACTIVE_TAB'); }
    else if (ctrl && e.key === 'Tab' && !shift) { e.preventDefault(); send('NEXT_TAB'); }
    else if (ctrl && shift && e.key === 'Tab') { e.preventDefault(); send('PREV_TAB'); }
    else if (e.key === 'F5' || (ctrl && e.key.toLowerCase() === 'r')) { e.preventDefault(); send('RELOAD'); }
    else if (alt && e.key === 'ArrowLeft') { e.preventDefault(); send('BACK'); }
    else if (alt && e.key === 'ArrowRight') { e.preventDefault(); send('FORWARD'); }
});

if (ctxMenu) {
    let ctxTargetId = null;
    document.addEventListener('click', () => ctxMenu.style.display = 'none');
    document.addEventListener('contextmenu', (e) => {
        const tab = e.target.closest('.tab');
        if (tab) {
            e.preventDefault();
            ctxTargetId = parseInt(tab.dataset.id);
            ctxMenu.style.display = 'block';
            ctxMenu.style.left = `${e.clientX}px`;
            ctxMenu.style.top = `${e.clientY}px`;
        }
    });
    ctxMenu.addEventListener('click', (e) => {
        const action = e.target.dataset.action;
        if (action && ctxTargetId !== null) {
            window.chrome.webview.postMessage({ action: 'CONTEXT_ACTION', type: action, id: ctxTargetId });
            ctxMenu.style.display = 'none';
        }
    });
}

window.chrome.webview.addEventListener('message', event => {
    const msg = event.data;
    if (msg.action === 'APPLY_THEME' && msg.theme?.css) {
        for (const [k, v] of Object.entries(msg.theme.css)) document.documentElement.style.setProperty(k, v);
    }
    else if (msg.action === 'TAB_CREATED') renderTab(msg.id, msg.title, true);
    else if (msg.action === 'TAB_SWITCHED') highlightTab(msg.id);
    else if (msg.action === 'TAB_CLOSED') removeTab(msg.id);
    else if (msg.action === 'UPDATE_URL' && urlInput) {
        urlInput.value = msg.url;
        originalUrl = msg.url;
        urlInput.setSelectionRange(0, 0);
    }
    // FIX 1: Bulletproof Focus with a tiny delay to let the OS catch up
    else if (msg.action === 'FOCUS_URL' && urlInput) {
        setTimeout(() => {
            urlInput.focus();
            urlInput.select();
        }, 10);
    }
    // FIX 2: Bulletproof 2-Step Escape Logic
    else if (msg.action === 'ESCAPE_PRESSED') {
        if (document.activeElement === urlInput) {
            if (urlInput.value !== originalUrl) {
                // 1st Esc: Revert text
                urlInput.value = originalUrl;
                urlInput.select();
            } else {
                // 2nd Esc: Blur and return focus to web page
                urlInput.blur();
                window.chrome.webview.postMessage({ action: 'FOCUS_TAB' });
            }
        } else {
            // Address bar not focused: Stop page load
            window.chrome.webview.postMessage({ action: 'STOP' });
        }
    }
});

document.querySelector('.new-tab-btn')?.addEventListener('click', () => window.chrome.webview.postMessage({ action: 'NEW_TAB' }));

function renderTab(id, title, isActive) {
    const container = document.querySelector('.tabs-container');
    if (!container || document.querySelector(`.tab[data-id="${id}"]`)) return;
    const tab = document.createElement('div');
    tab.className = `tab ${isActive ? 'active' : ''}`;
    tab.dataset.id = id;
    tab.innerHTML = `<span class="tab-title">${title}</span><button class="tab-close">${getIcon('tab-close')}</button>`;
    tab.onclick = (e) => { if (!e.target.closest('.tab-close')) window.chrome.webview.postMessage({ action: 'SWITCH_TAB', id: parseInt(id) }); };
    tab.querySelector('.tab-close').onclick = (e) => { e.stopPropagation(); window.chrome.webview.postMessage({ action: 'CLOSE_TAB', id: parseInt(id) }); };
    container.appendChild(tab);
}

function highlightTab(id) {
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    document.querySelector(`.tab[data-id="${id}"]`)?.classList.add('active');
}

function removeTab(id) {
    document.querySelector(`.tab[data-id="${id}"]`)?.remove();
}

window.chrome.webview.postMessage({ action: 'SHELL_READY' });