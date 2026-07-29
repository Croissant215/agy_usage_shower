# Antigravity Taskbar Usage Shower (AGY Usage Shower) Design Spec

## 1. Overview
The **AGY Usage Shower** is a lightweight, native Windows application built with C# .NET 8 WPF. It anchors a sleek, dark-themed frameless overlay widget onto the bottom-right corner of the Windows Taskbar (next to the System Tray / Clock area). It periodically fetches Google Antigravity (`agy`) model quotas (Gemini 3.5 Pro, Flash, etc.), token usage, and reset countdown timers, displaying them in a compact progress bar and micro card.

---

## 2. Key Features & UI Specifications
- **Positioning**: Docked to Taskbar Bottom-Right (next to System Tray / Clock).
- **Compact Widget**:
  - Active Model Name (e.g., `Gemini 3.5 Pro`)
  - Quota Bar (Color-coded: Green >= 80%, Yellow <= 40%, Red <= 10%)
  - Reset Timer Badge (e.g., `⏳ 2h 15m`)
- **Detail Card Popup**:
  - Expands upon clicking the widget.
  - Multi-model quota breakdown (Pro vs Flash vs Credits).
  - Manual Refresh (`🔄`) and Settings button.
- **Resource Footprint**: Target < 15MB RAM, zero CPU idle load.

---

## 3. Architecture & Win32 Integration
1. **Taskbar Anchor Engine (`TaskbarDockingService`)**:
   - Locates `Shell_TrayWnd` handle and calculates right-hand offset using `GetWindowRect` & `TrayNotifyWnd`.
   - Applies Window Styles:
     - `WS_EX_TOPMOST` (0x00000008) - Stay above taskbar
     - `WS_EX_TOOLWINDOW` (0x00000080) - Hide from Alt+Tab
     - `WS_EX_NOACTIVATE` (0x08000000) - Prevent stealing window focus on click
2. **Quota Engine (`AntigravityQuotaService`)**:
   - Background timer (60s interval).
   - Polls `antigravity-usage` or local `agy` quota endpoints.
3. **UI Layer (`MainWindow` / `DetailPopup`)**:
   - WPF MVVM with Reactive Properties.
   - Dark Glassmorphism aesthetic.

---

## 4. Error Handling & System Events
- **Display / Resolution Changes**: Listens to `WM_DISPLAYCHANGE` and taskbar movement events to recalculate docking bounds.
- **API Failure**: Displays status indicator `⚠️ Offline` with graceful retry backoff.
