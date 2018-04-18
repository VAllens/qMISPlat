using CacheManager.Core;
using CPFrameWork.Utility.DbOper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Global
{
   public   class CPAutoNumHelper
    {
        #region 实例 
        /// <summary>
        /// 获取节点对象服务类
        /// </summary>
        /// <returns></returns>
        public static void StartupInit( IServiceCollection services, IConfigurationRoot Configuration)
        {
            // Add framework services.
            services.AddDbContext<CPCommonDbContext>(options =>//手工高亮
                options.UseSqlServer(Configuration.GetConnectionString("CPCommonIns")));
            services.TryAddTransient<ICPCommonDbContext, CPCommonDbContext>();
            services.TryAddTransient<BaseCPAutoNumRep, CPAutoNumRep>();
            services.TryAddTransient<CPAutoNumHelper, CPAutoNumHelper>();
        }
        public static CPAutoNumHelper Instance()
        {
            return CPAppContext.GetService<CPAutoNumHelper>();
        }
        #endregion

        private BaseCPAutoNumRep _CPAutoNumRep;
        public CPAutoNumHelper(
         BaseCPAutoNumRep CPAutoNumRep
            )
        {
            this._CPAutoNumRep = CPAutoNumRep;
        }
        public CPAutoNum GetAutoNum(string autoCode)
        {
            ISpecification<CPAutoNum> specification;
            specification = new ExpressionSpecification<CPAutoNum>(t => t.AutoCode.Equals(autoCode));
            IList<CPAutoNum> col = this._CPAutoNumRep.GetByCondition(specification);
            if (col.Count > 0)
                return col[0];
            else
                return null;
        }
        public string GetNextAutoNum(string autoCode,out int autoIndex)
        {
            autoIndex = 0;
            CPAutoNum auto = this.GetAutoNum(autoCode);
            if (auto == null)
                return "";
            string s = auto.AutoTemplate;
            if (s == null)
                throw new Exception("没有找到编号为【" + autoCode + "】的自动编号，或者此自动编号的编号模板为空！");
            if (s.IndexOf("{@AutoNum@}",StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                autoIndex = this._CPAutoNumRep.GetMaxAutoIndex(auto);
                autoIndex++;
                string sIntValue = autoIndex.ToString().Trim();
                int nLength = sIntValue.Length;
                for (int i = 0; i < auto.AutoNumLen - nLength; i++)
                {
                    sIntValue = "0" + sIntValue;
                }
                s = s.Replace("{@AutoNum@}", sIntValue);
            }
            s = CPExpressionHelper.Instance.RunCompile(s);
            return s;
        }


        #region 配置导出，同步相关
        public string GetAutoConfigXml(List<int> autoIdCol)
        {

            if (autoIdCol.Count <= 0)
                return "";
            DataSet ds = this._CPAutoNumRep.GetConfig(autoIdCol);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ds.WriteXml(ms);
            byte[] bData = ms.GetBuffer();
            ms.Close();
            return System.Text.UTF8Encoding.UTF8.GetString(bData);
        }
        /// <summary>
        /// 从xml创建新的列表配置实例 
        /// </summary>
        /// <param name="funcId"></param>
        /// <param name="sysId"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public bool InitAutoFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPAutoNumRep.SyncConfigFromDataSet(targetSysId, ds, true);
            return b;
        }
        public bool SyncAutoFromConfigXml(int targetSysId, byte[] bData)
        {
            DataSet ds = new DataSet();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(bData, 0, bData.Length);
            ms.Position = 0;
            ds.ReadXml(ms);
            ms.Close();
            bool b = true;
            b = _CPAutoNumRep.SyncConfigFromDataSet(targetSysId, ds, false);
            return b;
        }
        #endregion      

    }


    public class CPAutoNum:BaseEntity
    {

        /// <summary>
        /// 标识
        /// </summary>
        public string AutoCode { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string AutoName { get; set; }

        /// <summary>
        /// 编号模板        /// 
        /// </summary>
        public string AutoTemplate { get; set; }

        /// <summary>
        /// 流水位长度
        /// </summary>
        public int? AutoNumLen { get; set; }
        /// <summary>
        /// 所属子系统
        /// </summary>
        public int? SysId { get; set; }

        /// <summary>
        /// 所属业务功能，用户自行输入或选择，主要是用来将某一个功能的相关配置都组合到一起，便于后面的查找或修改
        /// </summary>
        public string FuncTitle { get; set; }
        /// <summary>
        /// 流水号是否每年从1开始编号
        /// </summary>
        public bool? FormAutoYearSplit { get; set; }

        /// <summary>
        /// 流水号对应表单的数据库实例名
        /// </summary>
        public string FormDbIns { get; set; }

        /// <summary>
        /// 表单对应的表名
        /// </summary>
        public string FormTableName { get; set; }

        /// <summary>
        /// 表单对应表的主键
        /// </summary>
        public string FormPKField { get; set; }

        /// <summary>
        /// 表单存储流水号的字段名
        /// </summary>
        public string FormAumField { get; set; }

        /// <summary>
        /// 表单存储年份的字段名
        /// </summary>
        public string FormYearField { get; set; }
        /// <summary>
        /// 表单查找最大流水号是其他过滤条件，即sql语句里where后面的过滤条件
        /// </summary>
        public string FormDataSearch { get; set; }
        public override void FormatInitValue()
        {
            base.FormatInitValue();
            if (this.AutoNumLen.HasValue == false)
                this.AutoNumLen = 4;
            if (this.SysId.HasValue == false)
                this.SysId = 1;
            if (this.FormAutoYearSplit.HasValue == false)
                this.FormAutoYearSplit = false;
        }
    }
    

    public abstract class BaseCPAutoNumRep : BaseRepository<CPAutoNum>
    {
        public BaseCPAutoNumRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public abstract int GetMaxAutoIndex(CPAutoNum auto);
        public abstract DataSet GetConfig(List<int> idCol);
        public abstract bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew);
    }
    public class CPAutoNumRep: BaseCPAutoNumRep
    {
        public CPAutoNumRep(ICPCommonDbContext dbContext) : base(dbContext)
        {

        }
        public override bool SyncConfigFromDataSet(int targetSysId, DataSet ds, bool isCreateNew)
        {
            DbHelper _helper = new DbHelper("CPCommonIns", CPAppContext.CurDbType());

            bool b = true;
            #region 先删除数据           
            if (isCreateNew == false)
            {
                string delCodes = "";
                foreach (DataRow drMain in ds.Tables["CP_AutoNum"].Rows)
                {
                    if (string.IsNullOrEmpty(delCodes)) delCodes = drMain["AutoCode"].ToString();
                    else delCodes += "," + drMain["AutoCode"].ToString();
                }
                if (string.IsNullOrEmpty(delCodes) == false)
                {
                    string delSql = @"DELETE FROM CP_AutoNum WHERE     AutoCode IN ('" + delCodes.Replace(",", "','") + @"')";
                    _helper.ExecuteNonQuery(delSql);
                    if (!b)
                        throw new Exception("先删除已经存在的配置时出错");
                }
            }
            #endregion

            #region 写入数据
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM CP_AutoNum WHERE 1=2",
                _helper.GetConnection() as SqlConnection);
            SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd);
            // SqlCommandBuilder builder = new SqlCommandBuilder(da);
            //AddWithKey: 自动填充数据表结构,如：主键和限制
            //预设值Add,不填充结构
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;//Default Value is: Add
            DataSet dsStruct = new DataSet();
            da.Fill(dsStruct);
            dsStruct.Tables[0].TableName = "CP_AutoNum";
            #region CP_AutoNum 
            foreach (DataRow dr in ds.Tables["CP_AutoNum"].Rows)
            {
                dr["SysId"] = targetSysId;
                if (isCreateNew)
                {
                    dr["AutoName"] = dr["AutoName"].ToString() + "_副本";
                    dr["AutoCode"] = dr["AutoCode"].ToString() + "_副本";
                }
                string insertSql = CPAppContext.GetInsertSql("CP_AutoNum", dsStruct.Tables["CP_AutoNum"].Columns, dr);
             
                SqlCommand cmdInsert = new SqlCommand(insertSql, _helper.GetConnection() as SqlConnection);
                foreach (DataColumn dc in dsStruct.Tables["CP_AutoNum"].Columns)
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
            string strSql = "SELECT * FROM CP_AutoNum WHERE AutoId in(" + ids + ")";
            DataSet ds = _helper.ExecuteDataSet(strSql);
            ds.Tables[0].TableName = "CP_AutoNum";
            return ds;
        }
        public override int GetMaxAutoIndex(CPAutoNum auto)
        {
            DbHelper _db = new DbHelper(auto.FormDbIns, CPAppContext.CurDbType());
            if (CPAppContext.CurDbType() == DbHelper.DbTypeEnum.SqlServer)
            {
                string strSql = @"SELECT ISNULL(MAX(" + auto.FormAumField + @"),0)   FROM dbo." + auto.FormTableName;
                if (auto.FormAutoYearSplit.Value)
                {
                    strSql += " WHERE " + auto.FormYearField + "=" + DateTime.Now.Year;
                    if (string.IsNullOrEmpty(auto.FormDataSearch) == false)
                        strSql += "  AND (" + CPExpressionHelper.Instance.RunCompile(auto.FormDataSearch) + " ) ";
                }
                else
                {
                    if (string.IsNullOrEmpty(auto.FormDataSearch) == false)
                        strSql += "  WHERE (" + CPExpressionHelper.Instance.RunCompile(auto.FormDataSearch) + " ) ";
                }
                int NextAutoNum = Convert.ToInt32(_db.ExecuteScalar( strSql));
                return NextAutoNum;
            }
            else
            {
                throw new Exception("未实现");
            }
        }
    }
}
