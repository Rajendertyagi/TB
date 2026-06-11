using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using TB.Infrastructure;
using Windows.Foundation;

namespace TB.Input;

public sealed class WebView2ControllerAccessor
{
    // GUID {4D00C0D1-9434-4EB6-8078-8697A560334F} = CoreWebView2Controller IID
    // WARNING: This GUID is tied to the WebView2 SDK version (currently 1.0.xxx).
    // A future SDK update could change the IID, breaking the raw QueryInterface below.
    // If accelerator keys stop working after a WebView2 SDK update, this is the first place to check.
    private static readonly Guid ControllerGuid = new("4D00C0D1-9434-4EB6-8078-8697A560334F");

    private WebView2ControllerAccessor() { }

    public static IDisposable? TrySubscribeAcceleratorKeyPressed(WebView2 webView, Func<Windows.System.VirtualKey, bool> handler)
    {
        if (webView?.CoreWebView2 == null)
            return null;

        try
        {
            var pWebView2 = Marshal.GetIUnknownForObject(webView.CoreWebView2);
            if (pWebView2 == IntPtr.Zero)
                return null;

            try
            {
                if (Marshal.QueryInterface(pWebView2, in ControllerGuid, out var pController) != 0 || pController == IntPtr.Zero)
                    return null;

                try
                {
                    var controller = Marshal.GetObjectForIUnknown(pController) as CoreWebView2Controller;
                    if (controller == null)
                        return null;

                    TypedEventHandler<CoreWebView2Controller, CoreWebView2AcceleratorKeyPressedEventArgs> acceleratorHandler = (_, e) =>
                    {
                        if (handler((Windows.System.VirtualKey)e.VirtualKey))
                            e.Handled = true;
                    };

                    controller.AcceleratorKeyPressed += acceleratorHandler;

                    return new Disposable(() =>
                    {
                        try { controller.AcceleratorKeyPressed -= acceleratorHandler; } catch (ObjectDisposedException) { }
                    });
                }
                finally { Marshal.Release(pController); }
            }
            finally { Marshal.Release(pWebView2); }
        }
        catch (COMException ex)
        {
            Logger.Warning($"WebView2 COM failure: 0x{ex.HResult:X8}");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Warning($"WebView2ControllerAccessor failed: {ex}");
            return null;
        }
    }

    private sealed class Disposable : IDisposable
    {
        private readonly Action _cleanup;
        public Disposable(Action cleanup) => _cleanup = cleanup;
        public void Dispose() => _cleanup();
    }
}
