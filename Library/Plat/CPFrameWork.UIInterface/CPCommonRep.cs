using CPFrameWork.Global;
using CPFrameWork.UIInterface.Form;
using CPFrameWork.UIInterface.Grid;
using CPFrameWork.UIInterface.Tab;
using CPFrameWork.UIInterface.Tree;
using CPFrameWork.Utility.DbOper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.UIInterface
{
    #region 表单部分
    public abstract class BaseCPFormRep : BaseRepository<CPForm>
    {
        public BaseCPFormRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public abstract DataSet ReadRealData(CPForm form, List<CPFormChildTable> childTableCol, List<CPFormField> fieldCol, string pkValue);
        public abstract bool SaveData(CPForm form, List<CPFormChildTable> childTableCol,
            List<CPFormField> fieldCol, ref string pkValue, string formDataJSON, out string errorMsg);
        public abstract DataSet GetConfig(List<int> formIdCol);
        public abstract bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew);
    }
    internal class CPFormRep : BaseCPFormRep
    {
        public CPFormRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }

        #region 同步 配置相关
        public override bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew)
        {
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());

            bool b = true;
            #region 先删除数据           
            if (isCreateNew == false)
            {
                string delFormCodes = "";
                foreach (DataRow drMain in ds.Tables["Form_Main"].Rows)
                {
                    if (string.IsNullOrEmpty(delFormCodes)) delFormCodes = drMain["FormCode"].ToString();
                    else delFormCodes += "," + drMain["FormCode"].ToString();
                }
                if (string.IsNullOrEmpty(delFormCodes) == false)
                {
                    string strSql = "SELECT FormId from Form_Main where FormCode in ('" + delFormCodes.Replace(",", "','") + "')";
                    string formIds = "";
                    IDataReader dr = _helper.ExecuteReader(strSql);
                    while(dr.Read())
                    {
                        if (string.IsNullOrEmpty(formIds))
                            formIds = dr["FormId"].ToString();
                        else
                            formIds += "," +  dr["FormId"].ToString();
                    }
                    dr.Close();
                    string delSql = "";
                    delSql += "DELETE   FROM Form_Rule WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_FieldRight WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_FieldInitValue WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_Group WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_UseSceneFunc WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_UseScene WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_View_Inner WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_View WHERE FormId in(" + formIds + ")";
                    delSql += "DELETE   FROM Form_Field WHERE FormId in(" + formIds + ");";
                    delSql += "DELETE   FROM Form_ChildTable WHERE FormId in(" + formIds + ");";
                    delSql += "DELETE   FROM Form_Main WHERE FormId in(" + formIds + ");";
                    _helper.ExecuteNonQuery(delSql);
                    if (!b)
                        throw new Exception("先删除已经存在的配置时出错");
                }
            }
            #endregion

            #region 写入数据
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Form_Main WHERE 1=2; 
                                    SELECT * FROM Form_ChildTable WHERE 1=2;
                                    SELECT * FROM Form_Field WHERE 1=2;
                                    SELECT * FROM Form_View WHERE 1=2;
                                    SELECT * FROM Form_View_Inner WHERE 1=2;
                                    SELECT * FROM Form_UseScene WHERE 1=2;
                                    SELECT * FROM Form_UseSceneFunc WHERE 1=2;
                                    SELECT * FROM Form_Group WHERE 1=2;
                                    SELECT * FROM Form_FieldRight WHERE 1=2;
                                    SELECT * FROM Form_FieldInitValue WHERE 1=2;
                                    SELECT * FROM Form_Rule WHERE 1=2",
                _helper.GetConnection() as SqlConnection);
            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
            // SqlCommandBuilder builder = new SqlCommandBuilder(da);
            //AddWithKey: 自动填充数据表结构,如：主键和限制
            //预设值Add,不填充结构
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
            DataSet dsStruct = new DataSet();
            da.Fill(dsStruct);
            dsStruct.Tables[0].TableName = "Form_Main";
            dsStruct.Tables[1].TableName = "Form_ChildTable";
            dsStruct.Tables[2].TableName = "Form_Field";
            dsStruct.Tables[3].TableName = "Form_View";
            dsStruct.Tables[4].TableName = "Form_View_Inner";
            dsStruct.Tables[5].TableName = "Form_UseScene";
            dsStruct.Tables[6].TableName = "Form_UseSceneFunc";
            dsStruct.Tables[7].TableName = "Form_Group";
            dsStruct.Tables[8].TableName = "Form_FieldRight";
            dsStruct.Tables[9].TableName = "Form_FieldInitValue";
            dsStruct.Tables[10].TableName = "Form_Rule";
            #region Form_Main
            Dictionary<int, int> oldNewFormId = new Dictionary<int, int>();
            foreach (DataRow dr in ds.Tables["Form_Main"].Rows)
            {
                dr["SysId"] = targetSysId;
                if (isCreateNew)
                {
                    dr["FormTitle"] = dr["FormTitle"].ToString() + "_副本";
                    int autoIndex;
                    dr["FormCode"] = CPAutoNumHelper.Instance().GetNextAutoNum("FormCodeAuto", out autoIndex);
                    dr["AutoIndex"] = autoIndex;
                }
                string insertSql = CPAppContext.GetInsertSql("Form_Main", dsStruct.Tables["Form_Main"].Columns, dr);
                insertSql += ";select SCOPE_IDENTITY() as Id;";
                SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                foreach (DataColumn dc in dsStruct.Tables["Form_Main"].Columns)
                {
                    if (dc.AutoIncrement)
                    {
                        continue;
                    }
                    if (dr.Table.Columns.Contains(dc.ColumnName))
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                    }
                    else
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                    }
                }
                int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                oldNewFormId.Add(int.Parse(dr["FormId"].ToString()), newId);
            }

            #endregion

            #region Form_ChildTable
          
            if (ds.Tables["Form_ChildTable"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_ChildTable"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Form_ChildTable", dsStruct.Tables["Form_ChildTable"].Columns, dr);

                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_ChildTable"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert);
                }
            }
            #endregion

            #region Form_Field
            Dictionary<int, int> oldNewFieldId = new Dictionary<int, int>();
            if (ds.Tables["Form_Field"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_Field"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Form_Field", dsStruct.Tables["Form_Field"].Columns, dr);
                    insertSql += ";select SCOPE_IDENTITY() as Id;";
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_Field"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                    oldNewFieldId.Add(int.Parse(dr["FieldId"].ToString()), newId);
                }
            }
            #endregion

            #region Form_View
            Dictionary<int, int> oldNewViewId = new Dictionary<int, int>();
            if (ds.Tables["Form_View"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_View"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    if (isCreateNew)
                    {
                        int autoIndex;
                        dr["ViewCode"] = CPAutoNumHelper.Instance().GetNextAutoNum("FormViewCodeAuto", out autoIndex);
                        dr["AutoIndex"] = autoIndex;
                    }
                    string insertSql = CPAppContext.GetInsertSql("Form_View", dsStruct.Tables["Form_View"].Columns, dr);
                    insertSql += ";select SCOPE_IDENTITY() as Id;";
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_View"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName,DBNull.Value);
                        }
                    }
                    int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                    oldNewViewId.Add(int.Parse(dr["ViewId"].ToString()), newId);
                }
            }
            #endregion

            #region Form_View_Inner 
            if (ds.Tables["Form_View_Inner"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_View_Inner"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    dr["ViewId"] = oldNewViewId[int.Parse(dr["ViewId"].ToString())];
                    dr["FieldId"] = oldNewFieldId[int.Parse(dr["FieldId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Form_View_Inner", dsStruct.Tables["Form_View_Inner"].Columns, dr); 
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_View_Inner"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert).ToString();
                }
            }
            #endregion

            #region Form_UseScene
            Dictionary<int, int> oldNewUseSceneId = new Dictionary<int, int>();
            if (ds.Tables["Form_UseScene"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_UseScene"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    if (isCreateNew)
                    {
                        int autoIndex;
                        dr["SceneCode"] = CPAutoNumHelper.Instance().GetNextAutoNum("FormSceneCodeAuto", out autoIndex);
                        dr["AutoIndex"] = autoIndex;
                    }
                    string insertSql = CPAppContext.GetInsertSql("Form_UseScene", dsStruct.Tables["Form_UseScene"].Columns, dr);
                    insertSql += ";select SCOPE_IDENTITY() as Id;";
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_UseScene"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                    oldNewUseSceneId.Add(int.Parse(dr["SceneID"].ToString()), newId);
                }
            }
            #endregion

            #region Form_UseSceneFunc 
            if (ds.Tables["Form_UseSceneFunc"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_UseSceneFunc"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    dr["SceneID"] = oldNewUseSceneId[int.Parse(dr["SceneID"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Form_UseSceneFunc", dsStruct.Tables["Form_UseSceneFunc"].Columns, dr);
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_UseSceneFunc"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert).ToString();
                }
            }
            #endregion

            #region Form_Group
            Dictionary<int, int> oldNewGroupId = new Dictionary<int, int>();
            if (ds.Tables["Form_Group"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_Group"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    if (isCreateNew)
                    {
                        int autoIndex;
                        dr["GroupCode"] = CPAutoNumHelper.Instance().GetNextAutoNum("FormGroupCodeAuto", out autoIndex);
                        dr["AutoIndex"] = autoIndex;
                    }
                    string insertSql = CPAppContext.GetInsertSql("Form_Group", dsStruct.Tables["Form_Group"].Columns, dr);
                    insertSql += ";select SCOPE_IDENTITY() as Id;";
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_Group"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                    oldNewGroupId.Add(int.Parse(dr["GroupID"].ToString()), newId);
                }
            }
            #endregion

            #region Form_FieldRight 
            if (ds.Tables["Form_FieldRight"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_FieldRight"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    dr["GroupID"] = oldNewGroupId[int.Parse(dr["GroupID"].ToString())];
                    dr["FieldId"] = oldNewFieldId[int.Parse(dr["FieldId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Form_FieldRight", dsStruct.Tables["Form_FieldRight"].Columns, dr);
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_FieldRight"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert).ToString();
                }
            }
            #endregion

            #region Form_FieldInitValue 
            if (ds.Tables["Form_FieldInitValue"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_FieldInitValue"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    dr["GroupID"] = oldNewGroupId[int.Parse(dr["GroupID"].ToString())];
                    dr["FieldId"] = oldNewFieldId[int.Parse(dr["FieldId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Form_FieldInitValue", dsStruct.Tables["Form_FieldInitValue"].Columns, dr);
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_FieldInitValue"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert).ToString();
                }
            }
            #endregion

            #region Form_Rule 
            if (ds.Tables["Form_Rule"] != null)
            {
                foreach (DataRow dr in ds.Tables["Form_Rule"].Rows)
                {
                    dr["FormId"] = oldNewFormId[int.Parse(dr["FormId"].ToString())];
                    dr["FieldId"] = oldNewFieldId[int.Parse(dr["FieldId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Form_Rule", dsStruct.Tables["Form_Rule"].Columns, dr);
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Form_Rule"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert).ToString();
                }
            }
            #endregion

            #endregion
            return b;
        }
        public override DataSet GetConfig(List<int> formIdCol)
        {
            string ids = "";
            formIdCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids))
                    ids = t.ToString();
                else
                    ids += "," + t.ToString();
            });
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
            string strSql = "SELECT * FROM Form_Main WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_ChildTable WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_Field WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_View WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_View_Inner WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_UseScene WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_UseSceneFunc WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_Group WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_FieldRight WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_FieldInitValue WHERE FormId in(" + ids + ");";
            strSql += "SELECT * FROM Form_Rule WHERE FormId in(" + ids + ");";
            DataSet ds = _helper.ExecuteDataSet(strSql);
            ds.Tables[0].TableName = "Form_Main";
            ds.Tables[1].TableName = "Form_ChildTable";
            ds.Tables[2].TableName = "Form_Field";
            ds.Tables[3].TableName = "Form_View";
            ds.Tables[4].TableName = "Form_View_Inner";
            ds.Tables[5].TableName = "Form_UseScene";
            ds.Tables[6].TableName = "Form_UseSceneFunc";
            ds.Tables[7].TableName = "Form_Group";
            ds.Tables[8].TableName = "Form_FieldRight";
            ds.Tables[9].TableName = "Form_FieldInitValue";
            ds.Tables[10].TableName = "Form_Rule";
            return ds;
        }
        #endregion

        #region 读取表单对应物理表的数据
        public override DataSet ReadRealData(CPForm form, List<CPFormChildTable> childTableCol, List<CPFormField> fieldCol, string pkValue)
        {
            DbHelper _helper = new DbHelper(form.DbIns, CPAppContext.CurDbType());
            #region 组SQL
            string sql = "";

            List<CPFormField> FieldCol = fieldCol.Where(t => t.TableName.Equals(form.MainTableName)).ToList();
            sql = "SELECT ";
            string fields = "";
            FieldCol.ForEach(t =>
            {
                if (string.IsNullOrEmpty(fields))
                    fields = t.FieldName;
                else
                    fields += "," + t.FieldName;
            });
            sql += fields + " FROM " + form.MainTableName;
            if (string.IsNullOrEmpty(pkValue))
            {
                sql += " WHERE 1=2";
            }
            else
            {
                sql += " WHERE " + form.PKFieldName + "='" + pkValue + "'";
            }
            if (childTableCol != null && childTableCol.Count > 0)
            {
                childTableCol.ForEach(cTable =>
                {
                    FieldCol = fieldCol.Where(t => t.TableName.Equals(cTable.TableName)).ToList();
                    fields = "";
                    FieldCol.ForEach(t =>
                    {
                        if (t.IsChildTable == false)
                        {
                            if (string.IsNullOrEmpty(fields))
                                fields = t.FieldName;
                            else
                                fields += "," + t.FieldName;
                        }
                    });
                    sql += ";SELECT " + fields + " FROM " + cTable.TableName;
                    if (string.IsNullOrEmpty(pkValue))
                    {
                        sql += " WHERE 1=2";
                    }
                    else
                    {
                        sql += " WHERE " + cTable.RelateFieldName + "='" + pkValue + "'";
                    }


                });
            }
            #endregion
            DataSet ds = _helper.ExecuteDataSet(sql);
            ds.Tables[0].TableName = form.MainTableName;
            if (childTableCol != null && childTableCol.Count > 0)
            {
                int nIndex = 1;
                childTableCol.ForEach(cTable =>
                {
                    ds.Tables[nIndex].TableName = cTable.TableName;
                    nIndex++;
                }
                );
            }
            return ds;

        }
        #endregion

        private SqlCommand GetUpdateCommand(DataRow dr, SqlConnection conn)
        {
            string sql = "UPDATE " + dr.Table.TableName;
            string field = "";
            string pk = "";
            foreach (DataColumn dc in dr.Table.Columns)
            {
                if (dr.Table.PrimaryKey.Contains(dc))
                {
                    if (string.IsNullOrEmpty(pk))
                        pk = dc.ColumnName + "=@" + dc.ColumnName;
                    else
                        pk += " AND " + dc.ColumnName + "=@" + dc.ColumnName;
                }
                else
                {
                    if (string.IsNullOrEmpty(field))
                        field = dc.ColumnName + "=@" + dc.ColumnName;
                    else
                        field += " , " + dc.ColumnName + "=@" + dc.ColumnName;
                }
            }
            sql += " SET " + field + " WHERE " + pk;
            SqlCommand cmd = new SqlCommand(sql, conn);
            foreach (DataColumn dc in dr.Table.Columns)
            {
                cmd.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
              //  cmd.Parameters.Add("@" + dc.ColumnName, GetSqlType(dc.DataType )); 
            }

            return cmd;

        }
        //public   SqlDbType GetSqlType(Type type)
        //{
        //    if (type == typeof(string))
        //        return SqlDbType.NVarChar;

        //    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        //        type = Nullable.GetUnderlyingType(type);

        //    var param = new SqlParameter("", Activator.CreateInstance(type));
        //    return param.SqlDbType;
        //}
        private SqlCommand GetInsertCommand(DataRow dr, SqlConnection conn)
        {
            string sql = "insert into " + dr.Table.TableName;
            string field = "";
            string fieldValue = "";
            foreach (DataColumn dc in dr.Table.Columns)
            {
                if (dc.AutoIncrement)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(field))
                    field = dc.ColumnName;
                else
                    field += " , " + dc.ColumnName;
                if (string.IsNullOrEmpty(fieldValue))
                    fieldValue = "@" + dc.ColumnName;
                else
                    fieldValue += " , @" + dc.ColumnName;

            }
            sql += " ( " + field + ") VALUES ( " + fieldValue + ")";
            SqlCommand cmd = new SqlCommand(sql, conn);
            foreach (DataColumn dc in dr.Table.Columns)
            {
                if (dc.AutoIncrement)
                {
                    continue;
                }
                cmd.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
            }

            return cmd;

        }

        private SqlCommand GetDeleteCommand(DataRow dr, SqlConnection conn)
        {
            string sql = "DELETE FROM  " + dr.Table.TableName;
            string pk = "";
            foreach (DataColumn dc in dr.Table.Columns)
            {
                if (dr.Table.PrimaryKey.Contains(dc))
                {
                    if (string.IsNullOrEmpty(pk))
                        pk = dc.ColumnName + "=@" + dc.ColumnName;
                    else
                        pk += " AND " + dc.ColumnName + "=@" + dc.ColumnName;
                }

            }
            sql += " WHERE " + pk;
            SqlCommand cmd = new SqlCommand(sql, conn);
            foreach (DataColumn dc in dr.Table.Columns)
            {
                cmd.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
            }

            return cmd;

        }

        public override bool SaveData(CPForm form, List<CPFormChildTable> childTableCol,
            List<CPFormField> fieldCol,ref string pkValue, string formDataJSON, out string errorMsg)
        {
            bool b = true;
            errorMsg = "";
            DbHelper _helper = new DbHelper(form.DbIns, CPAppContext.CurDbType());
            var DynamicObject = JsonConvert.DeserializeObject<dynamic>(formDataJSON);
            Dictionary<string, string> tableSql = new Dictionary<string, string>();
            #region 组SQL


            List<CPFormField> FieldCol = fieldCol.Where(t => t.TableName.Equals(form.MainTableName)).ToList();
            string sql = "SELECT ";
            string fields = "";
            FieldCol.ForEach(t =>
            {
                if (string.IsNullOrEmpty(fields))
                    fields = t.FieldName;
                else
                    fields += "," + t.FieldName;
            });
            sql += fields + " FROM " + form.MainTableName;
            if (string.IsNullOrEmpty(pkValue))
            {
                sql += " WHERE 1=2 ";
            }
            else
            {
                sql += " WHERE " + form.PKFieldName + "='" + pkValue + "'";

            }
            tableSql.Add(form.MainTableName, sql);
            sql = "";
            string tmpPKValue0 = pkValue;
            if (childTableCol != null && childTableCol.Count > 0)
            {
                childTableCol.ForEach(cTable =>
                {
                    FieldCol = fieldCol.Where(t => t.TableName.Equals(cTable.TableName)).ToList();
                    fields = "";
                    FieldCol.ForEach(t =>
                    {
                        if (t.IsChildTable == false)
                        {
                            if (string.IsNullOrEmpty(fields))
                                fields = t.FieldName;
                            else
                                fields += "," + t.FieldName;
                        }
                    });
                    sql += ";SELECT " + fields + " FROM " + cTable.TableName;
                    if (string.IsNullOrEmpty(tmpPKValue0))
                    {
                        sql += " WHERE 1=2 ";
                    }
                    else
                    {
                        sql += " WHERE " + cTable.RelateFieldName + "='" + tmpPKValue0 + "'";
                    }
                    tableSql.Add(cTable.TableName, sql);
                    sql = "";

                });
            }
            #endregion
            if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.SqlServer)
            {

                if (string.IsNullOrEmpty(pkValue))
                {
                    #region 新增

                    SqlConnection conn = _helper.GetConnection() as SqlConnection;
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            #region 主表
                            SqlCommand cmd = new SqlCommand(tableSql[form.MainTableName], conn);
                            cmd.Transaction = trans;
                            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
                            // SqlCommandBuilder builder = new SqlCommandBuilder(da);
                            //AddWithKey: 自动填充数据表结构,如：主键和限制
                            //预设值Add,不填充结构
                            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            ds.Tables[0].TableName = form.MainTableName;
                            JObject mainTableObject = JsonConvert.DeserializeObject<JObject>(Convert.ToString(DynamicObject[form.MainTableName]));
                            List<CPFormField> mFieldCol = fieldCol.Where(c => c.TableName.Equals(form.MainTableName)).ToList();
                            DataRow drNew = ds.Tables[form.MainTableName].NewRow();
                            string sqlTmp = "";
                            #region 设置主键字段的值
                            if (form.PKValueType == CPFormEnum.PKValueTypeEnum.GUID)
                                pkValue = Guid.NewGuid().ToString();
                            else if (form.PKValueType == CPFormEnum.PKValueTypeEnum.IntNotSelfIncreasing)
                            {
                                sqlTmp = "select  ISNULL(MAX([" + form.PKFieldName + "]),0) + 1 from [" + form.MainTableName + "]";
                                SqlCommand cmdTmp0 = new SqlCommand(sqlTmp, conn);
                                cmdTmp0.Transaction = trans;
                                object max0 = cmdTmp0.ExecuteScalar();
                                if (DBNull.Value != max0 && null != max0)
                                {
                                    pkValue = max0.ToString();
                                }
                            }
                            #endregion
                            string tmppkValue = pkValue;
                            mFieldCol.ForEach(t =>
                            {
                                if (t.FieldName.Equals(form.PKFieldName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (form.PKValueType == CPFormEnum.PKValueTypeEnum.GUID || form.PKValueType == CPFormEnum.PKValueTypeEnum.IntNotSelfIncreasing)
                                    {
                                        drNew[t.FieldName] = tmppkValue;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }

                                // drNew[t.FieldName] = mainTableObject[t.FieldName];
                                if (ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Int16")
                                     ||
                                     ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Int32")
                                     ||
                                     ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Int64")
                                     ||
                                     ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Double")
                                      ||
                                     ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Single")
                                     )
                                {
                                    string sValue = string.Empty;
                                    sValue = mainTableObject[t.FieldName].ToString();
                                    if (string.IsNullOrEmpty(sValue))
                                    {
                                        drNew[t.FieldName] = DBNull.Value;
                                    }
                                    else
                                    {
                                        drNew[t.FieldName] = sValue;
                                    }
                                }
                                else if (ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Boolean"))
                                {
                                    string sValue = string.Empty;
                                    sValue = mainTableObject[t.FieldName].ToString();
                                    if (string.IsNullOrEmpty(sValue))
                                    {
                                        drNew[t.FieldName] = DBNull.Value;
                                    }
                                    else
                                    {
                                        drNew[t.FieldName] = sValue;
                                    }
                                }
                                else if (ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.DateTime"))
                                {
                                    string sValue = string.Empty;
                                    if (Convert.IsDBNull(mainTableObject[t.FieldName])==false && mainTableObject[t.FieldName] != null)
                                    {
                                        sValue = mainTableObject[t.FieldName].ToString();
                                    }
                                    if (string.IsNullOrEmpty(sValue))
                                    {
                                        drNew[t.FieldName] = DBNull.Value;
                                    }
                                    else
                                    {
                                        drNew[t.FieldName] = sValue;
                                    }
                                } 
                                else
                                {
                                    drNew[t.FieldName] = mainTableObject[t.FieldName];
                                }
                            });
                            ds.Tables[form.MainTableName].Rows.Add(drNew);
                            if (da.InsertCommand == null)
                            {
                                da.InsertCommand = this.GetInsertCommand(drNew, conn);
                                da.InsertCommand.Transaction = trans;
                            }
                            da.Update(ds.Tables[form.MainTableName]);
                            //获取主键的值
                            //pkValue = drNew[form.PKFieldName].ToString();
                            if (form.PKValueType == CPFormEnum.PKValueTypeEnum.IntSelfIncreasing)
                            {
                                sqlTmp = "select Max([" + form.PKFieldName + "]) from [" + form.MainTableName + "]";
                                SqlCommand cmdTmp = new SqlCommand(sqlTmp, conn);
                                cmdTmp.Transaction = trans;
                                object max = cmdTmp.ExecuteScalar();
                                if (DBNull.Value != max && null != max)
                                {
                                    pkValue = max.ToString();
                                }
                            }
                            tmppkValue = pkValue;
                            #endregion

                            #region 子表 
                            childTableCol.ForEach(cTable =>
                            {
                                SqlCommand cmdChild = new SqlCommand(tableSql[cTable.TableName], conn);
                                cmdChild.Transaction = trans;
                                SqlDataAdapter daChild = new System.Data.SqlClient.SqlDataAdapter(cmdChild);
                                //   SqlCommandBuilder builderChild = new SqlCommandBuilder(daChild);
                                //AddWithKey: 自动填充数据表结构,如：主键和限制
                                //预设值Add,不填充结构
                                daChild.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
                                DataSet dsChild = new DataSet();
                                daChild.Fill(dsChild);
                                dsChild.Tables[0].TableName = cTable.TableName;
                                var cTableObject = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(DynamicObject[cTable.TableName]));
                                List<CPFormField> cFieldCol = fieldCol.Where(c => c.TableName.Equals(cTable.TableName) && c.IsChildTable == false).ToList();
                                foreach (var cRowValue in cTableObject)
                                {
                                    #region 先检查下必须 的几个字 段有没有值，没有则不保存这条数据
                                    bool notSave = false;
                                    if (string.IsNullOrEmpty(cTable.FieldNullNotSaveData) == false)
                                    {

                                        string[] sArray = cTable.FieldNullNotSaveData.Split(',');
                                        for (int i = 0; i < sArray.Length; i++)
                                        {
                                            if (fieldCol.Where(c => c.TableName.Equals(cTable.TableName, StringComparison.CurrentCultureIgnoreCase)
                                                       && c.FieldName.Equals(sArray[i], StringComparison.CurrentCultureIgnoreCase)).Count() <= 0)
                                                continue;
                                            object obj = cRowValue[sArray[i]];
                                            if (Convert.IsDBNull(obj) || obj == null || string.IsNullOrEmpty(obj.ToString().Trim()))
                                            {
                                                notSave = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (notSave)
                                        continue;
                                    #endregion
                                    //新增
                                    DataRow dr = dsChild.Tables[cTable.TableName].NewRow();
                                    string childPKValue = "";
                                    if (cTable.PKValueType == CPFormEnum.PKValueTypeEnum.GUID)
                                        childPKValue = Guid.NewGuid().ToString();
                                    else if (cTable.PKValueType == CPFormEnum.PKValueTypeEnum.IntNotSelfIncreasing)
                                    {
                                        sqlTmp = "select  ISNULL(MAX([" + cTable.PKFieldName + "]),0) + 1 from [" + cTable.TableName + "]";
                                        SqlCommand cmdTmp0 = new SqlCommand(sqlTmp, conn);
                                        cmdTmp0.Transaction = trans;
                                        object max0 = cmdTmp0.ExecuteScalar();
                                        if (DBNull.Value != max0 && null != max0)
                                        {
                                            childPKValue = max0.ToString();
                                        }
                                    }
                                    cFieldCol.ForEach(t =>
                                    {
                                        if (t.FieldName.Equals(cTable.PKFieldName, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            if (form.PKValueType == CPFormEnum.PKValueTypeEnum.GUID || form.PKValueType == CPFormEnum.PKValueTypeEnum.IntNotSelfIncreasing)
                                            {
                                                dr[t.FieldName] = childPKValue;
                                            }
                                            else
                                                return;
                                        }

                                        if (t.FieldName.Equals(cTable.RelateFieldName))
                                        {
                                            dr[t.FieldName] = tmppkValue;
                                        }
                                        else
                                        {
                                            // dr[t.FieldName] = cRowValue[t.FieldName];
                                            if (dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Int16")
                                                         ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Int32")
                                                         ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Int64")
                                                         ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Double")
                                                          ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Single")
                                                         )
                                            {
                                                string sValue = string.Empty;
                                                sValue = cRowValue[t.FieldName].ToString();                                              
                                                if (string.IsNullOrEmpty(sValue))
                                                {
                                                    dr[t.FieldName] = DBNull.Value;
                                                }
                                                else
                                                {
                                                    dr[t.FieldName] = sValue;
                                                }
                                            }
                                            else if (dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Boolean"))
                                            {
                                                string sValue = string.Empty;
                                                sValue = cRowValue[t.FieldName].ToString();
                                                if (string.IsNullOrEmpty(sValue))
                                                {
                                                    dr[t.FieldName] = DBNull.Value;
                                                }
                                                else
                                                {
                                                    dr[t.FieldName] = sValue;
                                                }
                                            }
                                            else if ((dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.DateTime")))
                                            {
                                                string sValue = string.Empty;
                                                if (Convert.IsDBNull(cRowValue[t.FieldName]) == false && cRowValue[t.FieldName] != null)
                                                {
                                                    sValue = cRowValue[t.FieldName].ToString();
                                                }                                                
                                                if (string.IsNullOrEmpty(sValue))
                                                {
                                                    dr[t.FieldName] = DBNull.Value;
                                                }
                                                else
                                                {
                                                    dr[t.FieldName] = sValue;
                                                }
                                            }
                                            else
                                            {
                                                dr[t.FieldName] = cRowValue[t.FieldName];
                                            }
                                        }
                                    });
                                    //dsChild.Tables[cTable.TableName].Rows.Add(dr);
                                    //daChild.InsertCommand = GetInsertCommand(dr, conn);
                                    //daChild.InsertCommand.Transaction = trans;

                                    daChild.InsertCommand = this.GetInsertCommand(dr, conn);
                                    daChild.InsertCommand.Transaction = trans;
                                    //
                                    daChild.InsertCommand.ExecuteNonQuery();
                                    dsChild.Tables[cTable.TableName].Rows.Add(dr);

                                }
                              //  daChild.Update(dsChild.Tables[cTable.TableName]);
                            });
                            #endregion
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            conn.Close();
                            errorMsg = ex.Message.ToString();
                            return false;
                        }
                    }
                    conn.Close();

                    #endregion
                }
                else
                {
                    #region 修改                   
                    SqlConnection conn = _helper.GetConnection() as SqlConnection;
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {

                            #region 主表


                            SqlCommand cmd = new SqlCommand(tableSql[form.MainTableName], conn);
                            cmd.Transaction = trans;
                            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
                            //SqlCommandBuilder builder = new SqlCommandBuilder(da);
                            //AddWithKey: 自动填充数据表结构,如：主键和限制
                            //预设值Add,不填充结构
                            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            ds.Tables[0].TableName = form.MainTableName;
                            JObject mainTableObject = JsonConvert.DeserializeObject<JObject>(Convert.ToString(DynamicObject[form.MainTableName]));
                            List<CPFormField> mFieldCol = fieldCol.Where(c => c.TableName.Equals(form.MainTableName)).ToList();
                            mFieldCol.ForEach(t =>
                            {
                                if (t.FieldName.Equals(form.PKFieldName, StringComparison.CurrentCultureIgnoreCase))
                                    return;
                                // JObject jo = (JObject)mainTableObject[t.FieldName]; 
                                if (ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Int16")
                                    ||
                                    ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Int32")
                                    ||
                                    ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Int64")
                                    ||
                                    ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Double")
                                     ||
                                    ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Single")
                                    )
                                {
                                    string sValue = string.Empty;
                                    sValue = mainTableObject[t.FieldName].ToString();
                                  
                                    if (string.IsNullOrEmpty(sValue))
                                    {
                                        ds.Tables[form.MainTableName].Rows[0][t.FieldName] = DBNull.Value;
                                    }
                                    else
                                    {
                                        ds.Tables[form.MainTableName].Rows[0][t.FieldName] = sValue;
                                    }
                                }
                                else if (ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.Boolean"))
                                {
                                    string sValue = string.Empty;
                                    sValue = mainTableObject[t.FieldName].ToString();                                    
                                    if (string.IsNullOrEmpty(sValue))
                                    {
                                        ds.Tables[form.MainTableName].Rows[0][t.FieldName] = DBNull.Value;
                                    }
                                    else
                                    {
                                        ds.Tables[form.MainTableName].Rows[0][t.FieldName] = sValue;
                                    }
                                }
                                else if (ds.Tables[form.MainTableName].Columns[t.FieldName].DataType == Type.GetType("System.DateTime"))
                                {
                                    string sValue = string.Empty;
                                    if (Convert.IsDBNull(mainTableObject[t.FieldName]) == false && mainTableObject[t.FieldName] != null)
                                    {
                                        sValue = mainTableObject[t.FieldName].ToString();
                                    } 
                                    if (string.IsNullOrEmpty(sValue))
                                    {
                                        ds.Tables[form.MainTableName].Rows[0][t.FieldName] = DBNull.Value;
                                    }
                                    else
                                    {
                                        ds.Tables[form.MainTableName].Rows[0][t.FieldName] = sValue;
                                    }
                                }
                                else
                                {
                                    ds.Tables[form.MainTableName].Rows[0][t.FieldName] = mainTableObject[t.FieldName];
                                }
                            });
                            da.UpdateCommand = this.GetUpdateCommand(ds.Tables[form.MainTableName].Rows[0], conn);
                            da.UpdateCommand.Transaction = trans;
                            da.Update(ds.Tables[form.MainTableName]);
                            #endregion

                            #region 子表
                            string tmppkValue = pkValue;
                            childTableCol.ForEach(cTable =>
                            {
                                SqlCommand cmdChild = new SqlCommand(tableSql[cTable.TableName], conn);
                                cmdChild.Transaction = trans;
                                SqlDataAdapter daChild = new System.Data.SqlClient.SqlDataAdapter(cmdChild);
                                //  SqlCommandBuilder builderChild = new SqlCommandBuilder(daChild);
                                //AddWithKey: 自动填充数据表结构,如：主键和限制
                                //预设值Add,不填充结构
                                daChild.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
                                DataSet dsChild = new DataSet();
                                daChild.Fill(dsChild);
                                dsChild.Tables[0].TableName = cTable.TableName;
                                var cTableObject = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(DynamicObject[cTable.TableName]));
                                List<CPFormField> cFieldCol = fieldCol.Where(c => c.TableName.Equals(cTable.TableName) && c.IsChildTable == false).ToList();
                                //先看看有没有要删除的
                                foreach (DataRow dr in dsChild.Tables[cTable.TableName].Rows)
                                {
                                    bool bExists = false;
                                    foreach (var cRowValue in cTableObject)
                                    {
                                        if (bExists)
                                            break;
                                        cFieldCol.ForEach(t =>
                                        {
                                            if (t.FieldName.Equals(cTable.PKFieldName, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                if (dr[cTable.PKFieldName].ToString().Equals(Convert.ToString(cRowValue[t.FieldName])))
                                                {
                                                    bExists = true;
                                                    return;
                                                }
                                            }
                                        });

                                    }
                                    if (bExists == false)
                                    {
                                        daChild.DeleteCommand = this.GetDeleteCommand(dr, conn);
                                        daChild.DeleteCommand.Transaction = trans;
                                        daChild.DeleteCommand.ExecuteNonQuery();
                                        dr.Delete();
                                    }
                                }
                                foreach (var cRowValue in cTableObject)
                                {
                                    //找出修改和新增 的
                                    if (cRowValue[cTable.PKFieldName] == null || Convert.ToString(cRowValue[cTable.PKFieldName]) == "")
                                    {
                                        //新增
                                        #region 先检查下必须 的几个字 段有没有值，没有则不保存这条数据
                                        bool notSave = false;
                                        if (string.IsNullOrEmpty(cTable.FieldNullNotSaveData) == false)
                                        {
                                            string[] sArray = cTable.FieldNullNotSaveData.Split(',');
                                            for (int i = 0; i < sArray.Length; i++)
                                            {
                                                if (fieldCol.Where(c => c.TableName.Equals(cTable.TableName, StringComparison.CurrentCultureIgnoreCase)
                                                  && c.FieldName.Equals(sArray[i], StringComparison.CurrentCultureIgnoreCase)).Count() <= 0)
                                                    continue;
                                                object obj = cRowValue[sArray[i]];
                                                if (Convert.IsDBNull(obj) || obj == null || string.IsNullOrEmpty(obj.ToString().Trim()))
                                                {
                                                    notSave = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (notSave)
                                            continue;
                                        #endregion
                                        DataRow dr = dsChild.Tables[cTable.TableName].NewRow();
                                        string childPKValue = "";
                                        if (cTable.PKValueType == CPFormEnum.PKValueTypeEnum.GUID)
                                            childPKValue = Guid.NewGuid().ToString();
                                        else if (cTable.PKValueType == CPFormEnum.PKValueTypeEnum.IntNotSelfIncreasing)
                                        {
                                            string sqlTmp = "select  ISNULL(MAX([" + cTable.PKFieldName + "]),0) + 1 from [" + cTable.TableName + "]";
                                            SqlCommand cmdTmp0 = new SqlCommand(sqlTmp, conn);
                                            cmdTmp0.Transaction = trans;
                                            object max0 = cmdTmp0.ExecuteScalar();
                                            if (DBNull.Value != max0 && null != max0)
                                            {
                                                childPKValue = max0.ToString();
                                            }
                                        }
                                      
                                        cFieldCol.ForEach(t =>
                                        {
                                            if (t.FieldName.Equals(cTable.PKFieldName, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                if (form.PKValueType == CPFormEnum.PKValueTypeEnum.GUID || form.PKValueType == CPFormEnum.PKValueTypeEnum.IntNotSelfIncreasing)
                                                {
                                                    dr[t.FieldName] = childPKValue;
                                                }
                                                else
                                                    return;
                                            }
                                          
                                            if (t.FieldName.Equals(cTable.RelateFieldName))
                                            {
                                                dr[t.FieldName] = tmppkValue;
                                            }
                                            else
                                            {
                                                //  dr[t.FieldName] = cRowValue[t.FieldName];
                                                if (dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Int16")
                                                         ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Int32")
                                                         ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Int64")
                                                         ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Double")
                                                          ||
                                                         dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Single")
                                                         )
                                                {
                                                    string sValue = string.Empty;
                                                    sValue = cRowValue[t.FieldName].ToString();
                                                    
                                                    if (string.IsNullOrEmpty(sValue))
                                                    {
                                                        dr[t.FieldName] =  DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        dr[t.FieldName] = sValue;
                                                    }
                                                }
                                                else if (dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.Boolean"))
                                                {
                                                    string sValue = string.Empty;
                                                    sValue = cRowValue[t.FieldName].ToString();
                                                    if (string.IsNullOrEmpty(sValue))
                                                    {
                                                        dr[t.FieldName] = DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        dr[t.FieldName] = sValue;
                                                    }
                                                }
                                                else if ((dr.Table.Columns[t.FieldName].DataType == Type.GetType("System.DateTime")))
                                                {
                                                    string sValue = string.Empty;
                                                    if (Convert.IsDBNull(cRowValue[t.FieldName]) == false && cRowValue[t.FieldName] != null)
                                                    {
                                                        sValue = cRowValue[t.FieldName].ToString();
                                                    }
                                                    if (string.IsNullOrEmpty(sValue))
                                                    {
                                                        dr[t.FieldName] = DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        dr[t.FieldName] = sValue;
                                                    }
                                                }
                                                else
                                                {
                                                    dr[t.FieldName] = cRowValue[t.FieldName];
                                                }
                                            }
                                        });
                                        daChild.InsertCommand = this.GetInsertCommand(dr, conn);
                                        daChild.InsertCommand.Transaction = trans;
                                        //
                                        daChild.InsertCommand.ExecuteNonQuery();
                                        dsChild.Tables[cTable.TableName].Rows.Add(dr);
                                    }
                                    else
                                    {
                                        DataRow[] DRS = dsChild.Tables[cTable.TableName].Select(cTable.PKFieldName + "='" + cRowValue[cTable.PKFieldName] + "'");
                                        cFieldCol.ForEach(t =>
                                        {
                                            if (t.FieldName.Equals(cTable.PKFieldName, StringComparison.CurrentCultureIgnoreCase))
                                                return;
                                            //  DRS[0][t.FieldName] = cRowValue[t.FieldName];
                                            if (DRS[0].Table.Columns[t.FieldName].DataType == Type.GetType("System.Int16")
                                                          ||
                                                          DRS[0].Table.Columns[t.FieldName].DataType == Type.GetType("System.Int32")
                                                          ||
                                                          DRS[0].Table.Columns[t.FieldName].DataType == Type.GetType("System.Int64")
                                                          ||
                                                          DRS[0].Table.Columns[t.FieldName].DataType == Type.GetType("System.Double")
                                                           ||
                                                          DRS[0].Table.Columns[t.FieldName].DataType == Type.GetType("System.Single")
                                                          )
                                            {
                                                string sValue = string.Empty;
                                                sValue = cRowValue[t.FieldName].ToString();
                                               
                                                if (string.IsNullOrEmpty(sValue))
                                                {
                                                    DRS[0][t.FieldName] = DBNull.Value;
                                                }
                                                else
                                                {
                                                    DRS[0][t.FieldName] = sValue;
                                                }
                                            }
                                            else if (DRS[0].Table.Columns[t.FieldName].DataType == Type.GetType("System.Boolean"))
                                            {
                                                string sValue = string.Empty;
                                                sValue = cRowValue[t.FieldName].ToString();
                                                
                                                if (string.IsNullOrEmpty(sValue))
                                                {
                                                    DRS[0][t.FieldName] = DBNull.Value;
                                                }
                                                else
                                                {
                                                    DRS[0][t.FieldName] = sValue;
                                                }
                                            }
                                            else
                                            {
                                                DRS[0][t.FieldName] = cRowValue[t.FieldName];
                                            }
                                        });
                                        daChild.UpdateCommand = this.GetUpdateCommand(DRS[0], conn);
                                        daChild.UpdateCommand.Transaction = trans;
                                        daChild.UpdateCommand.ExecuteNonQuery();
                                    }
                                }
                              //  daChild.Update(dsChild.Tables[cTable.TableName]);
                            });
                            #endregion
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            conn.Close();
                            errorMsg = ex.Message.ToString();
                            return false;
                        }
                    }
                    conn.Close();


                    #endregion
                }
            }
            else
            {
                throw new Exception("未实现");
            }
            return b;
        }
    }
    internal class CPFormChildTableRep : BaseRepository<CPFormChildTable>
    {
        public CPFormChildTableRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }

    internal class CPFormFieldRep : BaseRepository<CPFormField>
    {
        public CPFormFieldRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPFormViewRep : BaseRepository<CPFormView>
    {
        public CPFormViewRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }

    internal class CPFormViewFieldRep : BaseRepository<CPFormViewField>
    {
        public CPFormViewFieldRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPFormUseSceneRep : BaseRepository<CPFormUseScene>
    {
        public CPFormUseSceneRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPFormUseSceneFuncRep : BaseRepository<CPFormUseSceneFunc>
    {
        public CPFormUseSceneFuncRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPFormGroupRep : BaseRepository<CPFormGroup>
    {
        public CPFormGroupRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPFormFieldRightRep : BaseRepository<CPFormFieldRight>
    {
        public CPFormFieldRightRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPFormFieldInitRep : BaseRepository<CPFormFieldInit>
    {
        public CPFormFieldInitRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPFormFieldRuleRep : BaseRepository<CPFormFieldRule>
    {
        public CPFormFieldRuleRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    #endregion


    #region 列表部分
    public abstract class BaseCPGridRep : BaseRepository<CPGrid>
    {
        public BaseCPGridRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public abstract DataTable ReadData(CPGrid gridObj, int currentPage,
            int pageSize, string otherCondition, string orderBy, out int recordSize);
        public abstract string StatisticsColumnSum(CPGrid gridObj, CPGridColumn curColumn, string otherCondition);
        public abstract DataSet GetConfig(List<int> gridIdCol);
        public abstract bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew);
    }
    internal class CPGridRep : BaseCPGridRep
    {
        public CPGridRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public override bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew)
        {
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());

            bool b = true;
            #region 先删除数据           
            if (isCreateNew==false)
            {
                string delGridCodes = "";
                foreach (DataRow drMain in ds.Tables["Grid_Main"].Rows)
                { 
                        if (string.IsNullOrEmpty(delGridCodes)) delGridCodes = drMain["GridCode"].ToString();
                        else delGridCodes += "," + drMain["GridCode"].ToString();
                }
                if (string.IsNullOrEmpty(delGridCodes) == false)
                {
                   string  delSql = @"DELETE FROM Grid_Func WHERE GridId IN (SELECT GridId FROM Grid_Main WHERE GridCode IN ('" + delGridCodes.Replace(",", "','") + @"'))
                        ;DELETE FROM Grid_Column WHERE GridId IN (SELECT GridId FROM Grid_Main WHERE GridCode IN ('" + delGridCodes.Replace(",", "','") + @"'))
                        ;DELETE FROM Grid_Main WHERE     GridCode IN ('" + delGridCodes.Replace(",", "','") + @"')";
                    _helper.ExecuteNonQuery(delSql);
                    if (!b)
                        throw new Exception("先删除已经存在的配置时出错");
                }
            }
            #endregion

            #region 写入数据
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Grid_Main WHERE 1=2
                    ;SELECT * FROM Grid_Column WHERE 1=2
                    ;SELECT * FROM Grid_Func WHERE 1=2", 
                _helper.GetConnection() as SqlConnection);
            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
            // SqlCommandBuilder builder = new SqlCommandBuilder(da);
            //AddWithKey: 自动填充数据表结构,如：主键和限制
            //预设值Add,不填充结构
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
            DataSet dsStruct = new DataSet();
            da.Fill(dsStruct);
            dsStruct.Tables[0].TableName = "Grid_Main";
            dsStruct.Tables[1].TableName = "Grid_Column";
            dsStruct.Tables[2].TableName = "Grid_Func";
            #region Grid_Main
            Dictionary<int, int> oldNewGridId = new Dictionary<int, int>();
            foreach (DataRow dr in ds.Tables["Grid_Main"].Rows)
            {
                dr["SysId"] = targetSysId;
                if (isCreateNew)
                {
                    dr["GridTitle"] = dr["GridTitle"].ToString() + "_副本";
                    int autoIndex;
                    dr["GridCode"] = CPAutoNumHelper.Instance().GetNextAutoNum("GridCodeAuto", out autoIndex);
                    dr["AutoIndex"] = autoIndex;
                }
                string insertSql = CPAppContext.GetInsertSql("Grid_Main", dsStruct.Tables["Grid_Main"].Columns, dr);
                insertSql += ";select SCOPE_IDENTITY() as Id;";
                SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                foreach (DataColumn dc in dsStruct.Tables["Grid_Main"].Columns)
                {
                    if (dc.AutoIncrement)
                    {
                        continue;
                    }
                    if (dr.Table.Columns.Contains(dc.ColumnName))
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                    }
                    else
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                    }
                }
                int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                oldNewGridId.Add(int.Parse(dr["GridId"].ToString()), newId);
            }

            #endregion

            #region Grid_Column
            //循环列
            if (ds.Tables["Grid_Column"] != null)
            {
                foreach (DataRow dr in ds.Tables["Grid_Column"].Rows)
                {
                    dr["GridId"] = oldNewGridId[int.Parse(dr["GridId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Grid_Column", dsStruct.Tables["Grid_Column"].Columns, dr);
                   
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Grid_Column"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert);
                }
            }
            #endregion

            #region Grid_Func
            if (ds.Tables["Grid_Func"] != null)
            {
                foreach (DataRow dr in ds.Tables["Grid_Func"].Rows)
                {
                    dr["GridId"] = oldNewGridId[int.Parse(dr["GridId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Grid_Func", dsStruct.Tables["Grid_Func"].Columns, dr);

                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Grid_Func"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert);
                }
            }
            #endregion

            #endregion
            return b;
        }
        public override DataSet GetConfig(List<int> gridIdCol)
        {
            string ids = "";
            gridIdCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids))
                    ids = t.ToString();
                else
                    ids += "," + t.ToString();
            });
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
            string strSql = "SELECT * FROM Grid_Main WHERE GridId in(" + ids + ");";
            strSql += "SELECT * FROM Grid_Column WHERE GridId in(" + ids + ");";
            strSql += "SELECT * FROM Grid_Func WHERE GridId in(" + ids + ")";
            DataSet ds = _helper.ExecuteDataSet(strSql);
            ds.Tables[0].TableName = "Grid_Main";
            ds.Tables[1].TableName = "Grid_Column";
            ds.Tables[2].TableName = "Grid_Func";
            return ds;
        }

        #region 统计列合计值
        public override string  StatisticsColumnSum(CPGrid gridObj,CPGridColumn curColumn, string otherCondition)
        {
            if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.SqlServer)
            {
                DbHelper _helper = new DbHelper(gridObj.DbIns, DbHelper.DbTypeEnum.SqlServer);
                string strSql = gridObj.DataSource;
                strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                if (strSql.IndexOf("{@Condition@}", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    if (string.IsNullOrEmpty(otherCondition) == false)
                    {
                        strSql = strSql.Replace("{@Condition@}", otherCondition);
                    }
                    else
                        strSql = strSql.Replace("{@Condition@}", "1=1");
                }
                else if (strSql.IndexOf("{@ConditionEx@}", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    //注意，此关键字会把'换成''，以便于给存储过程传值
                    if (string.IsNullOrEmpty(otherCondition) == false)
                    {
                        strSql = strSql.Replace("{@ConditionEx@}", otherCondition.Replace("'", "''"));
                    }
                    else
                        strSql = strSql.Replace("{@ConditionEx@}", "1=1");
                }
                else
                {
                    if (gridObj.DataSourceType == CPGridEnum.DataSourceTypeEnum.Sql)
                    {
                        if (string.IsNullOrEmpty(otherCondition) == false)
                        {
                            strSql = "SELECT * FROM (" + strSql + " ) as CPGridTableTable";
                            strSql += " WHERE " + otherCondition;
                        }
                    }
                } 
                try
                {
                    #region 调用二次开发方法 
                    if (string.IsNullOrEmpty(gridObj.BeforeGridLoad) == false)
                    {

                        string[] sArray = gridObj.BeforeGridLoad.Split(';');
                        for (int i = 0; i < sArray.Length; i++)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(sArray[i]))
                                    continue;
                                CPBeforeReadDataFromDbEventArgs e = new CPBeforeReadDataFromDbEventArgs(gridObj, strSql);
                                CPGridInterface inter = Activator.CreateInstance(Type.GetType(sArray[i])) as CPGridInterface;
                                bool b = inter.BeforeReadDataFromDb(e);
                                if (b)
                                {
                                    strSql = e.StrSql;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("调用二次开发方法【" + sArray[i] + "】时出错，错误信息如下 ：" + ex.Message);
                            }
                        }

                    }

                    #endregion 
                    strSql = "SELECT " + curColumn.SumType.Trim() + "(" + curColumn.FieldName + ") FROM (" + strSql + " ) as CPGridTableTableSumTable";
                    object obj = _helper.ExecuteScalar(strSql);
                    if (Convert.IsDBNull(obj) || obj == null)
                        return "0";
                    else
                        return obj.ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception("读取列表统计值数据时出错，执行Sql语句如下：" + strSql + "；详细信息如下：" + ex.Message.ToString());
                }
            }
            else if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.Oracle)
            {
                throw new Exception("未实现");
            }
            return "0";
        }
        #endregion
        #region 读取数据
        public override DataTable ReadData(CPGrid gridObj, int currentPage,
            int pageSize, string otherCondition, string orderBy, out int recordSize)
        {
            recordSize = 0;
            if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.SqlServer)
            {
                DbHelper _helper = new DbHelper(gridObj.DbIns, DbHelper.DbTypeEnum.SqlServer);
                string strSql = gridObj.DataSource;
                strSql = CPExpressionHelper.Instance.RunCompile(strSql);
                if (strSql.IndexOf("{@Condition@}", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    if (string.IsNullOrEmpty(otherCondition) == false)
                    {
                        strSql = strSql.Replace("{@Condition@}", otherCondition);
                    }
                    else
                        strSql = strSql.Replace("{@Condition@}", "1=1");
                }
                else if (strSql.IndexOf("{@ConditionEx@}", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    //注意，此关键字会把'换成''，以便于给存储过程传值
                    if (string.IsNullOrEmpty(otherCondition) == false)
                    {
                        strSql = strSql.Replace("{@ConditionEx@}", otherCondition.Replace("'", "''"));
                    }
                    else
                        strSql = strSql.Replace("{@ConditionEx@}", "1=1");
                }
                else
                {
                    if (gridObj.DataSourceType ==  CPGridEnum.DataSourceTypeEnum.Sql)
                    {
                        if (string.IsNullOrEmpty(otherCondition) == false)
                        {
                            strSql = "SELECT * FROM (" + strSql + " ) as CPGridTableTable";
                            strSql += " WHERE " + otherCondition;
                        }
                    }
                }
                if (gridObj.DataSourceType == CPGridEnum.DataSourceTypeEnum.Sql)
                {
                    if (gridObj.IsGroup.Value)
                    {
                        if (string.IsNullOrEmpty(gridObj.GroupField) == false)
                        {
                            strSql += " ORDER BY " + gridObj.GroupField + " " + gridObj.GroupSort.ToString();
                            if (string.IsNullOrEmpty(orderBy) == false)
                            {
                                strSql += " , " + orderBy;
                            }
                        }

                        else
                        {
                            if (string.IsNullOrEmpty(orderBy) == false)
                            {
                                strSql += " ORDER BY " + orderBy;
                            }
                        }
                       
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(orderBy) == false)
                        {
                            strSql += " ORDER BY " + orderBy;
                        }
                    }
                }
                try
                {
                    #region 调用二次开发方法 
                    if (string.IsNullOrEmpty(gridObj.BeforeGridLoad) == false)
                    {

                        string[] sArray = gridObj.BeforeGridLoad.Split(';');
                        for (int i = 0; i < sArray.Length; i++)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(sArray[i]))
                                    continue;
                                CPBeforeReadDataFromDbEventArgs e = new CPBeforeReadDataFromDbEventArgs(gridObj, strSql);
                                CPGridInterface inter = Activator.CreateInstance(Type.GetType(sArray[i])) as CPGridInterface;
                                bool b = inter.BeforeReadDataFromDb(e);
                                if (b)
                                {
                                    strSql = e.StrSql;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("调用二次开发方法【" + sArray[i] + "】时出错，错误信息如下 ：" + ex.Message);
                            }
                        }

                    }
                    #endregion
                    SqlParameter[] para = new System.Data.SqlClient.SqlParameter[3];
                    para[0] = new SqlParameter("@sqlstr",strSql);
                    para[1] = new SqlParameter("@currentpage", currentPage );
                    para[2] = new SqlParameter("@pagesize", pageSize);
                    DataSet ds = _helper.ExecuteDataSet("CP_ReadDataPager",CommandType.StoredProcedure, para );
                    recordSize = Convert.ToInt32(ds.Tables[1].Rows[0][0].ToString());
                    _helper = null;
                    DataTable dtReturn = ds.Tables[2];
                    #region 调用二次开发方法
                    if (string.IsNullOrEmpty(gridObj.BeforeGridLoad) == false)
                    {

                        string[] sArray = gridObj.BeforeGridLoad.Split(';');
                        for (int i = 0; i < sArray.Length; i++)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(sArray[i]))
                                    continue;
                                CPAfterReadDataFromDbEventArgs e = new CPAfterReadDataFromDbEventArgs(gridObj, dtReturn);
                                CPGridInterface inter = Activator.CreateInstance(Type.GetType(sArray[i])) as CPGridInterface;
                                bool b = inter.AfterReadDataFromDb(e);
                                if (b)
                                {
                                    dtReturn = e.RealData;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("调用二次开发方法【" + sArray[i] + "】时出错，错误信息如下 ：" + ex.Message);
                            }
                        }

                    }
                    #endregion
                    return dtReturn;
                }
                catch (Exception ex)
                {
                    throw new Exception("读取列表实际数据时出错，执行Sql语句如下：" + strSql + "；详细信息如下：" + ex.Message.ToString());
                }
            }
            else if(CPAppContext.CurDbType() == DbHelper.DbTypeEnum.Oracle)
            {
                throw new Exception("未实现");
            }

            return null;
        }
        #endregion
    }
    internal class CPGridColumnRep : BaseRepository<CPGridColumn>
    {
        public CPGridColumnRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPGridFuncRep : BaseRepository<CPGridFunc>
    {
        public CPGridFuncRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    #endregion


    #region 标签 部分
     public abstract  class BaseCPTabRep : BaseRepository<CPTab>
    {
        public BaseCPTabRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public abstract DataSet GetConfig(List<int> tabIdCol);
        public abstract bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew);
    }


    public class CPTabRep : BaseCPTabRep
    {
        public CPTabRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public override bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew)
        {
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());

            bool b = true;
            #region 先删除数据           
            if (isCreateNew == false)
            {
                string delTabCodes = "";
                foreach (DataRow drMain in ds.Tables["Tab_Main"].Rows)
                {
                    if (string.IsNullOrEmpty(delTabCodes)) delTabCodes = drMain["TabCode"].ToString();
                    else delTabCodes += "," + drMain["TabCode"].ToString();
                }
                if (string.IsNullOrEmpty(delTabCodes) == false)
                {
                    string delSql = @"DELETE FROM Tab_Element WHERE TabId IN (SELECT TabId FROM Tab_Main WHERE TabCode IN ('" + delTabCodes.Replace(",", "','") + @"'))
                        ;DELETE FROM Tab_Main WHERE     TabCode IN ('" + delTabCodes.Replace(",", "','") + @"')";
                    _helper.ExecuteNonQuery(delSql);
                    if (!b)
                        throw new Exception("先删除已经存在的配置时出错");
                }
            }
            #endregion

            #region 写入数据
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Tab_Main WHERE 1=2
                    ;SELECT * FROM Tab_Element WHERE 1=2",
                _helper.GetConnection() as SqlConnection);
            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
            // SqlCommandBuilder builder = new SqlCommandBuilder(da);
            //AddWithKey: 自动填充数据表结构,如：主键和限制
            //预设值Add,不填充结构
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
            DataSet dsStruct = new DataSet();
            da.Fill(dsStruct);
            dsStruct.Tables[0].TableName = "Tab_Main";
            dsStruct.Tables[1].TableName = "Tab_Element";
            #region Tab_Main
            Dictionary<int, int> oldNewTabId = new Dictionary<int, int>();
            foreach (DataRow dr in ds.Tables["Tab_Main"].Rows)
            {
                dr["SysId"] = targetSysId;
                if (isCreateNew)
                {
                    dr["TabTitle"] = dr["TabTitle"].ToString() + "_副本";
                    int autoIndex;
                    dr["TabCode"] = CPAutoNumHelper.Instance().GetNextAutoNum("TabCodeAuto", out autoIndex);
                    dr["AutoIndex"] = autoIndex;
                }
                string insertSql = CPAppContext.GetInsertSql("Tab_Main", dsStruct.Tables["Tab_Main"].Columns, dr);
                insertSql += ";select SCOPE_IDENTITY() as Id;";
                SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                foreach (DataColumn dc in dsStruct.Tables["Tab_Main"].Columns)
                {
                    if (dc.AutoIncrement)
                    {
                        continue;
                    }
                    if (dr.Table.Columns.Contains(dc.ColumnName))
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                    }
                    else
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                    }
                }
                int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                oldNewTabId.Add(int.Parse(dr["TabId"].ToString()), newId);
            }

            #endregion


            #region Tab_Element
            if (ds.Tables["Tab_Element"] != null)
            {
                foreach (DataRow dr in ds.Tables["Tab_Element"].Rows)
                {
                    dr["TabId"] = oldNewTabId[int.Parse(dr["TabId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Tab_Element", dsStruct.Tables["Tab_Element"].Columns, dr);
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Tab_Element"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert);
                }
            }
            #endregion

            #endregion
            return b;
        }
        public override DataSet GetConfig(List<int> tabIdCol)
        {
            string ids = "";
            tabIdCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids))
                    ids = t.ToString();
                else
                    ids += "," + t.ToString();
            });
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
            string strSql = "SELECT * FROM Tab_Main WHERE TabId in(" + ids + ");";
            strSql += "SELECT * FROM Tab_Element WHERE TabId in(" + ids + ")"; 
            DataSet ds = _helper.ExecuteDataSet(strSql);
            ds.Tables[0].TableName = "Tab_Main";
            ds.Tables[1].TableName = "Tab_Element";
            return ds;
        }
    }
    internal class CPTabItemRep : BaseRepository<CPTabItem>
    {
        public CPTabItemRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    #endregion

    #region 树 部分
    public abstract class BaseCPTreeRep : BaseRepository<CPTree>
    {
        public BaseCPTreeRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public abstract DataSet GetConfig(List<int> treeIdCol);
        public abstract bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew);
    }
    public class CPTreeRep : BaseCPTreeRep
    {
        public CPTreeRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public override bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew)
        {
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());

            bool b = true;
            #region 先删除数据           
            if (isCreateNew == false)
            {
                string delTreeCodes = "";
                foreach (DataRow drMain in ds.Tables["Tree_Main"].Rows)
                {
                    if (string.IsNullOrEmpty(delTreeCodes)) delTreeCodes = drMain["TreeCode"].ToString();
                    else delTreeCodes += "," + drMain["TreeCode"].ToString();
                }
                if (string.IsNullOrEmpty(delTreeCodes) == false)
                {
                    string delSql = @"DELETE FROM Tree_Func WHERE TreeId IN (SELECT TreeId FROM Tree_Main WHERE TreeCode IN ('" + delTreeCodes.Replace(",", "','") + @"'))
                        ;DELETE FROM Tree_DataSource WHERE TreeId IN (SELECT TreeId FROM Tree_Main WHERE TreeCode IN ('" + delTreeCodes.Replace(",", "','") + @"'))
                        ;DELETE FROM Tree_Main WHERE     TreeCode IN ('" + delTreeCodes.Replace(",", "','") + @"')";
                    _helper.ExecuteNonQuery(delSql);
                    if (!b)
                        throw new Exception("先删除已经存在的配置时出错");
                }
            }
            #endregion

            #region 写入数据
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Tree_Main WHERE 1=2
                    ;SELECT * FROM Tree_DataSource WHERE 1=2
                    ;SELECT * FROM Tree_Func WHERE 1=2",
                _helper.GetConnection() as SqlConnection);
            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
            // SqlCommandBuilder builder = new SqlCommandBuilder(da);
            //AddWithKey: 自动填充数据表结构,如：主键和限制
            //预设值Add,不填充结构
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
            DataSet dsStruct = new DataSet();
            da.Fill(dsStruct);
            dsStruct.Tables[0].TableName = "Tree_Main";
            dsStruct.Tables[1].TableName = "Tree_DataSource";
            dsStruct.Tables[2].TableName = "Tree_Func";
            #region Tree_Main
            Dictionary<int, int> oldNewTreeId = new Dictionary<int, int>();
            foreach (DataRow dr in ds.Tables["Tree_Main"].Rows)
            {
                dr["SysId"] = targetSysId;
                if (isCreateNew)
                {
                    dr["TreeTitle"] = dr["TreeTitle"].ToString() + "_副本";
                    int autoIndex;
                    dr["TreeCode"] = CPAutoNumHelper.Instance().GetNextAutoNum("TreeCodeAuto", out autoIndex);
                    dr["AutoIndex"] = autoIndex;
                }
                string insertSql = CPAppContext.GetInsertSql("Tree_Main", dsStruct.Tables["Tree_Main"].Columns, dr);
                insertSql += ";select SCOPE_IDENTITY() as Id;";
                SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                foreach (DataColumn dc in dsStruct.Tables["Tree_Main"].Columns)
                {
                    if (dc.AutoIncrement)
                    {
                        continue;
                    }
                    if (dr.Table.Columns.Contains(dc.ColumnName))
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                    }
                    else
                    {
                        cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                    }
                }
                int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                oldNewTreeId.Add(int.Parse(dr["TreeId"].ToString()), newId);
            }

            #endregion

            #region Tree_DataSource
            //循环列
            Dictionary<int, int> oldNewSourceId = new Dictionary<int, int>();
            Dictionary<int, string> oldNewSourceIdAndParentId = new Dictionary<int, string>();
            if (ds.Tables["Tree_DataSource"] != null)
            {
                foreach (DataRow dr in ds.Tables["Tree_DataSource"].Rows)
                {
                    dr["TreeId"] = oldNewTreeId[int.Parse(dr["TreeId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Tree_DataSource", dsStruct.Tables["Tree_DataSource"].Columns, dr);
                    insertSql += ";select SCOPE_IDENTITY() as Id;";
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Tree_DataSource"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    int newId = int.Parse(_helper.ExecuteScalar(cmdInsert).ToString());
                    oldNewSourceId.Add(int.Parse(dr["SourceId"].ToString()), newId);
                    oldNewSourceIdAndParentId.Add(int.Parse(dr["SourceId"].ToString()), (newId.ToString() + "$" + dr["ParentSourceId"].ToString()));
                }
                string updateSql = "";
                oldNewSourceIdAndParentId.Keys.ToList().ForEach(t => {
                    string[] sArray = oldNewSourceIdAndParentId[t].ToString().Split('$');
                    if (int.Parse(sArray[1]).Equals(-1) == false)
                    {
                        if (string.IsNullOrEmpty(updateSql))
                        {
                            updateSql = "UPDATE Tree_DataSource SET ParentSourceId=" + oldNewSourceId[int.Parse(sArray[1])] + " WHERE SourceId=" + oldNewSourceId[t];
                        }
                        else
                        {
                            updateSql += ";UPDATE Tree_DataSource SET ParentSourceId=" + oldNewSourceId[int.Parse(sArray[1])] + " WHERE SourceId=" + oldNewSourceId[t];
                        }
                    }
                });
                if(string.IsNullOrEmpty(updateSql)==false)
                {
                    _helper.ExecuteNonQuery(updateSql);
                }
            }
            #endregion

            #region Tree_Func
            if (ds.Tables["Tree_Func"] != null)
            {
                foreach (DataRow dr in ds.Tables["Tree_Func"].Rows)
                {
                    dr["TreeId"] = oldNewTreeId[int.Parse(dr["TreeId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Tree_Func", dsStruct.Tables["Tree_Func"].Columns, dr);
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Tree_Func"].Columns)
                    {
                        if (dc.AutoIncrement)
                        {
                            continue;
                        }
                        if (dr.Table.Columns.Contains(dc.ColumnName))
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, dr[dc.ColumnName]);
                        }
                        else
                        {
                            cmdInsert.Parameters.AddWithValue("@" + dc.ColumnName, DBNull.Value);
                        }
                    }
                    _helper.ExecuteNonQuery(cmdInsert);
                }
            }
            #endregion

            #endregion
            return b;
        }
        public override DataSet GetConfig(List<int> treeIdCol)
        {
            string ids = "";
            treeIdCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids))
                    ids = t.ToString();
                else
                    ids += "," + t.ToString();
            });
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
            string strSql = "SELECT * FROM Tree_Main WHERE TreeId in(" + ids + ");";
            strSql += "SELECT * FROM Tree_DataSource WHERE TreeId in(" + ids + ");";
            strSql += "SELECT * FROM Tree_Func WHERE TreeId in(" + ids + ")";
            DataSet ds = _helper.ExecuteDataSet(strSql);
            ds.Tables[0].TableName = "Tree_Main";
            ds.Tables[1].TableName = "Tree_DataSource";
            ds.Tables[2].TableName = "Tree_Func";
            return ds;
        }
    }
    internal class CPTreeDataSourceRep : BaseRepository<CPTreeDataSource>
    {
        public CPTreeDataSourceRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    internal class CPTreeFuncRep : BaseRepository<CPTreeFunc>
    {
        public CPTreeFuncRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
    }
    #endregion
}
