using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Async
{
    public interface IAsyncCommandRemover
    {
        void RemoveCommand(string id, Type commandType);
    }
}
