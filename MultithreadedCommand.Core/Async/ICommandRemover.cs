using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Async
{
    public interface ICommandRemover
    {
        void RemoveCommand(string id, Type commandType);
    }
}
