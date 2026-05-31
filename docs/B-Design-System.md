# TB Design System v1.0

## Purpose

TB is a lightweight, fast, trading-focused browser.

The visual design is inspired by:

1. Helium Browser
2. Arc Browser
3. Modern desktop applications

TB is NOT a Chrome clone.

TB prioritizes:

* Focus
* Density
* Clarity
* Speed
* Long-session comfort
* Trading workflows

---

# Design Principles

## Principle 1: Compact

Every pixel must justify its existence.

Avoid:

* Large headers
* Excessive whitespace
* Oversized controls

Prefer:

* Compact layouts
* Dense information
* Efficient use of space

---

## Principle 2: Calm

The UI must never compete with chart data.

Charts are the primary content.

Browser chrome is secondary.

Avoid:

* Bright colors
* High saturation
* Excessive shadows
* Distracting animations

---

## Principle 3: Consistent

The same design rules apply everywhere.

Never introduce:

* Random spacing values
* Random border radii
* Random heights
* Random colors

---

## Principle 4: Browser First

TB should feel like a premium browser.

Not:

* A WPF application
* A dashboard
* An enterprise tool

---

# Design Reference

## Primary Reference

Helium Browser

Reference Areas:

* Window chrome
* Tab layout
* Density
* Address bar design
* Control sizing

## Secondary Reference

Arc Browser

Reference Areas:

* Polish
* Spacing
* Interaction quality

---

# Theme System

## Color Foundation

Source:

Radix Colors

Palette:

* Mauve
* Violet

Theme Name:

TB Helium Dark

---

# Color Tokens

## Background

Used for:

* Window
* Top bar

Resource:

TBBackgroundBrush

---

## Surface

Used for:

* Tabs
* Toolbar
* Inputs

Resource:

TBSurfaceBrush

---

## Surface Hover

Used for:

* Hover states
* Active states

Resource:

TBSurfaceHoverBrush

---

## Border

Used for:

* Control borders
* Separators

Resource:

TBBorderBrush

---

## Text

Used for:

* Primary text

Resource:

TBTextBrush

---

## Secondary Text

Used for:

* Placeholder text
* Metadata

Resource:

TBTextSecondaryBrush

---

## Accent

Used for:

* Focus states
* Highlights
* Active indicators

Resource:

TBAccentBrush

---

# Geometry System

## Corner Radius

Allowed Values:

* 0
* 6
* 10

No other values are permitted.

---

## Spacing Scale

Allowed Values:

* 4
* 8
* 12
* 16
* 24

No other values are permitted.

---

# Height System

## TopBar

Height:

36

Contains:

* Tabs
* Window controls

---

## Toolbar

Height:

36

Contains:

* Navigation
* Address bar

---

## Tab

Height:

28

---

## Button

Height:

28

---

## Address Bar

Height:

28

---

# Window Layout

Structure:

TopBar
Toolbar
BrowserHost

Only three primary layers are allowed.

---

# Tabs

## Shape

Rounded

Radius:

6

---

## Active Tab

Must be visually distinct.

Methods:

* Lighter surface
* Accent indicator
* Higher contrast

---

## Inactive Tabs

Lower emphasis than active tab.

---

# Address Bar

## Style

Rounded container.

The TextBox itself should be visually invisible.

Users should perceive:

A single rounded search field.

Not:

A standard Windows TextBox.

---

# Buttons

Buttons must:

* Match toolbar height
* Use consistent padding
* Follow theme tokens

Avoid:

* Default WPF appearance
* Platform-inconsistent styling

---

# Future Themes

Planned:

* TB Helium Dark
* TB Arc Dark
* TB Trading Dark
* TB Graphite
* TB Midnight

All themes must use the same semantic tokens.

Controls must never reference raw colors.

---

# Non-Goals

TB will not optimize for:

* Material Design
* Fluent Design imitation
* Chrome imitation
* Firefox imitation

TB follows its own identity built on Helium-inspired principles.

---

# Rule

When a design decision is unclear:

Ask:

"How would Helium solve this?"

If the answer improves clarity, consistency, or density, follow that direction.
