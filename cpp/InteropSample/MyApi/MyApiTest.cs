using System;

namespace InteropSample.MyApi
{
    class MyApiTest
    {
        IntPtr _api;

        readonly MyApiInterop.TapQuoteAPIEvent _onEvent;

        public MyApiTest()
        {
            _onEvent = OnEventCB;

        }

        public void Test()
        {
            if (_api == IntPtr.Zero)
            {
                _api = MyApiInterop.CreateAPI();

                if (_api == IntPtr.Zero)
                {
                    Console.WriteLine("MyApiTest,创建API失败");
                    return;
                }

                MyApiInterop.RegisterEventCallBack(_api, _onEvent);
            }

            MyApiInterop.API_Test1(_api);

            MyApiInterop.API_Test2(_api);

            MyApiInterop.API_Test3(_api);
        }

        private void OnEventCB(EventType type, int errorCode, TAPIYNFLAG isLast, IntPtr ptr)
        {
            switch (type)
            {
                case EventType.Test1:
                    Console.WriteLine("Test1,isLast:" + isLast);
                    break;
                case EventType.Test2:
                    var info = MyApiInterop.ToField<TapAPIPositionProfitNotice>(ptr);
                    if (info != null)
                    {
                        Console.WriteLine("Test2," + info);
                    }
                    else
                    {
                        Console.WriteLine("Test2,info is null");
                    }
                    break;
                case EventType.Test3:
                    {
                        var tp = MyApiInterop.ToField<TapAPIPositionProfit>(ptr);
                        if (tp != null)
                        {
                            Console.WriteLine("Test3," + tp);
                        }
                        else
                        {
                            Console.WriteLine("Test3,tp is null");
                        }
                    }
                    break;
            }
        }
    }
}
