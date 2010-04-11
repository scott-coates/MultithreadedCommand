using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultithreadedCommand.Web.Models;
using MultithreadedCommand.Core.Async;

namespace MultithreadedCommand.Web.Controllers
{
    public class ReportEmailerController : Controller
    {
        private IAsyncCommandContainer _container;

        public ReportEmailerController(IAsyncCommandContainer container)
        {
            _container = container;
        }

        public ActionResult Index()
        {
            return View();
        }

        public void CreateReport(ReportEmail reportEmail, string id)
        {
            if (ModelState.IsValid)
            {
                IAsyncCommand emailer = new ReportEmailer(reportEmail).AsAsync(id, _container);

                emailer.Start();
            }
        }
    }
}
