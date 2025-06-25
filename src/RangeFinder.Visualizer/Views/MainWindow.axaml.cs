using System;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RangeFinder.Visualizer.ViewModels;
using RangeFinder.Visualizer.Controls;

namespace RangeFinder.Visualizer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Connect events
        if (this.FindControl<RangeViewer>("RangeCanvas") is RangeViewer canvas &&
            DataContext is MainWindowViewModel viewModel)
        {
            canvas.PanRequested += (_, delta) => viewModel.OnPanRequested(delta);
            canvas.ScrollRequested += (_, args) => viewModel.OnScrollRequested(args.delta, args.isZoomModifier, args.mouseX);
            canvas.ResetViewportRequested += (_, _) => viewModel.ResetViewport();
        }

        // Connect load file button
        if (this.FindControl<Button>("LoadFileButton") is Button loadButton)
        {
            loadButton.Click += OnLoadFileClicked;
        }
    }

    private async void OnLoadFileClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var topLevel = GetTopLevel(this);
        if (topLevel == null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Range Data File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Range Data Files")
                {
                    Patterns = new[] { "*.csv", "*.parquet" }
                },
                new FilePickerFileType("CSV Files")
                {
                    Patterns = new[] { "*.csv" }
                },
                new FilePickerFileType("Parquet Files")
                {
                    Patterns = new[] { "*.parquet" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        });

        if (files.Count > 0 && files[0].Path?.LocalPath is string filePath)
        {
            try
            {
                await viewModel.LoadRangesFromFileAsync(filePath);
            }
            catch (Exception ex)
            {
                // In a real app, you'd show a proper error dialog
                // For now, just let the ViewModel handle the fallback
                System.Diagnostics.Debug.WriteLine($"Error loading file: {ex.Message}");
            }
        }
    }
}
