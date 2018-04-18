//点击树节点执行方法
function ConfigTreeSourceNodeClick()
{
    var url = "/Plat/Form/FormView?FormCode=Form201710190724280017&SceneCode=Scene201710190724550017&ViewCode=View201710190724540018&DeviceType=1&InitGroupCode=Group201710190729150016";
    url += "&TargetTreeId=" + $.CPGetQuery("TargetTreeId");
    url += "&ParentSourceId=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx;
    url += "&PKValues=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx;
    CPTreeSetFrameUrl(url);
}
//新增
function ConfigTreeSourceAdd() {
    //console.log(CPTreeGlobal_CurSelNodeData);
    var url = "/Plat/Form/FormView?FormCode=Form201710190724280017&SceneCode=Scene201710190724550017&ViewCode=View201710190724540018&DeviceType=1&InitGroupCode=Group201710190729150016";
    url += "&TargetTreeId=" + $.CPGetQuery("TargetTreeId");
    if (CPTreeGlobal_CurSelNodeData == null) {
        url += "&ParentSourceId=-1";
    }
    else {
        url += "&ParentSourceId=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx;
    }  
    CPTreeSetFrameUrl(url);
}
//导入内置按钮
function ImportTreeInnerFunc()
{
    var getDataUrl = CPWebRootPath + "/api/TreeEngine/ImportTreeInnerFunc?TreeId=" + $.CPGetQuery("TargetTreeId") + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(getDataUrl, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        } else {
            alert("导入成功！");
            CPGridRefresh();
        }
    });
}



//导出配置
function TreeExportConfig() {
    var sSel = CPGridGetSelChkData();
    if (sSel == "") {
        alert("请选择要导出的树！");
        return false;
    }
    var url = CPWebRootPath + "/api/TreeEngine/DownloadTreeConfig?TreeIds=" + sSel + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    window.location.href = url;
}
//更新配置
function TreeUpateConfig(sysId) {
    var url = CPWebRootPath + "/Plat/Tree/ManaConfig?IsCreateNew=false&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "更新配置", 400, 200);
}
//从配置文件创建新列表
function TreeCreateByConfigFile(sysId) {
    var url = CPWebRootPath + "/Plat/Tree/ManaConfig?IsCreateNew=true&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "从配置文件全新创建树", 400, 200);
}