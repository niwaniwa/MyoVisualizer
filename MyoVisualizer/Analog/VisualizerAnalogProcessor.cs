using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace MyoVisualizer.Analog
{
    internal class VisualizerAnalogProcessor : AnalogProcessor
    {
        
        private int _cycleCount = 0;
        
        public VisualizerAnalogProcessor(short samplingClock, short eventSamplingCount, short AiChannels) : base(samplingClock, eventSamplingCount, AiChannels)
        {
        }

        public override unsafe int CallBackProc(short Id, short Message, int wParam, int lParam, void* Param)
        {
            base.CallBackProc(Id, Message, wParam, lParam, Param);
            Console.WriteLine($"Cycle {++_cycleCount}: Sampling Count {lParam} : lastdata {_data[0].Last()}");
            return 0;
        }

    }
}
