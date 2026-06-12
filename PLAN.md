---
plan name: rt-winui-webview2
plan description: WinUI WebView2 TB application
plan status: active
---

## Idea
A comprehensive TB project implementing native Windows desktop application using WinUI 3 for modern UI/UX and WebView2 for web content integration, enabling cross-platform thermal/biological sensor data visualization and analysis.

## Implementation
- WinUI 3 + WebView2 integration ✅
- MVVM + DI with CommunityToolkit.Mvvm + MS.Ext.DependencyInjection ✅
- Keyboard shortcuts (4-layer intercept) ✅
- TB squashed tab layout ✅
- Theme system (theme.json single source of truth) ✅
- Internal pages (settings, downloads, flags) ✅
- GoF Command Pattern for shortcuts ✅
- File-based logging ✅
- Download tracking with routing ✅

## Status Summary
**PROGRESS: 100% COMPLETE — violet-dark theme | glass/slick UI | TB-style translucency**

**CURRENT THEME: violet-dark** (accent #ee82ee, dark magenta #1a0f1a→#3a1f3a, light lavender #f0e0f0)
**GLASS FILTER:** blur(16px) saturate(120%) on all surfaces — sidebar, cards, items, toast, nav, buttons
**BORDER STYLE:** Subtle accent-based `var(--tb-elevated-N)` instead of opaque `--page-border`
**PAGE BACKGROUND:** `background: var(--page-bg)` (solid). Optional gradient via `@supports (color-mix)` — 175deg gradient with 3% accent tint for glass depth
**CSS ARCHITECTURE:** Unified stylesheet — single `tb.css` contains `@font-face`, base theme vars + fallbacks, all component styles, all page layouts, all Prism tokens
**FONTS:** 4 Open Sans WOFF2 variants in `wwwroot/Fonts/`, preloaded via `<link rel="preload">` in all HTML pages
**CSS FILES:** 1 — `tb.css` only

**CURRENT IMPLEMENTATION STATUS:**
- ✅ Core architecture complete (TB.csproj, all packages)
- ✅ WinUI 3 + WebView2 integration
- ✅ MVVM + DI (CommunityToolkit.Mvvm 8.4, MS.Ext.DI 9.0)
- ✅ Circular DI fix: KeyboardShortcutHandler lazy-resolves ITabManager via IServiceProvider
- ✅ **Single source of truth**: theme.json drives both XAML chrome and internal-page CSS
- ✅ **No hardcoded colors**: all themed values from theme.json only
- ✅ Chrome-parity keyboard shortcuts — 63 total, 4-layer intercept
- ✅ TB squashed tab layout (32px floor, equal-width allocation)
- ✅ Hover opacity pipeline (0.40/0.85/1.0) + anti-jiggle deferred recalc
- ✅ Close button trap at 36px
- ✅ Keyboard shortcut handling: live GetAsyncKeyState P/Invoke polling
- ✅ Win32 WndProc subclass for chrome focus interception
- ✅ WH_KEYBOARD_LL hook for WebView2 focus interception (process-ID guard)
- ✅ webView.KeyDown routed event (redundant guard)
- ✅ **CoreWebView2Controller.AcceleratorKeyPressed** via COM interop
- ✅ 150ms Stopwatch debounce for destructive hotkeys
- ✅ Process-ID guard in LLKBHook
- ✅ Favicon support + internal page IPC
- ✅ Production-ready patterns (nullable, async, IAsyncDisposable)
- ✅ Alt+D (focus URL bar + select all), Ctrl+Enter (wrap www.+.com)
- ✅ Custom find bar (340×48px, theme-colored via GetCssVariables())
- ✅ Internal pages: settings.html, downloads.html, flags.html, 404.html
- ✅ Runtime testing: tabs, navigation, zoom, downloads
- ✅ Minimum window (400×300) via WM_GETMINMAXINFO
- ✅ ThemeDictionaries declared in App.xaml (Option A — WinUI Gallery pattern)
- ✅ Theme persistence: ActiveThemeName + SetThemeAsync() via ISettingsService
- ✅ GetCssVariables(): camelCase→kebab-case CSS var injection
- ✅ wwwroot/js/theme-sync.js: reads __themeVariables, handles THEME_UPDATE messages
- ✅ wwwroot/js/gradient-shimmer.js: Prism canvas engine
- ✅ tb.css :root — all var(--key) with NO fallback values
- ✅ BoolToActiveBgConverter: reads ThemeDictionaries["Dark"] directly, no fallback chain
- ✅ ThemeService.ApplyNativeTheme(): uses _theme.Colors directly, no fallback hexes
- ✅ Crash page + find bar colors from GetCssVariables(), no hardcoded hexes
- ✅ All 4 intercept layers active: AcceleratorKeyPressed + WndProc + LLKBHook + KeyDown
- ✅ Bug fixes: _internalPageTabs.Add(id), NavigationStarted/Completed guards, _lastClosedUrls, CycleTheme
- ✅ DispatcherQueue.TryEnqueue in ChromeViewModel event handlers
- ✅ Find bar auto-focus fixed (inline focus in IIFE)
- ✅ Prism UI: pages.css with --tb-elevated color-mix, all design token classes
- ✅ GoF Command Pattern: ShortcutCommand.cs + CommandRegistry.cs (63 binding map)
- ✅ KeyboardShortcutHandler refactored to registry dispatch (removed 330 if-else lines)
- ✅ ChromeViewModel cleanup: named event methods, DispatcherQueue alias
- ✅ Logger: file-based logs/tb-YYYY-MM-DD.log, all Console.WriteLine removed
- ✅ Dead code deleted: Features/Tabs/, Features/Bookmarks/
- ✅ Theme pipeline: InjectThemeToWebView removed → theme-sync.js + postMessage
- ✅ Download routing: _downloadOwners + SendToDownloadOwner
- ✅ COM interop isolated: WebView2ControllerAccessor in Input/
- ✅ Converters.cs: BoolToActiveBgConverter simplified (no fallback chain), extra parens fixed
- ✅ wwwroot/themes/github-dark.css deleted (no more separate CSS theme files)
- ✅ Build: 0 errors, 0 warnings

**ACTUAL PROJECT STRUCTURE (C:\Users\RTPC\Documents\TB):**

```
C:\Users\RTPC\Documents\TB\
├── App.xaml                                    // DI + ThemeDictionaries[Dark/Light/HighContrast]
├── App.xaml.cs                                 // 3 global handlers, DI registration (8 singleton + 1 transient)
├── MainWindow.xaml                             // Window chrome XAML
├── MainWindow.xaml.cs                          // AppWindow, drag regions, keyboard hooks, InitializeAsync
├── AGENTS.md                                   // OpenCode automation rules
├── MEMORY.md                                   // Context checkpoint
├── PLAN.md                                     // Master plan + specs
├── RULES.md                                    // Coding standards
├── TB.csproj                                   // .NET 10, WinAppSDK 2.1.3, WebView2 1.0.3967.48
├── TB.slnx                                     // Solution file
│
├── Controls\                                   // [ACTIVE] Decomposed XAML user controls
│   ├── NavigationBar.xaml                      // URL bar + nav button layout
│   ├── NavigationBar.xaml.cs                   // URL focus, security icon, Ctrl+Enter, border theming
│   ├── TabStrip.xaml                           // ItemsRepeater tab list + new-tab button
│   ├── TabStrip.xaml.cs                        // Tab widths, hover, right-click context menu
│   └── TabStripLayout.cs                       // Custom NonVirtualizingLayout, width clamping, IsSquashed
│
├── Views\                                      // [EXISTING / ACTIVE] View pages and menu dictionaries
│   └── Menus\                                  // [EXISTING / ACTIVE] Catppuccin-themed context menu definitions
│
├── ViewModels\                                 // MVVM layer (CommunityToolkit.Mvvm 8.4 source generators)
│   ├── ChromeViewModel.cs                      // Tabs OC, UrlText/TabWidth, 8 RelayCommands
│   ├── TabItemViewModel.cs                     // Per-tab props, 6 RelayCommands + Switch
│   └── MainViewModel.cs                        // WindowTitle, NewWindow() process spawn
│
├── Services\                                   // Business logic — TabManager split across 6 partials
│   ├── TabManager.cs                           // PARTIAL 1/6: fields (10 dicts), events (11), ctor, Init
│   ├── TabManager.Commands.cs                  // PARTIAL 2/6: 40+ command methods
│   ├── TabManager.Events.cs                    // PARTIAL 3/6: WebView2EventHandlers, 8 events + handlers
│   ├── TabManager.Ipc.cs                       // PARTIAL 4/6: IpcActions, WebMessageReceived routing
│   ├── TabManager.Lifecycle.cs                 // PARTIAL 5/6: Create, Switch, Close, DetachAndCleanState
│   ├── TabManager.Session.cs                   // PARTIAL 6/6: Session save/load, DisposeAsync
│   ├── NavigationService.cs                    // Thin 5-method facade over ITabManager
│   ├── ThemeService.cs                         // theme.json loader, XAML dicts, native title bar, CSS vars
│   │
│   ├── Downloads\                              // Downloads feature module
│   │   ├── DownloadItem.cs                     // Id, Name, Url, Bytes, ProgressPercent, Status
│   │   ├── DownloadService.cs                  // List<DownloadItem> → downloads.json, 3 events
│   │   └── DownloadViewModel.cs                // JSON DTO for IPC, FromItem() factory
│   │
│   └── Interfaces\                             // DI contracts
│       ├── ITabManager.cs                      // 11 events, 4 props, 50+ methods
│       ├── IThemeService.cs                    // CurrentTheme, ApplyXamlResources, GetCssVariables, CycleTheme
│       ├── INavigationService.cs               // 5-method slice: Navigate, Back, Forward, Reload, Stop
│       ├── ISettingsService.cs                 // Get<T>/Set/GetAll/GetAllJson
│       └── IDownloadService.cs                 // Download CRUD + 3 events
│
├── Input\                                      // Keyboard interception + GoF Command Pattern
│   ├── CommandRegistry.cs                      // O(1) Dictionary of ~50 ShortcutBinding entries
│   ├── KeyboardShortcutHandler.cs              // 4-layer intercept, 150ms debounce
│   ├── ShortcutCommand.cs                      // IShortcutCommand + ShortcutBinding readonly struct
│   └── WebView2ControllerAccessor.cs           // COM interop → AcceleratorKeyPressed
│
├── Infrastructure\                             // Foundation services
│   ├── Logger.cs                               // Static file logger + Error(string, Exception) stack trace overload
│   └── SettingsService.cs                      // Thread-safe JSON key-value store
│
├── Models\                                     // Pure data objects
│   ├── TabItem.cs                              // Id, Url, Title, Zoom, CreatedAt, IsInternalPage
│   ├── ThemeDefinition.cs                      // Active, Native, Colors dict + Load() parser
│   ├── SessionState.cs                         // Tabs (List<TabEntry>), ActiveTabIndex
│   └── Events.cs                               // TabEventArgs, UrlEventArgs, FaviconEventArgs, NavStateEventArgs, TabMovedEventArgs
│
├── Helpers\                                    // Shared utilities
│   ├── Constants.cs                            // Routes, Actions, Defaults, Layout (px constants)
│   ├── Converters.cs                           // BoolToActiveBg, BoolToVisibility (with invert)
│   ├── Extensions.cs                           // ColorExtensions.ParseHex, TaskExtensions.FireAndForget
│   ├── Paths.cs                                // Static readonly paths (BaseDir, AppData, Cache, etc.)
│   └── UrlResolver.cs                          // IsInternalUrl, Resolve, ResolveOmnibar, GetTabTitle
│
├── Styles\                                     // XAML resource dictionaries
│   ├── ChromeResources.xaml                    // Fallback brushes, layout doubles, CornerRadii, Open Sans
│   └── Icons.xaml                              // Segoe Fluent Icon path geometries
│
├── Assets\Fonts\                               // App fonts
│   ├── OpenSans-Bold.ttf
│   ├── OpenSans-Medium.ttf
│   ├── OpenSans-Regular.ttf
│   └── OpenSans-SemiBold.ttf
│
    └── wwwroot\                                    // Internal page web assets
        ├── theme.json                              // Active: "violet-dark", colors dict
        ├── 404.html                                // Not-found page (tb.css + theme-sync.js + font preloads)
        ├── crash.html                              // Crash page (inline style, no theme deps)
        ├── downloads.html                          // Download manager page (tb.css + theme-sync.js + font preloads)
        ├── flags.html                              // Experimental flags page (tb.css + theme-sync.js + font preloads, .toggle-switch + .flag-card)
        ├── settings.html                           // Settings page (tb.css + theme-sync.js + font preloads, 4 sections)
        ├── Fonts\                                  // Web fonts (4 Open Sans WOFF2 variants)
        │   ├── OpenSans-Bold.woff2
        │   ├── OpenSans-Medium.woff2
        │   ├── OpenSans-Regular.woff2
        │   └── OpenSans-SemiBold.woff2
        ├── css\                                    // Style sheets (1 file)
        │   └── tb.css                              // UNIFIED: @font-face, theme vars, fallbacks, Prism, all layouts
        ├── js\                                     // JavaScript files
        │   ├── find-bar.js                         // Custom find-in-page overlay (340×48px)
        │   ├── gradient-shimmer.js                 // Prism canvas gradient animation
        │   └── theme-sync.js                       // Pure JS bridge: applies __themeVariables + THEME_UPDATE listener
        └── themes\                                 // Theme packs (switchable via ISettingsService)
        ├── catppuccin-mocha.json
        ├── dracula.json
        ├── github-dark.json
        ├── gruvbox-dark.json
        ├── nord.json
        └── tokyo-night.json
```

## Blocked Items
- [RESOLVED] Keyboard Shortcut Focus Bug: Chromium/WebView2 swallowing raw inputs and triggering native Windows OS error sound ("ding"). **Solution**: Four-layer interception — (1) CoreWebView2Controller.AcceleratorKeyPressed via COM interop (`Marshal.GetIUnknownForObject` + QI + `Marshal.GetObjectForIUnknown`), (2) Win32 WndProc subclass for chrome focus, (3) WH_KEYBOARD_LL low-level hook for WebView2 focus, (4) webView.KeyDown routed event — with process-ID guard to avoid blocking other apps.
- [RESOLVED] Inject `CoreWebView2Settings.AllowHostInputProcessing = true` — API unavailable in WebView2 1.0.3967.48, AcceleratorKeyPressed COM interop used instead.
- [RESOLVED] Implement args.Handled=true for error sound deflection at all four interception layers.
- [FIXED] F3/Ctrl+F find-in-page: Custom Catppuccin find bar via JS injection handles all find operations.
- [BY DESIGN] F12/Ctrl+Shift+J opens DevTools in a separate Chromium DevTools window. WebView2 `OpenDevToolsWindow()` always creates an external window; embedding requires DevTools Protocol which is out of scope.
- [FIXED] Enter in URL bar: Added `binding?.UpdateSource()` before `NavigateCommand` to flush TwoWay binding
- [FIXED] Esc handler consuming key before Chromium find-bar dismiss: Moved from WndProc pre-filter to KeyboardShortcutHandler
- [FIXED] Edge-themed native find bar replaced with custom Catppuccin Mocha find bar via JS injection
- [FIXED] CSS kebab→camelCase key mismatch: ToCamelCase() helper in ThemeService, targets ThemeDictionaries["Dark"]
- [FIXED] `CoreWebView2.Controller` not projected: COM QI workaround via Marshal.GetIUnknownForObject
- [FIXED] wwwroot/flags.html created for tb://flags route

## Completed This Session (Latest — 2026-06-12)
- ✅ **Unified stylesheet**: `tb.css` now single file — `@font-face` (4 Open Sans WOFF2), merged fallback vars, all Prism/component/page styles
- ✅ **Fonts consolidated**: TTF→WOFF2, moved from `Assets/Fonts/` to `wwwroot/Fonts/`, preloaded via `<link rel="preload">` in all internal pages
- ✅ **Deleted 3 CSS files**: `internal.css`, `settings.css`, `prism-base.css` (all merged into `tb.css`)
- ✅ **`.toggle` → `.toggle-switch`**: Single toggle component (36×20px, `.on`/`.knob`) shared by settings + flags pages
- ✅ **Flags page rewritten**: Uses `.toggle-switch` + `.flag-card` (no separate flag toggle CSS)
- ✅ **CSS files: 4 → 1** (`tb.css` only)
- ✅ **`HandleSourceChanged` simplified + URL masking**: Now `(int id)` only. Masks `file:///` with logical `tb://` for internal pages. Calls `FireNavState(id)` after every change
- ✅ **`HandleNavigationStarting` security upgrade**: Downgrades tab internal→standard when user clicks web links inside internal pages (unregisters IPC). Blocks manual `file:///` navigation on non-internal tabs via `args.Cancel = true`

- ✅ Theme pipeline redesigned: `GetAvailableThemes()` returns `IReadOnlyList<ThemeInfo>` with `Id`/`Name`
- ✅ `ThemeInfo` record (string Id, string Name) in `IThemeService` namespace
- ✅ Theme hot-reload pipeline: dropdown → `SAVE_SETTING(theme-name)` → `SetThemeAsync` → `ThemeChanged` → `PostWebMessageAsJson({action:"THEME_UPDATE",variables})` → `theme-sync.js` applies `:root`
- ✅ `theme-sync.js` 3-tier fallback: `__themeVariables` (host) → baked-in fallback colors (`file:///`) → layout vars only (last resort)
- ✅ `--white: #ffffff` removed from tb.css, moved to theme.json
- ✅ Accent picker reads `var(--accent)` at runtime instead of hardcoded `#ee82ee`
- ✅ Navigation bar: Download, Settings, Flags icon buttons added
- ✅ Created `wwwroot/themes/violet-dark.json` for `SetThemeAsync("violet-dark")`
- ✅ `SettingsReady` IPC response flattened: `{action:"SETTINGS_DATA",settings,themes}` (no `{c,a,d}` wrapper)
- ✅ `OnThemeChanged` sends `{action:"THEME_UPDATE",variables}` via `PostWebMessageAsJson` to all open `tb://` tabs
- ✅ `OnThemeChanged` also updates `__themeVariables` init script for future navigations
- ✅ `InjectThemeVariablesAsync` removed — replaced by dynamic init in Lifecycle.cs + THEME_UPDATE broadcast
- ✅ settings.html listens on BOTH `window` (`ExecuteScriptAsync` events) and `chrome.webview` (`PostWebMessageAsJson` events)
- ✅ `ThemeInfo` record (`Id`, `Name`) created in `Models/ThemeInfo.cs`
- ✅ `IThemeService`: `AvailableThemes` property → `GetAvailableThemes()` method returning `IReadOnlyList<ThemeInfo>`
- ✅ `ThemeService.GetAvailableThemes()` now returns `IReadOnlyList<ThemeInfo>` with display names via `ToDisplayName()` (dashes → spaces, capitalize)
- ✅ `SaveSetting` handler simplified: all values stored as strings, `"theme-select"` key check → `"theme-name"`
- ✅ `SettingsReady` handler: flattened `{action:"SETTINGS_DATA",settings,themes}` via `ExecuteScriptAsync` + `dispatchEvent`
- ✅ `theme-sync.js`: `fetch('theme.json')` removed — violet-dark fallback colors baked in as `--bg-app:#1a0f1a`, `--accent:#ee82ee`, etc.
- ✅ `theme-init.js` deleted (replaced by dynamic init in Lifecycle.cs after bridge script injection)
- ✅ `_themeInitScript` lazy field removed from TabManager.cs
- ✅ Build: 0 errors, 0 warnings


- ✅ App.xaml ThemeDictionaries[Dark/Light/HighContrast] declared as explicit elements (Option A)
- ✅ Theme persistence: IThemeService.ActiveThemeName + SetThemeAsync() via ISettingsService
- ✅ App.xaml.cs DI: ThemeService receives ISettingsService via sp.GetRequiredService
- ✅ **Single source of truth**: theme.json → XAML resources + CSS variables via GetCssVariables(). No separate CSS theme files.
- ✅ ThemeDictionaries populated identically (Dark/Light), no runtime dict creation needed
- ✅ Created `wwwroot/js/theme-sync.js` — reads __themeVariables, applies as CSS custom properties, handles THEME_UPDATE
- ✅ TabManager: InjectInitialTheme injects full CSS var dict; OnThemeChanged sends {action: "THEME_UPDATE", variables}
- ✅ ThemeService.GetCssVariables(): camelCase→kebab-case conversion
- ✅ tb.css :root — all var(--key) with NO fallback hexes
- ✅ Added \<script src="js/theme-sync.js"\> to all internal pages
- ✅ Deleted wwwroot/themes/github-dark.css
- ✅ **Hardcoded color purge**: ApplyNativeTheme() no fallback hexes; BoolToActiveBgConverter no fallback chain; crash page + find bar from GetCssVariables()
- ✅ Converters.cs: extra parens fixed in BoolToActiveBgConverter
- ✅ **Service Locator removed**: All 15 `App.Services.GetRequiredService<T>()` calls eliminated from TabManager.cs (10) and MainWindow.xaml.cs (5). Constructor injection used everywhere. MainWindow resolved via DI container.
- ✅ **Script injection timing fix**: `AddScriptToExecuteOnDocumentCreatedAsync` moved BEFORE `webView.Source` — bug was root cause of themes not working on first load
- ✅ **404.html created** with theme-sync.js + tb.css
- ✅ **Hardcoded Catppuccin colors purged from tb.css**: 15 rgba() values → `color-mix(in srgb, var(--key) X%, transparent)` (TB-style translucency)
- ✅ `--page-danger`, `--page-success` added to theme.json
- ✅ Empty `catch {}` in crash page handler now logs via `Logger.Error()`
- ✅ **Theme changed from github-dark to violet-dark** (accent #ee82ee, dark magenta backgrounds)
- ✅ **"Thick paper" → glass/slick UI**: All `color-mix(..., var(--page-bg))` → `color-mix(..., transparent)` for true alpha translucency. `backdrop-filter: blur(16px) saturate(120%)` on sidebar, cards, download items, toast, nav items, buttons. Subtle gradient page background for visible blur depth. Border colors switched from opaque `var(--page-border)` to translucent `var(--tb-elevated-N)` (accent-based)
- ✅ **White pages regression fixed**: Reverted `html, body background` to safe `var(--page-bg)`. Gradient moved to `@supports (color-mix)` progressive enhancement (only activates when CSS vars + color-mix both valid). Added `-webkit-backdrop-filter` for WebView2 compat
- ✅ Build: 0 errors, 0 warnings
- ✅ PLAN.md updated with completed tasks and revised project structure updates
- ✅ **Fallback theme resources**: ChromeResources.xaml now declares fallback `SolidColorBrush` (bgAppBrush, textMainBrush, etc.), `x:Double`, and `CornerRadius` values so `{ThemeResource}` never throws if theme.json fails to load
- ✅ **IPC origin verification**: `SetupInternalPageIpc` now validates `e.Source` starts with `tb://` before processing SAVE_SETTING, CLEAR_DOWNLOADS, REMOVE_DOWNLOAD
- ✅ **Startup diagnostics**: `App.xaml.cs` logs `bgAppBrush exists` and theme dictionary count after `ApplyXamlResources()`
- ✅ **Right-click context menu rework**: Replaced XAML `RightTapped` with `PointerPressed` (fires earlier in the input pipeline) + `WM_NCRBUTTONDOWN` suppression in WndProc + dynamic `MenuFlyout` creation in code-behind. Deleted `Views/TabContextMenu.xaml` and removed from App.xaml. Build: 0 errors, 0 warnings.
- ✅ **Title bar drag regions**: Added `AppWindow.TitleBar.SetDragRectangles()` to exclude the tab strip from the caption area. Only the navigation bar is a drag region, so DWM never intercepts right-clicks on tabs. Updated on window resize via `RootGrid.SizeChanged`.
- ✅ **TB-styled context menu**: MenuFlyout uses `TBMenuFlyoutPresenterStyle` (bgAppBrush bg, borderCrispBrush border, radiusMd corners, 4px padding) and `TBMenuFlyoutItemStyle` (Open Sans 13px, textMain fg, 32px height, 8px horizontal padding). Defined in ChromeResources.xaml, applied in `ShowTabContextMenu`.
- ✅ **Open Sans font in WinUI chrome**: Added implicit `TextBlock`, `TextBox`, and `Button` styles in ChromeResources.xaml with `FontFamily="ms-appx:///Assets/Fonts/OpenSans-Regular.ttf#Open Sans"` for consistent typography across the UI.
- ✅ **Session persistence**: New `Models/SessionState.cs` + `SessionState`/`TabEntry` serializable model. `TabManager.SaveSessionAsync()` saves tabs (url, title) + active index to `AppData/session.json`. `TryLoadSessionAsync()` restores on startup. Auto-save on: tab created, closed, switched, moved (left/right), navigated (URL change), `DocumentTitleChanged`, `SourceChanged`, and `DisposeAsync` (shutdown). `SessionPersistence` replaces the hardcoded single-tab startup.
- ✅ **ValidateState (DEBUG)**: `AssertConsistent` now checks tab count == WebView count == zoom count, active tab has WebView, internal page tabs have WebViews. Called after every mutation.
- ✅ **Crash page reload**: `ProcessFailed` handler now captures the crash URL from the tab model and inlines it into the crash HTML as `window.location=url` onclick, so clicking "Click to reload" actually reloads the original page.
- ✅ **IPC hardening**: All `GetProperty` calls replaced with `TryGetProperty` + type checks in `SetupInternalPageIpc` (action, key, value, id fields). Malformed JSON, missing fields, or wrong types are logged and dropped instead of throwing.
- ✅ **Page right-click context menu**: Disabled `AreDefaultContextMenusEnabled` in WebView2 settings. Subscribed to `CoreWebView2.ContextMenuRequested` (args.Handled=true) to show a custom TB-styled `MenuFlyout` with dynamic Back/Forward (enabled based on CanGoBack/CanGoForward), Reload, separator, and Inspect (DevTools) items. Styled with `TBMenuFlyoutPresenterStyle` + `TBMenuFlyoutItemStyle`.
- ✅ **Page context menu rework (crash fix)**: Removed shared XAML `MenuFlyout` resources (crashed on show). Both tab and page context menus now build fresh `MenuFlyout` instances per-show programmatically. Page context uses JS context detection (`document.elementFromPoint` + `closest('a','img','input')`) to show appropriate items (Standard/Link/Image/Text modes). `DuplicateTabAsync(int id)` + `ITabManager.DuplicateTabAsync`. `NewTab` command on `TabItemViewModel`. Build: 0 errors, 0 warnings.
- ✅ **Residual crash fixes**: Tab context menu wrapped in try/catch (was crashing intermittently on detached elements). Page context menu now re-checks `_disposed` + `CoreWebView2!=null` after JS `await` + try/catch around `ShowAt`. PointerPressed handler wrapped in try/catch for additional crash safety.
- ✅ **Session save race fix**: Added `_isRestoring` flag to skip session saves during tab restore (was causing file-lock errors and potential corruption). Added `_sessionSaveLock` semaphore for exclusive file access during saves. Session saved once at end of restore instead of per-tab.
- ✅ **URI registry audit fixes**: 3 violations fixed — `NavigationBar.xaml.cs` (`UrlResolver.IsInternalUrl`), `TabManager.Ipc.cs` x2 (`UrlResolver.IsInternalUrl` + `Routes.Downloads`). DI singleton audit: `ChromeViewModel.cs` (`ActivatorUtilities.CreateInstance` + `IServiceProvider` + `AddTransient<TabItemViewModel>`)
- ✅ **ThemeService.cs complete rewrite**: Removed `RequestedTheme` toggle hack; Light dictionary gets derived colors (bg→lighten 75%, text→darken 85%); `FileSystemWatcher` events wrapped in `_dispatcherQueue.TryEnqueue()`; all lookups use `TryGetValue`; `CycleTheme` persists `"theme-mode"`; startup reads persisted mode
- ✅ **Phase 1 — Collection expressions `[]`**: 8 files updated — `TabManager.cs` (10 fields), `TabManager.Ipc.cs`, `ThemeService.cs`, `CommandRegistry.cs`, `KeyboardShortcutHandler.cs`, `DownloadService.cs`, `ThemeDefinition.cs`, `SessionState.cs`
- ✅ **Event args extracted**: `TabEventArgs`, `UrlEventArgs`, `FaviconEventArgs`, `NavStateEventArgs`, `TabMovedEventArgs` moved from `ITabManager.cs` to new `Models/Events.cs`
- ✅ **DownloadService relocated**: `DownloadService.cs`, `DownloadItem.cs`, `DownloadViewModel.cs` moved from `Features/Downloads/` to `Services/Downloads/` (namespace `TB.Features.Downloads` → `TB.Services.Downloads`). `Features/` directory deleted.
- ✅ **Logger upgraded**: Added `Error(string context, Exception ex)` overload writing full stack trace. 5 catch blocks in ThemeService.cs + 1 in ThemeDefinition.cs updated to use new overload.
- ✅ **Phase 2 — Pattern matching guard clauses**: 4 files updated — `TabManager.Commands.cs` (`is not { } tab`), `TabStrip.xaml.cs` (early return), `NavigationBar.xaml.cs` (recursive `{ Parent: ... }` patterns x2), `SettingsService.cs` (ternary `val is JsonElement element`)
- ✅ **Phase 4 — JsonSerializer.Serialize**: `SetZoomAsync` CDTP call replaced fragile string formatting with `JsonSerializer.Serialize(new { scaleFactor = zoom })`. Removed unused `System.Globalization` import.
- ✅ **Exception visibility — empty catch blocks**: All 10 `catch { }` in TabManager partials replaced with typed `catch (Exception ex)` + `Logger.Debug`/`Logger.Warn` per RULES.md §5 three-tier dispatch. Single intentional exception at `Logger.cs:28`.

## Next Steps (Priority Order)
- [x] Theme pipeline — `GetAvailableThemes()` + `ThemeInfo` record + hot-reload broadcast
- [x] Audit colors — `#ffffff` → `theme.json`, `#ee82ee` → `var(--accent)` at runtime
- [x] Nav bar icons — Download, Settings, Flags buttons
- [x] `violet-dark.json` theme pack
- [x] `SettingsReady` flat response format
- [x] `OnThemeChanged` → `PostWebMessageAsJson` broadcast
- [x] `tb.css` unified — single file with `@font-face`, merged fallback vars, all components + layouts
- [x] Fonts: TTF→WOFF2, moved to `wwwroot/Fonts/`, preloaded in all pages
- [x] Deleted `internal.css`, `settings.css`, `prism-base.css` (merged into `tb.css`)
- [x] `.toggle` → `.toggle-switch` — single component for all pages
- [x] Flags page rewritten: `.toggle-switch` + `.flag-card`
- [ ] Implement tab suspension (discard WebView2 on low memory / inactivity)
- [ ] Separate "Close window" vs "Close tab" pathways
- [ ] Global exception handler
- [ ] Hot-reload durability — `FileShare.Read` with retry

## Modernization Plan (Complete ✅ — ~13 files, 4 phases)

### Phase 1 — Modern C# Collection Expressions `[]` ✅
All locations updated. Replaces `new List<T>()` / `new HashSet<T>()` / `new Dictionary<K,V>()` field initializers with `[]`.

| File | Lines | Change |
|------|-------|--------|
| `Services/TabManager.cs` | 32-41 | ✅ 10 field initializers switched to `[]` |
| `Services/TabManager.Ipc.cs` | 136 | ✅ `HashSet<int> targetIds = []` |
| `Services/ThemeService.cs` | 282 | ✅ `Dictionary<string, string> vars = []` |
| `Input/CommandRegistry.cs` | 16,19 | ✅ `_bindingMap = []`, `_bindings = []` |
| `Input/KeyboardShortcutHandler.cs` | 14 | ✅ `= [VirtualKey.W, VirtualKey.F4]` |
| `Services/Downloads/DownloadService.cs` | 14 | ✅ `_downloads = []` |
| `Models/ThemeDefinition.cs` | 17 | ✅ `Colors = []` |
| `Models/SessionState.cs` | 5 | ✅ `Tabs = []` |

### Phase 2 — Modern Pattern Matching & Guard Clauses ✅
All locations updated.

| File | Lines | Change |
|------|-------|--------|
| `Services/TabManager.Commands.cs` | 38-39 | ✅ `is not { } tab` pattern |
| `Controls/TabStrip.xaml.cs` | 59-72 | ✅ Early return guard (flattened right-click check) |
| `Controls/NavigationBar.xaml.cs` | 88,108 | ✅ Recursive `{ Parent: { Parent: ... } }` patterns |
| `Infrastructure/SettingsService.cs` | 63-68 | ✅ Ternary `val is JsonElement element ? ... : ...` |

### Phase 3 — DispatcherQueue Modernization (`HasThreadAccess`) ✅
Also applied to `Services/ThemeService.cs` — `FileSystemWatcher` events + `ApplyXamlResources()` dispatch wrapped in `_dispatcherQueue.TryEnqueue()`.

All locations updated. `_dispatcherQueue` field captured once at constructor time; re-entry pattern avoids lambda duplication.

| File | Lines | Change |
|------|-------|--------|
| `Services/TabManager.Ipc.cs` | 126-141 | ✅ `SendToDownloadOwner`: `HasThreadAccess` guard with re-entry dispatch |
| `Services/TabManager.Ipc.cs` | 145-151 | ✅ `OnThemeChanged`: `HasThreadAccess` guard with re-entry dispatch |
| `Services/TabManager.Lifecycle.cs` | 147 | ✅ `CloseTab` exit: inline `Application.Current.Exit()` if on UI thread |
| `Services/TabManager.Lifecycle.cs` | 151 | ✅ `CloseTab` cleanup: `HasThreadAccess` guard with Low priority dispatch |
| `ViewModels/ChromeViewModel.cs` | 27-50 | ✅ All 4 event handlers: `DispatcherQueue.HasThreadAccess` guards around `Tabs.Add`/`Remove`/iteration |

### Phase 4 — WebView2 Interop Cleanup (JsonSerializer) ✅
All locations updated.

| File | Lines | Change |
|------|-------|--------|
| `Services/TabManager.Commands.cs` | 71 | ✅ CDTP zoom: `$"{{ \"scaleFactor\": ... }}"` → `JsonSerializer.Serialize(new { scaleFactor = zoom })` |
| ~~`Services/TabManager.Commands.cs`~~ | ~~72~~ | ~~Fallback zoom~~ ✅ Already using `JsonSerializer.Serialize` |
| ~~`Services/TabManager.Ipc.cs`~~ | ~~61~~ | ~~Settings dispatch~~ ✅ Already using `IpcActions` const, no interpolation |

### Verification
```powershell
dotnet build --no-restore
# Expected: Build succeeded. 0 warnings, 0 errors
```

### Excluded (rationale)
- `ChromeViewModel.cs:20` — `ObservableCollection<T>` lacks `[CollectionBuilder]`, cannot use `[]`
- `KeyboardShortcutHandler.cs:17` — `ConcurrentDictionary` lacks `[CollectionBuilder]`
- `SettingsService.cs:25`, `ThemeService.cs:180` — content-populated dicts, not empty `new()`
- `MainWindow.xaml.cs` — already uses `MainWindow.DispatcherQueue` (not `GetForCurrentThread`)
- `App.xaml.cs:118` — already uses `MainWindow?.DispatcherQueue` (safe pattern)
- `TabManager.Commands.cs:80-82, 84-85, 105-138` — `window.__findNext`, `window.print()`, etc. are fixed JS call patterns (no user data injected), safe as-is

## Project Location
The actual implementation is located at: C:\Users\RTPC\Documents\TB
All plan specifications match the project requirements.

## Required Specs
<!-- SPECS_START -->

## 🚫 STRICTLY OUT OF SCOPE (DELETED / FORBIDDEN)
- Compile-time {x:Bind} Syntax: Permanently removed. The project standard is strictly runtime reflection {Binding} to match our functional codebase architecture.
- Zen Mode / Slide-down Hidden Gestures: Permanently removed. The top browser chrome must remain permanently visible and flat at all times. No hidden hover thresholds.
- Tab Pinning Features: Permanently removed. The tab collection manager will not allocate state properties for sticky or minimized icon-only pinned headers.
- Tab Grouping Features: Permanently removed. No visual grouping borders, color-coded tab rings, or multi-tab cluster data models are allowed.

All master architectural, input, styling, and interaction specifications are consolidated in `RULES.md`.

### 🚀 Modern Framework & Documentation Governance Layer [MANDATORY]
Governance rules fully defined in `RULES.md` Section 3, including:
- Absolute mandate for latest framework standards (C# 14, WinAppSDK 2.1+, WinUI 3)
- Mandatory official Microsoft Learn documentation reference loop before any API/new-control/interop change
- Modern async, collection expressions `[]`, pattern matching, DispatcherQueue.HasThreadAccess enforcement
- Zero refactor drift — no monolithic re-compression of decomposed partial files
- All refactoring phases tracked in `PLAN.md` Modernization Plan (Phases 1-4 above)

### 🚫 Empty Catch Block Ban [MANDATORY]
Governance rule fully defined in `RULES.md` Section 5 (Performance, Resilience, & Thread Safety):
- Zero silent swallowing — all `catch` blocks must use `catch (Exception ex)` with appropriate logging
- Three-tier dispatch: `Logger.Debug` (cleanup), `Logger.Warn` (interop/events), `Logger.Error` (high-risk faults)
- Single exception: `Infrastructure/Logger.cs:28` to prevent recursive stack overflow

### 📦 Strict File Decomposition & Resource Centralization [MANDATORY]
Governance rules fully defined in `RULES.md` Section 1, including:
- Absolute ban on monolithic files — decompose on responsibility boundary
- Semantic modular naming conventions (self-describing file/class/interface names)
- Zero inline colors or hardcoded object replicas — all styles via centralized `ThemeDictionaries` or config service store
- Centralized URI protocol registry — all `tb://` routes as `public const string` in `Helpers/Constants.cs`
- Absolute DI singleton enforcement — no `new` for shared managers, all via `App.xaml.cs` container

## 📐 MANDATORY HELIUM SQUASHING PHYSICS [ACTIVE]
- Linear Fractional Distribution: Tab sizing must shrink dynamically and fluidly without arbitrary minimum width walls (like 140px ceilings), layout jiggling, or hover expansions. 
- Symmetrical Compression: As the user loads more sessions, all tab widths must scale down symmetrically and uniformly until they hit the absolute 32px baseline floor (housing only the dead-centered 16x16 favicon).

## 📐 Tab Right-Click Context Menu Specification [IMPLEMENTED]
- **Issue:** Right-click on tabs in extended title bar shows native system menu (Restore/Move/Minimize/Close) instead of our custom menu.
- **Root Cause:** DWM intercepts right-click for windows with `ExtendsContentIntoTitleBar = true` and shows the system menu via `WM_NCHITTEST → HTCAPTION → WM_SYSCOMMAND/SC_MOUSEMENU` path, bypassing WinUI's input pipeline.
- **Attempted fixes (all failed):**
   1. `ContextFlyout` on tab buttons — WinUI never receives the event
   2. `RightTapped` / `PointerPressed` with `handledEventsToo` — same
   3. `ContextRequested` with `Handled = true` — same
   4. `WM_NCRBUTTONDOWN` suppression in WndProc — DWM handles before WndProc
   5. `WM_SYSCOMMAND/SC_MOUSEMENU` suppression — DWM may still show menu
- **Final solution (working):** Three-layer approach: (1) Remove `WS_SYSMENU` window style + `SetWindowPos(SWP_FRAMECHANGED)` in `RootGrid.Loaded`; (2) Set custom drag rectangles via `AppWindow.TitleBar.SetDragRectangles()` — only the navigation bar is a drag region; the tab strip is excluded so DWM never intercepts right-clicks on tabs; (3) `PointerPressed` handler on each tab `Button` checks `IsRightButtonPressed`, sets `e.Handled = true`, and creates a `MenuFlyout` dynamically in code-behind. `WndProcHook` also suppresses `WM_NCRBUTTONDOWN` (0xA4) as a backup on Windows 10.
- **Items:** New Tab | Reload | Close | Close Other Tabs

## 📐 Multi-Mode Page WebView2 Context Menu Logic [IMPLEMENTED]
- **Trigger:** `CoreWebView2.ContextMenuRequested` event (args.Handled=true) on each tab's WebView2. `AreDefaultContextMenusEnabled = true` — native menu now shows as fallback; custom handler runs alongside.
- **JS Context Detection Pipeline:**
  ```javascript
  var el = document.elementFromPoint(e.Location.x, e.Location.y);
  ```
  Then walks `el.closest('a')`, `el.closest('img')`, `el.closest('input, textarea, [contenteditable]')` to classify the context.
- **Four Menu Modes (all built fresh per-show — no shared XAML flyouts):**
  | Mode | Detection | Items |
  |------|-----------|-------|
  | **Standard** | No special element match | Back (if CanGoBack), Forward (if CanGoForward), Reload, separator, Inspect (DevTools) |
  | **Link** | `closest('a')` yields an anchor | Open Link in New Tab (→ `CreateTabAsync`), Copy Link Address (→ clipboard), separator + Standard items |
  | **Image** | `closest('img')` yields an image | Copy Image, Open Image in New Tab, separator + Standard items |
  | **Editable** | `closest('input, textarea, [contenteditable]')` | Undo, Redo, separator, Cut, Copy, Paste, separator, Select All, separator + Standard items |
- **TB Styling:** All flyouts use `TBMenuFlyoutPresenterStyle` (bgAppBrush bg, radiusMd corners, 4px padding) and `TBMenuFlyoutItemStyle` (Open Sans 13px, textMain fg, 32px height, 8px horizontal padding). Both styles defined in `Styles/ChromeResources.xaml`.
- **Crash Guards:** `_disposed` + `CoreWebView2==null` re-checked after JS `await` (WebView2 may close during async gap). `ShowAt()` wrapped in try/catch. PointerPressed handler on tab buttons also wrapped in try/catch.
- **Duplicate Tab Command:** `ITabManager.DuplicateTabAsync(int id)` creates a new tab with the same URL as the source tab. Exposed on `TabItemViewModel` as `[RelayCommand] Duplicate`. Wired in `TabManager.Commands.cs:DuplicateTabAsync`.
- **New Tab Command:** `TabItemViewModel.NewTab` command calls `ITabManager.CreateTabAsync()`. Used in both tab strip right-click and page context menu.
- **Key lesson:** XAML `MenuFlyout` resources declared as `<Application.Resources>` **cannot** be reused via `ShowAt()` from different placement targets — WinUI 3 crashes with 0xc000027b. All flyout instances are created programmatically per-show.

## 📐 Complete TB Navigation Bar & Omnibar Architecture Spec [ACTIVE]

### 1. Core Component Layout & Dimensions
- **Global Height Anchor**: The entire navigation bar grid is locked to a flat height constraint of **36px**. It sits directly underneath the tab bar, separated only by a clean 0px gap layout boundary.
- **Action Control Group (Left Block)**: Contains the core navigation buttons: Back, Forward, and Reload/Stop.
- **Button Box Boundaries**: Each button sits in a square bounding box container of exactly **24px × 24px**.
- **Icon Glyph Sizing**: Core glyph markings (Segoe Fluent Icons) are micro-scaled to an internal frame of exactly **14px** to maximize visual white space.
- **TB Omnibar (Central Block)**: The address input bar is fluid, automatically expanding horizontally to consume all remaining space between the Left Action Group and the Right Extension/Menu targets.
- **Omnibar Height Box**: Fixed at **28px** inside the 36px navigation strip, leaving a mathematically perfect 4px top and bottom layout spacing margin.
- **Omnibar Rounded Profiles**: Exact, uniform **4px corner radius** boundary on all four corners.

### 2. Spacing, Margins, & Alignment Detailing
- **Inter-Button Micro-Gaps**: Navigation buttons are tightly packed, separated by a structural horizontal gap of exactly **4px**.
- **Action-to-Omnibar Buffer Column**: A strict **8px** margin column spaces out the right edge of the Reload button from the left starting bounds of the Omnibar frame.
- **End-Cap Menu Spacing**: An **8px** spacer column separates the right boundary edge of the Omnibar from the primary Menu trigger button, followed by a final **4px** flush inset margin before hitting the outer window shell.

### 3. Advanced Pointer & Hover Physics
- **No Blocky Outlines**: TB completely bans blocky outline frames or sudden bright highlights on hover. Uses low-overhead alpha interpolation states to convey depth.
- **Hover Box Dimension**: When the cursor hits a navigation button, the circle background indicator scales exactly within the **24px × 24px** bounding zone. Never pushes or shifts adjacent components.
- **Alpha Illumination Track**:
  - *Base/Idle State*: Background ring completely clear (Alpha = **0.00**). Inner glyph icon maps softly to Catppuccin Subtext (`#a6adc8`).
  - *Hovered State*: Low-latency transition fades the background box to Alpha = **0.15** (rgba(255,255,255,0.15) or theme equivalent). Inner icon instantly brightens to crisp white (`#cdd6f4`).

**Visual Layout:**
```
[4px] [24px Back] [4px] [24px Fwd] [4px] [24px Reload] [8px] [Omnibar Fluid] [8px] [24px Menu] [4px]
```

## 🖱️ TB Hover & Visual Interaction Schema (IMPLEMENTED)
- Tab Strip Width Isolation: No recalc while cursor in ChromeContainer. Deferred until full pointer exit (anti-jiggle).
- Opacity Pipeline:
  * Active Tab: Opacity 1.0, blending flush with toolbar background
  * Inactive Unhovered: Opacity 0.40 for Mica/Acrylic translucency
  * Inactive Hovered: Opacity 0.85 with micro-tinted border, dimensions unchanged
- Close Button Trap: Hidden at ≤36px tab width, re-revealed on per-tab PointerEntered.

## User32 WinRT Interop Window Specification
- HWND Retrieval Core: WinRT.Interop.WindowNative.GetWindowHandle(this)
- Extended Window Styles Flags: GWL_EXSTYLE, WS_EX_LAYERED, WS_EX_TRANSLUCENT
- Hard Window Constraints: Track WM_GETMINMAXINFO to enforce 400x300px minimum

## 🌐 1. Core Browser Functions Module [ACTIVE]

### Favicon & Security Module
- **Favicon Support**: Native site icon fetching for browser tabs
- **Security Indicators**: HTTPS lock indicator display for connection security
- **SSL Verification**: Secure certificate validation and visual feedback

### Smart Routing Module
- **URL Detection**: Intelligent pattern recognition between search queries and direct URLs
- **Query Parsing**: Context-aware search intent identification
- **Redirection Logic**: Dynamic routing based on input type

### Snapping Integration Module
- **Windows Snap Integration**: Perfect compatibility with Windows Snap Layouts
- **Multi-Monitor Support**: Cross-screen window positioning
- **Responsive Sizing**: Automatic adjustment for different screen configurations

## 📐 2. Core Omni Routing Bangs Spec [PLANNED]
- Intercept search queries using regex pattern matching: ^!(?<bang>[a-zA-Z0-9]+)\s+(?<query>.+)$
- Map bangs directly to direct endpoints without cloud telemetry proxies:
  * !gh -> ://github.com
  * !yt -> ://youtube.com
  * !w  -> wikipedia.org/w/index.php?search=

## 📐 3. Native Split-View Multi-Tasking Spec [PLANNED]
- UI Layout: Column-allocation Grid infrastructure inside MainWindow.xaml.
- Execution: Toggled horizontal 50/50 dual-WebView2 views driven by ViewModel state flags.

## ⌨️ 4. Chrome Parity Keyboard Shortcuts Master Matrix [ACTIVE]

### Tab and Window Management
- **New Window**: Ctrl + N | Navigate to a fresh browser instance
- **New Tab**: Ctrl + T | Open tab at the right of current selection
- **New Tab (Foreground)**: Ctrl + Shift + T | Open and immediately switch to new tab
- **New Incognito Window**: Ctrl + Shift + N | Private browsing session
- **Reopen Last Closed Tab**: Ctrl + Shift + T | Restore most recently closed tab (10-tab limit)
- **Close Current Tab**: Ctrl + W or Ctrl + F4 | Close active tab
- **Close Window**: Alt + F4 or Ctrl + Shift + W | Close current window
- **Switch Tabs**: Ctrl + Tab | Move to next tab
- **Switch Previous Tab**: Ctrl + Shift + Tab | Move to previous tab
- **Jump to Specific Tab**: Ctrl + 1-8 | Switch to tab by position
- **Jump to Last Tab**: Ctrl + 9 | Move to rightmost tab
- **Move Tab Right**: Ctrl + Shift + PageDown | Reorder active tab to right
- **Move Tab Left**: Ctrl + Shift + PageUp | Reorder active tab to left

### Navigation Controls
- **Go Back**: Alt + Left Arrow | Navigate to previous page
- **Go Forward**: Alt + Right Arrow | Navigate to next page
- **Open Home Page**: Alt + Home | Return to browser's home page
- **Refresh Current Page**: F5 or Ctrl + R | Reload current page without cache
- **Force Refresh**: Ctrl + Shift + R | Reload with cache bypass
- **Stop Loading**: Esc | Cancel current page load
- **Scroll Down One Screen**: Space Bar | Page down
- **Scroll Up One Screen**: Shift + Space Bar | Page up
- **Go to Top of Page**: Home | Jump to beginning
- **Go to Bottom of Page**: End | Jump to end

### Browser Feature Access
- **Open Chrome Menu**: Alt + F or Alt + E or F10 | Access main browser menu
- **Show/Hide Bookmarks Bar**: Ctrl + Shift + B | Toggle bookmark toolbar visibility
- **Open Bookmarks Manager**: Ctrl + Shift + O | Manage bookmark organization
- **Open History Page**: Ctrl + H | View browsing history
- **Open Downloads Page**: Ctrl + J | View download manager
- **Open Chrome Task Manager**: Shift + Esc | Monitor browser process performance
- **Open Developer Tools**: F12 or Ctrl + Shift + J | Launch DevTools for debugging
- **Open Find Bar**: Ctrl + F or F3 | Search within current page
- **Find Next Match**: Ctrl + G or F3 | Move to next search result
- **Find Previous Match**: Ctrl + Shift + G, Shift + F3, or Shift + Enter | Move to previous search result
- **Clear Browsing Data**: Ctrl + Shift + Delete | Open privacy settings
- **Open Feedback Window**: Alt + Shift + I | Submit browser feedback

### Address Bar Controls
- **Focus Address Bar**: Alt + D | Select entire address bar text
- **Select All Text**: Ctrl + A | Highlight all address bar content
- **End of Input**: End | Move cursor to end of address bar
- **Home of Input**: Home | Move cursor to start of address bar
- **Search with Default Engine**: Ctrl + Enter | Append .com to query
- **Navigate with Custom Engine**: Ctrl + Enter + "yahoo.com" | Search with custom site

### Zoom and Display
- **Zoom In**: Ctrl + + | Increase page zoom by 10%
- **Zoom Out**: Ctrl + - | Decrease page zoom by 10%
- **Reset Zoom**: Ctrl + 0 | Return to 100% zoom
- **Full Screen Mode**: F11 | Toggle fullscreen viewing
- **Select Toolbar Items**: Shift + Alt + T | Access toolbar
- **Select Rightmost Toolbar Item**: F10 | Focus on rightmost toolbar element
- **Switch Focus Between Elements**: F6 | Cycle through page elements

### Accessibility and Selection
- **Move Cursor to Previous Word**: Ctrl + Left Arrow | Word-level navigation
- **Move Cursor to Next Word**: Ctrl + Right Arrow | Word-level navigation
- **Delete Previous Word**: Ctrl + Backspace | Delete word content
- **Select Multiple Tabs**: F6 twice then Shift + Ctrl + H | Multi-tab selection mode

## 📐 TB Visual Layout Spec (Downloads & Settings)
- Theme Configuration: Enforce global Catppuccin Mocha hex values (#1e1e2e, #313244, #89b4fa, #cdd6f4) across all HTML text, backgrounds, and control borders. Inline colors are strictly banned.
- Settings Panel Grid: Implement a fixed 200px sidebar routing view alongside a fluid right-aligned settings item block using 16px row padding lines and 4px input border-radius limits.
- Downloads Progress Matrix: Structure download entries inside clean horizontal stacked containers using CSS Grid. Enforce a flat 6px height on all custom HTML progress bars running an absolute 0ms rendering latency constraint to ensure real-time data tracking accuracy.

## 📐 Custom Catppuccin Find-In-Page Specification [PLANNED]
- Core Intercept Logic: Intercept `Ctrl + F` keyboard actions inside `KeyboardShortcutHandler.cs` and force return true to kill native process UI initialization loops.
- Floating View Injection: Programmatically inject a lightweight, floating web component into the page layout viewport using `CoreWebView2.ExecuteScriptAsync`. The container element must strictly track centralized Catppuccin Mocha tokens (#1e1e2e, #45475a), completely replacing the default Edge gray background bars.
- Automation Pipeline: Map query strings asynchronously using native Chromium DevTools Protocol (CDP) execution hooks to handle background string parsing while keeping the interface flat and lightweight.

## 📐 TB Custom Find Dialog Property Matrix [ACTIVE]
- Structural Grid Boundaries: Absolute viewport overlay positioning fixed at Width: 340px, Height: 48px. Offset coordinates clamped strictly to Top: 61px (below 32px tab strip + 1px separator + 28px nav bar), Right: 8px with z-index: 99999.
- Frame Styling Specs: Enforce Catppuccin Mocha canvas rendering (#1e1e2e background, 1px solid #45475a border wrap, sharp 4px uniform corner radius). Inline styles are forbidden.
- Spacing & Micro-Margins: Input node font size locked to 13px. Match counter telemetry tracks at 11px with a 12px right padding channel. Individual navigation action targets fixed at 22px x 22px square targets separated by 4px inter-button gaps and a final 8px close item spacer column.
- Interaction Pointer Track: Base background opacity Alpha=0.00. Interactive hover states must interpolate instantly to Alpha=0.15 background fill with zero boundary dimensions distortion.

## 📐 Theme Hot Reload Runtime Specification [PLANNED]
- Core Dispatch Engine: Implement a global event notifier within `IThemeService`. Toggling themes must instantly overwrite native `Application.Current.Resources` color brushes in memory without application thread restarts.
- Web Canvas Synchronization: Broadcast an asynchronous JSON theme payload via `PostWebMessageAsJson` to all active internal pages. Internal HTML views must capture the payload and dynamically modify root CSS variables in real time with 0ms visual flickering.

## 📋 Audit Findings (2026-06-12)

### 🔴 Inline Colors / Hardcoded Objects (3 files violating §1 — 2 resolved)
| File | Violations | Severity |
|------|-----------|----------|
| ~~`Styles/ChromeResources.xaml:13-20`~~ | ~~7 hex fallback brushes~~ | ✅ RESOLVED — moved to `Views/Themes/Themes.xaml` |
| ~~`Services/TabManager.Events.cs:68`~~ | ~~2 inline hexes in crash HTML~~ | ✅ RESOLVED — extracted to `wwwroot/crash.html` |
| ~~`wwwroot/css/tb.css:40`~~ | ~~1 hardcoded `#ffffff`~~ | ✅ RESOLVED — `--white` removed from tb.css, added to theme.json as `white` |
| ~~`wwwroot/settings.html:173`~~ | ~~1 hardcoded `#ee82ee` in color-picker~~ | ✅ RESOLVED — default set dynamically from `var(--accent)` at runtime |
| ~~`Services/ThemeService.cs:131-163`~~ | ~~11 `Colors.*` named fallback refs~~ | ✅ RESOLVED — replaced with `TryGetValue` |

### 🔴 Monolithic Files — ✅ RESOLVED
- `Services/ThemeService.cs` — Rewritten with extracted `DeriveLightColor`/`Lighten`/`Darken`/`ApplyLayoutResources` helpers. No longer oversized.

### 🔴 File Naming / Decomposition — ✅ ALL RESOLVED
| File | Issue | Action |
|------|-------|--------|
| ~~`Helpers/Constants.cs`~~ | ~~4 unrelated classes~~ | ✅ Split into `Routes.cs`, `Actions.cs`, `Defaults.cs`, `Layout.cs` |
| ~~`Helpers/Converters.cs`~~ | ~~2 unrelated converters~~ | ✅ Split into `BoolToActiveBgConverter.cs`, `BoolToVisibilityConverter.cs` |
| ~~`Helpers/Extensions.cs`~~ | ~~ColorExtensions + TaskExtensions~~ | ✅ Split into `ColorExtensions.cs`, `TaskExtensions.cs` |
| ~~`Views/`~~ | ~~Empty directory~~ | ✅ Deleted; `Themes/` is upstream of `App.xaml` |
| ~~`Services/Interfaces/ITabManager.cs`~~ | ~~Interface + 5 event args classes~~ | ✅ Extracted to `Models/Events.cs` |
| ~~`Features/Downloads/DownloadService.cs`~~ | ~~Impl not in `Services/`~~ | ✅ Moved to `Services/Downloads/` |

### 🟡 Modern Framework Phase 3 — Completed ✅
- ✅ `Services/TabManager.Ipc.cs:126-151` — `HasThreadAccess` pattern applied
- ✅ `Services/TabManager.Lifecycle.cs:147-151` — same

<!-- SPECS_END -->