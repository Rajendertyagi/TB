using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TB.Infrastructure;
using Windows.System;

namespace TB.Input;

public class KeyboardShortcutHandler
{
    private readonly CommandRegistry _registry;
    private static readonly HashSet<VirtualKey> _destructiveKeys = new() { VirtualKey.W, VirtualKey.F4 };
    private readonly Dictionary<VirtualKey, long> _lastDestructiveHit = new();
    private static readonly long DebounceTicks = Stopwatch.Frequency * 150 / 1000;

    public event Action? FocusUrlBarRequested;
    public event Action? NavigateRequested;
    public event Action? CloseWindowRequested;
    public event Action? ToggleFullScreenRequested;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    public KeyboardShortcutHandler(CommandRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public bool HandleKey(VirtualKey key)
    {
        try
        {
            bool ctrl = (GetAsyncKeyState(0x11) & 0x8000) != 0;
            bool alt = (GetAsyncKeyState(0x12) & 0x8000) != 0;
            bool shift = (GetAsyncKeyState(0x10) & 0x8000) != 0;

            if (_destructiveKeys.Contains(key) && (ctrl || alt))
            {
                var now = Stopwatch.GetTimestamp();
                if (_lastDestructiveHit.TryGetValue(key, out var last) && (now - last) < DebounceTicks)
                    return true;
                _lastDestructiveHit[key] = now;
            }

            var binding = _registry.Match(key, ctrl, alt, shift);
            if (binding.HasValue)
            {
                if (binding.Value.Execute())
                    return true;

                // Execute returned false — signals non-handler routing (events)
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

            Logger.Debug($"Keyboard shortcut unmatched: {key}");
        }
        catch (Exception ex)
        {
            Logger.Error($"KeyboardShortcutHandler exception: {ex.Message}");
        }
        return false;
    }
}
