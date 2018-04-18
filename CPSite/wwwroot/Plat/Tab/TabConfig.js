//导出配置
function TabExportConfig() {
    var sSel = CPGridGetSelChkData();
    if (sSel == "") {
        alert("请选择要导出的标签页！");
        return false;
    }
    var url = CPWebRootPath + "/api/TabEngine/DownloadTabConfig?TabIds=" + sSel + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    window.location.href = url;
}
//更新配置
function TabUpateConfig(sysId) {
    var url = CPWebRootPath + "/Plat/Tab/ManaConfig?IsCreateNew=false&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "更新配置", 400, 200);
}
//从配置文件创建新列表
function TabCreateByConfigFile(sysId) {
    var url = CPWebRootPath + "/Plat/Tab/ManaConfig?IsCreateNew=true&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "从配置文件全新创建标签页", 400, 200);
}