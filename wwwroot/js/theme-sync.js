(function () {
    'use strict';

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
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener('message', function (event) {
            var msg = event.data;
            if (msg && msg.action === 'THEME_UPDATE' && msg.variables) {
                applyVars(msg.variables);
            }
        });
    }
})();
