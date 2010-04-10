using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultithreadedCommand.Web.Helpers
{
    public class ReflectionHelper
    {
        public static Type GetTypeByName(string name)
        {
            return Type.GetType(name);
        }
    }
}