using Microsoft.UI.Input;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using TB.Infrastructure;
using Windows.System;
using Windows.UI.Core;

namespace TB.Input;

public class KeyboardShortcutHandler
{
    private readonly CommandRegistry _registry;
    private static readonly HashSet<VirtualKey> _destructiveKeys = [VirtualKey.W, VirtualKey.F4];

    // FIX 1: Thread-safe dictionary to prevent crashes if events fire off the UI thread
    private readonly ConcurrentDictionary<VirtualKey, long> _lastDestructiveHit = new();
    private static readonly long DebounceTicks = Stopwatch.Frequency * 150 / 1000;

    public event Action? FocusUrlBarRequested;
    public event Action? NavigateRequested;
    public event Action? CloseWindowRequested;
    public event Action? ToggleFullScreenRequested;

    // REMOVED: [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int virtualKey);

    public KeyboardShortcutHandler(CommandRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public bool HandleKey(VirtualKey key)
    {
        try
        {
            // FIX 2: Use WinUI 3 native API instead of user32.dll P/Invoke.
            // This is safer, managed, and reads the state exactly as it was when the key was pressed.
            bool ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            bool alt = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down); // Alt is Menu
            bool shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);

            // Debounce destructive keys (Ctrl+W, Alt+F4) to prevent accidental rapid closing
            if (_destructiveKeys.Contains(key) && (ctrl || alt))
            {
                var now = Stopwatch.GetTimestamp();
                if (_lastDestructiveHit.TryGetValue(key, out var last) && (now - last) < DebounceTicks)
                    return true; // Swallow the key, it was pressed too recently

                _lastDestructiveHit[key] = now;
            }

            var binding = _registry.Match(key, ctrl, alt, shift);
            if (binding.HasValue)
            {
                if (binding.Value.Execute())
                    return true;

                // Execute returned false — signals non-handler routing (UI Events)
                switch (key)
                {
                    case VirtualKey.L or VirtualKey.F6 when ctrl && !alt && !shift:
                    case VirtualKey.D when !ctrl && alt && !shift:
                        FocusUrlBarRequested?.Invoke();
                        return true;

                    case VirtualKey.N when ctrl && !alt && !shift:
                    case VirtualKey.N when ctrl && !alt && shift:
                        NavigateRequested?.Invoke();
                        return true;

                    case VirtualKey.F11:
                        ToggleFullScreenRequested?.Invoke();
                        return true;

                    case VirtualKey.W when ctrl && !alt && shift:
                        CloseWindowRequested?.Invoke();
                        return true;
                }

                return true;
            }

            Logger.Debug($"Keyboard shortcut unmatched: {key} (Ctrl:{ctrl} Alt:{alt} Shift:{shift})");
        }
        catch (Exception ex)
        {
            Logger.Error($"KeyboardShortcutHandler exception: {ex.Message}");
        }
        return false;
    }
}
