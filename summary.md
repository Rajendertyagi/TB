## Goal
- Rewrite TB browser chrome from HTML/WebView2 to native XAML/WinUI 3 with MVVM, Helium violet theme, DI, production-grade code.

## Constraints & Preferences
- **No hardcoding** — constants classes (Routes, Actions, Defaults), no magic strings/colors.
- **Separate file per concern** — interfaces, implementations, ViewModels.
- **Modular DI** — Microsoft.Extensions.DependencyInjection, interface-based, loose coupling.
- **No inline colors** — all from theme.json, applied by ThemeService as XAML resources.
- **MVVM** — CommunityToolkit.Mvvm (ObservableObject, RelayCommand), proper async/await, IAsyncDisposable.
- **No hacks** — no Win32 hooks, no IPC for chrome, no async void.
- **Production ready** — nullable enabled, disposal patterns, error handling, session restore capability.
- **Helium violet theme** — #1C1130 deep violet background, #A855F7 accent, #E8D5F5 text, flat opaque backgrounds (no Mica/Acrylic).
- **Keep theme.json** as sole source for colors/layout constants.
- **Keep internal pages** (settings.html, downloads.html) in tab WebView2s with IPC.
- **Delete obsolete files**: index.html, styles.css, app.js, icons.js, ipc.js.

## Progress
### Done
- TB.csproj updated with CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 9.0.0, ImplicitUsings enabled.
- Helpers/: Constants.cs, UrlResolver.cs, Extensions.cs (ColorExtensions.ParseHex, TaskExtensions.FireAndForget).
- Models/: TabItem.cs, ThemeDefinition.cs (typed theme.json parser).
- Services/Interfaces/: IThemeService, ITabManager, INavigationService, ISettingsService, IDownloadService — all with event args.
- Services/ThemeService.cs — loads theme.json, applies AppWindow colors + XAML ResourceDictionary brushes.
- Services/NavigationService.cs — delegates to ITabManager, resolves URLs.
- Services/TabManager.cs — rewrite: two-phase init (InitializeAsync), per-tab zoom, internal page IPC (settings/downloads), download starting handler, IAsyncDisposable, events instead of IPC.
- Infrastructure/SettingsService.cs — implements ISettingsService.
- Infrastructure/Logger.cs — added Warn method.
- Styles/Icons.xaml — 19 x:String path data + NavIconPathStyle, SmallIconPathStyle, NavButtonStyle, NewTabButtonStyle.
- Styles/ChromeResources.xaml — fallback brushes/lengths, merged with Icons.xaml.
- ViewModels/: TabItemViewModel.cs (Switch/Close/Duplicate), ChromeViewModel.cs (ObservableCollection, URL binding, nav commands, ITabManager event subscriptions), MainViewModel.cs (chrome property, window title binding).
- **App.xaml rewritten** — merges ChromeResources.xaml (which includes Icons.xaml).
- **App.xaml.cs rewritten** — DI container (IThemeService, ISettingsService, IDownloadService, ITabManager, INavigationService, ChromeViewModel, MainViewModel), theme loading, MainWindow creation.
- **MainWindow.xaml rewritten** — inlines chrome toolbar (tab strip + URL bar + nav buttons) + content area in a single file, uses `{Binding}` (runtime) instead of `{x:Bind}` (compile-time) to avoid WMC9999.
- **MainWindow.xaml.cs rewritten** — adds FocusUrlBar(), OnUrlInputKeyDown(), keyboard shortcuts (Ctrl+T/W/Tab/N/L/R, Alt+Left/Right, F5, zoom, print, view source), TabManager init with env creation.
- Obsolete wwwroot files deleted: index.html, styles.css, app.js, icons.js, ipc.js.
- Old Infrastructure/ThemeService.cs deleted (duplicate class).
- Views/ChromeView.xaml/.cs, Views/ContentHost.xaml/.cs deleted (inlined into MainWindow).
- Build succeeds with 0 errors.

### In Progress
- Fix 13 MVVMTK0045 warnings: convert `[ObservableProperty]` fields to partial properties for WinUI AOT compat.

### Done (WMC9999 Resolution)
- **Root cause**: XamlCompiler.exe (net472) missing `en-US` satellite assembly with `ErrorMessages.resources` in WinAppSDK 2.1.0's WinUI package. External (UseXamlCompilerExecutable=true) hit "Could not find resources"; in-proc (false) hit NullReferenceException when LocalAssembly not set during MarkupCompilePass2.
- **Fix**: Inlined ChromeView + ContentHost UserControls into MainWindow.xaml, using `{Binding}` (runtime) instead of `{x:Bind}` (compile-time bindings). This avoids MarkupCompilePass2 entirely since no project-specific types are referenced in XAML.

## Key Decisions
- **TabManager two-phase init** — DI registers before Grid/WebView2 environment exist; InitializeAsync(Grid, CoreWebView2Environment) called from MainWindow.Loaded.
- **Download handling moved to TabManager** — each WebView2 tab subscribes to DownloadStarting in CreateTabAsync, not in MainWindow.
- **Keyboard shortcuts use CoreWindow.KeyDown** rather than Win32 hooks or KeyDown on root Grid — works even when WebView2 tab has focus.
- **Inline MainWindow.xaml** avoids WMC9999 XAML compiler bug. Uses `{Binding}` for data binding (no compile-time type resolution needed).

## Next Steps
1. Fix MVVMTK0045 warnings by converting `[ObservableProperty]` fields to partial properties.
2. Runtime test: run the app, verify chrome rendering (tabs, toolbar, URL bar work natively), tab create/switch/close, keyboard shortcuts, internal page IPC (settings/downloads), theme.json application.
3. Register `tb://flags` route in UrlResolver/TabManager to point to a flags page.

## Relevant Files
- `TB.csproj`: csproj with DI, MVVM, ImplicitUsings (no XAML workarounds needed)
- `App.xaml`: merges ChromeResources.xaml
- `App.xaml.cs`: DI setup, theme loading
- `MainWindow.xaml`: inlined chrome layout (tab strip, toolbar, content grid) with `{Binding}`
- `MainWindow.xaml.cs`: keyboard shortcuts, FocusUrlBar, TabManager init
- `Services/TabManager.cs`: two-phase init, download handler, IPC for internal pages
- `ViewModels/ChromeViewModel.cs`: tab list, URL, nav commands, event subscriptions
- `Styles/ChromeResources.xaml`: fallback brushes/lengths merging Icons.xaml
- `Styles/Icons.xaml`: 19 SVG path resources + icon styles
