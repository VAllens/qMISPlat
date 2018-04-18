using CPFrameWork.Flow.Domain;
using CPFrameWork.Flow.Infrastructure;
using CPFrameWork.Global;
using CPFrameWork.Utility.DbOper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CPFrameWork.Flow.Repository
{
    public abstract class BaseCPFlowRep : BaseRepository<CPFlow>
    {
        public BaseCPFlowRep(ICPFlowDbContext dbContext) : base(dbContext)
        {

        }
        public abstract DataSet GetConfigByFlowVerId(int flowVerId);
        public abstract bool SyncConfigFromDataSet( DataSet ds, int targetFlowClassId,bool isCreateNew);
        public abstract bool DeletePhase(List<int> phaseIdCol);
        public abstract CPFlow GetFlowMaxVer(int flowId);
        public abstract List<CPFlow> GetFlowMaxTwoVer(int flowId);
        public abstract List<CPFlow> GetAllFlowMaxVer( );
    }
    public class CPFlowRep : BaseCPFlowRep
    {
        public CPFlowRep(ICPFlowDbContext dbContext) : base(dbContext)
        {

        }
        #region 同步 配置相关
        public override bool SyncConfigFromDataSet(DataSet ds, int targetFlowClassId, bool isCreateNew)
        {
            //isCreateNew是否是全新创建流程，创建流程版本不算全新创建，包括配置更新也不算。
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());

            bool b = true;
             

            #region 写入数据
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Flow_Template WHERE 1=2; 
                                    SELECT * FROM Flow_TemplatePhase WHERE 1=2;
                                    SELECT * FROM Flow_TemplatePhaseForm WHERE 1=2;
                                    SELECT * FROM Flow_TemplatePhaseLink WHERE 1=2;
                                    SELECT * FROM Flow_TemplatePhaseRule WHERE 1=2;
                                    SELECT * FROM Flow_TemplatePhaseRuleHandle WHERE 1=2",
                _helper.GetConnection() as SqlConnection);
            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
            // SqlCommandBuilder builder = new SqlCommandBuilder(da);
            //AddWithKey: 自动填充数据表结构,如：主键和限制
            //预设值Add,不填充结构
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
            DataSet dsStruct = new DataSet();
            da.Fill(dsStruct);
            dsStruct.Tables[0].TableName = "Flow_Template";
            dsStruct.Tables[1].TableName = "Flow_TemplatePhase";
            dsStruct.Tables[2].TableName = "Flow_TemplatePhaseForm";
            dsStruct.Tables[3].TableName = "Flow_TemplatePhaseLink";
            dsStruct.Tables[4].TableName = "Flow_TemplatePhaseRule";
            dsStruct.Tables[5].TableName = "Flow_TemplatePhaseRuleHandle";
            #region Flow_Template
            
            Dictionary<int, int> oldNewFlowVerId = new Dictionary<int, int>();
            foreach (DataRow dr in ds.Tables["Flow_Template"].Rows)
            { 
                if (isCreateNew)
                {
                    dr["FlowName"] = dr["FlowName"].ToString() + "_副本";
                    string tmpSql = "SELECT ISNULL(MAX(flowid),1) FROM Flow_Template";
                    int newFlowId = int.Parse(_helper.ExecuteScalar(tmpSql).ToString());
                    dr["FlowId"] = newFlowId;
                }
                dr["FlowClassId"] = targetFlowClassId;
                string insertSql = CPAppContext.GetInsertSql("Flow_Template", dsStruct.Tables["Flow_Template"].Columns, dr);
                insertSql += ";select SCOPE_IDENTITY() as Id;";
                SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                foreach (DataColumn dc in dsStruct.Tables["Flow_Template"].Columns)
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
                oldNewFlowVerId.Add(int.Parse(dr["FlowVerId"].ToString()), newId);
            }

            #endregion

            #region Flow_TemplatePhase
            Dictionary<int, int> oldNewPhaseId = new Dictionary<int, int>();
            if (ds.Tables["Flow_TemplatePhase"] != null)
            {
                foreach (DataRow dr in ds.Tables["Flow_TemplatePhase"].Rows)
                {
                    dr["FlowVerId"] = oldNewFlowVerId[int.Parse(dr["FlowVerId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Flow_TemplatePhase", dsStruct.Tables["Flow_TemplatePhase"].Columns, dr);
                    insertSql += ";select SCOPE_IDENTITY() as Id;";
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Flow_TemplatePhase"].Columns)
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
                    oldNewPhaseId.Add(int.Parse(dr["PhaseId"].ToString()), newId);
                }
            }
            #endregion

            #region Flow_TemplatePhaseForm 
            if (ds.Tables["Flow_TemplatePhaseForm"] != null)
            {
                foreach (DataRow dr in ds.Tables["Flow_TemplatePhaseForm"].Rows)
                {
                    dr["FlowVerId"] = oldNewFlowVerId[int.Parse(dr["FlowVerId"].ToString())];
                    dr["PhaseId"] = oldNewPhaseId[int.Parse(dr["PhaseId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Flow_TemplatePhaseForm", dsStruct.Tables["Flow_TemplatePhaseForm"].Columns, dr);
                  
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Flow_TemplatePhaseForm"].Columns)
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

            #region Flow_TemplatePhaseLink 
            if (ds.Tables["Flow_TemplatePhaseLink"] != null)
            {
                foreach (DataRow dr in ds.Tables["Flow_TemplatePhaseLink"].Rows)
                {
                    dr["FlowVerId"] = oldNewFlowVerId[int.Parse(dr["FlowVerId"].ToString())];
                    dr["StartPhaseId"] = oldNewPhaseId[int.Parse(dr["StartPhaseId"].ToString())];
                    dr["EndPhaseId"] = oldNewPhaseId[int.Parse(dr["EndPhaseId"].ToString())];

                    string insertSql = CPAppContext.GetInsertSql("Flow_TemplatePhaseLink", dsStruct.Tables["Flow_TemplatePhaseLink"].Columns, dr);
                
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Flow_TemplatePhaseLink"].Columns)
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

            #region Flow_TemplatePhaseRule 
            Dictionary<int, int> oldNewRuleId = new Dictionary<int, int>();
            if (ds.Tables["Flow_TemplatePhaseRule"] != null)
            {
                foreach (DataRow dr in ds.Tables["Flow_TemplatePhaseRule"].Rows)
                {
                    dr["FlowVerId"] = oldNewFlowVerId[int.Parse(dr["FlowVerId"].ToString())];
                    dr["PhaseId"] = oldNewPhaseId[int.Parse(dr["PhaseId"].ToString())];
                    string insertSql = CPAppContext.GetInsertSql("Flow_TemplatePhaseRule", dsStruct.Tables["Flow_TemplatePhaseRule"].Columns, dr);
                    insertSql += ";select SCOPE_IDENTITY() as Id;";
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Flow_TemplatePhaseRule"].Columns)
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
                    oldNewRuleId.Add(int.Parse(dr["RuleId"].ToString()), newId);
                }
            }
            #endregion

            #region Flow_TemplatePhaseRuleHandle 
            if (ds.Tables["Flow_TemplatePhaseRuleHandle"] != null)
            {
                foreach (DataRow dr in ds.Tables["Flow_TemplatePhaseRuleHandle"].Rows)
                {
                    dr["FlowVerId"] = oldNewFlowVerId[int.Parse(dr["FlowVerId"].ToString())];
                    dr["PhaseId"] = oldNewPhaseId[int.Parse(dr["PhaseId"].ToString())];
                    dr["RuleId"] = oldNewRuleId[int.Parse(dr["RuleId"].ToString())];

                    string insertSql = CPAppContext.GetInsertSql("Flow_TemplatePhaseRuleHandle", dsStruct.Tables["Flow_TemplatePhaseRuleHandle"].Columns, dr);
          
                    SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                    foreach (DataColumn dc in dsStruct.Tables["Flow_TemplatePhaseRuleHandle"].Columns)
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
        public override DataSet GetConfigByFlowVerId(int flowVerId)
        {
            string ids = flowVerId.ToString();
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
            string strSql = "SELECT * FROM Flow_Template WHERE FlowVerId in(" + ids + ");";
            strSql += "SELECT * FROM Flow_TemplatePhase WHERE FlowVerId in(" + ids + ");";
            strSql += "SELECT * FROM Flow_TemplatePhaseForm WHERE FlowVerId in(" + ids + ");";
            strSql += "SELECT * FROM Flow_TemplatePhaseLink WHERE FlowVerId in(" + ids + ");";
            strSql += "SELECT * FROM Flow_TemplatePhaseRule WHERE FlowVerId in(" + ids + ");";
            strSql += "SELECT * FROM Flow_TemplatePhaseRuleHandle WHERE FlowVerId in(" + ids + ")";
            DataSet ds = _helper.ExecuteDataSet(strSql);
            ds.Tables[0].TableName = "Flow_Template";
            ds.Tables[1].TableName = "Flow_TemplatePhase";
            ds.Tables[2].TableName = "Flow_TemplatePhaseForm";
            ds.Tables[3].TableName = "Flow_TemplatePhaseLink";
            ds.Tables[4].TableName = "Flow_TemplatePhaseRule";
            ds.Tables[5].TableName = "Flow_TemplatePhaseRuleHandle";

            return ds;
        }
        #endregion

        public override List<CPFlow> GetAllFlowMaxVer()
        {
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
            string strSql = @" SELECT FlowId,MAX(FlowVerId)  AS FlowVerId FROM dbo.Flow_Template
                GROUP BY FlowId";
            DataTable dt = _helper.ExecuteDataTable(strSql);
            List<int> col = new List<int>();
            foreach(DataRow dr in dt.Rows)
            {
                col.Add(int.Parse(dr["FlowVerId"].ToString()));
            }
            CPFlowDbContext _db = this._dbContext as CPFlowDbContext;
            var q = (from flow in _db.CPFlowCol
                     where col.Contains(flow.FlowVerId)
                     select flow);
            return q.ToList();

        }

        public override bool DeletePhase(List<int> phaseIdCol)
        {
            if (phaseIdCol.Count <= 0)
                return true;
            string ids = "";
            phaseIdCol.ForEach(t => {
                if (string.IsNullOrEmpty(ids))
                    ids = t.ToString();
                else
                    ids += "," + t.ToString();
            });
            string strSql = @"DELETE FROM Flow_TemplatePhaseRuleHandle WHERE PhaseId IN (" + ids + @");
                            DELETE FROM Flow_TemplatePhaseRule WHERE PhaseId IN (" + ids + @");
                            DELETE FROM Flow_TemplatePhaseLink WHERE StartPhaseId IN (" + ids + @") OR  EndPhaseId IN (" + ids + @");
                            DELETE FROM Flow_TemplatePhaseForm WHERE PhaseId IN (" + ids + @");
                            DELETE FROM Flow_TemplatePhase WHERE PhaseId IN (" + ids + @");";
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());
            _helper.ExecuteNonQuery(strSql);
            return true;
        }
        public override CPFlow GetFlowMaxVer(int flowId)
        {

            CPFlowDbContext _db = this._dbContext as CPFlowDbContext;
            var q = (from flow in _db.CPFlowCol                    
                    where flow.FlowId.Equals(flowId)
                    orderby flow.FlowVerId descending
                    select flow).Take(1);
            if (q.Count() > 0)
                return q.ToList()[0];
            else
                return null;
        }
        public override List<CPFlow> GetFlowMaxTwoVer(int flowId)
        {
            CPFlowDbContext _db = this._dbContext as CPFlowDbContext;
            var q = (from flow in _db.CPFlowCol
                     where flow.FlowId.Equals(flowId)
                     orderby flow.FlowVerId descending
                     select flow).Take(2);
            return q.ToList();
        }
    }

    public  class CPFlowPhaseRep : BaseRepository<CPFlowPhase>
    {
        public CPFlowPhaseRep(ICPFlowDbContext dbContext) : base(dbContext)
        {

        }
    }
    public class CPFlowPhaseLinkRep : BaseRepository<CPFlowPhaseLink>
    {
        public CPFlowPhaseLinkRep(ICPFlowDbContext dbContext) : base(dbContext)
        {

        }
    }

    public class CPFlowPhaseFormRep : BaseRepository<CPFlowPhaseForm>
    {
        public CPFlowPhaseFormRep(ICPFlowDbContext dbContext) : base(dbContext)
        {

        }
    }

    public class CPFlowPhaseRuleRep : BaseRepository<CPFlowPhaseRule>
    {
        public CPFlowPhaseRuleRep(ICPFlowDbContext dbContext) : base(dbContext)
        {

        }
    }

    public class CPFlowPhaseRuleHandleRep : BaseRepository<CPFlowPhaseRuleHandle>
    {
        public CPFlowPhaseRuleHandleRep(ICPFlowDbContext dbContext) : base(dbContext)
        {

        }
    }


    public abstract class BaseCPFlowInstanceRep : BaseRepository<CPFlowInstance>
    {
        public BaseCPFlowInstanceRep(ICPFlowInsDbContext dbContext) : base(dbContext)
        {

        }
        public abstract bool DeleteInstanceTask(int insId, int phaseId);
        public abstract bool DeleteInstanceTask(int insId);
    }
    public  class CPFlowInstanceRep : BaseCPFlowInstanceRep
    {
        public CPFlowInstanceRep(ICPFlowInsDbContext dbContext) : base(dbContext)
        {

        }
        public override bool DeleteInstanceTask(int insId, int phaseId)
        {
            string strSql = "DELETE FROM Flow_InstanceTask WHERE InsId=" + insId + " AND RevPhaseId=" + phaseId;
            DbHelper _helper = new DbHelper("CPFlowIns", CPAppContext.CurDbType());
            _helper.ExecuteNonQuery(strSql);
            return true;
        }
        public override bool DeleteInstanceTask(int insId)
        {
            string strSql = "DELETE FROM Flow_InstanceTask WHERE InsId=" + insId;
            DbHelper _helper = new DbHelper("CPFlowIns", CPAppContext.CurDbType());
            _helper.ExecuteNonQuery(strSql);
            return true;
        }
    }
    public class CPFlowInstanceTaskRep : BaseRepository<CPFlowInstanceTask>
    {
        public CPFlowInstanceTaskRep(ICPFlowInsDbContext dbContext) : base(dbContext)
        {

        }
    }
    public class CPFlowInstanceLogRep : BaseRepository<CPFlowInstanceLog>
    {
        public CPFlowInstanceLogRep(ICPFlowInsDbContext dbContext) : base(dbContext)
        {

        }
    }

    public class CPFlowInstanceLogUniqueRep : BaseRepository<CPFlowInstanceLogUnique>
    {
        public CPFlowInstanceLogUniqueRep(ICPFlowInsDbContext dbContext) : base(dbContext)
        {

        }
    }
    public class CPFlowInstanceFormRep : BaseRepository<CPFlowInstanceForm>
    {
        public CPFlowInstanceFormRep(ICPFlowInsDbContext dbContext) : base(dbContext)
        {

        }
    }
}
