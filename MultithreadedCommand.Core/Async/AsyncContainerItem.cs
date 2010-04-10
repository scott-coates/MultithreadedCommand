using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MultithreadedCommand.Core.Async
{
    public class AsyncContainerItem : IAsyncContainerItem
    {
        public IAsyncCommand AsyncCommand { get; private set; }
        public event Action OnItemRemoved;
        public event Action OnItemAdded;

        public AsyncContainerItem(IAsyncCommand _command)
        {
            AsyncCommand = _command;
        }

        public void DoOnRemoved()
        {
            if (OnItemRemoved != null)
            {
                OnItemRemoved();
            }
        }

        public void DoOnAdded()
        {
            if (OnItemAdded != null)
            {
                OnItemAdded();
            }
        }

        public void Dispose()
        {
            AsyncCommand.Dispose();
        }
    }
}
