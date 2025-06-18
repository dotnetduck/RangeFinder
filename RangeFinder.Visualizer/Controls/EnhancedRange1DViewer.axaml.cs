using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RangeFinder.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RangeFinder.Visualizer.Controls;

public partial class EnhancedRange1DViewer : UserControl
{
    public static readonly StyledProperty<ObservableCollection<NumericRange<double, string>>> StringRangesProperty =
        AvaloniaProperty.Register<EnhancedRange1DViewer, ObservableCollection<NumericRange<double, string>>>(
            nameof(StringRanges), new ObservableCollection<NumericRange<double, string>>());

    public static readonly StyledProperty<double> ViewportStartProperty =
        AvaloniaProperty.Register<EnhancedRange1DViewer, double>(nameof(ViewportStart), 0.0);

    public static readonly StyledProperty<double> ViewportEndProperty =
        AvaloniaProperty.Register<EnhancedRange1DViewer, double>(nameof(ViewportEnd), 1000.0);

    public static readonly StyledProperty<double> DataMinProperty =
        AvaloniaProperty.Register<EnhancedRange1DViewer, double>(nameof(DataMin), double.NaN);

    public static readonly StyledProperty<double> DataMaxProperty =
        AvaloniaProperty.Register<EnhancedRange1DViewer, double>(nameof(DataMax), double.NaN);

    public static readonly StyledProperty<bool> ShowControlsProperty =
        AvaloniaProperty.Register<EnhancedRange1DViewer, bool>(nameof(ShowControls), true);

    // Events for external handling
    public event EventHandler<double>? PanRequested;
    public event EventHandler<(double delta, bool isZoomModifier, double mouseX)>? ScrollRequested;
    public event EventHandler? ResetViewportRequested;

    public ObservableCollection<NumericRange<double, string>> StringRanges
    {
        get => GetValue(StringRangesProperty);
        set => SetValue(StringRangesProperty, value);
    }

    public double ViewportStart
    {
        get => GetValue(ViewportStartProperty);
        set => SetValue(ViewportStartProperty, value);
    }

    public double ViewportEnd
    {
        get => GetValue(ViewportEndProperty);
        set => SetValue(ViewportEndProperty, value);
    }

    public double DataMin
    {
        get => GetValue(DataMinProperty);
        set => SetValue(DataMinProperty, value);
    }

    public double DataMax
    {
        get => GetValue(DataMaxProperty);
        set => SetValue(DataMaxProperty, value);
    }

    public bool ShowControls
    {
        get => GetValue(ShowControlsProperty);
        set => SetValue(ShowControlsProperty, value);
    }

    private SimpleRange1DCanvas? _canvas;

    public EnhancedRange1DViewer()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _canvas = this.FindControl<SimpleRange1DCanvas>("VisualizationCanvas");
        var zoomInButton = this.FindControl<Button>("ZoomInButton");
        var zoomOutButton = this.FindControl<Button>("ZoomOutButton");
        var resetButton = this.FindControl<Button>("ResetButton");

        if (_canvas != null)
        {
            _canvas.PanRequested += OnCanvasPanRequested;
            _canvas.ScrollRequested += OnCanvasScrollRequested;
        }

        if (zoomInButton != null)
            zoomInButton.Click += OnZoomInClicked;

        if (zoomOutButton != null)
            zoomOutButton.Click += OnZoomOutClicked;

        if (resetButton != null)
            resetButton.Click += OnResetClicked;

        UpdateCanvasProperties();
        SetSmartInitialViewport();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StringRangesProperty || e.Property == DataMinProperty || e.Property == DataMaxProperty)
        {
            if (StringRanges?.Count > 0 && !double.IsNaN(DataMin) && !double.IsNaN(DataMax))
            {
                SetSmartInitialViewport();
            }
        }

        if (e.Property == StringRangesProperty || e.Property == ViewportStartProperty || 
            e.Property == ViewportEndProperty || e.Property == DataMinProperty || e.Property == DataMaxProperty)
        {
            UpdateCanvasProperties();
        }
    }

    private void SetSmartInitialViewport()
    {
        if (StringRanges?.Count == 0 || double.IsNaN(DataMin) || double.IsNaN(DataMax))
            return;

        var dataSpan = DataMax - DataMin;
        var rangeCount = StringRanges.Count;
        
        // Calculate optimal initial viewport - show roughly 20-30 ranges
        var optimalRangeCount = Math.Min(rangeCount, Math.Max(20, rangeCount / 4));
        var unitsPerRange = dataSpan / rangeCount;
        var idealViewportSpan = unitsPerRange * optimalRangeCount;
        
        // Ensure minimum viewport size
        var minViewportSpan = dataSpan * 0.1; // At least 10% of data
        var viewportSpan = Math.Max(idealViewportSpan, minViewportSpan);
        
        // Center on data or start from beginning if viewport covers most data
        double newStart, newEnd;
        if (viewportSpan >= dataSpan * 0.8)
        {
            // Show all data with some padding
            var padding = dataSpan * 0.05;
            newStart = DataMin - padding;
            newEnd = DataMax + padding;
        }
        else
        {
            // Center the viewport on the data
            var center = DataMin + dataSpan / 2;
            newStart = center - viewportSpan / 2;
            newEnd = center + viewportSpan / 2;
        }

        ViewportStart = newStart;
        ViewportEnd = newEnd;
    }

    private void UpdateCanvasProperties()
    {
        if (_canvas == null) return;

        _canvas.StringRanges = StringRanges;
        _canvas.ViewportStart = ViewportStart;
        _canvas.ViewportEnd = ViewportEnd;
        _canvas.DataMin = DataMin;
        _canvas.DataMax = DataMax;
    }

    private void OnCanvasPanRequested(object? sender, double delta)
    {
        var constrainedDelta = ConstrainPanToDataBounds(delta);
        if (Math.Abs(constrainedDelta) > 1e-10)
        {
            ViewportStart += constrainedDelta;
            ViewportEnd += constrainedDelta;
            PanRequested?.Invoke(this, constrainedDelta);
        }
    }

    private void OnCanvasScrollRequested(object? sender, (double delta, bool isZoomModifier, double mouseX) args)
    {
        if (args.isZoomModifier)
        {
            // Zoom functionality
            PerformZoom(args.delta, args.mouseX);
        }
        else
        {
            // Scroll functionality
            var scrollSensitivity = (ViewportEnd - ViewportStart) * 0.1;
            var scrollDelta = args.delta * scrollSensitivity;
            OnCanvasPanRequested(sender, scrollDelta);
        }
        ScrollRequested?.Invoke(this, args);
    }

    private void PerformZoom(double zoomDelta, double mouseXRatio)
    {
        var currentSpan = ViewportEnd - ViewportStart;
        var zoomFactor = 1.0 + (zoomDelta * 0.001);
        
        // Limit zoom levels
        zoomFactor = Math.Max(0.1, Math.Min(10.0, zoomFactor));
        
        var newSpan = currentSpan / zoomFactor;
        
        // Limit minimum and maximum zoom
        var dataSpan = double.IsNaN(DataMax) || double.IsNaN(DataMin) ? double.PositiveInfinity : DataMax - DataMin;
        var minSpan = dataSpan * 0.001; // Can zoom in to 0.1% of data
        var maxSpan = dataSpan * 5.0;   // Can zoom out to 5x data span
        
        newSpan = Math.Max(minSpan, Math.Min(maxSpan, newSpan));
        
        // Calculate new viewport bounds centered on mouse position
        var mouseValue = ViewportStart + mouseXRatio * currentSpan;
        var newStart = mouseValue - (newSpan * mouseXRatio);
        var newEnd = newStart + newSpan;

        // Apply bounds constraints
        var constrainedBounds = ConstrainViewportToDataBounds(newStart, newEnd);
        ViewportStart = constrainedBounds.start;
        ViewportEnd = constrainedBounds.end;
    }

    private double ConstrainPanToDataBounds(double delta)
    {
        if (double.IsNaN(DataMin) || double.IsNaN(DataMax))
            return delta;

        var newStart = ViewportStart + delta;
        var newEnd = ViewportEnd + delta;
        var viewportSpan = ViewportEnd - ViewportStart;

        // Prevent panning beyond data bounds
        if (newStart < DataMin)
        {
            delta = DataMin - ViewportStart;
        }
        else if (newEnd > DataMax)
        {
            delta = (DataMax - viewportSpan) - ViewportStart;
        }

        return delta;
    }

    private (double start, double end) ConstrainViewportToDataBounds(double start, double end)
    {
        if (double.IsNaN(DataMin) || double.IsNaN(DataMax))
            return (start, end);

        var span = end - start;
        var dataSpan = DataMax - DataMin;

        // If viewport is larger than data, center it
        if (span >= dataSpan)
        {
            var center = (DataMin + DataMax) / 2;
            start = center - span / 2;
            end = center + span / 2;
        }
        else
        {
            // Keep viewport within data bounds
            if (start < DataMin)
            {
                start = DataMin;
                end = start + span;
            }
            else if (end > DataMax)
            {
                end = DataMax;
                start = end - span;
            }
        }

        return (start, end);
    }

    private void OnResetClicked(object? sender, RoutedEventArgs e)
    {
        SetSmartInitialViewport();
        ResetViewportRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnZoomInClicked(object? sender, RoutedEventArgs e)
    {
        PerformZoom(500, 0.5); // Zoom in at center
    }

    private void OnZoomOutClicked(object? sender, RoutedEventArgs e)
    {
        PerformZoom(-500, 0.5); // Zoom out at center
    }

    // Public methods for programmatic control
    public void ResetViewport() => OnResetClicked(null, default!);
    public void ZoomIn() => OnZoomInClicked(null, default!);
    public void ZoomOut() => OnZoomOutClicked(null, default!);
    public void ZoomToFit()
    {
        if (double.IsNaN(DataMin) || double.IsNaN(DataMax)) return;
        
        var padding = (DataMax - DataMin) * 0.05;
        ViewportStart = DataMin - padding;
        ViewportEnd = DataMax + padding;
    }
}