//全局传入参数start
function CPGetQuery(name)
{
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");//不区分大小写 
    var r = window.location.search.substr(1).match(reg);
    if (r != null)
        return unescape(r[2]);
    return null;
}
//表单唯一标识符
var CPFlowGlobal_InsId = CPGetQuery("InsId"); if (CPFlowGlobal_InsId == null || CPFlowGlobal_InsId == undefined) CPFlowGlobal_InsId = "";
var CPFlowGlobal_FlowId = CPGetQuery("FlowId"); if (CPFlowGlobal_FlowId == null || CPFlowGlobal_FlowId == undefined) CPFlowGlobal_FlowId = "";
var CPFlowGlobal_MonitorData = null;
//JavaScript代码区域
layui.use('element', function () {
    var element = layui.element;

});
layui.use('table', function () {
});
//阶段操作start
function PhaseDetailConfig()
{
    $("#PhaseEditor").hide();
    layer.closeAll();
} 
var CPFlowGlobal_CurPhaseText;
var CPFlowGlobal_CurPhaseConfig; 
var CPFlowGlobal_CurPhaseId;
var CPFlowGlobal_CurPhaseOldType;
var CPFlowGlobal_CurPhaseOldName;
function PhaseOndblclick(curPhase, curPhaseId, curPhasebbox, curPhaseText,
    curPhaseRect, curPhaseName, curPhaseImg
    , curPhaseConfig 
)
{
    
    CPFlowGlobal_CurPhaseText = curPhaseText;
    CPFlowGlobal_CurPhaseConfig = curPhaseConfig;
    CPFlowGlobal_CurPhaseId = curPhaseId;
    var sHTML = "";
    $.each(CPFlowGlobal_MonitorData.LogCol, function (nIndex, nObj) {
        if (Number(CPFlowGlobal_CurPhaseConfig.props.temp1.value) == nObj.PhaseId)
        {
            sHTML += " <tr>";
            sHTML += " <td ><span title='" + nObj.SubmitPhaseNames + "'>" + nObj.SubmitPhaseNames + "</span></td>";
            sHTML += " <td>" + nObj.RevTime + "</td>";
            sHTML += " <td><span title='" + nObj.SubmitUserNames + "'>" + nObj.SubmitUserNames + "</span></td>";
            sHTML += " <td>" + nObj.RevUserName + "</td>";
            sHTML += " <td>" + nObj.TaskState + "</td>";
            sHTML += " <td>" + nObj.TaskManaTime + "</td> ";
            sHTML += " </tr> ";
        }
    });
    if (sHTML == "")
        return;
    $("#PhaseLogListContext").html(sHTML);
    var nLeft = $(window).width() / 2;
    var nTop = $(window).height() / 2;
    var table = layui.table;
    table.init('parse-table-demo', { //转化静态表格
        //height: 'full-500'
    }); 
    layer.open({
        type: 1,
        title: curPhaseConfig.text.text,
        maxmin: false,
        shadeClose: false, //不允许点击外面关闭
        offset: [curPhasebbox.y.toString() + 'px', curPhasebbox.x.toString() + 'px'],//	同时定义top、left坐标
        area: ["768px", "300px"],
        content: $("#PhaseEditor"),//[url, 'no']
        cancel: function (index, layero) {

        }
    });
}
//阶段操作end
 
 
function InitFlowDesign(flowJsonObj) {

    //$(function () {
    //    $('#myflow')
    //        .myflow($.extend(true, {
    //            basePath: "",
    //            restore: eval("({states:{rect4:{type:'start',text:{text:'开始'}, attr:{ x:409, y:10, width:50, height:50}, props:{text:{value:'开始'},temp1:{value:''},temp2:{value:''}}},rect5:{type:'task',text:{text:'任务1'}, attr:{ x:386, y:116, width:100, height:50}, props:{text:{value:'任务1'},assignee:{value:''},form:{value:''},desc:{value:''}}},rect6:{type:'fork',text:{text:'分支'}, attr:{ x:410, y:209, width:50, height:50}, props:{text:{value:'分支'},temp1:{value:''},temp2:{value:''}}},rect7:{type:'task',text:{text:'任务2'}, attr:{ x:192, y:317, width:100, height:50}, props:{text:{value:'任务2'},assignee:{value:''},form:{value:''},desc:{value:''}}},rect8:{type:'task',text:{text:'任务3'}, attr:{ x:385, y:317, width:100, height:50}, props:{text:{value:'任务3'},assignee:{value:''},form:{value:''},desc:{value:''}}},rect9:{type:'task',text:{text:'任务4'}, attr:{ x:556, y:320, width:100, height:50}, props:{text:{value:'任务4'},assignee:{value:''},form:{value:''},desc:{value:''}}},rect10:{type:'join',text:{text:'合并'}, attr:{ x:410, y:416, width:50, height:50}, props:{text:{value:'合并'},temp1:{value:''},temp2:{value:''}}},rect11:{type:'end',text:{text:'结束'}, attr:{ x:409, y:633, width:50, height:50}, props:{text:{value:'结束'},temp1:{value:''},temp2:{value:''}}},rect12:{type:'task',text:{text:'任务5'}, attr:{ x:384, y:528, width:100, height:50}, props:{text:{value:'任务5'},assignee:{value:''},form:{value:''},desc:{value:''}}}},paths:{path13:{from:'rect4',to:'rect5', dots:[],text:{text:'TO 任务1'},textPos:{x:37,y:-4}, props:{text:{value:''}}},path14:{from:'rect5',to:'rect6', dots:[],text:{text:'TO 分支'},textPos:{x:56,y:-1}, props:{text:{value:''}}},path15:{from:'rect6',to:'rect8', dots:[],text:{text:'TO 任务3'},textPos:{x:24,y:-5}, props:{text:{value:''}}},path16:{from:'rect8',to:'rect10', dots:[],text:{text:'TO 合并'},textPos:{x:41,y:8}, props:{text:{value:''}}},path17:{from:'rect10',to:'rect12', dots:[],text:{text:'TO 任务5'},textPos:{x:36,y:-5}, props:{text:{value:''}}},path18:{from:'rect12',to:'rect11', dots:[],text:{text:'TO 结束'},textPos:{x:32,y:0}, props:{text:{value:''}}},path19:{from:'rect6',to:'rect7', dots:[{x:244,y:232}],text:{text:'TO 任务2'},textPos:{x:0,y:-10}, props:{text:{value:'TO 任务2'}}},path20:{from:'rect7',to:'rect10', dots:[{x:242,y:435}],text:{text:'TO 合并'},textPos:{x:-3,y:17}, props:{text:{value:'TO 合并'}}},path21:{from:'rect6',to:'rect9', dots:[{x:607,y:234}],text:{text:'TO 任务4'},textPos:{x:0,y:-10}, props:{text:{value:'TO 任务4'}}},path22:{from:'rect9',to:'rect10', dots:[{x:607,y:439}],text:{text:'TO 合并'},textPos:{x:-8,y:16}, props:{text:{value:'TO 合并'}}}},props:{props:{name:{value:'新建流程'},key:{value:''},desc:{value:''}}}})"),
    //            editable: false

    //        }, {
    //                "activeRects": {
    //                    "rects": [{ "paths": [], "name": "任务3" },
    //                    { "paths": [], "name": "任务4" },
    //                    { "paths": [], "name": "任务2" }]
    //                }, "historyRects": {
    //                    "rects":
    //                    [{ "paths": ["TO 任务1"], "name": "开始" },
    //                    { "paths": ["TO 分支"], "name": "任务1" },
    //                    { "paths": ["TO 任务3", "TO 任务4", "TO 任务2"], "name": "分支" }]
    //                }
    //            })
    //        );
    //});
    $('#myflow').html("");
    //start
    $('#myflow')
        .myflow(
        $.extend(true, {
            basePath: "",
            restore: flowJsonObj,//eval("({states:{rect1:{type:'start',text:{text:'开始'}, attr:{ x:507, y:63, width:50, height:50}, props:{text:{value:'开始'},temp1:{value:''},temp2:{value:''}}},rect2:{type:'state',text:{text:'状态2'}, attr:{ x:489, y:286, width:100, height:50}, props:{text:{value:'状态2'},temp1:{value:''},temp2:{value:''}}},rect3:{type:'task',text:{text:'任务1'}, attr:{ x:311, y:284, width:100, height:50}, props:{text:{value:'任务1'},assignee:{value:''},form:{value:''},desc:{value:''},temp1:{value:''},temp2:{value:''}}},rect4:{type:'task',text:{text:'任务3'}, attr:{ x:675, y:287, width:100, height:50}, props:{text:{value:'任务3'},assignee:{value:''},form:{value:''},desc:{value:''},temp1:{value:''},temp2:{value:''}}},rect5:{type:'fork',text:{text:'分支'}, attr:{ x:509, y:176, width:50, height:50}, props:{text:{value:'分支'},temp1:{value:''},temp2:{value:''}}},rect6:{type:'join',text:{text:'合并'}, attr:{ x:517, y:398, width:50, height:50}, props:{text:{value:'合并'},temp1:{value:''},temp2:{value:''}}},rect7:{type:'end',text:{text:'结束'}, attr:{ x:519, y:517, width:50, height:50}, props:{text:{value:'结束'},temp1:{value:''},temp2:{value:''}}}},paths:{path8:{from:'rect1',to:'rect5', dots:[],text:{text:'TO 分支',textPos:{x:55,y:-8}}, props:{text:{value:'TO 分支'}}},path9:{from:'rect5',to:'rect2', dots:[],text:{text:'TO 状态2',textPos:{x:28,y:-4}}, props:{text:{value:'TO 状态2'}}},path10:{from:'rect2',to:'rect6', dots:[],text:{text:'TO 合并',textPos:{x:25,y:-10}}, props:{text:{value:'TO 合并'}}},path11:{from:'rect6',to:'rect7', dots:[],text:{text:'TO 结束ddd',textPos:{x:31,y:-10}}, props:{text:{value:''}}},path12:{from:'rect5',to:'rect3', dots:[],text:{text:'TO 任务1',textPos:{x:-21,y:-10}}, props:{text:{value:'TO 任务1'}}},path13:{from:'rect3',to:'rect6', dots:[],text:{text:'TO 合并',textPos:{x:-34,y:21}}, props:{text:{value:'TO 合并'}}},path14:{from:'rect5',to:'rect4', dots:[],text:{text:'TO 任务3',textPos:{x:55,y:-17}}, props:{text:{value:'TO 任务3'}}},path15:{from:'rect4',to:'rect6', dots:[],text:{text:'TO 合并',textPos:{x:30,y:18}}, props:{text:{value:'TO 合并'}}}},props:{props:{name:{value:'新建流程'},key:{value:''},desc:{value:''}}}})"),
            editable: false//把默认修改属性的功能给关闭了

        })
        );
    //end
}
function InitFlowDesignJSONFromServer()
{
    var getUrl = CPWebRootPath + "/api/CPFlowEngine/GetFlowInfoForMonitor?FlowId=" + CPFlowGlobal_FlowId;
    getUrl += "&InsId=" + CPFlowGlobal_InsId;
    getUrl += "&CurUserId=" + CPCurUserId;
    getUrl += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(getUrl, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        CPFlowGlobal_MonitorData = data;
        if (data.FlowVerState == 0) {
            $("#spanFlowVerState").html("未发布");
            CPFlowGlobal_IsRelease = false;
        }
        else if (data.FlowVerState == 1)
        {
            CPFlowGlobal_IsRelease = true;
            $("#spanFlowVerState").html("已发布");
        }
        $("#FlowName").val(data.FlowName);
        InitFlowDesign(data.FlowDesigner);

    });
}
$(function () { 
    InitFlowDesignJSONFromServer(); 
});
//初始化流程图end