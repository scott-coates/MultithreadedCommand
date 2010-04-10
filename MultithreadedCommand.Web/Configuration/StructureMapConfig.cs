using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap;

namespace MultithreadedCommand.Web.Configuration
{
    public class StructureMapConfig
    {
        public class DomainServiceRegistry : Registry
        {
            public DomainServiceRegistry()
            {
                Scan(x =>
                {
                    x.Assembly("MultithreadedCommand.Core");
                    x.Assembly("MultithreadedCommand.Web");
                    x.TheCallingAssembly();
                    x.With<DefaultConventionScanner>();
                });
            }
        }

        public static class Bootstrapper
        {
            public static void ConfigureStructureMap()
            {
                ObjectFactory.Initialize(c =>
                {
                    c.AddRegistry<DomainServiceRegistry>();
                });
            }
        }
    }
}