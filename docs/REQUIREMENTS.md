# RTBrowser Requirements

# Vision

A lightweight, fast, stable, trading-focused browser.

# Priorities

1. Stability
2. Reliability
3. Performance
4. UI
5. Features

# Required

## Session

* Auto restore tabs
* Preserve tab order
* Preserve active tab
* Debounced session saving
* Save on shutdown

## Recovery

* Crash recovery
* Offline recovery
* Automatic retry
* WebView auto reload

## Performance

* Fast startup
* Active tab loads first
* Background tab restoration
* Low RAM
* Low CPU

## Navigation

* Full URL visible
* URL or search behavior
* Google default search
* Alt+D support

## Bookmarks

* Simple bookmarks only

## Logging

* Daily log files
* 30 day retention

## Windowing

* Single browser window
* New windows open as tabs

# Not Planned

* Profiles
* Workspaces
* Bookmark folders
* Extension platform
* Chrome clone features

# RTBrowser Diagnostics Policy

## Purpose

During development and testing, RTBrowser must provide enough diagnostics to identify failures without guessing.

## Rule

If a user can test a feature, that feature must be logged before additional feature development continues.

## Development Mode

Diagnostics Level: Maximum

Retention: 30 Days

## Browser Interaction Logging

Log:

* Button clicks
* Menu clicks
* Toolbar clicks
* Tab clicks
* Keyboard shortcuts
* Address bar navigation requests
* Command execution
* Service execution
* State transitions

Do Not Log:

* Passwords
* Cookies
* Authentication tokens
* Form contents
* Credit card information
* User-entered website content

## Required Logging Categories

Application:

* Startup
* Shutdown
* Crash detection
* Crash recovery
* Session save
* Session restore

Tabs:

* Tab added
* Tab closed
* Tab activated
* Tab count changes

Navigation:

* Requested URL
* Normalized URL
* Navigation started
* Navigation completed
* Navigation failed

WebView:

* Attached
* Initialized
* Process failed
* Navigation events

Network:

* DNS failures
* Connection failures
* TLS/SSL failures
* Timeouts
* HTTP errors
* Offline detection

Performance:

* Startup duration
* Navigation duration
* Session restore duration

Exceptions:

* DispatcherUnhandledException
* AppDomain unhandled exceptions
* TaskScheduler unobserved exceptions

State Changes:

* Active tab changes
* Address changes
* Settings changes
* Theme changes
* Recovery actions

## Future Settings Support

Users must be able to configure:

* Log categories
* Log verbosity
* Log retention period
* Log file location

## Development Rule

No new browser features should be added while existing user-facing bugs cannot be diagnosed from logs.

























