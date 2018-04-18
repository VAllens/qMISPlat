//初步化表单字段信息
function InitField(TargetFormId) {
    var url = CPWebRootPath + "/api/FormEngine/SynFormFieldInfo?FormId=" + TargetFormId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
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
//初步化内置排版信息
function InitInnerView(TargetFormId) {
    var url = CPWebRootPath + "/api/FormEngine/InitFormDefaultView?FormId=" + TargetFormId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(url, function (data) {
        if (data.Result == false) {
            alert("初始化失败，详细信息：" + data.ErrorMsg);
            return false;
        }
        else {
            alert("初始化成功！");
            CPGridRefresh();
        }

    });
} 
//添加 一使用编辑器排版的视图
function InitInnerViewForEditor(TargetFormId) {
    var url = CPWebRootPath + "/api/FormEngine/InitFormDefaultViewForEditor?FormId=" + TargetFormId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(url, function (data) {
        if (data.Result == false) {
            alert("初始化失败，详细信息：" + data.ErrorMsg);
            return false;
        }
        else {
            alert("初始化成功！");
            CPGridRefresh();
        }

    });
} 
//修改初始化组配置
function UpdateFormInitInfo()
{
    var GroupId = $.CPGetQuery("GroupId");
    var FormId = $.CPGetQuery("FormId");
    var inputObj = new Object();
    //调用列表方法
    inputObj.Items = new Array();
    $.each($(".CPGridChkCss"), function (nIndex, nObj) {
        var tmp = new Object();
        tmp.DataPK = $(nObj).val();
        tmp.FieldNamCol = new Array();
        tmp.FieldValueCol = new Array();
        $.each(CPGridGlobal_GridObj.ColumnCol, function (cIndex, cObj) {
            if (cObj.ColumnType == 10
                || cObj.ColumnType == 11
                || cObj.ColumnType == 12
            ) {
                tmp.FieldNamCol.push(cObj.FieldName);
                tmp.FieldValueCol.push($("#CPGridEditControl_" + cObj.FieldName + "_" + tmp.DataPK).val());
            }
        });
        inputObj.Items.push(tmp);
    });
    if (inputObj.Items.length <= 0) {
        alert("请选择要修改的数据！");
        return false;
    }
    inputObj.GroupId = GroupId;
    inputObj.FormId = FormId;
    inputObj.CurUserId = CPCurUserId;
    inputObj.CurUserIden = CPCurUserIden;
    var updateUrl = CPWebRootPath + "/api/FormEngine/UpdateFormInitData";

    $.ajax({
        type: "POST",
        url: updateUrl,
        data: JSON.stringify(inputObj),
        contentType: 'application/json',
        success: function (data) {
            if (data.Result == false) {
                alert("修改失败，详细信息：" + data.ErrorMsg);
                return false;
            }
            else {
                alert("修改成功！");
                CPGridRefresh();
            }
        }
    });
}
//获取表单预览地址
function PreviewForm()
{
    if (CPFormGlobal_FormObj.Data.Form_Preview.SceneCode == "")
    {
        alert("请选择使用场景！");
        return;
    }
    if (CPFormGlobal_FormObj.Data.Form_Preview.ViewCode == "") {
        alert("请选择表单视图！");
        return;
    }
    var url = CPWebRootPath + "/Plat/Form/FormView?FormCode=" + CPFormGlobal_FormObj.Data.Form_Preview.FormCode +
        "&SceneCode=" + CPFormGlobal_FormObj.Data.Form_Preview.SceneCode +
        "&ViewCode=" + CPFormGlobal_FormObj.Data.Form_Preview.ViewCode +
        "&DeviceType=" + CPFormGlobal_FormObj.Data.Form_Preview.DeviceType;
    if (CPFormGlobal_FormObj.Data.Form_Preview.InitGroupCode != "")
    {
        url += "&InitGroupCode=" + CPFormGlobal_FormObj.Data.Form_Preview.InitGroupCode;
    }
    if (CPFormGlobal_FormObj.Data.Form_Preview.RightGroupCode != "") {
        url += "&RightGroupCode=" + CPFormGlobal_FormObj.Data.Form_Preview.RightGroupCode;
    }
    window.open(url);
}
function SetFormView(viewId, ViewCode, ViewType) {
    if (ViewType == "3") {
        var url = "/Plat/Form/FormDesign?ViewId=" + viewId;
        top.OpenNewModel(url, "表单排版配置", 9200, 9200);
    }
    else {
        var url = "/Plat/Grid/GridView?GridCode=Grid201709180801120005&ViewId=" + viewId;
        parent.OpenNewModel(url, "表单排版配置", 1200, 1000);
    }
  
}
var AutoGetDbInfo_DbInfo;
function AutoGetDbInfo()
{ 
    AutoGetDbInfo_DbInfo = null;
    var url = CPWebRootPath + "/api/FormEngine/GetDbInfo?dbIns=" + CPFormGlobal_Scope.GetFieldValue("Form_Main", "DbIns", 0) + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    $.getJSON(url, function (data) {
        if (data.Result == false) {
            alert("获取数据库信息失败，详细信息：" + data.ErrorMsg);
            return false;
        }
        else {
            AutoGetDbInfo_DbInfo = data;
            CPFormGlobal_Scope.SetFieldValue("Form_Main", "DbName", 0, data.DbName);
            CPFormGlobal_Scope.SetFieldValue("Form_Main", "MainTableName", 0, "");
            CPFormGlobal_Scope.SetFieldValue("Form_Main", "PKFieldName", 0, "");
            $.each(CPFormGlobal_FormObj.Data.Form_ChildTable, function (nIndex, nObj) {
                CPFormGlobal_FormObj.Data.Form_ChildTable[nIndex]["PKFieldName"] = "";
                CPFormGlobal_FormObj.Data.Form_ChildTable[nIndex]["TableName"] = "";
            });
            var tmpArray = new Array();
            $.each(data.TableCol, function (nIndex, nObj) {
                var o = new Object();
                o.textEx = nObj.TableName;
                o.valueEx = nObj.TableName;
                o.listRelateEx = "";
                tmpArray.push(o);
            }); 
            CPFormGlobal_FormObj.Data.Form_Main_MainTableName = tmpArray;
            //子表表名
            CPFormGlobal_FormObj.Data.Form_ChildTable_TableName = tmpArray;

        }

    });
}
function AutoAddPKName()
{

    var MainTableName = CPFormGlobal_Scope.GetFieldValue("Form_Main", "MainTableName", 0);
    if (AutoGetDbInfo_DbInfo != null)
    {
        $.each(AutoGetDbInfo_DbInfo.TableCol, function (nIndex, nObj) {
            if (nObj.TableName == MainTableName)
            {
                CPFormGlobal_Scope.SetFieldValue("Form_Main", "PKFieldName", 0, nObj.PKNames);
                return;
            }
        });
    }
}
function ChildTableAutoAddPKName() { 
    var TableName = CPFormGlobal_Scope.GetFieldValue("Form_ChildTable", "TableName", CPFormGlobal_CurControlDataRowIndex);
    if (AutoGetDbInfo_DbInfo != null) {
        $.each(AutoGetDbInfo_DbInfo.TableCol, function (nIndex, nObj) {
            if (nObj.TableName == TableName) {
                CPFormGlobal_Scope.SetFieldValue("Form_ChildTable", "PKFieldName", CPFormGlobal_CurControlDataRowIndex, nObj.PKNames);
                return;
            }
        });
    }
}

//导出配置
function FormExportConfig() {
    var sSel = CPGridGetSelChkData();
    if (sSel == "") {
        alert("请选择要导出的表单！");
        return false;
    }
    var url = CPWebRootPath + "/api/FormEngine/DownloadFormConfig?FormIds=" + sSel + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    window.location.href = url;
}
//更新配置
function FormUpateConfig(sysId) {
    var url = CPWebRootPath + "/Plat/Form/ManaConfig?IsCreateNew=false&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "更新配置", 400, 200);
}
//从配置文件创建新列表
function FormCreateByConfigFile(sysId) {
    var url = CPWebRootPath + "/Plat/Form/ManaConfig?IsCreateNew=true&TargetSysId=" + sysId + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    OpenNewModel(url, "从配置文件全新创建表单", 400, 200);
}