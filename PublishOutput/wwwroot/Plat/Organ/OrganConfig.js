//点击树节点执行方法
function DepTreeNodeClick() {
    var url = "/Plat/Form/FormView?FormCode=Form201711020720480023&SceneCode=Scene201711020721050025&ViewCode=View201711020721050026&DeviceType=1";
    url += "&PKValues=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx;
    CPTreeSetFrameUrl(url);
}
//新增
function DepAdd() {
    //console.log(CPTreeGlobal_CurSelNodeData);
    var url = "/Plat/Form/FormView?FormCode=Form201711020720480023&SceneCode=Scene201711020721050025&ViewCode=View201711020721050026&DeviceType=1&InitGroupCode=Group201711020726580020";  
    if (CPTreeGlobal_CurSelNodeData == null) {
        url += "&ParentId=-1";
    }
    else {
        url += "&ParentId=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx;
    }
    CPTreeSetFrameUrl(url);
}
//删除部门
function DepDel()
{
    if (CPTreeGlobal_CurSelNodeData == null) {
        alert("请选择要删除的部门");
        return;
    }
    if (confirm("您确实要删除当前选中的部门吗！"))
    {
        var url = CPWebRootPath + "/api/COOrganEngine/SetDepDeleteState?DepId=" + CPTreeGlobal_CurSelNodeData.NodeAttrEx + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;   
        $.getJSON(url, function (data) {
            if (data.Result == false) {
                alert(data.ErrorMsg);
                return false;
            } 
            try {
                var treeview = CPTreeGetTreeInstance();
                var cNode = CPTreeGlobal_CurSelNode;
                var cNodeDataItem = null;
                var parentCNode = treeview.parent(cNode);
                alert("删除成功");
                treeview.remove(cNode);
                cNodeDataItem = treeview.dataItem(parentCNode);
                cNodeDataItem.loaded(false);
                cNodeDataItem.load();
                treeview.expand(parentCNode);
                CPTreeSetFrameUrl("about:_blank;");
            }
            catch (e) {
                //删除根节点时，会出错
                alert("删除成功");
                window.location.reload();
                return;
            }
        });
    }

}