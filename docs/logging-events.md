# RTBrowser Logging Event Specification

## Purpose

RTBrowser follows a strict observability rule:

> Nothing happens without a log entry.

Every user action, system action, state transition, lifecycle event, success, warning, failure, exception, cancellation, and no-op must produce a log entry.

---

# Event Naming Standard

Format:

```text
Category.Action.Result
```

Examples:

```text
Tab.Create.Started
Tab.Create.Success
Tab.Create.Failed

Navigation.Requested
Navigation.Started
Navigation.Completed
Navigation.Failed

Bookmark.Add.Requested
Bookmark.Add.Completed
```

---

# Common Result States

All categories should use these whenever applicable.

```text
Requested
Started
Completed
Success
Failed
Cancelled
Warning
Created
Initialized
Activated
Deactivated
Disposed
Changed
Loaded
Saved
Restored
```

---

# Application Events

```text
Application.Startup
Application.Shutdown
Application.CrashDetected
Application.UnhandledException
Application.ConfigurationLoaded
Application.ConfigurationFailed
Application.HostCreated
Application.HostDisposed
```

---

# Window Events

```text
Window.Created
Window.Loaded
Window.Activated
Window.Deactivated
Window.StateChanged
Window.Closing
Window.Closed
Window.Exception
```

---

# Lifecycle Events

```text
Lifecycle.Created
Lifecycle.Initialized
Lifecycle.Activated
Lifecycle.Deactivated
Lifecycle.Disposed
Lifecycle.Failed
```

Applied to:

```text
BrowserViewModel
TabsViewModel
TabManager
WebViewManager
WebView2BrowserService
SettingsService
ThemeService
BookmarkService
FeatureFlagService
InternalPageService
```

---

# Command Events

```text
Command.Requested
Command.Started
Command.Completed
Command.Success
Command.Failed
Command.Cancelled
Command.Warning
```

---

# Navigation Events

```text
Navigation.Requested
Navigation.Started
Navigation.Completed
Navigation.Failed
Navigation.Cancelled
Navigation.Redirected
Navigation.Blocked
Navigation.Timeout
```

---

# Address Bar Events

```text
AddressBar.TextChanged
AddressBar.Submitted
AddressBar.Cleared
AddressBar.FocusGained
AddressBar.FocusLost
AddressBar.InvalidAddress
```

---

# Tab Events

```text
Tab.Create.Requested
Tab.Create.Started
Tab.Create.Completed

Tab.Close.Requested
Tab.Close.Completed

Tab.Activated
Tab.Deactivated

Tab.Restored
Tab.Suspended

Tab.TitleChanged
Tab.UrlChanged
```

---

# Bookmark Events

```text
Bookmark.Add.Requested
Bookmark.Add.Completed

Bookmark.Remove.Requested
Bookmark.Remove.Completed

Bookmark.Update.Requested
Bookmark.Update.Completed

Bookmark.Open.Requested
Bookmark.Open.Completed

Bookmark.Loaded
Bookmark.Saved
Bookmark.Failed
```

---

# WebView Events

```text
WebView.Created
WebView.Initialized
WebView.Attached
WebView.Detached
WebView.Disposed

WebView.NavigationStarting
WebView.NavigationCompleted

WebView.SourceChanged
WebView.HistoryChanged

WebView.NewWindowRequested

WebView.PermissionRequested
WebView.PermissionGranted
WebView.PermissionDenied

WebView.DownloadStarted
WebView.DownloadCompleted

WebView.ProcessFailed

WebView.WebMessageReceived

WebView.ContextMenuOpened

WebView.Warning
WebView.Error
```

---

# Browser Control Events

```text
Browser.Back.Requested
Browser.Back.Completed

Browser.Forward.Requested
Browser.Forward.Completed

Browser.Refresh.Requested
Browser.Refresh.Completed

Browser.Stop.Requested
Browser.Stop.Completed
```

---

# Keyboard Events

```text
Keyboard.KeyDown
Keyboard.KeyUp

Keyboard.Shortcut.CtrlT
Keyboard.Shortcut.CtrlW
Keyboard.Shortcut.CtrlTab
Keyboard.Shortcut.CtrlShiftTab
Keyboard.Shortcut.AltD
Keyboard.Shortcut.F5
```

---

# Mouse Events

```text
Mouse.LeftClick
Mouse.RightClick
Mouse.MiddleClick

Mouse.DoubleClick

Mouse.Scroll

Mouse.Enter
Mouse.Leave
```

---

# Feature Flag Events

```text
FeatureFlag.Loaded

FeatureFlag.Enabled
FeatureFlag.Disabled

FeatureFlag.Changed

FeatureFlag.Failed
```

---

# Settings Events

```text
Settings.Loaded
Settings.Saved
Settings.Changed
Settings.Reset
Settings.Failed
```

---

# Theme Events

```text
Theme.Loaded
Theme.Applied
Theme.Changed
Theme.Reset
Theme.Failed
```

---

# Session Events

```text
Session.Save.Requested
Session.Save.Completed
Session.Save.Failed

Session.Restore.Requested
Session.Restore.Completed
Session.Restore.Failed

Session.AutoSave.Started
Session.AutoSave.Completed
```

---

# Internal Page Events

```text
InternalPage.Requested
InternalPage.Loaded
InternalPage.Failed
```

---

# ViewModel Events

```text
ViewModel.Created
ViewModel.Initialized
ViewModel.Disposed

ViewModel.PropertyChanged

ViewModel.CommandExecuted

ViewModel.Failed
```

---

# Exception Events

```text
Exception.Handled
Exception.Unhandled
Exception.Fatal
```

---

# Verification Rule

A feature is not complete until:

```text
Action logged
Success logged
Failure logged
Warning logged
Exception logged
Lifecycle logged
```

---

# RTBrowser Logging Rule

```text
If an event happened,
there must be a log.

If an event failed,
there must be a log.

If an event did not happen when expected,
there must be enough logs to determine why.
```
