<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Index
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Index</h2>
    <ul>
        <% var values = Enumerable.Range(1, 501).Where(i => i % 10 == 0); %>
        <% foreach (var value in values)
           {%>
        <li>
            <%= Html.ActionLink(value.ToString(), "LongRunningJob", new { id = value })%>
        </li>
        <%} %>
    </ul>
</asp:Content>
