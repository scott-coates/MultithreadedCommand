using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Language.Flow;

namespace MultithreadedCommand.Tests
{
    //http://haacked.com/archive/2009/09/29/moq-sequences.aspx
    public static class MoqExtensions
    {
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
          params TResult[] results) where T : class
        {
            setup.Returns(new Queue<TResult>(results).Dequeue);
            
            //var reader = new Mock<IDataReader>();
            //reader.Setup(r => r.Read()).ReturnsInOrder(true, true, false);
        }
    }
}
