using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using MultithreadedCommand.Core.MultithreadedDictionary;

namespace MultithreadedCommand.Core.Async
{
    public class TimedStaticAsyncCommandContainer : StaticAsyncCommandContainer, ITimedAsyncCommandContainer
    {
        #region ITimedAsyncCommandContainer Members

        public void SetActive(string id, Type commandType)
        {
            throw new NotImplementedException();
        }

        public void SetInactive(string id, Type commandType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
