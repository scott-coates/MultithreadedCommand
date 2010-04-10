using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCommand.Core.Helpers
{
    public static class ProgressHelper
    {
        public static TimeSpan? TimeRemaining(double elapsedTime, float percentComplete, DateTime startTime)
        {
            return TimeRemaining(elapsedTime, percentComplete, startTime, DateTime.Now);
        }

        public static TimeSpan? TimeRemaining(double elapsedTime, float percentComplete, DateTime startTime, DateTime endTime)
        {
            TimeSpan? retVal = null;
            if (percentComplete != 0 && percentComplete < 100)
            {
                double expectedRunSeconds = (elapsedTime * 100) / percentComplete;
                double secondsInFuture = (startTime.AddSeconds(expectedRunSeconds) - endTime).TotalSeconds;
                retVal = new TimeSpan(0, 0, Convert.ToInt32(secondsInFuture));
            }

            return retVal;
        }

        public static TimeSpan? TimeRemaining(double elapsedTime, int finishedSoFar, int total, DateTime startTime)
        {
            return TimeRemaining(elapsedTime, GetPercentComplete(finishedSoFar, total), startTime);
        }

        public static TimeSpan? TimeRemaining(double elapsedTime, int finishedSoFar, int total, DateTime startTime, DateTime endTime)
        {
            return TimeRemaining(elapsedTime, GetPercentComplete(finishedSoFar, total), startTime, endTime);
        }

        public static float GetPercentComplete(int finishedSoFar, int total)
        {
            if (finishedSoFar == 0 && total == 0)
            {
                return 100;
            }
            else
            {
                return (finishedSoFar / Convert.ToSingle(total)) * 100;
            }
        }
    }
}
