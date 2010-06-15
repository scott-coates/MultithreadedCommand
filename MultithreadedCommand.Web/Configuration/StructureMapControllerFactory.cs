using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using System.Web.Routing;

namespace MultithreadedCommand.Web.Configuration
{
    public class StructureMapControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(Type controllerType)
        {
            try
            {
                return ObjectFactory.GetInstance(controllerType) as Controller;
            }

            catch (StructureMapException)
            {
                System.Diagnostics.Debug.WriteLine(ObjectFactory.WhatDoIHave());
                throw;
            }
        }
    }
}