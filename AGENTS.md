# TB Browser — Agent Behaviour Rules
> Mandatory for Claude, Gemini, OpenCode, Qwen, Copilot, Cursor, and any future AI agent.
> For technical rules, patterns, and code standards → see RULES.md

---

## SESSION START

Read in this order before doing anything:

1. `PLAN.md` — current milestone and next steps
2. `MEMORY.md` — past decisions, bugs, patterns
3. `RULES.md` — technical standards
4. `AGENTS.md` — this file

If any file is missing → stop and ask. Do not guess.

---

## BEFORE WRITING ANY CODE

Provide and wait for approval on:

1. Technical flow (User → View → ViewModel → Service → Persistence)
2. Files modified / created / deleted
3. Public API of every new class
4. Ownership boundaries (owns / does not own)
5. How this design handles future expansion

**No code until approved.**

---

## DURING WORK

- Surgical edits only — change the minimum needed
- Never rewrite a file that does not need changing
- Never repeat context already in PLAN.md or MEMORY.md
- If blocked → state the blocker clearly, do not guess a solution
- If unsure about an API → check docs first, state uncertainty, then ask

---

## AFTER EVERY TASK

**Build** (required for any C#, XAML, or DI change):
```
dotnet build --no-restore
```
Required: 0 errors, 0 warnings.

**Update docs:**
- `PLAN.md` — mark done, remove completed steps, add new blockers
- `MEMORY.md` — log decisions, bugs found, patterns established

**Change report** (no task is complete without this):
```
Files Modified:
Files Created:
Files Deleted:
Build: ✅ 0 errors / 0 warnings
Visual Changes:
Non-Visual Changes:
Technical Debt Added:
```

---

## WHEN TO ASK VS WHEN TO CODE

Ask when:
- Requirements are ambiguous
- Two valid approaches exist
- A frozen system (Keyboard, Theme, Settings, Session, Downloads, Find) needs touching
- The change affects more than 3 files
- An API cannot be verified in the current package version

Code when:
- The task is clear
- Approach is approved
- Build is green

---

## FROZEN SYSTEMS

Do not modify without a bug fix, security fix, or approved architecture review:

- Keyboard → `CommandRegistry`
- Theme → `ThemeService` + `theme.json`
- Settings → `SettingsService`
- Session → `SessionService`
- Downloads → `DownloadService`
- Find → `FindService`

---

## GIT SAFETY

Before major work:
```
git add .
git commit -m "checkpoint: description"
```

Before architecture changes:
```
git checkout -b feature/name
```

Never refactor directly on `main`.

---

## COMMUNICATION STYLE

- Short status updates, not essays
- State what changed and why — not how the code works line by line
- If something is not done, say so — do not hide it in the change report
- One question at a time if clarification is needed

## FILE SIZE GOVERNANCE

| Lines | Action                  |
| ----- | ----------------------- |
| 300   | Review structure        |
| 500   | Review required         |
| 700+  | Mandatory decomposition |

When decomposing:

* Create dedicated feature files
* Create dedicated feature folders
* Separate responsibilities

Never create dumping grounds:

* Manager.cs
* Helper.cs
* Utils.cs

without a clearly defined ownership boundary.

---

ARCHITECTURE DRIFT

Solve the approved problem only.

Example:

Task:
Fix tab compression

Allowed:
- TabStrip.xaml
- TabStrip.xaml.cs
- TabStripLayout.cs

Not Allowed:
- Theme refactor
- Keyboard changes
- Session redesign

Adjacent improvements require a separate review.
FUTURE EXPANSION CHECK

Before creating:

- Service
- Manager
- Registry
- Serializer
- Abstraction

Answer:

1. What future problem does this solve?
2. Why can an existing component not be extended?
3. What maintenance cost does this add?

No abstraction without justification.

FEATURE MODULE STRUCTURE

New features should follow:

Feature/
├── Models/
├── Services/
├── ViewModels/
├── Views/
└── Contracts/

Example:

Workspaces/
├── WorkspaceDefinition.cs
├── WorkspaceManager.cs
├── IWorkspaceManager.cs
└── WorkspaceSerializer.cs

Avoid:

WorkspaceHelper.cs
WorkspaceUtils.cs
WorkspaceManagerEverything.cs