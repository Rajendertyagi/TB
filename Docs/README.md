# RTBrowser

RTBrowser is a compact native Windows trading-focused workstation browser optimized for:

- responsiveness
- compactness
- multi-tab workflows
- TradingView-heavy usage
- lightweight runtime behavior

RTBrowser uses:

- WPF
- WebView2
- native rendering
- portable-first architecture
- runtime-managed WebViews
- aggressive memory lifecycle management

---

# Product Philosophy

RTBrowser is designed to feel like:

> A native trading workstation that happens to browse the web.

RTBrowser prioritizes:

- instant-feeling UI
- compact layouts
- minimal chrome
- stable runtime behavior
- efficient resource usage

RTBrowser avoids:

- Electron-style rendering
- bloated UI systems
- unnecessary animations
- extension-heavy architecture
- cloud dependency

---

# Core Features

## UI

- Helium-inspired compact UI
- Vivaldi-style efficiency
- integrated titlebar tabs
- compact omnibox
- minimal status bar
- dark graphite theme

---

## Runtime

- runtime-managed WebViews
- sleeping tabs
- lazy restoration
- WebView pooling
- memory pressure cleanup
- async runtime scheduling

---

## Architecture

- modular project structure
- portable-first deployment
- local-only telemetry
- lightweight diagnostics
- GitHub-first workflow

---

# Project Structure

```text
RTBrowser/
│
├── Docs/
├── Assets/
├── Scripts/
├── Tests/
│
└── src/
    ├── RTBrowser.App/
    ├── RTBrowser.UI/
    ├── RTBrowser.Runtime/
    ├── RTBrowser.Services/
    ├── RTBrowser.Core/
    ├── RTBrowser.WebView/
    └── RTBrowser.Telemetry/
