using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Async
{
    public interface ITimedAsyncCommandContainer : IAsyncCommandContainer
    {
        void SetActive(string id, Type commandType);
        void SetInactive(string id, Type commandType);
    }
}