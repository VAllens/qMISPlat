//全局传入参数start
//表单唯一标识符
//启动流程时传入，办理流程时不传入
var CPFlowGlobal_FlowId = $.CPGetQuery("FlowId"); if (CPFlowGlobal_FlowId == null || CPFlowGlobal_FlowId == undefined) CPFlowGlobal_FlowId = "";
//是否导出到Excel
//启动流程时不传入，办理流程时传入
var CPFlowGlobal_InsId = $.CPGetQuery("InsId"); if (CPFlowGlobal_InsId == null || CPFlowGlobal_InsId == undefined) CPFlowGlobal_InsId = "";
//启动流程时不传入，办理流程时传入
var CPFlowGlobal_TaskId = $.CPGetQuery("TaskId"); if (CPFlowGlobal_TaskId == null || CPFlowGlobal_TaskId == undefined) CPFlowGlobal_TaskId = "";
//全局传入参数end
//全局参数
//记录全局流程配置信息
var CPFlowGlobal_FlowInfo;
//记录当前显示的表单对象
var CPFlowGlobal_CurFormConfig;
//记录当前点击按钮 类型
var CPFlowGlobal_ButtonClickType = "1";//1:提交  2：回退   3：保存表单按钮 
//JavaScript代码区域
layui.use('element', function () {
    var element = layui.element;

});
//设置按钮权限start
function CPFlowSetFunctionRight()
{    
    if (CPFlowGlobal_InsId != "" && CPFlowGlobal_TaskId == "")
    {
        //流程查看状态
        $("#btnSubmitFlow").hide();
        $("#btnSaveForm").hide();
    }
    else if (CPFlowGlobal_InsId == "" && CPFlowGlobal_TaskId == "" && CPFlowGlobal_FlowId !="")
    {
        //发起流程时
        $("#btnSaveForm").hide();
        $("#btnSubmitFlow").show();
    }
    else {
        $("#btnSubmitFlow").show();
        $("#btnSaveForm").show();
    }
    //回退按钮 
    if (CPFlowGlobal_FlowInfo.TaskCanFallbackCount > 0) {
        $("#btnFallbackFlow").show();
    }
    //取回按钮 
    if (CPFlowGlobal_FlowInfo.CheckUserCanTakeBackFlow) {
        $("#btnTakeBackFlow").show();
    }
    //如果按钮区域没有一个按钮，则不显示这个区域
    var bShow = false;
    $.each($("#CPFlowButtonContainer").children("button"), function (nIndex, nObj) {
        if ($(nObj).css("display") != "none") {
            bShow = true;
            return;
        }
    });
    if (bShow)
        $("#CPFlowHeaderContainer").show();
}
function CPFlowSetContainerHeight()
{
    if ($("#CPFlowHeaderContainer").css("display") != "none") {
        $("#CPFlowFormFrame").height($(window).height() - $("#CPFlowHeaderContainer").height() -
            $(".layui-tab-title").height() - 34);
    }
    else {
        $("#CPFlowFormFrame").height($(window).height() -
            $(".layui-tab-title").height() - 34);
    }
    $("#CPFlowFormFrame").width($(window).width() - 3);
    $("#CPFlowLogFrame").height($("#CPFlowFormFrame").height());
    $("#CPFlowLogFrame").width($("#CPFlowFormFrame").width());
}
//设置按钮 权限end
$(function () {
    //提交流程
    $("#btnSubmitFlow").click(function () {
        CPFlowSubmitFlow();
    });
    //保存表单
    $("#btnSaveForm").click(function () {
        CPFlowSaveForm();
    });
    //回退流程
    $("#btnFallbackFlow").click(function () {
        CPFlowFallbackFlow();
    });
    //取回流程
    $("#btnTakeBackFlow").click(function () {
        TakeBackFlow();
    });

    var GetFlowInfo = CPWebRootPath + "/api/CPFlowEngine/GetFlowInfo";
    GetFlowInfo += "?FlowId=" + CPFlowGlobal_FlowId + "&InsId=" + CPFlowGlobal_InsId + "&TaskId=" + CPFlowGlobal_TaskId + "&CurUserId=" + CPCurUserId;
    GetFlowInfo += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(GetFlowInfo, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        else {
            CPFlowGlobal_FlowInfo = data;
            console.log(CPFlowGlobal_FlowInfo);
            CPFlowSetFunctionRight();
            CPFlowSetContainerHeight();
            if (CPFlowGlobal_InsId == "")
            {
                $("#spanInsTitleIcon").show();
                $("#spanInsTitle").html(CPFlowGlobal_FlowInfo.Flow.FlowName);
                
            }
            else {
                //$("#spanInsTitle").html(CPFlowGlobal_FlowInfo.InsTitle);
                $("#spanInsTitle").hide();
                $("#spanInsTitleIcon").hide();
            }
            var sHtml = "";
            var defaultUrl = "";
            $.each(CPFlowGlobal_FlowInfo.CurPhase.FormCol, function (nIndex, nFormObj) {              
                if (nFormObj.IsMainForm)
                {
                    sHtml += "<li class=\"layui-this\">" + nFormObj.FormTitle + "</li>";
                    var url = nFormObj.FormPageUrl;
                    url += "&FormUseInCPFlow=true";
                    if (nFormObj.FormInitGroupCode != null && nFormObj.FormInitGroupCode != "") {
                        url += "&InitGroupCode=" + nFormObj.FormInitGroupCode;
                    }
                    url = CPUrlAddWebRootPath(url);
                    defaultUrl = url;
                    CPFlowGlobal_CurFormConfig = nFormObj;
                }
                else {
                    sHtml += "<li >" + nFormObj.FormTitle + "</li>";
                }
            });
            sHtml += "<li>流程</li>";
            $("#CPFlowFormFrame").attr("src", defaultUrl);
            $(".layui-tab-title").html(sHtml);
            layui.use('element', function () {
                var element = layui.element;
                //一些事件监听
                element.on('tab(cpFlowTabBrief)', function (dataTab) {                   
                    if ($("#CPFlowLogFrame").is(":hidden")){
                        if ($("#CPFlowLogFrame").attr("src") != "")
                        {
                            $("#CPFlowLogFrame").show();
                            $("#CPFlowFormFrame").hide();
                            return;
                        }
                    }
                    var url = "";     
                    var bExists = false;
                    var tmpPreCPFlowGlobal_CurFormConfig = CPFlowGlobal_CurFormConfig;
                    $.each(CPFlowGlobal_FlowInfo.CurPhase.FormCol, function (nIndex, nFormObj) {
                        if (nIndex == dataTab.index) {
                             url = nFormObj.FormPageUrl;
                            url += "&FormUseInCPFlow=true";
                            if (nFormObj.FormInitGroupCode != null && nFormObj.FormInitGroupCode != "") {
                                url += "&InitGroupCode=" + nFormObj.FormInitGroupCode;
                            }
                            if (nFormObj.FormRightGroupCode != null && nFormObj.FormRightGroupCode != "") {
                                url += "&RightGroupCode=" + nFormObj.FormRightGroupCode;
                            }
                            url = CPUrlAddWebRootPath(url);
                            CPFlowGlobal_CurFormConfig = nFormObj;
                            bExists = true;
                            return;
                        }
                    });
                    if (bExists) {      
                        if ($("#CPFlowLogFrame").is(":visible")) {
                            if (tmpPreCPFlowGlobal_CurFormConfig != null)
                            {
                                if (tmpPreCPFlowGlobal_CurFormConfig.Id == CPFlowGlobal_CurFormConfig.Id)
                                {
                                    $("#CPFlowFormFrame").show();
                                    $("#CPFlowLogFrame").hide();
                                    return;
                                }                                
                            }                           
                        }
                        $("#CPFlowFormFrame").attr("src", url);
                        $("#CPFlowFormFrame").show();
                        $("#CPFlowLogFrame").hide();
                    }
                    else
                    {
                        //点击流程标签
                        url = "/Plat/Flow/FlowMonitor?InsId=" + CPFlowGlobal_InsId + "&FlowId=" + CPFlowGlobal_FlowId;
                        url = CPUrlAddWebRootPath(url);
                        $("#CPFlowLogFrame").attr("src", url);
                        $("#CPFlowLogFrame").show();
                        $("#CPFlowFormFrame").hide();
                    }
                });
            });
        }
    });

  
});
//表单保存成功后回调方法start
function CPFlowAfterFormSaveSuccess(formPK, insTitle)
{
   // CPFlowGlobal_ButtonClickType = "1";//1:提交  2：回退   3：保存表单按钮 
    if (CPFlowGlobal_ButtonClickType == "3") {
        //点击保存表单按钮 
        CPFlowSaveFormEx(formPK);
    }
    else if (CPFlowGlobal_ButtonClickType == "1") {
        //点击提交按钮 
        if (CPFlowGlobal_InsId == "") {
            //发起流程
            if (insTitle == "") {
                insTitle = CPFlowGlobal_FlowInfo.InsTitle;
            }
            CPFlowStartFlowEx(formPK, insTitle);
        }
        else {
            CPFlowSubmitFlowEx();
        }
    }
    else if (CPFlowGlobal_ButtonClickType == "2") {
        //点击驳回
        CPFlowFallbackFlowEx();
    }
    CPFlowGlobal_ButtonClickType = "1";
}
//表单保存成功后回调方法end
//保存成功或检验失败后方法start
function CPFlowAfterFormSaveError()
{
    CPFlowGlobal_ButtonClickType = "1";
}
//保存成功或检验失败后方法end
//发起流程start
function CPFlowStartFlow()
{
    //先调用表单保存方法
    document.getElementById("CPFlowFormFrame").contentWindow.CPFormSaveFormData();   
}
//发起流程表单保存后调用方法
function CPFlowStartFlowEx(formPK,insTitle)
{
    var inputObj = new Object();
    inputObj.CurUserId = CPCurUserId;
    inputObj.CurUserIden = CPCurUserIden;
    inputObj.FlowVerId = CPFlowGlobal_FlowInfo.Flow.FlowVerId;
    inputObj.InsTitle = insTitle;
    inputObj.MainFormPK = formPK;
    var startFlowUrl = CPWebRootPath + "/api/CPFlowEngine/StartFlow";

    $.ajax({
        type: "POST",
        url: startFlowUrl,
        data: JSON.stringify(inputObj),
        contentType: 'application/json',
        success: function (data) {
            if (data.Result == false) {
                alert("启动失败，详细信息：" + data.ErrorMsg);
                return false;
            }
            else {
                //   alert("启动成功!");
                CPFlowGlobal_InsId = data.InsId;
                CPFlowGlobal_TaskId = data.TaskId;
                CPFlowSubmitFlow();
                //parent.CloseNewModel();
            }
        }
    });
}
//发起流程end
//直接提交流程，无需转到下一个提交界面start
function CPFlowSubmitFlowDirect()
{
    var inputObj = new Object();
    inputObj.CurUserId = CPCurUserId;
    inputObj.CurUserIden = CPCurUserIden;
    inputObj.InsId = CPFlowGlobal_InsId;
    inputObj.TaskId = CPFlowGlobal_TaskId;
    var submitFlowUrl = CPWebRootPath + "/api/CPFlowEngine/SubmitFlow";

    $.ajax({
        type: "POST",
        url: submitFlowUrl,
        data: JSON.stringify(inputObj),
        contentType: 'application/json',
        success: function (data) {
            if (data.Result == false) {
                alert("操作失败，详细信息：" + data.ErrorMsg);
                return false;
            }
            else {
                var alertS = "操作成功!";
                if (data.NewTaskUserNames != "")
                {
                    alertS += "流程已转交用户【" + data.NewTaskUserNames + "】";
                }
                alert(alertS);
                parent.CloseNewModel();
            }
        }
    });
}
//直接提交流程，无需转到下一个提交界面end

//转向下一个提交界面start
function CPFlowSubmitFlowToNext()
{
    var url = "/Plat/Flow/FlowSubmit?TaskId=" + CPFlowGlobal_TaskId + "&InsId=" + CPFlowGlobal_InsId;
    OpenNewModel(url, "提交流程", 850, 650);
}
//转向下一个提交界面end

//提交流程start
function CPFlowSubmitFlow()
{
    CPFlowGlobal_ButtonClickType = "1";//1:提交  2：回退   3：保存表单按钮 
    if (CPFlowGlobal_InsId == "")
    {
        CPFlowStartFlow();
        return;
    }
    else {       
        document.getElementById("CPFlowFormFrame").contentWindow.CPFormSaveFormData();   
    }
}
//提交流程表单保存后调用方法
function CPFlowSubmitFlowEx()
{
    var CheckTaskIsCanDirectSubmit = CPWebRootPath + "/api/CPFlowEngine/CheckTaskIsCanDirectSubmit";
    CheckTaskIsCanDirectSubmit += "?TaskId=" + CPFlowGlobal_TaskId + "&CurUserId=" + CPCurUserId;
    CheckTaskIsCanDirectSubmit += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(CheckTaskIsCanDirectSubmit, function (data) {
        if (data.Result == false) {
            alert("检测是否需要转向提交界面时出错，错误信息如下：" + data.ErrorMsg);
            return false;
        }
        else {
            if (data.IsNeedFallback) {
                //回退
                CPFlowFallbackFlowDirect();
            }
            else {
                if (data.IsCanDirectSubmit) {
                    //直接提交
                    CPFlowSubmitFlowDirect();
                }
                else {
                    //转向下一个界面
                    CPFlowSubmitFlowToNext();
                }
            }
        }
    });
}
//提交流程end

//保存表单方法start
function CPFlowSaveForm()
{
    if (CPFlowGlobal_InsId == "") {
        //发起流程
        alert("流程未发起，不允许保存表单！");
        return false;
    }
    else {
        CPFlowGlobal_ButtonClickType = "3";//1:提交  2：回退   3：保存表单按钮 
        document.getElementById("CPFlowFormFrame").contentWindow.CPFormSaveFormData();
       
    }
}
//表单保存成功后调用 方法
function CPFlowSaveFormEx(formPK)
{
    //CPFlowGlobal_CurFormConfig
    var curUrl = window.location.href;
    if (curUrl.toLowerCase().indexOf("pkvalues=") != -1) {
        alert("保存成功！");
        return;
    }
    var SaveFlowFormInfo = CPWebRootPath + "/api/CPFlowEngine/SaveFlowFormInfo";
    SaveFlowFormInfo += "?InsId=" + CPFlowGlobal_InsId + "&FormCode=" + CPFlowGlobal_CurFormConfig.FormCode+ "&FormPK=" + formPK + "&CurUserId=" + CPCurUserId;
    SaveFlowFormInfo += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(SaveFlowFormInfo, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        else {
            $.each(CPFlowGlobal_FlowInfo.CurPhase.FormCol, function (nIndex, nFormObj) {
                if (nFormObj.FormId == CPFlowGlobal_CurFormConfig.FormId)
                {
                    var url = nFormObj.FormPageUrl;
                    if (url.toLowerCase().indexOf("pkvalues=") == -1)
                    {
                        url += "&PKValues=" + formPK;
                    }
                    nFormObj.FormPageUrl = url;
                } 
            });
            alert("保存成功！");
        }
    });
}
//保存表单方法end


//回退流程start
function CPFlowFallbackFlowDirect()
{
    var FallbackFlow = CPWebRootPath + "/api/CPFlowEngine/FallbackFlow";
    FallbackFlow += "?InsId=" + CPFlowGlobal_InsId + "&TaskId=" + CPFlowGlobal_TaskId + "&CurUserId=" + CPCurUserId;
    FallbackFlow += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(FallbackFlow, function (data) {
        if (data.Result == false) {
            alert("驳回流程时出错，错误信息如下：" + data.ErrorMsg);
            return false;
        }
        else {
            var alertS = "操作成功!";
            if (data.NewTaskUserNames != "") {
                alertS += "流程已驳回给用户【" + data.NewTaskUserNames + "】";
            }
            alert(alertS);
            parent.CloseNewModel();
        }
    });
}
//转向回退选择界面
function CPFlowFallbackFlowToNext()
{
    var url = "/Plat/Flow/FlowFallback?TaskId=" + CPFlowGlobal_TaskId + "&InsId=" + CPFlowGlobal_InsId;
    OpenNewModel(url, "驳回流程", 550, 450);
}
//点击驳回，表单保存成功后调用 方法
function CPFlowFallbackFlowEx()
{
    if (CPFlowGlobal_FlowInfo.TaskCanFallbackCount > 1)
    {
        CPFlowFallbackFlowToNext();
    }
    else {
        CPFlowFallbackFlowDirect();
    }
}
function CPFlowFallbackFlow()
{
    CPFlowGlobal_ButtonClickType = "2";//1:提交  2：回退   3：保存表单按钮 
    document.getElementById("CPFlowFormFrame").contentWindow.CPFormSaveFormData();   
    
}
//回退流程end

//取回流程start
function TakeBackFlow() {
    if (confirm("您确实要取回流程吗？")) {
        var TakeBackFlow = CPWebRootPath + "/api/CPFlowEngine/TakeBackFlow";
        TakeBackFlow += "?InsId=" + CPFlowGlobal_InsId + "&CurUserId=" + CPCurUserId;
        TakeBackFlow += "&CurUserIden=" + CPCurUserIden;
        $.getJSON(TakeBackFlow, function (data) {
            if (data.Result == false) {
                alert("取回流程时出错，错误信息如下：" + data.ErrorMsg);
                return false;
            }
            else {
                var alertS = "操作成功，流程已取回!";
                alert(alertS);
                parent.CloseNewModel();
            }
        });
    }
}
//取回流程end