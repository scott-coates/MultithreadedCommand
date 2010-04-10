using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.MultithreadedDictionary
{
    public class SynchronizedValue<TValue> : ISynchronizedValue<TValue>
    {
        private TValue _value = default(TValue);

        private readonly object _syncRoot;

        public object SynchronizationObject { get { return _syncRoot; } }

        public SynchronizedValue(object syncRoot, TValue value)
        {
            if (syncRoot == null)
            {
                throw new NullReferenceException("syncRoot cannot be null.");
            }

            _syncRoot = syncRoot;
        }
        
        public TValue Value { get { return _value; } }
    }
}
