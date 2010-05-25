using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Commands
{
    public interface ICommand : IDisposable
    {
        void Start();
        void Cancel();
        event Action OnStart;
        event Action OnEnd;
        event Action OnCancelled;
        event Action OnSuccess;
        FuncStatus Progress { get; }
        void SetActive();
    }

    public interface ICommand<T> : ICommand
    {
        T Value { get; }
    }
}
