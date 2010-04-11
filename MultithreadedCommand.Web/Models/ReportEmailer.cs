using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using MultithreadedCommand.Core.Commands;

namespace MultithreadedCommand.Web.Models
{
    public class ReportEmailer : CommandBase
    {
        private int _totalSteps = 5;
        private int _currentStep = 0;
        private ReportEmail _reportEmail;

        public ReportEmailer(ReportEmail reportEmail)
        {
            _reportEmail = reportEmail;
            _funcProperties.ShouldBeRemovedOnComplete = true;
        }

        private void NextStep()
        {
            Thread.Sleep(2000);
            _currentStep++;
        }

        protected override void CoreStart()
        {
            _message = string.Format("Retrieving Report: {0}", _reportEmail.ReportName);
            NextStep();

            _message = string.Format("Calculating Report: {0}", _reportEmail.ReportName);
            NextStep();

            _message = string.Format("Saving Report: {0}", _reportEmail.ReportName);
            NextStep();

            _message = "Queueing Email";
            NextStep();

            _message = string.Format("Sending email to: {0}.  From: {1}", _reportEmail.RecipientEmail, _reportEmail.FromEmail);
            NextStep();
        }

        protected override int Total
        {
            get
            {
                return _totalSteps;
            }
        }

        protected override int FinishedSoFar
        {
            get
            {
                return _currentStep;
            }
        }
    }
}