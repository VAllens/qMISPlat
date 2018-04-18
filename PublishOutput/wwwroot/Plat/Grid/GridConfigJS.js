//初步化列表列信息
function InitColumn(TargetGridId)
{
    var url = CPWebRootPath + "/api/GridEngine/SynGridFieldInfo?GridId=" + TargetGridId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(url, function (data) {
        if (data.Result == false) {
            alert("同步失败，详细信息：" + data.ErrorMsg);
            return false;
        }
        else {
            alert("同步成功！");
            CPGridRefresh();
        }
        
    });
}
//导出内置按钮
function ImportGridInnerFunc(TargetGridId)
{
    var url = CPWebRootPath + "/api/GridEngine/ImportGridInnerFunc?GridId=" + TargetGridId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(url, function (data) {
        if (data.Result == false) {
            alert("导入失败，详细信息：" + data.ErrorMsg);
            return false;
        }
        else {
            alert("导入成功！");
            CPGridRefresh();
        }

    });
}
//导出配置
function GridExportConfig()
{
    var sSel = CPGridGetSelChkData();
    if (sSel == "")
    {
        alert("请选择要导出的列表！");
        return false;
    }
    var url = CPWebRootPath + "/api/GridEngine/DownloadGridConfig?GridIds=" + sSel + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    window.location.href = url;
}
//更新配置
function GridUpateConfig(sysId)
{
    var url = CPWebRootPath + "/Plat/Grid/ManaConfig?IsCreateNew=false&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "更新配置", 400, 200);
}
//从配置文件创建新列表
function GridCreateByConfigFile(sysId)
{
    var url = CPWebRootPath + "/Plat/Grid/ManaConfig?IsCreateNew=true&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "从配置文件全新创建列表", 400, 200);
}