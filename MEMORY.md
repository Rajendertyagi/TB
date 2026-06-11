---
type: architectural-checkpoint
created: 2026-06-11
purpose: Freeze architectural context against free-tier token compaction cycles
---

# MEMORY.md — WinUI 3 TB Browser Architectural Checkpoint

**CURRENT THEME:** violet-dark (accent `#ee82ee`, dark magenta `#1a0f1a`→`#3a1f3a`→`#2a152a`, light lavender text `#f0e0f0`, muted `#b89cb8`)
**UI STYLE:** Glass/slick — `backdrop-filter: blur(16px) saturate(120%)` on all surfaces, `-webkit-backdrop-filter` for WebView2 compat
**TRANSLUCENCY:** All `color-mix(..., var(--page-bg))` → `color-mix(..., transparent)` for true alpha translucency (Helium-style)
**BORDERS:** Accent-based `var(--helium-elevated-N)` at 5-15% opacity instead of opaque `--page-border`
**PAGE BG:** `var(--page-bg)` solid base; optional gradient via `@supports (color-mix)` progressive enhancement
**BUILD:** 0 errors, 0 warnings
**LATEST FIXES:** Tab crash guard (try/catch on PointerPressed). Session save race fix (`_isRestoring` flag + `_sessionSaveLock` semaphore). Page context menu disposed guards.

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

## III. Helium Squashing Physics

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
|---|---|---|
| **theme.json** | Single source: `{active, native, colors, sizes}` | No `css`/`palette` keys |
| **XAML chrome** | `ThemeService.ApplyXamlResources()` | `_theme.Colors` → `{key}Brush`/`{key}Color`; `_theme.Sizes` → `{key}Length`/`Thickness`/`CornerRadius` |
| **CSS variables** | `ThemeService.GetCssVariables()` | camelCase→kebab-case (`bgApp`→`--bg-app`). Sizes get `px` suffix |
| **Internal pages** | `theme-sync.js` | `window.__themeVariables` → `setProperty(key, value)`. Handles `THEME_UPDATE` postMessage |
| **ThemeDictionaries** | `App.xaml` | Explicit `<ResourceDictionary x:Key="Dark/Light/HighContrast">` (Option A — WinUI Gallery). Dark & Light populated identically |
| **Persistence** | `ISettingsService` | `SetThemeAsync()` → `AppData/settings.json` |
| **No fallbacks** | Fail-fast | No `var(--x, #fallback)` in tb.css. No `?? Colors.Transparent` in C#. No fallback hexes anywhere |

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
- `CycleTheme` doesn't load different `theme.json` — needs theme selector UI in settings.
- Tab suspension (WebView2 discard on memory pressure) not implemented.
- No global unhandled-exception handler (only `App.UnhandledException` writes crash.log).
- `Views/` and `Controls/` directories exist but are empty.
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
    ├── css/tb.css                      [:root uses var(--key), no fallback hexes]
    ├── js/theme-sync.js                [CSS variable injection via __themeVariables]
    ├── js/gradient-shimmer.js          [Prism canvas engine]
    ├── settings.html                   [All pages use glass-children + theme-sync.js]
    ├── downloads.html
    ├── flags.html
    └── 404.html
```

## IX. Recent & Ongoing Work

### Context Menu Architecture (Completed)
- **Tab right-click:** Fresh `MenuFlyout` per-show with `HeliumMenuFlyoutPresenterStyle` + styled items. Commands bound via `Command` property (New Tab, Duplicate Tab, Reload, Close, Close Other Tabs). Code in `MainWindow.xaml.cs:ShowTabContextMenu`.
- **Page right-click:** `CoreWebView2.ContextMenuRequested` handler (args.Handled=true) → JS `document.elementFromPoint` + `closest('a','img','input')` to detect mode → builds appropriate flyout (Standard/Link/Image/Text) with Click handlers. Code in `TabManager.cs:ContextMenuRequested` handler.
- **Key lesson:** Shared XAML `MenuFlyout` resources **cannot** be reused via `ShowAt()` from different placement targets — WinUI crashes. Always create fresh instances.
- **New API:** `DuplicateTabAsync(int id)` on `ITabManager`/`TabManager`. `NewTab` command on `TabItemViewModel`.

## X. Next Steps (Priority Order)

1. **Theme selector UI** — settings.html dropdown, calls `SetThemeAsync`
2. **Tab suspension** — discard WebView2 on memory pressure / inactivity
3. **Global exception handler** — pipe unhandled exceptions to `crash.log`
4. **Close window vs Close tab** — separate pathways
5. **Hot-reload durability** — `FileShare.Read` with retry
6. **Migrate** XAML templates from `MainWindow` to `Views/Controls/`
