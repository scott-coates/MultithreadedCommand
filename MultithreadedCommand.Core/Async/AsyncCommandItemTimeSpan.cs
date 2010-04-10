using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MultithreadedCommand.Core.Async
{
    public class AsyncCommandItemTimeSpan : IAsyncCommandItemTimeSpan
    {
        private TimeSpan _removalTime = Properties.Settings.Default.ContainerRemovalTime;

        public TimeSpan Time
        {
            get { return _removalTime; }
            set { _removalTime = value; }
        }
        //private Timer _timer = null;

        //public AsyncCommandItemTimer()
        //{
        //    _timer = new Timer(_removalTime.TotalMilliseconds);
        //}

        //public void Start()
        //{
        //    _timer.Start();
        //}

        //public void Stop()
        //{
        //    _timer.Stop();
        //}

        //public bool Enabled
        //{
        //    get
        //    {
        //        return _timer.Enabled;
        //    }
        //    set
        //    {
        //        _timer.Enabled = value;
        //    }
        //}

        //public double Interval
        //{
        //    get
        //    {
        //        return _timer.Interval;
        //    }
        //    set
        //    {
        //        _timer.Interval = value;
        //    }
        //}

        //public event ElapsedEventHandler Elapsed
        //{
        //    add { _timer.Elapsed += value; }
        //    remove { _timer.Elapsed -= value; }
        //}
    }
}
