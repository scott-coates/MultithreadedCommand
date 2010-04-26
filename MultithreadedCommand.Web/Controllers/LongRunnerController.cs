using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultithreadedCommand.Core.Async;
using MultithreadedCommand.Web.Models;

namespace MultithreadedCommand.Web.Controllers
{
    public class LongRunnerController : Controller
    {
        private IAsyncCommandContainer _container;
        //
        // GET: /LongRunning/

        public LongRunnerController(IAsyncCommandContainer container)
        {
            _container = container;
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
            IAsyncCommand longRunner = new LongRunner(id).AsAsync(id.ToString(), _container);
            longRunner.Start();
        }
    }
}
