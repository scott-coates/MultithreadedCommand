using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.MultithreadedDictionary
{
    public interface ISynchronizedValue<T> : ISynchronizable
    {
        T Value { get; }
    }
}
