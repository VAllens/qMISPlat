using CPFrameWork.Global;
using CPFrameWork.Utility;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; 
using Newtonsoft.Json;
using Remotion.Linq.Parsing.Structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.UIInterface.Form
{
   public class CPFormEngine
    {

        


        #region 获取实例 

        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit(IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CPCommonDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPCommonIns")));

            services.TryAddTransient<ICPCommonDbContext,  CPCommonDbContext>(); 
            services.TryAddTransient<BaseCPFormRep, CPFormRep>(); 
            services.TryAddTransient<CPFormEngine, CPFormEngine>();
        }
        public static CPFormEngine Instance(int curUserId)
        {
            CPFormEngine iObj = CPAppContext.GetService<CPFormEngine>();
            iObj.CurUserId = curUserId;
            return iObj;
        }

         
        #endregion
        public int CurUserId { get; set; }


        private BaseCPFormRep _CPFormRep;
        
        public CPFormEngine(
            BaseCPFormRep CPFormRep
            
            )
        {
            this._CPFormRep = CPFormRep;
           
        }

        public DataSet GetFormData(CPForm form, List<CPFormChildTable> childTableCol, List<CPFormField> fieldCol, string pkValue)
        {
            return this._CPFormRep.ReadRealData(form, childTableCol, fieldCol, pkValue);
        }
        public string GetFormDataJSON(CPForm form, List<CPFormChildTable> childTableCol, List<CPFormField> fieldCol, List<CPFormFieldInit> initValueCol, 
            string pkValue,CPFormUseScene useScene)
        {
            string sJSON = "";
            Dictionary<string, string> col = new Dictionary<string, string>();
            DataSet ds = this.GetFormData(form, childTableCol, fieldCol, pkValue);
            if (string.IsNullOrEmpty(pkValue)==false)
            {
                #region 有数据
                if (childTableCol != null && childTableCol.Count > 0)
                {
                    childTableCol.ForEach(t => {
                        if (ds.Tables[t.TableName].Rows.Count <= 0)
                        {
                            DataRow drChild = ds.Tables[t.TableName].NewRow();
                            foreach (DataColumn dc in ds.Tables[t.TableName].Columns)
                            {
                                drChild[dc.ColumnName] = DBNull.Value;
                            }
                            ds.Tables[t.TableName].Rows.Add(drChild);
                        }
                    });
                }

                #endregion
            }
            else
            {
                #region 无数据，新增 ,自动给每个表加一条空的数据
                DataRow drMain = ds.Tables[form.MainTableName].NewRow();
                foreach(DataColumn dc in  ds.Tables[form.MainTableName].Columns)
                {
                    drMain[dc.ColumnName] = DBNull.Value;
                }
                ds.Tables[form.MainTableName].Rows.Add(drMain);
                if (childTableCol != null && childTableCol.Count > 0)
                {
                    childTableCol.ForEach(t => {
                        DataRow drChild = ds.Tables[t.TableName].NewRow();
                        foreach (DataColumn dc in ds.Tables[t.TableName].Columns)
                        {
                            drChild[dc.ColumnName] = DBNull.Value;
                        }
                        ds.Tables[t.TableName].Rows.Add(drChild);
                    });
                }
                
                #endregion
            }
            //加入初始化的代码
            ds = this.InitFieldValue(form, childTableCol, fieldCol, initValueCol, ds, pkValue);
            if(string.IsNullOrEmpty(useScene.FormLoadHandler)==false)
            {
                string[] sArray = useScene.FormLoadHandler.Split(';');
                for(int i =0;i< sArray.Length;i++)
                {
                    if (string.IsNullOrEmpty(sArray[i]))
                        continue;
                    try
                    {
                        ICPFormBeforeLoadEventArgs e = new ICPFormBeforeLoadEventArgs(form, pkValue, ds);
                        ICPFormBeforeLoad inter = Activator.CreateInstance(Type.GetType(sArray[i])) as ICPFormBeforeLoad;
                        inter.BeforeLoad(e);

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("调用表单加载前扩展二次开发方法【" + sArray[i] + "】时出错，错误信息如下 ：" + ex.Message);
                    }
                }
             
            }
            foreach (DataTable dt in ds.Tables)
            {
                col.Add(dt.TableName, CPUtils.DataTable2Json2(dt));
            }
            #region 获取所有下拉列表的数据源
            fieldCol.ForEach(t => {
                if(t.ControlType == CPFormEnum.ControlTypeEnum.DropDownList 
                || t.ControlType == CPFormEnum.ControlTypeEnum.Radio || t.ControlType == CPFormEnum.ControlTypeEnum.CheckBox
                )
                {
                    if (string.IsNullOrEmpty(t.ListDbIns))
                    {
                        throw new Exception("字段[" + t.FieldName + "]配置成下拉列表或复选框或单选，但未配置数据源数据库链接实例");
                    }
                    DbHelper _helper = new DbHelper(t.ListDbIns,CPAppContext.CurDbType());
                    string s = t.ListSql;
                    s = CPExpressionHelper.Instance.RunCompile(s);
                    if(string.IsNullOrEmpty(s))
                    {
                        throw new Exception("字段[" + t.FieldName + "]配置成下拉列表或复选框或单选，但未配置数据源SQL语句");
                    }
                    DataTable dt = _helper.ExecuteDataSet(s).Tables[0];
                    DataTable dtNew = new DataTable();
                    dtNew.Columns.Add(new DataColumn() {  ColumnName = "textEx",DataType = typeof(string)});
                   
                    
                    dtNew.Columns.Add(new DataColumn() { ColumnName = "valueEx", DataType = typeof(string) });
                    dtNew.Columns.Add(new DataColumn() { ColumnName = "listRelateEx", DataType = typeof(string) });
                    if (t.ControlType == CPFormEnum.ControlTypeEnum.DropDownList)
                    {
                        DataRow newDR0 = dtNew.NewRow();
                        newDR0["textEx"] = "==请选择==";
                        newDR0["valueEx"] = "";
                        newDR0["listRelateEx"] = "";
                        dtNew.Rows.Add(newDR0);
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        DataRow newDR = dtNew.NewRow();
                        newDR["textEx"] = dr[0];
                        if (dt.Columns[1].DataType == Type.GetType("System.Boolean"))
                        {
                            newDR["valueEx"] = dr[1].ToString().Trim().ToLower();
                        }
                        else
                        {
                            newDR["valueEx"] = dr[1];
                        }
                        newDR["listRelateEx"] = "";
                        if(string.IsNullOrEmpty(t.ListRelateTargetField)==false)
                        {
                            try
                            {
                                if (dt.Columns[2].DataType == Type.GetType("System.Boolean"))
                                {
                                    newDR["listRelateEx"] = dr[2].ToString().Trim().ToLower();
                                }
                                else
                                {
                                    newDR["listRelateEx"] = dr[2];
                                }
                            }
                            catch(Exception ex)
                            {
                                throw new Exception("字段[" + t.FieldName + "]配置成下拉列表联动，请确保数据源第三个字段值为联动值，配置的SQL为：" + s + "详细信息如下：" + ex.Message.ToString());
                            }
                        }
                        dtNew.Rows.Add(newDR);
                    }
                    col.Add(t.TableName + "_" + t.FieldName, CPUtils.DataTable2Json2(dtNew));
                }
                else if ( t.ControlType == CPFormEnum.ControlTypeEnum.Combox 
                )
                {
                    DbHelper _helper = new DbHelper(t.ListDbIns,CPAppContext.CurDbType());
                    string s = t.ListSql;
                    s = CPExpressionHelper.Instance.RunCompile(s);
                    DataTable dt = _helper.ExecuteDataSet(s).Tables[0];
                    DataTable dtNew = new DataTable();
                    dtNew.Columns.Add(new DataColumn() { ColumnName = "textEx", DataType = typeof(string) });
                    DataRow newDR0 = dtNew.NewRow();
                    newDR0["textEx"] = "";
                    dtNew.Rows.Add(newDR0);
                    foreach (DataRow dr in dt.Rows)
                    {
                        DataRow newDR = dtNew.NewRow(); 
                        if (dt.Columns[0].DataType == Type.GetType("System.Boolean"))
                        {
                            newDR["textEx"] = dr[0].ToString().Trim().ToLower();
                        }
                        else
                        {
                            newDR["textEx"] = dr[0];
                        }
                        dtNew.Rows.Add(newDR);
                    }
                    col.Add(t.TableName + "_" + t.FieldName, CPUtils.DataTable2Json2(dtNew));
                }
            });
            #endregion

            #region 如果没有拓展表，则自动构建一个隐藏的下拉列表，用来解决没有ng-repeat，不能执行ngRepeatFinished事件的问题
            if(form.ChildTableCol.Count <=0)
            {
                DataTable dtNew = new DataTable();
                dtNew.Columns.Add(new DataColumn() { ColumnName = "textEx", DataType = typeof(string) });


                dtNew.Columns.Add(new DataColumn() { ColumnName = "valueEx", DataType = typeof(string) });
               
                col.Add("CPFormTmpHideSelectTable", CPUtils.DataTable2Json2(dtNew));
            }
            #endregion
            sJSON = JsonConvert.SerializeObject(col);
            return sJSON;
        }
        public DataSet InitFieldValue(CPForm form, List<CPFormChildTable> childTableCol, List<CPFormField> fieldCol,List<CPFormFieldInit> initValueCol,DataSet ds, string pkValue)
        {
            if (initValueCol == null || initValueCol.Count <= 0)
                return ds;
            #region  先看看有没有子表的初始化，如果有，针对每个子表再搞一个新的表来，用来存储初始化字段的值，便于在JS里加新行时，再次初始化
            childTableCol.ForEach(cTable => {
                List<CPFormField> fCol = fieldCol.Where(c => c.TableName.Equals(cTable.TableName)).ToList();
                List<int> aFieldIdCol = new List<int>();
                fCol.ForEach(f => {
                    aFieldIdCol.Add(f.Id);
                });
                List<CPFormFieldInit> tmpInitCol = initValueCol.Where(c => aFieldIdCol.Contains(c.FieldId.Value)).ToList();
                if(tmpInitCol.Count >0)
                {
                    DataTable dtNew = new DataTable();
                    dtNew.TableName = cTable.TableName + "_ExtendTableInitValue";
                    tmpInitCol.ForEach(t => {
                        List<CPFormField> tF = fCol.Where(c => c.Id.Equals(t.FieldId.Value)).ToList();
                        DataColumn dc = new DataColumn();
                        dc.DataType = typeof(string);
                        dc.ColumnName = tF[0].FieldName;
                        dtNew.Columns.Add(dc);
                    });
                    DataRow dr = dtNew.NewRow();
                    foreach(DataColumn dc in dtNew.Columns)
                    {
                        dr[dc.ColumnName] = DBNull.Value;
                    }
                    dtNew.Rows.Add(dr);
                    ds.Tables.Add(dtNew);
                }
            });
            #endregion
            initValueCol.ForEach(t =>
            {
                //查找字段
                List< CPFormField> tmpFCol =fieldCol.Where(c => c.Id.Equals(t.FieldId)).ToList();
                if (tmpFCol.Count <= 0)
                    return;
               
                if (t.InitTimeType == CPFormEnum.InitTimeTypeEnum.Add)
                {
                    #region 新增时初始化
                    if (string.IsNullOrEmpty(pkValue))
                    {
                        #region 新增 
                        //看看是不是拓展表的
                        bool isExtendTable = false;
                        if (childTableCol.Where(c => c.TableName.Equals(tmpFCol[0].TableName)).Count() > 0)
                        {
                            isExtendTable = true;
                        }
                        int autoIndex = 0;
                        string autoIndexField = "";
                        string sValue = this.GetInitValue(t, out autoIndex, out autoIndexField);
                        ds.Tables[tmpFCol[0].TableName].Rows[0][tmpFCol[0].FieldName] = sValue;
                        if(t.InitType == CPFormEnum.InitTypeEnum.Auto)
                        {
                            ds.Tables[tmpFCol[0].TableName].Rows[0][autoIndexField] = autoIndex;
                        }
                        if(isExtendTable)
                        {
                            ds.Tables[tmpFCol[0].TableName + "_ExtendTableInitValue"].Rows[0][tmpFCol[0].FieldName] = sValue;
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (t.InitTimeType == CPFormEnum.InitTimeTypeEnum.NoValue)
                {
                    #region 无值时初始化
                    //看下是主表字段还是子表字段
                    List<CPFormChildTable> cTCol = childTableCol.Where(c => c.TableName.Equals(tmpFCol[0].TableName)).ToList();
                    if(cTCol.Count <=0)
                    {
                        //主表
                        int autoIndex = 0;
                        string autoIndexField = "";
                        string sValue = this.GetInitValue(t, out autoIndex, out autoIndexField);
                        object obj = ds.Tables[tmpFCol[0].TableName].Rows[0][tmpFCol[0].FieldName];
                        if(Convert.IsDBNull(obj) || obj == null || string.IsNullOrEmpty(obj.ToString().Trim()))
                        {
                            ds.Tables[tmpFCol[0].TableName].Rows[0][tmpFCol[0].FieldName] = sValue;
                            if (t.InitType == CPFormEnum.InitTypeEnum.Auto)
                            {
                                ds.Tables[tmpFCol[0].TableName].Rows[0][autoIndexField] = autoIndex;
                            }
                        }
                       
                    }
                    else
                    {
                        //子表
                        foreach(DataRow dr in ds.Tables[tmpFCol[0].TableName].Rows)
                        {
                            object obj = dr[tmpFCol[0].FieldName];
                            int autoIndex = 0;
                            string autoIndexField = "";
                            string sValue = this.GetInitValue(t, out autoIndex, out autoIndexField);
                            ds.Tables[tmpFCol[0].TableName + "_ExtendTableInitValue"].Rows[0][tmpFCol[0].FieldName] = sValue;
                            if (Convert.IsDBNull(obj) || obj == null || string.IsNullOrEmpty(obj.ToString().Trim()))
                            {
                                dr[tmpFCol[0].FieldName] = sValue;
                                
                                if (t.InitType == CPFormEnum.InitTypeEnum.Auto)
                                {
                                    //感觉自动编号的初始化在拓展表里会有问题
                                    dr[autoIndexField] = autoIndex;
                                }
                            }
                        }
                    }
                    #endregion

                }
                else if (t.InitTimeType == CPFormEnum.InitTimeTypeEnum.All)
                {
                    #region 总是初始化
                    //看下是主表字段还是子表字段
                    List<CPFormChildTable> cTCol = childTableCol.Where(c => c.TableName.Equals(tmpFCol[0].TableName)).ToList();
                    if (cTCol.Count <= 0)
                    {
                        //主表
                        int autoIndex = 0;
                        string autoIndexField = "";
                        string sValue = this.GetInitValue(t, out autoIndex, out autoIndexField);
                        ds.Tables[tmpFCol[0].TableName].Rows[0][tmpFCol[0].FieldName] = sValue;
                        if (t.InitType == CPFormEnum.InitTypeEnum.Auto)
                        {
                            ds.Tables[tmpFCol[0].TableName].Rows[0][autoIndexField] = autoIndex;
                        }

                    }
                    else
                    {
                        //子表
                        foreach (DataRow dr in ds.Tables[tmpFCol[0].TableName].Rows)
                        {
                            object obj = dr[tmpFCol[0].FieldName];
                            int autoIndex = 0;
                            string autoIndexField = "";
                            string sValue = this.GetInitValue(t, out autoIndex, out autoIndexField);
                            dr[tmpFCol[0].FieldName] = sValue;
                            ds.Tables[tmpFCol[0].TableName + "_ExtendTableInitValue"].Rows[0][tmpFCol[0].FieldName] = sValue;
                            if (t.InitType == CPFormEnum.InitTypeEnum.Auto)
                            {
                                //感觉自动编号的初始化在拓展表里会有问题
                                dr[autoIndexField] = autoIndex;
                            }
                        }
                    }
                    #endregion

                }


            });
            return ds;
        }
        private string GetInitValue(CPFormFieldInit init,out  int autoIndex,out string autoIndexField)
        {
            autoIndex = 0;
            autoIndexField = "";
            string sValue = "";
            if (init.InitType == CPFormEnum.InitTypeEnum.StaticValue)
                sValue = init.InitInfo;
            else if (init.InitType == CPFormEnum.InitTypeEnum.Auto)
            {
                sValue = CPAutoNumHelper.Instance().GetNextAutoNum(init.InitInfo, out autoIndex);
                //再获取下存储索引号的字段名
                autoIndexField = CPAutoNumHelper.Instance().GetAutoNum(init.InitInfo).FormAumField;
            }
            else if (init.InitType == CPFormEnum.InitTypeEnum.Expression)
                sValue = CPExpressionHelper.Instance.RunCompile(init.InitInfo);
            else if (init.InitType == CPFormEnum.InitTypeEnum.Sql)
            {
                if (string.IsNullOrEmpty(init.InitSqlDbIns))
                    throw new Exception("获取初始化信息时，存储数据库链接的字段值InitSqlDbIns值为空");
                DbHelper _helper = new DbHelper(init.InitSqlDbIns, CPAppContext.CurDbType());
                string sql = init.InitInfo;
                sql = CPExpressionHelper.Instance.RunCompile(sql);
                object obj = _helper.ExecuteScalar(  sql);
                if (Convert.IsDBNull(obj) == false && obj != null)
                    sValue = obj.ToString();
            }
            else
                sValue = init.InitInfo;
            return sValue;
        }
        #region 保存数据
        public bool SaveData(CPForm form, List<CPFormChildTable> childTableCol, 
            List<CPFormField> fieldCol,ref string pkValue,string formDataJSON,CPFormUseScene useScene, out string errorMsg)
        {
            bool b =  this._CPFormRep.SaveData(form, childTableCol, fieldCol, ref pkValue, formDataJSON, out errorMsg);
            if(b)
            {
               dynamic formData =   JsonConvert.DeserializeObject<dynamic>(formDataJSON);
                if (string.IsNullOrEmpty(useScene.FormSaveExeSql) == false)
                {
                    CPExpressionHelper.Instance.Add(CPFormExpression.DataRowKey, formData);
                    CPExpressionHelper.Instance.Add(CPFormExpression.PKValueKey, pkValue);
                    CPExpressionHelper.Instance.Add(CPFormExpression.MainTableKey, form.MainTableName);
                    CPExpressionHelper.Instance.Add(CPFormExpression.MainTablePKKey, form.PKFieldName);
                    string sql = CPExpressionHelper.Instance.RunCompile(useScene.FormSaveExeSql);
                    CPExpressionHelper.Instance.Remove(CPFormExpression.DataRowKey);
                    CPExpressionHelper.Instance.Remove(CPFormExpression.PKValueKey);
                    CPExpressionHelper.Instance.Remove(CPFormExpression.MainTableKey);
                    CPExpressionHelper.Instance.Remove(CPFormExpression.MainTablePKKey);
                    try
                    {
                        DbHelper _helper = new DbHelper(form.DbIns, CPAppContext.CurDbType());
                        _helper.ExecuteNonQuery(sql);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("表单保存后，执行扩展配置的SQL时出错，SQL语句为【" + sql + "】，错误信息如下 ：" + ex.Message);
                    }
                }
                if (string.IsNullOrEmpty(useScene.FormSaveHandler) == false)
                {
                    string[] sArray = useScene.FormSaveHandler.Split(';');
                    for(int i =0;i<sArray.Length;i++)
                    {
                        if (string.IsNullOrEmpty(sArray[i]))
                            continue;
                        try
                        {
                            ICPFormAfterSaveEventArgs e = new ICPFormAfterSaveEventArgs(form, pkValue, formData);
                            ICPFormAfterSave inter = Activator.CreateInstance(Type.GetType(sArray[i])) as ICPFormAfterSave;
                            inter.AfterSave(e);

                        }
                        catch (Exception ex)
                        {
                            throw new Exception("调用表单保存后扩展二次开发方法【" + sArray[i] + "】时出错，错误信息如下 ：" + ex.Message);
                        }
                    }
                  
                }
            }
            return b;
        }
        #endregion

      
    }
}
