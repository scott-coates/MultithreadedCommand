using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Commands
{
    public enum StatusEnum
    {
        NotStarted,
        Running,
        Finished,
        Cancelled,
        Error
    }
}
