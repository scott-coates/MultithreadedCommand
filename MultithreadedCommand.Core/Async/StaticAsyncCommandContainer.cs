using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using MultithreadedCommand.Core.MultithreadedDictionary;

namespace MultithreadedCommand.Core.Async
{
    public class StaticAsyncCommandContainer : IAsyncCommandContainer
    {
        protected MultithreadedDictionary<KeyValuePair<string, Type>, IAsyncContainerItem> _container = null;

        public StaticAsyncCommandContainer()
        {
            _container = MultithreadedDictionary<KeyValuePair<string, Type>, IAsyncContainerItem>.Instance;
        }

        private KeyValuePair<string, Type> GetKey(string id, Type commandType)
        {
            return new KeyValuePair<string, Type>(id, commandType);
        }

        public void Add(IAsyncCommand funcToRun, string id, Type commandType)
        {
                if (Exists(id, commandType))
                {
                    throw new Exception(string.Format("{0} already exists.", id));
                }
            
                var item = GetAsyncItemToAdd(funcToRun, id, commandType);

                _container.Add(GetKey(id, commandType), item);
        }

        private AsyncContainerItem GetAsyncItemToAdd(IAsyncCommand funcToRun, string id, Type commandType)
        {
            //var timer = new Timer(_timeSpan.Time.TotalMilliseconds) { Enabled = false, AutoReset = false };
            //timer.Elapsed += (object sender, ElapsedEventArgs e) => RemoveJob(id, commandType);
            
            var item = new AsyncContainerItem(funcToRun);
            return item;
        }

        public IAsyncContainerItem Get(string id, Type commandType)
        {
                if (!Exists(id, commandType))
                {
                    throw new Exception(string.Format("{0} does not exist.", id));
                }

                return _container[GetKey(id, commandType)];
        }

        private void RemoveJob(string id, Type commandType)
        {
                try
                {
                    _container[GetKey(id, commandType)].Dispose();
                    _container[GetKey(id, commandType)].DoOnRemoved();
                }
                catch (Exception e)
                {
                    //error disposing.
                }

                _container.Remove(GetKey(id, commandType));
        }

        public int Count
        {
            get
            {
                    return _container.Count;
            }
        }

        public void Remove(string id, Type commandType)
        {
                var func = Get(id, commandType); //make sure it exists -- will throw error if it doesn't
                RemoveJob(id, commandType);
        }

        public bool Exists(string id, Type commandType)
        {
                return _container.Keys.Contains(GetKey(id, commandType));
        }

        internal void RemoveAll()
        {
                _container.Clear();
        }

        //public void SetActive(string id, Type commandType)
        //{
        //    lock (_syncRoot)
        //    {
        //        GetContainerItem(id, commandType).Timer.Stop();
        //    }
        //}

        //public virtual void SetInactive(string id, Type commandType)
        //{
        //    lock (_syncRoot)
        //    {
        //        GetContainerItem(id, commandType).Timer.Start();
        //    }
        //}
    }
}
