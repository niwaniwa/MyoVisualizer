using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CaioCs;

namespace MyoVisualizer.Analog
{
    public class AnalogProcessor
    {

        protected AnalogInputSystem _analog;
        protected Dictionary<short, List<float>> _data = new Dictionary<short, List<float>>();
        private int _cycleCount = 0;
        private short _samplingClock = 0;
        private DateTime _startTime = DateTime.Now, _lastTime;

        public Dictionary<short, List<float>> Data
        {
            get => _data;
        }

        public DateTime StartTime
        {
            get => _startTime;
            private set => _startTime = value;
        }

        public DateTime LastTime
        {
            get => _lastTime;
            private set => _lastTime = value;
        }

        public AnalogProcessor(short samplingClock, short eventSamplingCount, short AiChannels)
        {
            _samplingClock = samplingClock;
            unsafe
            {
                
                _analog = new AnalogInputSystem("AIO000");
                _analog.SetInputMethod(0);
                _analog.SetAiRangeAll((short)CaioConst.PM25);
                _analog.SetAiChannels(AiChannels);
                _analog.SetAiTransferMode(0);
                _analog.SetAiMemoryType(0);
                _analog.SetAiStopTrigger(4);
                _analog.Caio.SetAiMemoryType(_analog.Id, 0);
                _analog.SetAiEventSamplingTimes(eventSamplingCount);

                _analog.SetAiSamplingClock(_samplingClock); // 1000 = 1ミリ秒
                _analog.SetCallback(new PAICALLBACK(CallBackProc));

            }
        }

        public void Start()
        {
            _startTime = DateTime.Now;
            _analog.StartInput();
            Console.WriteLine($"StartInput");
        }

        public void Stop()
        {
            _analog.Destory();
        }

        public List<object> GetDataTime()
        {
            return Enumerable.Range(0, _data[0].Count).Select(i => (object)i).ToList();
        }


        unsafe public virtual int CallBackProc(short Id, short Message, int wParam, int lParam, void* Param)
        {

            // Console.WriteLine($"Cycle {++_cycleCount}: Sampling Count {lParam}");
            // _analog.Caio.GetAiSamplingCount(_analog.Id, out var count);
            float[] AiData = new float[lParam * 2];
            var result = _analog.Caio.GetAiSamplingDataEx(_analog.Id, ref lParam, ref AiData);

            if (result != 0)
            {
                _analog.Caio.GetErrorString(result, out var errorString);
                Console.WriteLine("aio.GetAiSamplingDataEx : " + errorString);
                return 0;
            }


            _lastTime = DateTime.Now;


            _analog.Caio.GetAiChannels(_analog.Id, out var channels);

            for (int i = 0; i < channels; i++)
            {
                if (!_data.ContainsKey((short)i))
                {
                    _data.Add((short)i, new List<float>());
                }
            }

            for (int i = 0; i < AiData.Length; i++)
            {

                if (i == 0)
                {
                    _data[0].Add(AiData[0]);
                    continue;
                }

                _data[(short)(i % channels)].Add(AiData[i]);
            }


            return 0;
        }
    }
}