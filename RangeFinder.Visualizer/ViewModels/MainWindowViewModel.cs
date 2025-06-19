using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RangeFinder.Core;
using RangeFinder.IO;

namespace RangeFinder.Visualizer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _selectedDataset = "timeseries_sample.csv";
    private ObservableCollection<NumericRange<double, string>> _ranges = new();
    private double _viewportStart = 0.0;
    private double _viewportEnd = 100.0;
    private double _dataMin = double.NaN;
    private double _dataMax = double.NaN;

    public string[] AvailableDatasets { get; } = 
    {
        "sparse",
        "medium", 
        "dense",
        "timeseries_sample.csv",
        "overlapping_sample.csv", 
        "large_dataset_sample.csv"
    };

    public string SelectedDataset
    {
        get => _selectedDataset;
        set
        {
            if (SetField(ref _selectedDataset, value))
            {
                LoadDataset(value);
            }
        }
    }

    public ObservableCollection<NumericRange<double, string>> Ranges
    {
        get => _ranges;
        set => SetField(ref _ranges, value);
    }

    public double ViewportStart
    {
        get => _viewportStart;
        set => SetField(ref _viewportStart, value);
    }

    public double ViewportEnd
    {
        get => _viewportEnd;
        set => SetField(ref _viewportEnd, value);
    }

    public double DataMin
    {
        get => _dataMin;
        set => SetField(ref _dataMin, value);
    }

    public double DataMax
    {
        get => _dataMax;
        set => SetField(ref _dataMax, value);
    }

    public MainWindowViewModel()
    {
        LoadDataset(_selectedDataset);
    }

    private void LoadDataset(string datasetName)
    {
        try
        {
            if (datasetName.EndsWith(".csv"))
            {
                // Load sample CSV file
                LoadSampleCsvFile(datasetName);
            }
            else
            {
                // Generate sample data
                var sampleRanges = GenerateSampleData(datasetName);
                Ranges = new ObservableCollection<NumericRange<double, string>>(sampleRanges);
                
                if (Ranges.Any())
                {
                    DataMin = Ranges.Min(r => r.Start);
                    DataMax = Ranges.Max(r => r.End);
                    ViewportStart = DataMin;
                    ViewportEnd = DataMax;
                }
            }
        }
        catch (Exception)
        {
            // Fall back to sparse data on error
            var fallbackRanges = GenerateSampleData("sparse");
            Ranges = new ObservableCollection<NumericRange<double, string>>(fallbackRanges);
            
            if (Ranges.Any())
            {
                DataMin = Ranges.Min(r => r.Start);
                DataMax = Ranges.Max(r => r.End);
                ViewportStart = DataMin;
                ViewportEnd = DataMax;
            }
        }
    }

    private void LoadSampleCsvFile(string fileName)
    {
        try
        {
            // Get the directory where the executable is located
            var exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(exeDirectory!, "SampleData", fileName);
            
            if (File.Exists(filePath))
            {
                var loadedRanges = RangeSerializer.ReadCsv<double, string>(filePath).ToList();
                Ranges = new ObservableCollection<NumericRange<double, string>>(loadedRanges);
                
                if (Ranges.Any())
                {
                    DataMin = Ranges.Min(r => r.Start);
                    DataMax = Ranges.Max(r => r.End);
                    ViewportStart = DataMin;
                    ViewportEnd = DataMax;
                }
            }
            else
            {
                throw new FileNotFoundException($"Sample file not found: {filePath}");
            }
        }
        catch (Exception)
        {
            // Fall back to generated data if file loading fails
            var fallbackRanges = GenerateSampleData("sparse");
            Ranges = new ObservableCollection<NumericRange<double, string>>(fallbackRanges);
            
            if (Ranges.Any())
            {
                DataMin = Ranges.Min(r => r.Start);
                DataMax = Ranges.Max(r => r.End);
                ViewportStart = DataMin;
                ViewportEnd = DataMax;
            }
        }
    }

    private List<NumericRange<double, string>> GenerateSampleData(string datasetName)
    {
        var random = new Random(42);
        var ranges = new List<NumericRange<double, string>>();

        switch (datasetName)
        {
            case "sparse":
                // Sparse: 1000 ranges with minimal overlap, spread over wide area
                for (int i = 0; i < 1000; i++)
                {
                    var start = random.NextDouble() * 10000; // Wide spread
                    var duration = random.NextDouble() * 5 + 1; // Short ranges
                    ranges.Add(new NumericRange<double, string>(
                        start, start + duration, $"Sparse_{i:D4}"));
                }
                break;

            case "medium":
                // Medium: 2000 ranges with moderate overlap
                for (int i = 0; i < 2000; i++)
                {
                    var start = random.NextDouble() * 5000; // Medium spread
                    var duration = random.NextDouble() * 15 + 5; // Medium ranges
                    ranges.Add(new NumericRange<double, string>(
                        start, start + duration, $"Medium_{i:D4}"));
                }
                break;

            case "dense":
                // Dense: 5000 ranges with heavy overlap
                for (int i = 0; i < 5000; i++)
                {
                    var start = random.NextDouble() * 2000; // Narrow spread
                    var duration = random.NextDouble() * 25 + 10; // Longer ranges
                    ranges.Add(new NumericRange<double, string>(
                        start, start + duration, $"Dense_{i:D4}"));
                }
                break;

            default:
                // Fallback to sparse
                for (int i = 0; i < 1000; i++)
                {
                    var start = random.NextDouble() * 10000;
                    var duration = random.NextDouble() * 5 + 1;
                    ranges.Add(new NumericRange<double, string>(
                        start, start + duration, $"Default_{i:D4}"));
                }
                break;
        }

        return ranges;
    }

    public async Task LoadRangesFromFileAsync(string filePath)
    {
        try
        {
            IEnumerable<NumericRange<double, string>> loadedRanges;
            
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            loadedRanges = extension switch
            {
                ".csv" => RangeSerializer.ReadCsv<double, string>(filePath),
                ".parquet" => await RangeSerializer.ReadParquetAsync<double, string>(filePath),
                _ => throw new NotSupportedException($"File format '{extension}' is not supported. Use .csv or .parquet files.")
            };

            var rangesList = loadedRanges.ToList();
            if (rangesList.Count == 0)
            {
                throw new InvalidOperationException("The file contains no valid ranges.");
            }

            Ranges = new ObservableCollection<NumericRange<double, string>>(rangesList);
            
            DataMin = Ranges.Min(r => r.Start);
            DataMax = Ranges.Max(r => r.End);
            ViewportStart = DataMin;
            ViewportEnd = DataMax;
        }
        catch (Exception ex)
        {
            // For now, fall back to sample data on error
            // In a real app, you'd show an error dialog
            LoadDataset("sparse");
            throw new InvalidOperationException($"Failed to load file: {ex.Message}", ex);
        }
    }

    public void OnPanRequested(double delta)
    {
        var newStart = ViewportStart + delta;
        var newEnd = ViewportEnd + delta;
        var span = ViewportEnd - ViewportStart;

        if (!double.IsNaN(DataMin) && !double.IsNaN(DataMax))
        {
            if (newStart < DataMin)
            {
                newStart = DataMin;
                newEnd = newStart + span;
            }
            else if (newEnd > DataMax)
            {
                newEnd = DataMax;
                newStart = newEnd - span;
            }
        }

        ViewportStart = newStart;
        ViewportEnd = newEnd;
    }

    public void OnScrollRequested(double delta, bool isZoomModifier, double mouseX)
    {
        if (isZoomModifier)
        {
            var span = ViewportEnd - ViewportStart;
            // Scale zoom amount based on viewport span - larger spans need bigger zoom steps
            // Increased base sensitivity for faster zooming
            var zoomSensitivity = Math.Max(0.01, span * 0.0001);
            var zoomFactor = 1.0 + (delta * zoomSensitivity);
            var newSpan = span / zoomFactor;
            
            var mouseValue = ViewportStart + mouseX * span;
            var newStart = mouseValue - (newSpan * mouseX);
            var newEnd = newStart + newSpan;

            ViewportStart = newStart;
            ViewportEnd = newEnd;
        }
        else
        {
            OnPanRequested(delta * (ViewportEnd - ViewportStart) * 0.05);
        }
    }

    public void ResetViewport()
    {
        if (Ranges?.Count > 0 && !double.IsNaN(DataMin) && !double.IsNaN(DataMax))
        {
            var dataSpan = DataMax - DataMin;
            var rangeCount = Ranges.Count;
            
            // Calculate optimal initial viewport - show roughly 20-30 ranges
            var optimalRangeCount = Math.Min(rangeCount, Math.Max(20, rangeCount / 4));
            var unitsPerRange = dataSpan / rangeCount;
            var idealViewportSpan = unitsPerRange * optimalRangeCount;
            
            // Ensure minimum viewport size
            var minViewportSpan = dataSpan * 0.1;
            var viewportSpan = Math.Max(idealViewportSpan, minViewportSpan);
            
            // Center on data or show all if viewport covers most data
            if (viewportSpan >= dataSpan * 0.8)
            {
                var padding = dataSpan * 0.05;
                ViewportStart = DataMin - padding;
                ViewportEnd = DataMax + padding;
            }
            else
            {
                var center = DataMin + dataSpan / 2;
                ViewportStart = center - viewportSpan / 2;
                ViewportEnd = center + viewportSpan / 2;
            }
        }
    }
}