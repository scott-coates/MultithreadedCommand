//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using MultithreadedCommand.Web.Helpers;
//using MultithreadedCommand.Core.Commands;
//using MultithreadedCommand.Core.Async;

//namespace MultithreadedCommand.Web.Controllers
//{
//    public class AsyncManagerController : Controller
//    {
//        public JsonResult GetCurrentProgress(string id, string funcName)
//        {
//            MVCHelper.AddNoCache(ControllerContext.HttpContext.Response);
//            Type commandType = ReflectionHelper.GetTypeByName(funcName);

//            FuncStatus progress = null;

//            if (new StaticAsyncCommandContainer(new AsyncCommandItemTimeSpan()).Exists(id, commandType))
//            {
//                progress = new StaticAsyncCommandContainer(new AsyncCommandItemTimeSpan()).Get(id, commandType).Progress;
//            }
//            else
//            {
//                progress = new FuncStatus { Status = StatusEnum.NotStarted };
//            }

//            return Json(progress, JsonRequestBehavior.AllowGet);
//        }

//        public JsonResult RestartProcess(string id, string funcName)
//        {
//            MVCHelper.AddNoCache(ControllerContext.HttpContext.Response);
//            Type commandType = ReflectionHelper.GetTypeByName(funcName);

//            FuncStatus progress = null;

//            if (new StaticAsyncCommandContainer(new AsyncCommandItemTimeSpan()).Exists(id, commandType))
//            {
//                IAsyncCommand manager = new StaticAsyncCommandContainer(new AsyncCommandItemTimeSpan()).Get(id, commandType);
//                manager.Start();
//                progress = manager.Progress;
//            }
//            else
//            {
//                progress = new FuncStatus { Status = StatusEnum.NotStarted };
//            }

//            return Json(progress, JsonRequestBehavior.AllowGet);
//        }

//        public void CancelProcess(string id, string funcName)
//        {
//            MVCHelper.AddNoCache(ControllerContext.HttpContext.Response);
//            Type commandType = ReflectionHelper.GetTypeByName(funcName);

//            IAsyncCommand funcToCheck = new StaticAsyncCommandContainer(new AsyncCommandItemTimeSpan()).Get(id, commandType);

//            if (funcToCheck != null)
//            {
//                new StaticAsyncCommandContainer(new AsyncCommandItemTimeSpan()).Remove(id, commandType);
//            }
//        }
//    }
//}
