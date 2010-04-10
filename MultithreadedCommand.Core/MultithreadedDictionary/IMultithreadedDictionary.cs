using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.MultithreadedDictionary
{
    public interface IMultithreadedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        ISynchronizedValue<TValue> SynchronizedValue(TKey Key);
    }
}