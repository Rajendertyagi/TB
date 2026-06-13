using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.System;
using TB.Helpers;
using TB.Infrastructure;
using TB.Services.Interfaces;

namespace TB.Input;

public sealed class KeyboardShortcutHandler
{
    private readonly CommandRegistry _registry;
    private readonly ISettingsService _settingsService;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYUP = 0x0105;

    private LowLevelKeyboardProc? _hookProc;
    private IntPtr _hookId = IntPtr.Zero;

    private readonly Dictionary<VirtualKey, long> _lastDestructiveHit = new(capacity: 2);
    private static readonly long DebounceTicks = Stopwatch.Frequency * 150 / 1000;

    private static readonly HashSet<VirtualKey> _destructiveKeys = new() { VirtualKey.W, VirtualKey.F4 };

    private readonly object _debounceLock = new();

    private readonly HashSet<VirtualKey> _pressedKeys = new();
    private readonly object _pressedKeysLock = new();
    private bool _isCurrentEventRepeat;

    public CommandRegistry Registry => _registry;

    public KeyboardShortcutHandler(CommandRegistry registry, ISettingsService settingsService)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool IsKeyDown(VirtualKey key) => (GetAsyncKeyState((int)key) & 0x8000) != 0;

    public static bool IsCtrlPressed() => IsKeyDown(VirtualKey.Control);
    public static bool IsShiftPressed() => IsKeyDown(VirtualKey.Shift);
    public static bool IsAltPressed() => IsKeyDown(VirtualKey.Menu);

    private bool ShouldDebounce(VirtualKey key)
    {
        lock (_debounceLock)
        {
            long now = Stopwatch.GetTimestamp();
            if (_lastDestructiveHit.TryGetValue(key, out long last) && (now - last) < DebounceTicks)
                return true;
            _lastDestructiveHit[key] = now;
            return false;
        }
    }

    public void StartHook()
    {
        if (_hookId != IntPtr.Zero) return;

        try
        {
            _hookProc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(curModule.ModuleName), 0);
            }

            if (_hookId == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Logger.Error($"Failed to register low-level keyboard hook. Win32 Error: {errorCode}");
            }
            else
            {
                Logger.Info("Low-level keyboard hook registered");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Exception occurred in StartHook", ex);
            _hookId = IntPtr.Zero;
            _hookProc = null;
        }
    }

    public void StopHook()
    {
        if (_hookId == IntPtr.Zero) return;

        try
        {
            bool success = UnhookWindowsHookEx(_hookId);
            if (!success)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Logger.Error($"Failed to unregister low-level keyboard hook. Win32 Error: {errorCode}");
            }
            else
            {
                Logger.Info("Low-level keyboard hook unregistered");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Exception occurred in StopHook", ex);
        }
        finally
        {
            _hookId = IntPtr.Zero;
            _hookProc = null;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(hwnd, out uint activePid);
                if (activePid == Environment.ProcessId)
                {
                    var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    var key = (VirtualKey)kb.vkCode;

                    if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                    {
                        bool isRepeat = false;
                        lock (_pressedKeysLock)
                        {
                            isRepeat = !_pressedKeys.Add(key);
                        }

                        _isCurrentEventRepeat = isRepeat;
                        try
                        {
                            if (HandleKey(key))
                            {
                                return new IntPtr(1); // Consume key
                            }
                        }
                        finally
                        {
                            _isCurrentEventRepeat = false;
                        }
                    }
                    else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                    {
                        lock (_pressedKeysLock)
                        {
                            _pressedKeys.Remove(key);
                        }
                    }
                }
                else
                {
                    // Active window belongs to another process. Clear keys to prevent stuck state.
                    lock (_pressedKeysLock)
                    {
                        if (_pressedKeys.Count > 0)
                        {
                            _pressedKeys.Clear();
                        }
                    }
                }
            }
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static bool IsRepeatAllowed(BrowserCommand cmd)
    {
        return cmd is BrowserCommand.ZoomIn or BrowserCommand.ZoomOut;
    }

    public bool HandleKey(VirtualKey key)
    {
        try
        {
            // Fast-path filter for isolated modifier keys
            if (key is VirtualKey.Control or VirtualKey.Shift or VirtualKey.Menu)
                return false;

            bool ctrl = IsCtrlPressed();
            bool shift = IsShiftPressed();
            bool alt = IsAltPressed();

            // Handle destructive keys debounce (W and F4 when Ctrl or Alt is pressed)
            if (_destructiveKeys.Contains(key) && (ctrl || alt))
            {
                if (ShouldDebounce(key))
                    return true;
            }

            // Intercept Escape key when Find Bar is open to close it
            if (key == VirtualKey.Escape && !ctrl && !shift && !alt)
            {
                var tabManager = _registry.TabManager;
                if (tabManager != null && tabManager.IsInitialized && tabManager.IsFindBarOpen)
                {
                    tabManager.CloseFindBarAsync().FireAndForget();
                    return true; // Consume Escape key
                }
            }

            var command = _registry.Match(key, ctrl, alt, shift);
            if (command.HasValue)
            {
                if (IsActiveTabDomainExcluded() && !IsCoreBrowserCommand(command.Value))
                {
                    return false; // Skip intercepting, pass through to webpage
                }

                if (_isCurrentEventRepeat && !IsRepeatAllowed(command.Value))
                {
                    return true; // Consume repeat event without executing command
                }

                return _registry.Execute(command.Value);
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.Error($"{nameof(KeyboardShortcutHandler)}.{nameof(HandleKey)}", ex);
            return false;
        }
    }

    private bool IsActiveTabDomainExcluded()
    {
        try
        {
            var tabManager = _registry.TabManager;
            if (tabManager == null || !tabManager.IsInitialized) return false;

            var tabs = tabManager.Tabs;
            if (tabs == null) return false;

            int activeId = tabManager.ActiveTabId;
            string? url = null;
            for (int i = 0; i < tabs.Count; i++)
            {
                var t = tabs[i];
                if (t != null && t.Id == activeId)
                {
                    url = t.Url;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(url)) return false;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var host = uri.Host;
                var exclusionsStr = _settingsService.Get<string>("excluded-domains", "") ?? "";
                var exclusions = exclusionsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var domain in exclusions)
                {
                    if (host.Equals(domain, StringComparison.OrdinalIgnoreCase)) return true;
                    if (host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to evaluate shortcut domain exclusion: {ex.Message}");
        }
        return false;
    }

    private static bool IsCoreBrowserCommand(BrowserCommand cmd)
    {
        return cmd switch
        {
            BrowserCommand.NewTab => true,
            BrowserCommand.CloseTab => true,
            BrowserCommand.NextTab => true,
            BrowserCommand.PrevTab => true,
            BrowserCommand.GoToTab1 => true,
            BrowserCommand.GoToTab2 => true,
            BrowserCommand.GoToTab3 => true,
            BrowserCommand.GoToTab4 => true,
            BrowserCommand.GoToTab5 => true,
            BrowserCommand.GoToTab6 => true,
            BrowserCommand.GoToTab7 => true,
            BrowserCommand.GoToTab8 => true,
            BrowserCommand.GoToTab9 => true,
            BrowserCommand.MoveTabLeft => true,
            BrowserCommand.MoveTabRight => true,
            BrowserCommand.CloseWindow => true,
            BrowserCommand.NewWindow => true,
            BrowserCommand.ToggleCommandPalette => true,
            _ => false
        };
    }
}