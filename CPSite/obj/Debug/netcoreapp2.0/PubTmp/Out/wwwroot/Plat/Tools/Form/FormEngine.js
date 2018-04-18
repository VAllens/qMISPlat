//全局传入参数start
//表单唯一标识符
var CPFormGlobal_FormCode = $.CPGetQuery("FormCode"); if (CPFormGlobal_FormCode == null || CPFormGlobal_FormCode == undefined) CPFormGlobal_FormCode = "";
//使用场景
var CPFormGlobal_SceneCode = $.CPGetQuery("SceneCode"); if (CPFormGlobal_SceneCode == null || CPFormGlobal_SceneCode == undefined) CPFormGlobal_SceneCode = "";
//使用视图，可能为空，则取默认的
var CPFormGlobal_ViewCode = $.CPGetQuery("ViewCode"); if (CPFormGlobal_ViewCode == null || CPFormGlobal_ViewCode == undefined) CPFormGlobal_ViewCode = "";
//初始化组，可能为空
var CPFormGlobal_InitGroupCode = $.CPGetQuery("InitGroupCode"); if (CPFormGlobal_InitGroupCode == null || CPFormGlobal_InitGroupCode == undefined) CPFormGlobal_InitGroupCode = "";
//权限组,可能为空
var CPFormGlobal_RightGroupCode = $.CPGetQuery("RightGroupCode"); if (CPFormGlobal_RightGroupCode == null || CPFormGlobal_RightGroupCode == undefined) CPFormGlobal_RightGroupCode = "";
//主键值
var CPFormGlobal_PKValues = $.CPGetQuery("PKValues"); if (CPFormGlobal_PKValues == null || CPFormGlobal_PKValues == undefined) CPFormGlobal_PKValues = "";
//当前用户ID
var CPFormGlobal_CurUserId = $.CPGetQuery("CurUserId"); if (CPFormGlobal_CurUserId == null || CPFormGlobal_CurUserId == undefined) CPFormGlobal_CurUserId = "";
//当前用户唯一标识
var CPFormGlobal_CurUserIden = $.CPGetQuery("CurUserIden"); if (CPFormGlobal_CurUserIden == null || CPFormGlobal_CurUserIden == undefined) CPFormGlobal_CurUserIden = "";
//当前设备
//1:PC浏览器   2：IOS手机 3 ：安卓手机4 ：IOS平板 5：安卓平板
var CPFormGlobal_DeviceType = $.CPGetQuery("DeviceType"); if (CPFormGlobal_CurUserIden == null || CPFormGlobal_CurUserIden == undefined) CPFormGlobal_CurUserIden = "1";
//全局传入参数end

//全局变量start
var CPFormGlobal_Scope = null;
//全局表单对象
var CPFormGlobal_FormObj = new Object();

//全局变量end
//http://localhost:9000/CPSite/plat/tools/form/formview.html?FormCode=Form0001&SceneCode=Scene0001&ViewCode=View201706280837070001&InitGroupCode=Group0001&RightGroupCode=&CurUserId=1&CurUserIden=E0A04F94-90D5-4C2A-8C9D-836813F73DC4&PKValues=1&DeviceType=1&SysId=1
angular.module('CPFormEngineApp',[]).controller('MyCtrl', function ($scope, $sce, $http, $timeout, $filter) {
   
    var formUrl = "/api/FormEngine/GetFormInfo?FormCode=" + CPFormGlobal_FormCode + "&SceneCode=" + CPFormGlobal_SceneCode + "&ViewCode=" + CPFormGlobal_ViewCode
                     + "&InitGroupCode=" + CPFormGlobal_InitGroupCode + "&RightGroupCode=" + CPFormGlobal_RightGroupCode + "&PKValues=" + CPFormGlobal_PKValues
                     + "&CurUserId=" + CPFormGlobal_CurUserId + "&CurUserIden=" + CPFormGlobal_CurUserIden + "&DeviceType=" + CPFormGlobal_DeviceType;
    //为了表单初始化，需要把URL里其它字段再加上
    //再加其它参数
    var sResultArray = CPGetQueryString();
    for (var mm = 0; mm < sResultArray.length; mm++) {
        var tempSarray = sResultArray[mm].split('=');
        if (tempSarray[0].toLowerCase() == "formcode"
        || tempSarray[0].toLowerCase() == "scenecode"
        || tempSarray[0].toLowerCase() == "viewcode"
        || tempSarray[0].toLowerCase() == "initgroupcode"
        || tempSarray[0].toLowerCase() == "rightgroupcode"
        || tempSarray[0].toLowerCase() == "pkvalues"
        || tempSarray[0].toLowerCase() == "curuserid"
        || tempSarray[0].toLowerCase() == "curuseriden"
        || tempSarray[0].toLowerCase() == "devicetype"
        )
            continue;
        formUrl += "&" + sResultArray[mm];
    }

    $http.get(formUrl).then(function (data) {
        //存储当前用户对象信息
        if (data.data.Result == false) {
            alert(data.data.ErrorMsg);
            return false;
        }
        //console.log(data);
        CPFormGlobal_FormObj.Config = data.data.Form;
        //加载扩展脚本
        if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormScript != null && CPFormGlobal_FormObj.Config.UseSceneCol[0].FormScript != "") {
            // document.write(CPFormGlobal_FormObj.Config.UseSceneCol[0].FormScript)
            $("#divCPFormJSContainer").html(CPFormGlobal_FormObj.Config.UseSceneCol[0].FormScript);
        }
        CPFormGlobal_FormObj.FormHtml = data.data.ViewHTML;// "<p><input type='text'  ng-bind='FormObj.Data.UserName' value='{{FormObj.Data.UserName}}' /></p>";
        var tmpObj = JSON.parse(data.data.FormDataJSON);
        //console.log(JSON.parse(tmpObj[CPFormGlobal_FormObj.Config.MainTableName]));
        //主表数据反序列化
        tmpObj[CPFormGlobal_FormObj.Config.MainTableName] = JSON.parse(tmpObj[CPFormGlobal_FormObj.Config.MainTableName]);
        tmpObj[CPFormGlobal_FormObj.Config.MainTableName] = tmpObj[CPFormGlobal_FormObj.Config.MainTableName][0];
        //子表数据反序列化
        if (CPFormGlobal_FormObj.Config.ChildTableCol != undefined && CPFormGlobal_FormObj.Config.ChildTableCol != null) {
            $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nIndex, nObj) {
                tmpObj[nObj.TableName] = JSON.parse(tmpObj[nObj.TableName]);
                //再看看没有拓展表初始化的表 
                if(tmpObj[nObj.TableName + "_ExtendTableInitValue"] != undefined
                    && tmpObj[nObj.TableName + "_ExtendTableInitValue"] != null)
                {
                    tmpObj[nObj.TableName + "_ExtendTableInitValue"] = JSON.parse(tmpObj[nObj.TableName + "_ExtendTableInitValue"]);
                }
            });
        }
        //下拉列表等数据源反序列化
        $.each(CPFormGlobal_FormObj.Config.FieldCol, function (nIndex, nObj) {
            if (nObj.ControlType == 3 || nObj.ControlType == 4 || nObj.ControlType == 5 || nObj.ControlType == 6) {
                tmpObj[nObj.TableName + "_" + nObj.FieldName] = JSON.parse(tmpObj[nObj.TableName + "_" + nObj.FieldName]);
            }
        });
        //给主数据和拓展表数据增加一字段，用来记录行索引号，用来拓展表删除等使用start
        tmpObj[CPFormGlobal_FormObj.Config.MainTableName].CPFormDataIndex = 0;
        if (CPFormGlobal_FormObj.Config.ChildTableCol != undefined && CPFormGlobal_FormObj.Config.ChildTableCol != null) {
            $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nIndex, nObj) {

                $.each(tmpObj[nObj.TableName], function (nDataIndex, nDataObj) {
                    nDataObj.CPFormDataIndex = nDataIndex;
                });
            });
        }

        //给主数据和拓展表数据增加一字段，用来记录行索引号，用来拓展表删除等使用end
        CPFormGlobal_FormObj.Data = tmpObj;
        console.log(CPFormGlobal_FormObj);
        $scope.FormObj = CPFormGlobal_FormObj;
    });
   
    
    //拓展表添加 新行start
    $scope.CPFormAddChildRow = function (btnId,isClickButton) { 
        var exTableName = $("#" + btnId).attr("data-TableName");
       
        var nObj = new Object();
        //先找出子表与主表的关联字段，除关联字段外，其它的字段的值都设置成空
        var relateFieldName = "";
        $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nIndex, nObj) {
            if(nObj.TableName == exTableName)
            {
                relateFieldName = nObj.RelateFieldName;
                return;
            }
        });
        var maxCPFormDataIndex = 0;
        $.each(CPFormGlobal_FormObj.Data[exTableName], function (nIndex, nObj) {
            if(nObj.CPFormDataIndex>maxCPFormDataIndex)
            {
                maxCPFormDataIndex = nObj.CPFormDataIndex;
            }
        });
        //待复制的行
        var sourceRowValue = CPFormGlobal_FormObj.Data[exTableName][CPFormGlobal_FormObj.Data[exTableName].length-1];
        for (var item in sourceRowValue) {
            //alert("person中" + item + "的值=" + person[item]);
            if (item == "$$hashKey")
                continue;
            if (item == relateFieldName) {
                nObj[item] = sourceRowValue[item];
            }
            else if(item == "CPFormDataIndex")
            {
                //取最大值 加1
                nObj["CPFormDataIndex"] = maxCPFormDataIndex + 1;
            }
            else {
                //再取初始化字段的值_ExtendTableInitValue
                var objDT = CPFormGlobal_FormObj.Data[exTableName + "_ExtendTableInitValue"];
                //console.log(objDT);
                if (objDT != undefined && objDT != null && objDT.length > 0) {
                    nObj[item] = objDT[0][item];
                }
                else {
                    nObj[item] = "";
                }
            }
        }
        CPFormGlobal_FormObj.Data[exTableName].push(nObj);
        //console.log(CPFormGlobal_FormObj.Data);
        if (isClickButton == false) {
            //不知道为什么，如果点击添加按钮调用此方法时，再执行这个函数则会出错
            $scope.$apply();
        }
    };
    //拓展表添加 新行end

    //拓展表删除行start
    $scope.CPFormDelChildRow = function (CPFormDataIndex, tableName) {
        if (CPFormGlobal_FormObj.Data[tableName].length <= 1)
        {
            alert("至少得留一行，不能删除了！");
            return;
        }
        var removeIndex = -1;
        $.each(CPFormGlobal_FormObj.Data[tableName], function (nIndex, nObj) {
            if(Number(nObj.CPFormDataIndex) == Number(CPFormDataIndex))
            {
                removeIndex = nIndex;
                return;
            }
        });
        CPFormGlobal_FormObj.Data[tableName].splice(removeIndex, 1);
        //console.log(CPFormGlobal_FormObj.Data[tableName]);
        $scope.$apply();
        $scope.SetFormDivContainerHeight();
    };
    //拓展表删除行end
    
    //设置表单主体的高度start
    $scope.SetFormDivContainerHeight = function()
    {
        var nHeight = $(window).height();
        nHeight = nHeight - $("#CPFormButton").height() - 40;
        if (nHeight < $("#CPFormMain").height()) {
            $("#CPFormMain").height(nHeight);
        }
    }
    //设置表单主体的高度end
    
    //设置表单checkbox默认选中状态start
    $scope.SetCheckBoxFieldChecked = function (fieldValue,curChkValue) {
        if (fieldValue == null || fieldValue == "")
            return false;
        var sArray = fieldValue.split(',');
        if ($.inArray(curChkValue, fieldValue) == -1)
            return false;
        else
            return true;
    };
    //设置表单checkbox默认选中状态end

    //设置表单radio默认选中状态start
    $scope.SetRadioFieldChecked = function (fieldValue, curChkValue) {
        if (fieldValue == null || curChkValue == null)
            return false;
        if (fieldValue == curChkValue)
            return true;
        else
            return false;
    };
    //设置表单radio默认选中状态end


    //设置表单checkbox的值start
    $scope.SetCheckBoxFieldValue = function (isExtendTable,extendTableDataIndex, tableName, fieldName, formFieldId) {
        //var extendTableDataIndex = 1;
        var sArray = $("input[name='CPForm_" + formFieldId + "']");
        var sValue = "";
        $.each(sArray, function (nIndex, nObj) {
            if(nObj.checked)
            {
                if (sValue == "")
                    sValue = nObj.value;
                else
                    sValue += "," + nObj.value;
            }
        });
        if(isExtendTable=="true")
        {
            //拓展表
            $.each(CPFormGlobal_FormObj.Data[tableName], function (nIndex, nObj) {
                if(Number(nObj.CPFormDataIndex) == Number(extendTableDataIndex))
                {
                    nObj[fieldName] = sValue;
                    return;
                }
            });
        }
        else {
            //主表
            CPFormGlobal_FormObj.Data[tableName][fieldName] = sValue;
        }
    };
    //设置表单checkbox的值end

    //设置combox样式start
    $scope.SetComboxSkin = function () {
        $(".comboxwrapper").each(function () {
            $(this).children(".comboxinput-wrapper").click(function (event) {
                content = $(this).parent().children(".comboxcontent");
                //阻止事件想下传递
                event.stopPropagation();
                //点击展开、收起
                if (content.css("display") == "none") {
                    $(".comboxcontent").each(function () {
                        $(this).hide();
                        $(this).css("opacity", "0");
                        $(this).css("top", "84px");
                        $(".comboxinput-wrapper").children("img").css("transform", "rotate(0deg)");
                    });
                    content.show();
                    $(this).children("img").css("transform", "rotate(180deg)");
                    /*设置一个延时以便出现后续的动画*/
                    setTimeout(myAnimate1, 100);
                    function myAnimate1() {
                        content.css("opacity", "1");
                        content.css("top", "34px");
                    }
                } else {
                    content.css("opacity", "0");
                    content.css("top", "84px");
                    $(".comboxinput-wrapper").children("img").css("transform", "rotate(0deg)");
                    setTimeout(myAnimate2, 100);
                    function myAnimate2() {
                        content.hide();
                    }
                }
                //点击内容之外的地方收起
                $(document).click(function () {
                    content.css("opacity", "0");
                    content.css("top", "84px");
                    $(".comboxinput-wrapper").children("img").css("transform", "rotate(0deg)");
                    setTimeout(myAnimate2, 100);
                    function myAnimate2() {
                        content.hide();
                    }
                });
                content.click(function (event) {
                    //阻止本身事件向下传递
                    event.stopPropagation();
                });
            });
            //列表点击事件
            $(this).children(".comboxcontent").children("li").each(function (value, element) {
                $(this).click(function () {
                    var inputObj = $(this).parent().parent().children(".comboxinput-wrapper").children("input");
                    inputObj.val($(this).html()); //点击替换input的值
                    var ngModel = inputObj.attr("ng-model");                    
                    var datatablerowindex = inputObj.attr("data-datatablerowindex");
                    var sArray = ngModel.split('.');
                    //检查是不是拓展表的字段start
                    var bExtendTableField = false;
                    $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nTableIndex, nTableObj) {
                        if (nTableObj.TableName == sArray[2]) { 
                            bExtendTableField = true;
                            return;
                        }
                    });
                    //检查是不是拓展表的字段end
                    if (bExtendTableField)
                    {
                        CPFormGlobal_FormObj.Data[sArray[2]][datatablerowindex][sArray[3]] = $(this).html();
                    }
                    else {
                        //FormObj.Data.Form_Main.FuncTitle
                        CPFormGlobal_FormObj.Data[sArray[2]][sArray[3]] = $(this).html();
                    }
                    
                    $(this).parent().children("li").removeClass("active");
                    $(this).addClass("active");
                    $(".comboxinput-wrapper").children("img").css("transform", "rotate(0deg)");
                    $(this).parent().css("opacity", "0");
                    $(this).parent().css("top", "84px");
                    setTimeout(myAnimate3, 100);
                    function myAnimate3() {
                        $(".comboxcontent").hide();
                    }
                })
            });
        });
    };
    //设置combox样式end

    //保存表单前数据校验start
    $scope.CheckData = function () {
        //console.log(CPFormGlobal_FormObj.Data);
        var errorMsg = "";
        $.each(CPFormGlobal_FormObj.Config.FieldCol, function (nIndex, nObjField) {
            if (nObjField.IsChildTable)
                return;
            //检查是不是拓展表的字段start
            var bExtendTableField = false;
            var ExtendTableObj = null;
            $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nTableIndex, nTableObj) {
                if (nTableObj.TableName == nObjField.TableName)
                {
                    ExtendTableObj = nTableObj;
                    bExtendTableField = true;
                    return;
                }
            });
            //检查是不是拓展表的字段end
            var sTip = "";
            if (nObjField.NotAllowNullTip == null || nObjField.NotAllowNullTip == "")
            {
                sTip = nObjField.FieldTitle + "不允许为空";
            }
            else {
                sTip = nObjField.NotAllowNullTip;
            }
            //先看看这个字段是不是不允许为空，或者这个字段是不是有校验的规则
            if(bExtendTableField==false)
            {
                //主表数据start
                if (nObjField.FieldName != CPFormGlobal_FormObj.Config.PKFieldName)
                {
                    if (nObjField.IsAllowNull == false)
                    {
                        var sValue = CPFormGlobal_FormObj.Data[CPFormGlobal_FormObj.Config.MainTableName][nObjField.FieldName];
                        if(sValue == "")
                        {
                            if(errorMsg == "")
                            {
                                errorMsg = sTip;
                            }
                            else {
                                errorMsg += "\n" + sTip;
                            }
                        }
                    }
                }
                //主表数据end
            }
            else {
                //从表数据start
                $.each(CPFormGlobal_FormObj.Data[ExtendTableObj.TableName], function (nCDataIndex, nCDataObj) {
                    if (nObjField.FieldName != ExtendTableObj.PKFieldName
                        && nObjField.FieldName != ExtendTableObj.RelateFieldName
                        ) {
                        if (nObjField.IsAllowNull == false) {
                            var sValue = nCDataObj[nObjField.FieldName];
                            if (sValue == "") {
                                if (errorMsg == "") {
                                    errorMsg = sTip;
                                }
                                else {
                                    errorMsg += "\n" + sTip;
                                }
                            }
                        }
                    }
                });
                //从表数据end
            }

        });
        //再看看规则里有没有数据校验start
        $.each(CPFormGlobal_FormObj.Config.FieldRuleCol, function (nIndex, nObj) {
            //规则
            if (nObj.RuleType == 1) {
                //不满足返回false，满足返回任何值
                var sCondition = nObj.RuleCondition;
                if (sCondition.indexOf("[index]") != -1)
                {
                    //拓展表
                    var fieldObj = $scope.GetFieldObj(nObj.FieldId);
                    console.log(CPFormGlobal_FormObj.Data[fieldObj.TableName]);
                    $.each(CPFormGlobal_FormObj.Data[fieldObj.TableName], function (nDataIndex, nDataObj) {
                        console.log(nDataObj);
                        var dataTableRowIndex = nDataObj.CPFormDataIndex;
                        sCondition = nObj.RuleCondition;
                        while (sCondition.indexOf("[index]") != -1) {
                            sCondition = sCondition.replace("[index]", "[" + dataTableRowIndex + "]");
                        }
                        var render = template.compile(sCondition);
                        var sValue = render(CPFormGlobal_FormObj.Data);
                        if (sValue.toLowerCase() == "false") {
                            if (errorMsg == "") {
                                errorMsg = nObj.RuleTargetOpertion;
                            }
                            else {
                                errorMsg += "\n" + nObj.RuleTargetOpertion;
                            }
                        }
                    });
                  
                }
                else {
                    //非拓展表
                    var render = template.compile(sCondition);
                    var sValue = render(CPFormGlobal_FormObj.Data);
                    if (sValue.toLowerCase() == "false") {
                        if (errorMsg == "") {
                            errorMsg = nObj.RuleTargetOpertion;
                        }
                        else {
                            errorMsg += "\n" + nObj.RuleTargetOpertion;
                        }
                    }
                }
                
            }
        });
        //再看看规则里有没有数据校验start
        if (errorMsg != "") {
            alert(errorMsg);
            return false;
        }
        else
            return true;

    };
    //保存表单前数据校验end

    //根据字段ID，获取 字段对象start
    $scope.GetFieldObj = function (fieldId) {
        var nTmpObj;
        $.each(CPFormGlobal_FormObj.Config.FieldCol, function (nIndex, nObj) {
            if(nObj.Id == fieldId)
            {
                nTmpObj = nObj; return;
            }
        });
        return nTmpObj;
    };
    //根据字段ID，获取 字段对象end



    //根据表名，看是主表拓展表start
    $scope.IsExtendTable = function (tableName) {
        //返回true 是
        var b = false;
        if (CPFormGlobal_FormObj.Config.MainTableName.toLocaleLowerCase() == tableName.toLocaleLowerCase())
            return false;
        else
            return true;
    };
    //根据表名，看是主表拓展表end

    //保存表单数据start
    $scope.SaveFormData = function () {
        if ($scope.CheckData() == false)
            return false;
        var url = "/api/FormEngine/SaveFormData";
        //保存数据时，只把表单对应数据发回去
        var formData = new Object();
        formData[CPFormGlobal_FormObj.Config.MainTableName] = CPFormGlobal_FormObj.Data[CPFormGlobal_FormObj.Config.MainTableName];
        //子表数据反序列化
        if (CPFormGlobal_FormObj.Config.ChildTableCol != undefined && CPFormGlobal_FormObj.Config.ChildTableCol != null) {
            $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nIndex, nObj) {
                formData[nObj.TableName] = CPFormGlobal_FormObj.Data[nObj.TableName];
            });
        }
        var obj =new Object();
        obj.CurUserId = CPFormGlobal_CurUserId;
        obj.CurUserIden = CPFormGlobal_CurUserIden;
        obj.FormCode= CPFormGlobal_FormCode;
        obj.PKValue = CPFormGlobal_PKValues;
        obj.FormDataJSON = JSON.stringify(formData);
       // console.log(formData);
        $http.post(url, obj).then(function (data) {
            if(data.data.Result==false)
            {
                alert(data.data.ErrorMsg);
                return false;
            }
            alert("保存成功！");
        });
    };
    //保存表单数据end
    //启用时间选择控件start
    $scope.SetTimeSelControl=function(){
        //console.log($($event.target).attr("id"));
        var nArray = $(".layerTimeSel");
        var laydate = layui.laydate;
        $.each(nArray, function (nIndex, nObj) {
            var layerUIInnerType = $(nObj).attr("data-layerUIInnerType");
            if (layerUIInnerType != "")
            {
                laydate.render({
                    elem: '#' + $(nObj).attr("id")
                      , type: layerUIInnerType
                });
            }
            else {
                laydate.render({
                    elem: '#' + $(nObj).attr("id")
             , format: $(nObj).attr("data-timeFormat") //可任意组合
                });
            }
           
        });
    };
    //启用时间选择控件end
    // 以下为重复表渲染完成执行的操作start
    $scope.bExtendTableContextMenuIsCreated = false;
    $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {
        //render完成之后要执行的方法
        $scope.SetFormDivContainerHeight();
        //由于有些引用可能没有注册以下方法，所以暂时使用try来解决
        try {
            //添加右键菜单star
            if ($scope.bExtendTableContextMenuIsCreated == false) {
                $.contextMenu({
                    selector: '.CPFormExtendTableTdCss',
                    trigger: 'right',
                    callback: function (key, options) {
                        
                        var datatablerowindex = $(CPForm_ContenxtMenuClickTarget).attr("data-datatablerowindex");
                        var tableName = $(CPForm_ContenxtMenuClickTarget).attr("data-tableName");
                        if (datatablerowindex == undefined || datatablerowindex == null)
                        {
                            //表示点击的td，需要重新查找控件//从table里找
                            var array = $("input[data-datatablerowindex]", $(CPForm_ContenxtMenuClickTarget).parent().parent());
                            if(array.length>0)
                            {
                                datatablerowindex = $(array[0]).attr("data-datatablerowindex");
                                tableName = $(array[0]).attr("data-tableName");
                            }
                            else {
                                array = $("select[data-datatablerowindex]", $(CPForm_ContenxtMenuClickTarget).parent().parent());
                                if (array.length > 0) {
                                    datatablerowindex = $(array[0]).attr("data-datatablerowindex");
                                    tableName = $(array[0]).attr("data-tableName");
                                }
                            }
                        }
                        if (datatablerowindex == undefined || datatablerowindex == null)
                        {
                            alert("未获取到datatablerowindex索引值");
                            return;
                        }
                        //console.log(datatablerowindex);
                        //查找点击区域所在的行数
                        if(key == "delete")
                        {
                            $scope.CPFormDelChildRow(datatablerowindex, tableName);
                        }
                        else if(key== "add")
                        {
                            $scope.CPFormAddChildRow("btnCPFormAddChildRow_" + tableName,false);
                        }
                    },
                    items: {
                        "add": { name: "添加新行", icon: "edit" },
                        "delete": { name: "删除此行", icon: "delete" }

                    }
                }); 
                $scope.bExtendTableContextMenuIsCreated = true; 
            }
            //添加右键菜单end
        } catch (e) {; }
        $scope.SetComboxSkin();
        $scope.SetTimeSelControl();
    });
    // 以上为重复表渲染完成执行的操作

    //表单控件onchange事件start
    $scope.CPFormChange = function (fieldId, controlId) {
        $.each(CPFormGlobal_FormObj.Config.FieldRuleCol, function (nIndex, nObj) {
            if (fieldId == nObj.FieldId) {
                //规则
                var sCondition = nObj.RuleCondition; 
                var dataTableRowIndex = $("#" + controlId).attr("data-dataTableRowIndex"); 
                while (sCondition.indexOf("[index]") != -1)
                {
                    sCondition = sCondition.replace("[index]", "[" + dataTableRowIndex+"]");
                }
                //console.log(sCondition);
                if (nObj.RuleType == 3) {
                    //设置某个控件的值start
                    var render = template.compile(sCondition);
                    var sValue = render(CPFormGlobal_FormObj.Data);
                    //console.log(sValue);
                    //设置某个控件的值end
                }
                else if  (nObj.RuleType == 2) {
                    //禁用某个控件start
                    //禁用返回true，不禁用返回任何值
                    var render = template.compile(sCondition);
                    var sValue = render(CPFormGlobal_FormObj.Data); 
                    var disableFieldObj = $scope.GetFieldObj(nObj.RuleTargetOpertion);
                    // console.log(disableFieldObj);
                    if (disableFieldObj != null) {
                        if ($scope.IsExtendTable(disableFieldObj.TableName)) {
                            //子表
                            if (sValue.toLowerCase() == "true") {
                                $("#CPForm_" + disableFieldObj.Id + "_" + dataTableRowIndex).attr("disabled", "disabled");
                            }
                            else {
                                $("#CPForm_" + disableFieldObj.Id + "_" + dataTableRowIndex).removeAttr("disabled");
                            }
                        }
                        else {
                            if (sValue.toLowerCase() == "true") {
                                $("#CPForm_" + disableFieldObj.Id).attr("disabled", "disabled");
                            }
                            else {
                                $("#CPForm_" + disableFieldObj.Id).removeAttr("disabled");
                            }
                        }
                    }
                    //console.log(sValue);
                    //禁用某个控件end
                }
            }
        });
    };
    //表单控件onchange事件end

    //获取某个字段的值start
    $scope.GetFieldValue = function (tableName, fieldName, CPFormDataIndex)
    {
        if($scope.IsExtendTable(tableName))
        {
            //拓展表
            return CPFormGlobal_FormObj.Data[tableName][CPFormDataIndex][fieldName];
        }
        else {
            return CPFormGlobal_FormObj.Data[tableName][fieldName];
        }
    };
    //获取某个字段的值end

    //设置某个字段的值start
    $scope.SetFieldValue = function (tableName, fieldName, CPFormDataIndex,sValue) {
        if ($scope.IsExtendTable(tableName)) {
            //拓展表
            CPFormGlobal_FormObj.Data[tableName][CPFormDataIndex][fieldName] = sValue;
        }
        else {
            CPFormGlobal_FormObj.Data[tableName][fieldName] = sValue;
        }
    };
    //设置某个字段的值end

    //拆分附件上传字段的值，转成数组返回，便于使用ng-repeat输出
    $scope.FileUploadInfoReturnArray = function (sValue) {
        //alert(sValue);
        return sValue.split(',');
    };
    //拆分附件上传字段的值，转成数组返回，便于使用ng-repeat输出

    //上传附件start
    $scope.UploadNewFile = function (tableName, fileNameField, filePathField, CPFormDataIndex) {
        var ReturnMethod = "CPFormGlobal_Scope.UploadNewFile_SetReturn('" + tableName + "','" + fileNameField + "','" + filePathField + "'," + CPFormDataIndex + ")";
        var url = "/CPSite/Plat/Common/Pages/FileUploadCommon.aspx?BusinessCode=" + CPFormGlobal_FormObj.Config.FormCode + "&ReturnMethod=" + escape(ReturnMethod);
        CPOpenDialog(url, "上传附件", 800, 600);
    };
    //上传附件返回
    $scope.UploadNewFile_SetReturn = function (tableName, fileNameField, filePathField, CPFormDataIndex) {
        //console.log(CPShowModalDialogReturnArgs);
        // console.log(CPShowModalDialogReturnArgs);
        //文件名,分隔，   文件存储路径|分隔
        var names = $scope.GetFieldValue(tableName, fileNameField, CPFormDataIndex);
        if (names != "")
        {
            names += "," + CPShowModalDialogReturnArgs.FileNames;
        }
        else
            names = CPShowModalDialogReturnArgs.FileNames;
        var paths = $scope.GetFieldValue(tableName, filePathField, CPFormDataIndex);
        if (paths != "") {
            paths += "|" + CPShowModalDialogReturnArgs.FilePaths;
        }
        else
            paths = CPShowModalDialogReturnArgs.FilePaths;
        $scope.SetFieldValue(tableName, fileNameField, CPFormDataIndex, names);
        $scope.SetFieldValue(tableName, filePathField, CPFormDataIndex,paths );
        $scope.$apply();
        
    };
    //查看附件或下载
    $scope.ViewAttachFile = function (filePath) {
        var url = "/CPSite/Plat/Common/Pages/DownLoadFile.aspx?FilePath=" + escape(filePath);
        CPOpenDialog(url, "下载附件", 50, 50);
    };
    //删除附件
    $scope.DelAttachFile = function (tableName, fileNameField, filePathField, CPFormDataIndex,filePath)
    {
        var names = $scope.GetFieldValue(tableName, fileNameField, CPFormDataIndex);       
        var paths = $scope.GetFieldValue(tableName, filePathField, CPFormDataIndex);
        var nameArray = names.split(',');
        var pathArray = paths.split('|');
        names = ""; paths = "";
        for (var i = 0; i < pathArray.length; i++)
        {
            if(filePath != pathArray[i])
            {
                if(paths == "")
                {
                    paths = pathArray[i];
                    names = nameArray[i];
                }
                else {
                    paths += "|"+ pathArray[i];
                    names += "," + nameArray[i];
                }
            }
        }
        $scope.SetFieldValue(tableName, fileNameField, CPFormDataIndex, names);
        $scope.SetFieldValue(tableName, filePathField, CPFormDataIndex, paths);
       
    };
    //上传附件end

    
  
    CPFormGlobal_Scope = $scope;
})
.filter('to_trusted', ['$sce', function ($sce) {
    //过滤器，解决模板列里有HTML，但显示了的问题。
    return function (text) {
        return $sce.trustAsHtml(text);
    };
}])
.filter('SplitStringToArray', function() { //可以注入依赖
    return function (text) {
        if (text == "")
            return null;
        return text.split('|');
    }
})
 .filter('ShowFileName', function () { //显示文件名
     return function (text) {
         if (text == "")
             return "";
         var array = text.split('/');
         return array[array.length - 1];
        }
    })
.directive('ionicstringtohtml', ['$compile', function ($compile) {
      return function (scope, element, attrs) {
          scope.$watch(
            function (scope) {
                return scope.$eval(attrs.inputstring);
            },
            function (value) {
                element.html(value);
                $compile(element.contents())(scope);
            }
          );
      };
  }])
.directive('onRepeatFinishedRender', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
           
            if (scope.$last === true) {
              
                $timeout(function () {
                    //这里element, 就是ng-repeat渲染的最后一个元素
                    scope.$emit('ngRepeatFinished', element);
                }, 100);
            }
        }
    };
});


/***********layer uistart  ******/
layui.use('laydate', function () {
  //不能删除
});
/***********layer ui end  ******/ 

 