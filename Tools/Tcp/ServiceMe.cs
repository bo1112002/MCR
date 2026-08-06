using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Configuration;

namespace Tools.Tcp
{

    /// <summary>
    /// 当前外部服务的接口
    /// </summary>
    public abstract class ServiceMe
    {
        /// <summary>监视端口</summary>
        public abstract int Port { get; }
        /// <summary>检查指定的http服务是否正常开启</summary>
        public abstract void Checking_Http() ;

        protected bool _IsRuning = true;
        /// <summary>当前服务是否运行</summary>
        public bool IsRuning 
        {
            get { return _IsRuning; } 
        }


        /// <summary>计时器对象</summary>
        public readonly TimerRuner TimerRunerOne;

        protected ServiceMe()
        {
            TimerRunerOne = new TimerRuner(this);
        }

    }



    /// <summary>计时器</summary>
    public class TimerRuner
    {
        ServiceMe _Me;
        Thread thr;
        public TimerRuner(ServiceMe me)
        {
            _Me = me;
            thr = new Thread(DoWorker);
            thr.Start();
        }

        DateTime _CurrentTime = DateTime.Now;
        /// <summary>获取当前时间</summary>
        public DateTime CurrentTime
        {
            get { return _CurrentTime; }
            private set { _CurrentTime = value; }
        }

        void DoWorker()
        {
            while (_Me.IsRuning)
            {
                Thread.Sleep(1000 * 30);
                CurrentTime = CurrentTime.AddSeconds(30);
                Console.WriteLine(CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"));
                _Me.Checking_Http();
            }
        }

        public void Stop()
        {
            if (thr != null)
            {
                thr.Abort();
            }
        }
    }
    
}
