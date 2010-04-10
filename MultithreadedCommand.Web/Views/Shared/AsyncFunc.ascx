<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MultithreadedCommand.Web.Models.AsyncFuncViewModel>" %>
<%@ Import Namespace="MultithreadedCommand.Core.Commands" %>
<script src="../../Scripts/MicrosoftMvcAjax.js" type="text/javascript"></script>
<script src="../../Scripts/jquery-1.3.2.js" type="text/javascript"></script>
<script src="../../Scripts/jquery-ui-1.7.2.min.js" type="text/javascript"></script>
<link href="../../Content/jquery-ui-1.7.2.custom.css" rel="stylesheet" type="text/css" />
<script type="text/javascript" language="javascript">
var isRunning = false;
    var total = 0;
    
    var isAnimating = false;
    var xpos = 0;
	var ypos = 220;
	
	function AnimateProgressBar() {
		xpos -= 2;
		ypos -= 1;
		$("#ProgressBar").css("background-position", xpos + "px "+ ypos + "px");
	}
	    
    $(function() {
        SetupAsyncForm();
        
        GetStatus();
    });
    
    function SetupAsyncForm()
    {
        <%if(!Model.UseActionLink)
        {%>
        var form = $("#asyncForm").parents("form:first");
        if(form.length > 0)
        {
            form.submit(function(event)
            {
                Sys.Mvc.AsyncForm.handleSubmit(form[0], new Sys.UI.DomEvent(event), 
                { 
                    insertionMode: Sys.Mvc.InsertionMode.replace, 
                    onBegin: Function.createDelegate(form[0], RunCommand), 
                    onComplete: Function.createDelegate(form[0], GetStatus) 
                }); 
            });

            form.click(function(event)
            {
                Sys.Mvc.AsyncForm.handleClick(form[0], new Sys.UI.DomEvent(event));
            });
        }
        else
        {
            alert("No parent form detected.");
        }
        <%} %>
    }
    
    function RunCommand()
    {
        isRunning = true;
        HideWorkResults();
        ShowMainProgress();
        AddCancelButton();
    }
    
    function AddCancelButton()
    {
        <%if(Model.IsCancellable){ %>
            $('.Progress').dialog('option', 'buttons', 
            { 
                "Cancel": function() {
                    if(confirm('Are you sure you want to cancel this process?'))
                    {
                        ShowCancelled();
                    }
                } 
            });
        <%} %>
    }
    
    function ShowCancelled()
    {
        StopRunning();
        EndLoopGetStatus();
        CancelCommand();
        $("#cancel").click();
    }
    
    function SetAnimateProgressBar()
    {
        if(!isAnimating)
        {
            setInterval("AnimateProgressBar()", 80);
            isAnimating = true;
        }
    }
    
    function ClearAnimateProgressBar()
    {
        window.clearTimeout("AnimateProgressBar()");
    }
    
    function ShowDialog()
    {
        $(".Progress").dialog(
            {
                draggable: false,
                resizable: false, 
                width: 630, 
                modal: true,
                closeOnEscape: false,
                open: function(event, ui) { $(".ui-dialog-titlebar-close").hide(); } //hide close button.
            });
            
        $(".Progress").dialog('open');
    }
    
    function CloseDialog()
    {
        $(".Progress").dialog('close');
    }
    
    function RestartCommand()
    {
        isRunning = true;
        HideWorkResults();
        ShowMainProgress();
    }
    
    function CancelCommand()
    {
        HideWorkResults();
        CloseDialog();
        ShowEmptyProgress();
    }
    
    function ShowEmptyProgress()
    {
        //if user clicks cancel and then restarts the command, this will null it all out.
        var data = new Object();
        data.PercentDone = 0;
        data.FinishedSoFar = 0;
        data.Total = 0;
        data.TimeRemaining = new Object();
        data.TimeRemaining.TotalMinutes = 0;
        data.TimeRemaining.Seconds = 0;
        data.Message = '';
        ShowStatus(data);
    }
    
    function StopRunning()
    {
        isRunning = false;
    }
    
    function ShowMainProgress()
    {
        SetAnimateProgressBar(); //this checks to make sure not animating before doing so.
        ShowDialog();
        $(".Progress").fadeIn('slow');
    }

    function LoopGetStatus()
    {
        window.setTimeout("GetStatus()", 1000);
    }
    
    function EndLoopGetStatus()
    {
        window.clearTimeout("GetStatus()");
    }


    function ShowWorkResults()
    {
        ShowMainProgress();
        $("#results").fadeIn('slow');
    }
    
    function HideWorkResults()
    {
        $("#results").children().hide();
        $("#results").hide();
    }
    
    function ShowFinished()
    {
        StopRunning();
        $("#finished").show();
        AddFinishedButtons();
        ShowWorkResults();
        var data = new Object();
        data.PercentDone = 100;
        data.FinishedSoFar = total;
        data.Total = total;
        data.TimeRemaining = new Object();
        data.TimeRemaining.TotalMinutes = 0;
        data.TimeRemaining.Seconds = 0;
        data.Message = '';
        ShowStatus(data);
        EndLoopGetStatus();
        OnFinish();
    }
    
    function AddFinishedButtons()
    {
        $('.Progress').dialog('option', 'buttons', 
        { 
            "OK": function() { 
                ShowEmptyProgress();
                $(this).dialog("close"); 
                $(this).dialog('option', 'buttons',{ }); 
                OnSuccess();
            } 
        });
    }

    function OnSuccess()
    {
        <%if(!string.IsNullOrEmpty(Model.OnSuccess))
        {%>
            eval('<%= Model.OnSuccess %>');
        <%} %>
    }

    function OnFinish()
    {
        <%if(!string.IsNullOrEmpty(Model.OnFinish))
        {%>
            eval('<%= Model.OnFinish %>');
        <%} %>
    }

    function ShowError(data)
    {
        StopRunning();
        $("#error").show();
        ShowWorkResults();
        ShowStatus(data);
        EndLoopGetStatus();
    }

    function ShowStatus(data)
    {
        var percentComplete = parseInt(data.PercentDone) + '%';
        $('#percent').text(percentComplete);
        $('#numbersProcessed').text('Processed ' + data.FinishedSoFar.toString() + ' of ' + data.Total.toString());
        $('#ProgressBar').width(percentComplete);
        $('#message').text(data.Message);
        if (data.TimeRemaining == null) {
            $('#time').text('Time remaining: Unknown');
        }
        else 
        {
            var minutes = data.TimeRemaining.TotalMinutes >= 1 ? parseInt(data.TimeRemaining.TotalMinutes).toString() + ' minute(s) ' : '';
            $('#time').text('Time remaining: ' + minutes + data.TimeRemaining.Seconds.toString() + ' seconds');
        }
    }
    
    function GetStatus() {
        var id = '<%= Model.Id %>';
        var funcName = '<%=Model.CommandTypeAssemblyQualifiedName %>';
        var url = '/AsyncManager/GetCurrentProgress/' + id + '/?funcName=' + funcName;
        $.getJSON(url,  
            function (data)
            {      
                if(data.Status == <%=(int)StatusEnum.NotStarted %>)
                {
                    if(isRunning)
                    {
                        ShowFinished();
                    }
                }
                else if(data.Status == <%=(int)StatusEnum.Cancelled %>)
                {
                    ShowCancelled();
                }
                else if (data.Status == <%=(int)StatusEnum.Finished %>)
                {
                    ShowFinished();
                }
                else if (data.Status == <%=(int)StatusEnum.Error%>)
                {
                    ShowError(data);
                }
                else if(data.Status == <%=(int)StatusEnum.Running%>)
                {
                    if(!isRunning)
                    {
                        //show if first time. if they refresh page.
                        RunCommand();
                    }
                
                    total = data.Total; //store this as it might increase during the process.  Number is needed at the end of operation for display purposes.
                    ShowStatus(data);
                    LoopGetStatus();
                }
            });
    }
</script>
<div class="Progress" title="Perform Command" style="display: none;">
    <div id="percent" class="PercentComplete">
        &nbsp;
    </div>
    <div id="time" class="TimeRemaining">
        &nbsp;
    </div>
    <div id="numbersProcessed" class="CountComplete">
        &nbsp;
    </div>
    <div id="ProgressOuterBox">
        <div id="ProgressOuterBar">
            <div id="ProgressBar">
            </div>
        </div>
    </div>
    <div id="message">
    </div>
    <div id="results" class="WorkResults" style="display: none;">
        <div id="finished" class="CommandFinished" style="display: none;">
            Finished
        </div>
        <div id="error" class="CommandError" style="display: none;">
            Error Occurred
            <div>
                Retry?
                <%= Ajax.ActionLink("Yes", "RestartProcess", "AsyncManager", new { Id = Model.Id, funcName = Model.CommandTypeAssemblyQualifiedName }, new AjaxOptions { OnBegin = "RestartCommand", OnComplete = "GetStatus" })%>
                <%= Ajax.ActionLink("No", "CancelProcess", "AsyncManager", new { Id = Model.Id, funcName = Model.CommandTypeAssemblyQualifiedName }, new AjaxOptions { OnBegin = "CancelCommand" }, new { id = "cancel" })%>
            </div>
        </div>
    </div>
</div>
<p id="PerformCommand">
    <%if (Model.UseActionLink)
      {
    %><%=Ajax.ActionLink(Model.CommandDescription, Model.Action, Model.Controller, Model.RouteValues, new AjaxOptions { OnBegin = "RunCommand", OnComplete = "GetStatus" })%>
    <%}
      else
      {%>
    <input type="hidden" name="id" value="<%=Model.Id %>" />
    <input type="submit" id="asyncForm" value="<%=Model.CommandDescription %>" />
    <%}
    %>
</p>
