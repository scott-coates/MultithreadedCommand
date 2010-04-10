using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Commands
{
    public class FuncStatus
    {
        public virtual StatusEnum Status { get; set; }
        public float PercentDone { get; set; }
        public int FinishedSoFar { get; set; }
        public int Total { get; set; }
        public TimeSpan? TimeRemaining { get; set; }
        public string Message { get; set; }
    }   
}