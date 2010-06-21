<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%= Html.Encode(ViewData["Message"]) %></h2>
    <p>
        To learn more about ASP.NET MVC visit <a href="http://asp.net/mvc" title="ASP.NET MVC Website">
            http://asp.net/mvc</a>.
    </p>
    <ul>
        <li>
            <%= Html.ActionLink("Reports", "Index", "ReportEmailer")%></li>
        <li>
            <%= Html.ActionLink("Long Running", "Index", "LongRunner")%></li>
        <li>Failure </li>
        <li>Never Finishes </li>
    </ul>
</asp:Content>
