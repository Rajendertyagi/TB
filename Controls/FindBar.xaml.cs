using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using TB.Helpers;
using TB.Models;
using TB.Services.Interfaces;
using Windows.System;
using Windows.UI.Core;

namespace TB.Controls;

public sealed partial class FindBar : UserControl
{
    private ITabManager? _tabManager;

    public FindBar()
    {
        this.InitializeComponent();
    }

    public void Initialize(ITabManager tabManager)
    {
        _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        _tabManager.FindBarOpenRequested += OnFindBarOpenRequested;
        _tabManager.FindBarCloseRequested += OnFindBarCloseRequested;
        _tabManager.FindResultReceived += OnFindResultReceived;
        _tabManager.TabSwitched += OnTabSwitched;
    }

    private void RunOnUI(Action action)
    {
        if (DispatcherQueue.HasThreadAccess)
            action();
        else
            DispatcherQueue.TryEnqueue(() => action());
    }

    private void OnFindBarOpenRequested(object? sender, EventArgs e)
    {
        RunOnUI(() =>
        {
            this.Visibility = Visibility.Visible;
            FindInput.Focus(FocusState.Programmatic);
            FindInput.SelectAll();
            if (_tabManager != null && !string.IsNullOrEmpty(FindInput.Text))
            {
                _tabManager.StartFindAsync(FindInput.Text).FireAndForget();
            }
        });
    }

    private void OnFindBarCloseRequested(object? sender, EventArgs e)
    {
        RunOnUI(() =>
        {
            if (_tabManager != null)
            {
                _tabManager.FocusActiveTab();
                _tabManager.StopFindAsync().FireAndForget();
            }
            this.Visibility = Visibility.Collapsed;
        });
    }

    private void OnFindResultReceived(object? sender, FindResultEventArgs e)
    {
        RunOnUI(() =>
        {
            if (e.MatchCount == 0)
            {
                FindCounter.Text = "0 of 0";
            }
            else
            {
                FindCounter.Text = $"{e.ActiveMatchIndex + 1} of {e.MatchCount}";
            }
        });
    }

    private void OnTabSwitched(object? sender, TabEventArgs e)
    {
        RunOnUI(() =>
        {
            if (this.Visibility == Visibility.Visible && _tabManager != null && !string.IsNullOrEmpty(FindInput.Text))
            {
                _tabManager.StartFindAsync(FindInput.Text).FireAndForget();
            }
        });
    }

    private void FindInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_tabManager == null) return;
        var text = FindInput.Text;
        if (string.IsNullOrEmpty(text))
        {
            FindCounter.Text = "0 of 0";
            _tabManager.StopFindAsync().FireAndForget();
        }
        else
        {
            _tabManager.StartFindAsync(text).FireAndForget();
        }
    }

    private void FindInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_tabManager == null) return;

        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            bool shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            if (shift)
                _tabManager.FindPreviousAsync().FireAndForget();
            else
                _tabManager.FindNextAsync().FireAndForget();
        }
        else if (e.Key == VirtualKey.Escape)
        {
            e.Handled = true;
            _tabManager.CloseFindBarAsync().FireAndForget();
        }
    }

    private void FindPrevButton_Click(object sender, RoutedEventArgs e)
    {
        _tabManager?.FindPreviousAsync().FireAndForget();
    }

    private void FindNextButton_Click(object sender, RoutedEventArgs e)
    {
        _tabManager?.FindNextAsync().FireAndForget();
    }

    private void FindCloseButton_Click(object sender, RoutedEventArgs e)
    {
        _tabManager?.CloseFindBarAsync().FireAndForget();
    }
}
