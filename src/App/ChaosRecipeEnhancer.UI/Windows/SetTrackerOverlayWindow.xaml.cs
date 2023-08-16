using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ChaosRecipeEnhancer.UI.Models.Enums;
using ChaosRecipeEnhancer.UI.Properties;
using ChaosRecipeEnhancer.UI.Services;
using ChaosRecipeEnhancer.UI.UserControls.SetTrackerOverlayDisplays;
using ChaosRecipeEnhancer.UI.Utilities;

namespace ChaosRecipeEnhancer.UI.Windows;

public partial class SetTrackerOverlayWindow
{
    private readonly SetTrackerOverlayViewModel _model;
    private readonly StashTabOverlayWindow _stashTabOverlay;
    private LogWatcherManager _logWatcherManager;

    public SetTrackerOverlayWindow()
    {
        DataContext = _model = new SetTrackerOverlayViewModel();

        // initialize stash tab overlay window and log watcher alongside this window
        _stashTabOverlay = new StashTabOverlayWindow();

        InitializeComponent();
        UpdateOverlayType();

        Settings.Default.PropertyChanged += OnSettingsChanged;
    }

    public bool IsOpen { get; private set; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Win32.MakeToolWindow(this);
    }

    private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.SetTrackerOverlayDisplayMode))
            UpdateOverlayType();
        else if (e.PropertyName == nameof(Settings.FullSetThreshold)
                 || e.PropertyName == nameof(Settings.StashTabIndices)
                 || e.PropertyName == nameof(Settings.StashTabPrefix))
            _model.UpdateNotificationMessage();
    }

    private void UpdateOverlayType()
    {
        if (Settings.Default.SetTrackerOverlayDisplayMode == (int)SetTrackerOverlayMode.Standard)
            MainOverlayContentControl.Content = new StandardDisplay(this);
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && !Settings.Default.SetTrackerOverlayOverlayLockPositionEnabled && Mouse.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    public new virtual void Hide()
    {
        IsOpen = false;
        _stashTabOverlay.Hide();
        if (_logWatcherManager is not null && _logWatcherManager.WorkerThread != null && _logWatcherManager.WorkerThread.IsAlive) _logWatcherManager.StopWatchingLogFile();
        base.Hide();
    }

    public new virtual void Show()
    {
        try
        {
            IsOpen = true;
            if (Settings.Default.AutoFetchOnRezoneEnabled) _logWatcherManager = new LogWatcherManager(this);
            base.Show();
        }
        catch (ArgumentNullException)
        {
            IsOpen = false;
            _stashTabOverlay.Hide();
            base.Hide();

            ErrorWindow.Spawn(
                "No PoE Client.txt file path set. Please set the file path or disable 'Fetch on New Map' setting.",
                "Error: Auto-Fetching"
            );
        }

    }

    public void RunFetching()
    {
        if (!IsOpen) return;

        _model.FetchDataAsync(); // Fire and forget async
    }

    public void RunReloadFilter()
    {
        if (!IsOpen) return;

        _model.RunReloadFilter();
    }

    public void RunStashTabOverlay()
    {
        if (_stashTabOverlay.IsOpen)
        {
            _stashTabOverlay.Hide();
        }
        else
        {
            _stashTabOverlay.Show();
        }
    }
}