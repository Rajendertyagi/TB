# 1. COMPONENT ISOLATION & CODEBASE ARCHITECTURE
- Monolithic structures, quick-hacks, and shortcut code patterns are strictly banned.
- All code files must enforce absolute Separation of Concerns using this folder schema:
  * Views/ & Controls/ -> Holds raw UI elements and XAML templates. No native User32 hooks allowed inside XAML code-behinds.
  * ViewModels/ -> Isolated view feature files (e.g., TabStripViewModel.cs, ChromeViewModel.cs, MainViewModel.cs) matching exact view responsibilities.
  * Services/ -> Holds background logic (TabManager.cs, ThemeService.cs, NavigationService.cs). Every service must implement an explicit Interface.
  * Features/ -> Feature-grouped modules (Tabs/, Layout/).
  * Infrastructure/Native/ -> Clean isolation for external native window hooks ('WindowManagementService.cs').
- All ViewModels and Services must receive dependencies explicitly via Constructor Injection through our Microsoft.Extensions.DependencyInjection container setup in App.xaml.cs.
- All XAML bindings use standard runtime `{Binding}` syntax for compatibility and stability.

# 2. CENTRALIZED STYLING & MANDATORY ASSET CENTERING (HELIUM EXACT FLUID PHYSICS)
- FORBID ALL INLINE COLORS: It is strictly forbidden to hardcode hex colors or explicit brush values inside individual XAML layout elements. All visual components must resolve backgrounds, text colors, and borders using dynamic resource keys bound to our centralized ThemeService.
- NO HARDCODED COLORS IN C# OR CSS: Hex color values are banned from all C# code-behind and CSS files. The sole source for all themed values is `theme.json` → XAML `{ThemeResource}` (chrome) or injected CSS `var(--key)` variables (internal pages). No `var(--x, #fallback)` fallback hexes in CSS. No `?? Colors.Transparent` or `?? "#hex"` fallback chains in C# converters/helpers. If `theme.json` is missing a key, crash explicitly.
- IMMUTABLE RESOURCE KEY MAPPING: Ban manual string lookups for styling values. All dictionary references must leverage centralized, static constant configurations to maintain theme integrity.
- MANDATORY ASSET CENTERING: Forbid rigid 3-column Grid layouts with runtime width-conditional checks. The tab visual template must utilize a centralized canvas layering model where the Favicon container is anchored to the absolute geometric center (`HorizontalAlignment="Center"`, `VerticalAlignment="Center"`). Title text blocks and close controls must occupy a higher z-index stretching layer that clips out fluidly at layout boundaries under compression, guaranteeing that the favicon remains fixed and dead-centered when the tab shrinks to its 32px limit footprint.

# 3. DEPENDENCY MINIMIZATION & FINALIZED INPUT STRATEGY
- Eliminate heavy CsWinRT framework input projections. Stripping out CsWinRT translation dependencies is mandatory to ensure absolute ahead-of-time (AOT) compilation speed.
- FINALIZED KEYBOARD DISPATCHER: To successfully bypass CsWinRT projection limitations, all input routing must utilize our proven, dual-hook standard: a localized 'WebView2.KeyDown' loop for web content focus, combined with a native Win32 'WH_KEYBOARD_LL' global hook loop inside MainWindow.
- All 46 keyboard shortcut tracking logic triggers must map directly to raw hardware enums using native `Windows.System.VirtualKey` and `Windows.System.VirtualKeyModifiers`.
- Intercept and process input event streams at the core application window level before they can be swallowed or blocked by the WebView2 rendering component. Mark args.Handled = true instantly upon a shortcut match to kill OS propagation and completely suppress the native hardware error beep system sound.
- Error Sound Deflection: (1) WndProc returns `IntPtr.Zero` on match, (2) LLKBHook returns 1 (non-zero) to block message dispatch, (3) KeyDown sets `e.Handled = true`. Triple-layer protection.
- Live `GetAsyncKeyState` P/Invoke polling for `VK_CONTROL`, `VK_MENU`, `VK_SHIFT` — no cached modifier state, no desync risk.
- ENFORCE INPUT THROTTLING: Implement a high-speed hardware debounce mechanism using `Stopwatch.Frequency * 150 / 1000`. Impose a strict 150ms cooldown limit on destructive hotkeys (Ctrl+W, Ctrl+Shift+W, Ctrl+F4, Alt+F4). Repeated input inside the cooldown window must be ignored AND return `true` (consumed) to maintain UI thread stability.
- GoF Command Pattern: `KeyboardShortcutHandler.cs` acts as invoker, mapping key combos to decoupled service interfaces via lazy `IServiceProvider` resolution (circular-dependency-safe). No ViewModel or visual control coupling.
- Enforce native App-Level Window hooks via User32 Interop to clamp minimum window size configurations to 400x300px dynamically.

# 4. REAL-WORLD HELIUM HOVER & VISUAL INTERACTION SCHEMA (NO ZEN MODE)
- ZERO ZEN MODE OVERHEAD: The browser retains a clean, permanently visible, lightweight top chrome shell. No slide-down gestures, top-boundary hidden triggers, or auto-hide animations.
- TAB STRIP WIDTH ISOLATION (ANTI-JIGGLE): Forbid all immediate layout resizing operations while the cursor resides inside the active `TabStripGrid` / `ChromeContainer`. Sizing updates must be deferred until a full pointer exit event registers to eliminate mouse jiggling and misclicks.
- MULTI-TIERED OPACITY TRANSITION FRAMEWORK:
  * Active Tab: Completely opaque (Alpha = 1.0) blending flush into the primary toolbar container background.
  * Inactive Tab Unhovered: Baseline opacity (Alpha = 0.40) to let Mica/Acrylic desktop translucency show through behind the tab frame text.
  * Inactive Tab Hovered: Smooth interpolation targeting Alpha = 0.85 with a micro-tinted outline border. Dimensions must remain strictly unaltered — no pixel changes on hover.
- CLOSE BUTTON HOVER TRAP: Hide the close icon completely when tab widths compress under 36px. Re-render the close glyph overlay only when an individual tab container triggers a localized PointerEntered execution state.

# 5. PERFORMANCE, RESILIENCE, & THREAD SAFETY
- Maintain absolute global null-safety across the entire application workspace (`#nullable enable`).
- VIRTUALIZED LAYOUT METRICS: Enforce strict UI Virtualization behaviors across our dynamic Tab container to safeguard system thread memory when tracking a high volume of active browser tab collections.
- CACHED PATH COMPOSITION: Mandate direct hardware-accelerated DirectComposition caching for custom vector path geometry elements to avoid micro-stutters during heavy interface resizing adjustments.
- NON-BLOCKING UI ACTIONS: Enforce strict usage of `.ConfigureAwait(false)` on all background data layers or interop routines to cleanly separate processing operations from our main rendering dispatcher thread.
- Wrap all file asynchronous operations and Interop communications in structured try-catch-finally resilience blocks with production logging triggers.
- Implement an explicit Global Exception Handler under 'Diagnostics/Logging/' to pipe unhandled UI thread exceptions asynchronously to a local `crash.log` file.
- Forbid all raw global/static configuration variables. All user setting attributes, default homepage queries, downloads tracking logs, and dynamic Feature Flags must pass through a modular, local JSON state store under 'Common/Configuration/'.
- Any ViewModel data property binding must utilize strict CommunityToolkit.Mvvm source generators ([ObservableProperty]) to ensure absolute AOT compilation compatibility.
