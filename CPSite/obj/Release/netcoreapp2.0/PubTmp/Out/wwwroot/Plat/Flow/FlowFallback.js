//全局传入参数start
//表单唯一标识符
//是否导出到Excel
//办理流程时传入
var CPFlowGlobal_InsId = $.CPGetQuery("InsId"); if (CPFlowGlobal_InsId == null || CPFlowGlobal_InsId == undefined) CPFlowGlobal_InsId = "";
//办理流程时传入
var CPFlowGlobal_TaskId = $.CPGetQuery("TaskId"); if (CPFlowGlobal_TaskId == null || CPFlowGlobal_TaskId == undefined) CPFlowGlobal_TaskId = "";
//全局传入参数end

var CPFlowGlobal_PhaseCol;
$(function () {

    $("#divPhaseContainer").height($(window).height() - $("#divButtonContainer").height() - 5);
    var GetCanFallbackPhase = CPWebRootPath + "/api/CPFlowEngine/GetCanFallbackPhase";
    GetCanFallbackPhase += "?TaskId=" + CPFlowGlobal_TaskId + "&CurUserId=" + CPCurUserId;
    GetCanFallbackPhase += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(GetCanFallbackPhase, function (data) {
        if (data.Result == false) {
            alert("获取可驳回的阶段信息时出错，错误信息如下：" + data.ErrorMsg);
            return false;
        }
        CPFlowGlobal_PhaseCol = data.PhaseCol;
        var sHTML = "";
        var schecked = "";
        if (data.PhaseCol.length <= 1)
        {
            schecked = " checked='checked'";
        }        
        $.each(data.PhaseCol, function (nIndex, nObj) {
            var uIds = ""; var uNames = "&nbsp;";
            if (nObj.TaskRevUser != null)
            {
                $.each(nObj.TaskRevUser, function (uIndex, uObj) {
                    if (uIds == "")
                    {
                        uIds = uObj.RevUserId.toString();
                        uNames = uObj.RevUserName.toString();
                    }
                    else {
                        uIds += "," + uObj.RevUserId.toString();
                        uNames += "," + uObj.RevUserName.toString();
                    }
                });
            }
            sHTML += "<fieldset class='layui-elem-field layui-field-title' style='margin-top: 20px;'>";
            sHTML += "    <legend style= 'margin-left:0px;'> ";
            sHTML += " <input type='radio' id='CPPhaseChk_" + nObj.PhaseId + "' " + schecked + "  name='CPPhaseChk_Radio'  class='CPFlowRadioCss' /> <label for='CPPhaseChk_" + nObj.PhaseId + "' style='margin-right:6px; '></label><label for='CPPhaseChk_" + nObj.PhaseId + "'  style='cursor:pointer;'>" + nObj.PhaseName + "</label></legend > ";
            sHTML += " </fieldset>";
            sHTML += "    <div class='layui-row'>";
            sHTML += "        <div class='layui-col-xs12' style='padding-left:30px;' >";
            sHTML += "            <span id='SpanPhaseUserName_" + nObj.PhaseId + "'>" + uNames + "</span>";
            sHTML += "            <input type='hidden' id='HidPhaseUserId_" + nObj.PhaseId + "' value='" + uIds + "'></input>";
            sHTML += "        </div>";
            //sHTML += "        <div class='layui-col-xs1' style='text-align:center;' >";
            //sHTML += "           <i class='icon iconfont icon-zhucetianjiahaoyou' title='点击选择' onclick='PhaseSelUser(this);' data-PhaseId='" + nObj.PhaseId+ "'  style='cursor: pointer;font-size:26px;'></i>";
            //sHTML += "        </div>";
            sHTML += "   </div>";
        });
        $("#divPhaseContainer").html(sHTML);
    });
    $("#btnFallbackFlow").click(function () {
        FallbackFlow();
    });
}); 

//驳回流程start

function FallbackFlow()
{
    var isHasSel = false;
    var isHasEmptyUser = false;
    var inputPhaseCol = new Array();
    $.each(CPFlowGlobal_PhaseCol, function (nIndex, nObj) {
        if ($("#CPPhaseChk_" + nObj.PhaseId).is(':checked')) {
            isHasSel = true;
            if ($("#HidPhaseUserId_" + nObj.PhaseId).val() == "") {
                isHasEmptyUser = true;
            }
            var tmpObj = new Object();
            tmpObj.PhaseId = nObj.PhaseId;
            tmpObj.TaskRevUser = new Array();
            if ($("#HidPhaseUserId_" + nObj.PhaseId).val() != "") {
                var idArray = $("#HidPhaseUserId_" + nObj.PhaseId).val().split(',');
                var nameArray = $("#SpanPhaseUserName_" + nObj.PhaseId).html().split(',');
                for (var i = 0; i < idArray.length; i++) {
                    var tUser = new Object();
                    tUser.RevUserId = Number(idArray[i]);
                    tUser.RevUserName = nameArray[i];
                    tUser.RevSourceUserId = Number(idArray[i]);
                    tUser.RevSourceUserName = nameArray[i];
                    tmpObj.TaskRevUser.push(tUser);
                }
            }
            inputPhaseCol.push(tmpObj);
        }
    });
    if (isHasSel == false)
    {
        alert("请至少选择一个阶段！");
        return false;
    }
    if (isHasEmptyUser)
    {
        alert('请选择办理用户！');
        return false;
    }
    var FallbackFlow = CPWebRootPath + "/api/CPFlowEngine/FallbackFlowToAppointPhase";
    FallbackFlow += "?InsId=" + CPFlowGlobal_InsId + "&TaskId=" + CPFlowGlobal_TaskId + "&FallbackTargetPhaseId=" + inputPhaseCol[0].PhaseId + "&CurUserId=" + CPCurUserId;
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
            parent.parent.CloseNewModel();
        }
    });
}
//驳回流程end