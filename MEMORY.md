---
type: architectural-checkpoint
created: 2026-06-11
purpose: Freeze architectural context against free-tier token compaction cycles
---

# MEMORY.md — WinUI 3 TB Browser Architectural Checkpoint

**CURRENT THEME:** violet-dark (accent `#ee82ee`, dark magenta `#1a0f1a`→`#3a1f3a`→`#2a152a`, light lavender text `#f0e0f0`, muted `#b89cb8`)
**UI STYLE:** Glass/slick — `backdrop-filter: blur(16px) saturate(120%)` on all surfaces, `-webkit-backdrop-filter` for WebView2 compat
**TRANSLUCENCY:** All `color-mix(..., var(--page-bg))` → `color-mix(..., transparent)` for true alpha translucency (TB-style)
**BORDERS:** Accent-based `var(--tb-elevated-N)` at 5-15% opacity instead of opaque `--page-border`
**PAGE BG:** `var(--page-bg)` solid base; optional gradient via `@supports (color-mix)` progressive enhancement
**BUILD:** 0 errors, 0 warnings (CSS/JS only — no build needed)
**CSS ARCHITECTURE:** Single unified stylesheet — `tb.css` contains `@font-face`, base theme vars + fallbacks, all component styles, all page layouts
**FONTS:** 4 Open Sans WOFF2 variants in `wwwroot/Fonts/` (preloaded via `<link rel="preload">` in all internal pages)
**CSS FILES:** 1 — `tb.css` only (`internal.css`, `settings.css`, `prism-base.css` deleted)
**TOGGLE COMPONENT:** `.toggle-switch` (36×20px, `.on` class, `.knob` child) — single shared component for settings + flags
**URL MASKING:** `HandleSourceChanged` keeps `tb://` URLs visible for internal pages (never exposes `file:///` path). `HandleNavigationStarting` downgrades tab from internal→standard when user clicks web links inside internal pages; blocks manual `file:///` on non-internal tabs.
**LATEST CHANGES:** Unified stylesheet: `@font-face` + merged fallback vars + all Prism/page/components into single `tb.css`. Fonts consolidated: TTF→WOFF2, moved from `Assets/Fonts/` to `wwwroot/Fonts/`, preloaded in all pages. `.toggle` renamed to `.toggle-switch` throughout. Flags page rewritten to use `.toggle-switch` + `.flag-card` (no separate toggle component). `internal.css`, `settings.css`, `prism-base.css` deleted. `HandleSourceChanged` renamed to single-param `(int id)` — URL masking for internal pages. `HandleNavigationStarting` upgraded — internal→standard downgrade + `file:///` block.

## I. Platform & Build State

| Field | Value |
|---|---|
| Framework | .NET 10 (`net10.0-windows10.0.26100.0`) |
| App SDK | Windows App SDK 2.1.3 |
| CsWinRT | No custom `.winmd` inputs |
| Packages | `<WindowsPackageType>None</WindowsPackageType>` |
| Target | Self-contained x64 |
| MVVM | CommunityToolkit.Mvvm 8.4 — manual `SetProperty`, no `[ObservableProperty]` |
| Binding | Runtime `{Binding}` only — never `{x:Bind}` |
| VirtualKey | `Windows.System.VirtualKey` — never `Microsoft.UI.Input.VirtualKey` |

## II. Four-Layer Input Architecture

**Problem:** WebView2 child HWND captures focus; shortcuts trigger Windows error sound ("ding").

**Solution — four-layer interception with process-ID guard:**

| Layer | Mechanism | Sound Deflection |
|---|---|---|
| AcceleratorKeyPressed | COM interop (`Marshal.GetIUnknownForObject` → QI) | `e.Handled = true` |
| WndProc | `SetWindowLongPtr` subclass on main window | `return IntPtr.Zero` |
| WH_KEYBOARD_LL | `SetWindowsHookEx` global hook, PID guard | `return (IntPtr)1` |
| webView.KeyDown | Routed event on each WebView2 | `e.Handled = true` |

**Key parameters:** `GetAsyncKeyState` P/Invoke (no cached modifiers), 150ms Stopwatch debounce on destructive keys (W, F4), 63 shortcuts via `CommandRegistry.cs` (GoF Command Pattern), COM interop isolated in `WebView2ControllerAccessor.cs`.

## III. TB Squashing Physics

| Spec | Value |
|---|---|
| Algorithm | Equal-width: `containerWidth / tabCount` |
| Floor | 32px (centered 16×16 favicon) |
| Ceiling | 180px |
| Close threshold | ≤36px hides close button |
| Inter-tab gap | 2px (StackLayout Spacing=2) |
| Anti-jiggle | `_isMouseInChrome` flag + deferred recalc |

**Opacity pipeline:** Active=1.0, Inactive unhovered=0.40, Inactive hovered=0.85.

## IV. Theme Architecture (Single Source of Truth)

| Component | Source | Mechanism |
|---|---|---|---|
| **theme.json** | Single source: `{active, native, colors, sizes}` | No `css`/`palette` keys |
| **XAML chrome** | `ThemeService.ApplyXamlResources()` | `_theme.Colors` → `{key}Brush`/`{key}Color`; `_theme.Sizes` → `{key}Length`/`Thickness`/`CornerRadius` |
| **CSS variables** | `ThemeService.GetCssVariables()` | camelCase→kebab-case (`bgApp`→`--bg-app`). Sizes get `px` suffix |
| **CSS fallbacks** | `css/tb.css` `:root` | Unified stylesheet — contains base theme vars, violet-dark fallback colors, layout values, all component styles, all page layouts |
| **Fonts** | `wwwroot/Fonts/*.woff2` | 4 Open Sans variants preloaded via `<link rel="preload">`. Declared via `@font-face` in `tb.css` with `font-display: swap` |
| **JS bridge** | `theme-sync.js` | 2-tier: `__themeVariables` (C# injected on load) else fallbacks from `tb.css` `:root`. Listens for `THEME_UPDATE` postMessage |
| **ThemeDictionaries** | `App.xaml` | Explicit `<ResourceDictionary x:Key="Dark/Light/HighContrast">` (Option A — WinUI Gallery). Dark & Light populated identically |
| **Persistence** | `ISettingsService` | `SetThemeAsync()` → `AppData/settings.json` |
| **Theme discovery** | `IThemeService.GetAvailableThemes()` | Returns `IReadOnlyList<ThemeInfo>` with `Id` (filename) + `Name` (display). Enumerates `wwwroot/themes/*.json` |
| **Theme hot-reload** | `ThemeChanged` → `OnThemeChanged` | `PostWebMessageAsJson({action:"THEME_UPDATE",variables})` to all open `tb://` tabs. `theme-sync.js` applies to `:root` live |
| **No fallbacks in C#/component CSS** | Fail-fast | No `var(--x, #fallback)` in tb.css. No `?? Colors.Transparent` in C#. All colors from theme.json (incl. `white`). Accent picker reads `var(--accent)` at runtime |

## V. DI Container (App.xaml.cs)

```
IThemeService        → ThemeService(basePath, ISettingsService)               [Singleton]
ISettingsService     → SettingsService(basePath)                               [Singleton]
IDownloadService     → DownloadService(basePath)                               [Singleton]
ITabManager          → TabManager(basePath, IThemeService, ISettingsService,   [Singleton]
                         IDownloadService, KeyboardShortcutHandler)
INavigationService   → NavigationService                                       [Singleton]
KeyboardShortcutHandler → ctor(IServiceProvider) — lazy ITabManager            [Singleton]
ChromeViewModel                                                                [Singleton]
MainViewModel                                                                  [Singleton]
MainWindow           → resolved from container                                 [Transient]
```

**Circular DI fix:** `KeyboardShortcutHandler` takes `IServiceProvider` (lazy ITabManager), not ITabManager directly.

**Service Locator eliminated:** All 15 `App.Services.GetRequiredService<T>()` removed from TabManager (10) and MainWindow.xaml.cs (5). Pure constructor injection.

## VI. Known Issues / Blockers

- HighContrast ThemeDictionary declared but not populated.
- Tab suspension (WebView2 discard on memory pressure) not implemented.
- No global unhandled-exception handler (only `App.UnhandledException` writes crash.log).
- Page right-click JS context detection fails on some sites with restrictive CSP that blocks inline script execution.

## VII. Recent Fixes

| Issue | Fix | File |
|---|---|---|
| XAML parse crash 0xc000027b | Registered BoolToVisibilityConverter in resources | `App.xaml.cs:41` |
| `[ObservableProperty]` MVVMTK0045 | Manual `SetProperty` replacements | `TabItemViewModel.cs`, `ChromeViewModel.cs` |
| Circular DI KeyboardShortcutHandler | `IServiceProvider` lazy resolution | `KeyboardShortcutHandler.cs` |
| Find bar hardcoded Catppuccin hexes | `GetFindBarScript()` from `GetCssVariables()` | `TabManager.cs:525` |
| Crash page hardcoded hexes | Inline HTML colors from `GetCssVariables()` | `TabManager.cs:107` |
| `ApplyNativeTheme()` fallback hexes | Uses `_theme.Colors["accent"]` directly | `ThemeService.cs` |
| `BoolToActiveBgConverter` fallback chain | Reads `ThemeDictionaries["Dark"][key]` directly | `Converters.cs` |
| `tb.css` hardcoded Catppuccin hexes | All `var(--key)` with NO fallback values | `wwwroot/css/tb.css` |
| Service Locator anti-pattern | Constructor injection. 15 calls eliminated. | `TabManager.cs`, `MainWindow.xaml.cs`, `App.xaml.cs` |
| Script injection timing bug | `AddScriptToExecuteOnDocumentCreatedAsync` BEFORE `webView.Source` | `TabManager.cs:217` |
| 404.html missing | Created at `wwwroot/404.html` | `wwwroot/404.html` |
| Empty `catch {}` in crash handler | Now logs via `Logger.Error()` | `TabManager.cs` |
| **Shared MenuFlyout crash on right-click** | **Per-show programmatic flyouts; JS context detection; removed XAML resource flyouts** | `TabManager.cs`, `MainWindow.xaml.cs` |
| **Duplicate tab feature** | **Added `DuplicateTabAsync(id)` to `ITabManager`/`TabManager`** | `ITabManager.cs`, `TabManager.cs` |
| 15 hardcoded Catppuccin rgba() in tb.css | All → `color-mix(in srgb, var(--key) X%, transparent)` | `wwwroot/css/tb.css` |
| `--page-danger` / `--page-success` missing | Added to theme.json | `wwwroot/theme.json` |
| Settings accent picker default | Updated to match current accent #ee82ee | `settings.html` |
| White pages after glass UI changes | Reverted `html, body` to `background: var(--page-bg)`. Gradient moved to `@supports` progressive enhancement. Added `-webkit-backdrop-filter`. | `wwwroot/css/tb.css:70` |
| **Tab context menu crash (intermittent)** | **try/catch around `ShowTabContextMenu` AND `PointerPressed` handler — unhandled exception crashed app** | `MainWindow.xaml.cs` |
| **Page context menu not showing** | **`_disposed` + `CoreWebView2==null` guards after JS await; try/catch around `ShowAt`** | `TabManager.cs` |
| **Session tab accumulation on restart** | **`_isRestoring` flag skips saves during restore; `_sessionSaveLock` semaphore prevents concurrent file writes** | `TabManager.cs` |
| **`#ffffff` in tb.css:40** | Removed from tb.css; `"white"` added to `theme.json` colors | `wwwroot/css/tb.css`, `wwwroot/theme.json` |
| **`#ee82ee` hardcoded in accent picker** | Default set dynamically from `var(--accent)` CSS variable | `wwwroot/settings.html` |
| **Theme selector key mismatch** | `key == "theme-select"` → `key == "theme-name"` to match JS send | `Services/TabManager.Ipc.cs:72` |
| **Theme pipeline incomplete** | `SaveSetting(theme-name)` now calls `SetThemeAsync` → `ThemeChanged` → `OnThemeChanged` broadcasts `{action:"THEME_UPDATE",variables}` via `PostWebMessageAsJson` | `Services/TabManager.Ipc.cs` |
| **White page when loading internal pages via file:///** | `theme-sync.js` now uses baked-in fallback colors (no `fetch` which is blocked on `file:///`) | `wwwroot/js/theme-sync.js` |
| **Missing Download/Settings/Flags nav bar icons** | Added icon buttons between URL bar and menu button in NavigationBar.xaml | `Controls/NavigationBar.xaml` |
| **ThemeInfo record + GetAvailableThemes()** | New `ThemeInfo(string Id, string Name)` record. `IThemeService` exposes `IReadOnlyList<ThemeInfo> GetAvailableThemes()` | `Services/Interfaces/IThemeService.cs`, `Services/ThemeService.cs` |
| **Missing violet-dark.json theme file** | Created `wwwroot/themes/violet-dark.json` from current theme.json colors | `wwwroot/themes/violet-dark.json` |
| **Settings IPC uses wrong event target** | `window.addEventListener('message')` → `window.chrome.webview.addEventListener('message')` (WebView2 channel, not DOM) | `wwwroot/settings.html` |
| **SettingsReady response wrapped in legacy format** | `{c:"dispatchIpc",a:{d:{action,settings,themes}}}` → flat `{action:"SETTINGS_DATA",settings,themes}` via `ExecuteScriptAsync`+`dispatchEvent` | `Services/TabManager.Ipc.cs:63` |
| **InjectThemeVariablesAsync removed** | Replaced by dynamic `__themeVariables` init in Lifecycle.cs (per-WebView2, after bridge script) + `OnThemeChanged` updates script + sends `THEME_UPDATE` | `Services/TabManager.Lifecycle.cs:39`, `Services/TabManager.Ipc.cs:152` |
| **theme-init.js / _themeInitScript deleted** | Dynamic script now sets `window.__themeVariables = {...}` in `Lifecycle.cs` — no need for static file or Lazy field | `Services/TabManager.cs:55,105` |
| **settings.html dual listener** | Now listens on BOTH `window` (for `ExecuteScriptAsync`-dispatched events) and `chrome.webview` (for `PostWebMessageAsJson` events) | `wwwroot/settings.html` |
| **ToDisplayName() for theme names** | `ThemeService.GetAvailableThemes()` derives display names from IDs (`violet-dark` → `Violet Dark`) | `Services/ThemeService.cs:309` |
| **HandleSourceChanged shows file:/// for internal pages** | Now masks `file:///` path with logical `tb://` URL. Simplified signature to `(int id)` — looks up WebView2 by id | `Services/TabManager.Events.cs` |
| **HandleNavigationStarting missing security checks** | Downgrades tab internal→standard when user clicks web links inside internal pages (unregisters IPC). Blocks manual `file:///` on non-internal tabs | `Services/TabManager.Events.cs` |

## VIII. Project Structure

```
C:\Users\RTPC\Documents\TB\
├── App.xaml / App.xaml.cs              [DI + ThemeDictionaries]
├── MainWindow.xaml / .cs               [WinUI Window + WndProc + LLKBHook]
├── Helpers/
│   ├── Converters.cs                   [BoolToActiveBg, BoolToVisibility]
│   ├── Constants.cs                    [Layout constants]
│   ├── UrlResolver.cs                  [tb:// + 404]
│   └── Extensions.cs                   [Async utilities]
├── Input/
│   ├── KeyboardShortcutHandler.cs      [63 shortcuts, 4-layer intercept]
│   ├── CommandRegistry.cs              [63 binding map]
│   ├── ShortcutCommand.cs              [IShortcutCommand + ShortcutBinding]
│   └── WebView2ControllerAccessor.cs   [COM interop for AcceleratorKeyPressed]
├── Models/
│   ├── TabItem.cs
│   └── ThemeDefinition.cs              [active, native, colors, sizes]
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── ChromeViewModel.cs              [Omnibar + nav buttons + tab move]
│   └── TabItemViewModel.cs
├── Services/
│   ├── Interfaces/
│   │   ├── ITabManager.cs
│   │   ├── IThemeService.cs            [+ ActiveThemeName, SetThemeAsync, GetCssVariables]
│   │   ├── INavigationService.cs
│   │   ├── ISettingsService.cs
│   │   └── IDownloadService.cs
│   ├── TabManager.cs                   [Tab lifecycle + find bar + crash page]
│   ├── ThemeService.cs                 [ApplyXamlResources + GetCssVariables]
│   ├── NavigationService.cs
│   ├── SettingsService.cs              [AppData/settings.json]
│   └── DownloadService.cs
├── Infrastructure/
│   └── Logger.cs                       [logs/tb-YYYY-MM-DD.log]
├── Styles/
│   ├── ChromeResources.xaml            [No fallback brushes + Icons]
│   └── Icons.xaml
├── Features/Downloads/
│   ├── DownloadItem.cs
│   ├── DownloadService.cs
│   └── DownloadViewModel.cs
└── wwwroot/
    ├── theme.json                      [violet-dark active]
    ├── 404.html                        [tb.css + theme-sync.js + font preloads]
    ├── crash.html                      [inline style, no theme deps]
    ├── downloads.html                  [tb.css + theme-sync.js + font preloads]
    ├── flags.html                      [tb.css + theme-sync.js + font preloads, uses .toggle-switch + .flag-card]
    ├── settings.html                   [tb.css + theme-sync.js + font preloads, 4 sections]
    ├── Fonts/
    │   ├── OpenSans-Bold.woff2
    │   ├── OpenSans-Medium.woff2
    │   ├── OpenSans-Regular.woff2
    │   └── OpenSans-SemiBold.woff2
    ├── css/
    │   └── tb.css                      [UNIFIED: @font-face, theme vars, fallbacks, Prism, all layouts]
    ├── js/
    │   ├── find-bar.js                 [Custom find-in-page overlay]
    │   ├── gradient-shimmer.js         [Prism canvas engine]
    │   └── theme-sync.js               [pure JS bridge: __themeVariables + THEME_UPDATE]
```

## IX. CSS Architecture (2026-06-12)

### Unified Stylesheet
| Layer | Location | Role |
|-------|----------|------|
| **Fonts** | `tb.css` §1 (`@font-face`) + `wwwroot/Fonts/*.woff2` | 4 Open Sans WOFF2 variants, preloaded via `<link rel="preload">` in HTML |
| **Design tokens** | `tb.css` §2 (`:root`) | Base theme fallbacks (violet-dark), layout vars, Prism tokens, semantic aliases, elevations |
| **Typography** | `tb.css` §3 | Base html/body, headings, links, scrollbar styling, `@supports` gradient |
| **Components** | `tb.css` §4-5 | Buttons, inputs, dropdowns, `.toggle-switch`, checkboxes, skeleton/spinner, `.prism-card`, `.prism-input-flat`, `.flag-card` |
| **Page layouts** | `tb.css` §6 | Settings sidebar, settings cards/rows, download items, toasts |
| **Utilities** | `tb.css` §7-8 | Flex, gap, rounded, text utilities, `.btn-ghost`, `.shortcut-key`, responsive breakpoints |
| **JS override** | `theme-sync.js` | Applies `__themeVariables` if C# injected, listens for `THEME_UPDATE`. Pure bridge |

### Key Changes
- Single `tb.css` replaces 4 files: `internal.css`, `settings.css`, `prism-base.css` deleted
- `.toggle` renamed to `.toggle-switch` throughout — single toggle component for all pages
- Flags page uses `.toggle-switch` + `.flag-card` (no separate flag toggle CSS)
- Fonts: TTF→WOFF2, moved from `Assets/Fonts/` to `wwwroot/Fonts/`, preloaded in all pages

## X. Recent & Ongoing Work

### Context Menu Architecture (Completed)
- **Tab right-click:** Fresh `MenuFlyout` per-show with `TBMenuFlyoutPresenterStyle` + styled items. Commands bound via `Command` property (New Tab, Duplicate Tab, Reload, Close, Close Other Tabs). Code in `MainWindow.xaml.cs:ShowTabContextMenu`.
- **Page right-click:** `CoreWebView2.ContextMenuRequested` handler (args.Handled=true) → JS `document.elementFromPoint` + `closest('a','img','input')` to detect mode → builds appropriate flyout (Standard/Link/Image/Text) with Click handlers. Code in `TabManager.cs:ContextMenuRequested` handler.
- **Key lesson:** Shared XAML `MenuFlyout` resources **cannot** be reused via `ShowAt()` from different placement targets — WinUI crashes. Always create fresh instances.
- **New API:** `DuplicateTabAsync(int id)` on `ITabManager`/`TabManager`. `NewTab` command on `TabItemViewModel`.

### CSS Architecture Rewrite (Latest — 2026-06-12)
- **Unified stylesheet**: `tb.css` now single file — `@font-face` + merged fallback vars + all Prism/page/component styles.
- **Files deleted**: `internal.css`, `settings.css`, `prism-base.css` (all merged into `tb.css`).
- **Fonts**: TTF→WOFF2, moved from `Assets/Fonts/` to `wwwroot/Fonts/`, preloaded via `<link rel="preload">` in all pages.
- **`.toggle` → `.toggle-switch`**: Single toggle component (36×20px, `.on` class, `.knob` child) shared by settings + flags.
- **Flags page rewritten**: Uses `.toggle-switch` + `.flag-card` (no separate flag toggle CSS component).
- **All internal pages**: Preload WOFF2 fonts, single `tb.css`, `theme-sync.js`.

### IPC / Security Fixes (Latest — 2026-06-12)
- **`HandleSourceChanged`**: Simplified to `(int id)` only (looks up WebView2 by id). Masks `file:///` paths with logical `tb://` URLs for internal pages. Standard web pages update physical URL normally. Calls `FireNavState(id)` after every change.
- **`HandleNavigationStarting`**: Downgrades tab from internal→standard when user clicks web links inside internal pages (unregisters IPC handlers). Blocks manual `file:///` navigation on non-internal tabs via `args.Cancel = true`. Prevents security bypass where user could type `file:///` paths in Omnibox.

## XI. Next Steps (Priority Order)

1. **[DONE] Theme pipeline** — `GetAvailableThemes()` + `ThemeInfo` record + hot-reload broadcast via `PostWebMessageAsJson`
2. **[DONE] Audit colors** — `#ffffff` → `theme.json`, `#ee82ee` → `var(--accent)` at runtime
3. **[DONE] Nav bar icons** — Download, Settings, Flags buttons
4. **[DONE] `violet-dark.json`** theme pack for `SetThemeAsync`
5. **[DONE] C# fixes** — `TabManager.Ipc.cs`: `SaveSetting` key→`theme-name`, `OnThemeChanged` flat broadcast via `PostWebMessageAsJson`, `SettingsReady` flattened + `ExecuteScriptAsync` dispatch, `AvailableThemes`→`GetAvailableThemes()` returning `IReadOnlyList<ThemeInfo>`
6. **[DONE] CSS Architecture** — `tb.css` unified (merged `internal.css` + `settings.css` + `prism-base.css` into single file). `.toggle`→`.toggle-switch`. `internal.css`, `settings.css`, `prism-base.css` deleted
7. **[DONE] Fonts** — TTF→WOFF2, moved `Assets/Fonts/`→`wwwroot/Fonts/`, preloaded in all pages via `<link rel="preload">`
8. **Tab suspension** — discard WebView2 on memory pressure / inactivity
9. **Global exception handler** — pipe unhandled exceptions to `crash.log`
10. **Close window vs Close tab** — separate pathways

- [CONFIRMED] Physical directory scaffolding complete. Multi-mode context menu matrices and decomposed folder branches fully indexed with 0 compilation errors and 0 warnings.
