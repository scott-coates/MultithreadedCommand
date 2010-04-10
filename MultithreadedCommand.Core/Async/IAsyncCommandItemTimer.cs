using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MultithreadedCommand.Core.Async
{
    public interface IAsyncCommandItemTimer
    {
        double Interval { get; set; }
        event ElapsedEventHandler Elapsed;
        bool Enabled { get; set; }
        void Start();
        void Stop();
    }
}