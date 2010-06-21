using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultithreadedCommand.Core.Async;
using MultithreadedCommand.Web.Models;
using CaresCommon.Logging;

namespace MultithreadedCommand.Web.Controllers
{
    public class LongRunnerController : Controller
    {
        private IAsyncCommandContainer _container;
        private ILogger _logger;

        public LongRunnerController(IAsyncCommandContainer container, ILogger logger)
        {
            _container = container;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ViewResult LongRunningJob(int id)
        {
            ViewData["Number"] = id;
            return View();
        }

        public void RunLongJob(int id)
        {
            IAsyncCommand longRunner = new LongRunner(id).AsAsync(id.ToString(), _container, _logger);
            longRunner.Start();
        }
    }
}
