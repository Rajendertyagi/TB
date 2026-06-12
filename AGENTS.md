# OpenCode Automation Rule
You are working on a strict free-tier token limit.

## Strict Context Rules:
1. BEFORE starting any work, read `PLAN.md` and `MEMORY.md` to restore full context. Query the internal SQLite/USearch DB last if docs are stale.
2. AFTER every command or edit, immediately update `PLAN.md` (toggle checkmarks, shift completed items out of "Next Steps").
3. Keep descriptions ultra-short to save tokens.
4. Always verify build (`dotnet build --no-restore`) after C# or XAML changes. CSS-only changes skip build.
5. The current theme is **violet-dark** (accent #ee82ee). Do NOT reference Catppuccin in any new code.
6. **MASTER GRID INVARIANT:** A WebView2 is **created once, attached once, destroyed once**. Layout changes must never require reparenting. No `Children.Remove`/`Children.Add` on WebView2 controls. All hosts live permanently in `BrowserSurfaceGrid`. Tab switching changes Grid coordinates + visibility only. Preview = `CapturePreviewAsync` bitmap, never live WebView in preview UI.
7. **THEME INVARIANTS:**
   - `Application.Current.Resources` owns all 8 runtime brush instances (declared in `App.xaml`).
   - ThemeDictionaries own Color values only (empty currently, for future Dark/Light use).
   - All XAML uses `{ThemeResource KeyName}` — unchanged, no `{StaticResource}` for theme colors.
   - Brushes are created once in `App.xaml`; theme changes mutate `brush.Color` via `ThemeService.SetBrushResource()`.
   - Brushes must **never be replaced at runtime** (no `Resources[key] = new SolidColorBrush(...)`).
   - Layout tokens (`tabHeightLength`, `radiusMdLength`, etc.) are static design constants, never theme-dependent.
