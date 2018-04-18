//全局传入参数start
//表单唯一标识符
var CPTreeGlobal_TreeCode = $.CPGetQuery("TreeCode"); if (CPTreeGlobal_TreeCode == null || CPTreeGlobal_TreeCode == undefined) CPTreeGlobal_TreeCode = ""; 
//全局传入参数end

//全局变量start 
//全局对象
var CPTreeGlobal_TreeObj = null;
//记录当前选中的节点数据对象
var CPTreeGlobal_CurSelNodeData = null;
//记录当前选中的节点对象
var CPTreeGlobal_CurSelNode = null;
//记录所有加载 的数据
var CPTreeGlobal_AllLoadData = new Array();
//获取本页面里通过layer产生弹出层的iframe
function CPGetContextFrame() {
    return document.getElementById("CPTreeFrame").contentWindow;
}
function CPTreeSetFrameUrl(url)
{
    $("#CPTreeFrame").attr("src", CPWebRootPath + url);
}
//选中第一个节点
function CPTreeSelectFirstNode()
{
    var treeview = $("#CPTreeDiv").data("kendoTreeView");
    if (treeview.dataSource.view().length <= 0)
        return;
    treeview.select(".k-item:first");//展开第一个节点
    var dataItem = treeview.dataItem(treeview.select());
    CPTreeNodeSelect(dataItem, treeview.select());
}
//获取所有选中的节点数据集合
function CPTreeGetSelNodeData()
{
    var CPTreeGetSelNodeDataArray = new Array();
    var treeView = $("#CPTreeDiv").data("kendoTreeView");
    CPTreeCheckedNodeIds(treeView.dataSource.view(), CPTreeGetSelNodeDataArray);
    return CPTreeGetSelNodeDataArray;
}
// function that gathers IDs of checked nodes
function CPTreeCheckedNodeIds(nodes, CPTreeGetSelNodeDataArray) {
    for (var i = 0; i < nodes.length; i++) {
        if (nodes[i].checked) {
            CPTreeGetSelNodeDataArray.push(nodes[i]);
        }

        if (nodes[i].hasChildren) {
            CPTreeCheckedNodeIds(nodes[i].children.view(), CPTreeGetSelNodeDataArray);
        }
    }
}
function CPTreeGetTreeInstance()
{
    var treeview = $("#CPTreeDiv").data("kendoTreeView");
    return treeview;
}
function CPTreeSetContainerWidth()
{
    $("#CPTreeContainer").height($(window).height() - 4);
    $("#CPTreeContainer").width($(window).width() - 2);
    if (CPTreeGlobal_TreeObj.ShowType == 2) {
        //只显示树
        $("#center-pane").hide();
    }
    else {
        $("#CPTreeContainer").kendoSplitter({
            panes: [
                { collapsible: true, size: CPTreeGlobal_TreeObj.LeftTreeWidth + "px" },
                { collapsible: false }
            ]
        });
        //设置高度 
        $("#CPTreeFrame").height($(window).height() - 8);
        $("#CPTreeFrame").width($(window).width() - $("#left-pane").width() - 10);
    }
}

$(function () {
  

    var getDataUrl = CPWebRootPath + "/api/TreeEngine/GetTreeInfo?TreeCode=" + CPTreeGlobal_TreeCode + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    //为了表达式，需要把URL里其它字段再加上
    //再加其它参数
    var sResultArray = CPGetQueryString();
    for (var mm = 0; mm < sResultArray.length; mm++) {
        var tempSarray = sResultArray[mm].split('=');
        if (tempSarray[0].toLowerCase() == "treecode"
            || tempSarray[0].toLowerCase() == "curuserid"
            || tempSarray[0].toLowerCase() == "curuseriden"
        )
            continue;
        getDataUrl += "&" + sResultArray[mm];
    }
    $.getJSON(getDataUrl, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        //console.log(data);
        var sHtml = "";
        CPTreeGlobal_TreeObj = data.Tree;
        CPTreeSetContainerWidth();
        //加载扩展脚本
        if (CPTreeGlobal_TreeObj.JSEx != null && CPTreeGlobal_TreeObj.JSEx != "") {
            $("#divCPTreeJSContainer").html(CPTreeGlobal_TreeObj.JSEx);
        }
        //按钮start
        var btnHtml = "";
        $.each(CPTreeGlobal_TreeObj.FuncCol, function (nIndex, nObj) {
            var display = "";
            if (nObj.SourceId != -1)
            {
                display = " display:none; "
            }
            btnHtml += "<button type=\"button\" id=\"CPTree_FuncBtn_" + nObj.Id + "\" class=\"CPTreeFuncBtn_Css\" style=\"" + display + "\"><i class=\"icon iconfont " + nObj.FuncIcon + "\"  ></i><span>" + nObj.FuncTitle + "</span></button>";
        });
        $("#CPTreeButtonDiv").html(btnHtml);
        if (btnHtml == "")
            $("#CPTreeButtonDiv").hide();
        $.each(CPTreeGlobal_TreeObj.FuncCol, function (nIndex, nObj) {
            $("#CPTree_FuncBtn_" + nObj.Id).kendoButton({
                click: function (e) {                   
                    //执行自定义脚本
                    eval(nObj.JSMethod);
                }
            });
        });
        //按钮end
        //目录树相关start
        var treeId = "CPTreeDiv";
        var readRealDataUrl = CPWebRootPath + "/api/TreeEngine/GetTreeData" + window.location.search;
        var dirDataSource = null;
        var checkedConfig = null;
        if (CPTreeGlobal_TreeObj.IsShowCheckBox)
        {
            if (CPTreeGlobal_TreeObj.IsSelectChild)
            {
                checkedConfig = {
                    checkChildren: true                   
                };
            }
            else {
                checkedConfig = {
                    checkChildren: false
                };
            }
        }
        if (CPTreeGlobal_TreeObj.DataLoadType == 1)
        {
            //全加载start  
            var obj = new Object();
            obj.TreeCode = CPTreeGlobal_TreeCode;
            obj.TreeDataSourceId = 0;
            obj.CurUserId = CPCurUserId;
            obj.CurUserIden = CPCurUserIden;
            obj.DataRowJSON = "";
            $.ajax({
                type: "POST",
                url: readRealDataUrl,
                data: JSON.stringify(obj),
                contentType: 'application/json',
                success: function (data) {
                    if (data.Result == false) {
                        alert("获取数据失败，详细信息：" + data.ErrorMsg);
                        return false;
                    }
                    else {
                        var dsSource = new kendo.data.HierarchicalDataSource({
                            data: data.DataCol,                            
                            schema: {
                                model: {
                                    id: "NodeId",
                                    hasChildren: "hasChildren",
                                    children: "items",
                                    checked: "checked"
                                }
                            }
                        });
                      // console.log(data.DataCol);
                        $("#" + treeId).kendoTreeView({
                           dragAndDrop: CPTreeGlobal_TreeObj.IsCanDrag,
                            checkboxes: checkedConfig,
                            template: kendo.template($("#treeview-template").html()),
                            dataSource: dsSource,
                            loadOnDemand: false,//必须设置，否则会导致获取checkbox选择值时，在节点未展开的情况下，获取不全的问题
                            dataBound: function (e) {
                                if (e.node == undefined) {
                                    ////默认展开第一级节点
                                    var treeview = $("#" + treeId).data("kendoTreeView");
                                    treeview.expand(".k-item:first");//展开第一个节点
                                    CPTreeOnLoadEx();
                                }
                            },
                            select: function (e) {
                                //节点选中事件
                                var treeview = $("#" + treeId).data("kendoTreeView");
                                //获取到服务器端发送过来的数据对象
                                var dataItem = treeview.dataItem(e.node);
                                CPTreeNodeSelect(dataItem, e.node);
                            },
                            navigate: function (e) {
                                // 不知道为什么，这个事件不起作用
                            },
                            dragend: function (e) {
                                //console.log("Drag end", e.sourceNode, e.dropPosition, e.destinationNode);
                                CPTreeDragEnd(e);
                            }
                        });
                      
                    }
                }
            });
            //全加载end
        }
        else {
            //逐级加载start
            dirDataSource = new kendo.data.HierarchicalDataSource({
                transport: {
                    read: {
                        url: readRealDataUrl,
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json"
                    },
                    parameterMap: function (options, operation) {
                       
                        var curObj = null;
                        if (options != null && options != "") {
                            $.each(CPTreeGlobal_AllLoadData, function (nIndex, nObj) {
                                if (nObj.NodeId == options.NodeId) {
                                    curObj = nObj;
                                    return;
                                }
                            });
                        } 
                        // note that you may need to merge that postData with the options send from the DataSource
                        var obj = new Object();
                        obj.TreeCode = CPTreeGlobal_TreeCode;
                        if (curObj== null  ) {
                            obj.TreeDataSourceId = CPTreeGlobal_TreeObj.DataSourceCol[0].Id;
                            obj.DataRowJSON = "";
                        }
                        else {
                            obj.TreeDataSourceId = curObj.TreeDataSourceId;
                            obj.DataRowJSON = curObj.DataRowJSON;
                        }
                        obj.CurUserId = CPCurUserId;
                        obj.CurUserIden = CPCurUserIden;
                        
                        return JSON.stringify(obj);
                    }
                },
                schema: {
                    model: {
                        id: "NodeId",
                        hasChildren: "hasChildren",
                        checked: "checked"
                    }
                    , data: function (response) {
                        //  return jQuery.parseJSON(response.DataJson);  
                      //  console.log(response.DataCol);
                        $.each(response.DataCol, function (nIndex, nObj) {
                            CPTreeGlobal_AllLoadData.push(nObj);
                        });                        
                        return response.DataCol;
                        // return response.results; // twitter's response is { "results": [ /* results */ ] }
                    }
                }
            });
            $("#" + treeId).kendoTreeView({
                dragAndDrop: CPTreeGlobal_TreeObj.IsCanDrag,
                checkboxes: checkedConfig,
                template: kendo.template($("#treeview-template").html()),
                dataSource: dirDataSource,
                loadOnDemand: true,
                dataBound: function (e) {
                    if (e.node == undefined) {
                        ////默认展开第一级节点
                        var treeview = $("#" + treeId).data("kendoTreeView");
                        treeview.expand(".k-item:first");//展开第一个节点
                        CPTreeOnLoadEx();
                    }
                },
                select: function (e) {
                    //节点选中事件
                    var treeview = $("#" + treeId).data("kendoTreeView");
                    //获取到服务器端发送过来的数据对象
                    var dataItem = treeview.dataItem(e.node);
                    CPTreeNodeSelect(dataItem, e.node);
                },
                navigate: function (e) {
                    // 不知道为什么，这个事件不起作用
                },
                dragend: function (e) {
                    //console.log("Drag end", e.sourceNode, e.dropPosition, e.destinationNode);
                    CPTreeDragEnd(e);
                }
            });
            //逐级加载end
          
        }
            
       
       
    });
});
//树节点选中事件
function CPTreeNodeSelect(dataItem,curNode)
{
    CPTreeGlobal_CurSelNodeData = dataItem;
    CPTreeGlobal_CurSelNode = curNode;
    //设置按钮可见
    $.each(CPTreeGlobal_TreeObj.FuncCol, function (nIndex, nObj) {
        if (nObj.SourceId != -1) {
            if (nObj.SourceId == CPTreeGlobal_CurSelNodeData.TreeDataSourceId)
                $("#CPTree_FuncBtn_" + nObj.Id).show();
            else
                $("#CPTree_FuncBtn_" + nObj.Id).hide();
        }
    });
    if (CPTreeGlobal_TreeObj.SelectEventMethod != "") {
        eval(CPTreeGlobal_TreeObj.SelectEventMethod);
    }
}
//删除选中节点
function CPTreeDeleteNode()
{
    if (CPTreeGlobal_CurSelNodeData == null)
    {
        alert("请先选中待删除的节点！");
        return false;
    } 
    if (confirm("您确实要删除选中节点及其子节点吗？"))
    {
        var delData = CPWebRootPath + "/api/TreeEngine/DeleteTreeData";
        delData += "?TreeDataSourceId=" + CPTreeGlobal_CurSelNodeData.TreeDataSourceId;
        delData += "&pkFieldValue=" + CPTreeGlobal_CurSelNodeData.DeleteFieldValue;
        delData += "&CurUserId=" + CPCurUserId;
        delData += "&CurUserIden=" + CPCurUserIden;
        $.getJSON(delData, function (data) {
            if (data.Result == false) {
                alert(data.ErrorMsg);
                return false;
            }
            else {
                alert("删除成功！");
                try {
                    var treeview = $("#CPTreeDiv").data("kendoTreeView");
                    var cNode = CPTreeGlobal_CurSelNode;
                    var cNodeDataItem = null;
                    var parentCNode = treeview.parent(cNode);
                   
                    treeview.remove(cNode);
                    cNodeDataItem = treeview.dataItem(parentCNode);
                    cNodeDataItem.loaded(false);
                    cNodeDataItem.load();
                    treeview.expand(parentCNode);
                    CPTreeSetFrameUrl("about:_blank;");
                }
                catch (e) {
                    //删除根节点时，会出错
                  
                    window.location.reload();
                    return;
                }
            }
        });
    }
}
//拖拽节点事件
//console.log("Drag end", e.sourceNode, e.dropPosition, e.destinationNode);
function CPTreeDragEnd(e)
{
    var treeview = $("#CPTreeDiv").data("kendoTreeView");
    //获取到服务器端发送过来的数据对象
    var dataItemSource = treeview.dataItem(e.sourceNode);
    var dataItemTarget = treeview.dataItem(e.destinationNode);
    //console.log(dataItemSource);
    //console.log(dataItemTarget);
    // over, before, or after. e.dropPosition
    if (dataItemSource.DeleteFieldValue == "")
    {
        alert("数据源未配置表主键字段，无法将更改保存到数据库！")
        return;
    }
    var SourcePK = dataItemSource.DeleteFieldValue;
    var TargetPK = "";
    if (e.dropPosition == "before" || e.dropPosition == "after")
    {
        //直接到根
        TargetPK = -1;
    }
    else {
        //拖动到目标节点下
        TargetPK = dataItemTarget.DeleteFieldValue;
    }
    var updateData = CPWebRootPath + "/api/TreeEngine/UpdateTreeDataParent";
    updateData += "?TreeDataSourceId=" + dataItemSource.TreeDataSourceId;
    updateData += "&SourcePKValue=" + SourcePK;
    updateData += "&TargetPKValue=" + TargetPK;
    updateData += "&CurUserId=" + CPCurUserId;
    updateData += "&CurUserIden=" + CPCurUserIden;
    $.getJSON(updateData, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        else {
            alert("保存成功");
        }
    });
}
//双击事件执行方法
function CPTreeNodedblClick(nodeId)
{
   // console.log(nodeId);
   // var treeview = $("#CPTreeDiv").data("kendoTreeView");
    if (CPTreeGlobal_TreeObj.DoubleClickEventMethod != null && CPTreeGlobal_TreeObj.DoubleClickEventMethod != "")
    {
        eval(CPTreeGlobal_TreeObj.DoubleClickEventMethod);
    }
}
//树加载完成执行方法
function CPTreeOnLoadEx()
{
    if (CPTreeGlobal_TreeObj.OnLoadEventMethod != null && CPTreeGlobal_TreeObj.OnLoadEventMethod != "") {
        eval(CPTreeGlobal_TreeObj.OnLoadEventMethod);
    }
}
//刷新树
function CPTreeRefresh()
{
    if (CPTreeGlobal_TreeObj.DataLoadType == 1) {
        //全部加载模式
        alert("当前数据加载模式为一次性全部加载，不支持局部刷新树节点，建议改成逐级加载模式");
        window.location.reload();
        return;
    }
    if (CPTreeGlobal_CurSelNode == null)
    {
        window.location.reload();
        return;
    }   
    var treeview = $("#CPTreeDiv").data("kendoTreeView"); 
    var cNode = CPTreeGlobal_CurSelNode;
    var cNodeDataItem = treeview.dataItem(cNode);
    var parentCNode = treeview.parent(cNode);
    if (parentCNode == null) {
        window.location.reload();
        return;
    }
    cNodeDataItem = treeview.dataItem(parentCNode);
    if (cNodeDataItem == null) {
        window.location.reload();
        return;

    }
    cNodeDataItem.loaded(false);
    cNodeDataItem.load();
    treeview.expand(parentCNode);
}
//全展开
function CPTreeExpandAllNode()
{
    if (CPTreeGlobal_TreeObj.DataLoadType == 2)
    {
        alert("当前数据加载模式为逐级加载，不允许使用全展开功能");
        return;
    }
    var nCount = 0;
    var treeview = $("#CPTreeDiv").data("kendoTreeView");
    while ($(".k-i-expand").length > 0 && nCount <10) {
        treeview.expand(".k-item");
        nCount++;
    }
}
//全收缩
function CPTreeCollapseAllNode() {
    if (CPTreeGlobal_TreeObj.DataLoadType == 2) {
        alert("当前数据加载模式为逐级加载，不允许使用全收缩功能");
        return;
    }
    var treeview = $("#CPTreeDiv").data("kendoTreeView"); 
    treeview.collapse(".k-item");
}
function CPTreeUpdateConfig(treeId) {
    var url = "/Plat/Tab/TabView?TabCode=Tab201710181518190005&TargetTreeId=" + treeId;
    try {
        top.OpenNewModel(url, "修改配置", 9000, 9000);
    }
    catch (e) {
        OpenNewModel(url, "修改配置", 9000, 9000);
    }
}