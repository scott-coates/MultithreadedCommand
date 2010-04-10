using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MultithreadedCommand.Core.Async
{
    public interface IAsyncCommandItemTimeSpan
    {
        //void Start();
        //void Stop();
        //bool Enabled { get; set; }
        TimeSpan Time { get; set; }
        //event ElapsedEventHandler Elapsed;
    }
}
