using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace MultithreadedCommand.Core.Logging
{
    public interface ILogProvider
    {
        ILog GetLogger();
    }
}
