using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MultithreadedCommand.Core.Helpers;

namespace MultithreadedCommand.Core.Commands
{
    public abstract class CommandBase : ICommand
    {
        protected object _syncRoot = new object();
        protected Stopwatch _watch = null;
        protected DateTime _startDateTime;
        protected DateTime _endDateTime;
        protected FuncProperties _funcProperties = new FuncProperties();
        protected StatusEnum _status;
        protected string _message = string.Empty;

        public event Action OnStart;

        public event Action OnEnd;

        public event Action OnCancelled;

        public event Action OnSuccess;

        public CommandBase()
        {
        }

        public void Start()
        {
            try
            {
                _status = StatusEnum.Running;
                StartTiming();
                DoOnStart();
                CoreStart();
                DoOnSuccess();//not called if cancelled
                DoOnEnd();
                EndTiming();

                if (_status != StatusEnum.Cancelled)
                {
                    _status = StatusEnum.Finished;
                }
            }
            catch
            {
                _status = StatusEnum.Error;
                EndTiming(); //job is essentially done. stop counting.
                throw;
            }
        }

        public void Cancel()
        {
            if (Progress.Status == StatusEnum.Running)
            {
                SetCancelledFlags();
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
            _watch = new Stopwatch();
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
                lock (_syncRoot)
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
                    else if (_watch != null)
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
    }
}
