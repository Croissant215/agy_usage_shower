# Antigravity Taskbar Usage Shower Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a native C# .NET 9 WPF desktop application (`AgyUsageShower`) that docks a sleek dark-mode overlay onto the bottom-right corner of the Windows Taskbar to display real-time Google Antigravity (`agy`) model quotas, token usage, and reset countdown timers.

**Architecture:** MVVM WPF application with Win32 P/Invoke interop (`Shell_TrayWnd` bounds calculation and window style modification `WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE`) and a background data provider polling local `agy` quota state.

**Tech Stack:** C# .NET 9 WPF, System.Text.Json, P/Invoke (User32.dll), INotifyPropertyChanged.

## Global Constraints
- Target Framework: `net9.0-windows`
- WPF Output Type: `WinExe`
- Position: Bottom-Right of Windows Taskbar (Left of System Tray / Clock)
- Memory footprint: Target < 25MB RAM
- No external heavy dependencies (use native WPF, System.Text.Json, P/Invoke)

---

### Task 1: WPF Project Scaffolding & Core Models

**Files:**
- Create: `AgyUsageShower.csproj`
- Create: `Models/UsageData.cs`
- Create: `App.xaml`
- Create: `App.xaml.cs`

**Interfaces:**
- Consumes: None
- Produces: `UsageData` record/class containing `ModelName`, `QuotaPercent`, `ResetCountdown`, `IsOffline`, `TokenCount`

- [ ] **Step 1: Scaffold .NET WPF project file `AgyUsageShower.csproj`**
- [ ] **Step 2: Create `Models/UsageData.cs` data model**
- [ ] **Step 3: Create `App.xaml` and `App.xaml.cs`**
- [ ] **Step 4: Build project using `dotnet build`**
- [ ] **Step 5: Commit**

---

### Task 2: Win32 Taskbar Interop & Docking Service

**Files:**
- Create: `Services/Win32TaskbarService.cs`

**Interfaces:**
- Consumes: User32.dll Win32 functions (`FindWindow`, `GetWindowRect`, `SetWindowPos`, `GetWindowLong`, `SetWindowLong`)
- Produces: `Win32TaskbarService.GetTaskbarRightDockPosition(double width, double height)` returning `(double x, double y)`

- [ ] **Step 1: Write `Services/Win32TaskbarService.cs` with P/Invoke definitions and docking logic**
- [ ] **Step 2: Add extended window style helpers (`WS_EX_TOPMOST`, `WS_EX_TOOLWINDOW`, `WS_EX_NOACTIVATE`)**
- [ ] **Step 3: Build project using `dotnet build`**
- [ ] **Step 4: Commit**

---

### Task 3: Antigravity Usage Data Service

**Files:**
- Create: `Services/AntigravityUsageService.cs`

**Interfaces:**
- Consumes: Local `~/.gemini/antigravity-cli/` cache or `agy` CLI output
- Produces: `AntigravityUsageService.FetchUsageAsync()` returning `UsageData`

- [ ] **Step 1: Write `Services/AntigravityUsageService.cs` with local Antigravity CLI status parsing**
- [ ] **Step 2: Build project using `dotnet build`**
- [ ] **Step 3: Commit**

---

### Task 4: MVVM ViewModel & UI Views (Compact Overlay + Popup Card)

**Files:**
- Create: `ViewModels/MainViewModel.cs`
- Create: `MainWindow.xaml`
- Create: `MainWindow.xaml.cs`
- Create: `Views/DetailCardWindow.xaml`
- Create: `Views/DetailCardWindow.xaml.cs`

**Interfaces:**
- Consumes: `Services/Win32TaskbarService.cs`, `Services/AntigravityUsageService.cs`
- Produces: Complete running WPF App displaying taskbar right docked widget and interactive click popup.

- [ ] **Step 1: Create `ViewModels/MainViewModel.cs` with reactive binding properties and commands**
- [ ] **Step 2: Create `MainWindow.xaml` & `MainWindow.xaml.cs` with frameless dark glass UI**
- [ ] **Step 3: Create `Views/DetailCardWindow.xaml` & `Views/DetailCardWindow.xaml.cs` for popup detail card**
- [ ] **Step 4: Build project using `dotnet build`**
- [ ] **Step 5: Commit**
