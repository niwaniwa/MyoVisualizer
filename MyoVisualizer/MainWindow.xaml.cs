using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using MyoVisualizer.Data;

namespace MyoVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private VisualizerAnalogProcessor _processor;
        private ScatterSeries _scatterSeries;
        private const int viewSize = 5000;
        private DispatcherTimer timer;
        private int time;
        private int count = 0;
        private  short samplingClock = 1000;
        private short eventSamplingCount = 100;

        public MainWindow()
        {
            InitializeComponent();


            var model = new PlotModel { Title = "Frequency Data" };


            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = viewSize,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = (1000 * 60) * 15 // 上限は15分
            });

            _scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = 2  };
            model.Series.Add(_scatterSeries);

            Plot.Model = model;
            
            _processor = new VisualizerAnalogProcessor(samplingClock, eventSamplingCount, 2);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); 
            timer.Tick += TimerTick;
            Console.WriteLine("---- End init ----");
        }

        private int deleteIndex = 0;
        
        private void TimerTick(object sender, EventArgs e)
        {
            time++;
            
            Console.Write($"data size {_processor.Data.Count} ");
            
            if (_processor.Data.Count == 0) return; 
            
            Console.Write($"data[0] size {_processor.Data[0].Count} count {count}\n");
            if(count >= _processor.Data[0].Count)
            {
                return;
            }

            var newPoints = _processor.Data[0].Skip(count).ToList();
            _scatterSeries.Points.AddRange(newPoints.Select((point, index) => new ScatterPoint(count + index, point)));

            count += newPoints.Count;
            
            deleteIndex = count;
            
            if (count > viewSize)
            {
                Plot.Model.Axes[0].Minimum = count - viewSize;
                Plot.Model.Axes[0].Maximum = count;
                _scatterSeries.Points.RemoveRange(0, newPoints.Count);
    
                
            }
            
            Plot.InvalidatePlot(true);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Start Input");
            _processor.Start();
            timer.Start();
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            _processor.Stop();
            timer.Stop();
            Save();
        }
        
        private void Save(){
            var endTime = DateTime.Now;

            var hoge = new DataCommunicator();
            var rawData = _processor.Data;
            var dictionary = new Dictionary<string, List<object>>();
            dictionary.Add("index", _processor.GetDataTime());
            foreach (var list in rawData)
            {
                dictionary.Add($"Channel {list.Key.ToString()}", list.Value.Select(f => (object) f).ToList());
            }
            
            hoge.SaveCsv($"./MyoData/Data_{DateTime.Now.ToString("yy-MM-dd_hh_mm_ss")}_{samplingClock}usec.csv", dictionary);
            File.AppendAllLines($"./MyoData/_time.txt",
                new[]
                {
                    $"Data_{DateTime.Now.ToString("yy-MM-dd_hh_mm_ss")}_{samplingClock}usec.csv, start={_processor.StartTime.ToString("hh:mm:ss.fff")}, end={endTime.ToString("hh:mm:ss.fff")}, elapsed={(DateTime.Now - _processor.StartTime)}"
                });
            Console.WriteLine($"end process. {DateTime.Now - _processor.StartTime}");
        }

    }
}