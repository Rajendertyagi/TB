(function () {
    'use strict';

    if (!document.documentElement) return;
    if (window.__themeSyncLoaded) return;
    window.__themeSyncLoaded = true;

    function applyVars(vars) {
        var root = document.documentElement;
        for (var key in vars) {
            if (vars.hasOwnProperty(key) && vars[key]) {
                root.style.setProperty(key, vars[key]);
            }
        }
    }

    // 1. Apply C# injected variables on initial load (Host Mode)
    if (window.__themeVariables) {
        applyVars(window.__themeVariables);
    }

    // 2. Listen for live THEME_UPDATE from C# via WebView2 IPC
    function setupListener() {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.addEventListener('message', function (event) {
                var msg = event.data;
                if (msg && msg.action === 'THEME_UPDATE' && msg.variables) {
                    applyVars(msg.variables);
                }
            });
        } else {
            setTimeout(setupListener, 50);
        }
    }
    setupListener();

    // 3. Inject native search text styling targeting theme variables
    var style = document.getElementById('tb-search-styles');
    if (!style) {
        style = document.createElement('style');
        style.id = 'tb-search-styles';
        style.textContent = '::search-text { background-color: color-mix(in srgb, var(--accent) 35%, transparent) !important; color: var(--text-main) !important; } ::search-text:current { background-color: var(--accent) !important; color: var(--bg-app) !important; }';
        document.documentElement.appendChild(style);
    }
})();
