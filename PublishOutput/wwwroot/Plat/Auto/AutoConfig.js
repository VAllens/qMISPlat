//导出配置
function AutoExportConfig() {
    var sSel = CPGridGetSelChkData();
    if (sSel == "") {
        alert("请选择要导出的自动编号！");
        return false;
    }
    var url = CPWebRootPath + "/api/AutoEngine/DownloadAutoConfig?AutoIds=" + sSel + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    window.location.href = url;
}
//更新配置
function AutoUpateConfig(sysId) {
    var url = CPWebRootPath + "/Plat/Auto/ManaConfig?IsCreateNew=false&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "更新配置", 400, 200);
}
//从配置文件创建新列表
function AutoCreateByConfigFile(sysId) {
    var url = CPWebRootPath + "/Plat/Auto/ManaConfig?IsCreateNew=true&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "从配置文件全新创建自动编号", 400, 200);
}