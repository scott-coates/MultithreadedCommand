using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MultithreadedCommand.Core.Async
{
    public class AsyncContainerItem : IDisposable
    {
        public IAsyncCommand AsyncCommand { get; private set; }
        public Timer Timer { get; private set; }
        public event Action OnItemRemoved;

        public AsyncContainerItem(IAsyncCommand _command, Timer timer)
        {
            AsyncCommand = _command;
            Timer = timer;
        }

        public void DoOnRemoved()
        {
            if (OnItemRemoved != null)
            {
                OnItemRemoved();
            }
        }

        public void Dispose()
        {
            AsyncCommand.Dispose();
            Timer.Dispose();
        }
    }
}
