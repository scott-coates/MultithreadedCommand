using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MultithreadedCommand.Core.Async
{
    public class AsyncCommandContainer : IAsyncCommandContainer
    {
        private readonly IAsyncCommandItemTimeSpan _timeSpan = null;
        private static object _syncRoot = new object();
        //each job must have a unique ID.
        private static IDictionary<KeyValuePair<string, Type>, AsyncContainerItem> _processStatus { get; set; }

        static AsyncCommandContainer()
        {
            InitializeContainer();
        }

        public AsyncCommandContainer(IAsyncCommandItemTimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        private static void InitializeContainer()
        {
            if (_processStatus == null)
            {
                _processStatus = new Dictionary<KeyValuePair<string, Type>, AsyncContainerItem>();
            }
        }

        private KeyValuePair<string, Type> GetKey(string id, Type commandType)
        {
            return new KeyValuePair<string, Type>(id, commandType);
        }

        public void Add(IAsyncCommand funcToRun, string id, Type commandType)
        {
            lock (_syncRoot)
            {
                if (Exists(id, commandType))
                {
                    throw new Exception(string.Format("{0} already exists.", id));
                }

                var item = GetAsyncItemToAdd(funcToRun, id, commandType);

                _processStatus.Add(GetKey(id, commandType), item);
            }
        }

        private AsyncContainerItem GetAsyncItemToAdd(IAsyncCommand funcToRun, string id, Type commandType)
        {
            var timer = new Timer(_timeSpan.Time.TotalMilliseconds) { Enabled = false, AutoReset = false };
            timer.Elapsed += (object sender, ElapsedEventArgs e) => RemoveJob(id, commandType);
            
            var item = new AsyncContainerItem(funcToRun, timer);
            return item;
        }

        public IAsyncCommand Get(string id, Type commandType)
        {
            lock (_syncRoot)
            {
                //reset timer here.
                ResetTimer(id, commandType);
                return GetContainerItem(id, commandType).AsyncCommand;
            }
        }

        public virtual void ResetTimer(string id, Type commandType)
        {
            SetInactive(id, commandType);
            SetActive(id, commandType);
        }

        internal AsyncContainerItem GetContainerItem(string id, Type commandType)
        {
            lock (_syncRoot)
            {
                if (!Exists(id, commandType))
                {
                    throw new Exception(string.Format("{0} does not exist.", id));
                }

                return _processStatus[GetKey(id, commandType)];
            }
        }

        private void RemoveJob(string id, Type commandType)
        {
            lock (_syncRoot)
            {
                try
                {
                    _processStatus[GetKey(id, commandType)].Dispose();
                    _processStatus[GetKey(id, commandType)].DoOnRemoved();
                }
                catch (Exception e)
                {
                    //error disposing.
                }

                _processStatus.Remove(GetKey(id, commandType));
            }
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _processStatus.Count;
                }
            }
        }

        public void Remove(string id, Type commandType)
        {
            lock (_syncRoot)
            {
                var func = Get(id, commandType); //make sure it exists -- will throw error if it doesn't
                RemoveJob(id, commandType);
            }
        }

        public bool Exists(string id, Type commandType)
        {
            lock (_syncRoot)
            {
                return _processStatus.Keys.Contains(GetKey(id, commandType));
            }
        }

        internal void RemoveAll()
        {
            lock (_syncRoot)
            {
                _processStatus.Clear();
            }
        }

        public void SetActive(string id, Type commandType)
        {
            lock (_syncRoot)
            {
                GetContainerItem(id, commandType).Timer.Stop();
            }
        }

        public virtual void SetInactive(string id, Type commandType)
        {
            lock (_syncRoot)
            {
                GetContainerItem(id, commandType).Timer.Start();
            }
        }
    }
}
