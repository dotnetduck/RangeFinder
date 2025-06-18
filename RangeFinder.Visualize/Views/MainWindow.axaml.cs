using Avalonia.Controls;
using RangeFinder.Visualize.ViewModels;
using RangeFinder.Visualize.Controls;

namespace RangeFinder.Visualize.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Connect events
        if (this.FindControl<SimpleRange1DCanvas>("RangeCanvas") is SimpleRange1DCanvas canvas &&
            DataContext is MainWindowViewModel viewModel)
        {
            canvas.PanRequested += (_, delta) => viewModel.OnPanRequested(delta);
            canvas.ScrollRequested += (_, args) => viewModel.OnScrollRequested(args.delta, args.isZoomModifier, args.mouseX);
        }
    }
}