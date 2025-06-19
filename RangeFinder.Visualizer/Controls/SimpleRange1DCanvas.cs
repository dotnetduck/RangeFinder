using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using RangeFinder.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RangeFinder.Visualizer.Controls;

/// <summary>
/// Simplified high-performance canvas for rendering 1D ranges
/// </summary>
public class SimpleRange1DCanvas : Control
{
    public static readonly StyledProperty<ObservableCollection<NumericRange<double, string>>> StringRangesProperty =
        AvaloniaProperty.Register<SimpleRange1DCanvas, ObservableCollection<NumericRange<double, string>>>(
            nameof(StringRanges), new ObservableCollection<NumericRange<double, string>>());

    public static readonly StyledProperty<double> ViewportStartProperty =
        AvaloniaProperty.Register<SimpleRange1DCanvas, double>(nameof(ViewportStart), 0.0);

    public static readonly StyledProperty<double> ViewportEndProperty =
        AvaloniaProperty.Register<SimpleRange1DCanvas, double>(nameof(ViewportEnd), 1000.0);

    public static readonly StyledProperty<double> DataMinProperty =
        AvaloniaProperty.Register<SimpleRange1DCanvas, double>(nameof(DataMin), double.NaN);

    public static readonly StyledProperty<double> DataMaxProperty =
        AvaloniaProperty.Register<SimpleRange1DCanvas, double>(nameof(DataMax), double.NaN);

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

    // Events for interaction
    public event EventHandler<double>? PanRequested;
    public event EventHandler<(double delta, bool isZoomModifier, double mouseX)>? ScrollRequested;

    private bool _isDragging = false;
    private Point _lastPointerPosition;
    private ToolTip? _currentToolTip;
    private List<List<NumericRange<double, string>>> _cachedLayers = new();
    
    static SimpleRange1DCanvas()
    {
        AffectsRender<SimpleRange1DCanvas>(StringRangesProperty, ViewportStartProperty, ViewportEndProperty);
    }

    public SimpleRange1DCanvas()
    {
        // Enable keyboard input for navigation shortcuts
        Focusable = true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var hasStringRanges = StringRanges != null && StringRanges.Count > 0;
        
        if (!hasStringRanges)
        {
            RenderEmptyState(context);
            return;
        }

        RenderBackground(context);
        RenderRanges(context);
        RenderAxis(context);
    }

    private void RenderEmptyState(DrawingContext context)
    {
        var text = "No ranges to display";
        var textGeometry = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            14,
            Brushes.Gray);

        var position = new Point(
            (Bounds.Width - textGeometry.WidthIncludingTrailingWhitespace) / 2,
            (Bounds.Height - textGeometry.Height) / 2);

        context.DrawText(textGeometry, position);
    }

    private void RenderBackground(DrawingContext context)
    {
        context.FillRectangle(Brushes.White, new Rect(0, 0, Bounds.Width, Bounds.Height));
        
        // Draw simple grid lines
        var gridPen = new Pen(Brushes.LightGray, 0.5);
        var gridCount = 10;
        
        for (int i = 1; i < gridCount; i++)
        {
            var x = (Bounds.Width / gridCount) * i;
            context.DrawLine(gridPen, new Point(x, 0), new Point(x, Bounds.Height));
        }
    }

    private void RenderRanges(DrawingContext context)
    {
        if (StringRanges != null && StringRanges.Count > 0)
        {
            var visibleStringRanges = GetVisibleStringRanges();
            _cachedLayers = ArrangeStringRangesInLayers(visibleStringRanges);

            for (int layer = 0; layer < _cachedLayers.Count; layer++)
            {
                var y = 50 + layer * 30; // Start ranges below axis
                
                foreach (var range in _cachedLayers[layer])
                {
                    RenderStringRange(context, range, y);
                }
            }
        }
    }

    private void RenderStringRange(DrawingContext context, NumericRange<double, string> range, double y)
    {
        var startX = ValueToScreenX(range.Start);
        var endX = ValueToScreenX(range.End);
        
        // Clamp to visible area
        startX = Math.Max(0, startX);
        endX = Math.Min(Bounds.Width, endX);
        
        if (endX <= startX) return; // Range not visible
        
        var width = endX - startX;
        var rect = new Rect(startX, y, width, 20);
        
        // Color scheme based on range value hashcode for better distribution
        var brush = GetBrushFromHashCode(range.Value.GetHashCode());
        
        context.FillRectangle(brush, rect);
        context.DrawRectangle(new Pen(Brushes.Black, 1), rect);
        
        // Draw range value if there's space
        if (width > 30)
        {
            var labelText = range.Value;
            if (!string.IsNullOrEmpty(labelText))
            {
                var textGeometry = new FormattedText(
                    labelText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    10,
                    Brushes.White);
                
                // Center the text in the rectangle
                var textWidth = textGeometry.Width;
                var textHeight = textGeometry.Height;
                var labelX = startX + (width - textWidth) / 2;
                var labelY = y + (20 - textHeight) / 2;
                
                var labelPos = new Point(Math.Max(startX + 2, labelX), labelY);
                context.DrawText(textGeometry, labelPos);
            }
        }
    }

    private void RenderAxis(DrawingContext context)
    {
        var axisPen = new Pen(Brushes.Black, 2);
        var axisY = 40;
        
        // Draw main axis line
        context.DrawLine(axisPen, new Point(0, axisY), new Point(Bounds.Width, axisY));
        
        // Draw tick marks and labels above the axis
        var tickCount = 8;
        var step = (ViewportEnd - ViewportStart) / tickCount;
        
        for (int i = 0; i <= tickCount; i++)
        {
            var value = ViewportStart + (step * i);
            var x = ValueToScreenX(value);
            
            if (x >= 0 && x <= Bounds.Width)
            {
                // Tick mark
                context.DrawLine(axisPen, new Point(x, axisY - 5), new Point(x, axisY + 5));
                
                // Label - positioned above the axis
                var labelText = value.ToString("F1");
                var textGeometry = new FormattedText(
                    labelText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    10,
                    Brushes.Black);
                
                var labelPos = new Point(x - textGeometry.WidthIncludingTrailingWhitespace / 2, axisY - textGeometry.Height - 8);
                context.DrawText(textGeometry, labelPos);
            }
        }
    }

    private double ValueToScreenX(double value)
    {
        var viewportSpan = ViewportEnd - ViewportStart;
        if (viewportSpan == 0) return 0;
        
        return (value - ViewportStart) / viewportSpan * Bounds.Width;
    }

    private List<NumericRange<double, string>> GetVisibleStringRanges()
    {
        return StringRanges?.Where(r => r.Overlaps(ViewportStart, ViewportEnd)).ToList() ?? new List<NumericRange<double, string>>();
    }

    private List<List<NumericRange<double, string>>> ArrangeStringRangesInLayers(List<NumericRange<double, string>> ranges)
    {
        var layers = new List<List<NumericRange<double, string>>>();
        var sortedRanges = ranges.OrderBy(r => r.Start).ToList();
        
        foreach (var range in sortedRanges)
        {
            bool placed = false;
            
            // Try to place in existing layer
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                var canPlace = !layer.Any(r => r.Overlaps(range));
                
                if (canPlace)
                {
                    layer.Add(range);
                    placed = true;
                    break;
                }
            }
            
            // Create new layer if needed
            if (!placed)
            {
                layers.Add(new List<NumericRange<double, string>> { range });
            }
        }
        
        return layers;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        // Give focus to enable keyboard shortcuts
        Focus();
        
        _isDragging = true;
        _lastPointerPosition = e.GetPosition(this);
        e.Pointer.Capture(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isDragging = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
        if (_isDragging)
        {
            var currentPosition = e.GetPosition(this);
            var deltaX = currentPosition.X - _lastPointerPosition.X;
            
            // Scale pan delta relative to displayed range span for gentler panning
            var displayedSpan = ViewportEnd - ViewportStart;
            var scaledDelta = (deltaX / Bounds.Width) * displayedSpan * 0.5;
            PanRequested?.Invoke(this, -scaledDelta);
            
            _lastPointerPosition = currentPosition;
        }
        else
        {
            // Handle tooltip on hover
            HandleTooltipOnHover(e.GetPosition(this));
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        
        var position = e.GetPosition(this);
        var mouseXRatio = position.X / Bounds.Width;
        var isZoomModifier = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        
        ScrollRequested?.Invoke(this, (e.Delta.Y, isZoomModifier, mouseXRatio));
        
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        var span = ViewportEnd - ViewportStart;
        var centerX = 0.5; // Center for zoom operations
        
        switch (e.Key)
        {
            // Arrow keys for slow scroll
            case Key.Left:
                // Left = backward/earlier = negative pan
                var slowLeftDelta = span * 0.05; // 5% of viewport
                PanRequested?.Invoke(this, -slowLeftDelta);
                e.Handled = true;
                break;
                
            case Key.Right:
                // Right = forward/later = positive pan
                var slowRightDelta = span * 0.05;
                PanRequested?.Invoke(this, slowRightDelta);
                e.Handled = true;
                break;
                
            // Page Up/Down for fast scroll
            case Key.PageUp:
                // Page Up = backward/earlier = negative pan
                var fastLeftDelta = span * 0.5; // 50% of viewport
                PanRequested?.Invoke(this, -fastLeftDelta);
                e.Handled = true;
                break;
                
            case Key.PageDown:
                // Page Down = forward/later = positive pan
                var fastRightDelta = span * 0.5;
                PanRequested?.Invoke(this, fastRightDelta);
                e.Handled = true;
                break;
                
            // Home/End to jump to boundaries
            case Key.Home:
                if (!double.IsNaN(DataMin))
                {
                    var newStart = DataMin;
                    var newEnd = newStart + span;
                    // Use pan to move to start
                    var homeDelta = ViewportStart - newStart;
                    PanRequested?.Invoke(this, homeDelta);
                }
                e.Handled = true;
                break;
                
            case Key.End:
                if (!double.IsNaN(DataMax))
                {
                    var newEnd = DataMax;
                    var newStart = newEnd - span;
                    // Use pan to move to end
                    var endDelta = ViewportStart - newStart;
                    PanRequested?.Invoke(this, endDelta);
                }
                e.Handled = true;
                break;
                
            // +/- for zoom
            case Key.Add:
            case Key.OemPlus:
                // Zoom in
                ScrollRequested?.Invoke(this, (100, true, centerX));
                e.Handled = true;
                break;
                
            case Key.Subtract:
            case Key.OemMinus:
                // Zoom out
                ScrollRequested?.Invoke(this, (-100, true, centerX));
                e.Handled = true;
                break;
        }
    }

    private IBrush GetBrushFromHashCode(int hashCode)
    {
        // Generate colors based on hashcode for better distribution
        var hash = Math.Abs(hashCode);
        
        // Extract RGB components from hash
        var r = (byte)((hash >> 16) & 0xFF);
        var g = (byte)((hash >> 8) & 0xFF);
        var b = (byte)(hash & 0xFF);
        
        // Ensure colors are vibrant and visible by adjusting brightness
        r = (byte)Math.Max(80, Math.Min(220, (int)r));
        g = (byte)Math.Max(80, Math.Min(220, (int)g));
        b = (byte)Math.Max(80, Math.Min(220, (int)b));
        
        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }

    private void HandleTooltipOnHover(Point mousePosition)
    {
        var hoveredRange = GetRangeAtPosition(mousePosition);
        
        if (hoveredRange != null)
        {
            ShowTooltip(hoveredRange, mousePosition);
        }
        else
        {
            HideTooltip();
        }
    }

    private NumericRange<double, string>? GetRangeAtPosition(Point position)
    {
        if (_cachedLayers.Count == 0) return null;

        for (int layer = 0; layer < _cachedLayers.Count; layer++)
        {
            var layerY = 50 + layer * 30;
            var layerRect = new Rect(0, layerY, Bounds.Width, 20);
            
            if (layerRect.Contains(position))
            {
                foreach (var range in _cachedLayers[layer])
                {
                    var startX = ValueToScreenX(range.Start);
                    var endX = ValueToScreenX(range.End);
                    
                    // Clamp to visible area
                    startX = Math.Max(0, startX);
                    endX = Math.Min(Bounds.Width, endX);
                    
                    if (endX <= startX) continue;
                    
                    var rangeRect = new Rect(startX, layerY, endX - startX, 20);
                    if (rangeRect.Contains(position))
                    {
                        return range;
                    }
                }
            }
        }
        
        return null;
    }

    private void ShowTooltip(NumericRange<double, string> range, Point position)
    {
        if (_currentToolTip == null)
        {
            _currentToolTip = new ToolTip();
        }

        var tooltipText = $"{range.Value}\nStart: {range.Start:F2}\nEnd: {range.End:F2}\nSpan: {range.Span:F2}";
        _currentToolTip.Content = tooltipText;
        
        ToolTip.SetTip(this, _currentToolTip);
        ToolTip.SetIsOpen(this, true);
    }

    private void HideTooltip()
    {
        ToolTip.SetIsOpen(this, false);
        ToolTip.SetTip(this, null);
    }
    
}