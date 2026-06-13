# TB Browser — Technical Rules

Code standards, patterns, architecture constraints, and design governance.

For agent workflow and operating procedures → see AGENTS.md

---

## ARCHITECTURE

### Flow

User Action
↓
View
↓
ViewModel
↓
Service
↓
Persistence

### Single Owner State

Every piece of state has exactly one owner.

No duplication across:

* View
* ViewModel
* Service
* Control

| State          | Owner             |
| -------------- | ----------------- |
| Tab collection | TabManager        |
| Current theme  | ThemeService      |
| Session        | Session subsystem |
| Settings       | SettingsService   |

### Single Source Of Truth

| Configuration     | Source                             |
| ----------------- | ---------------------------------- |
| Routes / URLs     | Helpers/Constants.cs               |
| Theme colors      | wwwroot/theme.json                 |
| XAML brushes      | App.xaml (mutated by ThemeService) |
| Layout metrics    | LayoutConst / InputConst           |
| Keyboard bindings | CommandRegistry                    |
| Feature flags     | Flags Registry                     |

If a value exists in two places, one is wrong.

Remove the duplicate.

---

## OWNERSHIP

Every new class must declare:

Owns:

* A
* B

Does Not Own:

* X
* Y

Rules:

* ViewModels never own visual controls
* Services never own UI state
* MainWindow never owns business logic
* App never owns browser behavior

Forbidden:

```csharp
public WebView2 WebView { get; }
```

WebView ownership belongs to dedicated managers only.

---

## DEPENDENCY INJECTION

Constructor Injection only.

Allowed:

```csharp
public DownloadService(
    ISettingsService settings,
    ILogger logger)
{
}
```

Forbidden:

* ServiceLocator
* GetService<T>() inside feature code
* Static service access
* Global singleton access

Dependencies must be visible through constructors.

---

## FEATURE OWNERSHIP

Every feature must have a clear home.

Example:

Services/
└── Downloads/
├── DownloadService.cs
├── DownloadItem.cs
└── DownloadViewModel.cs

Rules:

1. Check existing feature folders first.
2. Extend existing modules when reasonable.
3. Do not create new root folders without approval.

Avoid scattering a single feature across unrelated folders.

---

## MAINWINDOW & APP

MainWindow.xaml
MainWindow.xaml.cs

App.xaml
App.xaml.cs

are infrastructure entry points only.

Allowed:

* Event wiring
* Dependency injection
* Service resolution
* Startup orchestration
* Window lifecycle coordination
* Drag region registration
* AppWindow configuration

Forbidden:

* Business logic
* Layout calculations
* Persistence logic
* WebView management
* Keyboard handling
* Browser behavior
* Theme calculations

If feature logic exceeds 30 lines:

Extract to a dedicated file.

---


## WEBVIEW2

A WebView2 is:

* Created once
* Attached once
* Destroyed once

Never reparent.

Forbidden:

```csharp
Children.Remove(webView);
Children.Add(webView);
```

Tab switching changes:

* Visibility
* Grid coordinates

Only.

---

## EVENT HANDLER LIFECYCLE

Events may fire during or after Close().

Always guard handlers.

```csharp
webView.CoreWebView2.DocumentTitleChanged += (_, _) =>
{
    if (_disposed || !_webViews.ContainsKey(id))
        return;

    try
    {
        var core = webView.CoreWebView2;

        if (core == null)
            return;

        // safe work
    }
    catch (ObjectDisposedException) { }
    catch (COMException) { }
};
```

Close order:

1. _webViews.Remove(id)
2. _tabs.Remove(...)
3. wv.Close()

Never reverse this sequence.

---

## ASYNC EVENT DEFERRAL

Any async event handler performing work after await must use a deferral.

```csharp
var deferral = args.GetDeferral();

try
{
    await SomeAsyncCall();
}
finally
{
    deferral.Complete();
}
```

Without GetDeferral(), event completion becomes unreliable.

---

## THREAD DISPATCH

Background threads must never mutate UI directly.

Required:

```csharp
var dq = DispatcherQueue.GetForCurrentThread();

if (dq is null)
    return;

dq.TryEnqueue(() =>
{
    // UI work
});
```

UI-thread-only operations:

* Application.Current.Exit()
* ObservableCollection mutations
* BitmapImage.SetSourceAsync()
* XAML property writes

---

## THEME

Flow:

theme.json
↓
ThemeService
↓
Brush mutation

Brushes are created once.

Brushes are never replaced.

Forbidden:

```csharp
Resources[key] =
    new SolidColorBrush(...);
```

Required:

```csharp
existingBrush.Color = color;
```

CSS and XAML are separate concerns.

Never mix them.

---

## NO INLINE STYLING

Forbidden in XAML:

```xml
FontSize="13"
FontWeight="SemiBold"
Background="#1a0a2e"
```

Forbidden in HTML:

```html
<div style="color:red">
```

Forbidden in JavaScript:

```js
element.style.color = "red";
```

All styling must originate from:

* theme.json
* ThemeService
* tb.css
* centralized XAML resources

---

## NO EMBEDDED MARKUP

Forbidden:

```csharp
string html = "...";
string css = "...";
string js = "...";
```

Required:

wwwroot/css/
wwwroot/js/
wwwroot/html/

Exceptions require documentation in MEMORY.md.

---

## NO MAGIC NUMBERS

Repeated values must become constants.

Forbidden:

```csharp
if (width < 32)
```

Required:

```csharp
if (width < LayoutConst.MinTabWidth)
```

If a number appears multiple times:

Centralize it.

---

## API RESEARCH FIRST

Before calling an API:

1. Check official Microsoft documentation.
2. Check official WebView2 documentation.
3. Verify the API exists in the exact NuGet version.
4. Verify limitations.

If uncertain:

State uncertainty and ask.

Never invent framework APIs.

---

## MATH BEFORE UI

For layout bugs:

Show:

Input
↓
Formula
↓
Output

Example:

Input:
Window = 800

Formula:
Available = 800 - 88 - 34 - 14 - 40

Output:
624px

Fix calculations first.

Do not patch symptoms with UI hacks.

---

## ARCHITECTURE DRIFT

Solve the approved problem only.

Example:

Task:
Fix tab compression

Allowed:

* TabStrip.xaml
* TabStrip.xaml.cs
* TabStripLayout.cs

Not Allowed:

* Theme refactor
* Keyboard changes
* MainWindow redesign
* Settings redesign

Adjacent improvements require a separate review.

Do not expand scope automatically.

---

## FUTURE EXPANSION CHECK

Before introducing:

* Service
* Manager
* Registry
* Serializer
* Abstraction

Answer:

1. What future problem does this solve?
2. Why can't an existing component be extended?
3. What maintenance cost does it add?

No abstractions without justification.

---

## WORKAROUNDS

Every workaround must be documented in MEMORY.md.

Required:

Reason:
Why it exists

Limitation:
What caused it

Replacement:
Proper solution

Removal Trigger:
When it can be removed

Undocumented workarounds are forbidden.

Temporary workarounds must never silently become permanent architecture.
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