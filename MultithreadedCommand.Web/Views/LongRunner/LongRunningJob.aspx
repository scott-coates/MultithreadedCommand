<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Import Namespace="MultithreadedCommand.Web.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    LongRunningJob
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script src="../../Scripts/MicrosoftAjax.js" type="text/javascript"></script>
    <% string number = ViewData["Number"].ToString(); %>
    <h2>
        Job #<%=number%></h2>
    <div>
        <% 
            var viewModel = new AsyncFuncViewModel
            {
                Id = number,
                RouteValues = new { id = number },
                UseActionLink = true,
                Action = "RunLongJob",
                Controller = "LongRunner",
                CommandDescription = "Run Long Job",
                FuncType = typeof(LongRunner),
                IsCancellable = true
            };
            Html.RenderPartial("AsyncFunc", viewModel);
        %>
    </div>
</asp:Content>
