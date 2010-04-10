using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultithreadedCommand.Core.Commands;
using System.Runtime.Remoting.Messaging;

namespace MultithreadedCommand.Core.Async
{
    public class AsyncManager : IAsyncCommand
    {
        protected readonly ICommand _asyncFunc = null;
        protected readonly string _id = string.Empty;
        protected readonly IAsyncCommandContainer _container = null;
        
        public event Action OnStart
        {
            add
            {
                _asyncFunc.OnStart += value;
            }
            remove
            {
                _asyncFunc.OnStart -= value;
            }
        }

        public event Action OnEnd
        {
            add
            {
                _asyncFunc.OnEnd += value;
            }
            remove
            {
                _asyncFunc.OnEnd -= value;
            }
        }

        public event Action OnCancelled
        {
            add
            {
                _asyncFunc.OnCancelled += value;
            }
            remove
            {
                _asyncFunc.OnCancelled -= value;
            }
        }

        public event Action OnSuccess
        {
            add
            {
                _asyncFunc.OnSuccess += value;
            }
            remove
            {
                _asyncFunc.OnSuccess -= value;
            }
        }

        public event Action AfterSetActionInactive;

        public AsyncManager(ICommand funcToRun, string id, IAsyncCommandContainer container)
        {
            _container = container;
            _id = id;
            _asyncFunc = funcToRun;
            _container.Add(this, id, funcToRun.GetType());
        }

        public virtual void Start()
        {
            if (!IsRunning)
            {
                Action del = _asyncFunc.Start;
                AsyncCallback callback = new AsyncCallback(this.End);
                del.BeginInvoke(callback, null); //could use the return value of BeginInvoke to store AsyncHandle in Dictionary.  Could block till done for testing.
            }
        }

        protected bool IsRunning
        {
            get
            {
                return _asyncFunc.Progress.Status == StatusEnum.Running;
            }
        }

        public void Cancel()
        {
            DecoratedCommand.Cancel();
        }

        public FuncStatus Progress
        {
            get
            {
                return _asyncFunc.Progress;
            }
        }

        public FuncProperties Properties
        {
            get
            {
                return _asyncFunc.Properties;
            }
        }

        /// <summary>
        /// The callback of command being run.  This is executed on a worker thread - not the thread the user invoked.
        /// </summary>
        /// <param name="callback"></param>
        protected virtual void End(IAsyncResult callback)
        {
            AsyncResult result = (AsyncResult)callback;
            Action del = (Action)result.AsyncDelegate;

            bool wasCancelled = _asyncFunc.Progress.Status == StatusEnum.Cancelled;

            
                //TODO:Place logging here
                del.EndInvoke(result);
            
            

            //only remove if the user didn't already explicitly remove. cancelling will remove from container
            if (!wasCancelled && _asyncFunc.Properties.ShouldBeRemovedOnComplete)
            {
                _container.Remove(_id, _asyncFunc.GetType()); //Remove this process from our collection. //wont be removed if exception occurs
            }
        }

        protected void DoOnAfterSetActionInactive()
        {
            if (AfterSetActionInactive != null)
            {
                AfterSetActionInactive();
            }
        }

        public void Dispose()
        {
            _asyncFunc.Dispose();
        }

        public ICommand DecoratedCommand
        {
            get { return _asyncFunc; }
        }
    }
}
