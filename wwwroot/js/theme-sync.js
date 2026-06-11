(function () {
  function applyTheme(vars) {
    var root = document.documentElement;
    for (var key in vars) {
      if (vars.hasOwnProperty(key)) {
        root.style.setProperty(key, vars[key]);
      }
    }
  }

  if (window.__themeVariables) {
    applyTheme(window.__themeVariables);
  }

  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', function (event) {
      var msg = event.data;
      if (msg && msg.action === 'THEME_UPDATE' && msg.variables) {
        applyTheme(msg.variables);
      }
    });
  }
})();
