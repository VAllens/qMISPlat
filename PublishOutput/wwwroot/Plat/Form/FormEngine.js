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
//是否只读
var CPFormGlobal_IsView = $.CPGetQuery("IsView"); if (CPFormGlobal_IsView == null || CPFormGlobal_IsView == undefined) CPFormGlobal_IsView = "false";  
//当前设备
//1:PC浏览器   2：IOS手机 3 ：安卓手机4 ：IOS平板 5：安卓平板
var CPFormGlobal_DeviceType = $.CPGetQuery("DeviceType"); if (CPFormGlobal_DeviceType == null || CPFormGlobal_DeviceType == undefined) CPFormGlobal_DeviceType = "1";
//表单是否在流程中使用true:是
var CPFormGlobal_FormUseInCPFlow = $.CPGetQuery("FormUseInCPFlow"); if (CPFormGlobal_FormUseInCPFlow == null || CPFormGlobal_FormUseInCPFlow == undefined) CPFormGlobal_FormUseInCPFlow = "false";
//全局传入参数end

//全局变量start
var CPFormGlobal_Scope = null;
//全局表单对象
var CPFormGlobal_FormObj = new Object();
 //记录当前操作的按钮是哪一行数据的
var CPFormGlobal_CurControlDataRowIndex = 0;
//全局变量end
//http://localhost:9000/CPSite/plat/tools/form/formview.html?FormCode=Form0001&SceneCode=Scene0001&ViewCode=View201706280837070001&InitGroupCode=Group0001&RightGroupCode=&CurUserId=1&CurUserIden=E0A04F94-90D5-4C2A-8C9D-836813F73DC4&PKValues=1&DeviceType=1&SysId=1
angular.module('CPFormEngineApp', []).controller('MyCtrl', function ($scope, $sce, $http, $timeout, $filter) {
    var formUrl = CPWebRootPath + "/api/FormEngine/GetFormInfo?FormCode=" + CPFormGlobal_FormCode + "&SceneCode=" + CPFormGlobal_SceneCode + "&ViewCode=" + CPFormGlobal_ViewCode
        + "&InitGroupCode=" + CPFormGlobal_InitGroupCode + "&RightGroupCode=" + CPFormGlobal_RightGroupCode + "&PKValues=" + CPFormGlobal_PKValues
        + "&CurUserId=" + CPCurUserId + "&CurUserIden=" + CPCurUserIden + "&DeviceType=" + CPFormGlobal_DeviceType;
    formUrl += "&IsView=" + CPFormGlobal_IsView;
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
            || tempSarray[0].toLowerCase() == "isview"
        )
            continue;
        formUrl += "&" + sResultArray[mm];
    }
    if ($(window).width() < $("#CPFormContainer").width()) {
        $("#CPFormContainer").width($(window).width() - 50);
    }
    $http.get(formUrl).then(function (data) {
        //存储当前用户对象信息
        if (data.data.Result == false) {
            alert(data.data.ErrorMsg);
            return false;
        }
        console.log(data);
        CPFormGlobal_FormObj.Config = data.data.Form;
        //console.log(CPFormGlobal_FormObj);
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
                if (tmpObj[nObj.TableName + "_ExtendTableInitValue"] != undefined
                    && tmpObj[nObj.TableName + "_ExtendTableInitValue"] != null) {
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
        $scope.CPFormInitButton();
        //  console.log(CPFormGlobal_FormObj);
        $scope.FormObj = CPFormGlobal_FormObj;
    });

    //加载操作按钮start
    $scope.CPFormInitButton = function () {
        if (CPFormGlobal_FormObj.Config.UseSceneCol.length <= 0) {
            alert("请传入使用场景参数");
            return false;
        }
        var sHtml = "";
        $.each(CPFormGlobal_FormObj.Config.UseSceneCol[0].FuncCol, function (nIndex, nObj) {
            sHtml += "<button onclick=\"" + nObj.FuncExeJS + "\" class=\"layui-btn layui-btn-normal\">" + nObj.FuncTitle + "</button>";
        });
        $("#CPFormButton").html(sHtml);

    };
    //加载操作按钮end
    //拓展表添加 新行start
    $scope.CPFormAddChildRow = function (btnId, isClickButton) {
        var exTableName = $("#" + btnId).attr("data-TableName");

        var nObj = new Object();
        //先找出子表与主表的关联字段，除关联字段外，其它的字段的值都设置成空
        var relateFieldName = "";
        $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nIndex, nObj) {
            if (nObj.TableName == exTableName) {
                relateFieldName = nObj.RelateFieldName;
                return;
            }
        });
        var maxCPFormDataIndex = 0;
        $.each(CPFormGlobal_FormObj.Data[exTableName], function (nIndex, nObj) {
            if (nObj.CPFormDataIndex > maxCPFormDataIndex) {
                maxCPFormDataIndex = nObj.CPFormDataIndex;
            }
        });
        //待复制的行
        var sourceRowValue = CPFormGlobal_FormObj.Data[exTableName][CPFormGlobal_FormObj.Data[exTableName].length - 1];
        for (var item in sourceRowValue) {
            //alert("person中" + item + "的值=" + person[item]);
            if (item == "$$hashKey")
                continue;
            if (item == relateFieldName) {
                nObj[item] = sourceRowValue[item];
            }
            else if (item == "CPFormDataIndex") {
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
            $scope.$applyAsync();
        }
    };
    //拓展表添加 新行end

    //拓展表删除行start
    $scope.CPFormDelChildRow = function (CPFormDataIndex, tableName) {
        if (CPFormGlobal_FormObj.Data[tableName].length <= 1) {
            alert("至少得留一行，不能删除了！");
            return;
        }
        var removeIndex = -1;
        $.each(CPFormGlobal_FormObj.Data[tableName], function (nIndex, nObj) {
            if (Number(nObj.CPFormDataIndex) == Number(CPFormDataIndex)) {
                removeIndex = nIndex;
                return;
            }
        });
        CPFormGlobal_FormObj.Data[tableName].splice(removeIndex, 1);
        //console.log(CPFormGlobal_FormObj.Data[tableName]);
        $scope.$applyAsync();
        $scope.SetFormDivContainerHeight();
    };
    //拓展表删除行end

    //设置表单主体的高度start
    $scope.SetFormDivContainerHeight = function () {
        if (CPFormGlobal_FormUseInCPFlow != "true") {
            var nHeight = $(window).height();
            nHeight = nHeight - $("#CPFormButton").height() - 25;
            //alert(nHeight);
            //alert($("#CPFormMain").height());
            if (nHeight < $("#CPFormMain").height()) {
                $("#CPFormMain").height(nHeight);
            }
        }
        else {
            $(document.body).css("overflow", "auto");
        }
    }
    //设置表单主体的高度end

    //设置表单checkbox默认选中状态start
    $scope.SetCheckBoxFieldChecked = function (fieldValue, curChkValue) {
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
    $scope.SetCheckBoxFieldValue = function (isExtendTable, extendTableDataIndex, tableName, fieldName, formFieldId) {
        //var extendTableDataIndex = 1;
        var sArray = $("input[name='CPForm_" + formFieldId + "']");
        var sValue = "";
        $.each(sArray, function (nIndex, nObj) {
            if (nObj.checked) {
                if (sValue == "")
                    sValue = nObj.value;
                else
                    sValue += "," + nObj.value;
            }
        });
        if (isExtendTable == "true") {
            //拓展表
            $.each(CPFormGlobal_FormObj.Data[tableName], function (nIndex, nObj) {
                if (Number(nObj.CPFormDataIndex) == Number(extendTableDataIndex)) {
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
    $scope.SetComboxSkinIsExecute = false;
    $scope.SetComboxSkin = function () {
        if ($scope.SetComboxSkinIsExecute == true)
            return;
        $(".comboxwrapper").each(function () {
            $scope.SetComboxSkinIsExecute = true;
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
                    //console.log(1);
                    var inputObj = $(this).parent().parent().children(".comboxinput-wrapper").children("input");
                    inputObj.val($(this).html()); //点击替换input的值
                    //console.log(2);
                    var ngModel = inputObj.attr("ng-model");
                    //console.log(ngModel);
                    var datatablerowindex = inputObj.attr("data-datatablerowindex");
                    //console.log(datatablerowindex);
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
                   
                    if (bExtendTableField) {
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
                if (nTableObj.TableName == nObjField.TableName) {
                    ExtendTableObj = nTableObj;
                    bExtendTableField = true;
                    return;
                }
            });
            //检查是不是拓展表的字段end
            var sTip = "";
            if (nObjField.NotAllowNullTip == null || nObjField.NotAllowNullTip == "") {
                sTip = nObjField.FieldTitle + "不允许为空";
            }
            else {
                sTip = nObjField.NotAllowNullTip;
            }
            //先看看这个字段是不是不允许为空，或者这个字段是不是有校验的规则
            if (bExtendTableField == false) {
                //主表数据start
                if (nObjField.FieldName != CPFormGlobal_FormObj.Config.PKFieldName) {
                    if (nObjField.IsAllowNull == false) {
                        var sValue = CPFormGlobal_FormObj.Data[CPFormGlobal_FormObj.Config.MainTableName][nObjField.FieldName];
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
                    $.each(CPFormGlobal_FormObj.Data[fieldObj.TableName], function (nDataIndex, nDataObj) {
                        var dataTableRowIndex = nDataObj.CPFormDataIndex;
                        sCondition = nObj.RuleCondition;
                        while (sCondition.indexOf("[index]") != -1) {
                            sCondition = sCondition.replace("[index]", "[" + dataTableRowIndex + "]");
                        }                       
                        var render = template.compile(sCondition);
                        var sValue = render(CPFormGlobal_FormObj.Data);
                        sValue = CPTrim(sValue);
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
                    sValue = CPTrim(sValue);
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
            if (nObj.Id == fieldId) {
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
        $("#divLoading").show();
        if ($scope.CheckData() == false) {
            $("#divLoading").hide();
            if (CPFormGlobal_FormUseInCPFlow == "true")
            {
                //在流程中使用
                parent.CPFlowAfterFormSaveError();
            }
            return false;
        }
        //执行保存前扩展事件
        if (CPFormGlobal_FormObj.Config.UseSceneCol[0].BeforeFormClientSave != "")
        {
            var sB = eval(CPFormGlobal_FormObj.Config.UseSceneCol[0].BeforeFormClientSave);
            if (sB == false) {
                $("#divLoading").hide();
                if (CPFormGlobal_FormUseInCPFlow == "true") {
                    //在流程中使用
                    parent.CPFlowAfterFormSaveError();
                }
                return false;
            }
        }
        var url = CPWebRootPath + "/api/FormEngine/SaveFormData?" + window.location.search;
        //保存数据时，只把表单对应数据发回去
        var formData = new Object();
        formData[CPFormGlobal_FormObj.Config.MainTableName] = CPFormGlobal_FormObj.Data[CPFormGlobal_FormObj.Config.MainTableName];
        //子表数据反序列化
        if (CPFormGlobal_FormObj.Config.ChildTableCol != undefined && CPFormGlobal_FormObj.Config.ChildTableCol != null) {
            $.each(CPFormGlobal_FormObj.Config.ChildTableCol, function (nIndex, nObj) {
                formData[nObj.TableName] = CPFormGlobal_FormObj.Data[nObj.TableName];
            });
        }
        var obj = new Object();
        obj.CurUserId = CPCurUserId;
        obj.CurUserIden = CPCurUserIden;
        obj.FormCode = CPFormGlobal_FormCode;
        obj.PKValue = CPFormGlobal_PKValues;
        obj.SceneCode = CPFormGlobal_SceneCode;
        obj.FormDataJSON = JSON.stringify(formData);
        // console.log(formData);
        $http.post(url, obj).then(function (data) {
            if (data.data.Result == false) {
                alert(data.data.ErrorMsg);
                if (CPFormGlobal_FormUseInCPFlow == "true") {
                    //在流程中使用
                    parent.CPFlowAfterFormSaveError();
                }
                return false;
            }
            if (CPFormGlobal_FormUseInCPFlow == "false") {
                //非流程中使用
                alert("保存成功！");
            }
            else {
                //流程中使用
            }
            $("#divLoading").hide();
            //执行保存后操作Start
            $scope.AfterSaveFormData(data.data.PKValues);
            //执行保存后操作
        });
    };
    //执行保存后操作start
    $scope.AfterSaveFormData = function (PKValues) {

        if (CPFormGlobal_FormUseInCPFlow == "true") {
            //在流程中使用,获取表单里的流程标题，如果没有，则流程自动编号
            try {
                var insTitle = $scope.GetFieldValue(CPFormGlobal_FormObj.Form.MainTableName, "FlowTitle", 0);
                if (insTitle != null && insTitle != undefined && insTitle != "")
                {
                    parent.CPFlowAfterFormSaveSuccess(PKValues, insTitle);
                }
                else {
                    parent.CPFlowAfterFormSaveSuccess(PKValues, "");
                }
            }
            catch (e) {
                parent.CPFlowAfterFormSaveSuccess(PKValues, "");
            }
          
        }
        else {
            if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedAction == 1) {
                //关闭window窗口
                window.close();
            }
            else if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedAction == 2) {
                //关闭弹出层
                parent.CloseNewModel();
            }
            else if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedAction == 3) {
                //返回表单修改界面
                if (CPFormGlobal_PKValues == "") {
                    var url = window.location.href;
                    url += "&PKValues=" + PKValues;
                    window.location.href = url;
                }
            }
            else if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedAction == 4) {
                //跳转到指定页面
                window.location.href = CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedInfo;
            }
            else if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedAction == 5) {
                //自定义脚本
                eval(CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedInfo);
            }
            else if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedAction == 6) {
                //关闭在top中打开的弹出层
                top.CloseNewModel();
            }
            else if (CPFormGlobal_FormObj.Config.UseSceneCol[0].FormSavedAction == 7) {
                //返回表单修改界面并刷新左侧树
                //刷新树
                parent.CPTreeRefresh();
                //返回表单修改界面
                if (CPFormGlobal_PKValues == "") {
                    var url = window.location.href;
                    url += "&PKValues=" + PKValues;
                    window.location.href = url;
                }
            }
        }
    };
    //执行保存后操作end
    //保存表单数据end
    //启用时间选择控件start
    $scope.SetTimeSelControlIsExecute = false;
    $scope.SetTimeSelControl = function () {
        //console.log($($event.target).attr("id"));
        if ($scope.SetTimeSelControlIsExecute)
            return;
        var nArray = $(".layerTimeSel");
        var laydate = layui.laydate;
        $.each(nArray, function (nIndex, nObj) {
            var layerUIInnerType = $(nObj).attr("data-layerUIInnerType");
            if (layerUIInnerType != "")
            {
                laydate.render({
                    elem: '#' + $(nObj).attr("id")
                    , type: layerUIInnerType
                    , done: function (value, date, endDate) {
                        //console.log(value); //得到日期生成的值，如：2017-08-18
                        //console.log(date); //得到日期时间对象：{year: 2017, month: 8, date: 18, hours: 0, minutes: 0, seconds: 0}
                        //console.log(endDate); //得结束的日期时间对象，开启范围选择（range: true）才会返回。对象成员同上。
                        $scope.SetFieldValue($(nObj).attr("data-tablename"), $(nObj).attr("data-fieldname"), $(nObj).attr("data-datatablerowindex"), value);
                    }
                });
            }
            else {
                laydate.render({
                    elem: '#' + $(nObj).attr("id")
                    , format: $(nObj).attr("data-timeFormat") //可任意组合
                    , done: function (value, date, endDate) {
                        //console.log(value); //得到日期生成的值，如：2017-08-18
                        //console.log(date); //得到日期时间对象：{year: 2017, month: 8, date: 18, hours: 0, minutes: 0, seconds: 0}
                        //console.log(endDate); //得结束的日期时间对象，开启范围选择（range: true）才会返回。对象成员同上。
                        $scope.SetFieldValue($(nObj).attr("data-tablename"), $(nObj).attr("data-fieldname"), $(nObj).attr("data-datatablerowindex"), value);
                    }
                });
            }
           
        });
        $scope.SetTimeSelControlIsExecute = true;
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
        } catch (e) { ; }
        $("#divLoading").hide();
        $("#CPFormContainer").show();
        if (CPFormGlobal_FormUseInCPFlow != "true") {
            $("#CPFormButton").show();
        }
        $scope.SetComboxSkin();
        $scope.SetTimeSelControl(); 
        
    });
    // 以上为重复表渲染完成执行的操作
    //根据表名字段名，获取字段ID
    $scope.GetFieldId = function (tableName, fieldName) {
        var id = "";
        $.each(CPFormGlobal_FormObj.Config.FieldCol, function (nIndex, nObj) {
            if (nObj.FieldName.toLowerCase() == fieldName.toLowerCase() && nObj.TableName.toLowerCase() == tableName.toLowerCase())
            {
                id = nObj.Id;
                return;
            }
        });
        return id;
    };
    //表单控件onchange事件start
   
    $scope.CPFormChange = function (fieldId, controlId, onchangeExMethod) {
        var dataTableRowIndex = $("#" + controlId).attr("data-dataTableRowIndex"); 
        CPFormGlobal_CurControlDataRowIndex = dataTableRowIndex;
        $.each(CPFormGlobal_FormObj.Config.FieldRuleCol, function (nIndex, nObj) {
            if (fieldId == nObj.FieldId) {
                //规则
                var sCondition = nObj.RuleCondition; 
             
                while (sCondition.indexOf("[index]") != -1)
                {
                    sCondition = sCondition.replace("[index]", "[" + dataTableRowIndex+"]");
                }
                //console.log(sCondition);
                if (nObj.RuleType == 3) {
                    //设置某个控件的值start
                    var render = template.compile(sCondition);
                    var sValue = render(CPFormGlobal_FormObj.Data);
                    sValue = CPTrim(sValue);
                    if (sValue.toLowerCase() == "true") 
                    {
                        sCondition = nObj.RuleTargetOpertion;
                        var dataTableRowIndex = $("#" + controlId).attr("data-dataTableRowIndex");
                        while (sCondition.indexOf("[index]") != -1) {
                            sCondition = sCondition.replace("[index]", "[" + dataTableRowIndex + "]");
                        }
                        var renderNew = template.compile(sCondition);
                        var sValueNew = renderNew(CPFormGlobal_FormObj.Data);
                    }
                    //console.log(sValue);
                    //设置某个控件的值end
                }
                else if  (nObj.RuleType == 2) {
                    //禁用某个控件start
                    //禁用返回true，不禁用返回任何值
                    var render = template.compile(sCondition);
                    var sValue = render(CPFormGlobal_FormObj.Data);
                    sValue = CPTrim(sValue);
                    if (nObj.RuleTargetOpertion != "") {
                        var arrayT = nObj.RuleTargetOpertion.split('.');
                        if (arrayT.length >= 2) {
                            var disableFieldId = $scope.GetFieldId(arrayT[0], arrayT[1]);
                            // console.log(disableFieldObj);
                            if (disableFieldId != "") {
                                if ($scope.IsExtendTable(arrayT[0])) {
                                    //子表
                                    if (sValue.toLowerCase() == "true") {
                                        $("#CPForm_" + disableFieldId + "_" + dataTableRowIndex).attr("disabled", "disabled");
                                    }
                                    else {
                                        $("#CPForm_" + disableFieldId + "_" + dataTableRowIndex).removeAttr("disabled");
                                    }
                                }
                                else {
                                    if (sValue.toLowerCase() == "true") {
                                        $("#CPForm_" + disableFieldId).attr("disabled", "disabled");
                                    }
                                    else {
                                        $("#CPForm_" + disableFieldId).removeAttr("disabled");
                                    }
                                }
                            }
                        }
                    }
                    //console.log(sValue);
                    //禁用某个控件end
                }
            }
        });
        //执行扩展onchange事件
        //console.log(onchangeExMethod);
        if (onchangeExMethod != undefined && onchangeExMethod != null &&  onchangeExMethod != "")
            eval(onchangeExMethod);
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
        $scope.$applyAsync();
    };
    //设置某个字段的值end

    //拆分附件上传字段的值，转成数组返回，便于使用ng-repeat输出
    $scope.FileUploadInfoReturnArray = function (sValue) {
        //alert(sValue);
        return sValue.split(',');
    };
    //拆分附件上传字段的值，转成数组返回，便于使用ng-repeat输出

    //上传附件start
    $scope.UploadNewFile = function (tableName, fileNameField, filePathField, CPFormDataIndex, FileAllowType, FileAllowCount) {
        var ReturnMethod = "CPFormGlobal_Scope.UploadNewFile_SetReturn('" + tableName + "','" + fileNameField + "','" + filePathField + "'," + CPFormDataIndex + ")";
        var url = CPWebRootPath + "/Plat/Common/FileUpload?BusinessCode=" + CPFormGlobal_FormObj.Config.FormCode + "&ReturnMethod=" + escape(ReturnMethod);
        url += "&AllowFileTypes=" + escape(FileAllowType) + "&AllowFileCount="+ FileAllowCount;
        CPOpenDialog(url, "上传附件", 600, 400);
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
      
        
    };
    //查看附件或下载
    $scope.ViewAttachFile = function (filePath) {
        
        var url = CPWebRootPath + "/api/CPCommonEngine/DownloadFile?FilePath=" + escape(filePath);
       // CPOpenDialog(url, "下载附件", 50, 50);
        window.location.href = url;
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
    //.filter('textAreaStringFormat', function () { //可以注入依赖
    //    return function (text) {
    //        if (text == "")
    //            return "";
    //        text = text.replace("\\n", "<br/>");
    //        console.log(text);
    //        return text;
    //    }
    //})
    .filter('ListRelateFilterMethod', function (){ //下拉列表联动过滤值
    return function (text, tableName, ListRelateTargetField) {
        //console.log(tableName);
        //console.log(ListRelateTargetField);
       // console.log(CPFormGlobal_Scope);
        if (ListRelateTargetField != "")
        {
            //下拉列表 联动
            //由于暂时找不到怎么取子表第几行值，所以暂时只支持主表
            if (CPFormGlobal_Scope.IsExtendTable(tableName))
            {
                alert("下拉列表暂时不支持在子表中使用！");
                return text;
            }
            var mValue = CPFormGlobal_Scope.GetFieldValue(tableName, ListRelateTargetField, 0);
            var sArray = new Array();
            $.each(text, function (nIndex, nObj) {
                if (nObj.listRelateEx == mValue)
                {
                    sArray.push(nObj);
                }
            });
            return sArray;

        }
        else {
            return text;
        }
        //if (text == "")
        //    return null;
        //return text.split('|');
        
    }
} )
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
//表单保存按钮调用
function CPFormSaveFormData()
{
    CPFormGlobal_Scope.SaveFormData();
}

/***********layer uistart  ******/
layui.use('laydate', function () {
  //不能删除
});
/***********layer ui end  ******/
//选择表达式
//nType: 0通用表达式  1列表  2表单   3树 4流程
var SelectExpression_ThisObj;
function SelectExpression_SetReturn()
{
    //alert(CPShowModalDialogReturnArgs);
    var obj = $(SelectExpression_ThisObj);
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex");
    var fieldName = obj.attr("data-fieldName");
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    var sValue = CPFormGlobal_Scope.GetFieldValue(tableName, fieldName, dataTableRowIndex);
    sValue = sValue.replace("{}", CPShowModalDialogReturnArgs);
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldName, dataTableRowIndex, sValue);
}
function SelectExpression(thisObj, nType)
{
    var sValue = $(thisObj).val();
    if (sValue.indexOf("{}") != -1)
    {
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
//选择阿里巴巴图标
function SelectFontIcon(thisObj)
{
    SelectFontIcon_ThisObj = thisObj;
    var url = "/Style/AibabaIcon/SelIcon.html";
    CPOpenDialog(url, "选择图标", 1000, 1000);
}
var SelectFontIcon_ThisObj;
function SelectFontIcon_SetReturn() {
    //alert(CPShowModalDialogReturnArgs);
    var obj = $(SelectFontIcon_ThisObj);
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex");
    var fieldName = obj.attr("data-fieldName");
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    //alert(dataTableRowIndex);
    //alert(fieldName);
    //alert(fieldValueName);
    //alert(tableName);
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldName, dataTableRowIndex, CPShowModalDialogReturnArgs);
}
//用户选择方法
var CPFormUserSelectMethod_CurControlId;
function CPOrganSelectMethod_SetReturn()
{
    if (CPShowModalDialogReturnArgs == null)
        return;
    var obj = $("#" + CPFormUserSelectMethod_CurControlId );
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex");
    var fieldName = obj.attr("data-fieldName");
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldName, dataTableRowIndex, CPShowModalDialogReturnArgs.UserNames);
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldValueName, dataTableRowIndex, CPShowModalDialogReturnArgs.UserIds);
}
//存储已经选择用户IDs
var CPOrganSelectPage_SelUserIds = "";//存储已经选择用户ID，多个用,分隔
function CPFormUserSelectMethod(controlId, organIsCanMultiSel)
{
    var obj = $("#" + controlId);
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex"); 
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    if (fieldValueName == "")
    {
        alert("你未配置存储用户ID的字段，请先在字段详细信息里配置存储值的字段！");
        return;
    }
    CPOrganSelectPage_SelUserIds =  CPFormGlobal_Scope.GetFieldValue(tableName, fieldValueName, dataTableRowIndex);
    CPFormUserSelectMethod_CurControlId = controlId;
    var url = "/Plat/Organ/OrganSel?IsMultiSel=" + organIsCanMultiSel;
 OpenNewModel(url, "选择用户", 760, 500);
    //   OpenNewModel(url, "选择用户", 760, 400);
}

//部门选择方法
var CPFormDepSelectMethod_CurControlId;
function CPFormDepSelectMethod_SetReturn() {
    if (CPShowModalDialogReturnArgs == null)
        return;
    var obj = $("#" + CPFormDepSelectMethod_CurControlId);
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex");
    var fieldName = obj.attr("data-fieldName");
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldName, dataTableRowIndex, CPShowModalDialogReturnArgs.DepNames);
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldValueName, dataTableRowIndex, CPShowModalDialogReturnArgs.DepIds);
} 
function CPFormDepSelectMethod(controlId, organIsCanMultiSel) {
    var obj = $("#" + controlId);
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex");
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    if (fieldValueName == "") {
        alert("你未配置存储部门ID的字段，请先在字段详细信息里配置存储值的字段！");
        return;
    }
    var CPDepSelectPage_SelDepIds = CPFormGlobal_Scope.GetFieldValue(tableName, fieldValueName, dataTableRowIndex);
    CPFormDepSelectMethod_CurControlId = controlId;
    var url = "";
    if (organIsCanMultiSel == "false")
    {
        url = "/Plat/Tree/TreeView?TreeCode=Tree201711041939260006&SelDepIds=" + CPDepSelectPage_SelDepIds;
    }
    else {
        url = "/Plat/Tree/TreeView?TreeCode=Tree201711041918460005&SelDepIds=" + CPDepSelectPage_SelDepIds;
    }
    OpenNewModel(url, "选择部门", 550, 500);
}


//角色选择方法
var CPFormRoleSelectMethod_CurControlId;
function CPFormRoleSelectMethod_SetReturn() {
    if (CPShowModalDialogReturnArgs == null)
        return;
    var obj = $("#" + CPFormRoleSelectMethod_CurControlId);
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex");
    var fieldName = obj.attr("data-fieldName");
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldName, dataTableRowIndex, CPShowModalDialogReturnArgs.RoleNames);
    CPFormGlobal_Scope.SetFieldValue(tableName, fieldValueName, dataTableRowIndex, CPShowModalDialogReturnArgs.RoleIds);
}
function CPFormRoleSelectMethod(controlId, organIsCanMultiSel) {
    var obj = $("#" + controlId);
    var dataTableRowIndex = obj.attr("data-dataTableRowIndex");
    var fieldValueName = obj.attr("data-fieldValueName");
    var tableName = obj.attr("data-tableName");
    if (fieldValueName == "") {
        alert("你未配置存储角色ID的字段，请先在字段详细信息里配置存储值的字段！");
        return;
    }
    var CPRoleSelectPage_SelRoleIds = CPFormGlobal_Scope.GetFieldValue(tableName, fieldValueName, dataTableRowIndex);
    CPFormRoleSelectMethod_CurControlId = controlId;
    var url = "";
    if (organIsCanMultiSel == "false") {
        url = "/Plat/Grid/GridView?GridCode=Grid201711042006440020";
    }
    else {
        url = "/Plat/Grid/GridView?GridCode=Grid201711041956490019";
    }
    OpenNewModel(url, "选择角色", 650, 500);
}
function CPFormUpdateConfig(formId)
{
    var url = "/Plat/Tab/TabView?TabCode=Tab0003&DeviceType=1&SysId=1&TargetFormId=" + formId;
    try {
        top.OpenNewModel(url, "修改配置", 9000, 9000);
    }
    catch (e) {
        OpenNewModel(url, "修改配置", 9000, 9000);
    }
}