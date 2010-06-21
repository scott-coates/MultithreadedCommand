using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultithreadedCommand.Core.Commands;
using CaresCommon.Logging;

namespace MultithreadedCommand.Core.Async
{
    public static class AsyncExtensions
    {
        public static IAsyncCommand AsAsync(this ICommand func, string id, IAsyncCommandContainer container, ILogger logger)
        {
            IAsyncCommand asyncFuncManager = new AsyncManager(func, id, container, logger);
            return asyncFuncManager;
        }
    }
}
