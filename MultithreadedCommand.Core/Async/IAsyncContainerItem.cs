using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MultithreadedCommand.Core.Async
{
    public interface IAsyncContainerItem : IDisposable
    {
        IAsyncCommand AsyncCommand { get; }
        event Action OnItemRemoved;
        event Action OnItemAdded;
        void DoOnRemoved();
        void DoOnAdded();
    }
}
