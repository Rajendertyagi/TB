# PERFORMANCE_RULES.md

# RTBrowser Performance Rules

## Startup Goal

Target:

* under 1 second startup

---

# Initialization Rules

Use:

* lazy initialization
* deferred WebView creation
* async startup pipeline

Avoid:

* blocking startup tasks
* eager loading systems
* unnecessary initialization

---

# Rendering Rules

Use:

* native WinUI rendering
* lightweight composition
* GPU-balanced rendering

Avoid:

* HTML-rendered browser chrome
* Electron-style UI rendering
* heavy acrylic systems
* excessive transparency

---

# Memory Rules

RTBrowser prioritizes aggressive memory efficiency.

Use:

* sleeping tabs
* lazy restoration
* WebView pooling
* memory pressure unloading

Tabs should not permanently own WebViews.

---

# Runtime Rules

Use:

* lifecycle-managed WebViews
* pooled runtime resources
* deferred loading

Avoid:

* permanent active WebViews
* uncontrolled background execution
* excessive hidden tabs

---

# Animation Rules

RTBrowser uses Instant UI philosophy.

Allowed:

* tiny hover state transitions

Avoid:

* complex animation systems
* blur transitions
* spring physics
* elastic motion
* floating effects

---

# Telemetry Rules

Telemetry remains:

* local-only
* lightweight
* developer-focused

Track:

* startup timing
* RAM usage
* WebView count
* sleeping tabs
* restore timing

---

# Product Rule

Performance is a core product feature.

Every feature must be evaluated for:

* startup impact
* RAM impact
* rendering cost
* responsiveness impact
