---
name: "重灌清單"
description: "A calm cross-platform desktop desk for restoring software and sensitive licence notes after a reinstall."
colors:
  ink-deep: "#101828"
  ink-panel: "#172337"
  ink-raised: "#233552"
  ink-progress-track: "#0D1522"
  paper-workspace: "#F5F7FA"
  paper-panel: "#FFFFFF"
  paper-line: "#DFE5EC"
  text-strong: "#172337"
  text-muted: "#5A6B80"
  text-sidebar: "#B5C2D5"
  cobalt-action: "#3578E5"
  cobalt-progress: "#71A9FF"
  cobalt-badge: "#E8F0FF"
  cobalt-badge-text: "#2559AA"
  warning-licence: "#A84700"
  danger-delete: "#A32929"
typography:
  display:
    fontSize: "27px"
    fontWeight: 600
  headline:
    fontSize: "25px"
    fontWeight: 600
  title:
    fontSize: "21px"
    fontWeight: 600
  body:
    fontSize: "16px"
    fontWeight: 400
  label:
    fontSize: "12px"
    fontWeight: 400
rounded:
  badge: "9px"
  card: "12px"
  progress-panel: "14px"
spacing:
  compact: "5px"
  control-gap: "8px"
  field-gap: "10px"
  card: "16px"
  panel: "24px"
  header-horizontal: "28px"
components:
  button-cobalt:
    backgroundColor: "{colors.cobalt-action}"
    textColor: "#FFFFFF"
  button-ink:
    backgroundColor: "{colors.ink-panel}"
    textColor: "#FFFFFF"
    padding: "8px 18px"
  record-card:
    backgroundColor: "{colors.paper-panel}"
    rounded: "{rounded.card}"
    padding: "{spacing.card}"
  state-badge:
    backgroundColor: "{colors.cobalt-badge}"
    textColor: "{colors.cobalt-badge-text}"
    rounded: "{rounded.badge}"
    padding: "4px 9px"
---

# Design System: 重灌清單

## Overview

**Creative North Star: "The Calm Install Desk"**

This is a desktop operating surface for a consequential but routine job: rebuilding a machine without losing the important details. The shell is deep ink and deliberately quiet; the central work surface is a pale paper field where the checklist stays readable and actionable. Cobalt is reserved for affirmative progress and save/scan actions, while licence-related and destructive content receive their own direct warning colors.

The interface is a fixed three-part desk at its designed desktop size: progress and tools at left, the active checklist in the center, and a focused editor at right. It uses the Avalonia Fluent theme for standard controls; the custom system is expressed through surfaces, hierarchy, spacing, and a small number of explicit colors.

**Key Characteristics:**

- Dark operational rail, bright working canvas, and a white detail pane.
- Dense enough for ongoing checklist work, with generous panel padding to keep it calm.
- One cobalt action language; warning and delete colors are semantic exceptions.
- Rounded cards and badges with restrained, diffuse list-card elevation.

## Colors

The palette separates persistent navigation from focused work: cool ink contains the supporting tools, neutral paper hosts records, and cobalt tells the user what can move forward.

### Primary

- **Action Cobalt:** `cobalt-action` drives scan and save actions; `cobalt-progress` is the lighter readout for the progress bar against the dark rail.

### Secondary

- **Status Blue:** `cobalt-badge` and `cobalt-badge-text` form the quiet installed-state badge inside checklist cards.

### Neutral

- **Deep Ink:** `ink-deep`, `ink-panel`, and `ink-raised` make the surrounding rail and its progress module feel stable rather than decorative.
- **Working Paper:** `paper-workspace` is the list canvas; `paper-panel` is reserved for record cards and the detail work area.
- **Hairline Divider:** `paper-line` separates the central header and right editor without introducing heavy chrome.
- **Reading Ink:** `text-strong`, `text-muted`, and `text-sidebar` establish a clear hierarchy across light and dark surfaces.

### Named Rules

**The One Cobalt Rule.** Cobalt signals progress and affirmative action; do not use it as broad decoration or for every interactive surface.

**The Semantic Exception Rule.** `warning-licence` identifies paid/licensed software, and `danger-delete` identifies deletion only. Neither is a general accent.

## Typography

The window relies on the Avalonia Fluent theme's platform-appropriate UI face. Weight and size—not a custom display font—create the hierarchy.

### Hierarchy

- **Display** (600, 27px): the rail product name.
- **Headline** (600, 25px): the central checklist heading.
- **Title** (600, 21px): the detail-pane heading.
- **Body** (400, 16px): record names and ordinary control text.
- **Label** (400, 12px): progress hints, record metadata, state badges, and status text.

**The Weight-Only Hierarchy Rule.** Keep headings semibold and supporting text regular; avoid decorative type treatments in this utility surface.

## Layout

The main window is designed at 1180 × 760, with a minimum of 940 × 620. Its desktop grid is 250px / flexible / 310px: a fixed left rail, an elastic central checklist, and a fixed right editor. The center has a header followed by a scrollable list; the detail pane has a fixed heading followed by a scrollable field stack.

Use 24px padding for dark and detail panels, 28px horizontal padding in the central header, 18px list inset, and 16px inside record cards. The rhythm is intentionally compact within a group (5–10px) and spacious between structures (18–28px). Preserve this three-region reading order on desktop; it is the form that makes current progress, the next task, and sensitive details visible together.

## Elevation & Depth

Depth is mostly structural and tonal: ink rail, pale canvas, and white detail pane are separated with flat fills and thin `paper-line` dividers. Only checklist cards lift, using a subdued diffuse shadow (`0 3px 14px 0 #160D1B2A`) so the active records scan as discrete working units without turning the interface into a card gallery.

**The Flat Desk Rule.** Panels remain flat by default; reserve shadows for repeatable record cards.

## Shapes

The geometry is gently rounded rather than pill-shaped: progress modules use `progress-panel` corners, record cards use `card` corners, and compact state labels use `badge` corners. Borders are structural hairlines, not outlined containers. Standard Fluent controls retain their platform-native shape and behavior.

## Components

### Buttons

- **Character:** direct, legible actions inside a Fluent-control baseline.
- **Primary:** scan and save use `button-cobalt` with white text.
- **Dark utility action:** adding an item uses `button-ink` in the bright header, with 18px horizontal and 8px vertical padding.
- **Destructive action:** delete stays visually plain except for `danger-delete` text, so the semantic signal is precise.

### Progress Module

- **Style:** an `ink-raised` panel with `progress-panel` corners and 16px internal padding.
- **Readout:** large white completion figure above an 8px `ink-progress-track` bar with `cobalt-progress` fill.
- **Supporting copy:** sidebar labels remain in the softer `text-sidebar` range.

### Record Cards / Containers

- **Corner Style:** gently curved (`card`).
- **Background:** `paper-panel` on the `paper-workspace` list canvas.
- **Elevation:** the single diffuse card shadow described above.
- **Internal Padding:** `card`; cards are separated by a 9px vertical gap.

### State Badge

- **Style:** `cobalt-badge` container, `cobalt-badge-text` label, compact 12px type.
- **Shape:** softly rounded (`badge`) and padded `4px 9px`.

### Inputs / Fields

- **Style:** use Avalonia Fluent `TextBox`, `ComboBox`, and `CheckBox` controls without custom chrome.
- **Arrangement:** the editor uses a single vertical field stack with semibold `text-strong` labels and 7–10px inter-field rhythm; multi-line licence and notes fields retain generous minimum heights.

## Do's and Don'ts

### Do:

- **Do** preserve the dark-left / pale-center / white-right desk structure for desktop checklist work.
- **Do** use cobalt for positive, state-changing actions and progress only.
- **Do** keep record metadata smaller and quieter than record names.
- **Do** preserve the explicit semantic distinction between licences and deletion.
- **Do** lean on Fluent controls for cross-platform inputs, selection, and keyboard behavior.

### Don't:

- **Don't** turn the central list into a dense generic dashboard or a grid of equally elevated panels.
- **Don't** introduce additional accent colors for decoration.
- **Don't** use warnings or red as a substitute for normal action styling.
- **Don't** replace native Fluent control affordances with custom web-like input chrome.
