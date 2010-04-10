using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Async
{
    public class AsyncCommandRemover :IAsyncCommandRemover
    {
        IAsyncCommandContainer _container = null;

        public AsyncCommandRemover(IAsyncCommandContainer container)
        {
            _container = container;
        }

        public void RemoveCommand(string id, Type commandType)
        {
            IAsyncCommand command = _container.Get(id, commandType);
            command.Cancel();
            _container.Remove(id, commandType);
        }
    }
}
