using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultithreadedCommand.Core.Commands;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace MultithreadedCommand.Core.Async
{
    public class AsyncManager : IAsyncCommand
    {
        private readonly ICommand _asyncFunc = null;
        private readonly string _id = string.Empty;
        private readonly IAsyncCommandContainer _container = null;
        
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

        public AsyncManager(ICommand funcToRun, string id, IAsyncCommandContainer container)
        {
            _container = container;
            _id = id;
            _asyncFunc = funcToRun;
            _container.Add(this, id, funcToRun.GetType());
            SetInactive();//set as inactive right away.
        }

        public void Start(bool runAsync)
        {
            if (_asyncFunc.Progress.Status != StatusEnum.Running)
            {
                SetActive();
                Action del = _asyncFunc.Start;
                if (runAsync)
                {
                    AsyncCallback callback = new AsyncCallback(this.End);
                    del.BeginInvoke(callback, null);
                }
                else
                {
                    var result = del.BeginInvoke(null, null);
                    result.AsyncWaitHandle.WaitOne();
                    End(result);
                }
            }
        }

        public void Start()
        {
            Start(true);
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
        private void End(IAsyncResult callback)
        {
            AsyncResult result = (AsyncResult)callback;
            Action del = (Action)result.AsyncDelegate;
            
            try
            {
                del.EndInvoke(result);
                SetInactive();
                if (_asyncFunc.Properties.ShouldBeRemovedOnComplete)
                {
                    //Remove this process from our collection. 
                    //wont be removed if exception occurs
                    _container.Remove(_id, _asyncFunc.GetType()); 
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("Error with command: {0}. Id: {1}.", DecoratedCommand.GetType().ToString(), _id)
                    , e);
            }
        }

        private void SetInactive()
        {
            _container.SetInactive(_id, DecoratedCommand.GetType());
        }

        public void SetActive()
        {
            _asyncFunc.SetActive();
            _container.SetActive(_id, DecoratedCommand.GetType());
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