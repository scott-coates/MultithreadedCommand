using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MultithreadedCommand.Core.Commands;
using System.Threading;

namespace MultithreadedCommand.Web.Models
{
    public class LongRunner : CommandBase
    {
        private readonly int _totalSteps = 0;
        private int _currentStep = 0;

        public LongRunner(int totalSteps)
        {
            _totalSteps = totalSteps;
        }

        protected override void CoreStart()
        {
            for (_currentStep = 1; _currentStep <= _totalSteps; _currentStep++)
            {
                Thread.Sleep(1000);
            }
        }

        protected override int FinishedSoFar
        {
            get
            {
                return _currentStep;
            }
        }

        protected override int Total
        {
            get
            {
                return _totalSteps;
            }
        }
    }
}