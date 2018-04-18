//点击树节点执行方法
function ModuleConfigNodeClick() {
    var url = "/Plat/Form/FormView?FormCode=Form201710200836580019&SceneCode=Scene201710200837200019&ViewCode=View201710200837190020&DeviceType=1&InitGroupCode=Group201710200840550018";
    url += "&SysId=" + $.CPGetQuery("SysId");
    url += "&PKValues=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx;

    CPTreeSetFrameUrl(url);
}
function AddModule() {
    var url = "/Plat/Form/FormView?FormCode=Form201710200836580019&SceneCode=Scene201710200837200019&ViewCode=View201710200837190020&DeviceType=1&InitGroupCode=Group201710200840550018";
    url += "&SysId=" + $.CPGetQuery("SysId");
    if (CPTreeGlobal_CurSelNodeData != null) {
        url += "&ParentId=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx;
    }
    else {
        url += "&ParentId=-1";
    }
    CPTreeSetFrameUrl(url);
}
//树角色授权
function ModuleRoleAuth(roleId,roleName)
{
    var url = CPWebRootPath + "/api/CPModuleEngine/SetRoleModuleIdsToSession?RoleId=" + roleId + "&SysId=1&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(url, function (data) {
        if (data.Result == false) {
            alert("将模块写入Session失败，详细信息：" + data.ErrorMsg);
            return false;
        }
        else {
            //alert("登录成功！");;
            url = "/Plat/Tree/TreeView?TreeCode=Tree201711030720190004&SysId=1&RoleId=" + roleId;
            top.OpenNewModel(url, "给【" + roleName + "】授权", 1024,768);
        }
    });
}
//保存角色授权
function SaveRoleModuleRight(roleId)
{
    var obj = new Object();
    obj.CurUserId = CPCurUserId;
    obj.CurUserIden = CPCurUserIden;
    obj.RoleId = roleId;
    obj.ModuleIds = "";
    var sArray = CPTreeGetSelNodeData();
    $.each(sArray, function (nIndex, nObj) {
        if (obj.ModuleIds == "")
            obj.ModuleIds = nObj.NodeAttrEx;
        else
            obj.ModuleIds += "," +  nObj.NodeAttrEx;
    });
    var url = CPWebRootPath + "/api/CPModuleEngine/SetRoleModuleRight";
    $.ajax({
        type: "POST",
        url: url,
        data: JSON.stringify(obj),
        contentType: 'application/json',
        success: function (data) {
            if (data.Result == false) {
                alert("保存失败，详细信息：" + data.ErrorMsg);
                return false;
            }
            else {
                alert("保存成功！");
                parent.CloseNewModel();
            }
        }
    });
}
