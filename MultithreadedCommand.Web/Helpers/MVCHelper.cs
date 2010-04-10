using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultithreadedCommand.Web.Helpers
{
    public class MVCHelper
    {
        public static void AddNoCache(HttpResponseBase response)
        {
            response.AddHeader("cache-control", "no-cache");
        }
    }
}