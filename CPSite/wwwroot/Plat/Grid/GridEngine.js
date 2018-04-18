//全局传入参数start
//表单唯一标识符
var CPGridGlobal_GridCode = $.CPGetQuery("GridCode"); if (CPGridGlobal_GridCode == null || CPGridGlobal_GridCode == undefined) CPGridGlobal_GridCode = "";
//是否导出到Excel
var CPGridGlobal_IsExportToExcel = $.CPGetQuery("IsExportToExcel"); if (CPGridGlobal_IsExportToExcel == null || CPGridGlobal_IsExportToExcel == undefined) CPGridGlobal_IsExportToExcel = "false";
var CPGridGlobal_ExportToExcelPageSize = $.CPGetQuery("ExportToExcelPageSize"); if (CPGridGlobal_ExportToExcelPageSize == null || CPGridGlobal_ExportToExcelPageSize == undefined) CPGridGlobal_ExportToExcelPageSize = "20"; 
//当前设备
//1:PC浏览器   2：IOS手机 3 ：安卓手机4 ：IOS平板 5：安卓平板
var CPGridGlobal_DeviceType = $.CPGetQuery("DeviceType"); if (CPGridGlobal_DeviceType == null || CPGridGlobal_DeviceType == undefined) CPGridGlobal_DeviceType = "1";
//记录查询条件
var CPGridGlobal_OtherCondition = $.CPGetQuery("OtherCondition"); if (CPGridGlobal_OtherCondition == null || CPGridGlobal_OtherCondition == undefined) CPGridGlobal_OtherCondition = ""; 
var CPGridGlobal_CurPage = $.CPGetQuery("CurPage"); if (CPGridGlobal_CurPage == null || CPGridGlobal_CurPage == undefined) CPGridGlobal_CurPage = 1; 
//全局传入参数end

//全局变量start 
//全局表单对象
var CPGridGlobal_GridObj = null;  

//记录所有的查询字段名
var CPGridGlobal_AllSearchField = new Array();
var CPGridGlobal_AllSearchFieldTip = "";
//记录排序 字段
var CPGridGlobal_OrderBy = "";
/**
 *公用函数区start
 */
//刷新当前列表
function CPGridRefresh()
{
    var grid = $("#CPGirdDiv").data("kendoGrid");
    CPGridGlobal_CurPage = grid.pager.page();
    grid.destroy();
    $("#CPGirdDiv").html("");
    SetGridSearchCondition();
    InitGridAndLoadData();
}
//获取选中checkbox的值
function CPGridGetSelChkData()
{
    var sSelValue = "";
    $.each($(".CPGridChkCss"), function (nIndex, nObj) {
        if ($(nObj).is(':checked')) {
            if (sSelValue == "")
                sSelValue = $(nObj).val();
            else
                sSelValue += "@" +  $(nObj).val();
        }
    });
   
    return sSelValue;
}
//获取选中radio的值
function CPGridGetRadioSelData()
{
    var sSelValue = "";
    $.each($(".CPGridRadioCss"), function (nIndex, nObj) {
        if ($(nObj).is(':checked')) {
            if (sSelValue == "")
                sSelValue = $(nObj).val();
            else
                sSelValue += "@" + $(nObj).val();
        }
    });
    return sSelValue;
}
/**
 *公用函数区end
 */
function FormatGetDataUrl()
{
    var getDataUrl = CPWebRootPath + "/api/GridEngine/GetGridData?GridCode=" + CPGridGlobal_GridCode + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    getDataUrl += "&IsReturnRealData=true";
    getDataUrl += "&OtherCondition=" + escape(CPGridGlobal_OtherCondition);
    getDataUrl += "&OrderBy=" + escape(CPGridGlobal_OrderBy);
    //为了表达式，需要把URL里其它字段再加上
    //再加其它参数
    var sResultArray = CPGetQueryString();
    for (var mm = 0; mm < sResultArray.length; mm++) {
        var tempSarray = sResultArray[mm].split('=');
        if (tempSarray[0].toLowerCase() == "gridcode"
            || tempSarray[0].toLowerCase() == "curuserid"
            || tempSarray[0].toLowerCase() == "curuseriden"
            || tempSarray[0].toLowerCase() == "isreturnrealdata"
            || tempSarray[0].toLowerCase() == "currentpage"
            || tempSarray[0].toLowerCase() == "othercondition"
            || tempSarray[0].toLowerCase() == "orderby"
        )
            continue;
        getDataUrl += "&" + sResultArray[mm];
    }
    return getDataUrl;
}
//加载时设置html显隐
function SetHtmlShowOrHideWhenLoad()
{ 
    $("#divLoading").hide();
    $("#CPGridSearch").show();
    $("#CPGridButton").show();
    $("#CPGirdDiv").show();
    if (CPGridGlobal_AllSearchField.length <= 0) {
        $("#CPGridSearch").hide();
    }
    else {
        $("#CPGridSearchTxt").attr("placeholder", "可搜索" + CPGridGlobal_AllSearchFieldTip);
        $("#CPGridSearchTxt").attr("title", "可搜索" + CPGridGlobal_AllSearchFieldTip);
    }
}
//设置元素的宽度
function SetGridHeightAndWidth()
{
    var nWidth = $(window).width();
    nWidth = nWidth - $("#CPGridSearchTxt").width() - $("#CPBtnSearch").width()-48;
    $("#CPGridButton").width(nWidth);
}
//设置时间编辑控件
function SetGridEditTimeSelectControl() {
    var nArray = $(".layerTimeSel");
    var laydate = layui.laydate;
    $.each(nArray, function (nIndex, nObj) {
        laydate.render({
            elem: '#' + $(nObj).attr("id")
            , type: $(nObj).attr("data-timeFormat")
        });

    });
}

//设置操作按钮start
function InitGridFunc()
{
    var btnHtml = "";
    $.each(CPGridGlobal_GridObj.FuncCol, function (nIndex, nObj) {
        btnHtml += "<button type=\"button\" id=\"CPGrid_FuncBtn_" + nObj.Id + "\" class=\"CPGridFuncBtn_Css\"><i class=\"icon iconfont " + nObj.FuncIcon + "\"  ></i><span>" + nObj.FuncTitle + "</span></button>";
    });
    $("#CPGridButton").html(btnHtml);
    $.each(CPGridGlobal_GridObj.FuncCol, function (nIndex, nObj) {
        $("#CPGrid_FuncBtn_" + nObj.Id).kendoButton({
            click: function (e) {
                if (nObj.FuncType == 0)
                {
                    //执行自定义脚本
                    eval(nObj.FuncContext);
                }
                else if (nObj.FuncType == 1) {
                    //导出excel
                    //询问框
                    layer.confirm('请选择是导出本页数据还是导出全部数据？', {
                        btn: ['导出本页', '导出全部', '取消'] //按钮
                    }, function () {
                        //layer.msg('的确很重要', { icon: 1 });
                        //导出本页
                        layer.close(layer.index);
                        var grid = $("#CPGirdDiv").data("kendoGrid");
                        //console.log(grid);
                        var toUrl = "./GridView?othercondition=" + escape(CPGridGlobal_OtherCondition) + "&IsExportToExcel=true&ExportToExcelPageSize=" + grid.pager.options.dataSource._pageSize;
                        toUrl += "&CurPage=" + grid.pager.page();
                        //再加其它参数
                        var sResultArray = CPGetQueryString();
                        for (var mm = 0; mm < sResultArray.length; mm++) {
                            var tempSarray = sResultArray[mm].split('=');
                            if (tempSarray[0].toLowerCase() == "isexporttoexcel"
                                || tempSarray[0].toLowerCase() == "othercondition"
                                || tempSarray[0].toLowerCase() == "exporttoexcelpagesize"
                                || tempSarray[0].toLowerCase() == "curpage"
                            )
                                continue;
                            toUrl += "&" + sResultArray[mm];
                        }
                        OpenNewModel(toUrl, "导出Excel", 200, 100);
                    }, function () {
                        //导出全部
                        layer.close(layer.index);
                        var grid = $("#CPGirdDiv").data("kendoGrid");
                        var toUrl = "./GridView?othercondition=" + escape(CPGridGlobal_OtherCondition) + "&IsExportToExcel=true&ExportToExcelPageSize=99999999";
                        //再加其它参数
                        var sResultArray = CPGetQueryString();
                        for (var mm = 0; mm < sResultArray.length; mm++) {
                            var tempSarray = sResultArray[mm].split('=');
                            if (tempSarray[0].toLowerCase() == "isexporttoexcel"
                                || tempSarray[0].toLowerCase() == "othercondition"
                                || tempSarray[0].toLowerCase() == "exporttoexcelpagesize"
                                || tempSarray[0].toLowerCase() == "curpage"
                            )
                                continue;
                            toUrl += "&" + sResultArray[mm];
                        }
                        //window.open(toUrl);
                        OpenNewModel(toUrl, "导出Excel", 200, 100);
                    });
                    //alert(000);
                    
                }
                else if (nObj.FuncType == 2) {
                    //导出pdf
                }
                else if (nObj.FuncType == 3) {
                    //在本窗口打开页面
                    window.location.href = CPWebRootPath +nObj.FuncContext;
                }
                else if (nObj.FuncType == 4) {
                    //在新窗口打开页面
                    window.open(CPWebRootPath +nObj.FuncContext);
                }
               
                else if (nObj.FuncType == 5) {
                    //内置修改
                    CPGridUpdateData();
                    //内置修改
                }
                else if (nObj.FuncType == 6) {
                    //内置删除
                    CPGridDeleteData();
                }
                else if (nObj.FuncType == 7) {
                    //top.OpenNewModel
                    try {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "top", "");
                        top.OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                    catch (e) {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "");
                        OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                }
                else if (nObj.FuncType == 8) {
                    //top.OpenNewModel并且刷新
                    try {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "top", "CPGridRefresh()");
                        top.OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                    catch (e) {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "CPGridRefresh()");
                        OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                }
                else if (nObj.FuncType == 9) {
                    //Parent.OpenNewModel
                    try {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "parent", "");
                        parent.OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                    catch (e) {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "");
                        OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                }
                else if (nObj.FuncType == 10) {
                    //Parent.OpenNewModel并且刷新
                    try {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "parent", "CPGridRefresh()");
                        parent.OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                    catch (e) {
                        //f增加这个，用来告诉表单，保存后如何刷新原页面
                        var tmpUrlTmp = nObj.FuncContext;
                        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "CPGridRefresh()");
                        OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                    }
                }
                else if (nObj.FuncType == 11) {
                    //OpenNewModel
                    //f增加这个，用来告诉表单，保存后如何刷新原页面
                    var tmpUrlTmp = nObj.FuncContext;
                    tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "");
                    OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                }
                else if (nObj.FuncType == 12) {
                    //OpenNewModel并且刷新
                    //f增加这个，用来告诉表单，保存后如何刷新原页面
                    var tmpUrlTmp = nObj.FuncContext;
                    tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "CPGridRefresh()");
                    OpenNewModel(tmpUrlTmp, nObj.FuncTitle, nObj.OpenWinWidth, nObj.OpenWinHeight);
                }

            }
        });
    });
}
//设置操作按钮end
//列表列点击方法start
function CPGridColumnOpenLink(obj)
{
    var title = $(obj).attr("data-title");
    var TargetContent = $(obj).attr("data-TargetContent");
    var TargetType = $(obj).attr("data-TargetType");
    var OpenWinWidth = $(obj).attr("data-OpenWinWidth");
    var OpenWinHeight = $(obj).attr("data-OpenWinHeight");
    if (Number(TargetType) == 1)
    {
        //_self
        //自动添加，解决生产环境和开发环境的问题
        TargetContent = CPWebRootPath + TargetContent;
        window.location.href = TargetContent;
    }
    else if (Number(TargetType) == 2) {
        //blank
        //自动添加，解决生产环境和开发环境的问题
        TargetContent = CPWebRootPath + TargetContent;
        window.open(TargetContent);
    }
    else if (Number(TargetType) == 3) {
        //onclick
        eval(TargetContent);
    }
    else if (Number(TargetType) == 4) {
        //top.OpenNewModel
        try {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "top", "");
            top.OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
        catch (e) {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "");
            OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
    }
    else if (Number(TargetType) == 5) {
        //top.OpenNewModel并刷新
        try {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "top", "CPGridRefresh()");
            top.OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
        catch (e) {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "CPGridRefresh()");
            OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
    }
    
    else if (Number(TargetType) == 6) {
        //parent.OpenNewModel
        try {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "parent", "");
            parent.OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
        catch (e) {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "");
            OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
    }
    else if (Number(TargetType) == 7) {
        //parent.OpenNewModel并刷新 
        try {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "parent", "CPGridRefresh()");
            parent.OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
        catch (e) {
            //f增加这个，用来告诉表单，保存后如何刷新原页面
            var tmpUrlTmp = TargetContent;
            tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "CPGridRefresh()");
            OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
        }
    }
    else if (Number(TargetType) == 8) {
        //OpenNewModel
        //f增加这个，用来告诉表单，保存后如何刷新原页面
        var tmpUrlTmp = TargetContent;
        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "");
        OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
    }
    else if (Number(TargetType) == 9) {
        //OpenNewModel并刷新
        //f增加这个，用来告诉表单，保存后如何刷新原页面
        var tmpUrlTmp = TargetContent;
        tmpUrlTmp = FormatUrlForOpenNewModel(tmpUrlTmp, "self", "CPGridRefresh()");
        OpenNewModel(tmpUrlTmp, title, OpenWinWidth, OpenWinHeight);
    }
}

//列表列点击方法end
//创建列 方法start
function CPGridGetColumnAlign(columnItem)
{
    var align = "left";
    if (columnItem.ShowPosition == 1)
    {
        align = "left";
    }
    else if (columnItem.ShowPosition == 2) {
        align = "center";
    }
    else if (columnItem.ShowPosition == 3) {
        align = "right";
    }
    return align;
}
function CPGridFormColumnFooter(columnItem, kendoGridColumnItem)
{
    if (columnItem.IsShowSum) {
        kendoGridColumnItem.footerTemplate = function (dataItem) {
            var sumTip = "";
            if (columnItem.SumType == "Count") {
                sumTip = "计数:";
            }
            else if (columnItem.SumType == "Average") {
                sumTip = "平均值:";
            }
            else if (columnItem.SumType == "Sum") {
                sumTip = "合计:";
            }
            else if (columnItem.SumType == "Max") {
                sumTip = "最大值:";
            }
            else if (columnItem.SumType == "Min") {
                sumTip = "最小值:";
            }
            return sumTip + columnItem.TempSumValue
        };
        kendoGridColumnItem.footerAttributes = {
            //"class": "table-footer-cell",
           style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
        }
    }
    return kendoGridColumnItem;
}
var nLockIndex = 1;
function CPGridCreateColumn(columnItem, ListDataObj) {

    var gridId = "CPGirdDiv";
    if (columnItem.IsSearchShow) {
        CPGridGlobal_AllSearchField.push(columnItem.FieldName);
        if (CPGridGlobal_AllSearchFieldTip == "") {
            CPGridGlobal_AllSearchFieldTip = columnItem.ColumnTitle;

        } else {
            CPGridGlobal_AllSearchFieldTip += "," + columnItem.ColumnTitle;
        }
    }
    var kendoGridColumnItem = null;
    var bTmpIsShow = columnItem.IsShow;
    if (CPGridGlobal_IsExportToExcel == "true")
    {
        if (columnItem.IsCanExport == false)
            bTmpIsShow = false;
        if (columnItem.ColumnType == 2 || columnItem.ColumnType == 3)
        {
            //复选，单选不导出
            bTmpIsShow = false;
        }
    }
    if (bTmpIsShow) {
        var locked = false;
        if (nLockIndex <= CPGridGlobal_GridObj.LockColumnCount) {
            locked = true;
        }
        nLockIndex++;
        if (columnItem.IsShowSum)
        {
            CPGrid_IsHasFooter = true;
        }
        if (columnItem.ColumnType == 1) {
            //序号列
            kendoGridColumnItem={
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                filterable: false,
                sortable: false,
                locked: locked,
                width: 3 * 14,
                attributes: {
                    style: "text-align: center;"
                },
                headerAttributes: {
                    class: 'GridHeaderXH',
                    style: "text-align: center;vertical-align:middle"
                }
            };
            //序号列
        }
        else if (columnItem.ColumnType == 2) {
            //复选列
            kendoGridColumnItem={
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                filterable: false,
                sortable: false,
                locked: locked,
                width: 40,
                headerTemplate: '<input  class="CPGridChkCssAll"  type="checkbox" id="' + gridId + '_Chk_ChkAll" style="cursor:pointer;" data-gridid="' + gridId + '" onclick="GridChkAllClick(this);" /><label for=\"' + gridId + '_Chk_ChkAll\" ></label>',
                template: "<input type='checkbox'  class='CPGridChkCss'    id='" + gridId + "_Chk_#: " + "ColumnCPGridPK #' style='cursor:pointer;'  value='#: " + "ColumnCPGridPK #' data-gridid='" + gridId + "' data-childChk='true'  /><label for=\"" + gridId + "_Chk_#: " + "ColumnCPGridPK #\"></label>",
                attributes: {
                    style: "text-align: center;"
                },
                headerAttributes: {
                    class: 'GridHeaderXH',
                    style: "text-align: center;vertical-align:middle"
                }

            };
            //复选列
        }
        else if (columnItem.ColumnType == 3) {
            //单选列
            kendoGridColumnItem ={
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                filterable: false,
                sortable: false,
                locked: locked,
                width: 30,
                template: "<input type='radio'  class='CPGridRadioCss'  name='" + gridId + "_Radio' id='" + gridId + "_Radio_#: " + "ColumnCPGridPK #' style='cursor:pointer;'  value='#: " + "ColumnCPGridPK #' /><label for=\"" + gridId + "_Radio_#: " + "ColumnCPGridPK #\" style='cursor:pointer;'></label>",
                attributes: {
                    style: "text-align: center;padding-left:8px;"
                },
                headerAttributes: {
                    class: 'GridHeaderXH',
                    style: "text-align: center;padding-left:8px;padding-top:5px;"
                }

            };
            //单选列
        }
        else if (columnItem.ColumnType == 4) {
            //普通列

            kendoGridColumnItem ={
                template: function (dataItem) {
                    var sHtml = "";
                    var sStyle = "";
                    var sColumnValue = dataItem["Column" + columnItem.Id.toString()];
                    var sColumnValueAll = sColumnValue;
                    if (columnItem.MaxString > 0) {
                        // 配置了最多显示字符数
                        if (sColumnValue.length > columnItem.MaxString) {
                            sColumnValue = sColumnValue.substring(0, columnItem.MaxString) + "...";
                        }
                    }
                    if (sStyle != "") {
                        sHtml += "<span " + sStyle + "  title='" + sColumnValueAll + "'>" + sColumnValue + "</span>";
                    }
                    else {
                        //sHtml += sColumnValue;
                        sHtml += "<span  title='" + sColumnValueAll + "'>" + sColumnValue + "</span>";
                    }

                    return sHtml;
                },
                sortable: columnItem.IsCanOrder,
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                filterable: false,
                locked: locked,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            
            //普通列
        }
        else if (columnItem.ColumnType == 5) {
            //日期列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var sHtml = "";
                    var sStyle = "";
                    var sColumnValue = dataItem["Column" + columnItem.Id.toString()];
                    var sColumnValueAll = sColumnValue;
                    sHtml += "<span  title='" + sColumnValueAll + "'>" + sColumnValue + "</span>";

                    return sHtml;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                filterable: false,
                locked: locked,
                sortable: columnItem.IsCanOrder,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //日期列
        }
        else if (columnItem.ColumnType == 6) {
            //列表内置删除列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var sHtml = "";
                    var sTxt = "icon-close";
                    if (columnItem.ColumnIconOrText != "") {
                        sTxt = columnItem.ColumnIconOrText;
                    }
                    sHtml += "<a href='javascript:;'  class='GridHref'  onclick='CPGridRowInnerDelete(\"" + dataItem["ColumnCPGridPK"] + "\")'><i class=\"icon iconfont " + sTxt + "\"  style='font-size:22px;' ></i></a>";

                    return sHtml;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                filterable: false,
                locked: locked,
                sortable: false,
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //列表内置删除列
        }
        else if (columnItem.ColumnType == 7) {
            //模板列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var sColumnValue = dataItem["Column" + columnItem.Id.toString()];
                    return sColumnValue;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                filterable: false,
                locked: locked,
                sortable:false,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //模板列
        }
        else if (columnItem.ColumnType == 8) {
            //图片超链接列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var sHtml = "";
                    var sTxt = dataItem["Column" + columnItem.Id.toString()];
                    if (columnItem.ColumnIconOrText != "") {
                        sTxt = columnItem.ColumnIconOrText;
                    }
                    var sColor = "";
                    if (columnItem.ColumnIconOrTextColor != "") {
                        sColor = " color:" + columnItem.ColumnIconOrTextColor + "; ";
                    }
                    sHtml += "<a href='javascript:;'  class='GridHref' ";
                    sHtml += " data-title=\"" + dataItem["Column" + columnItem.Id.toString()] + "\"";
                    sHtml += " data-TargetContent=\"" + dataItem["ColumnTargetContent" + columnItem.Id.toString()]  + "\"";
                    sHtml += " data-TargetType=\"" + columnItem.TargetType + "\"";
                    sHtml += " data-OpenWinWidth=\"" + columnItem.OpenWinWidth  + "\"";
                    sHtml += " data-OpenWinHeight=\"" + columnItem.OpenWinHeight + "\"";
                    sHtml += "  onclick = 'CPGridColumnOpenLink(this)' > <i class=\"icon iconfont " + sTxt + "\"  style='font-size:22px;" + sColor + "' ></i></a>";

                    return sHtml;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                filterable: false,
                sortable: false,
                locked: locked,
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //图片超链接列
        }
        else if (columnItem.ColumnType == 9) {
            //文字超链接列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var sHtml = "";
                    var sTxt = dataItem["Column" + columnItem.Id.toString()];
                    if (columnItem.ColumnIconOrText != "") {
                        sTxt = columnItem.ColumnIconOrText;
                    }
                    sHtml += "<a href='javascript:;'  class='GridHref' ";
                    sHtml += " data-title=\"" + dataItem["Column" + columnItem.Id.toString()] + "\"";
                    sHtml += " data-TargetContent=\"" + dataItem["ColumnTargetContent" + columnItem.Id.toString()] + "\"";
                    sHtml += " data-TargetType=\"" + columnItem.TargetType + "\"";
                    sHtml += " data-OpenWinWidth=\"" + columnItem.OpenWinWidth + "\"";
                    sHtml += " data-OpenWinHeight=\"" + columnItem.OpenWinHeight + "\"";
                    sHtml += " onclick = 'CPGridColumnOpenLink(this)' > " + sTxt + "</a > ";

                    return sHtml;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                filterable: false,
                sortable: false,
                locked: locked,
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //文字超链接列
        }
        else if (columnItem.ColumnType == 10) {
            //文本框编辑列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var event = "";
                    if (columnItem.EventMethod != null && columnItem.EventMethod != "" && columnItem.EventName != null && columnItem.EventName != "") {
                        event = columnItem.EventName += "=\"" + columnItem.EventMethod + "\"";
                    }
                    var sHtml = "";
                    sHtml += "<input type='text' id='CPGridEditControl_" + columnItem.FieldName + "_" + dataItem["ColumnCPGridPK"].toString() + "' value=\"" + dataItem["Column" + columnItem.Id.toString()] + "\" class='CPGridEditTextBoxCss' " + event + " />"
                    return sHtml;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                filterable: false,
                sortable: columnItem.IsCanOrder,
                locked: locked,
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //文本框编辑列
        }
        else if (columnItem.ColumnType == 11) {
            //下拉框编辑列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var event = "";
                    if (columnItem.EventMethod != null && columnItem.EventMethod != "" && columnItem.EventName != null && columnItem.EventName != "") {
                        event = columnItem.EventName += "=\"" + columnItem.EventMethod + "\"";
                    }
                    var sHtml = "";
                    sHtml += "<select id='CPGridEditControl_" + columnItem.FieldName + "_" + dataItem["ColumnCPGridPK"].toString() + "' class='CPGridEditListCss' " + event + ">";
                    var tObjT = JSON.parse(ListDataObj["Column" + columnItem.Id]);
                    var curColumnValue = dataItem["Column" + columnItem.Id.toString()];
                    for (var i = 0; i < tObjT.length; i++) {
                        // alert(tObjT[i].servs);
                        var sSel = "";
                        //if (columnItem.FieldName == "IsShow")
                        //{
                        //    console.log(curColumnValue);
                        //    console.log(tObjT[i].valueEx);
                        //}
                        if (curColumnValue == tObjT[i].valueEx) {
                            sSel = " selected=\"selected\" ";
                        }
                        sHtml += "<option value=\"" + tObjT[i].valueEx + "\" " + sSel + ">" + tObjT[i].textEx + "</option>";
                    }
                    sHtml += "</select>";
                    return sHtml;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                filterable: false,
                sortable: columnItem.IsCanOrder,
                locked: locked,
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //下拉框编辑列
        }
        else if (columnItem.ColumnType == 12) {
            //时间选择编辑列
            kendoGridColumnItem ={
                template: function (dataItem) {
                    var event = "";
                    if (columnItem.EventMethod != null && columnItem.EventMethod != "" && columnItem.EventName != null && columnItem.EventName != "") {
                        event = columnItem.EventName += "=\"" + columnItem.EventMethod + "\"";
                    }
                    var timeFormat = CPTrim(columnItem.TimeFormat);
                    if (timeFormat == "")
                        timeFormat = "yyyy-MM-dd";
                    timeFormat = timeFormat.toLowerCase();
                    if (timeFormat == "yyyy-mm-dd" || timeFormat == "yyyy年mm月dd日" || timeFormat == "dd-mm-yy" || timeFormat == "yy-mm-dd") {
                        timeFormat = "date";
                    }
                    else if (timeFormat == "mm月dd日") {
                        timeFormat = "date";
                    }
                    else if (timeFormat == "yyyy年mm月" || timeFormat == "mm-yy") {
                        timeFormat = "month";
                    }
                    else if (timeFormat == "yyyy年mm月dd日  hh:mm" || timeFormat == "yyyy年mm月dd日  hh:mm:ss"
                        || timeFormat == "yyyy-mm-dd hh:mm" || timeFormat == "yyyy-mm-dd hh:mm:ss"
                    ) {
                        timeFormat = "datetime";
                    }
                    else if (timeFormat == "hh:mm"
                        || timeFormat == "hh:mm:ss"
                    ) {
                        timeFormat = "time";
                    }
                    var sHtml = "";
                    sHtml += "<input type='text' id='CPGridEditControl_" + columnItem.FieldName + "_" + dataItem["ColumnCPGridPK"].toString() + "' value=\"" + dataItem["Column" + columnItem.Id.toString()] + "\" class='layerTimeSel CPGridEditTextBoxCss' " + event + " data-timeFormat=\"" + timeFormat + "\" />"
                    return sHtml;
                },
                field: "Column" + columnItem.Id.toString(),
                title: columnItem.ColumnTitle,
                headerTemplate: "<span title=\"" + columnItem.ColumnTitle + "\">" + columnItem.ColumnTitle + "</span>",
                filterable: false,
                locked: locked,
                sortable: columnItem.IsCanOrder,
                width: columnItem.ShowWidth * 14 + 2,
                attributes: {
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                },
                headerAttributes: {
                    class: 'GridHeaderNormal',
                    style: "text-align:" + CPGridGetColumnAlign(columnItem) + ";"
                }
            };
            //时间选择编辑列
        }

        kendoGridColumnItem = CPGridFormColumnFooter(columnItem, kendoGridColumnItem);
    }
    
    return kendoGridColumnItem;
}
//创建列 方法end
function GetGridColumnByColumnId(columnId)
{
    var rItem;
    $.each(CPGridGlobal_GridObj.ColumnCol, function (nIndex, nObj) {
        if (Number(columnId) == Number(nObj.Id))
        {
            rItem = nObj;
            return;
        }
    });
    return rItem;
}
//初始化列表start
//记录是否显示底部统计值行
var CPGrid_IsHasFooter = false;

function InitGridAndLoadData()
{
    //加载 
    var url = CPWebRootPath + "/api/GridEngine/GetGridInfo?GridCode=" + CPGridGlobal_GridCode + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
    url += "&OtherCondition=" + escape(CPGridGlobal_OtherCondition);
    //为了表达式，需要把URL里其它字段再加上
    //再加其它参数
    var sResultArray = CPGetQueryString();
    for (var mm = 0; mm < sResultArray.length; mm++) {
        var tempSarray = sResultArray[mm].split('=');
        if (tempSarray[0].toLowerCase() == "gridcode"
            || tempSarray[0].toLowerCase() == "curuserid"
            || tempSarray[0].toLowerCase() == "curuseriden"
            || tempSarray[0].toLowerCase() == "othercondition"
        )
            continue;
        url += "&" + sResultArray[mm];
    }
    var getDataUrl = FormatGetDataUrl();    
    $.getJSON(url, function (data) {
        console.log(data);
        CPGridGlobal_GridObj = data.Grid;
        if (data.Result == false) {
            alert(data.ErrorMsg);
            return false;
        }
        //加载扩展脚本
        if (CPGridGlobal_GridObj.JsEx != null && CPGridGlobal_GridObj.JsEx != "") {
            // document.write(CPFormGlobal_FormObj.Config.UseSceneCol[0].FormScript)
            $("#divCPGridJSContainer").html(CPGridGlobal_GridObj.JsEx);
        }
        //设置页面标题
        $(document).attr("title", CPGridGlobal_GridObj.GridTitle);
        InitGridFunc();
        var gridId = "CPGirdDiv";//DIV ID
        var ListDataObj = null;
        if (data.ListDataJson != null && data.ListDataJson != "")
        {
            ListDataObj = JSON.parse(data.ListDataJson);
        }
        //绑定列类型start
        var kendoGridColumn = new Array();
        CPGridGlobal_AllSearchField.splice(0, CPGridGlobal_AllSearchField.length);
        CPGridGlobal_AllSearchFieldTip = "";
        nLockIndex = 1;
        $.each(data.Grid.HeaderGroup, function (nGroupIndex1, nGroupObj1) {
            if (nGroupObj1.GroupTitle == "")
            {
                //表示没有标题头，则直接加
                $.each(nGroupObj1.ChildColumnId, function (columnIdIndex, columnIdObj) {
                    var kColumn = CPGridCreateColumn(GetGridColumnByColumnId(columnIdObj), ListDataObj);
                    if (kColumn != null) {
                        kendoGridColumn.push(kColumn);
                    }
                });                
            }
            else {
                //title: "Location",
                //    columns: [{
                //        field: "Country",
                //        width: 200
                //    }, {
                //        field: "City",
                //        width: 200
                //    }]
                var kColumnRoot = new Object();
                kColumnRoot.title = nGroupObj1.GroupTitle;
                kColumnRoot.columns = new Array();    
                kColumnRoot.headerAttributes = {
                    class: 'GridHeaderXH',
                    style: "text-align: center;vertical-align:middle"
                };
                $.each(nGroupObj1.ChildGroupCol, function (childGroupIndex, childGroupObj) {
                    if (childGroupObj.GroupTitle == "") {
                        //表示没有标题头，则直接加
                        $.each(childGroupObj.ChildColumnId, function (columnIdIndex, columnIdObj) {
                            var kColumn = CPGridCreateColumn(GetGridColumnByColumnId(columnIdObj), ListDataObj);                            
                            if (kColumn != null) {
                                kColumnRoot.columns.push(kColumn);
                            }
                        });
                    }
                    else {
                        var kColumnChild = new Object();
                        kColumnChild.title = childGroupObj.GroupTitle;
                        kColumnChild.columns = new Array();  
                        kColumnChild.headerAttributes = {
                            class: 'GridHeaderXH',
                            style: "text-align: center;vertical-align:middle"
                        };
                        $.each(childGroupObj.ChildColumnId, function (columnIdIndex, columnIdObj) {
                            var kColumn = CPGridCreateColumn(GetGridColumnByColumnId(columnIdObj), ListDataObj);                           
                            if (kColumn != null) {
                                kColumnChild.columns.push(kColumn);
                            }
                        });
                        if (childGroupObj.ChildColumnId.length > 0) {
                            kColumnRoot.columns.push(kColumnChild);
                        }
                    }
                    
                });  
                if (nGroupObj1.ChildGroupCol.length > 0) {
                    kendoGridColumn.push(kColumnRoot);
                }

            }
        });
        //console.log(kendoGridColumn);
        //绑定列类型end

        //列表分组相关start
        var groupArray = new Array();
        if (data.Grid.IsGroup && data.Grid.GroupField != "") {
            var gArray = data.Grid.GroupField.split(',');
            $.each(gArray, function (nIndex, nObj) {
                nObj = CPTrim(nObj);
                if (nObj != "") {
                    groupArray.push({ field: nObj + "_CPGroup", title: data.Grid.GroupAlias });
                    kendoGridColumn.push({
                        field: nObj + "_CPGroup",
                        title: data.Grid.GroupAlias,
                        filterable: false,
                        hidden: true,
                        width: 1 * 14 + 2,
                        attributes: {
                            style: ""
                        },
                        headerAttributes: {
                            class: 'GridHeaderNormal'
                        }
                    });
                }
            });
            
        }
        //列表分组相关end

        var nHeight = $(window).height() - $("#CPGridSearch").height() - 20;
        if (CPGrid_IsHasFooter)
        {
            nHeight = nHeight - 32;
        }
        var pageSet = {
            refresh: false,
            pageSizes: true
        };
        var tmpPageSize = data.Grid.PageSize;
        if (data.Grid.IsPage == false)
        {
            pageSet = false;
            tmpPageSize = 9999999;
        }
        if (CPGridGlobal_IsExportToExcel == "true")
        {
            pageSet = false;
            tmpPageSize = Number(CPGridGlobal_ExportToExcelPageSize);
        }
        //创建列表start
       $("#" + gridId).kendoGrid({
            dataSource: {
                type: "json",
                transport: {
                    read: getDataUrl
                },
                schema: {
                    data: function (response) {
                        if (response.Result == false)
                        {
                            alert(response.ErrorMsg);
                            return false;
                        }
                        return jQuery.parseJSON(response.DataJson);
                        // return response.results; // twitter's response is { "results": [ /* results */ ] }
                    },
                    total: function (response) {
                        return response.RecordSize; // total is returned in the "total" field of the response
                    }
                },
                requestStart: function (e) {
                    if (data.Grid.LockColumnCount <= 0) {
                        //如果有锁定列，加上这个的话，会导致计算宽度出错，所以不能加
                        $("#CPGirdDiv").hide();
                    }
                },
                requestEnd: function (e) {
                    
                    var response = e.response;
                    var type = e.type;

                },
                page: CPGridGlobal_CurPage,
                pageSize: tmpPageSize,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
                group: groupArray
            },
            excel: {
                fileName: data.Grid.GridTitle + ".xlsx"
            },
            width:"800px",
            height: nHeight,
            filterable: false,
            sortable: true,//  mode: "multiple"
            pageable: pageSet,
            allowCopy: true,
            groupable: data.Grid.IsShowGroupArea,
            scrollable: true,//是否出现滚动条，只有列表所有列宽度大于屏幕宽度时，才设置成true
            selectable: "row",
            resizable: true,
            navigatable: false,//这个不能设置成false，否则右键里将不能获取当前是点击的是哪行
            columns: kendoGridColumn,
            change: function (e) {
                //行选中事件
               
                //if (event.button == 0 && event.ctrlKey == false && event.shiftKey == false) {
                //    //点击左键 并未按住ctrl 和shift键
                //}
                if (event.button == 0) {
                    //点击左键 并未按住ctrl 和shift键
                    var selectedRows = this.select();
                    var addedIdCol = new Array();
                    for (var i = 0; i < selectedRows.length; i++) {
                        var dataItem = this.dataItem(selectedRows[i]);
                        if ($.inArray(dataItem.ColumnCPGridPK, addedIdCol) == -1) {
                            //不知道为什么在，当列表有编辑列时，同一行会执行两次，所以只好做这个判断了。
                            addedIdCol.push(dataItem.ColumnCPGridPK);                         
                            //选中checkbox
                            var tmpCHK = document.getElementById("CPGirdDiv_Chk_" + dataItem.ColumnCPGridPK);
                            if (tmpCHK != undefined
                                &&
                                tmpCHK != null) {
                                if (tmpCHK.checked) {
                                    tmpCHK.checked = "";
                                }
                                else {
                                    tmpCHK.checked = "checked";
                                }
                            }
                        }
                    }
                }
            },
            excelExport: function (e) {
                parent.CloseNewModel();
            },
            dataBound: function (e) {               
                if (CPGridGlobal_IsExportToExcel == "true")
                {
                     var grid = $("#CPGirdDiv").data("kendoGrid");
                     grid.saveAsExcel();
                    
                }
                else {
                    SetHtmlShowOrHideWhenLoad();
                    SetGridHeightAndWidth();
                    SetGridEditTimeSelectControl();
                }
            },
            navigate: function (e) {
                var grid = e.sender;
                var curDataItem = grid.dataItem($(e.element).parent());
                //点击右键，自动选中行，原理同window资源管理器
                if (event.button == 2) {
                    //您点击了鼠标右键

                }
                else {
                    //您点击了鼠标左键
                }


            }
        });
        //创建列表end

    });
}
//初始化列表end

/***********layer uistart  ******/
layui.use('laydate', function () {
    //不能删除
});
/***********layer ui end  ******/

$(function () {
    //设置查询按钮
    $("#CPBtnSearch").kendoButton({
        imageUrl: "../../Style/CommonIcon/search.png",
        click: function (e) {
            CPGridSearch();
        }
    });
    InitGridAndLoadData()
});

/**
 * 查询文本框
 * @param {any} obj
 */
function SearchTextOnClick(obj) {
    $(obj).attr("placeholder", "");
    if (event.keyCode == 13) {
        CPGridSearch();
    }  
}
/*
 *查询方法 start
 */
function SetGridSearchCondition()
{
    CPGridGlobal_OtherCondition = "";
    var sValue = CPTrim($("#CPGridSearchTxt").val());
    if (sValue == "") {
        CPGridGlobal_OtherCondition = "";
    }
    else {
        $.each(CPGridGlobal_AllSearchField, function (nIndex, nObj) {
            if (CPGridGlobal_OtherCondition == "")
                CPGridGlobal_OtherCondition = " (" + nObj + " like '@" + sValue + "@') ";
            else
                CPGridGlobal_OtherCondition += " OR  (" + nObj + " like '@" + sValue + "@') ";
        });
    }
}
function CPGridSearch()
{
    var grid = $("#CPGirdDiv").data("kendoGrid");
    grid.destroy();
    $("#CPGirdDiv").html("");
    SetGridSearchCondition();
    CPGridGlobal_CurPage = 1;
    InitGridAndLoadData();
}
//查询方法 end
function GridChkAllClick(obj) {
    //CPGridChkCss
    //不知道为什么用JQUERY设置老是会有问题，所以改成最原始的
    if ($(obj).is(':checked')) {
        // do something
        $.each($(".CPGridChkCss"), function (nIndex, nObj) {
            document.getElementById($(nObj).attr("id")).checked = "checked";
        });
        
    }
    else {
        $.each($(".CPGridChkCss"), function (nIndex, nObj) {
            document.getElementById($(nObj).attr("id")).checked = "";
        });
    }
   
}
function GridChkItemClick(obj) {
   
}
//内置修改方法
function CPGridGetSelDataWhenUpdate()
{ 
    var Items = new Array();
    $.each($(".CPGridChkCss"), function (nIndex, nObj) {
        if ($(nObj).is(':checked')) {
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
            Items.push(tmp);
        }
    });
    return Items;
}
function CPGridUpdateData()
{
    var inputObj = new Object();
    inputObj.Items = CPGridGetSelDataWhenUpdate();
    if (inputObj.Items.length <= 0)
    {
        alert("请选择要修改的数据！");
        return false;
    }
    inputObj.GridCode = CPGridGlobal_GridObj.GridCode;
    inputObj.CurUserId = CPCurUserId;
    inputObj.CurUserIden = CPCurUserIden;
    var updateUrl = CPWebRootPath + "/api/GridEngine/UpdateGridData";
  
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
//列表内置删除方法
function CPGridRowInnerDelete(dataPK)
{
    if (confirm("确实要删除选中的数据吗？")) {
        var Url = CPWebRootPath + "/api/GridEngine/DeleteGridData?GridCode=" + CPGridGlobal_GridCode + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
        Url += "&DataPks=" + dataPK;
        //为了表达式，需要把URL里其它字段再加上
        //再加其它参数
        var sResultArray = CPGetQueryString();
        for (var mm = 0; mm < sResultArray.length; mm++) {
            var tempSarray = sResultArray[mm].split('=');
            if (tempSarray[0].toLowerCase() == "gridcode"
                || tempSarray[0].toLowerCase() == "curuserid"
                || tempSarray[0].toLowerCase() == "curuseriden"
                || tempSarray[0].toLowerCase() == "datapks"
            )
                continue;
            Url += "&" + sResultArray[mm];
        }
        $.getJSON(Url, function (data) {
            if (data.Result == false) {
                alert("删除失败，详细信息：" + data.ErrorMsg);
                return false;
            }
            else {
                alert("删除成功！");
                CPGridRefresh();
            }
        });
    }
}
//删除列表选中数据
function CPGridDeleteData()
{
    var sValue = CPGridGetSelChkData();
    if (sValue == "")
    {
        alert("请选择要删除的数据！");
        return false;
    }
    if (confirm("确实要删除选中的数据吗？"))
    {
        var Url = CPWebRootPath + "/api/GridEngine/DeleteGridData?GridCode=" + CPGridGlobal_GridCode + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden;
        Url += "&DataPks=" + sValue;
        //为了表达式，需要把URL里其它字段再加上
        //再加其它参数
        var sResultArray = CPGetQueryString();
        for (var mm = 0; mm < sResultArray.length; mm++) {
            var tempSarray = sResultArray[mm].split('=');
            if (tempSarray[0].toLowerCase() == "gridcode"
                || tempSarray[0].toLowerCase() == "curuserid"
                || tempSarray[0].toLowerCase() == "curuseriden"
                || tempSarray[0].toLowerCase() == "datapks"
            )
                continue;
            Url += "&" + sResultArray[mm];
        }
        $.getJSON(Url, function (data) {
            if (data.Result == false) {
                alert("删除失败，详细信息：" + data.ErrorMsg);
                return false;
            }
            else {
                alert("删除成功！");
                CPGridRefresh();
            }
        });
    }
    
}
//选择表达式
//nType: 0通用表达式  1列表  2表单   3树 4流程
var SelectExpression_ThisObj;
function SelectExpression_SetReturn() {
    //alert(CPShowModalDialogReturnArgs);
    var obj = $(SelectExpression_ThisObj);
    var sValue = $(obj).val();
    sValue = sValue.replace("{}", CPShowModalDialogReturnArgs);
    $(obj).val(sValue);
}
function SelectExpression(thisObj, nType) {
    var sValue = $(thisObj).val();
    if (sValue.indexOf("{}") != -1) {
        SelectExpression_ThisObj = thisObj;
        var url = CPWebRootPath + "/Plat/Common/SelectExp?Type=" + nType;
        //   CPOpenDialog(url, "选择表达式", 700, 600);
        var nHeight = 600;
        var nWidth = 750;
        var iTop = (window.screen.availHeight - 30 - nHeight) / 2;       //获得窗口的垂直位置;
        var iLeft = (window.screen.availWidth - 10 - nWidth) / 2;           //获得窗口的水平位置;
        var name = "新窗口_" + Math.random();

        //IE9下面不能加name
        window.open(url, '', "height=" + nHeight + ", width=" + nWidth + ",top=" + iTop + ",left=" + iLeft + ", location=yes,menubar=no,resizable=yes,scrollbars=yes,status=no,toolbar=no"); //写成一行

        return;
    }
}
//修改配置
function CPGridUpdateConfig(gridId)
{
    var url = "/Plat/Tab/TabView?TabCode=Tab0001&DeviceType=1&SysId=1&TargetGridId=" + gridId;
    try {
        top.OpenNewModel(url, "修改配置", 9000, 9000);
    }
    catch (e)
    {
        OpenNewModel(url, "修改配置", 9000, 9000);
    }
}
 