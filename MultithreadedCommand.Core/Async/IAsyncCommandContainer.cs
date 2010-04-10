using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Async
{
    public interface IAsyncCommandContainer
    {
        IAsyncContainerItem Get(string id, Type commandType);
        bool Exists(string id, Type commandType);
        void Add(IAsyncCommand funcToRun, string id, Type commandType);
        int Count { get; }
        void Remove(string id, Type commandType);
        //void SetActive(string id, Type commandType);
        //void SetInactive(string id, Type commandType);
    }
}
