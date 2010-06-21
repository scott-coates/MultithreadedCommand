using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MultithreadedCommand.Core.Helpers;
using CaresCommon.Logging;

namespace MultithreadedCommand.Core.Commands
{
    public abstract class CommandBase : ICommand
    {
        protected object _syncRoot = new object();
        protected Stopwatch _watch = new Stopwatch();
        protected DateTime _startDateTime;
        protected DateTime _endDateTime;
        protected FuncProperties _funcProperties = new FuncProperties();
        protected StatusEnum _status;
        protected string _message = string.Empty;
        protected ILogger _logger;

        public event Action OnStart;

        public event Action OnEnd;

        public event Action OnCancelled;

        public event Action OnSuccess;

        public CommandBase(ILogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            try
            {
                SetRunning();
                StartTiming();
                DoOnStart();
                CoreStart();
                DoOnSuccess();//not called if cancelled
                DoOnEnd();
                EndTiming();
                
                SetEndOfJobStatus();
            }
            catch
            {
                lock (_syncRoot)
                {
                    _status = StatusEnum.Error;
                    EndTiming(); //job is essentially done. stop counting.
                    throw;
                }
            }
        }

        private void SetEndOfJobStatus()
        {
            lock (_syncRoot)
            {
                if (_status != StatusEnum.Cancelled)
                {
                    _status = StatusEnum.Finished;
                }
            }
        }

        private void SetRunning()
        {
            lock (_syncRoot)
            {
                _status = StatusEnum.Running;
            }
        }

        public void Cancel()
        {
            lock (_syncRoot)
            {
                if (_status  == StatusEnum.Running)
                {
                    SetCancelledFlags();
                }
            }
        }

        private void EndTiming()
        {
            if (_watch != null)
            {
                _watch.Stop();
            }
            _endDateTime = DateTime.Now;
        }

        private void StartTiming()
        {
            _watch.Reset();
            _startDateTime = DateTime.Now;
            _watch.Start();
        }

        protected internal abstract void CoreStart();

        protected void DoOnStart()
        {
            if (OnStart != null)
            {
                OnStart();
            }
        }

        protected void DoOnEnd()
        {
            //dont run if cancelled
            if (OnEnd != null)
            {
                OnEnd();
            }
        }

        protected void DoOnSuccess()
        {
            //dont run if cancelled
            if (_status == StatusEnum.Running && OnSuccess != null)
            {
                OnSuccess();
            }
        }

        protected void DoOnCancelled()
        {
            if (OnCancelled != null)
            {
                OnCancelled();
            }
        }

        protected void SetCancelledFlags()
        {
            lock (_syncRoot)
            {
                _status = StatusEnum.Cancelled;
            }
        }

        public virtual FuncStatus Progress
        {
            get
            {
                //lock (_syncRoot) //Really needed?
                {
                    var retVal = new FuncStatus();

                    retVal.Message = _message;
                    retVal.Status = _status;
                    retVal.FinishedSoFar = FinishedSoFar;
                    retVal.Total = Total;
                    retVal.PercentDone = ProgressHelper.GetPercentComplete(FinishedSoFar, Total);
                    if (_status == StatusEnum.Running)
                    {
                        retVal.TimeRemaining = ProgressHelper.TimeRemaining(_watch.Elapsed.TotalSeconds, retVal.PercentDone, _startDateTime);
                    }
                    else
                    {
                        retVal.TimeRemaining = ProgressHelper.TimeRemaining(_watch.Elapsed.TotalSeconds, retVal.PercentDone, _startDateTime, _endDateTime);
                    }

                    return retVal;
                }
            }
        }

        protected virtual int FinishedSoFar
        {
            get
            {
                return 0;
            }
        }

        protected virtual int Total
        {
            get
            {
                return 0;
            }
        }

        public virtual void Dispose()
        {

        }

        public virtual FuncProperties Properties
        {
            get
            {
                return _funcProperties;
            }
        }

        public void SetActive()
        {
            lock (_syncRoot)
            {
                _status = StatusEnum.Running;
            }
        }
    }
}
