using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyoVisualizer.Analog
{
    internal class VisualizerAnalogProcessor : AnalogProcessor
    {
        public VisualizerAnalogProcessor(short samplingClock, short eventSamplingCount, short AiChannels) : base(samplingClock, eventSamplingCount, AiChannels)
        {
        }

        public override unsafe int CallBackProc(short Id, short Message, int wParam, int lParam, void* Param)
        {
            return base.CallBackProc(Id, Message, wParam, lParam, Param);
        }

    }
}
