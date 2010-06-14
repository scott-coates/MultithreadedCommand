using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace MultithreadedCommand.Core.Logging
{
    public class LogProvider : ILogProvider
    {
        private readonly ILog _log = null;

        public LogProvider()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger("MemoryLogger");
        }

        public ILog GetLogger()
        {
            return _log;   
        }
    }
}