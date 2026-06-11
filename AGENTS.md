# OpenCode Automation Rule
You are working on a strict free-tier token limit.

## Strict Context Rules:
1. BEFORE starting any work, read `PLAN.md` and `MEMORY.md` to restore full context. Query the internal SQLite/USearch DB last if docs are stale.
2. AFTER every command or edit, immediately update `PLAN.md` (toggle checkmarks, shift completed items out of "Next Steps").
3. Keep descriptions ultra-short to save tokens.
4. Always verify build (`dotnet build --no-restore`) after C# or XAML changes. CSS-only changes skip build.
5. The current theme is **violet-dark** (accent #ee82ee). Do NOT reference Catppuccin in any new code.
