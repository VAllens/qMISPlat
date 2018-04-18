using AutoMapper;
using CPFrameWork.Global;
using CPFrameWork.UIInterface.Form;
using CPFrameWork.Utility.DbOper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CPFrameWork.UIInterface.Form
{

    public class FormEngineController : CPWebApiBaseController
    {


        #region 获取配置信息
        public class CPFormClient : CPForm
        {
            // public List<CPFormFieldRight> FieldRightCol { get; set; }
            public List<CPFormFieldInit> FieldInitCol { get; set; }
        }
        public class GetFormInfoReturn : CPWebApiBaseReturnEntity
        {
            public CPFormClient Form { get; set; }
            public string ViewHTML { get; set; }
            public string FormDataJSON { get; set; }

        }
        [HttpGet]
        public GetFormInfoReturn GetFormInfo(string FormCode, string SceneCode, string ViewCode, string InitGroupCode,
            string RightGroupCode, string PKValues
            , int CurUserId, string CurUserIden, string DeviceType = "1",
            bool IsView = false
            )
        {
            GetFormInfoReturn re = new GetFormInfoReturn();
            try
            {
                base.SetHeader();
                FormCode = CPAppContext.FormatSqlPara(FormCode);
                SceneCode = CPAppContext.FormatSqlPara(SceneCode);
                ViewCode = CPAppContext.FormatSqlPara(ViewCode);
                InitGroupCode = CPAppContext.FormatSqlPara(InitGroupCode);
                RightGroupCode = CPAppContext.FormatSqlPara(RightGroupCode);
                PKValues = CPAppContext.FormatSqlPara(PKValues);
                CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
                DeviceType = CPAppContext.FormatSqlPara(DeviceType);

                if (this.CheckUserIden(CurUserId, CurUserIden) == false)
                {
                    re.Result = false;
                    re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                    return re;
                }
                CPForm form = CPFormTemplate.Instance(CurUserId).GetForm(FormCode, true, true, false, false, false, true);
                form.ViewCol = new List<CPFormView>();
                form.ViewCol.Add(CPFormTemplate.Instance(CurUserId).GetFormView(ViewCode, true));
                //re.Form = EmitMapper.ObjectMapperManager.DefaultInstance.GetMapper<CPForm, CPFormClient>()
                //                                  .Map(form); 
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<CPForm, CPFormClient>();
                });

                re.Form = AutoMapper.Mapper.Map<CPFormClient>(form);
                CPFormGroup rightGroup = CPFormTemplate.Instance(CurUserId).GetFormRightGroup(RightGroupCode, true);
                // re.FieldRightCol = (rightGroup == null ? null : rightGroup.FieldRightCol);
                CPFormGroup initGroup = CPFormTemplate.Instance(CurUserId).GetFormInitGroup(InitGroupCode, true);
                re.Form.FieldInitCol = (initGroup == null ? null : initGroup.FieldInitCol);
                re.ViewHTML = CPFormTemplate.Instance(CurUserId).FormatFormViewHtml(form, form.FieldCol, form.ViewCol[0],
                    (rightGroup == null ? null : rightGroup.FieldRightCol),
                    CPEnum.ConvertDeviceTypeEnum(int.Parse(DeviceType)),true, IsView);
                //获取应用场景
                re.Form.UseSceneCol = new List<CPFormUseScene>();
                re.Form.UseSceneCol.Add(CPFormTemplate.Instance(CurUserId).GetFormUseScene(SceneCode, true));
                re.Form.UseSceneCol.ForEach(t =>
                {
                    t.FormSavedInfo = CPExpressionHelper.Instance.RunCompile(t.FormSavedInfo);
                  
                    //处理按钮是否显示
                    List<CPFormUseSceneFunc> funcCol = new List<CPFormUseSceneFunc>();
                    t.FuncCol.ForEach(func => {
                        if(func.FuncIsShowInView == CPFormEnum.FuncIsShowInViewEnum.ShowAll)
                        {
                            funcCol.Add(func);
                        }
                        else if(func.FuncIsShowInView == CPFormEnum.FuncIsShowInViewEnum.OnlyWriteShow)
                        {
                            if (IsView == false)
                                funcCol.Add(func);
                        }
                        else if(func.FuncIsShowInView == CPFormEnum.FuncIsShowInViewEnum.OnlyReadShow)
                        {
                            if (IsView == true)
                            {
                                funcCol.Add(func);
                            }
                        }
                    });
                    #region 添加修改配置按钮
                    if (re.Form.SysId.HasValue && re.Form.SysId.Value.Equals(CPAppContext.InnerSysId) == false)
                    {
                        string UserAdminSysIds = CPExpressionHelper.Instance.RunCompile("${CPUser.UserAdminSysIds()}");
                        if (UserAdminSysIds.Split(',').ToList().Contains(re.Form.SysId.ToString()))
                        {
                            CPFormUseSceneFunc func1 = new CPFormUseSceneFunc();
                            func1.FuncTitle = "修改配置";
                            func1.ShowOrder = 999999;
                            func1.FuncIcon = "icon-shezhi1";
                            func1.IsControlByRight = false;
                            func1.FuncIsShowInView = CPFormEnum.FuncIsShowInViewEnum.ShowAll;
                            func1.FuncExeJS = "CPFormUpdateConfig(" + re.Form.Id + ")";
                            func1.SceneID = t.Id;
                            funcCol.Add(func1);
                        }
                    }
                    #endregion
                    t.FuncCol = funcCol;
                    t.FuncCol = t.FuncCol.OrderBy(c => c.ShowOrder).ToList();
                });

                //读取真实数据
                re.FormDataJSON = CPFormEngine.Instance(CurUserId).GetFormDataJSON(form, form.ChildTableCol, 
                    form.FieldCol, re.Form.FieldInitCol, PKValues, re.Form.UseSceneCol[0]);
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 根据配置，自动同步 字段信息
        [HttpGet]

        //http://localhost:9000/CPSite/api/FormEngine/SynFormFieldInfo?FormId=1&CurUserId=1&CurUserIden=E0A04F94-90D5-4C2A-8C9D-836813F73DC4
        public CPWebApiBaseReturnEntity SynFormFieldInfo(int FormId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = CPFormTemplate.Instance(CurUserId).SynFieldFromDb(FormId);
                if (re.Result == false)
                {
                    re.ErrorMsg = "从数据库同步字段信息时出错了！";
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 自动初始化一个默认的排版组
        [HttpGet]
        //http://localhost:9000/CPSite/api/FormEngine/InitFormDefaultView?FormId=1&CurUserId=1&CurUserIden=E0A04F94-90D5-4C2A-8C9D-836813F73DC4
        public CPWebApiBaseReturnEntity InitFormDefaultView(int FormId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = CPFormTemplate.Instance(CurUserId).InitFormDefaultView(FormId);
                if (re.Result == false)
                {
                    re.ErrorMsg = "自动初始化一个默认视图时出错！";
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 自动初始化一个使用编辑器的排版组
        [HttpGet]
        //http://localhost:9000/CPSite/api/FormEngine/InitFormDefaultView?FormId=1&CurUserId=1&CurUserIden=E0A04F94-90D5-4C2A-8C9D-836813F73DC4
        public CPWebApiBaseReturnEntity InitFormDefaultViewForEditor(int FormId, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = CPFormTemplate.Instance(CurUserId).InitFormDefaultViewForEditor(FormId);
                if (re.Result == false)
                {
                    re.ErrorMsg = "自动初始化一个默认视图时出错！";
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion
        #region 保存数据
        public class SaveFormDataInput
        {
            public int CurUserId { get; set; }
            public string CurUserIden { get; set; }
            public string FormDataJSON { get; set; }
            public string FormCode { get; set; }
            public string PKValue { get; set; }
            public string SceneCode { get; set; }
        }
        public class SaveFormDataReturn : CPWebApiBaseReturnEntity
        {
            public string PKValues { get; set; }
        }
        [HttpPost]
        public SaveFormDataReturn SaveFormData([FromBody] SaveFormDataInput input)
        {
            base.SetHeader();
            SaveFormDataReturn re = new SaveFormDataReturn();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                CPForm form = CPFormTemplate.Instance(input.CurUserId).GetForm(input.FormCode, true, true, false, false, false, false);
                string errormsg;
                string pkValues = input.PKValue;
                CPFormUseScene useScene = CPFormTemplate.Instance(input.CurUserId).GetFormUseScene(input.SceneCode, false);
                re.Result = CPFormEngine.Instance(input.CurUserId).SaveData(form, form.ChildTableCol, form.FieldCol,
                    ref pkValues, input.FormDataJSON, useScene, out errormsg);
                if (re.Result == false)
                    re.ErrorMsg = errormsg;
                else
                {
                    re.PKValues = pkValues;
                }
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 保存初始化数据
        public class UpdateFormInitDataInput
        {
            public int CurUserId { get; set; }
            public string CurUserIden { get; set; }
            public int FormId { get; set; }
            public int GroupId { get; set; }
            public List<UpdateGridDataInputItem> Items { get; set; }
        }
        public class UpdateGridDataInputItem
        {
            public string DataPK { get; set; }
            public List<string> FieldNamCol { get; set; }
            public List<string> FieldValueCol { get; set; }
        }
        [HttpPost]
        public CPWebApiBaseReturnEntity UpdateFormInitData([FromBody] UpdateFormInitDataInput input)
        {
            base.SetHeader();

            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                DbHelper dbHelper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
                StringBuilder sb = new StringBuilder();
                sb.Append("DELETE FROM Form_FieldInitValue WHERE FormId=" + input.FormId + " AND GroupID=" + input.GroupId);

                input.Items.ForEach(t =>
                {
                    string strSql = "";
                    strSql = "INSERT INTO Form_FieldInitValue";
                    string fieldNames = "";
                    string fieldValues = "";
                    bool needAdd = true;
                    for (int i = 0; i < t.FieldNamCol.Count; i++)
                    {
                        if (t.FieldNamCol[i].Equals("InitInfo", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (string.IsNullOrEmpty(t.FieldValueCol[i].Trim()))
                            {
                                needAdd = false;
                                break;
                            }
                        }
                        string sValue = t.FieldValueCol[i];
                        if (string.IsNullOrEmpty(sValue) == false)
                            sValue = sValue.Replace("'", "''");
                        else
                            sValue = "";
                        if (i == 0)
                        {
                            fieldNames += t.FieldNamCol[i];
                            fieldValues += "'" + sValue + "'";
                        }
                        else
                        {
                            fieldNames += "," + t.FieldNamCol[i];
                            fieldValues += ",'" + sValue + "'";
                        }
                    }
                    if (needAdd == false)
                        return;
                    fieldNames += ",GroupID,FieldId,FormId";
                    fieldValues += "," + input.GroupId + "," + t.DataPK + "," + input.FormId;
                    strSql += "(" + fieldNames + ") values (" + fieldValues + ")";
                    if (sb.Length > 0)
                        sb.Append(";");
                    sb.Append(strSql);

                });
                if (sb.Length > 0)
                {
                    dbHelper.ExecuteNonQuery(sb.ToString());
                }
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion


        #region 根据视图ID，获取所有字段信息
        public class GetFormFieldByViewIdReturn:CPWebApiBaseReturnEntity
        {
            public CPFormView View { get; set; }
            public List<CPFormField> FieldCol { get; set; }
        }
        [HttpGet]
        public GetFormFieldByViewIdReturn GetFormViewByViewId(int ViewId
           , int CurUserId, string CurUserIden
           )
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetFormFieldByViewIdReturn re = new GetFormFieldByViewIdReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.FieldCol = CPFormTemplate.Instance(CurUserId).GetFormFieldByViewId(ViewId);
                re.View = CPFormTemplate.Instance(CurUserId).GetFormViewByViewId(ViewId, false);
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion


        #region 保存表单视图
        public class SaveViewHTMLInput
        {
            public int CurUserId { get; set; }
            public string CurUserIden { get; set; }
            public string ViewHTML { get; set; }
            public int ViewId { get; set; }
        }
        [HttpPost]
        public CPWebApiBaseReturnEntity SaveViewHTML([FromBody] SaveViewHTMLInput input )
        {
            base.SetHeader();
            GetFormFieldByViewIdReturn re = new GetFormFieldByViewIdReturn();
            if (this.CheckUserIden(input.CurUserId, input.CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                re.Result = CPFormTemplate.Instance(input.CurUserId).SaveViewHTML(input.ViewId, input.ViewHTML);
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion


        #region 根据数据库链接实例，获取数据库库名和所有的表名和主键字段名
        public class GetDbInfoReturn:CPWebApiBaseReturnEntity
        {
            public string DbName { get; set; }
            public List<GetDbInfoReturnItem> TableCol { get; set; }

        }
        public class GetDbInfoReturnItem
        {
            public string TableName { get; set; }
            public string PKNames { get; set; }
        }
        public GetDbInfoReturn GetDbInfo(string dbIns, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetDbInfoReturn re = new GetDbInfoReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                re.DbName = CPAppContext.GetDbName(dbIns);
                List<CPDbTable> tableCol = CPAppContext.GetTable(dbIns);
                re.TableCol = new List<GetDbInfoReturnItem>();
                tableCol.ForEach(t =>
                {
                    GetDbInfoReturnItem i = new GetDbInfoReturnItem();
                    i.TableName = t.TableName;
                    i.PKNames = t.PKNames;
                    re.TableCol.Add(i);
                });
                List<CPDbTable> viewCol = CPAppContext.GetView(dbIns);
                viewCol.ForEach(t =>
                {
                    GetDbInfoReturnItem i = new GetDbInfoReturnItem();
                    i.TableName = t.TableName;
                    i.PKNames = t.PKNames;
                    re.TableCol.Add(i);
                });
                re.TableCol = re.TableCol.OrderBy(c => c.TableName).ToList();
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion


        #region 根据数据库链接实例，获取数据库库名
        public class GetDbNameReturn : CPWebApiBaseReturnEntity
        {
            public string DbName { get; set; } 

        }
        
        public GetDbNameReturn GetDbName(string dbIns, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            GetDbNameReturn re = new GetDbNameReturn();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            re.Result = true;
            try
            {
                re.DbName = CPAppContext.GetDbName(dbIns);

                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion

        #region 导出配置
        [HttpGet]
        public FileResult DownloadFormConfig(string FormIds, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                throw new Exception("系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！");
            } 
            FormIds = FormIds.Replace("@", ",");
            FormIds = CPAppContext.FormatSqlPara(FormIds);
            List<int> col = new List<int>();
            FormIds.Split(',').ToList().ForEach(t => {
                if (string.IsNullOrEmpty(t) == false)
                    col.Add(int.Parse(t));
            });
            string sXml = CPFormTemplate.Instance(CurUserId).GetFormConfigXml(col);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(sXml);
            return File(byteArray, "application/x-msdownload", "表单配置.CPXml");
        }
        #endregion

        #region 根据配置文件新增或修改配置
        [HttpPost]
        public CPWebApiBaseReturnEntity SynFormConfig(int TargetSysId, bool IsCreateNew, int CurUserId, string CurUserIden)
        {
            base.SetHeader();
            CurUserIden = CPAppContext.FormatSqlPara(CurUserIden);
            CPWebApiBaseReturnEntity re = new CPWebApiBaseReturnEntity();
            if (this.CheckUserIden(CurUserId, CurUserIden) == false)
            {
                re.Result = false;
                re.ErrorMsg = "系统检测到非法获取数据，请传入正确的用户会话Key与用户Id参数！";
                return re;
            }
            try
            {
                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    //  var filename = ContentDispositionHeaderValue
                    //                   .Parse(file.ContentDisposition)
                    //                .FileName
                    //                .Trim('"');
                    ////  filename = _FilePath + $@"\{filename}";
                    //  size += file.Length;
                    byte[] bData = null;
                    using (var fileStream = file.OpenReadStream())
                    using (var ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        bData = ms.ToArray();
                        //var fileBytes = ms.ToArray();
                        //string s = Convert.ToBase64String(fileBytes);
                        //// act on the Base64 data
                    }
                    if (bData != null)
                    {
                        if (IsCreateNew)
                        {
                            re.Result = CPFormTemplate.Instance(CurUserId).InitFormFromConfigXml(TargetSysId, bData);
                        }
                        else
                        {
                            re.Result = CPFormTemplate.Instance(CurUserId).SyncFormFromConfigXml(TargetSysId, bData);
                        }
                    }
                }
                re.Result = true;
                return re;
            }
            catch (Exception ex)
            {
                re.Result = false;
                re.ErrorMsg = ex.Message.ToString();
                return re;
            }
        }
        #endregion
    }
}