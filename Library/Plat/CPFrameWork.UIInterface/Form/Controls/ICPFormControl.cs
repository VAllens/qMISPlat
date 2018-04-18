using CPFrameWork.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.UIInterface.Form.Controls
{
     public   interface ICPFormControl
    {
        string FormatHtml(CPFormField field,  CPFormFieldRight fieldRight, bool isExtendTable);
    }

    public class ICPFormControlManager
    {
        public static ICPFormControl GetControlInstance(CPFormField formField)
        {
            if (formField.FieldStatus == CPFormEnum.FieldStatusEnum.Hidden)
            {
                return new CPFormHidden();
            }
            else
            {
                if (formField.ControlType == CPFormEnum.ControlTypeEnum.TextBox)
                    return new CPFormTextBox();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.TextArea)
                    return new CPFormTextArea();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.DropDownList)
                    return new CPFormDropDownList();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.Combox)
                    return new CPFormCombox();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.Radio)
                    return new CPFormRadio();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.CheckBox)
                    return new CPFormCheckBox();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.DateTimeSelect)
                    return new CPFormTimeSelect();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.TextEditor)
                {
                    return new CPFormUEditor();
                }
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.FileUpload)
                    return new CPFormFileUpload();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.UserSelect)
                    return new CPFormUserSel();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.DepSelect)
                    return new CPFormDepSel();
                else if (formField.ControlType == CPFormEnum.ControlTypeEnum.RoleSelect)
                    return new CPFormRoleSel();
                else
                    return new CPFormTextBox();
            }
        }
    }

    #region 文本框
    public class CPFormTextBox : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请输入";
            string sTitle = field.FieldTip;
            string bindPre = "";
            if(isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "readonly=\"readonly\"";
                pTip = "";
            }
            string id = "CPForm_" + field.Id.ToString();
            if(isExtendTable)
            {
                id +=  "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = " " + field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }

            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string sHtml = "<input type='text' id='" + id + "' " + readonlyS + ngChange + extendEvent +  " class='textboxCss' placeholder='" + pTip + "' style='width:" + field.ShowWidth + ";'  ng-model='" + bindPre + field.FieldName + "' title=\"" + sTitle + "\" value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "' data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' />";
            sHtml += field.HtmlEx;
            if(string.IsNullOrEmpty(field.ExButtonIcon)==false)
            {
                string clickMethod = "";
                if(string.IsNullOrEmpty(field.ExButtonClickMethod)==false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon+ " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod  + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 多行文本框
    public class CPFormTextArea : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请输入";
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "readonly=\"readonly\"";
                pTip = "";
            }
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }


            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if(string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent +=" " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string sHtml = "<textarea  id='" + id + "' " + readonlyS + ngChange + " class='textboxCss textAreaCss' rows='" + field.MultiRows + "' placeholder='" + pTip + "' style='width:" + field.ShowWidth + ";height:" + field.ShowHeight + "px;'  ng-model='" + bindPre + field.FieldName + "'  title=\"" + sTitle + "\"  data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "'  " + extendEvent + "  ></textarea>";
            sHtml += field.HtmlEx;
            if (string.IsNullOrEmpty(field.ExButtonIcon) == false)
            {
                string clickMethod = "";
                if (string.IsNullOrEmpty(field.ExButtonClickMethod) == false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon + " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 用户选择
    public class CPFormUserSel : ICPFormControl
    {
        public string FormatHtml(CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请选择";
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            readonlyS = "readonly=\"readonly\"";
            pTip = "";
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }


            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string width = field.ShowWidth;
            if(width.IndexOf("%")!=-1)
            {
                width = width.Replace("%","");
                try
                {
                    if(Convert.ToDouble(width)>=95)
                    {
                        width = "90%";
                    }
                }
                catch(Exception ex)
                {
                    ex.ToString();
                    width = field.ShowWidth;
                }
            }
            string sHtml = "";
            if(field.OrganIsCanMultiSel.Value==false)
            {
                 sHtml = "<input type='text' id='" + id + "' " + readonlyS + ngChange + extendEvent + " class='textboxCss' placeholder='" + pTip + "' style='width:" + width + ";'  ng-model='" + bindPre + field.FieldName + "' title=\"" + sTitle + "\" value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "' data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' />";
            }
            else
            {
                sHtml = "<textarea  id='" + id + "' " + readonlyS + ngChange + " class='textboxCss' rows='" + field.MultiRows + "' placeholder='" + pTip + "' style='width:" + width + ";height:" + field.ShowHeight + "px;'  ng-model='" + bindPre + field.FieldName + "' title=\"" + sTitle + "\"  data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "'  " + extendEvent + " >{{" + bindPre + field.FieldName + "}}</textarea>";
            }
            sHtml += field.HtmlEx;
            //添加选择按钮
            string clickMethod1 = "";
            clickMethod1 = "CPFormUserSelectMethod('" + id + "','" + field.OrganIsCanMultiSel.Value.ToString().ToLower()+ "')";
            clickMethod1 = " onclick=\"" + clickMethod1 + ";\" style='cursor:pointer;'";
            string icon1 = "<i class=\"icon iconfont icon-zhucetianjiahaoyou FormControlAfterIconCss\" title=\"点击选择\" " + clickMethod1 + "></i>";
            sHtml += icon1;
            //添加 选择按钮
            if (string.IsNullOrEmpty(field.ExButtonIcon) == false)
            {
                string clickMethod = "";
                if (string.IsNullOrEmpty(field.ExButtonClickMethod) == false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon + " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 部门选择
    public class CPFormDepSel : ICPFormControl
    {
        public string FormatHtml(CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请选择";
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            readonlyS = "readonly=\"readonly\"";
            pTip = "";
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }


            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string width = field.ShowWidth;
            if (width.IndexOf("%") != -1)
            {
                width = width.Replace("%", "");
                try
                {
                    if (Convert.ToDouble(width) >= 95)
                    {
                        width = "90%";
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    width = field.ShowWidth;
                }
            }
            string sHtml = "";
            if (field.OrganIsCanMultiSel.Value == false)
            {
                sHtml = "<input type='text' id='" + id + "' " + readonlyS + ngChange + extendEvent + " class='textboxCss' placeholder='" + pTip + "' style='width:" + width + ";'  ng-model='" + bindPre + field.FieldName + "' title=\"" + sTitle + "\" value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "' data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' />";
            }
            else
            {
                sHtml = "<textarea  id='" + id + "' " + readonlyS + ngChange + " class='textboxCss' rows='" + field.MultiRows + "' placeholder='" + pTip + "' style='width:" + width + ";height:" + field.ShowHeight + "px;'  ng-model='" + bindPre + field.FieldName + "' title=\"" + sTitle + "\"  data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "'  " + extendEvent + " >{{" + bindPre + field.FieldName + "}}</textarea>";
            }
            sHtml += field.HtmlEx;
            //添加选择按钮
            string clickMethod1 = "";
            clickMethod1 = "CPFormDepSelectMethod('" + id + "','" + field.OrganIsCanMultiSel.Value.ToString().ToLower() + "')";
            clickMethod1 = " onclick=\"" + clickMethod1 + ";\" style='cursor:pointer;'";
            string icon1 = "<i class=\"icon iconfont icon-zuzhijiagoujiekou FormControlAfterIconCss\" title=\"点击选择\" " + clickMethod1 + "></i>";
            sHtml += icon1;
            //添加 选择按钮
            if (string.IsNullOrEmpty(field.ExButtonIcon) == false)
            {
                string clickMethod = "";
                if (string.IsNullOrEmpty(field.ExButtonClickMethod) == false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon + " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 角色选择
    public class CPFormRoleSel : ICPFormControl
    {
        public string FormatHtml(CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请选择";
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            readonlyS = "readonly=\"readonly\"";
            pTip = "";
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }


            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string width = field.ShowWidth;
            if (width.IndexOf("%") != -1)
            {
                width = width.Replace("%", "");
                try
                {
                    if (Convert.ToDouble(width) >= 95)
                    {
                        width = "90%";
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    width = field.ShowWidth;
                }
            }
            string sHtml = "";
            if (field.OrganIsCanMultiSel.Value == false)
            {
                sHtml = "<input type='text' id='" + id + "' " + readonlyS + ngChange + extendEvent + " class='textboxCss' placeholder='" + pTip + "' style='width:" + width + ";'  ng-model='" + bindPre + field.FieldName + "' title=\"" + sTitle + "\" value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "' data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' />";
            }
            else
            {
                sHtml = "<textarea  id='" + id + "' " + readonlyS + ngChange + " class='textboxCss' rows='" + field.MultiRows + "' placeholder='" + pTip + "' style='width:" + width + ";height:" + field.ShowHeight + "px;'  ng-model='" + bindPre + field.FieldName + "' title=\"" + sTitle + "\"  data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "'  " + extendEvent + " >{{" + bindPre + field.FieldName + "}}</textarea>";
            }
            sHtml += field.HtmlEx;
            //添加选择按钮
            string clickMethod1 = "";
            clickMethod1 = "CPFormRoleSelectMethod('" + id + "','" + field.OrganIsCanMultiSel.Value.ToString().ToLower() + "')";
            clickMethod1 = " onclick=\"" + clickMethod1 + ";\" style='cursor:pointer;'";
            string icon1 = "<i class=\"icon iconfont icon-zhucetianjiahaoyou FormControlAfterIconCss\" title=\"点击选择\" " + clickMethod1 + "></i>";
            sHtml += icon1;
            //添加 选择按钮
            if (string.IsNullOrEmpty(field.ExButtonIcon) == false)
            {
                string clickMethod = "";
                if (string.IsNullOrEmpty(field.ExButtonClickMethod) == false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon + " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 隐藏域
    public class CPFormHidden : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
      
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "');\" ";
            string sHtml = "<input type='hidden' id='" + id.ToString() + "' " + ngChange + "  class='textboxCss'  style='width:" + field.ShowWidth + ";'  ng-model='" + bindPre + field.FieldName + "' value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "'  />";
            return sHtml;
        }
    }
    #endregion

    #region 下拉列表
    public class CPFormDropDownList : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            //  <select ng-model="search.stafftype">
            //  <option value="{{$index}}" ng-repeat="t in staffTypes" ng-selected="number==$index">{{t}}</option>         
            //</select>
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "disabled=\"disabled\"";
            }
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if(field.EventName1 != null && field.EventName1.Equals("onchange",StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }
            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange",StringComparison.CurrentCultureIgnoreCase)==false)
            {
                extendEvent = " " + field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string ListRelateTargetField = field.ListRelateTargetField;
            string sHtml = @"<select   id='" + id.ToString() + "' " + readonlyS  + ngChange  +  extendEvent + "  class='selectCss' title=\"" + sTitle + "\"  style='width:" + field.ShowWidth + @";' 
                ng-model='" + bindPre + field.FieldName + "'   data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' ";
            sHtml += @"<option value='' >==请选择==</option>";
            sHtml += @"<option value='{{selectItem.valueEx}}' ng-repeat='selectItem in FormObj.Data." + field.TableName + "_" + field.FieldName + " |ListRelateFilterMethod:\"" + field.TableName + "\":\"" + ListRelateTargetField + "\" track by $index'  on-Repeat-Finished-Render >{{selectItem.textEx}}</option>";     
            sHtml += " />";
            sHtml += field.HtmlEx;
            if (string.IsNullOrEmpty(field.ExButtonIcon) == false)
            {
                string clickMethod = "";
                if (string.IsNullOrEmpty(field.ExButtonClickMethod) == false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon + " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 组合框
    public class CPFormCombox : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {

            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请输入";
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "readonly=\"readonly\"";
                pTip = "";
            }
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }
            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = " " + field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string sHtml = "";
            sHtml += @" <div class='comboxwrapper'>
                            <div class='comboxinput-wrapper'>";
            sHtml += "<input type='text' id='" + id.ToString() + "' " + readonlyS + ngChange + extendEvent +   " class='textboxCss' placeholder='" + pTip + "' style='width:" + field.ShowWidth + ";'  ng-model='" + bindPre + field.FieldName + "' value='{{" + bindPre + field.FieldName + "}}' title=\"" + sTitle + "\" data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'   data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' />";
            sHtml += @" <img src='../../Style/CommonIcon/down.png'>
                        </div>
                        <ul class='comboxcontent' style='display: none'>
                            <li ng-repeat='selectItem in FormObj.Data." + field.TableName + "_" + field.FieldName + @" track by $index'>{{selectItem.textEx}}</li>
                        </ul>
                    </div>";
            sHtml += field.HtmlEx;
            if (string.IsNullOrEmpty(field.ExButtonIcon) == false)
            {
                string clickMethod = "";
                if (string.IsNullOrEmpty(field.ExButtonClickMethod) == false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon + " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 单选框
    public class CPFormRadio : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            //  <select ng-model="search.stafftype">
            //  <option value="{{$index}}" ng-repeat="t in staffTypes" ng-selected="number==$index">{{t}}</option>         
            //</select>
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "disabled=\"disabled\"";
            }
            string name = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                name += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }

            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + name.ToString() + "_{{$index}}','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = " " + field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string sHtml = "";
            sHtml = " <div  class=\"radioDivParentCss\" style='width:" + field.ShowWidth + "; ' ng-repeat='selectItem in FormObj.Data." + field.TableName + "_" + field.FieldName + " track by $index'><input type=\"radio\"  title=\"" + sTitle + "\" class='radioCss'  " + readonlyS + ngChange+ extendEvent + "  value =\"{{selectItem.valueEx}}\"  ng-checked=\"SetRadioFieldChecked('{{" + bindPre + field.FieldName + "}}','{{selectItem.valueEx}}')\"    ng-model='" + bindPre + field.FieldName + "' name='" + name.ToString() + "'  id='" + name.ToString() + "_{{$index}}'  data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "'  /><label class='radioLabelCss' for=\"" + name.ToString() + "_{{$index}}\">{{selectItem.textEx}}</label></div>"; 
            return sHtml;
        }
    }
    #endregion

    #region 复选框
    public class CPFormCheckBox : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string sTitle = field.FieldTip;
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            //  <select ng-model="search.stafftype">
            //  <option value="{{$index}}" ng-repeat="t in staffTypes" ng-selected="number==$index">{{t}}</option>         
            //</select>
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "disabled=\"disabled\"";
            }
            string name = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                name += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }

            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + name.ToString() + "_{{$index}}','" + onChangeEx + "');\" ";

            string sHtml = "";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = " " + field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            sHtml = " <div  class=\"checkboxDivParentCss\" style='width:" + field.ShowWidth + "; ' ng-repeat='selectItem in FormObj.Data." + field.TableName + "_" + field.FieldName + " track by $index'><input type=\"checkbox\" title=\"" + sTitle + "\"  class='checkboxCss'  " + readonlyS + ngChange + extendEvent +  " ng-checked=\"SetCheckBoxFieldChecked('{{" + bindPre + field.FieldName + "}}','{{selectItem.valueEx}}')\"  ng-click=\"SetCheckBoxFieldValue('" + isExtendTable.ToString().ToLower() + "','{{" + bindPre + "CPFormDataIndex}}','" + field.TableName + "','" + field.FieldName + "'," + field.Id + ")\" value =\"{{selectItem.valueEx}}\"      name='" + name.ToString() + "'  id='" + name.ToString() + "_{{$index}}'  data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' /><label class='checkboxLabelCss' for=\"" + name.ToString() + "_{{$index}}\">{{selectItem.textEx}}</label></div>";
            return sHtml;
        }
    }
    #endregion

    #region 时间选择框
    public class CPFormTimeSelect : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string sTitle = field.FieldTip;
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请输入";
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "readonly=\"readonly\"";
                pTip = "";
            }
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }

            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string timeFormat = field.TimeFormat;
            if (string.IsNullOrEmpty(timeFormat))
                timeFormat = "yyyy-MM-dd";
            string layerUIInnerType = "";
            if(timeFormat.Trim().Equals("yyyy",StringComparison.CurrentCultureIgnoreCase))
            {
                layerUIInnerType = "year";
            }
            else if (timeFormat.Trim().Equals("yyyy-mm", StringComparison.CurrentCultureIgnoreCase))
            {
                layerUIInnerType = "month";
            }
            else if (timeFormat.Trim().Equals("yyyy-mm-dd", StringComparison.CurrentCultureIgnoreCase))
            {
                layerUIInnerType = "date";
            }
            else if (timeFormat.Trim().Equals("hh-mm-ss", StringComparison.CurrentCultureIgnoreCase))
            {
                layerUIInnerType = "time";
            }
            else if (timeFormat.Trim().Equals("YYYY-MM-DD hh:mm:ss", StringComparison.CurrentCultureIgnoreCase))
            {
                layerUIInnerType = "datetime";
            }
            //string isTime = "";
            //if (timeFormat.IndexOf("hh") != -1)
            //    isTime = "true";
            string onClick = "";// " ng-click=\"SetDateSel($event,'" + timeFormat + "','" + isTime + "')\" ";
                                //onClick = "";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = " " + field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            string sHtml = "<input type='text' id='" + id + "' " + readonlyS + ngChange + onClick + extendEvent + " title=\"" + sTitle + "\" class='textboxCss layerTimeSel' placeholder='" + pTip + "' data-timeFormat='" + timeFormat + "' data-layerUIInnerType='" + layerUIInnerType + "' style ='width:" + field.ShowWidth + ";'  ng-model='" + bindPre + field.FieldName + "' value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'  data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "'  />";
            sHtml += field.HtmlEx;
            if (string.IsNullOrEmpty(field.ExButtonIcon) == false)
            {
                string clickMethod = "";
                if (string.IsNullOrEmpty(field.ExButtonClickMethod) == false)
                {
                    clickMethod = CPExpressionHelper.Instance.RunCompile(field.ExButtonClickMethod);
                    clickMethod = " onclick=\"" + clickMethod + ";\" style='cursor:pointer;'";
                }
                string icon = "<i class=\"icon iconfont " + field.ExButtonIcon + " FormControlAfterIconCss\" title=\"" + field.ExButtonTip + "\" " + clickMethod + "></i>";
                sHtml += icon;
            }
            return sHtml;
        }
    }
    #endregion

    #region 编辑器
    public class CPFormUEditor : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请输入";
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "readonly=\"readonly\"";
                pTip = "";
            }
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }

            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";
            string extendEvent = "";
            if (string.IsNullOrEmpty(field.EventName1) == false && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent = " " + field.EventName1 + "=\"" + field.EventMethod1 + "\" ";
            }
            if (string.IsNullOrEmpty(field.EventName2) == false && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                extendEvent += " " + field.EventName2 + "=\"" + field.EventMethod2 + "\" ";
            }
            //注意 ngurl，里不直接使用{{}}绑定，必须使用如下拼接字符串的方式才可以
            string url = "{{'" +CPAppContext.CPWebRootPath() + "/Plat/Form/Baidueditor.html?IFrameHeight="+ field.ShowHeight + "&TableName=" + field.TableName + "&FieldName=" + field.FieldName + "&CPFormDataIndex='+" + bindPre + "CPFormDataIndex}}";
            string sHtml = "<input type='text' id='" + id + "' " + readonlyS + ngChange + extendEvent +  "  placeholder='" + pTip + "' style='display:none; width:" + field.ShowWidth + ";'  ng-model='" + bindPre + field.FieldName + "' value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'   data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' />";
            sHtml += " <iframe   id='" + id + "_UEditor'  ng-src=" + url + "    frameborder='0' scrolling='no' width='" + field.ShowWidth + "' height='" + field.ShowHeight+ "px'    ></iframe>";
            return sHtml;
        }
    }
    #endregion


    #region 附件上传
    public class CPFormFileUpload : ICPFormControl
    {
        public string FormatHtml( CPFormField field, CPFormFieldRight fieldRight, bool isExtendTable)
        {
            if(string.IsNullOrEmpty(field.FieldName) || string.IsNullOrEmpty(field.FieldValueName))
            {
                throw new Exception("附件上传控件必须配置两个字段，其中显示字段存储文件名，值字段存储文件路径！");
            }
            string pTip = field.FieldPlaceHolder;
            if (string.IsNullOrEmpty(pTip))
                pTip = "请输入";
            string bindPre = "";
            if (isExtendTable)
            {
                bindPre = "item.";
            }
            else
            {
                bindPre = "FormObj.Data." + field.TableName + ".";
            }
            string readonlyS = "";
            if (field.FieldStatus == CPFormEnum.FieldStatusEnum.Read)
            {
                readonlyS = "readonly=\"readonly\"";
                pTip = "";
            }
            string id = "CPForm_" + field.Id.ToString();
            if (isExtendTable)
            {
                id += "_{{" + bindPre + "CPFormDataIndex}}";
            }
            string delIcon = "";
            if (string.IsNullOrEmpty(readonlyS))
                delIcon = "<a href='javascript:;' ng-click=\"DelAttachFile('" + field.TableName + "','" + field.FieldName + "','" + field.FieldValueName + "'," + bindPre + "CPFormDataIndex,item);\" class=\"AttachFileADel\"><img src='" +CPAppContext.CPWebRootPath() +"/Style/CommonIcon/Delete1.png'/><a>";
            string onChangeEx = "";
            if (field.EventName1 != null && field.EventName1.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod1;
            }
            if (field.EventName2 != null && field.EventName2.Equals("onchange", StringComparison.CurrentCultureIgnoreCase))
            {
                onChangeEx = field.EventMethod2;
            }

            string ngChange = " ng-change=\"CPFormChange(" + field.Id + ",'" + id + "','" + onChangeEx + "');\" ";           
            string sHtml = "<input type='text' id='" + id + "' " + readonlyS + ngChange + "  placeholder='" + pTip + "' style='display:none; width:" + field.ShowWidth + ";'  ng-model='" + bindPre + field.FieldName + "' value='{{" + bindPre + field.FieldName + "}}' data-dataTableRowIndex='{{" + bindPre + "CPFormDataIndex}}' data-tableName='" + field.TableName + "'   data-fieldName='" + field.FieldName + "' data-fieldValueName='" + field.FieldValueName + "' />";
            sHtml += " <div ng-repeat='item in " + bindPre + field.FieldValueName + " |SplitStringToArray track by $index'  ><a href='javascript:;' ng-click=\"ViewAttachFile(item);\" class=\"AttachFileAFileName\">{{item |ShowFileName}}</a>" + delIcon + "</div>";
            if(string.IsNullOrEmpty(readonlyS))
            {
                //field.TableName + "&FieldName=" + field.FieldName + "&CPFormDataIndex='+" + bindPre + "CPFormDataIndex}}";
                sHtml += "<div><a href=\"javascript:;\"  ng-click=\"UploadNewFile('" + field.TableName + "','" + field.FieldName + "','" + field.FieldValueName + "'," + bindPre + "CPFormDataIndex,'" + field.FileAllowType.Trim()+ "','" + field.FileAllowCount+ "')\"><img src='../../Style/CommonIcon/add-red.png' title='上传附件'/></a></div>";
            }
            return sHtml;
        }
    }
    #endregion
}
