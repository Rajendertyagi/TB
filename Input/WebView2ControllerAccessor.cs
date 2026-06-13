using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using TB.Infrastructure;
using Windows.Foundation;
using Windows.System;

namespace TB.Input;

/// <summary>
/// Provides access to the internal ICoreWebView2Controller COM interface.
/// This is required to subscribe to AcceleratorKeyPressed events, which allows
/// the browser to intercept global shortcuts (Ctrl+T, Ctrl+W) before the 
/// web page content can consume them.
/// 
/// WARNING: This uses undocumented COM interop. If WebView2 SDK updates 
/// change the internal IID or interface layout, this may need updating.
/// </summary>
public sealed class WebView2ControllerAccessor
{
    // IID for ICoreWebView2Controller: {4D00C0D1-9434-4EB6-8078-8697A560334F}
    // This GUID is stable across WebView2 SDK versions as it refers to the base interface.
    private static readonly Guid ControllerIid = new("4D00C0D1-9434-4EB6-8078-8697A560334F");

    // 🛡️ Win32 API to read keyboard state directly from the OS, bypassing XAML's blind spot
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    /// <summary>
    /// Checks if a specific key is currently pressed at the OS level.
    /// Use this instead of XAML's CoreWindow.GetKeyState when the WebView2 has focus.
    /// </summary>
    public static bool IsKeyDown(VirtualKey key) => (GetAsyncKeyState((int)key) & 0x8000) != 0;

    public static bool IsCtrlPressed() => IsKeyDown(VirtualKey.Control);
    public static bool IsShiftPressed() => IsKeyDown(VirtualKey.Shift);
    public static bool IsAltPressed() => IsKeyDown(VirtualKey.Menu);

    private WebView2ControllerAccessor() { }

    /// <summary>
    /// Attempts to hook into the WebView2 Controller's AcceleratorKeyPressed event.
    /// </summary>
    /// <returns>An IDisposable that unsubscribes the event when disposed, or null if failed.</returns>
    public static IDisposable? TrySubscribeAcceleratorKeyPressed(WebView2 webView, Func<VirtualKey, bool> handler)
    {
        if (webView?.CoreWebView2 == null)
            return null;

        IntPtr pUnk = IntPtr.Zero;
        IntPtr pController = IntPtr.Zero;

        try
        {
            // 1. Get the IUnknown pointer for the CoreWebView2 object
            pUnk = Marshal.GetIUnknownForObject(webView.CoreWebView2);
            if (pUnk == IntPtr.Zero)
                return null;

            // 2. Query for the ICoreWebView2Controller interface
            int hr = Marshal.QueryInterface(pUnk, in ControllerIid, out pController);

            if (hr != 0 || pController == IntPtr.Zero)
            {
                // This can happen if the SDK version changes or the object doesn't support QI
                Logger.Warning($"Failed to query ICoreWebView2Controller (HR: 0x{hr:X8}). Global shortcuts may not work when web content is focused.");
                return null;
            }

            // 3. Cast the COM pointer back to the managed Controller type
            var controller = Marshal.GetObjectForIUnknown(pController) as CoreWebView2Controller;
            if (controller == null)
                return null;

            // 4. Subscribe to the event
            TypedEventHandler<CoreWebView2Controller, CoreWebView2AcceleratorKeyPressedEventArgs> acceleratorHandler = (_, e) =>
            {
                // Pass the key to the handler. If it returns true, mark as handled to prevent the web page from seeing it.
                if (handler((VirtualKey)e.VirtualKey))
                {
                    e.Handled = true;
                }
            };

            controller.AcceleratorKeyPressed += acceleratorHandler;

            // 5. Return a disposable to clean up the subscription later
            return new Disposable(() =>
            {
                try
                {
                    controller.AcceleratorKeyPressed -= acceleratorHandler;
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if the controller was already destroyed
                }
            });
        }
        catch (COMException ex)
        {
            Logger.Warning($"WebView2 COM interop failure: 0x{ex.HResult:X8}");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Warning($"WebView2ControllerAccessor failed: {ex.Message}");
            return null;
        }
        finally
        {
            // CRITICAL: Release COM references to prevent memory leaks
            if (pController != IntPtr.Zero) Marshal.Release(pController);
            if (pUnk != IntPtr.Zero) Marshal.Release(pUnk);
        }
    }

    /// <summary>
    /// Helper class to wrap an Action in an IDisposable interface.
    /// </summary>
    private sealed class Disposable : IDisposable
    {
        private Action? _cleanup;

        public Disposable(Action cleanup) => _cleanup = cleanup;

        public void Dispose()
        {
            var cleanup = _cleanup;
            _cleanup = null; // Prevent double disposal
            cleanup?.Invoke();
        }
    }
}

