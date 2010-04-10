<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MultithreadedCommand.Web.Models.ReportEmail>" %>

<%@ Import Namespace="MultithreadedCommand.Web.Models" %>
<%@ Import Namespace="MultithreadedCommand.Web.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Index
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Index</h2>
    <script src="../../Scripts/MicrosoftAjax.js" type="text/javascript"></script>
    <script src="../../Scripts/MicrosoftMvcValidation.js" type="text/javascript"></script>
    <%Html.EnableClientValidation(); %>
    <% using (Html.BeginForm("CreateReport","ReportEmailer")) 
    //<% using (Ajax.BeginForm("CreateReport", new AjaxOptions { OnBegin = "RunCommand", OnComplete = "GetStatus" }))
    { %>
    <fieldset>
        <legend>Fields</legend>
        <div class="editor-label">
            <%= Html.LabelFor(model => model.RecipientEmail)%>
        </div>
        <div class="editor-field">
            <%= Html.TextBoxFor(model => model.RecipientEmail)%>
            <%= Html.ValidationMessageFor(model => model.RecipientEmail)%>
        </div>
        <div class="editor-label">
            <%= Html.LabelFor(model => model.FromEmail)%>
        </div>
        <div class="editor-field">
            <%= Html.TextBoxFor(model => model.FromEmail)%>
            <%= Html.ValidationMessageFor(model => model.FromEmail)%>
        </div>
        <div class="editor-label">
            <%= Html.LabelFor(model => model.ReportName)%>
        </div>
        <div class="editor-field">
            <%= Html.TextBoxFor(model => model.ReportName)%>
            <%= Html.ValidationMessageFor(model => model.ReportName)%>
        </div>
    </fieldset>
    <div>
        <% 
        string id = Guid.NewGuid().ToString();
        var viewModel = new AsyncFuncViewModel
        {
            Id = id,
            UseActionLink = false,
            //Action = "CreateReport",
            //Controller = "ReportEmailer",
            CommandDescription = "Create Report",
            FuncType = typeof(ReportEmailer),
            IsCancellable = true
        };
        Html.RenderPartial("AsyncFunc", viewModel);
        %>
    </div>
    <% } %>
</asp:Content>
