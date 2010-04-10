using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Async
{
    public class CommandContainerManager : ICommandContainerManager
    {
        private IAsyncCommandContainer _container = null;

        public CommandContainerManager(IAsyncCommandContainer container)
        {
            _container = container;
        }
    }
}
