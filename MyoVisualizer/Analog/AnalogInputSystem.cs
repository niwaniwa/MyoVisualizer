using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CaioCs;

namespace MyoVisualizer.Analog
{
    public class AnalogInputSystem
    {

        private string _deviceName;
        private short _id, samplingClockTime = 1000;
        private Caio _caio;
        
        static public	GCHandle		gCh;
        static public	PAICALLBACK		pdelegate_func;
        static public   IntPtr          pfunc;

        public Caio Caio
        {
            get => _caio;
        }

        public short Id
        {
            get => _id;
        }

        public AnalogInputSystem(string deviceName)
        {
            _caio = new Caio();
            _deviceName = deviceName;
            var result = _caio.Init( _deviceName , out _id);
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.Init : " + errorString );
            }
            
        }

        /// <summary>
        /// アナログ入力方式の設定
        /// </summary>
        /// <param name="id"></param>
        public void SetInputMethod(short mode)
        {
            var result = _caio.SetAiInputMethod( _id , mode );
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.SetAiInputMethod : " + errorString );
            }
        }
        
        /// <summary>
        /// アナログ入力レンジの設定
        /// レンジ：±10V
        /// </summary>
        /// <param name="mode"></param>
        public void SetAiRangeAll(short mode)
        {
            
            var result = _caio.SetAiRangeAll( _id , mode);
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.SetAiRangeAll : " + errorString );
                _caio.Exit( _id );
                return;
            }
        }

        /// <summary>
        /// 変換に使用する入力チャンネル数の設定
        /// </summary>
        public void SetAiChannels(short channelCount)
        {
            var result = _caio.SetAiChannels( _id , channelCount);
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.SetAiChannels : " + errorString );
                _caio.Exit( _id );
                return;
            }
        }

        /// <summary>
        /// デバイスバッファモード
        /// </summary>
        /// <param name="mode"></param>
        public void SetAiTransferMode(short mode)
        {
            var result = _caio.SetAiTransferMode( _id , (short) 0 );
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.SetAiTransferMode : " + errorString );
                _caio.Exit(_id);
                return;
            }
        }

        /// <summary>
        /// データ格納用メモリ形式の設定 Fifo = 0, Ring = 1
        /// </summary>
        /// <param name="type"></param>
        public void SetAiMemoryType(short type)
        {
            var result = _caio.SetAiMemoryType  ( _id , type );
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.SetAiMemorySize : " + errorString );
                _caio.Exit( _id );
                return;
            }
        }

        /// <summary>
        /// サンプリングクロックの設定(サンプリング回数ではない)
        /// 1000 usec = 毎秒1000回サンプリング
        /// 2000 usec = 毎秒500回サンプリング
        /// </summary>
        /// <param name="microSec">マイクロ秒</param>
        public void SetAiSamplingClock(short microSec)
        {
            samplingClockTime = microSec;
            var result = _caio.SetAiSamplingClock( _id , samplingClockTime );
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.SetAiSamplingClock : " + errorString );
                _caio.Exit( _id );
                return;
            }
        }

        /// <summary>
        /// 指定サンプリング回数格納イベントを使用する場合のサンプリング数の設定
        /// </summary>
        /// <param name="samplingCount">サンプリング回数</param>
        public void SetAiEventSamplingTimes(int samplingCount)
        {
            _caio.SetAiEventSamplingTimes(_id, samplingCount);
        }

        /// <summary>
        /// 停止条件設定
        /// </summary>
        /// <param name="mode">default = 0, 0=設定回数変換終了, 4=コマンド(stopAIなど)</param>
        public void SetAiStopTrigger(short mode)
        {
            _caio.SetAiStopTrigger(_id, mode);
        }

        public void StartInput()
        {
            _caio.StartAi(_id);
        }
        
        public unsafe void SetCallback(PAICALLBACK action)
        {
            int Ret;
            // イベント通知用デリゲートの固定ポインタを取得			
            pdelegate_func = action;
            pfunc = Marshal.GetFunctionPointerForDelegate(pdelegate_func);
            //コールバックルーチンの設定：デバイス動作終了イベント要因
            //void *パラメータ渡しテスト
            fixed (short *temp = &_id) 
                Ret= _caio.SetAiCallBackProc(_id, pfunc, (short) CaioConst.AIE_END | (short) CaioConst.AIE_DATA_NUM, temp);
            
            if(Ret != 0)
            {
                _caio.GetErrorString(Ret, out var errorString);
                Console.WriteLine("aio.SetAiCallBackProc = " + Ret.ToString() + " : " + errorString);
                return;
            }
            
            // イベント通知用デリゲート初期化
            pdelegate_func	= action;
            // デリゲートが開放されないように設定します。
            if(gCh.IsAllocated == false)
            {
                gCh	= GCHandle.Alloc(pdelegate_func);
            }


        }


        public void Destory()
        {
            var result = _caio.Exit(_id);
            if( result != 0 )
            {
                _caio.GetErrorString( result , out var errorString );
                Console.WriteLine( "aio.Exit : " + errorString );
            }
            gCh.Free();
        }
        
    }
}