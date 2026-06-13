using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TB.Core.Browser; // 🛡️ MILESTONE 32: Added for WebViewRegistry
using TB.Services.Downloads;
using TB.Helpers;
using TB.Infrastructure;
using TB.Input;
using TB.Models;
using TB.Services.Interfaces;
using Windows.Foundation;

namespace TB.Services;

public partial class TabManager : ITabManager
{
    // 🛡️ MILESTONE 32: Registry replaces Grid and Environment ownership
    internal WebViewRegistry? _registry;

    // Helper to retrieve WebView2 instances safely without holding direct references in dictionaries
    internal WebView2? GetWebView(int id) => _registry?.GetWebView(id);

    private readonly DispatcherQueue _dispatcherQueue;
    private readonly string _wwwrootPath;
    private readonly string _sessionPath;

    private readonly IThemeService _themeService;
    private readonly ISettingsService _settingsService;
    private readonly IDownloadService _downloads;
    private readonly KeyboardShortcutHandler _keyboardHandler;
    private readonly IFlagService _flagService;

    // ❌ REMOVED: _contentGrid, _env, and _webViews dictionary. 
    // The WebViewRegistry now strictly owns all WebView2 instances and XAML attachments.

    internal readonly Dictionary<int, double> _zoomLevels = [];
    internal readonly List<TabItem> _tabs = [];
    internal readonly HashSet<int> _internalPageTabs = [];
    internal readonly Dictionary<int, int> _downloadOwners = [];
    internal readonly Dictionary<int, TypedEventHandler<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs>> _ipcHandlers = [];
    internal readonly Dictionary<int, IDisposable> _acceleratorSubscriptions = [];
    internal readonly Dictionary<int, WebView2EventHandlers> _wvHandlers = [];
    internal readonly Dictionary<int, long> _lastFaviconTimestamp = [];
    internal readonly List<string> _lastClosedUrls = [];

    internal int _nextId = 1;
    internal int _activeId = -1;
    internal bool _disposed;
    internal bool _initialized;
    internal bool _isRestoring;
    internal bool _isFindBarOpen;

    internal readonly SemaphoreSlim _sessionSaveLock = new(1, 1);
    internal CancellationTokenSource? _saveCts;
    internal readonly object _saveLock = new();
    internal readonly Lazy<string> _themeSyncScript;
    internal readonly Lazy<string> _crashPage;
    internal readonly Lazy<string> _bridgeScript;

    public int ActiveTabId => _activeId;
    public int TabCount => _tabs.Count;
    public bool IsInitialized => _initialized;
    public bool IsFindBarOpen => _isFindBarOpen;
    public IReadOnlyList<TabItem> Tabs => _tabs.AsReadOnly();

    public event EventHandler<TabEventArgs>? TabCreated;
    public event EventHandler<TabEventArgs>? TabSwitched;
    public event EventHandler<TabEventArgs>? TabClosed;
    public event EventHandler<TabEventArgs>? TabTitleChanged;
    public event EventHandler<UrlEventArgs>? UrlChanged;
#pragma warning disable CS0067 // NavigationStarted raised by WebView2 event handlers in TabManager.Events.cs
    public event EventHandler? NavigationStarted;
#pragma warning restore CS0067
    public event EventHandler? NavigationCompleted;
    public event EventHandler<EventArgs>? TabsCleared;
    public event EventHandler<FaviconEventArgs>? FaviconUpdated;
    public event EventHandler? BeforeShutdown;
    public event EventHandler<NavStateEventArgs>? NavStateChanged;
    public event EventHandler<TabMovedEventArgs>? TabMoved;
    public event EventHandler<FindResultEventArgs>? FindResultReceived;
    public event EventHandler? FindBarOpenRequested;
    public event EventHandler? FindBarCloseRequested;

    public TabManager(string basePath, IThemeService themeService, ISettingsService settingsService, IDownloadService downloads, KeyboardShortcutHandler keyboardHandler, IFlagService flagService)
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread()!;
        _wwwrootPath = Path.Combine(basePath, "wwwroot");
        _sessionPath = Paths.SessionFile;

        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _downloads = downloads ?? throw new ArgumentNullException(nameof(downloads));
        _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
        _flagService = flagService ?? throw new ArgumentNullException(nameof(flagService));

        _themeSyncScript = new Lazy<string>(() =>
        {
            var path = Path.Combine(_wwwrootPath, "js", "theme-sync.js");
            return File.Exists(path) ? File.ReadAllText(path) : "";
        });

        _crashPage = new Lazy<string>(() =>
        {
            var path = Path.Combine(_wwwrootPath, "crash.html");
            return File.Exists(path) ? File.ReadAllText(path) : "";
        });

        _bridgeScript = new Lazy<string>(() =>
        {
            var path = Path.Combine(_wwwrootPath, "js", "bridge.js");
            return File.Exists(path) ? File.ReadAllText(path) : "";
        });

        _downloads.OnProgress += OnDownloadProgress;
        _downloads.OnCompleted += OnDownloadCompleted;
        _downloads.OnFailed += OnDownloadFailed;
        _themeService.ThemeChanged += OnThemeChanged;
    }

    // 🛡️ MILESTONE 32: Signature changed to accept WebViewRegistry instead of Grid + Environment
    public async Task InitializeAsync(WebViewRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _initialized = true;
        Logger.Info("TabManager initialized with WebViewRegistry");

        if (!await TryLoadSessionAsync())
        {
            await CreateTabAsync(Defaults.HomeUrl);
        }
    }

    internal void EnsureInitialized()
    {
        if (!_initialized) throw new InvalidOperationException("TabManager not initialized.");
    }
}