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
        private const int viewSize = 5800;
        private DispatcherTimer timer;
        private int time;
        private int count = 0;
        private  short samplingClock = 256;
        private short eventSamplingCount = 500;
        private bool isRecording = false;
        private int sizeLimit = (2000 * 60) * 25; // 上限は25分]
        private Stopwatch _stopwatch;

        public MainWindow()
        {
            Log($"---- Initializing ----");
            InitializeComponent();

            this.Closing += MainWindow_Closing;
            _stopwatch = new Stopwatch();

            UpdateSamplingCount(samplingClock);

            // ch.1
            var model1 = new PlotModel { Title = "Frequency Data ch.1" };
            model1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = viewSize,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = sizeLimit
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
                AbsoluteMaximum = sizeLimit
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
            Log("---- End init ----");
        }
        
        private void TimerTick(object sender, EventArgs e)
        {
            time++;
            
            if (_processor.Data.Count == 0) return; 
            
            // Log($"data[0] size {_processor.Data[0].Count} count {count}\n");
            if(count >= _processor.Data[0].Count)
            {
                return;
            }

            var newPoints1 = _processor.Data[0].Skip(count).ToList(); // ch.1
            var newPoints2 = _processor.Data[1].Skip(count).ToList(); // ch.2
            
            _scatterSeries1.Points.AddRange(newPoints1.Select((point, index) => new ScatterPoint(count + index, point)));
            _scatterSeries2.Points.AddRange(newPoints2.Select((point, index) => new ScatterPoint(count + index, point)));

            count += newPoints1.Count;
            
            if (count > viewSize)
            {
                // ch.1
                Plot.Model.Axes[0].Minimum = count - viewSize;
                Plot.Model.Axes[0].Maximum = count;
                _scatterSeries1.Points.RemoveRange(0, newPoints1.Count);
                // ch.2
                Plot2.Model.Axes[0].Minimum = count - viewSize;
                Plot2.Model.Axes[0].Maximum = count;
                _scatterSeries2.Points.RemoveRange(0, newPoints2.Count);
                
            }
            
            Plot.InvalidatePlot(true);
            Plot2.InvalidatePlot(true);

            UpdateElapsedTime();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Log($"Start Input");
            _processor.Start();
            timer.Start();
            isRecording = true;
            _stopwatch.Start();
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            EndProccess();
        }

        private void EndProccess()
        {
            if (!isRecording) return;
            _processor.Stop();
            timer.Stop();
            Save();
            isRecording = false;
            _stopwatch.Reset();
        }
        
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EndButton_Click(this, new RoutedEventArgs());
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
            Log($"end process. {DateTime.Now - _processor.StartTime}");
        }
        
        public void Log(string message)
        {
            
            Console.WriteLine(message);
            
            Dispatcher.InvokeAsync(() =>
            {
                if (LogListView.Items.Count > 1000)
                {
                    LogListView.Items.RemoveAt(0);
                }

                LogListView.Items.Add(new { Timestamp = DateTime.Now, Message = message });
                LogListView.ScrollIntoView(LogListView.Items[LogListView.Items.Count - 1]);
            });
        }

        private void UpdateSamplingCount(int samplingCount)
        {
            SamplingCountText.Text = $"Samplings: {samplingCount} usec";
        }
        
        private void UpdateElapsedTime()
        {
            ElapsedTimeCount.Text = $"Elapsed time (s) : {_stopwatch.Elapsed}";
        }
       

    }
}