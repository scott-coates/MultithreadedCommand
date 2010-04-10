using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultithreadedCommand.Web.Models
{
    public class AsyncFuncViewModel
    {
        public string CommandDescription { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Id { get; set; }
        public bool UseActionLink { get; set; }
        public object RouteValues { get; set; }
        public string CommandTypeAssemblyQualifiedName
        {
            get
            {
                if (FuncType == null)
                {
                    throw new Exception("FuncType must be set.");
                }
                else
                {
                    return FuncType.AssemblyQualifiedName;
                }
            }
        }
        public Type FuncType { get; set; }
        public bool IsCancellable { get; set; }
        public string OnSuccess { get; set; } //Invoked when user clicks 'ok'
        public string OnFinish { get; set; } //invoked immediately after finishing.  before user clicks ok
        public string OnError { get; set; } //Invoked when an error occurs
        public string OnCancelled { get; set; } //Invoked when user cancels operation
    }
}