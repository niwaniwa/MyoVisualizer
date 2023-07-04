using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyoVisualizer.Analog;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.Axes;
using System.Windows.Threading;

namespace MyoVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private VisualizerAnalogProcessor processor;
        private LineSeries lineSeries;
        private const int viewSize = 100;

        public MainWindow()
        {
            InitializeComponent();

            short samplingClock = 1000;
            short eventSamplingCount = 100;

            processor = new VisualizerAnalogProcessor(samplingClock, eventSamplingCount, 2);

            var model = new PlotModel { Title = "Frequency Data" };

            // Define x-axis with a scrolling window of size 'viewSize'
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = viewSize,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 1000 // Adjust this to fit your needs
            });

            lineSeries = new LineSeries { StrokeThickness = 2 };
            model.Series.Add(lineSeries);

            Plot.Model = model;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            processor.Start();
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            processor.Stop();
        }

    }
}