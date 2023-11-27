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
        private ScatterSeries _scatterSeries1;
        private ScatterSeries _scatterSeries2;
        private const int viewSize = 6000;
        private DispatcherTimer timer;
        private int time;
        private int count = 0;
        private  short samplingClock = 500;
        private short eventSamplingCount = 200;

        public MainWindow()
        {
            InitializeComponent();

            // ch.1
            var model1 = new PlotModel { Title = "Frequency Data ch.1" };
            model1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = viewSize,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = (2000 * 60) * 20 // 上限は10分
            });

            _scatterSeries1 = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = 2  };
            model1.Series.Add(_scatterSeries1);
            
            // ch.2
            var model2 = new PlotModel { Title = "Frequency Data ch.2" };
            model2.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = viewSize,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = (2000 * 60) * 20 // 上限は10分
            });

            _scatterSeries2 = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = 2  };
            model2.Series.Add(_scatterSeries2);

            // ここまで
            Plot.Model = model1; // ch.1の軸
            Plot2.Model = model2; // ch.2の軸
            
            _processor = new VisualizerAnalogProcessor(samplingClock, eventSamplingCount, 2);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500); 
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

            var newPoints1 = _processor.Data[0].Skip(count).ToList(); // ch.1
            var newPoints2 = _processor.Data[1].Skip(count).ToList(); // ch.2
            
            _scatterSeries1.Points.AddRange(newPoints1.Select((point, index) => new ScatterPoint(count + index, point)));
            _scatterSeries2.Points.AddRange(newPoints2.Select((point, index) => new ScatterPoint(count + index, point)));

            count += newPoints1.Count;
            
            deleteIndex = count;
            
            if (count > viewSize)
            {
                // ch.1
                Plot.Model.Axes[0].Minimum = count - viewSize;
                Plot.Model.Axes[0].Maximum = count;
                _scatterSeries1.Points.RemoveRange(0, newPoints1.Count);
                // ch.2
                Plot.Model.Axes[1].Minimum = count - viewSize;
                Plot.Model.Axes[1].Maximum = count;
                _scatterSeries2.Points.RemoveRange(0, newPoints2.Count);
            }
            
            Plot.InvalidatePlot(true);
            Plot2.InvalidatePlot(true);
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