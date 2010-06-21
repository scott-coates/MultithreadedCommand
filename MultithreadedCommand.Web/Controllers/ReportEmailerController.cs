using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultithreadedCommand.Web.Models;
using MultithreadedCommand.Core.Async;
using CaresCommon.Logging;

namespace MultithreadedCommand.Web.Controllers
{
    public class ReportEmailerController : Controller
    {
        private IAsyncCommandContainer _container;
        private ILogger _logger;

        public ReportEmailerController(IAsyncCommandContainer container, ILogger logger)
        {
            _container = container;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View();
        }

        public void CreateReport(ReportEmail reportEmail, string id)
        {
            if (ModelState.IsValid)
            {
                IAsyncCommand emailer = new ReportEmailer(reportEmail).AsAsync(id, _container, _logger);

                emailer.Start();
            }
        }
    }
}
