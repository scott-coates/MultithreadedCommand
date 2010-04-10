using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultithreadedCommand.Core.Commands;

namespace MultithreadedCommand.Core.Async
{
    public class TimedAsyncManager : AsyncManager
    {
        private new ITimedAsyncCommandContainer _container = null;
        public TimedAsyncManager(ICommand funcToRun, string id, ITimedAsyncCommandContainer container)
            : base(funcToRun, id, container)
        {
            SetInactive();//set as inactive right away.
        }

        public override void Start()
        {
            if (!IsRunning)
            {
                SetActive();
                base.Start();
            }
        }

        protected override void End(IAsyncResult callback)
        {
            try
            {
                base.End(callback);
            }
            finally
            {
                //cancelling removes from container
                bool wasCancelled = _asyncFunc.Progress.Status == StatusEnum.Cancelled;
                if (!wasCancelled)
                {
                    SetInactive();
                }
            }
        }

        private void SetInactive()
        {
            if (_container.Exists(_id, DecoratedCommand.GetType()))
            {
                _container.SetInactive(_id, DecoratedCommand.GetType());
                DoOnAfterSetActionInactive();
            }
        }

        private void SetActive()
        {
            _container.SetActive(_id, DecoratedCommand.GetType());
        }
    }
}
