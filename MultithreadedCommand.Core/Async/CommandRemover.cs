using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Async
{
    public class CommandRemover : ICommandRemover
    {
        private IAsyncCommandContainer _container = null;

        public CommandRemover(IAsyncCommandContainer container)
        {
            _container = container;
        }

        public void RemoveCommand(string id, Type commandType)
        {
            IAsyncCommand asyncCommand = _container.Get(id, commandType).AsyncCommand;
            asyncCommand.Cancel();
            _container.Remove(id, commandType);
        }
    }
}
