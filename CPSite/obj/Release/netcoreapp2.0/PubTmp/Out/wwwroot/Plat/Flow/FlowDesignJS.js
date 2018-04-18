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
var CPFlowGlobal_FlowVerId = CPGetQuery("FlowVerId"); if (CPFlowGlobal_FlowVerId == null || CPFlowGlobal_FlowVerId == undefined) CPFlowGlobal_FlowVerId = "";
var CPFlowGlobal_FlowId = CPGetQuery("FlowId"); if (CPFlowGlobal_FlowId == null || CPFlowGlobal_FlowId == undefined) CPFlowGlobal_FlowId = "";
var CPFlowGlobal_IsRestoreVer = CPGetQuery("IsRestoreVer"); if (CPFlowGlobal_IsRestoreVer == null || CPFlowGlobal_IsRestoreVer == undefined) CPFlowGlobal_IsRestoreVer = "false";

//记录是否需要先点击保存
var CPFlowGlobal_IsNeedSave = false;
//记录流程配置信息
var CPFlowGlobal_DesignInfo;
//记录当前流程是否已发布
var CPFlowGlobal_IsRelease = false;

function CPFlowToolSet()
{
    if (CPFlowGlobal_IsRestoreVer == "true")
    {
        $("#myflow_BasicInfo").hide();
        $("#myflow_save").hide();
        $("#myflow_Release").hide();
        $("#myflow_CreateNewVer").hide();
        $("#myflow_RestoreVer").show();
    }
    else {

    }
    //设置宽高 
    $("#myflow").height($(window).height() - $("#FlowTop").height());
    $("#FlowRight").width(2854);
    $("#myflow").width($("#FlowRight").width());
    $("#FlowRight").height(1434);
    $("#myflow").height($("#FlowRight").height());
    $("#FlowContext").width($("#myflow_tools").width() + $("#myflow").width() + 5);
    $("#myflow_props").css("left", $(window).width() - $("#myflow_props").width() - 20);
    //$("#myflow_props").css("top", $(window).height() / 2);
    //设置宽高
    //工具条相关操作
    $("#pointer").click(function () {
        if ($("#pointer").hasClass("ToolRowCssSelected")) {
            $("#pointer").removeClass("ToolRowCssSelected");
        }
        else {
            $("#path").removeClass("ToolRowCssSelected");
            $("#pathfallback").removeClass("ToolRowCssSelected");
            $("#pointer").addClass("ToolRowCssSelected");
        }
    });
    $("#path").click(function () {
        if ($("#path").hasClass("ToolRowCssSelected")) {
            $("#path").removeClass("ToolRowCssSelected");
        }
        else {
            $("#pointer").removeClass("ToolRowCssSelected");
            $("#pathfallback").removeClass("ToolRowCssSelected");
            $("#path").addClass("ToolRowCssSelected");
        }
    });
    $("#pathfallback").click(function () {
        if ($("#pathfallback").hasClass("ToolRowCssSelected")) {
            $("#pathfallback").removeClass("ToolRowCssSelected");
        }
        else {
            $("#pointer").removeClass("ToolRowCssSelected");
            $("#path").removeClass("ToolRowCssSelected");
            $("#pathfallback").addClass("ToolRowCssSelected");
        }
    });
    $(".layui-layer-btn0").click(function () {
        $("#myflow_props").hide();
    });
    $(".layui-layer-close").click(function () {
        $("#myflow_props").hide();
    });
    //工具条相关操作
}
//阶段操作start
function PhaseDetailConfig()
{
    if (CPFlowGlobal_CurPhaseOldName != $("#CurPhaseName").val()) {
        CPFlowGlobal_IsNeedSave = true;
    }
    if (CPFlowGlobal_CurPhaseOldType != $("#CurPhaseType").val()) {
        CPFlowGlobal_IsNeedSave = true;
    }
    if (CPFlowGlobal_IsNeedSave == true) {
        alert("请先点击保存，再配置阶段详细信息！");
        return;
    }
    if ($("#CurPhaseType").val() == "5")
    {
        alert("节点类型为结束节点时，无需配置详细信息");
        return;
    }
    //配置详细阶段信息
    //console.log(CPFlowGlobal_CurPhaseConfig);
    //alert(CPFlowGlobal_CurPhaseConfig.props.temp1.value);
    var url = "/Plat/Tab/TabView?TabCode=Tab201711260633240008&PhaseId=" + CPFlowGlobal_CurPhaseConfig.props.temp1.value;
    top.OpenNewModel(url, "阶段详细信息", $(window).width() - 20, $(window).height() );
    PhaseEditorCancel();
}
function PhaseEditorCancel()
{
    $("#PhaseEditor").hide();
    layer.closeAll();
}
function PhaseEditorOK()
{
    CPFlowGlobal_CurPhaseText.attr({
        text: $("#CurPhaseName").val()
    }); 
    if (CPFlowGlobal_CurPhaseOldName != $("#CurPhaseName").val())
    {
        CPFlowGlobal_IsNeedSave = true;
    }
    //需要 判断下，如果没有改变过节点类型，则不需要 重新建立流程图
    if (CPFlowGlobal_CurPhaseOldType != $("#CurPhaseType").val()) {
        if ($("#CurPhaseType").val() == "1") {
            myFlow_AllStates[CPFlowGlobal_CurPhaseId].ChangePhaseType("image", "start");
        }
        else if ($("#CurPhaseType").val() == "2") {
            //普通节点
            myFlow_AllStates[CPFlowGlobal_CurPhaseId].ChangePhaseType("text", "task");
        }
        else if ($("#CurPhaseType").val() == "4") {
            //传阅节点
            myFlow_AllStates[CPFlowGlobal_CurPhaseId].ChangePhaseType("textcirculation", "state");
        }
        else if ($("#CurPhaseType").val() == "3") {
            //会签节点
            myFlow_AllStates[CPFlowGlobal_CurPhaseId].ChangePhaseType("image&text", "fork");
        }
        else if ($("#CurPhaseType").val() == "5") {
            //结束节点
            myFlow_AllStates[CPFlowGlobal_CurPhaseId].ChangePhaseType("image", "end");
        }
        
        var sData = $.myflow.getFlowData();
        InitFlowDesign(eval("(" + sData + ")"));
        CPFlowGlobal_IsNeedSave = true;
    }
    PhaseEditorCancel();
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
   // console.log(curPhaseConfig);
     //console.log(curPhasebbox);
    var tipTitle = "";
    if (curPhaseText != null && curPhaseText.attrs != null)
    {
        tipTitle = curPhaseText.attrs.text;
    }
    $("#CurPhaseName").val(tipTitle);
    CPFlowGlobal_CurPhaseOldName = tipTitle;
    if (curPhaseConfig.type == "start")
    {
        $("#CurPhaseType").val("1");
        CPFlowGlobal_CurPhaseOldType = "1";
    }
    else if (curPhaseConfig.type == "task")
    {
        //普通节点
        $("#CurPhaseType").val("2");
        CPFlowGlobal_CurPhaseOldType = "2";
    }
    else if (curPhaseConfig.type == "state")
    {
        //传阅节点
        $("#CurPhaseType").val("4");
        CPFlowGlobal_CurPhaseOldType = "4";
    }
    else if (curPhaseConfig.type == "fork")
    {
        //会签节点
        $("#CurPhaseType").val("3");
        CPFlowGlobal_CurPhaseOldType = "3";
    }
    else if (curPhaseConfig.type == "end")
    {
        //结束节点
        $("#CurPhaseType").val("5");
        CPFlowGlobal_CurPhaseOldType = "5";
    }
    var nLeft = $(window).width() / 2;
    var nTop = $(window).height() / 2;
    layer.open({
        type: 1,
        title: "阶段属性编辑",
        maxmin: false,
        shadeClose: false, //不允许点击外面关闭
        offset: [curPhasebbox.y.toString() + 'px', curPhasebbox.x.toString() + 'px'],//	同时定义top、left坐标
        area: ["400px", "200px"],
        content: $("#PhaseEditor"),//[url, 'no']
        cancel: function (index, layero) {
           
        }
    });
}
//阶段操作end

//路由操作start
function LinkOndblclick(from,to,linkObj)
{
    if (CPFlowGlobal_IsNeedSave == true)
    {
        alert("请先点击保存，再配置路径详细信息！");
        return;
    }
   // console.log(linkObj.getId());
  //  console.log(from.GetConfig());
  //console.log(to.GetConfig());
    var fromPhaseId = "rect" + from.GetConfig().props.temp1.value;
    var toPhaseId = "rect" + to.GetConfig().props.temp1.value;
    //console.log(fromPhaseId);
    //console.log(toPhaseId);
    //console.log(CPFlowGlobal_DesignInfo);
    var linkId = "";
    $.each(CPFlowGlobal_DesignInfo.paths, function (nIndex, nObj) {
       
        if (nObj.from == fromPhaseId && nObj.to == toPhaseId)
        {
            linkId = nObj.props.temp1.value;
            return;
        }
    });
    if (linkId == "")
    {
        alert("在数据库中未找到ID为【" + linkId + "】的路由，请先点击保存！");
        return;
    }
    //alert(linkId);
    var url = "/Plat/Form/FormView?FormCode=Form201711260753150033&SceneCode=Scene201711260753340035&ViewCode=View201711260753340036&DeviceType=1&PKValues=" + linkId;
    top.OpenNewModel(url, "路由条件配置",900, 450);
}
//路由操作end

//初始化流程图start

function SaveFlowDesign(data)
{
    var inputObj = new Object();
    inputObj.FlowJSON = data;
    inputObj.FlowVerId = CPFlowGlobal_FlowVerId;
    inputObj.CurUserId = CPCurUserId;
    inputObj.CurUserIden = CPCurUserIden;
    inputObj.FlowName = $("#FlowName").val();
    var updateUrl = CPWebRootPath + "/api/CPFlowEngine/SaveFlowDesign";
    $.ajax({
        type: "POST",
        url: updateUrl,
        data: JSON.stringify(inputObj),
        contentType: 'application/json',
        success: function (data) {
            if (data.Result == false) {
                alert("保存失败，详细信息：" + data.ErrorMsg);
                return false;
            }
            else {
                CPFlowGlobal_IsNeedSave = false;
                alert("保存成功！");
                InitFlowDesignJSONFromServer();
            }
        }
    });
}
function InitFlowDesign(flowJsonObj)
{
    
    $('#myflow').html("");
    //start
    $('#myflow')
        .myflow(
        {
            basePath: "",
            restore: flowJsonObj,//eval("({states:{rect1:{type:'start',text:{text:'开始'}, attr:{ x:507, y:63, width:50, height:50}, props:{text:{value:'开始'},temp1:{value:''},temp2:{value:''}}},rect2:{type:'state',text:{text:'状态2'}, attr:{ x:489, y:286, width:100, height:50}, props:{text:{value:'状态2'},temp1:{value:''},temp2:{value:''}}},rect3:{type:'task',text:{text:'任务1'}, attr:{ x:311, y:284, width:100, height:50}, props:{text:{value:'任务1'},assignee:{value:''},form:{value:''},desc:{value:''},temp1:{value:''},temp2:{value:''}}},rect4:{type:'task',text:{text:'任务3'}, attr:{ x:675, y:287, width:100, height:50}, props:{text:{value:'任务3'},assignee:{value:''},form:{value:''},desc:{value:''},temp1:{value:''},temp2:{value:''}}},rect5:{type:'fork',text:{text:'分支'}, attr:{ x:509, y:176, width:50, height:50}, props:{text:{value:'分支'},temp1:{value:''},temp2:{value:''}}},rect6:{type:'join',text:{text:'合并'}, attr:{ x:517, y:398, width:50, height:50}, props:{text:{value:'合并'},temp1:{value:''},temp2:{value:''}}},rect7:{type:'end',text:{text:'结束'}, attr:{ x:519, y:517, width:50, height:50}, props:{text:{value:'结束'},temp1:{value:''},temp2:{value:''}}}},paths:{path8:{from:'rect1',to:'rect5', dots:[],text:{text:'TO 分支',textPos:{x:55,y:-8}}, props:{text:{value:'TO 分支'}}},path9:{from:'rect5',to:'rect2', dots:[],text:{text:'TO 状态2',textPos:{x:28,y:-4}}, props:{text:{value:'TO 状态2'}}},path10:{from:'rect2',to:'rect6', dots:[],text:{text:'TO 合并',textPos:{x:25,y:-10}}, props:{text:{value:'TO 合并'}}},path11:{from:'rect6',to:'rect7', dots:[],text:{text:'TO 结束ddd',textPos:{x:31,y:-10}}, props:{text:{value:''}}},path12:{from:'rect5',to:'rect3', dots:[],text:{text:'TO 任务1',textPos:{x:-21,y:-10}}, props:{text:{value:'TO 任务1'}}},path13:{from:'rect3',to:'rect6', dots:[],text:{text:'TO 合并',textPos:{x:-34,y:21}}, props:{text:{value:'TO 合并'}}},path14:{from:'rect5',to:'rect4', dots:[],text:{text:'TO 任务3',textPos:{x:55,y:-17}}, props:{text:{value:'TO 任务3'}}},path15:{from:'rect4',to:'rect6', dots:[],text:{text:'TO 合并',textPos:{x:30,y:18}}, props:{text:{value:'TO 合并'}}}},props:{props:{name:{value:'新建流程'},key:{value:''},desc:{value:''}}}})"),
            editable: true//把默认修改属性的功能给关闭了
            
        });
        //end
}
function InitFlowDesignJSONFromServer()
{
    var getUrl = CPWebRootPath + "/api/CPFlowEngine/GetFlowInfoForDesigner?FlowVerId=" + CPFlowGlobal_FlowVerId;
    getUrl += "&CurUserId=" + CPCurUserId;
    getUrl += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(getUrl, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        CPFlowGlobal_DesignInfo = data.FlowDesigner;
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
    CPFlowToolSet();
    InitFlowDesignJSONFromServer();
    $('#myflow_save').click(function () {// 保存
        var data = $.myflow.getFlowData();

        SaveFlowDesign(data);
        //myflow.config.tools.save.onclick(data);
        // alert(data);
    });
    //流程基本信息
    $("#myflow_BasicInfo").click(function () {
        var url = "/Plat/Form/FormView?FormCode=Form201711192015360030&SceneCode=Scene201711192015530032&ViewCode=View201711192015530033&DeviceType=1";
        url += "&PKValues=" + CPFlowGlobal_FlowVerId;
        top.OpenNewModel(url, "流程基本信息", 1024, 8000);
    });
    //刷新
    $("#myFlow_Refresh").click(function () {
        if (CPFlowGlobal_IsNeedSave)
        {
            if (confirm("系统检测到您有修改未保存，您确认要刷新吗？")) {
                window.location.reload();
            }
        }
        else {
            window.location.reload();
        }
       
    })
    //发布
    $("#myflow_Release").click(function () {
        if (CPFlowGlobal_IsNeedSave == true) {
            alert("系统检测到你修改了流程但未保存，请先点击保存，再发布流程！");
            return;
        }
        var getUrl = CPWebRootPath + "/api/CPFlowEngine/ReleaseFlow?FlowVerId=" + CPFlowGlobal_FlowVerId;
        getUrl += "&CurUserId=" + CPCurUserId;
        getUrl += "&CurUserIden=" + CPCurUserIden;
        $.getJSON(getUrl, function (data) {
            if (data.Result == false) {
                alert(data.ErrorMsg);
                return false;
            }
            alert("发布成功！");
            CPFlowGlobal_IsRelease = true;
            $("#spanFlowVerState").html("已发布");

        });

    });

    //生成新版本
    $("#myflow_CreateNewVer").click(function () {
        if (CPFlowGlobal_IsRelease ==false)
        {
            alert("当前版本未发布，不能生成新版本!");
            return;
        }
        if (confirm("您确实要创建新版本吗？")) {
            var getUrl = CPWebRootPath + "/api/CPFlowEngine/CreateFlowNewVer?FlowVerId=" + CPFlowGlobal_FlowVerId;
            getUrl += "&CurUserId=" + CPCurUserId;
            getUrl += "&CurUserIden=" + CPCurUserIden;
            $.getJSON(getUrl, function (data) {
                if (data.Result == false) {
                    alert(data.ErrorMsg);
                    return false;
                }
                alert("创建成功！");
                var url = CPWebRootPath + "/Plat/Flow/FlowDesign?FlowVerId=" + data.NewFlowVerId + "&FlowId=" + CPFlowGlobal_FlowId;
                window.location.href = url;

            });
        }
    });
    //还原至此版本
    $("#myflow_RestoreVer").click(function () { 
        if (confirm("您确实要还原至此版本吗？")) {
            var getUrl = CPWebRootPath + "/api/CPFlowEngine/RestoreFlowVer?RestoreFlowVerId=" + CPFlowGlobal_FlowVerId + "&FlowId=" + CPFlowGlobal_FlowId;
            getUrl += "&CurUserId=" + CPCurUserId;
            getUrl += "&CurUserIden=" + CPCurUserIden;
            $.getJSON(getUrl, function (data) {
                if (data.Result == false) {
                    alert(data.ErrorMsg);
                    return false;
                }
                alert("还原成功！");
                parent.CloseNewModel();

            });
        }
    });
});
//初始化流程图end