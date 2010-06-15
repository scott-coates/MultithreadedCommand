using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultithreadedCommand.Web.Helpers;
using MultithreadedCommand.Core.Commands;
using MultithreadedCommand.Core.Async;

namespace MultithreadedCommand.Web.Controllers
{
    public class AsyncManagerController : Controller
    {
        private IAsyncCommandContainer _container;
        private IAsyncCommandRemover _remover;

        public AsyncManagerController(IAsyncCommandContainer container, IAsyncCommandRemover remover)
        {
            _container = container;
            _remover = remover;
        }

        public JsonResult GetCurrentProgress(string id, string funcName)
        {
            MVCHelper.AddNoCache(ControllerContext.HttpContext.Response);
            Type commandType = ReflectionHelper.GetTypeByName(funcName);

            FuncStatus progress = null;

            if (_container.Exists(id, commandType))
            {
                progress = _container.Get(id, commandType).Progress;
            }
            else
            {
                progress = new FuncStatus { Status = StatusEnum.NotStarted };
            }

            return Json(progress);
        }

        public JsonResult RestartProcess(string id, string funcName)
        {
            MVCHelper.AddNoCache(ControllerContext.HttpContext.Response);
            Type commandType = ReflectionHelper.GetTypeByName(funcName);

            FuncStatus progress = null;

            if (_container.Exists(id, commandType))
            {
                IAsyncCommand manager = _container.Get(id, commandType);
                manager.Start();
                progress = manager.Progress;
            }
            else
            {
                progress = new FuncStatus { Status = StatusEnum.NotStarted };
            }

            return Json(progress);
        }

        public void CancelProcess(string id, string funcName)
        {
            MVCHelper.AddNoCache(ControllerContext.HttpContext.Response);
            Type commandType = ReflectionHelper.GetTypeByName(funcName);

            IAsyncCommand funcToCheck = _container.Get(id, commandType);

            if (funcToCheck != null)
            {
                _remover.RemoveCommand(id, commandType);
            }
        }
    }
}
