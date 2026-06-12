using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
    private Grid? _contentGrid;
    private CoreWebView2Environment? _env;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly string _wwwrootPath;
    private readonly string _sessionPath;

    private readonly IThemeService _themeService;
    private readonly ISettingsService _settingsService;
    private readonly IDownloadService _downloads;
    private readonly KeyboardShortcutHandler _keyboardHandler;

    internal readonly Dictionary<int, WebView2> _webViews = [];
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

    internal readonly SemaphoreSlim _sessionSaveLock = new(1, 1);
    internal CancellationTokenSource? _saveCts;
    internal readonly object _saveLock = new();
    internal readonly Lazy<string> _findBarScript;
    internal readonly Lazy<string> _crashPage;
    internal readonly Lazy<string> _bridgeScript;

    public int ActiveTabId => _activeId;
    public int TabCount => _tabs.Count;
    public bool IsInitialized => _initialized;
    public IReadOnlyList<TabItem> Tabs => _tabs.AsReadOnly();

    public event EventHandler<TabEventArgs>? TabCreated;
    public event EventHandler<TabEventArgs>? TabSwitched;
    public event EventHandler<TabEventArgs>? TabClosed;
    public event EventHandler<UrlEventArgs>? UrlChanged;
    public event EventHandler? NavigationStarted;
    public event EventHandler? NavigationCompleted;
    public event EventHandler<EventArgs>? TabsCleared;
    public event EventHandler<FaviconEventArgs>? FaviconUpdated;
    public event EventHandler? BeforeShutdown;
    public event EventHandler<NavStateEventArgs>? NavStateChanged;
    public event EventHandler<TabMovedEventArgs>? TabMoved;

    public TabManager(string basePath, IThemeService themeService, ISettingsService settingsService, IDownloadService downloads, KeyboardShortcutHandler keyboardHandler)
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread()!;
        _wwwrootPath = Path.Combine(basePath, "wwwroot");
        var appDataFolder = Path.Combine(basePath, "AppData");
        Directory.CreateDirectory(appDataFolder);
        _sessionPath = Path.Combine(appDataFolder, "session.json");

        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _downloads = downloads ?? throw new ArgumentNullException(nameof(downloads));
        _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));

        _findBarScript = new Lazy<string>(() =>
        {
            var path = Path.Combine(_wwwrootPath, "js", "find-bar.js");
            return File.Exists(path) ? File.ReadAllText(path) : "console.error('find-bar.js missing');";
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

    public async Task InitializeAsync(Grid contentGrid, CoreWebView2Environment env)
    {
        _contentGrid = contentGrid ?? throw new ArgumentNullException(nameof(contentGrid));
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _initialized = true;
        Logger.Info("TabManager initialized");

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

