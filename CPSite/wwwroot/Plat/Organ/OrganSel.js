
//是否可以多选，true可以，false不可以
var CPOrganSel_IsMultiSel = $.CPGetQuery("IsMultiSel"); if (CPOrganSel_IsMultiSel == null || CPOrganSel_IsMultiSel == undefined) CPOrganSel_IsMultiSel = "true";

$(function () {

    if (CPOrganSel_IsMultiSel == "true")
    {
        $("#CPBtnAllSel").show();
        $("#CPBtnAllDel").show();
    }
   // $("#vertical").height($(window).height() - 2);
    $("#vertical").width($(window).width() - 2);
    $("#vertical").kendoSplitter({
        orientation: "vertical",
        panes: [
            { collapsible: false, resizable: false, size: "394px" }, 
            { collapsible: false, resizable: false,size:"50px" }
        ]
    }); 
    $("#horizontal").kendoSplitter({
        panes: [
            { collapsible: false, resizable: false,size:300 },
            { collapsible: false, resizable: false }
        ]
    });
    $("#CPBtnSearch").kendoButton({
        imageUrl: "../../Style/CommonIcon/search.png",
        click: function (e) {
            console.log(CPTrim($("#CPOrganSelSearchTxt").val()));
            if (CPTrim($("#CPOrganSelSearchTxt").val()) == "")
                return;
            var getDepUrl = CPWebRootPath + "/api/COOrganEngine/GetUserByUserNameOrLoginName?NameEx=" + CPTrim($("#CPOrganSelSearchTxt").val()) + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
            $.getJSON(getDepUrl, function (data) {
                if (data.Result == false) {
                    alert(data.ErrorMsg);
                    return false;
                }
                $("#LeftUserList").empty();
                $.each(data.UserCol, function (nIndex, nObj) {
                    //nObj.Id, nObj.UserName;
                    //console.log($('#RightUserList option[value=' + nObj.Id + ']').length);
                    if ($('#RightUserList option[value=' + nObj.Id + ']').length <= 0) {
                        $("#LeftUserList").append("<option value='" + nObj.Id + "'>" + nObj.UserName + "</option > ");
                    }
                });
            });
        }
    });
    //所有用户
    $("#CPBtnAllUser").kendoButton({
        click: function (e) {
            var getDepUrl = CPWebRootPath + "/api/COOrganEngine/GetAllUser?CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
            $.getJSON(getDepUrl, function (data) {
                if (data.Result == false) {
                    alert(data.ErrorMsg);
                    return false;
                }
                $("#LeftUserList").empty();
                $.each(data.UserCol, function (nIndex, nObj) {
                    //nObj.Id, nObj.UserName;
                    //console.log($('#RightUserList option[value=' + nObj.Id + ']').length);
                    if ($('#RightUserList option[value=' + nObj.Id + ']').length <= 0) {
                        $("#LeftUserList").append("<option value='" + nObj.Id + "'>" + nObj.UserName + "</option > ");
                    }
                });
            });
        }
    });
    //左侧单选
    $("#CPBtnSel").kendoButton({
        click: function (e) {   
            if (CPOrganSel_IsMultiSel == "false")
            {//只允许选择一个用户
                $('#RightUserList option').each(function () {
                    if ($('#LeftUserList option[value=' + $(this).val() + ']').length <= 0) {
                        $("#LeftUserList").append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option > ");
                    }
                });
                $("#RightUserList").empty();
            }
            var nAddCount = 0;
            $('#LeftUserList option:selected').each(function () {
                if (CPOrganSel_IsMultiSel == "false" && nAddCount >= 1)
                {
                    return;
                }
                if ($('#RightUserList option[value=' + $(this).val() + ']').length <= 0) {
                    nAddCount = nAddCount + 1;
                    $("#RightUserList").append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option > ");
                }
            });
            nAddCount = 0;
            $('#LeftUserList option:selected').each(function () {
                if (CPOrganSel_IsMultiSel == "false" && nAddCount >= 1) {
                    return;
                }
                nAddCount = nAddCount + 1;
                $(this).remove();
            });
        }
    });
    //右侧单选
    $("#CPBtnDel").kendoButton({
        click: function (e) {
            $('#RightUserList option:selected').each(function () {
                if ($('#LeftUserList option[value=' + $(this).val() + ']').length <= 0) {
                    $("#LeftUserList").append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option > ");
                }
            });
            $('#RightUserList option:selected').each(function () {
                $(this).remove();
            });
        }
    });
    //左侧全选
    $("#CPBtnAllSel").kendoButton({
        click: function (e) {
            $('#LeftUserList option').each(function () {
                if ($('#RightUserList option[value=' + $(this).val() + ']').length <= 0) {
                    $("#RightUserList").append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option > ");
                }
            });
            $("#LeftUserList").empty();
        }
    });
    //右侧全选
    $("#CPBtnAllDel").kendoButton({
        click: function (e) {
            $('#RightUserList option').each(function () {
                if ($('#LeftUserList option[value=' + $(this).val() + ']').length <= 0) {
                    $("#LeftUserList").append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option > ");
                }
            });
            $("#RightUserList").empty();
        }
    });
    //点击确定
    $("#CPBtnOK").kendoButton({
        click: function (e) {
            var obj = new Object();
            obj.UserIds = "";
            obj.UserNames = "";
            $('#RightUserList option').each(function () {
                if (obj.UserIds == "")
                {
                    obj.UserIds = $(this).val();
                    obj.UserNames = $(this).text();
                }
                else {
                    obj.UserIds += "," +  $(this).val();
                    obj.UserNames += "," +  $(this).text();
                }
            });
            //console.log(obj);
            parent.CPShowModalDialogReturnArgs = obj;
            parent.CPOrganSelectMethod_SetReturn();
            parent.CloseNewModel();
        }
    });
    //点击取消
    $("#CPBtnCancel").kendoButton({
        click: function (e) {
            parent.CloseNewModel();
        }
    });
    var getDepUrl = CPWebRootPath + "/api/COOrganEngine/GetAllDep?CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;   
    $.getJSON(getDepUrl, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        //树
        var dsSource = new kendo.data.HierarchicalDataSource({
            data: data.DepCol,
            schema: {
                model: {
                    id: "Id",
                    hasChildren: "HasChildDep",
                    children: "ChildDep"
                }
            }
        });
        // console.log(dsSource);
        var treeId = "CPOrganSelTree";
        $("#" + treeId).kendoTreeView({
            dragAndDrop: false,
            template: kendo.template($("#treeview-template").html()),
            dataSource: dsSource,
            //loadOnDemand: true,
            dataBound: function (e) {
                if (e.node == undefined) {
                    ////默认展开第一级节点
                    var treeview = $("#" + treeId).data("kendoTreeView");
                    treeview.expand(".k-item:first");//展开第一个节点
                }
            },
            select: function (e) {
                //节点选中事件
                var treeview = $("#" + treeId).data("kendoTreeView");
                //获取到服务器端发送过来的数据对象
                var dataItem = treeview.dataItem(e.node);
                CPOrganSelNodeClick(dataItem, e.node);
            },
            navigate: function (e) {
                // 不知道为什么，这个事件不起作用
            }
        });
        //树
    });
    //加载当前用户所在部门的用户
    var GetUserByMyDep = CPWebRootPath + "/api/COOrganEngine/GetUserByMyDep?CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;  
    $.getJSON(GetUserByMyDep, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        $.each(data.UserCol, function (nIndex, nObj) {
            $("#LeftUserList").append("<option value='" + nObj.Id + "'>" + nObj.UserName + "</option > ");
        });
    });
    //加载已经选择的用户
    if (parent.CPOrganSelectPage_SelUserIds != undefined && parent.CPOrganSelectPage_SelUserIds != null && parent.CPOrganSelectPage_SelUserIds != "")
    {
        var getSelUserUrl = CPWebRootPath + "/api/COOrganEngine/GetUserInfo";
        var objInput = new Object();
        objInput.UserIds = parent.CPOrganSelectPage_SelUserIds; 
        objInput.CurUserId = CPCurUserId; 
        objInput.CurUserIden = CPCurUserIden; 
        $.ajax({
            type: "POST",
            url: getSelUserUrl,
            data: JSON.stringify(objInput),
            contentType: 'application/json',
            success: function (data) {
                if (data.Result == false) {
                    alert("获取用户信息失败，详细信息：" + data.ErrorMsg);
                    return false;
                }
                else { 
                    $.each(data.UserCol, function (nIndex, nObj) {
                        $("#RightUserList").append("<option value='" + nObj.Id + "'>" + nObj.UserName + "</option > ");
                    });
                }
            }
        });
    }
    //加载已经选择的用户
  
});
//节点选择事件
function CPOrganSelNodeClick(dataItem, node)
{
    var getDepUrl = CPWebRootPath + "/api/COOrganEngine/GetUserByDep?DepId=" + dataItem.Id+ "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(getDepUrl, function (data) {
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        $("#LeftUserList").empty();
        $.each(data.UserCol, function (nIndex, nObj) {
            //nObj.Id, nObj.UserName;
            //console.log($('#RightUserList option[value=' + nObj.Id + ']').length);
            if ($('#RightUserList option[value=' + nObj.Id + ']').length <= 0) {
                $("#LeftUserList").append("<option value='" + nObj.Id + "'>" + nObj.UserName + "</option > ");
            }
        });
    });
}
//左侧双击
function LeftUserListdblclick() {
    $("#CPBtnSel").click();
}
//右侧双击
function RightUserListdblclick()
{
    $("#CPBtnDel").click();
}
//查询
function SearchTextOnClick(obj) {
    if (event.keyCode == 13) {
        $("#CPBtnSearch").click();
    }
}